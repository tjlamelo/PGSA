using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class AddStudentToGroupeService
    {
        private readonly ApplicationDbContext _context;

        public AddStudentToGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ajoute un étudiant à un groupe
        public async Task AddStudentToGroupeAsync(int etudiantId, int groupeId)
        {
            var etudiant = await _context.Etudiants.FindAsync(etudiantId);
            if (etudiant == null)
            {
                throw new ArgumentException("Étudiant non trouvé.");
            }

            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Id == groupeId);
            if (groupe == null)
            {
                throw new ArgumentException("Groupe non trouvé.");
            }

            // Ajouter seulement s'il n'y est pas déjà
            if (groupe.Etudiants != null && !groupe.Etudiants.Contains(etudiant))
            {
                groupe.Etudiants.Add(etudiant);
            }

            await _context.SaveChangesAsync();
        }

        // Change un étudiant de groupe
        public async Task ChangeStudentGroupeAsync(int etudiantId, int newGroupeId)
        {
            var etudiant = await _context.Etudiants.FindAsync(etudiantId);
            if (etudiant == null)
            {
                throw new ArgumentException("Étudiant non trouvé.");
            }

            // Trouver l'ancien groupe
            var currentGroupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Etudiants != null && g.Etudiants.Any(e => e.Id == etudiantId));

            var newGroupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Id == newGroupeId);
            if (newGroupe == null)
            {
                throw new ArgumentException("Nouveau groupe non trouvé.");
            }

            // Retirer de l'ancien groupe
            if (currentGroupe != null && currentGroupe.Etudiants != null)
            {
                currentGroupe.Etudiants.Remove(etudiant);
            }

            // Ajouter au nouveau
            if (newGroupe.Etudiants != null && !newGroupe.Etudiants.Contains(etudiant))
            {
                newGroupe.Etudiants.Add(etudiant);
            }

            await _context.SaveChangesAsync();
        }
    }
}