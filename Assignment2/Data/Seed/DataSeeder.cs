using Assignment2.Controllers;
using Assignment2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAccounts(int customerId, ApplicationDbContext context)
        {
            Random rnd = new Random(DateTime.UnixEpoch.Minute);

            int checkingNumber = 0;
            int savingNumber = 0;// Generate a 4 digit number as needed for a customer id

            do
            {
                checkingNumber = rnd.Next(1000, 9999);// Generate a 4 digit number as needed for a customer id

            } while (context.Account.Where(a => a.AccountNumber == checkingNumber).Any());
            do
            {
                savingNumber = rnd.Next(1000, 9999);// Generate a 4 digit number as needed for a customer id

            } while (context.Account.Where(a => a.AccountNumber == savingNumber).Any());


            await context.Account.AddRangeAsync(new List<Account>
            {
                new Account
                {
                    AccountNumber = checkingNumber,
                    AccountType = AccountType.Checking,
                    CustomerId = customerId,
                    ModifyDate = DateTime.UtcNow,
                    Transactions = SeedOpeningTransactions(checkingNumber)
                },
                new Account
                {
                    AccountNumber = savingNumber,
                    AccountType = AccountType.Saving,
                    CustomerId = customerId,
                    ModifyDate = DateTime.UtcNow,
                    Transactions = SeedOpeningTransactions(savingNumber)
                }
            }.ToArray());
        }

        private static List<Transaction> SeedOpeningTransactions(int accountNumber)
        {
            return new List<Transaction>
            {
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = accountNumber,
                    DestAccount = 0,
                    Amount = 500,
                    Comment  = "Inital balance",
                    ModifyDate = DateTime.UtcNow
                }
            };
        }

        public static async Task SeedPayee(ApplicationDbContext context)
        {
            if(!context.Payee.Any())
            {
                await context.Payee.AddRangeAsync(new List<Payee>
            {
                new Payee
                {
                    Name = "Totally Real Water Co.",
                    Address = "1 Water Lane",
                    City = "Melbourne",
                    PostCode = "3000",
                    State = State.Vic,
                    Phone = "+61 0000 0000"
                },
                new Payee
                {
                    Name = "Completely Legit Electricity Company",
                    Address = "12 Main street",
                    City = "Brisbane",
                    PostCode = "4000",
                    State = State.Qld,
                    Phone = "+61 1234 5678"
                },
                new Payee
                {
                    Name = "The Tax Man",
                    Address = "43 Tax Evasion Boulevard",
                    City = "Hobart",
                    PostCode = "7000",
                    State = State.Tas,
                    Phone = "+61 1111 1111"
                },
            }.ToArray());
            }
            
        }

    }
}
