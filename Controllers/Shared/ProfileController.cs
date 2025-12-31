using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Auth;
using System.Security.Claims;
using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers.Shared
{
    [Route("Profile")]
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly LoginService _loginService;
        private readonly ILogger<ProfileController> _logger;
        private readonly ApplicationDbContext _db;

        public ProfileController(ILogger<ProfileController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            _loginService = new LoginService(db); // instanciation manuelle
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
                return RedirectToAction("Index", "Login");

            var user = await _loginService.GetUserByEmailAsync(email);
            if (user == null)
                return RedirectToAction("Index", "Login");

            // Récupérer les rôles de l'utilisateur
            var roles = await _db.Roles
                .Where(r => r.Users.Any(u => u.Id == user.Id))
                .ToListAsync();

            // Déterminer si c'est un étudiant ou un enseignant
            Etudiant? etudiant = null;
            Enseignant? enseignant = null;

            // Essayer de récupérer en tant qu'étudiant
            etudiant = await _db.Etudiants
                .Include(e => e.Cycle)
                .Include(e => e.Niveau)
                .Include(e => e.Specialite)
                .Include(e => e.Groupes)
                .FirstOrDefaultAsync(e => e.Id == user.Id);

            if (etudiant != null)
            {
                // C'est un étudiant
                ViewBag.UserType = "Etudiant";
                ViewBag.Roles = roles;
                return View("~/Views/User/Staff/Profile.cshtml", etudiant);
            }
            else
            {
                // Essayer de récupérer en tant qu'enseignant
                enseignant = await _db.Enseignants
                    .Include(e => e.Cours)
                    .FirstOrDefaultAsync(e => e.Id == user.Id);

                if (enseignant != null)
                {
                    // C'est un enseignant
                    ViewBag.UserType = "Enseignant";
                    ViewBag.Roles = roles;
                    return View("~/Views/User/Staff/Profile.cshtml", enseignant);
                }
            }

            // Si ce n'est ni un étudiant ni un enseignant, retourner l'utilisateur de base
            ViewBag.UserType = "User";
            ViewBag.Roles = roles;
            return View("~/Views/User/Staff/Profile.cshtml", user);
        }
    }
}