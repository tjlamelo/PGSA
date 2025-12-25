using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services
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
                .Include(s => s.Groupe)
                .Include(s => s.Cycle)
                .Include(s => s.Niveau)
                .Include(s => s.Specialite)
                .ToListAsync();
        }

        // Récupérer une séance par Id
        public async Task<Seance?> GetByIdAsync(int id)
        {
            return await _context.Seances
                .Include(s => s.Cours)
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
            if (await HasConflictAsync(seance))
                throw new Exception("Conflit détecté : la séance chevauche une autre séance existante.");

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

                existing.CoursId = seance.CoursId;
                existing.GroupeId = seance.GroupeId;
                existing.CycleId = seance.CycleId;
                existing.NiveauId = seance.NiveauId;
                existing.SpecialiteId = seance.SpecialiteId;
                existing.DateHeureDebut = seance.DateHeureDebut;
                existing.DateHeureFin = seance.DateHeureFin;
                existing.Salle = seance.Salle;
                existing.Type = seance.Type;
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

        // Vérifier les conflits
        private async Task<bool> HasConflictAsync(Seance seance)
        {
            return await _context.Seances.AnyAsync(s =>
                s.Id != seance.Id && // ignorer la séance elle-même
                s.Salle == seance.Salle && // même salle
                (
                    (seance.DateHeureDebut < s.DateHeureFin && s.DateHeureDebut < seance.DateHeureFin) // chevauchement
                )
            );
        }

        // Récupérer les cours pour liste déroulante
        public async Task<List<Cours>> GetCoursAsync()
        {
            return await _context.Cours.OrderBy(c => c.Nom).ToListAsync();
        }

        // Récupérer les groupes pour liste déroulante
        public async Task<List<Groupe>> GetGroupesAsync()
        {
            return await _context.Groupes.OrderBy(g => g.Nom).ToListAsync();
        }

        // Récupérer les cycles pour liste déroulante
        public async Task<List<Cycle>> GetCyclesAsync()
        {
            return await _context.Cycles.OrderBy(c => c.NomCycle).ToListAsync();
        }

        // Récupérer les niveaux pour liste déroulante
        public async Task<List<Niveau>> GetNiveauxAsync()
        {
            return await _context.Niveaux.OrderBy(n => n.NomNiveau).ToListAsync();
        }

        // Récupérer les spécialités pour liste déroulante
        public async Task<List<Specialite>> GetSpecialitesAsync()
        {
            return await _context.Specialites.OrderBy(s => s.NomSpecialite).ToListAsync();
        }
    }
}