using System.ComponentModel.DataAnnotations;

namespace PGSA_Licence3.Models
{
    public class Specialite
    {
        [Key] 
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string NomSpecialite { get; set; } 
        // Exemple : ISI, SRT, Génie Civil, Conception et developpement d'applications pour l'economie numérique
    }
}
