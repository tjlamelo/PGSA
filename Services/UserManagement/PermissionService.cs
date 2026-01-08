using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using PGSA_Licence3.Models.Seeders;

namespace PGSA_Licence3.Services.UserManagement
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Rôles
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Nom == name);
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrWhiteSpace(role.Nom))
                throw new ArgumentException("Le nom du rôle est obligatoire");

            // Vérifier si le rôle existe déjà
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nom == role.Nom);

            if (existingRole != null)
                throw new InvalidOperationException($"Un rôle avec le nom '{role.Nom}' existe déjà");

            role.CreatedAt = DateTime.UtcNow;
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrWhiteSpace(role.Nom))
                throw new ArgumentException("Le nom du rôle est obligatoire");

            var existingRole = await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == role.Id);

            if (existingRole == null)
                throw new Exception($"Le rôle avec l'ID {role.Id} n'existe pas");

            // Vérifier si un autre rôle avec le même nom existe
            var duplicateRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nom == role.Nom && r.Id != role.Id);

            if (duplicateRole != null)
                throw new InvalidOperationException($"Un autre rôle avec le nom '{role.Nom}' existe déjà");

            existingRole.Nom = role.Nom;
            existingRole.Description = role.Description;
            existingRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new Exception($"Le rôle avec l'ID {id} n'existe pas");

            // Vérifier si des utilisateurs sont associés à ce rôle
            if (role.Users != null && role.Users.Any())
                throw new InvalidOperationException($"Impossible de supprimer le rôle '{role.Nom}' car des utilisateurs y sont encore associés");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        // Permissions
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Include(p => p.Roles)
                .ToListAsync();
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Permission?> GetPermissionByNameAsync(string name)
        {
            return await _context.Permissions
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Nom == name);
        }

        public async Task<Permission> CreatePermissionAsync(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            if (string.IsNullOrWhiteSpace(permission.Nom))
                throw new ArgumentException("Le nom de la permission est obligatoire");

            // Vérifier si la permission existe déjà
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Nom == permission.Nom);

            if (existingPermission != null)
                throw new InvalidOperationException($"Une permission avec le nom '{permission.Nom}' existe déjà");

            permission.CreatedAt = DateTime.UtcNow;
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();

            return permission;
        }

        public async Task<Permission> UpdatePermissionAsync(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            if (string.IsNullOrWhiteSpace(permission.Nom))
                throw new ArgumentException("Le nom de la permission est obligatoire");

            var existingPermission = await _context.Permissions
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == permission.Id);

            if (existingPermission == null)
                throw new Exception($"La permission avec l'ID {permission.Id} n'existe pas");

            // Vérifier si une autre permission avec le même nom existe
            var duplicatePermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Nom == permission.Nom && p.Id != permission.Id);

            if (duplicatePermission != null)
                throw new InvalidOperationException($"Une autre permission avec le nom '{permission.Nom}' existe déjà");

            existingPermission.Nom = permission.Nom;
            existingPermission.Description = permission.Description;
            existingPermission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPermission;
        }

        public async Task DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.Roles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
                throw new Exception($"La permission avec l'ID {id} n'existe pas");

            // Vérifier si des rôles sont associés à cette permission
            if (permission.Roles != null && permission.Roles.Any())
                throw new InvalidOperationException($"Impossible de supprimer la permission '{permission.Nom}' car des rôles y sont encore associés");

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
        }

        // Gestion des associations Rôle-Permission
        public async Task<Role> AddPermissionToRoleAsync(int roleId, int permissionId)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new Exception($"Le rôle avec l'ID {roleId} n'existe pas");

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == permissionId);

            if (permission == null)
                throw new Exception($"La permission avec l'ID {permissionId} n'existe pas");

            if (role.Permissions == null)
                role.Permissions = new List<Permission>();

            // Vérifier si la permission est déjà associée au rôle
            if (role.Permissions.Any(p => p.Id == permissionId))
                throw new InvalidOperationException($"La permission '{permission.Nom}' est déjà associée au rôle '{role.Nom}'");

            role.Permissions.Add(permission);
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new Exception($"Le rôle avec l'ID {roleId} n'existe pas");

            var permission = role.Permissions
                .FirstOrDefault(p => p.Id == permissionId);

            if (permission == null)
                throw new Exception($"La permission avec l'ID {permissionId} n'est pas associée au rôle '{role.Nom}'");

            role.Permissions.Remove(permission);
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return role;
        }

        // Gestion des associations Utilisateur-Rôle
        public async Task<User> AddRoleToUserAsync(int userId, int roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception($"L'utilisateur avec l'ID {userId} n'existe pas");

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new Exception($"Le rôle avec l'ID {roleId} n'existe pas");

            if (user.Roles == null)
                user.Roles = new List<Role>();

            // Vérifier si le rôle est déjà associé à l'utilisateur
            if (user.Roles.Any(r => r.Id == roleId))
                throw new InvalidOperationException($"Le rôle '{role.Nom}' est déjà associé à l'utilisateur '{user.Prenom} {user.Nom}'");

            user.Roles.Add(role);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception($"L'utilisateur avec l'ID {userId} n'existe pas");

            var role = user.Roles
                .FirstOrDefault(r => r.Id == roleId);

            if (role == null)
                throw new Exception($"Le rôle avec l'ID {roleId} n'est pas associé à l'utilisateur '{user.Prenom} {user.Nom}'");

            user.Roles.Remove(role);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        // Vérification des permissions
        public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                return false;

            var user = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Roles == null)
                return false;

            return user.Roles
                .Any(r => r.Permissions != null && r.Permissions.Any(p => p.Nom == permissionName));
        }

        public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Roles == null)
                return new List<Permission>();

            return user.Roles
                .Where(r => r.Permissions != null)
                .SelectMany(r => r.Permissions!)
                .Distinct()
                .ToList();
        }

        // Méthodes utilitaires
        public async Task<bool> IsRoleNameUniqueAsync(string name, int? excludeRoleId = null)
        {
            return !await _context.Roles
                .AnyAsync(r => r.Nom == name && (excludeRoleId == null || r.Id != excludeRoleId.Value));
        }

        public async Task<bool> IsPermissionNameUniqueAsync(string name, int? excludePermissionId = null)
        {
            return !await _context.Permissions
                .AnyAsync(p => p.Nom == name && (excludePermissionId == null || p.Id != excludePermissionId.Value));
        }

        // Initialisation des données par défaut
        public async Task InitializeDefaultRolesAndPermissionsAsync()
        {
            // Vérifier si les rôles existent déjà
            if (!await _context.Roles.AnyAsync())
            {
                await RoleSeeder.SeedRolesAsync(_context);
            }

            // Vérifier si les permissions existent déjà
            if (!await _context.Permissions.AnyAsync())
            {
                await PermissionSeeder.SeedAsync(_context);
            }
        }
    }
}