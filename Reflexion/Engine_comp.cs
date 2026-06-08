using IA_echecs;
using Regles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Regles.Moteur;

namespace IA_echecs
{
    public class Engine_comp
    {

        private Moteur mot;
        private bool couleur;

        public Moteur Mot
        {
            get { return mot; }
            private set { mot = value; }
        }

        public bool Couleur
        {
            get { return couleur; }
            private set { couleur = value; }
        }

        public Engine_comp(Moteur moteur, bool couleur)
        {
            mot = moteur;
            Couleur = couleur;
        }

        public Coup CoupRandom()
        {
            Coup[] légaux = Mot.Calcul_légaux();   //Récupère tous les coups légaux avec case de départ associé
            if (légaux.Length == 0)
            {
                return Mot.CreerCoup(0, 0);
            }

            int random = Choix_random(Mot.NombreLegaux);    //On a xx 1 dans les 64 ulong, on choisi un nombre qui correspond à un 1

            return légaux[random];
        }

        public int Choix_random(int total)
        {
            Random random = new Random();
            return random.Next(total);       //Index choisi au hasard parmi la liste
        }
    }
}
