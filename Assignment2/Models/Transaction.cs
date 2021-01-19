using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Transaction
    {
        [Key, Required] // TOOD Must be auto generated ID
        public int TransactionId { get; set; }

        [Required] // TODO Limit to D,W,T,S,B
        public char TransactionType { get; set; }

        [Required]
        public int AccountNumber { get; set; }
        public int DestAccount { get; set; }
        public decimal Amount { get; set; }

        [MaxLength(255)]
        public string Comment { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ModifyDate { get; set; }
    }
}
