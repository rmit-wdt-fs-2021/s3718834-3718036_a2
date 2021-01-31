using Assignment2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AdminSite.ViewModels
{
    public class TransactionsViewModel
    {
        [Range(1000, 9999)]
        public int? CustomerId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}
