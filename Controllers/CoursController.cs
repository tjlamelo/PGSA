using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PGSA_Licence3.Controllers
{
    public class CoursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Courses
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Cours";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Cours", "/Courses")
            };

            var cours = await _context.Cours
                .Include(c => c.Enseignant)
                .OrderBy(c => c.Niveau)
                .ThenBy(c => c.Nom)
                .ToListAsync();

            return View(cours);
        }

        // GET: /Courses/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Créer un cours";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Cours", "/Courses"),
                ("Créer", "/Courses/Create")
            };

            var enseignants = await _context.Enseignants.ToListAsync();
            ViewBag.Enseignants = enseignants;

            return View();
        }

        // POST: /Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nom, string code, string filiere, string niveau, string semestre, int anneeAcademique, int enseignantId)
        {
            if (ModelState.IsValid)
            {
                var cours = new Cours
                {
                    Nom = nom,
                    Code = code,
                    Filiere = filiere,
                    Niveau = niveau,
                    Semestre = semestre,
                    AnneeAcademique = anneeAcademique,
                    EnseignantId = enseignantId
                };

                _context.Cours.Add(cours);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Le cours '{cours.Nom}' a été créé avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var enseignants = await _context.Enseignants.ToListAsync();
            ViewBag.Enseignants = enseignants;

            ViewData["Title"] = "Créer un cours";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Cours", "/Courses"),
                ("Créer", "/Courses/Create")
            };

            return View();
        }

        // GET: /Courses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cours = await _context.Cours
                .Include(c => c.Enseignant)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cours == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Modifier le cours {cours.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Cours", "/Courses"),
                (cours.Nom, $"/Courses/Edit/{id}")
            };

            var enseignants = await _context.Enseignants.ToListAsync();
            ViewBag.Enseignants = enseignants;

            return View(cours);
        }

        // POST: /Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nom, string code, string filiere, string niveau, string semestre, int anneeAcademique, int enseignantId)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                cours.Nom = nom;
                cours.Code = code;
                cours.Filiere = filiere;
                cours.Niveau = niveau;
                cours.Semestre = semestre;
                cours.AnneeAcademique = anneeAcademique;
                cours.EnseignantId = enseignantId;
                cours.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Le cours a été modifié avec succès.";
                return RedirectToAction(nameof(Index));
            }

            var enseignants = await _context.Enseignants.ToListAsync();
            ViewBag.Enseignants = enseignants;

            ViewData["Title"] = $"Modifier le cours {cours.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Cours", "/Courses"),
                (cours.Nom, $"/Courses/Edit/{id}")
            };

            return View(cours);
        }

        // POST: /Courses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
            {
                return NotFound();
            }

            _context.Cours.Remove(cours);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Le cours a été supprimé avec succès.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Courses/GetByNiveau?niveau=L3
        [HttpGet]
        public async Task<IActionResult> GetByNiveau(string niveau)
        {
            if (string.IsNullOrEmpty(niveau))
            {
                return Json(new List<object>());
            }

            var cours = await _context.Cours
                .Where(c => c.Niveau == niveau)
                .OrderBy(c => c.Nom)
                .Select(c => new { id = c.Id, nom = c.Nom, code = c.Code })
                .ToListAsync();

            return Json(cours);
        }

        // GET: /Courses/GetDefaultMatieres?niveau=L3
        [HttpGet]
        public IActionResult GetDefaultMatieres(string niveau)
        {
            var matieres = new Dictionary<string, List<object>>();

            // Matières pour L3
            matieres["L3"] = new List<object>
            {
                new { nom = "RO", code = "RO", description = "Réseaux et Ordinateurs" },
                new { nom = "Génie Logiciel", code = "GL", description = "Génie Logiciel" },
                new { nom = "Dev Mobile", code = "DM", description = "Développement Mobile" },
                new { nom = "Communication", code = "COM", description = "Communication" },
                new { nom = "Sécurité", code = "SEC", description = "Sécurité Informatique" },
                new { nom = "Anglais", code = "ANG", description = "Anglais" },
                new { nom = "Projet TUTO", code = "PT", description = "Projet Tutoré" }
            };

            // Matières pour L1 (exemple)
            matieres["L1"] = new List<object>
            {
                new { nom = "Algorithmique", code = "ALG", description = "Algorithmique" },
                new { nom = "Programmation", code = "PROG", description = "Programmation" },
                new { nom = "Mathématiques", code = "MATH", description = "Mathématiques" }
            };

            // Matières pour L2 (exemple)
            matieres["L2"] = new List<object>
            {
                new { nom = "Structures de Données", code = "SD", description = "Structures de Données" },
                new { nom = "Bases de Données", code = "BD", description = "Bases de Données" },
                new { nom = "Systèmes d'Exploitation", code = "SE", description = "Systèmes d'Exploitation" }
            };

            if (matieres.ContainsKey(niveau))
            {
                return Json(matieres[niveau]);
            }

            return Json(new List<object>());
        }
    }
}

