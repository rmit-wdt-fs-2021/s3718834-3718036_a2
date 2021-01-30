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
            var transactionHistoryModel = new ScheduledPaysViewModel
            {
                Accounts = new List<Account>(await _dataAccess.GetAccounts())
            };

            switch (transactionHistoryModel.Accounts.Count)
            {
                case 0:
                    return RedirectToAction(actionName: "Error", controllerName: "Home");
                case 1:
                    accountNumber = transactionHistoryModel.Accounts[0].AccountNumber;
                    break;
            }

            if (accountNumber != null)
            {
                transactionHistoryModel.BillPayList =
                    await _dataAccess.GetPagedBillPayments((int)accountNumber, page);
                transactionHistoryModel.SelectedAccountNumber = (int)accountNumber;
            }

            return View(transactionHistoryModel);
        }

        public async Task<IActionResult> Modify(int billPayId)
        {
            return View(
            new ScheduledPaysViewModel
            {
                BillPayId = billPayId,
                BillPay = await _dataAccess.GetBillPay(billPayId)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Modify([Bind("BillPayId,AccountNumber,PayeeId,Amount,ScheduleDate,Period,ModifyDate")] ScheduledPaysViewModel viewModel)
        {
            viewModel.BillPay = await _dataAccess.GetBillPay(viewModel.BillPayId);

            // TODO: Add stuff into here to modify existing bill payments.
            // i.e. make a new method EditBillPay(int billPayId)??? in the DataAccessController?

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create(int selectedAccountNumber)
        {
            return View(
                new ScheduledPaysViewModel
                {
                    SelectedAccountNumber = selectedAccountNumber,
                    SelectedAccount = await _dataAccess.GetUserAccount(selectedAccountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("AccountNumber,PayeeId,Amount,ScheduleDate,Period,ModifyDate")] ScheduledPaysViewModel viewModel)
        {
            viewModel.SelectedAccount = await _dataAccess.GetUserAccountWithBillPays(viewModel.SelectedAccountNumber);

            await _dataAccess.AddScheduledPayment(viewModel.SelectedAccount, new BillPay
            {
                AccountNumber = viewModel.SelectedAccountNumber,
                PayeeId = viewModel.BillPay.PayeeId,
                Amount = viewModel.BillPay.Amount,
                ScheduleDate = viewModel.BillPay.ScheduleDate,
                Period = viewModel.BillPay.Period,
                ModifyDate = DateTime.UtcNow
            });

            // Check the user has enough money to pay the scheduled payment?
            if (viewModel.BillPay.Amount > await viewModel.SelectedAccount.Balance(_dataAccess))
            {
                ModelState.AddModelError(nameof(viewModel.BillPay.Amount), "Amount must not exceed current balance.");
                return View(viewModel);
            }

            await viewModel.SelectedAccount.UpdateBalance(viewModel.BillPay.Amount, _dataAccess);
            await _dataAccess.AddTransaction(viewModel.SelectedAccount, new Transaction
            {
                AccountNumber = viewModel.SelectedAccountNumber,
                TransactionType = TransactionType.BillPay,
                Amount = viewModel.BillPay.Amount,
                ModifyDate = DateTime.UtcNow
            });

            return RedirectToAction(nameof(Index));
        }
    }
}