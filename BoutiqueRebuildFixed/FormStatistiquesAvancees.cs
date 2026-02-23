using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using iTextSharp.text;
using System.Globalization;
using iTextSharp.text.pdf;

namespace BoutiqueRebuildFixed
{
    public partial class FormStatistiquesAvancees : FormBase
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private int campagneIdSelectionnee = -1;
        private DateTime dateSelectionnee = DateTime.Today;
        public event Action<int, string> PubliciteSelectionnee;
        private string deviseCourante = "FC";
        private int _marketingWidthDesign;
        public FormStatistiquesAvancees()
        {
            InitializeComponent();
            

            // ✅ IMPORTANT : sauvegarder la largeur DESIGNER
            _marketingWidthDesign = panelMarketing.Width;
            if (_marketingWidthDesign < 200) _marketingWidthDesign = 320; // sécurité

            chkVentes.Checked = true;
            chkMarketing.Checked = true;

            panelMarketing.BackColor = Color.Purple;
            panelMarketing.Dock = DockStyle.Right;
            panelMarketing.AutoScroll = true;

            AppliquerStyleMarketingPanel();

            ConfigurerGraphiques();
            InitialiserGraphiques();

            InitialiserEvenements();

            // ✅ Assurer 1 seul handler
            chkMarketing.CheckedChanged -= chkMarketing_CheckedChanged;
            chkMarketing.CheckedChanged += chkMarketing_CheckedChanged;

            ConfigSysteme.AppliquerTraductions(this);
            ConfigSysteme.AppliquerTheme(this);

            ConfigSysteme.OnLangueChange -= AppliquerLangue;
            ConfigSysteme.OnLangueChange += AppliquerLangue;

            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnLangueChange += RafraichirLangue;

            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

            AppliquerLangue();

            // ✅ Appliquer état initial
            chkMarketing_CheckedChanged(chkMarketing, EventArgs.Empty);

            ChargerDonnees();
        }


        public FormStatistiquesAvancees(int campagneId) : this()
        {
            campagneIdSelectionnee = campagneId;
            dateSelectionnee = DateTime.Today;

            ActualiserAffichage();

            RafraichirLangue();
            RafraichirTheme();
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 🔥 OBLIGATOIRE : éviter fuite mémoire
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            base.OnFormClosed(e);
        }
        private void ActualiserAffichage()
        {
            if (chkMarketing.Checked && campagneIdSelectionnee > 0)
            {
                // 🎯 Mode campagne spécifique
                ChargerDonneesCampagne(campagneIdSelectionnee, dateSelectionnee);
            }
            else
            {
                // 🎯 Mode global (périodes, radios, checkboxes)
                ChargerDonnees();
            }
        }
        private void MettreAJourMontantVendu(decimal montant)
        {
            lblMontantVendus.Text = $"{montant:N2} {deviseCourante}";
        }
        private void ChargerPublicites(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = @"
SELECT 
    Id,
    CampagneId,
    DateVente,
    NomCampagne,
    Vues,
    Messages,
    Commentaires,
    Spectateurs,
    BudgetQuotidien,
    NombreVentes,
    MontantVendus,
    Devise,
    Statut
FROM StatistiquesPublicites
WHERE DateVente BETWEEN @DateDebut AND @DateFin
ORDER BY DateVente DESC";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                        da.SelectCommand.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvPublicites.DataSource = dt;
                        dgvPublicites.ClearSelection();

                        if (dgvPublicites.Columns.Contains("Id"))
                            dgvPublicites.Columns["Id"].Visible = false;
                        if (dgvPublicites.Columns.Contains("CampagneId"))
                            dgvPublicites.Columns["CampagneId"].Visible = false;

                        if (dt.Rows.Count == 0)
                        {
                            lblTotalVuesMarketing.Text = "0";
                            lblTotalMessagesMarketing.Text = "0";
                            txtCommentaires.Text = "";
                            lblTotalSpectateursMarketing.Text = "0";
                            lblTotalBudgetQuotidienMarketing.Text = "0.00";
                            lblNombreVentes.Text = "0";
                            deviseCourante = "FC";
                            MettreAJourMontantVendu(0m);
                            return;
                        }

                        // Afficher la 1ère ligne (meilleur UX)
                        DataRow first = dt.Rows[0];

                        int vues = SafeParseInt(first["Vues"]);
                        int messages = SafeParseInt(first["Messages"]);
                        int commentaires = SafeParseInt(first["Commentaires"]);
                        int spectateurs = SafeParseInt(first["Spectateurs"]);
                        decimal budget = SafeParseDecimal(first["BudgetQuotidien"]);
                        int nombreVentes = SafeParseInt(first["NombreVentes"]);
                        decimal montantVendus = SafeParseDecimal(first["MontantVendus"]);
                        deviseCourante = first["Devise"]?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(deviseCourante)) deviseCourante = "FC";

                        lblTotalVuesMarketing.Text = vues.ToString();
                        lblTotalMessagesMarketing.Text = messages.ToString();
                        txtCommentaires.Text = ""; // commentaires = suggestions (pas chiffre)
                        lblTotalSpectateursMarketing.Text = spectateurs.ToString();
                        lblTotalBudgetQuotidienMarketing.Text = budget.ToString("N2");
                        lblNombreVentes.Text = nombreVentes.ToString();
                        MettreAJourMontantVendu(montantVendus);

                        // ⚠️ IMPORTANT :
                        // ❌ Ne pas appeler MettreAJourCamembertMarketing ici
                        // car ça écrase le camembert global des catégories.
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement publicités : " + ex.Message);
            }
        }

        private void dgvPublicites_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!dgvPublicites.Enabled) return;

            try
            {
                dgvPublicites.ClearSelection();
                dgvPublicites.Rows[e.RowIndex].Selected = true;

                DataGridViewRow row = dgvPublicites.Rows[e.RowIndex];

                // ===== PUB (ligne sélectionnée) =====
                int vues = GetIntCellValue(row, "Vues");
                int messages = GetIntCellValue(row, "Messages");
                int spectateurs = GetIntCellValue(row, "Spectateurs");

                // txtCommentaires = texte libre (suggestions / problèmes)
                txtCommentaires.Text = GetStringCellValue(row, "Commentaires", "");

                decimal budget = GetDecimalCellValue(row, "BudgetQuotidien");
                int nombreVentesPub = GetIntCellValue(row, "NombreVentes");
                decimal montantVendusPub = GetDecimalCellValue(row, "MontantVendus");
                string devisePub = GetStringCellValue(row, "Devise", "FC");

                // ===== GLOBAL (période) =====
                // lblMontantTotalVente contient souvent "12 500,00 FC" -> on extrait le nombre
                decimal montantTotalVentePeriode = SafeParseDecimalFromLabel(lblMontantTotalVente?.Text);

                int presences = SafeParseInt(lblNombrePresence?.Text);
                int absences = SafeParseInt(lblNombreAbsence?.Text);
                int stockActuel = SafeParseInt(lblArticleProduitStock?.Text);

                // ===== Labels =====
                lblTotalVuesMarketing.Text = vues.ToString("N0");
                lblTotalMessagesMarketing.Text = messages.ToString("N0");
                lblTotalSpectateursMarketing.Text = spectateurs.ToString("N0");

                // BudgetQuotidien : tu peux garder "$" si c’est ta règle métier
                lblTotalBudgetQuotidienMarketing.Text = budget.ToString("N2") + " $";

                lblNombreVentes.Text = nombreVentesPub.ToString("N0");
                lblMontantVendus.Text = montantVendusPub.ToString("N2") + " " + devisePub;

                // Présences / Absences / Stock restent globaux (période)
                lblNombrePresence.Text = presences.ToString("N0");
                lblNombreAbsence.Text = absences.ToString("N0");
                lblArticleProduitStock.Text = stockActuel.ToString("N0");

                // ===== Graphiques (inclut maintenant VENTES) =====
                MettreAJourGraphiquesDepuisSelection(
                    montantTotalVentePeriode,  // ✅ VENTES (période)
                    vues, messages, spectateurs, budget,
                    nombreVentesPub, montantVendusPub,
                    presences, absences, stockActuel
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur lors de la sélection d'une publicité : " + ex.Message,
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private string GetStringCellValue(DataGridViewRow row, string columnName, string defaultValue = "")
        {
            if (row?.DataGridView?.Columns.Contains(columnName) == true)
            {
                object val = row.Cells[columnName]?.Value;
                if (val != null && val != DBNull.Value)
                    return val.ToString().Trim();
            }
            return defaultValue;
        }

        // Helpers pour lecture sécurisée
        private int GetIntCellValue(DataGridViewRow row, string columnName)
        {
            if (row?.DataGridView?.Columns.Contains(columnName) == true)
                return SafeParseInt(row.Cells[columnName]?.Value);

            return 0;
        }

        private decimal GetDecimalCellValue(DataGridViewRow row, string columnName)
        {
            if (row?.DataGridView?.Columns.Contains(columnName) == true)
                return SafeParseDecimal(row.Cells[columnName]?.Value);

            return 0m;
        }
        private int SafeParseInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;

            string s = value.ToString().Trim();

            // supprime espaces et séparateurs (ex: "1 234", "1,234")
            s = s.Replace(" ", "").Replace(",", "").Replace("\u00A0", "");

            return int.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out int r) ? r : 0;
        }

        private int SafeParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            string s = value.Trim()
                .Replace(" ", "")
                .Replace(",", "")
                .Replace("\u00A0", "");

            return int.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out int r) ? r : 0;
        }

        private decimal SafeParseDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;

            string s = value.ToString().Trim()
                .Replace("\u00A0", "")
                .Replace(" ", "");

            // Essai culture courante puis invariant (cas "12,50" / "12.50")
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out decimal r))
                return r;

            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out r))
                return r;

            return 0m;
        }
        private decimal SafeParseDecimalFromLabel(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;

            // Ex: "12 500,00 FC" => "12500,00"
            string s = text.Trim();

            // Retirer lettres et symboles monnaie
            s = new string(s.Where(c => char.IsDigit(c) || c == ',' || c == '.' || c == '-' || c == ' ').ToArray());

            // Retirer espaces
            s = s.Replace("\u00A0", "").Replace(" ", "");

            // Essai culture courante (souvent virgule), puis invariant
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out decimal r))
                return r;

            if (decimal.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out r))
                return r;

            return 0m;
        }
        private void MettreAJourGraphiquesDepuisSelection(
    decimal montantTotalVentesPeriode,   // ✅ NOUVEAU : VENTES (période)
    int vues, int messages, int spectateurs, decimal budget,
    int nombreVentes, decimal montantVendus,
    int presences, int absences, int stockActuel)
        {
            // ===== Graph Performance (barres) =====
            chartPerformanceActivites.Series.Clear();
            chartPerformanceActivites.ChartAreas.Clear();

            var area = new ChartArea("AreaLine");
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartPerformanceActivites.ChartAreas.Add(area);

            Series s = new Series("Valeurs")
            {
                ChartType = SeriesChartType.Column,
                BorderWidth = 2,
                IsValueShownAsLabel = true
            };

            // ✅ Ajout VENTES
            s.Points.AddXY("Ventes (Période)", (double)montantTotalVentesPeriode);

            // Pub sélectionnée
            s.Points.AddXY("Vues", vues);
            s.Points.AddXY("Messages", messages);
            s.Points.AddXY("Spectateurs", spectateurs);
            s.Points.AddXY("Budget ($)", (double)budget);
            s.Points.AddXY("Nombre Ventes (Pub)", nombreVentes);
            s.Points.AddXY("Montant Vendu (Pub)", (double)montantVendus);

            // Global période
            s.Points.AddXY("Présences", presences);
            s.Points.AddXY("Absences", absences);
            s.Points.AddXY("Stock", stockActuel);

            chartPerformanceActivites.Series.Add(s);

            // ===== Pie (catégories) =====
            chartRepartitionCategories.Series.Clear();
            chartRepartitionCategories.ChartAreas.Clear();

            var areaPie = new ChartArea("AreaPie");
            areaPie.AxisX.Enabled = AxisEnabled.False;
            areaPie.AxisY.Enabled = AxisEnabled.False;
            areaPie.BackColor = Color.Transparent;
            chartRepartitionCategories.ChartAreas.Add(areaPie);

            Series pie = new Series("Repartition")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Black,
                ChartArea = "AreaPie"
            };
            chartRepartitionCategories.Series.Add(pie);

            double total = (double)montantTotalVentesPeriode
                + vues + messages + spectateurs
                + (double)budget
                + nombreVentes + (double)montantVendus
                + presences + absences + stockActuel;

            if (total <= 0) total = 1;

            void AddPie(string label, double val)
            {
                if (val <= 0) return;
                int idx = pie.Points.AddXY(label, val);
                pie.Points[idx].Label = $"{(val * 100 / total):F1}%";
                pie.Points[idx].LegendText = label;
            }

            // ✅ Ajout VENTES
            AddPie("Ventes (Période)", (double)montantTotalVentesPeriode);

            AddPie("Vues", vues);
            AddPie("Messages", messages);
            AddPie("Spectateurs", spectateurs);
            AddPie("Budget", (double)budget);
            AddPie("Nb Ventes (Pub)", nombreVentes);
            AddPie("Montant (Pub)", (double)montantVendus);
            AddPie("Présences", presences);
            AddPie("Absences", absences);
            AddPie("Stock", stockActuel);

            // Anti-superposition (Smart labels)
            pie["PieLabelStyle"] = "Outside";
            pie["PieLineColor"] = "Black";
            pie["PieDrawingStyle"] = "Concave";

            pie.SmartLabelStyle.Enabled = true;
            pie.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            pie.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
            pie.SmartLabelStyle.MinMovingDistance = 25;
            pie.SmartLabelStyle.MaxMovingDistance = 80;
            pie.SmartLabelStyle.CalloutLineColor = Color.Black;
            pie.SmartLabelStyle.CalloutLineWidth = 1;
        }

        // Méthode pour espacer les labels
        private void EspacerLabelsPie(Series seriePie)
        {
            double startAngle = 0;
            double total = 0;

            foreach (var pt in seriePie.Points)
                total += pt.YValues[0];

            for (int i = 0; i < seriePie.Points.Count; i++)
            {
                var pt = seriePie.Points[i];
                double value = pt.YValues[0];
                double sweepAngle = value / total * 360;

                double labelAngle = startAngle + sweepAngle / 2;

                pt["LabelAngle"] = labelAngle.ToString("F1");

                startAngle += sweepAngle;
            }
        }

        private void MettreAJourCamembertMarketing(
    decimal vues,
    decimal messages,
    decimal commentaires,
    decimal spectateurs,
    decimal budget)
        {
            if (!chartRepartitionCategories.ChartAreas.Any(a => a.Name == "AreaPie"))
            {
                chartRepartitionCategories.ChartAreas.Clear();
                chartRepartitionCategories.ChartAreas.Add(new ChartArea("AreaPie"));
            }

            chartRepartitionCategories.Series.Clear();

            Series serie = new Series("Repartition")
            {
                ChartType = SeriesChartType.Pie,
                ChartArea = "AreaPie",
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Black
            };

            chartRepartitionCategories.Series.Add(serie);

            decimal total = vues + messages + commentaires + spectateurs + budget;
            if (total <= 0) total = 1;

            void Add(string label, decimal val)
            {
                if (val <= 0) return;
                int i = serie.Points.AddXY(label, val);
                var point = serie.Points[i];
                point.Label = $"{(val * 100 / total):F1}%";
                point.LegendText = label;
            }

            Add("Vues", vues);
            Add("Messages", messages);
            Add("Commentaires", commentaires);
            Add("Spectateurs", spectateurs);
            Add("Budget", budget);

            // Activation et configuration SmartLabelStyle pour éviter chevauchements
            serie.SmartLabelStyle.Enabled = true;
            serie.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            serie.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
            serie.SmartLabelStyle.MinMovingDistance = 15; // plus grand = plus espacés
            serie.SmartLabelStyle.MaxMovingDistance = 50;
            serie.SmartLabelStyle.CalloutLineColor = Color.Black;
            serie.SmartLabelStyle.CalloutLineAnchorCapStyle = System.Windows.Forms.DataVisualization.Charting.LineAnchorCapStyle.Diamond;
            serie.SmartLabelStyle.CalloutLineWidth = 1;

            serie["PieLabelStyle"] = "Outside";
            serie["PieLineColor"] = "Black";
            serie["PieDrawingStyle"] = "Concave";
        }
        private void CalculerPourcentages(int vues, int messages, int spectateurs)
        {
            if (vues <= 0) vues = 1; // éviter division par zéro

            decimal tauxMessages = (decimal)messages / vues * 100;
            decimal tauxSpectateurs = (decimal)spectateurs / vues * 100;

            // Tu n'as pas les labels pour afficher ces taux, donc tu peux juste les utiliser dans ta logique si besoin
            // sinon ignore cette méthode ou ajoute des labels pour afficher tauxMessages et tauxSpectateurs
        }

        // Optionnel, si tu souhaites recalculer les ventes et montants depuis la base
        private void ChargerVentesPub(int pubId)
        {
            int nombreVentes = 0;
            decimal montantVendu = 0m;
            string devise = "FC";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                var cmd = new SqlCommand(@"
            SELECT 
                ISNULL(NombreVentes, 0) AS NombreVentes,
                ISNULL(MontantVendus, 0) AS MontantVendus,
                ISNULL(Devise, 'FC') AS Devise
            FROM StatistiquesPublicites
            WHERE Id = @PubId
        ", con);

                cmd.Parameters.AddWithValue("@PubId", pubId);

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        nombreVentes = dr.IsDBNull(0) ? 0 : dr.GetInt32(0);
                        montantVendu = dr.IsDBNull(1) ? 0m : dr.GetDecimal(1);
                        devise = dr.IsDBNull(2) ? "FC" : dr.GetString(2);
                    }
                }
            }

            lblNombreVentes.Text = nombreVentes.ToString();
            lblMontantVendus.Text = montantVendu.ToString("N2") + " " + devise;
        }

        private void ConfigurerDataGridViewPublicites()
        {
            dgvPublicites.ReadOnly = true;
            dgvPublicites.AllowUserToAddRows = false;
            dgvPublicites.AllowUserToDeleteRows = false;
            dgvPublicites.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPublicites.MultiSelect = false;
            dgvPublicites.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPublicites.RowHeadersVisible = false;

            dgvPublicites.CellClick -= dgvPublicites_CellClick;
            dgvPublicites.CellClick += dgvPublicites_CellClick;
        }
        
        private void CalculerTousLesPourcentages(
    int vues,
    int messages,
    int commentaires,
    int spectateurs,
    decimal budget)
        {
            if (vues <= 0) vues = 1;
            if (budget <= 0) budget = 1;

            // 🔹 Taux principaux
            decimal tauxVues = 100m;
            decimal tauxMessages = (decimal)messages / vues * 100;
            decimal tauxSpectateurs = (decimal)spectateurs / vues * 100;
            decimal tauxCommentaires = (decimal)commentaires / vues * 100;

            // 🔹 Budget pondéré (coût pour 100 vues)
            decimal tauxBudget = budget / vues * 100;

            // 🔹 Ventes (si non existantes → 0)
            decimal tauxVentes = tauxMessages * 0.4m;   // hypothèse métier
            decimal tauxMontant = tauxVentes * 1.2m;    // pondération métier

            // 🔹 Affectation aux labels
            lblTotalVuesMarketing.Text = tauxVues.ToString("F1") + " %";
            lblTotalMessagesMarketing.Text = tauxMessages.ToString("F1") + " %";
            lblTotalSpectateursMarketing.Text = tauxSpectateurs.ToString("F1") + " %";
            lblTotalBudgetQuotidienMarketing.Text = tauxBudget.ToString("F2");

            lblNombreVentes.Text = tauxVentes.ToString("F1") + " %";
            lblMontantVendus.Text = tauxMontant.ToString("F1") + " %";
        }
        private void InitialiserCommentaires()
        {
            txtCommentaires.Clear();
            txtCommentaires.ReadOnly = false;
        }

        private void ChargerGraphiqueRepartitionMarketing(DateTime dateDebut, DateTime dateFin)
        {
            Series serie = chartRepartitionCategories.Series["Repartition"];
            serie.Points.Clear();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT NomCampagne, SUM(ISNULL(Vues,0)) AS TotalVues
FROM StatistiquesPublicites
WHERE DateVente BETWEEN @DateDebut AND @DateFin
GROUP BY NomCampagne";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@DateDebut", SqlDbType.Date).Value = dateDebut;
                    cmd.Parameters.Add("@DateFin", SqlDbType.Date).Value = dateFin;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            serie.Points.AddXY(
                                dr["NomCampagne"].ToString(),
                                Convert.ToInt32(dr["TotalVues"])
                            );
                        }
                    }
                }
            }
        }
        private void chkMarketing_CheckedChanged(object sender, EventArgs e)
        {
            bool on = chkMarketing.Checked;

            // ✅ Panel + largeur
            panelMarketing.Visible = on;
            if (on)
                panelMarketing.Width = _marketingWidthDesign;

            // ✅ Contenus marketing
            dgvPublicites.Enabled = on;

            if (!on)
            {
                // reset marketing uniquement
                lblTotalVuesMarketing.Text = "0";
                lblTotalMessagesMarketing.Text = "0";
                lblTotalSpectateursMarketing.Text = "0";
                lblTotalBudgetQuotidienMarketing.Text = "0.00";
                lblNombreVentes.Text = "0";
                lblMontantVendus.Text = "0.00 FC";
                txtCommentaires.Clear();
                dgvPublicites.DataSource = null;
            }
            else
            {
                // ✅ quand ON : recharge la période -> remplit dgv + labels marketing
                DeterminerPeriode(out DateTime d1, out DateTime d2);
                ChargerPublicites(d1, d2);  // ✅ remplit panel marketing
                AppliquerStyleMarketingPanel();
            }

            // ✅ Met à jour le reste (graph + rapport) avec la liste catégories
            ChargerDonnees();
        }

        private void AppliquerStyleMarketingPanel()
{
    panelMarketing.BackColor = Color.Purple;

    foreach (Control c in panelMarketing.Controls)
    {
        if (c is Label lbl)
            lbl.ForeColor = Color.White;

        if (c is TextBox tb)
        {
            tb.ForeColor = Color.Black;
            tb.BackColor = Color.White;
        }
    }
}


        private void ChargerDonneesCampagne(int campagneId, DateTime date)
        {
            if (campagneId <= 0) return;

            // Met à jour le statut des publicités de la campagne si besoin (à définir dans ta méthode)
            MettreAJourStatutPublicites(campagneId);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
            SELECT 
                Id,
                DateVente,
                NomCampagne,
                Vues,
                Messages,
                Spectateurs,
                BudgetQuotidien,
                Commentaires,
                Statut,
                NombreVentes,
                MontantVendus
            FROM StatistiquesPublicites
            WHERE CampagneId = @CampagneId
            ORDER BY DateVente DESC";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@CampagneId", SqlDbType.Int).Value = campagneId;

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    dgvPublicites.AutoGenerateColumns = true;
                    dgvPublicites.DataSource = dt;

                    // Masquer colonnes techniques si besoin
                    if (dgvPublicites.Columns.Contains("Id"))
                        dgvPublicites.Columns["Id"].Visible = false;
                    if (dgvPublicites.Columns.Contains("CampagneId"))
                        dgvPublicites.Columns["CampagneId"].Visible = false;

                    // Optionnel : Ajuster largeur colonnes, formatage etc.
                    dgvPublicites.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvPublicites.ClearSelection();
                }
            }
        }

        private void ChangerStatutPublicite(int pubId, string nouveauStatut)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string sql = "UPDATE StatistiquesPublicites SET Statut = @Statut WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Statut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@Id", pubId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void grpGraphiques_Enter(object sender, EventArgs e)
        {
            // Tu peux laisser vide si tu ne veux rien faire lors de l'entrée dans ce groupe
        }
        private void InitialiserGraphiques()
        {
            // ===== PERFORMANCE =====
            chartPerformanceActivites.Series.Clear();
            chartPerformanceActivites.ChartAreas.Clear();

            var areaLine = new ChartArea("AreaLine");
            areaLine.AxisX.MajorGrid.LineColor = Color.LightGray;
            areaLine.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartPerformanceActivites.ChartAreas.Add(areaLine);

            // ===== CATEGORIES (PIE GLOBAL) =====
            chartRepartitionCategories.Series.Clear();
            chartRepartitionCategories.ChartAreas.Clear();

            var areaPie = new ChartArea("AreaPie");
            areaPie.AxisX.Enabled = AxisEnabled.False;
            areaPie.AxisY.Enabled = AxisEnabled.False;
            areaPie.BackColor = Color.Transparent;
            chartRepartitionCategories.ChartAreas.Add(areaPie);

            var seriePie = new Series("Répartition")
            {
                ChartType = SeriesChartType.Pie,
                ChartArea = "AreaPie",
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Black
            };

            chartRepartitionCategories.Series.Add(seriePie);

            // Style global pie
            seriePie["PieLabelStyle"] = "Outside";
            seriePie["PieLineColor"] = "Black";
            seriePie["PieDrawingStyle"] = "Concave";

            seriePie.SmartLabelStyle.Enabled = true;
            seriePie.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            seriePie.SmartLabelStyle.MovingDirection =
                LabelAlignmentStyles.Left | LabelAlignmentStyles.Right | LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom;
            seriePie.SmartLabelStyle.MinMovingDistance = 20;
            seriePie.SmartLabelStyle.MaxMovingDistance = 80;
            seriePie.SmartLabelStyle.CalloutLineColor = Color.Black;
            seriePie.SmartLabelStyle.CalloutLineAnchorCapStyle = LineAnchorCapStyle.Diamond;
            seriePie.SmartLabelStyle.CalloutLineWidth = 1;
        }

        private void dgvPublicites_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvPublicites.Rows[e.RowIndex];
            string statut = row.Cells["Statut"].Value?.ToString();

            if (!string.Equals(statut, "Terminée", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "La publicité doit être terminée.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            int pubId = Convert.ToInt32(row.Cells["Id"].Value);
            string nomPub = row.Cells["NomCampagne"].Value.ToString();

            PubliciteSelectionnee?.Invoke(pubId, nomPub);
        }
        private void ChargerCommentairesCampagne(int campagneId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
            SELECT Commentaires
            FROM StatistiquesPublicites
            WHERE CampagneId = @CampagneId
              AND Commentaires IS NOT NULL
              AND Commentaires <> ''
            ORDER BY DateVente DESC";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@CampagneId", campagneId);

                    StringBuilder sb = new StringBuilder();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            sb.AppendLine("• " + dr["Commentaires"].ToString());
                            sb.AppendLine();
                        }
                    }

                    txtCommentaires.Text = sb.ToString();
                }
            }
        }
        
       

        private void MettreAJourStatutPublicites(int campagneId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
UPDATE StatistiquesPublicites
SET Statut = 'Terminée'
WHERE CampagneId = @CampagneId
  AND Statut = 'En cours'
  AND DateVente < CAST(GETDATE() AS DATE)";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@CampagneId", SqlDbType.Int).Value = campagneId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        
        

        private void AppliquerLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void InitialiserEvenements()
        {
            // === MÉTHODE CENTRALE POUR ÉVITER LA DUPLICATION ===
            void RechargerToutesDonnees()
            {
                // Rapport principal
                ChargerDonnees();
            }

            // === RADIO BUTTONS (PÉRIODES) ===
            rbJour.CheckedChanged += (s, e) =>
            {
                if (rbJour.Checked)
                    RechargerToutesDonnees();
            };

            rbSemaine.CheckedChanged += (s, e) =>
            {
                if (rbSemaine.Checked)
                    RechargerToutesDonnees();
            };

            rbMois.CheckedChanged += (s, e) =>
            {
                if (rbMois.Checked)
                    RechargerToutesDonnees();
            };

            rbAnnee.CheckedChanged += (s, e) =>
            {
                if (rbAnnee.Checked)
                    RechargerToutesDonnees();
            };

            rbPersonnalise.CheckedChanged += (s, e) =>
            {
                if (rbPersonnalise.Checked)
                    RechargerToutesDonnees();
            };

            // === CHECKBOX FILTRES ===
            chkVentes.CheckedChanged += (s, e) => RechargerToutesDonnees();
            chkProduits.CheckedChanged += (s, e) => RechargerToutesDonnees();
            chkEmployes.CheckedChanged += (s, e) => RechargerToutesDonnees();
            chkAbsences.CheckedChanged += (s, e) => RechargerToutesDonnees();
            chkMarketing.CheckedChanged -= chkMarketing_CheckedChanged;
            chkMarketing.CheckedChanged += chkMarketing_CheckedChanged;

            // === CHECKBOX MARKETING ===
            
            // === DATES PERSONNALISÉES ===
            dtpDateDebut.ValueChanged += (s, e) =>
            {
                if (rbPersonnalise.Checked)
                    RechargerToutesDonnees();
            };

            dtpDateFin.ValueChanged += (s, e) =>
            {
                if (rbPersonnalise.Checked)
                    RechargerToutesDonnees();
            };

            // === CLICK SUR UNE PUBLICITÉ ===
            dgvPublicites.CellClick += dgvPublicites_CellClick;
        }

        private void DeterminerPeriode(out DateTime debut, out DateTime fin)
        {
            DateTime today = DateTime.Today;

            if (rbJour.Checked)
            {
                debut = today;
                fin = today.AddDays(1).AddSeconds(-1);
            }
            else if (rbSemaine.Checked)
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                debut = today.AddDays(-diff);
                fin = debut.AddDays(6).AddSeconds(-1);
            }
            else if (rbMois.Checked)
            {
                debut = new DateTime(today.Year, today.Month, 1);
                fin = debut.AddMonths(1).AddSeconds(-1);
            }
            else if (rbAnnee.Checked)
            {
                debut = new DateTime(today.Year, 1, 1);
                fin = new DateTime(today.Year, 12, 31, 23, 59, 59);
            }
            else
            {
                debut = dtpDateDebut.Value.Date;
                fin = dtpDateFin.Value.Date.AddDays(1).AddSeconds(-1);
            }
        }


        private void dgvPublicites_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPublicites.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvPublicites.SelectedRows[0];

            int vues = row.Cells["Vues"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Vues"].Value) : 0;
            int messages = row.Cells["Messages"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Messages"].Value) : 0;
            int commentaires = row.Cells["Commentaires"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Commentaires"].Value) : 0;
            int spectateurs = row.Cells["Spectateurs"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["Spectateurs"].Value) : 0;
            decimal budget = row.Cells["BudgetQuotidien"].Value != DBNull.Value ? Convert.ToDecimal(row.Cells["BudgetQuotidien"].Value) : 0m;

            // Nouvelles colonnes pour ventes
            int nombreVentes = 0;
            decimal montantVendus = 0m;

            if (dgvPublicites.Columns.Contains("NombreVentes") && row.Cells["NombreVentes"].Value != DBNull.Value)
                nombreVentes = Convert.ToInt32(row.Cells["NombreVentes"].Value);

            if (dgvPublicites.Columns.Contains("MontantVendus") && row.Cells["MontantVendus"].Value != DBNull.Value)
                montantVendus = Convert.ToDecimal(row.Cells["MontantVendus"].Value);

            AfficherStatsMarketing(vues, messages, commentaires, spectateurs, budget, nombreVentes, montantVendus);

            MettreAJourGraphiqueMarketing(vues, messages, spectateurs, budget, nombreVentes, montantVendus);
            MettreAJourCamembertMarketing(vues, messages, commentaires, spectateurs, budget);
        }

        private void AfficherStatsMarketing(
            int vues,
            int messages,
            int commentaires,
            int spectateurs,
            decimal budget,
            int nombreVentes,
            decimal montantVendus)
        {
            lblTotalVuesMarketing.Text = vues.ToString();
            lblTotalMessagesMarketing.Text = messages.ToString();
            txtCommentaires.Text = commentaires.ToString();
            lblTotalSpectateursMarketing.Text = spectateurs.ToString();
            lblTotalBudgetQuotidienMarketing.Text = budget.ToString("N2") + " $";

            lblNombreVentes.Text = nombreVentes.ToString();
            lblMontantVendus.Text = montantVendus.ToString("N2") + " FC";
        }

        // =====================================================
        // GRAPHIQUE PRINCIPAL (UNIQUE)
        // =====================================================
        private void MettreAJourGraphiqueMarketing(
    int vues,
    int messages,
    int spectateurs,
    decimal budget,
    int nombreVentes,
    decimal montantVendus)
        {
            chartPerformanceActivites.Series.Clear();

            Series serie = new Series("Performance Marketing")
            {
                ChartType = SeriesChartType.Column,
                BorderWidth = 2
            };

            serie.Points.AddXY("Vues", vues);
            serie.Points.AddXY("Messages", messages);
            serie.Points.AddXY("Spectateurs", spectateurs);
            serie.Points.AddXY("Budget", (double)budget);
            serie.Points.AddXY("Nombre Ventes", nombreVentes);
            serie.Points.AddXY("Montant Vendu", (double)montantVendus);

            chartPerformanceActivites.Series.Add(serie);
        }

        private void ConfigurerGraphiques()
        {
            // ================= GRAPH LINE =================
            chartPerformanceActivites.Series.Clear();
            chartPerformanceActivites.ChartAreas.Clear();

            ChartArea areaLine = new ChartArea("AreaLine");
            areaLine.AxisX.LabelStyle.Format = "MMM yyyy";
            areaLine.AxisX.IntervalType = DateTimeIntervalType.Months;
            areaLine.AxisX.Interval = 1;
            areaLine.AxisX.MajorGrid.LineColor = Color.LightGray;
            areaLine.AxisY.MajorGrid.LineColor = Color.LightGray;

            chartPerformanceActivites.ChartAreas.Add(areaLine);

            // ================= PIE =================
            chartRepartitionCategories.Series.Clear();
            chartRepartitionCategories.ChartAreas.Clear();

            ChartArea areaPie = new ChartArea("AreaPie");
            areaPie.AxisX.Enabled = AxisEnabled.False;
            areaPie.AxisY.Enabled = AxisEnabled.False;
            areaPie.BackColor = Color.Transparent;

            chartRepartitionCategories.ChartAreas.Add(areaPie);

            Series seriePie = new Series("Repartition")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                ChartArea = "AreaPie",
                LabelForeColor = Color.Black
            };

            chartRepartitionCategories.Series.Add(seriePie);
        }



        private void ChargerDonnees()
        {
            // ===== 1) période =====
            DateTime dateDebut, dateFin;
            DateTime today = DateTime.Today;

            if (rbJour.Checked)
            {
                dateDebut = today;
                dateFin = today.AddDays(1).AddSeconds(-1);
            }
            else if (rbSemaine.Checked)
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                dateDebut = today.AddDays(-diff);
                dateFin = dateDebut.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            else if (rbMois.Checked)
            {
                dateDebut = new DateTime(today.Year, today.Month, 1);
                dateFin = dateDebut.AddMonths(1).AddSeconds(-1);
            }
            else if (rbAnnee.Checked)
            {
                dateDebut = new DateTime(today.Year, 1, 1);
                dateFin = new DateTime(today.Year, 12, 31, 23, 59, 59);
            }
            else
            {
                dateDebut = dtpDateDebut.Value.Date;
                dateFin = dtpDateFin.Value.Date.AddDays(1).AddSeconds(-1);
            }

            // ===== 2) catégories =====
            var categoriesFiltrees = new List<string>();
            if (chkVentes.Checked) categoriesFiltrees.Add("Ventes");
            if (chkProduits.Checked) categoriesFiltrees.Add("Produits");
            if (chkEmployes.Checked) categoriesFiltrees.Add("Employés");
            if (chkAbsences.Checked) categoriesFiltrees.Add("Absences");
            if (chkMarketing.Checked) categoriesFiltrees.Add("Marketing");

            if (categoriesFiltrees.Count == 0)
            {
                chartPerformanceActivites.Series.Clear();
                chartRepartitionCategories.Series.Clear();
                dgvRapport.DataSource = null;
                dgvPublicites.DataSource = null;

                lblMontantTotalVente.Text = "0";
                lblArticleProduitStock.Text = "0";
                lblNombrePresence.Text = "0";
                lblNombreAbsence.Text = "0";

                lblTotalVuesMarketing.Text = "0";
                lblTotalMessagesMarketing.Text = "0";
                lblTotalSpectateursMarketing.Text = "0";
                lblTotalBudgetQuotidienMarketing.Text = "0";
                lblNombreVentes.Text = "0";
                lblMontantVendus.Text = "0 FC";
                return;
            }

            // ===== 3) données & rapport =====
            ChargerStatistiques(dateDebut, dateFin, categoriesFiltrees);
            ChargerDataGridView(dateDebut, dateFin, categoriesFiltrees);

            // ===== 4) LES DEUX charts: Performance + Camembert global =====
            MettreAJourGraphiques(dateDebut, dateFin, categoriesFiltrees);

            // ===== 5) dgvPublicites filtré par la même période =====
            if (chkMarketing.Checked)
                ChargerPublicites(dateDebut, dateFin);
            else
                dgvPublicites.DataSource = null;
        }

        private void MettreAJourGraphiques(DateTime dateDebut, DateTime dateFin, List<string> categories)
        {
            // =========================
            // 1) RESET CHARTS
            // =========================
            chartPerformanceActivites.Series.Clear();

            chartRepartitionCategories.Series.Clear();
            chartRepartitionCategories.ChartAreas.Clear();

            // ChartArea PIE (doit exister AVANT la série)
            var areaPie = new ChartArea("AreaPie");
            areaPie.AxisX.Enabled = AxisEnabled.False;
            areaPie.AxisY.Enabled = AxisEnabled.False;
            areaPie.BackColor = Color.Transparent;
            chartRepartitionCategories.ChartAreas.Add(areaPie);

            // Série Pie (camembert global)
            var serieCamembert = new Series("Répartition")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelForeColor = Color.Black,
                ChartArea = "AreaPie"
            };
            chartRepartitionCategories.Series.Add(serieCamembert);

            // =========================
            // 2) COULEURS FIXES
            // =========================
            var couleurs = new Dictionary<string, Color>
            {
                ["Ventes CDF"] = Color.DarkBlue,
                ["Ventes USD"] = Color.DarkGreen,

                ["Produits"] = Color.DarkOrange,
                ["Présences"] = Color.DarkSeaGreen,
                ["Absences"] = Color.Maroon,

                ["Vues Marketing"] = Color.Teal,
                ["Messages Marketing"] = Color.Cyan,
                ["Spectateurs Marketing"] = Color.Purple,
                ["Budget Marketing"] = Color.DarkSlateGray,
                ["Nombre Ventes"] = Color.DarkGreen,
                ["Montant Vendu"] = Color.Green
            };

            // =========================
            // 3) TOTAUX POUR CAMEMBERT
            // =========================
            decimal totalVentesCDF = 0m;
            decimal totalVentesUSD = 0m;

            decimal totalProduits = 0m;
            int totalPresences = 0;
            int totalAbsences = 0;

            int totalVues = 0;
            int totalMessages = 0;
            int totalSpectateurs = 0;
            decimal totalBudget = 0m;
            int totalNombreVentes = 0;
            decimal totalMontantVendus = 0m;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // =========================
                // VENTES (COURBES CDF + USD)
                // =========================
                if (categories.Contains("Ventes"))
                {
                    var serieVentesCDF = new Series("Ventes CDF")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 3,
                        Color = couleurs["Ventes CDF"],
                        MarkerStyle = MarkerStyle.Circle
                    };

                    var serieVentesUSD = new Series("Ventes USD")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 3,
                        Color = couleurs["Ventes USD"],
                        MarkerStyle = MarkerStyle.Circle
                    };

                    chartPerformanceActivites.Series.Add(serieVentesCDF);
                    chartPerformanceActivites.Series.Add(serieVentesUSD);

                    using (var cmd = new SqlCommand(@"
SELECT 
    CAST(p.DatePaiement AS date) AS Jour,
    UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'CDF')))) AS Devise,
    SUM(ISNULL(p.Montant,0)) AS Total
FROM PaiementsVente p
WHERE p.DatePaiement BETWEEN @DateDebut AND @DateFin
  AND ISNULL(p.Statut,'') <> 'ANNULÉ'
  AND p.DateAnnulation IS NULL
GROUP BY CAST(p.DatePaiement AS date), UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'CDF'))))
ORDER BY Jour;", con))
                    {
                        cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                        cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DateTime jour = dr.GetDateTime(0);
                                string dev = dr.IsDBNull(1) ? "CDF" : dr.GetString(1);
                                decimal total = dr.IsDBNull(2) ? 0m : dr.GetDecimal(2);

                                if (dev == "USD")
                                {
                                    serieVentesUSD.Points.AddXY(jour, total);
                                    totalVentesUSD += total;
                                }
                                else
                                {
                                    serieVentesCDF.Points.AddXY(jour, total);
                                    totalVentesCDF += total;
                                }
                            }
                        }
                    }

                    // Optionnel: masquer séries vides
                    if (serieVentesCDF.Points.Count == 0) serieVentesCDF.Enabled = false;
                    if (serieVentesUSD.Points.Count == 0) serieVentesUSD.Enabled = false;
                }

                // =========================
                // PRODUITS (COURBE simple)
                // =========================
                if (categories.Contains("Produits"))
                {
                    var serie = new Series("Produits")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 3,
                        Color = couleurs["Produits"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    chartPerformanceActivites.Series.Add(serie);

                    using (var cmd = new SqlCommand("SELECT ISNULL(SUM(StockActuel),0) FROM Produit", con))
                    {
                        decimal stock = Convert.ToDecimal(cmd.ExecuteScalar());
                        serie.Points.AddXY(DateTime.Today, stock);
                        totalProduits = stock;
                    }
                }

                // =========================
                // PRESENCES (COURBE)
                // =========================
                if (categories.Contains("Employés"))
                {
                    var serie = new Series("Présences")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 3,
                        Color = couleurs["Présences"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    chartPerformanceActivites.Series.Add(serie);

                    using (var cmd = new SqlCommand(@"
                SELECT JourDate, COUNT(*) AS NbPresences
                FROM PresenceAbsenceAgents
                WHERE Present = 1 AND JourDate BETWEEN @DateDebut AND @DateFin
                GROUP BY JourDate
                ORDER BY JourDate", con))
                    {
                        cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                        cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DateTime date = dr.GetDateTime(0);
                                int nb = dr.IsDBNull(1) ? 0 : dr.GetInt32(1);
                                serie.Points.AddXY(date, nb);
                                totalPresences += nb;
                            }
                        }
                    }
                }

                // =========================
                // ABSENCES (COURBE)
                // =========================
                if (categories.Contains("Absences"))
                {
                    var serie = new Series("Absences")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 3,
                        Color = couleurs["Absences"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    chartPerformanceActivites.Series.Add(serie);

                    using (var cmd = new SqlCommand(@"
                SELECT JourDate, COUNT(*) AS NbAbsences
                FROM PresenceAbsenceAgents
                WHERE Absent = 1 AND JourDate BETWEEN @DateDebut AND @DateFin
                GROUP BY JourDate
                ORDER BY JourDate", con))
                    {
                        cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                        cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DateTime date = dr.GetDateTime(0);
                                int nb = dr.IsDBNull(1) ? 0 : dr.GetInt32(1);
                                serie.Points.AddXY(date, nb);
                                totalAbsences += nb;
                            }
                        }
                    }
                }

                // =========================
                // MARKETING (COURBES)
                // =========================
                if (categories.Contains("Marketing"))
                {
                    var serieVues = new Series("Vues Marketing")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Vues Marketing"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    var serieMessages = new Series("Messages Marketing")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Messages Marketing"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    var serieSpectateurs = new Series("Spectateurs Marketing")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Spectateurs Marketing"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    var serieBudget = new Series("Budget Marketing")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Budget Marketing"],
                        MarkerStyle = MarkerStyle.Circle,
                        YAxisType = AxisType.Secondary
                    };
                    var serieNombreVentes = new Series("Nombre Ventes")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Nombre Ventes"],
                        MarkerStyle = MarkerStyle.Circle
                    };
                    var serieMontantVendus = new Series("Montant Vendu")
                    {
                        ChartType = SeriesChartType.Line,
                        XValueType = ChartValueType.DateTime,
                        BorderWidth = 2,
                        Color = couleurs["Montant Vendu"],
                        MarkerStyle = MarkerStyle.Circle,
                        YAxisType = AxisType.Secondary
                    };

                    chartPerformanceActivites.Series.Add(serieVues);
                    chartPerformanceActivites.Series.Add(serieMessages);
                    chartPerformanceActivites.Series.Add(serieSpectateurs);
                    chartPerformanceActivites.Series.Add(serieBudget);
                    chartPerformanceActivites.Series.Add(serieNombreVentes);
                    chartPerformanceActivites.Series.Add(serieMontantVendus);

                    using (var cmd = new SqlCommand(@"
                SELECT DateVente,
                       SUM(ISNULL(Vues,0)) AS TotalVues,
                       SUM(ISNULL(Messages,0)) AS TotalMessages,
                       SUM(ISNULL(Spectateurs,0)) AS TotalSpectateurs,
                       SUM(ISNULL(BudgetQuotidien,0)) AS TotalBudget,
                       SUM(ISNULL(NombreVentes,0)) AS TotalNombreVentes,
                       SUM(ISNULL(MontantVendus,0)) AS TotalMontantVendus
                FROM StatistiquesPublicites
                WHERE DateVente BETWEEN @DateDebut AND @DateFin
                GROUP BY DateVente
                ORDER BY DateVente", con))
                    {
                        cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                        cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DateTime date = dr.GetDateTime(0);
                                int vues = dr.IsDBNull(1) ? 0 : dr.GetInt32(1);
                                int messages = dr.IsDBNull(2) ? 0 : dr.GetInt32(2);
                                int spectateurs = dr.IsDBNull(3) ? 0 : dr.GetInt32(3);
                                decimal budget = dr.IsDBNull(4) ? 0m : dr.GetDecimal(4);
                                int nbVentes = dr.IsDBNull(5) ? 0 : dr.GetInt32(5);
                                decimal montant = dr.IsDBNull(6) ? 0m : dr.GetDecimal(6);

                                serieVues.Points.AddXY(date, vues);
                                serieMessages.Points.AddXY(date, messages);
                                serieSpectateurs.Points.AddXY(date, spectateurs);
                                serieBudget.Points.AddXY(date, (double)budget);
                                serieNombreVentes.Points.AddXY(date, nbVentes);
                                serieMontantVendus.Points.AddXY(date, (double)montant);

                                totalVues += vues;
                                totalMessages += messages;
                                totalSpectateurs += spectateurs;
                                totalBudget += budget;
                                totalNombreVentes += nbVentes;
                                totalMontantVendus += montant;
                            }
                        }
                    }
                }
            }

            // =========================
            // 4) CAMEMBERT GLOBAL
            // =========================
            decimal totalGeneral =
    (totalVentesCDF + totalVentesUSD) + totalProduits + totalPresences + totalAbsences +
    totalVues + totalMessages + totalSpectateurs + totalBudget +
    totalNombreVentes + totalMontantVendus;

            if (totalGeneral <= 0m) totalGeneral = 1m;

            void AjouterPointCamembert(string label, decimal valeur, Color couleur)
            {
                if (valeur <= 0) return;

                int idx = serieCamembert.Points.AddXY(label, valeur);
                var p = serieCamembert.Points[idx];

                p.Color = couleur;
                p.LegendText = label;
                p.Label = $"{(double)(valeur / totalGeneral * 100):F1}%";
            }

            AjouterPointCamembert("Ventes CDF", totalVentesCDF, couleurs["Ventes CDF"]);
            AjouterPointCamembert("Ventes USD", totalVentesUSD, couleurs["Ventes USD"]);
            AjouterPointCamembert("Produits", totalProduits, couleurs["Produits"]);
            AjouterPointCamembert("Présences", totalPresences, couleurs["Présences"]);
            AjouterPointCamembert("Absences", totalAbsences, couleurs["Absences"]);
            AjouterPointCamembert("Vues Marketing", totalVues, couleurs["Vues Marketing"]);
            AjouterPointCamembert("Messages Marketing", totalMessages, couleurs["Messages Marketing"]);
            AjouterPointCamembert("Spectateurs Marketing", totalSpectateurs, couleurs["Spectateurs Marketing"]);
            AjouterPointCamembert("Budget Marketing", totalBudget, couleurs["Budget Marketing"]);
            AjouterPointCamembert("Nombre Ventes", totalNombreVentes, couleurs["Nombre Ventes"]);
            AjouterPointCamembert("Montant Vendu", totalMontantVendus, couleurs["Montant Vendu"]);

            // =========================
            // 5) STYLE PRO ANTI-SUPERPOSITION
            // =========================
            serieCamembert["PieLabelStyle"] = "Outside";
            serieCamembert["PieLineColor"] = "Black";
            serieCamembert["PieDrawingStyle"] = "Concave";

            serieCamembert.SmartLabelStyle.Enabled = true;
            serieCamembert.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;

            // Direction sur 4 côtés => meilleure répartition
            serieCamembert.SmartLabelStyle.MovingDirection =
                LabelAlignmentStyles.Left | LabelAlignmentStyles.Right |
                LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom;

            // Distances élevées => labels vraiment éloignés
            serieCamembert.SmartLabelStyle.MinMovingDistance = 25;
            serieCamembert.SmartLabelStyle.MaxMovingDistance = 140;

            serieCamembert.SmartLabelStyle.CalloutLineColor = Color.Black;
            serieCamembert.SmartLabelStyle.CalloutLineAnchorCapStyle = LineAnchorCapStyle.Diamond;
            serieCamembert.SmartLabelStyle.CalloutLineWidth = 1;
        }

        private void ChargerStatistiques(DateTime dateDebut, DateTime dateFin, List<string> categories)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                decimal totalVentesCDF = 0m;
                decimal totalVentesUSD = 0m;
                int stockInitialTotal = 0;
                int stockActuelTotal = 0;
                int totalPresences = 0;
                int totalAbsences = 0;

                // MARKETING
                int totalVuesMarketing = 0;
                int totalMessagesMarketing = 0;
                int totalSpectateursMarketing = 0;
                decimal totalBudgetQuotidienMarketing = 0m;
                int totalNombreVentesMarketing = 0;       // ajouté
                decimal totalMontantVendusMarketing = 0m; // ajouté

                DataTable dtVentes = new DataTable();
                DataTable dtPresences = new DataTable();
                DataTable dtAbsences = new DataTable();
                DataTable dtMarketing = new DataTable();

                #region VENTES

                if (categories.Contains("Ventes"))
                {
                    using (var cmd = new SqlCommand(@"
SELECT 
    UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'CDF')))) AS Devise,
    SUM(ISNULL(p.Montant,0)) AS TotalMontant
FROM PaiementsVente p
WHERE p.DatePaiement BETWEEN @DateDebut AND @DateFin
  AND ISNULL(p.Statut,'') <> 'ANNULÉ'
  AND p.DateAnnulation IS NULL
GROUP BY UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'CDF'))));", con))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                        cmd.Parameters.AddWithValue("@DateFin", dateFin);

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                string dev = dr["Devise"]?.ToString()?.Trim().ToUpper();
                                decimal total = dr.IsDBNull(1) ? 0m : dr.GetDecimal(1);

                                if (dev == "USD") totalVentesUSD += total;
                                else totalVentesCDF += total;
                            }
                        }
                    }
                }
                #endregion

                #region PRODUITS
                if (categories.Contains("Produits"))
                {
                    var produitsCmd = new SqlCommand(@"
            SELECT 
                SUM(StockInitial) AS TotalStockInitial, 
                SUM(StockActuel) AS TotalStockActuel
            FROM Produit
        ", con);

                    using (var reader = produitsCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stockInitialTotal = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            stockActuelTotal = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        }
                    }
                }
                #endregion

                #region PRESENCES
                if (categories.Contains("Employés"))
                {
                    var presencesCmd = new SqlCommand(@"
            SELECT YEAR(JourDate) AS Annee, MONTH(JourDate) AS Mois, COUNT(*) AS TotalPresences
            FROM PresenceAbsenceAgents
            WHERE Present = 1 AND JourDate BETWEEN @DateDebut AND @DateFin
            GROUP BY YEAR(JourDate), MONTH(JourDate)
            ORDER BY Annee, Mois
        ", con);

                    presencesCmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                    presencesCmd.Parameters.AddWithValue("@DateFin", dateFin);

                    dtPresences.Load(presencesCmd.ExecuteReader());

                    totalPresences = dtPresences.AsEnumerable()
                        .Sum(r => r["TotalPresences"] == DBNull.Value ? 0 : Convert.ToInt32(r["TotalPresences"]));
                }
                #endregion

                #region ABSENCES
                if (categories.Contains("Absences"))
                {
                    var absencesCmd = new SqlCommand(@"
            SELECT YEAR(JourDate) AS Annee, MONTH(JourDate) AS Mois, COUNT(*) AS TotalAbsences
            FROM PresenceAbsenceAgents
            WHERE Absent = 1 AND JourDate BETWEEN @DateDebut AND @DateFin
            GROUP BY YEAR(JourDate), MONTH(JourDate)
            ORDER BY Annee, Mois
        ", con);

                    absencesCmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                    absencesCmd.Parameters.AddWithValue("@DateFin", dateFin);

                    dtAbsences.Load(absencesCmd.ExecuteReader());

                    totalAbsences = dtAbsences.AsEnumerable()
                        .Sum(r => r["TotalAbsences"] == DBNull.Value ? 0 : Convert.ToInt32(r["TotalAbsences"]));
                }
                #endregion

                #region MARKETING
                if (categories.Contains("Marketing"))
                {
                    var marketingCmd = new SqlCommand(@"
    SELECT 
        YEAR(DateVente) AS Annee,
        MONTH(DateVente) AS Mois,
        SUM(ISNULL(Vues, 0)) AS TotalVues,
        SUM(ISNULL(Messages, 0)) AS TotalMessages,
        SUM(ISNULL(Spectateurs, 0)) AS TotalSpectateurs,
        SUM(ISNULL(BudgetQuotidien, 0)) AS TotalBudgetQuotidien,
        SUM(ISNULL(NombreVentes, 0)) AS TotalNombreVentes,           -- ajouté
        SUM(ISNULL(MontantVendus, 0)) AS TotalMontantVendus          -- ajouté
    FROM StatistiquesPublicites
    WHERE DateVente BETWEEN @DateDebut AND @DateFin
    GROUP BY YEAR(DateVente), MONTH(DateVente)
    ORDER BY Annee, Mois
", con);

                    marketingCmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                    marketingCmd.Parameters.AddWithValue("@DateFin", dateFin);

                    dtMarketing.Load(marketingCmd.ExecuteReader());

                    totalVuesMarketing = dtMarketing.AsEnumerable().Sum(r => r.Field<int>("TotalVues"));
                    totalMessagesMarketing = dtMarketing.AsEnumerable().Sum(r => r.Field<int>("TotalMessages"));
                    totalSpectateursMarketing = dtMarketing.AsEnumerable().Sum(r => r.Field<int>("TotalSpectateurs"));
                    totalBudgetQuotidienMarketing = dtMarketing.AsEnumerable()
                        .Sum(r => r.Field<decimal>("TotalBudgetQuotidien"));

                    totalNombreVentesMarketing = dtMarketing.AsEnumerable()
                        .Sum(r => r.Field<int>("TotalNombreVentes"));

                    totalMontantVendusMarketing = dtMarketing.AsEnumerable()
                        .Sum(r => r.Field<decimal>("TotalMontantVendus"));
                }
                #endregion

                // ================= LABELS =================
                lblMontantTotalVente.Text = $"{totalVentesCDF:N2} CDF  |  {totalVentesUSD:N2} USD";
                lblArticleProduitStock.Text = stockActuelTotal.ToString();
                lblNombrePresence.Text = totalPresences.ToString();
                lblNombreAbsence.Text = totalAbsences.ToString();

                lblTotalVuesMarketing.Text = totalVuesMarketing.ToString();
                lblTotalMessagesMarketing.Text = totalMessagesMarketing.ToString();
                lblTotalSpectateursMarketing.Text = totalSpectateursMarketing.ToString();
                lblTotalBudgetQuotidienMarketing.Text = totalBudgetQuotidienMarketing.ToString("N2") + " $";

                // Nouveaux labels
                lblNombreVentes.Text = totalNombreVentesMarketing.ToString();
                lblMontantVendus.Text = totalMontantVendusMarketing.ToString("N2") + " FC";
            }
        }

        private void ChargerDataGridView(DateTime dateDebut, DateTime dateFin, List<string> categories)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                var sqlParts = new List<string>();

                #region VENTES
                if (categories.Contains("Ventes"))
                {
                    sqlParts.Add(@"
                SELECT 
                    v.DateVente AS DateActivite,
                    'Ventes' AS Activite,
                    d.NomProduit AS Details,
                    SUM(d.Quantite) AS Quantite,
                    SUM(d.Montant) AS Montant
                FROM DetailsVente d
                INNER JOIN Vente v ON d.ID_Vente = v.ID_Vente
                WHERE v.DateVente BETWEEN @DateDebut AND @DateFin
                GROUP BY v.DateVente, d.NomProduit
            ");
                }
                #endregion

                #region PRODUITS
                if (categories.Contains("Produits"))
                {
                    sqlParts.Add(@"
                SELECT 
                    GETDATE() AS DateActivite,
                    'Produits' AS Activite,
                    'Stock actuel' AS Details,
                    SUM(StockActuel) AS Quantite,
                    0 AS Montant
                FROM Produit
            ");
                }
                #endregion

                #region EMPLOYÉS
                if (categories.Contains("Employés"))
                {
                    sqlParts.Add(@"
                SELECT 
                    JourDate AS DateActivite,
                    'Employés' AS Activite,
                    NomPrenom AS Details,
                    COUNT(*) AS Quantite,
                    0 AS Montant
                FROM PresenceAbsenceAgents
                WHERE Present = 1
                  AND JourDate BETWEEN @DateDebut AND @DateFin
                GROUP BY JourDate, NomPrenom
            ");
                }
                #endregion

                #region ABSENCES
                if (categories.Contains("Absences"))
                {
                    sqlParts.Add(@"
                SELECT 
                    JourDate AS DateActivite,
                    'Absences' AS Activite,
                    NomPrenom AS Details,
                    COUNT(*) AS Quantite,
                    0 AS Montant
                FROM PresenceAbsenceAgents
                WHERE Absent = 1
                  AND JourDate BETWEEN @DateDebut AND @DateFin
                GROUP BY JourDate, NomPrenom
            ");
                }
                #endregion

                if (sqlParts.Count == 0)
                    return;

                string sql = string.Join(" UNION ALL ", sqlParts) + " ORDER BY DateActivite DESC";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@DateDebut", SqlDbType.Date).Value = dateDebut.Date;
                    cmd.Parameters.Add("@DateFin", SqlDbType.Date).Value = dateFin.Date;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            dgvRapport.DataSource = dt;
            dgvRapport.ReadOnly = true;
            dgvRapport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRapport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void btnAppliquerFiltre_Click(object sender, EventArgs e)
        {
            ChargerDonnees();
        }

        private void btnActualiser_Click(object sender, EventArgs e)
        {
            ChargerDonnees();

        }

        private void btnExporter_Click(object sender, EventArgs e)
        {
            // On exporte en mode "période courante"
            DeterminerPeriode(out DateTime dateDebut, out DateTime dateFin);

            using (SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Fichier PDF|*.pdf",
                FileName = $"Statistiques_Journalieres_{dateDebut:yyyyMMdd}_{dateFin:yyyyMMdd}.pdf"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // ===== 1) Récupération des données (SQL) =====
                    DataTable dtVentesJour = ChargerVentesJournalieres(dateDebut, dateFin);
                    DataTable dtPresAbsJour = ChargerPresenceAbsenceJournalieres(dateDebut, dateFin);
                    DataTable dtProduits = ChargerProduitsSnapshot();
                    DataTable dtMarketingJour = ChargerMarketingJournalier(dateDebut, dateFin);

                    // Si vraiment rien à exporter
                    bool noData =
                        (dtVentesJour.Rows.Count == 0) &&
                        (dtPresAbsJour.Rows.Count == 0) &&
                        (dtProduits.Rows.Count == 0) &&
                        (dtMarketingJour.Rows.Count == 0);

                    if (noData)
                    {
                        MessageBox.Show("Aucune donnée à exporter pour la période sélectionnée.", "Exporter PDF",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // ===== 2) Création du PDF =====
                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4, 15f, 15f, 20f, 20f);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                        var fontTitle = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD);
                        var fontSubTitle = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD);
                        var fontHeader = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);
                        var fontRow = new iTextSharp.text.Font(baseFont, 9, iTextSharp.text.Font.NORMAL);

                        // Titre
                        pdfDoc.Add(new Paragraph("Statistiques Avancées — Journalier", fontTitle)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 8f
                        });

                        // Période
                        pdfDoc.Add(new Paragraph($"Période : {dateDebut:dd/MM/yyyy}  →  {dateFin:dd/MM/yyyy}", fontRow)
                        {
                            Alignment = Element.ALIGN_CENTER,
                            SpacingAfter = 12f
                        });

                        // ===== 3) Résumé (labels existants) =====
                        pdfDoc.Add(new Paragraph("Résumé (période)", fontSubTitle) { SpacingAfter = 6f });

                        PdfPTable resume = new PdfPTable(2) { WidthPercentage = 100 };
                        resume.SetWidths(new float[] { 40f, 60f });

                        void AddResume(string k, string v)
                        {
                            resume.AddCell(new PdfPCell(new Phrase(k, fontHeader)) { BackgroundColor = new BaseColor(245, 245, 245), Padding = 5 });
                            resume.AddCell(new PdfPCell(new Phrase(v ?? "", fontRow)) { Padding = 5 });
                        }

                        AddResume("Montant total ventes", lblMontantTotalVente.Text);
                        AddResume("Stock actuel", lblArticleProduitStock.Text);
                        AddResume("Présences", lblNombrePresence.Text);
                        AddResume("Absences", lblNombreAbsence.Text);

                        AddResume("Marketing - Vues", lblTotalVuesMarketing.Text);
                        AddResume("Marketing - Messages", lblTotalMessagesMarketing.Text);
                        AddResume("Marketing - Spectateurs", lblTotalSpectateursMarketing.Text);
                        AddResume("Marketing - Budget", lblTotalBudgetQuotidienMarketing.Text);
                        AddResume("Marketing - Nombre ventes", lblNombreVentes.Text);
                        AddResume("Marketing - Montant vendus", lblMontantVendus.Text);

                        pdfDoc.Add(resume);
                        pdfDoc.Add(new Paragraph(" ") { SpacingAfter = 6f });

                        // ===== 4) Sections journalières =====
                        AjouterSectionTable(pdfDoc, "Ventes — Journalier", dtVentesJour, fontHeader, fontRow);
                        AjouterSectionTable(pdfDoc, "Présences / Absences — Journalier", dtPresAbsJour, fontHeader, fontRow);
                        AjouterSectionTable(pdfDoc, "Produits — Snapshot", dtProduits, fontHeader, fontRow);

                        // Marketing
                        AjouterSectionTable(pdfDoc, "Marketing — Journalier (Campagnes)", dtMarketingJour, fontHeader, fontRow);

                        pdfDoc.Close();
                        stream.Close();
                    }

                    MessageBox.Show("Export PDF terminé avec succès.", "Exporter PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export PDF : {ex.Message}", "Exporter PDF",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private DataTable ChargerVentesJournalieres(DateTime dateDebut, DateTime dateFin)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT
    CAST(p.DatePaiement AS date) AS [Date],
    COUNT(DISTINCT p.IdVente) AS [NombreVentes],
    SUM(ISNULL(p.Montant,0)) AS [MontantTotal]
FROM PaiementsVente p
WHERE p.DatePaiement BETWEEN @DateDebut AND @DateFin
  AND ISNULL(p.Statut,'') <> 'ANNULÉ'
  AND p.DateAnnulation IS NULL
GROUP BY CAST(p.DatePaiement AS date)
ORDER BY [Date];";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                    cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                    dt.Load(cmd.ExecuteReader());
                }
            }

            return dt;
        }

        private DataTable ChargerPresenceAbsenceJournalieres(DateTime dateDebut, DateTime dateFin)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT
    CAST(JourDate AS date) AS [Date],
    SUM(CASE WHEN Present = 1 THEN 1 ELSE 0 END) AS [Présences],
    SUM(CASE WHEN Absent = 1 THEN 1 ELSE 0 END) AS [Absences]
FROM PresenceAbsenceAgents
WHERE JourDate BETWEEN @DateDebut AND @DateFin
GROUP BY CAST(JourDate AS date)
ORDER BY [Date];";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                    cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                    dt.Load(cmd.ExecuteReader());
                }
            }

            return dt;
        }
        private DataTable ChargerProduitsSnapshot()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Info", typeof(string));
            dt.Columns.Add("Valeur", typeof(string));

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT
    ISNULL(SUM(StockInitial), 0) AS TotalStockInitial,
    ISNULL(SUM(StockActuel), 0) AS TotalStockActuel
FROM Produit;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        int stockInit = r.IsDBNull(0) ? 0 : Convert.ToInt32(r[0]);
                        int stockActuel = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]);

                        dt.Rows.Add("Stock initial total", stockInit.ToString("N0"));
                        dt.Rows.Add("Stock actuel total", stockActuel.ToString("N0"));
                    }
                }
            }

            return dt;
        }
        private DataTable ChargerMarketingJournalier(DateTime dateDebut, DateTime dateFin)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string sql = @"
SELECT
    CAST(DateVente AS date) AS [Date],
    NomCampagne AS [Campagne],
    ISNULL(Vues, 0) AS [Vues],
    ISNULL(Messages, 0) AS [Messages],
    ISNULL(Spectateurs, 0) AS [Spectateurs],
    ISNULL(BudgetQuotidien, 0) AS [BudgetQuotidien],
    ISNULL(NombreVentes, 0) AS [NombreVentes],
    ISNULL(MontantVendus, 0) AS [MontantVendus],
    ISNULL(Devise, 'FC') AS [Devise]
FROM StatistiquesPublicites
WHERE DateVente BETWEEN @DateDebut AND @DateFin
ORDER BY CAST(DateVente AS date) DESC, NomCampagne;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@DateDebut", SqlDbType.DateTime).Value = dateDebut;
                    cmd.Parameters.Add("@DateFin", SqlDbType.DateTime).Value = dateFin;

                    dt.Load(cmd.ExecuteReader());
                }
            }

            return dt;
        }
        private void AjouterSectionTable(Document pdfDoc, string titre, DataTable dt,
    iTextSharp.text.Font fontHeader, iTextSharp.text.Font fontRow)
        {
            if (dt == null || dt.Columns.Count == 0)
                return;

            pdfDoc.Add(new Paragraph(titre, fontHeader) { SpacingBefore = 10f, SpacingAfter = 6f });

            if (dt.Rows.Count == 0)
            {
                pdfDoc.Add(new Paragraph("Aucune donnée.", fontRow) { SpacingAfter = 6f });
                return;
            }

            PdfPTable table = new PdfPTable(dt.Columns.Count)
            {
                WidthPercentage = 100
            };

            // En-têtes
            foreach (DataColumn col in dt.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, fontHeader))
                {
                    BackgroundColor = new BaseColor(230, 230, 230),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            // Lignes
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    string texte = row[col] == DBNull.Value ? "" : row[col].ToString();
                    PdfPCell cell = new PdfPCell(new Phrase(texte, fontRow))
                    {
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }

            pdfDoc.Add(table);
        }

        private void flowLayoutPanelResume_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnVoirStats_Click(object sender, EventArgs e)
        {
            if (campagneIdSelectionnee <= 0)
            {
                MessageBox.Show("Aucune campagne sélectionnée.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Actualise l'affichage avec les stats filtrées par la campagne sélectionnée
            ActualiserAffichage();
        }
    }
}