using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDR.TCPDataBaseManagement.Models
{
    public class BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
    }
}
