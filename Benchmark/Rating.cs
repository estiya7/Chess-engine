using IA_echecs;
using Regles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Benchmark
{
    public class Rating
    {
        private Moteur moteur;
        private Reflexion engine_1;
        private Engine_comp engine_2;
        private float score;
        private float[] résultats;
        private List<string>[] parties;


        public Moteur Moteur
        {
            get { return moteur; }
            private set { moteur = value; }
        }
        public Reflexion Engine_1
        {
            get { return engine_1; }
            private set { engine_1 = value; }
        }
        public Engine_comp Engine_2
        {
            get { return engine_2; }
            private set { engine_2 = value; }
        }

        public float Score
        {
            get { return score; }
            set { score = value; }
        }

        public float[] Resultats
        {
            get { return résultats; }
            set { résultats = value; }
        }
        public List<string>[] Parties
        {
            get { return parties; }
            set { parties = value; }
        }

        public Rating()
        {
            moteur = new Moteur();
            engine_1 = new Reflexion(Moteur, true);
            engine_2 = new Engine_comp(Moteur, false);
            parties = new List<string>[10];
            score = 0;
            résultats = new float[10];
        }

        public Rating(int nombre_parties)
        {
            moteur = new Moteur();
            engine_1 = new Reflexion(Moteur, true);
            engine_2 = new Engine_comp(Moteur, false);
            parties = new List<string>[nombre_parties];
            Score = 0;
            résultats = new float[nombre_parties];
        }

        public Moteur GetMoteur()
        {
            return Moteur;
        }




        //PB de détection sur un pion random sur capture d'un pion de la même couleur d'un cavalier
        //Sur Position 2 : quand cavalier 35 prend en 51, pion g (46) n'avance plus et quand cavalier prend en 53, pion d (51) n'avance plus
        //QUE EN SIMULATION, PAS QUAND ON FAIT LES COUPS SANS LA FONCTION D'ANNULATION

        public static int[] NbCoupsProfondeur = new int[9];
        public static int[] NbCoupsParCoup = new int[48];
        public static (int, int)[] LegendeCoup = new (int, int)[48];
        static int compteur = 0;
        const int profondeur_max = 6;
        public static int TestNbLegaux(Moteur moteur, int profondeur = profondeur_max)
        {
            int compt = 1;
            //Stopwatch sw = new Stopwatch();
            NbCoupsProfondeur[profondeur_max - profondeur] += 1;
            if (profondeur == 0)
            {
                return compt;
            }

            Moteur.Coup[] légaux_position = moteur.Calcul_légaux();
            int nombre_coups = moteur.NombreLegaux;

            /*
            if (profondeur == 2)
            {
                Debug.WriteLine("Référence coups : ");
                Debug.WriteLine($"En 53 : piece.numéro = {moteur.pieces[51].numéro} et légaux = {légaux_position[51]:B64}");
                Debug.WriteLine($"En 51 : piece.numéro = {moteur.pieces[46].numéro} et légaux = {légaux_position[46]:B64}");
            }
            if (moteur.pieces[51].numéro == 5 || moteur.pieces[53].numéro == 5)
            {
                Debug.WriteLine($"Cas cavalier proche de roi, coups légaux : ");
                for (int i = 0; i < 64; i++)
                {
                    Debug.WriteLine($"{i}. {légaux_position[i]:B64}");
                }
                Debug.WriteLine($"\n\nLes pièces : {moteur.Pieces_long_blanc:B64} et {moteur.Pieces_long_noir:B64}");
                Debug.WriteLine($"En 53 : piece.numéro = {moteur.pieces[51].numéro} et légaux = {légaux_position[51]:B64}");
                Debug.WriteLine($"En 51 : piece.numéro = {moteur.pieces[46].numéro} et légaux = {légaux_position[46]:B64}");
            }
            */
            //Reflexion.AffichageCaractéristiquesMoteur(moteur);
            
            for (int rang = 0; rang < nombre_coups; rang++)
            {
                Moteur.Coup coup = légaux_position[rang];
                bool rb = moteur.Roque_blanc; bool rlb = moteur.Roque_long_blanc; bool rn = moteur.Roque_noir; bool rln = moteur.Roque_long_noir;
                string dernier_coup = moteur.DernierCoup; int compt50coups = moteur.Compteur_50coups;
                int piece_prise = moteur.pieces[coup.arrivee];

                bool checkmate = moteur.Realisation_coup_logique(coup.départ, coup.arrivee);
                bool promotion = moteur.Promotion_bool;

                if (promotion)
                {
                    compt += TestNbLegaux(moteur, profondeur - 1);
                    if (profondeur == profondeur_max) { NbCoupsParCoup[compteur]--; LegendeCoup[compteur] = (coup.départ, coup.arrivee); compteur++; }
                    moteur.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, true, rb, rlb, rn, rln, compt50coups);

                    moteur.Realisation_coup_logique(coup.départ, coup.arrivee, 1);
                    compt += TestNbLegaux(moteur, profondeur - 1);
                    if (profondeur == profondeur_max) { NbCoupsParCoup[compteur]--; LegendeCoup[compteur] = (coup.départ, coup.arrivee); compteur++; }
                    moteur.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, true, rb, rlb, rn, rln, compt50coups);

                    moteur.Realisation_coup_logique(coup.départ, coup.arrivee, 2);
                    compt += TestNbLegaux(moteur, profondeur - 1);
                    if (profondeur == profondeur_max) { NbCoupsParCoup[compteur]--; LegendeCoup[compteur] = (coup.départ, coup.arrivee); compteur++; }
                    moteur.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, true, rb, rlb, rn, rln, compt50coups);

                    moteur.Realisation_coup_logique(coup.départ, coup.arrivee, 3);
                }

                compt += TestNbLegaux(moteur, profondeur - 1);
                NbCoupsParCoup[compteur]++;
                //Debug.WriteLine($"On annule le coup en faisant {coup} --> {i}");
                moteur.Annulation_Realisation_coup(coup.arrivee, coup.départ, piece_prise, promotion, rb, rlb, rn, rln, compt50coups);

                if (profondeur != 1 && profondeur < profondeur_max) { NbCoupsParCoup[compteur]--; }
                if (profondeur == profondeur_max) { NbCoupsParCoup[compteur]--; LegendeCoup[compteur] = (coup.départ, coup.arrivee); compteur++; }
            }
            return compt;
        }

        public bool Evaluation()
        {
            bool Couleur = true;
            for (int partie = 0; partie < Parties.Length; partie++)
            {
                float résultat = 0;
                try
                {
                    résultat = FairePartie(Couleur, partie);   //Donne le résultat du blanc, 1, 0 ou 0.5
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }

                Score += Couleur ? résultat : 1 - résultat;   //Si Couleur, Reflexion 1 est blanc et inversement sinon

                Couleur = !Couleur;   //On alterne les couleurs à chaque partie
            }
            return true;
        }


        // 1 <=> Reflexion gagne ; 0 <=> engine_comp gagne  et 0.5 nulle
        public float FairePartie(bool Couleur, int index_partie)
        {
            bool vainqueur = false;
            while (Moteur.PartieFinie == false)
            {
                if (Couleur)     //Au premier tour, couleur indique les blancs
                {
                    Moteur.Coup coup_1 = Engine_1.CoupRandom();
                    vainqueur = Moteur.Realisation_coup_logique(coup_1.départ, coup_1.arrivee);
                }
                else
                {
                    Moteur.Coup coup_2 = Engine_2.CoupRandom();
                    vainqueur = Moteur.Realisation_coup_logique(coup_2.départ, coup_2.arrivee);
                }
                Couleur = !Couleur;
            }
            Parties[index_partie] = Moteur.Partie;
            Moteur.Reset_partie();
            if (vainqueur)   //On sait que un des deux a gagné
            {
                return Couleur ? 0 : 1; //La couleur est inversé donc si Couleur = false, les blancs ont gagné
            }
            return 0.5f;
        }

        public static void PerftMoteur()
        {
            Moteur moteur = new Moteur("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int nb_coups = TestNbLegaux(moteur);

            sw.Stop();
            Debug.WriteLine($"Profondeur 1 {nb_coups - 1} coups : temps écoulé = " + sw.Elapsed);

            int[] NombreCoupsProfondeurs = Rating.NbCoupsProfondeur;
            for (int i = 0; i < NombreCoupsProfondeurs.Length; i++)
            {
                Console.WriteLine($"Nombre de positions disponibles après {i} plis : {NombreCoupsProfondeurs[i]}");
            }
            Console.WriteLine("\n\n\n");
            for (int i = 0; i < NbCoupsParCoup.Length; i++)
            {
                Console.WriteLine($"Nombre de coups dans le coup légal {LegendeCoup[i]} : {NbCoupsParCoup[i]}");
            }
        }





        //4k3/p1pp1p2/4p1p1/3PN3/8/8/8/4K3 w -   //Isolation pions et cavalier blanc
        //4k3/3p4/6p1/4N3/8/8/8/4K3 w -  Variante isolation pion g sans f
        //ELEMENT DIFFERENCIANT : L'AJOUT DU PION C

        //8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1 

        //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1  Position de départ

        //r3k2r/p1pNqpb1/bn2pnp1/3P4/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - //Original + Cxd7
        //r3k2r/p1ppqNb1/bn2pnp1/3P4/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - //Original + Cxf7   //44 coups en profondeur 1
        //r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - //Original (Position 2 Kiwipete)
        private static void Main(string[] args)
        {
            PerftMoteur();


            //Pour mettre le nom d'un programme dans la base
            /*
            int elo = 100;
            string nom_1 = "random";
            string nom_2 = "Matérialiste";
            Sauvegarde_database.Sauvegarde_programme(nom_1, elo);
            Sauvegarde_database.Sauvegarde_programme(nom_2, elo);
            */
            //TEST POUR LA DATABASE QUI MARCHE
            /*
            List<string>[] parties = new List<string>[3];
            parties[0] = new List<string>();
            parties[1] = new List<string>();
            parties[2] = new List<string>();

            float[] resultats = new float[3];
            float score = 2f;
            int moteur_1 = 1;
            int moteur_2 = 1;
            parties[0].Add("p0_coup1");
            parties[0].Add("p0_coup2");
            parties[0].Add("p0_coup3");
            parties[0].Add("p0_coup4");
            parties[1].Add("p1_coup1");
            parties[1].Add("p1_coup2");
            parties[1].Add("p1_coup3");
            parties[1].Add("p1_coup4");
            parties[1].Add("p1_coup5");
            parties[2].Add("p2_coup1");
            parties[2].Add("p2_coup2");
            parties[2].Add("p2_coup3");
            resultats[0] = 1;
            resultats[1] = 0.5f;
            resultats[2] = 0.5f;
            Sauvegarde_database.Sauvegarde_affrontement(parties, resultats, score, moteur_1, moteur_2);
            */
        }
    }
}
