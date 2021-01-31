using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment2.Models;
using Assignment2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment2.Controllers
{
    [Authorize(Roles ="Customer")]
    public class BankController : Controller
    {
        private readonly IDataAccessProvider _dataAccess;

        public BankController(IDataAccessProvider dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataAccess.GetAccounts());
        }

        public async Task<IActionResult> Deposit(int accountNumber)
        {
            return View(
                new AtmTransactionViewModel
                {
                    AccountNumber = accountNumber,
                    Account = await _dataAccess.GetUserAccount(accountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Deposit([Bind("AccountNumber,Amount")] AtmTransactionViewModel viewModel)
        {
            viewModel.Account = await _dataAccess.GetUserAccountWithTransactions(viewModel.AccountNumber);

            // Check that the deposit amount is a positive integer.
            if (viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }
            // Check whether the withdraw amount has more than 2 decimal places.
            // Code originally sourced from tutorial 5
            if (decimal.Round(viewModel.Amount, 2) != viewModel.Amount)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
                return View(viewModel);
            }

            await viewModel.Account.UpdateBalance(viewModel.Amount, _dataAccess);
            await _dataAccess.AddTransaction(viewModel.Account, new Transaction
            {
                TransactionType = TransactionType.Deposit,
                Amount = viewModel.Amount,
                ModifyDate = DateTime.UtcNow,
                AccountNumber = viewModel.AccountNumber
            });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int accountNumber)
        {
            return View(
                new AtmTransactionViewModel
                {
                    AccountNumber = accountNumber,
                    Account = await _dataAccess.GetUserAccount(accountNumber)
                });
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw([Bind("AccountNumber, Amount")] AtmTransactionViewModel viewModel)
        {
            viewModel.Account = await _dataAccess.GetUserAccountWithTransactions(viewModel.AccountNumber);

            // Check that the withdraw amount is a positive integer.
            if (viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }

            // Check that the user has enough money to withdraw the desired amount.
            if (viewModel.Amount > await viewModel.Account.Balance(_dataAccess))
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must not exceed current balance.");
                return View(viewModel);
            }

            // Check whether the withdraw amount has more than 2 decimal places.
            // Code originally sourced from tutorial 5
            if (decimal.Round(viewModel.Amount, 2) != viewModel.Amount)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
                return View(viewModel);
            }

            await viewModel.Account.UpdateBalance(viewModel.Amount, _dataAccess);
            await _dataAccess.AddTransaction(viewModel.Account, new Transaction
            {
                AccountNumber = viewModel.AccountNumber,
                TransactionType = TransactionType.Withdraw,
                Amount = viewModel.Amount,
                ModifyDate = DateTime.UtcNow
            });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer(int accountNumber, string comment)
        {
            return View(
                new TransferModel
                {
                    AccountNumber = accountNumber,
                    Account = await _dataAccess.GetUserAccount(accountNumber),
                    Comment = comment
                });
        }

        [HttpPost]
        public async Task<IActionResult> Transfer([Bind("AccountNumber, DestinationAccountNumber, Amount, Comment")]
            TransferModel viewModel)
        {
            viewModel.Account = await _dataAccess.GetUserAccountWithTransactions(viewModel.AccountNumber);
            viewModel.DestinationAccount =
                await _dataAccess.GetAccountWithTransactions(viewModel.DestinationAccountNumber);

            // Check that the withdraw amount is a positive integer.
            if (viewModel.Amount <= 0)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must be positive.");
                return View(viewModel);
            }

            // Check that the user has enough money to withdraw the desired amount.
            if (viewModel.Amount > await viewModel.Account.Balance(_dataAccess))
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount must not exceed current balance.");
                return View(viewModel);
            }

            // Check whether the withdraw amount has more than 2 decimal places.
            if (decimal.Round(viewModel.Amount, 2) != viewModel.Amount)
            {
                ModelState.AddModelError(nameof(viewModel.Amount), "Amount cannot have more than 2 decimal places.");
                return View(viewModel);
            }

            await viewModel.Account.UpdateBalance(viewModel.Amount, _dataAccess);
            await _dataAccess.AddTransaction(viewModel.Account, new Transaction
            {
                AccountNumber = viewModel.AccountNumber,
                DestAccount = viewModel.DestinationAccountNumber,
                TransactionType = TransactionType.Transfer,
                Amount = viewModel.Amount,
                Comment = viewModel.Comment,
                ModifyDate = DateTime.UtcNow
            });

            await _dataAccess.AddTransaction(viewModel.DestinationAccount, new Transaction
            {
                AccountNumber = viewModel.DestinationAccountNumber,
                DestAccount = viewModel.AccountNumber,
                TransactionType = TransactionType.Deposit,
                Amount = viewModel.Amount,
                Comment = viewModel.Comment,
                ModifyDate = DateTime.UtcNow
            });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transactions(int? accountNumber, int page = 1)
        {
            var transactionHistoryModel = new TransactionHistoryModel
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
                transactionHistoryModel.Transactions =
                    await _dataAccess.GetPagedTransactions((int) accountNumber, page);
                transactionHistoryModel.SelectedAccountNumber = (int) accountNumber;
            }

            return View(transactionHistoryModel);
        }
    }
}