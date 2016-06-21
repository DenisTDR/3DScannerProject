using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KinectLibrary;
using KinectLibrary._3D;
using TDR.Logging;

namespace Laptop
{
    class Program
    {
        private static Kinect kinect;
        [STAThread]
        static void Main(string[] args)
        {
            LoggerHandler.Init(new[] {(Action<string>) Console.WriteLine}, "" /*GetAvailableLogFilename(logFilePath)*/,
                minLogLevel: LogLevel.Warn);
            CheckAndCopyKinectFusionSDKLibrary();
            //new Object3D().IgnoreLowerPoints(@"D:\Projects\OC\test\MeshedReconstruction.ply");
            //return;

            bool a = true;
            if (a)
            {
                kinect = new Kinect();
                kinect.Start();
                Console.WriteLine("kinect started");
                Loop();
            }
            else
            {
                KinectLibrary.Program1.Main(null);
            }
        }

        private static async void Loop()
        {
            var working = true;
            var cnt = 0;
            while (working)
            {
                Console.WriteLine("&:");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                switch (line)
                {
                    case "exit":
                        Console.WriteLine("closing kinect");
                        kinect.Stop();
                        kinect.WaitUntilClose();
                        Console.WriteLine("kinect closed");
                        working = false;
                        break;
                    case "reset":
                        kinect.Reset();
                        break;

                    default:
                        await kinect.ProcessMessage(line);
                        break;
                }
            }
            Console.WriteLine("loop end");
            
        }

        static void CheckAndCopyKinectFusionSDKLibrary()
        {
            if (!File.Exists("KinectFusion180_64.dll") || !File.Exists("KinectFusion180_32.dll"))
            {
                Console.WriteLine("Copying kinect fussion libraries.");
                File.Copy("../KinectFusion180_32.dll", "KinectFusion180_32.dll", true);
                File.Copy("../KinectFusion180_64.dll", "KinectFusion180_64.dll", true);
                Console.WriteLine("copied.");
            }
        }
    }
}
