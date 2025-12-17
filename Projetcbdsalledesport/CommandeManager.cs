using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;

namespace Projetcbdsalledesport
{
    // Classe pour gérer les commandes SQL vers la base de données
    public class CommandeManager
    {
        private string connectionString;//variable de connexion à la base qui stock ses coordonnées
        private int privilege;//valeur de l'id du rôle de l'utilisateur connecté

        public CommandeManager(string utilisateur, int privilege, string mdp)
        {
            connectionString = $"Server=localhost;Database=SalleDeSport;Uid={utilisateur};Pwd={mdp};";
        }
        /// <summary>
        /// Méthode pour lire les données dans la base de données
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ExecuterLecture(string sql)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))//création d'une connexion à la base
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);//création d'un adaptateur de données
                DataTable dt = new DataTable();//création d'un tableau de données
                adapter.Fill(dt);//remplissage du tableau de données avec les données de la base
                return dt;
            }
        }
        /// <summary>
        /// Méthode pour modifier les données dans la base de données
        /// </summary>
        /// <param name="sql"></param>
        public void ExecuterAction(string sql)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();//ouverture de la connexion
                MySqlCommand cmd = new MySqlCommand(sql, conn);//création d'une commande SQL
                cmd.ExecuteNonQuery();//exécution de la commande SQL
            }
        }
        /// <summary>
        /// Méthode pour exécuter des calculs dans la base de données
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object ExecuterCalcul(string sql)
        {
            using (MySqlConnection conn = new MySqlConnection(this.connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                return cmd.ExecuteScalar();//exécution de la commande SQL et retour du résultat
            }
        }
    }
          
}
