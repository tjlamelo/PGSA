using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("CahierDeTexte")]
    public class CahierDeTexteController : Controller
    {
        private readonly CahierDeTexteService _cahierDeTexteService;

        public CahierDeTexteController(ApplicationDbContext db)
        {
            _cahierDeTexteService = new CahierDeTexteService(db);
        }

        // GET: /CahierDeTexte
        [Route("")]
        public async Task<IActionResult> Index(int? seanceId)
        {
            var cahiersDeTexte = await _cahierDeTexteService.GetAllAsync();
            ViewBag.Seances = await _cahierDeTexteService.GetSeancesAsync();
            ViewBag.InitialSeanceId = seanceId;
            return View("~/Views/User/Staff/CahierDeTexte.cshtml", cahiersDeTexte);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(PGSA_Licence3.Models.CahierDeTexte cahierDeTexte)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Formulaire invalide. Veuillez vérifier vos champs." });
            }

            try
            {
                await _cahierDeTexteService.CreateOrUpdateAsync(cahierDeTexte);
                return Json(new { success = true, message = "Cahier de texte enregistré avec succès !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetByIdJson/{id}")]
        public async Task<IActionResult> GetByIdJson(int id)
        {
            var cahierDeTexte = await _cahierDeTexteService.GetByIdAsync(id);
            if (cahierDeTexte == null) return NotFound();
            return Json(cahierDeTexte);
        }

        [HttpGet("GetBySeanceIdJson/{seanceId}")]
        public async Task<IActionResult> GetBySeanceIdJson(int seanceId)
        {
            var cahierDeTexte = await _cahierDeTexteService.GetBySeanceIdAsync(seanceId);
            if (cahierDeTexte == null) return NotFound();
            return Json(cahierDeTexte);
        }

        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _cahierDeTexteService.DeleteAsync(id);
                TempData["Success"] = "Cahier de texte supprimé avec succès !";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}