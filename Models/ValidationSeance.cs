using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class ValidationSeance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SeanceId { get; set; }

        [ForeignKey("SeanceId")]
        public Seance? Seance { get; set; }

        [Required]
        public int ValidateurId { get; set; }   

        [ForeignKey("ValidateurId")]
        public User? Validateur { get; set; }

        [Required]
        public TypeValidation TypeValidation { get; set; }  
        
        [Required]
        public StatutValidation Statut { get; set; } = StatutValidation.EnAttente;

        [MaxLength(500)]
        public string? Commentaire { get; set; }

        public DateTime DateValidation { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    public enum TypeValidation
    {
        Enseignant,
        Delegue,
        Administration
    }
    
    public enum StatutValidation
    {
        EnAttente,
        Validee,
        Rejetee
    }
}