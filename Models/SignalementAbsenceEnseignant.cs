using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class SignalementAbsenceEnseignant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeanceId { get; set; }   

        [ForeignKey("SeanceId")]
        public Seance? Seance { get; set; }

        [Required]
        public int DelegueId { get; set; }   

        [ForeignKey("DelegueId")]
        public User? Delegue { get; set; }

        [Required]
        public DateTime DateSignalement { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Motif { get; set; }
        
        [Required]
        public StatutSignalement Statut { get; set; } = StatutSignalement.EnAttente;  
        
        public int? TraitantId { get; set; }  
        
        [ForeignKey("TraitantId")]
        public User? Traitant { get; set; }
        
        public DateTime? DateTraitement { get; set; }   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum StatutSignalement
    {
        EnAttente,
        Traite,
        Rejete
    }
}