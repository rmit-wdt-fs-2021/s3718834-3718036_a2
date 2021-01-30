using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AdminSite.ViewModels;
using Assignment2.Controllers;
using Assignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdminSite.Controllers
{
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

            var response = await _client.GetAsync("api/movies/Transactions");

            if(!response.IsSuccessStatusCode)
            {
                throw new Exception(); // TODO Get a better exception
            }

            var result = await response.Content.ReadAsStringAsync();

            var transactionViewModel = new TransactionsViewModel {
                CustomerId = customerId,
                StartDate = startDate,
                EndDate = endDate,
                Transactions = JsonConvert.DeserializeObject<List<Transaction>>(result)
            };


            return View(transactionViewModel);
        }

        public IActionResult Lock()
        {
            return View();
        }

        public IActionResult Block()
        {
            return View();
        }
    }
}
