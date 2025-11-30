using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Services.Users
{
    public class UserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Users.Include(u => u.Roles).ToListAsync();
        }

        public async Task<User?> GetUserWithRolesAsync(int id)
        {
            return await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _db.Roles.ToListAsync();
        }

        public async Task<List<Groupe>> GetAllGroupesAsync()
        {
            return await _db.Groupes.ToListAsync();
        }

        public async Task<Enseignant?> GetEnseignantWithGroupesAsync(int id)
        {
            return await _db.Enseignants.Include(e => e.Groupes).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            var role = await _db.Roles.FindAsync(roleId);
            if (role == null) return false;

            user.Roles ??= new List<Role>();

            if (!user.Roles.Any(r => r.Id == roleId))
            {
                user.Roles.Add(role);
                await _db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> RevokeRoleAsync(int userId, int roleId)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;
            user.Roles ??= new List<Role>();

            var role = user.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            user.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Enseignant> CreateEnseignantAsync(Enseignant enseignant, List<int> coursIds)
        {
            await _db.Enseignants.AddAsync(enseignant);
            await _db.SaveChangesAsync();

            if (coursIds != null && coursIds.Count > 0)
            {
                var cours = await _db.Cours.Where(c => coursIds.Contains(c.Id)).ToListAsync();
                foreach (var c in cours)
                {
                    c.EnseignantId = enseignant.Id;
                }
                await _db.SaveChangesAsync();
            }

            return enseignant;
        }

        public async Task<bool> AssignGroupeAsync(int enseignantId, int groupeId)
        {
            var enseignant = await _db.Enseignants.Include(e => e.Groupes).FirstOrDefaultAsync(e => e.Id == enseignantId);
            if (enseignant == null) return false;

            var groupe = await _db.Groupes.FindAsync(groupeId);
            if (groupe == null) return false;

            enseignant.Groupes ??= new List<Groupe>();

            if (!enseignant.Groupes.Any(g => g.Id == groupeId))
            {
                enseignant.Groupes.Add(groupe);
                await _db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RevokeGroupeAsync(int enseignantId, int groupeId)
        {
            var enseignant = await _db.Enseignants.Include(e => e.Groupes).FirstOrDefaultAsync(e => e.Id == enseignantId);
            if (enseignant == null) return false;

            enseignant.Groupes ??= new List<Groupe>();

            var groupe = enseignant.Groupes.FirstOrDefault(g => g.Id == groupeId);
            if (groupe == null) return false;

            enseignant.Groupes.Remove(groupe);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
