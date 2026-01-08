using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Services.Students
{
    public class SaveImportedStudentsService
    {
        private readonly ApplicationDbContext _db;

        public SaveImportedStudentsService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Type interne pour résultat détaillé
        public class SaveResult
        {
            public Etudiant Student { get; set; } = null!; // obligatoire
            public bool Saved { get; set; } = false;
            public string? Problem { get; set; } // message si conflit ou erreur
        }

        /// <summary>
        /// Enregistre les étudiants avec détection des conflits
        /// Import irréversible, aucun écrasement
        /// </summary>
   // Dans SaveImportedStudentsService.cs

public async Task<List<SaveResult>> SaveWithConflictsAsync(List<Etudiant> students)
{
    var results = new List<SaveResult>();
    var existingStudentsInDb = await _db.Etudiants
        .Select(s => new { s.Matricule, s.Username, s.Email })
        .ToListAsync();

    // Utiliser un HashSet pour une recherche rapide des doublons dans le lot actuel
  var seenInBatch = new HashSet<(string? Matricule, string? Username, string? Email)>();
    foreach (var student in students)
    {
        var studentKey = (student.Matricule, student.Username, student.Email);
        
        // 1. Vérifier les doublons dans le lot d'importation actuel
        if (!seenInBatch.Add(studentKey))
        {
            results.Add(new SaveResult
            {
                Student = student,
                Saved = false,
                Problem = "Doublon détecté dans le fichier d'importation : matricule, username ou email en double."
            });
            continue;
        }

        // 2. Vérifier les doublons dans la base de données
        if (existingStudentsInDb.Any(es => 
                string.Equals(es.Matricule, student.Matricule, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(es.Username, student.Username, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(es.Email, student.Email, StringComparison.OrdinalIgnoreCase)))
        {
            results.Add(new SaveResult
            {
                Student = student,
                Saved = false,
                Problem = "Doublon détecté : un étudiant avec ce matricule, username ou email existe déjà en base de données."
            });
            continue;
        }

        // 3. Pas de conflit, préparer à la sauvegarde
        // S'assurer que les rôles sont correctement attachés et non déjà suivis par le contexte
        if (student.Roles != null && student.Roles.Any())
        {
            var roleIds = student.Roles.Select(r => r.Id).ToList();
            var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            student.Roles.Clear();
            foreach(var role in roles)
            {
                student.Roles.Add(role);
            }
        }
        
        // S'assurer que l'ID est 0 pour que la base de données le génère
        student.Id = 0;
        
        await _db.Etudiants.AddAsync(student);
        results.Add(new SaveResult
        {
            Student = student,
            Saved = true
        });
    }

    try
    {
        await _db.SaveChangesAsync();
    }
    catch (DbUpdateException dbEx)
    {
        // Si une erreur se produit quand même, on la loggue et on met à jour les résultats
        // TODO: Utilisez un vrai logger
        Console.WriteLine($"Erreur lors de SaveChangesAsync: {dbEx.InnerException?.Message ?? dbEx.Message}");
        // Marquer tous les étudiants "Saved = true" comme ayant échoué
        foreach (var result in results.Where(r => r.Saved))
        {
            result.Saved = false;
            result.Problem = $"Erreur lors de la sauvegarde en base de données : {dbEx.InnerException?.Message ?? dbEx.Message}";
        }
    }

    return results;
}
    }
}