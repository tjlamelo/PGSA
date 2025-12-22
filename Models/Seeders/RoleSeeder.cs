using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(ApplicationDbContext context)
        {
            // Les rôles corrigés
            var roles = new List<Role>
            {
                new Role { Id = 1, Nom = "Enseignant", Description = "Rôle pour les enseignants" },
                new Role { Id = 2, Nom = "Étudiant", Description = "Rôle pour les étudiants" },
                new Role { Id = 3, Nom = "Délégué", Description = "Rôle pour les délégués de classe" },
                new Role { Id = 4, Nom = "Administrateur", Description = "Rôle pour l'administration" }
            };

            foreach (var role in roles)
            {
                // Vérifie si le rôle existe déjà
                if (!await context.Roles.AnyAsync(r => r.Nom == role.Nom))
                {
                    await context.Roles.AddAsync(role);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
