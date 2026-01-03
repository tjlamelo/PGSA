using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class ImportStudentsToGroupeService
    {
        private readonly ApplicationDbContext _context;

        public ImportStudentsToGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Importe plusieurs étudiants dans un groupe
        public async Task<ImportResult> ImportStudentsToGroupeAsync(List<int> etudiantIds, int groupeId)
        {
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Id == groupeId);
            if (groupe == null)
            {
                throw new ArgumentException("Groupe non trouvé.");
            }

            var etudiants = await _context.Etudiants
                .Where(e => etudiantIds.Contains(e.Id))
                .ToListAsync();

            var result = new ImportResult
            {
                TotalRequested = etudiantIds.Count,
                SuccessfullyImported = 0,
                AlreadyInGroup = 0,
                NotFound = etudiantIds.Count - etudiants.Count
            };

            foreach (var etudiant in etudiants)
            {
                if (groupe.Etudiants != null && groupe.Etudiants.Contains(etudiant))
                {
                    result.AlreadyInGroup++;
                }
                else
                {
                    if (groupe.Etudiants == null)
                    {
                        groupe.Etudiants = new List<Etudiant>();
                    }
                    groupe.Etudiants.Add(etudiant);
                    result.SuccessfullyImported++;
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        // Liste des étudiants libres
        public async Task<List<Etudiant>> GetAvailableStudentsAsync()
        {
            // Tous les étudiants
            var allStudents = await _context.Etudiants.ToListAsync();
            
            // IDs des étudiants dans des groupes
            var studentsInGroups = await _context.Groupes
                .Where(g => g.Etudiants != null)
                .SelectMany(g => g.Etudiants)
                .Select(e => e.Id)
                .ToListAsync();

            // Retourner ceux qui ne sont pas dans un groupe
            return allStudents
                .Where(e => !studentsInGroups.Contains(e.Id))
                .OrderBy(e => e.Nom)
                .ThenBy(e => e.Prenom)
                .ToList();
        }

        // Liste des étudiants dans un groupe
        public async Task<List<Etudiant>> GetStudentsInGroupeAsync(int groupeId)
        {
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Id == groupeId);

            return groupe?.Etudiants
                .OrderBy(e => e.Nom)
                .ThenBy(e => e.Prenom)
                .ToList() ?? new List<Etudiant>();
        }
    }

    // Résultat de l'import
    public class ImportResult
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyImported { get; set; }
        public int AlreadyInGroup { get; set; }
        public int NotFound { get; set; }
    }
}