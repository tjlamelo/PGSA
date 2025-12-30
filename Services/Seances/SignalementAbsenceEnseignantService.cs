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
        public async Task<SignalementAbsenceEnseignant?> GetByIdAsync(int id) // <-- LIGNE CORRIGÉE
        {
            return await _context.SignalementsAbsenceEnseignant
                .Include(s => s.Seance)
                    .ThenInclude(se => se!.Cours!)
                .Include(s => s.Delegue)
                .Include(s => s.Traitant)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

       // Dans SignalementAbsenceEnseignantService.cs

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
        /// Récupère la liste de tous les étudiants (délégués potentiels).
        /// </summary>
        public async Task<List<Etudiant>> GetDeleguesAsync()
        {
            return await _context.Users.OfType<Etudiant>().Where(e => e.Active).ToListAsync();
        }

        /// <summary>
        /// Récupère la liste de toutes les séances pour les listes déroulantes.
        /// </summary>
        public async Task<List<Seance>> GetSeancesAsync()
        {
            return await _context.Seances
                .Include(s => s!.Cours!)
                .Include(s => s!.Groupe)
                .OrderByDescending(s => s.DateHeureDebut)
                .ToListAsync();
        }
    }
}