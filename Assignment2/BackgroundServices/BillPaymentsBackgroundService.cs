using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assignment2.Controllers;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignment2.BackgroundServices
{
    public class BillPaymentsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActivityReportScheduler> _logger;

        public BillPaymentsBackgroundService(IServiceProvider serviceProvider, ILogger<ActivityReportScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope to access needed services
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dataAccessProvider = scope.ServiceProvider.GetRequiredService<IDataAccessProvider>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var scheduledBillPays = await context.BillPay
                    .Where(billPay => billPay.Status == Status.Waiting 
                    && billPay.Period == Period.OnceOff).ToListAsync();
                foreach (var billPay in scheduledBillPays)
                {
                    if (billPay.Status == Status.Waiting && billPay.Period == Period.OnceOff)
                    {
                        await context.Entry(billPay).Reference(b => billPay.Account).LoadAsync();
                        await context.Entry(billPay.Account).Collection(a => a.Transactions).LoadAsync();

                        if (await billPay.Account.Balance(dataAccessProvider) < billPay.Amount)
                        {
                            billPay.Status = Status.Fail;
                        }
                        else
                        {
                            billPay.Account.Transactions.Add(new Transaction
                            {
                                TransactionType = TransactionType.BillPay,
                                AccountNumber = billPay.AccountNumber,
                                Amount = billPay.Amount,
                                Comment = $"Scheduled payment to {billPay.PayeeId}",
                                ModifyDate = DateTime.UtcNow,
                                Account = billPay.Account
                            });

                            await billPay.Account.UpdateBalance(-billPay.Amount, dataAccessProvider);
                            billPay.Status = Status.Success;
                        }
                    }
                }
                await context.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}