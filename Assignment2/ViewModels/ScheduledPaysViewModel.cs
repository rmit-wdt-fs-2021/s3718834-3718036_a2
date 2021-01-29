using Assignment2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace Assignment2.ViewModels
{
    public class ScheduledPaysViewModel
    {
        public List<Account> Accounts { get; set; }

        public int SelectedAccountNumber { get; set; }

        public Account SelectedAccount { get; set; }

        public IPagedList<BillPay> BillPayList { get; set; }

        public BillPay BillPay { get; set; }
    }
}
