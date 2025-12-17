using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projetcbdsalledesport
{
    //Classe 1 du pôle 1 utilisateur représentant un utilisateur de l'application
    public class Utilisateur
    {
        public int IdUtilisateur { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public Role RoleUtilisateur { get; set; } // La liaison avec la classe Role pour gérer les privilèges, c'est ici qu'on saura si c'est l'Admin Principal, Secondaire ou un Membre

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Utilisateur() { }

        /// <summary>
        /// Constructeur complet 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nom"></param>
        /// <param name="prenom"></param>
        /// <param name="email"></param>
        /// <param name="role"></param>
        public Utilisateur(int id, string nom, string prenom, string email, Role role)
        {
            this.IdUtilisateur = id;
            this.Nom = nom;
            this.Prenom = prenom;
            this.Email = email;
            this.RoleUtilisateur = role;
        }
    }
}

