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
        [ForeignKey("CustomerForeignKey")]
        public Customer Customer { get; set; }
    }
}
