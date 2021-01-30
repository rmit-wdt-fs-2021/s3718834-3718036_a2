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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillPayId,AccountNumber,PayeeId,Amount,ScheduleDate,Period,ModifyDate")] BillPay billPay)
        {
            if (ModelState.IsValid)
            {
                billPay.Status = Status.Waiting;
                billPay.ModifyDate = DateTime.UtcNow;
                _context.Add(billPay);
                await _context.SaveChangesAsync();

                // TODO: validate the amount with the users current balance

                var selectedAccount = await _dataAccess.GetAccount(billPay.AccountNumber);
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