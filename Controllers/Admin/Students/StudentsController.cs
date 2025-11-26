using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Services.Students;

namespace PGSA_Licence3.Controllers.Admin.Students

{
    public class StudentsController : Controller
    {
        private readonly StudentImportService _importService;

        public StudentsController()
        {
            _importService = new StudentImportService();
        }

        // GET : /Students/Import
        [HttpGet]
        public IActionResult Import()
        {
         return View("~/Views/Admin/Students/Import.cshtml");

        }

        // POST : /Students/Import
        [HttpPost]
        public IActionResult Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Veuillez fournir un fichier Excel.");

            using var stream = file.OpenReadStream();
            var students = _importService.ImportExcel(stream);

            return Json(students);
        }
    }
}
