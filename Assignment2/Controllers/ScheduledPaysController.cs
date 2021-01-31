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

        public async Task<IActionResult> Create(int accountNumber, BillPay billPay)
        {
            var selectedAccount = await _dataAccess.GetAccount(accountNumber);
            return View(billPay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillPayId,AccountNumber,PayeeId,Amount,ScheduleDate,Period,ModifyDate")] BillPay billPay)
        {
            if (ModelState.IsValid)
            {
                var selectedAccount = await _dataAccess.GetAccount(billPay.AccountNumber);
                var balance = await _dataAccess.GetAccountBalance(selectedAccount);

                // Check that the payee the payment is going to actually exists.
                //if(billPay.PayeeId == null)
                //{
                //    ModelState.AddModelError(nameof(billPay.PayeeId), "Payee with id " + billPay.PayeeId + " does not exist.");
                //    return View(billPay);
                //}

                if (billPay.Amount > await selectedAccount.Balance(_dataAccess))
                {
                    ModelState.AddModelError(nameof(billPay.Amount), "Amount must not exceed current balance.");
                    return View(billPay);
                }

                // Check that the user would not go below the minimum balance requirements of their accounts when withdrawing.
                if ((selectedAccount.AccountType == AccountType.Checking) && (balance - billPay.Amount < 200))
                {
                    ModelState.AddModelError(nameof(billPay.Amount), "Amount must not go lower than the minimum balance requirements of $200.00.");
                    return View(billPay);
                }

                if (decimal.Round(billPay.Amount, 2) != billPay.Amount)
                {
                    ModelState.AddModelError(nameof(billPay.Amount), "Amount cannot have more than 2 decimal places.");
                    return View(billPay);
                }

                billPay.Status = Status.Waiting;
                billPay.ModifyDate = DateTime.UtcNow;
                _context.Add(billPay);
                await _context.SaveChangesAsync();

                await selectedAccount.UpdateBalance(billPay.Amount, _dataAccess);
                await _dataAccess.AddTransaction(selectedAccount, new Transaction
                {
                    AccountNumber = billPay.AccountNumber,
                    TransactionType = TransactionType.BillPay,
                    Amount = billPay.Amount,
                    ModifyDate = DateTime.UtcNow
                });

                return RedirectToAction(nameof(Index));
            }
            return View(billPay);
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
                    var selectedAccount = await _dataAccess.GetAccount(billPay.AccountNumber);
                    var balance = await _dataAccess.GetAccountBalance(selectedAccount);

                    if (billPay.Amount > await selectedAccount.Balance(_dataAccess))
                    {
                        ModelState.AddModelError(nameof(billPay.Amount), "Amount must not exceed current balance.");
                        return View(billPay);
                    }

                    // Check that the user would not go below the minimum balance requirements of their accounts when withdrawing.
                    if ((selectedAccount.AccountType == AccountType.Checking) && (balance - billPay.Amount < 200))
                    {
                        ModelState.AddModelError(nameof(billPay.Amount), "Amount must not go lower than the minimum balance requirements of $200.00.");
                        return View(billPay);
                    }

                    if (decimal.Round(billPay.Amount, 2) != billPay.Amount)
                    {
                        ModelState.AddModelError(nameof(billPay.Amount), "Amount cannot have more than 2 decimal places.");
                        return View(billPay);
                    }

                    billPay.ModifyDate = DateTime.UtcNow;
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