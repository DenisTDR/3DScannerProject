using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.ViewModels;

namespace TDR.TCPDataBaseManagement
{
    public interface IUserRepository
    {
        bool CheckCredentials(string user, string password, out string info);
        List<UserViewModel> GetUsers();
        UserViewModel GetUser(string username);
    }
}
