using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Role_Managment;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Controllers
{
    public class RoleManagementController : Controller
    {
        private readonly RoleService _roleService;
        private readonly PermissionSeedService _permissionSeedService;

        public RoleManagementController(RoleService roleService, PermissionSeedService permissionSeedService)
        {
            _roleService = roleService;
            _permissionSeedService = permissionSeedService;
        }

        // GET: /RoleManagement
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Rôles";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement")
            };

            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        // GET: /RoleManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Détails du Rôle";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Détails", $"/RoleManagement/Details/{id}")
            };

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
             

            return View(role);
        }

        // GET: /RoleManagement/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Créer un Rôle";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Créer", "/RoleManagement/Create")
            };

            return View();
        }

        // POST: /RoleManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nom, string? description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nom))
                {
                    ModelState.AddModelError("nom", "Le nom du rôle est requis.");
                    return View();
                }

                await _roleService.CreateRoleAsync(nom, description);
                TempData["SuccessMessage"] = "Rôle créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: /RoleManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Modifier le Rôle";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Rôles", "/RoleManagement"),
                ("Modifier", $"/RoleManagement/Edit/{id}")
            };

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: /RoleManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nom, string? description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nom))
                {
                    ModelState.AddModelError("nom", "Le nom du rôle est requis.");
                    var role = await _roleService.GetRoleByIdAsync(id);
                    return View(role);
                }

                await _roleService.UpdateRoleAsync(id, nom, description);
                TempData["SuccessMessage"] = "Rôle modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var role = await _roleService.GetRoleByIdAsync(id);
                return View(role);
            }
        }

        // POST: /RoleManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Rôle supprimé avec succès.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Rôle introuvable.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /RoleManagement/SeedPermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedPermissions()
        {
            try
            {
                await _permissionSeedService.SeedPermissionsAsync();
                TempData["SuccessMessage"] = "Permissions de base créées avec succès.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la création des permissions: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

