using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe qui vérifie les identifiants et permet au Program.cs de savoir quel menu afficher, c'est le pont entre l'utilisateur et le reste de l'application
//Elle s'appuis sur la classe CommandeManager pour exécuter la requête SQL
namespace Projetcbdsalledesport
{
    public static class Authentification
    {
        /// <summary>
        /// Méthode de connexion qui vérifie les identifiants et retourne l'utilisateur connecté avec son rôle
        /// </summary>
        /// <param name="email"></param>
        /// <param name="mdp"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static Utilisateur Login(string email, string mdp, CommandeManager manager)
        {
            string sql = $@"SELECT u.*, r.fonction 
                            FROM Utilisateur u 
                            JOIN Role r ON u.IdRole = r.IdRole 
                            WHERE u.email = '{email}' AND u.motDePasse = '{mdp}'";

            DataTable dt = manager.ExecuterLecture(sql); //tableau temporaire en mémoire qui stocke le résultat de la requête.

            if (dt.Rows.Count > 0)//on vérifie si la base de données a trouvé au moins une ligne et si oui, l'utilisateur est authentifié
            {
                DataRow row = dt.Rows[0];

               
                Role role = new Role(Convert.ToInt32(row["IdRole"]), row["fonction"].ToString());// On crée l'objet Role

                // On crée et on renvoie l'objet Utilisateur complet
                return new Utilisateur(
                    Convert.ToInt32(row["IdUtilisateur"]),
                    row["nom"].ToString(),
                    row["prenom"].ToString(),
                    row["email"].ToString(),
                    role
                );
            }

            return null; 
        }
    }
}
