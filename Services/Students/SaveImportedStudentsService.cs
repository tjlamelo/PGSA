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
        public async Task<List<SaveResult>> SaveWithConflictsAsync(List<Etudiant> students)
        {
            var results = new List<SaveResult>();

            foreach (var student in students)
            {
                var existing = await _db.Etudiants
                    .FirstOrDefaultAsync(s =>
                        s.Matricule == student.Matricule ||
                        s.Username == student.Username ||
                        s.Email == student.Email
                    );

                if (existing != null)
                {
                    // Conflit détecté → ne pas sauvegarder
                    results.Add(new SaveResult
                    {
                        Student = student,
                        Saved = false,
                        Problem = "Doublon détecté : matricule, username ou email existant"
                    });
                    continue;
                }

                // Pas de conflit → ajout
                await _db.Etudiants.AddAsync(student);
                results.Add(new SaveResult
                {
                    Student = student,
                    Saved = true
                });
            }

            await _db.SaveChangesAsync();
            return results;
        }
    }
}
