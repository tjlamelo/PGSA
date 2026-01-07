using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PGSA_Licence3.Models
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}