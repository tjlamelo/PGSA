using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PGSA_Licence3.Controllers.Auth
{
    [Route("Logout")]
    public class LogoutController : Controller
    {
        // GET: /Logout
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Déconnecte l'utilisateur en supprimant son cookie d'authentification
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige vers la page de login
            return RedirectToAction("Index", "Login");
        }
    }
}
