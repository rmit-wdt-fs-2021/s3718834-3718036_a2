using Assignment2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace Assignment2.ViewModels
{
    public class TransactionHistoryModel
    {
        public List<Account> Accounts { get; set; }

        public int SelectedAccountNumber { get; set; }

        public IPagedList<Transaction> Transactions { get; set; }
    }
}
