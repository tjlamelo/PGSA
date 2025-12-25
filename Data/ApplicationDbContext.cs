using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entités principales
        public DbSet<User> Users { get; set; }
        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Enseignant> Enseignants { get; set; }
        public DbSet<Groupe> Groupes { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<Seance> Seances { get; set; }

        // Entités fonctionnelles
        public DbSet<Assiduite> Assiduites { get; set; }
        public DbSet<CahierDeTexte> CahiersDeTexte { get; set; }
        public DbSet<Justification> Justifications { get; set; }
        public DbSet<ValidationSeance> ValidationsSeance { get; set; }
        public DbSet<SignalementAbsenceEnseignant> SignalementsAbsenceEnseignant { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Entités de sécurité
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        // Ajoutez ces lignes à votre classe ApplicationDbContext
        public DbSet<Cycle> Cycles { get; set; }
        public DbSet<Niveau> Niveaux { get; set; }
        public DbSet<Specialite> Specialites { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'héritage pour User
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Etudiant>("Etudiant")
                .HasValue<Enseignant>("Enseignant");

            // Configuration des relations many-to-many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j => j.ToTable("UserRoles"));

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j => j.ToTable("RolePermissions"));

            // Configuration des relations un-à-plusieurs
            modelBuilder.Entity<Groupe>()
                .HasMany(g => g.Etudiants)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Groupe>()
                .HasMany(g => g.Seances)
                .WithOne(s => s.Groupe)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cours>()
                .HasMany(c => c.Seances)
                .WithOne(s => s.Cours)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seance>()
                .HasMany(s => s.Assiduites)
                .WithOne(a => a.Seance)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seance>()
                .HasOne(s => s.CahierDeTexte)
                .WithOne(ct => ct.Seance)
                .HasForeignKey<CahierDeTexte>(ct => ct.SeanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seance>()
                .HasMany(s => s.Validations)
                .WithOne(v => v.Seance)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration des contraintes supplémentaires
            modelBuilder.Entity<Assiduite>()
                .HasIndex(a => new { a.EtudiantId, a.SeanceId })
                .IsUnique()
                .HasDatabaseName("IX_Assiduite_EtudiantId_SeanceId");

            modelBuilder.Entity<Justification>()
                .HasIndex(j => new { j.EtudiantId, j.SeanceId })
                .IsUnique()
                .HasDatabaseName("IX_Justification_EtudiantId_SeanceId");

            modelBuilder.Entity<ValidationSeance>()
                .HasIndex(v => new { v.SeanceId, v.ValidateurId, v.TypeValidation })
                .IsUnique()
                .HasDatabaseName("IX_ValidationSeance_SeanceId_ValidateurId_TypeValidation");

            modelBuilder.Entity<SignalementAbsenceEnseignant>()
                .HasIndex(s => s.SeanceId)
                .IsUnique()
                .HasDatabaseName("IX_SignalementAbsenceEnseignant_SeanceId");
        }
    }
}