using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using PGSA_Licence3.Models;

namespace PGSA_Licence3.Services.Students
{
    public class StudentImportService
    {
        public StudentImportService()
        {
            // Configure la licence EPPlus 8.x
            ExcelPackage.License.SetNonCommercialPersonal("TJBeats");
        }

        /// <summary>
        /// Importe le fichier Excel et retourne une liste de dictionnaires pour test.
        /// </summary>
        public List<Dictionary<string, string>> ImportExcel(Stream fileStream)
        {
            var result = new List<Dictionary<string, string>>();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets[0];

            int rowCount = ws.Dimension.End.Row;

            // Colonnes fixes : B = Matricule, C = Noms et Prénoms
            for (int row = 2; row <= rowCount; row++)
            {
                var matricule = ws.Cells[row, 2].Text.Trim();
                var nomsPrenoms = ws.Cells[row, 3].Text.Trim();

                // Parse Nom et Prénom selon le nombre de mots
                var words = nomsPrenoms.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                string nom = "";
                string prenom = "";

                if (words.Length >= 4)
                {
                    nom = string.Join(' ', words[0], words[1]);
                    prenom = string.Join(' ', words[2], words[3]);
                }
                else if (words.Length == 3)
                {
                    nom = string.Join(' ', words[0], words[1]);
                    prenom = words[2];
                }
                else if (words.Length == 2)
                {
                    nom = words[0];
                    prenom = words[1];
                }
                else if (words.Length == 1)
                {
                    nom = words[0];
                    prenom = "";
                }

                // Crée un dictionnaire pour test
                var dict = new Dictionary<string, string>
                {
                    ["Matricule"] = matricule,
                    ["Nom"] = nom,
                    ["Prenom"] = prenom,
                    ["Email"] = $"{matricule}@saintjeaningenieur.org",
                    ["Telephone"] = "",           // valeur par défaut
                    ["Niveau"] = "L3",           // valeur par défaut
                    ["Filiere"] = "Informatique",// valeur par défaut
                    ["EmailInstitutionnel"] = "" // valeur par défaut
                };

                result.Add(dict);
            }

            return result;
        }

        /// <summary>
        /// Convertit le dictionnaire en objet Etudiant avec tous les champs remplis
        /// </summary>
        public Etudiant ToEtudiant(Dictionary<string, string> row)
        {
            var matricule = row.ContainsKey("Matricule") ? row["Matricule"] : "unknown";

            return new Etudiant
            {
                Username = matricule,
                MotDePasseHash = "hashedDefault", // à remplacer par un vrai hash
                Email = row.ContainsKey("Email") ? row["Email"] : $"{matricule}@saintjeaningenieur.org",
                Nom = row.ContainsKey("Nom") ? row["Nom"] : "NomParDefaut",
                Prenom = row.ContainsKey("Prenom") ? row["Prenom"] : "PrenomParDefaut",
                Telephone = row.ContainsKey("Telephone") ? row["Telephone"] : "",
                Matricule = matricule,
                Niveau = row.ContainsKey("Niveau") ? row["Niveau"] : "L3",
                Filiere = row.ContainsKey("Filiere") ? row["Filiere"] : "Informatique",
                EmailInstitutionnel = row.ContainsKey("EmailInstitutionnel") ? row["EmailInstitutionnel"] : "",
                DateInscription = DateTime.UtcNow
            };
        }
    }
}
