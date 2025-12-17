using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe 3 du pôle 2 Cours et Planning représentant une salle de sport où se déroulent les séances d'un cours
namespace Projetcbdsalledesport
{
    internal class Salle
    {
        public int IdSalle { get; set; }//id associé à la salle
        public string NomSalle { get; set; }//(Salle Cardio, Studio Yoga, Salle Muscu)
        public int Capacite { get; set; }//taille max de la salle en nombre de personnes

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nom"></param>
        /// <param name="capacite"></param>
        public Salle(int id, string nom, int capacite)
        {
            this.IdSalle = id;
            this.NomSalle = nom;
            this.Capacite = capacite;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Salle() { }
    }
}
