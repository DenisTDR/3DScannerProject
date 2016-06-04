using System;
using System.Threading;
using RaspberryPiDotNet;

namespace csTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    int high, period;
            high = 4;
		    period = 10;
		    if (args.Length == 2)
		    {
		        period = int.Parse(args[0]);
		        high = int.Parse(args[1]);
		    }
		    var rm = new RaspberryMaster(period, high, 4);
            rm.Start();
            var working = true;
		    while (working)
		    {
                Console.WriteLine("Type something:");
		        var line = Console.ReadLine();
		        switch (line.ToLower())
		        {
                    case "exit":
                        rm.Stop();
		                working = false;
                        break;
                    case "stop":
                        rm.Stop();
                        break;
                    case "start":
                        rm.Start();
                        break;
                    case "startpwm":
                    case "s":
                        rm.StartPWM();
                        break;
                    default:
                        Console.WriteLine("You wrote: '" + line + "'");
                        break;
		        }
		    }
            Console.WriteLine("program ended");
		}
        
    }
}
