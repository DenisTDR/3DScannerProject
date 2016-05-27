using System;
using System.IO;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TDR.ChatLibrary.Messages;
using TDR.ChatLibrary.Messages.Requests;
using TDR.ChatLibrary.Messages.Responses;
using TDR.Logging;
using TDR.TCPLibrary;
using TDR.TCPLibrary.Client;
using UtilisAndExtensionsLibrary;

namespace TDR.ChatLibrary.Client
{
    public class ChatClient : IChatClient
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        private readonly IClientBase _clientBase;
        public bool IsServerSideClient { get; }
        public bool IsLoggedIn { get; set; }
        public ChatClientConnectionState ConnectionState { get; protected set; }
        private AuthRequest AuthRequest { get; }
        public string UserName { get; set; }
        public bool IsConnected => _clientBase != null && _clientBase.Connected;

        public event ChatClientConnectionStateChangedEventHandler ConnectionStateChanged;
        public event ChatClientMessageReceivedEventHandler MessageReceived;
        public event ChatClientLostConnectionEventHandler LostConnection;
        public event ChatClientDataResponseReceivedEventHandler DataResponseReceived;
        private bool _authRequestSent;
        private bool _lostConnectionSent;
        public ChatClient(string host, int port, string username, string password)
        {
            _clientBase = new ClientBase(host, port);
            IsServerSideClient = false;
            AuthRequest = new AuthRequest {Username = username, Password = password};
            UserName = username;
        }

        public ChatClient(IClientBase clientBase)
        {
            IsServerSideClient = true;
            _clientBase = clientBase;
            Connect();
            SendKeepAliveMessage();
        }
        public void Connect()
        {
            _clientBase.Disconnected += _clientBase_Disconnected;
            _clientBase.MessageReceived += ClientBaseMessageReceived;
            _clientBase.ConnectionStateChanged += _clientBase_ConnectionStateChanged;
            if (!_clientBase.Connected && !this.IsServerSideClient)
                _clientBase.Connect();
        }

        public void Disconnect()
        {
            _clientBase.Disconnect();
        }
        private void _clientBase_ConnectionStateChanged(IClientBase sender, ClientBaseConnectionState clientBaseConnectionState)
        {
            switch (clientBaseConnectionState)
            {
                case ClientBaseConnectionState.CantConnect:
                    OnConnectionStateChange(ChatClientConnectionState.CantConnect);
                    break;
                case ClientBaseConnectionState.Disconnected:
                    OnLostConnection(null);
                    OnConnectionStateChange(ChatClientConnectionState.Disconnected);
                    break;
                case ClientBaseConnectionState.Connecting:
                    OnConnectionStateChange(ChatClientConnectionState.Connecting);
                    break;
                case ClientBaseConnectionState.TcpConnected:
                    OnConnectionStateChange(ChatClientConnectionState.WaitingWelcomeMessage);
                    SendKeepAliveMessage();
                    break;
                default:

                    break;
            }
        }

        private void ClientBaseMessageReceived(IClientBase sender, byte[] message)
        {
            Logger.Debug("received a message in ChatClient");
            BruteMessage bruteMessage;
            MessageDeserializingErrorInfo errorInfo;
            if (!BruteMessage.TryDebinarize(message, out bruteMessage, out errorInfo))
            {
                Logger.Warn("Can't debinarize a message:\n" + JsonConvert.SerializeObject(errorInfo));
                return;
            }
            if (IsServerSideClient)
            {
                //Logger.Debug("ChatClient received st for ChatServer: " + bruteMessage.MessageType);
                MessageReceived?.Invoke(this, bruteMessage);
                return;
            }
            switch (bruteMessage.MessageType)
            {
                case MessageType.KeepAlive:
                    Logger.Debug("KeepAlive received in client");
                    break;
                case MessageType.AuthRequired:
                case MessageType.Welcome:
                    if (!_authRequestSent)
                    {
                        Logger.Debug("ChatClient sending authRequest");
                        _authRequestSent = true;
                        this.SendMessage(AuthRequest);
                        OnConnectionStateChange(ChatClientConnectionState.LoggingIn);
                    }
                    break;
                case MessageType.AuthResponse:
                    Logger.Debug("ChatClient received authResponse");
                    var authResponse = (AuthResponse) bruteMessage.Message;
                    if (authResponse.Ok)
                    {
                        Logger.Warn("ChatClient auth successfully");
                        OnConnectionStateChange(ChatClientConnectionState.LoggedIn);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(authResponse.Info))
                        {
                            Logger.Warn("ChatClient auth error with no info.");
                        }
                        else
                        {
                            Logger.Warn("ChatClient auth error with info: " + authResponse.Info);
                            _clientBase.Disconnect();
                            OnConnectionStateChange(ChatClientConnectionState.LoggingInFailed);
                        }
                    }
                    break;
                case MessageType.TextMessage:
                case MessageType.ChatMessage:
                    OnMessageReceived(bruteMessage);
                    break;
                case MessageType.DataResponse:
                    DataResponseReceived?.Invoke(this, bruteMessage.Message.ConvertTo<DataResponse>());
                        break;
                default:

                    break;
            }
        }

        private void _clientBase_Disconnected(IClientBase sender, object message)
        {
            OnLostConnection(message);
            OnConnectionStateChange(ChatClientConnectionState.Disconnected);
        }

        protected void OnConnectionStateChange(ChatClientConnectionState connectionState)
        {
            if (connectionState == this.ConnectionState) return;
            this.ConnectionState = connectionState;
            ConnectionStateChanged?.Invoke(this, connectionState);
        }

        protected void OnMessageReceived(BruteMessage message)
        {
            MessageReceived?.Invoke(this, message);
        }
        public void SendMessage(IMessage message)
        {
            if (message is ITransportMessage)
            {
                if (((ITransportMessage) message).Id == null)
                    ((ITransportMessage) message).Id = MessageId.NewMessageId();
            }
            var bm = new BruteMessage(message);
            if (bm.MessageType == MessageType.AuthRequest)
            {
                Logger.Warn("Sending auth request <<<");
            }
            _clientBase.SendMessage(bm.ToBinary());
            _lastSendingDateTime = DateTime.Now;
        }

        protected void OnLostConnection(object message)
        {
            if (_lostConnectionSent) return;
            _lostConnectionSent = true;
            _shouldSendKeepAliveMessages = false;
            LostConnection?.Invoke(this, message);
        }

        private DateTime _lastSendingDateTime = DateTime.Now;
        private bool _shouldSendKeepAliveMessages;
        private void SendKeepAliveMessage()
        {
            _shouldSendKeepAliveMessages = true;
            (new Thread(() =>
            {
                Interlocked.Increment(ref Counters.KeepAliveThreads);
                Thread.Sleep(2000);
                begin:
                if (!_shouldSendKeepAliveMessages)
                {
                    Interlocked.Decrement(ref Counters.KeepAliveThreads);
                    Logger.Warn("Ended sending keepAlive in ChatClient.");
                    return;
                }
                if ((DateTime.Now - _lastSendingDateTime).TotalMilliseconds > 500)
                {
                    this.SendMessage(new KeepAlive());
                }
                Thread.Sleep(1000);
                goto begin;

            })).Start();
        }
    }

    public delegate void ChatClientConnectionStateChangedEventHandler(
        ChatClient sender, ChatClientConnectionState connectionState);

    public delegate void ChatClientMessageReceivedEventHandler(IChatClient sender, BruteMessage bruteMessage);

    public delegate void ChatClientLostConnectionEventHandler(IChatClient sender, object reason);
    public delegate void ChatClientDataResponseReceivedEventHandler(IChatClient sender, DataResponse dataResponse);
}
