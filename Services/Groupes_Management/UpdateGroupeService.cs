


namespace PGSA_Licence3.Services.Groupes_Management
{
    using PGSA_Licence3.Data;
    using PGSA_Licence3.Models;
    using Microsoft.EntityFrameworkCore;

    public class UpdateGroupeService
    {
        private readonly ApplicationDbContext _context;

        public UpdateGroupeService(ApplicationDbContext context)
        {
            _context = context;
        }

 
        /// Change les infos d'un groupe
        /// </summary>
        /// <param name="id">Numéro du groupe</param>
        /// <param name="nom">Nouveau nom</param>
        /// <param name="niveau">Nouveau niveau</param>
        /// <param name="filiere">Nouvelle filière</param>
        /// <returns>Le groupe modifié</returns>
        public async Task<Groupe> ModifierGroupeAsync(int id, string nom, string niveau, string filiere)
        {
            // Vérifier les infos
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

            // Trouver le groupe
            var groupe = await _context.Groupes.FindAsync(id);
            if (groupe == null)
            {
                throw new ArgumentException("Groupe non trouvé.");
            }

            // Changer les infos
            groupe.Nom = nom;
            groupe.Niveau = niveau;
            groupe.Filiere = filiere;
            groupe.UpdatedAt = DateTime.UtcNow;

            // Sauvegarder
            await _context.SaveChangesAsync();
            return groupe;
        }
     }
}