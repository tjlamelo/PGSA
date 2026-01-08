using PGSA_Licence3.Data;
using PGSA_Licence3.Models.Seeders;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // S'assure que la base de données existe
            await context.Database.EnsureCreatedAsync();

            // 1. Référentiels de base
            await CycleSeeder.SeedAsync(context);
            await NiveauSeeder.SeedAsync(context);
            await SpecialiteSeeder.SeedAsync(context);

            // 2. Sécurité (Rôles et Permissions)
            if (!await context.Roles.AnyAsync())
                await RoleSeeder.SeedRolesAsync(context);

            if (!await context.Permissions.AnyAsync())
                await PermissionSeeder.SeedAsync(context);

            // 3. Utilisateurs Système (Admin direct car User n'est plus abstract)
            // On le place ici pour qu'il soit créé avant les données académiques
            await AdminSeeder.SeedAsync(context);

            // 4. Acteurs académiques
            if (!await context.Etudiants.AnyAsync())
                await EtudiantSeeder.SeedAsync(context);

            if (!await context.Enseignants.AnyAsync())
                await EnseignantSeeder.SeedAsync(context);

            // 5. Organisation et Contenu
            if (!await context.Groupes.AnyAsync())
                await GroupeSeeder.SeedAsync(context);

            if (!await context.Cours.AnyAsync())
                await CoursSeeder.SeedAsync(context);
        }
    }
}