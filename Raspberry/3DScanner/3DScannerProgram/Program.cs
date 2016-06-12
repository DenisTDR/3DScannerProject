using System;
using TDR.ChatLibrary.Client;

namespace _3DScannerProgram
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		    var delayBetweenScans = 0;
            var high = 4;
		    var period = 10;
		    if (args.Length >= 2)
		    {
		        period = int.Parse(args[0]);
		        high = int.Parse(args[1]);
		    }
		    if (args.Length == 3)
		    {
		        delayBetweenScans = int.Parse(args[2]);
		    }

		    var pi = new Raspberry(period, high, delayBetweenScans, 4);
            pi.Job();

           
            Console.WriteLine("program ended");
		}
        
    }
}
