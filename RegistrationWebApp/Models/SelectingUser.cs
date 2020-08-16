using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationWebApp.Models
{
    public class SelectingUser
    {
        public User User { get; set; }
        public bool IsSelected { get; set; }

        public SelectingUser(User user)
        {
            User = user;
            IsSelected = false;
        }
    }
}
