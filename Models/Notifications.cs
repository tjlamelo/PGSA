using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DestinataireId { get; set; }  

        [ForeignKey("DestinataireId")]
        public User? Destinataire { get; set; }  

        [Required]
        [MaxLength(100)]
        public required string Titre { get; set; }  

        [Required]
        [MaxLength(1000)]
        public required string Contenu { get; set; }  

        [Required]
        public TypeNotification Type { get; set; }   

        [Required]
        public StatutNotification Statut { get; set; } = StatutNotification.NonLue;   

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;  

        public DateTime? DateLecture { get; set; }  

        [MaxLength(500)]
        public string? LienAction { get; set; }   

        public int? EntiteAssocieeId { get; set; }   

        [MaxLength(50)]
        public string? TypeEntiteAssociee { get; set; }   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum TypeNotification
    {
        SignalementAbsenceEnseignant,  // Notification pour signalement d'absence d'enseignant
        JustificatifSoumis,            // Notification pour dépôt de justificatif
        JustificatifValide,            // Notification pour validation de justificatif
        JustificatifRejete,            // Notification pour rejet de justificatif
        SeanceAValider,                // Rappel pour valider une séance
        SeanceValidee,                 // Confirmation de validation de séance
        RappelPresence,                // Rappel pour prendre les présences
        Systeme                        // Notifications générales du système
    }

    public enum StatutNotification
    {
        NonLue,
        Lue,
        Archivee
    }
}