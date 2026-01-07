using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Statistics;
using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers
{
    public class ReportsController : Controller
    {
        private readonly StatisticsService _statisticsService;
        private readonly ApplicationDbContext _context;

        public ReportsController(StatisticsService statisticsService, ApplicationDbContext context)
        {
            _statisticsService = statisticsService;
            _context = context;
        }

        // GET: /Reports
        public IActionResult Index()
        {
            ViewData["Title"] = "Statistiques et Rapports";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports")
            };

            return View();
        }

        // GET: /Reports/Student
        public async Task<IActionResult> Student(int? id, DateTime? dateDebut, DateTime? dateFin)
        {
            if (id == null)
            {
                // Afficher la liste des étudiants pour sélection
                var etudiants = await _context.Etudiants
                    .OrderBy(e => e.Nom)
                    .ThenBy(e => e.Prenom)
                    .ToListAsync();
                
                ViewData["Title"] = "Sélection d'un étudiant";
                ViewData["Breadcrumb"] = new List<(string Text, string Url)>
                {
                    ("Statistiques et Rapports", "/Reports"),
                    ("Rapport étudiant", "/Reports/Student")
                };
                
                ViewBag.Etudiants = etudiants;
                return View("StudentSelect");
            }

            ViewData["Title"] = "Rapport individuel étudiant";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Rapport étudiant", $"/Reports/Student/{id}")
            };

            var etudiant = await _context.Etudiants.FindAsync(id);
            if (etudiant == null)
            {
                return NotFound();
            }

            var statistics = await _statisticsService.GetStudentStatisticsAsync(id.Value, dateDebut, dateFin);

            ViewBag.Etudiant = etudiant;
            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/Course
        public async Task<IActionResult> Course(DateTime? dateDebut, DateTime? dateFin)
        {
            ViewData["Title"] = "Rapport par cours";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Rapport par cours", "/Reports/Course")
            };

            var statistics = await _statisticsService.GetAbsenteeismByCourseAsync(dateDebut, dateFin);

            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/Group
        public async Task<IActionResult> Group(DateTime? dateDebut, DateTime? dateFin)
        {
            ViewData["Title"] = "Rapport par groupe";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Rapport par groupe", "/Reports/Group")
            };

            var statistics = await _statisticsService.GetAbsenteeismByGroupAsync(dateDebut, dateFin);

            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/Promotion
        public async Task<IActionResult> Promotion(DateTime? dateDebut, DateTime? dateFin)
        {
            ViewData["Title"] = "Rapport par promotion";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Rapport par promotion", "/Reports/Promotion")
            };

            var statistics = await _statisticsService.GetAbsenteeismByPromotionAsync(dateDebut, dateFin);

            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/Critical
        public async Task<IActionResult> Critical(double seuil = 75.0, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            ViewData["Title"] = "Étudiants en situation critique";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Étudiants critiques", "/Reports/Critical")
            };

            var statistics = await _statisticsService.GetCriticalStudentsAsync(seuil, dateDebut, dateFin);

            ViewBag.Seuil = seuil;
            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/TeacherAbsence
        public async Task<IActionResult> TeacherAbsence(DateTime? dateDebut, DateTime? dateFin)
        {
            ViewData["Title"] = "Statistiques absences enseignants";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Statistiques et Rapports", "/Reports"),
                ("Absences enseignants", "/Reports/TeacherAbsence")
            };

            var statistics = await _statisticsService.GetTeacherAbsenceStatisticsAsync(dateDebut, dateFin);

            ViewBag.DateDebut = dateDebut;
            ViewBag.DateFin = dateFin;

            return View(statistics);
        }

        // GET: /Reports/Export/Student/{id}
        public async Task<IActionResult> ExportStudent(int id, string format = "csv", DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var statistics = await _statisticsService.GetStudentStatisticsAsync(id, dateDebut, dateFin);
            var etudiant = await _context.Etudiants.FindAsync(id);

            if (etudiant == null)
            {
                return NotFound();
            }

            if (format.ToLower() == "csv")
            {
                var csv = "Étudiant,Total Séances,Présences,Absences,Retards,Taux Assiduité (%),Absences Justifiées,Absences Injustifiées\n";
                csv += $"{etudiant.Nom} {etudiant.Prenom},{statistics.TotalSeances},{statistics.Presences},{statistics.Absences},{statistics.Retards},{statistics.TauxAssiduite:F2},{statistics.AbsencesJustifiees},{statistics.AbsencesInjustifiees}\n";
                csv += "\nDétails par cours:\n";
                csv += "Cours,Code,Total Séances,Présences,Absences,Retards,Taux Assiduité (%)\n";
                foreach (var detail in statistics.DetailsParCours)
                {
                    csv += $"{detail.CoursNom},{detail.CoursCode},{detail.TotalSeances},{detail.Presences},{detail.Absences},{detail.Retards},{detail.TauxAssiduite:F2}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"rapport_etudiant_{etudiant.Matricule}_{DateTime.Now:yyyyMMdd}.csv");
            }

            return Json(statistics);
        }
    }
}

