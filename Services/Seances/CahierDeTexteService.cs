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
                        .ThenInclude(c => c!.Enseignant)
                .ToListAsync();
        }

        // Récupérer les cahiers de texte d'un enseignant spécifique
        public async Task<List<PGSA_Licence3.Models.CahierDeTexte>> GetByEnseignantAsync(int enseignantId)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                    .ThenInclude(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                .Where(ct => ct.Seance != null && 
                             ct.Seance.Cours != null && 
                             ct.Seance.Cours.EnseignantId == enseignantId)
                .ToListAsync();
        }

        // Récupérer les cahiers de texte pour un étudiant délégué (selon sa classe)
        // MODIFIÉ: Plus de vérification de groupe, requête simplifiée pour éviter l'erreur EF.
        public async Task<List<PGSA_Licence3.Models.CahierDeTexte>> GetByDelegueAsync(int delegueId)
        {
            // Récupérer les informations de classe du délégué (cycle, niveau, spécialité)
            var delegue = await _context.Etudiants
                .FirstOrDefaultAsync(e => e.Id == delegueId);

            if (delegue == null) return new List<PGSA_Licence3.Models.CahierDeTexte>();

            // Récupérer les cahiers de texte qui correspondent à la classe de l'étudiant délégué
            // La requête ne compare que des IDs, ce qui est parfaitement traduisible en SQL.
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                    .ThenInclude(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                .Where(ct => 
                    ct.Seance != null &&
                    ct.Seance.CycleId == delegue.CycleId &&
                    ct.Seance.NiveauId == delegue.NiveauId &&
                    ct.Seance.SpecialiteId == delegue.SpecialiteId
                )
                .ToListAsync();
        }

        // Vérifier si un utilisateur est autorisé à accéder à une séance
        // MODIFIÉ: Utilise les rôles et ne vérifie plus l'appartenance au groupe pour les délégués.
        public async Task<bool> IsUserAuthorizedForSeanceAsync(int userId, int seanceId)
        {
            var seance = await _context.Seances
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == seanceId);

            if (seance == null) return false;

            // Vérifier si l'utilisateur est l'enseignant de cette séance
            if (seance.Cours?.EnseignantId == userId)
            {
                return true;
            }

            // Vérifier si l'utilisateur est un étudiant délégué de la classe concernée en utilisant son rôle
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Roles.Any(r => r.Nom == "Délégué") == true)
            {
                // Récupérer les informations de classe du délégué
                var delegue = await _context.Etudiants.FindAsync(userId);
                if (delegue != null)
                {
                    // Un délégué est autorisé s'il est de la même classe (cycle, niveau, spécialité)
                    return seance.CycleId == delegue.CycleId &&
                           seance.NiveauId == delegue.NiveauId &&
                           seance.SpecialiteId == delegue.SpecialiteId;
                }
            }
            
            // Un administrateur aurait aussi accès, vous pouvez ajouter cette vérification si nécessaire
            if (user?.Roles.Any(r => r.Nom == "Administrateur") == true)
            {
                return true;
            }

            return false;
        }

        // Récupérer un cahier de texte par ID
        public async Task<PGSA_Licence3.Models.CahierDeTexte?> GetByIdAsync(int id)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                    .ThenInclude(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                .FirstOrDefaultAsync(ct => ct.Id == id);
        }

        // Récupérer un cahier de texte par ID de séance
        public async Task<PGSA_Licence3.Models.CahierDeTexte?> GetBySeanceIdAsync(int seanceId)
        {
            return await _context.CahiersDeTexte
                .Include(ct => ct.Seance)
                    .ThenInclude(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
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

        // Récupérer les séances pour la liste déroulante (filtrées selon le rôle de l'utilisateur)
        // MODIFIÉ: Plus de vérification de groupe pour les délégués.
        public async Task<List<Seance>> GetSeancesAsync(int userId, bool isEnseignant, bool isDelegue)
        {
            if (isEnseignant)
            {
                return await _context.Seances
                    .Include(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                    .Include(s => s!.Groupe)
                    .Include(s => s!.Cycle)
                    .Include(s => s!.Niveau)
                    .Include(s => s!.Specialite)
                    .Where(s => s.Cours != null && s.Cours.EnseignantId == userId)
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
            else if (isDelegue)
            {
                var delegue = await _context.Etudiants
                    .FirstOrDefaultAsync(e => e.Id == userId);

                if (delegue == null) return new List<Seance>();

                return await _context.Seances
                    .Include(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                    .Include(s => s!.Groupe)
                    .Include(s => s!.Cycle)
                    .Include(s => s!.Niveau)
                    .Include(s => s!.Specialite)
                    .Where(s => 
                        s.CycleId == delegue.CycleId &&
                        s.NiveauId == delegue.NiveauId &&
                        s.SpecialiteId == delegue.SpecialiteId
                    )
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
            else // Pour un administrateur ou autre
            {
                return await _context.Seances
                    .Include(s => s!.Cours!)
                        .ThenInclude(c => c!.Enseignant)
                    .Include(s => s!.Groupe)
                    .Include(s => s!.Cycle)
                    .Include(s => s!.Niveau)
                    .Include(s => s!.Specialite)
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
        }
    }
}