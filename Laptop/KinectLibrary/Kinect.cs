using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KinectLibrary._3D;
using Microsoft.Samples.Kinect.KinectFusionExplorer;
using TDR.ChatLibrary.Client;
using TDR.Logging;
using TDR.Models.Messages;
using TDR.Models.Messages.Requests;
using TDR.Models.Messages.Responses;
using TDR.Models.ViewModels;

namespace KinectLibrary
{
    public class Kinect
    {
        private StartClass _sc;
        private ChatClient _chat;
        private int cnt = 0;
        public Kinect()
        {
            LoggerHandler.Init(new[] {(Action<string>) Console.WriteLine}, "" /*GetAvailableLogFilename(logFilePath)*/,
                minLogLevel: LogLevel.Warn);
            _chat = new ChatClient("192.169.0.123", 10240, "kinect", "parola01");
            _chat.MessageReceived += _chat_MessageReceived;
            _chat.ErrorReceived += _chat_ErrorReceived;
        }

        
        

        private void ProcessMessage(string message)
        {
            switch (message)
            {
                case "Angle Completed!":
                    if (cnt > 10)
                    {
                        Console.WriteLine("st done");
                        break;
                    }
                    _sc.Reset();
                    Thread.Sleep(500);
                    _sc.SaveToFile(@"D:\Projects\OC\test\test-" + cnt + ".ply");
                    _chat.SendMessage(new ChatMessage()
                    {
                        MessageBody = "startpwm",
                        Recipient = new UserViewModel{ Username = "raspberry" }
                    });
                    Console.WriteLine("running async task");
                    int auxCnt = cnt;
                    new Thread(() =>
                    {
                        var inPath = @"D:\Projects\OC\test\test-" + auxCnt + ".ply";
                        var parser = new PlyParser();
                        Console.WriteLine("reading st");
                        parser.Read(inPath);
                        Console.WriteLine("making st");
                        new Object3D().MakeRotateAndSaveObject(parser, new Point3D(0.125571, -0.02198797, -0.6631655),
                            36);
                        Console.WriteLine("aux Thread job done");
                    }).Start();
                    cnt ++;
                    break;
                default:
                    Console.WriteLine("Can't process message");
                    break;
            }
        }


        #region public methods
        public void Start()
        {
            _sc = new StartClass();
            _sc.OpenWindow();

            _chat.Connect();
        }

        public void Stop()
        {
            _chat.Disconnect();
            _sc.Kill();
        }

        public void Reset()
        {
            _sc.Reset();
        }

        public bool SaveToPlyFile(string path)
        {
            return _sc.SaveToFile(path);
        }

        public void WaitUntilClose()
        {
            _sc.WaitUntilClose();
        }
        #endregion  

        private void _chat_MessageReceived(IChatClient sender, TDR.Models.Messages.BruteMessage bruteMessage)
        {
            switch (bruteMessage.MessageType)
            {
                case MessageType.ChatMessage:
                    var chatMessage = bruteMessage.Message.ConvertTo<ChatMessage>();
                    Console.WriteLine(chatMessage?.Sender?.Nick + ": " + chatMessage?.MessageBody);
                    ProcessMessage(chatMessage?.MessageBody);
                    break;
                default:
                    Console.WriteLine("Invalid messageType (" + bruteMessage.MessageType + ") received");
                    break;
            }
        }
        private void _chat_ErrorReceived(IChatClient sender, ErrorResponse errorResponse)
        {
            Console.WriteLine("chatError: " + errorResponse.Reason);
        }
    }
}
