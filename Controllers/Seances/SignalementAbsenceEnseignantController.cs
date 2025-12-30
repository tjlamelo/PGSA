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

        public SignalementAbsenceEnseignantController(ApplicationDbContext db)
        {
            _signalementService = new SignalementAbsenceEnseignantService(db);
        }

        // GET: /SignalementAbsenceEnseignant
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var signalements = await _signalementService.GetAllAsync();
            
            // Les ViewBag sont utiles pour le modal qui sera sur cette même page
            ViewBag.Seances = await _signalementService.GetSeancesAsync();

            return View("~/Views/User/Staff/SignalementAbsenceEnseignant.cshtml", signalements);
        }

        // POST: /SignalementAbsenceEnseignant/Traiter/{id}
        [HttpPost("Traiter/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Traiter(int id)
        {
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int traitantId))
            // {
            //     return Challenge();
            // }

            try
            {
                await _signalementService.UpdateStatusAsync(id, StatutSignalement.Traite, 16);
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
            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int traitantId))
            // {
            //     return Challenge();
            // }

            try
            {
                await _signalementService.UpdateStatusAsync(id, StatutSignalement.Rejete, 16);
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

            // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            // if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int delegueId))
            // {
            //     return Json(new { success = false, message = "Utilisateur non connecté." });
            // }

            try
            {
                await _signalementService.CreateAsync(signalement, 16);
                return Json(new { success = true, message = "Votre signalement a été envoyé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Une erreur est survenue : " + ex.Message });
            }
        }
    }
}