using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;

namespace Assignment2.Controllers
{
    [Authorize]
    public class ScheduledPaysController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataAccessProvider _dataAccess;

        public ScheduledPaysController(ApplicationDbContext context, IDataAccessProvider dataAccess)
        {
            _context = context;
            _dataAccess = dataAccess;
        }

        public async Task<IActionResult> Index(int? accountNumber, int page = 1)
        {
            var scheduledPaysViewModel = new ScheduledPaysViewModel
            {
                Accounts = await _context.Account.ToListAsync()
            };

            if (scheduledPaysViewModel.Accounts.Count == 0)
            {
                return RedirectToAction(actionName: "Error", controllerName:"Home");
            }

            if (scheduledPaysViewModel.Accounts.Count == 1)
            {
                accountNumber = scheduledPaysViewModel.Accounts[0].AccountNumber;
            }

            if (accountNumber != null)
            {
                scheduledPaysViewModel.BillPay = await _dataAccess.GetPagedBillPayments((int) accountNumber, page);
            }

            return View(scheduledPaysViewModel);
        }
    }
}