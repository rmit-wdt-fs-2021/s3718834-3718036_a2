using System;
using System.Threading;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignment2.BackgroundServices
{
    public class ActivityReportBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan timeBetweenReports = TimeSpan.FromMinutes(3);

        public ActivityReportBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope to access needed services
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var lastReportTime = await context.ActivityReportHistory.MaxAsync(activityReportHistory => activityReportHistory.OccuredWhen, cancellationToken);
                var timeRemaining = DateTime.UtcNow.Subtract(lastReportTime); // Check the time left before another report must be sent
                if (timeRemaining <= timeBetweenReports) // If the time between reports hasn't already passed the time needed
                {
                    await Task.Delay(timeRemaining, cancellationToken); // Wait the remaining time
                }
            }
            catch (InvalidOperationException) // Occurs when there is no records in the database. In that case then the above nested code is not needed
            {
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                var provider = scope.ServiceProvider.GetRequiredService<IActivityReportProvider>();
                var emailCount = await provider.PerformActivityReports();

                // Store that activity reports were sent out such that times can persist after application closure
                context.ActivityReportHistory.Add(new ActivityReportHistory
                {
                    OccuredWhen = DateTime.UtcNow,
                    EmailsSent = emailCount
                });

                await context.SaveChangesAsync(cancellationToken);

                await Task.Delay(timeBetweenReports, cancellationToken);
            }
        }
    }
}
