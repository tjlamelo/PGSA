using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class CoursSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Cours.AnyAsync())
            {
                // Récupérer les enseignants
                var enseignants = await context.Enseignants.ToListAsync();
                
                // Récupérer les spécialités
                var specialites = await context.Specialites.ToListAsync();
                
                // Récupérer les cycles et niveaux
                var cycles = await context.Cycles.ToListAsync();
                var niveaux = await context.Niveaux.ToListAsync();

                // Trouver la spécialité "Conception et développement d'applications pour l'économie numérique"
                var specialiteDev = specialites.FirstOrDefault(s => s.NomSpecialite.Contains("Conception"));
                
                // Trouver le cycle Licence
                var cycleLicence = cycles.FirstOrDefault(c => c.NomCycle == "Licence");
                
                // Trouver les niveaux 1, 2 et 3
                var niveau1 = niveaux.FirstOrDefault(n => n.NomNiveau == "1");
                var niveau2 = niveaux.FirstOrDefault(n => n.NomNiveau == "2");
                var niveau3 = niveaux.FirstOrDefault(n => n.NomNiveau == "3");

                var cours = new List<Cours>();
                var anneeActuelle = DateTime.Now.Year;

                // Si nous n'avons pas les entités nécessaires, nous ne pouvons pas continuer
                if (specialiteDev == null || cycleLicence == null || niveau1 == null || niveau2 == null || niveau3 == null || !enseignants.Any())
                {
                    return;
                }

                // NIVEAU L1 - Semestre 1
                cours.Add(new Cours
                {
                    Nom = "Introduction aux systèmes d'information",
                    Code = "INP1011",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Architecture des ordinateurs",
                    Code = "INP1021",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Programmation Web I",
                    Code = "INP1031",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Enjeux de l'économie Numérique",
                    Code = "ECP1011",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction aux algorithmes",
                    Code = "INP1041",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Mathématiques pour l'informatique",
                    Code = "MAP1011",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Mathématiques discrètes I",
                    Code = "MAP1021",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Anglais Remise à niveau A2",
                    Code = "LAP1011",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Réflexion Humaine1",
                    Code = "HUP1011",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S1",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                // NIVEAU L1 - Semestre 2
                cours.Add(new Cours
                {
                    Nom = "Initiation Programmation orientée objet I",
                    Code = "INP1052",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Initiation à la programmation",
                    Code = "INP1062",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction à l'Analyse Merise",
                    Code = "INP1072",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Algorithmique et Structure de données I",
                    Code = "INP1082",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Mathématiques discrètes II",
                    Code = "MAP1032",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Logique pour l'Informatique",
                    Code = "INP1092",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Communication Orale, Ecrite et audio Visual",
                    Code = "COP1012",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Anglais niveau pratique B1",
                    Code = "LAP1022",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Réflexion Humaine 2",
                    Code = "HUP1022",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S2",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                // NIVEAU L2 - Semestre 3
                cours.Add(new Cours
                {
                    Nom = "Algorithmique et Complexité",
                    Code = "INP2103",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Modélisation des Systèmes d'Information(UML)",
                    Code = "INP2113",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Algèbre linaire I",
                    Code = "MAP2043",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Programmation Orientée Objet II",
                    Code = "INP2123",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction aux Base de données",
                    Code = "INP2133",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction aux Réseaux informatiques",
                    Code = "REP2013",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Systèmes d'exploitation",
                    Code = "INP2143",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Anglais niveau pratique B1/B2",
                    Code = "LAP2033",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Ethique et Développement",
                    Code = "HUP2033",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Stage découverte de l'entreprise",
                    Code = "STP2033",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S3",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                // NIVEAU L2 - Semestre 4
                cours.Add(new Cours
                {
                    Nom = "Algèbre II",
                    Code = "MAP2054",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Langage C++ et POO",
                    Code = "INP2154",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Programmation Web II",
                    Code = "INP2164",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "L'entreprise et la gestion, environnement comptable, financier",
                    Code = "ECP2024",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Économie numérique",
                    Code = "ECP2034",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction à la sécurité informatique",
                    Code = "INP2174",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Projets tutorés",
                    Code = "INP2184",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Anglais Niveau pratique B2",
                    Code = "LAP2034",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Éthique et Philosophie",
                    Code = "HUP2044",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S4",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                // NIVEAU L3 - Semestre 5
                cours.Add(new Cours
                {
                    Nom = "Programmation et administration des bases de Données (Oracle ou SQLServer)",
                    Code = "INP3195",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Technologie .NET",
                    Code = "INP3256",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Ingénierie du Génie Logiciel",
                    Code = "INP3205",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Introduction au Big Data",
                    Code = "INP3215",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Sécurité avancée des réseaux et systèmes",
                    Code = "INP3225",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Conception et Développement d'applications pour mobiles",
                    Code = "INP3286",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Recherche opérationnelle et aide à la décision",
                    Code = "MAP3065",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Fondamentaux de la communication",
                    Code = "COP3025",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Anglais pratique",
                    Code = "LAP3045",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Sagesse et science1",
                    Code = "HUP3055",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Stage Technique",
                    Code = "STP2025",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S5",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                // NIVEAU L3 - Semestre 6
                cours.Add(new Cours
                {
                    Nom = "JEE(Programmation par Objets avancée)",
                    Code = "INP3246",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Gestion des Projets informatique",
                    Code = "INP3266",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Enterprise Resource Planning (ERP)",
                    Code = "INP3276",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Entreprenariat et Marketing",
                    Code = "ECP3045",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Ateliers de création d'entreprise",
                    Code = "ECP3055",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Marketing Informatique",
                    Code = "COP3026",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[2 % enseignants.Count].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Stage Professionnel",
                    Code = "STP3036",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[0].Id
                });

                cours.Add(new Cours
                {
                    Nom = "Sagesse et science2",
                    Code = "HUP3066",
                    Filiere = specialiteDev.NomSpecialite,
                    Semestre = "S6",
                    AnneeAcademique = anneeActuelle,
                    EnseignantId = enseignants[1 % enseignants.Count].Id
                });

                context.Cours.AddRange(cours);
                await context.SaveChangesAsync();
            }
        }
    }
}