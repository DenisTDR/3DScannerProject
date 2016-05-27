using System;
using System.Collections.Generic;
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
    public class NetworkStreamReaderBase : IReaderBase
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        public string Tag { get; set; }
        public event NetworkStreamReaderBaseCantReadEventHandler CantRead;
        public event NetworkStreamReaderBaseMessageReceivedEventHandler MessageReceived;
        private InfiniteLoop _infiniteLoop;
        protected NetworkStream NetworkStream;
        private readonly AutoResetEvent _waiter;


        public bool Working => _infiniteLoop != null && _infiniteLoop.Running;
        public bool CanRead => NetworkStream != null && NetworkStream.CanRead;

        public NetworkStreamReaderBase(NetworkStream networkStream)
        {
            this.NetworkStream = networkStream;
            _waiter = new AutoResetEvent(false);
        }

        public bool Start()
        {
            if (Working)
            {
                Logger.Debug("[" + Tag + "]NetworkStreamReaderBase already working.");
                return false;
            }
            _infiniteLoop = new InfiniteLoop(ReadSomething, 4);
            _infiniteLoop.ExceptionOccurred += InfiniteLoop_ExceptionOccurred;
            _infiniteLoop.StoppedEvent += InfiniteLoop_StoppedEvent;
            _infiniteLoop.Started += _infiniteLoop_Started;
            _infiniteLoop.Tag = this.Tag + "->Reader";
            _infiniteLoop.Start();
            return true;
        }

        private void _infiniteLoop_Started()
        {
            Interlocked.Increment(ref Counters.ReaderThreads);
        }

        public void Stop()
        {
            if (!Working)
            {
                Logger.Debug("[" + Tag + "]NetworkStreamReaderBase not working.");
                return;
            }
            Logger.Debug("[" + Tag + "]NetworkStreamReaderBase Stop.");
            _infiniteLoop?.Stop();
            _waiter.Set();
        }

        private int _bytesToRead = sizeof (int);
        private int _bytesRead = 0;
        private byte[] buffer = new byte[1024*10];
        private bool readingLength = true;

        private void ReadSomething()
        {
            try
            {
                if (!this.CanRead)
                {
                    return;
                }
                NetworkStream.BeginRead(buffer, _bytesRead, _bytesToRead - _bytesRead, SomethingHasRead, NetworkStream);
            }
            catch (Exception exc)
            {
                var sexc = exc.InnerException as SocketException;
                if (sexc?.SocketErrorCode != SocketError.ConnectionAborted &&
                    sexc?.SocketErrorCode != SocketError.ConnectionReset && this.CanRead)
                    throw;
                OnCantRead(StoppedReason.Disconnected, exc);
                return;
            }
            _waiter.WaitOne();
        }

        private void SomethingHasRead(IAsyncResult result)
        {
            if (!this.CanRead)
            {
                return;
            }
            int bytesReadHere;
            try
            {
                bytesReadHere = NetworkStream.EndRead(result);
            }
            catch (Exception exc)
            {
                var sexc = exc.InnerException as SocketException;
                if (sexc?.SocketErrorCode != SocketError.ConnectionAborted &&
                    sexc?.SocketErrorCode != SocketError.ConnectionReset)
                    throw;
                OnCantRead(StoppedReason.Disconnected, exc);
                return;
            }
            _bytesRead += bytesReadHere;
            _bytesToRead -= bytesReadHere;
            if (_bytesToRead == 0)
            {
                if (readingLength)
                {
                    _bytesToRead = BitConverter.ToInt32(buffer, 0);
                    _bytesRead = 0;
                    readingLength = false;
                    if (_bytesToRead == 0)
                    {
                        
                    }
                }
                else
                {
                    if (MessageReceived != null)
                    {
                        var tmpBuffer = new byte[_bytesRead];
                        for (var i = 0; i < _bytesRead; i++)
                            tmpBuffer[i] = buffer[i];
                        MessageReceived(this, tmpBuffer);
                    }
                    _bytesToRead = sizeof (int);
                    _bytesRead = 0;
                    readingLength = true;
                }
            }
            _waiter.Set();
        }

        private void InfiniteLoop_StoppedEvent(StoppedReason stoppedReason)
        {
            Interlocked.Decrement(ref Counters.ReaderThreads);
            Logger.Debug("[" + Tag + "]NetworkStreamReaderBase stopped: " + stoppedReason);
        }

        private void InfiniteLoop_ExceptionOccurred(InfiniteLoop sender, Exception exc)
        {
            if (exc.InnerException is SocketException)
            {
                var sexc = (SocketException) exc.InnerException;
                Logger.Error("[" + Tag + "]NetworkStreamReaderBase SocketException (" + sexc.SocketErrorCode + "): " +
                             exc);
            }
            else
            {
                Logger.Error("[" + Tag + "]NetworkStreamReaderBase exc: " + exc);
            }
        }

        protected void OnCantRead(StoppedReason stoppedReason = StoppedReason.Unknown, object info = null)
        {
            _infiniteLoop?.Stop();
            CantRead?.Invoke(this, StoppedReason.Exceptions, info);
        }

    }

    public delegate void NetworkStreamReaderBaseCantReadEventHandler(
        NetworkStreamReaderBase sender, StoppedReason stoppedReason, object info);

    public delegate void NetworkStreamReaderBaseMessageReceivedEventHandler(
        NetworkStreamReaderBase sender, byte[] message);
}
