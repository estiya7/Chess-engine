using Regles;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using static Regles.Moteur;

namespace IA_echecs
{
    public class Reflexion
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
            get { return mot_test;}
            set { mot_test = value; }
        }


        public Reflexion(Moteur moteur, bool couleur)
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


        //Tableaux représentant la valeur d'une pièce selon sa case
        /*
        private int[] Valeur_case_roi = [-200, -200, -190, -190, -190, -190, -200, -200,    //1
                                        -170, -170, -170, -170, -170, -170, -170, -170,
                                        -140, -160, -160, -170, -170, -160, -160, -140,    //3
                                        -110, -140, -160, -170, -170, -160, -140, -110,
                                         -70, -110, -150, -160, -160, -150, -110,  -70,    //5
                                         -40,  -80, -130, -140, -140, -130,  -80,  -40,
                                           0,  -30,  -80, -120, -120,  -70,   40,   50,    //7
                                         100,   90,   60,  -40,   30,  -50,   70,   60];

        private int[] Valeur_case_reine =[-40, -30, -50, -50, -50, -50, -30, -40,    //1
                                         -30, -40, -30, -20, -20, -30,  10, -20,
                                          10,   0, -10, -20, -20, -10,   0,  10,    //3
                                          20,  30,  20,  10,  10,  20,  30,  20,
                                          40,  50,  40,  30,  30,  40,  50,  40,    //5
                                          20,  40,  40,  30,  30,  40,  40,  40,
                                         -20,  10,  50,  50,  50,  10, -20, -30,    //7
                                         -40, -30,   0,  30,  10, -10, -40, -50];

        private int[] Valeur_case_tour = [40,  50,  40,  30,  30,  40,  50,  40,    //1
                                         70,  70,  70,  70,  80,  70,  70,  70,
                                         50,  40,  30,  20,  20,  30,  40,  50,    //3
                                         30,  20,  10,   0,   0,  10,  20,  30,
                                         10,   0, -10, -20, -20,  10,  20,  30,    //5
                                         20,  40,  40,  30,  30,  40,  40,  40,
                                        -20,  10,  50,  50,  50,  10, -20, -30,    //7
                                          0,  20,  45,  50,  50,  40,  20,   0];

        private int[] Valeur_case_fou = [-60, -30, -50, -50, -50, -50, -50, -60,    //1
                                        -50, -40, -40, -30, -30, -40, -40, -50,
                                        -20,  10,  10,   0,   0,  10,  10,   0,    //3
                                        -10,  20,  30,   0,   0,  30,  20,   0,
                                          40,  50,  40,  30,  30,  40,  50,  40,    //5
                                          20,  40,  40,  30,  30,  40,  40,  40,
                                         -20,  10,  20,   0,   0,  10,  50, -20,    //7
                                         -40, -30, -40, -30, -30, -40, -40, -10];

        private int[] Valeur_case_cavalier = [-40, -20, -10,  0,   0, -10, -20, -40,    //1
                                             -20, -10,   0,  10,  10,   0, -10, -20,
                                             -10,   0,  20,  30,  30,  20,   0, -10,    //3
                                               0,  20,  30,  50,  50,  30,  20,   0,
                                               0,  20,  30,  50,  50,  30,  20,   0,    //5
                                             -10,   0,  20,  30,  30,  20,   0, -10,
                                             -20, -10,   0,  10,  10,   0, -10, -20,    //7
                                             -40, -20, -10,   0,   0, -10, -20, -40];

        private int[] Valeur_case_pion = [  0,   0,   0,   0,   0,   0,   0,   0,    //1
                                          60,  60,  60,  60,  60,  60,  60,  60,
                                          50,  40,  40,  40,  40,  40,  40,  50,    //3
                                          30,  10,  20,  30,  30,  20,  10,  20,
                                          10,   0,  40,  40,  40,  40, -30,  10,    //5
                                          40,  20,  50,  30,  30,  50,  20,  30,
                                          40,   0,  20,   0,   0,  20,   0,  40,    //7
                                           0,   0,   0,   0,   0,   0,   0,   0,];
        */

        public List<int> Cases_attaque_carré(int carré)
        {
            List<int> liste_attaque = new List<int>();
            int ligne_carré = carré / 8;
            int colonne_carré = carré % 8;
            int diff_carré = ligne_carré - colonne_carré;
            int square = -1;
            int ligne = 0;
            int colonne = 0;
            for (ligne = 0; ligne < 8;  ligne++)
            { 
                for (colonne = 0; colonne < 8; colonne++)
                {
                    square++;
                    int diff = ligne - colonne;
                    int colonne_inverse = Math.Abs(colonne - 7);
                    int diff_inverse = ligne - colonne_inverse;
                    if (diff == diff_carré)
                    {
                        liste_attaque.Add(square);
                    }
                    if (ligne == ligne_carré || colonne == colonne_carré)
                    {
                        liste_attaque.Add(square);
                    }
                    if (ligne - colonne_inverse == diff)
                    {
                        liste_attaque.Add(square);
                    }
                }
            }
            int[] cavalier = [carré - 17, carré - 15, carré - 10, carré - 6, carré + 6, carré + 10, carré + 15, carré + 17];
            foreach (int coup in cavalier)
            {
                ligne = coup / 8;
                colonne = coup % 8;
                if (coup < 0 || coup > 63)
                {
                    if (Math.Abs(colonne - colonne_carré) > 2 == false)
                    {
                        liste_attaque.Add(coup);
                    }
                }
            }
            return liste_attaque;
        }


        public void Get_Moteur_IA(Moteur moteur)
        {
            mot = moteur;
        }

        //Retourne la valeur supplémentaire de la pièce selon sa case
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



        public Coup MeilleurCoup()
        {
            Debug.WriteLine($"On commence à trouver le meilleur coup avec couleur = {mot.Blanc}");

            mot_test = CopieMoteur();

            int profondeur_max = 2;
            Coup coup = SetupSearch(profondeur_max, mot.Blanc);

            Debug.WriteLine("On à finir de trouver le meilleur coup");
            return coup;
        }

        public Coup SetupSearch(int profondeur_max, bool maximiser)
        {
            int eval_opti = maximiser ? int.MinValue : int.MaxValue;
            Coup coup_opti = MoteurTest.CreerCoup(0, 0);

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

                int eval = Search(profondeur_max, int.MinValue, int.MaxValue, maximiser);
                Debug.WriteLine($"Pour le coup {coup.départ} --> {coup.arrivee}, on eval = {eval}");
                MoteurTest.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, promotion, rb, rlb, rn, rln, compt50coups);

                if ((eval >= eval_opti) == maximiser)
                {
                    eval_opti = eval;
                    coup_opti = coup;
                }
            }
            return coup_opti;
        }


        //ATTENTION : Quand la profondeur change, il faut update certaines comparaisons
        public int Search(int profondeur, int alpha, int beta, bool maximiser)   //Recherche minimax, retourne la best eval d'une profondeur
        {
            if (profondeur == 0)
            {
                return Evaluation();
            }
            Coup[] coups_disponibles = MoteurTest.Calcul_légaux();
            int nombre_max = MoteurTest.NombreLegaux;
            for (int rang = 0; rang < nombre_max; rang++)
            {
                Coup coup_joué = coups_disponibles[rang];
                bool rb = MoteurTest.Roque_blanc; bool rlb = MoteurTest.Roque_long_blanc; bool rn = MoteurTest.Roque_noir; bool rln = MoteurTest.Roque_long_noir;
                string dernier_coup = MoteurTest.DernierCoup; int compt50coups = MoteurTest.Compteur_50coups;
                int piece_prise = MoteurTest.pieces[coup_joué.arrivee];

                bool checkmate = MoteurTest.Realisation_coup_logique(coup_joué.départ, coup_joué.arrivee);
                if (checkmate)
                {
                    return MoteurTest.Blanc ? int.MinValue : int.MaxValue;
                }
                bool promotion = MoteurTest.Promotion_bool;

                int eval_coup_joué = Search(profondeur - 1, alpha, beta, !maximiser);   //Eval max des tours précédents

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
            return maximiser ? alpha : beta;
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
                int piece = mot.pieces[square];
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




        public Coup CoupRandom()
        {
            Coup[] légaux = Mot.Calcul_légaux();   //Récupère tous les coups légaux avec case de départ associé

            if (légaux.Length == 0)
            {
                return Mot.CreerCoup(0, 0);
            }
            /*
            if (total == 0)
            {
                (List<ulong> resoudre, List<ulong> garder) masques = Mot.MasquesCheck();
                if (masques.resoudre.Count == 0)
                {
                    return (0, 0);
                }
                return (100, 100);
            }
            */

            int random = Choix_random(légaux.Length);    //On a xx 1 dans les 64 ulong, on choisi un nombre qui correspond à un 1

            Coup coup_random = légaux[random];
            return coup_random;
        }


        public static void AffichageCaractéristiquesMoteur(Moteur moteur)
        {
            Debug.WriteLine("Caractéristiques du moteur : ");
            Debug.WriteLine($"Les pièces : {moteur.Pieces_long:B64}");
            Debug.WriteLine($"Etats des booléens : inCheck = {moteur.InCheck}, les roques : {moteur.Roque_blanc} {moteur.Roque_long_blanc} {moteur.Roque_noir} {moteur.Roque_long_noir}");
            Debug.WriteLine($"tour blanc : {moteur.Blanc}, Cases roi : {moteur.CaseRoiBlanc} {moteur.CaseRoiNoir}");
        }
    }
}
