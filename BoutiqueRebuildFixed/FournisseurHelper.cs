using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public static class FournisseurHelper
    {
        public static void ChargerFournisseursDansCombo(ComboBox cmb, bool seulementActifs = true)
        {
            if (cmb == null) return;

            cmb.Items.Clear();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                string sql = @"
SELECT ID_Fournisseur, Nom
FROM dbo.Fournisseur
WHERE (@actifs = 0 OR Actif = 1)
ORDER BY Nom;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@actifs", SqlDbType.Bit).Value = seulementActifs ? 1 : 0;

                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            cmb.Items.Add(new ComboboxItem(
                                rd["Nom"]?.ToString() ?? "",
                                Convert.ToInt32(rd["ID_Fournisseur"])
                            ));
                        }
                    }
                }
            }

            cmb.DropDownStyle = ComboBoxStyle.DropDown;
            cmb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmb.AutoCompleteSource = AutoCompleteSource.ListItems;

            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        public static bool TryGetSelectedFournisseurId(ComboBox cmb, out int idFournisseur)
        {
            idFournisseur = 0;
            if (cmb?.SelectedItem is ComboboxItem it && it.Value > 0)
            {
                idFournisseur = it.Value;
                return true;
            }
            return false;
        }
    }
}
