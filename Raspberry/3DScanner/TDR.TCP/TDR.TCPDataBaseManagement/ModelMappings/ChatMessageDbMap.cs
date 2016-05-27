using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.ModelMappings
{
    public class ChatMessageDbMap: EntityTypeConfiguration<ChatMessageModel>
    {
        public ChatMessageDbMap()
        {
            HasKey(x => x.Id);
            ToTable("ChatMessage");
        }
    }
}
