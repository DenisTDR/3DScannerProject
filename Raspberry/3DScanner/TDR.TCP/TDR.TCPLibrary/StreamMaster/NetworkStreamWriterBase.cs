using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication1;
using TDR.Logging;
using UtilisAndExtensionsLibrary;

namespace TDR.TCPLibrary.StreamMaster
{
    public class NetworkStreamWriterBase : IWriterBase
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        public string Tag { get; set; }
        protected NetworkStream NetworkStream;
        private InfiniteLoop _infiniteLoop;
        private readonly ConcurrentQueue<byte[]> _waitingMessages;
        private readonly AutoResetEvent _waiter;
        public bool Working => _infiniteLoop != null && _infiniteLoop.Running;
        public bool CanWrite => NetworkStream != null && NetworkStream.CanWrite;
        public event NetworkStreamWriterBaseCantWriteEventHandler CantWrite;
        private bool _cantWriteEventFired = false;
        public NetworkStreamWriterBase(NetworkStream newtworkStream)
        {
            this.NetworkStream = newtworkStream;
            _waitingMessages = new ConcurrentQueue<byte[]>();
            _waiter = new AutoResetEvent(false);
        }

        public void Start()
        {
            if (_infiniteLoop != null && _infiniteLoop.Running)
            {
                Logger.Debug("[" + Tag + "]NetworkStreamWriterBase already working.");
                return;
            }
            Logger.Debug("[" + Tag + "]NetworkStreamWriterBase Start.");
            _infiniteLoop = new InfiniteLoop(WriteMessagesIntoStream, 5);
            _infiniteLoop.ExceptionOccurred += InfiniteLoop_ExceptionOccurred;
            _infiniteLoop.StoppedEvent += InfiniteLoop_StoppedEvent;
            _infiniteLoop.Started += _infiniteLoop_Started;
            _infiniteLoop.Tag = this.Tag + "->Writer";
            _infiniteLoop.Start();
        }

        private void _infiniteLoop_Started()
        {
            Interlocked.Increment(ref Counters.WriterThreads);
        }

        public void Stop()
        {
            if (!Working)
            {
                Logger.Debug("[" + Tag + "]NetworkStreamWriterBase not working.");
                return;
            }
            Logger.Debug("[" + Tag + "]NetworkStreamWriterBase Stop. " + _infiniteLoop.Running);
            _infiniteLoop?.Stop();
            _waiter.Set();
        }

        public bool WriteMessage(byte[] message)
        {
            if (!this.CanWrite)
            {
                Logger.Debug("[" + Tag + "]Can't enqueue this message!");
                return false;
            }
            _waitingMessages.Enqueue(message);
            _waiter.Set();
            return true;
        }

        private void InfiniteLoop_StoppedEvent(StoppedReason stoppedReason)
        {
            Interlocked.Decrement(ref Counters.WriterThreads);
            Logger.Debug("[" + Tag + "]NetworkStreamWriterBase stopped!: " + stoppedReason);
            OnCantWrite();
        }

        private void InfiniteLoop_ExceptionOccurred(InfiniteLoop sender, Exception exc)
        {
            Logger.Error("[" + Tag + "]An error occurred in NetworkStreamWriterBase: " + exc);
            OnCantWrite();
        }

        protected void WriteMessagesIntoStream()
        {
            _waiter.WaitOne();
            while (_waitingMessages.Count != 0)
            {
                byte[] message;
                if (!_waitingMessages.TryDequeue(out message))
                {
                    Logger.Debug("[" + Tag + "]Can't get message from concurrentQueue!");
                    Thread.Sleep(100);
                    continue;
                }
                Logger.Debug("[" + Tag + "]Writing message with length: " + message.Length);
                if (!CanWrite)
                {
                    Logger.Debug("[" + Tag + "]Can't write into networkStream because it is closed.");
                    _infiniteLoop?.Stop();
                    OnCantWrite();
                    continue;
                }

                var length = message.Length;
                var lengthInBytes = BitConverter.GetBytes(length);
                try
                {
                    NetworkStream.Write(lengthInBytes, 0, lengthInBytes.Length);
                    NetworkStream.Write(message, 0, length);
                    NetworkStream.Flush();
                }
                catch (IOException exc)
                {
                    var sexc = exc.InnerException as SocketException;
                    if (sexc?.SocketErrorCode != SocketError.ConnectionAborted &&
                        sexc?.SocketErrorCode != SocketError.ConnectionReset) throw;
                    OnCantWrite(StoppedReason.Disconnected, exc);
                    return;
                }
            }
        }

        protected void OnCantWrite(StoppedReason stoppedReason = StoppedReason.Unknown, object info = null)
        {
            if (!_cantWriteEventFired)
            {
                _cantWriteEventFired = true;
                _infiniteLoop?.Stop();
                CantWrite?.Invoke(this, stoppedReason, null);
            }
        }
    }

    public delegate void NetworkStreamWriterBaseCantWriteEventHandler(
        NetworkStreamWriterBase sender, StoppedReason stoppedReason, object info);
}