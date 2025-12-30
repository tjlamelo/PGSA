using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class GroupeSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Phase 1 : Création des groupes s'ils n'existent pas
            if (!await context.Groupes.AnyAsync())
            {
                // Récupérer les niveaux et spécialités depuis la base de données
                var niveaux = await context.Niveaux.ToListAsync();
                var specialites = await context.Specialites.ToListAsync();

                var groupesToCreate = new List<Groupe>();

                // Créer des groupes pour chaque combinaison de niveau et de spécialité
                // Par exemple, pour chaque niveau, on crée un groupe pour chaque spécialité
                foreach (var niveau in niveaux)
                {
                    foreach (var specialite in specialites)
                    {
                        // On crée 2 groupes par combinaison pour simuler des classes de taille plus raisonnable
                        for (int i = 1; i <= 2; i++)
                        {
                            // Créer un nom de groupe clair (ex: G1-DEV-A, G1-DEV-B)
                            var specialiteAbbreviation = specialite.NomSpecialite.Length > 20
                                ? specialite.NomSpecialite.Substring(0, 20).Replace(" ", "")
                                : specialite.NomSpecialite.Replace(" ", "");

                            var nomGroupe = $"G{niveau.NomNiveau}-{specialiteAbbreviation}-{(char)('A' + i - 1)}";

                            groupesToCreate.Add(new Groupe
                            {
                                Nom = nomGroupe,
                                Niveau = niveau.NomNiveau, // On stocke le nom du niveau pour faciliter la recherche
                                Filiere = specialite.NomSpecialite // On stocke le nom de la filière pour faciliter la recherche
                            });
                        }
                    }
                }

                context.Groupes.AddRange(groupesToCreate);
                await context.SaveChangesAsync(); // Sauvegarder pour obtenir les IDs des groupes
            }

            // Phase 2 : Répartir les étudiants dans les groupes appropriés
            // On s'assure qu'il y a des étudiants à répartir
            if (await context.Etudiants.AnyAsync())
            {
                // Récupérer tous les étudiants et tous les groupes
                var etudiants = await context.Etudiants
                    .Include(e => e.Niveau) // Inclure les entités liées pour accéder à leurs propriétés
                    .Include(e => e.Specialite)
                    .ToListAsync();

                var groupes = await context.Groupes.ToListAsync();

                var random = new Random();

                foreach (var etudiant in etudiants)
                {
                    // Vérifier que l'étudiant a bien un niveau et une spécialité associés
                    if (etudiant.Niveau == null || etudiant.Specialite == null)
                    {
                        // On pourrait logger un avertissement ici, mais on continue
                        continue;
                    }

                    // Trouver tous les groupes qui correspondent au niveau et à la spécialité de l'étudiant
                    var groupesCorrespondants = groupes
                        .Where(g => g.Niveau == etudiant.Niveau.NomNiveau && g.Filiere == etudiant.Specialite.NomSpecialite)
                        .ToList();

                    // Si des groupes correspondants sont trouvés, en choisir un au hasard et y ajouter l'étudiant
                    if (groupesCorrespondants.Any())
                    {
                        var groupeChoisi = groupesCorrespondants[random.Next(groupesCorrespondants.Count)];

                        groupeChoisi.Etudiants ??= new List<Etudiant>();
                        groupeChoisi.Etudiants.Add(etudiant);
                    }

                }

                // Sauvegarder toutes les affectations étudiant-groupe
                await context.SaveChangesAsync();
            }
        }
    }
}