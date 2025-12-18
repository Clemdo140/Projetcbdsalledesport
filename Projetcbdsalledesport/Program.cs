using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
                Console.WriteLine("--- SESSION TERMINÉE ---");
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
            string sqlPop = @"SELECT T.nomCours, COUNT(*) as Total 
                  FROM Reservation R
                  JOIN Seance S ON R.IdSeance = S.IdSeance 
                  JOIN TypeCours T ON S.IdCours = T.IdCours
                  GROUP BY T.nomCours 
                  ORDER BY Total DESC LIMIT 1";
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
                Console.WriteLine("2) Gestion complète des Membres (Ajout/Modif/Suppr)");
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
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- FORMULAIRE D'INSCRIPTION ---");
                                    Console.ResetColor();
                                    Console.Write("Nom : "); string nom = Console.ReadLine();
                                    Console.Write("Prénom : "); string prenom = Console.ReadLine();
                                    Console.Write("Email : "); string email = Console.ReadLine();
                                    Console.Write("Mot de passe : "); string mdpMembre = Console.ReadLine();
                                    Console.Write("Téléphone : "); string tel = Console.ReadLine();

                                    // Requête SQL pour insérer le membre (IdRole 3 = Membre)
                                    // Note : On utilise NOW() pour la date d'inscription en MySQL
                                    string sqlPerso = $"INSERT INTO Utilisateur (nom, prenom, email, motDePasse, telephone, IdRole) " +
                                                      $"VALUES ('{nom}', '{prenom}', '{email}', '{mdpMembre}', '{tel}', 3)";

                                    manager.ExecuterAction(sqlPerso);

                                    Console.WriteLine("\nMembre ajouté avec succès dans la base MySQL !");
                                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": //SUPPRIMER UN MEMBRE
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRESSION D'UN MEMBRE ---");
                                    Console.ResetColor();
                                    Console.Write("Entrez l'Email du membre à supprimer : ");
                                    string emailASupprimer = Console.ReadLine();

                                    // On vérifie d'abord si le membre existe pour éviter de supprimer n'importe quoi
                                    string sqlVerif = $"SELECT COUNT(*) FROM Utilisateur WHERE email = '{emailASupprimer}' AND IdRole = 3";
                                    int existe = Convert.ToInt32(manager.ExecuterCalcul(sqlVerif));

                                    if (existe > 0)
                                    {
                                        Console.WriteLine($"\nÊtes-vous sûr de vouloir supprimer {emailASupprimer} ? Taper O");
                                        if (Console.ReadLine().ToUpper() == "O")
                                        {
                                            // Requête de suppression
                                            string sqlDelete = $"DELETE FROM Utilisateur WHERE email = '{emailASupprimer}'";
                                            manager.ExecuterAction(sqlDelete);
                                            Console.WriteLine("\nMembre supprimé avec succès.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("\nSuppression annulée.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nErreur : Aucun membre trouvé avec cet email.");
                                    }
                                    Console.WriteLine("\nAppuyez sur une touche...");
                                    Console.ReadKey();
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
                                    Console.Write("Nom : "); string n = Console.ReadLine();
                                    Console.Write("Prénom : "); string p = Console.ReadLine();
                                    Console.Write("Spécialité : "); string s = Console.ReadLine();
                                    Console.Write("Formation : "); string f = Console.ReadLine();
                                    Console.Write("Numéro de téléphone : "); string t = Console.ReadLine();
                                    Console.Write("Email : "); string e = Console.ReadLine();

                                    // Requête d'insertion dans la table Coach
                                    string sqlAdd = $"INSERT INTO Coach (nom, prenom, specialite, formation, telephone, email) " +
                                                    $"VALUES ('{n}', '{p}', '{s}', '{f}', '{t}','{e}')";

                                    manager.ExecuterAction(sqlAdd);

                                    Console.WriteLine("\nLe coach a été ajouté avec succès !");
                                    Console.WriteLine("Appuyez sur une touche pour continuer...");
                                    Console.ReadKey();
                                    break;

                                case "3": // SUPPRIMER UN COACH
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- SUPPRIMER UN COACH ---");
                                    Console.ResetColor();
                                    Console.Write("Entrez le Nom du coach à supprimer : ");
                                    string nomASupprimer = Console.ReadLine();

                                    // Vérification avant suppression
                                    string sqlVerif = $"SELECT COUNT(*) FROM Coach WHERE nom = '{nomASupprimer}'";
                                    int existe = Convert.ToInt32(manager.ExecuterCalcul(sqlVerif));

                                    if (existe > 0)
                                    {
                                        Console.WriteLine($"\nVoulez-vous vraiment supprimer le coach {nomASupprimer} ? Tapez Oui");
                                        if (Console.ReadLine().ToUpper() == "O")
                                        {
                                            manager.ExecuterAction($"DELETE FROM Coach WHERE nom = '{nomASupprimer}'");
                                            Console.WriteLine("\nCoach supprimé.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\nAucun coach trouvé avec ce nom.");
                                    }
                                    Console.ReadKey();
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

                                case "2": // ENREGISTRER UN COURS

                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("--- CRÉATION D'UN COURS ---");
                                    Console.ResetColor();
                                    Console.Write("Nom du cours (ex: Yoga) : "); string nomC = Console.ReadLine();
                                    Console.Write("Horaire (AAAA-MM-JJ HH:MM) : "); string hor = Console.ReadLine();

                                    // On affiche les coachs pour que le gérant choisisse
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("\n--- CHOISIR UN COACH ---");
                                    Console.ResetColor();
                                    DataTable dtCo = manager.ExecuterLecture("SELECT IdCoach, nom FROM Coach");
                                    foreach (DataRow r in dtCo.Rows) Console.WriteLine($"{r["IdCoach"]} - {r["nom"]}");
                                    Console.Write("ID du coach : "); string idC = Console.ReadLine();

                                    // On affiche les salles pour que le gérant choisisse
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    Console.WriteLine("\n--- CHOISIR UNE SALLE ---");
                                    Console.ResetColor();
                                    DataTable dtSa = manager.ExecuterLecture("SELECT IdSalle, nom, capacite FROM Salle");
                                    foreach (DataRow r in dtSa.Rows) Console.WriteLine($"{r["IdSalle"]} - {r["nom"]} (Capacité: {r["capacite"]})");
                                    Console.Write("ID de la salle : "); string idS = Console.ReadLine();

                                    // On récupère la capacité de la salle choisie pour l'insérer dans la séance
                                    string sqlCap = $"SELECT capacite FROM Salle WHERE IdSalle = {idS}";
                                    int cap = Convert.ToInt32(manager.ExecuterCalcul(sqlCap));

                                    // Insertion finale
                                    string sqlIns = $"INSERT INTO Seance (nomC, horaire, IdCoach, IdSalle, CapaciteMax) " +
                                                    $"VALUES ('{nomC}', '{hor}', {idC}, {idS}, {cap})";
                                    manager.ExecuterAction(sqlIns);

                                    Console.WriteLine("\nCours ajouté au planning avec succès !");
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

                                    Console.WriteLine("\nLe cours et ses réservations ont été supprimés.");
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
                            Console.WriteLine("--- [GESTION DES SALLES] ---");
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
                                    Console.WriteLine("--- LISTE DES SALLES ---");

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
                                    Console.WriteLine("--- AJOUTER UNE SALLE ---");
                                    Console.Write("Nom de la salle (ex: Studio A) : "); string nomS = Console.ReadLine();
                                    Console.Write("Capacité maximale : "); string capS = Console.ReadLine();

                                    string sqlAddSalle = $"INSERT INTO Salle (nom, capacite) VALUES ('{nomS}', {capS})";
                                    manager.ExecuterAction(sqlAddSalle);

                                    Console.WriteLine("\nSalle ajoutée avec succès !");
                                    Console.ReadKey();
                                    break;

                                case "3": // SUPPRIMER UNE SALLE
                                    Console.Clear();
                                    Console.WriteLine("--- SUPPRIMER UNE SALLE ---");
                                    Console.Write("Entrez le Nom de la salle à supprimer : ");
                                    string nomSallesuppr = Console.ReadLine();

                                    manager.ExecuterAction($"DELETE FROM Salle WHERE nom = '{nomSallesuppr}'");
                                    Console.WriteLine("\nSuppression effectuée si la salle existait.");
                                    Console.ReadKey();
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
                Console.WriteLine("1. Valider une inscription (Adhésion)");
                Console.WriteLine("2. Voir la liste des membres"); // Option utile pour le staff
                Console.WriteLine("3. Consulter le planning");
                Console.WriteLine("0. Retour / Déconnexion");
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
                        Console.WriteLine("--- RÉSERVER UN COURS ---");

                        // 1. VÉRIFICATION DE L'ABONNEMENT (Le membre a-t-il une souscription validée ?)
                        string sqlAbo = $"SELECT statut FROM Souscription WHERE IdUtilisateur = {membre.IdUtilisateur} AND statut = 'Validée'";
                        DataTable dtAbo = manager.ExecuterLecture(sqlAbo);

                        if (dtAbo.Rows.Count == 0)
                        {
                            Console.WriteLine("\n❌ Accès refusé : Vous devez avoir un abonnement validé par le staff pour réserver.");
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
                                    Console.WriteLine("\n❌ Vous avez déjà réservé cette séance !");
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
                                        Console.WriteLine("\n❌ Conflit d'horaire : Vous avez déjà une réservation à ce moment-là !");
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
                                            Console.WriteLine("\n✅ Réservation confirmée !");
                                        }
                                        else
                                        {
                                            Console.WriteLine("\n❌ Ce cours est complet.");
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
                        Console.WriteLine("--- MES RÉSERVATIONS ---");
                        Console.ResetColor();

                        // 1. Affichage des réservations actuelles
                        string sqlMesResas = $@"SELECT R.IdReservation, T.NomCours, S.DateDebut 
                           FROM Reservation R
                           JOIN Seance S ON R.IdSeance = S.IdSeance
                           JOIN TypeCours T ON S.IdCours = T.IdCours
                           WHERE R.IdUtilisateur = {membre.IdUtilisateur}";

                        DataTable dtMesResas = manager.ExecuterLecture(sqlMesResas);

                        if (dtMesResas.Rows.Count > 0)
                        {
                            foreach (DataRow row in dtMesResas.Rows)
                            {
                                Console.WriteLine($"- ID Réservation : {row["IdReservation"]} | Cours : {row["NomCours"]} à {row["DateDebut"]}");
                            }

                            Console.Write("\nSouhaitez-vous annuler une réservation ? (Entrez l'ID ou 'N' pour quitter) : ");
                            string saisie = Console.ReadLine();

                            // 2. GESTION DE L'ERREUR DE SAISIE
                            if (saisie.ToUpper() != "N")
                            {
                                // On vérifie si la saisie est bien un nombre entier
                                if (int.TryParse(saisie, out int idReservation))
                                {
                                    // On vérifie aussi que cette réservation appartient bien au membre connecté (SÉCURITÉ)
                                    string sqlAnnuler = $"DELETE FROM Reservation WHERE IdReservation = {idReservation} AND IdUtilisateur = {membre.IdUtilisateur}";

                                    manager.ExecuterAction(sqlAnnuler);
                                    Console.WriteLine("\n✅ Réservation annulée avec succès.");
                                }
                                else
                                {
                                    Console.WriteLine("\n❌ Erreur : Veuillez entrer un ID numérique valide (ex: 4).");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Vous n'avez aucune réservation.");
                        }

                        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                        Console.ReadKey();
                        break;

                    case "4": // ADHÉRER À UN ABONNEMENT
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- CHOISIR UN ABONNEMENT ---");
                        Console.ResetColor();

                        // 1. Lister les types d'adhésion disponibles
                        DataTable dtTypes = manager.ExecuterLecture("SELECT * FROM TypeAdhesion");
                        foreach (DataRow r in dtTypes.Rows)
                        {
                            Console.WriteLine($"{r["IdTypeAdhesion"]}. {r["libelle"]} - {r["prix"]}€/mois");
                        }

                        Console.Write("\nEntrez le numéro de l'abonnement souhaité : ");
                        string choixAdh = Console.ReadLine();

                        // 2. Créer la souscription (en statut 'En attente')
                        // Le staff devra ensuite la valider avec le bouton qu'on a réparé tout à l'heure !
                        string dateDebut = DateTime.Now.ToString("yyyy-MM-dd");
                        string dateFin = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");

                        string sqlSouscrire = $"INSERT INTO Souscription (dateDebut, dateFin, statut, IdUtilisateur, IdTypeAdhesion) " +
                                             $"VALUES ('{dateDebut}', '{dateFin}', 'En attente', {membre.IdUtilisateur}, {choixAdh})";

                        manager.ExecuterAction(sqlSouscrire);

                        Console.WriteLine("\n✅ Demande envoyée ! Veuillez vous présenter à l'accueil pour le règlement.");
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
            Console.WriteLine("--- LISTE DES SOUSCRIPTIONS EN ATTENTE ---");

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
    }
}
    