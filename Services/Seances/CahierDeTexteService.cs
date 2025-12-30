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

        /// <summary>
        /// Récupère tous les cahiers de texte avec les détails de la séance et du cours associés.
        /// Utilisé pour afficher la liste principale.
        /// </summary>
        public async Task<List<CahierDeTexte>> GetAllAsync()
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                .ThenInclude(s => s!.Cours!) // L'opérateur ! est nécessaire pour rassurer le compilateur
                .ToListAsync();
        }

        /// <summary>
        /// Récupère un cahier de texte par son ID avec les détails de la séance et du cours.
        /// Peut être utilisé pour d'autres vues qui nécessitent les informations complètes.
        /// </summary>
        public async Task<CahierDeTexte?> GetByIdAsync(int id)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                .ThenInclude(s => s!.Cours!) // L'opérateur ! est nécessaire
                .FirstOrDefaultAsync(ct => ct.Id == id);
        }

        /// <summary>
        /// Récupère un cahier de texte par son ID SANS charger les données liées (Seance, Cours).
        /// Méthode sécurisée et optimisée pour les appels AJAX de type "édition".
        /// </summary>
        public async Task<CahierDeTexte?> GetByIdForEditAsync(int id)
        {
            // FindAsync est très rapide et ne charge pas les propriétés de navigation,
            // ce qui évite les erreurs si les données liées sont manquantes.
            return await _context.CahiersDeTexte.FindAsync(id);
        }

        /// <summary>
        /// Récupère un cahier de texte par l'ID de sa séance, sans charger les données liées.
        /// Utilisé pour vérifier si un cahier existe déjà pour une séance donnée.
        /// </summary>
        public async Task<CahierDeTexte?> GetBySeanceIdAsync(int seanceId)
        {
            // On retire les Include car ils ne sont pas nécessaires pour cette opération
            // et peuvent causer une erreur si les données liées sont manquantes.
            return await _context.CahiersDeTexte
                .FirstOrDefaultAsync(ct => ct.SeanceId == seanceId);
        }

        /// <summary>
        /// Crée un nouveau cahier de texte ou met à jour un cahier existant.
        /// </summary>
        public async Task<CahierDeTexte> CreateOrUpdateAsync(CahierDeTexte cahierDeTexte)
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

        /// <summary>
        /// Supprime un cahier de texte par son ID.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var cahierDeTexte = await _context.CahiersDeTexte.FindAsync(id);
            if (cahierDeTexte == null) throw new Exception("Cahier de texte non trouvé");

            _context.CahiersDeTexte.Remove(cahierDeTexte);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Récupère toutes les séances avec leurs détails (cours, groupe, etc.).
        /// Utilisé pour peupler la liste déroulante des séances dans le modal.
        /// </summary>
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