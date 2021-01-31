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
            var dataAccessProvider = scope.ServiceProvider.GetRequiredService<IDataAccessRepository>();

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Checking if there are scheduled payments");
                var scheduledBillPays = await context.BillPay
                    .Where(billPay => billPay.Status == Status.Waiting 
                    && billPay.Period == Period.OnceOff).ToListAsync(cancellationToken);
                
                _logger.LogInformation("Found {} payments to process", scheduledBillPays.Count);
                // Loop through payments we can process
                foreach (var billPay in scheduledBillPays.Where(billPay => billPay.Status == Status.Waiting && billPay.Period == Period.OnceOff))
                {
                    // Retrieve the bill payments attached account so we can update the balance
                    await context.Entry(billPay).Reference(b => b.Account).LoadAsync(cancellationToken);
                    // Retrieve the bill payment's transactions so we can add another
                    await context.Entry(billPay.Account).Collection(a => a.Transactions).LoadAsync(cancellationToken);

                    if (await billPay.Account.Balance(dataAccessProvider) -  billPay.Amount < billPay.Account.MinimumBalance())
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
                _logger.LogInformation("Processing complete, waiting");
                await context.SaveChangesAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}