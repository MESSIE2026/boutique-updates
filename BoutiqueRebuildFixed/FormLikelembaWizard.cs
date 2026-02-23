using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PdfSharp.Drawing;
using System.Data.SqlClient;
using PdfSharp.Pdf;
using PdfSharp.Fonts;

namespace BoutiqueRebuildFixed
{
    public partial class FormLikelembaWizard : Form
    {
        // ===================== Données =====================
        private BindingList<MemberInfo> _members = new BindingList<MemberInfo>();
        private BindingList<PaymentRecord> _payments = new BindingList<PaymentRecord>();

        // ===================== UI - Wizard =====================
        private TabControl tabSteps;
        private TabPage tab1General, tab2Members, tab3Advance, tab4Payments;

        private Button btnPrev, btnNext, btnGeneratePdf;
        private Label lblStepTitle;

        // ===================== En-tête =====================
        private Label lblHeader;

        // ===================== Étape 1 - General =====================
        private TextBox txtNomLikemba, txtResponsable;
        private NumericUpDown nudMontantMensuel, nudNbMembres;
        private DateTimePicker dtStartCycle;

        // Règles
        private CheckBox chkRuleFixRules, chkRuleNotePayment, chkRuleSignRemise, chkRuleNoMixMoney, chkRuleDefaultRule;

        // ===================== Étape 2 - Membres =====================
        private DataGridView dgvMembers;
        private Button btnAddMember, btnRemoveMember, btnAutoMonths;

        // ===================== Étape 3 - Avance =====================
        private ComboBox cboMemberAdvance, cboMonthAdvance, cboPayModeAdvance;
        private NumericUpDown nudAdvanceMonths;
        private TextBox txtSignatureRemise, txtObsAdvance;
        private DateTimePicker dtAdvancePayDate;
        private Button btnAddAdvancePayment;

        // ===================== Étape 4 - Paiements =====================
        private DataGridView dgvPayments;
        private Button btnAddPaymentRow, btnRemovePaymentRow, btnAutoBuildPayments;
        private Button btnGenerateRemisePdf;
        private Button btnNewLikemba;
        private Button btnQuitter;
        private ComboBox cboReceiptMember, cboReceiptMonth;
        private DateTimePicker dtReceiptDate;
        private TextBox txtReceiptTreasurerSign, txtReceiptResponsibleSign;

        private ToolTip _hint;
        private int _currentLikelembaId = 0; // 0 = nouveau dossier
        private bool _isLoadingLikelemba = false;
        private Timer _searchTimer;
        private bool _searchInitDone = false;
        private AutoCompleteStringCollection _likelembaAutoSource = new AutoCompleteStringCollection();

        // ===================== Constantes =====================
        private readonly string[] MonthsFr = new[]
        {
            "Janvier","Février","Mars","Avril","Mai","Juin",
            "Juillet","Août","Septembre","Octobre","Novembre","Décembre"
        };
        public FormLikelembaWizard()
        {
            InitializeComponent();

            // ✅ Important : avant toute création de XFont / PDF
            if (!_fontResolverReady)
            {
                GlobalFontSettings.FontResolver = new MinimalFontResolver();
                _fontResolverReady = true;
            }

            Text = "Likelemba - Zaïre Sociale (Assistant)";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1200;
            Height = 760;
            MinimumSize = new Size(1080, 680);
            Font = new Font("Segoe UI", 10);

            BuildUi();
            BindData();
            FixAllDgvColumns();
            RefreshCombos();
            InitLikelembaSearch();
            LoadLikelembaNamesForAutocomplete();
            UpdateStepUi();
        }

        private void FormLikelembaWizard_Load(object sender, EventArgs e)
        {

        }

        private void SetHint(Control c, string hint)
        {
            if (_hint == null)
            {
                _hint = new ToolTip
                {
                    AutoPopDelay = 8000,
                    InitialDelay = 400,
                    ReshowDelay = 200,
                    ShowAlways = true
                };
            }
            _hint.SetToolTip(c, hint);
        }

        private string NormalizeInput(string s)
        {
            if (s == null) return "";
            s = s.Replace('\u00A0', ' ')
                 .Replace('\u2007', ' ')
                 .Replace('\u202F', ' ');
            return s.Trim();
        }

        private void SaveLikelembaToDb()
        {
            CommitGridEdits();

            string nom = NormalizeInput(txtNomLikemba.Text);
            string resp = NormalizeInput(txtResponsable.Text);

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(resp))
            {
                MessageBox.Show("Nom Likemba et Responsable obligatoires.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var tr = con.BeginTransaction())
                {
                    try
                    {
                        if (_currentLikelembaId == 0)
                        {
                            var cmd = new SqlCommand(@"
INSERT INTO dbo.Likelemba
(NomLikemba, PeriodeCycle, MontantMensuelUSD, NombreMembres, ResponsableNom, DateDebutCycle, DateCreation, Actif)
VALUES
(@Nom, @Periode, @Montant, @Nb, @Resp, @DateDebut, GETDATE(), 1);
SELECT SCOPE_IDENTITY();", con, tr);

                            cmd.Parameters.AddWithValue("@Nom", nom);
                            cmd.Parameters.AddWithValue("@Periode", dtStartCycle.Value.ToString("dd/MM/yyyy"));
                            cmd.Parameters.AddWithValue("@Montant", nudMontantMensuel.Value);
                            cmd.Parameters.AddWithValue("@Nb", (int)nudNbMembres.Value);
                            cmd.Parameters.AddWithValue("@Resp", resp);
                            cmd.Parameters.AddWithValue("@DateDebut", dtStartCycle.Value.Date);

                            _currentLikelembaId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else
                        {
                            var cmd = new SqlCommand(@"
UPDATE dbo.Likelemba SET
NomLikemba=@Nom,
PeriodeCycle=@Periode,
MontantMensuelUSD=@Montant,
NombreMembres=@Nb,
ResponsableNom=@Resp,
DateDebutCycle=@DateDebut,
Actif=1
WHERE IdLikelemba=@Id;", con, tr);

                            cmd.Parameters.AddWithValue("@Id", _currentLikelembaId);
                            cmd.Parameters.AddWithValue("@Nom", nom);
                            cmd.Parameters.AddWithValue("@Periode", dtStartCycle.Value.ToString("dd/MM/yyyy"));
                            cmd.Parameters.AddWithValue("@Montant", nudMontantMensuel.Value);
                            cmd.Parameters.AddWithValue("@Nb", (int)nudNbMembres.Value);
                            cmd.Parameters.AddWithValue("@Resp", resp);
                            cmd.Parameters.AddWithValue("@DateDebut", dtStartCycle.Value.Date);

                            cmd.ExecuteNonQuery();

                            new SqlCommand("DELETE FROM dbo.LikelembaRegles WHERE IdLikelemba=@Id", con, tr)
                            { Parameters = { new SqlParameter("@Id", _currentLikelembaId) } }.ExecuteNonQuery();

                            new SqlCommand("DELETE FROM dbo.LikelembaPaiement WHERE IdLikelemba=@Id", con, tr)
                            { Parameters = { new SqlParameter("@Id", _currentLikelembaId) } }.ExecuteNonQuery();

                            new SqlCommand("DELETE FROM dbo.LikelembaMembre WHERE IdLikelemba=@Id", con, tr)
                            { Parameters = { new SqlParameter("@Id", _currentLikelembaId) } }.ExecuteNonQuery();
                        }

                        var cmdR = new SqlCommand(@"
INSERT INTO dbo.LikelembaRegles
(IdLikelemba, FixerReglesAvant, NoterChaquePaiement, SignerLorsRemise, NePasMelangerArgent, RegleDefaillanceClair, TexteRegleDefaillance)
VALUES
(@Id, @R1, @R2, @R3, @R4, @R5, @Txt);", con, tr);

                        cmdR.Parameters.AddWithValue("@Id", _currentLikelembaId);
                        cmdR.Parameters.AddWithValue("@R1", chkRuleFixRules.Checked);
                        cmdR.Parameters.AddWithValue("@R2", chkRuleNotePayment.Checked);
                        cmdR.Parameters.AddWithValue("@R3", chkRuleSignRemise.Checked);
                        cmdR.Parameters.AddWithValue("@R4", chkRuleNoMixMoney.Checked);
                        cmdR.Parameters.AddWithValue("@R5", chkRuleDefaultRule.Checked);
                        cmdR.Parameters.AddWithValue("@Txt", "");
                        cmdR.ExecuteNonQuery();

                        var idMap = new Dictionary<int, int>(); // tempId -> newId

                        foreach (var m in _members)
                        {
                            if (string.IsNullOrWhiteSpace((m.Nom ?? "").Trim())) continue;

                            int oldId = m.IdMembre;

                            var cmdM = new SqlCommand(@"
INSERT INTO dbo.LikelembaMembre
(IdLikelemba, Numero, NomMembre, Telephone, MontantMensuelUSD, OrdreReception, MoisReception, MontantRecuUSD,
 StatutReception, SignatureReception, Observation, DateCreation)
VALUES
(@IdL, @Numero, @Nom, @Tel, @Montant, @Ordre, @Mois, @MontantRecu,
 @Statut, @Sign, @Obs, GETDATE());
SELECT SCOPE_IDENTITY();", con, tr);

                            cmdM.Parameters.AddWithValue("@IdL", _currentLikelembaId);
                            cmdM.Parameters.AddWithValue("@Numero", m.Numero);
                            cmdM.Parameters.AddWithValue("@Nom", (m.Nom ?? "").Trim());
                            cmdM.Parameters.AddWithValue("@Tel", (object)(m.Telephone ?? "") ?? DBNull.Value);
                            cmdM.Parameters.AddWithValue("@Montant", m.MontantMensuel);
                            cmdM.Parameters.AddWithValue("@Ordre", m.OrdreReception);
                            cmdM.Parameters.AddWithValue("@Mois", (object)(m.MoisReception ?? "") ?? DBNull.Value);
                            cmdM.Parameters.AddWithValue("@MontantRecu", m.MontantRecu);
                            cmdM.Parameters.AddWithValue("@Statut", m.StatutReception);
                            cmdM.Parameters.AddWithValue("@Sign", (object)(m.SignatureReception ?? "") ?? DBNull.Value);
                            cmdM.Parameters.AddWithValue("@Obs", (object)(m.Observation ?? "") ?? DBNull.Value);

                            int newIdMembre = Convert.ToInt32(cmdM.ExecuteScalar());
                            m.IdMembre = newIdMembre;

                            if (oldId != 0 && oldId != newIdMembre)
                                idMap[oldId] = newIdMembre;
                        }

                        // Remap paiements (si IDs temporaires)
                        if (idMap.Count > 0)
                        {
                            foreach (var p in _payments)
                            {
                                if (p.IdMembre != 0 && idMap.TryGetValue(p.IdMembre, out var newId))
                                    p.IdMembre = newId;
                            }
                        }

                        foreach (var p in _payments)
                        {
                            if (p.IdMembre == 0) continue;
                            if (!IsValidMonth(p.Mois)) continue;

                            int annee = p.Annee <= 0 ? DateTime.Today.Year : p.Annee;

                            var cmdP = new SqlCommand(@"
INSERT INTO dbo.LikelembaPaiement
(IdLikelemba, IdMembre, Mois, Annee, MontantDuUSD, MontantPayeUSD, DatePaiement, ModePaiement, Retard, PenaliteUSD, SignatureTresorier, Observation, DateCreation)
VALUES
(@IdL, @IdM, @Mois, @Annee, @Du, @Paye, @Date, @Mode, @Retard, @Pen, @Sign, @Obs, GETDATE());", con, tr);

                            cmdP.Parameters.AddWithValue("@IdL", _currentLikelembaId);
                            cmdP.Parameters.AddWithValue("@IdM", p.IdMembre);
                            cmdP.Parameters.AddWithValue("@Mois", NormalizeMonth(p.Mois));
                            cmdP.Parameters.AddWithValue("@Annee", annee);
                            cmdP.Parameters.AddWithValue("@Du", p.MontantDu);
                            cmdP.Parameters.AddWithValue("@Paye", p.MontantPaye);
                            cmdP.Parameters.AddWithValue("@Date", (object)(p.DatePaiement ?? "") ?? DBNull.Value);
                            cmdP.Parameters.AddWithValue("@Mode", (object)(p.ModePaiement ?? "") ?? DBNull.Value);
                            cmdP.Parameters.AddWithValue("@Retard", p.Retard);
                            cmdP.Parameters.AddWithValue("@Pen", p.Penalite);
                            cmdP.Parameters.AddWithValue("@Sign", (object)(p.SignatureTresorier ?? "") ?? DBNull.Value);
                            cmdP.Parameters.AddWithValue("@Obs", (object)(p.Observation ?? "") ?? DBNull.Value);

                            cmdP.ExecuteNonQuery();
                        }

                        tr.Commit();

                        MessageBox.Show($"Enregistré ✅ (ID Likelemba = {_currentLikelembaId}).",
                            "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        tr.Rollback();
                        MessageBox.Show("Erreur sauvegarde : " + ex.Message, "Erreur",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void LoadLikelemba(int idLikelemba)
        {
            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                // 1) Likelemba
                var cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.Likelemba WHERE IdLikelemba=@Id", con);
                cmd.Parameters.AddWithValue("@Id", idLikelemba);

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                    {
                        MessageBox.Show("Dossier introuvable.", "Info",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    _currentLikelembaId = idLikelemba;

                    txtNomLikemba.Text = rd["NomLikemba"].ToString();
                    txtResponsable.Text = rd["ResponsableNom"].ToString();
                    nudMontantMensuel.Value = Convert.ToDecimal(rd["MontantMensuelUSD"]);
                    nudNbMembres.Value = Convert.ToInt32(rd["NombreMembres"]);
                    dtStartCycle.Value = Convert.ToDateTime(rd["DateDebutCycle"]);
                }

                // 2) Règles
                chkRuleFixRules.Checked = false;
                chkRuleNotePayment.Checked = false;
                chkRuleSignRemise.Checked = false;
                chkRuleNoMixMoney.Checked = false;
                chkRuleDefaultRule.Checked = false;

                var cmdR = new SqlCommand("SELECT TOP 1 * FROM dbo.LikelembaRegles WHERE IdLikelemba=@Id", con);
                cmdR.Parameters.AddWithValue("@Id", idLikelemba);
                using (var rd = cmdR.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        chkRuleFixRules.Checked = Convert.ToBoolean(rd["FixerReglesAvant"]);
                        chkRuleNotePayment.Checked = Convert.ToBoolean(rd["NoterChaquePaiement"]);
                        chkRuleSignRemise.Checked = Convert.ToBoolean(rd["SignerLorsRemise"]);
                        chkRuleNoMixMoney.Checked = Convert.ToBoolean(rd["NePasMelangerArgent"]);
                        chkRuleDefaultRule.Checked = Convert.ToBoolean(rd["RegleDefaillanceClair"]);
                    }
                }

                // 3) Membres
                _members.Clear();
                var cmdM = new SqlCommand(@"
SELECT * FROM dbo.LikelembaMembre
WHERE IdLikelemba=@Id
ORDER BY Numero;", con);
                cmdM.Parameters.AddWithValue("@Id", idLikelemba);

                using (var rd = cmdM.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        _members.Add(new MemberInfo
                        {
                            IdMembre = Convert.ToInt32(rd["IdMembre"]),
                            Numero = Convert.ToInt32(rd["Numero"]),
                            Nom = rd["NomMembre"].ToString(),
                            Telephone = rd["Telephone"].ToString(),
                            MontantMensuel = Convert.ToDecimal(rd["MontantMensuelUSD"]),
                            OrdreReception = Convert.ToInt32(rd["OrdreReception"]),
                            MoisReception = rd["MoisReception"].ToString(),
                            MontantRecu = Convert.ToDecimal(rd["MontantRecuUSD"]),
                            StatutReception = Convert.ToBoolean(rd["StatutReception"]),
                            SignatureReception = rd["SignatureReception"].ToString(),
                            Observation = rd["Observation"].ToString()
                        });
                    }
                }

                // 4) Paiements
                _payments.Clear();
                var cmdP = new SqlCommand(@"
SELECT * FROM dbo.LikelembaPaiement
WHERE IdLikelemba=@Id
ORDER BY Annee, Mois, IdMembre;", con);
                cmdP.Parameters.AddWithValue("@Id", idLikelemba);

                using (var rd = cmdP.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        _payments.Add(new PaymentRecord
                        {
                            IdMembre = Convert.ToInt32(rd["IdMembre"]),
                            Mois = rd["Mois"].ToString(),
                            Annee = Convert.ToInt32(rd["Annee"]),
                            AnneeKey = 0, // on ne l’utilise pas en logique
                            MontantDu = Convert.ToDecimal(rd["MontantDuUSD"]),
                            MontantPaye = Convert.ToDecimal(rd["MontantPayeUSD"]),
                            DatePaiement = rd["DatePaiement"].ToString(),
                            ModePaiement = rd["ModePaiement"].ToString(),
                            Retard = Convert.ToBoolean(rd["Retard"]),
                            Penalite = Convert.ToDecimal(rd["PenaliteUSD"]),
                            SignatureTresorier = rd["SignatureTresorier"].ToString(),
                            Observation = rd["Observation"].ToString()
                        });
                    }
                }

                RefreshCombos();
                FixAllDgvColumns();
                dgvMembers.Refresh();
                dgvPayments.Refresh();
                UpdateStepUi();

                MessageBox.Show("Dossier chargé ✅ tu peux continuer.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplyPlaceholder(TextBox tb, string placeholder)
        {
            tb.ForeColor = Color.Gray;
            tb.Text = placeholder;

            tb.GotFocus += (s, e) =>
            {
                if (tb.ForeColor == Color.Gray)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.ForeColor = Color.Gray;
                    tb.Text = placeholder;
                }
            };
        }

        private readonly string[] PayModes = new[] { "Cash", "Mobile", "Virement" };

        // ✅ transforme "Janvier" -> 0, etc.
        private int MonthIndex(string mois)
        {
            if (string.IsNullOrWhiteSpace(mois)) return -1;
            for (int i = 0; i < MonthsFr.Length; i++)
                if (string.Equals(MonthsFr[i], mois.Trim(), StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private bool IsValidMonth(string mois) => MonthIndex(mois) >= 0;

        private string NormalizeMonth(string mois)
        {
            int idx = MonthIndex(mois);
            return idx >= 0 ? MonthsFr[idx] : "";
        }

        private MemberInfo FindMemberByName(string name)
        {
            name = (name ?? "").Trim();
            return _members.FirstOrDefault(m => string.Equals((m.Nom ?? "").Trim(), name, StringComparison.OrdinalIgnoreCase));
        }


        private void ConfigureDgvMembers()
        {
            if (dgvMembers == null) return;

            // ✅ EN-TÊTES visibles
            dgvMembers.ColumnHeadersVisible = true;

            // ✅ Look propre
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMembers.ColumnHeadersHeight = 34;

            dgvMembers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvMembers.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgvMembers.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvMembers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 255);

            dgvMembers.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvMembers.RowTemplate.Height = 28;

            dgvMembers.AllowUserToResizeRows = false;
            dgvMembers.AllowUserToOrderColumns = true;

            // ✅ Tooltips sur les colonnes (aide à quoi écrire)
            foreach (DataGridViewColumn col in dgvMembers.Columns)
                col.ToolTipText = GetMemberColumnHint(col.DataPropertyName);
        }

        private string GetMemberColumnHint(string prop)
        {
            switch (prop)
            {
                case "Numero": return "Numéro automatique (ne touche pas).";
                case "Nom": return "Nom complet du membre (ex: John B.).";
                case "Telephone": return "Téléphone (ex: +243 9xx xxx xxx).";
                case "MontantMensuel": return "Cotisation mensuelle par membre.";
                case "OrdreReception": return "Ordre de réception (1 = premier à recevoir).";
                case "MoisReception": return "Mois où ce membre reçoit (auto possible).";
                case "MontantRecu": return "Montant total que le membre reçoit ce mois-là.";
                case "StatutReception": return "Coche si le membre a déjà reçu.";
                case "SignatureReception": return "Nom/Signature lors de la réception.";
                case "Observation": return "Notes (retard, arrangement, etc.).";
                default: return "";
            }
        }

        private void CommitGridEdits()
        {
            try
            {
                // Force commit DataGridView membres
                if (dgvMembers != null)
                {
                    dgvMembers.EndEdit();
                    dgvMembers.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }

                // Force commit DataGridView paiements
                if (dgvPayments != null)
                {
                    dgvPayments.EndEdit();
                    dgvPayments.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }

                // Force commit côté binding (BindingList)
                var cmMembers = BindingContext[_members] as CurrencyManager;
                cmMembers?.EndCurrentEdit();

                var cmPayments = BindingContext[_payments] as CurrencyManager;
                cmPayments?.EndCurrentEdit();
            }
            catch
            {
                // ignore
            }
        }


        private void ConfigureDgvPayments()
        {
            if (dgvPayments == null) return;

            // ✅ EN-TÊTES visibles
            dgvPayments.ColumnHeadersVisible = true;

            // ✅ Look propre
            dgvPayments.EnableHeadersVisualStyles = false;
            dgvPayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPayments.ColumnHeadersHeight = 34;

            dgvPayments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvPayments.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvPayments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgvPayments.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvPayments.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 255);

            dgvPayments.RowTemplate.Height = 28;
            dgvPayments.AllowUserToResizeRows = false;
            dgvPayments.AllowUserToOrderColumns = true;

            // ✅ Tooltips colonnes
            foreach (DataGridViewColumn col in dgvPayments.Columns)
                col.ToolTipText = GetPaymentColumnHint(col.DataPropertyName);
        }

        private string GetPaymentColumnHint(string prop)
        {
            switch (prop)
            {
                case "Mois": return "Mois concerné (Janvier, Février...).";
                case "IdMembre": return "Choisis le membre (la cellule stocke l'ID, affiche le Nom).";
                case "MontantDu": return "Montant à payer ce mois.";
                case "MontantPaye": return "Montant réellement payé.";
                case "DatePaiement": return "Date du paiement (jj/mm/aaaa).";
                case "ModePaiement": return "Cash / Mobile / Virement.";
                case "Retard": return "Coche si paiement en retard.";
                case "Penalite": return "Montant de pénalité si retard.";
                case "SignatureTresorier": return "Nom/Signature du trésorier qui confirme.";
                case "Observation": return "Notes (avance, retard, etc.).";
                default: return "";
            }
        }

        private string CleanText(TextBox tb, string placeholder)
        {
            if (tb == null) return "";
            var t = (tb.Text ?? "").Trim();
            if (tb.ForeColor == Color.Gray && string.Equals(t, placeholder, StringComparison.Ordinal))
                return "";
            return t;
        }

        private void AddRow(TableLayoutPanel tbl, int row, string leftLabel, Control leftControl, string rightLabel, Control rightControl)
        {
            var l1 = new Label { Text = leftLabel, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var l2 = new Label { Text = rightLabel, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

            leftControl.Dock = DockStyle.Fill;
            rightControl.Dock = DockStyle.Fill;

            tbl.Controls.Add(l1, 0, row);
            tbl.Controls.Add(leftControl, 1, row);
            tbl.Controls.Add(l2, 2, row);
            tbl.Controls.Add(rightControl, 3, row);
        }

        private Control BuildTwoTextBoxes(TextBox a, TextBox b)
        {
            var p = new Panel { Dock = DockStyle.Fill };
            a.Width = 170; b.Width = 220;
            a.Left = 0; a.Top = 2;
            b.Left = a.Right + 12; b.Top = 2;
            p.Controls.Add(a);
            p.Controls.Add(b);
            return p;
        }

        // ==========================================================
        // Build UI (COMPLET)
        // ==========================================================
        private void BuildUi()
        {
            SuspendLayout();
            FixAllDgvColumns();

            // ---------- Header ----------
            lblHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 90,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Text =
        @"RÉPUBLIQUE DÉMOCRATIQUE DU CONGO
VILLE PROVINCE DE KINSHASA
BOUTIQUE ZAÏRE
COTISATION SOCIALE : ZAÏRE SOCIALE"
            };
            Controls.Add(lblHeader);

            // ---------- Step title ----------
            lblStepTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 6, 12, 6),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(245, 245, 245)
            };
            Controls.Add(lblStepTitle);

            // ---------- Bottom buttons ----------
            // ---------- Bottom buttons ----------
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(12)
            };
            Controls.Add(pnlBottom);

            // Boutons
            btnPrev = new Button { Text = "◀ Précédent", Width = 140, Height = 36, Top = 10 };
            btnNext = new Button { Text = "Suivant ▶", Width = 140, Height = 36, Top = 10 };

            // ✅ Nouveau Likelemba
            btnNewLikemba = new Button { Text = "Nouveau +", Width = 140, Height = 36, Top = 10 };

            // ✅ PDF cotisation (comme avant)
            btnGeneratePdf = new Button { Text = "Générer PDF reçu du mois", Width = 240, Height = 36, Top = 10 };

            // ✅ PDF remise total (900$ etc.)
            btnGenerateRemisePdf = new Button { Text = "PDF REMISE (TOTAL)", Width = 200, Height = 36, Top = 10 };

            // Quitter à droite
            btnQuitter = new Button
            {
                Text = "Quitter ⏻",
                Width = 140,
                Height = 36,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Ajout au panel (ordre important)
            pnlBottom.Controls.Add(btnPrev);
            pnlBottom.Controls.Add(btnNext);
            pnlBottom.Controls.Add(btnNewLikemba);
            pnlBottom.Controls.Add(btnGeneratePdf);
            pnlBottom.Controls.Add(btnGenerateRemisePdf);
            pnlBottom.Controls.Add(btnQuitter);

            // ✅ Layout automatique (évite chevauchement)
            void LayoutBottomButtons()
            {
                int left = pnlBottom.Padding.Left;
                int right = pnlBottom.ClientSize.Width - pnlBottom.Padding.Right;
                int gap = 10;

                // Droite : Quitter
                btnQuitter.Left = right - btnQuitter.Width;

                // Gauche : Prev, Next, New
                btnPrev.Left = left;
                btnNext.Left = btnPrev.Right + gap;
                btnNewLikemba.Left = btnNext.Right + gap;

                // Centre : PDF Reçu + PDF Remise (entre New et Quitter)
                int zoneLeft = btnNewLikemba.Right + gap;
                int zoneRight = btnQuitter.Left - gap;
                int zoneWidth = Math.Max(0, zoneRight - zoneLeft);

                int totalPdfWidth = btnGeneratePdf.Width + gap + btnGenerateRemisePdf.Width;

                // Si ça ne rentre pas, on réduit un peu automatiquement (sans casser)
                if (totalPdfWidth > zoneWidth && zoneWidth > 0)
                {
                    // On garde des minimums raisonnables
                    int minRecu = 180;
                    int minRemise = 160;

                    int available = zoneWidth - gap;
                    int recuW = Math.Max(minRecu, (int)(available * 0.55));
                    int remiseW = Math.Max(minRemise, available - recuW);

                    // sécurité
                    if (recuW + remiseW > available)
                        remiseW = Math.Max(minRemise, available - recuW);

                    btnGeneratePdf.Width = recuW;
                    btnGenerateRemisePdf.Width = remiseW;
                    totalPdfWidth = btnGeneratePdf.Width + gap + btnGenerateRemisePdf.Width;
                }

                // Centrage dans la zone centrale
                int startX = zoneLeft + Math.Max(0, (zoneWidth - totalPdfWidth) / 2);

                btnGeneratePdf.Left = startX;
                btnGenerateRemisePdf.Left = btnGeneratePdf.Right + gap;
            }

            // Appel layout initial + au resize
            pnlBottom.Resize += (s, e) => LayoutBottomButtons();
            LayoutBottomButtons();

            // ---------- TabControl ----------
            tabSteps = new TabControl { Dock = DockStyle.Fill };
            tabSteps.Appearance = TabAppearance.FlatButtons;
            tabSteps.ItemSize = new Size(0, 1);
            tabSteps.SizeMode = TabSizeMode.Fixed;

            tab1General = new TabPage("1");
            tab2Members = new TabPage("2");
            tab3Advance = new TabPage("3");
            tab4Payments = new TabPage("4");

            tabSteps.TabPages.Add(tab1General);
            tabSteps.TabPages.Add(tab2Members);
            tabSteps.TabPages.Add(tab3Advance);
            tabSteps.TabPages.Add(tab4Payments);

            Controls.Add(tabSteps);
            tabSteps.BringToFront(); // important (reste entre header et bottom)

            // ==========================
            // TAB 1 : Infos + Règles
            // ==========================
            var pnl1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
            tab1General.Controls.Add(pnl1);

            var gbInfo = new GroupBox
            {
                Text = "Informations générales",
                Dock = DockStyle.Top,
                Height = 210,
                Padding = new Padding(12)
            };
            pnl1.Controls.Add(gbInfo);

            var tblInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3
            };
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            gbInfo.Controls.Add(tblInfo);

            txtNomLikemba = new TextBox();
            SetHint(txtNomLikemba, "Ex: Zaïre Sociale");

            txtResponsable = new TextBox();
            SetHint(txtResponsable, "Nom du responsable");

            dtStartCycle = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Value = DateTime.Today
            };

            nudNbMembres = new NumericUpDown { Minimum = 1, Maximum = 200, Value = 6 };
            nudMontantMensuel = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1000000,
                Value = 150,
                DecimalPlaces = 0,
                ThousandsSeparator = true
            };

            AddRow(tblInfo, 0, "Nom du Likemba", txtNomLikemba, "Période / Cycle", dtStartCycle);
            AddRow(tblInfo, 1, "Responsable", txtResponsable, "Nombre de membres", nudNbMembres);
            AddRow(tblInfo, 2, "Montant mensuel ($)", nudMontantMensuel, "", new Label());

            var gbRules = new GroupBox
            {
                Text = "Règles (obligatoires avant de commencer)",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(12)
            };
            pnl1.Controls.Add(gbRules);
            gbRules.BringToFront();

            chkRuleFixRules = new CheckBox { Text = "Toujours fixer les règles avant de commencer", AutoSize = true };
            chkRuleNotePayment = new CheckBox { Text = "Toujours noter chaque paiement", AutoSize = true };
            chkRuleSignRemise = new CheckBox { Text = "Toujours signer lors de la remise", AutoSize = true };
            chkRuleNoMixMoney = new CheckBox { Text = "Ne jamais mélanger likemba et argent personnel", AutoSize = true };
            chkRuleDefaultRule = new CheckBox { Text = "Prévoir une règle claire en cas de défaillance", AutoSize = true };

            var flowRules = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            flowRules.Controls.Add(chkRuleFixRules);
            flowRules.Controls.Add(chkRuleNotePayment);
            flowRules.Controls.Add(chkRuleSignRemise);
            flowRules.Controls.Add(chkRuleNoMixMoney);
            flowRules.Controls.Add(chkRuleDefaultRule);
            gbRules.Controls.Add(flowRules);

            // ==========================
            // TAB 2 : Membres
            // ==========================
            var pnl2 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            tab2Members.Controls.Add(pnl2);

            var top2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.LeftToRight };
            pnl2.Controls.Add(top2);

            btnAddMember = new Button { Text = "+ Ajouter membre", Width = 160, Height = 34 };
            btnRemoveMember = new Button { Text = "– Supprimer membre", Width = 170, Height = 34 };
            btnAutoMonths = new Button { Text = "Auto: Mois réception", Width = 190, Height = 34 };

            top2.Controls.Add(btnAddMember);
            top2.Controls.Add(btnRemoveMember);
            top2.Controls.Add(btnAutoMonths);

            dgvMembers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            pnl2.Controls.Add(dgvMembers);

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°", DataPropertyName = "Numero", Width = 60 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nom du membre (*)", DataPropertyName = "Nom", Width = 220 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Téléphone", DataPropertyName = "Telephone", Width = 160 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Montant mensuel", DataPropertyName = "MontantMensuel", Width = 140 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ordre réception", DataPropertyName = "OrdreReception", Width = 130 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mois réception", DataPropertyName = "MoisReception", Width = 150 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Montant reçu", DataPropertyName = "MontantRecu", Width = 130 });
            dgvMembers.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Reçu ?", DataPropertyName = "StatutReception", Width = 80 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Signature réception", DataPropertyName = "SignatureReception", Width = 210 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Observation", DataPropertyName = "Observation", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            // ✅ IMPORTANT : activer tooltips + config colonnes (quoi écrire)
            dgvMembers.ShowCellToolTips = true;
            ConfigureDgvMembers();
            dgvMembers.ColumnHeadersVisible = true;

            // ==========================
            // TAB 3 : Avance
            // ==========================
            var pnl3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            tab3Advance.Controls.Add(pnl3);

            var gbAdvance = new GroupBox
            {
                Text = "Versement d’avance (enregistrer le paiement + signature lors de la remise)",
                Dock = DockStyle.Top,
                Height = 280,
                Padding = new Padding(12)
            };
            pnl3.Controls.Add(gbAdvance);

            var tblAdv = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4
            };
            tblAdv.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblAdv.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblAdv.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblAdv.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            tblAdv.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblAdv.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblAdv.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblAdv.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            gbAdvance.Controls.Add(tblAdv);

            cboMemberAdvance = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboMonthAdvance = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboMonthAdvance.Items.AddRange(MonthsFr);
            cboMonthAdvance.SelectedIndex = 0;

            nudAdvanceMonths = new NumericUpDown { Minimum = 1, Maximum = 24, Value = 1 };
            dtAdvancePayDate = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.Today };

            cboPayModeAdvance = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboPayModeAdvance.Items.AddRange(new[] { "Cash", "Mobile", "Virement" });
            cboPayModeAdvance.SelectedIndex = 0;

            txtSignatureRemise = new TextBox();
            ApplyPlaceholder(txtSignatureRemise, "Nom + signature (ex: Jean K.)");

            txtObsAdvance = new TextBox();
            ApplyPlaceholder(txtObsAdvance, "Observation (optionnel)");

            AddRow(tblAdv, 0, "Membre", cboMemberAdvance, "Mois de départ", cboMonthAdvance);
            AddRow(tblAdv, 1, "Nombre de mois payés", nudAdvanceMonths, "Date paiement", dtAdvancePayDate);
            AddRow(tblAdv, 2, "Mode paiement", cboPayModeAdvance, "Signature remise", txtSignatureRemise);
            AddRow(tblAdv, 3, "Observation", txtObsAdvance, "", new Label());

            btnAddAdvancePayment = new Button { Text = "Enregistrer versement d'avance", Dock = DockStyle.Bottom, Height = 40 };
            gbAdvance.Controls.Add(btnAddAdvancePayment);
            btnAddAdvancePayment.BringToFront();

            var lblHint3 = new Label
            {
                Dock = DockStyle.Top,
                Text = "Astuce : Tu peux ensuite générer le PDF reçu du mois (en bas) pour faire signer le membre + responsable.",
                Padding = new Padding(8),
                ForeColor = Color.DimGray,
                Height = 40
            };
            pnl3.Controls.Add(lblHint3);
            lblHint3.BringToFront();

            // ==========================
            // TAB 4 : Paiements + Reçu PDF
            // ==========================
            var pnl4 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            tab4Payments.Controls.Add(pnl4);

            var top4 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.LeftToRight };
            pnl4.Controls.Add(top4);

            btnAutoBuildPayments = new Button { Text = "Auto: Générer suivi du cycle", Width = 220, Height = 34 };
            btnAddPaymentRow = new Button { Text = "+ Ligne paiement", Width = 150, Height = 34 };
            btnRemovePaymentRow = new Button { Text = "– Supprimer ligne", Width = 170, Height = 34 };
            top4.Controls.Add(btnAutoBuildPayments);
            top4.Controls.Add(btnAddPaymentRow);
            top4.Controls.Add(btnRemovePaymentRow);

            dgvPayments = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            pnl4.Controls.Add(dgvPayments);

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mois", DataPropertyName = "Mois", Width = 130 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nom du membre", DataPropertyName = "NomMembre", Width = 220 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Montant dû", DataPropertyName = "MontantDu", Width = 120 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Montant payé", DataPropertyName = "MontantPaye", Width = 130 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date paiement", DataPropertyName = "DatePaiement", Width = 130 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", DataPropertyName = "ModePaiement", Width = 110 });
            dgvPayments.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Retard ?", DataPropertyName = "Retard", Width = 90 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Pénalité", DataPropertyName = "Penalite", Width = 110 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Signature trésorier", DataPropertyName = "SignatureTresorier", Width = 170 });
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Observation", DataPropertyName = "Observation", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            // ✅ IMPORTANT : activer tooltips + config colonnes (quoi écrire)
            dgvPayments.ShowCellToolTips = true;
            ConfigureDgvPayments();
            dgvPayments.ColumnHeadersVisible = true;

            var gbReceipt = new GroupBox
            {
                Text = "PDF reçu du mois (à faire signer)",
                Dock = DockStyle.Bottom,
                Height = 160,
                Padding = new Padding(12)
            };
            pnl4.Controls.Add(gbReceipt);

            var tblReceipt = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
            tblReceipt.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblReceipt.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblReceipt.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            tblReceipt.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblReceipt.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            tblReceipt.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            gbReceipt.Controls.Add(tblReceipt);

            cboReceiptMember = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboReceiptMonth = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cboReceiptMonth.Items.AddRange(MonthsFr);
            cboReceiptMonth.SelectedIndex = DateTime.Today.Month - 1;

            dtReceiptDate = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = DateTime.Today };

            txtReceiptTreasurerSign = new TextBox();
            ApplyPlaceholder(txtReceiptTreasurerSign, "Nom du trésorier");

            txtReceiptResponsibleSign = new TextBox();
            ApplyPlaceholder(txtReceiptResponsibleSign, "Nom du responsable Likelemba");

            AddRow(tblReceipt, 0, "Membre", cboReceiptMember, "Mois", cboReceiptMonth);
            AddRow(tblReceipt, 1, "Date", dtReceiptDate, "Sign (Trésorier / Responsable)",
                BuildTwoTextBoxes(txtReceiptTreasurerSign, txtReceiptResponsibleSign));

            // ==========================
            // Events (tous)
            // ==========================
            btnPrev.Click += (s, e) =>
            {
                CommitGridEdits();
                if (tabSteps.SelectedIndex > 0) tabSteps.SelectedIndex--;
                UpdateStepUi();
            };

            // ✅ CORRIGÉ : commit AVANT la validation (sinon "Nom vide" à tort)
            btnNext.Click += (s, e) =>
            {
                CommitGridEdits();

                Validate();
                ActiveControl = null;
                Application.DoEvents();

                // ✅ Si on est au dernier step => on ENREGISTRE
                if (tabSteps.SelectedIndex == tabSteps.TabCount - 1)
                {
                    SaveLikelembaToDb();   // ✅ Enregistre Likelemba + Membres + Paiements
                    return;
                }

                if (!CanGoNext()) return;

                // sinon on avance
                if (tabSteps.SelectedIndex < tabSteps.TabCount - 1)
                    tabSteps.SelectedIndex++;

                UpdateStepUi();
            };

            btnGeneratePdf.Click += (s, e) => GenerateReceiptPdf();
            btnGenerateRemisePdf.Click += (s, e) => GenerateRemisePdf();
            btnQuitter.Click += (s, e) =>
            {
                CommitGridEdits();

                // ✅ Option "Quitter en sauvegardant" si un dossier est déjà commencé
                // (tu peux commenter si tu veux quitter sans save)
                if (!string.IsNullOrWhiteSpace(NormalizeInput(txtNomLikemba.Text)) &&
                    !string.IsNullOrWhiteSpace(NormalizeInput(txtResponsable.Text)))
                {
                    SaveLikelembaToDb();
                }

                // ✅ IMPORTANT : on cache (ne pas Close)
                this.Hide();

                // Si tu avais caché Salaires à l’ouverture, on le ré-affiche :
                if (Owner != null)
                {
                    Owner.Show();
                    Owner.BringToFront();
                    Owner.Activate();
                }
            };

            btnAddMember.Click += (s, e) => AddMember();
            btnRemoveMember.Click += (s, e) => RemoveMember();
            btnAutoMonths.Click += (s, e) => AutoAssignReceptionMonths();

            btnAutoBuildPayments.Click += (s, e) => AutoBuildPaymentsForCycle();
            btnAddPaymentRow.Click += (s, e) => AddPaymentRow();
            btnRemovePaymentRow.Click += (s, e) => RemovePaymentRow();

            btnAddAdvancePayment.Click += (s, e) => AddAdvancePayments();
            btnNewLikemba.Click += (s, e) => NewLikelemba();

            tabSteps.SelectedIndexChanged += (s, e) => UpdateStepUi();

            ResumeLayout();
            InitLikelembaSearch();
            LoadLikelembaNamesForAutocomplete();
        }

        // ==========================================================
        // ✅ PRO : colonnes claires + combos + statut + tooltips
        // ==========================================================
        private void FixAllDgvColumns()
        {
            FixMembersColumns();
            FixPaymentsColumns();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();

                if (Owner != null)
                {
                    Owner.Show();
                    Owner.BringToFront();
                    Owner.Activate();
                }
                return;
            }

            base.OnFormClosing(e);
        }

        private void GenerateRemisePdf()
        {
            if (_members.Count == 0)
            {
                MessageBox.Show("Ajoute des membres d’abord.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string likembaName = NormalizeInput(txtNomLikemba.Text);
            string periode = dtStartCycle.Value.ToString("dd/MM/yyyy");
            string responsable = NormalizeInput(txtResponsable.Text);

            if (string.IsNullOrWhiteSpace(likembaName) || string.IsNullOrWhiteSpace(responsable))
            {
                MessageBox.Show("Complète d’abord les informations générales (Nom du Likemba, Responsable).",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ bénéficiaire (celui qui reçoit le pot)
            var mem = cboReceiptMember.SelectedItem as MemberInfo;
            if (mem == null || string.IsNullOrWhiteSpace(mem.Nom))
            {
                MessageBox.Show("Sélectionne le membre bénéficiaire (celui qui reçoit la remise).",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mois = NormalizeMonth(cboReceiptMonth.Text);
            if (!IsValidMonth(mois))
            {
                MessageBox.Show("Sélectionne un mois valide.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Montant total théorique = montantMensuel × nombreMembres (ex: 150×6=900)
            decimal montantMensuel = nudMontantMensuel.Value;
            int nbMembres = _members.Count;
            decimal totalTheorique = montantMensuel * nbMembres;

            // (optionnel) total réellement payé (si tu veux afficher dans le PDF)
            decimal totalPaye = _payments
                .Where(p => string.Equals(NormalizeMonth(p.Mois), mois, StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.MontantPaye);

            string date = dtReceiptDate.Value.ToString("dd/MM/yyyy");

            string tresReal = CleanText(txtReceiptTreasurerSign, "Nom du trésorier");
            string respReal = CleanText(txtReceiptResponsibleSign, "Nom du responsable Likelemba");

            string tresSign = string.IsNullOrWhiteSpace(tresReal) ? "________________" : tresReal;
            string respSign = string.IsNullOrWhiteSpace(respReal) ? responsable : respReal;

            using (var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Remise_Likelemba_{mem.Nom}_{mois}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    CreateRemisePdf(
                        filePath: sfd.FileName,
                        likembaName: likembaName,
                        periode: periode,
                        responsable: responsable,
                        beneficiaireNom: mem.Nom,
                        beneficiaireTel: mem.Telephone,
                        mois: mois,
                        montantMensuel: montantMensuel,
                        nbMembres: nbMembres,
                        totalTheorique: totalTheorique,
                        totalPaye: totalPaye,
                        dateRemise: date,
                        signatureTresorier: tresSign,
                        signatureResponsable: respSign
                    );

                    MessageBox.Show("PDF REMISE (TOTAL) généré avec succès.", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur PDF REMISE : " + ex.Message, "Erreur",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateRemisePdf(
    string filePath,
    string likembaName,
    string periode,
    string responsable,
    string beneficiaireNom,
    string beneficiaireTel,
    string mois,
    decimal montantMensuel,
    int nbMembres,
    decimal totalTheorique,
    decimal totalPaye,
    string dateRemise,
    string signatureTresorier,
    string signatureResponsable)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Reçu Remise Likelemba";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                double left = 55;
                double right = page.Width.Point - 55;
                double y = 55;

                var title = new XFont("Arial", 14, XFontStyleEx.Bold);
                var bold = new XFont("Arial", 11, XFontStyleEx.Bold);
                var font = new XFont("Arial", 11, XFontStyleEx.Regular);

                // Titre
                gfx.DrawString("REÇU DE REMISE (TOTAL) - LIKELEMBA", title, XBrushes.Black,
                    new XRect(left, y, right - left, 20), XStringFormats.TopCenter);
                y += 28;

                gfx.DrawLine(XPens.Black, left, y, right, y);
                y += 18;

                // Infos Likelemba
                gfx.DrawString("Likemba :", bold, XBrushes.Black, left, y);
                gfx.DrawString(likembaName ?? "", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Cycle :", bold, XBrushes.Black, left, y);
                gfx.DrawString(periode ?? "", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Mois :", bold, XBrushes.Black, left, y);
                gfx.DrawString(mois ?? "", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Cotisation mensuelle :", bold, XBrushes.Black, left, y);
                gfx.DrawString($"{FormatMoney(montantMensuel)} $", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Nombre de membres :", bold, XBrushes.Black, left, y);
                gfx.DrawString(nbMembres.ToString(), font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("TOTAL THÉORIQUE :", bold, XBrushes.Black, left, y);
                gfx.DrawString($"{FormatMoney(totalTheorique)} $", font, XBrushes.Black, left + 160, y);
                y += 20;

                // (Optionnel) total payé (tu peux laisser)
                gfx.DrawString("Total réellement payé :", bold, XBrushes.Black, left, y);
                gfx.DrawString($"{FormatMoney(totalPaye)} $", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Date remise :", bold, XBrushes.Black, left, y);
                gfx.DrawString(dateRemise ?? "", font, XBrushes.Black, left + 160, y);
                y += 26;

                gfx.DrawLine(XPens.Gray, left, y, right, y);
                y += 16;

                // Bénéficiaire
                gfx.DrawString("Bénéficiaire :", bold, XBrushes.Black, left, y);
                gfx.DrawString(beneficiaireNom ?? "", font, XBrushes.Black, left + 160, y);
                y += 20;

                gfx.DrawString("Téléphone :", bold, XBrushes.Black, left, y);
                gfx.DrawString(string.IsNullOrWhiteSpace(beneficiaireTel) ? "—" : beneficiaireTel,
                    font, XBrushes.Black, left + 160, y);
                y += 30;

                // Signature bénéficiaire (réception)
                gfx.DrawString("Signature bénéficiaire :", bold, XBrushes.Black, left, y);
                gfx.DrawLine(XPens.Black, left + 200, y + 12, right, y + 12);
                y += 30;

                // Signatures
                gfx.DrawString("Trésorier :", bold, XBrushes.Black, left, y);
                gfx.DrawString(signatureTresorier ?? "________________", font, XBrushes.Black, left + 160, y);
                y += 22;

                gfx.DrawString("Responsable :", bold, XBrushes.Black, left, y);
                gfx.DrawString(string.IsNullOrWhiteSpace(signatureResponsable) ? responsable : signatureResponsable,
                    font, XBrushes.Black, left + 160, y);
                y += 30;

                gfx.DrawLine(XPens.Gray, left, y, right, y);
                y += 14;

                gfx.DrawString("Rappel : la remise correspond au total du cycle pour le mois indiqué.",
                    font, XBrushes.Black, new XRect(left, y, right - left, 20), XStringFormats.TopLeft);
            }

            doc.Save(filePath);
        }



        private void FixMembersColumns()
        {
            if (dgvMembers == null) return;

            dgvMembers.SuspendLayout();

            dgvMembers.AutoGenerateColumns = false;
            dgvMembers.ColumnHeadersVisible = true;
            dgvMembers.RowHeadersVisible = false;

            // Si tu veux être 100% sûr d’éviter des colonnes “fantômes”
            dgvMembers.Columns.Clear();

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNumero",
                HeaderText = "N°",
                DataPropertyName = "Numero",
                Width = 60,
                ReadOnly = true
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNom",
                HeaderText = "NOM MEMBRE (*)",
                DataPropertyName = "Nom",
                Width = 240
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTel",
                HeaderText = "TÉLÉPHONE",
                DataPropertyName = "Telephone",
                Width = 160
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMontantMensuel",
                HeaderText = "COTISATION ($)",
                DataPropertyName = "MontantMensuel",
                Width = 140
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrdre",
                HeaderText = "ORDRE RÉCEPTION",
                DataPropertyName = "OrdreReception",
                Width = 150
            });

            // Mois réception : mieux en ComboBox (choix contrôlé)
            dgvMembers.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colMoisReception",
                HeaderText = "MOIS RÉCEPTION",
                DataPropertyName = "MoisReception",
                Width = 160,
                DataSource = MonthsFr.ToList()
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMontantRecu",
                HeaderText = "MONTANT REÇU ($)",
                DataPropertyName = "MontantRecu",
                Width = 150,
                ReadOnly = true
            });

            dgvMembers.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colStatutReception",
                HeaderText = "DÉJÀ REÇU ?",
                DataPropertyName = "StatutReception",
                Width = 110
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSignReception",
                HeaderText = "SIGNATURE (RÉCEPTION)",
                DataPropertyName = "SignatureReception",
                Width = 220
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colObs",
                HeaderText = "OBSERVATION",
                DataPropertyName = "Observation",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Tooltips (quand tu survoles l’entête)
            foreach (DataGridViewColumn col in dgvMembers.Columns)
                col.ToolTipText = GetMemberColumnHint(col.DataPropertyName);

            // Placeholder visuel pour guider l’écriture
            dgvMembers.CellFormatting -= DgvMembers_CellFormatting;
            dgvMembers.CellFormatting += DgvMembers_CellFormatting;

            dgvMembers.ResumeLayout();
        }

        private void FixPaymentsColumns()
        {
            if (dgvPayments == null) return;

            dgvPayments.SuspendLayout();

            dgvPayments.AutoGenerateColumns = false;
            dgvPayments.ColumnHeadersVisible = true;
            dgvPayments.RowHeadersVisible = false;

            // Purge totale pour éviter colonnes vides / sans titres
            dgvPayments.Columns.Clear();

            // Mois : ComboBox
            dgvPayments.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colMois",
                HeaderText = "MOIS",
                DataPropertyName = "Mois",
                Width = 140,
                DataSource = MonthsFr.ToList()
            });

            // Nom membre : ComboBox (liste membres) => STOCKE IdMembre, AFFICHE Nom
            dgvPayments.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colNomMembre",
                HeaderText = "NOM MEMBRE",
                DataPropertyName = "IdMembre",  // <-- IMPORTANT
                Width = 240,
                DataSource = _members,
                DisplayMember = "Nom",
                ValueMember = "IdMembre"
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMontantDu",
                HeaderText = "MONTANT DÛ ($)",
                DataPropertyName = "MontantDu",
                Width = 140
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMontantPaye",
                HeaderText = "MONTANT PAYÉ ($)",
                DataPropertyName = "MontantPaye",
                Width = 150
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "DATE PAIEMENT (jj/mm/aaaa)",
                DataPropertyName = "DatePaiement",
                Width = 190
            });

            // Mode paiement : ComboBox
            dgvPayments.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "colMode",
                HeaderText = "MODE",
                DataPropertyName = "ModePaiement",
                Width = 120,
                DataSource = PayModes.ToList()
            });

            dgvPayments.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colRetard",
                HeaderText = "RETARD ?",
                DataPropertyName = "Retard",
                Width = 90
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPenalite",
                HeaderText = "PÉNALITÉ ($)",
                DataPropertyName = "Penalite",
                Width = 120
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSignTreso",
                HeaderText = "SIGNATURE TRÉSORIER",
                DataPropertyName = "SignatureTresorier",
                Width = 190
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colObs",
                HeaderText = "OBSERVATION",
                DataPropertyName = "Observation",
                Width = 220
            });

            // ✅ PRO : colonnes calculées (affichage)
            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colReste",
                HeaderText = "RESTE ($)",
                DataPropertyName = "Reste",
                Width = 120,
                ReadOnly = true
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatut",
                HeaderText = "STATUT",
                DataPropertyName = "Statut",
                Width = 120,
                ReadOnly = true
            });

            // Tooltips sur entêtes
            foreach (DataGridViewColumn col in dgvPayments.Columns)
                col.ToolTipText = GetPaymentColumnHint(col.DataPropertyName);

            // Placeholder visuel
            dgvPayments.CellFormatting -= DgvPayments_CellFormatting;
            dgvPayments.CellFormatting += DgvPayments_CellFormatting;

            dgvPayments.ResumeLayout();
        }

        // ==========================================================
        // ✅ Placeholders visuels (guidage dans cellules vides)
        // ==========================================================
        private void DgvMembers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dgvMembers == null) return;

                var col = dgvMembers.Columns[e.ColumnIndex]?.Name ?? "";
                var value = e.Value?.ToString() ?? "";

                // Met une indication quand c’est vide
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (col == "colNom") { e.Value = "Ex: John B."; e.CellStyle.ForeColor = Color.Gray; }
                    else if (col == "colTel") { e.Value = "Ex: +243 9xx xxx xxx"; e.CellStyle.ForeColor = Color.Gray; }
                    else if (col == "colObs") { e.Value = "Note (optionnel)"; e.CellStyle.ForeColor = Color.Gray; }
                }
            }
            catch { }
        }

        private void DgvPayments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dgvPayments == null) return;

                var col = dgvPayments.Columns[e.ColumnIndex]?.Name ?? "";
                var value = e.Value?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(value))
                {
                    if (col == "colDate") { e.Value = "jj/mm/aaaa"; e.CellStyle.ForeColor = Color.Gray; }
                    else if (col == "colObs") { e.Value = "Ex: avance / retard / note"; e.CellStyle.ForeColor = Color.Gray; }
                    else if (col == "colSignTreso") { e.Value = "Nom + signature"; e.CellStyle.ForeColor = Color.Gray; }
                }
            }
            catch { }
        }


        // ==========================================================
        // Bind + Combos
        // ==========================================================
        private void BindData()
        {
            dgvMembers.DataSource = _members;
            dgvPayments.DataSource = _payments;
            FixAllDgvColumns();

            nudMontantMensuel.ValueChanged += (s, e) =>
            {
                foreach (var m in _members)
                    m.MontantMensuel = nudMontantMensuel.Value;
                dgvMembers.Refresh();
            };
        }

        private void RefreshCombos()
        {
            cboMemberAdvance.DataSource = null;
            cboMemberAdvance.DataSource = _members;
            cboMemberAdvance.DisplayMember = "Nom";
            cboMemberAdvance.ValueMember = "IdMembre";

            cboReceiptMember.DataSource = null;
            cboReceiptMember.DataSource = _members;
            cboReceiptMember.DisplayMember = "Nom";
            cboReceiptMember.ValueMember = "IdMembre";
        }

        private void UpdateStepUi()
        {
            btnPrev.Enabled = tabSteps.SelectedIndex > 0;
            btnNext.Text = (tabSteps.SelectedIndex == tabSteps.TabCount - 1) ? "Terminer" : "Suivant ▶";

            if (tabSteps.SelectedIndex == 0) lblStepTitle.Text = "TITRE : Informations générales + règles";
            if (tabSteps.SelectedIndex == 1) lblStepTitle.Text = "TITRE : Tableau principal (par membre)";
            if (tabSteps.SelectedIndex == 2) lblStepTitle.Text = "TITRE : Versement d’avance + signature remise";
            if (tabSteps.SelectedIndex == 3) lblStepTitle.Text = "TITRE : Suivi mensuel (très important) + PDF reçu";

            btnGeneratePdf.Enabled = _members.Count > 0;

            RefreshCombos();
            
        }

        private void InitLikelembaSearch()
        {
            if (_searchInitDone) return;
            _searchInitDone = true;

            // Autocomplete sur txtNomLikemba
            txtNomLikemba.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtNomLikemba.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtNomLikemba.AutoCompleteCustomSource = _likelembaAutoSource;

            // Timer unique
            _searchTimer = new Timer { Interval = 450 };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                TryAutoLoadLikelembaByName();
            };

            txtNomLikemba.TextChanged += (s, e) =>
            {
                if (_isLoadingLikelemba) return;
                _searchTimer.Stop();
                _searchTimer.Start();
            };

            txtNomLikemba.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    TryAutoLoadLikelembaByName(force: true);
                }
            };
        }

        private void LoadLikelembaNamesForAutocomplete()
        {
            _likelembaAutoSource.Clear();

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT NomLikemba 
FROM dbo.Likelemba 
WHERE Actif = 1
ORDER BY NomLikemba;", con))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var name = (rd["NomLikemba"]?.ToString() ?? "").Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                            _likelembaAutoSource.Add(name);
                    }
                }
            }
        }

        private void TryAutoLoadLikelembaByName(bool force = false)
        {
            // évite chargement si texte trop court pendant saisie
            string name = NormalizeInput(txtNomLikemba.Text);
            if (string.IsNullOrWhiteSpace(name)) return;

            // si pas force, tu peux exiger 4+ lettres pour éviter chargements agressifs
            if (!force && name.Length < 4) return;

            int? id = GetLikelembaIdByExactName(name);
            if (id == null) return;

            // ✅ Charge uniquement si ce n’est pas déjà le dossier courant
            if (_currentLikelembaId == id.Value) return;

            _isLoadingLikelemba = true;
            try
            {
                LoadLikelemba(id.Value);      // ta méthode existe déjà ✅
            }
            finally
            {
                _isLoadingLikelemba = false;
            }
        }

        private int? GetLikelembaIdByExactName(string name)
        {
            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdLikelemba
FROM dbo.Likelemba
WHERE Actif = 1 AND NomLikemba = @Nom
ORDER BY DateCreation DESC;", con))
                {
                    cmd.Parameters.AddWithValue("@Nom", name);
                    var obj = cmd.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value) return null;
                    return Convert.ToInt32(obj);
                }
            }
        }

        private void NewLikelemba()
        {
            CommitGridEdits();

            // ✅ Détecter s'il y a un dossier en cours (chargé ou commencé)
            bool hasCurrent =
                _currentLikelembaId != 0 ||
                !string.IsNullOrWhiteSpace(NormalizeInput(txtNomLikemba.Text)) ||
                !string.IsNullOrWhiteSpace(NormalizeInput(txtResponsable.Text)) ||
                _members.Count > 0 ||
                _payments.Count > 0;

            // ✅ Message pour éviter effacement par erreur
            if (hasCurrent)
            {
                var r = MessageBox.Show(
                    "⚠️ Un Likelemba est déjà ouvert.\n\n" +
                    "Si tu cliques OUI, TOUT sera effacé (membres, paiements, règles) " +
                    "et tu vas créer un NOUVEAU dossier.\n\n" +
                    "Veux-tu continuer ?",
                    "Confirmation - Nouveau Likelemba",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (r != DialogResult.Yes)
                    return;
            }

            // ✅ Anti-déclenchement auto-load pendant reset
            _isLoadingLikelemba = true;
            try
            {
                // 1) Nouveau dossier => ID = 0
                _currentLikelembaId = 0;

                // 2) Vider les données
                _members.Clear();
                _payments.Clear();

                // 3) Reset champs généraux
                txtNomLikemba.Text = "";
                txtResponsable.Text = "";
                dtStartCycle.Value = DateTime.Today;

                // valeurs par défaut (à ajuster si besoin)
                nudMontantMensuel.Value = 150;
                nudNbMembres.Value = 6;

                // 4) Reset règles
                chkRuleFixRules.Checked = false;
                chkRuleNotePayment.Checked = false;
                chkRuleSignRemise.Checked = false;
                chkRuleNoMixMoney.Checked = false;
                chkRuleDefaultRule.Checked = false;

                // 5) Reset controls Avance/Reçu (si présents)
                if (cboMonthAdvance != null && cboMonthAdvance.Items.Count > 0)
                    cboMonthAdvance.SelectedIndex = 0;

                if (nudAdvanceMonths != null)
                    nudAdvanceMonths.Value = 1;

                if (dtAdvancePayDate != null)
                    dtAdvancePayDate.Value = DateTime.Today;

                if (cboPayModeAdvance != null && cboPayModeAdvance.Items.Count > 0)
                    cboPayModeAdvance.SelectedIndex = 0;

                if (dtReceiptDate != null)
                    dtReceiptDate.Value = DateTime.Today;

                if (cboReceiptMonth != null && cboReceiptMonth.Items.Count > 0)
                    cboReceiptMonth.SelectedIndex = DateTime.Today.Month - 1;

                // 6) Rebind / colonnes / refresh
                RefreshCombos();
                FixAllDgvColumns();

                dgvMembers?.Refresh();
                dgvPayments?.Refresh();

                // 7) Revenir à l'étape 1
                if (tabSteps != null) tabSteps.SelectedIndex = 0;
                UpdateStepUi();

                // 8) Focus direct
                txtNomLikemba?.Focus();

                // ✅ Petit message OK (optionnel)
                // MessageBox.Show("Nouveau Likelemba prêt ✅", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                _isLoadingLikelemba = false;
            }
        }



        // ==========================================================
        // Validation Wizard
        // ==========================================================
        private bool CanGoNext()
        {
            // ✅ Toujours commit avant toute validation (double sécurité)
            CommitGridEdits();

            if (tabSteps.SelectedIndex == 0)
            {
                string nomLikemba = NormalizeInput(txtNomLikemba.Text);
                string resp = NormalizeInput(txtResponsable.Text);

                if (string.IsNullOrWhiteSpace(nomLikemba) || string.IsNullOrWhiteSpace(resp))
                {
                    MessageBox.Show("Complète les informations générales (Nom du Likemba, Responsable).",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!(chkRuleFixRules.Checked && chkRuleNotePayment.Checked && chkRuleSignRemise.Checked &&
                      chkRuleNoMixMoney.Checked && chkRuleDefaultRule.Checked))
                {
                    MessageBox.Show("Tu dois cocher toutes les règles avant de continuer.",
                        "Règles obligatoires", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (tabSteps.SelectedIndex == 1)
            {
                if (_members.Count < 2)
                {
                    MessageBox.Show("Ajoute au moins 2 membres dans le tableau principal.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // ✅ Trouver le 1er membre dont Nom est vide (message + focus cellule)
                var bad = _members
                    .Select((m, idx) => new { m, idx })
                    .FirstOrDefault(x => string.IsNullOrWhiteSpace((x.m.Nom ?? "").Trim()));

                if (bad != null)
                {
                    MessageBox.Show($"La colonne Nom ne peut pas être vide (ligne {bad.idx + 1}).",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    if (dgvMembers != null && dgvMembers.Rows.Count > bad.idx)
                    {
                        dgvMembers.ClearSelection();
                        dgvMembers.Rows[bad.idx].Selected = true;

                        // Colonne 1 = "Nom du membre" dans ton code
                        dgvMembers.CurrentCell = dgvMembers.Rows[bad.idx].Cells[1];
                        dgvMembers.BeginEdit(true);
                    }

                    return false;
                }
            }

            return true;
        }

        // ==========================================================
        // Membres
        // ==========================================================
        private void AddMember()
        {
            int next = _members.Count + 1;

            _members.Add(new MemberInfo
            {
                IdMembre = -next,               // ✅ ID temporaire (unique) avant sauvegarde DB
                Numero = next,
                Nom = "",
                Telephone = "",
                MontantMensuel = nudMontantMensuel.Value,
                OrdreReception = next,
                MoisReception = "",
                MontantRecu = 0,
                StatutReception = false,
                SignatureReception = "",
                Observation = ""
            });

            RefreshCombos();
            FixPaymentsColumns();
            dgvPayments?.Refresh();
        }

        private void RemovePaymentRow()
        {
            CommitGridEdits();

            if (dgvPayments == null || dgvPayments.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une ligne de paiement à supprimer.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvPayments.CurrentRow.DataBoundItem is PaymentRecord p)
            {
                _payments.Remove(p);
                dgvPayments.Refresh();
            }
        }

        private void RemoveMember()
        {
            if (dgvMembers.CurrentRow == null) return;
            if (!(dgvMembers.CurrentRow.DataBoundItem is MemberInfo m)) return;

            _members.Remove(m);

            for (int i = 0; i < _members.Count; i++)
            {
                _members[i].Numero = i + 1;
                if (_members[i].OrdreReception <= 0) _members[i].OrdreReception = i + 1;
            }
            dgvMembers.Refresh();
            RefreshCombos();
            FixPaymentsColumns();
        }

        private void AutoAssignReceptionMonths()
        {
            if (_members.Count == 0) return;

            var ordered = _members.OrderBy(m => m.OrdreReception).ToList();
            int startIndex = dtStartCycle.Value.Month - 1;

            for (int i = 0; i < ordered.Count; i++)
            {
                int monthIndex = (startIndex + i) % 12;
                ordered[i].MoisReception = MonthsFr[monthIndex];
                ordered[i].MontantRecu = nudMontantMensuel.Value * _members.Count;
            }
            dgvMembers.Refresh();
        }

        // ==========================================================
        // Paiements
        // ==========================================================
        private void AutoBuildPaymentsForCycle()
        {
            CommitGridEdits();

            if (_members.Count < 2)
            {
                MessageBox.Show("Ajoute au moins 2 membres.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifier noms
            if (_members.Any(m => string.IsNullOrWhiteSpace((m.Nom ?? "").Trim())))
            {
                MessageBox.Show("Chaque membre doit avoir un nom avant de générer le suivi.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _payments.Clear();

            int monthsCount = _members.Count; // 1 cycle = nb membres = nb mois
            int startMonthIndex = dtStartCycle.Value.Month - 1;
            int year = dtStartCycle.Value.Year;

            for (int m = 0; m < monthsCount; m++)
            {
                string mois = MonthsFr[(startMonthIndex + m) % 12];

                foreach (var mem in _members)
                {
                    if (mem.IdMembre == 0) continue; // 0 seulement = invalide

                    AddOrUpdatePayment(new PaymentRecord
                    {
                        Mois = mois,
                        IdMembre = mem.IdMembre,           // peut être négatif => OK
                        MontantDu = nudMontantMensuel.Value,
                        MontantPaye = 0,
                        DatePaiement = "",
                        ModePaiement = "Cash",
                        Retard = false,
                        Penalite = 0,
                        SignatureTresorier = "",
                        Observation = "",
                        Annee = year,
                        AnneeKey = 0 // on ignore
                    });
                }
            }

            dgvPayments.Refresh();
            MessageBox.Show("Suivi du cycle généré ✅", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddPaymentRow()
        {
            CommitGridEdits();

            if (_members.Count == 0)
            {
                MessageBox.Show("Ajoute d’abord des membres.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var mem = _members[0];
            if (mem.IdMembre == 0)
            {
                MessageBox.Show("Membre invalide (ID).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string mois = MonthsFr[DateTime.Today.Month - 1];

            AddOrUpdatePayment(new PaymentRecord
            {
                Mois = mois,
                IdMembre = mem.IdMembre,
                MontantDu = nudMontantMensuel.Value,
                MontantPaye = 0,
                DatePaiement = DateTime.Today.ToString("dd/MM/yyyy"),
                ModePaiement = "Cash",
                Retard = false,
                Penalite = 0,
                SignatureTresorier = "",
                Observation = "",
                Annee = DateTime.Today.Year,
                AnneeKey = DateTime.Today.Year
            }, showInfo: true);

            dgvPayments.Refresh();
        }


        // ==========================================================
        // Avance
        // ==========================================================
        private void AddAdvancePayments()
        {
            CommitGridEdits();

            if (_members.Count < 1)
            {
                MessageBox.Show("Ajoute des membres d’abord.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var mem = cboMemberAdvance.SelectedItem as MemberInfo;
            if (mem == null || mem.IdMembre == 0 || string.IsNullOrWhiteSpace((mem.Nom ?? "").Trim()))
            {
                MessageBox.Show("Sélectionne un membre valide (Nom + ID obligatoire).", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string moisDepart = NormalizeMonth(cboMonthAdvance.Text);
            if (!IsValidMonth(moisDepart))
            {
                MessageBox.Show("Sélectionne un mois de départ valide.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mode = (cboPayModeAdvance.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(mode)) mode = "Cash";

            string sign = CleanText(txtSignatureRemise, "Nom + signature (ex: Jean K.)");
            if (string.IsNullOrWhiteSpace(sign))
            {
                MessageBox.Show("La signature lors de la remise est obligatoire.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int monthsCount = (int)nudAdvanceMonths.Value;
            int startIndex = MonthIndex(moisDepart);

            for (int i = 0; i < monthsCount; i++)
            {
                string mois = MonthsFr[(startIndex + i) % 12];

                AddOrUpdatePayment(new PaymentRecord
                {
                    Mois = mois,
                    IdMembre = mem.IdMembre,
                    MontantDu = nudMontantMensuel.Value,
                    MontantPaye = nudMontantMensuel.Value,
                    DatePaiement = dtAdvancePayDate.Value.ToString("dd/MM/yyyy"),
                    ModePaiement = mode,
                    Retard = false,
                    Penalite = 0,
                    SignatureTresorier = sign,
                    Observation = "[AVANCE] " + CleanText(txtObsAdvance, "Observation (optionnel)"),
                    Annee = dtStartCycle.Value.Year,
                    AnneeKey = dtStartCycle.Value.Year
                });
            }

            dgvPayments.Refresh();
            MessageBox.Show("Versement d’avance enregistré (anti-doublon activé).", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==========================================================
        // PDF Reçu
        // ==========================================================
        private void GenerateReceiptPdf()
        {
            if (_members.Count == 0)
            {
                MessageBox.Show("Ajoute des membres d’abord.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string likembaName = NormalizeInput(txtNomLikemba.Text);
            string periode = dtStartCycle.Value.ToString("dd/MM/yyyy");
            string responsable = NormalizeInput(txtResponsable.Text);

            if (string.IsNullOrWhiteSpace(likembaName) || string.IsNullOrWhiteSpace(responsable))
            {
                MessageBox.Show("Complète d’abord les informations générales (Nom du Likemba, Responsable).",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var mem = cboReceiptMember.SelectedItem as MemberInfo;
            if (mem == null || mem.IdMembre == 0 || string.IsNullOrWhiteSpace(mem.Nom))
            {
                MessageBox.Show("Sélectionne un membre pour le reçu.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mois = cboReceiptMonth.Text;
            decimal montant = nudMontantMensuel.Value;

            var pay = _payments
                .Where(p => p.IdMembre == mem.IdMembre
                         && string.Equals(NormalizeMonth(p.Mois), NormalizeMonth(mois), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.DatePaiement)
                .FirstOrDefault();

            string mode = string.IsNullOrWhiteSpace(pay?.ModePaiement) ? "Cash" : pay.ModePaiement;
            string date = !string.IsNullOrWhiteSpace(pay?.DatePaiement) ? pay.DatePaiement : dtReceiptDate.Value.ToString("dd/MM/yyyy");

            decimal montantRecu = (pay != null && pay.MontantPaye > 0) ? pay.MontantPaye : montant;

            string tresReal = CleanText(txtReceiptTreasurerSign, "Nom du trésorier");
            string respReal = CleanText(txtReceiptResponsibleSign, "Nom du responsable Likelemba");

            string tresSign = string.IsNullOrWhiteSpace(tresReal) ? "________________" : tresReal;
            string respSign = string.IsNullOrWhiteSpace(respReal) ? responsable : respReal;

            using (var sfd = new SaveFileDialog
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Recu_Likelemba_{mem.Nom}_{mois}_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    CreateReceiptPdf(
                        filePath: sfd.FileName,
                        likembaName: likembaName,
                        periode: periode,
                        responsable: responsable,
                        membreNom: mem.Nom,
                        membreTel: mem.Telephone,
                        mois: mois,
                        montant: montantRecu,
                        datePaiement: date,
                        modePaiement: mode,
                        signatureTresorier: tresSign,
                        signatureResponsable: respSign
                    );

                    if (_currentLikelembaId == 0)
                    {
                        MessageBox.Show("Sauvegarde d’abord le dossier Likelemba (Terminer) avant d'émettre un reçu.",
                            "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int annee = dtStartCycle.Value.Year; // ou pay?.Annee si tu veux

                    SaveReceiptToDb(
                        idLikelemba: _currentLikelembaId,
                        idMembre: mem.IdMembre,
                        mois: mois,
                        annee: annee,
                        montant: montantRecu,
                        dateRecu: date,
                        modePaiement: mode,
                        signatureMembre: "",                 // si tu le captes plus tard, mets-le ici
                        signatureTresorier: tresSign,
                        signatureResponsable: respSign,
                        cheminPdf: sfd.FileName
                    );

                    MessageBox.Show("PDF généré avec succès.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur PDF : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateReceiptPdf(
    string filePath,
    string likembaName,
    string periode,
    string responsable,
    string membreNom,
    string membreTel,
    string mois,
    decimal montant,
    string datePaiement,
    string modePaiement,
    string signatureTresorier,
    string signatureResponsable)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Reçu Likelemba";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                double left = 55;
                double right = page.Width.Point - 55;
                double y = 55;

                // ✅ Polices simples (Arial)
                var title = new XFont("Arial", 14, XFontStyleEx.Bold);
                var bold = new XFont("Arial", 11, XFontStyleEx.Bold);
                var font = new XFont("Arial", 11, XFontStyleEx.Regular);

                // En-tête
                gfx.DrawString("REÇU DE COTISATION - LIKELEMBA", title, XBrushes.Black,
                    new XRect(left, y, right - left, 20), XStringFormats.TopCenter);
                y += 28;

                gfx.DrawLine(XPens.Black, left, y, right, y);
                y += 18;

                // Infos essentielles
                gfx.DrawString("Likemba :", bold, XBrushes.Black, left, y);
                gfx.DrawString(likembaName ?? "", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Cycle :", bold, XBrushes.Black, left, y);
                gfx.DrawString(periode ?? "", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Mois :", bold, XBrushes.Black, left, y);
                gfx.DrawString(mois ?? "", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Montant :", bold, XBrushes.Black, left, y);
                gfx.DrawString($"{FormatMoney(montant)} $", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Date :", bold, XBrushes.Black, left, y);
                gfx.DrawString(datePaiement ?? "", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Mode :", bold, XBrushes.Black, left, y);
                gfx.DrawString(modePaiement ?? "Cash", font, XBrushes.Black, left + 120, y);
                y += 26;

                gfx.DrawLine(XPens.Gray, left, y, right, y);
                y += 16;

                // Membre
                gfx.DrawString("Membre :", bold, XBrushes.Black, left, y);
                gfx.DrawString(membreNom ?? "", font, XBrushes.Black, left + 120, y);
                y += 20;

                gfx.DrawString("Téléphone :", bold, XBrushes.Black, left, y);
                gfx.DrawString(string.IsNullOrWhiteSpace(membreTel) ? "—" : membreTel, font, XBrushes.Black, left + 120, y);
                y += 30;

                // Signatures simples
                gfx.DrawString("Signature membre :", bold, XBrushes.Black, left, y);
                gfx.DrawLine(XPens.Black, left + 160, y + 12, right, y + 12);
                y += 28;

                gfx.DrawString("Trésorier :", bold, XBrushes.Black, left, y);
                gfx.DrawString(signatureTresorier ?? "________________", font, XBrushes.Black, left + 160, y);
                y += 22;

                gfx.DrawString("Responsable :", bold, XBrushes.Black, left, y);
                gfx.DrawString(string.IsNullOrWhiteSpace(signatureResponsable) ? responsable : signatureResponsable,
                    font, XBrushes.Black, left + 160, y);
                y += 30;

                // Petit rappel
                gfx.DrawLine(XPens.Gray, left, y, right, y);
                y += 14;
                gfx.DrawString("Rappel : noter chaque paiement et faire signer le reçu.", font, XBrushes.Black,
                    new XRect(left, y, right - left, 20), XStringFormats.TopLeft);
            }

            doc.Save(filePath);
        }

        private void DrawCentered(XGraphics gfx, string text, XFont font, ref double y)
        {
            double w = gfx.PageSize.Width; // ici c’est déjà un double chez toi
            gfx.DrawString(text, font, XBrushes.Black, new XRect(0, y, w, 18), XStringFormats.TopCenter);
            y += 18;
        }

        private void DrawPair(XGraphics gfx, string label, string value, XFont fontLabel, XFont fontValue, ref double y)
        {
            double w = gfx.PageSize.Width; // double
            gfx.DrawString(label, fontLabel, XBrushes.Black, new XRect(60, y, 200, 18), XStringFormats.TopLeft);
            gfx.DrawString(value ?? "", fontValue, XBrushes.Black, new XRect(260, y, w - 320, 18), XStringFormats.TopLeft);
            y += 20;
        }

        private string FormatMoney(decimal v) => v.ToString("N0", CultureInfo.InvariantCulture);

        // ==========================================================
        // Models
        // ==========================================================
        public class MemberInfo : INotifyPropertyChanged
        {
            private int _idMembre;
            private int _numero;
            private string _nom;
            private string _telephone;
            private decimal _montantMensuel;
            private int _ordreReception;
            private string _moisReception;
            private decimal _montantRecu;
            private bool _statutReception;
            private string _signatureReception;
            private string _observation;

            public int IdMembre { get => _idMembre; set { _idMembre = value; OnChanged(nameof(IdMembre)); } }

            public int Numero { get => _numero; set { _numero = value; OnChanged(nameof(Numero)); } }
            public string Nom { get => _nom; set { _nom = value; OnChanged(nameof(Nom)); } }
            public string Telephone { get => _telephone; set { _telephone = value; OnChanged(nameof(Telephone)); } }
            public decimal MontantMensuel { get => _montantMensuel; set { _montantMensuel = value; OnChanged(nameof(MontantMensuel)); } }
            public int OrdreReception { get => _ordreReception; set { _ordreReception = value; OnChanged(nameof(OrdreReception)); } }
            public string MoisReception { get => _moisReception; set { _moisReception = value; OnChanged(nameof(MoisReception)); } }
            public decimal MontantRecu { get => _montantRecu; set { _montantRecu = value; OnChanged(nameof(MontantRecu)); } }
            public bool StatutReception { get => _statutReception; set { _statutReception = value; OnChanged(nameof(StatutReception)); } }
            public string SignatureReception { get => _signatureReception; set { _signatureReception = value; OnChanged(nameof(SignatureReception)); } }
            public string Observation { get => _observation; set { _observation = value; OnChanged(nameof(Observation)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ✅ Une seule initialisation globale
        private static bool _fontResolverReady = false;

        // ✅ Resolver minimal (pas une classe utilitaire séparée, juste interne au Form)
        private sealed class MinimalFontResolver : IFontResolver
        {
            public string DefaultFontName => "Arial";

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                // On force tout vers Arial (stable)
                if (isBold && isItalic) return new FontResolverInfo("arialbi");
                if (isBold) return new FontResolverInfo("arialbd");
                if (isItalic) return new FontResolverInfo("ariali");
                return new FontResolverInfo("arial");
            }

            public byte[] GetFont(string faceName)
            {
                string name = (faceName ?? "").ToLowerInvariant();
                string path;

                if (name == "arial") path = @"C:\Windows\Fonts\arial.ttf";
                else if (name == "arialbd") path = @"C:\Windows\Fonts\arialbd.ttf";
                else if (name == "ariali") path = @"C:\Windows\Fonts\ariali.ttf";
                else if (name == "arialbi") path = @"C:\Windows\Fonts\arialbi.ttf";
                else path = @"C:\Windows\Fonts\arial.ttf";

                if (!File.Exists(path))
                    throw new FileNotFoundException("Police introuvable: " + path);

                return File.ReadAllBytes(path);
            }
        }

        private PaymentRecord AddOrUpdatePayment(PaymentRecord incoming, bool showInfo = false)
        {
            if (incoming == null) return null;

            incoming.Mois = NormalizeMonth(incoming.Mois);

            // Obligatoire : IdMembre + Mois
            if (incoming.IdMembre == 0 || !IsValidMonth(incoming.Mois))
                return null;

            // Année par défaut
            if (incoming.Annee <= 0) incoming.Annee = dtStartCycle.Value.Year;

            var existing = _payments.FirstOrDefault(p =>
                p.IdMembre == incoming.IdMembre &&
                string.Equals(NormalizeMonth(p.Mois), incoming.Mois, StringComparison.OrdinalIgnoreCase) &&
                (p.Annee <= 0 ? dtStartCycle.Value.Year : p.Annee) == incoming.Annee
            );

            if (existing != null)
            {
                existing.MontantDu = incoming.MontantDu;
                existing.MontantPaye = incoming.MontantPaye;
                existing.DatePaiement = incoming.DatePaiement;
                existing.ModePaiement = incoming.ModePaiement;
                existing.Retard = incoming.Retard;
                existing.Penalite = incoming.Penalite;
                existing.SignatureTresorier = incoming.SignatureTresorier;
                existing.Observation = incoming.Observation;

                if (showInfo)
                    MessageBox.Show("Paiement mis à jour (ligne existante).", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                return existing;
            }

            _payments.Add(incoming);

            if (showInfo)
                MessageBox.Show("Paiement ajouté.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            return incoming;
        }

        private void SaveReceiptToDb(
    int idLikelemba,
    int idMembre,
    string mois,
    int annee,
    decimal montant,
    string dateRecu,
    string modePaiement,
    string signatureMembre,
    string signatureTresorier,
    string signatureResponsable,
    string cheminPdf)
        {
            byte[] pdfBytes = null;

            // si tu veux stocker le PDF en binaire
            if (!string.IsNullOrWhiteSpace(cheminPdf) && File.Exists(cheminPdf))
                pdfBytes = File.ReadAllBytes(cheminPdf);

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var tr = con.BeginTransaction())
                {
                    try
                    {
                        // 1) INSERT RECU (historique)
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.LikelembaRecu
(IdLikelemba, IdMembre, Mois, Annee, MontantUSD, DateRecu, ModePaiement,
 SignatureMembre, SignatureTresorier, SignatureResponsable,
 CheminFichierPdf, PdfBinary, DateCreation, AnneeKey)
VALUES
(@IdL, @IdM, @Mois, @Annee, @Montant, @DateRecu, @Mode,
 @SignMembre, @SignTreso, @SignResp,
 @Chemin, @Pdf, GETDATE(), @AnneeKey);", con, tr))
                        {
                            cmd.Parameters.AddWithValue("@IdL", idLikelemba);
                            cmd.Parameters.AddWithValue("@IdM", idMembre);
                            cmd.Parameters.AddWithValue("@Mois", NormalizeMonth(mois));
                            cmd.Parameters.AddWithValue("@Annee", annee);
                            cmd.Parameters.AddWithValue("@Montant", montant);
                            cmd.Parameters.AddWithValue("@DateRecu", (object)(dateRecu ?? "") ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Mode", (object)(modePaiement ?? "") ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@SignMembre", (object)(signatureMembre ?? "") ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@SignTreso", (object)(signatureTresorier ?? "") ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@SignResp", (object)(signatureResponsable ?? "") ?? DBNull.Value);

                            cmd.Parameters.AddWithValue("@Chemin", (object)(cheminPdf ?? "") ?? DBNull.Value);
                            cmd.Parameters.Add("@Pdf", SqlDbType.VarBinary).Value = (object)pdfBytes ?? DBNull.Value;

                            cmd.Parameters.AddWithValue("@AnneeKey", annee);
                            cmd.ExecuteNonQuery();
                        }

                        // 2) OPTIONNEL : marquer paiement comme payé (validation)
                        using (var cmdUp = new SqlCommand(@"
UPDATE dbo.LikelembaPaiement
SET MontantPayeUSD = @Montant,
    DatePaiement    = @DateRecu,
    ModePaiement    = @Mode,
    SignatureTresorier = @SignTreso
WHERE IdLikelemba=@IdL AND IdMembre=@IdM AND Mois=@Mois AND Annee=@Annee;

IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dbo.LikelembaPaiement
    (IdLikelemba, IdMembre, Mois, Annee, MontantDuUSD, MontantPayeUSD, DatePaiement, ModePaiement,
     Retard, PenaliteUSD, SignatureTresorier, Observation, DateCreation, AnneeKey)
    VALUES
    (@IdL, @IdM, @Mois, @Annee, @Montant, @Montant, @DateRecu, @Mode,
     0, 0, @SignTreso, '[AUTO-RECU]', GETDATE(), @AnneeKey);
END", con, tr))
                        {
                            cmdUp.Parameters.AddWithValue("@IdL", idLikelemba);
                            cmdUp.Parameters.AddWithValue("@IdM", idMembre);
                            cmdUp.Parameters.AddWithValue("@Mois", NormalizeMonth(mois));
                            cmdUp.Parameters.AddWithValue("@Annee", annee);
                            cmdUp.Parameters.AddWithValue("@Montant", montant);
                            cmdUp.Parameters.AddWithValue("@DateRecu", (object)(dateRecu ?? "") ?? DBNull.Value);
                            cmdUp.Parameters.AddWithValue("@Mode", (object)(modePaiement ?? "") ?? DBNull.Value);
                            cmdUp.Parameters.AddWithValue("@SignTreso", (object)(signatureTresorier ?? "") ?? DBNull.Value);
                            cmdUp.Parameters.AddWithValue("@AnneeKey", annee);
                            cmdUp.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }


        public class PaymentRecord : INotifyPropertyChanged
        {
            private string _mois;
            private string _nomMembre;
            private decimal _montantDu;
            private decimal _montantPaye;
            private string _datePaiement;
            private string _modePaiement;
            private bool _retard;
            private decimal _penalite;
            private string _signatureTresorier;
            private string _observation;


            public string Mois { get => _mois; set { _mois = value; OnChanged(nameof(Mois)); } }
            public string NomMembre { get => _nomMembre; set { _nomMembre = value; OnChanged(nameof(NomMembre)); } }

            public decimal MontantDu { get => _montantDu; set { _montantDu = value; OnChanged(nameof(MontantDu)); OnChanged(nameof(Reste)); OnChanged(nameof(Statut)); } }
            public decimal MontantPaye { get => _montantPaye; set { _montantPaye = value; OnChanged(nameof(MontantPaye)); OnChanged(nameof(Reste)); OnChanged(nameof(Statut)); } }

            public string DatePaiement { get => _datePaiement; set { _datePaiement = value; OnChanged(nameof(DatePaiement)); } }
            public string ModePaiement { get => _modePaiement; set { _modePaiement = value; OnChanged(nameof(ModePaiement)); } }

            public bool Retard { get => _retard; set { _retard = value; OnChanged(nameof(Retard)); } }
            public decimal Penalite { get => _penalite; set { _penalite = value; OnChanged(nameof(Penalite)); } }

            public string SignatureTresorier { get => _signatureTresorier; set { _signatureTresorier = value; OnChanged(nameof(SignatureTresorier)); } }
            public string Observation { get => _observation; set { _observation = value; OnChanged(nameof(Observation)); } }
            private int _idMembre;
            public int IdMembre { get => _idMembre; set { _idMembre = value; OnChanged(nameof(IdMembre)); } }

            public int Annee { get; set; } = DateTime.Today.Year;
            public int AnneeKey { get; set; } = DateTime.Today.Year; // simple

            // ✅ PRO
            public decimal Reste => Math.Max(0, MontantDu - MontantPaye);
            public string Statut
            {
                get
                {
                    if (MontantPaye <= 0) return "NON PAYÉ";
                    if (MontantPaye >= MontantDu) return "PAYÉ";
                    return "PARTIEL";
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
