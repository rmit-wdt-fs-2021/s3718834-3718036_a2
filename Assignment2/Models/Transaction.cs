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
        [Display(Name = "Type")]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Display(Name = "Source account")]
        public int AccountNumber { get; set; }

        [Display(Name = "Destination account")]
        public int DestAccount { get; set; }

        [DataType(DataType.Currency)]
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string Comment { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "Last Modified")]
        public DateTime ModifyDate { get; set; }

        [ForeignKey("AccountForeignKey")]
        public Account Account { get; set; }
    }
}
