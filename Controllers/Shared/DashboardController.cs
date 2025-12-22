using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Controllers
{
    [Route("dashboard")]
    public class DashboardController : Controller
    {
        private readonly ILogger< DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

       [HttpGet("index")]
    public IActionResult Index()
    {
        return View("~/Views/DashboardIndex.cshtml");
    }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
