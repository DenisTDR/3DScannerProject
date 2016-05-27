using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPDataBaseManagement.Models
{
    public class UserToUserConversationModel : BaseModel
    {
        public Guid FirstId { get; set; }

        [ForeignKey("FirstId")]
        [Required]
        public virtual UserModel First { get; set; }

        public Guid SecondId { get; set; }

        [ForeignKey("SecondId")]
        [Required]
        public virtual UserModel Second { get; set; }
    }
}
