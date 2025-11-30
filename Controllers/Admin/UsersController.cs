using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Users;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using PGSA_Licence3.Helpers;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers.Admin
{
    [Route("admin/users")]
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly ApplicationDbContext _db;

        public UsersController(UserService userService, ApplicationDbContext db)
        {
            _userService = userService;
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet("editroles/{id}")]
        public async Task<IActionResult> EditRoles(int id)
        {
            var user = await _userService.GetUserWithRolesAsync(id);
            if (user == null) return NotFound();

            var roles = await _userService.GetAllRolesAsync();
            ViewBag.Roles = roles;
            return View(user);
        }

        [HttpPost("editroles/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRolesPost(int id, int[] selectedRoles)
        {
            var user = await _userService.GetUserWithRolesAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _userService.GetAllRolesAsync();

            // Synchronise roles: supprime les non sélectionnées, ajoute les sélectionnées
            var currentRoleIds = user.Roles?.Select(r => r.Id).ToList() ?? new List<int>();

            // Revoke roles not in selected
            foreach (var rid in currentRoleIds)
            {
                if (!selectedRoles.Contains(rid))
                {
                    await _userService.RevokeRoleAsync(id, rid);
                }
            }

            // Assign selected roles
            foreach (var rid in selectedRoles)
            {
                if (!currentRoleIds.Contains(rid))
                {
                    await _userService.AssignRoleAsync(id, rid);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("create-enseignant")]
        public async Task<IActionResult> CreateEnseignant()
        {
            var cours = await _db.Cours.ToListAsync();
            var groupes = await _db.Groupes.ToListAsync();
            ViewBag.Cours = cours;
            ViewBag.Groupes = groupes;
            return View();
        }

        [HttpPost("create-enseignant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnseignantPost([FromForm] Enseignant model, [FromForm] int[] coursIds, [FromForm] int[] groupeIds, [FromForm] string plainPassword)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cours = await _db.Cours.ToListAsync();
                ViewBag.Groupes = await _db.Groupes.ToListAsync();
                return View("CreateEnseignant", model);
            }

            // set username/email if not set
            if (string.IsNullOrWhiteSpace(model.Username)) model.Username = model.Email.Split('@')[0];
            if (string.IsNullOrWhiteSpace(model.MotDePasseHash) && !string.IsNullOrEmpty(plainPassword))
            {
                model.MotDePasseHash = PasswordHelper.Hash(plainPassword);
            }

            var result = await _userService.CreateEnseignantAsync(model, coursIds?.ToList() ?? new List<int>());

            // Assign groupes
            if (groupeIds != null && groupeIds.Length > 0)
            {
                var groupes = await _db.Groupes.Where(g => groupeIds.Contains(g.Id)).ToListAsync();
                result.Groupes = groupes;
                await _db.SaveChangesAsync();
            }

            // Optionally assign role "Enseignant" if exists
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Nom.ToLower() == "enseignant");
            if (role != null)
            {
                await _userService.AssignRoleAsync(result.Id, role.Id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit-groupes/{id}")]
        public async Task<IActionResult> EditGroupes(int id)
        {
            var enseignant = await _userService.GetEnseignantWithGroupesAsync(id);
            if (enseignant == null) return NotFound();

            var groupes = await _userService.GetAllGroupesAsync();
            ViewBag.Groupes = groupes;
            return View(enseignant);
        }

        [HttpPost("edit-groupes/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupesPost(int id, int[] selectedGroupes)
        {
            var enseignant = await _userService.GetEnseignantWithGroupesAsync(id);
            if (enseignant == null) return NotFound();

            var currentGroupeIds = enseignant.Groupes?.Select(g => g.Id).ToList() ?? new List<int>();

            // Revoke groupes not in selected
            foreach (var gid in currentGroupeIds)
            {
                if (!selectedGroupes.Contains(gid))
                {
                    await _userService.RevokeGroupeAsync(id, gid);
                }
            }

            // Assign selected groupes
            foreach (var gid in selectedGroupes)
            {
                if (!currentGroupeIds.Contains(gid))
                {
                    await _userService.AssignGroupeAsync(id, gid);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
