using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class StockService
    {
        private readonly string connectionString;

        public StockService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool VerifierStockDisponible(string reference, int quantiteDemandee)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "SELECT Quantite FROM Inventaire WHERE RefProduit = @RefProduit";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@RefProduit", reference);

                        object result = cmd.ExecuteScalar();
                        if (result == null) return false;

                        int quantiteStock = Convert.ToInt32(result);
                        return quantiteStock >= quantiteDemandee;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur vérification stock : " + ex.Message);
                return false;
            }
        }

        public bool MiseAJourStock(string reference, int quantiteVendue)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = @"
                        UPDATE Inventaire
                        SET Quantite = Quantite - @QuantiteVendue
                        WHERE RefProduit = @Reference AND Quantite >= @QuantiteVendue";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Quantite", quantiteVendue);
                        cmd.Parameters.AddWithValue("@RefProduit", reference);

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur mise à jour stock : " + ex.Message);
                return false;
            }
        }

        public int ObtenirQuantiteStock(string reference)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "SELECT Quantite FROM Inventaire WHERE RefProduit = @Reference";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@RefProduit", reference);
                        object result = cmd.ExecuteScalar();
                        if (result == null) return 0;

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur récupération stock : " + ex.Message);
                return 0;
            }
        }
        public void AjouterOuMettreAJourStock(string reference, int quantiteAjoutee)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string sql = @"
                        IF EXISTS (SELECT 1 FROM Inventaire WHERE RefProduit = @Reference)
                            UPDATE Inventaire SET Quantite = Quantite + @Quantite WHERE RefProduit = @Reference
                        ELSE
                            INSERT INTO Inventaire (RefProduit, Quantite) VALUES (@Reference, @Quantite)";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@RefProduit", reference);
                        cmd.Parameters.AddWithValue("@Quantite", quantiteAjoutee);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur mise à jour stock : " + ex.Message);
            }
        }
    }
}
