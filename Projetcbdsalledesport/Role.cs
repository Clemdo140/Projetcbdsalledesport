using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projetcbdsalledesport
{
    // Classe 2 du pôle 1 utilisateur représentant un rôle attribué à un utilisateur
    public class Role
    {
        public int IdRole { get; set; }//id associé au role (1 pour amin, 2 pour membre, etc...)
        public string Fonction { get; set; }//nom associé au role (admin, membre,coach etc...)

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fonction"></param>
        public Role() { }
        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fonction"></param>
        public Role(int id, string fonction)
        {
            this.IdRole = id;
            this.Fonction = fonction;
        }
    }
}
