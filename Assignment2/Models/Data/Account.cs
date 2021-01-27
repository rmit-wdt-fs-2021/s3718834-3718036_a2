using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public enum AccountType
    {
        Checking,
        Saving
    }

    public class Account
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccountNumber { get; set; }

        [DataType(DataType.Currency)]
        [Required, Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ModifyDate { get; set; }

        [ForeignKey("CustomerForeignKey")]
        public Customer Customer { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public virtual List<BillPay> BillPays { get; set; }
    }
}
