using Assignment2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.ViewModels
{
    public class ActivityReportModel
    {
        public Account Account { get; set; }
        public decimal BalanceChange { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
