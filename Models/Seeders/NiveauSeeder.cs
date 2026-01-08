using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class NiveauSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Niveaux.AnyAsync())
            {
                var niveaux = new List<Niveau>
                {
                    new Niveau { NomNiveau = "1" },
                    new Niveau { NomNiveau = "2" }, //max Master
                    new Niveau { NomNiveau = "3" }, // max Licence
                    new Niveau { NomNiveau = "4" },
                    new Niveau { NomNiveau = "5" } // max ingenieur
                };

                context.Niveaux.AddRange(niveaux);
                await context.SaveChangesAsync();
            }
        }
    }
}
