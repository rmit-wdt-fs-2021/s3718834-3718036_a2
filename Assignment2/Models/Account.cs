using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Account
    {
        [Key, Required]
        public int AccountNumber { get; set; }

        [Required]
        public char AccountType { get; set; } // TODO Limit to C and S

        [Required]
        public int CustomerId { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ModifyDate { get; set; }
    }
}
