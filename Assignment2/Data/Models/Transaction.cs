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
        Deposit = 'D',
        Withdraw = 'W',
        Transfer = 'T',
        ServiceCharge = 'S',
        BillPay = 'B'
    }

    public class Transaction
    {
        [Key, Required] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        [Required] 
        [Display(Name = "Type")]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Display(Name = "Source account")]
        public int AccountNumber { get; set; }

        [Display(Name = "Destination account")]
        public int DestAccount { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string Comment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Last Modified")]
        public DateTime ModifyDate { get; set; }
        
        public Account Account { get; set; }

        public decimal GetBalanceImpact()
        {
            if (TransactionType == TransactionType.Deposit)
            {
                return Amount;
            }
            else
            {
                return -Amount;
            }
        }
    }
}
