using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using PGSA_Licence3.Services.UserManagement;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Globalization;

namespace PGSA_Licence3.Controllers.Admin.Account
{
    [Route("Admin/UserManagement/Account")]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _accountService = new AccountService(context);
        }

        // Page principale
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Récupérer tous les utilisateurs avec leurs détails
            var users = await _accountService.GetAllUsersAsync();

            // Récupérer les données de référence pour les formulaires
            ViewBag.Cycles = await _accountService.GetCyclesAsync();
            ViewBag.Niveaux = await _accountService.GetNiveauxAsync();
            ViewBag.Specialites = await _accountService.GetSpecialitesAsync();

            // Préparer les listes pour les dropdowns
            ViewBag.CycleList = new SelectList(await _accountService.GetCyclesAsync(), "Id", "NomCycle");
            ViewBag.NiveauList = new SelectList(await _accountService.GetNiveauxAsync(), "Id", "NomNiveau");
            ViewBag.SpecialiteList = new SelectList(await _accountService.GetSpecialitesAsync(), "Id", "NomSpecialite");

            return View("~/Views/Admin/UserManagement/Account.cshtml", users);
        }

        // Générer un email institutionnel à partir du nom et prénom
        private string GenerateEmailInstitutionnel(string prenom, string nom)
        {
            // Normaliser les chaînes : minuscules, sans accents, sans espaces
            string normalizedPrenom = RemoveAccents(prenom.ToLowerInvariant()).Replace(" ", "");
            string normalizedNom = RemoveAccents(nom.ToLowerInvariant()).Replace(" ", "");

            return $"{normalizedPrenom}.{normalizedNom}@isj.org";
        }

        // Supprimer les accents d'une chaîne de caractères
        private string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        // Création d'un étudiant
        [HttpPost("CreateEtudiant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEtudiant(Etudiant etudiant)
        {
            try
            {
                ModelState.Remove("MotDePasseHash");

                if (ModelState.IsValid)
                {
                    // Générer l'email institutionnel
                    etudiant.EmailInstitutionnel = GenerateEmailInstitutionnel(etudiant.Prenom, etudiant.Nom);

                    // Utiliser le mot de passe par défaut
                    string password = "Changeme@2025";

                    var newEtudiant = await _accountService.CreateEtudiantAsync(etudiant, password);
                    TempData["Success"] = $"L'étudiant {newEtudiant.Prenom} {newEtudiant.Nom} a été créé avec succès. Nom d'utilisateur: {newEtudiant.Username}, Email: {newEtudiant.EmailInstitutionnel}";
                }
                else
                {
                    // Affiche les erreurs de validation dans TempData
                    var errors = string.Join(" | ", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Les données fournies sont invalides : {errors}";
                }
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de la création de l'étudiant: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Création d'un enseignant
        [HttpPost("CreateEnseignant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnseignant(Enseignant enseignant)
        {
            try
            {

                ModelState.Remove("MotDePasseHash");
                if (ModelState.IsValid)
                {
                    // Générer l'email institutionnel
                    enseignant.EmailInstitutionnel = GenerateEmailInstitutionnel(enseignant.Prenom, enseignant.Nom);

                    // Utiliser le mot de passe par défaut
                    string password = "Changeme@2025";

                    var newEnseignant = await _accountService.CreateEnseignantAsync(enseignant, password);
                    TempData["Success"] = $"L'enseignant {newEnseignant.Prenom} {newEnseignant.Nom} a été créé avec succès. Nom d'utilisateur: {newEnseignant.Username}, Email: {newEnseignant.EmailInstitutionnel}";
                }
                else
                {
                    // Affiche les erreurs de validation dans TempData
                    var errors = string.Join(" | ", ModelState.Values
                                                    .SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Les données fournies sont invalides : {errors}";
                }
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de la création de l'enseignant: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Mise à jour d'un étudiant
        [HttpPost("UpdateEtudiant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEtudiant(Etudiant etudiant)
        {
            try
            {
                ModelState.Remove("MotDePasseHash");
                if (ModelState.IsValid)
                {
                    // Régénérer l'email institutionnel si le nom ou prénom a changé
                    etudiant.EmailInstitutionnel = GenerateEmailInstitutionnel(etudiant.Prenom, etudiant.Nom);

                    var updatedEtudiant = await _accountService.UpdateEtudiantAsync(etudiant);
                    TempData["Success"] = $"Les informations de l'étudiant {updatedEtudiant.Prenom} {updatedEtudiant.Nom} ont été mises à jour. Email institutionnel: {updatedEtudiant.EmailInstitutionnel}";
                }
                else
                {
                    // Affiche les erreurs de validation dans TempData
                    var errors = string.Join(" | ", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Les données fournies sont invalides : {errors}";
                }
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de la mise à jour de l'étudiant: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Mise à jour d'un enseignant
        [HttpPost("UpdateEnseignant")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnseignant(Enseignant enseignant)
        {
            try
            {
                ModelState.Remove("MotDePasseHash");
                if (ModelState.IsValid)
                {
                    // Régénérer l'email institutionnel si le nom ou prénom a changé
                    enseignant.EmailInstitutionnel = GenerateEmailInstitutionnel(enseignant.Prenom, enseignant.Nom);

                    var updatedEnseignant = await _accountService.UpdateEnseignantAsync(enseignant);
                    TempData["Success"] = $"Les informations de l'enseignant {updatedEnseignant.Prenom} {updatedEnseignant.Nom} ont été mises à jour. Email institutionnel: {updatedEnseignant.EmailInstitutionnel}";
                }
                else
                {
                    // Affiche les erreurs de validation dans TempData
                    var errors = string.Join(" | ", ModelState.Values
                                            .SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage));
                    TempData["Error"] = $"Les données fournies sont invalides : {errors}";
                }
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de la mise à jour de l'enseignant: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Désactivation d'un utilisateur
        [HttpPost("DeactivateUser/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilisateur non trouvé.";
                    return RedirectToAction(nameof(Index));
                }

                await _accountService.DeactivateUserAsync(userId);
                TempData["Success"] = $"L'utilisateur {user.Prenom} {user.Nom} a été désactivé.";
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de la désactivation de l'utilisateur: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Activation d'un utilisateur
        [HttpPost("ActivateUser/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilisateur non trouvé.";
                    return RedirectToAction(nameof(Index));
                }

                await _accountService.ActivateUserAsync(userId);
                TempData["Success"] = $"L'utilisateur {user.Prenom} {user.Nom} a été activé.";
            }
            catch (Exception ex)
            {
                // Inclut le message, la pile et les inner exceptions
                string innerEx = ex.InnerException != null ? $" | InnerException: {ex.InnerException.Message}" : "";
                TempData["Error"] = $"Erreur lors de l'activation de l'utilisateur: {ex.Message}{innerEx}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ResetPassword/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilisateur non trouvé.";
                    return RedirectToAction(nameof(Index));
                }

                await _accountService.ResetPasswordAsync(userId);

                TempData["Success"] =
                    $"Le mot de passe de {user.Prenom} {user.Nom} a été réinitialisé à Changeme@2026.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la réinitialisation : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Suppression logique d'un utilisateur
        [HttpPost("DeleteUser/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "Utilisateur non trouvé.";
                    return RedirectToAction(nameof(Index));
                }

                await _accountService.DeleteUserAsync(userId);

                TempData["Success"] =
                    $"L'utilisateur {user.Prenom} {user.Nom} a été supprimé définitivement.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Obtenir les détails d'un utilisateur pour l'édition
        [HttpGet("GetUserDetails/{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            var user = await _accountService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Déterminer le type d'utilisateur
            if (user is Etudiant etudiant)
            {
                return Json(new
                {
                    type = "Etudiant",
                    user = new
                    {
                        etudiant.Id,
                        etudiant.Nom,
                        etudiant.Prenom,
                        etudiant.Email,
                        etudiant.Username,
                        etudiant.Telephone,
                        etudiant.EmailInstitutionnel,
                        etudiant.CycleId,
                        etudiant.NiveauId,
                        etudiant.SpecialiteId,
                        etudiant.Active
                    }
                });
            }
            else if (user is Enseignant enseignant)
            {
                return Json(new
                {
                    type = "Enseignant",
                    user = new
                    {
                        enseignant.Id,
                        enseignant.Nom,
                        enseignant.Prenom,
                        enseignant.Email,
                        enseignant.Username,
                        enseignant.Telephone,
                        enseignant.EmailInstitutionnel,
                        enseignant.Specialite,
                        enseignant.Active
                    }
                });
            }

            return NotFound();
        }
    }
}