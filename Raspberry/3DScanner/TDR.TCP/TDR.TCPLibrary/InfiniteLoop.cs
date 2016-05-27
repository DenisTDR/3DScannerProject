using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDR.Logging;
using UtilisAndExtensionsLibrary;

namespace ConsoleApplication1
{
    public class InfiniteLoop
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string Tag { get; set; }
        public event StoppedEventHandler StoppedEvent;
        public event ExceptionOccurredEventHandler ExceptionOccurred;
        public event StartedEventHandler Started;
        public bool Running { get; private set; }
        private readonly Action _mainAction;
        private int _remainingErrors;
        private Thread _mainThread;
        private bool _shouldRun;
        public InfiniteLoop(Action mainAction, int maxErrorCountBeforeStop )
        {
            Running = _shouldRun = false;
            this._mainAction = mainAction;
            _remainingErrors = maxErrorCountBeforeStop;
        }

        public void Start()
        {
            if (Running)
                return;
            (_mainThread = new Thread(TheLoop)).Start();
        }

        public void Stop()
        {
            _shouldRun = false;
        }

        private static int cnt = 0;
        void TheLoop()
        {
            Interlocked.Increment(ref Counters.ActiveInfiniteLoops);
            _shouldRun = Running = true;
            var stoppedReason = StoppedReason.Command;
            Started?.Invoke();
            while (_shouldRun)
            {
                cnt++;
                try
                {
                    _mainAction();
                }
                catch (Exception exc)
                {
                    ExceptionOccurred?.Invoke(this, exc);
                    _remainingErrors--;
                    if (_remainingErrors == 0)
                    {
                        stoppedReason = StoppedReason.Exceptions;
                        _shouldRun = false;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                cnt --;
            }
            Interlocked.Decrement(ref Counters.ActiveInfiniteLoops);
            Logger.Debug("[" + Tag + "]InfiniteLoop ended! ");
            Running = false;
            StoppedEvent?.Invoke(stoppedReason);
        }
    }
    public delegate void StoppedEventHandler(StoppedReason stoppedReason);
    public delegate void StartedEventHandler();
    public delegate void ExceptionOccurredEventHandler(InfiniteLoop sender, Exception exc);

    public enum StoppedReason
    {
        Unknown,
        Command,
        Exceptions,
        Disconnected
    }
}
