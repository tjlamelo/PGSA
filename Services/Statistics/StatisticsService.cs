using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Services.Statistics
{
    public class StatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Rapports individuels - Statistiques d'un étudiant
        public async Task<StudentStatisticsDto> GetStudentStatisticsAsync(int etudiantId, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var assiduites = await _context.Assiduites
                .Include(a => a.Seance)
                    .ThenInclude(s => s.Cours)
                .Include(a => a.Seance)
                    .ThenInclude(s => s.Groupe)
                .Where(a => a.EtudiantId == etudiantId 
                    && a.Seance.DateHeureDebut >= dateStart 
                    && a.Seance.DateHeureDebut <= dateEnd)
                .ToListAsync();

            var justifications = await _context.Justifications
                .Include(j => j.Seance)
                .Where(j => j.EtudiantId == etudiantId 
                    && j.Seance != null
                    && j.Seance.DateHeureDebut >= dateStart 
                    && j.Seance.DateHeureDebut <= dateEnd)
                .ToListAsync();

            var seancesTotal = assiduites.Count;
            var absences = assiduites.Count(a => a.Statut == StatutPresence.Absent);
            var presences = assiduites.Count(a => a.Statut == StatutPresence.Present);
            var retards = assiduites.Count(a => a.Statut == StatutPresence.Retard);

            var absencesJustifiees = justifications.Count(j => j.Statut == StatutJustification.Validee);
            var absencesInjustifiees = absences - absencesJustifiees;

            var dureeTotaleSeances = assiduites
                .Where(a => a.Seance != null)
                .Sum(a => (a.Seance.DateHeureFin - a.Seance.DateHeureDebut).TotalHours);

            var dureeAbsences = assiduites
                .Where(a => a.Statut == StatutPresence.Absent && a.Seance != null)
                .Sum(a => (a.Seance.DateHeureFin - a.Seance.DateHeureDebut).TotalHours);

            var tauxAssiduite = seancesTotal > 0 ? (presences / (double)seancesTotal) * 100 : 0;

            var detailsParCours = assiduites
                .Where(a => a.Seance?.Cours != null)
                .GroupBy(a => a.Seance.Cours)
                .Select(g => new CourseStatisticsDto
                {
                    CoursId = g.Key.Id,
                    CoursNom = g.Key.Nom,
                    CoursCode = g.Key.Code,
                    TotalSeances = g.Count(),
                    Presences = g.Count(a => a.Statut == StatutPresence.Present),
                    Absences = g.Count(a => a.Statut == StatutPresence.Absent),
                    Retards = g.Count(a => a.Statut == StatutPresence.Retard),
                    TauxAssiduite = g.Count() > 0 ? (g.Count(a => a.Statut == StatutPresence.Present) / (double)g.Count()) * 100 : 0
                })
                .ToList();

            return new StudentStatisticsDto
            {
                EtudiantId = etudiantId,
                PeriodeDebut = dateStart,
                PeriodeFin = dateEnd,
                TotalSeances = seancesTotal,
                Presences = presences,
                Absences = absences,
                Retards = retards,
                AbsencesJustifiees = absencesJustifiees,
                AbsencesInjustifiees = absencesInjustifiees,
                DureeTotaleSeances = dureeTotaleSeances,
                DureeAbsences = dureeAbsences,
                TauxAssiduite = tauxAssiduite,
                DetailsParCours = detailsParCours
            };
        }

        // Rapports collectifs - Taux d'absentéisme par cours
        public async Task<List<CourseAbsenteeismDto>> GetAbsenteeismByCourseAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var assiduites = await _context.Assiduites
                .Include(a => a.Seance)
                    .ThenInclude(s => s.Cours)
                .Where(a => a.Seance.DateHeureDebut >= dateStart && a.Seance.DateHeureDebut <= dateEnd)
                .ToListAsync();

            return assiduites
                .Where(a => a.Seance?.Cours != null)
                .GroupBy(a => a.Seance.Cours)
                .Select(g => new CourseAbsenteeismDto
                {
                    CoursId = g.Key.Id,
                    CoursNom = g.Key.Nom,
                    CoursCode = g.Key.Code,
                    TotalSeances = g.Select(a => a.SeanceId).Distinct().Count(),
                    TotalPresences = g.Count(a => a.Statut == StatutPresence.Present),
                    TotalAbsences = g.Count(a => a.Statut == StatutPresence.Absent),
                    TauxAbsenteisme = g.Count() > 0 ? (g.Count(a => a.Statut == StatutPresence.Absent) / (double)g.Count()) * 100 : 0
                })
                .OrderByDescending(c => c.TauxAbsenteisme)
                .ToList();
        }

        // Rapports collectifs - Taux d'absentéisme par groupe
        public async Task<List<GroupAbsenteeismDto>> GetAbsenteeismByGroupAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var assiduites = await _context.Assiduites
                .Include(a => a.Seance)
                    .ThenInclude(s => s.Groupe)
                .Where(a => a.Seance != null 
                    && a.Seance.GroupeId != null
                    && a.Seance.DateHeureDebut >= dateStart 
                    && a.Seance.DateHeureDebut <= dateEnd)
                .ToListAsync();

            return assiduites
                .Where(a => a.Seance?.Groupe != null)
                .GroupBy(a => a.Seance.Groupe)
                .Select(g => new GroupAbsenteeismDto
                {
                    GroupeId = g.Key.Id,
                    GroupeNom = g.Key.Nom,
                    Niveau = g.Key.Niveau,
                    Filiere = g.Key.Filiere,
                    TotalEtudiants = g.Select(a => a.EtudiantId).Distinct().Count(),
                    TotalSeances = g.Select(a => a.SeanceId).Distinct().Count(),
                    TotalPresences = g.Count(a => a.Statut == StatutPresence.Present),
                    TotalAbsences = g.Count(a => a.Statut == StatutPresence.Absent),
                    TauxAbsenteisme = g.Count() > 0 ? (g.Count(a => a.Statut == StatutPresence.Absent) / (double)g.Count()) * 100 : 0
                })
                .OrderByDescending(g => g.TauxAbsenteisme)
                .ToList();
        }

        // Rapports collectifs - Taux d'absentéisme par promotion
        public async Task<List<PromotionAbsenteeismDto>> GetAbsenteeismByPromotionAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var assiduites = await _context.Assiduites
                .Include(a => a.Etudiant)
                .Include(a => a.Seance)
                .Where(a => a.Seance.DateHeureDebut >= dateStart && a.Seance.DateHeureDebut <= dateEnd)
                .ToListAsync();

            return assiduites
                .Where(a => a.Etudiant != null)
                .GroupBy(a => new { a.Etudiant.Niveau, a.Etudiant.Filiere })
                .Select(g => new PromotionAbsenteeismDto
                {
                    Niveau = g.Key.Niveau,
                    Filiere = g.Key.Filiere,
                    TotalEtudiants = g.Select(a => a.EtudiantId).Distinct().Count(),
                    TotalSeances = g.Select(a => a.SeanceId).Distinct().Count(),
                    TotalPresences = g.Count(a => a.Statut == StatutPresence.Present),
                    TotalAbsences = g.Count(a => a.Statut == StatutPresence.Absent),
                    TauxAbsenteisme = g.Count() > 0 ? (g.Count(a => a.Statut == StatutPresence.Absent) / (double)g.Count()) * 100 : 0
                })
                .OrderByDescending(p => p.TauxAbsenteisme)
                .ToList();
        }

        // Étudiants en situation critique (taux d'assiduité < 75%)
        public async Task<List<CriticalStudentDto>> GetCriticalStudentsAsync(double seuilCritique = 75.0, DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var etudiants = await _context.Etudiants.ToListAsync();

            var result = new List<CriticalStudentDto>();

            foreach (var etudiant in etudiants)
            {
                var stats = await GetStudentStatisticsAsync(etudiant.Id, dateStart, dateEnd);
                
                if (stats.TauxAssiduite < seuilCritique && stats.TotalSeances > 0)
                {
                    result.Add(new CriticalStudentDto
                    {
                        EtudiantId = etudiant.Id,
                        Nom = etudiant.Nom,
                        Prenom = etudiant.Prenom,
                        Matricule = etudiant.Matricule,
                        Niveau = etudiant.Niveau,
                        Filiere = etudiant.Filiere,
                        TauxAssiduite = stats.TauxAssiduite,
                        TotalAbsences = stats.Absences,
                        TotalSeances = stats.TotalSeances
                    });
                }
            }

            return result.OrderBy(s => s.TauxAssiduite).ToList();
        }

        // Statistiques sur les absences enseignants
        public async Task<TeacherAbsenceStatisticsDto> GetTeacherAbsenceStatisticsAsync(DateTime? dateDebut = null, DateTime? dateFin = null)
        {
            var dateStart = dateDebut ?? DateTime.MinValue;
            var dateEnd = dateFin ?? DateTime.MaxValue;

            var signalements = await _context.SignalementsAbsenceEnseignant
                .Include(s => s.Seance)
                    .ThenInclude(se => se.Cours)
                .Include(s => s.Delegue)
                .Where(s => s.DateSignalement >= dateStart && s.DateSignalement <= dateEnd)
                .ToListAsync();

            var totalSignalements = signalements.Count;
            var signalementsEnAttente = signalements.Count(s => s.Statut == StatutSignalement.EnAttente);
            var signalementsTraites = signalements.Count(s => s.Statut == StatutSignalement.Traite);
            var signalementsRejetes = signalements.Count(s => s.Statut == StatutSignalement.Rejete);

            var signalementsParCours = signalements
                .Where(s => s.Seance?.Cours != null)
                .GroupBy(s => s.Seance.Cours)
                .Select(g => new
                {
                    CoursNom = g.Key.Nom,
                    CoursCode = g.Key.Code,
                    NombreSignalements = g.Count()
                })
                .OrderByDescending(x => x.NombreSignalements)
                .ToList();

            return new TeacherAbsenceStatisticsDto
            {
                PeriodeDebut = dateStart,
                PeriodeFin = dateEnd,
                TotalSignalements = totalSignalements,
                SignalementsEnAttente = signalementsEnAttente,
                SignalementsTraites = signalementsTraites,
                SignalementsRejetes = signalementsRejetes,
                SignalementsParCours = signalementsParCours.Select(s => new CourseSignalementDto
                {
                    CoursNom = s.CoursNom,
                    CoursCode = s.CoursCode,
                    NombreSignalements = s.NombreSignalements
                }).ToList()
            };
        }
    }

    // DTOs pour les statistiques
    public class StudentStatisticsDto
    {
        public int EtudiantId { get; set; }
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
        public int TotalSeances { get; set; }
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public int AbsencesJustifiees { get; set; }
        public int AbsencesInjustifiees { get; set; }
        public double DureeTotaleSeances { get; set; }
        public double DureeAbsences { get; set; }
        public double TauxAssiduite { get; set; }
        public List<CourseStatisticsDto> DetailsParCours { get; set; } = new();
    }

    public class CourseStatisticsDto
    {
        public int CoursId { get; set; }
        public string CoursNom { get; set; } = string.Empty;
        public string CoursCode { get; set; } = string.Empty;
        public int TotalSeances { get; set; }
        public int Presences { get; set; }
        public int Absences { get; set; }
        public int Retards { get; set; }
        public double TauxAssiduite { get; set; }
    }

    public class CourseAbsenteeismDto
    {
        public int CoursId { get; set; }
        public string CoursNom { get; set; } = string.Empty;
        public string CoursCode { get; set; } = string.Empty;
        public int TotalSeances { get; set; }
        public int TotalPresences { get; set; }
        public int TotalAbsences { get; set; }
        public double TauxAbsenteisme { get; set; }
    }

    public class GroupAbsenteeismDto
    {
        public int GroupeId { get; set; }
        public string GroupeNom { get; set; } = string.Empty;
        public string Niveau { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public int TotalEtudiants { get; set; }
        public int TotalSeances { get; set; }
        public int TotalPresences { get; set; }
        public int TotalAbsences { get; set; }
        public double TauxAbsenteisme { get; set; }
    }

    public class PromotionAbsenteeismDto
    {
        public string Niveau { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public int TotalEtudiants { get; set; }
        public int TotalSeances { get; set; }
        public int TotalPresences { get; set; }
        public int TotalAbsences { get; set; }
        public double TauxAbsenteisme { get; set; }
    }

    public class CriticalStudentDto
    {
        public int EtudiantId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Matricule { get; set; }
        public string Niveau { get; set; } = string.Empty;
        public string Filiere { get; set; } = string.Empty;
        public double TauxAssiduite { get; set; }
        public int TotalAbsences { get; set; }
        public int TotalSeances { get; set; }
    }

    public class TeacherAbsenceStatisticsDto
    {
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
        public int TotalSignalements { get; set; }
        public int SignalementsEnAttente { get; set; }
        public int SignalementsTraites { get; set; }
        public int SignalementsRejetes { get; set; }
        public List<CourseSignalementDto> SignalementsParCours { get; set; } = new();
    }

    public class CourseSignalementDto
    {
        public string CoursNom { get; set; } = string.Empty;
        public string CoursCode { get; set; } = string.Empty;
        public int NombreSignalements { get; set; }
    }
}

