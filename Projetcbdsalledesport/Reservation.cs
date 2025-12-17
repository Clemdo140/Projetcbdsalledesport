using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projetcbdsalledesport
{
    //Classe 2 du pôle 3 Liaisons et Historique(représentant les relations du pôle 2) qui relie l'utilisateur à unu séance
    internal class Reservation
    {
        public int IdReservation { get; set; }//Identifiant unique de la réservation
        public Utilisateur Membre { get; set; }//lien avec l'utilisateur (membre) qui a fait la réservation
        public Seance SeanceReservee { get; set; }//lien avec la séance qui a été réservée
        public DateTime DateReservation { get; set; }// date de la réservation

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="membre"></param>
        /// <param name="seance"></param>
        /// <param name="date"></param>
        public Reservation(int id, Utilisateur membre, Seance seance, DateTime date)
        {
            this.IdReservation = id;
            this.Membre = membre;
            this.SeanceReservee = seance;
            this.DateReservation = date;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Reservation() { }
    }
}

