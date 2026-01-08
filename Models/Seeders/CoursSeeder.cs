using PGSA_Licence3.Data;
using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;

namespace PGSA_Licence3.Models.Seeders
{
    public static class CoursSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // On ne crée les cours que s'il n'y en a aucun
            if (!await context.Cours.AnyAsync())
            {
                // 1. Récupérer les entités nécessaires de la base de données
                var enseignants = await context.Enseignants.ToListAsync();
                var specialites = await context.Specialites.ToListAsync();
                var cycles = await context.Cycles.ToListAsync();
                var niveaux = await context.Niveaux.ToListAsync();

                // Vérifier que les données de base existent
                if (!enseignants.Any() || !specialites.Any() || !cycles.Any() || !niveaux.Any())
                {
                    // Si les données de base manquent, on ne peut pas peupler les cours.
                    // Assurez-vous que les autres seeders (Enseignant, Specialite, etc.) sont exécutés avant celui-ci.
                    return;
                }

                // Pour ce seeder, on se concentre sur une seule spécialité, cycle et niveaux comme dans l'exemple
                var specialiteDev = specialites.FirstOrDefault(s => s.NomSpecialite.Contains("Conception"));
                var cycleLicence = cycles.FirstOrDefault(c => c.NomCycle == "Licence");
                var niveau1 = niveaux.FirstOrDefault(n => n.NomNiveau == "1");
                var niveau2 = niveaux.FirstOrDefault(n => n.NomNiveau == "2");
                var niveau3 = niveaux.FirstOrDefault(n => n.NomNiveau == "3");

                var cours = new List<Cours>();
                var anneeActuelle = DateTime.Now.Year;

                // 2. Créer la liste de tous les cours SANS leur assigner d'enseignant pour l'instant
                // NIVEAU L1 - Semestre 1
                cours.Add(new Cours { Nom = "Introduction aux systèmes d'information", Code = "INP1011", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Architecture des ordinateurs", Code = "INP1021", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Programmation Web I", Code = "INP1031", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Enjeux de l'économie Numérique", Code = "ECP1011", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction aux algorithmes", Code = "INP1041", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Mathématiques pour l'informatique", Code = "MAP1011", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Mathématiques discrètes I", Code = "MAP1021", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Anglais Remise à niveau A2", Code = "LAP1011", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Réflexion Humaine1", Code = "HUP1011", Semestre = "S1", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // NIVEAU L1 - Semestre 2
                cours.Add(new Cours { Nom = "Initiation Programmation orientée objet I", Code = "INP1052", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Initiation à la programmation", Code = "INP1062", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction à l'Analyse Merise", Code = "INP1072", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Algorithmique et Structure de données I", Code = "INP1082", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Mathématiques discrètes II", Code = "MAP1032", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Logique pour l'Informatique", Code = "INP1092", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Communication Orale, Ecrite et audio Visual", Code = "COP1012", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Anglais niveau pratique B1", Code = "LAP1022", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Réflexion Humaine 2", Code = "HUP1022", Semestre = "S2", AnneeAcademique = anneeActuelle, NiveauId = niveau1.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // NIVEAU L2 - Semestre 3
                cours.Add(new Cours { Nom = "Algorithmique et Complexité", Code = "INP2103", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Modélisation des Systèmes d'Information(UML)", Code = "INP2113", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Algèbre linaire I", Code = "MAP2043", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Programmation Orientée Objet II", Code = "INP2123", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction aux Base de données", Code = "INP2133", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction aux Réseaux informatiques", Code = "REP2013", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Systèmes d'exploitation", Code = "INP2143", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Anglais niveau pratique B1/B2", Code = "LAP2033", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Ethique et Développement", Code = "HUP2033", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Stage découverte de l'entreprise", Code = "STP2033", Semestre = "S3", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // NIVEAU L2 - Semestre 4
                cours.Add(new Cours { Nom = "Algèbre II", Code = "MAP2054", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Langage C++ et POO", Code = "INP2154", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Programmation Web II", Code = "INP2164", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "L'entreprise et la gestion, environnement comptable, financier", Code = "ECP2024", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Économie numérique", Code = "ECP2034", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction à la sécurité informatique", Code = "INP2174", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Projets tutorés", Code = "INP2184", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Anglais Niveau pratique B2", Code = "LAP2034", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Éthique et Philosophie", Code = "HUP2044", Semestre = "S4", AnneeAcademique = anneeActuelle, NiveauId = niveau2.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // NIVEAU L3 - Semestre 5
                cours.Add(new Cours { Nom = "Programmation et administration des bases de Données (Oracle ou SQLServer)", Code = "INP3195", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Technologie .NET", Code = "INP3256", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Ingénierie du Génie Logiciel", Code = "INP3205", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Introduction au Big Data", Code = "INP3215", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Sécurité avancée des réseaux et systèmes", Code = "INP3225", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Conception et Développement d'applications pour mobiles", Code = "INP3286", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Recherche opérationnelle et aide à la décision", Code = "MAP3065", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Fondamentaux de la communication", Code = "COP3025", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Anglais pratique", Code = "LAP3045", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Sagesse et science1", Code = "HUP3055", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Stage Technique", Code = "STP2025", Semestre = "S5", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // NIVEAU L3 - Semestre 6
                cours.Add(new Cours { Nom = "JEE(Programmation par Objets avancée)", Code = "INP3246", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Gestion des Projets informatique", Code = "INP3266", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Enterprise Resource Planning (ERP)", Code = "INP3276", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Entreprenariat et Marketing", Code = "ECP3045", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Ateliers de création d'entreprise", Code = "ECP3055", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Marketing Informatique", Code = "COP3026", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Stage Professionnel", Code = "STP3036", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });
                cours.Add(new Cours { Nom = "Sagesse et science2", Code = "HUP3066", Semestre = "S6", AnneeAcademique = anneeActuelle, NiveauId = niveau3.Id, CycleId = cycleLicence.Id, SpecialiteId = specialiteDev.Id });

                // 3. Distribuer les cours de manière équitable (tourniquet)
                for (int i = 0; i < cours.Count; i++)
                {
                    // Utilise l'opérateur modulo pour boucler sur la liste des enseignants
                    var enseignantIndex = i % enseignants.Count;
                    cours[i].EnseignantId = enseignants[enseignantIndex].Id;
                }

                // 4. Ajouter tous les cours (avec un enseignant assigné) à la base de données
                context.Cours.AddRange(cours);
                await context.SaveChangesAsync();
            }
        }
    }
}