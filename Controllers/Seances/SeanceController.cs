using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;
using System.Security.Claims;

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

        // Méthode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            
            // Alternative avec la session
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return HttpContext.Session.GetInt32("UserId").Value;
            }
            
            // Valeur par défaut si non connecté
            return 0;
        }

        // Méthode pour vérifier si l'utilisateur est un enseignant
        private async Task<bool> IsEnseignant()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return false;
            
            return await _seanceService.IsEnseignantAsync(userId);
        }

        // Méthode pour vérifier si l'utilisateur est un étudiant délégué
        private async Task<bool> IsDelegue()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return false;
            
            return await _seanceService.IsDelegueAsync(userId);
        }

        // Liste des séances
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var isEnseignant = await IsEnseignant();
            var isDelegue = await IsDelegue();
            
            List<Seance> seances;
            
            if (isEnseignant)
            {
                // Si l'utilisateur est un enseignant, récupérer uniquement ses séances
                seances = await _seanceService.GetSeancesByEnseignantAsync(userId);
            }
            else if (isDelegue)
            {
                // Si l'utilisateur est un délégué, récupérer les séances de sa classe
                seances = await _seanceService.GetSeancesByDelegueAsync(userId);
            }
            else
            {
                // Sinon, récupérer toutes les séances (comportement par défaut)
                seances = await _seanceService.GetAllAsync();
            }
            
            await LoadDropdowns();
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
                await LoadDropdowns();
                var seances = await GetFilteredSeances();
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

            await LoadDropdowns();
            var allSeances = await GetFilteredSeances();
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

            var seances = await GetFilteredSeances();
            await LoadDropdowns();
            return View("~/Views/User/Staff/Seance.cshtml", seances);
        }

        // Méthode helper pour obtenir les séances filtrées selon le rôle de l'utilisateur
        private async Task<List<Seance>> GetFilteredSeances()
        {
            var userId = GetCurrentUserId();
            var isEnseignant = await IsEnseignant();
            var isDelegue = await IsDelegue();
            
            if (isEnseignant)
            {
                return await _seanceService.GetSeancesByEnseignantAsync(userId);
            }
            else if (isDelegue)
            {
                return await _seanceService.GetSeancesByDelegueAsync(userId);
            }
            else
            {
                return await _seanceService.GetAllAsync();
            }
        }

        // Vérification des conflits (AJAX)
        [HttpPost("CheckConflict")]
        public async Task<JsonResult> CheckConflict(Seance seance)
        {
            try
            {
                var conflicts = await _seanceService.CheckConflictAsync(seance);
                return Json(new { success = true, conflicts = conflicts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Méthode helper pour remplir ViewBag (modifiée pour filtrer selon l'enseignant)
        private async Task LoadDropdowns()
        {
            var userId = GetCurrentUserId();
            var isEnseignant = await IsEnseignant();
            
            // Si l'utilisateur est un enseignant, ne récupérer que ses cours et les options associées
            if (isEnseignant)
            {
                ViewBag.Cours = await _seanceService.GetCoursAsync(userId);
                ViewBag.Cycles = await _seanceService.GetCyclesAsync(userId);
                ViewBag.Niveaux = await _seanceService.GetNiveauxAsync(userId);
                ViewBag.Specialites = await _seanceService.GetSpecialitesAsync(userId);
            }
            else
            {
                // Sinon, récupérer toutes les options
                ViewBag.Cours = await _seanceService.GetCoursAsync();
                ViewBag.Cycles = await _seanceService.GetCyclesAsync();
                ViewBag.Niveaux = await _seanceService.GetNiveauxAsync();
                ViewBag.Specialites = await _seanceService.GetSpecialitesAsync();
            }
            
            ViewBag.Groupes = await _seanceService.GetGroupesAsync();
            ViewBag.Salles = await _seanceService.GetSallesAsync();
        }
    }
}