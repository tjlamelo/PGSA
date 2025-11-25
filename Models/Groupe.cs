using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class Groupe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Nom { get; set; }  

        [Required]
        [MaxLength(50)]
        public required string Niveau { get; set; }  

        [Required]
        [MaxLength(50)]
        public required string Filiere { get; set; }

  
        public ICollection<Etudiant>? Etudiants { get; set; } = new List<Etudiant>();
        
        public ICollection<Seance>? Seances { get; set; } = new List<Seance>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}