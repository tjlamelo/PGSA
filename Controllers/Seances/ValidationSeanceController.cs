using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.Seances;
using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Controllers.Seances
{
    [Route("ValidationSeance")]
    [Authorize]
    public class ValidationSeanceController : Controller
    {
        private readonly ValidationSeanceService _validationSeanceService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ValidationSeanceController> _logger;

        public ValidationSeanceController(ApplicationDbContext db, ILogger<ValidationSeanceController> logger)
        {
            _context = db;
            _logger = logger;
            _validationSeanceService = new ValidationSeanceService(db);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var seances = await _validationSeanceService.GetSeancesEligiblesAValidationAsync();
                return View("~/Views/User/Staff/ValidationSeance.cshtml", seances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERREUR DANS Index");
                TempData["Error"] = "Impossible de charger les séances à valider.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost("Valider")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Valider([FromBody] ValidationRequestModel model)
        {
            try
            {
                // Log pour déboguer
                _logger.LogInformation("Requête de validation reçue: {@Model}", model);

                // Vérification que le modèle n'est pas null
                if (model == null)
                {
                    _logger.LogWarning("Modèle de validation null");
                    return Json(new { success = false, message = "Données de validation invalides." });
                }

                // Vérification que l'ID de séance est valide
                if (model.SeanceId <= 0)
                {
                    _logger.LogWarning("ID de séance invalide: {SeanceId}", model.SeanceId);
                    return Json(new { success = false, message = "ID de séance invalide." });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int validateurId))
                {
                    _logger.LogWarning("Utilisateur non identifié");
                    return Json(new { success = false, message = "Utilisateur non identifié." });
                }

                TypeValidation typeValidation;
                if (User.IsInRole("Enseignant"))
                {
                    typeValidation = TypeValidation.Enseignant;
                }
                else if (User.IsInRole("Délégué"))
                {
                    typeValidation = TypeValidation.Delegue;
                }
                else
                {
                    _logger.LogWarning("Permissions insuffisantes pour l'utilisateur {UserId}", validateurId);
                    return Json(new { success = false, message = "Permissions insuffisantes pour valider." });
                }

                // Vérification que la séance existe
                var seance = await _context.Seances
                    .Include(s => s.Validations)
                    .FirstOrDefaultAsync(s => s.Id == model.SeanceId);
                    
                if (seance == null)
                {
                    _logger.LogWarning("Séance non trouvée: {SeanceId}", model.SeanceId);
                    return Json(new { success = false, message = $"La séance avec l'ID {model.SeanceId} n'a pas été trouvée." });
                }

                // Vérification que la séance est bien terminée
                if (seance.Statut != StatutSeance.Terminee)
                {
                    _logger.LogWarning("Tentative de validation d'une séance non terminée: {SeanceId}", model.SeanceId);
                    return Json(new { success = false, message = "Seule une séance terminée peut être validée." });
                }

                // Vérification que l'utilisateur n'a pas déjà validé cette séance
                if (seance.Validations != null && seance.Validations.Any(v => v.ValidateurId == validateurId && v.TypeValidation == typeValidation))
                {
                    _logger.LogWarning("L'utilisateur {UserId} a déjà validé la séance {SeanceId}", validateurId, model.SeanceId);
                    return Json(new { success = false, message = "Vous avez déjà validé cette séance." });
                }

                var validation = await _validationSeanceService.CreateOrUpdateValidationAsync(
                    model.SeanceId,
                    validateurId,
                    typeValidation,
                    model.Statut,
                    model.Commentaire
                );

                var validateurName = $"{validation.Validateur?.Prenom} {validation.Validateur?.Nom}";
                _logger.LogInformation("Validation enregistrée avec succès pour la séance {SeanceId}", model.SeanceId);
                
                return Json(new { 
                    success = true, 
                    message = $"Validation enregistrée par {validateurName}.",
                    statut = validation.Statut.ToString(),
                    statutValue = (int)validation.Statut, // Ajout de la valeur numérique
                    dateValidation = validation.DateValidation.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERREUR DANS Valider");
                return Json(new { success = false, message = "Une erreur technique est survenue. Détails : " + ex.Message });
            }
        }
    }

    public class ValidationRequestModel
    {
        public int SeanceId { get; set; }
        public StatutValidation Statut { get; set; }
        public string? Commentaire { get; set; }
    }
}