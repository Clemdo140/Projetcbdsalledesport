using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe 2 du pôle 2 Cours et Planning représentant le coach qui s'occupe d'une séance d'un cours
namespace Projetcbdsalledesport
{
    internal class Coach
    {
        public int IdCoach { get; set; }//id associé au coach
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Specialite { get; set; }//(yoga, crossfit, cardio, etc...)
        public string Formations { get; set; }//("BPJEPS", "Diplôme de Yoga", "Master STAPS", etc...)
        public string Telephone { get; set; }
        public string Email { get; set; }

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nom"></param>
        /// <param name="prenom"></param>
        /// <param name="specialite"></param>
        /// <param name="formations"></param>
        public Coach(int id, string nom, string prenom, string specialite, string formations)
        {
            this.IdCoach = id;
            this.Nom = nom;
            this.Prenom = prenom;
            this.Specialite = specialite;
            this.Formations = formations;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public Coach() { }
    }
}

