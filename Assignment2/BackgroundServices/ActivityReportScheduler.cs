using System;
using System.Threading;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Assignment2.BackgroundServices
{
    public class ActivityReportScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ActivityReportScheduler> _logger;

        private readonly TimeSpan _timeBetweenReports = TimeSpan.FromMinutes(3);

        public ActivityReportScheduler(IServiceProvider serviceProvider, ILogger<ActivityReportScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope to access needed services
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            _logger.LogDebug("Checking if there is time remaining since last report");
            
            DateTime lastReportTime;
            try
            {
                lastReportTime =
                    await context.ActivityReportHistory.MaxAsync(
                        activityReportHistory => activityReportHistory.OccuredWhen, cancellationToken);
                var timeSince =
                    DateTime.UtcNow.Subtract(lastReportTime); // Check the time left before another report must be sent
                
                _logger.LogInformation("It has been {} since the last report", timeSince.ToString());
                
                if (timeSince <= _timeBetweenReports
                ) // If the time between reports hasn't already passed the time needed
                {
                    _logger.LogDebug("Waiting before sending a report");
                    await Task.Delay(timeSince, cancellationToken); // Wait the remaining time
                }
            }
            catch (InvalidOperationException) // Occurs when there is no records in the database. 
            {
                lastReportTime = DateTime.MinValue; // So that nothing is missed in the initial report
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Sending activity report");
                var activityReportService = scope.ServiceProvider.GetRequiredService<IActivityReportService>();
                var emailCount = await activityReportService.PerformActivityReports(lastReportTime);
                _logger.LogInformation("Sent {} activity reports", emailCount);
                
                
                _logger.LogDebug("Recording that reports were sent");
                // Store that activity reports were sent out such that times can persist after application closure
                await context.ActivityReportHistory.AddAsync(new ActivityReportHistory
                {
                    OccuredWhen = DateTime.UtcNow,
                    EmailsSent = emailCount
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Beginning wait until next report");
                await Task.Delay(_timeBetweenReports, cancellationToken);
            }
        }
    }
}