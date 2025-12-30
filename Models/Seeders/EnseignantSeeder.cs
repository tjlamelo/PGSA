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

                // Récupérer les cours
                var cours = await context.Cours.ToListAsync();

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

                // Spécialités correspondant aux cours
                var specialites = new Dictionary<string, List<string>>
                {
                    ["Programmation et Développement"] = new List<string> { "INP1031", "INP1052", "INP1062", "INP2123", "INP2154", "INP2164", "INP3256", "INP3286", "INP3246" },
                    ["Algorithmique et Mathématiques"] = new List<string> { "INP1041", "INP1082", "MAP1011", "MAP1021", "MAP1032", "INP1092", "MAP2043", "MAP2054", "INP2103", "MAP3065" },
                    ["Base de Données"] = new List<string> { "INP2133", "INP3195" },
                    ["Réseaux et Sécurité"] = new List<string> { "INP2174", "INP3225", "REP2013" },
                    ["Systèmes d'Information"] = new List<string> { "INP1011", "INP1021", "INP1072", "INP2113", "INP2143", "INP3205", "INP3266", "INP3276" },
                    ["Économie et Gestion"] = new List<string> { "ECP1011", "ECP2024", "ECP2034", "ECP3045", "ECP3055", "COP3025", "COP3026" },
                    ["Communication et Langues"] = new List<string> { "LAP1011", "LAP1022", "LAP2033", "LAP2034", "LAP3045", "COP1012" },
                    ["Culture Générale"] = new List<string> { "HUP1011", "HUP1022", "HUP2033", "HUP2044", "HUP3055", "HUP3066" },
                    ["Projets et Stages"] = new List<string> { "INP2184", "STP2033", "STP2025", "STP3036" }
                };

                var enseignants = new List<Enseignant>();
                var random = new Random();

    for (int i = 0; i < 15; i++)
{
    var nom = noms[random.Next(noms.Count)];
    var prenom = prenoms[random.Next(prenoms.Count)];
    var specialite = specialites.ElementAt(i % specialites.Count).Key;

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
        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Changeme@2026"),
        Roles = new List<Role> { roleEnseignant }
    };

    enseignants.Add(enseignant);
}

                // Ajouter les enseignants à la base de données
                context.Enseignants.AddRange(enseignants);
                await context.SaveChangesAsync();

                // Lier les enseignants aux cours
                enseignants = await context.Enseignants.ToListAsync();

                foreach (var specialite in specialites)
                {
                    var enseignantsDeSpecialite = enseignants
                        .Where(e => e.Specialite == specialite.Key)
                        .ToList();

                    if (!enseignantsDeSpecialite.Any()) continue;

                    foreach (var codeCours in specialite.Value)
                    {
                        var coursTrouve = cours.FirstOrDefault(c => c.Code == codeCours);
                        if (coursTrouve == null) continue;

                        var indexEnseignant = specialite.Value.IndexOf(codeCours) % enseignantsDeSpecialite.Count;
                        var enseignantChoisi = enseignantsDeSpecialite[indexEnseignant];

                        coursTrouve.EnseignantId = enseignantChoisi.Id;
                    }
                }

                var coursNonAssignes = cours.Where(c => c.EnseignantId == 0).ToList();
                foreach (var coursNonAssigné in coursNonAssignes)
                    coursNonAssigné.EnseignantId = enseignants[random.Next(enseignants.Count)].Id;

                await context.SaveChangesAsync();
            }
        }
    }
}
