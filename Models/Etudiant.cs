using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Models
{
    public class Etudiant : User
    {
   


        [Required]
        [MaxLength(100)]
        public required string Nom { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Prenom { get; set; }

        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(20)]
        public string? Matricule { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Niveau { get; set; }  

        [Required]
        [MaxLength(50)]
        public required string Filiere { get; set; } 

        [MaxLength(100)]
        public string? EmailInstitutionnel { get; set; }  

        public DateTime DateInscription { get; set; } = DateTime.UtcNow;

 
    }
}
