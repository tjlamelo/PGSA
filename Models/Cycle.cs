using System.ComponentModel.DataAnnotations;

namespace PGSA_Licence3.Models
{
    public class Cycle
    {
        [Key] 
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public required string  NomCycle { get; set; } // Licence, Master, Ingénieur
    }
}
