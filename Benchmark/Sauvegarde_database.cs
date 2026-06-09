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
        public static void Sauvegarde_partie(MySqlConnection connexion, List<string> partie, float résultat, int affrontement_id)
        {
            int max_iter = 100;

            string ajout_partie = "INSERT INTO Partie (id_affrontement, Resultat, Coups) VALUES (@id_bataille, @res, @str_partie);";
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

            commande.Parameters.AddWithValue("@id_bataille", affrontement_id);
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

            string affrontement = "INSERT INTO Affrontement " +
                "(Programme_1, Programme_2, Nombre_parties, Victoires_prgm_1, Nulles, Victoires_prgm_2, Score) " +
                "VALUES (@mot_1, @mot_2, @total_parties, @score_reflexion, @score_neutre, @score_comparaison, @score_total);" +
                " SELECT LAST_INSERT_ID();";
            MySqlCommand commande = new MySqlCommand(affrontement, connexion);

            commande.Parameters.AddWithValue("@total_parties", nb_parties);
            commande.Parameters.AddWithValue("@score_reflexion", nb_wins);
            commande.Parameters.AddWithValue("@score_neutre", nb_draws);
            commande.Parameters.AddWithValue("@score_comparaison", nb_losses);
            commande.Parameters.AddWithValue("@score_total", score);
            commande.Parameters.AddWithValue("@mot_1", id_moteur1);
            commande.Parameters.AddWithValue("@mot_2", id_moteur2);

            int id_affrontement;
            try
            {
                id_affrontement = Convert.ToInt32(commande.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("On a pas réussi à rentrer l'affrontement : " + ex);
                return;
            }

            commande.Dispose();

            for (int partie = 0; partie < parties.Length; partie++)
            {
                Sauvegarde_partie(connexion, parties[partie], résultats[partie], id_affrontement);
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
        public static MySqlConnection ConnexionSql()
        {
            string str_conn = "SERVER=localhost;PORT=3306;DATABASE=Echecs;UID=IA_echecs;PASSWORD=Stockfish_v2;";
            MySqlConnection conn = new MySqlConnection(str_conn);
            conn.Open();
            return conn;
        }
    }
}
