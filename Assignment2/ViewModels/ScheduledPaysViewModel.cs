using Assignment2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.ViewModels
{
    public class ScheduledPaysViewModel
    {
        public int PayeeID { get; set; }

        public decimal Amount { get; set; }

        public DateTime ScheduleDate { get; set; }

        public char Period { get; set; }
    }
}
