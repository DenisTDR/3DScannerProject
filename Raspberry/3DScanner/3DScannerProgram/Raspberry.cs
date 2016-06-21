using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        private int _period, _highDuration, _fotoResistorChangesToStop;
        private IChatClient _chat;
        private RaspberryHardware _piHW;
        private SettingsContainer settings = null;
        public Raspberry(int period, int highDuration, int fotoResistorChangesToStop)
        {
            _period = period;
            _highDuration = highDuration;
            _fotoResistorChangesToStop = fotoResistorChangesToStop;
            LoadSettings();
            settings.PwmPeriod = _period;
            settings.PwmHighDuration = highDuration;
            settings.LinesToSkip = fotoResistorChangesToStop;
            SaveSettings();
        }

        public Raspberry()
        {
            LoadSettings();
        }

        private bool _working;
        public void Job()
        {
            LoggerHandler.Init(new[] { (Action<string>)Console.WriteLine }, ""/*GetAvailableLogFilename(logFilePath)*/,
                minLogLevel: LogLevel.Warn);
            _chat = new ChatClient(settings.TcpServerIp, 10240, "raspberry", "parola01");
            _piHW = new RaspberryHardware(settings.PwmPeriod, settings.PwmHighDuration, settings.LinesToSkip);

            BindEvents();

            _chat.Connect();
            _piHW.Start();

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
            _piHW.CompletedAngle += _pi_CompletedAngle;
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
                    _piHW.Stop();
                    LoggerHandler.Destroy();
                    _working = false;
                    break;
                case "stop":
                    _piHW.Stop();
                    break;
                case "start":
                    _piHW.Start();
                    break;
                case "startpwm":
                case "s":
                    _piHW.StartPWM();
                    break;
                case "":
                    break;
                default:
                    if (cmd.ToLower().StartsWith("setnroflines"))
                    {
                        var linesToSkip = int.Parse(cmd.Split(' ')[1]);
                        _piHW.SetVariables(_period, _highDuration, linesToSkip);
                        settings.LinesToSkip = linesToSkip;
                        SaveSettings();
                        Console.WriteLine("Set number of lines to skip to " + linesToSkip);
                        break;
                    }
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

        private void SaveSettings()
        {
            using (var sw = new StreamWriter("settings.json"))
            {
                sw.Write(JsonConvert.SerializeObject(settings));
            }
            Console.WriteLine("settings saved: " + JsonConvert.SerializeObject(settings));
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                if (File.Exists("settings.json"))
                {
                    try
                    {
                        using (var sr = new StreamReader("settings.json"))
                        {
                            settings =
                                JsonConvert.DeserializeObject<SettingsContainer>(
                                    sr.ReadToEnd());
                        }
                    }
                    catch { }
                }
            }
            if (settings == null)
            {
                settings = new SettingsContainer
                {
                    PwmPeriod = 10,
                    PwmHighDuration = 2,
                    LinesToSkip = 4,
                    TcpServerIp = "192.169.0.123"
                };
                SaveSettings();
            }
            Console.WriteLine("settings loaded: " + JsonConvert.SerializeObject(settings));
        }
    }
}
