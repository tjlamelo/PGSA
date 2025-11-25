using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class Assiduite
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
        public StatutPresence Statut { get; set; }  
        
        [Required]
        public int EnregistreurId { get; set; }  
        
        [ForeignKey("EnregistreurId")]
        public User? Enregistreur { get; set; }
        
        public StatutValidation StatutValidation { get; set; } = StatutValidation.EnAttente;   
        
        public string? Commentaire { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        
    }
    
    public enum StatutPresence
    {
        Present,
        Absent,
        Retard
    }
    
 
} 