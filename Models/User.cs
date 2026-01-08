using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PGSA_Licence3.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public  class User
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(20)]
        public string? Matricule { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(100)]
        [BindNever] 
        public required string MotDePasseHash { get; set; }   
 
        [Required]
        public bool Active { get; set; } = true;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public required string Email { get; set; }
      
        [MaxLength(100)]
        [EmailAddress]
        public string? EmailInstitutionnel { get; set; }
 
        [Required]
        [MaxLength(100)]
        public required string Nom { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Prenom { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Telephone { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<Role>? Roles { get; set; } = new List<Role>();
        
    
    }
}