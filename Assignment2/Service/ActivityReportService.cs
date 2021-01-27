using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Assignment2.Service
{
    public interface IActivityReportService
    {
        public Task<int> PerformActivityReports(DateTime reportsSince);
    }


    public class ActivityReportService : IActivityReportService
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActivityReportService> _logger;

        public ActivityReportService(IEmailService emailService, ApplicationDbContext context, ILogger<ActivityReportService> logger)
        {
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        
        
        public async Task<int> PerformActivityReports(DateTime reportsSince)
        {
            _logger.LogDebug("Generating activity reports");
            var activityReportModels = await GetActivityReportsSince(reportsSince);
            foreach (var (email, reportContents) in activityReportModels)
            {
                await _emailService.SendActivityReport(email, reportContents);
            }
            
            return activityReportModels.Count;
        }

        private async Task<Dictionary<string, List<ActivityReportModel>>> GetActivityReportsSince(DateTime dateMinimum)
        {
            var newTransactions = _context.Transaction.Where(transaction => transaction.ModifyDate > dateMinimum)
                .AsEnumerable()
                .GroupBy(transaction => transaction.AccountNumber);


            var activityReportModels = new List<ActivityReportModel>();
            foreach (var transactionGroup in newTransactions)
            {
                activityReportModels.Add(await ConvertToActivityReportModel(transactionGroup));
            }


            var groupedAccounts = activityReportModels.AsEnumerable()
                .GroupBy(activityReport => activityReport.Account.CustomerId);
            var emailActivityReportTable = new Dictionary<string, List<ActivityReportModel>>();
            foreach (var group in groupedAccounts)
            {
                var customer = await _context.Customer
                    .Include(c => c.Login)
                    .Where(c => c.CustomerId == group.Key)
                    .FirstAsync();

                emailActivityReportTable.Add(customer.Login.Email, group.ToList());
            }


            return emailActivityReportTable;
        }

        private async Task<ActivityReportModel> ConvertToActivityReportModel(
            IGrouping<int, Transaction> groupedTransaction)
        {
            var accountTask = _context.Account.Where(acc => acc.AccountNumber == groupedTransaction.Key)
                .FirstAsync();

            decimal balanceChange = 0;
            foreach (var transaction in groupedTransaction)
            {
                if (transaction.TransactionType == TransactionType.Deposit)
                {
                    balanceChange += transaction.Amount;
                }
                else
                {
                    balanceChange -= transaction.Amount;
                }
            }
            
            var account = await accountTask;

            return new ActivityReportModel
            {
                Account = account,
                Transactions = groupedTransaction.ToList(),
                BalanceChange = balanceChange
            };
        }
    }
}