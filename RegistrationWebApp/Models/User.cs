using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationWebApp.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            RegistrationDate = DateTime.Now;
            LoginDate = DateTime.Now;
            IsBlocked = false;
        }

        public DateTime RegistrationDate { get; set; }
        public DateTime LoginDate { get; set; }
        public bool IsBlocked { get; set; }
    }
}
