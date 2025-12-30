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
                {
                    new Specialite { NomSpecialite = "Conception et développement d'applications pour l'économie numérique" },
                    new Specialite { NomSpecialite = "Informatique et Systèmes d'Information" },
                    new Specialite { NomSpecialite = "Systèmes et Réseaux de Télécommunications" },
                    new Specialite { NomSpecialite = "Génie Civil" },
                    new Specialite { NomSpecialite = "Génie Électrique" },
                    new Specialite { NomSpecialite = "Génie Mécanique" },

                };

                context.Specialites.AddRange(specialites);
                await context.SaveChangesAsync();
            }
        }
    }
}