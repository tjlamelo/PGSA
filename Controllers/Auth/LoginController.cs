using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Auth;
using PGSA_Licence3.Data;
using System.Security.Claims;

namespace PGSA_Licence3.Controllers.Auth
{
    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;

        public LoginController(ApplicationDbContext db)
        {
            _loginService = new LoginService(db);
        }

        // GET: /Login
        [HttpGet]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View("~/Views/Auth/Login.cshtml");
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IFormCollection collection)
        {
            var email = collection["email"];
            var password = collection["password"];

            // La validation de base est toujours utile
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "L'email et le mot de passe sont requis.";
                return View("~/Views/Auth/Login.cshtml");
            }

            // On délègue TOUTE la logique de validation au service
            var user = await _loginService.ValidateUserCredentialsAsync(email, password);

            // Si le service renvoie null, c'est que les identifiants sont invalides
            if (user == null)
            {
                ViewBag.Error = "Email ou mot de passe incorrect.";
                return View("~/Views/Auth/Login.cshtml");
            }

            // L'utilisateur est valide, on le connecte
            await SignInUser(user);

            // Rediriger vers le dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // Cette méthode reste dans le contrôleur car elle gère des spécificités du framework HTTP (HttpContext, Cookies)
        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Prenom + " " + user.Nom),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Ajouter les rôles de l'utilisateur
            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    // J'ai corrigé .Nom par .Name, mais utilisez ce qui correspond à votre modèle Role
                    claims.Add(new Claim(ClaimTypes.Role, role.Nom)); 
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = false,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
       
    }
}