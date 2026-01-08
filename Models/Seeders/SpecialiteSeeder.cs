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
                    new Specialite { NomSpecialite = "Conception et développement d'applications pour l'économie numérique" }, // unique specialité licence
                    new Specialite { NomSpecialite = "Informatique et Systèmes d'Information" }, // unique specialité master
                    new Specialite { NomSpecialite = "Systèmes et Réseaux de Télécommunications" }, //   specialité ingenieur
                    new Specialite { NomSpecialite = "Génie Civil" }, //   specialité ingenieur
                    new Specialite { NomSpecialite = "Génie Électrique" }, //   specialité ingenieur
                    new Specialite { NomSpecialite = "Génie Mécanique" }, //   specialité ingenieur

                };

                context.Specialites.AddRange(specialites);
                await context.SaveChangesAsync();
            }
        }
    }
}