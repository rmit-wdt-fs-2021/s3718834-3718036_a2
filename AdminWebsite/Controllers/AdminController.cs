using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AdminWebsite.ViewModels;
using Assignment2.Controllers;
using Assignment2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdminWebsite.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private HttpClient _client;

        public AdminController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _client = _clientFactory.CreateClient("api");
        }

        public async Task<IActionResult> Index()
        {
            return await Transactions(startDate: DateTime.Today,endDate: DateTime.Today.AddDays(1));
        }

        public async Task<IActionResult> Transactions(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.MinValue;
            endDate ??= DateTime.MaxValue;

            var query = HttpUtility.ParseQueryString(string.Empty);
            if(customerId != null)
            {
                query["customerId"] = customerId.ToString();
            }
            query["startDate"] = startDate.ToString();
            query["endDate"] = endDate.ToString();
            var response = await _client.GetAsync("api/Admin/Transactions?" + query.ToString());


            var transactionViewModel = new AdminTransactionsViewModel
            {
                CustomerId = customerId,
                StartDate = startDate,
                EndDate = endDate
            };

            if(!response.IsSuccessStatusCode)
            {
                transactionViewModel.Transactions = new List<Transaction>();
            } else
            {
                var result = await response.Content.ReadAsStringAsync();
                transactionViewModel.Transactions = JsonConvert.DeserializeObject<List<Transaction>>(result);
            }


            return View("Transactions", transactionViewModel);
        }

        public async Task<IActionResult> Customers()
        {
            var response = await _client.GetAsync("api/Admin/Customer");

            List<Customer> customers;
            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                customers = JsonConvert.DeserializeObject<List<Customer>>(result);
            } else
            {
                customers = new List<Customer>();
            }

            return View(customers);
        }

        public async Task<IActionResult> Lock(string customerId)
        {
            var response = await _client.GetAsync($"/api/Admin/Lock/{customerId}");

            return RedirectToAction(nameof(Customers));
        }

        public async Task<IActionResult> Scheduled()
        {
            var response = await _client.GetAsync("api/Admin/ScheduledPayments");

            List<BillPay> billPays;
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                billPays = JsonConvert.DeserializeObject<List<BillPay>>(result);
            }
            else
            {
                billPays = new List<BillPay>();
            }

            return View(billPays);
        }

        public async Task<IActionResult> Block(string billPayId)
        {
            var response = await _client.GetAsync($"/api/Admin/Block/{billPayId}");

            return RedirectToAction(nameof(Scheduled));
        }
    }
}
