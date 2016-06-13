using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDR.ChatLibrary.Client;
using TDR.Logging;
using TDR.Models.Messages;
using TDR.Models.Messages.Requests;
using TDR.Models.Messages.Responses;
using TDR.Models.ViewModels;

namespace _3DScannerProgram
{
    class Raspberry
    {
        private int _period, _highDuration, _delayBetweenScans, _fotoResistorChangesToStop;
        private IChatClient _chat;
        private RaspberryHardware _pi;
        public Raspberry(int period, int highDuration, int delayBetweenScans, int fotoResistorChangesToStop)
        {
            _period = period;
            _highDuration = highDuration;
            _delayBetweenScans = delayBetweenScans;
            _fotoResistorChangesToStop = fotoResistorChangesToStop;


        }

        private bool _working;
        public void Job()
        {
            LoggerHandler.Init(new[] { (Action<string>)Console.WriteLine }, ""/*GetAvailableLogFilename(logFilePath)*/,
                minLogLevel: LogLevel.Warn);
            _chat = new ChatClient("192.169.0.123", 10240, "raspberry", "parola01");
            _pi = new RaspberryHardware(_period, _highDuration, _fotoResistorChangesToStop, _delayBetweenScans);

            BindEvents();

            _chat.Connect();
            _pi.Start();

            _working = true;
            while (_working)
            {
                Console.Write("&:");
                var line = Console.ReadLine();
                ProcessCommand(line);
            }
        }

        private void BindEvents()
        {
            Thread.Sleep(500);
            _pi.CompletedAngle += _pi_CompletedAngle;
            _chat.ErrorReceived += _chat_ErrorReceived;
            _chat.MessageReceived += _chat_MessageReceived;
            _chat.DataResponseReceived += _chat_DataResponseReceived;


        }

        private void _chat_DataResponseReceived(IChatClient sender, DataResponse dataResponse)
        {
            ProcessCommand(dataResponse.What);
        }

        private void ProcessCommand(string cmd)
        {
            switch (cmd.ToLower())
            {
                case "exit":
                    _chat.Disconnect();
                    _pi.Stop();
                    LoggerHandler.Destroy();
                    _working = false;
                    break;
                case "stop":
                    _pi.Stop();
                    break;
                case "start":
                    _pi.Start();
                    break;
                case "startpwm":
                case "s":
                    _pi.StartPWM();
                    break;
                case "":
                    break;
                default:
                    Console.WriteLine("Invalid command: '" + cmd + "'");
                    break;
            }
        }

        private void _chat_MessageReceived(IChatClient sender, BruteMessage bruteMessage)
        {
            switch (bruteMessage.MessageType)
            {
                case MessageType.ChatMessage:
                    var chatMessage = bruteMessage.Message.ConvertTo<ChatMessage>();
                    Console.WriteLine(chatMessage?.Sender?.Nick + ": " + chatMessage?.MessageBody);
                    ProcessCommand(chatMessage?.MessageBody);
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

        private void _pi_CompletedAngle(RaspberryHardware sender, int counter)
        {
            Console.WriteLine("Angle completed!");
            _chat.SendMessage(new ChatMessage()
            {
                MessageBody = "Angle Completed!",
                Recipient = new UserViewModel() {Username = "kinect", Nick = "TDR"}
            });
        }
    }
}
