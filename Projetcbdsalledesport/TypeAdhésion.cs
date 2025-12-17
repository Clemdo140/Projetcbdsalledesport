using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projetcbdsalledesport
{
    //Classe 3 du pôle 1 utilisateur représentant un type d'adhésion pour un utilisateur
    internal class TypeAdhésion
    {
        public int IdTypeAdhesion { get; set; }//id associé au type d'adhésion, de forfait
        public string Libelle { get; set; }//le nom du forfait (pass annuel, mensuel, étudiant...) 
        public int Prix { get; set; } // Le prix du forfait
        /// <summary>
        /// Constructeur complet
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        /// <param name="prix"></param>
        public TypeAdhésion(int id, string libelle, int prix)
        {
            this.IdTypeAdhesion = id;
            this.Libelle = libelle;
            this.Prix = prix;
        }
        /// <summary>
        /// Constructeur par défaut, pratique pour sql
        /// </summary>
        public TypeAdhésion() { }
    }
}

