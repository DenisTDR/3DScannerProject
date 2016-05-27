using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPDataBaseManagement.ViewModels
{
    public class ChatMessageViewModel
    {
        public UserViewModel Sender { get; set; }
        public UserViewModel Recipient { get; set; }
        public string Message { get; set; }
    }
}
