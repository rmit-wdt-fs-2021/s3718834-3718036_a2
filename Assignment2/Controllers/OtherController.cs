using Assignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Controllers
{
    public class OtherController : Controller
    {
        private readonly ILogger<OtherController> _logger;

        public OtherController(ILogger<OtherController> logger)
        {
            _logger = logger;
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
