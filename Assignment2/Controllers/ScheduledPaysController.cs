﻿using System;
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
                Accounts = new List<Account>(await _dataAccess.GetAccounts())
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
                scheduledPaysViewModel.BillPayList = await _dataAccess.GetPagedBillPayments((int) accountNumber, page);
            }

            return View(scheduledPaysViewModel);
        }

        public async Task<IActionResult> Modify(int billPayId)
        {
            //return View(
            //new ScheduledPaysViewModel
            //{
            //    BillPay = await _context.BillPay.FindAsync(billPayId)
            //});

            var billPay = await _context.BillPay.FindAsync(billPayId);
            if (billPay == null)
            {
                return NotFound();
            }
            return View(billPay);
        }

        public async Task<IActionResult> Create(int billPayId)
        {
            return View(
            new ScheduledPaysViewModel
            {
                BillPay = await _context.BillPay.FindAsync(billPayId)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduledPaysViewModel viewModel)
        {
            viewModel.SelectedAccount = await _dataAccess.GetUserAccount(viewModel.SelectedAccountNumber);

            await _dataAccess.AddScheduledPayment(viewModel.SelectedAccount, new BillPay
            {
                AccountNumber = viewModel.SelectedAccount.AccountNumber,
                PayeeId = viewModel.BillPay.PayeeId,
                Amount = viewModel.BillPay.Amount,
                ScheduleDate = viewModel.BillPay.ScheduleDate,
                ModifyDate = DateTime.UtcNow
            });

            return RedirectToAction(nameof(Index));
        }
    }
}