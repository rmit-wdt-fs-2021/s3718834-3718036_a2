using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assignment2.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Assignment2.Service
{
    public class EmailSenderSecrets
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
    }

    public interface IEmailService : IEmailSender
    {
        public Task SendActivityReport(string destination, List<ActivityReportModel> contents);
    }

    /*
     * Original code for this class sourced from https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-5.0&tabs=visual-studio
    */
    public class EmailService :  IEmailService
    {
        private readonly EmailSenderSecrets _options;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSenderSecrets> optionsAccessor, IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider, IServiceProvider serviceProvider, ILogger<EmailService> logger)
        {
            _options = optionsAccessor.Value;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SendEmailAsync(string destination, string subject, string message)
        {
            await SendEmail(destination, subject, message, message);
        }

        public async Task SendActivityReport(string destination, List<ActivityReportModel> contents)
        {
            _logger.LogDebug("Sending activity report");
            await SendEmail(destination, $"Your activity report for {DateTime.Now}", "Your activity report",
                await GetEmailHtml("ActivityReport/ActivityReport", contents));
        }

        private async Task SendEmail(string destination, string subject, string message, string htmlMessage)
        {
            if (_options.SendGridKey == null)
            {
                _logger.LogError("Found no send grid key in user secrets. No email was sent");
                return;
            }

            if (_options.SendGridUser == null)
            {
                _logger.LogError("Found no send grid user in user secrets. No email was sent");
                return;
            }
            
            var client = new SendGridClient(_options.SendGridKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = htmlMessage
            };
            msg.AddTo(new EmailAddress(destination));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            await client.SendEmailAsync(msg); // Comment out this line to stop emails from being sent
        }

        /*
         * Sourced from Hasan A Yousef https://stackoverflow.com/a/40932984
         */
        private async Task<string> GetEmailHtml(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext {RequestServices = _serviceProvider};
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());


            var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"Failed to find email view ({viewName})");
            }

            var viewDictionary =
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

            await using var stringWriter = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                stringWriter,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return stringWriter.ToString();
        }
    }
}