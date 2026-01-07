using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Role_Managment
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }

        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<Permission> CreatePermissionAsync(string nom, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom de la permission ne peut pas être vide.");
            }

            var existingPermission = await _context.Permissions.FirstOrDefaultAsync(p => p.Nom == nom);
            if (existingPermission != null)
            {
                throw new InvalidOperationException("Une permission avec ce nom existe déjà.");
            }

            var permission = new Permission
            {
                Nom = nom,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<bool> CheckUserHasPermissionAsync(int userId, string permissionName)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Roles == null)
            {
                return false;
            }

            return user.Roles
                .SelectMany(r => r.Permissions ?? new List<Permission>())
                .Any(p => p.Nom == permissionName);
        }
    }
}


