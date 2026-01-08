using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Students;
using PGSA_Licence3.Models;
using PGSA_Licence3.Data;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            ExcelPackage.License.SetNonCommercialPersonal("TJBeats");
        }

        // GET : /Students/Import
        [HttpGet]
        public async Task<IActionResult> Import()
        {
            // Charger les cycles, niveaux et spécialités depuis la BD
            ViewBag.Cycles = await _importService.GetCyclesAsync();
            ViewBag.Niveaux = await _importService.GetNiveauxAsync();
            ViewBag.Specialites = await _importService.GetSpecialitesAsync();
            
            return View("~/Views/Admin/Students/Import.cshtml");
        }

        // GET : /Students/DownloadTemplate
        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var stream = _importService.GenerateTemplateExcel();
            string fileName = "Template_Import_Etudiants.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(stream, contentType, fileName);
        }

        // POST : /Students/Import
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, int cycleId, int niveauId, int specialiteId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Veuillez fournir un fichier Excel.");

            using var stream = file.OpenReadStream();

            var rows = _importService.ImportExcel(stream);

            var etudiants = new List<Etudiant>();
            foreach (var row in rows)
            {
                etudiants.Add(await _importService.ToEtudiantAsync(row, cycleId, niveauId, specialiteId));
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
    catch (DbUpdateException dbEx) // Capter spécifiquement les erreurs de mise à jour de la BD
    {
        // Log l'erreur complète côté serveur
        // TODO: Utilisez un vrai logger comme ILogger ou Serilog
        Console.WriteLine($"Erreur de mise à jour de la base de données: {dbEx.Message}");
        if (dbEx.InnerException != null)
        {
            Console.WriteLine($"Exception interne: {dbEx.InnerException.Message}");
        }
        
        // Renvoyer une erreur 500 avec les détails de l'exception interne
        var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;
        return StatusCode(500, new { 
            error = "Une erreur de base de données est survenue lors de l'enregistrement.", 
            details = innerExceptionMessage 
        });
    }
    catch (Exception ex)
    {
        // Log l'erreur complète côté serveur
        Console.WriteLine($"Erreur inattendue: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Exception interne: {ex.InnerException.Message}");
        }

        // Renvoyer une erreur 500 avec les détails
        var innerExceptionMessage = ex.InnerException?.Message ?? ex.Message;
        return StatusCode(500, new { 
            error = "Une erreur inattendue est survenue.", 
            details = innerExceptionMessage 
        });
    }
}
        public class SaveStudentsRequest
        {
            public List<Etudiant> Students { get; set; } = new();
        }
        
    }
}