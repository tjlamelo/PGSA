using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class CycleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Cycles.AnyAsync())
            {
                var cycles = new List<Cycle>
                {
                    new Cycle { NomCycle = "Licence" },
                    new Cycle { NomCycle = "Master" },
                    new Cycle { NomCycle = "Ingénieur" }
                };

                context.Cycles.AddRange(cycles);
                await context.SaveChangesAsync();
            }
        }
    }
}
