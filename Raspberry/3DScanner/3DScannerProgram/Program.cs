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

		    var pi = new Raspberry();
            pi.Job();

           
            Console.WriteLine("program ended");
		}
        
    }
}
