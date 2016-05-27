﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDR.TCPDataBaseManagement.Models
{
    public class UserModel : BaseModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Nick { get; set; }
        public Guid AccountId { get; set; }
        
    }
}