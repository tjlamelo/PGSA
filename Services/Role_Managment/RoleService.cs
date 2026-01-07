using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Role_Managment
{
    public class RoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .OrderBy(r => r.Nom)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> CreateRoleAsync(string nom, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom du rôle ne peut pas être vide.");
            }

            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == nom);
            if (existingRole != null)
            {
                throw new InvalidOperationException("Un rôle avec ce nom existe déjà.");
            }

            var role = new Role
            {
                Nom = nom,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateRoleAsync(int id, string nom, string? description = null)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                throw new ArgumentException("Rôle introuvable.");
            }

            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom du rôle ne peut pas être vide.");
            }

            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == nom && r.Id != id);
            if (existingRole != null)
            {
                throw new InvalidOperationException("Un rôle avec ce nom existe déjà.");
            }

            role.Nom = nom;
            role.Description = description;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return false;
            }

            if (role.Users != null && role.Users.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer un rôle assigné à des utilisateurs.");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}


