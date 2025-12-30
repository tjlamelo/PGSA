using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class Etudiant : User
    {
        [Required]
        public int CycleId { get; set; }

        [ForeignKey(nameof(CycleId))]
        public Cycle? Cycle { get; set; }

        [Required]
        public int NiveauId { get; set; }

        [ForeignKey(nameof(NiveauId))]
        public Niveau? Niveau { get; set; }

        [Required]
        public int SpecialiteId { get; set; }

        [ForeignKey(nameof(SpecialiteId))]
        public Specialite? Specialite { get; set; }

 
        public DateTime DateInscription { get; set; } = DateTime.UtcNow;
  
        public ICollection<Groupe>? Groupes { get; set; } = new List<Groupe>();
    }
}
