using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Data;
using PGSA_Licence3.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PGSA_Licence3.Controllers.Auth
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LoginController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Login
        [HttpGet("/Login")]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost("/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexPost([FromForm] string usernameOrEmail, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Username/email et mot de passe requis");
                return View("Index");
            }

            var hashed = PasswordHelper.Hash(password);

            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null || user.MotDePasseHash != hashed || !user.Active)
            {
                ModelState.AddModelError(string.Empty, "Identifiants invalides");
                return View("Index");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.Roles != null)
            {
                foreach (var r in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r.Nom));
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect based on role (simple): if Enseignant -> /Home or /Teacher dashboard
            if (user.Roles != null && user.Roles.Any(r => r.Nom.ToLower() == "enseignant"))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("/Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
