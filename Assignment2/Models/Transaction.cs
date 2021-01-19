using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public char TransactionType { get; set; }
        public int AccountNumber { get; set; }
        public int DestAccount { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public DateTime ModifyDate { get; set; }
    }
}
