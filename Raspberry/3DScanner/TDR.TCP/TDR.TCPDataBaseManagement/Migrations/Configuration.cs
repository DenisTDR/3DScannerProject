using TDR.TCPDataBaseManagement.DataAccess;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TDR.TCPDataBaseManagement.DataAccess.DbContextTdr>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TDR.TCPDataBaseManagement.DataAccess.DbContextTdr context)
        {
            using (var db = new DbUnit())
            {
                var accRepo = db.Repo<AccountModel>();
                var userRepo = db.Repo<UserModel>();
                var acc1 = new AccountModel()
                {
                    Username = "tdr",
                    Password = "romania"
                };
                var acc2 = new AccountModel()
                {
                    Username = "user1",
                    Password = "romania"
                };
                var user1 = new UserModel()
                {
                    Nick = "TDR",
                    Username = "tdr"
                };
                var user2 = new UserModel()
                {
                    Nick = "User1",
                    Username = "user1"
                };
                userRepo.Add(user1).Add(user2);
                acc1.User = user1;
                acc2.User = user2;
                accRepo.Add(acc1).Add(acc2);
                user1.AccountId = acc1.Id;
                user2.AccountId = acc2.Id;
                userRepo.Update(user1, user1.Id);
                userRepo.Update(user2, user2.Id);
            }
        }
    }
}
