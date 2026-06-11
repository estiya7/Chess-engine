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
    public class Engine_random
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

        public Engine_random(Moteur moteur, bool couleur)
        {
            mot = moteur;
            Couleur = couleur;
        }

        public Coup CoupRandom()
        {
            Coup[] légaux = Mot.Calcul_légaux();   //Récupère tous les coups légaux avec case de départ associé
            if (légaux.Length == 0)
            {
                return Moteur.CreerCoup(0, 0);
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

    public class Engine_materialiste
    {
        private Moteur mot;
        private Moteur mot_test;
        private bool couleur;
        int[] pieces;

        public Moteur Mot
        {
            get { return mot; }
            set { mot = value; }
        }
        public bool Couleur
        {
            get { return couleur; }
            set { couleur = value; }
        }
        private Moteur MoteurTest
        {
            get { return mot_test; }
            set { mot_test = value; }
        }


        public Engine_materialiste(Moteur moteur, bool couleur)
        {
            mot = moteur;
            mot_test = new Moteur();
            Couleur = couleur;
            pieces = new int[64];
            pieces = moteur.pieces;
        }

        public Moteur CopieMoteur()
        {
            Moteur copie = new Moteur(mot);
            return copie;
        }


        public int Valeur_piece(int piece)
        {
            if (piece == 6 || piece == 106)
            {
                return 100;
            }
            else if (piece == 3 || piece == 103)
            {
                return 500;
            }
            else if (piece == 4 || piece == 104)
            {
                return 300;
            }
            else if (piece == 5 || piece == 105)
            {
                return 300;
            }
            else if (piece == 2 || piece == 102)
            {
                return 900;
            }
            return 0;
        }
        public Coup MeilleurCoup(int profondeur_max = 1)
        {
            mot_test = CopieMoteur();

            Coup coup = SetupSearch(profondeur_max, mot.Blanc);

            return coup;
        }

        public Coup SetupSearch(int profondeur_max, bool maximiser)
        {
            int eval_opti = maximiser ? int.MinValue : int.MaxValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Coup coup_opti = Moteur.CreerCoup(0, 0);

            Coup[] coups_initiaux = MoteurTest.Calcul_légaux();
            int nombre_max = MoteurTest.NombreLegaux;

            for (int rang = 0; rang < nombre_max; rang++)
            {
                Coup coup = coups_initiaux[rang];
                bool rb = MoteurTest.Roque_blanc;
                bool rlb = MoteurTest.Roque_long_blanc;
                bool rn = MoteurTest.Roque_noir;
                bool rln = MoteurTest.Roque_long_noir;
                string dernier_coup = MoteurTest.DernierCoup;
                int compt50coups = MoteurTest.Compteur_50coups;
                int piece_prise = MoteurTest.pieces[coup.arrivee];

                bool checkmate = MoteurTest.Realisation_coup_logique(coup.départ, coup.arrivee);
                bool promotion = MoteurTest.Promotion_bool;
                if (checkmate)
                {
                    return coup;
                }

                int eval = Search(profondeur_max, alpha, beta, !maximiser);
                MoteurTest.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, promotion, rb, rlb, rn, rln, compt50coups);

                if (maximiser)
                {
                    if (eval > alpha)
                    {
                        alpha = eval;
                        coup_opti = coup;
                    }
                    else if (eval == alpha)
                    {
                        int random = Choix_random(10);
                        if (random == 0)
                        {
                            coup_opti = coup;
                        }
                    }
                }
                else
                {
                    if (eval < beta)
                    {
                        beta = eval;
                        coup_opti = coup;
                    }
                    else if (eval == beta)
                    {
                        int random = Choix_random(10);
                        if (random == 0)
                        {
                            coup_opti = coup;
                        }
                    }
                }
            }
            return coup_opti;
        }


        public int Search(int profondeur, int alpha, int beta, bool maximiser)   //Recherche minimax, retourne la best eval d'une profondeur
        {
            if (profondeur == 0)
            {
                int eval_finale_depth0 = Evaluation();
                return eval_finale_depth0;
            }
            Coup[] coups_disponibles = MoteurTest.Calcul_légaux();
            int nombre_max = MoteurTest.NombreLegaux;
            for (int rang = 0; rang < nombre_max; rang++)
            {
                Coup coup_joué = coups_disponibles[rang];
                bool rb = MoteurTest.Roque_blanc;
                bool rlb = MoteurTest.Roque_long_blanc;
                bool rn = MoteurTest.Roque_noir;
                bool rln = MoteurTest.Roque_long_noir;
                string dernier_coup = MoteurTest.DernierCoup;
                int compt50coups = MoteurTest.Compteur_50coups;
                int eval_coup_joué = 0;
                int piece_prise = MoteurTest.pieces[coup_joué.arrivee];

                bool checkmate = MoteurTest.Realisation_coup_logique(coup_joué.départ, coup_joué.arrivee);
                bool promotion = MoteurTest.Promotion_bool;

                if (MoteurTest.PartieFinie)
                {
                    if (checkmate)
                    {
                        eval_coup_joué = MoteurTest.Blanc ? int.MinValue : int.MaxValue;  //Blanc <=> echec et mat noir et inverse
                    }
                    else
                    {
                        eval_coup_joué = 0;
                    }
                }
                else
                {
                    eval_coup_joué = Search(profondeur - 1, alpha, beta, !maximiser);   //Eval max des tours précédents
                }

                MoteurTest.Annulation_Realisation_coup(coup_joué.arrivee, coup_joué.départ, piece_prise, promotion, rb, rlb, rn, rln, compt50coups);

                if (maximiser == false)    //Si on est à une profondeur noire, on cherche tous les coups pour le meilleur
                {
                    if (eval_coup_joué < beta)
                    {
                        beta = eval_coup_joué;
                    }
                }
                else                    //Si on est à une profondeur blanche, on cherche le plus petit score
                {
                    if (eval_coup_joué > alpha)    //Donc on maximise
                    {
                        alpha = eval_coup_joué;
                    }
                }
                if (alpha >= beta)
                {
                    break;
                }
            }
            int eval_finale = maximiser ? alpha : beta;
            return eval_finale;
        }


        public int Evaluation()
        {
            if (MoteurTest.PartieFinie)
            {
                if (MoteurTest.InCheck)
                {
                    return MoteurTest.Blanc ? int.MinValue : int.MaxValue;
                }
                return 0;
            }
            int eval = Compte_matériel();
            return eval;
        }



        public int Compte_matériel()
        {
            int materiel = 0;

            for (int square = 0; square < 64; square++)
            {
                int piece = MoteurTest.pieces[square];
                if (piece < 10)
                {
                    materiel += Valeur_piece(piece);
                }
                if (piece > 100)
                {
                    materiel -= Valeur_piece(piece);
                }
            }
            return materiel;
        }

        public int Choix_random(int max)
        {
            Random random = new Random();
            return random.Next(max);       //Index choisi au hasard parmi la liste
        }

        //Pour l'instant, retourne toujours dame
        public int Promotion(int carré)
        {
            return 0;
        }
    }
}
