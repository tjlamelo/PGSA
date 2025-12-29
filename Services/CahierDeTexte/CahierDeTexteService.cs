using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Services.CahierDeTexte
{
    public class CahierDeTexteService
    {
        private readonly ApplicationDbContext _db;

        public CahierDeTexteService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Crée un nouveau cahier de texte pour une séance
        /// </summary>
        public async Task<Models.CahierDeTexte?> CreateAsync(Models.CahierDeTexte cahier)
        {
            // Vérifier que la séance existe
            var seanceExists = await _db.Seances.AnyAsync(s => s.Id == cahier.SeanceId);
            if (!seanceExists)
            {
                return null;
            }

            // Vérifier qu'il n'existe pas déjà un cahier pour cette séance
            var existing = await _db.CahiersDeTexte
                .FirstOrDefaultAsync(c => c.SeanceId == cahier.SeanceId);
            
            if (existing != null)
            {
                return null; // Un cahier existe déjà pour cette séance
            }

            cahier.CreatedAt = DateTime.UtcNow;
            await _db.CahiersDeTexte.AddAsync(cahier);
            await _db.SaveChangesAsync();
            
            return cahier;
        }

        /// <summary>
        /// Met à jour un cahier de texte existant
        /// </summary>
        public async Task<Models.CahierDeTexte?> UpdateAsync(int id, Models.CahierDeTexte cahierUpdated)
        {
            var existing = await _db.CahiersDeTexte.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.ObjectifsPedagogiques = cahierUpdated.ObjectifsPedagogiques;
            existing.ActivitesRealisees = cahierUpdated.ActivitesRealisees;
            existing.RessourcesUtilisees = cahierUpdated.RessourcesUtilisees;
            existing.TravauxAFaire = cahierUpdated.TravauxAFaire;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Récupère le cahier de texte d'une séance spécifique
        /// </summary>
        public async Task<Models.CahierDeTexte?> GetBySeanceIdAsync(int seanceId)
        {
            return await _db.CahiersDeTexte
                .Include(c => c.Seance)
                .FirstOrDefaultAsync(c => c.SeanceId == seanceId);
        }

        /// <summary>
        /// Récupère un cahier de texte par son ID
        /// </summary>
        public async Task<Models.CahierDeTexte?> GetByIdAsync(int id)
        {
            return await _db.CahiersDeTexte
                .Include(c => c.Seance)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Récupère tous les cahiers de texte d'un cours
        /// </summary>
        public async Task<List<Models.CahierDeTexte>> GetByCoursIdAsync(int coursId)
        {
            return await _db.CahiersDeTexte
                .Include(c => c.Seance)
                .Where(c => c.Seance!.CoursId == coursId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Supprime un cahier de texte
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var cahier = await _db.CahiersDeTexte.FindAsync(id);
            if (cahier == null)
            {
                return false;
            }

            _db.CahiersDeTexte.Remove(cahier);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Vérifie si un cahier de texte existe pour une séance
        /// </summary>
        public async Task<bool> ExistsForSeanceAsync(int seanceId)
        {
            return await _db.CahiersDeTexte.AnyAsync(c => c.SeanceId == seanceId);
        }
    }
}