using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models
{
 [Index(nameof(Nom), IsUnique = true)]
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Nom { get; set; }  

        [MaxLength(200)]
        public string? Description { get; set; }

       
        public ICollection<Role>? Roles { get; set; } = new List<Role>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
