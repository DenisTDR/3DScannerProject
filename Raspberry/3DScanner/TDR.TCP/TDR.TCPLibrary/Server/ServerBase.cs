using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using ConsoleApplication1;
using TDR.Logging;
using TDR.TCPLibrary.Client;

namespace TDR.TCPLibrary.Server
{
    public class ServerBase : IServerBase
    {
        private static readonly Logger Logger = LoggerHandler.RegisterLogger(MethodBase.GetCurrentMethod().DeclaringType, LogLevel.Debug);
        private TcpListener _listener;
        private InfiniteLoop _infiniteLoop;
        public int Port { get; }
        public string ListenIp { get; }
        public bool IsListening => _infiniteLoop != null && _infiniteLoop.Running;
        public List<IClientBase> Clients = new List<IClientBase>();

        public event ServerBaseClientConnectedEventHandler ClientConnected;
        public event ServerBaseClientDisconnectedEventHandler ClientDisconnected;

        public ServerBase(int port, string listenIp)
        {
            this.Port = port;
            this.ListenIp = listenIp;
        }

        public void StartListening()
        {
            if (IsListening)
            {
                Logger.Debug("Server Already running!");
                return;
            }
            OnStartListening();
        }

        public void StopListening()
        {
            if (_infiniteLoop != null && _infiniteLoop.Running)
            {
                _infiniteLoop.Stop();
                _listener?.Stop();
                _waiter.Set();
            }
        }

        public void KillConnections()
        {
            foreach (var clientBase in Clients)
            {
                clientBase.Disconnect();
            }
        }
        public void Broadcast(byte[] message)
        {
            foreach (var clientBase in Clients)
            {
                clientBase.SendMessage(message);
            }
        }
        protected void OnStartListening()
        {
            Logger.Debug("Starting Server Listener... #1");
            var ipAddress = ListenIp == "*" ? IPAddress.Any : IPAddress.Parse(ListenIp);
            _listener = new TcpListener(ipAddress, Port);
            _listener.Start();
            Logger.Debug("Starting Server Listener... #2");
            _infiniteLoop = new InfiniteLoop(ListenForClient, 10);
            _infiniteLoop.ExceptionOccurred += InfiniteLoop_ExceptionOccurred;
            _infiniteLoop.StoppedEvent += InfiniteLoop_StoppedEvent;
            _infiniteLoop.Tag = "ServerBase";
            _infiniteLoop.Start();
            Logger.Debug("Starting Server Listener... #3");
            Logger.Debug("Serverbase listening on " + ipAddress + ":" + Port);
        }

        private void InfiniteLoop_StoppedEvent(StoppedReason stoppedReason)
        {
            Logger.Debug("Server listening stopped!");
        }

        private void InfiniteLoop_ExceptionOccurred(InfiniteLoop sender, Exception exc)
        {
            Logger.Error("An error occurred in ServerBase: " + exc);
        }

        private readonly AutoResetEvent _waiter = new AutoResetEvent(false);
        protected void ListenForClient()
        {
            _listener.BeginAcceptTcpClient(SomeoneConnected, this);
            _waiter.WaitOne();
        }

        private void SomeoneConnected(IAsyncResult asyncResult)
        {
            if (!IsListening)
            {
                return;
            }
            try
            {
                var connectedClient = _listener.EndAcceptTcpClient(asyncResult);
                OnClientConnect(connectedClient);
                _waiter.Set();
            }
            catch (Exception exc)
            {
                // ignored
            }
        }

        protected void OnClientConnect(TcpClient client)
        {
            IClientBase clientBase = new ClientBase(client) {Tag = "ServerBase"};
            clientBase.Disconnected += ClientBase_Disconnected;
            clientBase.BindShits();
            Clients.Add(clientBase);
            
            Logger.Debug("someone connected");

            ClientConnected?.Invoke(this, clientBase);
        }

        private void ClientBase_Disconnected(IClientBase sender, object message)
        {
            Clients.Remove(sender);
            Logger.Debug("A client disconnected!");
            OnClientDisconnect(sender);
        }

        protected void OnClientDisconnect(IClientBase clientBase)
        {
            ClientDisconnected?.Invoke(this, clientBase);
        }

        public void DisownClient(IClientBase clientBase)
        {
            Clients.Remove(clientBase);
            clientBase.Disconnected -= ClientBase_Disconnected;
            //clientBase.MessageReceived -= ClientBase_Disconnected;
        }
    }

    public delegate void ServerBaseClientConnectedEventHandler(IServerBase sender, IClientBase clientBase);
    public delegate void ServerBaseClientDisconnectedEventHandler(IServerBase sender, IClientBase clientBase);
}