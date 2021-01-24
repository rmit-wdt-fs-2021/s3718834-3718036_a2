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
            Dictionary<string, List<ActivityReportModel>> activityReportModels =  await GetActivityReportsSince(DateTime.MinValue);
            foreach(var entry in activityReportModels)
            {
                // await _emailSender.SendActivityReportAsync(entry.Key, await GenerateActivityReportHtml(entry.Value));
                return View("ActivityReport", entry.Value);
            }


            return View("ActivityReport", new List<ActivityReportModel>());
        }

        private async Task<Dictionary<string, List<ActivityReportModel>>> GetActivityReportsSince(DateTime dateMinimum)
        {
            // TODO Find how to make this async
            var newTransactions = _context.Transaction.Where(transaction => transaction.ModifyDate > dateMinimum)
                .AsEnumerable()
                .GroupBy(transaction => transaction.AccountNumber);


            var activityReportModels = new List<ActivityReportModel>();
            foreach (var transactionGroup in newTransactions)
            {
                activityReportModels.Add(await ConvertToActivityReportModel(transactionGroup));
            }


            var groupedAccounts = activityReportModels.AsEnumerable().GroupBy(activityReport => activityReport.Account.CustomerId);
            var emailActivityReportTable = new Dictionary<string, List<ActivityReportModel>>();
            foreach (var group in groupedAccounts)
            {
                var customer = await _context.Customer
                    .Include(customer => customer.Login)
                    .Where(customer => customer.CustomerId == group.Key)
                    .FirstAsync();

                emailActivityReportTable.Add(customer.Login.Email, group.ToList());
            }

           
            return emailActivityReportTable;
        }

        private async Task<ActivityReportModel> ConvertToActivityReportModel(IGrouping<int, Transaction> groupedTransaction)
        {
            var accountTask = _context.Account.Where(account => account.AccountNumber == groupedTransaction.Key).FirstAsync();

            decimal balanceChange = 0;
            foreach(var transaction in groupedTransaction.ToList())
            {
                balanceChange += transaction.Amount;
            }

            var account = await accountTask;

            return new ActivityReportModel 
            { 
                Account = account, 
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
