using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DScannerProgram
{
    public class SettingsContainer
    {
        public int PwmPeriod { get; set; }
        public int PwmHighDuration { get; set; }
        public int LinesToSkip { get; set; }
        public string TcpServerIp { get; set; }
    }
}
