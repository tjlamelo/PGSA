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
                    "Tchaptche", "Mekoulou", "Nkengfack", "Tchamda", "Moukouri", "Nguemegne", "Fokoua", "Tchinda"
                };

                var prenoms = new List<string> 
                { 
                    "Jean-Pierre", "Marie-Claire", "Alain", "Patrice", "Josiane", "Étienne", "Cécile", 
                    "Gérard", "Thérèse", "Serge", "Marie-Thérèse", "Pierre", "Henriette", "Claude",
                    "Annie", "Michel", "Sophie", "André", "Yvonne", "Roger", "Jacqueline", "Daniel",
                    "Simone", "Paul", "Chantal", "Philippe", "Monique", "Louis", "Nathalie", "Joseph",
                    "René", "Françoise", "Bernard", "Christine", "Georges", "Madeleine", "Robert", "Paulette"
                };

        for (int i = 0; i < 60; i++)
{
    var nom = noms[random.Next(noms.Count)];
    var prenom = prenoms[random.Next(prenoms.Count)];

    var matricule = $"ETU{DateTime.Now.Year}{(i + 1):D3}";

    // On ajoute l'index i pour garantir unicité
    var username = $"{prenom.Substring(0, Math.Min(2, prenom.Length)).ToLower()}.{nom.ToLower()}{i + 1}";
    var emailInstitutionnel = $"{prenom.Replace("-", ".").ToLower()}.{nom.ToLower()}{i + 1}@pgsa.edu";
    var email = $"{username}@email.com";
    var telephone = $"6{random.Next(10000000, 99999999)}";

    var cycle = cycles[random.Next(cycles.Count)];

    Niveau niveau;
    if (cycle.NomCycle == "Licence")
        niveau = niveaux[random.Next(3)];
    else if (cycle.NomCycle == "Master")
        niveau = niveaux[random.Next(2)];
    else
        niveau = niveaux[random.Next(5)];

    var specialite = specialites[random.Next(specialites.Count)];
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
        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Changeme@2026"),
        Roles = new List<Role> { roleEtudiant }
    };

    // 10% des étudiants seront également délégués
    if (random.Next(100) < 10)
        etudiant.Roles.Add(roleDelegue);

    etudiants.Add(etudiant);
}

                context.Etudiants.AddRange(etudiants);
                await context.SaveChangesAsync();
            }
        }
    }
}
