using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Seances
{
    public class CahierDeTexteService
    {
        private readonly ApplicationDbContext _context;

        public CahierDeTexteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Récupérer tous les cahiers de texte
        public async Task<List<PGSA_Licence3.Models.CahierDeTexte>> GetAllAsync()
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                .ThenInclude(s => s!.Cours!)
                .ToListAsync();
        }

        // Récupérer un cahier de texte par ID
        public async Task<PGSA_Licence3.Models.CahierDeTexte?> GetByIdAsync(int id)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                .ThenInclude(s => s!.Cours!)
                .FirstOrDefaultAsync(ct => ct.Id == id);
        }

        // Récupérer un cahier de texte par ID de séance
        public async Task<PGSA_Licence3.Models.CahierDeTexte?> GetBySeanceIdAsync(int seanceId)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                .ThenInclude(s => s!.Cours!)
                .FirstOrDefaultAsync(ct => ct.SeanceId == seanceId);
        }

        // Créer ou mettre à jour un cahier de texte
        public async Task<PGSA_Licence3.Models.CahierDeTexte> CreateOrUpdateAsync(PGSA_Licence3.Models.CahierDeTexte cahierDeTexte)
        {
            if (cahierDeTexte.Id == 0)
            {
                // Création
                cahierDeTexte.CreatedAt = DateTime.UtcNow;
                _context.CahiersDeTexte.Add(cahierDeTexte);
            }
            else
            {
                // Mise à jour
                var existing = await _context.CahiersDeTexte.FindAsync(cahierDeTexte.Id);
                if (existing == null) throw new Exception("Cahier de texte non trouvé");

                existing.SeanceId = cahierDeTexte.SeanceId;
                existing.ObjectifsPedagogiques = cahierDeTexte.ObjectifsPedagogiques;
                existing.ActivitesRealisees = cahierDeTexte.ActivitesRealisees;
                existing.RessourcesUtilisees = cahierDeTexte.RessourcesUtilisees;
                existing.TravauxAFaire = cahierDeTexte.TravauxAFaire;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return cahierDeTexte;
        }

        // Supprimer un cahier de texte
        public async Task DeleteAsync(int id)
        {
            var cahierDeTexte = await _context.CahiersDeTexte.FindAsync(id);
            if (cahierDeTexte == null) throw new Exception("Cahier de texte non trouvé");

            _context.CahiersDeTexte.Remove(cahierDeTexte);
            await _context.SaveChangesAsync();
        }

        // Récupérer toutes les séances pour la liste déroulante
        public async Task<List<Seance>> GetSeancesAsync()
        {
            return await _context.Seances
                .Include(s => s!.Cours!)
                .Include(s => s!.Groupe)
                .Include(s => s!.Cycle)
                .Include(s => s!.Niveau)
                .Include(s => s!.Specialite)
                .OrderByDescending(s => s!.DateHeureDebut)
                .ToListAsync();
        }
    }
}