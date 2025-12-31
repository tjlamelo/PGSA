namespace PGSA_Licence3.Utils
{
    /// <summary>
    /// Helper centralisé pour le hachage et la vérification des mots de passe avec BCrypt.Net-Next.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hache un mot de passe en clair.
        /// </summary>
        public static string HashPassword(string password)
        {
            // Utilisation de l'espace de noms complet pour éviter les conflits
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Vérifie si un mot de passe en clair correspond à un mot de passe haché.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
