using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.ChatLibrary.Server
{
    public interface IChatServer
    {
        bool IsAlive { get; }
        int NumberOfActiveConnections { get; }
        int NumberOfReferencedClients { get; }
        void StopActivity();
        void KillConnections();
    }
}
