using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Auth;
using System.Security.Claims;
using PGSA_Licence3.Data; 
namespace PGSA_Licence3.Controllers.Shared
{
   [Route("dashboard")]
[Authorize] 
public class DashboardController : Controller
{
    private readonly LoginService _loginService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _loginService = new LoginService(db); // instanciation manuelle
    }

    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            return RedirectToAction("Index", "Login");

        var user = await _loginService.GetUserByEmailAsync(email);
        if (user == null)
            return RedirectToAction("Index", "Login");

        return View("~/Views/DashboardIndex.cshtml", user);
    }
}

    
}
