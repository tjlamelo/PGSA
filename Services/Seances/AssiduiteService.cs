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
                .ThenInclude(c => c!.Enseignant)
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

            // Initialisation de la requête de base
            var query = _context.Etudiants
                .Include(e => e.Cycle)
                .Include(e => e.Niveau)
                .Include(e => e.Specialite)
                .AsQueryable();

            if (seance.GroupeId.HasValue)
            {
                // On extrait l'ID dans une variable simple pour aider le traducteur SQL
                int targetGroupeId = seance.GroupeId.Value;

                // Correction de la traduction LINQ :
                // On filtre les étudiants dont la collection de groupes contient l'ID recherché
                query = query.Where(e => e.Groupes!.Any(g => g.Id == targetGroupeId));
            }
            else
            {
                // Filtrage par Cycle/Niveau/Spécialité
                query = query.Where(e => 
                    (seance.CycleId == null || e.CycleId == seance.CycleId) &&
                    (seance.NiveauId == null || e.NiveauId == seance.NiveauId) &&
                    (seance.SpecialiteId == null || e.SpecialiteId == seance.SpecialiteId));
            }

            return await query.ToListAsync();
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

        // Vérifier si un utilisateur est autorisé à accéder à une séance
        public async Task<bool> IsUserAuthorizedForSeanceAsync(int userId, int seanceId)
        {
            var seance = await GetSeanceByIdAsync(seanceId);
            if (seance == null) return false;

            // Vérifier si l'utilisateur est un enseignant de cette séance
            if (seance.Cours?.EnseignantId == userId)
            {
                return true;
            }

            // Vérifier si l'utilisateur est un étudiant délégué de la classe concernée
            var delegue = await _context.Etudiants
                .Include(e => e.Groupes)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (delegue != null)
            {
                // Vérifier si l'étudiant a le rôle de délégué
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.Roles.Any(r => r.Nom == "Délégué") == true)
                {
                    // Vérifier si le délégué appartient à la même classe que la séance
                    bool belongsToSameClass = 
                        (seance.CycleId == null || delegue.CycleId == seance.CycleId) &&
                        (seance.NiveauId == null || delegue.NiveauId == seance.NiveauId) &&
                        (seance.SpecialiteId == null || delegue.SpecialiteId == seance.SpecialiteId);

                    // Si la séance a un groupe spécifique, vérifier si le délégué en fait partie
                    if (seance.GroupeId.HasValue)
                    {
                        belongsToSameClass = belongsToSameClass && 
                            delegue.Groupes.Any(g => g.Id == seance.GroupeId.Value);
                    }

                    return belongsToSameClass;
                }
            }

            return false;
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
                    assiduiteExistante.Commentaire = commentaireEtudiant;
                    assiduiteExistante.UpdatedAt = DateTime.Now;
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
                        Commentaire = commentaireEtudiant,
                        StatutValidation = StatutValidation.EnAttente
                    };

                    _context.Assiduites.Add(nouvelleAssiduite);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}