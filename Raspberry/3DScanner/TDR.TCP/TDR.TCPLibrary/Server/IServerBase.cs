using TDR.TCPLibrary.Client;

namespace TDR.TCPLibrary.Server
{
    public interface IServerBase
    {
        void StartListening();
        void StopListening();
        bool IsListening { get; }
        int Port { get; }
        string ListenIp { get; }
        void Broadcast(byte[] message);
        void KillConnections();
        void DisownClient(IClientBase client);

        event ServerBaseClientConnectedEventHandler ClientConnected;
        event ServerBaseClientDisconnectedEventHandler ClientDisconnected;
    }
}
