using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TDR.ChatLibrary.Client;
using TDR.ChatLibrary.Messages;
using TDR.ChatLibrary.Messages.Others;
using TDR.ChatLibrary.Messages.Requests;
using TDR.ChatLibrary.Messages.Responses;
using TDR.Logging;
using TDR.TCPDataBaseManagement;
using TDR.TCPDataBaseManagement.ViewModels;
using TDR.TCPLibrary.Client;
using TDR.TCPLibrary.Server;

namespace TDR.ChatLibrary.Server
{
    public class ChatServer : IChatServer
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        private readonly IServerBase _serverBase;
        private readonly IUserRepository _userRepository;
        private readonly DataRequestResolver _dataResolver;
        private readonly IChatRepository _chatRepository;
        private readonly List<IChatClient> _notLoggedInChatClients = new List<IChatClient>();

        private readonly ConcurrentDictionary<string, List<IChatClient>> _loggedInClients =
            new ConcurrentDictionary<string, List<IChatClient>>();

        private readonly List<IChatClient> _loggedInChatClients = new List<IChatClient>();

        public bool IsAlive
            
        {
            get
            {
                lock (_notLoggedInChatClients)
                {
                    lock (_loggedInChatClients)
                    {
                        return
                            _notLoggedInChatClients.Any(x => x.IsConnected) ||
                            _loggedInChatClients.Any(x => x.IsConnected) ||
                            _serverBase.IsListening;
                    }
                }
            }
        }

        public int NumberOfActiveConnections
        {
            get
            {
                lock (_notLoggedInChatClients)
                {
                    lock (_loggedInChatClients)
                    {
                        return _loggedInChatClients.Count(x => x.IsConnected) +
                               _notLoggedInChatClients.Count(x => x.IsConnected);
                    }
                }
            }
        }
        public int NumberOfReferencedClients
        {
            get
            {
                lock (_notLoggedInChatClients)
                {
                    lock (_loggedInChatClients)
                    {
                        return _loggedInChatClients.Count + _notLoggedInChatClients.Count;
                    }
                }
            }
        }
        public ChatServer(string host, int port, IUserRepository userRepository)
        {
            _serverBase = new ServerBase(port, host);
            _serverBase.ClientConnected += _serverBase_ClientConnected;
            _serverBase.StartListening();
            _userRepository = userRepository;
            _dataResolver = new DataRequestResolver(userRepository);
            _chatRepository = new ChatDbRepository();
        }

        private void _serverBase_ClientConnected(IServerBase sender, TCPLibrary.Client.IClientBase clientBase)
        {
            OnClientConnected(clientBase);
            sender.DisownClient(clientBase);
        }

        protected void OnClientConnected(IClientBase clientBase)
        {
            Logger.Debug("in ChatServer OnClientConnected");
            var chatClient = new ChatClient(clientBase);
            BindEventsToChatClient(chatClient);
            lock (_notLoggedInChatClients)
            {
                _notLoggedInChatClients.Add(chatClient);
            }
            chatClient.SendMessage(new AuthRequired());
        }


        private void ChatClient_MessageReceived(IChatClient sender, BruteMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.KeepAlive:
                    Logger.Debug("KeepAlive received in server");
                    break;
                case MessageType.AuthRequest:
                    if (sender.IsLoggedIn)
                    {
                        Logger.Debug("a chatClient is already logged in");
                        return;
                    }
                    var authRequest = message.Message.ConvertTo<AuthRequest>();
                    var authResponse = new AuthResponse();
                    Logger.Debug("chatClient is logging in.");
                    string info;
                    if (_userRepository.CheckCredentials(authRequest.Username, authRequest.Password, out info))
                    {
                        Logger.Warn("chatClient successfully logged in.");
                        authResponse.Ok = true;
                        sender.UserName = authRequest.Username;
                        sender.IsLoggedIn = true;
                        OnChatClientLoggedIn(sender);
                    }
                    else
                    {
                        Logger.Warn("chatClient invalid auth creds.");
                        authResponse.Ok = false;
                        BindEventsToChatClient(sender, true);
                        authResponse.Info = info;
                        Task.Run(() =>
                        {
                            var x = Task.Delay(TimeSpan.FromSeconds(5));
                            x.Wait();
                            sender.Disconnect();
                        });
                    }

                    lock (_notLoggedInChatClients)
                    {
                        _notLoggedInChatClients.Remove(sender);
                    }
                    sender.SendMessage(authResponse);
                    
                    break;
                case MessageType.TextMessage:
                    if (!sender.IsLoggedIn)
                    {
                        sender.Disconnect();
                        Logger.Debug("receiving textMessage from not authed client. disconnecting");
                        return;
                    }
                    var textMessage = message.Message.ConvertTo<TextMessage>();
                    Logger.Warn(sender.UserName + " says: " + textMessage.MessageBody);
                    break;
                case MessageType.DataRequest:
                    Task.Run(() =>
                    {
                        var dataRequest = message.Message.ConvertTo<DataRequest>();
                        sender.SendMessage(_dataResolver.ProcessDataRequestMessage(dataRequest, sender));
                    });
                    break;
                case MessageType.ChatMessage:
                    var chatMessage = message.Message.ConvertTo<ChatMessage>();

                    if (!_loggedInClients.ContainsKey(chatMessage.Recipient.Username) ||
                        _loggedInClients[chatMessage.Recipient.Username].Count == 0)
                    {
                        sender.SendMessage(new ErrorResponse
                        {
                            TargetId = chatMessage.Id,
                            Reason = "The recipient isn't loggedIn!"
                        });
                    }
                    else
                    {
                        chatMessage.Sender = new UserViewModel {Username = sender.UserName};
                        if (!_chatRepository.SaveTextMessage(new ChatMessageViewModel
                        {
                            Message = chatMessage.MessageBody,
                            Recipient = chatMessage.Recipient,
                            Sender = chatMessage.Sender
                        }))
                        {
                            sender.SendMessage(new ErrorResponse
                            {
                                TargetId = chatMessage.Id,
                                Reason = "An unknown error occurred processing the request!"
                            });
                        }
                        else
                        {
                            var recipientClients = _loggedInClients[chatMessage.Recipient.Username];
                            foreach (var recipient in recipientClients.ToList())
                            {
                                recipient.SendMessage(chatMessage);
                            }
                        }
                    }

                    break;
                default:

                    break;
            }
        }

        protected void OnChatClientLoggedIn(IChatClient chatClient)
        {
            lock (_loggedInChatClients)
            {
                _loggedInChatClients.Add(chatClient);
            }
            if (!_loggedInClients.ContainsKey(chatClient.UserName))
            {
                _loggedInClients[chatClient.UserName] = new List<IChatClient>();
            }
            _loggedInClients[chatClient.UserName].Add(chatClient);
        }

        private void BindEventsToChatClient(IChatClient chatClient, bool unbind=false)
        {
            try
            {
                if (unbind)
                {
                    chatClient.MessageReceived -= ChatClient_MessageReceived;
                    chatClient.LostConnection -= ChatClient_LostConnection;
                }
                else
                {
                    chatClient.MessageReceived += ChatClient_MessageReceived;
                    chatClient.LostConnection += ChatClient_LostConnection;
                }
            }
            catch
            {
                // ignored
            }
        }
        private void ChatClient_LostConnection(IChatClient sender, object reason)
        {
            Logger.Warn("ChatClient connectionLost -> removed");
            if (sender.IsLoggedIn)
            {
                lock (_loggedInChatClients)
                {
                    _loggedInChatClients.Remove(sender);
                }
                _loggedInClients[sender.UserName].Remove(sender);
            }
            else
            {
                lock (_notLoggedInChatClients)
                {
                    _notLoggedInChatClients.Remove(sender);
                }
            }
            BindEventsToChatClient(sender, true);
        }

        public void KillConnections()
        {
            lock (_loggedInChatClients)
            {
                foreach (var loggedInChatClient in _loggedInChatClients)
                {
                    loggedInChatClient.Disconnect();
                }
            }
            lock (_notLoggedInChatClients)
            {
                foreach (var notLoggedInChatClient in _notLoggedInChatClients)
                {
                    notLoggedInChatClient.Disconnect();
                }
            }
        }

        public void StopActivity()
        {
            KillConnections();
            _serverBase.KillConnections();
            _serverBase.StopListening();
        }
    }
}
