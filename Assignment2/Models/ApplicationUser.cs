using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
