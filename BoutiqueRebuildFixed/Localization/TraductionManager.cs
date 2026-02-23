using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed.Localization;
using Newtonsoft.Json;

namespace BoutiqueRebuildFixed.Localization
{
        public static class TraductionManager
        {
            private static Dictionary<string, string> _dico =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public static IReadOnlyDictionary<string, string> Current => _dico;

            public static void LoadFromCulture(string cultureCode)
            {
                // map simple : fr* => fr.json, sinon en.json
                string lang = "fr";
                if (!string.IsNullOrWhiteSpace(cultureCode) &&
                    cultureCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    lang = "en";

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "Lang", $"{lang}.json");

                if (!File.Exists(path))
                {
                    _dico = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    return;
                }

                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                           ?? new Dictionary<string, string>();

                _dico = new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
            }

            public static void ApplyToAllOpenForms()
            {
                foreach (Form f in Application.OpenForms)
                {
                    if (f == null || f.IsDisposed) continue;
                    ApplyToForm(f);
                }
            }

            public static void ApplyToForm(Form f)
            {
                if (f == null || f.IsDisposed) return;

                // 1) Titre du form (clé = f.Name)
                if (TryGet(f.Name, out string title))
                    f.Text = title;

                // 2) Controls
                ApplyToControls(f.Controls);

                // 3) MenuStrip / ToolStrip
                ApplyToToolStrips(f);

                // 4) ContextMenuStrip attachés (si déjà créés)
                ApplyContextMenusRecursive(f);
            }

            private static void ApplyToControls(Control.ControlCollection controls)
            {
                foreach (Control c in controls)
                {
                    if (!string.IsNullOrWhiteSpace(c.Name) && TryGet(c.Name, out string t))
                        c.Text = t;

                    if (c.HasChildren)
                        ApplyToControls(c.Controls);
                }
            }

            private static void ApplyToToolStrips(Form f)
            {
                foreach (Control c in f.Controls)
                {
                    if (c is MenuStrip ms)
                    {
                        foreach (ToolStripItem it in ms.Items)
                            ApplyToolStripItem(it);
                    }
                    else if (c is ToolStrip ts)
                    {
                        foreach (ToolStripItem it in ts.Items)
                            ApplyToolStripItem(it);
                    }
                }
            }

            // ✅ PUBLIC pour être réutilisé (ContextMenuStrip, etc.)
            public static void ApplyToContextMenu(ContextMenuStrip cms)
            {
                if (cms == null) return;
                foreach (ToolStripItem it in cms.Items)
                    ApplyToolStripItem(it);
            }

            private static void ApplyContextMenusRecursive(Control root)
            {
                if (root == null) return;

                if (root.ContextMenuStrip != null)
                    ApplyToContextMenu(root.ContextMenuStrip);

                foreach (Control child in root.Controls)
                    ApplyContextMenusRecursive(child);
            }

            private static void ApplyToolStripItem(ToolStripItem item)
            {
                if (item == null) return;

                if (!string.IsNullOrWhiteSpace(item.Name) && TryGet(item.Name, out string t))
                    item.Text = t;

                if (item is ToolStripDropDownItem dd && dd.DropDownItems != null)
                {
                    foreach (ToolStripItem sub in dd.DropDownItems)
                        ApplyToolStripItem(sub);
                }
            }

            private static bool TryGet(string key, out string value)
            {
                value = null;
                if (string.IsNullOrWhiteSpace(key)) return false;
                return _dico.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value);
            }
        }
    }
