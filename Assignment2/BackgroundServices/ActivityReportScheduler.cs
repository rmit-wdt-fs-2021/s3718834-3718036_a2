using System;
using System.Threading;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Assignment2.BackgroundServices
{
    public class ActivityReportScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan _timeBetweenReports = TimeSpan.FromMinutes(3);

        public ActivityReportScheduler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope to access needed services
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            DateTime lastReportTime;
            try
            {
                lastReportTime =
                    await context.ActivityReportHistory.MaxAsync(
                        activityReportHistory => activityReportHistory.OccuredWhen, cancellationToken);
                var timeSince =
                    DateTime.UtcNow.Subtract(lastReportTime); // Check the time left before another report must be sent
                if (timeSince <= _timeBetweenReports
                ) // If the time between reports hasn't already passed the time needed
                {
                    await Task.Delay(timeSince, cancellationToken); // Wait the remaining time
                }
            }
            catch (InvalidOperationException) // Occurs when there is no records in the database. 
            {
                lastReportTime = DateTime.MinValue; // So that nothing is missed in the initial report
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var activityReportService = scope.ServiceProvider.GetRequiredService<IActivityReportService>();
                var emailCount = await activityReportService.PerformActivityReports(lastReportTime);
                
                // Store that activity reports were sent out such that times can persist after application closure
                await context.ActivityReportHistory.AddAsync(new ActivityReportHistory
                {
                    OccuredWhen = DateTime.UtcNow,
                    EmailsSent = emailCount
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);

                await Task.Delay(_timeBetweenReports, cancellationToken);
            }
        }
    }
}