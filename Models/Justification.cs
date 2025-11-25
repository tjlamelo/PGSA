using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class Justification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EtudiantId { get; set; }  

        [ForeignKey("EtudiantId")]
        public Etudiant? Etudiant { get; set; }  

        [Required]
        public int SeanceId { get; set; }  

        [ForeignKey("SeanceId")]
        public Seance? Seance { get; set; }     

        [Required]
        [MaxLength(500)]
        public required string Motif { get; set; }    
        
        [Required]
        public string? CheminDocument { get; set; }   
        
        [Required]
        public StatutJustification Statut { get; set; } = StatutJustification.EnAttente;  
        
        public int? ValidateurId { get; set; }  
        
        [ForeignKey("ValidateurId")]
        public User? Validateur { get; set; }
        
        public string? CommentaireValidation { get; set; }  
        
        public DateTime DateSoumission { get; set; } = DateTime.UtcNow;
        
        public DateTime? DateValidation { get; set; }   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum StatutJustification
    {
        EnAttente,
        Validee,
        Rejetee
    }
}