using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Models
{
    public class Enseignant : User
    {
        [MaxLength(100)]
        public string? Specialite { get; set; }

        public DateTime DateEmbauche { get; set; } = DateTime.UtcNow;         public ICollection<Cours>? Cours { get; set; } = new List<Cours>();

    }
}
