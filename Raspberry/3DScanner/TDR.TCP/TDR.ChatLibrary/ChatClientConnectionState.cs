using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.ChatLibrary
{
    public enum ChatClientConnectionState
    {
        WaitingForConnect,
        Connecting,
        Connected,
        LoggingIn,
        NotConnected,
        Disconnected,
        CantConnect,
        LoggedIn,
        LoggingInFailed,
        WaitingWelcomeMessage
    }
}
