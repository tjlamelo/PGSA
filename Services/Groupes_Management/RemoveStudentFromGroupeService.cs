using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class RemoveStudentFromGroupeService
    {
        private readonly ApplicationDbContext _context;

        public RemoveStudentFromGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retire un étudiant de son groupe
        public async Task RemoveStudentFromGroupeAsync(int etudiantId)
        {
            var etudiant = await _context.Etudiants.FindAsync(etudiantId);
            if (etudiant == null)
            {
                throw new ArgumentException("Étudiant non trouvé.");
            }

            // Trouver le groupe qui contient cet étudiant
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Etudiants != null && g.Etudiants.Any(e => e.Id == etudiantId));

            if (groupe != null)
            {
                groupe.Etudiants.Remove(etudiant);
                await _context.SaveChangesAsync();
            }
        }
    }
}