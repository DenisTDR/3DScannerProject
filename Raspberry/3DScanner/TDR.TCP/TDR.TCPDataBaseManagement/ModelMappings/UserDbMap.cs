using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.ModelMappings
{
    class UserDbMap : EntityTypeConfiguration<UserModel>
    {
        public UserDbMap()
        {
            HasKey(x => x.Id);
            ToTable("User");
        }
    }
}
