using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Seances
{
    public class ValidationSeanceService
    {
        private readonly ApplicationDbContext _context;

        public ValidationSeanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Seance>> GetSeancesEligiblesAValidationAsync()
        {
            try
            {
                return await _context.Seances
                    .Where(s => s.Statut == StatutSeance.Terminee)
                    .Include(s => s.Cours)
                        .ThenInclude(c => c != null ? c.Enseignant : null)
                    .Include(s => s.Groupe)
                    .Include(s => s.Validations!)
                        .ThenInclude(v => v.Validateur)
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS GetSeancesEligiblesAValidationAsync: " + ex.ToString());
                throw;
            }
        }

        // Récupérer les séances éligibles à validation pour un enseignant spécifique
        public async Task<List<Seance>> GetSeancesEligiblesAValidationByEnseignantAsync(int enseignantId)
        {
            try
            {
                return await _context.Seances
                    .Where(s => s.Statut == StatutSeance.Terminee && s.Cours != null && s.Cours.EnseignantId == enseignantId)
                    .Include(s => s.Cours)
                        .ThenInclude(c => c != null ? c.Enseignant : null)
                    .Include(s => s.Groupe)
                    .Include(s => s.Validations!)
                        .ThenInclude(v => v.Validateur)
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS GetSeancesEligiblesAValidationByEnseignantAsync: " + ex.ToString());
                throw;
            }
        }

        // Récupérer les séances éligibles à validation pour un délégué spécifique
        public async Task<List<Seance>> GetSeancesEligiblesAValidationByDelegueAsync(int delegueId)
        {
            try
            {
                // Récupérer l'étudiant délégué
                var delegue = await _context.Etudiants
                    .FirstOrDefaultAsync(e => e.Id == delegueId);

                if (delegue == null) return new List<Seance>();

                // Récupérer les séances qui correspondent à la classe de l'étudiant délégué
                // MODIFICATION: Suppression de la vérification du groupe pour les délégués
                return await _context.Seances
                    .Where(s => 
                        s.Statut == StatutSeance.Terminee &&
                        (s.CycleId == delegue.CycleId) &&
                        (s.NiveauId == delegue.NiveauId) &&
                        (s.SpecialiteId == delegue.SpecialiteId)
                        // Supprimé: (s.GroupeId == null || delegue.Groupes.Any(g => g.Id == s.GroupeId))
                    )
                    .Include(s => s.Cours)
                        .ThenInclude(c => c != null ? c.Enseignant : null)
                    .Include(s => s.Groupe)
                    .Include(s => s.Validations!)
                        .ThenInclude(v => v.Validateur)
                    .OrderByDescending(s => s.DateHeureDebut)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS GetSeancesEligiblesAValidationByDelegueAsync: " + ex.ToString());
                throw;
            }
        }

        // Vérifier si un enseignant est autorisé à valider une séance spécifique
        public async Task<bool> IsEnseignantAuthorizedForSeanceAsync(int enseignantId, int seanceId)
        {
            try
            {
                var seance = await _context.Seances
                    .Include(s => s.Cours)
                    .FirstOrDefaultAsync(s => s.Id == seanceId);

                return seance?.Cours?.EnseignantId == enseignantId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS IsEnseignantAuthorizedForSeanceAsync: " + ex.ToString());
                return false;
            }
        }

        // Vérifier si un délégué est autorisé à valider une séance spécifique
        public async Task<bool> IsDelegueAuthorizedForSeanceAsync(int delegueId, int seanceId)
        {
            try
            {
                // Récupérer l'étudiant délégué
                var delegue = await _context.Etudiants
                    .FirstOrDefaultAsync(e => e.Id == delegueId);

                if (delegue == null) return false;

                // Récupérer la séance
                var seance = await _context.Seances
                    .FirstOrDefaultAsync(s => s.Id == seanceId);

                if (seance == null) return false;

                // MODIFICATION: Simplification de la vérification pour les délégués
                // Un délégué est autorisé s'il est de la même classe (cycle, niveau, spécialité)
                return seance.CycleId == delegue.CycleId &&
                       seance.NiveauId == delegue.NiveauId &&
                       seance.SpecialiteId == delegue.SpecialiteId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS IsDelegueAuthorizedForSeanceAsync: " + ex.ToString());
                return false;
            }
        }

        public async Task<ValidationSeance> CreateOrUpdateValidationAsync(int seanceId, int validateurId, TypeValidation typeValidation, StatutValidation statut, string? commentaire = null)
        {
            try
            {
                // Vérification que la séance existe
                var seance = await _context.Seances.FindAsync(seanceId);
                if (seance == null)
                {
                    throw new ArgumentException($"La séance avec l'ID {seanceId} n'existe pas.");
                }

                // Vérification que le validateur existe
                var validateur = await _context.Users.FindAsync(validateurId);
                if (validateur == null)
                {
                    throw new ArgumentException($"L'utilisateur avec l'ID {validateurId} n'existe pas.");
                }

                // Vérifier si une validation de ce type existe déjà pour cette séance
                var existingValidation = await _context.ValidationsSeance
                    .Include(v => v.Validateur)
                    .FirstOrDefaultAsync(v => v.SeanceId == seanceId && v.TypeValidation == typeValidation);

                if (existingValidation != null)
                {
                    // Mettre à jour la validation existante
                    existingValidation.Statut = statut;
                    existingValidation.Commentaire = commentaire;
                    existingValidation.DateValidation = DateTime.UtcNow;
                    existingValidation.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    return existingValidation;
                }
                else
                {
                    // Créer une nouvelle validation
                    var nouvelleValidation = new ValidationSeance
                    {
                        SeanceId = seanceId,
                        ValidateurId = validateurId,
                        TypeValidation = typeValidation,
                        Statut = statut,
                        Commentaire = commentaire,
                        DateValidation = DateTime.UtcNow
                    };
                    
                    _context.ValidationsSeance.Add(nouvelleValidation);
                    await _context.SaveChangesAsync();
                    
                    // Recharger avec les données du validateur
                    return await _context.ValidationsSeance
                        .Include(v => v.Validateur)
                        .FirstAsync(v => v.Id == nouvelleValidation.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERREUR DANS CreateOrUpdateValidationAsync: " + ex.ToString());
                throw;
            }
        }
    }
}