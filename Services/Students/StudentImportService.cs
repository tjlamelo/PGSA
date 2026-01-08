using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using System.Text;

namespace PGSA_Licence3.Services.Students
{
    public class StudentImportService
    {
        private readonly ApplicationDbContext _context;

        public StudentImportService(ApplicationDbContext context)
        {
            _context = context;
            ExcelPackage.License.SetNonCommercialPersonal("TJBeats");
        }

        /// Normalise un texte (minuscules, supprime accents)
        private string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.ToLower().Trim();
            return string.Concat(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
                .Replace(" ", "-");
        }

        /// Génère matricule automatique si vide
        private string GenerateMatricule(DateTime inscription, string niveau, string cycle)
        {
            string yearStart = inscription.Year.ToString().Substring(2);
            string yearEnd = (inscription.Year + 1).ToString().Substring(2);

            string niveauLetter = !string.IsNullOrWhiteSpace(niveau) ? niveau[0].ToString().ToUpper() : "X";
            string cycleLetter = !string.IsNullOrWhiteSpace(cycle) ? cycle[0].ToString().ToUpper() : "X";

            string random = new Random().Next(1000, 9999).ToString();

            return $"{yearStart}{yearEnd}{niveauLetter}{cycleLetter}{random}";
        }

        /// Génère un template Excel pour l'importation des étudiants
        public Stream GenerateTemplateExcel()
        {
            var stream = new MemoryStream();
            
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Étudiants");
                
                // Définir les en-têtes
                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "Nom";
                worksheet.Cells[1, 3].Value = "Prénom";
                worksheet.Cells[1, 4].Value = "Téléphone";
                worksheet.Cells[1, 5].Value = "Date d'inscription (JJ/MM/AAAA)";
                
                // Exemple de données
                worksheet.Cells[2, 1].Value = "jean.dupont@example.com";
                worksheet.Cells[2, 2].Value = "Dupont";
                worksheet.Cells[2, 3].Value = "Jean";
                worksheet.Cells[2, 4].Value = "0123456789";
                worksheet.Cells[2, 5].Value = DateTime.Now.ToString("dd/MM/yyyy");
                
                // Mettre en forme les en-têtes
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
                
                package.Save();
            }
            
            stream.Position = 0;
            return stream;
        }

        /// Obtient la liste des cycles depuis la BD
        public async Task<List<Cycle>> GetCyclesAsync()
        {
            return await _context.Cycles.ToListAsync();
        }

        /// Obtient la liste des niveaux depuis la BD
        public async Task<List<Niveau>> GetNiveauxAsync()
        {
            return await _context.Niveaux.ToListAsync();
        }

        /// Obtient la liste des spécialités depuis la BD
        public async Task<List<Specialite>> GetSpecialitesAsync()
        {
            return await _context.Specialites.ToListAsync();
        }

        /// Lit Excel et ignore les lignes vides
        public List<Dictionary<string, string>> ImportExcel(Stream fileStream)
        {
            var result = new List<Dictionary<string, string>>();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];
            int rowCount = ws.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var email = ws.Cells[row, 1].Text.Trim();
                var nom = ws.Cells[row, 2].Text.Trim();
                var prenom = ws.Cells[row, 3].Text.Trim();
                var telephone = ws.Cells[row, 4].Text.Trim();
                var dateInscription = ws.Cells[row, 5].Text.Trim();

                // Ignorer la ligne si totalement vide
                if (string.IsNullOrWhiteSpace(email) &&
                    string.IsNullOrWhiteSpace(nom) &&
                    string.IsNullOrWhiteSpace(prenom) &&
                    string.IsNullOrWhiteSpace(telephone) &&
                    string.IsNullOrWhiteSpace(dateInscription))
                {
                    continue;
                }

                var dict = new Dictionary<string, string>
                {
                    ["Email"] = email,
                    ["Nom"] = nom,
                    ["Prenom"] = prenom,
                    ["Telephone"] = telephone,
                    ["DateInscription"] = dateInscription
                };

                result.Add(dict);
            }

            return result;
        }

        /// Convertit une ligne en Etudiant complet
        public async Task<Etudiant> ToEtudiantAsync(Dictionary<string, string> row, int cycleId, int niveauId, int specialiteId)
        {
            // Prénom et nom complets depuis Excel
            string prenomComplet = row["Prenom"];
            string nomComplet = row["Nom"];

            // Pour username/email : ne prendre que le **premier mot du prénom** et le **premier mot du nom**
            string prenom = prenomComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            string nom = nomComplet.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

            // Normalisation : minuscules, sans accents, sans caractères spéciaux
            string CleanString(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return "";
                input = input.ToLower().Trim();

                // Supprime les accents
                string normalized = string.Concat(input.Normalize(System.Text.NormalizationForm.FormD)
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

                // Ne garde que lettres et chiffres
                return new string(normalized.Where(c => char.IsLetterOrDigit(c)).ToArray());
            }

            string cleanPrenom = CleanString(prenom);
            string cleanNom = CleanString(nom);

            string username = $"{cleanPrenom}.{cleanNom}";
            string email = string.IsNullOrWhiteSpace(row["Email"]) ? $"{cleanPrenom}.{cleanNom}@gmail.com" : row["Email"];
            string emailInstitutionnel = $"{cleanPrenom}.{cleanNom}@saintjeaningenieur.org";

            // Date inscription
            DateTime dateInscription;
            if (!DateTime.TryParse(row["DateInscription"], out dateInscription))
                dateInscription = DateTime.UtcNow;

            // Mot de passe par défaut
            string password = "Changeme@2025";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Récupérer les entités Cycle, Niveau et Specialite
            var cycle = await _context.Cycles.FindAsync(cycleId);
            var niveau = await _context.Niveaux.FindAsync(niveauId);
            var specialite = await _context.Specialites.FindAsync(specialiteId);
            
            // Matricule auto avec gestion des valeurs nulles
            string matricule = GenerateMatricule(
                dateInscription, 
                niveau?.NomNiveau ?? "X", 
                cycle?.NomCycle ?? "X"
            );

            // Rôle par défaut : Étudiant
            var roleEtudiant = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Étudiant");

            // Créer l'étudiant
            var etudiant = new Etudiant
            {
                Username = username,
                MotDePasseHash = hashedPassword,
                Email = email,
                Active = true,
                CreatedAt = DateTime.UtcNow,

                Nom = nomComplet,       // conserve le nom complet
                Prenom = prenomComplet, // conserve le prénom complet
                Telephone = row["Telephone"],
                Matricule = matricule,
                CycleId = cycleId,
                NiveauId = niveauId,
                SpecialiteId = specialiteId,
                EmailInstitutionnel = emailInstitutionnel,
                DateInscription = dateInscription,
            };
            
            // Ajouter le rôle étudiant à la collection de rôles
            if (roleEtudiant != null)
            {
                etudiant.Roles = new List<Role> { roleEtudiant };
            }

            return etudiant;
        }
    }
}