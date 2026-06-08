using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Regles;
using Benchmark;

namespace AffichagePartie
{
    /// <summary>
    /// Logique d'interaction pour AffichageEchiquier.xaml
    /// </summary>
    public partial class AffichageEchiquier : UserControl
    {
        public Rating estimation;
        public Moteur mot;


        public AffichageEchiquier()
        {
            estimation = new Rating();
            InitializeComponent();
            SetPieces();
            mot = estimation.Moteur;
        }

        public Border[] cases = new Border[64];

        public void CreateChessboard()
        {
            for (int i = 0; i < 8; i++)
            {
                Damier.RowDefinitions.Add(new RowDefinition());
                Damier.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int row = 7; row > -1; row--)
            {
                for (int col = 0; col < 8; col++)
                {
                    Border square = new Border();
                    cases[row * 8 + col] = square;
                    // Couleur alternée
                    if ((row + col) % 2 == 0)
                    {
                        square.Background = Brushes.Maroon;
                    }
                    else
                    {
                        square.Background = Brushes.Beige;
                    }

                    // Positionner dans la grille
                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, col);

                    // Ajouter à la grille
                    Damier.Children.Add(square);
                }
            }
        }

        public struct PieceWPF
        {
            public Moteur.Piece piece_logique;
            public BitmapImage image;
        }



        public PieceWPF Completion_piece(PieceWPF piece)  //Complétion des infos sur la pièce blanche à partir du numéro
        {
            piece.image = new BitmapImage();
            piece.image.BeginInit();
            piece.piece_logique = mot.Completion_piece(piece.piece_logique);
            if (piece.piece_logique.numéro == 50) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\transparent.webp"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 1) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\roi_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 2) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 3) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\tour_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 4) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\fou_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 5) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\cavalier_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 6) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\pion_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 101) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\roi_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 102) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 103) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\tour_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 104) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\fou_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 105) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\cavalier_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique.numéro == 106) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\pion_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            //Pour assurer que tous les numéros aient une UriSource
            piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_blanc.png"), UriKind.Absolute);
            piece.image.EndInit();
            return piece;
        }


        public void SetPieces()
        {
            foreach (Border carré_piece in Damier.Children)
            {
                Button bouton = new Button();
                PieceWPF piece = new PieceWPF();
                int ligne = Math.Abs(Grid.GetRow(carré_piece) - 7);
                int colonne = Grid.GetColumn(carré_piece);
                int carré = ligne * 8 + colonne;
                Moteur.Piece piece_logique = new Moteur.Piece();
                piece_logique.numéro = mot.Donneur_numéro(piece_logique, carré);  //Récupère le numéro de la piece
                piece_logique = mot.Completion_piece(piece_logique);
                piece.piece_logique = piece_logique;
                piece = Completion_piece(piece);  //Complète toutes les infos sur la piece
                if (piece.piece_logique.numéro != 50)
                {
                    bouton.Content = new Image { Source = piece.image };  //Affiche l'image de la pièce
                }
                bouton.Tag = piece;
                //Stock dans le tableau cases[] chaque Border selon son emplacement et sa pièce dans l'autre tableau
                cases[carré] = carré_piece;
                carré_piece.Tag = piece;    //Stock dans les tags des boutons et des Border la pièce dessus
                //Associe un bouton à chaque case
                carré_piece.Child = bouton;
                //Gère toutes les interruptions lorsque que l'on clique une pièce
                //MessageBox.Show($"Je fais le setup de {carré}");
                bouton.Click += CaseCliquée;
            }
            // Reset le background de tous les boutons
            ResetBoard();
        }





        public int carré_précédent = -1;

        public void Realisation_coup_robot(int carré_précédent, int carré)
        {
            (int départ, int arrivée) coup = (carré_précédent, carré);
            if (coup.départ == coup.arrivée)
            {
                if (coup.départ == 100)
                {
                    _ = Procédure_nulle("échec et mat!!", true);
                }
                _ = Procédure_nulle("Pat", true);
            }
            PieceWPF piece_active = new PieceWPF();
            if (coup.arrivée > -1)
            {
                piece_active.piece_logique.numéro = mot.pieces[coup.départ].numéro;
                piece_active = Completion_piece(piece_active);
                Realisation_coup(coup.départ, coup.arrivée);
                Button bouton_départ = (Button)cases[coup.départ].Child;
                Button bouton_arrivée = (Button)cases[coup.arrivée].Child;
                bouton_départ.Background = new SolidColorBrush(Color.FromRgb(210, 150, 80));
                bouton_arrivée.Background = new SolidColorBrush(Color.FromRgb(210, 150, 80));
            }
            //else      //Test sans cette boucle
            //{
            //    _ = Procédure_nulle("échec et mat!!", false);   //Les noirs ont perdu par échec et mat
            //}
        }


        public async void CaseCliquée(object objet, RoutedEventArgs touche)  //Réaction à chaque clic sur une case
        {
            //Défini le bouton cliqué et la pièce dessus
            Button bouton = (Button)objet;
            PieceWPF piece = (PieceWPF)bouton.Tag;
            //obtient les coordonnées de la case de la pièce
            Border carré_piece = (Border)VisualTreeHelper.GetParent(bouton);
            int ligne = Math.Abs(Grid.GetRow(carré_piece) - 7);
            int colonne = Grid.GetColumn(carré_piece);
            int carré = ligne * 8 + colonne;            //de 0 à 63, de A1 à H1, puis de A2 à H2 etc
            if (((SolidColorBrush)bouton.Background).Color == Color.FromRgb(180, 64, 64))   //Un coup légal a une couleur de fond différente
            {
                //IA.Evaluation(mot.Blanc);
                Realisation_coup(carré_précédent, carré);   //On déplace la pièce cliquée (couleur = blanc et passe à noir dans la fonction)
                await Task.Delay(5);
                //Realisation_coup_robot();   //couleur = noir et passe à blanc
            }
            else
            {
                if (mot.Blanc) Affichage_légaux(bouton, piece, carré);   //Si la case sélectionné ne correspond pas à un coup jouable
            }
        }



        //Déplace la pièce en prenant compte des roques, des en passant et des promotions
        public void Realisation_coup(int carré_précédent, int carré)
        {
            if (mot.EnPassant_effectué(carré_précédent, carré) == true)   //Fonction de check, ne change rien au moteur
            {
                EchangePieces_EnPassant(carré_précédent, carré);   //Supprime juste le pion pris
            }

            EchangePieces(carré_précédent, carré);   //Fait juste l'affichage

            if (mot.Roque_effectué(carré_précédent, carré) == true)   //Vérifie si le dernier coup est le roque
            {
                Faire_roque(carré_précédent, carré);
            }

            bool fin_partie = mot.Realisation_coup_logique(carré_précédent, carré);   //Après roque, avant promotion     //SEULE MODIFICATION APPORTEE AU MOTEUR, déclencheur de la démarche moteur autonome

            if (mot.Promotion_bool) Promotion(carré);

            ResetBoard();
            if (mot.PartieFinie == true)     //Aucun coup n'est légal (on a changé de couleur juste avant donc couleur OK )
            {
                if (fin_partie == true)  //Si le roi est en échec sur sa case (piece se sert qu'à avoir la couleur en int)
                {
                    _ = Procédure_nulle("échec et mat!!", mot.Blanc);
                }
                else
                {
                    _ = Procédure_nulle("pat", mot.Blanc);
                }
            }
        }


        //Affiche les coups légaux d'une pièce cliquée
        public void Affichage_légaux(Button bouton, PieceWPF piece, int carré)
        {
            ResetBoard();
            if ((piece.piece_logique.numéro < 10) == mot.Blanc)   //Vérifie que la piece est de la couleur à jouer
            {
                //ulong légaux = mot.coups_légaux(carré, true);

                ulong[] tous_légaux = mot.Calcul_légaux();    //Calcule les coups légaux de base
                ulong légaux_piece = tous_légaux[carré];
                ulong légaux_to_list = légaux_piece;
                List<int> légaux = new List<int>();
                while (légaux_to_list != 0)
                {
                    int coup = BitOperations.TrailingZeroCount(légaux_to_list);
                    légaux_to_list &= légaux_to_list - 1;
                    légaux.Add(coup);
                }
                foreach (int coup in légaux)       //Colorie les cases
                {
                    Button bouton_coup = (Button)cases[coup].Child;
                    bouton_coup.Background = new SolidColorBrush(Color.FromRgb(180, 64, 64));
                }

            }
            carré_précédent = carré;
        }





        public void EchangePieces(int carré_départ, int carré_arrivée)  //Fait l'échange du carré_précédent vers le carré_objectif (départ vers arrivée)
        {
            Button bouton_carré_départ = (Button)cases[carré_départ].Child;    //Stock le Tag et bouton de la pièce à garder
            PieceWPF pieceWPF_carré_départ = (PieceWPF)cases[carré_départ].Tag;
            cases[carré_départ].Child = null;      //Retire l'ancien Tag et l'ancien Child du carré
            cases[carré_départ].Tag = null;
            cases[carré_arrivée].Child = null;
            cases[carré_arrivée].Tag = null;
            cases[carré_arrivée].Child = bouton_carré_départ;       //Restock à la case d'arrivée le Child et le Tag
            cases[carré_arrivée].Tag = pieceWPF_carré_départ;
            Button bouton = new Button();
            PieceWPF piece = new PieceWPF();
            piece.piece_logique.numéro = 50;
            piece = Completion_piece(piece);
            if ((carré_départ / 8 + carré_départ % 8) % 2 == 0) { bouton.Background = Brushes.Maroon; }
            else { bouton.Background = Brushes.Beige; }
            bouton.Tag = piece;            //content est vide car c'est une case vide
            cases[carré_départ].Tag = piece;
            cases[carré_départ].Child = bouton;
            bouton.Click += CaseCliquée;           //Réinstaure l'interruption pour pouvoir réagir au clic
        }



        public void EchangePieces_EnPassant(int carré_précédent, int carré)  //Echange les pieces dans le cas d'un en passant
        {
            PieceWPF pion_acteur = (PieceWPF)cases[carré_précédent].Tag;
            int case_pion_pris = 0;
            if (pion_acteur.piece_logique.numéro == 6)
            {
                case_pion_pris = carré - 8;  //Le pion est visuellement en dessous de celui pris
            }
            else
            {
                case_pion_pris = carré + 8;  //Idem mais au dessus
            }
            PieceWPF piece_prise = new PieceWPF();
            piece_prise.piece_logique.numéro = 50;
            piece_prise = Completion_piece(piece_prise);  //Recréer une piece vide pour la mettre à la place de la pièce prise
            Button bouton = new Button();
            bouton.Tag = piece_prise;            //Aucun content car case vide
            cases[case_pion_pris].Tag = null;       //Changement du Tag (=>piece vide) et du Child (=>nouveau bouton)
            cases[case_pion_pris].Child = null;
            cases[case_pion_pris].Tag = piece_prise;
            cases[case_pion_pris].Child = bouton;
            bouton.Click += CaseCliquée;        //Remets le clic en marche pour la case vide
        }


        public void Faire_roque(int carré_précédent, int carré)   //Complète l'action du roque en déplaçant la tour
        {
            if (carré_précédent == 4)
            {
                if (carré == 2)
                {
                    EchangePieces(0, 3);
                }
                if (carré == 6)
                {
                    EchangePieces(7, 5);
                }
            }
            if (carré_précédent == 60)
            {
                if (carré == 62)
                {
                    EchangePieces(63, 61);
                }
                if (carré == 58)
                {
                    EchangePieces(56, 59);
                }
            }
        }





        public void ResetBoard()
        {
            for (int carré_ = 0; carré_ < 64; carré_++)
            {
                int ligne_carré = carré_ / 8;
                int colonne_carré = carré_ % 8;
                Button bouton = (Button)cases[carré_].Child;
                if ((ligne_carré + colonne_carré) % 2 == 0)
                {
                    bouton.Background = Brushes.Maroon;
                }
                else
                {
                    bouton.Background = Brushes.Beige;
                }
                bouton.BorderBrush = Brushes.Transparent;
            }
        }





        public void Promotion(int carré)    //Gère la procédure de promotion d'un pion sur une case
        {
            //string choix = Promotion_choix();
            PieceWPF nvlle_piece = new PieceWPF();
            //nvlle_piece.piece_logique.numéro = mot.Promotion(choix, carré);

            nvlle_piece.piece_logique.numéro = mot.Blanc ? 102 : 2;   //Blanc a changé avant la procédure, noir signifie donc blanc et inversement
            nvlle_piece = Completion_piece(nvlle_piece);   //Lui donne une image au passage
            cases[carré].Tag = null;     //Nullification de la Border
            cases[carré].Child = null;
            Button bouton = new Button();
            bouton.Tag = nvlle_piece;           //Définition des deux Tag et du content
            bouton.Content = new Image { Source = nvlle_piece.image };
            cases[carré].Tag = nvlle_piece;
            cases[carré].Child = bouton;   //Redéfinition de son Tag et de son Child
            bouton.Click += CaseCliquée;           //Réinstaure l'interruption pour pouvoir réagir au clic
            mot.Promotion_bool = false;  //La procédure de promotion est finie, on remet à faux
        }



        public string Promotion_choix()   //Créer la fenêtre de choix et récupère la réponse
        {
            string piece = "";
            Window promotion = new Window();
            promotion.Title = "Quelle pièce à promouvoir ?";
            promotion.Width = 200;
            promotion.Height = 200;
            promotion.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ListBox options = new ListBox();
            options.Margin = new Thickness(10);
            options.ItemsSource = new string[] { "dame", "tour", "fou", "cavalier" };
            options.SelectionChanged += (s, e) =>
            {
                piece = (string)options.SelectedItem;
                promotion.Close();
            };
            promotion.Content = options;
            promotion.ShowDialog();
            return piece;
        }






        public async Task Procédure_nulle(string raison, bool blanc)  //Transmets la couleur qui a perdu
        {
            mot.PartieFinie = true;
            string couleur = "";  //couleur est la couleur qui gagne
            if (raison == "pat")
            {
                MessageBox.Show("La partie est nulle, il y a pat");
            }

            if (blanc == false) { couleur = "noirs"; }
            else { couleur = "blancs"; }
            MessageBox.Show($"Les {couleur} ont gagné par " + raison);

            await Task.Delay(100);
            //Close();
        }
    }
}