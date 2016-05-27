using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TDR.ChatLibrary;
using TDR.ChatLibrary.Client;
using TDR.ChatLibrary.Messages;
using TDR.ChatLibrary.Messages.Requests;
using TDR.ChatLibrary.Messages.Responses;
using TDR.Logging;
using TDR.TCPDataBaseManagement.ViewModels;
using TDR.TCPLibrary.Client;

namespace TDR.TCPClient
{
    class Program
    {
        private static Logger Logger = LoggerHandler.RegisterLogger("TCPClient");
        private static bool shouldLogCounters = true;
        private static List<UserViewModel> _users = new List<UserViewModel>();
        private static UserViewModel _selectedUser;
        private static UserViewModel _me;
        static void Main(string[] args)
        {
            string logFilePath = "Logs/ClientLogs.log";
            //if (args.Length != 0)
            //    logFilePath = args[0];
            var user = "tdr";
            var pass = "romania";
            var host = "127.0.0.1";
            host = "tdrs.me";
            var port = 10240;
            if (args.Length == 2)
            {
                user = args[0];
                pass = args[1];
            }

            LoggerHandler.Init(new[] {(Action<string>) Console.WriteLine}, ""/*GetAvailableLogFilename(logFilePath)*/,
                minLogLevel: LogLevel.Warn);
            Thread.Sleep(500);

            IChatClient chatClient = new ChatClient(host, port, user, pass);
            chatClient.ConnectionStateChanged += ChatClient_ConnectionStateChanged;
            chatClient.LostConnection += ChatClient_LostConnection;
            chatClient.DataResponseReceived += ChatClient_DataResponseReceived;
            chatClient.MessageReceived += ChatClient_MessageReceived;
            chatClient.Connect();

            new Thread(() =>
            {
                return;
                Thread.Sleep(5000);
                while (shouldLogCounters)
                {
                    LoggerHandler.LogCounters();
                    Thread.Sleep(3000);
                }
            }).Start();

            while (true)
            {
                var s = Console.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    if (s == "exit") break;
                    if (s.StartsWith("user ") && s.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).Length == 2)
                    {
                        var tmpUser = s.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)[1];
                        var tmpSelUser =
                            _users.FirstOrDefault(
                                x => x.Username.Equals(tmpUser, StringComparison.InvariantCultureIgnoreCase)) ??
                            _users.FirstOrDefault(
                                x => x.Nick.Equals(tmpUser, StringComparison.InvariantCultureIgnoreCase));
                        if (tmpSelUser == null)
                        {
                            Console.WriteLine("Invalid user selected.");
                        }
                        else
                        {
                            _selectedUser = tmpSelUser;
                            Console.WriteLine("Selected user " + tmpSelUser.Nick);
                        }
                        continue;
                    }
                    switch (s)
                    {
                        case "getUsers":
                            Console.WriteLine("sending getUsers request");
                            chatClient.SendMessage(new DataRequest {What = "users"});
                            break;
                        case "listUsers":
                            ListUsers();
                            break;
                        case "me":
                            if (_me == null)
                            {
                                Console.WriteLine("me data not available. getting from server");
                                chatClient.SendMessage(new DataRequest {What = "myInfo"});
                            }
                            else
                            {
                                Console.WriteLine(_me.Username + "  ->  " + _me.Nick);
                            }
                            break;
                        default:
                            if (_selectedUser == null)
                            {
                                Console.WriteLine(
                                    "You must select a recipient. Use 'getUsers' 'listUsers' 'user {Username or Nick}'");
                                break;
                            }
                            chatClient.SendMessage(new ChatMessage {MessageBody = s, Recipient = _selectedUser});
                            break;
                    }
                }
            }
            chatClient.Disconnect();
            Logger.Warn("ChatClient program end!");
            LoggerHandler.Destroy();
        }

        private static void ChatClient_MessageReceived(IChatClient sender, BruteMessage bruteMessage)
        {
            switch (bruteMessage.MessageType)
            {
                case MessageType.ChatMessage:
                    var chatMessage = bruteMessage.Message.ConvertTo<ChatMessage>();
                    Console.WriteLine(chatMessage.Sender.Nick + ":" + chatMessage.MessageBody);
                    break;
            }
        }

        private static void ChatClient_DataResponseReceived(IChatClient sender, DataResponse dataResponse)
        {
            var data = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dataResponse.Data),
                dataResponse.DataType);
            switch (dataResponse.What)
            {
                case "users":
                    if (data != null)
                    {
                        _users = (List<UserViewModel>)data;
                        Console.WriteLine("Got Users");

                        _selectedUser =
                            _users.FirstOrDefault(x => x.Username.Equals(sender.UserName == "tdr" ? "user1" : "tdr"));
                    }
                    break;
                case "myInfo":
                    if (data != null)
                    {
                        _me = (UserViewModel) data;
                        Console.WriteLine("Got My Info");
                    }
                    break;
                default:
                    break;
                    
            }
        }

        private static void ListUsers()
        {
            Console.Write("\nUsers:");
            var s = _users.Aggregate(string.Empty, (acc, user) => acc + "\n" + user.Nick);
            Console.WriteLine(s);
        }

        private static void ChatClient_LostConnection(IChatClient sender, object reason)
        {
            Logger.Warn("ChatClient_LostConnection");
        }

        private static void ChatClient_ConnectionStateChanged(ChatClient sender, ChatLibrary.ChatClientConnectionState connectionState)
        {
            if (connectionState == ChatClientConnectionState.CantConnect ||
                connectionState == ChatClientConnectionState.Disconnected ||
                connectionState == ChatClientConnectionState.LoggingInFailed)
                shouldLogCounters = false;
            if (connectionState == ChatClientConnectionState.LoggedIn ||
                connectionState == ChatClientConnectionState.LoggingInFailed)
            {
                Console.WriteLine(connectionState + " with un=" + sender.UserName);

                if (connectionState == ChatClientConnectionState.LoggedIn)
                {
                    sender.SendMessage(new DataRequest { What = "myInfo" });
                    sender.SendMessage(new DataRequest { What = "users" });
                }
            }
            if (connectionState == ChatClientConnectionState.LoggedIn)
            {
                //sender.SendMessage(new datareq);
            }
            Logger.Warn("ChatClientConnectionState changed to " + connectionState);
        }


        

        public static string GetAvailableLogFilename(string initialFilename)
        {
            var ok = false;
            var crtFilename = initialFilename;
            var cnter = 1;
            do
            {
                try
                {
                    var sr = new StreamWriter(crtFilename, true);
                    ok = true;
                    sr.Close();
                }
                catch
                {
                    ok = false;
                    crtFilename = (!string.IsNullOrEmpty(Path.GetDirectoryName(initialFilename))
                        ? Path.GetDirectoryName(initialFilename) + "/"
                        : "") +
                                  Path.GetFileNameWithoutExtension(initialFilename) + "_" + cnter +
                                  Path.GetExtension(initialFilename);
                    cnter++;
                }
            } while (!ok);
            return crtFilename;
        }
    }
}
