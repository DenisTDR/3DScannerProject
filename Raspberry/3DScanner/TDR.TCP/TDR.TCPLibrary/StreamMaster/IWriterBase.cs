using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPLibrary.StreamMaster
{
    public interface IWriterBase
    {
        string Tag { get; set; }
        bool Working { get; }
        bool CanWrite { get; }
        void Start();
        void Stop();
        bool WriteMessage(byte[] message);

        event NetworkStreamWriterBaseCantWriteEventHandler CantWrite;
    }
}
