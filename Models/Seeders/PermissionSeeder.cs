using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    // Permissions pour les enseignants
                    new Permission { Nom = "Gérer les cours", Description = "Créer, modifier et supprimer des cours" },
                    new Permission { Nom = "Gérer les séances", Description = "Créer, modifier et supprimer des séances" },
                    new Permission { Nom = "Prendre les présences", Description = "Enregistrer les présences des étudiants" },
                    new Permission { Nom = "Rédiger le cahier de texte", Description = "Rédiger et modifier le cahier de texte" },
                    
                    // Permissions pour les étudiants
                    new Permission { Nom = "Consulter les cours", Description = "Voir les informations des cours" },
                    new Permission { Nom = "Consulter les présences", Description = "Voir son historique de présence" },
                    new Permission { Nom = "Soumettre un justificatif", Description = "Déposer un justificatif d'absence" },
                    
                    // Permissions pour les délégués
                    new Permission { Nom = "Signaler une absence d'enseignant", Description = "Signaler l'absence d'un enseignant" },
                    new Permission { Nom = "Valider les séances", Description = "Valider les séances de cours" },
                    
                    // Permissions pour les administrateurs
                    new Permission { Nom = "Gérer les utilisateurs", Description = "Créer, modifier et supprimer des utilisateurs" },
                    new Permission { Nom = "Gérer les rôles", Description = "Assigner des rôles aux utilisateurs" },
                    new Permission { Nom = "Gérer les spécialités", Description = "Créer, modifier et supprimer des spécialités" },
                    new Permission { Nom = "Valider les justificatifs", Description = "Approuver ou rejeter les justificatifs" }
                };

                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();

                // Associer les permissions aux rôles
                var roleEnseignant = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Enseignant");
                var roleEtudiant = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Étudiant");
                var roleDelegue = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Délégué");
                var roleAdmin = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Administrateur");

                if (roleEnseignant != null)
                {
                    roleEnseignant.Permissions = permissions.Where(p => 
                        p.Nom.Contains("Gérer les cours") || 
                        p.Nom.Contains("Gérer les séances") || 
                        p.Nom.Contains("Prendre les présences") || 
                        p.Nom.Contains("Rédiger le cahier de texte")).ToList();
                }

                if (roleEtudiant != null)
                {
                    roleEtudiant.Permissions = permissions.Where(p => 
                        p.Nom.Contains("Consulter les cours") || 
                        p.Nom.Contains("Consulter les présences") || 
                        p.Nom.Contains("Soumettre un justificatif")).ToList();
                }

                if (roleDelegue != null)
                {
                    roleDelegue.Permissions = permissions.Where(p => 
                        p.Nom.Contains("Consulter les cours") || 
                        p.Nom.Contains("Consulter les présences") || 
                        p.Nom.Contains("Soumettre un justificatif") ||
                        p.Nom.Contains("Signaler une absence d'enseignant") || 
                        p.Nom.Contains("Valider les séances")).ToList();
                }

                if (roleAdmin != null)
                {
                    roleAdmin.Permissions = permissions.ToList();
                }

                await context.SaveChangesAsync();
            }
        }
    }
}