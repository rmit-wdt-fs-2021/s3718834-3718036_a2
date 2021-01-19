using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Payee
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayeeId { get; set; }

        [Required, StringLength(50)]
        public string PayeeName { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(40)]
        public string City { get; set; }

        [StringLength(20)]
        public State State { get; set; }

        [StringLength(4)]
        [RegularExpression("[0-9]{4}")]
        public string PostCode { get; set; }

        [Phone, Required]
        [RegularExpression("^(\\+61) [0-9]{4} [0-9]{4}")]
        public string Phone { get; set; }
    }
}
