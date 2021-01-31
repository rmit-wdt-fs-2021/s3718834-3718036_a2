using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Data.Seed;
using Assignment2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Assignment2.Controllers
{
    public interface IDataAccessRepository
    {
        /// <summary>
        /// Gets the accounts for provided customer or defaults to the currently logged in user
        /// </summary>
        /// <param name="customerId">The id of the customer to retrieved accounts for. Defaults to logged in user if not provided</param>
        /// <returns>The accounts for the customer</returns>
        /// <exception cref="RecordMissingException">There was a problem with the logged in customer or there was no customer with provided id</exception>
        public Task<IEnumerable<Account>> GetAccounts(int? customerId = null);
        
        /// <summary>
        /// Get a specific account for the logged in user
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The retrieved account</returns>
        /// <exception cref="RecordMissingException">When the logged in user doesn't have the provided account</exception>
        public Task<Account> GetUserAccount(int accountNumber);
        
        /// <summary>
        /// Get an account with the provided account number
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns>The retrieved account</returns>
        /// <exception cref="RecordMissingException">There were no accounts with provided account number</exception>
        public Task<Account> GetAccount(int accountNumber);
        
        /// <summary>
        /// Gets a specific account for the logged in user, loading in transactions with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The account retrieve with transactions populated</returns>
        /// <exception cref="RecordMissingException">User owns no account with provided id</exception>
        public Task<Account> GetUserAccountWithTransactions(int accountNumber);
        
        /// <summary>
        /// Gets an account, loading in transactions with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve with transactions</param>
        /// <returns>The account retrieved with transactions populated</returns>
        /// <exception cref="RecordMissingException">There was no account with the provided account number</exception>
        public Task<Account> GetAccountWithTransactions(int accountNumber);

        /// <summary>
        /// Gets a specific account for the logged in user, loading in billPays with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The account retrieve with billPays populated</returns>
        /// <exception cref="RecordMissingException">User owns no account with provided id</exception>
        public Task<Account> GetUserAccountWithBillPays(int accountNumber);

        /// <summary>
        /// Calculates the balance of the provided account
        /// </summary>
        /// <param name="account">The account to calculate a balance for</param>
        /// <returns>The balance of the provided account</returns>
        public Task<decimal> GetAccountBalance(Account account);
        
        /// <summary>
        /// Adds the provided transaction to the provided account
        /// </summary>
        /// <param name="account">The account to add the transaction to</param>
        /// <param name="transaction">The transaction to add to the account</param>
        public Task AddTransaction(Account account, Transaction transaction);

        /// <summary>
        /// Adds the provided billPay to the provided account
        /// </summary>
        /// <param name="account">The account to add the billPay to</param>
        /// <param name="billPay">The billPay to add to the account</param>
        public Task AddScheduledPayment(Account account, BillPay billPay);

        /// <summary>
        /// Generates a paged list of 4 transactions for the provided account (which the logged in user owns)
        /// </summary>
        /// <param name="accountNumber">The id of the account to get transactions for</param>
        /// <param name="page">The page to start the list at</param>
        /// <returns>A paged list of containing 4 transactions for provided account</returns>
        /// <exception cref="RecordMissingException">User doesn't own an account with the provided account number</exception>
        public Task<IPagedList<Transaction>> GetPagedTransactions(int accountNumber, int page);
        
        /// <summary>
        /// Gets a customer with the provided customer id, if not provided then defaults to logged in customer
        /// </summary>
        /// <param name="customerId">The id of the customer to retrieve. Defaults to logged in user</param>
        /// <returns>The customer retrieved</returns>
        /// <exception cref="RecordMissingException">There was no customer id provided or there was a problem with the logged in customer</exception>
        public Task<Customer> GetCustomer(int? customerId = null);
        
        /// <summary>
        /// Gets a billPay with the provided billPay id
        /// </summary>
        /// <param name="billPayId">The id of the billPay to retrieve.</param>
        /// <returns>The billPay retrieved</returns>
        /// <exception cref="RecordMissingException">There was no customer id provided or there was a problem with the logged in customer</exception>
        public Task<BillPay> GetBillPay(int billPayId);

        /// <summary>
        /// Gets the bill payments for the provided account in a paged format
        /// </summary>
        /// <param name="accountNumber">The account number of the account to get bill payments for</param>
        /// <param name="page">The page to start at</param>
        /// <returns>Bill payments for the provided account in a paged format</returns>
        /// /// <exception cref="RecordMissingException">User doesn't own an account with the provided account number</exception>
        public Task<IPagedList<BillPay>> GetPagedBillPayments(int accountNumber, int page);

        /// <summary>
        /// Gets the total number of withdrawals and transfers a user has made of a specified type.
        /// </summary>
        /// <param name="accountNumber">The account number of the account to get transactions for</param>
        /// <param name="transactionType">The transaction type which to get transactions for</param>
        /// <returns>Total number of transactions with this types</returns>
        public Task<int> GetTransactionWithType(int accountNumber, TransactionType transactionType);

        /// <summary>
        /// Gets the total number of withdrawals and transfers a user has made.
        /// </summary>
        /// <param name="accountNumber">The account number of the account to get transactions for</param>
        /// <returns>Total number of transactions with these types</returns>
        public Task<int> GetTransactionsWithFees(int accountNumber);

        /// <summary>
        /// Gets a payee with specified id as defined by the user.
        /// </summary>
        /// <param name="payeeId"></param>
        /// <returns>The payee object</returns>
        /// <exception cref="RecordMissingException">The Payee object doesn't exist</exception>
        public Task<bool> PayeeExists(int payeeId);

        public Task<List<Customer>> GetCustomersWithLogin();

        public Task LockCustomer(int customerId);

        public Task<List<Transaction>> GetFilteredTransactions(DateTime minDate, DateTime maxDate, int? customerId = null);

        public Task<List<BillPay>> GetScheduledPayments();

        public Task BlockPayment(int billPayId);

    }

    /// <summary>
    /// Provides access to the database for other classes to reduce repeated code and decrease coupling. 
    /// </summary>
    public class DataAccessController : IDataAccessRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Used to access the currently logged in user
        private readonly IHttpContextAccessor _httpContextAccessor; // Also used to access the currently logged in user

        public DataAccessController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        /// <summary>
        /// Gets the accounts for provided customer or defaults to the currently logged in user
        /// </summary>
        /// <param name="customerId">The id of the customer to retrieved accounts for. Defaults to logged in user if not provided</param>
        /// <returns>The accounts for the customer</returns>
        /// <exception cref="RecordMissingException">There was a problem with the logged in customer or there was no customer with provided id</exception>
        public async Task<IEnumerable<Account>> GetAccounts(int? customerId = null)
        {
            var customer = await GetCustomer(customerId);
            await _context.Entry(customer).Collection(c => c.Accounts).LoadAsync();

            return customer.Accounts;
        }

        /// <summary>
        /// Get a specific account for the logged in user
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The retrieved account</returns>
        public async Task<Account> GetUserAccount(int accountNumber)
        {
            var userAccounts = await GetAccounts();
            return userAccounts.First(a => a.AccountNumber == accountNumber);
        }

        /// <summary>
        /// Get an account with the provided account number
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns>The retrieved account</returns>
        /// <exception cref="RecordMissingException">There were no accounts with provided account number</exception>
        public async Task<Account> GetAccount(int accountNumber)
        {
            try
            {
                var result = await _context.Account.FirstAsync(a => a.AccountNumber == accountNumber);
                
                return result;
            }
            catch (InvalidOperationException)
            {
                throw new RecordMissingException("No account with provided account number");
            }
  
        }

        /// <summary>
        /// Gets a specific account for the logged in user, loading in transactions with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The account retrieve with transactions populated</returns>
        /// <exception cref="RecordMissingException">User owns no account with provided id</exception>
        public async Task<Account> GetUserAccountWithTransactions(int accountNumber)
        {
            var account = await GetUserAccount(accountNumber); // Will throw exception if no account found
            await _context.Entry(account).Collection(a => a.Transactions).LoadAsync();
            return account;
        }

        /// <summary>
        /// Gets a specific account for the logged in user, loading in billPays with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve</param>
        /// <returns>The account retrieve with billPays populated</returns>
        /// <exception cref="RecordMissingException">User owns no account with provided id</exception>
        public async Task<Account> GetUserAccountWithBillPays(int accountNumber)
        {
            var account = await GetUserAccount(accountNumber);
            await _context.Entry(account).Collection(a => a.BillPays).LoadAsync();
            return account;
        }

        /// <summary>
        /// Gets an account, loading in transactions with it
        /// </summary>
        /// <param name="accountNumber">The id of the account to retrieve with transactions</param>
        /// <returns>The account retrieved with transactions populated</returns>
        /// <exception cref="RecordMissingException">There was no account with the provided account number</exception>
        public async Task<Account> GetAccountWithTransactions(int accountNumber)
        {
            var account = await GetAccount(accountNumber); // Will throw exception if no account found
            await _context.Entry(account).Collection(a => a.Transactions).LoadAsync();
            return account;
        }

        /// <summary>
        /// Calculates the balance of the provided account
        /// </summary>
        /// <param name="account">The account to calculate a balance for</param>
        /// <returns>The balance of the provided account</returns>
        public async Task<decimal> GetAccountBalance(Account account)
        {
            await _context.Entry(account).Collection(a => a.Transactions).LoadAsync();

            return account.Transactions.Sum(transaction => transaction.GetBalanceImpact());
        }

        /// <summary>
        /// Adds the provided transaction to the provided account
        /// </summary>
        /// <param name="account">The account to add the transaction to</param>
        /// <param name="transaction">The transaction to add to the account</param>
        public async Task AddTransaction(Account account, Transaction transaction)
        {
            account.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds the provided billPay to the provided account
        /// </summary>
        /// <param name="account">The account to add the billPay to</param>
        /// <param name="billPay">The billPay to add to the account</param>
        public async Task AddScheduledPayment(Account account, BillPay billPay)
        {
            account.BillPays.Add(billPay);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Generates a paged list of 4 transactions for the provided account (which the logged in user owns)
        /// </summary>
        /// <param name="accountNumber">The id of the account to get transactions for</param>
        /// <param name="page">The page to start the list at</param>
        /// <returns>A paged list of containing 4 transactions for provided account</returns>
        /// <exception cref="RecordMissingException">User doesn't own an account with the provided account number</exception>
        public async Task<IPagedList<Transaction>> GetPagedTransactions(int accountNumber, int page)
        {
            var account = await GetUserAccountWithTransactions(accountNumber);
            
            return await account.Transactions
                .OrderByDescending(t => t.ModifyDate) // So that latest transactions appear first
                .ToPagedListAsync(page, 4);
        }

        /// <summary>
        /// Gets a customer with the provided customer id, if not provided then defaults to logged in customer
        /// </summary>
        /// <param name="customerId">The id of the customer to retrieve. Defaults to logged in user</param>
        /// <returns>The customer retrieved</returns>
        /// <exception cref="RecordMissingException">There was no customer id provided or there was a problem with the logged in customer</exception>
        public async Task<Customer> GetCustomer(int? customerId = null)
        {
            if (customerId == null)
            {
                return await GetLoggedInCustomer();
            }

            try
            {
                var customer = await _context.Customer.FirstAsync(c => c.CustomerId == customerId);
                return customer;
            }
            catch (InvalidOperationException)
            {
                throw new RecordMissingException("Found no customer with provided customer id");
            }

            
        }

        /// <summary>
        /// Gets a billPay with the provided billPay id
        /// </summary>
        /// <param name="billPayId">The id of the billPay to retrieve.</param>
        /// <returns>The billPay retrieved</returns>
        /// <exception cref="RecordMissingException">There was no customer id provided or there was a problem with the logged in customer</exception>
        public async Task<BillPay> GetBillPay(int billPayId)
        {
            try
            {
                var billPay = await _context.BillPay.FirstAsync(c => c.BillPayId == billPayId);
                return billPay;
            }
            catch (InvalidOperationException)
            {
                throw new RecordMissingException("Found no billPay with provided billPay id");
            }
        }

        /// <summary>
        /// Gets the customer object of the logged in user
        /// </summary>
        /// <returns>The customer object of the logged in user</returns>
        /// <exception cref="RecordMissingException">There was an error with the logged in user or customer object doesn't exist</exception>
        private async Task<Customer> GetLoggedInCustomer()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                throw new RecordMissingException(
                    "HttpContext was null, either user is not logged in or unexpected error");
            }

            // Get details of the logged in user in the form of a ApplicationUser object
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User); 
            
            // Get the linked customer object
            await _context.Entry(user).Reference(u => u.Customer).LoadAsync();

            if (user.Customer == null)
            {
                throw new RecordMissingException("No customer object attached to logged in user");
            }

            return user.Customer;
        }
        
        
       /// <summary>
       /// Gets the bill payments for the provided account in a paged format
       /// </summary>
       /// <param name="accountNumber">The account number of the account to get bill payments for</param>
       /// <param name="page">The page to start at</param>
       /// <returns>Bill payments for the provided account in a paged format</returns>
        public async Task<IPagedList<BillPay>> GetPagedBillPayments(int accountNumber, int page)
        {
            var account = await GetUserAccount(accountNumber);
            
            await _context.Entry(account).Collection(a => a.BillPays).LoadAsync();

            foreach(var billPay in account.BillPays)
            {
                _context.Entry(billPay).Reference(b => b.Payee).Load();
            }

            return await account.BillPays
                .OrderByDescending(billPay => billPay.ScheduleDate)
                .ToPagedListAsync(page, 8);
        }

        /// <summary>
        /// Gets the total number of withdrawals and transfers a user has made of a specified type.
        /// </summary>
        /// <param name="accountNumber">The account number of the account to get transactions for</param>
        /// <param name="transactionType">The transaction type which to get transactions for</param>
        /// <returns>Total number of transactions with this types</returns>
        public async Task<int> GetTransactionWithType(int accountNumber, TransactionType transactionType)
        {
            var account = await GetUserAccountWithTransactions(accountNumber);

            int temp = account.Transactions.Where(n => n.TransactionType == transactionType).Count();

            return temp;
        }

        /// <summary>
        /// Gets the total number of withdrawals and transfers a user has made.
        /// </summary>
        /// <param name="accountNumber">The account number of the account to get transactions for</param>
        /// <returns>Total number of transactions with these types</returns>
        public async Task<int> GetTransactionsWithFees(int accountNumber)
        {
            int totalTransfers = await GetTransactionWithType(accountNumber, TransactionType.Transfer);
            int totalWithdraws = await GetTransactionWithType(accountNumber, TransactionType.Withdraw);

            return totalTransfers + totalWithdraws;
        }


        public async Task<bool> PayeeExists(int payeeId)
        {
            await DataSeeder.SeedPayee(_context);
            try
            {
                await _context.Payee.FirstAsync(a => a.PayeeId == payeeId);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public async Task<List<Customer>> GetCustomersWithLogin()
        {
            return await _context.Customer.Include(customer => customer.Login).ToListAsync();
        }

        public async Task LockCustomer(int customerId)
        {
            var customer = await _context.Customer.Where(c => c.CustomerId == customerId).FirstAsync();


            await _context.Entry(customer).Reference(c => c.Login).LoadAsync();


            if (customer.Login != null)
            {
                customer.Login.LockoutEnabled = true;
                customer.Login.LockoutEnd = DateTime.UtcNow.AddMinutes(1);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Transaction>> GetFilteredTransactions(DateTime minDate, DateTime maxDate, int? customerId = null)
        {
            IQueryable<Transaction> totalTransactions = Enumerable.Empty<Transaction>().AsQueryable();
            if (customerId != null)
            {
                var accounts = await GetAccounts(customerId);
                foreach (var account in accounts)
                {
                    var transactions = _context.Transaction.Where(t => t.AccountNumber == account.AccountNumber);
                    if (totalTransactions == null)
                    {
                        totalTransactions = transactions;
                    } else
                    {
                        Queryable.Concat(totalTransactions, transactions);
                    }
                  
                }
            }
            else
            {
                totalTransactions = _context.Transaction;
            }


            return totalTransactions.Where(transaction => transaction.ModifyDate.ToLocalTime() >= minDate.Date && transaction.ModifyDate.ToLocalTime() <= maxDate.Date).ToList();
        }

        public async Task<List<BillPay>> GetScheduledPayments()
        {
            return await _context.BillPay.Where(billPay => billPay.Status == Status.Waiting).ToListAsync(); 
        }

        public async Task BlockPayment(int billPayId)
        {
            var billPay = await _context.BillPay.FirstAsync(billPay => billPay.BillPayId == billPayId);

            if(billPay.Status == Status.Waiting)
            {
                billPay.Status = Status.Blocked;
                await _context.SaveChangesAsync();

            }
        }
    }


    /// <summary>
    /// Thrown when the database proxy didn't find a record
    /// </summary>
    public class RecordMissingException : Exception
    {
        public RecordMissingException() : base("Failed to retrieve any records")
        {
        }

        public RecordMissingException(string message) : base(message)
        {
        }
    }
}