using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public enum TransactionType
    {
        Deposit,
        Withdraw,
        Transfer,
        ServiceCharge,
        BillPay
    }

    public class Transaction
    {
        [Key, Required] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [Required] 
        public TransactionType TransactionType { get; set; }

        [Required]
        public int AccountNumber { get; set; }
        public int DestAccount { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string Comment { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime ModifyDate { get; set; }
    }
}
