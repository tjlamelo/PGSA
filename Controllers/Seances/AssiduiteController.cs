using Microsoft.AspNetCore.Mvc;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;
using System.Security.Claims;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("Assiduite")]
    public class AssiduiteController : Controller
    {
        private readonly AssiduiteService _assiduiteService;

        public AssiduiteController(ApplicationDbContext db)
        {
            _assiduiteService = new AssiduiteService(db);
        }

        // Afficher la page de prise d'appel pour une séance
        [HttpGet("PriseAppel/{seanceId}")]
        public async Task<IActionResult> PriseAppel(int seanceId)
        {
            var seance = await _assiduiteService.GetSeanceByIdAsync(seanceId);
            if (seance == null)
            {
                TempData["Error"] = "Séance non trouvée";
                return RedirectToAction("Index", "Seance");
            }

            var etudiants = await _assiduiteService.GetEtudiantsForSeanceAsync(seanceId);
            var assiduitesExistantes = await _assiduiteService.GetBySeanceIdAsync(seanceId);
            var appelDejaFait = await _assiduiteService.AppelDejaFaitAsync(seanceId);

            // Créer le view model avec les informations nécessaires
            var viewModel = new PriseAppelViewModel
            {
                Seance = seance,
                Etudiants = etudiants,
                AssiduitesExistantes = assiduitesExistantes,
                AppelDejaFait = appelDejaFait,
                DureeSeance = (double)seance.Duree
            };

            return View("~/Views/User/Staff/PriseAppel.cshtml", viewModel);
        }

        // Traiter la soumission du formulaire de prise d'appel
        [HttpPost("SaveAppel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAppel(int seanceId, Dictionary<int, StatutPresence> presences, Dictionary<int, double> heuresEffectuees, Dictionary<int, string> commentaires)
        {
            try
            {
                // Récupérer l'ID de l'utilisateur connecté
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int enregistreurId))
                {
                    // return RedirectToAction("Login", "Auth");
                }

                await _assiduiteService.SaveAppelAsync(seanceId, presences, heuresEffectuees, commentaires, 16);
                TempData["Success"] = "L'appel a été enregistré avec succès";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de l'enregistrement de l'appel: {ex.Message}";
            }

            return RedirectToAction("PriseAppel", new { seanceId });
        }
    }

    // ViewModel pour la vue de prise d'appel
    public class PriseAppelViewModel
    {
        public Seance? Seance { get; set; }
        public List<Etudiant>? Etudiants { get; set; }
        public List<Assiduite>? AssiduitesExistantes { get; set; }
        public bool AppelDejaFait { get; set; }
        public double DureeSeance { get; set; }
    }
}