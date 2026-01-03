using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class GetGroupeDetailsService
    {
        private readonly ApplicationDbContext _context;

        public GetGroupeDetailsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Récupère les détails d'un groupe
        public async Task<Groupe?> GetGroupeByIdAsync(int id)
        {
            return await _context.Groupes
                .Include(g => g.Etudiants)
                .Include(g => g.Seances)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        // Vérifie si un groupe existe
        public async Task<bool> GroupeExistsAsync(int id)
        {
            return await _context.Groupes.AnyAsync(g => g.Id == id);
        }
    }
}