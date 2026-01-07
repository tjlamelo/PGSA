using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Role_Managment
{
    public class PermissionSeedService
    {
        private readonly ApplicationDbContext _context;

        public PermissionSeedService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedPermissionsAsync()
        {
            var permissions = new List<(string Nom, string Description)>
            {
                ("Gestion_Utilisateurs", "Gérer les utilisateurs (créer, modifier, supprimer)"),
                ("Gestion_Groupes", "Gérer les groupes d'étudiants"),
                ("Gestion_Etudiants", "Gérer les étudiants"),
                ("Gestion_Cours", "Gérer les cours"),
                ("Gestion_Seances", "Gérer les séances"),
                ("Prise_Appel", "Prendre l'appel des étudiants"),
                ("Validation_Seance", "Valider les séances"),
                ("Gestion_Roles", "Gérer les rôles et permissions"),
                ("Gestion_Permissions", "Gérer les permissions"),
                ("Voir_Rapports", "Consulter les rapports et statistiques"),
                ("Exporter_Donnees", "Exporter les données"),
                ("Gestion_Cahier_Texte", "Gérer le cahier de texte")
            };

            foreach (var (nom, description) in permissions)
            {
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Nom == nom);

                if (existingPermission == null)
                {
                    var permission = new Permission
                    {
                        Nom = nom,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Permissions.Add(permission);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}

