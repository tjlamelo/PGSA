using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Data;
using PGSA_Licence3.Services.Groupes_Management;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers.Groups
{
    /// <summary>
    /// Contrôleur principal pour la gestion CRUD des groupes
    /// </summary>
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CreateGroupeService _createGroupeService;
        private readonly UpdateGroupeService _updateGroupeService;
        private readonly GetGroupesService _getGroupesService;
        private readonly GetGroupeDetailsService _getGroupeDetailsService;
        private readonly DeleteGroupeService _deleteGroupeService;

        public GroupsController(
            ApplicationDbContext context,
            CreateGroupeService createGroupeService,
            UpdateGroupeService updateGroupeService,
            GetGroupesService getGroupesService,
            GetGroupeDetailsService getGroupeDetailsService,
            DeleteGroupeService deleteGroupeService)
        {
            _context = context;
            _createGroupeService = createGroupeService;
            _updateGroupeService = updateGroupeService;
            _getGroupesService = getGroupesService;
            _getGroupeDetailsService = getGroupeDetailsService;
            _deleteGroupeService = deleteGroupeService;
        }

        // GET: /Groups
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Gestion des Groupes";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups")
            };

            var groupes = await _getGroupesService.GetAllGroupesAsync();
            return View(groupes);
        }

        // GET: /Groups/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Détails du groupe {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}")
            };

            return View(groupe);
        }

        // GET: /Groups/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Créer un groupe";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Créer", "/Groups/Create")
            };

            return View();
        }

        // POST: /Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nom, string niveau, string filiere, string creationMethod, List<int> selectedStudentIds)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var groupe = await _createGroupeService.CreerGroupeAsync(nom, niveau, filiere, creationMethod, selectedStudentIds);
                    TempData["Success"] = $"Le groupe '{groupe.Nom}' a été créé avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["Title"] = "Créer un groupe";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Créer", "/Groups/Create")
            };

            return View();
        }

        // GET: /Groups/CreateEquilibres
        public IActionResult CreateEquilibres()
        {
            ViewData["Title"] = "Créer des groupes équilibrés";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Groupes équilibrés", "/Groups/CreateEquilibres")
            };

            return View();
        }

        // POST: /Groups/CreateEquilibres
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEquilibres(string niveau, string filiere, int nombreGroupes)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var groupes = await _createGroupeService.CreerGroupesEquilibresParLettreAsync(niveau, filiere, nombreGroupes);
                    TempData["Success"] = $"{groupes.Count} groupes équilibrés ont été créés avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewData["Title"] = "Créer des groupes équilibrés";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                ("Groupes équilibrés", "/Groups/CreateEquilibres")
            };

            return View();
        }

        // GET: /Groups/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Modifier le groupe {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}"),
                ("Modifier", $"/Groups/Edit/{id}")
            };

            return View(groupe);
        }

        // POST: /Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string nom, string niveau, string filiere)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _updateGroupeService.ModifierGroupeAsync(id, nom, niveau, filiere);
                    TempData["Success"] = $"Le groupe a été modifié avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Modifier le groupe {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}"),
                ("Modifier", $"/Groups/Edit/{id}")
            };

            return View(groupe);
        }

        // POST: /Groups/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deleteGroupeService.DeleteGroupeAsync(id);
                TempData["Success"] = "Le groupe a été supprimé avec succès.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
