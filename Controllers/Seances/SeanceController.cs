using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("Seance")]
    public class SeanceController : Controller
    {
        private readonly SeanceService _seanceService;
 
        public SeanceController(ApplicationDbContext db)
        {
            _seanceService = new SeanceService(db);
        }

        // Liste des séances
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var seances = await _seanceService.GetAllAsync();
            ViewBag.Cours = await _seanceService.GetCoursAsync();
            ViewBag.Groupes = await _seanceService.GetGroupesAsync();
            ViewBag.Cycles = await _seanceService.GetCyclesAsync();
            ViewBag.Niveaux = await _seanceService.GetNiveauxAsync();
            ViewBag.Specialites = await _seanceService.GetSpecialitesAsync();
            return View("~/Views/User/Staff/Seance.cshtml", seances);
        }

        // POST : création ou mise à jour
        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Seance seance)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Formulaire invalide. Veuillez vérifier vos champs.";
                ViewBag.Cours = await _seanceService.GetCoursAsync();
                ViewBag.Groupes = await _seanceService.GetGroupesAsync();
                ViewBag.Cycles = await _seanceService.GetCyclesAsync();
                ViewBag.Niveaux = await _seanceService.GetNiveauxAsync();
                ViewBag.Specialites = await _seanceService.GetSpecialitesAsync();
                var seances = await _seanceService.GetAllAsync();
                return View("~/Views/User/Staff/Seance.cshtml", seances);
            }

            try
            {
                await _seanceService.CreateOrUpdateAsync(seance);
                TempData["Success"] = "Séance enregistrée avec succès !";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            ViewBag.Cours = await _seanceService.GetCoursAsync();
            ViewBag.Groupes = await _seanceService.GetGroupesAsync();
            ViewBag.Cycles = await _seanceService.GetCyclesAsync();
            ViewBag.Niveaux = await _seanceService.GetNiveauxAsync();
            ViewBag.Specialites = await _seanceService.GetSpecialitesAsync();
            var allSeances = await _seanceService.GetAllAsync();
            return View("~/Views/User/Staff/Seance.cshtml", allSeances);
        }

        // Suppression
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _seanceService.DeleteAsync(id);
                TempData["Success"] = "Séance supprimée avec succès !";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            var seances = await _seanceService.GetAllAsync();
            ViewBag.Cours = await _seanceService.GetCoursAsync();
            ViewBag.Groupes = await _seanceService.GetGroupesAsync();
            ViewBag.Cycles = await _seanceService.GetCyclesAsync();
            ViewBag.Niveaux = await _seanceService.GetNiveauxAsync();
            ViewBag.Specialites = await _seanceService.GetSpecialitesAsync();
            return View("~/Views/User/Staff/Seance.cshtml", seances);
        }
    }
}