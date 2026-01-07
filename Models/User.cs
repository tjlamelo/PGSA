
 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models
{
    [Index(nameof(Username), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public required string MotDePasseHash { get; set; }   
        
        [Required]
        public bool Active { get; set; } = true;

        [Required]
        [MaxLength(150)]
        public required string Email { get; set; }
        
 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
         
        public ICollection<Role>? Roles { get; set; } = new List<Role>();
    }
}