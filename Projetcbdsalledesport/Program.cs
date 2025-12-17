using System;
using System.Collections.Generic;
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
            string connectionString = "Server=TON_SERVEUR;Database=NOM_BDD;User Id=TON_USER;Password=TON_MDP;";
            CommandeManager manager = new CommandeManager(connectionString, 0, "");//On crée le manager pour la première fois mais comme personne n'est connecté, on met le privilège à 0 et le mot de passe est vide

            Utilisateur utilisateurConnecte = null;//null permet donc d'afficher l'écran de connexion
            bool continuer = true;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("===========================================");
            Console.WriteLine("=== BIENVENUE DANS VOTRE SALLE DE SPORT ===");
            Console.WriteLine("===========================================");
            Console.ResetColor();

            while (continuer)//tant que l'utilisateur n'a pas choisi de quitter
            {

                if (utilisateurConnecte == null)//si personne n'est connecté
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("\n--- CONNEXION ---");
                    Console.ResetColor();
                    Console.Write("\nSaisissez votre email : ");
                    string emailSaisi = Console.ReadLine();
                    Console.Write("Saisisez votre mot de passe : ");
                    string mdpSaisi = Console.ReadLine();

                    utilisateurConnecte = Authentification.Login(emailSaisi, mdpSaisi, manager);//on l'envoie à la classe Authentification qui va vérifier si le couple email-mdp existe bien dans la base

                    if (utilisateurConnecte == null)
                    {
                        Console.ForegroundColor=ConsoleColor.Red;
                        Console.WriteLine("Identifiants incorrects. Réessayez.");
                        Console.ResetColor();
                        Console.ReadKey();
                    }
                    else
                    {

                        manager = new CommandeManager(connectionString, utilisateurConnecte.RoleUtilisateur.IdRole, mdpSaisi);//on écrase le manager par défaut et on le remplace par un avec les infos saisies
                        Console.WriteLine($"\nBienvenue {utilisateurConnecte.Prenom} !");
                    }
                }
                else
                {
                    //menu selon le rôle (privilège)
                    int roleId = utilisateurConnecte.RoleUtilisateur.IdRole;

                    if (roleId == 1) //Administrateur Principal (Gérant)
                    {
                        MenuAdminPrincipal(manager);
                    }
                    else if (roleId == 2) //Administrateur Secondaire (Staff/Accueil)
                    {
                        MenuAdminSecondaire(manager);
                    }
                    else // Membre(Client)
                    {
                        MenuMembre(manager, utilisateurConnecte);
                    }

                    Console.WriteLine("\n0. Se déconnecter et quitter");
                    Console.Write("Choix : ");
                    string choix = Console.ReadLine();

                    if (choix == "0")
                    {
                        utilisateurConnecte = null; // Reset de la session
                        continuer = false;
                        Console.WriteLine("Fermeture de l'application...");
                    }
                }
            }
        }
        static void MenuEvaluation(CommandeManager manager)
        {
            Console.Clear();
            Console.WriteLine("=== INTERFACE ÉVALUATION : RAPPORTS STATISTIQUES ===");

            // 1. Nombre de membres (COUNT)
            var nbMembres = manager.ExecuterCalcul("SELECT COUNT(*) FROM Utilisateur WHERE IdRole = 3");
            Console.WriteLine($"- Nombre total de membres : {nbMembres}");

            // 2. Chiffre d'affaires (SUM)
            var revenus = manager.ExecuterCalcul("SELECT SUM(prix) FROM TypeAdhesion");
            Console.WriteLine($"- Revenus totaux : {revenus} €");

            // 3. Prix moyen (AVG)
            var prixMoyen = manager.ExecuterCalcul("SELECT AVG(prix) FROM TypeAdhesion");
            Console.WriteLine($"- Prix moyen adhésion : {prixMoyen} €");

            // 4. Plus gros cours (MAX)
            var capMax = manager.ExecuterCalcul("SELECT MAX(CapaciteMax) FROM Seance");
            Console.WriteLine($"- Capacité max : {capMax} places");

            // 5. Inscriptions ce mois (COUNT)
            var mois = manager.ExecuterCalcul("SELECT COUNT(*) FROM Reservation WHERE MONTH(DateReservation) = MONTH(GETDATE())");
            Console.WriteLine($"- Réservations ce mois-ci : {mois}");

            // 6. Coach le plus populaire (TOP 1 + COUNT)
            // Note: Cette requête dépend de ta structure exacte
            var coachTop = manager.ExecuterCalcul("SELECT TOP 1 NomCoach FROM Seance GROUP BY NomCoach ORDER BY COUNT(*) DESC");
            Console.WriteLine($"- Coach le plus sollicité : {coachTop}");

            Console.WriteLine("\nAppuyez sur une touche pour revenir...");
            Console.ReadKey();
        }
        static void MenuAdminPrincipal(CommandeManager manager)
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                Console.WriteLine("--- [INTERFACE GÉRANT - PRIVILÈGE TOTAL] ---");
                Console.WriteLine("1. Consulter les Rapports (Interface Évaluation)"); // <--- L'option pour les 6 stats
                Console.WriteLine("2. Gestion complète des Membres (Ajout/Modif/Suppr)");
                Console.WriteLine("3. Gestion des Coachs et des Salles");
                Console.WriteLine("4. Gestion des Cours et Tarifs");
                Console.WriteLine("0. Retour au menu de déconnexion");
                Console.Write("\nVotre choix : ");

                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        // On appelle la fonction que nous venons de créer
                        MenuEvaluation(manager);
                        break;

                    case "2":
                        // Ici tu mettras ton code pour gérer les membres
                        Console.WriteLine("Accès à la gestion des membres...");
                        Console.ReadKey();
                        break;

                    case "3":
                        // Ici pour les coachs
                        Console.WriteLine("Accès à la gestion des coachs...");
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

        static void MenuAdminSecondaire(CommandeManager manager)
        {
            bool retour = false;
            while (!retour)
            {
                Console.WriteLine("\n--- MENU STAFF (NIVEAU 2) ---");
                Console.WriteLine("1. Valider une inscription (Adhésion)");
                Console.WriteLine("2. Modifier informations d'un membre");
                Console.WriteLine("3. Consulter le planning");
                Console.WriteLine("0. Retour");

                if (Console.ReadLine() == "0") retour = true;
            }
        }
        static void MenuMembre(CommandeManager manager, Utilisateur membre)
        {
            bool retour = false;
            while (!retour)
            {
                Console.WriteLine("\n--- ESPACE MEMBRE ---");
                Console.WriteLine("1. Voir les cours (Nom, Description, Intensité)");
                Console.WriteLine("2. Réserver un cours (Vérification capacité)");
                Console.WriteLine("3. Annuler une réservation");
                Console.WriteLine("0. Retour");

                string choix = Console.ReadLine();
                if (choix == "2")
                {
                    Console.Write("ID du cours : ");
                    string idC = Console.ReadLine();

                    // RÈGLE MÉTIER : Vérification de la capacité
                    int inscrits = (int)manager.ExecuterCalcul($"SELECT COUNT(*) FROM Reservation WHERE IdSeance = {idC}");
                    int max = (int)manager.ExecuterCalcul($"SELECT CapaciteMax FROM Seance WHERE IdSeance = {idC}");

                    if (inscrits < max)
                    {
                        manager.ExecuterAction($"INSERT INTO Reservation (IdUtilisateur, IdSeance, DateReservation) VALUES ({membre.Id}, {idC}, GETDATE())");
                        Console.WriteLine("Réservation confirmée !");
                    }
                    else
                    {
                        Console.WriteLine("ERREUR : Cours complet.");
                    }
                }
                else if (choix == "0") retour = true;
            }
        }
    }
}
    