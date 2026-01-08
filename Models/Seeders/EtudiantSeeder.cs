using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace PGSA_Licence3.Models.Seeders
{
    public static class EtudiantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Etudiants.AnyAsync())
            {
                // Récupérer les cycles, niveaux, spécialités et rôles
                var cycles = await context.Cycles.ToListAsync();
                var niveaux = await context.Niveaux.ToListAsync();
                var specialites = await context.Specialites.ToListAsync();
                var roleEtudiant = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Étudiant");
                var roleDelegue = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Délégué");

                if (roleEtudiant == null || roleDelegue == null)
                    throw new System.Exception("Les rôles 'Étudiant' ou 'Délégué' n'ont pas été trouvés. Assurez-vous que RoleSeeder a été exécuté.");

                var groupes = await context.Groupes.ToListAsync();
                var etudiants = new List<Etudiant>();
                var random = new Random();

                var noms = new List<string> 
                { 
                    "Mbakop", "Tchoumi", "Fotso", "Ngo", "Tchuen", "Moussié", "Kamga", "Tchamda", 
                    "Fokou", "Nkoulou", "Tchinda", "Mbianda", "Nguemo", "Fotie", "Kengne", "Tchakounte",
                    "Noumsi", "Mefo", "Tchinda", "Fotso", "Ngo", "Mballa", "Tchuenté", "Kamdem",
                    "Fouda", "Nkotto", "Tchamda", "Mvondo", "Ngueguim", "Kuete", "Fotso", "Ndonko",
                    "Tchaptche", "Mekoulou", "Nkengfack", "Tchamda", "Moukouri", "Nguemegne", "Fokoua", "Tchinda",
                    "Mballa", "Ngo", "Tchoumi", "Fotso", "Moussié", "Kamga", "Tchamda", "Fokou", "Nkoulou",
                    "Tchinda", "Mbianda", "Nguemo", "Fotie", "Kengne", "Tchakounte", "Noumsi", "Mefo"
                };

                var prenoms = new List<string> 
                { 
                    "Jean-Pierre", "Marie-Claire", "Alain", "Patrice", "Josiane", "Étienne", "Cécile", 
                    "Gérard", "Thérèse", "Serge", "Marie-Thérèse", "Pierre", "Henriette", "Claude",
                    "Annie", "Michel", "Sophie", "André", "Yvonne", "Roger", "Jacqueline", "Daniel",
                    "Simone", "Paul", "Chantal", "Philippe", "Monique", "Louis", "Nathalie", "Joseph",
                    "René", "Françoise", "Bernard", "Christine", "Georges", "Madeleine", "Robert", "Paulette",
                    "Antoine", "Isabelle", "Franck", "Martine", "Nicolas", "Brigitte", "Éric", "Sylvie"
                };

                // Définir les combinaisons de cycle/niveau/spécialité
                var classes = new List<(string Cycle, string Niveau, string Specialite)>
                {
                    // Licence (3 niveaux)
                    ("Licence", "1", "Conception et développement d'applications pour l'économie numérique"),
                    ("Licence", "2", "Conception et développement d'applications pour l'économie numérique"),
                    ("Licence", "3", "Conception et développement d'applications pour l'économie numérique"),
                    
                    // Master (2 niveaux)
                    ("Master", "1", "Informatique et Systèmes d'Information"),
                    ("Master", "2", "Informatique et Systèmes d'Information"),
                    
                    // Ingénieur - Systèmes et Réseaux de Télécommunications (5 niveaux)
                    ("Ingénieur", "1", "Systèmes et Réseaux de Télécommunications"),
                    ("Ingénieur", "2", "Systèmes et Réseaux de Télécommunications"),
                    ("Ingénieur", "3", "Systèmes et Réseaux de Télécommunications"),
                    ("Ingénieur", "4", "Systèmes et Réseaux de Télécommunications"),
                    ("Ingénieur", "5", "Systèmes et Réseaux de Télécommunications"),
                    
                    // Ingénieur - Génie Civil (5 niveaux)
                    ("Ingénieur", "1", "Génie Civil"),
                    ("Ingénieur", "2", "Génie Civil"),
                    ("Ingénieur", "3", "Génie Civil"),
                    ("Ingénieur", "4", "Génie Civil"),
                    ("Ingénieur", "5", "Génie Civil"),
                    
                    // Ingénieur - Génie Électrique (5 niveaux)
                    ("Ingénieur", "1", "Génie Électrique"),
                    ("Ingénieur", "2", "Génie Électrique"),
                    ("Ingénieur", "3", "Génie Électrique"),
                    ("Ingénieur", "4", "Génie Électrique"),
                    ("Ingénieur", "5", "Génie Électrique"),
                    
                    // Ingénieur - Génie Mécanique (5 niveaux)
                    ("Ingénieur", "1", "Génie Mécanique"),
                    ("Ingénieur", "2", "Génie Mécanique"),
                    ("Ingénieur", "3", "Génie Mécanique"),
                    ("Ingénieur", "4", "Génie Mécanique"),
                    ("Ingénieur", "5", "Génie Mécanique")
                };

                int etudiantId = 1; // Pour générer des IDs uniques

                foreach (var classe in classes)
                {
                    // Récupérer les IDs correspondants
                    var cycle = cycles.First(c => c.NomCycle == classe.Cycle);
                    var niveau = niveaux.First(n => n.NomNiveau == classe.Niveau);
                    var specialite = specialites.First(s => s.NomSpecialite == classe.Specialite);

                    // Créer 20 étudiants pour cette classe
                    for (int i = 0; i < 20; i++)
                    {
                        var nom = noms[random.Next(noms.Count)];
                        var prenom = prenoms[random.Next(prenoms.Count)];

                        var matricule = $"ETU{DateTime.Now.Year}{etudiantId:D3}";

                        // On ajoute l'ID pour garantir unicité
                        var username = $"{prenom.Substring(0, Math.Min(2, prenom.Length)).ToLower()}.{nom.ToLower()}{etudiantId}";
                        var emailInstitutionnel = $"{prenom.Replace("-", ".").ToLower()}.{nom.ToLower()}{etudiantId}@pgsa.edu";
                        var email = $"{username}@email.com";
                        var telephone = $"6{random.Next(10000000, 99999999)}";

                        var dateInscription = DateTime.Now.AddMonths(-random.Next(1, 36));

                        var etudiant = new Etudiant
                        {
                            Matricule = matricule,
                            Username = username,
                            Email = email,
                            EmailInstitutionnel = emailInstitutionnel,
                            Nom = nom,
                            Prenom = prenom,
                            Telephone = telephone,
                            CycleId = cycle.Id,
                            NiveauId = niveau.Id,
                            SpecialiteId = specialite.Id,
                            DateInscription = dateInscription,
                            Active = true,
                            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                            Roles = new List<Role> { roleEtudiant }
                        };

                        etudiants.Add(etudiant);
                        etudiantId++;
                    }

                    // Ajouter 2 délégués pour cette classe
                   // Ajouter 2 délégués pour cette classe
for (int i = 0; i < 2; i++)
{
    var nom = noms[random.Next(noms.Count)];
    var prenom = prenoms[random.Next(prenoms.Count)];

    var matricule = $"DEL{DateTime.Now.Year}{etudiantId:D3}";

    // Nouveau username pour le délégué
    // Remplacer les espaces et caractères spéciaux par des underscores ou rien
    string cycleClean = classe.Cycle.Replace(" ", "").ToLower();
    string niveauClean = classe.Niveau.Replace(" ", "").ToLower();
    string specialiteClean = classe.Specialite.Replace(" ", "").Replace("é","e").Replace("è","e").Replace("ê","e").Replace("à","a").Replace("ç","c").ToLower();
    
    var username = $"delegue{cycleClean}{niveauClean}{specialiteClean}";

    var emailInstitutionnel = $"{prenom.Replace("-", ".").ToLower()}.{nom.ToLower()}{etudiantId}@pgsa.edu";
    var email = $"{username}@email.com";
    var telephone = $"6{random.Next(10000000, 99999999)}";

    var dateInscription = DateTime.Now.AddMonths(-random.Next(1, 36));

    var delegue = new Etudiant
    {
        Matricule = matricule,
        Username = username,
        Email = email,
        EmailInstitutionnel = emailInstitutionnel,
        Nom = nom,
        Prenom = prenom,
        Telephone = telephone,
        CycleId = cycle.Id,
        NiveauId = niveau.Id,
        SpecialiteId = specialite.Id,
        DateInscription = dateInscription,
        Active = true,
        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
        Roles = new List<Role> { roleEtudiant, roleDelegue }
    };

    etudiants.Add(delegue);
    etudiantId++;
}

                }

                context.Etudiants.AddRange(etudiants);
                await context.SaveChangesAsync();
            }
        }
    }
}