﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Controllers;

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

        [NotMapped] private decimal? _balance;
        
        public AccountType AccountType { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime ModifyDate { get; set; }
        
        public Customer Customer { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public virtual List<BillPay> BillPays { get; set; }

        public async Task<decimal> Balance(IDataAccessProvider dataAccessProvider)
        {
            _balance ??= await dataAccessProvider.GetAccountBalance(this);

            return (decimal)_balance;
        }

        public decimal UpdateBalance(decimal amount)
        {
            return (decimal)(_balance += amount);
        }
    }
}
