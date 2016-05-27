using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDR.TCPDataBaseManagement.Models
{
    public class ChatMessageModel : BaseModel
    {
        public Guid SenderId { get; set; }

        [ForeignKey("SenderId")]
        [Required]
        public virtual UserModel Sender { get; set; }


        public Guid RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        [Required]
        public virtual UserModel Recipient { get; set; }
        public string Message { get; set; }
    }
}