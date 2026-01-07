using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Role_Managment;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly RoleService _roleService;
        private readonly PermissionService _permissionService;
        private readonly ApplicationDbContext _context;

        public RolePermissionController(
            RoleService roleService,
            PermissionService permissionService,
            ApplicationDbContext context)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _context = context;
        }

        // GET: /RolePermission/ManagePermissions/5
        public async Task<IActionResult> ManagePermissions(int id)
        {
            ViewData["Title"] = "Gérer les Permissions";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Gérer les Permissions", $"/RolePermission/ManagePermissions/{id}")
            };

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            var rolePermissionIds = role.Permissions?.Select(p => p.Id).ToList() ?? new List<int>();

            ViewBag.Role = role;
            ViewBag.AllPermissions = allPermissions;
            ViewBag.RolePermissionIds = rolePermissionIds;

            return View();
        }

        // POST: /RolePermission/ManagePermissions/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePermissions(int id, List<int>? selectedPermissions)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (role == null)
                {
                    return NotFound();
                }

                if (role.Permissions == null)
                {
                    role.Permissions = new List<Permission>();
                }

                role.Permissions.Clear();

                if (selectedPermissions != null && selectedPermissions.Any())
                {
                    var permissions = await _context.Permissions
                        .Where(p => selectedPermissions.Contains(p.Id))
                        .ToListAsync();

                    foreach (var permission in permissions)
                    {
                        role.Permissions.Add(permission);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Permissions mises à jour avec succès.";
                return RedirectToAction("Details", "RoleManagement", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("ManagePermissions", new { id = id });
            }
        }
    }
}

