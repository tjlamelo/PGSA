using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;
using System.Security.Claims;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("SignalementAbsenceEnseignant")]
    public class SignalementAbsenceEnseignantController : Controller
    {
        private readonly SignalementAbsenceEnseignantService _signalementService;
        private readonly SeanceService _seanceService;

        public SignalementAbsenceEnseignantController(ApplicationDbContext db)
        {
            _signalementService = new SignalementAbsenceEnseignantService(db);
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

        // Méthode pour vérifier si l'utilisateur est un administrateur
        private async Task<bool> IsAdministrateur()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return false;
            
            return await _signalementService.IsAdministrateurAsync(userId);
        }

        // Méthode pour vérifier si l'utilisateur est un étudiant délégué
        private async Task<bool> IsDelegue()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return false;
            
            return await _seanceService.IsDelegueAsync(userId);
        }

        // GET: /SignalementAbsenceEnseignant
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                TempData["Error"] = "Vous devez être connecté pour accéder à cette page";
                return RedirectToAction("Login", "Auth");
            }

            var isAdministrateur = await IsAdministrateur();
            var isDelegue = await IsDelegue();
            
            List<SignalementAbsenceEnseignant> signalements;
            
            if (isAdministrateur)
            {
                // Si l'utilisateur est un administrateur, récupérer tous les signalements
                signalements = await _signalementService.GetAllAsync();
            }
            else if (isDelegue)
            {
                // Si l'utilisateur est un délégué, récupérer uniquement ses signalements
                signalements = await _signalementService.GetByDelegueAsync(userId);
            }
            else
            {
                // Sinon, retourner une liste vide (l'utilisateur n'a pas les droits)
                signalements = new List<SignalementAbsenceEnseignant>();
                TempData["Error"] = "Vous n'avez pas les droits pour accéder à cette page";
            }
            
            ViewBag.Seances = await _signalementService.GetSeancesAsync(userId, isAdministrateur, isDelegue);
            ViewBag.IsAdministrateur = isAdministrateur;
            ViewBag.IsDelegue = isDelegue;

            return View("~/Views/User/Staff/SignalementAbsenceEnseignant.cshtml", signalements);
        }

        // POST: /SignalementAbsenceEnseignant/Traiter/{id}
        [HttpPost("Traiter/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Traiter(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vous devez être connecté pour effectuer cette action" });
            }

            // Vérifier si l'utilisateur est un administrateur
            var isAdministrateur = await IsAdministrateur();
            if (!isAdministrateur)
            {
                return Json(new { success = false, message = "Seuls les administrateurs peuvent traiter les signalements" });
            }

            try
            {
                await _signalementService.UpdateStatusAsync(id, StatutSignalement.Traite, userId);
                return Json(new { success = true, message = "Le signalement a été marqué comme traité." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /SignalementAbsenceEnseignant/Rejeter/{id}
        [HttpPost("Rejeter/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeter(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vous devez être connecté pour effectuer cette action" });
            }

            // Vérifier si l'utilisateur est un administrateur
            var isAdministrateur = await IsAdministrateur();
            if (!isAdministrateur)
            {
                return Json(new { success = false, message = "Seuls les administrateurs peuvent rejeter les signalements" });
            }

            try
            {
                await _signalementService.UpdateStatusAsync(id, StatutSignalement.Rejete, userId);
                return Json(new { success = true, message = "Le signalement a été rejeté." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /SignalementAbsenceEnseignant/Signaler (Action pour le modal AJAX)
        [HttpPost("Signaler")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signaler(SignalementAbsenceEnseignant signalement)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Formulaire invalide." });
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Utilisateur non connecté." });
            }

            // Vérifier si l'utilisateur est un délégué
            var isDelegue = await IsDelegue();
            if (!isDelegue)
            {
                return Json(new { success = false, message = "Seuls les délégués peuvent signaler des absences d'enseignants." });
            }

            // Vérifier si le délégué est autorisé à signaler cette séance
            var isAuthorized = await _signalementService.IsDelegueAuthorizedForSeanceAsync(userId, signalement.SeanceId);
            if (!isAuthorized)
            {
                return Json(new { success = false, message = "Vous n'êtes pas autorisé à signaler cette séance." });
            }

            try
            {
                await _signalementService.CreateAsync(signalement, userId);
                return Json(new { success = true, message = "Votre signalement a été envoyé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Une erreur est survenue : " + ex.Message });
            }
        }
    }
}