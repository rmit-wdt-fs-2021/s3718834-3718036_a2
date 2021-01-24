using Assignment2.Data;
using Assignment2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class EmailController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;
        private readonly ICompositeViewEngine _viewEngine;

        public EmailController(ApplicationDbContext context, IOptions<EmailSenderSecrets> emailOptions, ICompositeViewEngine compositeViewEngine)
        {
            _context = context;
            _emailSender = new EmailSender(emailOptions);
            _viewEngine = compositeViewEngine;
        }


        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Account.Include(x => x.Transactions).ToListAsync();

            List<ActivityReportModel> activityReportModels = new List<ActivityReportModel>();
            foreach(var account in accounts)
            {
                activityReportModels.Add(new ActivityReportModel
                {
                    Account = account,
                    BalanceChange = new decimal(1.01),
                    Transactions = account.Transactions
                });
            }
            await GetActivityReportsSince(DateTime.MinValue);
            //await _emailSender.SendActivityReportAsync("brodeyyendall@gmail.com", await GenerateActivityReportHtml(activityReportModels));
            return View("ActivityReport", activityReportModels);
        }

        public async Task<List<ActivityReportModel>> GetActivityReportsSince(DateTime dateMinimum)
        {
            // TODO Find how to make this async
            var newTransactions = _context.Transaction.Where(transaction => transaction.ModifyDate > dateMinimum)
                .AsEnumerable()
                .GroupBy(transaction => transaction.AccountNumber);


            // Start all the requests to the database for accessing the account the transasctions belong to.
            // Done like this so one request doesn't hold up the others and parallelisation reduces work time
            var activityReportCreationTasks = new List<Task<ActivityReportModel>>();
            foreach(var transactionGroup in newTransactions)
            {
                activityReportCreationTasks.Add(ConvertToActivityReportModel(transactionGroup));
            }

            // As the accounts come back from the database, add the constructed activity reports to the list
            var activityReportModels = new List<ActivityReportModel>();
            while(activityReportCreationTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(activityReportCreationTasks); // Get a finished task
                activityReportCreationTasks.Remove(finishedTask); // Remove from the list because it is done
                activityReportModels.Add(await finishedTask); // Add the result to the list
            }


            return activityReportModels;
        }

        private async Task<ActivityReportModel> ConvertToActivityReportModel(IGrouping<int, Transaction> groupedTransaction)
        {
            var accountTask = _context.Account.Where(account => account.AccountNumber == groupedTransaction.Key).FirstAsync();

            decimal balanceChange = 0;
            foreach(var transaction in groupedTransaction.ToList())
            {
                balanceChange += transaction.Amount;
            }

            return new ActivityReportModel 
            { 
                Account = await accountTask, 
                Transactions = groupedTransaction.ToList(),
                BalanceChange = balanceChange
            };
        }

        public async Task<string> GenerateActivityReportHtml(List<ActivityReportModel> model)
        {
            ViewData.Model = model;

            ViewEngineResult foundView = _viewEngine.FindView(ControllerContext, "ActivityReport", false);

            StringWriter stringWriter = new StringWriter();

            ViewContext viewContext = new ViewContext(
                ControllerContext,
                foundView.View,
                ViewData,
                TempData,
                stringWriter,
                new HtmlHelperOptions()
            );

            await foundView.View.RenderAsync(viewContext);

            return stringWriter.GetStringBuilder().ToString();
        }

    }
}
