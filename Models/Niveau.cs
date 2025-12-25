using System.ComponentModel.DataAnnotations;

namespace PGSA_Licence3.Models
{
    public class Niveau
    {
        [Key] 
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public required string NomNiveau { get; set; } 
        // Exemple : 1, 2, 3, 4, 5
    }
}
