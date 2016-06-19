using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectLibrary;
using KinectLibrary._3D;

namespace Laptop
{
    class Program
    {
        private static Kinect kinect;
        [STAThread]
        static void Main(string[] args)
        {
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

        static void Loop()
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
                    case "save":
                        kinect.SaveToPlyFile(@"D:\Projects\OC\test\test-" + cnt + ".ply");
                        cnt++;
                        break;
                    default:
                        kinect.ProcessMessage(line);
                        break;
                }
            }
            Console.WriteLine("loop end");
            
        }
        
    }
}
