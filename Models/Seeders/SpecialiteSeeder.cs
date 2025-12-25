using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class SpecialiteSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Specialites.AnyAsync())
            {
                var specialites = new List<Specialite>
                {      new Specialite { NomSpecialite = "Conception et développement d'applications pour l'économie numérique" },
                    new Specialite { NomSpecialite = "ISI" },
                    new Specialite { NomSpecialite = "SRT" },
                    new Specialite { NomSpecialite = "Génie Civil" },
               };

                context.Specialites.AddRange(specialites);
                await context.SaveChangesAsync();
            }
        }
    }
}
