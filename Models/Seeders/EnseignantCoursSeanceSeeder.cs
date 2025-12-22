using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;
using BCrypt.Net;

namespace PGSA_Licence3.Models.Seeders
{
    public static class EnseignantCoursSeanceSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // ===== Enseignants (50) =====
            if (!await context.Enseignants.AnyAsync())
            {
                var faker = new Faker<Enseignant>()
                    .RuleFor(e => e.Matricule, f => f.Random.Replace("ENS###"))
                    .RuleFor(e => e.Nom, f => f.Name.LastName())
                    .RuleFor(e => e.Prenom, f => f.Name.FirstName())
                    .RuleFor(e => e.Specialite, f => f.PickRandom(new[]
                        {
                            "Développement Web",
                            "Génie Logiciel",
                            "Bases de données",
                            "Intelligence Artificielle",
                            "Sécurité Informatique"
                        }))
                    .RuleFor(e => e.Telephone, f => f.Phone.PhoneNumber("699######"))
                    .RuleFor(e => e.Username, f => f.Internet.UserName())
                    .RuleFor(e => e.Email, f => f.Internet.Email())
                    .RuleFor(e => e.MotDePasseHash, f => BCrypt.Net.BCrypt.HashPassword("password123"))
                    .RuleFor(e => e.Active, true);

                var enseignants = faker.Generate(50);
                await context.Enseignants.AddRangeAsync(enseignants);
                await context.SaveChangesAsync();
            }

            // ===== Cours =====
            if (!await context.Cours.AnyAsync())
            {
                var enseignantsList = await context.Enseignants.ToListAsync();
                var coursList = new List<Cours>();
                int coursId = 1;

                string[] matieres = new[]
                {
                    "Programmation C#", "Bases de Données", "Analyse et Conception UML",
                    "Développement Web", "Architecture Logicielle", "Systèmes d'exploitation",
                    "Réseaux et Sécurité", "Intelligence Artificielle", "Gestion de Projet",
                    "Méthodologie Agile", "Mobile Development", "Cloud Computing"
                };

                for (int semestre = 1; semestre <= 6; semestre++)
                {
                    // 7 à 8 matières par semestre
                    var nbMatieres = new Random().Next(7, 9);
                    var matieresSemestre = matieres.OrderBy(x => Guid.NewGuid()).Take(nbMatieres).ToList();

                    foreach (var matiere in matieresSemestre)
                    {
                        // Assignation aléatoire d'un enseignant
                        var enseignant = enseignantsList[new Random().Next(enseignantsList.Count)];

                        coursList.Add(new Cours
                        {
                            Nom = matiere,
                            Code = $"CDEV{coursId + 100}",
                            Filiere = "Conception et Développement d'Applications pour l'Économie Numérique",
                            Semestre = $"S{semestre}",
                            AnneeAcademique = 2025,
                            EnseignantId = enseignant.Id
                        });

                        coursId++;
                    }
                }

                await context.Cours.AddRangeAsync(coursList);
                await context.SaveChangesAsync();
            }

            // ===== Séances =====
            if (!await context.Seances.AnyAsync())
            {
                var coursList = await context.Cours.ToListAsync();
                var seances = new List<Seance>();
                int seanceId = 1;

                foreach (var c in coursList)
                {
                    // 2 séances par cours
                    for (int i = 0; i < 2; i++)
                    {
                        seances.Add(new Seance
                        {
                            CoursId = c.Id,
                            Salle = $"S{seanceId:000}",
                            Type = i % 2 == 0 ? "Cours Magistral" : "TP",
                            Statut = StatutSeance.Planifiee,
                            DateHeureDebut = DateTime.UtcNow.AddDays(seanceId),
                            DateHeureFin = DateTime.UtcNow.AddDays(seanceId).AddHours(2)
                        });
                        seanceId++;
                    }
                }

                await context.Seances.AddRangeAsync(seances);
                await context.SaveChangesAsync();
            }
        }
    }
}
