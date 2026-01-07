using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Role_Managment;
using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers
{
    public class UserRoleController : Controller
    {
        private readonly UserRoleService _userRoleService;
        private readonly RoleService _roleService;
        private readonly ApplicationDbContext _context;

        public UserRoleController(
            UserRoleService userRoleService,
            RoleService roleService,
            ApplicationDbContext context)
        {
            _userRoleService = userRoleService;
            _roleService = roleService;
            _context = context;
        }

        // GET: /UserRole/AssignRole
        public async Task<IActionResult> AssignRole()
        {
            ViewData["Title"] = "Assigner des Rôles";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Assigner des Rôles", "/UserRole/AssignRole")
            };

            var users = await _userRoleService.GetAllUsersAsync();
            var roles = await _roleService.GetAllRolesAsync();

            ViewBag.Users = users;
            ViewBag.Roles = roles;

            return View();
        }

        // POST: /UserRole/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            try
            {
                await _userRoleService.AssignRoleToUserAsync(userId, roleId);
                TempData["SuccessMessage"] = "Rôle assigné avec succès.";
                return RedirectToAction(nameof(AssignRole));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(AssignRole));
            }
        }

        // POST: /UserRole/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            try
            {
                await _userRoleService.RemoveRoleFromUserAsync(userId, roleId);
                TempData["SuccessMessage"] = "Rôle retiré avec succès.";
                return RedirectToAction(nameof(AssignRole));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(AssignRole));
            }
        }

        // GET: /UserRole/UserRoles/5
        public async Task<IActionResult> UserRoles(int id)
        {
            ViewData["Title"] = "Rôles de l'utilisateur";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Rôles de l'utilisateur", $"/UserRole/UserRoles/{id}")
            };

            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _roleService.GetAllRolesAsync();
            var userRoleIds = user.Roles?.Select(r => r.Id).ToList() ?? new List<int>();

            ViewBag.User = user;
            ViewBag.AllRoles = allRoles;
            ViewBag.UserRoleIds = userRoleIds;

            return View();
        }
    }
}

