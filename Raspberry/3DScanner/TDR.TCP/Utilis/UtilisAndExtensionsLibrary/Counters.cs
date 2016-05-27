using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilisAndExtensionsLibrary
{
    public static class Counters
    {
        public static int WriterThreads = 0;
        public static int ReaderThreads = 0;
        public static int KeepAliveThreads = 0;
        public static int ActiveInfiniteLoops = 0;
        public static int LoggingFlushThreads = 0;
        public static int LoggingWriteThreads = 0;

        
    }
}
