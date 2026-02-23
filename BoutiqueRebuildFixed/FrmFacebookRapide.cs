using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FrmFacebookRapide : Form
    {
        private TextBox txtToken;
        private TextBox txtAdAccount;
        private ComboBox cmbVersion;
        private Button btnTester;
        private Button btnEnregistrer;
        private Button btnFermer;

        public FrmFacebookRapide()
        {
            Text = "Paramètres Facebook / Meta";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 560;
            Height = 280;

            BuildUI();
            LoadConfigToUi();
        }

        private void BuildUI()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            Controls.Add(pnl);

            int y = 10;

            pnl.Controls.Add(new Label
            {
                Text = "Configuration Facebook / Meta",
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Left = 10,
                Top = y
            });

            y += 40;

            pnl.Controls.Add(new Label { Text = "Meta Graph Version", AutoSize = true, Left = 10, Top = y + 6 });
            cmbVersion = new ComboBox
            {
                Left = 160,
                Top = y,
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbVersion.Items.AddRange(new object[] { "v19.0", "v20.0", "v21.0" });
            pnl.Controls.Add(cmbVersion);

            y += 38;

            pnl.Controls.Add(new Label { Text = "Access Token", AutoSize = true, Left = 10, Top = y + 6 });
            txtToken = new TextBox
            {
                Left = 160,
                Top = y,
                Width = 360,
                UseSystemPasswordChar = true
            };
            pnl.Controls.Add(txtToken);

            y += 38;

            pnl.Controls.Add(new Label { Text = "Ad Account ID (optionnel)", AutoSize = true, Left = 10, Top = y + 6 });
            txtAdAccount = new TextBox
            {
                Left = 160,
                Top = y,
                Width = 360
            };
            pnl.Controls.Add(txtAdAccount);

            y += 52;

            btnTester = new Button { Text = "Tester Token", Left = 10, Top = y, Width = 120, Height = 32 };
            btnEnregistrer = new Button { Text = "Enregistrer", Left = 140, Top = y, Width = 120, Height = 32 };
            btnFermer = new Button { Text = "Fermer", Left = 270, Top = y, Width = 120, Height = 32 };

            pnl.Controls.Add(btnTester);
            pnl.Controls.Add(btnEnregistrer);
            pnl.Controls.Add(btnFermer);

            btnTester.Click += async (s, e) => await TesterAsync();
            btnEnregistrer.Click += (s, e) => EnregistrerDansConfig();
            btnFermer.Click += (s, e) => Close();
        }

        private void LoadConfigToUi()
        {
            string version = ConfigurationManager.AppSettings["MetaGraphVersion"] ?? "v19.0";
            string token = ConfigurationManager.AppSettings["FacebookAccessToken"] ?? "";
            string ad = ConfigurationManager.AppSettings["FacebookAdAccountId"] ?? "";

            if (cmbVersion.Items.Contains(version)) cmbVersion.SelectedItem = version;
            else cmbVersion.SelectedIndex = 0;

            txtToken.Text = token;
            txtAdAccount.Text = ad;
        }

        private async Task TesterAsync()
        {
            try
            {
                string token = (txtToken.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(token) || token == "PASTE_YOUR_TOKEN_HERE")
                {
                    MessageBox.Show("Colle d'abord le Access Token.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var api = new FacebookApiClient(token, cmbVersion.Text);
                string raw = await api.GetMyAdAccountsRawAsync();
                MessageBox.Show("Token OK.\n\nRéponse Facebook:\n" + raw, "Test OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test échoué:\n" + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnregistrerDansConfig()
        {
            string version = (cmbVersion.Text ?? "v19.0").Trim();
            string token = (txtToken.Text ?? "").Trim();
            string ad = (txtAdAccount.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                MessageBox.Show("Le token est obligatoire.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ écrit App.config (exe.config)
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            SetAppSetting(config, "MetaGraphVersion", version);
            SetAppSetting(config, "FacebookAccessToken", token);
            SetAppSetting(config, "FacebookAdAccountId", ad);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            MessageBox.Show("Paramètres Facebook enregistrés.\nRedémarre l'application si nécessaire.", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetAppSetting(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] == null)
                config.AppSettings.Settings.Add(key, value ?? "");
            else
                config.AppSettings.Settings[key].Value = value ?? "";
        }
    }
}