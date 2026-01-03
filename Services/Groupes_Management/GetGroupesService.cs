using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Groupes_Management
{
    public class GetGroupesService
    {
        private readonly ApplicationDbContext _context;

        public GetGroupesService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Récupère tous les groupes avec leurs étudiants
        public async Task<List<Groupe>> GetAllGroupesAsync()
        {
            return await _context.Groupes
                .Include(g => g.Etudiants)
                .OrderBy(g => g.Nom)
                .ToListAsync();
        }

        // Récupère un résumé de tous les groupes
        public async Task<List<GroupeSummary>> GetGroupesSummaryAsync()
        {
            return await _context.Groupes
                .Select(g => new GroupeSummary
                {
                    Id = g.Id,
                    Nom = g.Nom,
                    Niveau = g.Niveau,
                    Filiere = g.Filiere,
                    NombreEtudiants = g.Etudiants != null ? g.Etudiants.Count : 0
                })
                .OrderBy(g => g.Nom)
                .ToListAsync();
        }

        // Récupère les groupes d'une classe (niveau + filière)
        public async Task<List<GroupeSummary>> GetGroupesParClasseAsync(string niveau, string filiere)
        {
            return await _context.Groupes
                .Where(g => g.Niveau == niveau && g.Filiere == filiere)
                .Select(g => new GroupeSummary
                {
                    Id = g.Id,
                    Nom = g.Nom,
                    Niveau = g.Niveau,
                    Filiere = g.Filiere,
                    NombreEtudiants = g.Etudiants != null ? g.Etudiants.Count : 0
                })
                .OrderBy(g => g.Nom)
                .ToListAsync();
        }
    }

    // Classe pour résumer un groupe
    public class GroupeSummary
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Niveau { get; set; }
        public string? Filiere { get; set; }
        public int NombreEtudiants { get; set; }
    }
}