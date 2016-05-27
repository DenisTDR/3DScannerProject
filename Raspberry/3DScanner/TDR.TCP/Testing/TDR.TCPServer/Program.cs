using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TDR.ChatLibrary.Server;
using TDR.Logging;
using TDR.TCPDataBaseManagement;

namespace TDR.TCPServer
{
    class Program
    {
        private static IChatServer _sb;
        private static readonly Logger Logger = LoggerHandler.RegisterLogger("ServerMain");

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                StartChatServer();
                return;
            }
            switch (args[0])
            {

                case "stop":
                    Stop();
                    break;

                case "restart":
                    Stop();
                    Start();
                    break;

                case "start":
                    Start();
                    break;

                case "normalStart":
                    StartChatServer(false);
                    break;
            }

        }

        private static void Start()
        {
            if (IsStarted())
            {
                Console.WriteLine("Chat Server is already running!");
                return;
            }
            var crtProc = Process.GetCurrentProcess();
            string fileName, args;
            if (IsLinux)
            {
                fileName = "mono";
                args = crtProc.MainModule.FileName + " " + "normalStart";
            }
            else
            {
                fileName = crtProc.MainModule.FileName;
                args = "normalStart";
            }
            var newP = new Process
            {

                StartInfo =
                {
                    FileName = fileName,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(crtProc.MainModule.FileName) ?? "/home"
                }
            };

            Console.WriteLine("Starting Chat Server ...");
            newP.Start();
            Console.WriteLine("Done!");
        }
        public static bool IsLinux
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        private static void Stop()
        {
            if (!IsStarted())
            {
                Console.WriteLine("Chat Server is not running!");
                return;
            }
            var pid = GetPidFromFile();
            try
            {
                var proc = Process.GetProcessById(pid);
                Console.WriteLine("Stopping Chat Server ...");
                proc.Kill();
                proc.WaitForExit();
                Console.WriteLine("Done!");
            }
            catch (Exception exc)
            {
                Console.WriteLine("An error ocurred: " + exc.Message);
            }
        }

        private static bool IsStarted(bool output=false)
        {
            int pid = GetPidFromFile();
            if (pid == -1)
            {
                return false;
            }
            try
            {
                Process.GetProcessById(pid);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int GetPidFromFile(bool output =false)
        {
            try
            {
                var sr = new StreamReader("pid.pid");
                var pidS = sr.ReadLine();
                sr.Close();
                int pid;
                if (!int.TryParse(pidS, out pid))
                {
                    return -1;
                }
                return pid;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static void StartChatServer(bool waitForInput=true)
        {
            var pid = Process.GetCurrentProcess().Id;
            var sw = new StreamWriter("pid.pid");
            sw.WriteLine(pid.ToString());
            sw.Close();

            LoggerHandler.Init(new[] { (Action<string>)Console.WriteLine }, "Logs/ServerLogs.log", minLogLevel: LogLevel.Warn);
            var userRepository = new UserDbRepository();
            _sb = new ChatServer("*", 10240, userRepository);

            new Thread(() =>
            {
                Thread.Sleep(5000);
                while (_sb.IsAlive)
                {
                    Logger.Warn(_sb.NumberOfActiveConnections + "/" + _sb.NumberOfReferencedClients);
                    LoggerHandler.LogCounters();
                    Thread.Sleep(10000);
                }
            }).Start();

            if (waitForInput)
            {
                while (true)
                {
                    Console.WriteLine("Input: ");
                    var s = Console.ReadLine();
                    if (s == "exit") break;
                    Logger.Warn(_sb.NumberOfActiveConnections + "/" + _sb.NumberOfReferencedClients);
                    LoggerHandler.LogCounters();
                }

                _sb.StopActivity();
            }
            while (_sb.IsAlive)
                Thread.Sleep(250);
            Thread.Sleep(1000);
            Logger.Error("Server end!");
            LoggerHandler.Destroy();
        }
    }
}
