using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Models.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // 1. Récupérer le rôle "Administrateur" (doit déjà exister via RoleSeeder)
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nom == "Administrateur");
            
            if (adminRole == null)
            {
                // On ne crée plus le rôle, on lève une exception s'il manque
                throw new Exception("Le rôle 'Administrateur' n'existe pas. Vérifiez que RoleSeeder a bien été exécuté.");
            }

            // 2. Vérifier si l'admin existe déjà
            bool adminExists = await context.Users
                .AnyAsync(u => u.Email == "admin@pgsa.edu" || u.Username == "admin");

            if (!adminExists)
            {
                // Création de l'utilisateur Admin (User n'est plus abstract)
                var admin = new User
                {
                    Matricule = "ADM-2026-001",
                    Username = "admin",
                    Nom = "Administrateur",
                    Prenom = "Système",
                    Email = "admin@pgsa.edu",
                    EmailInstitutionnel = "admin.tech@pgsa.edu",
                    Telephone = "600000000",
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    // Utilisation de votre mot de passe
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    Roles = new List<Role> { adminRole }
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}