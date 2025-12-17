using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe 4 du pôle 2 Cours et Planning représentant une séance de cours programmée dans la salle de sport
namespace Projetcbdsalledesport
{
    internal class Seance
    {
        public int IdSeance { get; set; }//id associé à la séance
        public TypeCours Type { get; set; } //le type de cours de la séance
        public Coach Encadrant { get; set; } //le coach associé à la séance
        public Salle Lieu { get; set; } //la salle associée à la séance
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int DureeMinutes { get; set; }//durée de la séance en minutes
        public int CapaciteMax { get; set; }//cette valeur sera comparée au nombre de réservations en SQL pour étudier l'état de remplissage de la salle

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="coach"></param>
        /// <param name="salle"></param>
        /// <param name="debut"></param>
        /// <param name="duree"></param>
        /// <param name="capMax"></param>
        public Seance(int id, TypeCours type, Coach coach, Salle salle, DateTime debut, int duree, int capMax)
        {
            this.IdSeance = id;
            this.Type = type;
            this.Encadrant = coach;
            this.Lieu = salle;
            this.DateDebut = debut;
            this.DureeMinutes = duree;
            this.DateFin = debut.AddMinutes(duree);// pour éviter que l'utilisateur se trompe dans la saisie de la date de fin, on calcul en fonction du débit et de la durée
            this.CapaciteMax = capMax;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Seance() {
        }
    }
}

