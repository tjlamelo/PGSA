using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class CahierDeTexte
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeanceId { get; set; }  

        [ForeignKey("SeanceId")]
        public Seance? Seance { get; set; }

        [Required]
        [MaxLength(500)]
        public required string ObjectifsPedagogiques { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public required string ActivitesRealisees { get; set; }
        
        [MaxLength(500)]
        public string? RessourcesUtilisees { get; set; }
        
        [MaxLength(500)]
        public string? TravauxAFaire { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}