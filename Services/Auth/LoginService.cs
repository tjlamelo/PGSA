using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Utils; // Ajout du using pour notre helper

namespace PGSA_Licence3.Services.Auth
{
    public class LoginService
    {
        private readonly ApplicationDbContext _db;

        public LoginService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Trouve un utilisateur par son email, en incluant ses rôles.
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.Roles) // Important pour charger les rôles de l'utilisateur
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Valide les identifiants d'un utilisateur (email et mot de passe).
        /// </summary>
        /// <param name="email">L'email de l'utilisateur.</param>
        /// <param name="password">Le mot de passe en clair.</param>
        /// <returns>L'objet User si les identifiants sont valides, sinon null.</returns>
        public async Task<User?> ValidateUserCredentialsAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await GetUserByEmailAsync(email);

            // 1. Vérifier si l'utilisateur existe et est actif.
            if (user == null || !user.Active)
            {
                return null;
            }

            // 2. Vérifier si le mot de passe correspond en utilisant BCrypt.
            if (!PasswordHelper.VerifyPassword(password, user.MotDePasseHash))
            {
                return null;
            }

            // Si tout est bon, retourner l'utilisateur
            return user;
        }
    }
}