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

            // 🔹 Seed Cycles
            await CycleSeeder.SeedAsync(context);

            // 🔹 Seed Niveaux
            await NiveauSeeder.SeedAsync(context);

            // 🔹 Seed Specialites
            await SpecialiteSeeder.SeedAsync(context);

            // 🔹 Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                await RoleSeeder.SeedRolesAsync(context);
            }

            // 🔹 Seed Enseignants, Cours et Séances
            if (!await context.Enseignants.AnyAsync())
            {
                await EnseignantCoursSeanceSeeder.SeedAsync(context);
            }
        }

    }
}
