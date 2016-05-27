using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UtilisAndExtensionsLibrary;

namespace TDR.Logging
{
    public class LoggerHandler
    {
        private static List<Action<string>> _customLogActions;
        private static readonly Dictionary<string, Logging.Logger> RegisteredLoggers = new Dictionary<string, Logging.Logger>();
        private static LogLocation _logLocation;
        private static Thread _initingThread;
        private static bool inited = false;
        public static LogLevel MinLogLevel { get; private set; }

        public static void Init(IEnumerable<Action<string>> customLogActions, string fileName, LogLocation logLocation = LogLocation.Both, LogLevel minLogLevel = LogLevel.Debug)
        {
            inited = true;
            _initingThread = Thread.CurrentThread;
            
            _customLogActions = customLogActions.ToList();
            _logLocation = logLocation;
            MinLogLevel = minLogLevel;
            (new Thread(WritingLoop)).Start();
            (new Thread(CheckAliveAndFlushLoop)).Start();

            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (!fi.Exists)
                    {
                        if (fi.DirectoryName == null)
                        {
                            return;
                        }
                        if (!Directory.Exists(fi.DirectoryName))
                            Directory.CreateDirectory(fi.DirectoryName);
                    }

                    _streamWriter = new StreamWriter(fileName, true);
                    _streamWriter.AutoFlush = true;
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Can't open file to write logs.\n" + exc);
                    _customLogActions?.ForEach(x => x?.Invoke("Can't open file to write logs.\n" + exc));
                    _streamWriter = null;
                }
            }
        }

        public static void Destroy()
        {
            _shouldWrite = false;
            Thread.Sleep(100);
            Waiter.Set();
        }

        public static Logger RegisterLogger(Type type, LogLevel minLogLevel = LogLevel.Debug)
        {
            return RegisterLogger(type.Name, minLogLevel);
        }
        public static Logger RegisterLogger(string preffix, LogLevel minLogLevel = LogLevel.Debug)
        {
            if (RegisteredLoggers.ContainsKey(preffix))
            {
                return RegisteredLoggers[preffix];
            }
            return RegisteredLoggers[preffix] = new Logger(preffix, minLogLevel);
        }

        private static StreamWriter _streamWriter;
        public static void LogAsync(string message, LogLocation logLocation = LogLocation.Default)
        {
            if (!inited)
            {
                Init(new[] { (Action<string>)Console.WriteLine }, "");
                LogAsync("Logger was not inited, just autoinited!");
            }
            if (logLocation == LogLocation.Default)
                logLocation = _logLocation;
            WaitingLogs.Enqueue(new Tuple<string, LogLocation>(message, logLocation));
            Waiter.Set();
        }

        public static void LogSync(string message, LogLocation logLocation = LogLocation.Default)
        {
          
            if (_customLogActions == null && _streamWriter == null)
            {
                Console.WriteLine("Can't write logs anywhere");
                return;
            }
            if (logLocation == LogLocation.Default)
                logLocation = _logLocation;
            if (logLocation == LogLocation.Both || logLocation == LogLocation.Console)
            {
                _customLogActions?.ForEach(x => x?.Invoke(message));
            }
            if (logLocation == LogLocation.Both || logLocation == LogLocation.File)
            {
                try
                {
                    if (_streamWriter != null)
                    {
                        _streamWriter.WriteLine(message);
                        _somethingWritten = true;
                    }
                }
                catch (Exception exc)
                {
                    _streamWriter = null;
                }
            }
        }



        private static readonly AutoResetEvent Waiter = new AutoResetEvent(false);
        private static bool _shouldWrite = true;
        private static void WritingLoop()
        {
            Interlocked.Increment(ref Counters.LoggingWriteThreads);
            LogSync("Loggers Inited.");
            while (_shouldWrite)
            {
                Waiter.WaitOne();
                while (WaitingLogs.Count > 0)
                {
                    Tuple<string, LogLocation> tmp;
                    if (WaitingLogs.TryDequeue(out tmp))
                    {
                        LogSync(tmp.Item1, tmp.Item2);
                    }
                    else
                    {
                        Console.WriteLine("cant deque");
                    }
                }
            }
            LogSync("Loggers Destroyed.");
            _streamWriter = null;
            RegisteredLoggers.Clear();
            Interlocked.Decrement(ref Counters.LoggingWriteThreads);
        }

        private static bool _somethingWritten = false;
        private static readonly ConcurrentQueue<Tuple<string, LogLocation>> WaitingLogs =
            new ConcurrentQueue<Tuple<string, LogLocation>>();

        private static void CheckAliveAndFlushLoop()
        {
            Interlocked.Increment(ref Counters.LoggingFlushThreads);
            while (_shouldWrite)
            {
                
                Thread.Sleep(2000);
                if (_somethingWritten)
                {
                    _somethingWritten = false;
                    _streamWriter?.Flush();
                }
                if (!_initingThread.IsAlive)
                {
                    _shouldWrite = false;
                    Waiter.Set();
                }
            }

            Interlocked.Decrement(ref Counters.LoggingFlushThreads);
        }

        public static void LogCounters()
        {
            string msg = "\n\t\tWriterThreads=" + Counters.WriterThreads +
                         "\n\t\tReaderThreads=" + Counters.ReaderThreads +
                         "\n\t\tKeepAliveThreads=" + Counters.KeepAliveThreads +
                         "\n\t\tActiveInfiniteLoops=" + Counters.ActiveInfiniteLoops +
                         "\n\t\tLoggingFlushThreads=" + Counters.LoggingFlushThreads +
                         "\n\t\tLoggingWriteThreads=" + Counters.LoggingWriteThreads;
            LogAsync("[Counters]" + msg);
        }

    }
    public enum LogLocation
    {
        Console,
        Both,
        File,
        Default
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}
