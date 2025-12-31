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
                    .Include(s => s.Validations)
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