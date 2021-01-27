using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{

    public enum State
    {
        VIC,
        NSW,
        SA,
        QLD,
        TAS
    }


    public class Customer
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; }

        [Required, StringLength(50)]
        public string CustomerName { get; set; }

        [StringLength(11)]
        public string Tfn { get; set; }

        [StringLength(50)]
        public string Address { get; set; }

        [StringLength(40)]
        public string City { get; set; }

        [EnumDataType(typeof(State))]
        public State State { get; set; }

        [StringLength(4)]
        [RegularExpression("[0-9]{4}")]
        public string PostCode { get; set; }

        [Phone, Required]
        [RegularExpression("^(\\+61) [0-9]{4} [0-9]{4}")]
        public string Phone { get; set; }

        public virtual List<Account> Accounts { get; set; }

        public virtual ApplicationUser Login { get; set; }
    }
}
