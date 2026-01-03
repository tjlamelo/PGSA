using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Groupes_Management;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PGSA_Licence3.Controllers.Groups
{
    /// <summary>
    /// Contrôleur pour la gestion des étudiants dans les groupes
    /// </summary>
    public class GroupStudentsController : Controller
    {
        private readonly AddStudentToGroupeService _addStudentService;
        private readonly RemoveStudentFromGroupeService _removeStudentService;
        private readonly ImportStudentsToGroupeService _importStudentsService;
        private readonly GetGroupeDetailsService _getGroupeDetailsService;

        public GroupStudentsController(
            AddStudentToGroupeService addStudentService,
            RemoveStudentFromGroupeService removeStudentService,
            ImportStudentsToGroupeService importStudentsService,
            GetGroupeDetailsService getGroupeDetailsService)
        {
            _addStudentService = addStudentService;
            _removeStudentService = removeStudentService;
            _importStudentsService = importStudentsService;
            _getGroupeDetailsService = getGroupeDetailsService;
        }

        // GET: /GroupStudents/AddStudent/5
        public async Task<IActionResult> AddStudent(int id)
        {
            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            var availableStudents = await _importStudentsService.GetAvailableStudentsAsync();

            ViewData["Title"] = $"Ajouter des étudiants - {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}"),
                ("Ajouter étudiants", $"/GroupStudents/AddStudent/{id}")
            };

            ViewBag.Groupe = groupe;
            ViewBag.AvailableStudents = availableStudents;

            return View();
        }

        // POST: /GroupStudents/AddStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(int id, List<int> selectedStudentIds)
        {
            if (selectedStudentIds != null && selectedStudentIds.Any())
            {
                try
                {
                    var result = await _importStudentsService.ImportStudentsToGroupeAsync(selectedStudentIds, id);
                    TempData["Success"] = $"{result.SuccessfullyImported} étudiant(s) ajouté(s) au groupe.";
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return RedirectToAction("Details", "Groups", new { id });
        }

        // POST: /GroupStudents/RemoveStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int groupeId, int etudiantId)
        {
            try
            {
                await _removeStudentService.RemoveStudentFromGroupeAsync(etudiantId);
                TempData["Success"] = "L'étudiant a été retiré du groupe.";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "Groups", new { id = groupeId });
        }

        // GET: /GroupStudents/GetAvailableStudents
        [HttpGet]
        public async Task<IActionResult> GetAvailableStudents()
        {
            var students = await _importStudentsService.GetAvailableStudentsAsync();
            return Json(students.Select(s => new { id = s.Id, text = $"{s.Nom} {s.Prenom}" }));
        }

        // GET: /GroupStudents/ImportStudents/5
        public async Task<IActionResult> ImportStudents(int id)
        {
            var groupe = await _getGroupeDetailsService.GetGroupeByIdAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Importer des étudiants - {groupe.Nom}";
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Gestion des Groupes", "/Groups"),
                (groupe.Nom, $"/Groups/Details/{id}"),
                ("Importer étudiants", $"/GroupStudents/ImportStudents/{id}")
            };

            ViewBag.Groupe = groupe;

            return View();
        }

        // POST: /GroupStudents/ImportStudents/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportStudents(int id, List<int> selectedStudentIds)
        {
            if (selectedStudentIds == null || !selectedStudentIds.Any())
            {
                TempData["Error"] = "Aucun étudiant sélectionné.";
                return RedirectToAction("AddStudent", new { id });
            }

            try
            {
                var result = await _importStudentsService.ImportStudentsToGroupeAsync(selectedStudentIds, id);
                TempData["Success"] = $"{result.SuccessfullyImported} étudiant(s) importé(s) avec succès. {result.AlreadyInGroup} déjà dans le groupe. {result.NotFound} non trouvé(s).";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "Groups", new { id });
        }
    }
}