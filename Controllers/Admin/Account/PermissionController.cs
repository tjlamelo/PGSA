using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.UserManagement;
using PGSA_Licence3.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace PGSA_Licence3.Controllers.Auth.Admin.Account
{
    [Route("Admin/Permissions")]
    // [Authorize(Roles = "Administrateur")]
    public class PermissionController : Controller
    {
        private readonly PermissionService _permissionService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(ApplicationDbContext context, ILogger<PermissionController> logger)
        {
            _context = context;
            _logger = logger;
            _permissionService = new PermissionService(context);
        }

        // Vue principale
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Chargement de la page de gestion des permissions.");
            try
            {
                var model = new PermissionManagementViewModel
                {
                    Roles = await _permissionService.GetAllRolesAsync(),
                    Permissions = await _permissionService.GetAllPermissionsAsync(),
                    Users = await _context.Users
                        .Include(u => u.Roles)
                        .ToListAsync()
                };
                _logger.LogInformation("Page de gestion des permissions chargée avec succès. {RolesCount} rôles, {PermissionsCount} permissions, {UsersCount} utilisateurs.", 
                    model.Roles.Count, model.Permissions.Count, model.Users.Count);
                return View("~/Views/Admin/UserManagement/Permission.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur est survenue lors du chargement de la page de gestion des permissions.");
                TempData["Error"] = "Impossible de charger les données.";
                return View("~/Views/Admin/UserManagement/Permission.cshtml", new PermissionManagementViewModel());
            }
        }

        // GESTION DES RÔLES

        // Créer un rôle - GET
        [HttpGet("CreateRole")]
        public IActionResult CreateRole()
        {
            _logger.LogInformation("Affichage du formulaire de création de rôle.");
            return View("~/Views/Admin/UserManagement/CreateRole.cshtml");
        }

        // Créer un rôle - POST
        [HttpPost("CreateRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(Role role)
        {
            _logger.LogInformation("Tentative de création d'un rôle avec le nom: {RoleName}", role.Nom);
            
            if (ModelState.IsValid)
            {
                try
                {
                    var newRole = await _permissionService.CreateRoleAsync(role);
                    _logger.LogInformation("Rôle créé avec succès: {RoleName} (ID: {RoleId})", newRole.Nom, newRole.Id);
                    TempData["Success"] = "Rôle créé avec succès";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création du rôle: {RoleName}", role.Nom);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Modèle invalide pour la création de rôle.");
            }

            return View("~/Views/Admin/UserManagement/CreateRole.cshtml", role);
        }

        // Modifier un rôle - GET
        [HttpGet("EditRole/{id}")]
        public async Task<IActionResult> EditRole(int id)
        {
            _logger.LogInformation("Affichage du formulaire de modification pour le rôle ID: {RoleId}", id);
            
            try
            {
                var role = await _permissionService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Rôle non trouvé avec l'ID: {RoleId}", id);
                    TempData["Error"] = "Rôle non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/UserManagement/EditRole.cshtml", role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du rôle ID: {RoleId}", id);
                TempData["Error"] = "Erreur lors du chargement du rôle";
                return RedirectToAction(nameof(Index));
            }
        }

        // Modifier un rôle - POST
        [HttpPost("EditRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(Role role)
        {
            _logger.LogInformation("Tentative de modification du rôle ID: {RoleId}", role.Id);
            
            if (ModelState.IsValid)
            {
                try
                {
                    var updatedRole = await _permissionService.UpdateRoleAsync(role);
                    _logger.LogInformation("Rôle modifié avec succès: {RoleName} (ID: {RoleId})", updatedRole.Nom, updatedRole.Id);
                    TempData["Success"] = "Rôle mis à jour avec succès";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la modification du rôle ID: {RoleId}", role.Id);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Modèle invalide pour la modification de rôle ID: {RoleId}", role.Id);
            }

            return View("~/Views/Admin/UserManagement/EditRole.cshtml", role);
        }

        // Supprimer un rôle - POST
        [HttpPost("DeleteRole/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(int id)
        {
            _logger.LogInformation("Tentative de suppression du rôle ID: {RoleId}", id);
            
            try
            {
                await _permissionService.DeleteRoleAsync(id);
                _logger.LogInformation("Rôle supprimé avec succès. ID: {RoleId}", id);
                TempData["Success"] = "Rôle supprimé avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du rôle ID: {RoleId}", id);
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GESTION DES PERMISSIONS

        // Créer une permission - GET
        [HttpGet("CreatePermission")]
        public IActionResult CreatePermission()
        {
            _logger.LogInformation("Affichage du formulaire de création de permission.");
            return View("~/Views/Admin/UserManagement/CreatePermission.cshtml");
        }

        // Créer une permission - POST
        [HttpPost("CreatePermission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePermission(Permission permission)
        {
            _logger.LogInformation("Tentative de création d'une permission avec le nom: {PermissionName}", permission.Nom);
            
            if (ModelState.IsValid)
            {
                try
                {
                    var newPermission = await _permissionService.CreatePermissionAsync(permission);
                    _logger.LogInformation("Permission créée avec succès: {PermissionName} (ID: {PermissionId})", newPermission.Nom, newPermission.Id);
                    TempData["Success"] = "Permission créée avec succès";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la création de la permission: {PermissionName}", permission.Nom);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Modèle invalide pour la création de permission.");
            }

            return View("~/Views/Admin/UserManagement/CreatePermission.cshtml", permission);
        }

        // Modifier une permission - GET
        [HttpGet("EditPermission/{id}")]
        public async Task<IActionResult> EditPermission(int id)
        {
            _logger.LogInformation("Affichage du formulaire de modification pour la permission ID: {PermissionId}", id);
            
            try
            {
                var permission = await _permissionService.GetPermissionByIdAsync(id);
                if (permission == null)
                {
                    _logger.LogWarning("Permission non trouvée avec l'ID: {PermissionId}", id);
                    TempData["Error"] = "Permission non trouvée";
                    return RedirectToAction(nameof(Index));
                }

                return View("~/Views/Admin/UserManagement/EditPermission.cshtml", permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la permission ID: {PermissionId}", id);
                TempData["Error"] = "Erreur lors du chargement de la permission";
                return RedirectToAction(nameof(Index));
            }
        }

        // Modifier une permission - POST
        [HttpPost("EditPermission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPermission(Permission permission)
        {
            _logger.LogInformation("Tentative de modification de la permission ID: {PermissionId}", permission.Id);
            
            if (ModelState.IsValid)
            {
                try
                {
                    var updatedPermission = await _permissionService.UpdatePermissionAsync(permission);
                    _logger.LogInformation("Permission modifiée avec succès: {PermissionName} (ID: {PermissionId})", updatedPermission.Nom, updatedPermission.Id);
                    TempData["Success"] = "Permission mise à jour avec succès";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la modification de la permission ID: {PermissionId}", permission.Id);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Modèle invalide pour la modification de permission ID: {PermissionId}", permission.Id);
            }

            return View("~/Views/Admin/UserManagement/EditPermission.cshtml", permission);
        }

        // Supprimer une permission - POST
        [HttpPost("DeletePermission/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePermission(int id)
        {
            _logger.LogInformation("Tentative de suppression de la permission ID: {PermissionId}", id);
            
            try
            {
                await _permissionService.DeletePermissionAsync(id);
                _logger.LogInformation("Permission supprimée avec succès. ID: {PermissionId}", id);
                TempData["Success"] = "Permission supprimée avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la permission ID: {PermissionId}", id);
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GESTION DES ASSOCIATIONS RÔLE-PERMISSION

        // Gérer les permissions d'un rôle - GET
        [HttpGet("RolePermissions/{id}")]
        public async Task<IActionResult> RolePermissions(int id)
        {
            _logger.LogInformation("Affichage de la page de gestion des permissions pour le rôle ID: {RoleId}", id);
            
            try
            {
                var role = await _permissionService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Rôle non trouvé avec l'ID: {RoleId}", id);
                    TempData["Error"] = "Rôle non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                var allPermissions = await _permissionService.GetAllPermissionsAsync();
                var rolePermissions = role.Permissions?.Select(p => p.Id).ToList() ?? new List<int>();

                ViewBag.Role = role;
                ViewBag.AllPermissions = allPermissions;
                ViewBag.RolePermissions = rolePermissions;

                return View("~/Views/Admin/UserManagement/RolePermissions.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des permissions pour le rôle ID: {RoleId}", id);
                TempData["Error"] = "Erreur lors du chargement des permissions du rôle";
                return RedirectToAction(nameof(Index));
            }
        }

        // Mettre à jour les permissions d'un rôle - POST
        [HttpPost("RolePermissions/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRolePermissions(int id, List<int> selectedPermissions)
        {
            _logger.LogInformation("Tentative de mise à jour des permissions pour le rôle ID: {RoleId}. Permissions sélectionnées: {@SelectedPermissions}", id, selectedPermissions);
            
            try
            {
                var role = await _permissionService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Rôle non trouvé avec l'ID: {RoleId}", id);
                    TempData["Error"] = "Rôle non trouvé";
                    return RedirectToAction(nameof(Index));
                }

                // Obtenir les permissions actuelles du rôle
                var currentPermissions = role.Permissions?.Select(p => p.Id).ToList() ?? new List<int>();

                // Ajouter les nouvelles permissions
                foreach (var permissionId in selectedPermissions ?? new List<int>())
                {
                    if (!currentPermissions.Contains(permissionId))
                    {
                        _logger.LogInformation("Ajout de la permission ID: {PermissionId} au rôle ID: {RoleId}", permissionId, id);
                        await _permissionService.AddPermissionToRoleAsync(id, permissionId);
                    }
                }

                // Supprimer les permissions qui ne sont plus sélectionnées
                foreach (var permissionId in currentPermissions)
                {
                    if (selectedPermissions == null || !selectedPermissions.Contains(permissionId))
                    {
                        _logger.LogInformation("Suppression de la permission ID: {PermissionId} du rôle ID: {RoleId}", permissionId, id);
                        await _permissionService.RemovePermissionFromRoleAsync(id, permissionId);
                    }
                }

                _logger.LogInformation("Permissions du rôle ID: {RoleId} mises à jour avec succès", id);
                TempData["Success"] = "Permissions du rôle mises à jour avec succès";
                return RedirectToAction(nameof(RolePermissions), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des permissions pour le rôle ID: {RoleId}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(RolePermissions), new { id });
            }
        }

        // GESTION DES ASSOCIATIONS UTILISATEUR-RÔLE

        // Gérer les rôles d'un utilisateur - GET
        [HttpGet("UserRoles/{id}")]
        public async Task<IActionResult> UserRoles(int id)
        {
            _logger.LogInformation("Affichage de la page de gestion des rôles pour l'utilisateur ID: {UserId}", id);
            
            try
            {
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé avec l'ID: {UserId}", id);
                    TempData["Error"] = "Utilisateur non trouvé";
                    return RedirectToAction("Index", "Account");
                }

                var allRoles = await _permissionService.GetAllRolesAsync();
                var userRoles = user.Roles?.Select(r => r.Id).ToList() ?? new List<int>();

                ViewBag.User = user;
                ViewBag.AllRoles = allRoles;
                ViewBag.UserRoles = userRoles;

                return View("~/Views/Admin/UserManagement/UserRoles.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des rôles pour l'utilisateur ID: {UserId}", id);
                TempData["Error"] = "Erreur lors du chargement des rôles de l'utilisateur";
                return RedirectToAction("Index", "Account");
            }
        }

        // Mettre à jour les rôles d'un utilisateur - POST
        [HttpPost("UserRoles/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(int id, List<int> selectedRoles)
        {
            _logger.LogInformation("Tentative de mise à jour des rôles pour l'utilisateur ID: {UserId}. Rôles sélectionnés: {@SelectedRoles}", id, selectedRoles);
            
            try
            {
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé avec l'ID: {UserId}", id);
                    TempData["Error"] = "Utilisateur non trouvé";
                    return RedirectToAction("Index", "Account");
                }

                // Obtenir les rôles actuels de l'utilisateur
                var currentRoles = user.Roles?.Select(r => r.Id).ToList() ?? new List<int>();

                // Ajouter les nouveaux rôles
                foreach (var roleId in selectedRoles ?? new List<int>())
                {
                    if (!currentRoles.Contains(roleId))
                    {
                        _logger.LogInformation("Ajout du rôle ID: {RoleId} à l'utilisateur ID: {UserId}", roleId, id);
                        await _permissionService.AddRoleToUserAsync(id, roleId);
                    }
                }

                // Supprimer les rôles qui ne sont plus sélectionnés
                foreach (var roleId in currentRoles)
                {
                    if (selectedRoles == null || !selectedRoles.Contains(roleId))
                    {
                        _logger.LogInformation("Suppression du rôle ID: {RoleId} de l'utilisateur ID: {UserId}", roleId, id);
                        await _permissionService.RemoveRoleFromUserAsync(id, roleId);
                    }
                }

                _logger.LogInformation("Rôles de l'utilisateur ID: {UserId} mis à jour avec succès", id);
                TempData["Success"] = "Rôles de l'utilisateur mis à jour avec succès";
                return RedirectToAction(nameof(UserRoles), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des rôles pour l'utilisateur ID: {UserId}", id);
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(UserRoles), new { id });
            }
        }
    }

    // ViewModel pour la vue de gestion des permissions
    public class PermissionManagementViewModel
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public List<User> Users { get; set; } = new List<User>();
    }
}