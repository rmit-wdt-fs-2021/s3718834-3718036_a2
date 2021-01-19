using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Customer
    {
        [Key, Required]
        public int CustomerId { get; set; }

        [Required, MaxLength(50)]
        public string CustomerName { get; set; }

        [MaxLength(11)]
        public string Tfn { get; set; }

        [MaxLength(50)]
        public string Address { get; set; }

        [MaxLength(40)]
        public string City { get; set; }

        [MaxLength(20)] // Limit to VIC, NSW, SA, QLD and TAS
        public string State { get; set; }

        [MaxLength(4)]
        public string PostCode { get; set; }

        [Phone, Required]
        [RegularExpression("^(\\+61) [0-9]{4} [0-9]{4}")]
        public string Phone { get; set; }

    }
}
