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
            //Initialisation de la connexion
            string maconnection = "Server=localhost;Database=SalleDeSport;Uid=AppUser;Pwd=MdpAppUser;";
            CommandeManager manager = new CommandeManager(maconnection, 0, "");//On crée le manager pour la première fois mais comme personne n'est connecté, on met le privilège à 0 et le mot de passe est vide
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

                        manager = new CommandeManager(dbUser, utilisateurConnecte.RoleUtilisateur.IdRole, dbPass);

                        Console.WriteLine($"\nBienvenue {utilisateurConnecte.Prenom} !");
                    }
                    
                }
             
                if (utilisateurConnecte.RoleUtilisateur.Fonction == "Gérant")
                {
                    MenuAdminPrincipal(manager);
                }
                else if (utilisateurConnecte.RoleUtilisateur.Fonction == "Staff")
                {
                    MenuAdminSecondaire(manager); 
                }
                else
                {
                    MenuMembre(manager, utilisateurConnecte);
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("--- SESSION TERMINÉE ---");
                Console.ResetColor();
                Console.WriteLine("0) Quitter définitivement");
                Console.WriteLine("1) Se reconnecter avec un autre compte");
                         Console.Write("\nChoix : ");

                string choixFinal = Console.ReadLine();
                if (choixFinal == "0")
                {
                    applicationEnCours = false; // Arrête tout
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Fermeture... Au revoir !");
                    Console.ResetColor();
                }

            }
        }
        /// <summary>
        /// Menu d'évaluation et de rapports pour le gérant
        /// </summary>
        /// <param name="manager"></param>
        static void MenuEvaluation(CommandeManager manager)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- [RAPPORT STATISTIQUE - ÉVALUATION] ---");
            Console.ResetColor();
            Console.WriteLine("Génération des rapports en cours...\n");

            int nbMembres = Convert.ToInt32(manager.ExecuterCalcul("SELECT COUNT(*) FROM Utilisateur WHERE IdRole = 3"));

            string sqlPop = @"SELECT T.NomCours AS nomC, 
                               COUNT(R.IdReservation) AS NbParticipants
                        FROM TypeCours T
                        LEFT JOIN Seance S ON T.IdCours = S.IdCours
                        LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance
                        GROUP BY T.NomCours";
            DataTable dtPop = manager.ExecuterLecture(sqlPop);
            string coursPop = dtPop.Rows.Count > 0 ? dtPop.Rows[0]["nomC"].ToString() : "Aucun";

            double occupation = 0;
            try
            {
                string Occ = "SELECT (COUNT(R.IdReservation) * 100.0 / SUM(S.CapaciteMax)) FROM Seance S LEFT JOIN Reservation R ON S.IdSeance = R.IdSeance";
                occupation = Convert.ToDouble(manager.ExecuterCalcul(Occ));
            }
            catch { occupation = 0; }

            // AFFICHAGE DES RESULTATS
            Console.WriteLine($"Nombre de membres inscrits : {nbMembres}");
            Console.WriteLine($"Cours le plus suivi : {coursPop}");
            Console.WriteLine($"Taux de remplissage global des séances : {Math.Round(occupation, 2)}%");
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
            Console.ReadKey();
        }
        /// <summary>
        /// Menu principal pour le gérant avec privilège total
        /// </summary>
        /// <param name="manager"></param>
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
                Console.WriteLine("1) Rapports et Statistiques"); 
                Console.WriteLine("2) Gestion des Membres");
                Console.WriteLine("3) Gestion des Coachs");
                Console.WriteLine("4) Gestion des Cours");
                Console.WriteLine("5) Gestion des Salles");

                Console.Write("\nVotre choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
        
                        MenuEvaluation(manager);
                        break;

                    case "2":
            
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
                                case "1": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES MEMBRES ENREGISTRÉS ---");
                                    Console.ResetColor();

                                    DataTable dt = manager.ExecuterLecture("SELECT nom, prenom, email, adresse, telephone FROM Utilisateur WHERE IdRole = 3");

                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            Console.WriteLine($"- {row["nom"].ToString().ToUpper()} {row["prenom"]} | Email: {row["email"]} | Adresse: {row["adresse"]} | Tel: {row["telephone"]}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucun membre trouvé dans la base.");
                                    }
                                    Console.ReadKey();
                                    break;

                                case "2": 
                                case "inscription": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- FORMULAIRE D'INSCRIPTION ---");
                                    Console.ResetColor();
                                    string nom = SaisirChampObligatoire("Nom");
                                    string prenom = SaisirChampObligatoire("Prénom");
                                    string email = SaisirChampObligatoire("Email");
                                    string mdpMembre = SaisirChampObligatoire("Mot de passe");
                                    string adr = SaisirChampObligatoire("Adresse");
                                    string tel = SaisirChampObligatoire("Téléphone");

                                    string sqlPerso = $"INSERT INTO Utilisateur (nom, prenom, email, motDePasse, telephone, IdRole) " +
                                                      $"VALUES ('{nom}', '{prenom}', '{email}', '{mdpMembre}','{adr}', '{tel}', 3)";

                                    manager.ExecuterAction(sqlPerso);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\nMembre ajouté avec succès !");
                                    Console.ResetColor();
                                    Console.ReadKey();
                                    break;

                                case "3": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRESSION D'UN MEMBRE ---");
                                    Console.ResetColor();

                                    Console.Write("Entrez l'Email du membre à supprimer : ");
                                    string emailSuppr = Console.ReadLine()?.Trim();

                                    if (string.IsNullOrEmpty(emailSuppr))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("Erreur : Vous devez saisir un email pour effectuer une recherche.");
                                        Console.ResetColor();
                                        Thread.Sleep(2000);
                                        break; 
                                    }
                                    string sqlGetId = $"SELECT IdUtilisateur FROM Utilisateur WHERE email = '{emailSuppr}'";
                                    DataTable dtU = manager.ExecuterLecture(sqlGetId);

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
                                            manager.ExecuterAction($"DELETE FROM Reservation WHERE IdUtilisateur = {idAuteur}");
                                            manager.ExecuterAction($"DELETE FROM Souscription WHERE IdUtilisateur = {idAuteur}");
                                            manager.ExecuterAction($"DELETE FROM Utilisateur WHERE IdUtilisateur = {idAuteur}");

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\nMembre supprimé avec succès.");
                                            Console.ResetColor();
                                            Thread.Sleep(1500);
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("\nOpération annulée");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\nAucun utilisateur trouvé avec l'email : {emailSuppr}");
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
                                case "1": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES COACHS ---");
                                    Console.ResetColor();
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
                                    Console.ReadKey();
                                    break;

                                case "2": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- ENREGISTREMENT D'UN COACH ---");
                                    Console.ResetColor();
                                    string n = SaisirChampObligatoire("Nom");
                                    string p = SaisirChampObligatoire("Prénom");
                                    string s = SaisirChampObligatoire("Spécialité");
                                    string f = SaisirChampObligatoire("Formation");
                                    string t = SaisirChampObligatoire("Numéro de téléphone");
                                    string e = SaisirChampObligatoire("Email");
                                    string sqlAdd = $"INSERT INTO Coach (nom, prenom, specialite, formation, telephone, email) " +
                                                    $"VALUES ('{n}', '{p}', '{s}', '{f}', '{t}', '{e}')";

                                    manager.ExecuterAction(sqlAdd);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\nLe coach a été ajouté avec succès !");
                                    Console.ResetColor();
                                    Console.ReadKey();
                                    break;

                                case "3": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRESSION D'UN COACH ---");
                                    Console.ResetColor();
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
                                            Console.Write($"\nConfirmez-vous la suppression du coach n°{idC} ? (O/N) : ");
                                            Console.ResetColor();

                                            if (Console.ReadLine().ToUpper() == "O")
                                            {
                                                manager.ExecuterAction($"DELETE FROM Seance WHERE IdCoach = {idC}");
                                                manager.ExecuterAction($"DELETE FROM Coach WHERE IdCoach = {idC}");

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("\nCoach supprimé avec succès.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\nCet ID n'existe pas.");
                                            Console.ResetColor();
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
                            Console.WriteLine("3) Supprimer un cours");
                           
                            Console.Write("\nVotre choix : ");

                            string sousChoix = Console.ReadLine();

                            switch (sousChoix)
                            {
                                case "1": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;    
                                    Console.WriteLine("--- PLANNING ACTUEL DÉTAILLÉ ---");
                                    Console.ResetColor();
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
                                    Console.ReadKey();
                                    break;

                                case "2": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- CRÉATION D'UNE NOUVELLE SÉANCE ---");
                                    Console.ResetColor();
                                    string nomCours = SaisirChampObligatoire("Nom du cours (ex: Yoga)");
                                    string sqlIdC = $"SELECT IdCours FROM TypeCours WHERE NomCours = '{nomCours}'";
                                    DataTable dtCours = manager.ExecuterLecture(sqlIdC);

                                    if (dtCours.Rows.Count == 0)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\nErreur : Le type de cours '{nomCours}' n'existe pas.");
                                        Console.ResetColor();
                                        Console.ReadKey();
                                        break;
                                    }
                                    int idCoursValide = Convert.ToInt32(dtCours.Rows[0]["IdCours"]);
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
                                            Console.WriteLine("Erreur : Veuillez saisir un nombre entre 1 et 300.");
                                            Console.ResetColor();
                                        }
                                    }
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
                                                Console.WriteLine("Erreur : La date doit être dans le futur.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Erreur : Format invalide (Ex: 2026-05-12 14:00)");
                                            Console.ResetColor();
                                        }
                                    }

                                
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("\n--- LISTE DES COACHS ---");
                                    Console.ResetColor();
                                    DataTable dtCo = manager.ExecuterLecture("SELECT IdCoach, nom, prenom FROM Coach");
                                    foreach (DataRow r in dtCo.Rows) Console.WriteLine($" ID: {r["IdCoach"]} | {r["prenom"]} {r["nom"]}");
                                    string idC = SaisirChampObligatoire("Entrez l'ID du coach choisi");

                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("\n--- LISTE DES SALLES ---");
                                    Console.ResetColor();
                                    DataTable dtSa = manager.ExecuterLecture("SELECT idSalle, nomSalle, capacite FROM Salle");
                                    foreach (DataRow r in dtSa.Rows) Console.WriteLine($" ID: {r["idSalle"]} | {r["nomSalle"]} (Capacité: {r["capacite"]})");
                                    string idS = SaisirChampObligatoire("Entrez l'ID de la salle choisie");

                                    try
                                    {
              
                                        string sqlCap = $"SELECT capacite FROM Salle WHERE idSalle = {idS}";
                                        object resultCap = manager.ExecuterCalcul(sqlCap);

                                        if (resultCap != null)
                                        {
                                            int capMax = Convert.ToInt32(resultCap);
                                            string sqlIns = $"INSERT INTO Seance (IdCours, DateDebut, IdCoach, idSalle, CapaciteMax, DureeMinutes) " +
                                                            $"VALUES ({idCoursValide}, '{dateSaisie:yyyy-MM-dd HH:mm:ss}', {idC}, {idS}, {capMax}, {dureeMinutes})";

                                            manager.ExecuterAction(sqlIns);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\nSéance ajoutée au planning avec succès !");
                                            Console.ResetColor();
                                        }
                                        else
                                        {
                                            Console.WriteLine("\nErreur : Salle introuvable.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nErreur MySQL : " + ex.Message);
                                        Console.ResetColor();
                                    }
                                    Console.ReadKey();
                                    break;

                                case "3":
                             
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- ANNULATION D'UN COURS ---");
                                    Console.ResetColor();

                                    // Afficher d'abord le planning actuel pour aider l'administrateur
                                    string sqlPlanning = @"SELECT S.IdSeance, T.NomCours, S.DateDebut 
                                                          FROM Seance S 
                                                          JOIN TypeCours T ON S.IdCours = T.IdCours 
                                                          ORDER BY S.DateDebut";

                                    DataTable dtPlanning = manager.ExecuterLecture(sqlPlanning);

                                    if (dtPlanning.Rows.Count > 0)
                                    {
                                        foreach (DataRow r in dtPlanning.Rows)
                                        {
                                            Console.WriteLine($"ID: {r["IdSeance"]} | {r["NomCours"]} le {r["DateDebut"]}");
                                        }

                                    
                                        Console.Write("\nEntrez l'ID du cours à supprimer (ou Entrée pour annuler) : ");
                                        string idSaisie = Console.ReadLine();

                                        if (!string.IsNullOrEmpty(idSaisie))
                                        {
                                           
                                            if (int.TryParse(idSaisie, out int idCours))
                                            {
                                       
                                                string sqlCheck = $"SELECT COUNT(*) FROM Seance WHERE IdSeance = {idCours}";
                                                int existe = Convert.ToInt32(manager.ExecuterCalcul(sqlCheck));

                                                if (existe > 0)
                                                {
                                                    try
                                                    {
                                                  
                                                        manager.ExecuterAction($"DELETE FROM Reservation WHERE IdSeance = {idCours}");
                                          
                                                        manager.ExecuterAction($"DELETE FROM Seance WHERE IdSeance = {idCours}");

                                                        Console.ForegroundColor = ConsoleColor.Green;
                                                        Console.WriteLine("\nLe cours et ses réservations ont été supprimés.");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.ForegroundColor = ConsoleColor.Red;
                                                        Console.WriteLine("\nErreur lors de la suppression : " + ex.Message);
                                                        Console.ResetColor();
                                                    }
                                                }
                                                else
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Red;
                                                    Console.WriteLine("\nAucun cours trouvé avec cet ID.");
                                                }
                                            }
                                            else
                                            {
                                                Console.ForegroundColor = ConsoleColor.Red;
                                                Console.WriteLine("\nErreur : Vous devez saisir un nombre.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucun cours n'est actuellement programmé.");
                                    }

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
                                case "1": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- LISTE DES SALLES ---");
                                    Console.ResetColor();
                                    DataTable dtSalles = manager.ExecuterLecture("SELECT * FROM Salle");

                                    if (dtSalles.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dtSalles.Rows)
                                        {
                                            Console.WriteLine($"- ID: {row["idSalle"]} | Nom: {row["nomSalle"]} | Capacité : {row["capacite"]} personnes");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Aucune salle enregistrée.");
                                    }
                                    Console.ReadKey();
                                    break;

                                case "2": 
                                 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- AJOUTER UNE NOUVELLE SALLE ---");
                                    Console.ResetColor();
                                    string nomS = SaisirChampObligatoire("Nom de la salle (ex: Muscu B)");
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
                                            Console.WriteLine("Erreur : Veuillez saisir un nombre entier positif.");
                                            Console.ResetColor();
                                        }
                                    }

                                    try
                                    {
                                        string sqlAddSalle = $"INSERT INTO Salle (nomSalle, capacite) VALUES ('{nomS}', {capS})";

                                        manager.ExecuterAction(sqlAddSalle);

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("\nLa salle a été ajoutée avec succès !");
                                        Console.ResetColor();
                                        Thread.Sleep(1500);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nErreur MySQL : " + ex.Message);
                                        Console.ResetColor();
                                    }
                                    Console.ReadKey();
                                    break;

                                case "3": 
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRIMER UNE SALLE (PAR ID) ---");
                                    Console.ResetColor();

                                    DataTable dtSalle = manager.ExecuterLecture("SELECT idSalle, nomSalle FROM Salle");
                                    Console.WriteLine("Liste des salles disponibles :");
                                    foreach (DataRow r in dtSalle.Rows)
                                    {
                                        Console.WriteLine($"  [{r["idSalle"]}] - {r["nomSalle"]}");
                                    }
                                    Console.WriteLine();
                                    string saisieId = SaisirChampObligatoire("Entrez l'ID de la salle à supprimer");

                                    if (int.TryParse(saisieId, out int idS))
                                    {
                                       
                                        object existe = manager.ExecuterCalcul($"SELECT COUNT(*) FROM Salle WHERE idSalle = {idS}");
                                        if (Convert.ToInt32(existe) > 0)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.Write($"\nConfirmez-vous la suppression de la salle n°{idS} et de ses séances ? (O/N) : ");
                                            Console.ResetColor();

                                            if (Console.ReadLine().ToUpper() == "O")
                                            {
                                                manager.ExecuterAction($"DELETE FROM Seance WHERE idSalle = {idS}");
                                                manager.ExecuterAction($"DELETE FROM Salle WHERE idSalle = {idS}");

                                                Console.ForegroundColor = ConsoleColor.Green;
                                                Console.WriteLine("\nSalle supprimée avec succès.");
                                                Console.ResetColor();
                                            }
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\nCet ID n'existe pas.");
                                            Console.ResetColor();
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nErreur : L'ID doit être un nombre.");
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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Choix invalide.");
                        Console.ResetColor();
                        Console.ReadKey();
                        break;
                }
            }
        }
        /// <summary>
        /// Menu secondaire pour le staff avec privilège limité
        /// </summary>
        /// <param name="manager"></param>
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
                        string sqlMembresAdh = @"SELECT U.nom, U.prenom, U.email, T.libelle as NomForfait, S.statut
                             FROM Utilisateur U
                             LEFT JOIN Souscription S ON U.IdUtilisateur = S.IdUtilisateur
                             LEFT JOIN TypeAdhesion T ON S.IdTypeAdhesion = T.IdTypeAdhesion
                             WHERE U.IdRole = 3"; 

                        DataTable dtMembres = manager.ExecuterLecture(sqlMembresAdh);

                        if (dtMembres.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtMembres.Rows)
                            {
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
                        Console.ReadKey();
                        break;

                    case "3":
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- PLANNING DES SÉANCES ---");
                        Console.ResetColor();
                        string sqlPlanning = @"SELECT T.NomCours, S.DateDebut 
                          FROM Seance S 
                          JOIN TypeCours T ON S.IdCours = T.IdCours";

                        DataTable dtP = manager.ExecuterLecture(sqlPlanning);

                        if (dtP.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtP.Rows)
                            {
                                Console.WriteLine($"- {r["NomCours"]} prévu le : {r["DateDebut"]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Aucune séance au planning.");
                        }
                        Console.ReadKey();
                        break;
                    case "0":
                        retour = true;
                        break;
                }
            }
        }
        /// <summary>
        /// Menu pour les membres avec options d'adhésion et de réservation
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="membre"></param>
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
                    case "1": 
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- PLANNING DES COURS ---");
                        Console.ResetColor();
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
                        Console.ReadKey();
                        break;

                    case "2":
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- RÉSERVER UN COURS ---");
                        Console.ResetColor();

                        try
                        {
                            // 1. Vérification de l'abonnement
                            string abo = $"SELECT statut FROM Souscription WHERE IdUtilisateur = {membre.IdUtilisateur} AND statut = 'Validée'";
                            DataTable dtAbo = manager.ExecuterLecture(abo);

                            if (dtAbo.Rows.Count == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nErreur : Abonnement non valide ou en attente.");
                                Console.ResetColor();
                                Console.WriteLine("Veuillez régulariser votre situation à l'accueil.");
                                Console.ReadKey();
                                break;
                            }

                            // 2. Affichage des séances avec calcul des places en SQL
                            string sqlDispo = @"SELECT S.IdSeance, T.NomCours, S.DateDebut, S.CapaciteMax,
                          (SELECT COUNT(*) FROM Reservation R WHERE R.IdSeance = S.IdSeance) as Inscrits
                          FROM Seance S JOIN TypeCours T ON S.IdCours = T.IdCours";

                            DataTable dtDispo = manager.ExecuterLecture(sqlDispo);

                            foreach (DataRow r in dtDispo.Rows)
                            {
                                int placesLibres = Convert.ToInt32(r["CapaciteMax"]) - Convert.ToInt32(r["Inscrits"]);
                                Console.WriteLine($"ID: {r["IdSeance"]} | {r["NomCours"]} le {r["DateDebut"]} | Libres: {placesLibres}/{r["CapaciteMax"]}");
                            }

                            // 3. Saisie sécurisée de l'ID
                            string idSaisie = SaisirChampObligatoire("\nEntrez l'ID de la séance choisie");

                            if (int.TryParse(idSaisie, out int idChoisi))
                            {
                                DataRow[] coursSelect = dtDispo.Select($"IdSeance = {idChoisi}");

                                if (coursSelect.Length > 0)
                                {
                                    // Vérification doublon
                                    string sqlDoublon = $"SELECT COUNT(*) FROM Reservation WHERE IdUtilisateur = {membre.IdUtilisateur} AND IdSeance = {idChoisi}";
                                    if (Convert.ToInt32(manager.ExecuterCalcul(sqlDoublon)) > 0)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nErreur : Vous êtes déjà inscrit à ce cours.");
                                    }
                                    else
                                    {
                                        int inscrits = Convert.ToInt32(coursSelect[0]["Inscrits"]);
                                        int max = Convert.ToInt32(coursSelect[0]["CapaciteMax"]);

                                        if (inscrits < max)
                                        {
                                            // Exécution de l'achat/réservation
                                            manager.ExecuterAction($"INSERT INTO Reservation (IdUtilisateur, IdSeance, dateReservation) VALUES ({membre.IdUtilisateur}, {idChoisi}, NOW())");
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine("\nRéservation confirmée !");
                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("\nErreur : Ce créneau est complet.");
                                        }
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\nErreur : Cette séance n'existe pas dans la liste.");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nErreur : Veuillez saisir un nombre valide.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nUne erreur technique est survenue : " + ex.Message);
                        }

                        Console.ResetColor();
                        Console.ReadKey();
                        break;

                    case "3":
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"--- PROFIL DE {membre.Prenom.ToUpper()} ---");
                        Console.ResetColor();

                        //Statut abonnement
                        string sqlAbo = "SELECT s.dateDebut, s.dateFin, t.libelle, s.statut " +
                                        "FROM Souscription s " +
                                        "JOIN TypeAdhesion t ON s.IdTypeAdhesion = t.IdTypeAdhesion " +
                                        $"WHERE s.IdUtilisateur = {membre.IdUtilisateur}";

                        DataTable dtAbon = manager.ExecuterLecture(sqlAbo);

                        Console.WriteLine("\n[ STATUT DE L'ABONNEMENT ]");
                        if (dtAbon.Rows.Count > 0)
                        {
                            DataRow abo = dtAbon.Rows[0];
                            DateTime dateFin = Convert.ToDateTime(abo["dateFin"]);
                            string statutBase = abo["statut"].ToString();

                            Console.Write($"Offre : {abo["libelle"]} | Expire le : {dateFin:dd/MM/yyyy}");

                            if (statutBase != "Validée")
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(" (EN ATTENTE DE VALIDATION)");
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

                        //liste des resa + annuler
                        Console.WriteLine("\n[ MES SÉANCES RÉSERVÉES ]");
                        string sqlRes = "SELECT r.IdReservation, t.NomCours, s.DateDebut, sa.nomSalle " +
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
                                Console.WriteLine($"ID {r["IdReservation"]} : {r["NomCours"]} le {d:dd/MM à HH:mm} ({r["nomSalle"]})");
                            }

                            Console.Write("\nEntrez l'ID d'une séance pour l'annuler (ou Entrée pour quitter) : ");
                            string choixAnnul = Console.ReadLine();

                            if (!string.IsNullOrEmpty(choixAnnul))
                            {

                                if (int.TryParse(choixAnnul, out int idResultat))
                                {
                              
                                    string sqlCheck = $"SELECT COUNT(*) FROM Reservation WHERE IdReservation = {idResultat} AND IdUtilisateur = {membre.IdUtilisateur}";
                                    int check = Convert.ToInt32(manager.ExecuterCalcul(sqlCheck));

                                    if (check > 0)
                                    {
                                        manager.ExecuterAction($"DELETE FROM Reservation WHERE IdReservation = {idResultat}");
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine("\nRéservation annulée avec succès.");
                                        Console.ResetColor();
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("\nID invalide ou ce n'est pas votre réservation.");
                                        Console.ResetColor();
                                    }
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\nErreur : Veuillez saisir un nombre (ID).");
                                    Console.ResetColor();
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Vous n'avez aucune réservation à venir.");
                        }

                        Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
                        Console.ReadKey();
                        break;

                    case "4": 
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("--- SOUSCRIRE À UN ABONNEMENT ---");
                        Console.ResetColor();
                        Console.WriteLine("En souscrivant à un nouvel abonnement, votre ancien abonnement sera remplacé et vos séances réservées annulées");
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
                            string sqlNettoyage = $"DELETE FROM Souscription WHERE IdUtilisateur = {membre.IdUtilisateur}";
                            manager.ExecuterAction(sqlNettoyage);
                            DateTime debut = DateTime.Now;
                            DateTime fin = DateTime.Now.AddMonths(12);
                            string dateDebutStr = debut.ToString("yyyy-MM-dd");
                            string dateFinStr = fin.ToString("yyyy-MM-dd");
                            string sqlSouscrire = $"INSERT INTO Souscription (dateDebut, dateFin, statut, IdUtilisateur, IdTypeAdhesion) " +
                                                  $"VALUES ('{dateDebutStr}', '{dateFinStr}', 'En attente', {membre.IdUtilisateur}, {choixAdh})";

                            try
                            {
                                manager.ExecuterAction(sqlSouscrire);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("\nVotre demande a été enregistrée avec succès !");
                                Console.WriteLine($"Statut : EN ATTENTE (Expire le {fin:dd/MM/yyyy})");
                                Console.WriteLine("\nVeuillez vous présenter à l'accueil pour valider votre paiement.");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("\nErreur lors de la souscription : " + ex.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Action annulée.");
                        }

                        Console.ResetColor();
                        Console.ReadKey();
                        break;

                    case "0":
                        retour = true;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Choix invalide.");
                        Console.ResetColor();
                        break;
                }
            }
        }
        /// <summary>
        /// Valider une inscription en attente
        /// </summary>
        /// <param name="manager"></param>
        static void ValiderInscription(CommandeManager manager)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--- ATTENTES DE VALIDATION ---");
            Console.ResetColor();

            string sql = "SELECT s.IdSouscription, u.nom, u.prenom FROM Souscription s " +
                         "JOIN Utilisateur u ON s.IdUtilisateur = u.IdUtilisateur " +
                         "WHERE s.statut = 'En attente'";

            DataTable dt = manager.ExecuterLecture(sql);

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Console.WriteLine($"ID: {r["IdSouscription"]} | Membre: {r["nom"]} {r["prenom"]}");
                }

                string idSaisie = SaisirChampObligatoire("\nID a valider");

                if (int.TryParse(idSaisie, out int idValide))
                {
                    
                    string sqlCheck = $"SELECT COUNT(*) FROM Souscription WHERE IdSouscription = {idValide} AND statut = 'En attente'";
                    int existe = Convert.ToInt32(manager.ExecuterCalcul(sqlCheck));

                    if (existe > 0)
                    {
                        try
                        {
                            manager.ExecuterAction($"UPDATE Souscription SET statut = 'Validée' WHERE IdSouscription = {idValide}");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nInscription validee.");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\nErreur SQL : " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nID introuvable ou deja valide.");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.WriteLine("\nSaisie invalide (chiffre requis).");
                }
            }
            else
            {
                Console.WriteLine("\nAucune demande en attente.");
            }

            Console.ReadKey();
        }
        /// <summary>
        /// Saisir un champ obligatoire avec validation
        /// </summary>
        /// <param name="nomChamp"></param>
        /// <returns></returns>
        static string SaisirChampObligatoire(string nomChamp)
        {
            string saisie;
            do
            {
                Console.Write($"{nomChamp} : ");
                saisie = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(saisie))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Erreur : Le champ '{nomChamp}' ne peut pas être vide.");
                    Console.ResetColor();
                }
            } while (string.IsNullOrEmpty(saisie));

            return saisie;
        }
    }
}
    