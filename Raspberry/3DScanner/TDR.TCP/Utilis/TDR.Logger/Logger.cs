using System;

namespace TDR.Logging
{
    public class Logger
    {
        private readonly string _preffix;
        private LogLevel _minLogLevel;
        public Logger(Type type, LogLevel minLogLevel = LogLevel.Debug)
        {
            _minLogLevel = minLogLevel;
            _preffix = "[" + type.Name + "]";
        }
        public Logger(string preffix, LogLevel minLogLevel = LogLevel.Debug)
        {
            _minLogLevel = minLogLevel;
            _preffix = "[" + preffix + "]";
        }
        public void Debug(string what, LogLocation logLocation = LogLocation.Default)
        {
            LogThis(what, logLocation, LogLevel.Debug);
        }
        public void Info(string what, LogLocation logLocation = LogLocation.Default)
        {
            LogThis(what, logLocation, LogLevel.Info);
        }
        public void Warn(string what, LogLocation logLocation = LogLocation.Default)
        {
            LogThis(what, logLocation, LogLevel.Warn);
        }
        public void Error(string what, LogLocation logLocation = LogLocation.Default)
        {
            LogThis(what, logLocation, LogLevel.Error);
        }
        public void Fatal(string what, LogLocation logLocation = LogLocation.Default)
        {
            LogThis(what, logLocation, LogLevel.Fatal);
        }
        private void LogThis(string what, LogLocation logLocation = LogLocation.Default,
            LogLevel logLevel = LogLevel.Debug)
        {
            if (LoggerHandler.MinLogLevel <= logLevel && _minLogLevel <= logLevel)
                LoggerHandler.LogAsync(string.Concat("[" + DateTime.Now + "]", _preffix, "[" + logLevel + "] ", what),
                    logLocation);
        }


    }

}
