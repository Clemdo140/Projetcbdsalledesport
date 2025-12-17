using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Classe 1 du pôle 2 Cours et Planning représentant un type de cours proposé par la salle de sport
namespace Projetcbdsalledesport
{
    internal class TypeCours
    {
        public int IdCours { get; set; }//id associé au type de cours
        public string NomCours { get; set; }//nom du cours (yoga, crossfit, zumba, etc...)
        public string Description { get; set; }//description du cours
        public string Intensite { get; set; }//niveau d'intensité du cours (faible, modéré, élevé, etc...)

        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nom"></param>
        /// <param name="description"></param>
        /// <param name="intensite"></param>
        public TypeCours(int id, string nom, string description, string intensite)
        {
            this.IdCours = id;
            this.NomCours = nom;
            this.Description = description;
            this.Intensite = intensite;
        }

        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public TypeCours() { }
    
}
}
