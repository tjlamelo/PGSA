


namespace PGSA_Licence3.Services.Groupes_Management
{
    using PGSA_Licence3.Data;
    using PGSA_Licence3.Models;
    using Microsoft.EntityFrameworkCore;

    public class CreateGroupeService
    {
        private readonly ApplicationDbContext _context;

        public CreateGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

 
        /// Crée un groupe d'étudiants et le sauvegarde dans la base
        /// </summary>
        /// <param name="nom">Nom du groupe</param>
        /// <param name="niveau">Niveau du groupe (ex: L3)</param>
        /// <param name="filiere">Filière du groupe (ex: Info)</param>
        /// <param name="creationMethod">Comment créer: "manual", "random", "alphabetical"</param>
        /// <param name="selectedStudentIds">Liste des IDs des étudiants choisis</param>
        /// <returns>Le groupe créé</returns>
        public async Task<Groupe> CreerGroupeAsync(string nom, string niveau, string filiere, string creationMethod = "manual", List<int>? selectedStudentIds = null)
        {
            // Vérifier que les infos sont remplies
            if (string.IsNullOrWhiteSpace(nom))
            {
                throw new ArgumentException("Le nom du groupe ne peut pas être vide.");
            }

            if (string.IsNullOrWhiteSpace(niveau))
            {
                throw new ArgumentException("Le niveau ne peut pas être vide.");
            }

            if (string.IsNullOrWhiteSpace(filiere))
            {
                throw new ArgumentException("La filière ne peut pas être vide.");
            }

            // Créer le groupe
            var groupe = new Groupe
            {
                Nom = nom,
                Niveau = niveau,
                Filiere = filiere,
                CreatedAt = DateTime.UtcNow
            };

            // Sauvegarder dans la base
            _context.Groupes.Add(groupe);
            await _context.SaveChangesAsync();

            // Si on veut ajouter des étudiants tout de suite
            if (creationMethod != "manual" || selectedStudentIds != null)
            {
                await AffecterEtudiantsAuGroupeAsync(groupe.Id, creationMethod, selectedStudentIds);
            }

            return groupe;
        }

        // Méthode privée pour ajouter des étudiants au groupe
        private async Task AffecterEtudiantsAuGroupeAsync(int groupeId, string method, List<int>? selectedIds)
        {
            // Charger le groupe avec ses étudiants
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                .FirstOrDefaultAsync(g => g.Id == groupeId);

            if (groupe == null) return;

            // Trouver les étudiants libres
            var etudiantsDisponibles = await GetAvailableStudentsAsync();

            List<Etudiant> etudiantsAAffecter = new();

            // Selon la méthode choisie
            if (method == "manual" && selectedIds != null)
            {
                // Prendre ceux qu'on a choisis
                etudiantsAAffecter = etudiantsDisponibles.Where(e => selectedIds.Contains(e.Id)).ToList();
            }
            else if (method == "random")
            {
                // Mélanger au hasard
                var random = new Random();
                etudiantsAAffecter = etudiantsDisponibles.OrderBy(e => random.Next()).ToList();
            }
            else if (method == "alphabetical")
            {
                // Trier par nom
                etudiantsAAffecter = etudiantsDisponibles.OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToList();
            }

            // Ajouter au groupe
            foreach (var etudiant in etudiantsAAffecter)
            {
                if (groupe.Etudiants == null)
                {
                    groupe.Etudiants = new List<Etudiant>();
                }
                if (!groupe.Etudiants.Contains(etudiant))
                {
                    groupe.Etudiants.Add(etudiant);
                }
            }

            await _context.SaveChangesAsync();
        }

        // Trouver les étudiants qui ne sont dans aucun groupe
        private async Task<List<Etudiant>> GetAvailableStudentsAsync()
        {
            // Tous les étudiants
            var allStudents = await _context.Etudiants.ToListAsync();
            
            // IDs des étudiants déjà dans un groupe
            var studentsInGroups = await _context.Groupes
                .Where(g => g.Etudiants != null)
                .SelectMany(g => g.Etudiants)
                .Select(e => e.Id)
                .ToListAsync();

            // Retourner ceux qui ne sont pas dans un groupe
            return allStudents
                .Where(e => !studentsInGroups.Contains(e.Id))
                .ToList();
        }

        // Crée des groupes équilibrés par première lettre du nom
        public async Task<List<Groupe>> CreerGroupesEquilibresParLettreAsync(string niveau, string filiere, int nombreGroupes)
        {
            if (string.IsNullOrWhiteSpace(niveau) || string.IsNullOrWhiteSpace(filiere))
            {
                throw new ArgumentException("Niveau et filière requis.");
            }

            if (nombreGroupes <= 0)
            {
                throw new ArgumentException("Nombre de groupes doit être positif.");
            }

            // Étudiants disponibles
            var etudiantsDisponibles = await GetAvailableStudentsAsync();

            // Grouper par première lettre
            var groupesParLettre = etudiantsDisponibles
                .GroupBy(e => char.ToUpper(e.Nom[0]))
                .OrderBy(g => g.Key)
                .ToList();

            // Créer les groupes
            var groupesCrees = new List<Groupe>();
            for (int i = 1; i <= nombreGroupes; i++)
            {
                var groupe = new Groupe
                {
                    Nom = $"Groupe {i}",
                    Niveau = niveau,
                    Filiere = filiere,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Groupes.Add(groupe);
                groupesCrees.Add(groupe);
            }

            await _context.SaveChangesAsync();

            // Répartir équitablement
            int groupeIndex = 0;
            foreach (var groupeLettre in groupesParLettre)
            {
                foreach (var etudiant in groupeLettre.OrderBy(e => e.Nom))
                {
                    if (groupesCrees[groupeIndex].Etudiants == null)
                    {
                        groupesCrees[groupeIndex].Etudiants = new List<Etudiant>();
                    }
                    groupesCrees[groupeIndex].Etudiants.Add(etudiant);
                    groupeIndex = (groupeIndex + 1) % nombreGroupes;
                }
            }

            await _context.SaveChangesAsync();
            return groupesCrees;
        }
     }
}