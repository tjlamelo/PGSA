using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace PGSA_Licence3.Models.Seeders
{
    public static class EnseignantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Enseignants.AnyAsync())
            {
                // Récupérer le rôle Enseignant
                var roleEnseignant = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Enseignant");
                if (roleEnseignant == null)
                    throw new System.Exception("Le rôle 'Enseignant' n'a pas été trouvé dans la base de données.");

                // Récupérer les spécialités
                var specialites = await context.Specialites.ToListAsync();

                // Listes de noms et prénoms camerounais
                var noms = new List<string>
                {
                    "Mbakop", "Tchoumi", "Fotso", "Ngo", "Tchuen", "Moussié", "Kamga", "Tchamda",
                    "Fokou", "Nkoulou", "Tchinda", "Mbianda", "Nguemo", "Fotie", "Kengne", "Tchakounte",
                    "Noumsi", "Mefo", "Tchinda", "Fotso", "Ngo", "Mballa", "Tchuenté", "Kamdem",
                    "Fouda", "Nkotto", "Tchamda", "Mvondo", "Ngueguim", "Kuete", "Fotso", "Ndonko"
                };

                var prenoms = new List<string>
                {
                    "Jean-Pierre", "Marie-Claire", "Alain", "Patrice", "Josiane", "Étienne", "Cécile",
                    "Gérard", "Thérèse", "Serge", "Marie-Thérèse", "Pierre", "Henriette", "Claude",
                    "Annie", "Michel", "Sophie", "André", "Yvonne", "Roger", "Jacqueline", "Daniel",
                    "Simone", "Paul", "Chantal", "Philippe", "Monique", "Louis", "Nathalie", "Joseph"
                };

                var enseignants = new List<Enseignant>();
                var random = new Random();

                // Créer 15 enseignants avec des spécialités variées
                for (int i = 0; i < 15; i++)
                {
                    var nom = noms[random.Next(noms.Count)];
                    var prenom = prenoms[random.Next(prenoms.Count)];
                    
                    // Assigner une spécialité de manière équilibrée
                    var specialite = specialites[i % specialites.Count].NomSpecialite;

                    var matricule = $"ENS{DateTime.Now.Year}{(i + 1):D3}";

                    // Ajouter l'index i pour garantir unicité
                    var username = $"{prenom.Substring(0, Math.Min(2, prenom.Length)).ToLower()}.{nom.ToLower()}{i + 1}";
                    var emailInstitutionnel = $"{prenom.Replace("-", ".").ToLower()}.{nom.ToLower()}{i + 1}@pgsa.edu";
                    var email = $"{username}@email.com";
                    var telephone = $"6{random.Next(10000000, 99999999)}";

                    var enseignant = new Enseignant
                    {
                        Matricule = matricule,
                        Username = username,
                        Email = email,
                        EmailInstitutionnel = emailInstitutionnel,
                        Nom = nom,
                        Prenom = prenom,
                        Telephone = telephone,
                        Specialite = specialite,
                        DateEmbauche = DateTime.Now.AddMonths(-random.Next(1, 60)),
                        Active = true,
                        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                        Roles = new List<Role> { roleEnseignant }
                    };

                    enseignants.Add(enseignant);
                }

                // Ajouter les enseignants à la base de données
                context.Enseignants.AddRange(enseignants);
                await context.SaveChangesAsync();
            }
        }
    }
}