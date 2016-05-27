using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPLibrary
{
    public enum ClientBaseConnectionState
    {
        WaitingForConnect,
        Connecting,
        TcpConnected,
        NotConnected,
        Disconnected,
        CantConnect
    }
}
