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

namespace Assignment2.Controllers
{
    public class BankController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Account.ToListAsync());
        }

        public async Task<IActionResult> Deposit(int accountNumber)
        {
            return View(
                new AtmTransactionViewModel
                {
                    AccountNumber = accountNumber,
                    Account = await _context.Account.FindAsync(accountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Deposit([Bind("AccountNumber,Amount")] AtmTransactionViewModel viewModel)
        {
            viewModel.Account = await _context.Account.Include(x => x.Transactions).
                FirstOrDefaultAsync(x => x.AccountNumber == viewModel.AccountNumber);

            // Check that the deposit amount is a positive integer.
            if(viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }
            // Check whether the deposit amount has more than 2 decimal places.
            //if (viewModel.Amount.HasMoreThanTwoDecimalPlaces())
            //{
            //    ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
            //    return View(viewModel);
            //}

            viewModel.Account.Balance += viewModel.Amount;
            viewModel.Account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    Amount = viewModel.Amount,
                    ModifyDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int accountNumber)
        {
            return View(
                new AtmTransactionViewModel
                {
                    AccountNumber = accountNumber,
                    Account = await _context.Account.FindAsync(accountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw([Bind("AccountNumber, Amount")] AtmTransactionViewModel viewModel)
        {
            viewModel.Account = await _context.Account.Include(x => x.Transactions).
                FirstOrDefaultAsync(x => x.AccountNumber == viewModel.AccountNumber);

            // Check that the withdraw amount is a positive integer.
            if (viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }
            // Check that the user has enough money to withdraw the desired amount.
            if (viewModel.Amount > viewModel.Account.Balance)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must not exceed current balance.");
                return View(viewModel);
            }
            // Check whether the withdraw amount has more than 2 decimal places.
            //if (viewModel.Amount.HasMoreThanTwoDecimalPlaces())
            //{
            //    ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
            //    return View(viewModel);
            //}

            viewModel.Account.Balance -= viewModel.Amount;
            viewModel.Account.Transactions.Add(
                new Transaction
                {
                    AccountNumber = viewModel.AccountNumber,
                    TransactionType = TransactionType.Withdraw,
                    Amount = viewModel.Amount,
                    ModifyDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer(int accountNumber, int destinationAccountNumber, string comment)
        {
            return View(
                new TransferModel
                {
                    AccountNumber = accountNumber,
                    DestinationAccountNumber = destinationAccountNumber,
                    Account = await _context.Account.FindAsync(accountNumber),
                    DestinationAccount = await _context.Account.FindAsync(destinationAccountNumber),
                    Comment = comment
                });
            //[Bind("AccountNumber, DestinationAccountNumber, Amount")]
        }

        [HttpPost]
        public async Task<IActionResult> Transfer([Bind("AccountNumber, DestinationAccountNumber, Amount, Comment")] TransferModel viewModel)
        {
            viewModel.Account = await _context.Account.Include(x => x.Transactions).
                FirstOrDefaultAsync(x => x.AccountNumber == viewModel.AccountNumber);

            viewModel.DestinationAccount = await _context.Account.Include(x => x.Transactions).
                FirstOrDefaultAsync(x => x.AccountNumber == viewModel.DestinationAccountNumber);

            // Check that the withdraw amount is a positive integer.
            if (viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }
            // Check that the user has enough money to withdraw the desired amount.
            if (viewModel.Amount > viewModel.Account.Balance)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must not exceed current balance.");
                return View(viewModel);
            }
            // Check whether the withdraw amount has more than 2 decimal places.
            //if (viewModel.Amount.HasMoreThanTwoDecimalPlaces())
            //{
            //    ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
            //    return View(viewModel);
            //}

            viewModel.Account.Balance -= viewModel.Amount;
            viewModel.Account.Transactions.Add(
                new Transaction
                {
                    AccountNumber = viewModel.AccountNumber,
                    DestAccount = viewModel.DestinationAccountNumber,
                    TransactionType = TransactionType.Transfer,
                    Amount = viewModel.Amount,
                    Comment = viewModel.Comment,
                    ModifyDate = DateTime.UtcNow
                });

            viewModel.DestinationAccount.Balance += viewModel.Amount;
            viewModel.DestinationAccount.Transactions.Add(
                new Transaction
                {
                    AccountNumber = viewModel.AccountNumber,
                    TransactionType = TransactionType.Deposit,
                    Amount = viewModel.Amount,
                    Comment = viewModel.Comment,
                    ModifyDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
