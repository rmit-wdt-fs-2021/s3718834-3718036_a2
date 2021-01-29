using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly ApplicationDbContext _context; 

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/<AdminController>/5
        [HttpGet("Transactions/{customerId?}/{min?}/{max?}")]
        public List<Transaction> Transactions(int? customerId = null, DateTime? min = null, DateTime? max = null)
        {
            min ??= DateTime.MinValue;
            max ??= DateTime.MaxValue;

            List<Transaction> transactions = new List<Transaction>();
            if (customerId != null)
            {
                var accounts = _context.Account.Where(a => a.CustomerId == customerId).ToList();
                foreach(var account in accounts) {
                    transactions.AddRange(_context.Transaction.Where(transaction => 
                    (
                        transaction.AccountNumber == account.AccountNumber &&
                         transaction.ModifyDate > min &&
                         transaction.ModifyDate < max
                    )).ToList());
                }   
            } else
            {
                transactions = _context.Transaction.Where(transaction => transaction.ModifyDate > min && transaction.ModifyDate < max).ToList();
            }

            foreach(var transaction in transactions)
            {
                transaction.Account = null;
            }

            return transactions;
        }

        [HttpGet("Customer")]
        public List<Customer> Customers()
        {
            return _context.Customer.ToList();
        }

        [HttpPut("Customer/{customerId}")]
        public async Task LockCustomer(int customerId)
        {
            
            var login = _context.Users.First(li => li.CustomerId == customerId);

            if(login != null)
            {
                login.LockoutEnabled = true;
                login.LockoutEnd = DateTime.Now.AddMinutes(1);
                await _context.SaveChangesAsync();
            }
        }
    }
}
