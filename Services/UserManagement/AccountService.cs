using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PGSA_Licence3.Services.UserManagement
{
    public class AccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Générer un username unique à partir du nom et prénom
        private async Task<string> GenerateUniqueUsernameAsync(string nom, string prenom)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom))
                throw new ArgumentException("Le nom et prénom sont obligatoires pour générer un username");

            string baseUsername = $"{prenom.ToLowerInvariant()}.{nom.ToLowerInvariant()}";
            string username = baseUsername;
            int counter = 1;
            
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{counter}";
                counter++;
                
                if (counter > 100)
                    throw new InvalidOperationException("Impossible de générer un nom d'utilisateur unique après 100 tentatives");
            }
            
            return username;
        }

        // Valider les données communes avant création
        private void ValidateUserData(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
                
            if (string.IsNullOrWhiteSpace(user.Nom))
                throw new ArgumentException("Le nom est obligatoire");
                
            if (string.IsNullOrWhiteSpace(user.Prenom))
                throw new ArgumentException("Le prénom est obligatoire");
                
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("L'email est obligatoire");
                
            if (!new EmailAddressAttribute().IsValid(user.Email))
                throw new ArgumentException("L'email n'est pas valide");
        }

        // Hacher le mot de passe
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        // Vérifier l'unicité d'un username
        private async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
        {
            return !await _context.Users
                .AnyAsync(u => u.Username == username && (excludeUserId == null || u.Id != excludeUserId.Value));
        }

        // S'assurer que la collection de rôles n'est pas null et la retourner
        private ICollection<Role> EnsureRolesNotNull(User user)
        {
            if (user.Roles == null)
            {
                user.Roles = new List<Role>();
            }
            return user.Roles;
        }

        // Créer un utilisateur étudiant
        public async Task<Etudiant> CreateEtudiantAsync(Etudiant etudiant, string password)
        {
            ValidateUserData(etudiant);
            
            if (etudiant.CycleId <= 0)
                throw new ArgumentException("Le cycle est obligatoire");
                
            if (etudiant.NiveauId <= 0)
                throw new ArgumentException("Le niveau est obligatoire");
                
            if (etudiant.SpecialiteId <= 0)
                throw new ArgumentException("La spécialité est obligatoire");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Le mot de passe est obligatoire");

            // Générer un username unique
            etudiant.Username = await GenerateUniqueUsernameAsync(etudiant.Nom, etudiant.Prenom);
            
            // Hacher le mot de passe
            etudiant.MotDePasseHash = HashPassword(password);
            
            // Initialiser les champs automatiques
            etudiant.CreatedAt = DateTime.UtcNow;
            etudiant.Active = true;
            
            // S'assurer que la collection de rôles existe et l'obtenir
            var roles = EnsureRolesNotNull(etudiant);
            
            // Assigner le rôle étudiant
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Étudiant");
            if (role == null) 
                throw new Exception("Le rôle Étudiant n'existe pas dans la base de données.");
            
            roles.Add(role);
            
            await _context.Etudiants.AddAsync(etudiant);
            await _context.SaveChangesAsync();
            
            return etudiant;
        }

        // Créer un utilisateur enseignant
        public async Task<Enseignant> CreateEnseignantAsync(Enseignant enseignant, string password)
        {
            ValidateUserData(enseignant);

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Le mot de passe est obligatoire");

            // Générer un username unique
            enseignant.Username = await GenerateUniqueUsernameAsync(enseignant.Nom, enseignant.Prenom);
            
            // Hacher le mot de passe
            enseignant.MotDePasseHash = HashPassword(password);
            
            // Initialiser les champs automatiques
            enseignant.CreatedAt = DateTime.UtcNow;
            enseignant.Active = true;
            
            // S'assurer que la collection de rôles existe et l'obtenir
            var roles = EnsureRolesNotNull(enseignant);
            
            // Assigner le rôle enseignant
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Enseignant");
            if (role == null) 
                throw new Exception("Le rôle Enseignant n'existe pas dans la base de données.");
            
            roles.Add(role);
            
            await _context.Enseignants.AddAsync(enseignant);
            await _context.SaveChangesAsync();
            
            return enseignant;
        }

        // Mettre à jour un utilisateur étudiant
        public async Task<Etudiant> UpdateEtudiantAsync(Etudiant etudiant)
        {
            if (etudiant == null || etudiant.Id <= 0)
                throw new ArgumentException("ID étudiant invalide");
                
            if (etudiant.CycleId <= 0)
                throw new ArgumentException("Le cycle est obligatoire");
                
            if (etudiant.NiveauId <= 0)
                throw new ArgumentException("Le niveau est obligatoire");
                
            if (etudiant.SpecialiteId <= 0)
                throw new ArgumentException("La spécialité est obligatoire");

            var existingEtudiant = await _context.Etudiants
                .Include(e => e.Roles)
                .FirstOrDefaultAsync(e => e.Id == etudiant.Id);
                
            if (existingEtudiant == null) 
                throw new Exception("Étudiant non trouvé.");
            
            // S'assurer que la collection de rôles existe
            EnsureRolesNotNull(existingEtudiant);
            
            // Vérifier l'unicité du username s'il est modifié
            if (!string.Equals(existingEtudiant.Username, etudiant.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsUsernameUniqueAsync(etudiant.Username, etudiant.Id))
                    throw new InvalidOperationException("Ce nom d'utilisateur est déjà utilisé");
            }
            
            // Mettre à jour les champs
            existingEtudiant.Nom = etudiant!.Nom;
            existingEtudiant.Prenom = etudiant!.Prenom;
            existingEtudiant.Telephone = etudiant!.Telephone;
            existingEtudiant.Email = etudiant!.Email;
            existingEtudiant.Username = etudiant!.Username;
            existingEtudiant.CycleId = etudiant!.CycleId;
            existingEtudiant.NiveauId = etudiant!.NiveauId;
            existingEtudiant.SpecialiteId = etudiant!.SpecialiteId;
            existingEtudiant.EmailInstitutionnel = etudiant!.EmailInstitutionnel;
            existingEtudiant.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existingEtudiant;
        }

        // Mettre à jour un utilisateur enseignant
        public async Task<Enseignant> UpdateEnseignantAsync(Enseignant enseignant)
        {
            if (enseignant == null || enseignant.Id <= 0)
                throw new ArgumentException("ID enseignant invalide");

            var existingEnseignant = await _context.Enseignants
                .Include(e => e.Roles)
                .FirstOrDefaultAsync(e => e.Id == enseignant.Id);
                
            if (existingEnseignant == null) 
                throw new Exception("Enseignant non trouvé.");
            
            // S'assurer que la collection de rôles existe
            EnsureRolesNotNull(existingEnseignant);
            
            // Vérifier l'unicité du username s'il est modifié
            if (!string.Equals(existingEnseignant.Username, enseignant.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsUsernameUniqueAsync(enseignant.Username, enseignant.Id))
                    throw new InvalidOperationException("Ce nom d'utilisateur est déjà utilisé");
            }
            
            // Mettre à jour les champs
            existingEnseignant.Nom = enseignant!.Nom;
            existingEnseignant.Prenom = enseignant!.Prenom;
            existingEnseignant.Telephone = enseignant!.Telephone;
            existingEnseignant.Email = enseignant!.Email;
            existingEnseignant.Username = enseignant!.Username;
            existingEnseignant.Specialite = enseignant!.Specialite;
            existingEnseignant.EmailInstitutionnel = enseignant!.EmailInstitutionnel;
            existingEnseignant.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existingEnseignant;
        }

        // Désactiver un utilisateur (suppression logique)
        public async Task DeactivateUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID utilisateur invalide");
                
            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                throw new Exception("Utilisateur non trouvé.");
            
            user.Active = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }

        // Activer un utilisateur
        public async Task ActivateUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID utilisateur invalide");
                
            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                throw new Exception("Utilisateur non trouvé.");
            
            user.Active = true;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
public async Task ResetPasswordAsync(int userId)
{
    var user = await _context.Users.FindAsync(userId);
    if (user == null)
        throw new Exception("Utilisateur non trouvé");

    const string defaultPassword = "Changeme@2026";

    user.MotDePasseHash = HashPassword(defaultPassword);
    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
}
public async Task DeleteUserAsync(int userId)
{
    var user = await _context.Users
        .Include(u => u.Roles)
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
        throw new Exception("Utilisateur non trouvé");

    // Supprimer les relations (important)
    user.Roles?.Clear();

    _context.Users.Remove(user);
    await _context.SaveChangesAsync();
}

        // Obtenir un utilisateur par son ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID utilisateur invalide");
                
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            // S'assurer que la collection de rôles existe
            if (user != null)
            {
                EnsureRolesNotNull(user);
            }
            
            return user;
        }

        // Obtenir un utilisateur par son username
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Le nom d'utilisateur est obligatoire");
                
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == username);
            
            // S'assurer que la collection de rôles existe
            if (user != null)
            {
                EnsureRolesNotNull(user);
            }
            
            return user;
        }

public async Task<List<UserViewModel>> GetAllUsersAsync()
{
    var etudiants = await _context.Etudiants
        .Include(e => e.Cycle)
        .Include(e => e.Niveau)
        .Include(e => e.Specialite)
        .Select(e => new UserViewModel 
        { 
            Id = e.Id,
            Nom = e.Nom,
            Prenom = e.Prenom,
            Email = e.Email,
            Active = e.Active,
            Type = "Étudiant",
            Cycle = e.Cycle != null ? e.Cycle.NomCycle : null,
            Niveau = e.Niveau != null ? e.Niveau.NomNiveau : null,
            Specialite = e.Specialite != null ? e.Specialite.NomSpecialite : null
        })
        .ToListAsync();
        
    var enseignants = await _context.Enseignants
        .Select(e => new UserViewModel 
        { 
            Id = e.Id,
            Nom = e.Nom,
            Prenom = e.Prenom,
            Email = e.Email,
            Active = e.Active,
            Type = "Enseignant",
            Cycle = null,
            Niveau = null,
            Specialite = e.Specialite
        })
        .ToListAsync();
        
    return etudiants.Concat(enseignants).ToList();
}


        // Méthodes pour récupérer les listes déroulantes
        public async Task<List<Cycle>> GetCyclesAsync()
        {
            return await _context.Cycles.ToListAsync();
        }

        public async Task<List<Niveau>> GetNiveauxAsync()
        {
            return await _context.Niveaux.ToListAsync();
        }

        public async Task<List<Specialite>> GetSpecialitesAsync()
        {
            return await _context.Specialites.ToListAsync();
        }

        // Vérifier les identifiants de connexion
        public async Task<User?> ValidateCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;
                
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.Active)
                return null;
                
            var hashedPassword = HashPassword(password);
            return user.MotDePasseHash == hashedPassword ? user : null;
        }
    }
}