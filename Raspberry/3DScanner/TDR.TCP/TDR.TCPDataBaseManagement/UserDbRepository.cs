using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.DataAccess;
using TDR.TCPDataBaseManagement.Models;
using TDR.TCPDataBaseManagement.ViewModels;

namespace TDR.TCPDataBaseManagement
{
    public class UserDbRepository : IUserRepository
    {
        public bool CheckCredentials(string user, string password, out string info)
        {
            info = null;
         //   return true;
            try
            {
                using (var db = new DbUnit())
                {
                    if (db.Repository<AccountModel>().Any(x => x.Username.Equals(user) && x.Password.Equals(password)))
                    {
                        info = null;
                        return true;
                    }
                    else
                    {
                        info = "Invalid user/password!";
                        return false;
                    }
                }
            }
            catch (Exception exc)
            {
                info = "Error ocurred: " + exc;
            }
            return false;
        }

        public List<UserViewModel> GetUsers()
        {
            using (var db = new DbUnit())
            {
                return
                    db.Repository<UserModel>()
                        .GetAll()
                        .Select(user => new UserViewModel {Nick = user.Nick, Username = user.Username})
                        .ToList();
            }
        }

        public UserViewModel GetUser(string username)
        {
            using (var db = new DbUnit())
            {
                return
                    db.Repository<UserModel>()
                        .FindAll(x => x.Username == username)
                        .Select(user => new UserViewModel {Username = user.Username, Nick = user.Nick})
                        .FirstOrDefault();
            }
        }
    }
}
