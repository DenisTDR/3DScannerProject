using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDR.TCPDataBaseManagement.Models
{
    public class AccountModel : BaseModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [Required]
        public virtual UserModel User { get; set; }
    }
}