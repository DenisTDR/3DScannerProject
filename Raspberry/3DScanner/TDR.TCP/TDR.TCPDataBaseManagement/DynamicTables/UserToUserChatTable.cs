using System;
using System.Linq;
using TDR.TCPDataBaseManagement.DataAccess;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.DynamicTables
{
    public class UserToUserChatTable
    {
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                    return _name;
                if (User1Id.Equals(Guid.Empty) || User2Id.Equals(Guid.Empty))
                {
                    throw new InvalidOperationException(
                        "The UserIds cannot be all zero, they have to be set before accessing this property.");
                }

                return
                    _name =
                        string.Concat("messages_", User1Id.ToString().Replace("-", "").Substring(20), "_",
                            User2Id.ToString().Replace("-", "").Substring(20));
            }
        }

        private bool _exists, _existsChecked;
        public bool Exists
        {
            get
            {
                if (_existsChecked)
                    return _exists;
                using (var db = new DbUnit())
                {
                    var rez =
                        db.Context.Database.SqlQuery<int>(
                            "SELECT Count(*) from information_schema.TABLES WHERE TABLE_NAME = '" + Name + "';")
                            .Any(x => x == 1);
                    _existsChecked = true;
                    _exists = rez;
                    return rez;
                }
            }
        }

        public void MakeTheTable()
        {
            using (var db = new DbUnit())
            {
               // db.Context.Database.SqlQuery("")
            }
        }

        public static string NameFromUsers(Guid user1, Guid user2)
        {
            return string.Concat("messages_", user1.ToString().Replace("-", "").Substring(0, 12), "_",
                user2.ToString().Replace("-", "").Substring(0, 12));
        }
    }
}
