using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPLibrary.Client
{
    public interface IClientBase
    {
        string Tag { get; set; }
        event ClientBaseDisconnectedEventHandler Disconnected;
        event ClientBaseMessageReceivedEventHandler MessageReceived;
        bool AutoReconnect { get; }
        bool Connected { get; }

        bool Connect();
        bool BindShits();
        void Disconnect();
        bool SendMessage(byte[] message);
        ClientBaseConnectionState ClientBaseConnectionState { get; }
        event ClientBaseConnectionStateChangedEventHandler ConnectionStateChanged;
    }
}
