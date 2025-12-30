using PGSA_Licence3.Data;
using PGSA_Licence3.Models.Seeders;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            await CycleSeeder.SeedAsync(context);
            await NiveauSeeder.SeedAsync(context);
            await SpecialiteSeeder.SeedAsync(context);

            if (!await context.Roles.AnyAsync())
                await RoleSeeder.SeedRolesAsync(context);

            if (!await context.Permissions.AnyAsync())
                await PermissionSeeder.SeedAsync(context);

            if (!await context.Etudiants.AnyAsync())
                await EtudiantSeeder.SeedAsync(context); // plus de passwordHasher

            if (!await context.Enseignants.AnyAsync())
                await EnseignantSeeder.SeedAsync(context); // plus de passwordHasher

            if (!await context.Groupes.AnyAsync())
                await GroupeSeeder.SeedAsync(context);

            if (!await context.Cours.AnyAsync())
                await CoursSeeder.SeedAsync(context);
        }
    }
}
