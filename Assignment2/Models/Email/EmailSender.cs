using System;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Assignment2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Assignment2.Models
{
    /*
     * Original code for this class sourced from https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-5.0&tabs=visual-studio
    */
    public class EmailSender : IEmailSender
    {

        public EmailSender(IOptions<EmailSenderSecrets> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public EmailSenderSecrets Options { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, message, email);
        }

        public Task SendActivityReportAsync(string destination, string contents)
        {
            return Execute(Options.SendGridKey, $"Activity report for {DateTime.Now}", "Your activity report", contents, destination);
        }


        public Task Execute(string apiKey, string subject, string message, string html, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("s3718834@student.rmit.edu.au", Options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = html
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
