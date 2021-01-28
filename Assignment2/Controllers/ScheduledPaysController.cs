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

        public ScheduledPaysController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? accountNumber, int? page = 1)
        {
            var scheduledPaysViewModel = new ScheduledPaysViewModel
            {
                Accounts = await _context.Account.ToListAsync()
            };

            if (scheduledPaysViewModel.Accounts.Count == 0)
            {
                return RedirectToAction(nameof(HomeController.Error));
            }

            if (scheduledPaysViewModel.Accounts.Count == 1)
            {
                accountNumber = scheduledPaysViewModel.Accounts[0].AccountNumber;
            }

            if (accountNumber != null)
            {
                 scheduledPaysViewModel.BillPayList = await _context.BillPay.Where(billPay => billPay.AccountNumber == accountNumber)
                    .OrderByDescending(billPay => billPay.ScheduleDate)
                    .ToPagedListAsync(page, 8);
                scheduledPaysViewModel.SelectedAccountNumber = (int)accountNumber;
            }

            return View(scheduledPaysViewModel);
        }

        public async Task<IActionResult> Modify(int billPayId)
        {
            return View(
            new ScheduledPaysViewModel
            {
                BillPay = await _context.BillPay.FindAsync(billPayId)
            });
        }
    }
}