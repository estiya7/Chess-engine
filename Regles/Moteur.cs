using System.Collections;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static Regles.Moteur;

namespace Regles
{
    public class Moteur
    {

        private List<string> partie;
        private string dernier_coup;
        private int compteur_50coups;
        private bool partie_finie;

        private bool blanc;
        private bool in_check;

        private bool roque_long_blanc;
        private bool roque_long_noir;
        private bool roque_noir;
        private bool roque_blanc;

        private bool en_passant_bool;   //Savoir si en passant a été réalisé au dernier coup
        private int potentiel_en_passant;
        private bool promotion_bool;

        private int case_roi_blanc;
        private int case_roi_noir;

        private ulong pieces_long;
        private ulong[] bitboards_blanc;
        private ulong[] bitboards_noir;
        private ulong[] légaux;
        private int nombre_légaux;

        private ulong[] filtres_colonne;
        private ulong[] masques_direction;
        private ulong somme_masques_echecs;
        private ulong checkers;
        private ulong masque_echec_direct;

        private bool pat;


        public List<string> Partie
        {
            get { return this.partie; }
            set { this.partie = value; }
        }
        public string DernierCoup
        {
            get { return this.dernier_coup;}
            set { this.dernier_coup = value; }
        }
        public int Compteur_50coups
        {
            get { return this.compteur_50coups;}
            set { this.compteur_50coups = value; }
        }
        public bool PartieFinie
        {
            get { return this.partie_finie; }
            set { this.partie_finie = value; }
        }
        public bool Blanc
        {
            get { return this.blanc; }
            set { this.blanc = value; }
        }
        public bool InCheck
        {
            get { return in_check; }
            set { in_check = value; }
        }
        public bool Roque_long_blanc
        {
            get { return this.roque_long_blanc; }
            set { this.roque_long_blanc = value; }
        }
        public bool Roque_long_noir
        {
            get { return this.roque_long_noir; }
            set { this.roque_long_noir = value; }
        }
        public bool Roque_blanc
        {
            get { return this.roque_blanc; }
            set { this.roque_blanc = value; }
        }
        public bool Roque_noir
        {
            get { return this.roque_noir; }
            set { this.roque_noir = value; }
        }
        public bool EnPassantBool // True <=> en_passant a été effectué sur le tour en cours de traitement dans RéalisationCoupLogique
        {
            get { return this.en_passant_bool; }
            set { this.en_passant_bool = value; }
        }
        public int PotentielEnPassant
        {
            get { return potentiel_en_passant; }
            set { potentiel_en_passant = value; }
        }
        public bool Promotion_bool
        {
            get { return this.promotion_bool; }
            set { this.promotion_bool = value; }
        }
        public int CaseRoiBlanc
        {
            get { return this.case_roi_blanc; }
            set { this.case_roi_blanc = value; }
        }
        public int CaseRoiNoir
        {
            get { return this.case_roi_noir; }
            set { this.case_roi_noir = value; }
        }
        public ulong Pieces_long
        {
            get { return pieces_long; }
            set { pieces_long = value; }
        }
        public ulong[] Bitboards_blanc
        {
            get { return bitboards_blanc; }
            set { bitboards_blanc = value; }
        }
        public ulong[] Bitboards_noir
        {
            get { return bitboards_noir; }
            set { bitboards_noir = value; }
        }


        //Légaux possède les coups semi-légaux, sans les échecs
        public ulong[] PseudoLégaux
        {
            get { return légaux; }
            set { légaux =  value; }
        }
        public int NombreLegaux
        {
            get { return nombre_légaux; }
            set { nombre_légaux = value; }
        }

        public ulong[] Filtres_colonne  //Met un filtre sur l'échiquier composé de 1 sur les colonnes à gauche, de 0 sinon (Filtres[3] => 1 sur les colonnes 0, 1, 2)
        {
            get { return filtres_colonne; }
            set { filtres_colonne = value; }
        }

        public ulong[] Masques_echecs  //Taille 9, un masque dans chaque direction, partant de la diagonale basse gauche (offset = -9) puis sens anti-horaire, avec un masque d'echecs direct en [8]. Recalculés à chaque pli et utilisé pour les deux camps
        {
            get { return  masques_direction; }
            set { masques_direction = value; }
        }
        public ulong SommeMasquesEchecs  //64 bits, union logique des masques précédent
        {
            get {  return somme_masques_echecs; }
            set { somme_masques_echecs = value; }
        }
        public ulong Checkers  //64 bits, 1 aux bits correspondant aux cases des pièces qui font échecs
        {
            get { return checkers; }
            set { checkers = value; }
        }
        public ulong MasqueEchecDirect   //Recalculer à chaque coup, représente les cases entre les pièces qui font echec et le roi (pieces comprises)
        {
            get { return masque_echec_direct; }
            set { masque_echec_direct = value; }
        }

        public bool Pat
        {
            get { return pat; }
            set { pat = value; }
        }


        public int[] pieces;

        public struct Coup
        {
            public int départ;
            public int arrivee;
        }

        public Coup CreerCoup(int d, int a)
        {
            Coup coup;
            coup.départ = d;
            coup.arrivee = a;
            return coup;
        }
        public Moteur()
        {
            pieces = new int[64];
            bitboards_blanc = new ulong[7];
            bitboards_noir = new ulong[7];
            partie = new List<string>();
            légaux = new ulong[64];
            dernier_coup = "";
            filtres_colonne = new ulong[8];
            masques_direction = new ulong[9];
            Reset_partie();
        }

        public Moteur(Moteur mot_modele)
        {
            this.partie = new List<string>();
            for (int i = 0; i < Partie.Count; i++)
            {
                partie.Add(mot_modele.Partie[i]);
            }
            this.bitboards_blanc = new ulong[7];
            this.bitboards_noir = new ulong[7];
            for (int i = 0; i < 7; i++)
            {
                Bitboards_blanc[i] = mot_modele.Bitboards_blanc[i];
                Bitboards_noir[i] = mot_modele.Bitboards_noir[i];
            }
            this.pieces = new int[64];
            this.dernier_coup = mot_modele.dernier_coup;
            this.compteur_50coups = mot_modele.compteur_50coups;
            this.partie_finie = mot_modele.partie_finie;
            this.blanc = mot_modele.blanc;
            this.in_check = mot_modele.InCheck;
            this.roque_blanc = mot_modele.Roque_blanc;
            this.roque_long_blanc = mot_modele.Roque_long_blanc;
            this.roque_noir = mot_modele.Roque_noir;
            this.roque_long_noir = mot_modele.Roque_long_noir;
            this.case_roi_blanc = mot_modele.CaseRoiBlanc;
            this.case_roi_noir = mot_modele.CaseRoiNoir;
            this.pieces_long = mot_modele.pieces_long;

            this.légaux = new ulong[64];
            for (int i =  0; i < 64; i++)
            {
                légaux[i] = mot_modele.légaux[i];
                pieces[i] = mot_modele.pieces[i];
            }

            this.somme_masques_echecs = mot_modele.somme_masques_echecs;
            this.masque_echec_direct = mot_modele.masque_echec_direct;
            this.checkers = mot_modele.Checkers;

            this.filtres_colonne = new ulong[8];
            for (int i = 0; i < 8; i++)
            {
                filtres_colonne[i] = mot_modele.filtres_colonne[i];
            }
            this.masques_direction = new ulong[9];
            for (int i = 0; i < 9; i++)
            {
                masques_direction[i] = mot_modele.masques_direction[i];
            }
        }
        
        public Moteur(string FEN)
        {
            partie = new List<string>();
            dernier_coup = "";
            pieces = new int[64];
            bitboards_blanc = new ulong[7];
            bitboards_noir = new ulong[7];
            légaux = new ulong[64];
            masques_direction = new ulong[9];
            filtres_colonne = new ulong[8];
            TraductionFEN(FEN);
        }
        
        
        public int Donneur_numéro(int carré)
        {
            return pieces[carré];
        }

        //Donne le offset d'une pièce par rapport à l'autre (faut être sûr que l'offset est possible, renvoie 7 en dernier)
        public int Offset(int carré, int valeur)
        {
            int ligne = carré / 8;
            int colonne = carré % 8;
            int ligne_valeur = valeur / 8;
            int colonne_valeur = valeur % 8;

            if (ligne == ligne_valeur)
            {
                if (carré > valeur)
                {
                    return -1;
                }
                return 1;
            }
            if (colonne == colonne_valeur)
            {
                if (carré > valeur)
                {
                    return -8;
                }
                return 8;
            }
            if ((carré - valeur) % 9 == 0)
            {
                if (carré > valeur)
                {
                    return -9;
                }
                return 9;
            }
            if (carré > valeur)
            {
                return -7;
            }
            return 7;
        }
        public string Notation_coup(int piece, int piece_prise, int carré_précédent, int carré)
        {
            int piece_num = piece % 10;
            string notation = "";
            int ligne = carré / 8;
            int colonne = carré % 8;
            int ligne_précédente = carré_précédent / 8;
            int colonne_précédente = carré_précédent % 8;
            if (piece_num == 1) { notation += "R"; }  //Ajoute dans la notation la pièce du coup
            if (piece_num == 2) { notation += "D"; }
            if (piece_num == 3) { notation += "T"; }
            if (piece_num == 4) { notation += "F"; }
            if (piece_num == 5) { notation += "C"; }

            notation += ColonneEnLettre(colonne_précédente);
            notation += ligne_précédente + 1;
            if (piece_prise > 0)            //Rajoute les captures de piece
            {
                notation += "x";
            }
            if (EnPassantBool == true)  //Rajoute la capture d'en passant
            {
                notation += ColonneEnLettre(colonne) + "x";
            }
            notation += ColonneEnLettre(colonne);
            notation += ligne + 1;
            //Debug.WriteLine(notation);
            return notation;
        }


        //Notations coup entier : Exemple avec Txa4, Dd4, e5 et bxc5 (Notations classiques) partants de a1, d2, e7 et b4
        /*
        public string Notation_coup_entier(Piece piece, Piece piece_prise, int carré_précédent, int carré)
        {
            string notation = Notation_coup(piece, piece_prise, carré, carré_précédent, false);   //Txa1, Dd2, e7, bxb4
            notation += Notation_coup(piece, piece_prise, carré_précédent, carré, false);         //Txa4, Dd4, e5, bxc5  --> Txa1Txa4, Dd2Dd4, e7e5, bxb4bxc5
            if (notation.Length == 6) { notation = notation.Substring(0, 3) + notation.Substring(4, 2); }   // Dd2d4  (Dame d2 -> d4)
            if (notation.Length == 8)
            {
                if (piece.Unicode == "\u2659")
                {
                    notation = notation.Substring(2, 2) + notation.Substring(5);  //b4xc5   (b4 prend c5    avec des pions)
                }
                else
                {
                    notation = notation[0] + notation.Substring(2, 2) + notation.Substring(5);   //Ta1xa4   (Tour en a1 prend a4)
                }
            }
            return notation;  //Ta1xa4, Dd2d4, e7e5, b4xc5
        }
        */

        public char ColonneEnLettre(int colonne)
        {
            if (colonne == 0) { return 'a'; }
            if (colonne == 1) { return 'b'; }
            if (colonne == 2) { return 'c'; }
            if (colonne == 3) { return 'd'; }
            if (colonne == 4) { return 'e'; }
            if (colonne == 5) { return 'f'; }
            if (colonne == 6) { return 'g'; }
            return 'h';
        }

        public ulong Shift(ulong value, int shift)
        {
            if (shift >= 0)
            {
                return value << shift;
            }
            return value >> -shift;
        }



        //Calcul des coups_légaux de la couleur Blanc (variable globale)
        public Coup[] Calcul_légaux()
        {
            //Debug.WriteLine($"On me demande de calculer tous les coups légaux avec Blanc = {Blanc} et InCheck = {InCheck} et PotentielEnPassant = {PotentielEnPassant} et pieces = {pieces_long:B64} avec noir = {Pieces_long_noir:B64}");
            ulong p_amis = Blanc ? Bitboards_blanc[0] : Bitboards_noir[0];
            ulong p_ennemis = pieces_long ^ p_amis;
            ulong full_pieces_amis = p_amis;
            ulong full_pieces_ennemis = p_ennemis;
            int CaseRoi = Blanc ? CaseRoiBlanc : CaseRoiNoir;
            int colonne_roi = CaseRoi % 8;
            ulong Légaux_ennemis = 0ul;
            int i = 0;
            int rang = 0;

            int masque = 0;
            Coup[] Coup_finaux = new Coup[100];
            ulong[] Légaux_couleur = new ulong[64];

            while (full_pieces_ennemis != 0ul)
            {
                i = BitOperations.TrailingZeroCount(full_pieces_ennemis);
                int colonne = i % 8;
                if (Blanc == false)
                {
                    if (pieces[i] == 6)
                    {
                        if (i < 56 && colonne != 0)
                        {
                            Légaux_ennemis |= 1ul << (i + 7);
                        }
                        if (i < 55 && colonne != 7)
                        {
                            Légaux_ennemis |= 1ul << (i + 9);
                        }
                    }
                    else
                    {
                        Légaux_ennemis |= PseudoLégaux[i];
                    }
                }
                else
                {
                    if (pieces[i] == 106)
                    {
                        if (i > 7 && colonne != 7)
                        {
                            Légaux_ennemis |= 1ul << (i - 7);
                        }
                        if (i > 8 && colonne != 0)
                        {
                            Légaux_ennemis |= 1ul << (i - 9);
                        }
                    }
                    else
                    {
                        Légaux_ennemis |= PseudoLégaux[i];
                    }
                }
                full_pieces_ennemis &= full_pieces_ennemis - 1;
            }

            while (full_pieces_amis != 0ul)
            {
                i = BitOperations.TrailingZeroCount(full_pieces_amis);
                //Debug.WriteLine($"La pièce en {i} est amie et les masques sont {SommeMasquesEchecs:B64} avec les pièces qui font échecs en {Checkers:B64}");
                if (((1ul << i) & SommeMasquesEchecs) == 0ul)  //Si la pièce ne fait pas partie des masques (INCLUS TOUJOURS LE ROI)
                {
                    //Debug.WriteLine($"La pièce en {i} n'est pas sur les masque");
                    if (InCheck)  //La pièce doit bloqué l'echec
                    {
                        Légaux_couleur[i] |= PseudoLégaux[i] & MasqueEchecDirect;
                    }
                    else   //La pièce fait ce qu'elle veut
                    {
                        //Debug.WriteLine($"La pièce en {i} fait ce qu'elle veut");
                        Légaux_couleur[i] = PseudoLégaux[i] & ~p_amis;
                    }
                }
                else  //On est dans un masque (<=> cloué)
                {
                    //Debug.WriteLine($"On est en {i} dans un masque");
                    if (InCheck)
                    {
                        for (masque = 0; masque < 8; masque++)
                        {
                            if (Masques_echecs[masque] != 0ul && ((1ul << i) & Masques_echecs[masque]) != 0)  //Pièce sur le masque
                            {
                                if ((~(1ul << i) & Masques_echecs[masque] & p_amis) == 0)
                                {
                                    Légaux_couleur[i] = 0;  //Pièce seule sur son masque (NE PEUT PAS BLOQUER L'ECHEC)
                                    masque = 10;
                                }
                                else
                                {
                                    masque = 7;  //Pièce pas seule sur son masque
                                }
                            }
                        }
                        if (masque == 8)  //Pas seule sur son masque
                        {
                            Légaux_couleur[i] = PseudoLégaux[i] & MasqueEchecDirect;
                        }
                    }
                    else   //Pas d'échecs mais cloué
                    {
                        for (masque = 0; masque < 8; masque++)
                        {
                            if (Masques_echecs[masque] != 0ul && ((1ul << i) & Masques_echecs[masque]) != 0)   //Si il existe un masque où la pièce bloque seule un echec (doit restée sur le masque)
                            {
                                //La pièce est la seule pièce de sa couleur sur le masque
                                if ((~(1ul << i) & Masques_echecs[masque] & p_amis) == 0)
                                {
                                    Légaux_couleur[i] = PseudoLégaux[i] & ~p_amis & Masques_echecs[masque];
                                    masque = 10;
                                }
                                else
                                {
                                    masque = 7;
                                }
                            }
                        }
                        if (masque == 8)
                        {
                            Légaux_couleur[i] = PseudoLégaux[i] & ~p_amis;
                        }
                    }
                }
                full_pieces_amis &= full_pieces_amis - 1;
            }


            if ((Checkers & (Checkers - 1ul)) != 0ul)  //Si il y a plusieurs echecs
            {
                Légaux_couleur = new ulong[64];
                Légaux_couleur[CaseRoi] = coups_légaux(CaseRoi);
                for (int masque_double_echec = 0; masque_double_echec < 8; masque_double_echec++)
                {
                    if (Masques_echecs[masque_double_echec] != 0ul)
                    {
                        if (masque_double_echec < 4)
                        {
                            Légaux_couleur[CaseRoi] &= ~(1ul << (CaseRoi + CaseRoi + BitOperations.LeadingZeroCount(Masques_echecs[masque_double_echec]) - 63));
                        }
                        else
                        {
                            Légaux_couleur[CaseRoi] &= ~(1ul << (CaseRoi + CaseRoi - BitOperations.TrailingZeroCount(Masques_echecs[masque_double_echec])));
                        }
                    }
                }
            }

            Légaux_couleur[CaseRoi] = coups_légaux(CaseRoi) & ~p_amis & ~Légaux_ennemis;
            Légaux_couleur[CaseRoi] |= Roque(CaseRoi, Légaux_ennemis);
            if (InCheck)
            {
                if (Masques_echecs[8] == 0ul)
                {
                    if ((BitOperations.TrailingZeroCount(checkers) - CaseRoi) > 0)   //La pièce fait echec au roi d'une case supérieure
                    {
                        Légaux_couleur[CaseRoi] &= ~(1ul << (CaseRoi + CaseRoi - BitOperations.TrailingZeroCount(MasqueEchecDirect)));
                    }
                    else
                    {
                        Légaux_couleur[CaseRoi] &= ~(1ul << (CaseRoi + CaseRoi + BitOperations.LeadingZeroCount(MasqueEchecDirect) - 63));
                    }
                }
            }
            if (PotentielEnPassant > 0)
            {
                if (PotentielEnPassant > 31)
                {
                    if (pieces[PotentielEnPassant - 7] == 6 && PotentielEnPassant != 47)
                    {
                        if (ClouageEnPassant(PotentielEnPassant - 7, true, p_ennemis) != 0ul)
                        {
                            Coup coup;
                            coup.départ = PotentielEnPassant - 7;
                            coup.arrivee = PotentielEnPassant;
                            Coup_finaux[rang] = coup;
                            rang++;
                        }
                    }
                    else if (pieces[PotentielEnPassant - 9] == 6 && PotentielEnPassant != 40)
                    {
                        if (ClouageEnPassant(PotentielEnPassant - 9, true, p_ennemis) != 0ul)
                        {
                            Coup coup;
                            coup.départ = PotentielEnPassant - 9;
                            coup.arrivee = PotentielEnPassant;
                            Coup_finaux[rang] = coup;
                            rang++;
                        }
                    }
                }
                else
                {
                    if (pieces[PotentielEnPassant + 7] == 106 && PotentielEnPassant != 16)
                    {
                        if (ClouageEnPassant(PotentielEnPassant + 7, false, p_ennemis) != 0ul)
                        {
                            Coup coup;
                            coup.départ = PotentielEnPassant + 7;
                            coup.arrivee = PotentielEnPassant;
                            Coup_finaux[rang] = coup;
                            rang++;
                        }
                    }
                    else if (pieces[PotentielEnPassant + 9] == 106 && PotentielEnPassant != 23)
                    {
                        if (ClouageEnPassant(PotentielEnPassant + 9, false, p_ennemis) != 0ul)
                        {
                            Coup coup;
                            coup.départ = PotentielEnPassant + 9;
                            coup.arrivee = PotentielEnPassant;
                            Coup_finaux[rang] = coup;
                            rang++;
                        }
                    }
                }
            }
            //Debug.WriteLine($"{Légaux_couleur[61]:B64}");
            for (int tour = 0; tour < 64; tour++)
            {
                ulong coups_dispo = Légaux_couleur[tour];
                while (coups_dispo != 0)
                {
                    Coup_finaux[rang].départ = tour;
                    Coup_finaux[rang].arrivee = BitOperations.TrailingZeroCount(coups_dispo);
                    rang++;
                    coups_dispo &= coups_dispo - 1;
                }
            }
            NombreLegaux = rang;
            return Coup_finaux;
        }



        //FONCTION DE COUPS_LEGAUX AVEC OPERATIONS BINAIRES LET HIM COOK
        //La fonction retourne les coups pseudo-légaux de la pièce, soit sans prendre en compte les echecs et les pièces amis (peut capturer la 1e pièce sur son chemin)

        public ulong coups_légaux(int square)
        {
            int piece = pieces[square];

            if (piece == 50)
            {
                return 0UL;
            }

            bool couleur = piece < 10;
            int numéro = piece % 10;

            ulong légaux = 0UL;
            int ligne = square / 8;
            int colonne = square % 8;
            ulong p_amis = couleur ? Bitboards_blanc[0] : Bitboards_noir[0];
            ulong p_ennemis = pieces_long ^ p_amis;

            //Ajoute les coups légaux en fonction de la piece
            if (numéro == 5)
            {
                légaux = légaux_cavalier(square, colonne);  //Les coups légaux dans les limites & les cases où il n'y a pas de pieces de la mm couleur
            }
            else if (numéro == 1)
            {
                légaux = légaux_roi(square, colonne);   //Egalement les coups autorisés (ECHECS NON COMPRIS ENCORE)
            }
            else if (numéro == 4)
            {
                légaux = légaux_fou(square, colonne);
            }
            else if (numéro == 2)
            {
                légaux = légaux_dame(square, ligne, colonne, couleur);
            }
            else if (numéro == 3)
            {
                légaux = légaux_tour(square, colonne);
            }
            else
            {
                légaux = légaux_pion(square, ligne, colonne, couleur, p_ennemis);
            }
            return légaux;
        }

        //Prend en compte les limites
        public ulong légaux_cavalier(int square, int colonne)
        {
            bool couleur = pieces[square] < 10;
            ulong légal = 0b00000000_00000000_00000000_00001010_00010001_00000000_00010001_00001010;   //Coups légaux du cavalier depuis la case 18
            if (colonne == 1)
            {
                légal = légal & 0b11111111_11111111_11111111_11111111_11111110_11111111_11111110_11111111;  //On supprime case 24 et 8
            }
            else if (colonne == 0)
            {
                légal = légal & 0b11111111_11111111_11111111_11111101_11111110_11111111_11111110_11111101;   //On supprime 24, 8, 33, 1
            }
            else if (colonne == 6)
            {
                légal = légal & 0b11111111_11111111_11111111_11111111_11101111_11111111_11101111_11111111;  //On supprime case 12 et 28
            }
            else if (colonne == 7)
            {
                légal = légal & 0b11111111_11111111_11111111_11110111_11101111_11111111_11101111_11110111;   //On supprime 3, 12, 28 et 35
            }
            légal = Shift(légal, square - 18);
            return légal;
        }

        //Prend en compte les limites, pas les échecs
        public ulong légaux_roi(int square, int colonne)
        {
            ulong légal = 0b00000000_00000000_00000000_00000000_00001110_00001010_00001110_00000000;   //Coups légaux du roi depuis la case 18
            if (colonne == 0)
            {
                légal = légal & 0b11111111_11111111_11111111_11111111_11111101_11111101_11111101_11111111;
            }
            if (colonne == 7)
            {
                légal = légal & 0b11111111_11111111_11111111_11111111_11110111_11110111_11110111_11111111;
            }
            légal = Shift(légal, square - 18);

            return légal;
        }

        //COMPLET : Prend en compte la couleur et la position
        public ulong légaux_pion(int square, int ligne, int colonne, bool couleur, ulong p_ennemis)
        {
            //Debug.WriteLine($"On me demande de mettre à jour les légaux du pion en {square}");
            ulong avance = 1UL;
            ulong légaux = couleur ? avance << square + 8 : Shift(avance, square - 8);
            légaux = légaux & ~pieces_long;   //Aucune pièce sur la case d'avancement
            if (légaux != 0UL)  //Si on peut avancer, on check l'avancement de 2 cases
            {
                if (couleur && ligne == 1)  //pion blanc sur la deuxième rangée
                {
                    légaux |= 1UL << square + 16;
                    légaux &= ~pieces_long;
                }
                if (couleur == false && ligne == 6)
                {
                    légaux |= 1UL << (square - 16);
                    légaux &= ~pieces_long;
                }
            }
            ulong capture = 5UL;
            if (colonne == 0)
            {
                capture -= 1UL;    //On enlève la capture gauche si colonne == 0
            }
            if (colonne == 7)
            {
                capture -= 4UL;    //idem pour la capture de droite
            }
            capture = couleur ? capture << square + 7 : Shift(capture, square - 9);   //Les décalages s'occupent des dépassement en < 0 ou >= 64

            capture &= p_ennemis;

            légaux = légaux | capture;   //On additionne avancement et captures
            return légaux;

        }

        public ulong légaux_fou(int square, int colonne)
        {
            ulong légaux = Direction_légaux_diag(square, colonne);
            //Debug.WriteLine($"Je recalcule les coups légaux du fou en {square}, et ils vallent {légaux:B64}");
            return légaux;
        }

        public ulong légaux_tour(int square, int colonne)
        {
            ulong légaux = Direction_légaux_droit(square, colonne);
            return légaux;
        }

        public ulong légaux_dame(int square, int ligne, int colonne, bool couleur)
        {
            return légaux_fou(square, colonne) | légaux_tour(square, colonne);
        }

        public ulong Direction_légaux_diag(int square, int colonne)
        {
            ulong filtre_colonne = Filtres_colonne[colonne];
            int décalage_négatif = 63 - square;
            ulong masque_n9 = 0b_00000000_01000000_00100000_00010000_00001000_00000100_00000010_00000001UL >> décalage_négatif;   //Le premier 1 est inutile, c'est la case de la pièce
            ulong masque_n7 = 0b_00000001_00000010_00000100_00001000_00010000_00100000_01000000_00000000UL >> décalage_négatif;   //7 cases maximums vues
            ulong masque_7 = 0b_00000000_00000010_00000100_00001000_00010000_00100000_01000000_10000000UL << square;
            ulong masque_9 = 0b_10000000_01000000_00100000_00010000_00001000_00000100_00000010_00000000UL << square;

            ulong legaux_n9 = masque_n9 & filtre_colonne;   //Le filtre est orienté vers la gauche, donc vers -9, -1, et -7
            ulong legaux_n7 = masque_n7 & ~filtre_colonne;
            ulong legaux_7 = masque_7 & filtre_colonne;  // Pour ceux vers la droite, on prend l'inverse (toujours 0 sur la colonne, assurés par les masques)
            ulong legaux_9 = masque_9 & ~filtre_colonne;

            int case_piece_n9 = 63 - BitOperations.LeadingZeroCount(masque_n9 & pieces_long);
            int case_piece_n7 = 63 - BitOperations.LeadingZeroCount(masque_n7 & pieces_long);
            int case_piece_7 = BitOperations.TrailingZeroCount(masque_7 & pieces_long);
            int case_piece_9 = BitOperations.TrailingZeroCount(masque_9 & pieces_long);
            //Debug.WriteLine($"Les carrés sont donc {case_piece_n9}, {case_piece_n7}, {case_piece_7} et {case_piece_9}");

            if (case_piece_n9 >= 0) legaux_n9 = (legaux_n9 >> case_piece_n9) << (case_piece_n9);
            if (case_piece_n7 >= 0) legaux_n7 = (legaux_n7 >> case_piece_n7) << (case_piece_n7);
            if (case_piece_7 < 64) legaux_7 = (legaux_7 << (63 - case_piece_7)) >> (63 - case_piece_7);
            if (case_piece_9 < 64) legaux_9 = (legaux_9 << (63 - case_piece_9)) >> (63 - case_piece_9);

            return legaux_n9 | legaux_n7 | legaux_7 | legaux_9;

            //Version v1, pareil en vitesse je pense
            /*
            int carré_check = square + offset;
            bool interference = false;
            ulong légal_dir = 0UL;
            while (interference == false)
            {
                if (carré_check < 0 || carré_check >= 64 || Math.Abs((carré_check - offset) / 8 - carré_check / 8) != 1) break;   //CONDITION D'ARRETS

                légal_dir |= 1UL << carré_check;   //On garde les carrés d'avant, on ajoute le carré à check
                légal_dir &= ~p_amis;    //Ils faut que les pièces amis ne soient pas là, donc ~p_long = 1, si ~p_long = 0, une pièce ami existe et on arrête, en retirant le carré_check
                if (((1UL << carré_check) & pieces_long) != 0UL)
                {
                    interference = true;     //Si une pièce est présente sur la case, on arrête la recherche (elle est ajouté si ennemi par la ligne du dessus)
                }
                carré_check = carré_check + offset;
            }
            return légal_dir;
            */
        }


        //Tour de l'adversaire peut sauter au-dessus de ses propres pièces, à regarder
        public ulong Direction_légaux_droit(int square, int colonne)
        {
            ulong filtre_colonne = Filtres_colonne[colonne];

            int décalage_négatif = 63 - square;
            ulong masque_n8 = 0b_00000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000UL >> décalage_négatif;   //Le premier 1 est inutile, c'est la case de la pièce
            ulong masque_n1 = 0b_01111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000UL >> décalage_négatif;   //7 cases maximums vues
            ulong masque_1 = 0b_00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111110UL << square;
            ulong masque_8 = 0b_00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000000UL << square;

            int case_piece_n8 = 63 - BitOperations.LeadingZeroCount(masque_n8 & pieces_long);
            int case_piece_n1 = 63 - BitOperations.LeadingZeroCount(masque_n1 & pieces_long);
            int case_piece_1 = BitOperations.TrailingZeroCount(masque_1 & pieces_long);
            int case_piece_8 = BitOperations.TrailingZeroCount(masque_8 & pieces_long);

            ulong legaux_n8 = masque_n8;   //Le filtre est orienté vers la gauche, donc vers -9, -1, et -7
            ulong legaux_n1 = masque_n1 & filtre_colonne;  // Pour ceux vers la droite, on prend l'inverse (toujours 0 sur la colonne, assurés par les masques)
            ulong legaux_1 = masque_1 & ~filtre_colonne;
            ulong legaux_8 = masque_8;

            if (case_piece_n8 >= 0) legaux_n8 = (legaux_n8 >> case_piece_n8) << (case_piece_n8);
            if (case_piece_n1 >= 0) legaux_n1 = (legaux_n1 >> case_piece_n1) << (case_piece_n1);
            if (case_piece_1 < 64) legaux_1 = (legaux_1 << (63 - case_piece_1)) >> (63 - case_piece_1);
            if (case_piece_8 < 64) legaux_8 = (legaux_8 << (63 - case_piece_8)) >> (63 - case_piece_8);

            return legaux_n8 | legaux_n1 | legaux_1 | legaux_8;

        }


        //Calcul des masques d'echecs
        public void MasquesCheck()
        {
            ulong[] p_amis = Blanc ? Bitboards_blanc : Bitboards_noir;
            ulong[] p_ennemis = Blanc ? Bitboards_noir : Bitboards_blanc;
            int case_roi = Blanc ? CaseRoiBlanc : CaseRoiNoir;
            CalculsMasque(case_roi, p_amis, p_ennemis);
        }


        //SI MASQUES[8] != 0, cela signifie échec de pion / cavalier donc à résoudre immédiatement
        //Les masques de 0 à 7 représentent les directions, dans l'ordre croissant de leur offset (-9, -8, -7, -1, 1, 7, 8, 9) et le masque 8 représente pion/cavalier
        public void CalculsMasque(int square, ulong[] pieces_amis, ulong[] pieces_ennemis)
        {
            //Debug.WriteLine($"On calcule les masques avec CaseRoi = {square}");
            Masques_echecs = new ulong[9];

            ulong p_ennemis = pieces_ennemis[0];
            ulong p_amis = pieces_amis[0];
            int colonne = square % 8;
            int couleur_numéro = Blanc ? 100 : 0;
            
            Checkers = 0ul;
            InCheck = false;
            MasqueEchecDirect = 0ul;
            SommeMasquesEchecs = 0ul;

            int décalage_négatif = 63 - square;
            ulong masque_n9 = 0x_00402010_08040201UL >> décalage_négatif;   //Le premier 1 est inutile, c'est la case de la pièce
            ulong masque_n8 = 0x_00808080_80808080UL >> décalage_négatif;
            ulong masque_n7 = 0x_01020408_10204000UL >> décalage_négatif;   //7 cases maximums vues
            ulong masque_n1 = 0x_7f000000_00000000UL >> décalage_négatif;   //0b_01111111_0000...000UL;
            ulong masque_1 = 0x_00000000_000000feUL << square;   //0b_0000000......00_11111110UL;
            ulong masque_7 = 0x_00020408_10204080UL << square;
            ulong masque_8 = 0x_01010101_01010100UL << square;
            ulong masque_9 = 0x_80402010_08040200UL << square;

            int carré_n9 = 63 - BitOperations.LeadingZeroCount(masque_n9 & p_ennemis & Filtres_colonne[colonne]);
            int carré_n8 = 63 - BitOperations.LeadingZeroCount(masque_n8 & p_ennemis);
            int carré_n7 = 63 - BitOperations.LeadingZeroCount(masque_n7 & p_ennemis & ~Filtres_colonne[colonne]);  //Négatif du filtre
            int carré_n1 = 63 - BitOperations.LeadingZeroCount(masque_n1 & p_ennemis & Filtres_colonne[colonne]);
            int carré_1 = BitOperations.TrailingZeroCount(masque_1 & p_ennemis & ~Filtres_colonne[colonne]); //Négatif du filtre
            int carré_7 = BitOperations.TrailingZeroCount(masque_7 & p_ennemis & Filtres_colonne[colonne]);
            int carré_8 = BitOperations.TrailingZeroCount(masque_8 & p_ennemis);
            int carré_9 = BitOperations.TrailingZeroCount(masque_9 & p_ennemis & ~Filtres_colonne[colonne]); //Négatif du filtre
            //Debug.Write($"On a les carrés {carré_n9}, {carré_n8}, {carré_n7}, {carré_n1}, {carré_1}, {carré_7}, {carré_8}, {carré_9} ");

            if (carré_n9 >= 0 && (pieces[carré_n9] == 2 + couleur_numéro || pieces[carré_n9] == 4 + couleur_numéro))
            {
                Masques_echecs[0] = (masque_n9 >> carré_n9) << carré_n9;

                if ((Masques_echecs[0] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[0];
                    Checkers |= 1ul << BitOperations.TrailingZeroCount(Masques_echecs[0]);
                }

                SommeMasquesEchecs |= Masques_echecs[0];
            }
            if (carré_n8 >= 0 && (pieces[carré_n8] == 2 + couleur_numéro || pieces[carré_n8] == 3 + couleur_numéro))
            {
                Masques_echecs[1] = (masque_n8 >> carré_n8) << carré_n8;

                if ((Masques_echecs[1] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[1]; 
                    Checkers |= 1ul << BitOperations.TrailingZeroCount(Masques_echecs[1]);
                }

                SommeMasquesEchecs |= Masques_echecs[1];
            }
            if (carré_n7 >= 0 && (pieces[carré_n7] == 2 + couleur_numéro || pieces[carré_n7] == 4 + couleur_numéro))
            {
                Masques_echecs[2] = (masque_n7 >> carré_n7) << carré_n7;

                if ((Masques_echecs[2] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[2];
                    Checkers |= 1ul << BitOperations.TrailingZeroCount(Masques_echecs[2]);
                }

                SommeMasquesEchecs |= Masques_echecs[2];
            }
            if (carré_n1 >= 0 && (pieces[carré_n1] == 2 + couleur_numéro || pieces[carré_n1] == 3 + couleur_numéro))
            {
                Masques_echecs[3] = (masque_n1 >> carré_n1) << carré_n1;

                if ((Masques_echecs[3] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[3];
                    Checkers |= 1ul << BitOperations.TrailingZeroCount(Masques_echecs[3]);
                }

                SommeMasquesEchecs |= Masques_echecs[3];
            }
            if (carré_1 < 64 && (pieces[carré_1] == 2 + couleur_numéro || pieces[carré_1] == 3 + couleur_numéro))
            {
                Masques_echecs[4] = (masque_1 << (63 - carré_1)) >> (63 - carré_1);

                if ((Masques_echecs[4] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[4];
                    Checkers |= 1ul << (63 - BitOperations.LeadingZeroCount(Masques_echecs[4]));
                }

                SommeMasquesEchecs |= Masques_echecs[4];
            }
            if (carré_7 < 64 && (pieces[carré_7] == 2 + couleur_numéro || pieces[carré_7] == 4 + couleur_numéro))
            {
                Masques_echecs[5] = (masque_7 << (63 - carré_7)) >> (63 - carré_7);

                if ((Masques_echecs[5] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[5];
                    Checkers |= 1ul << (63 - BitOperations.LeadingZeroCount(Masques_echecs[5]));
                }

                SommeMasquesEchecs |= Masques_echecs[5];
            }
            if (carré_8 < 64 && (pieces[carré_8] == 2 + couleur_numéro || pieces[carré_8] == 3 + couleur_numéro))
            {
                Masques_echecs[6] = (masque_8 << (63 - carré_8)) >> (63 - carré_8);

                if ((Masques_echecs[6] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[6];
                    Checkers |= 1ul << (63 - BitOperations.LeadingZeroCount(Masques_echecs[6]));
                }

                SommeMasquesEchecs |= Masques_echecs[6];
            }
            if (carré_9 < 64 && (pieces[carré_9] == 2 + couleur_numéro || pieces[carré_9] == 4 + couleur_numéro))
            {
                Masques_echecs[7] = (masque_9 << (63 - carré_9)) >> (63 - carré_9);

                if ((Masques_echecs[7] & p_amis) == 0ul)
                {
                    MasqueEchecDirect = Masques_echecs[7];
                    Checkers |= 1ul << (63 - BitOperations.LeadingZeroCount(Masques_echecs[7]));
                }
                SommeMasquesEchecs |= Masques_echecs[7];
            }

            if (MasqueEchecDirect != 0)
            {
                InCheck = true;
            }



            ulong check_cavalier = légaux_cavalier(square, colonne) & pieces_ennemis[5];
            if (check_cavalier != 0)
            {
                int LSB = BitOperations.TrailingZeroCount(check_cavalier);
                Masques_echecs[8] = 1ul << LSB;
                InCheck = true;
                MasqueEchecDirect = 1ul << LSB;
                Checkers |= 1ul << LSB;
            }
            if (InCheck == false)
            {
                if (Blanc)
                {
                    int pion_droit = square + 7;
                    int pion_gauche = square + 9;
                    if (square < 48 && pieces[pion_droit] == 106 && colonne != 0)
                    {
                        Masques_echecs[8] = 1ul << (pion_droit);
                        InCheck = true;
                        MasqueEchecDirect = 1ul << (pion_droit);
                        Checkers |= 1ul << (pion_droit);
                    }
                    else if (square < 47 && pieces[pion_gauche] == 106 && colonne != 7)
                    {

                        Masques_echecs[8] = 1ul << (pion_gauche);
                        InCheck = true;
                        MasqueEchecDirect = 1ul << (pion_gauche);
                        Checkers |= 1ul << (pion_gauche);
                    }
                }
                else
                {
                    if (square > 15 && pieces[square - 7] == 6 && colonne != 7)
                    {
                        Masques_echecs[8] = 1ul << (square - 7);
                        InCheck = true;
                        MasqueEchecDirect = 1ul << (square - 7);
                        Checkers |= 1ul << (square - 7);
                    }
                    else if (square > 16 && pieces[square - 9] == 6 && colonne != 0)
                    {
                        Masques_echecs[8] = 1ul << (square - 9);
                        InCheck = true;
                        MasqueEchecDirect = 1ul << (square - 9);
                        Checkers |= 1ul << (square - 9);
                    }
                }
            }
        }



        //On a les coups_légaux de chaque cases précédent par Légaux[64]
        //On a potentiellement les coups légaux noir et blanc fusionnés
        //On a les cases précises(FAITES AVANT) des pieces_blanches et pieces_noir avec pieces_long_blanc et pieces_long_noir
        public void MAJ_Légaux(int départ, int arrivée)
        {
            //Debug.WriteLine($"MAJ_LEGAUX pour le coup {départ} --> {arrivée} avec couleur = {Blanc} avec les pieces : {pieces_long:B64}");
            int ligne_départ = départ / 8;
            int colonne_départ = départ % 8;
            int ligne_arrivée = arrivée / 8;
            int colonne_arrivée = arrivée % 8;
            int CaseRoi = Blanc ? CaseRoiNoir : CaseRoiBlanc;
            PotentielEnPassant = -1;


            if (EnPassantBool)
            {
                if (Blanc)
                {
                    if (pieces[arrivée] < 10)  //Annulation blanche
                    {
                        //Debug.WriteLine("Il vient d'avoir un en_passant blanc, on met à jour les légaux liées à l'annulation du en passant");
                        Légaux_direction(départ - 8);
                    }
                    else   //Réalisation noire
                    {
                        //Debug.WriteLine("Il vient d'avoir un en_passant noir, on met à jour les légaux liées à la réalisation du en passant");
                        Légaux_direction(arrivée + 8);
                    }
                }
                else
                {
                    if (pieces[arrivée] < 10)  //Réalisation blanche
                    {
                        //Debug.WriteLine("Il vient d'avoir un en_passant blanc, on met à jour les légaux liées à la réalisation du en passant");
                        Légaux_direction(arrivée - 8);
                    }
                    else  //Annulation noire
                    {
                        //Debug.WriteLine("Il vient d'avoir un en_passant noir, on met à jour les légaux liées à l'annulation du en passant");
                        Légaux_direction(départ + 8);
                    }
                }
            }


            PseudoLégaux[départ] = coups_légaux(départ);
            PseudoLégaux[arrivée] = coups_légaux(arrivée);

            Légaux_direction(départ);
            Légaux_direction(arrivée);


            ulong cases_cavalier = (légaux_cavalier(départ, colonne_départ) | légaux_cavalier(arrivée, colonne_arrivée)) & pieces_long;
            while (cases_cavalier != 0UL)
            {
                int carré = BitOperations.TrailingZeroCount(cases_cavalier);
                int numéro = pieces[carré];
                if (numéro == 5 || numéro == 105)
                {
                    PseudoLégaux[carré] = coups_légaux(carré);
                }
                cases_cavalier &= cases_cavalier - 1UL;
            }

            PotentielEnPassant = CheckEnPassant(arrivée, Blanc);


            MasquesCheck();


            //Mise à jour faites automatiquement

            //Roi et pions ont des mouvements uniquements droit ou diagonales, ils sont pris en compte par le double RecalculsLegauxDirection
        }

        public void Légaux_direction(int carré)
        {
            int colonne = carré % 8;
            //Debug.WriteLine("On calcule officiellement les légaux depuis " + carré);
            int décalage_négatif = 63 - carré;
            ulong masque_n9 = 0b_00000000_01000000_00100000_00010000_00001000_00000100_00000010_00000001UL >> décalage_négatif;   //Le premier 1 est inutile, c'est la case de la pièce
            ulong masque_n8 = 0x_00808080_80808080UL >> décalage_négatif;
            ulong masque_n7 = 0b_00000001_00000010_00000100_00001000_00010000_00100000_01000000_00000000UL >> décalage_négatif;   //7 cases maximums vues
            ulong masque_n1 = 0x_7f000000_00000000UL >> décalage_négatif;   //0b_01111111_0000...000UL;
            ulong masque_1 = 0x_00000000_000000feUL << carré;   //0b_0000000......00_11111110UL;
            ulong masque_7 = 0b_00000000_00000010_00000100_00001000_00010000_00100000_01000000_10000000UL << carré;
            ulong masque_8 = 0x_01010101_01010100UL << carré;
            ulong masque_9 = 0b_10000000_01000000_00100000_00010000_00001000_00000100_00000010_00000000UL << carré;

            int carré_n9 = 63 - BitOperations.LeadingZeroCount(masque_n9 & pieces_long & Filtres_colonne[colonne]);
            int carré_n8 = 63 - BitOperations.LeadingZeroCount(masque_n8 & pieces_long);
            int carré_n7 = 63 - BitOperations.LeadingZeroCount(masque_n7 & pieces_long & ~Filtres_colonne[colonne]);
            int carré_n1 = 63 - BitOperations.LeadingZeroCount(masque_n1 & pieces_long & Filtres_colonne[colonne]);
            int carré_1 = BitOperations.TrailingZeroCount(masque_1 & pieces_long & ~Filtres_colonne[colonne]);
            int carré_7 = BitOperations.TrailingZeroCount(masque_7 & pieces_long & Filtres_colonne[colonne]);
            int carré_8 = BitOperations.TrailingZeroCount(masque_8 & pieces_long);
            int carré_9 = BitOperations.TrailingZeroCount(masque_9 & pieces_long & ~Filtres_colonne[colonne]);
            //Debug.WriteLine($"On a les carrés {carré_n9}, {carré_n8}, {carré_n7}, {carré_n1}, {carré_1}, {carré_7}, {carré_8}, {carré_9} ");

            if (carré_n9 >= 0) PseudoLégaux[carré_n9] = coups_légaux(carré_n9);
            if (carré_n8 >= 0) PseudoLégaux[carré_n8] = coups_légaux(carré_n8);
            if (carré_n7 >= 0) PseudoLégaux[carré_n7] = coups_légaux(carré_n7);
            if (carré_n1 >= 0) PseudoLégaux[carré_n1] = coups_légaux(carré_n1);
            if (carré_1 < 64) PseudoLégaux[carré_1] = coups_légaux(carré_1);
            if (carré_7 < 64) PseudoLégaux[carré_7] = coups_légaux(carré_7);
            if (carré_8 < 64) PseudoLégaux[carré_8] = coups_légaux(carré_8);
            if (carré_9 < 64) PseudoLégaux[carré_9] = coups_légaux(carré_9);
        }







        public bool Realisation_coup_logique(int carré_précédent, int carré, int numéro_promotion = 0)
        {
            int piece = pieces[carré_précédent];
            int piece_prise = pieces[carré];
            Promotion_bool = false;

            if (EnPassantEffectué(carré_précédent, carré))
            {
                EchangePieces_EnPassant_logique(carré_précédent, carré);   //Supprime juste le pion pris
            }

            EchangePieces_logique(carré_précédent, carré, piece_prise);
            
            if (Roque_effectué(carré_précédent, carré) == true)   //Vérifie si le dernier coup est le roque
            {
                //Debug.WriteLine("Roque effectué");
                Faire_roque_logique(carré_précédent, carré);   //Effectue le mouvement de tour
            }

            if (Promotion_check(carré) == true)
            {
                Promotion_bool = true;
                Promotion(numéro_promotion, carré);
            }

            if (roque_long_blanc && (carré_précédent == 0 || carré_précédent == 4 || carré == 0)) { Roque_long_blanc = false; } //Désactive les roques si une des pièces bouge
            if (roque_blanc && (carré_précédent == 7 || carré_précédent == 4 || carré == 7)) { Roque_blanc = false; }
            if (roque_long_noir && (carré_précédent == 56 || carré_précédent == 60 || carré == 56)) { Roque_long_noir = false; }
            if (roque_noir && (carré_précédent == 63 || carré_précédent == 60 || carré == 63)) { Roque_noir = false; }

            Partie.Add(Notation_coup(piece, piece_prise, carré_précédent, carré));   //Ajoute le dernier coup au tableau de notation
            DernierCoup = Partie[Partie.Count - 1];

            if (piece == 1 || piece == 101)
            {
                if (Blanc) { CaseRoiBlanc = carré; }
                else { CaseRoiNoir = carré; }
            }


            Blanc = !Blanc;

            MAJ_Légaux(carré_précédent, carré);  //On met à jour les coups légaux

            EnPassantBool = false;

            if (Vérification_pat())   //Aucun coup légal
            {
                //Debug.WriteLine($"Partie terminée !! Dernier coup : {carré_précédent} --> {carré}");
                PartieFinie = true;
                if (InCheck)
                {
                    return true;
                }
            }

            return false;
        }


        //On agit sur pieces[] et met la pièce de départ à l'arrivée et vide au départ
        //On agit sur les 3 ulong : blanc, noir et total. Actif : + arrivée, - départ. Passif : Same - Actif (si prise, on retire). Total : Actif + Passif
        //On met à jour les cases des rois si jamais un roi a bougé
        public void EchangePieces_logique(int carré_départ, int carré_arrivée, int piece_prise)
        {
            int piece_active = pieces[carré_départ];
            int piece_vide = 50;

            pieces[carré_arrivée] = piece_active;
            pieces[carré_départ] = piece_vide;

            //Mise à long des ulong représentants les pièces
            if (piece_active < 10)
            {
                Bitboards_blanc[0] |= 1UL << carré_arrivée;    //On force 1 à la case d'arrivée
                Bitboards_blanc[0] &= ~(1UL << carré_départ);   //On force 0 à la case de départ
                Bitboards_noir[0] &= ~Bitboards_blanc[0];    //On met à jour les pièces noir, noir = noir & ~blanc;

                if (piece_prise > 75)  //Pièce noir
                {
                    Bitboards_noir[piece_prise - 100] &= ~(1ul << carré_arrivée);  //Bitboard correspondants à la pièce
                }

                Bitboards_blanc[piece_active] |= 1ul << carré_arrivée;
                Bitboards_blanc[piece_active] &= ~(1ul << carré_départ);
                compteur_50coups = 0;
            }
            else if (piece_active > 75)
            {
                Bitboards_noir[0] |= 1UL << carré_arrivée;
                Bitboards_noir[0] &= ~(1UL << carré_départ);
                Bitboards_blanc[0] &= ~Bitboards_noir[0];

                if (piece_prise < 10)
                {
                    Bitboards_blanc[piece_prise] &= ~(1ul << carré_arrivée);
                }

                Bitboards_noir[piece_active - 100] |= 1ul << carré_arrivée;
                Bitboards_noir[piece_active - 100] &= ~(1ul << carré_départ);
                compteur_50coups = 0;
            }
            Pieces_long = Bitboards_blanc[0] | Bitboards_noir[0];

            if (piece_prise == 50 && piece_active != 6 && piece_active != 106)
            {
                compteur_50coups++;
            }
            else
            {
                compteur_50coups = 0;
            }
            //Mise à jour des variables globales pour les cases des rois
            if (piece_active == 1)
            {
                CaseRoiBlanc = carré_arrivée;
            }
            if (piece_active == 101)
            {
                CaseRoiNoir = carré_arrivée;
            }
        }

        //On doit échanger, et rajouter la pièce à pieces, ulong pieces_passive, pieces total
        public void AnnuleEchange_logique(int carré_départ, int carré_arrivée, int piece_prise)
        {
            EchangePieces_logique(carré_départ, carré_arrivée, piece_prise);

            pieces[carré_départ] = piece_prise;
            if (piece_prise < 10)
            {
                Bitboards_blanc[0] |= 1UL << carré_départ;
                Bitboards_blanc[piece_prise] |= 1ul << carré_départ;
            }
            else if (piece_prise > 75)
            {
                Bitboards_noir[0] |= 1UL << carré_départ;
                Bitboards_noir[piece_prise - 100] = 1ul << carré_départ;
            }
            pieces_long = Bitboards_blanc[0] | Bitboards_noir[0];
        }
        //   60        50        40        30        20        10        0     //20 a le carré n°20 en dessous du 2
        //1011100001111101011100110001100001010010101001001011111110111000    //Occurrence random (coup 14 --> 30)
        //1011100001111101011100110000100000010010101001001111111110111000   //Dernière occurrence (coup 36 --> 53)
        //1001000101111101011100110001100000010010101001001111111110010001  //Référence

        //PB PROMOTION VERIFIER SI LES PIECES REAPPARAISSENT BIEN
        public void Annulation_Realisation_coup(int carré_précédent, int carré, int piece_prise, bool prom, bool rb, bool rlb, bool rn, bool rln, int compt50coups)
        {
            Blanc = !Blanc;  //Blanc -> coup blanc / annulation blanc, sinon on a des problèmes sur les conditions (Blanc ?)
            if (Partie.Count > 1)
            {
                Partie.RemoveAt(Partie.Count - 1);
                DernierCoup = Partie[Partie.Count - 1];
            }

            int piece_active = pieces[carré_précédent];
            int numero_couleur = Blanc ? 0 : 100;
            if (piece_active == 6 && (carré - carré_précédent) % 8 != 0 && piece_prise == 50)
            {
                //Debug.WriteLine("On annule un en passant des blancs");
                int pion_pris = 106;
                pieces[carré_précédent - 8] = pion_pris;
                Bitboards_noir[0] |= 1ul << (carré_précédent - 8);
                Bitboards_noir[6] &= Bitboards_noir[0];
                EnPassantBool = true;
            }
            else if (piece_active == 106 && (carré - carré_précédent) % 8 != 0 && piece_prise == 50)
            {
                //Debug.WriteLine("On annule un en passant des noirs");
                int pion_pris = 6;
                pieces[carré_précédent + 8] = pion_pris;
                Bitboards_blanc[0] |= 1ul << (carré_précédent + 8);
                Bitboards_blanc[6] &= Bitboards_blanc[0];
                EnPassantBool = true;
            }
            else if (Roque_effectué(carré, carré_précédent))
            {
                //Debug.WriteLine("On annule un roque");
                Annulation_roque(carré_précédent, carré);
            }
            else if (prom)
            {
                //Debug.WriteLine("On annule une promotion");
                if (carré > 32)
                {
                    pieces[carré_précédent] = 6;
                }
                else
                {
                    pieces[carré_précédent] = 106;
                }
            }
            AnnuleEchange_logique(carré_précédent, carré, piece_prise);
            //On remet les bonnes autorisations de roque
            Roque_blanc = rb;
            Roque_long_blanc = rlb;
            Roque_noir = rn;
            Roque_long_noir = rln;
            Compteur_50coups = compt50coups;

            MAJ_Légaux(carré_précédent, carré);  //On met à jour les coups légaux

            EnPassantBool = false;

        }





        //Vérifie si le coup joué est en passant  avec True <=> en passant joué
        public bool EnPassantEffectué(int carré_précédent, int carré)
        {
            int diff = carré - carré_précédent;
            int piece = pieces[carré];
            if ((diff == 7 || diff == 9) && pieces[carré_précédent] == 6 && piece == 50)
            {
                //Debug.WriteLine($"En passant a été effectué de {carré_précédent} à {carré} par un pion blanc et diff = {diff}");
                EnPassantBool = true;
                return true;
            }
            if ((diff == -7 || diff == -9) && pieces[carré_précédent] == 106 && piece == 50)
            {
                EnPassantBool = true;
                return true;
            }
            return false;
        }


        public void EchangePieces_EnPassant_logique(int carré_précédent, int carré)
        {
            //Debug.WriteLine($"En passant effectué avec Blanc = {Blanc} et carré = {carré}");
            int pion_pris = 50;

            if (Blanc == true)
            {
                pieces[carré - 8] = pion_pris;
                Bitboards_noir[0] &= ~(1ul << carré - 8);
                Bitboards_noir[6] &= Bitboards_noir[0];
            }
            else
            {
                pieces[carré + 8] = pion_pris;
                Bitboards_blanc[0] &= ~(1ul << carré + 8);
                Bitboards_blanc[6] &= Bitboards_blanc[0];
            }
            pieces_long = Bitboards_blanc[0] | Bitboards_noir[0];
        }


        //Ne vérifie pas les lignes, regarde si le dernier coup est un pion qui a avancé de deux cases dans une colonne adjacente
        //Pas besoin de savoir si c'est un pion qui a joué ou pas
        public int CheckEnPassant(int carré, bool couleur)
        {
            int ligne = carré / 8;
            if (couleur == false && ligne == 3)
            {
                //Debug.WriteLine($"On check en_passant pour la case {carré} et couleur = {couleur} avec DernierCoup = {DernierCoup}");
                if (DernierCoup[1] == '2') 
                {
                    return 16 + (DernierCoup[0] - 'a'); 
                }
            }
            else if (couleur && ligne == 4)
            {
                //Debug.WriteLine($"On check en_passant pour la case {carré} et couleur = {couleur} avec DernierCoup = {DernierCoup}");
                if (DernierCoup[1] == '7')
                {
                    return 40 + (DernierCoup[0] - 'a');
                }
            }

            return -1;
        }

        //On appel toujours avec un en passant validé, sauf pour un clouage potentiel
        public ulong ClouageEnPassant(int carré, bool couleur, ulong p_ennemis)
        {
            //Debug.WriteLine($"On check {carré} et pieces = {pieces_long:B64}");
            int ligne = carré / 8;
            int colonne = carré % 8;
            ulong masque = 0x00000000_000000fc;  //On a 6 cases qu'on doit checker au maximum car on ne veut pas compter le pion qu'on en_passant

            if (couleur)
            {
                if (CaseRoiBlanc / 8 == ligne)  //On étudie pas si le pion n'est pas sur la même ligne que le roi
                {
                    if (PotentielEnPassant - carré == 7)
                    {
                        if (carré - CaseRoiBlanc > 0)  //Roi blanc à gauche
                        {
                            ulong piece_attaques = masque << (carré - 1) & ~Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = BitOperations.TrailingZeroCount(piece_attaques);  //Case 1e piece adverse
                            if (premier_piece_attaque < 64)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiBlanc - 1)) & pieces_long) << (64 - premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 102 || numéro_premiere_piece_attaque == 103)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                        else   //Roi blanc à droite
                        {
                            ulong piece_attaques = masque << (carré - 9) & Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = 63 - BitOperations.LeadingZeroCount(piece_attaques);
                            if (premier_piece_attaque > 0)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiBlanc - 8)) & pieces_long) >> (1 + premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 102 || numéro_premiere_piece_attaque == 103)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (carré - CaseRoiBlanc > 0)
                        {
                            ulong piece_attaques = masque << (carré) & ~Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = BitOperations.TrailingZeroCount(piece_attaques);
                            if (premier_piece_attaque < 64)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiBlanc - 1)) & pieces_long) << (64 - premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 102 || numéro_premiere_piece_attaque == 103)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                        else
                        {
                            ulong piece_attaques = masque << (carré - 8) & Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = 63 - BitOperations.LeadingZeroCount(piece_attaques);
                            if (premier_piece_attaque > 0)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiBlanc - 8)) & pieces_long) >> (1 + premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 102 || numéro_premiere_piece_attaque == 103)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //Debug.Write("pion noir ");
                if (CaseRoiNoir / 8 == ligne)  //On étudie pas si le pion n'est pas sur la même ligne que le roi
                {
                    //Debug.Write("avec le roi ");
                    if (carré - PotentielEnPassant == 9)
                    {
                        //Debug.WriteLine("et une diff de 9");
                        if (carré - CaseRoiNoir > 0)
                        {
                            ulong piece_attaques = masque << (carré - 1) & ~Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = BitOperations.TrailingZeroCount(piece_attaques);
                            if (premier_piece_attaque < 64)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiNoir - 1)) & pieces_long) << (64 - premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 2 || numéro_premiere_piece_attaque == 3)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                        else
                        {
                            ulong piece_attaques = masque << (carré - 9) & Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = 63 - BitOperations.LeadingZeroCount(piece_attaques);
                            if (premier_piece_attaque > 0)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiNoir - 8)) & pieces_long) >> (1 + premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 2 || numéro_premiere_piece_attaque == 3)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.WriteLine("et une diff de 7");
                        if (carré - CaseRoiNoir > 0)
                        {
                            ulong piece_attaques = masque << (carré) & ~Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = BitOperations.TrailingZeroCount(piece_attaques);
                            if (premier_piece_attaque < 64)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiNoir - 1)) & pieces_long) << (64 - premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 2 || numéro_premiere_piece_attaque == 3)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                        else
                        {
                            ulong piece_attaques = masque << (carré - 8) & Filtres_colonne[colonne] & p_ennemis;
                            int premier_piece_attaque = 63 - BitOperations.LeadingZeroCount(piece_attaques);
                            if (premier_piece_attaque > 0)
                            {
                                if ((BitOperations.PopCount(((masque << (CaseRoiNoir - 8)) & pieces_long) >> (1 + premier_piece_attaque)) > 2) == false)
                                {
                                    int numéro_premiere_piece_attaque = pieces[premier_piece_attaque];
                                    if (numéro_premiere_piece_attaque == 2 || numéro_premiere_piece_attaque == 3)
                                    {
                                        return 0ul;  //Si une tour / dame est derrière le pion en passant
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Debug.WriteLine("En passant validé");
            return 1ul << PotentielEnPassant;
        }

        public void EnPassantFEN(string case_en_passant)
        {
            int ligne = case_en_passant[1];
            if (ligne == 2)
            {
                DernierCoup = case_en_passant[0] + ligne.ToString() + case_en_passant[0] + (ligne + 2).ToString();
            }
            else
            {
                DernierCoup = case_en_passant[0] + (ligne + 2).ToString() + case_en_passant[0] + ligne.ToString();
            }
            PotentielEnPassant = (ligne - '1') * 8 + (case_en_passant[0] - 'a');
        }




        public ulong Roque(int case_roi, ulong Légaux_ennemis)  //Vérifie si le roque est possible
        {
            ulong roque_légaux = 0UL;

            if (Blanc)
            {
                if (roque_blanc)
                {
                    if (((3UL << 5) & pieces_long) == 0 && ((7UL << 4) & Légaux_ennemis) == 0UL)  //Si les cases entre 4 et 6 inclus sont soumis à un coup légal noir quelconque
                    {
                        roque_légaux |= 1UL << 6;
                    }
                }
                if (roque_long_blanc)
                {
                    if (((7UL << 1) & pieces_long) == 0 && ((7UL << 2) & Légaux_ennemis) == 0UL)   //Si les cases entre 4 et 6 inclus sont soumis à un coup légal noir quelconque
                    {
                        roque_légaux |= 1UL << 2;
                    }
                }
            }
            else
            {
                if (roque_noir)
                {
                    if (((7UL << 57) & pieces_long) == 0 && ((7UL << 60) & Légaux_ennemis) == 0UL)   //Si les cases entre 4 et 6 inclus sont soumis à un coup légal noir quelconque
                    {
                        roque_légaux |= 1UL << 62;
                    }
                }
                if (roque_long_noir)
                {
                    if (((3UL << 61) & pieces_long) == 0 && ((7UL << 58) & Légaux_ennemis) == 0UL)   //Si les cases entre 4 et 6 inclus sont soumis à un coup légal noir quelconque
                    {
                        roque_légaux |= 1UL << 58;
                    }
                }
            }

            return roque_légaux;
        }


        public bool Roque_effectué(int carré_précédent, int carré)  //Vérifie si l'action est Roque
        {
            if (Math.Abs(carré_précédent - carré) == 2 && pieces[carré] % 10 == 1)
            {
                return true;
            }
            return false;
        }


        public void Faire_roque_logique(int carré_précédent, int carré)   //Complète l'action du roque en déplaçant la tour
        {
            int piece_vide = 50;
            if (carré_précédent == 4)
            {
                if (carré == 2)
                {
                    EchangePieces_logique(0, 3, piece_vide);
                    Partie.Add("O-O-O");
                }
                if (carré == 6)
                {
                    EchangePieces_logique(7, 5, piece_vide);
                    Partie.Add("O-O");
                }
                Roque_blanc = false;
                Roque_long_blanc = false;

            }
            if (carré_précédent == 60)
            {
                if (carré == 62)
                {
                    EchangePieces_logique(63, 61, piece_vide);
                    Partie.Add("O-O");
                }
                if (carré == 58)
                {
                    EchangePieces_logique(56, 59, piece_vide);
                    Partie.Add("O-O-O");
                }
                Roque_noir = false;
                Roque_long_noir = false;
            }
        }

        public void Annulation_roque(int carré_précédent, int carré)  //Annule le déplacement de la tour selon le roque du roi
        {
            int piece_vide = 50;
            if (carré_précédent == 2)
            {
                EchangePieces_logique(3, 0, piece_vide);
            }
            if (carré_précédent == 6)
            {
                EchangePieces_logique(5, 7, piece_vide);
            }
            if (carré_précédent == 58)
            {
                EchangePieces_logique(59, 56, piece_vide);
            }
            if (carré_précédent == 62)
            {
                EchangePieces_logique(61, 63, piece_vide);
            }
        }


        public bool Promotion_check(int carré)
        {
            int piece = pieces[carré];
            if (piece == 6 && carré / 8 == 7) { return true; }
            if (piece == 106 && carré / 8 == 0) { return true; }
            return false;
        }

        public int Promotion(int choix, int carré)
        {
            //Debug.WriteLine($"Promotion affichage sur carré = {carré}");
            int numero_couleur = carré < 8 ? 100 : 0;

            int piece = 2 + choix + numero_couleur;

            pieces[carré] = piece;
            //Debug.WriteLine($"La nouvelle pièce a donc le numéro : {pieces[carré].numéro}");
            return piece;
        }





        //On a inversé la couleur juste avant, si c'était un tour blanc, on verifie du point de vue noir
        public bool Vérification_pat()
        {
            //Debug.WriteLine("On vérifie si y'a pat");
            if (compteur_50coups >= 100)
            {
                //Debug.WriteLine("On return true");
                return true;
            }
            if (BitOperations.PopCount(pieces_long) <= 2)
            {
                //Debug.WriteLine("On return true");
                return true;
            }
            //Debug.WriteLine("Assez de pièces et pas de 50 coups");
            ulong full_pieces = Blanc ? Bitboards_blanc[0] & ~(1ul << CaseRoiBlanc) : Bitboards_noir[0] & ~(1ul << CaseRoiNoir);
            int square = BitOperations.TrailingZeroCount(full_pieces);
            bool coups = false;

            while (coups == false && square < 64)  //Vérifie si une pièce de la couleur qu'on check peut bouger
            {
                //Debug.WriteLine($"On check les légaux de la pièce en {square}");
                int numéro = pieces[square];
                if (Blanc)  //Pièce blanche
                {
                    coups = légaux_verif_pat(square, numéro, Bitboards_blanc[0], Bitboards_noir[0]);
                }
                else
                {
                    coups = légaux_verif_pat(square, numéro, Bitboards_noir[0], Bitboards_blanc[0]);
                }
                //Debug.WriteLine($"Elle a des légaux : {coups}");

                full_pieces &= full_pieces - 1;
                square = BitOperations.TrailingZeroCount(full_pieces);
            }
            //Debug.WriteLine($"Donc on sort du while, on a coups = {coups}");
            if (coups == true)
            {
                return false;
            }

            //Seul roi restant
            //Debug.WriteLine("Il reste plus que le roi");
            ulong attaque = 0ul;
            full_pieces = Blanc ? Bitboards_noir[0] : Bitboards_blanc[0];  //On va faire les coups adverses
            ulong p_amis = Blanc ? Bitboards_blanc[0] : Bitboards_noir[0];
            coups = false;
            square = BitOperations.TrailingZeroCount(full_pieces);
            while (coups == false && square < 64)  //Pas de coups possibles, on continue à la pièce suivante
            {
                int numéro = pieces[square];
                attaque |= CalculsLégauxEnnemisPat(square, p_amis);

                full_pieces &= full_pieces - 1;
                square = BitOperations.TrailingZeroCount(full_pieces);
                //Debug.WriteLine($"On calcule les légaux de l'ennemi en {square} et cela donne légaux = {attaque:B64}");
            }
            if (Blanc)
            {
                ulong coups_légaux_roi_blanc = légaux_roi(CaseRoiBlanc, CaseRoiBlanc % 8) & ~p_amis & ~attaque;
                if (coups_légaux_roi_blanc == 0ul)  //Le roi n'a pas de coups
                {
                    return true;
                }
                coups_légaux_roi_blanc &= PatCoupIllégalDosPiece(CaseRoiBlanc);
                if (coups_légaux_roi_blanc == 0ul)
                {
                    return true;
                }
                return false;
            }
            ulong coups_légaux_roi_noir = légaux_roi(CaseRoiNoir, CaseRoiNoir % 8) & ~p_amis & ~attaque;
            if (coups_légaux_roi_noir == 0ul)
            {
                return true;
            }
            coups_légaux_roi_noir &= PatCoupIllégalDosPiece(CaseRoiBlanc);
            if (coups_légaux_roi_noir == 0ul)
            {
                return true;
            }
            return false;

        }

        public bool légaux_verif_pat(int i, int numéro, ulong p_amis, ulong p_ennemis)
        {
            int masque = 0;
            ulong légaux = 0ul;

            if (((1UL << i) & p_amis) != 0UL)   //La pièce est amie
            {
                if (((1ul << i) & SommeMasquesEchecs) == 0ul)  //Si la pièce ne fait pas partie des masques (INCLUS TOUJOURS LE ROI)
                {
                    if (InCheck)  //La pièce doit bloqué l'echec
                    {
                        légaux |= PseudoLégaux[i] & MasqueEchecDirect;
                        if (PotentielEnPassant > 0 && (checkers & (65537ul << (PotentielEnPassant - 8))) != 0)
                        {
                            if (pieces[i] == 6)
                            {
                                if (i + 7 == PotentielEnPassant && i % 8 != 0)
                                {
                                    légaux |= 1ul << PotentielEnPassant;
                                }
                                else if (i + 9 == PotentielEnPassant && i % 8 != 7)
                                {
                                    légaux |= 1ul << PotentielEnPassant;
                                }
                            }
                            else if (pieces[i] == 106)
                            {
                                if (i - 9 == PotentielEnPassant && i % 8 != 0)
                                {
                                    légaux |= 1ul << PotentielEnPassant;
                                }
                                else if (i - 7 == PotentielEnPassant && i % 8 != 7)
                                {
                                    légaux |= 1ul << PotentielEnPassant;
                                }
                            }
                        }
                    }
                    else   //La pièce fait ce qu'elle veut
                    {
                        légaux = PseudoLégaux[i] & ~p_amis;
                        if (PotentielEnPassant > 0)
                        {
                            //Debug.WriteLine($"En passant potentiel de {i}");
                            if (pieces[i] == 106 && (PotentielEnPassant == i - 7 || PotentielEnPassant == i - 9))
                            {
                                //Debug.WriteLine("On vérifie les clouages");
                                légaux |= ClouageEnPassant(i, false, p_ennemis);
                                //Debug.WriteLine($"Les coups légaux sont donc : {Légaux_couleur[i]:B64}");
                            }
                            else if (pieces[i] == 6 && (PotentielEnPassant == i + 7 || PotentielEnPassant == i + 9))
                            {
                                //Debug.WriteLine("On vérifie les clouages");
                                légaux |= ClouageEnPassant(i, true, p_ennemis);
                                //Debug.WriteLine($"Les coups légaux sont donc : {Légaux_couleur[i]:B64}");
                            }
                        }
                    }
                }
                else  //On est dans un masque (<=> cloué)
                {
                    if (!InCheck)   //Pas d'échecs mais cloué
                    {
                        for (masque = 0; masque < 8; masque++)
                        {
                            if (Masques_echecs[masque] != 0ul)   //Si il existe un masque où la pièce bloque seule un echec (doit restée sur le masque)
                            {
                                //Si la pièce est sur le masque étudié && La pièce est la seule pièce de sa couleur sur le masque
                                if (((1ul << i) & Masques_echecs[masque]) != 0 && ((~(1ul << i) & Masques_echecs[masque] & p_amis) == 0))
                                {
                                    légaux = PseudoLégaux[i] & ~p_amis & Masques_echecs[masque];
                                    masque = 10;
                                }
                            }
                        }
                        if (masque == 8)
                        {
                            légaux = PseudoLégaux[i] & ~p_amis;
                        }
                    }
                }
            }
            if (légaux == 0ul)
            {
                return false;
            }
            return true;
        }

        public ulong CalculsLégauxEnnemisPat(int square, ulong p_ennemis)
        {
            ulong cases_attaquees = 0ul;
            int colonne = square % 8;
            if (Blanc == false)
            {
                if (pieces[square] == 6)
                {
                    if (colonne != 0)
                    {
                        cases_attaquees |= 1ul << (square + 7);
                    }
                    if (colonne != 7)
                    {
                        cases_attaquees |= 1ul << (square + 9);
                    }
                }
                else
                {
                    cases_attaquees |= PseudoLégaux[square];
                }
            }
            else
            {
                if (pieces[square] == 106)
                {
                    if (colonne != 7)
                    {
                        cases_attaquees |= 1ul << (square - 7);
                    }
                    if (colonne != 0)
                    {
                        cases_attaquees |= 1ul << (square - 9);
                    }
                }
                else
                {
                    cases_attaquees |= PseudoLégaux[square];
                }
            }
            return cases_attaquees;
        }

        //Si tour fait échec offset 8, rend le coup du roi vers offset -8 illégal. Retourne un ulong des cases autorisées
        public ulong PatCoupIllégalDosPiece(int square)
        {
            //Debug.WriteLine("On vérifie les coups dans le dos");
            ulong piece_echec = Checkers;
            ulong cases_autorisees = 0ul;
            while (piece_echec != 0ul)
            {
                int attaquant = BitOperations.TrailingZeroCount(Checkers);
                int diff = Offset(square, attaquant);
                int numéro_attaquant = pieces[attaquant];
                //Debug.WriteLine($"On vérifie une potentielle case derrière avec diff = {diff}, attaquant = {attaquant} et numéro = {numéro_attaquant}");
                if (Blanc)
                {
                    //Debug.WriteLine("On est blanc");
                    if (square >= diff && square - diff < 64)
                    {
                        //Debug.WriteLine($"La case derrière existe : {square - diff}");
                        if (numéro_attaquant == 102 || numéro_attaquant == 103)
                        {
                            if (diff == 8 || diff == 1 || diff == -8 || diff == -1)
                            {
                                cases_autorisees |= 1ul << (square - diff);
                            }
                        }
                        if (numéro_attaquant == 102 || numéro_attaquant == 104)
                        {
                            if (diff == 7 || diff == 9 || diff == -7 || diff == -9)
                            {
                                cases_autorisees |= 1ul << (square - diff);
                            }
                        }
                    }
                }
                else
                {
                    //Debug.WriteLine("On est noir");
                    if (square >= diff && square - diff < 64)
                    {
                        //Debug.WriteLine($"La case derrière existe : {square - diff}");
                        if (numéro_attaquant == 2 || numéro_attaquant == 3)
                        {
                            if (diff == 8 || diff == 1 || diff == -8 || diff == -1)
                            {
                                cases_autorisees |= 1ul << (square - diff);
                            }
                        }
                        if (numéro_attaquant == 2 || numéro_attaquant == 4)
                        {
                            if (diff == 7 || diff == 9 || diff == -7 || diff == -9)
                            {
                                cases_autorisees |= 1ul << (square - diff);
                            }
                        }
                    }
                }
                piece_echec &= piece_echec - 1;
            }
            return ~cases_autorisees;  //Cases interdites sont avec un 0, 1 est ok
        }

        public void Reset_partie()    //Sert à garder le même objet en mémoire, donc pas de recharge au niveau des engine de réflexion, affichage etc
        {
            List<string> nv_partie = new List<string>();
            partie = nv_partie;
            dernier_coup = "";
            compteur_50coups = 0;
            partie_finie = false;
            blanc = true;

            roque_long_blanc = true;
            roque_long_noir = true;
            roque_noir = true;
            roque_blanc = true;

            en_passant_bool = false;   //Savoir si en passant fait parti des coups légaux
            promotion_bool = false;
            InCheck = false;

            somme_masques_echecs = 0ul;
            MasqueEchecDirect = 0ul;
            checkers = 0ul;

            for (int i = 2; i < 6; i++)
            {
                if (i == 2)
                {
                    Bitboards_blanc[i] = 0x00000000_00000008ul; //Positionnement de la dame blanche
                }
                else if (i == 3)
                {
                    Bitboards_blanc[i] = 0x00000000_00000081ul;  //Tours blanches
                }
                else if (i == 4)
                {
                    Bitboards_blanc[i] = 0x00000000_00000024ul;  //Fous blancs
                }
                else
                {
                    Bitboards_blanc[i] = 0x00000000_00000042ul; //Cavaliers blancs
                }
                Bitboards_noir[i] = Bitboards_blanc[i] << 56;
            }
            Bitboards_blanc[6] = 0x00000000_0000ff00ul;
            Bitboards_noir[6] = 0x00ff0000_00000000ul;
            Bitboards_noir[0] = 0b11111111_11111111_00000000_00000000_00000000_00000000_00000000_00000000ul;
            Bitboards_blanc[0] = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_11111111ul;
            pieces_long = Bitboards_blanc[0] | Bitboards_noir[0];

            case_roi_blanc = 4;
            case_roi_noir = 60;
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    int carré = ligne * 8 + colonne;
                    if (ligne == 7)
                    {
                        if (colonne == 0) { pieces[carré] = 103; }
                        if (colonne == 1) { pieces[carré] = 105; }
                        if (colonne == 2) { pieces[carré] = 104; }
                        if (colonne == 3) { pieces[carré] = 102; }
                        if (colonne == 4) { pieces[carré] = 101; }
                        if (colonne == 5) { pieces[carré] = 104; }
                        if (colonne == 6) { pieces[carré] = 105; }
                        if (colonne == 7) { pieces[carré] = 103; }
                    }
                    else if (ligne == 6)
                    { pieces[carré] = 106; }
                    else if (ligne == 1)
                    { pieces[carré] = 6; }
                    else if (ligne == 0)
                    {
                        if (colonne == 0) { pieces[carré] = 3; }
                        if (colonne == 1) { pieces[carré] = 5; }
                        if (colonne == 2) { pieces[carré] = 4; }
                        if (colonne == 3) { pieces[carré] = 2; }
                        if (colonne == 4) { pieces[carré] = 1; }
                        if (colonne == 5) { pieces[carré] = 4; }
                        if (colonne == 6) { pieces[carré] = 5; }
                        if (colonne == 7) { pieces[carré] = 3; }
                    }
                    else
                    {
                        pieces[carré] = 50;
                    }
                }
            }

            //Initialisation des légaux
            Filtrage_colonne();
            légaux_ini();
        }

        public void Filtrage_colonne()
        {
            Filtres_colonne[0] = 0x00000000_00000000;
            Filtres_colonne[1] = 0x01010101_01010101;
            Filtres_colonne[2] = 0x03030303_03030303;
            Filtres_colonne[3] = 0x07070707_07070707;
            Filtres_colonne[4] = 0x0f0f0f0f_0f0f0f0f;
            Filtres_colonne[5] = 0x1f1f1f1f_1f1f1f1f;
            Filtres_colonne[6] = 0x3f3f3f3f_3f3f3f3f;
            Filtres_colonne[7] = 0x7f7f7f7f_7f7f7f7f;
        }


        private void légaux_ini()
        {
            for (int i = 0; i < 64; i++)
            {
                PseudoLégaux[i] = coups_légaux(i);
            }
        }

        public void AffichageBoard()
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    int piece = pieces[ligne * 8 + colonne];
                    if (piece == 50)
                    {
                        Debug.Write("  ");
                    }
                    else if (piece % 10 == 1)
                    {
                        Debug.Write("\u2654 ");
                    }
                    else if (piece % 10 == 2)
                    {
                        Debug.Write("\u2655 ");
                    }
                    else if (piece % 10 == 3)
                    {
                        Debug.Write("\u2656 ");
                    }
                    else if (piece % 10 == 4)
                    {
                        Debug.Write("\u2657 ");
                    }
                    else if (piece % 10 == 5)
                    {
                        Debug.Write("\u2658 ");
                    }
                    else
                    {
                        Debug.Write("\u2659 ");
                    }
                }
                Debug.WriteLine("");
            }
        }

        public void TraductionFEN(string FEN)
        {
            FEN = FEN.Trim();
            int longueur = FEN.Length;
            int parcours = 0;
            int colonne = 0;
            int ligne = 7;
            while (FEN[parcours] != ' ')
            {
                char caractere = FEN[parcours];
                if (caractere == '/')
                {
                    ligne--;
                    colonne = 0;
                }
                else if (Char.IsDigit(caractere))
                {
                    for (int i = 0; i < caractere - '0'; i++)
                    {
                        pieces[ligne * 8 + colonne] = 50;
                        colonne++;
                    }
                }
                else if (Char.ToLower(caractere) == caractere)  //C'est un caractère minuscule, donc une pièce noire
                {
                    if (caractere == 'k')
                    {
                        pieces[ligne * 8 + colonne] = 101;
                        CaseRoiNoir = ligne * 8 + colonne;
                    }
                    if (caractere == 'q')
                    {
                        pieces[ligne * 8 + colonne] = 102;
                    }
                    if (caractere == 'r')
                    {
                        pieces[ligne * 8 + colonne] = 103;
                    }
                    if (caractere == 'b')
                    {
                        pieces[ligne * 8 + colonne] = 104;
                    }
                    if (caractere == 'n')
                    {
                        pieces[ligne * 8 + colonne] = 105;
                    }
                    if (caractere == 'p')
                    {
                        pieces[ligne * 8 + colonne] = 106;
                    }
                    Bitboards_noir[0] |= 1ul << (ligne * 8 + colonne);
                    colonne++;
                }
                else   //Pas un espace, pas un /, pas un numéro, pas une minuscule
                {
                    if (caractere == 'K')
                    {
                        pieces[ligne * 8 + colonne] = 1;
                        CaseRoiBlanc = ligne * 8 + colonne;
                    }
                    if (caractere == 'Q')
                    {
                        pieces[ligne * 8 + colonne] = 2;
                    }
                    if (caractere == 'R')
                    {
                        pieces[ligne * 8 + colonne] = 3;
                    }
                    if (caractere == 'B')
                    {
                        pieces[ligne * 8 + colonne] = 4;
                    }
                    if (caractere == 'N')
                    {
                        pieces[ligne * 8 + colonne] = 5;
                    }
                    if (caractere == 'P')
                    {
                        pieces[ligne * 8 + colonne] = 6;
                    }
                    Bitboards_blanc[0] |= 1ul << (ligne * 8 + colonne);
                    colonne++;
                }
                parcours++;
            }

            //Premier espace atteint, on a la couleur
            parcours++;
            Blanc = FEN[parcours] == 'w' ? true : false;

            //On s'attaque aux roques
            parcours = parcours + 2;
            if (FEN[parcours] == 'K')
            {
                Roque_blanc = true;
                parcours++;
            }
            else
            {
                Roque_blanc = false;
            }
            if (FEN[parcours] == 'Q')
            {
                Roque_long_blanc = true;
                parcours++;
            }
            else
            {
                Roque_long_blanc = false;
            }
            if (FEN[parcours] == 'k')
            {
                Roque_noir = true;
                parcours++;
            }
            else
            {
                Roque_noir = false;
            }
            if (FEN[parcours] == 'q')
            {
                Roque_long_noir = true;
                parcours++;
            }
            else
            {
                Roque_long_noir = false;
            }
            parcours++;

            if (parcours < longueur && FEN[parcours] == ' ') //Dans le cas où on est ....NR - e3 (Aucun roque possible => roques représentées par '-')
            {
                parcours++;
            }

            //On est à la case en passant
            if (parcours >= longueur || FEN[parcours] == '-')
            {
                DernierCoup = "xxxx";
                parcours = parcours + 2;
            }
            else
            {
                EnPassantFEN(FEN.Substring(parcours, 2));
                parcours = parcours + 3;
            }
            //Debug.WriteLine("Avant le compteur50coups : " + FEN[parcours]);
            //On est au demi-coups depuis capture
            if (parcours < longueur)
            {
                if (parcours  < longueur + 1)
                {
                    if (FEN[parcours + 1] != ' ')
                    {
                        compteur_50coups = (FEN[parcours] - '0') * 10 + FEN[parcours] - '0';
                    }
                }
                else
                {
                    compteur_50coups = FEN[parcours] - '0';
                }
            }
            else compteur_50coups = 0;
            
            parcours = parcours + 2;

            //On note juste le début dans le partie dans List<string> Partie
            if (parcours < longueur) Partie.Add("DEBUT COUP " + FEN[parcours]);

            //On va initialiser les coups légaux
            Filtrage_colonne();
            pieces_long = Bitboards_blanc[0] | Bitboards_noir[0];
            for (int i = 0; i < 64; i++)
            {
                PseudoLégaux[i] = coups_légaux(i);
            }
            MasquesCheck();

            //On met en place les derniers paramètres
            promotion_bool = false;
            partie_finie = false;

            //Test affichage légaux
            /*
            bool couleur = Blanc;
            Blanc = false;
            ulong[] Coups = Calcul_légaux();
            for (int i = 0; i < 64; i++)
            {
                Debug.WriteLine($"{i}. {Coups[i]:B64}");
            }
            Blanc = couleur;
            */
        }
    }
}