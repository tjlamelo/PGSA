using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
  public class Seance
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CoursId { get; set; }
    [ForeignKey("CoursId")]
    public Cours? Cours { get; set; }

    public int? GroupeId { get; set; }
    [ForeignKey("GroupeId")]
    public Groupe? Groupe { get; set; }

    // 🔹 Nouveau nom : Cycle (Licence / Master / Ingenieur)
    public int? CycleId { get; set; }
    [ForeignKey("CycleId")]
    public Cycle? Cycle { get; set; }

    // 🔹 Nouveau nom : Niveau (1, 2, 3, 4, 5)
    public int? NiveauId { get; set; }
    [ForeignKey("NiveauId")]
    public Niveau? Niveau { get; set; }

    // 🔹 Nouveau nom : Specialite (ISI / SRT / GC / etc.)
    public int? SpecialiteId { get; set; }
    [ForeignKey("SpecialiteId")]
    public Specialite? Specialite { get; set; }

    [Required]
    public DateTime DateHeureDebut { get; set; }

    [Required]
    public DateTime DateHeureFin { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Salle { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Type { get; set; }

    [Required]
    public StatutSeance Statut { get; set; } = StatutSeance.Planifiee;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Assiduite>? Assiduites { get; set; } = new List<Assiduite>();
    public CahierDeTexte? CahierDeTexte { get; set; }
    public ICollection<ValidationSeance>? Validations { get; set; } = new List<ValidationSeance>();
}


    public enum StatutSeance
    {
        Planifiee,
        EnCours,
        Terminee,
        Annulee
    }
}