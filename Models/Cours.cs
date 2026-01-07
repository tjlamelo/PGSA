using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PGSA_Licence3.Models
{
    public class Cours
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Nom { get; set; }  

        [Required]
        [MaxLength(20)]
        public required string Code { get; set; }  

        [Required]
        [MaxLength(50)]
        public required string Filiere { get; set; }
        
        [Required]
        [MaxLength(20)]
        public required string Niveau { get; set; }    
        
        [Required]
        [MaxLength(20)]
        public required string Semestre { get; set; }   
        
        [Required]
        public int AnneeAcademique { get; set; }   

        [Required]
        public int EnseignantId { get; set; }      

        [ForeignKey("EnseignantId")]
        public Enseignant? Enseignant { get; set; } 
        
 
        public ICollection<Seance>? Seances { get; set; } = new List<Seance>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}