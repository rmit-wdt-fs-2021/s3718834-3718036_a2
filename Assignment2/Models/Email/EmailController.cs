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

            await _emailSender.SendActivityReportAsync("brodeyyendall@gmail.com", await GenerateActivityReportHtml(activityReportModels));
            return View("ActivityReport", activityReportModels);
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
