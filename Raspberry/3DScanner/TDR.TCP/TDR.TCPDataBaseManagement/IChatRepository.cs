using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.ViewModels;

namespace TDR.TCPDataBaseManagement
{
    public interface IChatRepository
    {
        bool SaveTextMessage(ChatMessageViewModel chatMessage);

        //List<UserViewModel> 
    }
}
