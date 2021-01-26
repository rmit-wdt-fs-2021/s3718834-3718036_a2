using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models.Email
{
    public interface IActivityReportProvider
    {
        public Task<int> PerformActivityReports();
    }
}
