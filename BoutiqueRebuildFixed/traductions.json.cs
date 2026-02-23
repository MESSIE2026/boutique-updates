using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace BoutiqueRebuildFixed
{
    internal static class Traductions
    {
        private static Dictionary<string, string> _dico =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void Charger(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
            {
                _dico = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            string json = File.ReadAllText(cheminFichier);
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();

            _dico = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
        }

        public static void Appliquer(Form f)
        {
            if (f == null || f.IsDisposed) return;

            // ✅ Titre du form (clé = f.Name : "FormMain", "FormLogin"...)
            if (!string.IsNullOrWhiteSpace(f.Name) && _dico.TryGetValue(f.Name, out var title))
                f.Text = title;

            Appliquer((Control)f);
        }

        // Appliquer aux controls
        public static void Appliquer(Control parent)
        {
            if (parent == null) return;

            // ✅ Traduire par Name (clé JSON = Name)
            if (!string.IsNullOrWhiteSpace(parent.Name) && _dico.TryGetValue(parent.Name, out var t))
                parent.Text = t;

            // MenuStrip
            if (parent is MenuStrip menuStrip)
            {
                foreach (ToolStripItem item in menuStrip.Items)
                    Appliquer(item);
            }
            // ContextMenuStrip
            else if (parent is ContextMenuStrip contextMenu)
            {
                foreach (ToolStripItem item in contextMenu.Items)
                    Appliquer(item);
            }
            // DataGridView : traduire HeaderText par Name de colonne
            else if (parent is DataGridView dgv)
            {
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(col.Name) && _dico.TryGetValue(col.Name, out var h))
                        col.HeaderText = h;
                }
            }
            // ComboBox : ⚠️ si tes items sont des codes/keys, tu peux traduire
            else if (parent is ComboBox comboBox)
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is string key && _dico.TryGetValue(key, out var v))
                        comboBox.Items[i] = v;
                }
            }

            // récursif
            foreach (Control child in parent.Controls)
                Appliquer(child);
        }

        // ToolStripItem + sous-menus
        public static void Appliquer(ToolStripItem item)
        {
            if (item == null) return;

            if (!string.IsNullOrWhiteSpace(item.Name) && _dico.TryGetValue(item.Name, out var t))
                item.Text = t;

            if (item is ToolStripDropDownItem dd && dd.DropDownItems != null)
            {
                foreach (ToolStripItem sub in dd.DropDownItems)
                    Appliquer(sub);
            }
        }
    }
}