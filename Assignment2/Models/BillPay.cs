using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class BillPay
    {
        public int BillPayId { get; set; }
        public int AccountNumber { get; set; }
        public int PayeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ScheduleDate { get; set; }
        public char Period { get; set; }
        public DateTime ModifyDate { get; set; }

    }
}
