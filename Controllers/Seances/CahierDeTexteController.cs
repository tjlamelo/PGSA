using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;
using System.Security.Claims;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("CahierDeTexte")]
    public class CahierDeTexteController : Controller
    {
        private readonly CahierDeTexteService _cahierDeTexteService;
        private readonly ApplicationDbContext _db; // Ajouté pour vérifier les rôles

        // MODIFIÉ: Injecter ApplicationDbContext pour accéder aux rôles
        public CahierDeTexteController(ApplicationDbContext db)
        {
            _db = db;
            _cahierDeTexteService = new CahierDeTexteService(db);
        }

        // Méthode pour obtenir l'ID de l'utilisateur connecté
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // NOUVELLE: Méthode pour vérifier si l'utilisateur a un rôle spécifique
        private async Task<bool> HasRoleAsync(string roleName)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return false;
            
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return userRoles.Contains(roleName);
        }

        // GET: /CahierDeTexte
        [Route("")]
        public async Task<IActionResult> Index(int? seanceId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            // MODIFIÉ: Utiliser la nouvelle méthode basée sur les rôles
            var isEnseignant = await HasRoleAsync("Enseignant");
            var isDelegue = await HasRoleAsync("Délégué");
            
            List<PGSA_Licence3.Models.CahierDeTexte> cahiersDeTexte;
            
            if (isEnseignant)
            {
                cahiersDeTexte = await _cahierDeTexteService.GetByEnseignantAsync(userId);
            }
            else if (isDelegue)
            {
                cahiersDeTexte = await _cahierDeTexteService.GetByDelegueAsync(userId);
            }
            else // Par exemple, pour un administrateur
            {
                cahiersDeTexte = await _cahierDeTexteService.GetAllAsync();
            }
            
            ViewBag.Seances = await _cahierDeTexteService.GetSeancesAsync(userId, isEnseignant, isDelegue);
            ViewBag.InitialSeanceId = seanceId;
            ViewBag.IsEnseignant = isEnseignant;
            ViewBag.IsDelegue = isDelegue;

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
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Vous devez être connecté pour effectuer cette action" });
                }

                var isAuthorized = await _cahierDeTexteService.IsUserAuthorizedForSeanceAsync(userId, cahierDeTexte.SeanceId);
                if (!isAuthorized)
                {
                    return Json(new { success = false, message = "Vous n'êtes pas autorisé à modifier ce cahier de texte" });
                }

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
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vous devez être connecté pour accéder à cette ressource" });
            }

            var cahierDeTexte = await _cahierDeTexteService.GetByIdAsync(id);
            if (cahierDeTexte == null) return NotFound();
            
            var isAuthorized = await _cahierDeTexteService.IsUserAuthorizedForSeanceAsync(userId, cahierDeTexte.SeanceId);
            if (!isAuthorized)
            {
                return Json(new { success = false, message = "Vous n'êtes pas autorisé à accéder à ce cahier de texte" });
            }
            
            return Json(cahierDeTexte);
        }

        [HttpGet("GetBySeanceIdJson/{seanceId}")]
        public async Task<IActionResult> GetBySeanceIdJson(int seanceId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vous devez être connecté pour accéder à cette ressource" });
            }

            var isAuthorized = await _cahierDeTexteService.IsUserAuthorizedForSeanceAsync(userId, seanceId);
            if (!isAuthorized)
            {
                return Json(new { success = false, message = "Vous n'êtes pas autorisé à accéder à cette séance" });
            }

            var cahierDeTexte = await _cahierDeTexteService.GetBySeanceIdAsync(seanceId);
            if (cahierDeTexte == null) return NotFound();
            return Json(cahierDeTexte);
        }

        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["Error"] = "Vous devez être connecté pour effectuer cette action";
                    return RedirectToAction("Index");
                }

                var cahierDeTexte = await _cahierDeTexteService.GetByIdAsync(id);
                if (cahierDeTexte == null)
                {
                    TempData["Error"] = "Cahier de texte non trouvé";
                    return RedirectToAction("Index");
                }

                var isAuthorized = await _cahierDeTexteService.IsUserAuthorizedForSeanceAsync(userId, cahierDeTexte.SeanceId);
                if (!isAuthorized)
                {
                    TempData["Error"] = "Vous n'êtes pas autorisé à supprimer ce cahier de texte";
                    return RedirectToAction("Index");
                }

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