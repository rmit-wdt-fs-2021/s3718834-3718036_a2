using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Data;
using Assignment2.Models;
using Assignment2.Controllers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IDataAccessProvider _dataAccess; 

        public AdminController(IDataAccessProvider dataAccess)
        {
            _dataAccess = dataAccess;
        }

        // GET api/<AdminController>/5
        [HttpGet("Transactions")]
        public async Task<List<Transaction>> Transactions(int? customerId = null, DateTime? min = null, DateTime? max = null)
        {
            min ??= DateTime.MinValue;
            max ??= DateTime.MaxValue;

            return await _dataAccess.GetFilteredTransactions((DateTime) min, (DateTime)max, customerId);
        }

        [HttpGet("Customer")]
        public async Task<List<Customer>> Customers()
        {
            var customers = await _dataAccess.GetCustomersWithLogin();
            foreach(var customer in customers)
            {
                customer.Login.Customer = null;
            }
            return customers;
        }

        [HttpGet("Lock/{customerId}")]
        public async Task LockCustomer(int customerId)
        {
            await _dataAccess.LockCustomer(customerId);
        }


        [HttpGet("ScheduledPayments")]
        public async Task<List<BillPay>> ScheduledPayments()
        {
            var billPays = await _dataAccess.GetScheduledPayments();
            foreach (var billPay in billPays)
            {
                billPay.Account = null;
            }
            return billPays;
        }

        [HttpGet("Block/{billPayId}")]
        public async Task BlockPayment(int billPayId)
        {
            await _dataAccess.BlockPayment(billPayId);
        }
    }
}
