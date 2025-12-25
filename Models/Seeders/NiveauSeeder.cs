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
                    new Niveau { NomNiveau = "2" },
                    new Niveau { NomNiveau = "3" },
                    new Niveau { NomNiveau = "4" },
                    new Niveau { NomNiveau = "5" }
                };

                context.Niveaux.AddRange(niveaux);
                await context.SaveChangesAsync();
            }
        }
    }
}
