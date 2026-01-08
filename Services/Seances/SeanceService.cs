using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Seances
{
    public class SeanceService
    {
        private readonly ApplicationDbContext _context;

        public SeanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Récupérer toutes les séances
        public async Task<List<Seance>> GetAllAsync()
        {
            return await _context.Seances
                .Include(s => s.Cours)
                .ThenInclude(c => c!.Enseignant)
                .Include(s => s.Groupe)
                .Include(s => s.Cycle)
                .Include(s => s.Niveau)
                .Include(s => s.Specialite)
                .ToListAsync();
        }

        // Récupérer les séances d'un enseignant spécifique
        public async Task<List<Seance>> GetSeancesByEnseignantAsync(int enseignantId)
        {
            return await _context.Seances
                .Include(s => s.Cours)
                .ThenInclude(c => c!.Enseignant)
                .Include(s => s.Groupe)
                .Include(s => s.Cycle)
                .Include(s => s.Niveau)
                .Include(s => s.Specialite)
                .Where(s => s.Cours != null && s.Cours.EnseignantId == enseignantId)
                .ToListAsync();
        }

        // Récupérer les séances pour un étudiant délégué (selon sa classe)
       // Récupérer les séances pour un étudiant délégué (selon sa classe)
public async Task<List<Seance>> GetSeancesByDelegueAsync(int delegueId)
{
    // 1. Récupérer l'étudiant délégué avec ses groupes
    var delegue = await _context.Etudiants
        .Include(e => e.Groupes)
        .FirstOrDefaultAsync(e => e.Id == delegueId);

    if (delegue == null) return new List<Seance>();

    // 2. FIX: Extraire les IDs des groupes dans une liste simple (List<int>)
    // C'est cette étape qui permet à MySQL de traduire la requête correctement.
    var idsGroupesDelegue = delegue.Groupes.Select(g => g.Id).ToList();

    // 3. Récupérer les séances qui correspondent à la classe du délégué
    return await _context.Seances
        .Include(s => s.Cours)
            .ThenInclude(c => c!.Enseignant)
        .Include(s => s.Groupe)
        .Include(s => s.Cycle)
        .Include(s => s.Niveau)
        .Include(s => s.Specialite)
        .Where(s => 
            (s.CycleId == delegue.CycleId) &&
            (s.NiveauId == delegue.NiveauId) &&
            (s.SpecialiteId == delegue.SpecialiteId) &&
            // Utilisation de .Contains() sur la liste simple d'IDs
            (s.GroupeId == null || idsGroupesDelegue.Contains(s.GroupeId.Value))
        )
        .OrderByDescending(s => s.DateHeureDebut) // Optionnel: trier par date
        .ToListAsync();
}

        // Vérifier si un utilisateur est un enseignant
        public async Task<bool> IsEnseignantAsync(int userId)
        {
            return await _context.Enseignants.AnyAsync(e => e.Id == userId);
        }

        // Vérifier si un utilisateur est un étudiant délégué
        public async Task<bool> IsDelegueAsync(int userId)
        {
            // Vérifier si l'utilisateur est un étudiant
            var isEtudiant = await _context.Etudiants.AnyAsync(e => e.Id == userId);
            if (!isEtudiant) return false;
            
            // Vérifier si l'étudiant a le rôle de délégué
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            return user?.Roles.Any(r => r.Nom == "Délégué") ?? false;
        }

        // Récupérer une séance par Id
        public async Task<Seance?> GetByIdAsync(int id)
        {
            return await _context.Seances
                .Include(s => s.Cours)
                .ThenInclude(c => c!.Enseignant)
                .Include(s => s.Groupe)
                .Include(s => s.Cycle)
                .Include(s => s.Niveau)
                .Include(s => s.Specialite)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // Créer ou mettre à jour une séance
        public async Task<Seance> CreateOrUpdateAsync(Seance seance)
        {
            // Vérification des conflits
            var conflictDetails = await CheckConflictAsync(seance);
            if (conflictDetails.Any())
            {
                var conflictMessage = string.Join("; ", conflictDetails);
                throw new Exception($"Conflit détecté : {conflictMessage}");
            }

            if (seance.Id == 0)
            {
                // Création
                seance.CreatedAt = DateTime.UtcNow;
                seance.Statut = StatutSeance.Planifiee;
                _context.Seances.Add(seance);
            }
            else
            {
                // Mise à jour
                var existing = await _context.Seances.FindAsync(seance.Id);
                if (existing == null) throw new Exception("Séance non trouvée");

                // Mise à jour de toutes les propriétés
                existing.CoursId = seance.CoursId;
                existing.GroupeId = seance.GroupeId;
                existing.CycleId = seance.CycleId;
                existing.NiveauId = seance.NiveauId;
                existing.SpecialiteId = seance.SpecialiteId;
                existing.DateHeureDebut = seance.DateHeureDebut;
                existing.DateHeureFin = seance.DateHeureFin;
                existing.Salle = seance.Salle;
                existing.Type = seance.Type;
                existing.Duree = seance.Duree; // Ajout de la mise à jour de la durée
                existing.Statut = seance.Statut;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return seance;
        }

        // Supprimer une séance
        public async Task DeleteAsync(int id)
        {
            var seance = await _context.Seances.FindAsync(id);
            if (seance == null) throw new Exception("Séance non trouvée");

            _context.Seances.Remove(seance);
            await _context.SaveChangesAsync();
        }

        // Vérifier les conflits avec retour de détails
        public async Task<List<string>> CheckConflictAsync(Seance seance)
        {
            var conflicts = new List<string>();

            // Vérifier les conflits de salle
            var salleConflicts = await _context.Seances
                .Where(s => s.Id != seance.Id && // ignorer la séance elle-même
                           s.Salle == seance.Salle && // même salle
                           s.Statut != StatutSeance.Annulee && // séance non annulée
                           (
                               (seance.DateHeureDebut < s.DateHeureFin && s.DateHeureDebut < seance.DateHeureFin) // chevauchement
                           ))
                .ToListAsync();

            if (salleConflicts.Any())
            {
                conflicts.Add($"La salle {seance.Salle} est déjà utilisée pendant cette période");
            }

            // Vérifier les conflits d'enseignant
            if (seance.Cours?.EnseignantId != null)
            {
                var enseignantConflicts = await _context.Seances
                    .Where(s => s.Id != seance.Id && // ignorer la séance elle-même
                               s.Cours != null &&
                               s.Cours.EnseignantId == seance.Cours.EnseignantId && // même enseignant
                               s.Statut != StatutSeance.Annulee && // séance non annulée
                               (
                                   (seance.DateHeureDebut < s.DateHeureFin && s.DateHeureDebut < seance.DateHeureFin) // chevauchement
                               ))
                    .ToListAsync();

                if (enseignantConflicts.Any())
                {
                    conflicts.Add($"L'enseignant {seance.Cours.Enseignant?.Prenom} {seance.Cours.Enseignant?.Nom} a déjà une séance pendant cette période");
                }
            }

            // Vérifier les conflits de groupe
            if (seance.GroupeId != null)
            {
                var groupeConflicts = await _context.Seances
                    .Where(s => s.Id != seance.Id && // ignorer la séance elle-même
                               s.GroupeId == seance.GroupeId && // même groupe
                               s.Statut != StatutSeance.Annulee && // séance non annulée
                               (
                                   (seance.DateHeureDebut < s.DateHeureFin && s.DateHeureDebut < seance.DateHeureFin) // chevauchement
                               ))
                    .ToListAsync();

                if (groupeConflicts.Any())
                {
                    conflicts.Add($"Le groupe {seance.Groupe?.Nom} a déjà une séance pendant cette période");
                }
            }

            return conflicts;
        }

        // Récupérer les cours pour liste déroulante (modifié pour filtrer par enseignant)
        public async Task<List<Cours>> GetCoursAsync(int? enseignantId = null)
        {
            var query = _context.Cours
                .Include(c => c.Enseignant)
                .AsQueryable();

            // Si un ID d'enseignant est fourni, filtrer les cours pour cet enseignant
            if (enseignantId.HasValue)
            {
                query = query.Where(c => c.EnseignantId == enseignantId.Value);
            }

            return await query.OrderBy(c => c.Nom).ToListAsync();
        }

        // Récupérer les groupes pour liste déroulante
        public async Task<List<Groupe>> GetGroupesAsync()
        {
            return await _context.Groupes.OrderBy(g => g.Nom).ToListAsync();
        }

        // Récupérer les cycles pour liste déroulante (modifié pour filtrer par enseignant)
        public async Task<List<Cycle>> GetCyclesAsync(int? enseignantId = null)
        {
            if (enseignantId.HasValue)
            {
                // Récupérer les cycles où l'enseignant dispense des cours
                var cyclesIds = await _context.Cours
                    .Where(c => c.EnseignantId == enseignantId.Value)
                    .SelectMany(c => _context.Seances
                        .Where(s => s.CoursId == c.Id && s.CycleId.HasValue)
                        .Select(s => s.CycleId.Value))
                    .Distinct()
                    .ToListAsync();

                return await _context.Cycles
                    .Where(c => cyclesIds.Contains(c.Id))
                    .OrderBy(c => c.NomCycle)
                    .ToListAsync();
            }
            
            // Sinon, récupérer tous les cycles
            return await _context.Cycles.OrderBy(c => c.NomCycle).ToListAsync();
        }

        // Récupérer les niveaux pour liste déroulante (modifié pour filtrer par enseignant)
        public async Task<List<Niveau>> GetNiveauxAsync(int? enseignantId = null)
        {
            if (enseignantId.HasValue)
            {
                // Récupérer les niveaux où l'enseignant dispense des cours
                var niveauxIds = await _context.Cours
                    .Where(c => c.EnseignantId == enseignantId.Value)
                    .SelectMany(c => _context.Seances
                        .Where(s => s.CoursId == c.Id && s.NiveauId.HasValue)
                        .Select(s => s.NiveauId.Value))
                    .Distinct()
                    .ToListAsync();

                return await _context.Niveaux
                    .Where(n => niveauxIds.Contains(n.Id))
                    .OrderBy(n => n.NomNiveau)
                    .ToListAsync();
            }
            
            // Sinon, récupérer tous les niveaux
            return await _context.Niveaux.OrderBy(n => n.NomNiveau).ToListAsync();
        }

        // Récupérer les spécialités pour liste déroulante (modifié pour filtrer par enseignant)
        public async Task<List<Specialite>> GetSpecialitesAsync(int? enseignantId = null)
        {
            if (enseignantId.HasValue)
            {
                // Récupérer les spécialités où l'enseignant dispense des cours
                var specialitesIds = await _context.Cours
                    .Where(c => c.EnseignantId == enseignantId.Value)
                    .SelectMany(c => _context.Seances
                        .Where(s => s.CoursId == c.Id && s.SpecialiteId.HasValue)
                        .Select(s => s.SpecialiteId.Value))
                    .Distinct()
                    .ToListAsync();

                return await _context.Specialites
                    .Where(s => specialitesIds.Contains(s.Id))
                    .OrderBy(s => s.NomSpecialite)
                    .ToListAsync();
            }
            
            // Sinon, récupérer toutes les spécialités
            return await _context.Specialites.OrderBy(s => s.NomSpecialite).ToListAsync();
        }

        // Récupérer les salles disponibles
        public Task<List<string>> GetSallesAsync()
        {
            var salles = new List<string>
            {
                "A101", "A102", "A201", "A202", "A301", "A302",
                "B101", "B102", "B201", "B202", "B301", "B302",
                "Amphi 1", "Amphi 2", "Amphi 3",
                "Labo Info 1", "Labo Info 2", "Labo Info 3",
                "Labo Physique 1", "Labo Physique 2",
                "Labo Chimie 1", "Labo Chimie 2"
            };

            return Task.FromResult(salles);
        }
    }
}