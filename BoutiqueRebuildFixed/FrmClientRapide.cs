using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BoutiqueRebuildFixed.FormVentes;

namespace BoutiqueRebuildFixed
{
    public class FormClientsRapide : Form
    {
        // ===================== MODEL MINIMAL =====================
        public class ClientMin
        {
            public int ID { get; set; }
            public string Nom { get; set; }
            public string Telephone { get; set; }
            public string CategorieClient { get; set; } = "OCCASIONNEL";
            public override string ToString() => $"{Nom} ({Telephone})";
        }

        private ComboBox cmbCategorieClient;

        public string CategorieClientChoisie { get; private set; } = "OCCASIONNEL";

        // ===================== DEPENDANCES =====================
        private readonly string _cs;
        private readonly Action _finaliserAction;
        private readonly Action<FormClientsRapide> _injectToFormVentes;
        private readonly decimal _netAPayer;
        private readonly string _deviseVente;
        private readonly string _codeFactureEnCours;
        private readonly List<OrdonnanceLigneDTO> _lignesOrdonnance = new List<OrdonnanceLigneDTO>();
        public List<FormPaiementsVente.PaiementLine> PaiementsChoisis { get; private set; } = new List<FormPaiementsVente.PaiementLine>();
        public OrdonnanceVenteDTO OrdonnanceChoisie { get; private set; } = null;

        public void SetPaiements(List<FormPaiementsVente.PaiementLine> list)
        {
            PaiementsChoisis = list ?? new List<FormPaiementsVente.PaiementLine>();
        }

        // ===================== 1) FIX SetOrdonnance (Null-safe + copie propre) =====================
        public void SetOrdonnance(OrdonnanceVenteDTO ord)
        {
            if (ord == null) return;

            // ✅ IMPORTANT : créer l'objet si null
            if (OrdonnanceChoisie == null)
                OrdonnanceChoisie = new OrdonnanceVenteDTO();

            // ✅ copier champs
            OrdonnanceChoisie.Numero = ord.Numero;
            OrdonnanceChoisie.Prescripteur = ord.Prescripteur;
            OrdonnanceChoisie.Patient = ord.Patient;
            OrdonnanceChoisie.DateOrdonnance = ord.DateOrdonnance;
            OrdonnanceChoisie.Note = ord.Note;
            OrdonnanceChoisie.ScanPath = ord.ScanPath;
            OrdonnanceChoisie.PdfPath = ord.PdfPath;
            OrdonnanceChoisie.CodeFacture = ord.CodeFacture;
            OrdonnanceChoisie.CodeCarteClient = ord.CodeCarteClient;

            // ✅ copier lignes
            if (OrdonnanceChoisie.Lignes == null)
                OrdonnanceChoisie.Lignes = new List<OrdonnanceLigneDTO>();

            OrdonnanceChoisie.Lignes.Clear();
            if (ord.Lignes != null && ord.Lignes.Count > 0)
                OrdonnanceChoisie.Lignes.AddRange(ord.Lignes);
        }


        // ===================== DATA =====================
        private readonly List<ClientMin> _allClients = new List<ClientMin>();

        // ===================== UI =====================
        private ComboBox cmbNomClient;
        private TextBox txtTelephone;
        private TextBox txtCoupon;
        private CheckBox chkCredit;
        private DateTimePicker dtpEcheance;
        private ComboBox cboEmplacement;

        private Button btnOk;
        private Button btnCancel;
        private Button btnPaiements;
        private Button btnOrdonnance;
        private Button btnOuvrirPdfOrdonnance;
        private string _ordNumero = "";
        private string _ordPrescripteur = "SYSTEM";
        private string _ordPatient = "";
        private Button btnParamFidelite;
        private Button btnTauxChange;

        // ===================== VALEURS (lus par FormVentes) =====================
        public string NomClientChoisi { get; private set; } = "";
        public string TelephoneChoisi { get; private set; } = "";
        public string CouponChoisi { get; private set; } = "";
        public bool VenteCreditChoisi { get; private set; } = false;
        public DateTime EcheanceChoisie { get; private set; } = DateTime.Today;
        public string EmplacementChoisi { get; private set; } = "";



        public List<OrdonnanceLigneDTO> LignesPanier { get; set; } = new List<OrdonnanceLigneDTO>();

        // ===================== CTOR =====================
        public FormClientsRapide(
    string connectionString,
    string couponInitial,
    bool creditInitial,
    DateTime echeanceInitial,
    ComboBox.ObjectCollection emplacementsSource,
    string emplacementInitial,
    Action<FormClientsRapide> injectToFormVentes,
    Action finaliserAction,
    decimal netAPayer,
    string deviseVente,
    string codeFactureEnCours
)
        {
            _cs = connectionString;
            _injectToFormVentes = injectToFormVentes ?? throw new ArgumentNullException(nameof(injectToFormVentes));
            _finaliserAction = finaliserAction ?? throw new ArgumentNullException(nameof(finaliserAction));

            _netAPayer = netAPayer;
            _deviseVente = string.IsNullOrWhiteSpace(deviseVente) ? "CDF" : deviseVente.Trim().ToUpperInvariant();

            _ordPrescripteur = string.IsNullOrWhiteSpace(SessionEmploye.Nom)
    ? "SYSTEM"
    : (SessionEmploye.Nom + " " + (SessionEmploye.Prenom ?? "")).Trim();

            _codeFactureEnCours = (codeFactureEnCours ?? "").Trim();
            _ordNumero = string.IsNullOrWhiteSpace(_codeFactureEnCours)
    ? ""
    : _codeFactureEnCours;
            // ✅ ici on laisse vide (FormVentes pourra l’alimenter si tu veux plus tard)
            _lignesOrdonnance.Clear();

            BuildUI();

            // Prefill depuis FormVentes
            txtCoupon.Text = couponInitial ?? "";
            chkCredit.Checked = creditInitial;
            dtpEcheance.Value = (echeanceInitial == default) ? DateTime.Today.AddDays(30) : echeanceInitial.Date;

            InitEmplacements(emplacementsSource, emplacementInitial);
            ToggleCreditUI();

            Shown += async (s, e) =>
            {
                await ChargerClientsDepuisDbAsync();
                RafraichirAutoComplete();
                cmbNomClient.Focus();
            };
        }

        // ===================== UI BUILD =====================
        private void BuildUI()
        {
            Text = "Client rapide";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;
            Font = new Font("Segoe UI", 10f);
            Width = 660;
            Height = 460;

            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(660, 460);
            AutoScroll = false; // inutile si la mise en page est correcte

            var lblNom = new Label { Text = "Nom client", AutoSize = true, Left = 20, Top = 20 };

            cmbNomClient = new ComboBox
            {
                Left = 20,
                Top = 45,
                Width = 560,
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cmbNomClient.SelectedIndexChanged += cmbNomClient_SelectedIndexChanged;
            cmbNomClient.KeyDown += cmbNomClient_KeyDown;
            cmbNomClient.Leave += cmbNomClient_Leave;

            var lblTel = new Label { Text = "Téléphone", AutoSize = true, Left = 20, Top = 82 };
            txtTelephone = new TextBox { Left = 20, Top = 107, Width = 260 };
            txtTelephone.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnOk.PerformClick();
                }
            };

            // --- Catégorie (sous téléphone)
            var lblCat = new Label { Text = "Catégorie client", AutoSize = true, Left = 20, Top = 140 };

            cmbCategorieClient = new ComboBox
            {
                Left = 20,
                Top = 165,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategorieClient.Items.Clear();
            cmbCategorieClient.Items.AddRange(new object[] { "OCCASIONNEL", "FIDELE", "VIP", "ENTREPRISE" });
            cmbCategorieClient.SelectedItem = "OCCASIONNEL";

            var lblCoupon = new Label { Text = "Coupon (optionnel)", AutoSize = true, Left = 320, Top = 82 };
            txtCoupon = new TextBox { Left = 320, Top = 107, Width = 260 };

            chkCredit = new CheckBox { Text = "Vente à crédit", Left = 320, Top = 150, AutoSize = true };
            chkCredit.CheckedChanged += (s, e) => ToggleCreditUI();

            var lblEch = new Label { Text = "Échéance crédit", AutoSize = true, Left = 320, Top = 182 };
            dtpEcheance = new DateTimePicker
            {
                Left = 320,
                Top = 207,
                Width = 260,
                Format = DateTimePickerFormat.Short
            };

            var lblEmp = new Label { Text = "Emplacement", AutoSize = true, Left = 20, Top = 245 };
            cboEmplacement = new ComboBox
            {
                Left = 20,
                Top = 270,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // ✅ Boutons (1 seule fois)
            btnPaiements = new Button { Text = "Paiements (optionnel)", Left = 320, Top = 245, Width = 260, Height = 34 };
            btnOrdonnance = new Button { Text = "Ordonnance (PDF)", Left = 320, Top = 285, Width = 160, Height = 34 };
            btnOuvrirPdfOrdonnance = new Button { Text = "Ouvrir PDF", Left = 470, Top = 285, Width = 110, Height = 34, Enabled = false };

            btnOk = new Button { Text = "OK = Finaliser (Enter)", Left = 345, Top = 325, Width = 155, Height = 34 };
            btnCancel = new Button { Text = "Annuler (Esc)", Left = 510, Top = 325, Width = 90, Height = 34 };

            btnParamFidelite = new Button { Text = "Paramètres fidélité", Left = 20, Top = 325, Width = 170, Height = 34 };
            btnParamFidelite.Click += btnParamFidelite_Click;
            Controls.Add(btnParamFidelite);

            btnTauxChange = new Button { Text = "Taux de change", Left = 200, Top = 325, Width = 140, Height = 34 };
            btnTauxChange.Click += (s, e) =>
            {
                // ✅ blocage : manager obligatoire
                if (!RequireManagerApproval(
                    typeAction: "TAUX_CHANGE_EDIT",
                    permissionCode: "btnTauxChange",
                    details: "Modification du taux de change"))
                    return;

                try
                {
                    using (var f = new FrmTauxChange(_cs))
                        f.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur taux de change : " + ex.Message);
                }
            };

            Controls.Add(btnTauxChange);

            btnPaiements.Click += btnPaiements_Click;
            btnOrdonnance.Click += btnOrdonnance_Click;
            btnOuvrirPdfOrdonnance.Click += btnOuvrirPdfOrdonnance_Click;

            btnOk.Click += async (s, e) => await Ok_FinaliserAsync();
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[]
{
    lblNom, cmbNomClient,
    lblTel, txtTelephone,
    lblCoupon, txtCoupon,

    lblCat, cmbCategorieClient,   // ✅ AJOUT

    chkCredit,
    lblEch, dtpEcheance,
    lblEmp, cboEmplacement,

    btnPaiements, btnOrdonnance, btnOuvrirPdfOrdonnance,
    btnOk, btnCancel
});

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
        }


        private void btnParamFidelite_Click(object sender, EventArgs e)
        {
            // ✅ blocage : manager obligatoire
            if (!RequireManagerApproval(
                typeAction: "LOYALTY_RULES_EDIT",
                permissionCode: "btnParamFidelite",
                details: "Modification des règles de fidélité"))
                return;

            try
            {
                using (var f = new FrmParametresFidelite(_cs))
                    f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur paramètres fidélité : " + ex.Message);
            }
        }

        public void ChargerContexteOrdonnance(
    List<OrdonnanceLigneDTO> lignes,
    string numeroOrd,
    string prescripteur,
    string patient
)
        {
            _lignesOrdonnance.Clear();
            if (lignes != null) _lignesOrdonnance.AddRange(lignes);

            // ✅ si numeroOrd vide => reconstruire depuis _codeFactureEnCours
            string n = (numeroOrd ?? "").Trim();

            if (string.IsNullOrWhiteSpace(n))
            {
                if (!string.IsNullOrWhiteSpace(_codeFactureEnCours))
                    n = _codeFactureEnCours;
                else
                    n = $"TMP-{DateTime.Now:yyyyMMdd-HHmmss}";
            }

            _ordNumero = n;

            _ordPrescripteur = string.IsNullOrWhiteSpace(prescripteur) ? "SYSTEM" : prescripteur.Trim();
            _ordPatient = (patient ?? "").Trim();

            if (OrdonnanceChoisie != null)
            {
                if (string.IsNullOrWhiteSpace(OrdonnanceChoisie.Numero)) OrdonnanceChoisie.Numero = _ordNumero;
                if (string.IsNullOrWhiteSpace(OrdonnanceChoisie.Prescripteur)) OrdonnanceChoisie.Prescripteur = _ordPrescripteur;
                if (string.IsNullOrWhiteSpace(OrdonnanceChoisie.Patient)) OrdonnanceChoisie.Patient = _ordPatient;
            }
        }

        private void ToggleCreditUI() => dtpEcheance.Enabled = chkCredit.Checked;

        private void InitEmplacements(ComboBox.ObjectCollection emplacementsSource, string emplacementInitial)
        {
            cboEmplacement.Items.Clear();

            if (emplacementsSource != null && emplacementsSource.Count > 0)
            {
                foreach (var it in emplacementsSource) cboEmplacement.Items.Add(it);
            }
            else
            {
                for (int i = 1; i <= 20; i++) cboEmplacement.Items.Add("Rayon " + i);
                for (int i = 1; i <= 10; i++) cboEmplacement.Items.Add("Table " + i);
                cboEmplacement.Items.Add("Coin Gauche");
                cboEmplacement.Items.Add("Coin Droite");
            }

            if (!string.IsNullOrWhiteSpace(emplacementInitial))
                cboEmplacement.SelectedItem = emplacementInitial;

            if (cboEmplacement.SelectedIndex < 0 && cboEmplacement.Items.Count > 0)
                cboEmplacement.SelectedIndex = 0;
        }

        private void btnPaiements_Click(object sender, EventArgs e)
        {
            // Paiements = optionnel. On peut laisser l'user ouvrir même sans nom/tel.
            // Mais en pharmacie souvent il faut => tu peux forcer TryGetNomTel si tu veux.
            bool allowPartial = chkCredit.Checked;

            string devise = NormalizeDevise(_deviseVente);

            using (var f = new FormPaiementsVente(_netAPayer, devise, "SYSTEM", allowPartial))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                    PaiementsChoisis = f.Result;
            }
        }

        private string NormalizeDevise(string d)
        {
            d = (d ?? "").Trim().ToUpperInvariant();
            if (d == "FC") d = "CDF";
            if (d != "CDF" && d != "USD" && d != "EUR") d = "CDF";
            return d;
        }

        private bool RequireManagerApproval(
    string typeAction,
    string permissionCode,
    string details)
        {
            try
            {
                using (var frm = new FrmSignatureManager(
                    ConfigSysteme.ConnectionString,
                    typeAction: typeAction,
                    permissionCode: permissionCode,
                    reference: "FormClientsRapide",
                    details: details,
                    idEmployeDemandeur: SessionEmploye.ID_Employe,
                    roleDemandeur: SessionEmploye.Poste
                ))
                {
                    frm.CloseOnApproved = true;
                    frm.StartOnEmpreinteTab = true;

                    var dr = frm.ShowDialog(this);
                    if (dr != DialogResult.OK || !frm.Approved)
                        return false;

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Signature Manager : " + ex.Message);
                return false;
            }
        }


        // ===================== DB LOAD (TOUJOURS) =====================
        private async Task ChargerClientsDepuisDbAsync()
        {
            _allClients.Clear();
            cmbNomClient.BeginUpdate();
            try
            {
                cmbNomClient.Items.Clear();

                using (var con = new SqlConnection(_cs))
                {
                    await con.OpenAsync();
                    using (var cmd = new SqlCommand(@"
SELECT ID_Clients, Nom, Telephone,
       ISNULL(NULLIF(LTRIM(RTRIM(CategorieClient)),''),'OCCASIONNEL') AS CategorieClient
FROM dbo.Clients
ORDER BY Nom;", con))
                    using (var rd = await cmd.ExecuteReaderAsync())
                    {
                        while (await rd.ReadAsync())
                        {
                            var c = new ClientMin
                            {
                                ID = Convert.ToInt32(rd["ID_Clients"]),
                                Nom = (rd["Nom"]?.ToString() ?? "").Trim(),
                                Telephone = (rd["Telephone"]?.ToString() ?? "").Trim(),
                                CategorieClient = (rd["CategorieClient"]?.ToString() ?? "OCCASIONNEL").Trim().ToUpperInvariant()
                            };
                            _allClients.Add(c);
                            cmbNomClient.Items.Add(c);
                        }
                    }
                }
            }
            finally
            {
                cmbNomClient.EndUpdate();
            }
        }

        private void RafraichirAutoComplete()
        {
            var ac = new AutoCompleteStringCollection();

            foreach (var client in _allClients)
            {
                if (!string.IsNullOrWhiteSpace(client.Nom))
                    ac.Add(client.Nom.Trim());
            }

            cmbNomClient.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbNomClient.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cmbNomClient.AutoCompleteCustomSource = ac;
        }

        // ===================== EVENTS =====================
        private void cmbNomClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNomClient.SelectedItem is ClientMin c)
            {
                txtTelephone.Text = c.Telephone ?? "";

                string cat = string.IsNullOrWhiteSpace(c.CategorieClient) ? "OCCASIONNEL" : c.CategorieClient.Trim().ToUpperInvariant();
                if (cmbCategorieClient.Items.Contains(cat))
                    cmbCategorieClient.SelectedItem = cat;
                else
                    cmbCategorieClient.SelectedItem = "OCCASIONNEL";
            }
        }

        private void cmbNomClient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;

            // Si le texte correspond à un client existant => sélectionner + remplir tel
            string nom = (cmbNomClient.Text ?? "").Trim();

            var exact = _allClients.FirstOrDefault(client =>
                string.Equals((client.Nom ?? "").Trim(), nom, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
            {
                cmbNomClient.SelectedItem = exact;
                txtTelephone.Text = exact.Telephone ?? "";
            }

            txtTelephone.Focus();
            txtTelephone.SelectAll();
        }

        private void cmbNomClient_Leave(object sender, EventArgs e)
        {
            // Pas d’insert ici (sinon insertion involontaire)
            // On se contente de remplir si exact
            string nom = (cmbNomClient.Text ?? "").Trim();

            var exact = _allClients.FirstOrDefault(client =>
                string.Equals((client.Nom ?? "").Trim(), nom, StringComparison.OrdinalIgnoreCase));

            if (exact != null)
            {
                cmbNomClient.SelectedItem = exact;
                txtTelephone.Text = exact.Telephone ?? "";
            }
        }

        private bool TryGetNomTel(out string nom, out string tel)
        {
            nom = (cmbNomClient.Text ?? "").Trim();
            tel = (txtTelephone.Text ?? "").Trim();

            if (nom.Length < 2)
            {
                MessageBox.Show("Nom invalide.");
                cmbNomClient.Focus();
                cmbNomClient.SelectAll();
                return false;
            }

            if (tel.Length < 6)
            {
                MessageBox.Show("Téléphone invalide.");
                txtTelephone.Focus();
                txtTelephone.SelectAll();
                return false;
            }

            return true;
        }

        private void btnOrdonnance_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ si FormVentes a déjà chargé via ChargerContexteOrdonnance, on garde
                if (_lignesOrdonnance.Count == 0)
                {
                    // ✅ sinon on recharge depuis LignesPanier
                    if (LignesPanier != null && LignesPanier.Count > 0)
                        _lignesOrdonnance.AddRange(LignesPanier);
                }

                if (_lignesOrdonnance.Count == 0)
                {
                    MessageBox.Show("Panier vide : impossible de créer une ordonnance.");
                    return;
                }

                if (OrdonnanceChoisie == null)
                    OrdonnanceChoisie = new OrdonnanceVenteDTO();

                if (string.IsNullOrWhiteSpace(_ordNumero))
                {
                    _ordNumero = !string.IsNullOrWhiteSpace(_codeFactureEnCours)
                        ? _codeFactureEnCours
                        : $"TMP-{DateTime.Now:yyyyMMdd-HHmmss}";
                }

                OrdonnanceChoisie.Numero = _ordNumero;

                if (string.IsNullOrWhiteSpace(OrdonnanceChoisie.Prescripteur))
                    OrdonnanceChoisie.Prescripteur = _ordPrescripteur;

                if (string.IsNullOrWhiteSpace(OrdonnanceChoisie.Patient))
                    OrdonnanceChoisie.Patient = _ordPatient;

                if (OrdonnanceChoisie.DateOrdonnance == default)
                    OrdonnanceChoisie.DateOrdonnance = DateTime.Today;

                using (var f = new FrmOrdonnanceVente(_lignesOrdonnance.ToList(), OrdonnanceChoisie))
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        SetOrdonnance(f.Result);

                        btnOuvrirPdfOrdonnance.Enabled =
                            OrdonnanceChoisie != null &&
                            !string.IsNullOrWhiteSpace(OrdonnanceChoisie.PdfPath) &&
                            File.Exists(OrdonnanceChoisie.PdfPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur Ordonnance : " + ex.Message);
            }
        }

        private void btnOuvrirPdfOrdonnance_Click(object sender, EventArgs e)
        {
            try
            {
                if (OrdonnanceChoisie != null &&
                    !string.IsNullOrWhiteSpace(OrdonnanceChoisie.PdfPath) &&
                    File.Exists(OrdonnanceChoisie.PdfPath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = OrdonnanceChoisie.PdfPath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    MessageBox.Show("Aucun PDF d'ordonnance disponible.");
                }
            }
            catch { }
        }



        // ===================== OK = FINALISER =====================
        private async Task Ok_FinaliserAsync()
        {
            string categorie = (cmbCategorieClient.SelectedItem?.ToString() ?? "OCCASIONNEL").Trim().ToUpperInvariant();
            string nom = (cmbNomClient.Text ?? "").Trim();
            string tel = (txtTelephone.Text ?? "").Trim();

            if (nom.Length < 2)
            {
                MessageBox.Show("Nom invalide.");
                cmbNomClient.Focus();
                cmbNomClient.SelectAll();
                return;
            }

            if (tel.Length < 6)
            {
                MessageBox.Show("Téléphone invalide.");
                txtTelephone.Focus();
                txtTelephone.SelectAll();
                return;
            }

            // ✅ INSERT/UPDATE triggers-safe
            int idClient = await EnsureClientExistsAsync_NomTel(nom, tel, categorie);

            // ✅ mettre à jour la liste locale + combo pour la session en cours
            var exist = _allClients.FirstOrDefault(client => client.ID == idClient);

            if (exist == null)
            {
                exist = new ClientMin { ID = idClient, Nom = nom, Telephone = tel };
                _allClients.Add(exist);
                cmbNomClient.Items.Add(exist);
            }
            else
            {
                exist.Nom = nom;
                exist.Telephone = tel;
            }

            RafraichirAutoComplete();
            cmbNomClient.SelectedItem = exist;

            // ✅ stocker ce qui doit être injecté dans FormVentes
            NomClientChoisi = nom;
            TelephoneChoisi = tel;
            CouponChoisi = (txtCoupon.Text ?? "").Trim();
            VenteCreditChoisi = chkCredit.Checked;
            EcheanceChoisie = dtpEcheance.Value.Date;
            EmplacementChoisi = cboEmplacement.SelectedItem?.ToString() ?? "";
            CategorieClientChoisie = categorie;

            // ✅ injecter dans FormVentes
            _injectToFormVentes(this);

            DialogResult = DialogResult.OK;
            Close();

            // ✅ déclencher la finalisation (après fermeture)
            _finaliserAction();
        }

        // ===================== DB (TRIGGERS OK) =====================
        private async Task<int> EnsureClientExistsAsync_NomTel(string nom, string telephone, string categorie)
        {
            nom = (nom ?? "").Trim();
            telephone = (telephone ?? "").Trim();
            categorie = string.IsNullOrWhiteSpace(categorie) ? "OCCASIONNEL" : categorie.Trim().ToUpperInvariant();

            using (var con = new SqlConnection(_cs))
            {
                await con.OpenAsync();

                // 1) match NOM + TEL
                using (var cmdFind = new SqlCommand(@"
SELECT TOP 1 ID_Clients
FROM dbo.Clients
WHERE LTRIM(RTRIM(Nom)) = LTRIM(RTRIM(@nom))
  AND LTRIM(RTRIM(ISNULL(Telephone,''))) = LTRIM(RTRIM(@tel))
ORDER BY ID_Clients DESC;", con))
                {
                    cmdFind.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nom;
                    cmdFind.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telephone;

                    var o = await cmdFind.ExecuteScalarAsync();
                    if (o != null && o != DBNull.Value)
                    {
                        int idFound = Convert.ToInt32(o);

                        // ✅ Mise à jour catégorie (et nom/tel au besoin)
                        using (var cmdUp = new SqlCommand(@"
UPDATE dbo.Clients
SET Nom=@nom, Telephone=@tel, CategorieClient=@cat
WHERE ID_Clients=@id;", con))
                        {
                            cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = idFound;
                            cmdUp.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nom;
                            cmdUp.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telephone;
                            cmdUp.Parameters.Add("@cat", SqlDbType.NVarChar, 30).Value = categorie;
                            await cmdUp.ExecuteNonQueryAsync();
                        }

                        return idFound;
                    }
                }

                // 2) match TEL seul
                if (!string.IsNullOrWhiteSpace(telephone))
                {
                    using (var cmdTel = new SqlCommand(@"
SELECT TOP 1 ID_Clients
FROM dbo.Clients
WHERE LTRIM(RTRIM(ISNULL(Telephone,''))) = LTRIM(RTRIM(@tel))
ORDER BY ID_Clients DESC;", con))
                    {
                        cmdTel.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telephone;

                        var o = await cmdTel.ExecuteScalarAsync();
                        if (o != null && o != DBNull.Value)
                        {
                            int id = Convert.ToInt32(o);

                            using (var cmdUp = new SqlCommand(@"
UPDATE dbo.Clients
SET Nom=@nom, CategorieClient=@cat
WHERE ID_Clients=@id;", con))
                            {
                                cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = id;
                                cmdUp.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nom;
                                cmdUp.Parameters.Add("@cat", SqlDbType.NVarChar, 30).Value = categorie;
                                await cmdUp.ExecuteNonQueryAsync();
                            }

                            return id;
                        }
                    }
                }

                // 3) INSERT
                using (var cmdIns = new SqlCommand(@"
INSERT INTO dbo.Clients (Nom, Prenom, Adresse, Telephone, Email, CategorieClient)
VALUES (@nom, N'', N'', @tel, NULL, @cat);

SELECT CAST(SCOPE_IDENTITY() AS int);", con))
                {
                    cmdIns.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nom;
                    cmdIns.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telephone ?? "";
                    cmdIns.Parameters.Add("@cat", SqlDbType.NVarChar, 30).Value = categorie;

                    return Convert.ToInt32(await cmdIns.ExecuteScalarAsync());
                }
            }
        }

    }
}
