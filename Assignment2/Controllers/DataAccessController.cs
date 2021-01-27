using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace Assignment2.Controllers
{
    public interface IDataAccessProvider
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
        /// Adds the provided transaction to the provided account
        /// </summary>
        /// <param name="account">The account to add the transaction to</param>
        /// <param name="transaction">The transaction to add to the account</param>
        public Task AddTransaction(Account account, Transaction transaction);
        
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
    }

    /// <summary>
    /// Provides access to the database for other classes to reduce repeated code and decrease coupling. 
    /// </summary>
    public class DataAccessController : IDataAccessProvider
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