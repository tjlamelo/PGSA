using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Students;
using PGSA_Licence3.Models;
using PGSA_Licence3.Data; // pour ApplicationDbContext
using System.Threading.Tasks;

namespace PGSA_Licence3.Controllers.Admin.Students
{
    public class StudentsController : Controller
    {
        private readonly StudentImportService _importService;
        private readonly SaveImportedStudentsService _saveService;

        public StudentsController(ApplicationDbContext db)
        {
            _importService = new StudentImportService(db);
            _saveService = new SaveImportedStudentsService(db);
        }

        // GET : /Students/Import
        [HttpGet]
        public IActionResult Import()
        {
            return View("~/Views/Admin/Students/Import.cshtml");
        }

        // POST : /Students/Import
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Veuillez fournir un fichier Excel.");

            using var stream = file.OpenReadStream();

            var rows = _importService.ImportExcel(stream);

            var etudiants = new List<Etudiant>();
            foreach (var row in rows)
            {
                etudiants.Add(await _importService.ToEtudiantAsync(row));
            }

            return Json(etudiants);
        }

        // POST : /Students/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveStudentsRequest request)
        {
            if (request.Students == null || request.Students.Count == 0)
                return BadRequest("Aucun étudiant à enregistrer.");

            try
            {
                // Appel sans overwrite, import irréversible
                var results = await _saveService.SaveWithConflictsAsync(request.Students);

                var savedCount = results.Count(r => r.Saved);
                var conflicts = results.Where(r => !r.Saved).Select(r => new
                {
                    r.Student.Matricule,
                    r.Student.Username,
                    r.Student.Email,
                    r.Problem
                }).ToList();

                return Ok(new
                {
                    message = $"Import terminé. {savedCount} étudiant(s) enregistré(s).",
                    totalSaved = savedCount,
                    conflicts
                });
            }
            catch (Exception ex)
            {
                // Retourne l'exception en JSON pour debug
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        public class SaveStudentsRequest
        {
            public List<Etudiant> Students { get; set; } = new();
        }
    }
}