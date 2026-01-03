using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class DeleteGroupeService
    {
        private readonly ApplicationDbContext _context;

        public DeleteGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vérifie si on peut supprimer le groupe
        public async Task<bool> CanDeleteGroupeAsync(int groupeId)
        {
            var groupe = await _context.Groupes
                .Include(g => g.Seances)
                .FirstOrDefaultAsync(g => g.Id == groupeId);

            if (groupe == null)
            {
                return false;
            }

            // On peut supprimer seulement si pas de séances
            return groupe.Seances == null || !groupe.Seances.Any();
        }

        // Supprime un groupe
        public async Task DeleteGroupeAsync(int groupeId)
        {
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .Include(g => g.Seances)
                .FirstOrDefaultAsync(g => g.Id == groupeId);

            if (groupe == null)
            {
                throw new ArgumentException("Groupe non trouvé.");
            }

            if (groupe.Seances != null && groupe.Seances.Any())
            {
                throw new InvalidOperationException("Impossible de supprimer le groupe car il est utilisé dans des séances.");
            }

            // Vider la liste des étudiants
            if (groupe.Etudiants != null)
            {
                groupe.Etudiants.Clear();
            }

            // Supprimer le groupe
            _context.Groupes.Remove(groupe);
            await _context.SaveChangesAsync();
        }
    }
}