using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormPromoPartenaireManager : Form
    {
        private readonly int _idPartenaire;

        private Label _lbl;
        private DataGridView _dgv;
        private Button _btnRefresh;
        private Button _btnRetrait;

        private void BuildUiGeneral() { /* crée combo/grid etc */ }
        private void LoadPartenaires() { /* SELECT IdPartenaire, Nom,... */ }
        private void SelectPartenaire(int id) { /* set selected */ }
        private void RefreshCompte() { /* solde + mouvements */ }

        // ✅ Nouveau : ouverture générale
        public FormPromoPartenaireManager()
        {
            InitializeComponent();
            MessageBox.Show("Ouvre ce formulaire depuis un partenaire (avec ID).", "Info");
            Close();
            Text = "Promo Partenaires - Manager";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            BuildUiGeneral();   // combo/grid partenaires + boutons
            LoadPartenaires();  // remplir liste
        }
        public FormPromoPartenaireManager(int idPartenaire)
        {
            _idPartenaire = idPartenaire;

            Text = "Promo Partenaire - ID " + idPartenaire;
            Width = 980;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            _lbl = new Label { Dock = DockStyle.Top, Height = 42, TextAlign = ContentAlignment.MiddleLeft };

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false
            };

            _btnRetrait = new Button { Text = "➖ Faire un retrait", Dock = DockStyle.Bottom, Height = 42 };
            _btnRefresh = new Button { Text = "🔄 Actualiser", Dock = DockStyle.Bottom, Height = 42 };

            Controls.Add(_dgv);
            Controls.Add(_lbl);
            Controls.Add(_btnRetrait);
            Controls.Add(_btnRefresh);

            _btnRefresh.Click += (s, e) => LoadData();
            _btnRetrait.Click += (s, e) => DoRetrait();

            LoadData();
        }

        private void FormPromoPartenaireManager_Load(object sender, EventArgs e)
        {

        }

        private void LoadData()
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    // ====== Solde (table Partenaire) ======
                    using (var cmd = new SqlCommand(@"
SELECT 
    ISNULL(p.Nom,'') AS Nom,
    ISNULL(SUM(m.Montant),0) AS Solde
FROM Partenaire p
LEFT JOIN PartenairePromoMvt m ON m.IdPartenaire = p.IdPartenaire
WHERE p.IdPartenaire=@id
GROUP BY p.Nom;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idPartenaire;

                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string nom = r.IsDBNull(0) ? "" : r.GetString(0);
                                decimal solde = r.IsDBNull(1) ? 0m : Convert.ToDecimal(r.GetValue(1));
                                _lbl.Text = $"Partenaire: {nom}  |  Solde promo: {solde:N2}";
                            }
                            else
                            {
                                _lbl.Text = "Partenaire introuvable.";
                            }
                        }
                    }

                    // ====== Mouvements (table PartenairePromoMvt) ======
                    using (var da = new SqlDataAdapter(@"
SELECT 
    IdMvt,
    DateMvt,
    CASE WHEN ISNULL(Montant,0) < 0 THEN 'RETRAIT' ELSE 'CREDIT' END AS TypeMvt,
    Montant,
    IdVente,
    CodeCoupon,
    Note
FROM PartenairePromoMvt
WHERE IdPartenaire=@id
ORDER BY DateMvt DESC;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = _idPartenaire;

                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgv.DataSource = dt;
                    }
                }

                // ====== Headers (optionnel mais propre) ======
                if (_dgv.Columns.Contains("IdMvt")) _dgv.Columns["IdMvt"].HeaderText = "ID";
                if (_dgv.Columns.Contains("DateMvt")) _dgv.Columns["DateMvt"].HeaderText = "Date";
                if (_dgv.Columns.Contains("TypeMvt")) _dgv.Columns["TypeMvt"].HeaderText = "Type";
                if (_dgv.Columns.Contains("Montant")) _dgv.Columns["Montant"].HeaderText = "Montant";
                if (_dgv.Columns.Contains("IdVente")) _dgv.Columns["IdVente"].HeaderText = "Vente";
                if (_dgv.Columns.Contains("CodeCoupon")) _dgv.Columns["CodeCoupon"].HeaderText = "Coupon";
                if (_dgv.Columns.Contains("Note")) _dgv.Columns["Note"].HeaderText = "Note";

                // Format montant
                if (_dgv.Columns.Contains("Montant"))
                {
                    _dgv.Columns["Montant"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    _dgv.Columns["Montant"].DefaultCellStyle.Format = "N2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement solde/mouvements :\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int CreerVenteTechniqueRetraitPromo(
    SqlConnection cn, SqlTransaction tx,
    int idEntreprise, int idMagasin, int idPoste,
    string devise, string nomCaissier,
    int idPartenaire, decimal montant, string note)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Vente
(
    DateVente,
    ID_Client,
    IDEmploye,
    ModePaiement,
    MontantTotal,
    NomCaissier,
    Devise,
    IdSession,
    CodeFacture,
    Statut,
    AnnulePar,
    DateAnnulation,
    MotifAnnulation,
    RemiseTicketPct,
    RemiseTicketMontant,
    IdEntreprise,
    IdMagasin,
    IdPoste,
    DateVenteDT,
    MontantAnnule
)
VALUES
(
    CONVERT(date, GETDATE()),
    NULL,
    NULL,
    @mode,
    @total,
    @caissier,
    @devise,
    NULL,
    @codeFacture,
    'VALIDE',
    NULL,
    NULL,
    NULL,
    0,
    0,
    @idEnt,
    @idMag,
    @idPoste,
    GETDATE(),
    0
);

SELECT CAST(SCOPE_IDENTITY() AS int);", cn, tx))
            {
                cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 30).Value = "RETRAIT_PARTENAIRE";

                var pTot = cmd.Parameters.Add("@total", SqlDbType.Decimal);
                pTot.Precision = 18; pTot.Scale = 2;
                pTot.Value = -Math.Abs(montant);

                cmd.Parameters.Add("@caissier", SqlDbType.NVarChar, 120).Value = nomCaissier ?? "";
                cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value = devise ?? "CDF";

                cmd.Parameters.Add("@codeFacture", SqlDbType.NVarChar, 60).Value =
                    "RET-PART-" + idPartenaire + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                cmd.Parameters.Add("@idEnt", SqlDbType.Int).Value = idEntreprise;
                cmd.Parameters.Add("@idMag", SqlDbType.Int).Value = idMagasin;
                cmd.Parameters.Add("@idPoste", SqlDbType.Int).Value = (idPoste > 0 ? idPoste : (object)DBNull.Value);

                int idVente = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                if (idVente <= 0) throw new Exception("Vente technique non générée (SCOPE_IDENTITY = 0).");
                return idVente;
            }
        }

        private int InsererMouvementPromo(SqlConnection cn, SqlTransaction tx,
            int idPartenaire, int idVente, decimal montantNegatif, string note)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.PartenairePromoMvt
(
    IdPartenaire,
    DateMvt,
    CodeCoupon,
    IdVente,
    Montant,
    Note
)
VALUES
(
    @id,
    GETDATE(),
    @codeCoupon,
    @idVente,
    @mnt,
    @note
);

SELECT CAST(SCOPE_IDENTITY() AS int);", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPartenaire;
                cmd.Parameters.Add("@codeCoupon", SqlDbType.NVarChar, 50).Value = "RETRAIT";
                cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;

                var pM = cmd.Parameters.Add("@mnt", SqlDbType.Decimal);
                pM.Precision = 18; pM.Scale = 2;
                pM.Value = montantNegatif;

                cmd.Parameters.Add("@note", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note.Trim();

                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
        }

        private void InsererPaiementRetraitPromo(SqlConnection cn, SqlTransaction tx,
            int idVente, int idEntreprise, int idMagasin, int idPoste,
            string devise, decimal montantNegatif, int idPartenaire)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.PaiementsVente
(
    IdVente,
    ModePaiement,
    Devise,
    Montant,
    DatePaiement,
    ReferenceTransaction,
    Statut,
    AnnulePar,
    DateAnnulation,
    MotifAnnulation,
    DeviseOriginale,
    MontantOriginal,
    TauxApplique,
    IdEntreprise,
    IdMagasin,
    IdPoste
)
VALUES
(
    @idVente,
    @mode,
    @devise,
    @mnt,
    GETDATE(),
    @ref,
    'VALIDE',
    NULL,
    NULL,
    NULL,
    @devOrig,
    @mntOrig,
    NULL,
    @idEnt,
    @idMag,
    @idPoste
);", cn, tx))
            {
                cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 30).Value = "RETRAIT_PARTENAIRE";
                cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value = devise ?? "CDF";

                var pM = cmd.Parameters.Add("@mnt", SqlDbType.Decimal);
                pM.Precision = 18; pM.Scale = 2;
                pM.Value = montantNegatif;

                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 120).Value =
                    $"Retrait promo partenaire={idPartenaire} {DateTime.Now:yyyyMMddHHmmss}";

                cmd.Parameters.Add("@devOrig", SqlDbType.NVarChar, 10).Value = devise ?? "CDF";

                var pMO = cmd.Parameters.Add("@mntOrig", SqlDbType.Decimal);
                pMO.Precision = 18; pMO.Scale = 2;
                pMO.Value = montantNegatif;

                cmd.Parameters.Add("@idEnt", SqlDbType.Int).Value = idEntreprise;
                cmd.Parameters.Add("@idMag", SqlDbType.Int).Value = idMagasin;
                cmd.Parameters.Add("@idPoste", SqlDbType.Int).Value = (idPoste > 0 ? idPoste : (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        private void DoRetrait()
        {
            string sMontant = Prompt("Montant à retirer :", "Retrait Partenaire");
            if (string.IsNullOrWhiteSpace(sMontant)) return;

            if (!decimal.TryParse(sMontant, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"),
                out decimal montant) || montant <= 0m)
            {
                MessageBox.Show("Montant invalide.");
                return;
            }

            string note = Prompt("Note (optionnel) :", "Retrait Partenaire");

            // ✅ CONTEXTE POS (évite NON AFFECTE)
            int idEnt = AppContext.IdEntreprise;
            int idMag = AppContext.IdMagasin;
            int idPoste = AppContext.IdPoste;

            if (idEnt <= 0 || idMag <= 0)
            {
                MessageBox.Show("POS non configuré : IdEntreprise / IdMagasin manquant.\nConfigure le poste (BOSS / POS).");
                return;
            }

            string devise = "CDF"; // adapte si besoin
            string nomCaissier = ((SessionEmploye.Prenom + " " + SessionEmploye.Nom) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nomCaissier))
                nomCaissier = Environment.UserName ?? "CAISSIER";

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // 1) Vérifier solde partenaire (verrouillage)
                        decimal solde = 0m;
                        using (var cmd = new SqlCommand(@"
SELECT ISNULL(SUM(Montant),0)
FROM dbo.PartenairePromoMvt WITH (UPDLOCK, HOLDLOCK)
WHERE IdPartenaire=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idPartenaire;
                            solde = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m);
                        }

                        if (solde < montant)
                        {
                            MessageBox.Show($"Solde insuffisant. Solde={solde:N2}, Retrait demandé={montant:N2}");
                            tx.Rollback();
                            return;
                        }

                        decimal montantNeg = -Math.Abs(montant);

                        // 2) Vente technique (avec magasin/entreprise)
                        int idVente = CreerVenteTechniqueRetraitPromo(
                            cn, tx,
                            idEnt, idMag, idPoste,
                            devise, nomCaissier,
                            _idPartenaire, montant, note);

                        // 3) Mouvement promo lié directement à IdVente
                        int idMvt = InsererMouvementPromo(cn, tx, _idPartenaire, idVente, montantNeg, note);
                        if (idMvt <= 0) throw new Exception("Mouvement promo non généré.");

                        // 4) Paiement vente (même magasin)
                        InsererPaiementRetraitPromo(cn, tx, idVente, idEnt, idMag, idPoste, devise, montantNeg, _idPartenaire);

                        // 5) Sync SoldePromo (table Partenaire)
                        decimal soldeNew = 0m;
                        using (var cmd = new SqlCommand(@"
SELECT ISNULL(SUM(Montant),0)
FROM dbo.PartenairePromoMvt
WHERE IdPartenaire=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idPartenaire;
                            soldeNew = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m);
                        }

                        using (var cmd = new SqlCommand(@"
UPDATE dbo.Partenaire
SET SoldePromo = @s
WHERE IdPartenaire=@id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idPartenaire;

                            var pS = cmd.Parameters.Add("@s", SqlDbType.Decimal);
                            pS.Precision = 18; pS.Scale = 2;
                            pS.Value = soldeNew;

                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        MessageBox.Show("✅ Retrait effectué (mouvement + caisse + magasin bien affecté).");
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        MessageBox.Show("Erreur retrait :\n" + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            LoadData();
        }



        // =====================================================================
        // ✅ Crée une vente technique (squelette) et retourne IdVente
        // ⚠️ ADAPTE CE BLOC à ta table Vente/Ventes (colonnes obligatoires).
        // =====================================================================
        private int CreateVenteTechniquePourRetraitPartenaire(SqlConnection cn, SqlTransaction tx, int idPartenaire, decimal montant, string note)
        {
            // ⚠️ Remplace "Vente" par le nom exact de ta table des ventes (ex: Ventes)
            // ⚠️ Mets ici les colonnes obligatoires dans ta base.

            using (var cmd = new SqlCommand(@"
INSERT INTO Vente
(
    DateVente,
    Total,
    Statut,
    Note
)
OUTPUT INSERTED.IdVente
VALUES
(
    GETDATE(),
    0,
    'TECH',
    @note
);", cn, tx))
            {
                cmd.Parameters.Add("@note", SqlDbType.NVarChar, 200).Value =
                    $"Vente technique: retrait partenaire ID={idPartenaire} | {montant:N2}" +
                    (string.IsNullOrWhiteSpace(note) ? "" : " | " + note.Trim());

                object o = cmd.ExecuteScalar();
                if (o == null || o == DBNull.Value) throw new Exception("Impossible de créer la vente technique (IdVente null).");

                return Convert.ToInt32(o);
            }
        }


        // =====================================================================
        // ✅ Helpers : adapte avec ta Session/Config
        // =====================================================================
        private int GetIdEntrepriseSafe()
        {
           
            return 1;
        }
        private int GetIdMagasinSafe()
        {
            return 1;
        }
        private int GetIdPosteSafe()
        {
            return 1;
        }



        private static string Prompt(string text, string caption)
        {
            using (Form f = new Form())
            {
                f.Text = caption;
                f.Width = 420;
                f.Height = 160;
                f.StartPosition = FormStartPosition.CenterParent;

                Label lbl = new Label { Left = 10, Top = 10, Width = 380, Text = text };
                TextBox tb = new TextBox { Left = 10, Top = 35, Width = 380 };
                Button ok = new Button { Text = "OK", Left = 230, Width = 80, Top = 70, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Annuler", Left = 310, Width = 80, Top = 70, DialogResult = DialogResult.Cancel };

                f.Controls.Add(lbl);
                f.Controls.Add(tb);
                f.Controls.Add(ok);
                f.Controls.Add(cancel);
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
            }
        }
    }
}
