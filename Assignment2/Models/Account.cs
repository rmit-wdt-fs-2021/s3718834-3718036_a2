using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Controllers;

namespace Assignment2.Models
{
    /// <summary>
    /// Used to represent account types. The value of the entries are the ascii value of the character they represent.
    /// This is because C# doesn't allow the assigning of char to an enum
    /// </summary>
    public enum AccountType
    {
        Checking = 'C', 
        Saving = 'S'
    }

    public class Account
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccountNumber { get; set; }

        [NotMapped] private decimal? _balance;
        
        public AccountType AccountType { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ModifyDate { get; set; }
        
        public Customer Customer { get; set; }

        public List<Transaction> Transactions { get; set; }

        public List<BillPay> BillPays { get; set; }

        public async Task<decimal> Balance(IDataAccessProvider dataAccessProvider)
        {
            _balance ??= await dataAccessProvider.GetAccountBalance(this);

            return (decimal)_balance;
        }

        public async Task<decimal> UpdateBalance(decimal amount, IDataAccessProvider dataAccessProvider)
        {
            if (_balance == null)
            {
                await Balance(dataAccessProvider);
            }
            
            return (decimal)(_balance += amount);
        }

        public decimal MinimumBalance()
        {
            return AccountType == AccountType.Checking ? 200 : 0;
        }
    }
}
