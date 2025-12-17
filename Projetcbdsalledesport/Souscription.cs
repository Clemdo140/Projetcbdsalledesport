using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe 1 du pôle 3 Liaisons et Historique(représentant les relations du pôle 1) qui relie l'utilisateur à son un type d'adhésion
namespace Projetcbdsalledesport
{
    internal class Souscription
    {
        public int IdSouscription { get; set; }// Identifiant unique de la souscription
        public Utilisateur Membre { get; set; }//lien avec l'utilisateur (membre) qui a souscrit à un forfait
        public TypeAdhésion Forfait { get; set; }//le type d'adhésion (forfait) choisi par le membre
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Statut { get; set; }//de type "En attente", "Validée", "Expirée" utile pour la validation par l'admin

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="membre"></param>
        /// <param name="forfait"></param>
        /// <param name="debut"></param>
        /// <param name="fin"></param>
        /// <param name="statut"></param>
        public Souscription(int id, Utilisateur membre, TypeAdhésion forfait, DateTime debut, DateTime fin, string statut)
        {
            this.IdSouscription = id;
            this.Membre = membre;
            this.Forfait = forfait;
            this.DateDebut = debut;
            this.DateFin = fin;
            this.Statut = statut;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Souscription() { }
    
}
}
