using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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


        public async Task ProcessMessage(string message)
        {
            switch (message)
            {
                case "scan":
                    Console.WriteLine("Begin scanning object!");
                    ResetStateVars();
                    cnt = 0;
                    await ProcessMessage("angle done");
                    break;
                case "angle done":
                case "Angle Completed!":
                    if (cnt > 10)
                    {
                        Console.WriteLine("done scanning");
                        break;
                    }
                    if (cnt == 1)
                    {
                        new Thread(BuildFinalObject).Start();
                    }
                    this.Reset();
                    await Task.Delay(1000);
                    this.SaveCurrentObject();
                    _chat.SendMessage(new ChatMessage()
                    {
                        MessageBody = "startpwm",
                        Recipient = new UserViewModel{ Username = "raspberry" }
                    });
                    
                    StartThreadToRotate(cnt);
  
                    cnt ++;
                    break;
                case "S":
                case "save":
                    this.Reset();
                    await Task.Delay(750);
                    this.SaveCurrentObject();
                    break;
                case "R":
                case "rotate":
                    this.StartThreadToRotate(cnt);
                    break;
                case "SR":
                    await ProcessMessage("S");
                    await ProcessMessage("R");
                    break;
                case "calc center":
                    this.Reset();
                    await Task.Delay(1000); 
                    this.CalculateCenter();
                    this.Reset();
                    break;
                case "calc height":
                    this.Reset();
                    await Task.Delay(1000);
                    this.CalculateMinimumHeight();
                    this.Reset();
                    break;
                case "reset":
                    await Task.Delay(1000);
                    this.Reset();
                    break;
                case "open window":
                    this.OpenWindow();
                    break;
                case "reset counter":
                    cnt = 0;
                    break;
                case "open result":
                    if (!string.IsNullOrEmpty(resultPath) && File.Exists(resultPath))
                    {
                        try
                        {
                            Process.Start(resultPath);
                            break;
                        }
                        catch { }
                    }
                    Console.WriteLine("can't open result file ('" + resultPath + "').");

                    break;
                default:
                    Console.WriteLine("Can't process command: '" + message + "'.");
                    break;
            }
        }

        private static string tmpPath = @"D:\Projects\OC\tmp\crtBuild";
        private void SaveCurrentObject()
        {
            _sc.SaveToFile(tmpPath + @"\part-" + cnt + ".ply");
        }

        private List<bool> rotatedParts;
        private SemaphoreSlim semaphoreRotateParts;
        private int rotatedObjectsParts = 0;
        private void StartThreadToRotate(int _cnt)
        {
            Console.WriteLine("running aditional thread to rotate current object");
            new Thread(() =>
            {
                var crtThreadCnt = _cnt;
                Console.WriteLine("thread started");
                Console.WriteLine("waiting for lock");

                semaphoreRotateParts.Wait();

                Console.WriteLine("lock locked");
                var inPath = tmpPath + @"\part-" + crtThreadCnt + ".ply";
                if (File.Exists(inPath))
                {
                    var parser = new PlyParser();
                    Console.WriteLine("reading st");
                    parser.Read(inPath);
                    Console.WriteLine("making st");
                    new Object3D().MakeRotateAndSaveObject(parser,
                        Variables.Center,
                        36*crtThreadCnt);
                }
                else
                {
                    Console.WriteLine("Can't find file: " + inPath);
                }
                Console.WriteLine("aux Thread job done, releasing lock");
                rotatedParts[_cnt] = true;
                rotatedObjectsParts++;
                semaphoreRotateParts.Release();
            }).Start();

        }

        #region public methods
        public void Start()
        {
            _sc = new StartClass();
            this.OpenWindow();
            _chat.Connect();
        }

        public void OpenWindow()
        {
            _sc.OpenWindow();
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

        public void CalculateCenter()
        {
            var tmpFilePath = Path.GetTempFileName() + ".ply";
            if (_sc.SaveToFile(tmpFilePath))
            {
                var obj = new Object3D();
                obj.IgnoreLowerPoints(tmpFilePath);
                var cog = obj.RecalculateCenter();
                obj.Dispose();

                Console.WriteLine("center set to: " + cog);
            }
            else
            {
                throw new Exception("can't save to file");
            }
            try
            {
                File.Delete(tmpFilePath);
            }
            catch { }
        }

        public void CalculateMinimumHeight()
        {
            var tmpFilePath = tmpPath + "/tmp_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + ".ply";
            if (_sc.SaveToFile(tmpFilePath))
            {
                var obj = new Object3D();
                var max = obj.getMaxHeight(tmpFilePath);
                Console.WriteLine("minimum height set to: " + max.Y);
            }
            else
            {
                throw new Exception("can't save to file");
            }

            try
            {
                File.Delete(tmpFilePath);
            }
            catch { }
        }
        #endregion

        public static void CombineObjects(string path1, string path2, string outputPath)
        {
            Console.WriteLine("combining '" + path1 + "' with '" + path2 + "' ...");
            Console.WriteLine("reading: " + path1);
            var obj1 = new Object3D(path1);
            Console.WriteLine("read. now reading '" + path2 + "' ...");
            var obj2 = new Object3D(path2);
            Console.WriteLine("mergin objects ...");
            obj1.concatenate(obj2);
            Console.WriteLine("merged, now saving to '" + outputPath + "'");
            var tmpParser = obj1.LastUsedParser;
            tmpParser.LoadFromObject3D(obj1);
            tmpParser.Write(outputPath);
            Console.WriteLine("saved!");
        }

        private string resultPath = tmpPath + "/result.ply";
        private void BuildFinalObject()
        {
            var done = new List<bool>(new bool[11]);
            var tmpFinalPath = tmpPath + "/tmp.ply";
            File.Copy(tmpPath + "/proto.ply", tmpFinalPath);
            while (done.Count(x => x) < 10)
            {
                var stdone = false;
                for (var i = 0; i < cnt - 1; i++)
                {
                    if(done[i] || !rotatedParts[i])
                        continue;

                    Kinect.CombineObjects(tmpFinalPath, tmpPath + "/part-" + i + "-rotated.ply", tmpFinalPath);
                    done[i] = true;
                    stdone = true;
                }
                if (!stdone)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting for object parts to be rotated");
                }
            }
            resultPath = tmpPath + "/result.ply";
            File.Move(tmpFinalPath, resultPath);
            Console.WriteLine("Final object build: '" + resultPath + "'.");
            Console.WriteLine("type 'open result' to open it");
        }

        private void ResetStateVars()
        {
            rotatedObjectsParts = 0;
            semaphoreRotateParts?.Release(100);
            semaphoreRotateParts = new SemaphoreSlim(4, 150);
            rotatedParts = new List<bool>(new bool[11]);
            foreach (var file in Directory.GetFiles(tmpPath))
            {
                if (file.EndsWith(".ply") && !file.Contains("proto.ply"))
                {
                    File.Delete(file);
                }
            }
        }
        private void _chat_MessageReceived(IChatClient sender, BruteMessage bruteMessage)
        {
            switch (bruteMessage.MessageType)
            {
                case MessageType.ChatMessage:
                    var chatMessage = bruteMessage.Message.ConvertTo<ChatMessage>();
                    Console.WriteLine(chatMessage?.Sender?.Nick + ": " + chatMessage?.MessageBody);
                    Task.Run(async () =>
                    {
                        await ProcessMessage(chatMessage?.MessageBody);
                    });
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
