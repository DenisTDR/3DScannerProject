using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TDR.ChatLibrary.Messages
{
    public class MessageDeserializingErrorInfo
    {
        public Exception Exception { get; set; }
        public string Json { get; set; }
    }
}
