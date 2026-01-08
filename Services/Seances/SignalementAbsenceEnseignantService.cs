using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Seances
{
    public class SignalementAbsenceEnseignantService
    {
        private readonly ApplicationDbContext _context;

        public SignalementAbsenceEnseignantService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère tous les signalements avec leurs données associées (séance, délégué, traitant).
        /// </summary>
        public async Task<List<SignalementAbsenceEnseignant>> GetAllAsync()
        {
            return await _context.SignalementsAbsenceEnseignant
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Cours!) // Inclut le cours de la séance
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Groupe) // Inclut le groupe de la séance
                .Include(s => s.Delegue) // Inclut l'utilisateur qui a signalé
                .Include(s => s.Traitant) // Inclut l'utilisateur qui a traité
                .OrderByDescending(s => s.DateSignalement) // Trie par date de signalement la plus récente
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les signalements créés par un délégué spécifique.
        /// </summary>
        public async Task<List<SignalementAbsenceEnseignant>> GetByDelegueAsync(int delegueId)
        {
            return await _context.SignalementsAbsenceEnseignant
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Cours!) // Inclut le cours de la séance
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Groupe) // Inclut le groupe de la séance
                .Include(s => s.Delegue) // Inclut l'utilisateur qui a signalé
                .Include(s => s.Traitant) // Inclut l'utilisateur qui a traité
                .Where(s => s.DelegueId == delegueId)
                .OrderByDescending(s => s.DateSignalement) // Trie par date de signalement la plus récente
                .ToListAsync();
        }

        public async Task CreateAsync(SignalementAbsenceEnseignant signalement, int delegueId)
        {
            // Assigne l'ID du délégué connecté
            signalement.DelegueId = delegueId;

            // La date est déjà gérée par la valeur par défaut du modèle, mais on peut la redéfinir ici
            signalement.DateSignalement = DateTime.UtcNow;

            _context.SignalementsAbsenceEnseignant.Add(signalement);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Récupère un signalement par son ID avec toutes ses données associées.
        /// </summary>
        public async Task<SignalementAbsenceEnseignant?> GetByIdAsync(int id)
        {
            return await _context.SignalementsAbsenceEnseignant
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Cours!)
                .Include(s => s.Delegue)
                .Include(s => s.Traitant)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Met à jour le statut d'un signalement, en indiquant qui l'a traité et quand.
        /// Utilise une méthode de mise à jour explicite pour éviter les problèmes de tracking d'EF Core.
        /// </summary>
        /// <param name="id">L'ID du signalement à mettre à jour.</param>
        /// <param name="nouveauStatut">Le nouveau statut (Traité ou Rejeté).</param>
        /// <param name="traitantId">L'ID de l'utilisateur (admin/staff) qui traite le signalement.</param>
        public async Task UpdateStatusAsync(int id, StatutSignalement nouveauStatut, int traitantId)
        {
            // 1. Vérifier que l'utilisateur qui traite existe (très important)
            var traitant = await _context.Users.FindAsync(traitantId);
            if (traitant == null)
            {
                throw new Exception("L'utilisateur en charge du traitement n'a pas été trouvé.");
            }

            // 2. Créer une entité "stub" avec seulement l'ID et l'attacher au contexte
            var signalementToUpdate = new SignalementAbsenceEnseignant { Id = id };
            _context.SignalementsAbsenceEnseignant.Attach(signalementToUpdate);

            // 3. Mettre à jour les propriétés nécessaires
            signalementToUpdate.Statut = nouveauStatut;
            signalementToUpdate.TraitantId = traitantId;
            signalementToUpdate.DateTraitement = DateTime.UtcNow;
            signalementToUpdate.UpdatedAt = DateTime.UtcNow;

            // 4. Marquer explicitement chaque propriété comme modifiée
            _context.Entry(signalementToUpdate).Property(s => s.Statut).IsModified = true;
            _context.Entry(signalementToUpdate).Property(s => s.TraitantId).IsModified = true;
            _context.Entry(signalementToUpdate).Property(s => s.DateTraitement).IsModified = true;
            _context.Entry(signalementToUpdate).Property(s => s.UpdatedAt).IsModified = true;

            // 5. Sauvegarder les changements
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Vérifie si un utilisateur est un administrateur.
        /// </summary>
        public async Task<bool> IsAdministrateurAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            return user?.Roles.Any(r => r.Nom == "Administrateur") ?? false;
        }

        /// <summary>
        /// Vérifie si un délégué est autorisé à signaler une séance spécifique.
        /// </summary>
      public async Task<bool> IsDelegueAuthorizedForSeanceAsync(int delegueId, int seanceId)
{
    var delegue = await _context.Etudiants
        .Include(e => e.Groupes)
        .FirstOrDefaultAsync(e => e.Id == delegueId);

    if (delegue == null) return false;

    var seance = await _context.Seances.FirstOrDefaultAsync(s => s.Id == seanceId);
    if (seance == null) return false;

    // Vérification de la classe (Cycle, Niveau, Spécialité)
    bool belongsToSameClass = 
        (seance.CycleId == null || delegue.CycleId == seance.CycleId) &&
        (seance.NiveauId == null || delegue.NiveauId == seance.NiveauId) &&
        (seance.SpecialiteId == null || delegue.SpecialiteId == seance.SpecialiteId);

    // FIX: Extraction des IDs pour éviter l'erreur de traduction LINQ
    if (seance.GroupeId.HasValue)
    {
        var idsGroupesDelegue = delegue.Groupes.Select(g => g.Id).ToList();
        belongsToSameClass = belongsToSameClass && idsGroupesDelegue.Contains(seance.GroupeId.Value);
    }

    return belongsToSameClass;
}
        /// <summary>
        /// Récupère la liste de tous les étudiants (délégués potentiels).
        /// </summary>
        public async Task<List<Etudiant>> GetDeleguesAsync()
        {
            return await _context.Users.OfType<Etudiant>().Where(e => e.Active).ToListAsync();
        }

        /// <summary>
        /// Récupère la liste de toutes les séances pour les listes déroulantes (filtrées selon le rôle de l'utilisateur).
        /// </summary>
 public async Task<List<Seance>> GetSeancesAsync(int userId, bool isAdministrateur, bool isDelegue)
{
    if (isAdministrateur)
    {
        return await _context.Seances
            .Include(s => s.Cours).Include(s => s.Groupe)
            .Include(s => s.Cycle).Include(s => s.Niveau).Include(s => s.Specialite)
            .OrderByDescending(s => s.DateHeureDebut).ToListAsync();
    }
    
    if (isDelegue)
    {
        var delegue = await _context.Etudiants
            .Include(e => e.Groupes)
            .FirstOrDefaultAsync(e => e.Id == userId);

        if (delegue == null) return new List<Seance>();

        // FIX: On récupère les IDs des groupes AVANT la requête principale
        var idsGroupesDelegue = delegue.Groupes.Select(g => g.Id).ToList();

        return await _context.Seances
            .Include(s => s.Cours).Include(s => s.Groupe)
            .Include(s => s.Cycle).Include(s => s.Niveau).Include(s => s.Specialite)
            .Where(s => 
                (s.CycleId == delegue.CycleId) &&
                (s.NiveauId == delegue.NiveauId) &&
                (s.SpecialiteId == delegue.SpecialiteId) &&
                // Utilisation de Contains sur une liste simple (traduit en SQL IN)
                (s.GroupeId == null || idsGroupesDelegue.Contains(s.GroupeId.Value))
            )
            .OrderByDescending(s => s.DateHeureDebut)
            .ToListAsync();
    }

    return new List<Seance>();
}
    }
}