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
                                    DataTable dt = manager.ExecuterLecture("SELECT nom, prenom, specialite, telephone FROM Coach");

                                    if (dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            Console.WriteLine($"- {row["prenom"]} {row["nom"].ToString().ToUpper()}");
                                            Console.WriteLine($"  Spécialité : {row["specialite"]} | Tel : {row["telephone"]}");
                                            Console.WriteLine("-----------------------------------");
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
                                    Console.Write("Numéro de contact : "); string t = Console.ReadLine();

                                    // Requête d'insertion dans la table Coach
                                    string sqlAdd = $"INSERT INTO Coach (nom, prenom, specialite, telephone) " +
                                                    $"VALUES ('{n}', '{p}', '{s}', '{t}')";

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
                            Console.WriteLine("0. Retour au menu principal");
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
                                    Console.WriteLine("--- PLANNING ACTUEL ---");
                                    Console.ResetColor();
                                    // On fait une JOINTURE (Join) pour afficher le nom du coach et de la salle au lieu de simples IDs
                                    string sqlPlanning = @"SELECT S.IdSeance, S.nomC, S.horaire, S.CapaciteMax, C.nom as NomCoach, Sa.nom as NomSalle 
                                      FROM Seance S 
                                      JOIN Coach C ON S.IdCoach = C.IdCoach 
                                      JOIN Salle Sa ON S.IdSalle = Sa.IdSalle";

                                    DataTable dtP = manager.ExecuterLecture(sqlPlanning);
                                    if (dtP.Rows.Count > 0)
                                    {
                                        foreach (DataRow r in dtP.Rows)
                                        {
                                            Console.WriteLine($"ID: {r["IdSeance"]} | Cours: {r["nomC"]} | Horaire: {r["horaire"]}");
                                            Console.WriteLine($"   Coach: {r["NomCoach"]} | Salle: {r["NomSalle"]} | Max: {r["CapaciteMax"]} pers.");
                                            Console.WriteLine("---------------------------------------------------------");
                                        }
                                    }
                                    else Console.WriteLine("Aucun cours enregistré.");
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
                                    // On récupère le nom et la capacité de la salle
                                    DataTable dtSalles = manager.ExecuterLecture("SELECT * FROM Salle");

                                    if (dtSalles.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dtSalles.Rows)
                                        {
                                            Console.WriteLine($"- ID: {row["IdSalle"]} | Nom: {row["nom"]} | Capacité : {row["capacite"]} personnes");
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

                    case "3":
                        Console.Clear();
                        Console.WriteLine("--- PLANNING DES SÉANCES ---");

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
                Console.WriteLine("1. Voir le planning des cours (Détails, Intensité, Niveau)");
                Console.WriteLine("2. Réserver un cours (Vérification des places)");
                Console.WriteLine("3. Voir mes réservations / Annuler");
                Console.WriteLine("0. Retour (Déconnexion)");
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
                            Console.WriteLine($"[{r["IdSeance"]}] {r["nomCours"]} - {r["horaire"]}");
                            Console.WriteLine($"    Description : {r["description"]}");
                            Console.WriteLine($"    Intensité : {r["intensite"]} | Niveau : {r["niveau"]}");
                            Console.WriteLine("---------------------------------------------------------");
                        }
                        Console.WriteLine("\nAppuyez sur une touche pour revenir...");
                        Console.ReadKey();
                        break;

                    case "2": // RÉSERVER AVEC RÈGLE MÉTIER
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- RÉSERVATION D'UNE SÉANCE ---");
                        Console.ResetColor();
                        Console.Write("Entrez l'ID du cours souhaité : ");
                        string idC = Console.ReadLine();
                        if (string.IsNullOrEmpty(idC))
                        {
                            Console.WriteLine("Erreur : Vous devez saisir un ID valide.");
                            Console.ReadKey();
                            break;
                        }
                        // RÈGLE MÉTIER : Vérification de la capacité en temps réel
                        // On compte les réservations déjà existantes pour cette séance
                        int inscrits = Convert.ToInt32(manager.ExecuterCalcul($"SELECT COUNT(*) FROM Reservation WHERE IdSeance = {idC}"));
                        int max = Convert.ToInt32(manager.ExecuterCalcul($"SELECT CapaciteMax FROM Seance WHERE IdSeance = {idC}"));

                        if (inscrits < max)
                        {
                            // Insertion de la réservation (NOW() pour MySQL)
                            manager.ExecuterAction($"INSERT INTO Reservation (IdUtilisateur, IdSeance, DateReservation) VALUES ({membre.IdUtilisateur}, {idC}, NOW())");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\nSUCCÈS : Réservation confirmée !");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\nERREUR : Ce cours est complet (Capacité max atteinte).");
                            Console.ResetColor();
                        }
                        Console.ReadKey();
                        break;

                    case "3": // HISTORIQUE ET ANNULATION
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--- MES RÉSERVATIONS ---");
                        Console.ResetColor();
                        string sqlMesRes = $@"SELECT R.IdReservation, T.nomCours, S.DateDebut 
                                     FROM Reservation R 
                                     JOIN Seance S ON R.IdSeance = S.IdSeance 
                                     JOIN TypeCours T ON S.IdCours = T.IdCours 
                                     WHERE R.IdUtilisateur = {membre.IdUtilisateur}";

                        DataTable dtM = manager.ExecuterLecture(sqlMesRes);
                        if (dtM.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtM.Rows)
                            {
                                Console.WriteLine($"- ID Réservation : {r["IdReservation"]} | Cours : {r["nomCours"]} à {r["horaire"]}");
                            }

                            Console.Write("\nSouhaitez-vous annuler une réservation ? (Entrez l'ID ou 'N') : ");
                            string idAnnul = Console.ReadLine();
                            if (idAnnul.ToUpper() != "N")
                            {
                                manager.ExecuterAction($"DELETE FROM Reservation WHERE IdReservation = {idAnnul} AND IdUtilisateur = {membre.IdUtilisateur}");
                                Console.WriteLine("Réservation annulée avec succès.");
                            }
                        }
                        else Console.WriteLine("Vous n'avez aucune réservation en cours.");
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
    