using Benchmark;
using IA_echecs;
using Org.BouncyCastle.Tls;
using Regles;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Echiquier.MainWindow;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Formats.Asn1.AsnWriter;
//using AffichagePartie;

namespace Echiquier     //Seule règle non prise en compte : nulle par 3 répétitions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Moteur mot;
        public Reflexion IA;
        public bool EvaluationEnCours = false;
        public int usage;

        public MainWindow()
        {
            mot = new Moteur();
            IA = new Reflexion(mot, true);
            InitializeComponent();
            CreateChessboard();
            SetPieces();
            ModeUtilisation();
        }

        public Border[] cases = new Border[64];

        public void CreateChessboard()
        {
            for (int i = 0; i < 8; i++)
            {
                Echiquier.RowDefinitions.Add(new RowDefinition());
                Echiquier.ColumnDefinitions.Add(new ColumnDefinition());
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
                    Echiquier.Children.Add(square);
                }
            }
        }

        public bool CreerBoutonsAnalyse()
        {
            //On ajoute une nouvelle colonne à l'échiquier pour afficher les boutons
            Echiquier.ColumnDefinitions.Add(new ColumnDefinition());

            //Création du bouton d'avancer (procédure liée : AvancerReplay())
            Button avancer = new Button();
            avancer.Height = 30;
            avancer.Width = 30;
            avancer.Content = "▶";
            avancer.HorizontalAlignment = HorizontalAlignment.Left;
            avancer.VerticalAlignment = VerticalAlignment.Bottom;
            avancer.Click += AvancerReplay;

            //Création du bouton de retour en arrière (procédure liée : ReculerReplay())
            Button reculer = new Button();
            reculer.Height = 30;
            reculer.Width = 30;
            reculer.Content = "◀";
            reculer.HorizontalAlignment = HorizontalAlignment.Right;
            reculer.VerticalAlignment = VerticalAlignment.Bottom;
            reculer.Click += ReculerReplay;

            //Création de la Grid qui contient les deux boutons
            Grid panneau_analyse = new Grid();
            panneau_analyse.ColumnDefinitions.Add(new ColumnDefinition());
            panneau_analyse.ColumnDefinitions.Add(new ColumnDefinition());
            panneau_analyse.RowDefinitions.Add(new RowDefinition());
            panneau_analyse.Height = 90;
            panneau_analyse.Width = 65;

            //Affiliation des boutons à la grid
            Grid.SetRow(avancer, 0);
            Grid.SetColumn(avancer, 1);
            Grid.SetColumn(reculer, 0);
            Grid.SetRow(reculer, 0);
            panneau_analyse.Children.Add(reculer);
            panneau_analyse.Children.Add(avancer);

            //Affiliation de la grid à l'échiquier principal
            Grid.SetColumn(panneau_analyse, Echiquier.RowDefinitions.Count - 1);
            Grid.SetRow(panneau_analyse, Echiquier.Children.Count - 1);
            Echiquier.Children.Add(panneau_analyse);

            return true; //Nécessaire pour pouvoir attendre la fin de cette fonction avant de continuer
        }


        public struct PieceWPF
        {
            public int piece_logique;
            public BitmapImage image;
        }



        public PieceWPF Completion_piece(PieceWPF piece)  //Complétion des infos sur la pièce blanche à partir du numéro
        {
            piece.image = new BitmapImage();
            piece.image.BeginInit();
            if (piece.piece_logique == 50) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\transparent.webp"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 1) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\roi_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 2) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 3) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\tour_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 4) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\fou_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 5) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\cavalier_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 6) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\pion_blanc.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 101) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\roi_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 102) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 103) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\tour_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 104) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\fou_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 105) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\cavalier_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            if (piece.piece_logique == 106) { piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\pion_noir.png"), UriKind.Absolute); piece.image.EndInit(); return piece; }
            //Pour assurer que tous les numéros aient une UriSource
            Debug.WriteLine("Je suis arrivé à la fin de la création de pièce");
            piece.image.UriSource = new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Images Pieces\dame_blanc.png"), UriKind.Absolute);
            piece.image.EndInit();
            return piece;
        }


        public void SetPieces()
        {
            foreach (Border carré_piece in Echiquier.Children)
            {
                Button bouton = new Button();
                PieceWPF piece = new PieceWPF();
                int ligne = Math.Abs(Grid.GetRow(carré_piece) - 7);
                int colonne = Grid.GetColumn(carré_piece);
                int carré = ligne * 8 + colonne;
                int piece_logique = mot.Donneur_numéro(carré);  //Récupère le numéro de la piece
                piece.piece_logique = piece_logique;
                piece = Completion_piece(piece);  //Complète toutes les infos sur la piece
                if (piece.piece_logique != 50)
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

        //Peut choisir entre rejouer une partie (avec ou sans debug ?), faire un affrontement ou tester manuellement
        //Je ferai pas du front-end mdr ça va être un squelette le truc
        public void ModeUtilisation()
        {
            usage = 0;

            string manuel = "Jouer normalement";
            string bataille = "Faire un affrontement";
            string replay = "Revoir une partie";

            Window mode = new Window();
            mode.Width = 300;
            mode.Height = 175;
            mode.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            TextBlock Titre = new TextBlock();
            Titre.Text = "Mode d'utilisation de l'affichage ?";
            Titre.Height = 25;
            Titre.Width = 280;
            Titre.TextAlignment = TextAlignment.Center;

            ListBox options = new ListBox();
            options.Width = 280;
            options.Height = 150;
            options.Margin = new Thickness(5);
            options.ItemsSource = new string[] { bataille, manuel, replay };
            options.SelectionChanged += (s, e) =>
            {
                string utilisation = (string)((ListBox)s).SelectedItem;
                if (utilisation == "Jouer normalement")
                {
                    usage = 0;
                    mode.Close();
                }
                else if (utilisation == "Faire un affrontement")
                {
                    usage = 11;
                    mode.Close();
                    LancerEvaluation();
                }
                else
                {
                    usage = 222;
                    mode.Close();
                    ReplayPartie();
                }
            };

            StackPanel ecran_affichage = new StackPanel();
            ecran_affichage.Children.Add(Titre);
            ecran_affichage.Children.Add(options);

            mode.Content = ecran_affichage;
            mode.ShowDialog();
        }



        public int carré_précédent = -1;

        public bool Realisation_coup_robot(int carré_précédent, int carré)
        {
            bool vainqueur = false;
            (int départ, int arrivée) coup = (carré_précédent, carré);
            if (coup.départ == coup.arrivée)
            {
                if (coup.départ == 100)
                {
                    Procédure_nulle("échec et mat!!", true);
                }
                Procédure_nulle("Pat", true);
            }
            if (coup.arrivée > -1)
            {
                vainqueur = Realisation_coup(coup.départ, coup.arrivée);
                Button bouton_départ = (Button)cases[coup.départ].Child;
                Button bouton_arrivée = (Button)cases[coup.arrivée].Child;
                bouton_départ.Background = new SolidColorBrush(Color.FromRgb(210, 150, 80));
                bouton_arrivée.Background = new SolidColorBrush(Color.FromRgb(210, 150, 80));
            }
            return vainqueur;
            //else      //Test sans cette boucle
            //{
            //    _ = Procédure_nulle("échec et mat!!", false);   //Les noirs ont perdu par échec et mat
            //}
        }

        public async void CaseCliquée(object objet, RoutedEventArgs touche)  //Réaction à chaque clic sur une case
        {
            //Système de partie automatique

            if (usage == 0) //Partie normale
            {
                await PartieManuelle(objet, touche);
            }
            else if (usage == 11)
            {
                LancerEvaluation();
            }
            else
            {
                ReplayPartie();
            }
        }
        public async Task<bool> PartieManuelle(object objet, RoutedEventArgs touche)
        {
            Button bouton = (Button)objet;
            PieceWPF piece = (PieceWPF)bouton.Tag;
            //obtient les coordonnées de la case de la pièce
            Border carré_piece = (Border)VisualTreeHelper.GetParent(bouton);
            int ligne = Math.Abs(Grid.GetRow(carré_piece) - 7);
            int colonne = Grid.GetColumn(carré_piece);
            int carré = ligne * 8 + colonne;            //de 0 à 63, de A1 à H1, puis de A2 à H2 etc
            if (((SolidColorBrush)bouton.Background).Color == Color.FromRgb(180, 64, 64))   //Un coup légal a une couleur de fond différente
            {
                Realisation_coup(carré_précédent, carré);   //On déplace la pièce cliquée (couleur = blanc et passe à noir dans la fonction)
            }
            else
            {
                Affichage_légaux(bouton, piece, carré);   //Si la case sélectionné ne correspond pas à un coup jouable
            }
            await Task.Delay(200);
            return true;
        }

        public void LancerEvaluation()
        {
            if (EvaluationEnCours == false)
            {
                EvaluationEnCours = true;
                Evaluation();
            }
            else
            {
                //On affiche le résultat partiel

            }
        }

        public void ReplayPartie()
        {
            CreerBoutonsAnalyse();

            int affrontement = ChoixAffrontementReplay();

            (string? partie, float resultat) partie_a_jouer = SelectionPartieReplay(affrontement);

            if (partie_a_jouer.partie == null)
            {
                MessageBox.Show("Partie non trouvée, sortie du menu");
                Close();
                return;
            }

            Moteur.Coup[] coups_partie = ConversionStringCoups(partie_a_jouer.partie);

            for (int coup = 0; coup < coups_partie.Length; coup++)
            {

            }
        }


        private int ChoixAffrontementReplay()
        {
            int affrontement = 0;

            int nb_affrontement_max = Sauvegarde_database.NombreMaxAffrontement();

            Window fenetre = new Window();
            fenetre.Height = 175;
            fenetre.Width = 400;
            fenetre.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            TextBlock titre = new TextBlock();
            titre.Text = $"Choisir l'id de l'affrontement à étudier (maximum : {nb_affrontement_max})";
            titre.Height = 25;
            titre.Width = 400;

            TextBox textbox = new TextBox();
            textbox.Height = 50;
            textbox.Width = 400;
            textbox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        int affrontement_choisi = Convert.ToInt32(textbox.Text);
                        if (affrontement_choisi <= nb_affrontement_max)
                        {
                            affrontement = affrontement_choisi;
                            fenetre.Close();
                        }
                        else
                        {
                            MessageBox.Show("L'affrontement selectionné n'est pas trouvé");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("La valeur de rentrée ne correspond pas à un affrontement existant. " + ex.ToString());
                    }
                }
            };
            StackPanel ecran_affichage = new StackPanel();
            ecran_affichage.Children.Add(titre);
            ecran_affichage.Children.Add(textbox);

            fenetre.Content = ecran_affichage;
            fenetre.ShowDialog();

            return affrontement;
        }

        private (string?, float) SelectionPartieReplay(int id_affrontement)
        {
            int num_partie = 0;
            (string? partie, float resultat) partie_choisie = (null, -1);
            int Nb_parties_max = Sauvegarde_database.NombrePartiesDansAffrontement(id_affrontement);
            if (Nb_parties_max < 1)
            {
                return partie_choisie;
            }
            Window choix_partie = new Window();
            choix_partie.Width = 400;
            choix_partie.Height = 150;
            choix_partie.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            TextBlock Titre = new TextBlock();
            Titre.Width = 400;
            Titre.Height = 30;

            ListBox liste = new ListBox();
            liste.Height = 100;
            liste.Width = 400;
            liste.ItemsSource = new string[] { "aléatoire", "partie spécifique (à préciser)" };
            liste.SelectionChanged += (s, e) =>
            {
                if ((string)liste.SelectedItem == "aléatoire")
                {
                    Random random = new Random();
                    num_partie = random.Next(Nb_parties_max);
                    if (num_partie >= 0)
                    {
                        string? déroulement_partie = Sauvegarde_database.CoupsPartie(num_partie, id_affrontement);
                        
                        if (déroulement_partie != null)
                        {
                            float resultat = Sauvegarde_database.ResultatPartie(num_partie, id_affrontement);
                            partie_choisie = (déroulement_partie, resultat);
                        }
                    }
                }
                else
                {
                    num_partie = NumPartieDansAffrontement(id_affrontement, Nb_parties_max);
                    if (num_partie >= 0)
                    {
                        string? déroulement_partie = Sauvegarde_database.CoupsPartie(num_partie, id_affrontement);
                        if (déroulement_partie != null)
                        {
                            float resultat = Sauvegarde_database.ResultatPartie(num_partie, id_affrontement);
                            partie_choisie = (déroulement_partie, resultat);
                        }
                    }
                }
            };

            StackPanel affichage = new StackPanel();
            affichage.Children.Add(Titre);
            affichage.Children.Add(liste);

            choix_partie.Content = affichage;
            choix_partie.ShowDialog();

            return partie_choisie;
        }

        private int NumPartieDansAffrontement(int id_affrontement, int nombre_max)
        {
            int partie = 0;
            bool erreur = false;
            TextBlock titre = new TextBlock();
            titre.Text = $"Choisir l'id de la partie à étudier (maximum : {nombre_max})";

            TextBox textbox = new TextBox();
            textbox.PreviewTextInput += (s, e) =>
            {
                e.Handled = char.IsDigit(e.Text[0]) == false;   //Si c'est bien un numéro on le prend
            };
            textbox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        int partie_choisi = Convert.ToInt32(textbox.Text);
                        if (partie_choisi <= nombre_max)
                        {
                            partie = partie_choisi;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("La valeur de rentrée ne correspond pas à un affrontement existant. " + ex.ToString());
                        erreur = true;
                    }
                }
            };
            if (erreur)
            {
                return -1;
            }
            return partie;

        }
        private async void AvancerReplay(object case_cliquee, EventArgs args)
        {

        }

        private async void ReculerReplay(object case_cliquee, EventArgs args)
        {

        }

        //Déplace la pièce en prenant compte des roques, des en passant et des promotions
        public bool Realisation_coup(int carré_précédent, int carré)
        {
            if (mot.EnPassantEffectué(carré_précédent, carré) == true)   //Fonction de check, ne change rien au moteur
            {
                EchangePieces_EnPassant(carré_précédent, carré);   //Supprime juste le pion pris
            }
            EchangePieces(carré_précédent, carré);   //Fait juste l'affichage

            if (mot.Roque_effectué(carré_précédent, carré) == true)   //Vérifie si le dernier coup est le roque
            {
                Faire_roque(carré_précédent, carré);
            }

            bool partie_gagnée = mot.Realisation_coup_logique(carré_précédent, carré);   //Après roque, avant promotion     //SEULE MODIFICATION APPORTEE AU MOTEUR, déclencheur de la démarche moteur autonome

            if (mot.Promotion_bool) Promotion(carré);

            ResetBoard();
            if (mot.PartieFinie == true)     //Aucun coup n'est légal (on a changé de couleur juste avant donc couleur OK )
            {
                if (partie_gagnée)  //Si le roi est en échec sur sa case (piece se sert qu'à avoir la couleur en int)
                {
                    Debug.WriteLine("La partie a été gagnée");
                    Procédure_nulle("échec et mat!!", mot.Blanc);
                }
                else
                {
                    Procédure_nulle("pat", mot.Blanc);
                }
            }
            return partie_gagnée;
        }


        //Affiche les coups légaux d'une pièce cliquée
        public void Affichage_légaux(Button bouton, PieceWPF piece, int carré)
        {
            ResetBoard();
            if ((piece.piece_logique < 10) == mot.Blanc)   //Vérifie que la piece est de la couleur à jouer
            {
                //ulong légaux = mot.coups_légaux(carré, true);

                Moteur.Coup[] tous_légaux = mot.Calcul_légaux();    //Calcule les coups légaux de base
                List<int> légaux_piece = ConversionCoupUlong(tous_légaux, carré);

                foreach (int coup in légaux_piece)       //Colorie les cases
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
            piece.piece_logique = 50;
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
            if (pion_acteur.piece_logique == 6)
            {
                case_pion_pris = carré - 8;  //Le pion est visuellement en dessous de celui pris
            }
            else
            {
                case_pion_pris = carré + 8;  //Idem mais au dessus
            }
            PieceWPF piece_prise = new PieceWPF();
            piece_prise.piece_logique = 50;
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
            int choix = Promotion_choix_automatique(carré);
            nvlle_piece.piece_logique = mot.Promotion(choix, carré);

            //nvlle_piece.piece_logique.numéro = mot.Blanc ? 102 : 2;   //Blanc a changé avant la procédure, noir signifie donc blanc et inversement
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



        public int Promotion_choix()   //Créer la fenêtre de choix et récupère la réponse
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

            int choix = 0;
            if (piece == "dame")
            {
                choix = 0;
            }
            else if (piece == "tour")
            {
                choix = 1;
            }
            else if (piece == "fou")
            {
                choix = 2;
            }
            else
            {
                choix = 3;
            }
            return choix;
        }

        public int Promotion_choix_automatique(int carré)
        {
            int choix = IA.Promotion(carré);
            return choix;
        }




        public void Procédure_nulle(string raison, bool blanc)  //Transmets la couleur qui a perdu
        {
            Debug.WriteLine("La partie est finie");
        }


        public int NombrePartiesPourEvaluation()
        {
            int nombre_partie = -1;

            Window fenetre = new Window();
            fenetre.Height = 120;
            fenetre.Width = 400;
            fenetre.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            TextBlock titre = new TextBlock();
            titre.Text = $"Choisir le nombre de parties pour l'affrontement qui va avoir lieu)";
            titre.Height = 25;
            titre.Width = 400;

            TextBox textbox = new TextBox();
            textbox.Height = 50;
            textbox.Width = 200;
            textbox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        int nombre_choisi = Convert.ToInt32(textbox.Text);
                        if (nombre_choisi > 0 && nombre_choisi % 2 == 0)
                        {
                            nombre_partie = nombre_choisi;
                            fenetre.Close();
                        }
                        else
                        {
                            MessageBox.Show("Nombre de parties invalide (RAPPEL : il faut un nombre POSITIF ET PAIR)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("La valeur doit être POSITIVE et PAIRE. " + ex.ToString());
                    }
                }
            };

            StackPanel ecran_affichage = new StackPanel();
            ecran_affichage.Children.Add(titre);
            ecran_affichage.Children.Add(textbox);

            fenetre.Content = ecran_affichage;
            fenetre.ShowDialog();

            return nombre_partie;
        }
        public async Task<bool> Evaluation()
        {
            int nombre_parties_a_jouer = NombrePartiesPourEvaluation();
            if (nombre_parties_a_jouer < 0)
            {
                Debug.WriteLine("Evaluation impossible à faire, nombre de parties à jouer non conforme");
                return false;
            }

            Rating estimation = new Rating(mot, nombre_parties_a_jouer);
            bool Couleur = true;
            for (int partie = 0; partie < estimation.Parties.Length; partie++)
            {
                float résultat = 0;
                try
                {
                    résultat = await FairePartie(estimation, Couleur, partie);   //Donne le résultat du blanc, 1, 0 ou 0.5
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }
                estimation.Resultats[partie] = résultat;
                estimation.Score += résultat;   //Le résultat indique si le programme 1 a gagné (résultat = 1) ou perdu (résultat = 0)
                Debug.WriteLine($"Partie n°{partie} terminée, résultat = {résultat}");
                Couleur = !Couleur;   //On alterne les couleurs à chaque partie
            }
            Sauvegarde_database.Sauvegarde_affrontement(estimation.Parties, estimation.Resultats, estimation.Score, 2, 1);
            EvaluationEnCours = false;
            return true;
        }


        // 1 <=> Reflexion gagne ; 0 <=> engine_comp gagne  et 0.5 nulle
        public async Task<float> FairePartie(Rating estimation, bool Couleur, int index_partie)
        {
            bool vainqueur = false;
            while (mot.PartieFinie == false)
            {
                if (Couleur)     //Au premier tour, couleur indique les blancs
                {
                    Moteur.Coup coup_1 = estimation.Engine_1.MeilleurCoup();   
                    vainqueur = Realisation_coup_robot(coup_1.départ, coup_1.arrivee);
                }
                else
                {
                    Moteur.Coup coup_2 = estimation.Engine_2.CoupRandom();
                    vainqueur = Realisation_coup_robot(coup_2.départ, coup_2.arrivee);
                }
                Couleur = !Couleur;
                await Task.Delay(10);
            }
            estimation.Parties[index_partie] = new List<string>();
            estimation.Parties[index_partie] = mot.Partie;
            mot.Reset_partie();
            ResetPartie();
            if (vainqueur)   //On sait que un des deux a gagné
            {
                return Couleur ? 0 : 1; //La couleur est inversé donc si Couleur = false, le programme n°1 a gagné
            }
            return 0.5f;
        }

        public void ResetPartie()
        {
            SetPieces();
        }


        public List<int> ConversionCoupUlong(Moteur.Coup[] coups, int carré)
        {
            List<int> coups_autorisees = new List<int>();
            foreach (Moteur.Coup coup in  coups)
            {
                if (coup.départ == carré)
                {
                    coups_autorisees.Add(coup.arrivee);
                }
            }
            return coups_autorisees;
        }

        public Moteur.Coup[] ConversionStringCoups(string partie)
        {
            string[] coups_partie = partie.Trim().Split(' ');
            int ajout = coups_partie.Length % 3 == 0 ? 0 : (coups_partie.Length - 1) % 3;
            int nombre_coups = 2 * (coups_partie.Length / 3) + ajout;
            Moteur.Coup[] coups = new Moteur.Coup[nombre_coups];
            int rang_coup = 0;
            for (int i = 0; i < coups_partie.Length; i++)
            {
                string coup_specifique = coups_partie[i];
                if (i % 3 != 0 && coup_specifique.Length > 0)
                {
                    if (coup_specifique[0] == Char.ToUpper(coup_specifique[0]))
                    {
                        coup_specifique = coup_specifique.Substring(1);
                    }
                    int colonne_depart = coup_specifique[0] - 'a';
                    int ligne_départ = coup_specifique[1] - '1';

                    coup_specifique = coup_specifique[2] == 'x' ? coup_specifique.Substring(3) : coup_specifique.Substring(2);
                    int colonne_arrivee = coup_specifique[0] - 'a';
                    int ligne_arrivee = coup_specifique[1] - '1';

                    coups[rang_coup] = Moteur.CreerCoup(ligne_départ * 8 + colonne_depart, ligne_arrivee * 8 + colonne_arrivee);
                    rang_coup++;
                }
            }

            return coups;
        }
    }
}