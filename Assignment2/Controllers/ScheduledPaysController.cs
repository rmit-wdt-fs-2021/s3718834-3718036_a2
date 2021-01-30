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
            var billPay = await _context.BillPay.FirstOrDefaultAsync(m => m.BillPayId == billPayId);
            return View(billPay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(int billPayId, [Bind("BillPayId,AccountNumber,PayeeId,Amount,ScheduleDate,Period,ModifyDate")] BillPay billPay)
        {
            if (billPayId != billPay.BillPayId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(billPay);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillPayExists(billPay.BillPayId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(billPay);
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

            // Check whether the withdraw amount has more than 2 decimal places.
            // Code originally sourced from tutorial 5
            if (decimal.Round(viewModel.BillPay.Amount, 2) != viewModel.BillPay.Amount) 
            {
                ModelState.AddModelError(nameof(viewModel.BillPay.Amount), "Amount cannot have more than 2 decimal places.");
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

        public async Task<IActionResult> Delete(int? billPayId)
        {
            var billPay = await _context.BillPay.FirstOrDefaultAsync(m => m.BillPayId == billPayId);

            return View(billPay);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int billPayId)
        {
            var billPay = await _context.BillPay.FirstOrDefaultAsync(m => m.BillPayId == billPayId);

            if (billPay.Status == Status.Waiting)
            {
                _context.BillPay.Remove(billPay);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(nameof(BillPay.Status), "You can only delete a payment when it is 'Waiting'.");
                return View(billPay);
            }
        }

        private bool BillPayExists(int id)
        {
            return _context.BillPay.Any(e => e.BillPayId == id);
        }
    }
}