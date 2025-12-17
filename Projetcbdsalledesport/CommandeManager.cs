using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Projetcbdsalledesport
{
    // Classe pour gérer les commandes SQL vers la base de données
    public class CommandeManager
    {
        private string connectionString;//variable de connexion à la base qui stock ses coordonnées
        private int privilege;//valeur de l'id du rôle de l'utilisateur connecté

        public CommandeManager(string utilisateur, int privilege, string mdp)
        {
            connectionString = $"Server=(àchanger); Database=(àchanger); User Id={utilisateur}; Password={mdp};";
        }
        /// <summary>
        /// Méthode pour lire les données dans la base de données
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ExecuterLecture(string sql)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))//création d'une connexion à la base
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);//création d'un adaptateur de données
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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();//ouverture de la connexion
                SqlCommand cmd = new SqlCommand(sql, conn);//création d'une commande SQL
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
            using (SqlConnection conn = new SqlConnection(this.connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                return cmd.ExecuteScalar();//exécution de la commande SQL et retour du résultat
            }
        }
    }
          
}
