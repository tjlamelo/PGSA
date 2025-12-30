using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PGSA_Licence3.Services.Seances
{
    public class AssiduiteService
    {
        private readonly ApplicationDbContext _context;

        public AssiduiteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Seance?> GetSeanceByIdAsync(int seanceId)
        {
            return await _context.Seances
                .Include(s => s.Cours)
                .Include(s => s.Groupe)
                .Include(s => s.Cycle)
                .Include(s => s.Niveau)
                .Include(s => s.Specialite)
                .FirstOrDefaultAsync(s => s.Id == seanceId);
        }

        public async Task<List<Etudiant>> GetEtudiantsForSeanceAsync(int seanceId)
        {
            var seance = await GetSeanceByIdAsync(seanceId);
            if (seance == null) return new List<Etudiant>();

            List<Etudiant> etudiants = new List<Etudiant>();

            if (seance.GroupeId != null)
            {
                // Si la séance est pour un groupe spécifique
                etudiants = await _context.Etudiants
                    .Where(e => e.Groupes != null && e.Groupes.Any(g => g.Id == seance.GroupeId))
                    .Include(e => e.Cycle)
                    .Include(e => e.Niveau)
                    .Include(e => e.Specialite)
                    .ToListAsync();
            }
            else
            {
                // Si la séance est pour un cycle/niveau/spécialité
                etudiants = await _context.Etudiants
                    .Where(e => 
                        (seance.CycleId == null || e.CycleId == seance.CycleId) &&
                        (seance.NiveauId == null || e.NiveauId == seance.NiveauId) &&
                        (seance.SpecialiteId == null || e.SpecialiteId == seance.SpecialiteId))
                    .Include(e => e.Cycle)
                    .Include(e => e.Niveau)
                    .Include(e => e.Specialite)
                    .ToListAsync();
            }

            return etudiants;
        }

        public async Task<List<Assiduite>> GetBySeanceIdAsync(int seanceId)
        {
            return await _context.Assiduites
                .Include(a => a.Etudiant)
                .Where(a => a.SeanceId == seanceId)
                .ToListAsync();
        }

        public async Task<bool> AppelDejaFaitAsync(int seanceId)
        {
            return await _context.Assiduites.AnyAsync(a => a.SeanceId == seanceId);
        }

        public async Task SaveAppelAsync(int seanceId, Dictionary<int, StatutPresence> presences, Dictionary<int, double> heuresEffectuees, Dictionary<int, string> commentaires, int enregistreurId)
        {
            var seance = await GetSeanceByIdAsync(seanceId);
            if (seance == null) throw new Exception("Séance non trouvée");

            var etudiants = await GetEtudiantsForSeanceAsync(seanceId);
            var assiduitesExistantes = await GetBySeanceIdAsync(seanceId);

            foreach (var etudiant in etudiants)
            {
                // Vérifier si une assiduité existe déjà pour cet étudiant et cette séance
                var assiduiteExistante = assiduitesExistantes.FirstOrDefault(a => a.EtudiantId == etudiant.Id);

                // Récupérer le statut de présence pour cet étudiant
                if (!presences.TryGetValue(etudiant.Id, out var statutPresence))
                {
                    statutPresence = StatutPresence.Absent; // Valeur par défaut si non spécifié
                }

                // Récupérer le nombre d'heures effectué pour cet étudiant
                double heures = 0;
                if (!heuresEffectuees.TryGetValue(etudiant.Id, out heures))
                {
                    // Si pas de valeur spécifiée, utiliser la logique par défaut
                    heures = statutPresence == StatutPresence.Present ? (double)seance.Duree : 0;
                }

                // Récupérer le commentaire spécifique à cet étudiant
                string? commentaireEtudiant = null;
                commentaires?.TryGetValue(etudiant.Id, out commentaireEtudiant);

                if (assiduiteExistante != null)
                {
                    // Mettre à jour l'assiduité existante
                    assiduiteExistante.Statut = statutPresence;
                    assiduiteExistante.HeuresEffectuees = heures;
                    assiduiteExistante.Commentaire = commentaireEtudiant; // Commentaire spécifique à l'étudiant
                    assiduiteExistante.UpdatedAt = DateTime.Now;
                    
                    // Le statut de validation reste "EnAttente" sauf si c'est le professeur qui enregistre
                    // (logique à implémenter selon vos besoins)
                }
                else
                {
                    // Créer une nouvelle assiduité
                    var nouvelleAssiduite = new Assiduite
                    {
                        EtudiantId = etudiant.Id,
                        SeanceId = seanceId,
                        Statut = statutPresence,
                        HeuresEffectuees = heures,
                        EnregistreurId = enregistreurId,
                        Commentaire = commentaireEtudiant, // Commentaire spécifique à l'étudiant
                        StatutValidation = StatutValidation.EnAttente
                    };

                    _context.Assiduites.Add(nouvelleAssiduite);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}