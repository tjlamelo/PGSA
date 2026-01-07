using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Role_Managment
{
    public class UserRoleService
    {
        private readonly ApplicationDbContext _context;

        public UserRoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsersByRoleAsync(int roleId)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            return role?.Users?.ToList() ?? new List<User>();
        }

        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Roles?.ToList() ?? new List<Role>();
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("Utilisateur introuvable.");
            }

            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
            {
                throw new ArgumentException("Rôle introuvable.");
            }

            if (user.Roles == null)
            {
                user.Roles = new List<Role>();
            }

            if (!user.Roles.Any(r => r.Id == roleId))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Roles == null)
            {
                return;
            }

            var roleToRemove = user.Roles.FirstOrDefault(r => r.Id == roleId);
            if (roleToRemove != null)
            {
                user.Roles.Remove(roleToRemove);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Roles)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }
    }
}


