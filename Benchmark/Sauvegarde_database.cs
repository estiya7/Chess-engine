using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Benchmark
{
    public static class Sauvegarde_database
    {
        public static void Sauvegarde_partie(MySqlConnection connexion, List<string> partie, float résultat, int ordre, int affrontement_id)
        {
            int max_iter = 10000;
            int id_partie = 0;
            string cmd_partie = "SELECT MAX(ID_Partie) FROM Partie";
            using (MySqlCommand cmd = new MySqlCommand(cmd_partie, connexion))
            {
                try
                {
                    id_partie = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                catch
                {
                    id_partie = 1;
                }
            }

            string ajout_partie = "INSERT INTO Partie (ID_Partie, id_affrontement, Ordre_partie, Resultat, Coups) " +
                "VALUES (@id, @id_bataille, @num_partie, @res, @str_partie);";
            MySqlCommand commande = new MySqlCommand(ajout_partie, connexion);
            string partie_entiere = "";

            for (int i = 0; i < partie.Count && i < max_iter; i++)
            {
                if (i % 2 == 0)
                {
                    partie_entiere += ((i / 2) + 1).ToString() + ". ";
                }
                partie_entiere += partie[i] + " ";
            }

            commande.Parameters.AddWithValue("@id", id_partie);
            commande.Parameters.AddWithValue("@id_bataille", affrontement_id);
            commande.Parameters.AddWithValue("@num_partie", ordre);
            commande.Parameters.AddWithValue("@res", résultat);
            commande.Parameters.AddWithValue("@str_partie", partie_entiere);
            try
            {
                int nb_insertions = commande.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("On a pas réussi à rentrer une partie : " + ex);
                return;
            }

            commande.Dispose();
        }

        public static void Sauvegarde_affrontement(List<string>[] parties, float[] résultats, float score, int id_moteur1, int id_moteur2)
        {
            int id_affrontement = 0;
            int nb_wins = 0;
            int nb_draws = 0;
            int nb_losses = 0;
            int nb_parties = résultats.Length;
            for (int i = 0; i < nb_parties; i++)
            {
                if (résultats[i] == 1)
                {
                    nb_wins++;
                }
                else if (résultats[i] == 0)
                {
                    nb_losses++;
                }
                else
                {
                    nb_draws++;
                }
            }
            MySqlConnection connexion = null;
            try
            {
                connexion = ConnexionSql();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Connexion à la base de données impossible : " + ex);
                return;
            }
            string cmd_id_affrontement = "SELECT MAX(ID_Affrontement) FROM Affrontement;";
            using (MySqlCommand cmd = new MySqlCommand(cmd_id_affrontement, connexion))
            {
                try
                {
                    id_affrontement = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                }
                catch
                {
                    id_affrontement = 1;
                }
            }

            string affrontement = "INSERT INTO Affrontement " +
                "(ID_Affrontement, Programme_1, Programme_2, Nombre_parties, Victoires_prgm_1, Nulles, Victoires_prgm_2, Score) " +
                "VALUES (@id, @mot_1, @mot_2, @total_parties, @score_reflexion, @score_neutre, @score_comparaison, @score_total);";
            MySqlCommand commande = new MySqlCommand(affrontement, connexion);

            commande.Parameters.AddWithValue("@id", id_affrontement);
            commande.Parameters.AddWithValue("@total_parties", nb_parties);
            commande.Parameters.AddWithValue("@score_reflexion", nb_wins);
            commande.Parameters.AddWithValue("@score_neutre", nb_draws);
            commande.Parameters.AddWithValue("@score_comparaison", nb_losses);
            commande.Parameters.AddWithValue("@score_total", score);
            commande.Parameters.AddWithValue("@mot_1", id_moteur1);
            commande.Parameters.AddWithValue("@mot_2", id_moteur2);

            try
            {
                id_affrontement = Convert.ToInt32(commande.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("On a pas réussi à rentrer l'affrontement : " + ex);
                return;
            }

            commande.Dispose();

            for (int partie = 0; partie < parties.Length; partie++)
            {
                Sauvegarde_partie(connexion, parties[partie], résultats[partie], partie, id_affrontement);
            }

            connexion.Close();
            connexion.Dispose();
        }

        public static void Sauvegarde_programme(string nom, int elo)
        {
            MySqlConnection connexion = ConnexionSql();

            string ajout_programme = "INSERT INTO IA_versions (Nom, Elo) VALUES (@nom, @niveau);";

            MySqlCommand commande = new MySqlCommand(ajout_programme, connexion);
            commande.Parameters.AddWithValue("@nom", nom);
            commande.Parameters.AddWithValue("@niveau", elo);

            int nb_insertions = commande.ExecuteNonQuery();

            commande.Dispose();
            connexion.Close();
            connexion.Dispose();

        }
        public static int NombreMaxAffrontement()
        {
            int max_affrontement = 0;
            MySqlConnection conn = ConnexionSql();
            string commande = "SELECT MAX(ID_Affrontement) FROM Affrontement;";
            using (MySqlCommand command = new MySqlCommand(commande, conn))
            {
                try
                {
                    max_affrontement = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Impossible de récupérer le nombre d'affrontement déroulés : " + ex);
                }
            }

            conn.Close();
            conn.Dispose();

            return max_affrontement;
        }
        public static int NombrePartiesDansAffrontement(int id_affrontement)
        {
            int nb_parties = 0;
            MySqlConnection conn = ConnexionSql();
            string commande = "SELECT COUNT(ID_Partie) FROM Partie WHERE id_affrontement = @num_affron;";
            using (MySqlCommand command = new MySqlCommand(commande, conn))
            {
                command.Parameters.AddWithValue("@num_affron", id_affrontement);

                nb_parties = Convert.ToInt32(command.ExecuteScalar());
            }
            conn.Close();
            conn.Dispose();

            return nb_parties;
        }

        public static string? CoupsPartie(int num_partie, int id_affrontement)
        {
            string? déroulement_partie = null;

            MySqlConnection conn = ConnexionSql();

            string commande = "SELECT Coups FROM Partie WHERE id_affrontement = @id_affron AND Ordre_partie = @num_par;";
            using (MySqlCommand command = new MySqlCommand(commande, conn))
            {
                command.Parameters.AddWithValue("@id_affron", id_affrontement);
                command.Parameters.AddWithValue("@num_par", num_partie);

                try
                {
                    déroulement_partie = Convert.ToString(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Impossibilité d'extraire la partie avec" +
                        $" num_partie = {num_partie} et id_affrontement = {id_affrontement} : {ex.Message}");
                    déroulement_partie = null;
                }
            }

            conn.Close();
            conn.Dispose();

            return déroulement_partie;
        }

        public static int ResultatPartie(int num_partie, int id_affrontement)
        {
            int resultat = -1;

            MySqlConnection conn = ConnexionSql();

            string commande = "SELECT Resultat FROM Partie WHERE id_affrontement = @id_affron AND Ordre_partie = @num_par;";
            using (MySqlCommand command = new MySqlCommand(commande, conn))
            {
                command.Parameters.AddWithValue("@id_affron", id_affrontement);
                command.Parameters.AddWithValue("@num_par", num_partie);

                try
                {
                    resultat = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Impossibilité d'extraire le résultat de la partie avec" +
                        $" num_partie = {num_partie} et id_affrontement = {id_affrontement} : {ex.Message}");
                    resultat = -1;
                }
            }

            conn.Close();
            conn.Dispose();

            return resultat;
        }



        public static MySqlConnection ConnexionSql()
        {
            string str_conn = "SERVER=localhost;PORT=3306;DATABASE=Echecs;UID=IA_echecs;PASSWORD=Stockfish_v2;";
            MySqlConnection conn = new MySqlConnection(str_conn);
            conn.Open();
            return conn;
        }
    }
}
