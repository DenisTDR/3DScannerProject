using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPLibrary.StreamMaster
{
    public interface IReaderBase
    {
        string Tag { get; set; }
        bool Working { get; }
        bool CanRead { get; }
        bool Start();
        void Stop();

        event NetworkStreamReaderBaseCantReadEventHandler CantRead;
        event NetworkStreamReaderBaseMessageReceivedEventHandler MessageReceived;
    }
}
