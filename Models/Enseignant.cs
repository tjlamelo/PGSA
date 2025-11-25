using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Models
{
    public class Enseignant : User
    {

        [Required]
        [MaxLength(20)]
        public required string Matricule { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Nom { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Prenom { get; set; }

        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(100)]
        public string? Specialite { get; set; }


        public DateTime DateEmbauche { get; set; } = DateTime.UtcNow;
    }
}
