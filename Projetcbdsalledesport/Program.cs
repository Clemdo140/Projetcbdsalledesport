using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projetcbdsalledesport
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;//caracteres spéciaux    
            //Initialisation de la connexion
            string connectionString = "Server=localhost;Database=SalleDeSport;Uid=AppUser;Pwd=MdpAppUser;";
            CommandeManager manager = new CommandeManager(connectionString, 0, "");//On crée le manager pour la première fois mais comme personne n'est connecté, on met le privilège à 0 et le mot de passe est vide
            bool applicationEnCours = true;
            while (applicationEnCours) { 
                Utilisateur utilisateurConnecte = null;//null permet donc d'afficher l'écran de connexion
                manager = new CommandeManager("AppUser", 0, "MdpAppUser");

                while (utilisateurConnecte == null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("===========================================");
                    Console.WriteLine("=== BIENVENUE DANS VOTRE SALLE DE SPORT ===");
                    Console.WriteLine("===========================================");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("\n--- CONNEXION ---");
                    Console.ResetColor();
                    Console.Write("\nSaisissez votre email : ");
                    string emailSaisi = Console.ReadLine();
                    Console.Write("Saisisez votre mot de passe : ");
                    string mdpSaisi = Console.ReadLine();
                    utilisateurConnecte = Authentification.Login(emailSaisi, mdpSaisi, manager);//on l'envoie à la classe Authentification qui va vérifier si le couple email-mdp existe bien dans la base
                   
                        if (utilisateurConnecte == null)//si personne n'est connecté
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Identifiants incorrects. Réessayez.");
                            Console.ResetColor();
                            Console.ReadLine();
                        }
                        else
                        {

                        string dbUser = "AppUser";
                        string dbPass = "MdpAppUser";

                      

                        if (utilisateurConnecte.RoleUtilisateur.Fonction == "Gérant")
                        {
                            dbUser = "AdminPrincipal";
                            dbPass = "MdpAdminPrincipal";
                        }
                        else if (utilisateurConnecte.RoleUtilisateur.Fonction == "Staff")
                        {
                            dbUser = "AdminSecondaire";
                            dbPass = "MdpAdminSecondaire";
                        }
                        // Pour les membres, dbUser reste "AppUser" par défaut

                        // 2. Initialiser le manager avec le compte SQL correct
                        // On ne passe plus 'utilisateurConnecte.Nom', mais 'dbUser'
                        manager = new CommandeManager(dbUser, utilisateurConnecte.RoleUtilisateur.IdRole, dbPass);

                        Console.WriteLine($"\nBienvenue {utilisateurConnecte.Prenom} !");
                    }
                    
                }
             
                if (utilisateurConnecte.RoleUtilisateur.Fonction == "Gérant")
                {
                    MenuAdminPrincipal(manager); // Ton interface Gérant
                }
                else if (utilisateurConnecte.RoleUtilisateur.Fonction == "Staff")
                {
                    MenuAdminSecondaire(manager); // Ton interface Staff
                }
                else
                {
                    MenuMembre(manager, utilisateurConnecte); // Ton interface Membre
                }

                // --- ÉTAPE 3 : TON SYSTÈME DE SORTIE ---
                // Une fois qu'on sort d'un menu (choix 0 dans tes fonctions), on arrive ici
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("--- SESSION TERMINÉE ---");
                Console.ResetColor();
                Console.WriteLine("1) Se reconnecter avec un autre compte");
                Console.WriteLine("0) Quitter définitivement");
                Console.Write("\nChoix : ");

                string choixFinal = Console.ReadLine();
                if (choixFinal == "0")
                {
                    applicationEnCours = false; // Arrête tout
                    Console.WriteLine("Fermeture... Au revoir !");
                }
                // Si l'utilisateur tape 1, la boucle "applicationEnCours" recommence
            }
        }
        
        static void MenuEvaluation(CommandeManager manager)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- [RAPPORT STATISTIQUE - ÉVALUATION] ---");
            Console.ResetColor();
            Console.WriteLine("Génération des rapports en cours...\n");

            // 1. Nombre total de membres actifs
            int nbMembres = Convert.ToInt32(manager.ExecuterCalcul("SELECT COUNT(*) FROM Utilisateur WHERE IdRole = 3"));

            // 2. Le cours le plus populaire (celui qui a le plus de réservations)
            string sqlPop = @"SELECT T.NomCours AS nomC, 
                               COUNT(R.IdReservation) AS NbParticipants
                        FROM TypeCours T
                        LEFT JOIN Seance S ON T.IdCours = S.IdCours
                        LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance
                        GROUP BY T.NomCours";
            DataTable dtPop = manager.ExecuterLecture(sqlPop);
            string coursPop = dtPop.Rows.Count > 0 ? dtPop.Rows[0]["nomC"].ToString() : "Aucun";

            // 3. Taux d'occupation moyen (Somme inscrits / Somme capacité)
            // C'est une règle métier intéressante pour le jury
            double occupation = 0;
            try
            {
                string sqlOcc = "SELECT (COUNT(R.IdReservation) * 100.0 / SUM(S.CapaciteMax)) FROM Seance S LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance";
                occupation = Convert.ToDouble(manager.ExecuterCalcul(sqlOcc));
            }
            catch { occupation = 0; }

            // AFFICHAGE DES RESULTATS
            Console.WriteLine($"* Nombre de membres inscrits : {nbMembres}");
            Console.WriteLine($"* Cours le plus suivi : {coursPop}");
            Console.WriteLine($"* Taux de remplissage global : {Math.Round(occupation, 2)}%");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n--- TOP 3 COACHS LES PLUS SUIVIS ---");
            Console.ResetColor();
            string sqlCoachs = @"SELECT C.nom, COUNT(R.IdReservation) as Nb 
                         FROM Coach C 
                         JOIN Seance S ON C.IdCoach = S.IdCoach 
                         JOIN Reservation R ON S.IdSeance = R.IdSeance 
                         GROUP BY C.nom ORDER BY Nb DESC LIMIT 3";
            DataTable dtC = manager.ExecuterLecture(sqlCoachs);
            foreach (DataRow r in dtC.Rows) Console.WriteLine($"- {r["nom"]} : {r["Nb"]} réservations");

            Console.WriteLine("\nAppuyez sur une touche pour quitter le rapport...");
            Console.ReadKey();
        }
        static void MenuAdminPrincipal(CommandeManager manager)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("--- [INTERFACE GÉRANT - PRIVILÈGE TOTAL] ---");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("\nVeuillez choisir une option :");
                Console.WriteLine("0) Retour au menu de déconnexion");
                Console.WriteLine("1) Consulter les Rapports (Interface Évaluation)"); 
                Console.WriteLine("2) Gestion complète des Membres");
                Console.WriteLine("3) Gestion des Coachs");
                Console.WriteLine("4) Gestion des Cours et Tarifs");
                Console.WriteLine("5) Gestion des Salles");

                Console.Write("\nVotre choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        // On appelle la fonction que nous venons de créer
                        MenuEvaluation(manager);
                        break;

                    case "2":
                        // Gestion des membres
                        bool retourMembres = false;
                        while (!retourMembres)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("--- [GESTION DES MEMBRES] ---");
                            Console.ResetColor();
                            Console.WriteLine("0) Retour au menu principal");
                            Console.WriteLine("1) Voir la liste des membres");
                            Console.WriteLine("2) Ajouter un nouveau membre");
                            Console.WriteLine("3) Supprimer un membre");

                            Console.Write("\nVotre choix : ");

                            string sousChoix = Console.ReadLine();

                            switch (sousChoix)
                            {
                                case "1": // VOIR LES MEMBRES
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES MEMBRES ENREGISTRÉS ---");
                                    Console.ResetColor();
                                    // On récupère les utilisateurs ayant le rôle de 'Membre' (IdRole = 3 d'après ton SQL)
                                    DataTable dt = manager.ExecuterLecture("SELECT nom, prenom, email, telephone FROM Utilisateur WHERE IdRole = 3");

                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            Console.WriteLine($"- {row["nom"].ToString().ToUpper()} {row["prenom"]} | Email: {row["email"]} | Tel: {row["telephone"]}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucun membre trouvé dans la base.");
                                    }
                                    Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                                    Console.ReadKey();
                                    break;

                                case "2": // AJOUTER UN MEMBRE
                                case "inscription": // Ou votre case correspondant à l'inscription
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- FORMULAIRE D'INSCRIPTION ---");
                                    Console.ResetColor();

                                    // Utilisation d'une fonction interne pour valider que ce n'est pas vide
                                    string nom = SaisirChampObligatoire("Nom");
                                    string prenom = SaisirChampObligatoire("Prénom");
                                    string email = SaisirChampObligatoire("Email");
                                    string mdpMembre = SaisirChampObligatoire("Mot de passe");
                                    string tel = SaisirChampObligatoire("Téléphone");

                                    // Requête SQL sécurisée
                                    string sqlPerso = $"INSERT INTO Utilisateur (nom, prenom, email, motDePasse, telephone, IdRole) " +
                                                      $"VALUES ('{nom}', '{prenom}', '{email}', '{mdpMembre}', '{tel}', 3)";

                                    manager.ExecuterAction(sqlPerso);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\n✅ Membre ajouté avec succès !");
                                    Console.ResetColor();
                                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": //SUPPRIMER UN MEMBRE
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRESSION D'UN MEMBRE ---");
                                    Console.ResetColor();

                                    Console.Write("Entrez l'Email du membre à supprimer : ");
                                    string emailSuppr = Console.ReadLine()?.Trim();

                                    // 1. On vérifie d'abord si la saisie est vide
                                    if (string.IsNullOrEmpty(emailSuppr))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("⚠️ Erreur : Vous devez saisir un email pour effectuer une recherche.");
                                        Console.ResetColor();
                                        Thread.Sleep(2000);
                                        break; // On sort du case pour revenir au menu
                                    }

                                    // 2. Récupérer l'ID de l'utilisateur à partir de l'email
                                    string sqlGetId = $"SELECT IdUtilisateur FROM Utilisateur WHERE email = '{emailSuppr}'";
                                    DataTable dtU = manager.ExecuterLecture(sqlGetId);

                                    // 3. On vérifie si l'utilisateur existe
                                    if (dtU.Rows.Count > 0)
                                    {
                                        int idAuteur = Convert.ToInt32(dtU.Rows[0]["IdUtilisateur"]);
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.Write($"\nÊtes-vous sûr de vouloir supprimer {emailSuppr} ?\n(Taper 'O' pour confirmer ou n'importe quelle touche pour annuler) : ");
                                        Console.ResetColor();

                                        if (Console.ReadLine().ToUpper() == "O")
                                        {
                                            Console.Write("\nSuppression en cours...");
                                            Thread.Sleep(1000);

                                            // Suppression des données liées (Enfants)
                                            manager.ExecuterAction($"DELETE FROM Reservation WHERE IdUtilisateur = {idAuteur}");
                                            manager.ExecuterAction($"DELETE FROM Souscription WHERE IdUtilisateur = {idAuteur}");

                                            // Suppression de l'utilisateur (Parent)
                                            manager.ExecuterAction($"DELETE FROM Utilisateur WHERE IdUtilisateur = {idAuteur}");

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\n✅ Membre supprimé avec succès.");
                                            Console.ResetColor();
                                            Thread.Sleep(1500);
                                        }
                                        else
                                        {
                                            Console.WriteLine("\nOpération annulée.");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\n❌ Aucun utilisateur trouvé avec l'email : {emailSuppr}");
                                        Console.ResetColor();
                                        Thread.Sleep(2000);
                                    }
                                    break;

                                case "0":
                                    retourMembres = true;
                                    break;
                            }
                        }
                        break;

                    case "3":
                        // Ici pour la gestion des coachs
                        bool retourCoachs = false;
                        while (!retourCoachs)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("--- [GESTION DES COACHS] ---");
                            Console.ResetColor();
                            Console.WriteLine("0) Retour au menu principal");
                            Console.WriteLine("1) Voir la liste des coachs");
                            Console.WriteLine("2) Ajouter un nouveau coach");
                            Console.WriteLine("3) Supprimer un coach");
                          
                            Console.Write("\nVotre choix : ");

                            string sousChoix = Console.ReadLine();

                            switch (sousChoix)
                            {
                                case "1": // VOIR LES COACHS
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES COACHS ---");
                                    Console.ResetColor();
                                    // On récupère les colonnes demandées par ton sujet
                                    DataTable dt = manager.ExecuterLecture("SELECT nom, prenom, specialite, formation, telephone, email FROM Coach");

                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            Console.WriteLine($"- {row["prenom"]} {row["nom"].ToString().ToUpper()}");
                                            Console.WriteLine($"  Spécialité : {row["specialite"]} | Formation : {row["formation"]} | Tel: {row["telephone"]} | Email : {row["email"]}");
                                            Console.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucun coach enregistré pour le moment.");
                                    }
                                    Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                                    Console.ReadKey();
                                    break;

                                case "2": // AJOUTER UN COACH
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- ENREGISTREMENT D'UN COACH ---");
                                    Console.ResetColor();

                                    // Utilisation de la méthode de validation pour chaque champ
                                    string n = SaisirChampObligatoire("Nom");
                                    string p = SaisirChampObligatoire("Prénom");
                                    string s = SaisirChampObligatoire("Spécialité");
                                    string f = SaisirChampObligatoire("Formation");
                                    string t = SaisirChampObligatoire("Numéro de téléphone");
                                    string e = SaisirChampObligatoire("Email");

                                    // Requête d'insertion dans la table Coach
                                    string sqlAdd = $"INSERT INTO Coach (nom, prenom, specialite, formation, telephone, email) " +
                                                    $"VALUES ('{n}', '{p}', '{s}', '{f}', '{t}', '{e}')";

                                    manager.ExecuterAction(sqlAdd);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\n✅ Le coach a été ajouté avec succès !");
                                    Console.ResetColor();
                                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": // SUPPRIMER UN COACH
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRESSION D'UN COACH ---");
                                    Console.ResetColor();

                                    // 1. Affichage de la liste des coachs
                                    DataTable dtCoachs = manager.ExecuterLecture("SELECT IdCoach, nom, prenom, email FROM Coach");
                                    Console.WriteLine("Liste des coachs enregistrés :");
                                    foreach (DataRow r in dtCoachs.Rows)
                                    {
                                        Console.WriteLine($"  [{r["IdCoach"]}] {r["prenom"]} {r["nom"]} ({r["email"]})");
                                    }

                                    Console.WriteLine();
                                    string idCSuppr = SaisirChampObligatoire("Entrez l'ID du coach à supprimer");

                                    if (int.TryParse(idCSuppr, out int idC))
                                    {
                                        object countC = manager.ExecuterCalcul($"SELECT COUNT(*) FROM Coach WHERE IdCoach = {idC}");
                                        if (Convert.ToInt32(countC) > 0)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.Write($"\n❓ Confirmez-vous la suppression du coach n°{idC} ? (O/N) : ");
                                            Console.ResetColor();

                                            if (Console.ReadLine().ToUpper() == "O")
                                            {
                                                // A. Supprimer les séances (Enfants) d'abord
                                                manager.ExecuterAction($"DELETE FROM Seance WHERE IdCoach = {idC}");

                                                // B. Supprimer le coach (Parent)
                                                manager.ExecuterAction($"DELETE FROM Coach WHERE IdCoach = {idC}");

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("\n✅ Coach supprimé avec succès.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("\n❌ Cet ID n'existe pas.");
                                        }
                                    }
                                    Thread.Sleep(2000);
                                    break;
                                case "0":
                                    retourCoachs = true;
                                    break;
                            }
                        }
                        break;
                    case "4":
                        bool retourCours = false;
                        while (!retourCours)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("--- [GESTION DES COURS ET PLANNING] ---");
                            Console.ResetColor();
                            Console.WriteLine("0) Retour au menu principal");
                            Console.WriteLine("1) Voir le planning des cours");
                            Console.WriteLine("2) Enregistrer un nouveau cours");
                            Console.WriteLine("3) Supprimer (Annuler) un cours");
                           
                            Console.Write("\nVotre choix : ");

                            string sousChoix = Console.ReadLine();

                            switch (sousChoix)
                            {
                                case "1": // VOIR LES COURS
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;    
                                    Console.WriteLine("--- PLANNING ACTUEL DÉTAILLÉ ---");
                                    Console.ResetColor();

                                    // Requête avec jointures vers TypeCours, Coach et Salle + comptage des places
                                    string sqlPlanningComplet = @"
                                        SELECT 
                                            S.IdSeance, 
                                            T.NomCours AS nomC, 
                                            S.DateDebut, 
                                            S.CapaciteMax,
                                            C.prenom AS PrenomCoach, C.nom AS NomCoach,
                                            Sa.nomSalle,
                                            (SELECT COUNT(*) FROM Reservation R WHERE R.IdSeance = S.IdSeance) AS NbInscrits
                                        FROM Seance S 
                                        JOIN TypeCours T ON S.IdCours = T.IdCours
                                        JOIN Coach C ON S.IdCoach = C.IdCoach
                                        JOIN Salle Sa ON S.idSalle = Sa.idSalle
                                        ORDER BY S.DateDebut";

                                    DataTable dt = manager.ExecuterLecture(sqlPlanningComplet);

                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            int max = Convert.ToInt32(row["CapaciteMax"]);
                                            int restantes = max - Convert.ToInt32(row["NbInscrits"]);

                                            Console.WriteLine($"ID: {row["IdSeance"]} | Cours: {row["nomC"]}");
                                            Console.WriteLine($"   Salle    : {row["nomSalle"]}");
                                            Console.WriteLine($"   Coach    : {row["PrenomCoach"]} {row["NomCoach"]}");
                                            Console.WriteLine($"   Horaire  : {row["DateDebut"]}");
                                            Console.WriteLine($"   Places   : {restantes} restantes sur {max}");
                                            Console.WriteLine("---------------------------------------------------");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucune séance de prévue.");
                                    }

                                    Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                                    Console.ReadKey();
                                    break;

                                case "2": // ENREGISTRER UN COURS (SÉANCE)
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- CRÉATION D'UNE NOUVELLE SÉANCE ---");
                                    Console.ResetColor();

                                    // 1. Saisie du nom du cours (ex: Yoga)
                                    string nomCours = SaisirChampObligatoire("Nom du cours (ex: Yoga)");

                                    // 2. Récupération de l'ID du cours correspondant
                                    string sqlIdC = $"SELECT IdCours FROM TypeCours WHERE NomCours = '{nomCours}'";
                                    DataTable dtCours = manager.ExecuterLecture(sqlIdC);

                                    if (dtCours.Rows.Count == 0)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\n❌ Erreur : Le type de cours '{nomCours}' n'existe pas.");
                                        Console.ResetColor();
                                        Console.ReadKey();
                                        break;
                                    }
                                    int idCoursValide = Convert.ToInt32(dtCours.Rows[0]["IdCours"]);

                                    // 3. Saisie de la durée avec validation numérique (Évite l'erreur Out of Range)
                                    int dureeMinutes = 0;
                                    bool dureeValide = false;
                                    while (!dureeValide)
                                    {
                                        string saisieDuree = SaisirChampObligatoire("Durée du cours (en minutes, ex: 60)");
                                        if (int.TryParse(saisieDuree, out dureeMinutes) && dureeMinutes > 0 && dureeMinutes < 300)
                                        {
                                            dureeValide = true;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("   ⚠️ Erreur : Veuillez saisir un nombre entre 1 et 300.");
                                            Console.ResetColor();
                                        }
                                    }

                                    // 4. Saisie et validation de l'horaire (doit être dans le futur)
                                    string hor = "";
                                    DateTime dateSaisie = DateTime.Now;
                                    bool dateValide = false;
                                    while (!dateValide)
                                    {
                                        hor = SaisirChampObligatoire("Horaire (Format: AAAA-MM-JJ HH:MM)");
                                        if (DateTime.TryParse(hor, out dateSaisie))
                                        {
                                            if (dateSaisie > DateTime.Now) dateValide = true;
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("   ⚠️ Erreur : La date doit être dans le futur.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("   ⚠️ Erreur : Format invalide (Ex: 2026-05-12 14:00)");
                                            Console.ResetColor();
                                        }
                                    }

                                    // 5. Choix du Coach
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("\n--- LISTE DES COACHS ---");
                                    Console.ResetColor();
                                    DataTable dtCo = manager.ExecuterLecture("SELECT IdCoach, nom, prenom FROM Coach");
                                    foreach (DataRow r in dtCo.Rows) Console.WriteLine($" ID: {r["IdCoach"]} | {r["prenom"]} {r["nom"]}");
                                    string idC = SaisirChampObligatoire("Entrez l'ID du coach choisi");

                                    // 6. Choix de la Salle
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("\n--- LISTE DES SALLES ---");
                                    Console.ResetColor();
                                    DataTable dtSa = manager.ExecuterLecture("SELECT idSalle, nomSalle, capacite FROM Salle");
                                    foreach (DataRow r in dtSa.Rows) Console.WriteLine($" ID: {r["idSalle"]} | {r["nomSalle"]} (Capacité: {r["capacite"]})");
                                    string idS = SaisirChampObligatoire("Entrez l'ID de la salle choisie");

                                    try
                                    {
                                        // 7. Récupération de la capacité de la salle
                                        string sqlCap = $"SELECT capacite FROM Salle WHERE idSalle = {idS}";
                                        object resultCap = manager.ExecuterCalcul(sqlCap);

                                        if (resultCap != null)
                                        {
                                            int capMax = Convert.ToInt32(resultCap);

                                            // 8. INSERTION FINALE
                                            // Vérifie bien que tes colonnes SQL sont : IdCours, DateDebut, IdCoach, idSalle, CapaciteMax, DureeMinutes
                                            string sqlIns = $"INSERT INTO Seance (IdCours, DateDebut, IdCoach, idSalle, CapaciteMax, DureeMinutes) " +
                                                            $"VALUES ({idCoursValide}, '{dateSaisie:yyyy-MM-dd HH:mm:ss}', {idC}, {idS}, {capMax}, {dureeMinutes})";

                                            manager.ExecuterAction(sqlIns);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\n✅ Séance ajoutée au planning avec succès !");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.WriteLine("\n❌ Erreur : Salle introuvable.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\n❌ Erreur MySQL : " + ex.Message);
                                        Console.ResetColor();
                                    }

                                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": // SUPPRIMER UN COURS
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- ANNULATION D'UN COURS ---");
                                    Console.ResetColor();
                                    Console.Write("Entrez l'ID du cours à supprimer : ");
                                    string idSuppr = Console.ReadLine();

                                    // On supprime d'abord les réservations liées (sinon erreur SQL à cause de la clé étrangère)
                                    manager.ExecuterAction($"DELETE FROM Reservation WHERE IdSeance = {idSuppr}");
                                    // Puis on supprime la séance
                                    manager.ExecuterAction($"DELETE FROM Seance WHERE IdSeance = {idSuppr}");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\nLe cours et ses réservations ont été supprimés.");
                                    Console.ResetColor();
                                    Console.ReadKey();
                                    break;

                                case "0":
                                    retourCours = true;
                                    break;
                            }
                        }
                        break;

                    case "5":
                        bool retourSalles = false;
                        while (!retourSalles)
                        {
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("--- [GESTION DES SALLES] ---");
                            Console.ResetColor();
                            Console.WriteLine("0) Retour au menu principal");
                            Console.WriteLine("1) Voir la liste des salles");
                            Console.WriteLine("2) Ajouter une nouvelle salle");
                            Console.WriteLine("3) Supprimer une salle");
                          
                            Console.Write("\nVotre choix : ");

                            string sousChoix = Console.ReadLine();

                            switch (sousChoix)
                            {
                                case "1": // VOIR LES SALLES
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES SALLES ---");
                                    Console.ResetColor();

                                    // On récupère toutes les colonnes de la table Salle
                                    DataTable dtSalles = manager.ExecuterLecture("SELECT * FROM Salle");

                                    if (dtSalles.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dtSalles.Rows)
                                        {
                                            // Correction : On utilise "nomSalle" au lieu de "nom"
                                            Console.WriteLine($"- ID: {row["idSalle"]} | Nom: {row["nomSalle"]} | Capacité : {row["capacite"]} personnes");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucune salle enregistrée.");
                                    }
                                    Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                                    Console.ReadKey();
                                    break;

                                case "2": // AJOUTER UNE SALLE
                                 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- AJOUTER UNE NOUVELLE SALLE ---");
                                    Console.ResetColor();

                                    // 1. Saisie obligatoire du nom de la salle
                                    string nomS = SaisirChampObligatoire("Nom de la salle (ex: Muscu B)");

                                    // 2. Saisie et validation de la capacité (doit être un nombre positif)
                                    int capS = 0;
                                    bool capValide = false;
                                    while (!capValide)
                                    {
                                        string saisieCap = SaisirChampObligatoire("Capacité maximale");
                                        if (int.TryParse(saisieCap, out capS) && capS > 0)
                                        {
                                            capValide = true;
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("   ⚠️ Erreur : Veuillez saisir un nombre entier positif.");
                                            Console.ResetColor();
                                        }
                                    }

                                    try
                                    {
                                        // 3. Requête d'insertion (CORRECTION : nomSalle au lieu de nom)
                                        string sqlAddSalle = $"INSERT INTO Salle (nomSalle, capacite) VALUES ('{nomS}', {capS})";

                                        manager.ExecuterAction(sqlAddSalle);

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("\n✅ La salle a été ajoutée avec succès !");
                                        Console.ResetColor();
                                        Thread.Sleep(1500);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\n❌ Erreur MySQL : " + ex.Message);
                                        Console.ResetColor();
                                    }

                                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": // SUPPRIMER UNE SALLE
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRIMER UNE SALLE (PAR ID) ---");
                                    Console.ResetColor();

                                    // 1. Afficher la liste des salles pour que l'utilisateur voit les IDs
                                    DataTable dtSalle = manager.ExecuterLecture("SELECT idSalle, nomSalle FROM Salle");
                                    Console.WriteLine("Liste des salles disponibles :");
                                    foreach (DataRow r in dtSalle.Rows)
                                    {
                                        Console.WriteLine($"  [{r["idSalle"]}] - {r["nomSalle"]}");
                                    }

                                    // 2. Saisie de l'ID avec validation numérique
                                    Console.WriteLine();
                                    string saisieId = SaisirChampObligatoire("Entrez l'ID de la salle à supprimer");

                                    if (int.TryParse(saisieId, out int idS))
                                    {
                                        // 3. Vérification si l'ID existe
                                        object existe = manager.ExecuterCalcul($"SELECT COUNT(*) FROM Salle WHERE idSalle = {idS}");
                                        if (Convert.ToInt32(existe) > 0)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.Write($"\n⚠️ Confirmez-vous la suppression de la salle n°{idS} et de ses séances ? (O/N) : ");
                                            Console.ResetColor();

                                            if (Console.ReadLine().ToUpper() == "O")
                                            {
                                                // ORDRE CRITIQUE : Supprimer les dépendances d'abord
                                                manager.ExecuterAction($"DELETE FROM Seance WHERE idSalle = {idS}");
                                                manager.ExecuterAction($"DELETE FROM Salle WHERE idSalle = {idS}");

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("\n✅ Salle supprimée avec succès.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\n❌ Cet ID n'existe pas.");
                                            Console.ResetColor();
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\n❌ Erreur : L'ID doit être un nombre.");
                                        Console.ResetColor();
                                    }

                                    Thread.Sleep(2000);
                                    break;

                                case "0":
                                    retourSalles = true;
                                    break;
                            }
                        }
                        break;

                    case "0":
                        retour = true;
                        break;

                    default:
                        Console.WriteLine("Choix invalide.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void MenuAdminSecondaire(CommandeManager manager)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n--- [INTERFACE STAFF - GESTION DES OPÉRATIONS] ---");
                Console.ResetColor();
                Console.WriteLine("0) Retour / Déconnexion");
                Console.WriteLine("1) Valider une inscription (Adhésion)");
                Console.WriteLine("2) Voir la liste des membres"); // Option utile pour le staff
                Console.WriteLine("3) Consulter le planning");
              
                Console.Write("\nVotre choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        ValiderInscription(manager);
                        break;
                    case "2":
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- LISTE DES MEMBRES ET ABONNEMENTS ---");
                        Console.ResetColor();

                        // Requête avec jointures : 
                        // LEFT JOIN est important ici pour afficher même les membres qui n'ont pas encore d'abonnement
                        string sqlMembresAdh = @"SELECT U.nom, U.prenom, U.email, T.libelle as NomForfait, S.statut
                             FROM Utilisateur U
                             LEFT JOIN Souscription S ON U.IdUtilisateur = S.IdUtilisateur
                             LEFT JOIN TypeAdhesion T ON S.IdTypeAdhesion = T.IdTypeAdhesion
                             WHERE U.IdRole = 3"; // On ne filtre que les membres

                        DataTable dtMembres = manager.ExecuterLecture(sqlMembresAdh);

                        if (dtMembres.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtMembres.Rows)
                            {
                                // Vérification si le forfait est nul (cas où le membre n'a pas encore choisi d'abonnement)
                                string forfait = row["NomForfait"] == DBNull.Value ? "Aucun" : row["NomForfait"].ToString();
                                string statut = row["statut"] == DBNull.Value ? "N/A" : row["statut"].ToString();

                                Console.WriteLine($"- {row["nom"].ToString().ToUpper()} {row["prenom"]} | Email: {row["email"]}");
                                Console.WriteLine($"  Abonnement: {forfait} (Statut: {statut})");
                                Console.WriteLine("------------------------------------------------------------------------");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Aucun membre enregistré.");
                        }
                        Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                        Console.ReadKey();
                        break;

                    case "3":
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- PLANNING DES SÉANCES ---");
                        Console.ResetColor();

                        // On fait une jointure pour récupérer le nom du cours depuis la table TypeCours
                        string sqlPlanning = @"SELECT T.NomCours, S.DateDebut 
                          FROM Seance S 
                          JOIN TypeCours T ON S.IdCours = T.IdCours";

                        DataTable dtP = manager.ExecuterLecture(sqlPlanning);

                        if (dtP.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtP.Rows)
                            {
                                // Attention : utilisez "NomCours" (majuscules/minuscules) tel quel dans le SELECT
                                Console.WriteLine($"- {r["NomCours"]} prévu le : {r["DateDebut"]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Aucune séance au planning.");
                        }
                        Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                        Console.ReadKey();
                        break;
                    case "0":
                        retour = true;
                        break;
                }
            }
        }
        static void MenuMembre(CommandeManager manager, Utilisateur membre)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"--- ESPACE MEMBRE (Connecté en tant que : {membre.Prenom} {membre.Nom}) ---");
                Console.ResetColor();
                Console.WriteLine("0) Retour (Déconnexion)");
                Console.WriteLine("1) Voir le planning des cours (Détails, Intensité, Niveau)");
                Console.WriteLine("2) Réserver un cours (Vérification des places)");
                Console.WriteLine("3) Voir mes réservations / Annuler");
                Console.WriteLine("4) Souscrire à un abonnement");
                Console.Write("\nVotre choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1": // VOIR LES COURS DÉTAILLÉS
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- PLANNING DES COURS ---");
                        Console.ResetColor();
                        // Jointure pour avoir les infos textuelles demandées par le sujet
                        string sqlPlanning = @"SELECT S.IdSeance, T.NomCours, T.Description, T.Intensite, S.DateDebut 
                                              FROM Seance S 
                                              JOIN TypeCours T ON S.IdCours = T.IdCours";
                        DataTable dtP = manager.ExecuterLecture(sqlPlanning);

                        foreach (DataRow r in dtP.Rows)
                        {
                            Console.WriteLine($"[{r["IdSeance"]}] {r["NomCours"]} - {r["DateDebut"]}");
                            Console.WriteLine($"    Description : {r["Description"]}");
                            Console.WriteLine($"    Intensité : {r["Intensite"]} ");
                            Console.WriteLine("---------------------------------------------------------");
                        }
                        Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                        Console.ReadKey();
                        break;

                    case "2": // RÉSERVER AVEC RÈGLE MÉTIER
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- RÉSERVER UN COURS ---");
                        Console.ResetColor();

                        // 1. VÉRIFICATION DE L'ABONNEMENT (Le membre a-t-il une souscription validée ?)
                        string sqlAbo = $"SELECT statut FROM Souscription WHERE IdUtilisateur = {membre.IdUtilisateur} AND statut = 'Validée'";
                        DataTable dtAbo = manager.ExecuterLecture(sqlAbo);

                        if (dtAbo.Rows.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n❌ Accès refusé : Vous devez avoir un abonnement validé par le staff pour réserver.");
                            Console.ResetColor();
                            Console.WriteLine("Consultez l'accueil pour régulariser votre situation.");
                            Console.ReadKey();
                            break;
                        }

                        // 2. AFFICHAGE DES COURS AVEC PLACES RESTANTES
                        string sqlDispo = @"SELECT S.IdSeance, T.NomCours, S.DateDebut, S.CapaciteMax,
                        (SELECT COUNT(*) FROM Reservation R WHERE R.IdSeance = S.IdSeance) as Inscrits
                        FROM Seance S
                        JOIN TypeCours T ON S.IdCours = T.IdCours";

                        DataTable dtDispo = manager.ExecuterLecture(sqlDispo);

                        foreach (DataRow r in dtDispo.Rows)
                        {
                            int placesLibres = Convert.ToInt32(r["CapaciteMax"]) - Convert.ToInt32(r["Inscrits"]);
                            Console.WriteLine($"ID: {r["IdSeance"]} | {r["NomCours"]} le {r["DateDebut"]} | Libres: {placesLibres}/{r["CapaciteMax"]}");
                        }

                        Console.Write("\nEntrez l'ID de la séance choisie : ");
                        if (int.TryParse(Console.ReadLine(), out int idChoisi))
                        {
                            DataRow[] coursSelect = dtDispo.Select($"IdSeance = {idChoisi}");

                            if (coursSelect.Length > 0)
                            {
                                // --- A. VÉRIFICATION : DOUBLON (Déjà réservé ?) ---
                                string sqlDoublon = $@"SELECT COUNT(*) FROM Reservation 
                                   WHERE IdUtilisateur = {membre.IdUtilisateur} AND IdSeance = {idChoisi}";
                                int existeDeja = Convert.ToInt32(manager.ExecuterLecture(sqlDoublon).Rows[0][0]);

                                if (existeDeja > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\n❌ Vous avez déjà réservé cette séance !");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    // --- B. VÉRIFICATION : CONFLIT D'HORAIRE (Un autre cours à la même heure ?) ---
                                    string heureVoulue = Convert.ToDateTime(coursSelect[0]["DateDebut"]).ToString("yyyy-MM-dd HH:mm:ss");
                                    string sqlConflit = $@"SELECT COUNT(*) FROM Reservation R
                                       JOIN Seance S ON R.IdSeance = S.IdSeance
                                       WHERE R.IdUtilisateur = {membre.IdUtilisateur} 
                                       AND S.DateDebut = '{heureVoulue}'";
                                    int conflit = Convert.ToInt32(manager.ExecuterLecture(sqlConflit).Rows[0][0]);

                                    if (conflit > 0)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\n❌ Conflit d'horaire : Vous avez déjà une réservation à ce moment-là !");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        // --- C. VÉRIFICATION : CAPACITÉ (Reste-t-il des places ?) ---
                                        int inscrits = Convert.ToInt32(coursSelect[0]["Inscrits"]);
                                        int max = Convert.ToInt32(coursSelect[0]["CapaciteMax"]);

                                        if (inscrits < max)
                                        {
                                            string sqlReserver = $"INSERT INTO Reservation (IdUtilisateur, IdSeance, dateReservation) " +
                                                                 $"VALUES ({membre.IdUtilisateur}, {idChoisi}, NOW())";

                                            manager.ExecuterAction(sqlReserver);
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\n✅ Réservation confirmée !");
                                            Console.ResetColor();
                                        }
                                        else

                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\n❌ Ce cours est complet.");
                                            Console.ResetColor();
                                        }
                                    }
                                }
                            }
                            else Console.WriteLine("\n❌ ID de séance invalide.");
                        }

                        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;

                    case "3": // HISTORIQUE ET ANNULATION
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"--- PROFIL DE {membre.Prenom.ToUpper()} ---");
                        Console.ResetColor();

                        // 1. AFFICHAGE DE L'ABONNEMENT
                        string sqlAbonnement = "SELECT s.dateDebut, s.dateFin, t.libelle, s.statut " +
                                               "FROM Souscription s " +
                                               "JOIN TypeAdhesion t ON s.IdTypeAdhesion = t.IdTypeAdhesion " +
                                               $"WHERE s.IdUtilisateur = {membre.IdUtilisateur}";

                        DataTable dtAbon = manager.ExecuterLecture(sqlAbonnement);

                        Console.WriteLine("\n[ STATUT DE L'ABONNEMENT ]");
                        if (dtAbon.Rows.Count > 0)
                        {
                            DataRow abo = dtAbon.Rows[0];
                            DateTime dateFin = Convert.ToDateTime(abo["dateFin"]);
                            string statutBase = abo["statut"].ToString();

                            Console.Write($"Offre : {abo["libelle"]} | Expire le : {dateFin:dd/MM/yyyy}");

                            // Logique de couleur selon la date et le statut
                            if (statutBase != "Validée")
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(" (EN ATTENTE DE PAIEMENT)");
                            }
                            else if (dateFin < DateTime.Now)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" (EXPIRÉ)");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(" (ACTIF)");
                            }
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("Aucun abonnement enregistré.");
                        }

                        // 2. AFFICHAGE DES RÉSERVATIONS
                        Console.WriteLine("\n[ MES SÉANCES RÉSERVÉES ]");
                        string sqlRes = "SELECT t.NomCours, s.DateDebut, sa.nomSalle " +
                                        "FROM Reservation r " +
                                        "JOIN Seance s ON r.IdSeance = s.IdSeance " +
                                        "JOIN TypeCours t ON s.IdCours = t.IdCours " +
                                        "JOIN Salle sa ON s.idSalle = sa.idSalle " +
                                        $"WHERE r.IdUtilisateur = {membre.IdUtilisateur} " +
                                        "ORDER BY s.DateDebut ASC";

                        DataTable dtRes = manager.ExecuterLecture(sqlRes);

                        if (dtRes.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtRes.Rows)
                            {
                                DateTime d = Convert.ToDateTime(r["DateDebut"]);
                                Console.WriteLine($"- {r["NomCours"]} le {d:dd/MM à HH:mm} (Salle : {r["nomSalle"]})");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Vous n'avez aucune réservation à venir.");
                        }

                        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;

                    case "4": // ADHÉRER À UN ABONNEMENT
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("--- SOUSCRIRE À UN ABONNEMENT ---");
                        Console.ResetColor();
                        Console.WriteLine("En souscrivant à un nouvel abonnement, votre ancien abonnement sera remplacé et vos séances réservées annulées");
                        // 1. On affiche les offres disponibles en base de données
                        DataTable dtTypes = manager.ExecuterLecture("SELECT * FROM TypeAdhesion");
                        Console.WriteLine("\nNos offres disponibles :");
                        foreach (DataRow r in dtTypes.Rows)
                        {
                            Console.WriteLine($"{r["IdTypeAdhesion"]}. {r["libelle"]} ({r["prix"]}€/mois)");
                        }

                        Console.Write("\nChoisissez le numéro de l'offre : ");
                        string choixAdh = Console.ReadLine();

                        if (!string.IsNullOrEmpty(choixAdh))
                        {
                            // 2. NETTOYAGE : On supprime l'ancien abonnement (expiré ou en attente) 
                            // pour que le nouveau prenne la place et ne soit pas pollué par l'ancienne date.
                            string sqlNettoyage = $"DELETE FROM Souscription WHERE IdUtilisateur = {membre.IdUtilisateur}";
                            manager.ExecuterAction(sqlNettoyage);

                            // 3. CALCUL DES DATES : On part d'aujourd'hui pour 12 mois
                            DateTime debut = DateTime.Now;
                            DateTime fin = DateTime.Now.AddMonths(12);

                            // Formatage pour MySQL (YYYY-MM-DD)
                            string dateDebutStr = debut.ToString("yyyy-MM-dd");
                            string dateFinStr = fin.ToString("yyyy-MM-dd");

                            // 4. INSERTION : Création de la nouvelle demande
                            string sqlSouscrire = $"INSERT INTO Souscription (dateDebut, dateFin, statut, IdUtilisateur, IdTypeAdhesion) " +
                                                  $"VALUES ('{dateDebutStr}', '{dateFinStr}', 'En attente', {membre.IdUtilisateur}, {choixAdh})";

                            try
                            {
                                manager.ExecuterAction(sqlSouscrire);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\n✅ Votre demande a été enregistrée avec succès !");
                                Console.WriteLine($"Statut : EN ATTENTE (Expire le {fin:dd/MM/yyyy})");
                                Console.WriteLine("\nVeuillez vous présenter à l'accueil pour valider votre paiement.");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\n❌ Erreur lors de la souscription : " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Action annulée.");
                        }

                        Console.ResetColor();
                        Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
                        Console.ReadKey();
                        break;

                    case "0":
                        retour = true;
                        break;

                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }
        static void ValiderInscription(CommandeManager manager)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- LISTE DES SOUSCRIPTIONS EN ATTENTE ---");
            Console.ResetColor();

            // On affiche les souscriptions qui ne sont pas encore validées
            string sql = "SELECT s.IdSouscription, u.nom, u.prenom FROM Souscription s " +
                         "JOIN Utilisateur u ON s.IdUtilisateur = u.IdUtilisateur " +
                         "WHERE s.statut = 'En attente'";

            DataTable dt = manager.ExecuterLecture(sql);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow r in dt.Rows)
                    Console.WriteLine($"ID: {r["IdSouscription"]} | Membre: {r["nom"]} {r["prenom"]}");

                Console.Write("\nEntrez l'ID de la souscription à valider : ");
                string id = Console.ReadLine();

                // Mise à jour du statut dans la base de données
                manager.ExecuterAction($"UPDATE Souscription SET statut = 'Validée' WHERE IdSouscription = {id}");
                Console.WriteLine("Souscription validée avec succès !");
            }
            else
            {
                Console.WriteLine("Aucune inscription en attente.");
            }
            Console.ReadKey();
        }

        static string SaisirChampObligatoire(string nomChamp)
        {
            string saisie;
            do
            {
                Console.Write($"{nomChamp} : ");
                saisie = Console.ReadLine()?.Trim(); // .Trim() enlève les espaces inutiles

                if (string.IsNullOrEmpty(saisie))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   ⚠️ Erreur : Le champ '{nomChamp}' ne peut pas être vide.");
                    Console.ResetColor();
                }
            } while (string.IsNullOrEmpty(saisie));

            return saisie;
        }
    }
}
    