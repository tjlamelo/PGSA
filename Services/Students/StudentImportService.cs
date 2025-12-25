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

        /// Lit Excel et ignore les lignes vides
        public List<Dictionary<string, string>> ImportExcel(Stream fileStream)
        {
            var result = new List<Dictionary<string, string>>();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];
            int rowCount = ws.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                var matricule = ws.Cells[row, 2].Text.Trim();
                var nomsPrenoms = ws.Cells[row, 3].Text.Trim();
                var telephone = ws.Cells[row, 4].Text.Trim();
                var dateInscription = ws.Cells[row, 5].Text.Trim();
                var niveau = ws.Cells[row, 6].Text.Trim();
                var cycle = ws.Cells[row, 7].Text.Trim();
                var specialite = ws.Cells[row, 8].Text.Trim();
                var motDePasse = ws.Cells[row, 9].Text.Trim();

                // Ignorer la ligne si totalement vide
                if (string.IsNullOrWhiteSpace(matricule) &&
                    string.IsNullOrWhiteSpace(nomsPrenoms) &&
                    string.IsNullOrWhiteSpace(telephone) &&
                    string.IsNullOrWhiteSpace(dateInscription) &&
                    string.IsNullOrWhiteSpace(niveau) &&
                    string.IsNullOrWhiteSpace(cycle) &&
                    string.IsNullOrWhiteSpace(specialite) &&
                    string.IsNullOrWhiteSpace(motDePasse))
                {
                    continue;
                }

                // Parse nom et prénom
                var names = nomsPrenoms.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string nom = "";
                string prenom = "";

                if (names.Length >= 2)
                {
                    nom = string.Join(' ', names[0..2]);        // Les 2 premiers mots pour le nom
                    prenom = string.Join(' ', names[2..]);      // Le reste pour le prénom
                }
                else if (names.Length == 1)
                {
                    nom = names[0];
                    prenom = "";
                }

                var dict = new Dictionary<string, string>
                {
                    ["Matricule"] = matricule,
                    ["Nom"] = nom,
                    ["Prenom"] = prenom,
                    ["Telephone"] = telephone,
                    ["DateInscription"] = dateInscription,
                    ["Niveau"] = string.IsNullOrWhiteSpace(niveau) ? "3" : niveau,
                    ["Cycle"] = string.IsNullOrWhiteSpace(cycle) ? "Licence" : cycle,
                    ["Specialite"] = specialite,
                    ["MotDePasse"] = string.IsNullOrWhiteSpace(motDePasse) ? "" : motDePasse
                };

                result.Add(dict);
            }

            return result;
        }

        /// Récupère ou crée une entité Cycle
        private async Task<Cycle> GetOrCreateCycleAsync(string nomCycle)
        {
            var cycle = await _context.Cycles.FirstOrDefaultAsync(c => c.NomCycle == nomCycle);
            
            if (cycle == null)
            {
                cycle = new Cycle { NomCycle = nomCycle };
                _context.Cycles.Add(cycle);
                await _context.SaveChangesAsync();
            }
            
            return cycle;
        }

        /// Récupère ou crée une entité Niveau
        private async Task<Niveau> GetOrCreateNiveauAsync(string nomNiveau)
        {
            var niveau = await _context.Niveaux.FirstOrDefaultAsync(n => n.NomNiveau == nomNiveau);
            
            if (niveau == null)
            {
                niveau = new Niveau { NomNiveau = nomNiveau };
                _context.Niveaux.Add(niveau);
                await _context.SaveChangesAsync();
            }
            
            return niveau;
        }

        /// Récupère ou crée une entité Specialite
        private async Task<Specialite> GetOrCreateSpecialiteAsync(string nomSpecialite)
        {
            var specialite = await _context.Specialites.FirstOrDefaultAsync(s => s.NomSpecialite == nomSpecialite);
            
            if (specialite == null)
            {
                specialite = new Specialite { NomSpecialite = nomSpecialite };
                _context.Specialites.Add(specialite);
                await _context.SaveChangesAsync();
            }
            
            return specialite;
        }

        /// Convertit une ligne en Etudiant complet
        public async Task<Etudiant> ToEtudiantAsync(Dictionary<string, string> row)
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
            string email = $"{cleanPrenom}.{cleanNom}@gmail.com";
            string emailInstitutionnel = $"{cleanPrenom}.{cleanNom}@saintjeaningenieur.org";

            // Date inscription
            DateTime dateInscription;
            if (!DateTime.TryParse(row["DateInscription"], out dateInscription))
                dateInscription = DateTime.UtcNow;

            // Mot de passe
            string password = string.IsNullOrWhiteSpace(row["MotDePasse"])
                ? "Changeme@2025"
                : row["MotDePasse"];
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Matricule auto
            string matricule = string.IsNullOrWhiteSpace(row["Matricule"])
                ? GenerateMatricule(dateInscription, row["Niveau"], row["Cycle"])
                : row["Matricule"];

            // Récupérer ou créer les entités liées
            var cycle = await GetOrCreateCycleAsync(row["Cycle"]);
            var niveau = await GetOrCreateNiveauAsync(row["Niveau"]);
            var specialite = await GetOrCreateSpecialiteAsync(row["Specialite"]);

            return new Etudiant
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
                CycleId = cycle.Id,
                NiveauId = niveau.Id,
                SpecialiteId = specialite.Id,
                EmailInstitutionnel = emailInstitutionnel,
                DateInscription = dateInscription,
            
            };
        }

        /// Importe une liste d'étudiants depuis un fichier Excel
        public async Task<List<Etudiant>> ImportStudentsFromExcelAsync(Stream fileStream)
        {
            var studentData = ImportExcel(fileStream);
            var students = new List<Etudiant>();

            foreach (var row in studentData)
            {
                try
                {
                    var etudiant = await ToEtudiantAsync(row);
                    students.Add(etudiant);
                }
                catch (Exception ex)
                {
                    // Gérer les erreurs d'importation
                    Console.WriteLine($"Erreur lors de l'importation de l'étudiant {row["Matricule"]}: {ex.Message}");
                }
            }

            return students;
        }
    }
}