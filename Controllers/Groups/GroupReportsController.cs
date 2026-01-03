using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Groupes_Management;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PGSA_Licence3.Controllers.Groups
{
    /// <summary>
    /// Contrôleur pour les rapports et statistiques des groupes
    /// </summary>
    public class GroupReportsController : Controller
    {
        private readonly GetGroupesService _getGroupesService;
        private readonly GetGroupeDetailsService _getGroupeDetailsService;

        public GroupReportsController(
            GetGroupesService getGroupesService,
            GetGroupeDetailsService getGroupeDetailsService)
        {
            _getGroupesService = getGroupesService;
            _getGroupeDetailsService = getGroupeDetailsService;
        }

        // GET: /GroupReports/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Rapports des Groupes";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Rapports", "/GroupReports")
            };

            return View();
        }

        // GET: /GroupReports/GetGroupesParClasse
        [HttpGet]
        public async Task<IActionResult> GetGroupesParClasse(string niveau, string filiere)
        {
            var groupes = await _getGroupesService.GetGroupesParClasseAsync(niveau, filiere);
            return Json(groupes);
        }

        // GET: /GroupReports/GetGroupesSummary
        [HttpGet]
        public async Task<IActionResult> GetGroupesSummary()
        {
            var allGroupes = await _getGroupesService.GetAllGroupesAsync();

            var summary = new
            {
                TotalGroupes = allGroupes.Count,
                GroupesParNiveau = allGroupes
                    .GroupBy(g => g.Niveau)
                    .Select(g => new
                    {
                        Niveau = g.Key,
                        Count = g.Count(),
                        TotalStudents = g.Sum(gr => gr.Etudiants?.Count ?? 0)
                    })
                    .ToList(),
                GroupesParFiliere = allGroupes
                    .GroupBy(g => g.Filiere)
                    .Select(g => new
                    {
                        Filiere = g.Key,
                        Count = g.Count(),
                        TotalStudents = g.Sum(gr => gr.Etudiants?.Count ?? 0)
                    })
                    .ToList(),
                TotalStudents = allGroupes.Sum(g => g.Etudiants?.Count ?? 0)
            };

            return Json(summary);
        }

        // GET: /GroupReports/ExportGroupes
        [HttpGet]
        public async Task<IActionResult> ExportGroupes(string format = "json")
        {
            var groupes = await _getGroupesService.GetAllGroupesAsync();

            if (format.ToLower() == "csv")
            {
                var csv = "Nom,Niveau,Filière,Nombre d'étudiants\n";
                foreach (var groupe in groupes)
                {
                    csv += $"{groupe.Nom},{groupe.Niveau},{groupe.Filiere},{groupe.Etudiants?.Count ?? 0}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", "groupes.csv");
            }

            return Json(groupes);
        }

        // GET: /GroupReports/GroupDetailsReport/5
        public async Task<IActionResult> GroupDetailsReport(int id)
        {
            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            var report = new
            {
                Groupe = groupe,
                Statistiques = new
                {
                    NombreEtudiants = groupe.Etudiants?.Count ?? 0,
                    MoyenneAge = 0, // Propriété Age non disponible dans le modèle Etudiant
                    RepartitionGenre = new List<object>() // Propriété Genre non disponible dans le modèle Etudiant
                }
            };

            ViewData["Title"] = $"Rapport détaillé - {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}"),
                ("Rapport", $"/GroupReports/GroupDetailsReport/{id}")
            };

            return View(report);
        }

        // GET: /GroupReports/ClassReport
        public async Task<IActionResult> ClassReport(string niveau, string filiere)
        {
            var groupes = await _getGroupesService.GetGroupesParClasseAsync(niveau, filiere);

            var report = new
            {
                Niveau = niveau,
                Filiere = filiere,
                Groupes = groupes,
                StatistiquesGlobales = new
                {
                    NombreGroupes = groupes.Count,
                    TotalEtudiants = groupes.Sum(g => g.NombreEtudiants),
                    MoyenneEtudiantsParGroupe = groupes.Any()
                        ? groupes.Average(g => g.NombreEtudiants)
                        : 0,
                    GroupePlusGrand = groupes.OrderByDescending(g => g.NombreEtudiants).FirstOrDefault(),
                    GroupePlusPetit = groupes.OrderBy(g => g.NombreEtudiants).FirstOrDefault()
                }
            };

            ViewData["Title"] = $"Rapport de classe - {niveau} {filiere}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Rapports", "/GroupReports"),
                ($"{niveau} {filiere}", $"/GroupReports/ClassReport?niveau={niveau}&filiere={filiere}")
            };

            return View(report);
        }
    }
}