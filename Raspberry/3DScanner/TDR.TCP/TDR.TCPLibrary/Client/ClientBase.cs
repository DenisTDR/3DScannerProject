using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TDR.Logging;
using TDR.TCPLibrary.StreamMaster;

namespace TDR.TCPLibrary.Client
{
    public class ClientBase : IClientBase
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        public event ClientBaseDisconnectedEventHandler Disconnected;
        public event ClientBaseMessageReceivedEventHandler MessageReceived;
        public event ClientBaseConnectionStateChangedEventHandler ConnectionStateChanged;
        public string Tag { get; set; }
        public bool AutoReconnect { get; }
        public bool Connected => _tcpClient != null && _tcpClient.Connected;
        private bool _disconnectedEventSent = false;

        private TcpClient _tcpClient;
        private IReaderBase _reader;
        private IWriterBase _writer;
        private readonly IPEndPoint _endPoint;
        public ClientBaseConnectionState ClientBaseConnectionState { get; private set; } = ClientBaseConnectionState.WaitingForConnect;
        public ClientBase(TcpClient tcpClient)
        {
            Logger.Debug("[" + Tag + "]ClientBase constructor.");
            AutoReconnect = false;
            this._tcpClient = tcpClient;
            if (tcpClient.Connected)
            {
                _endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                ClientBaseConnectionState = ClientBaseConnectionState.TcpConnected;
            }
            else
            {
                _endPoint = null;
            }
        }

        public ClientBase(string host, int port)
        {
            Logger.Debug("[" + Tag + "]ClientBase constructor.");
            AutoReconnect = false;
            IPAddress ipAddr;
            if (!IPAddress.TryParse(host, out ipAddr))
            {
                ipAddr = Dns.GetHostEntry(host).AddressList.FirstOrDefault();
            }
            if (ipAddr == null)
                throw new InvalidOperationException("Can't resolve host/dns");

            _endPoint = new IPEndPoint(ipAddr, port);
        }

        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    OnConnectionStateChanged(ClientBaseConnectionState.Connecting);
                    this._tcpClient = new TcpClient();
                    this._tcpClient.BeginConnect(_endPoint.Address.ToString(), _endPoint.Port, TcpClientConnect, this._tcpClient);
                    return true;
                }
            }
            catch (Exception exc)
            {
                Logger.Error("1[" + Tag + "]Can't connect to " + _endPoint + "\n" + exc);
                OnConnectionStateChanged(ClientBaseConnectionState.CantConnect);
            }
            return false;
        }

        private void TcpClientConnect(IAsyncResult asyncResult)
        {
            try
            {
                this._tcpClient.EndConnect(asyncResult);
                BindShits();
                OnConnectionStateChanged(ClientBaseConnectionState.TcpConnected);
            }
            catch (Exception exc)
            {
                Logger.Error("2[" + Tag + "]Can't connect to " + _endPoint + "\n" + exc);
                OnConnectionStateChanged(ClientBaseConnectionState.CantConnect);
            }
        }

        private bool _shitsBinded = false;
        public bool BindShits()
        {
            if (_shitsBinded) return false;
            _shitsBinded = true;
            if (_tcpClient == null || !_tcpClient.Connected)
            {
                Logger.Warn("[" + Tag + "]Can't BindShits in ClientBase because the tcpClient isn't connected.");
                return false;
            }
            _reader = new NetworkStreamReaderBase(_tcpClient.GetStream());
            _writer = new NetworkStreamWriterBase(_tcpClient.GetStream());
            _reader.Tag = this.Tag + "->ClientBase";
            _writer.Tag = this.Tag + "->ClientBase";
            _reader.MessageReceived += Reader_MessageReceived;
            _reader.CantRead += Reader_CantRead;
            _writer.CantWrite += Writer_CantWrite;
            _reader.Start();
            _writer.Start();

            Logger.Debug("[" + Tag + "]ClientBase BindShits ok.");
            return true;
        }

        private void UnBindShits()
        {
            if (_reader != null)
            {
                _reader.Stop();
                _reader.MessageReceived -= Reader_MessageReceived;
                _reader.CantRead -= Reader_CantRead;
                _reader = null;
            }
            if (_writer != null)
            {
                _writer.Stop();
                _writer.CantWrite -= Writer_CantWrite;
                _writer = null;
            }
        }
        private void Writer_CantWrite(NetworkStreamWriterBase sender, ConsoleApplication1.StoppedReason stoppedReason,
            object info)
        {
            _reader?.Stop();
            _writer?.Stop();
            if (!_disconnectedEventSent)
            {
                _disconnectedEventSent = true;
                Disconnected?.Invoke(this, info);
                OnConnectionStateChanged(ClientBaseConnectionState.Disconnected);
                UnBindShits();
            }
            else
            {
                _disconnectedEventSent = false;
            }
        }

        private void Reader_CantRead(NetworkStreamReaderBase sender, ConsoleApplication1.StoppedReason stoppedReason,
            object info)
        {
            _reader?.Stop();
            _writer?.Stop();
            if (!_disconnectedEventSent)
            {
                _disconnectedEventSent = true;
                Disconnected?.Invoke(this, info);
                OnConnectionStateChanged(ClientBaseConnectionState.Disconnected);
                UnBindShits();
            }
            else
            {
                _disconnectedEventSent = false;
            }
        }

        private void Reader_MessageReceived(NetworkStreamReaderBase sender, byte[] message)
        {
            Logger.Debug("[" + Tag + "]ClientBase received message with length: " + message.Length);
            MessageReceived?.Invoke(this, message);
        }

        public void Disconnect()
        {
            _writer?.Stop();
            _reader?.Stop();
            _tcpClient?.Close();
        }

        public bool SendMessage(byte[] message)
        {
            if (this.ClientBaseConnectionState != ClientBaseConnectionState.TcpConnected)
                return false;
            return _writer.WriteMessage(message);
        }

        private void OnConnectionStateChanged(ClientBaseConnectionState clientBaseConnectionState)
        {
            if (clientBaseConnectionState == this.ClientBaseConnectionState) return;
            this.ClientBaseConnectionState = clientBaseConnectionState;
            ConnectionStateChanged?.Invoke(this, clientBaseConnectionState);
        }
    }

    public delegate void ClientBaseDisconnectedEventHandler(IClientBase sender, object message);

    public delegate void ClientBaseMessageReceivedEventHandler(IClientBase sender, byte[] message);

    public delegate void ClientBaseConnectionStateChangedEventHandler(
        IClientBase sender, ClientBaseConnectionState clientBaseConnectionState);
}