using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BoutiqueRebuildFixed;
using ZXing;
using static BoutiqueRebuildFixed.FormMain;

namespace BoutiqueRebuildFixed
{
    public partial class FormClients : Form
    {
        // Connexion SQL (tu peux aussi utiliser ConfigSysteme.ConnectionString)
        private readonly string _cs = ConfigSysteme.ConnectionString;
        private ContextMenuStrip menuClients;

        private readonly string _logoPath = @"D:\ZAIRE\LOGO1.png";

        // Impression
        private PrintDocument _printDoc = null;
        private string _printMode = ""; // "PLAST" ou "PVC"
        private int _printIndex = 0;

        // ✅ Fonts cachées (évite GDI leak)
        private readonly Font _gridHeaderFont = new Font("Segoe UI", 10, FontStyle.Bold);
        private readonly Font _gridFont = new Font("Segoe UI", 10, FontStyle.Regular);

        // ✅ Flag pour éviter de refaire le "styling" à chaque refresh
        private bool _gridStyledOnce = false;

        // ✅ PrintDoc reset propre

        // ✅ Soft delete (assume que tu ajoutes une colonne Actif BIT dans dbo.Clients)
        private const bool USE_SOFT_DELETE = true;


        // Cache des cartes à imprimer (pour A4)
        private class ClientCardInfo
        {
            public int IdClient { get; set; }
            public string NomComplet { get; set; }
            public string CodeCarte { get; set; }
        }

        private void ResetPrintDoc()
        {
            try
            {
                if (_printDoc != null)
                {
                    _printDoc.PrintPage -= PrintDoc_Plastification_PrintPage;
                    _printDoc.PrintPage -= PrintDoc_PVC_PrintPage;
                    _printDoc.Dispose();
                    _printDoc = null;
                }
            }
            catch { /* ignore */ }
        }



        private class FormCreditManager : Form
        {
            private readonly int _idClient;
            private DataGridView _dgvCredits;
            private DataGridView _dgvPays;
            private Label _lblInfo;
            private Button _btnRefresh;
            private Button _btnPay;
            private Button _btnDeletePay;

            public FormCreditManager(int idClient)
            {
                _idClient = idClient;
                Text = "Compte Crédit - Client " + idClient;
                Width = 1050;
                Height = 680;
                StartPosition = FormStartPosition.CenterParent;

                _lblInfo = new Label { Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleLeft };

                _dgvCredits = new DataGridView
                {
                    Dock = DockStyle.Top,
                    Height = 260,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false
                };

                _dgvPays = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false
                };

                _btnRefresh = new Button { Text = "Actualiser", Dock = DockStyle.Bottom, Height = 40 };
                _btnPay = new Button { Text = "Encaisser Paiement (sur crédit sélectionné)", Dock = DockStyle.Bottom, Height = 40 };

                Controls.Add(_dgvPays);
                Controls.Add(_dgvCredits);
                Controls.Add(_lblInfo);
                Controls.Add(_btnPay);
                Controls.Add(_btnRefresh);

                _btnRefresh.Click += (s, e) => LoadCredits();
                _btnPay.Click += (s, e) => PaySelectedCredit();
                _btnDeletePay = new Button { Text = "🗑 Supprimer Paiement sélectionné", Dock = DockStyle.Bottom, Height = 40 };
                Controls.Add(_btnDeletePay);

                _btnDeletePay.Click += (s, e) => DeleteSelectedPayment();
                _dgvCredits.SelectionChanged += (s, e) => LoadPaymentsForSelected();

                LoadCredits();
            }

            private int GetSelectedPaiementId()
            {
                if (_dgvPays.CurrentRow == null) return 0;
                object v = _dgvPays.CurrentRow.Cells["IdPaiement"]?.Value;
                return (v != null && int.TryParse(v.ToString(), out int id)) ? id : 0;
            }

           


            private void DeleteSelectedPayment()
            {
                int idPaiement = GetSelectedPaiementId();
                if (idPaiement <= 0)
                {
                    MessageBox.Show("Sélectionne un paiement.");
                    return;
                }

                if (MessageBox.Show($"Supprimer le paiement ID={idPaiement} ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                    {
                        cn.Open();
                        using (var tx = cn.BeginTransaction())
                        {
                            // ⚠️ Si tu as des recalculs à faire (Reste, CompteClient), fais-les ici
                            using (var cmd = new SqlCommand("DELETE FROM CreditPaiement WHERE IdPaiement=@id", cn, tx))
                            {
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idPaiement;
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                    }

                    LoadCredits(); // recharge crédits + paiements
                    MessageBox.Show("Paiement supprimé.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur suppression paiement: " + ex.Message);
                }
            }

            private void LoadCredits()
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    // résumé dette globale (CompteClient.Solde)
                    using (var cmd = new SqlCommand("SELECT ISNULL(Solde,0) FROM CompteClient WHERE IdClient=@id;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idClient;
                        decimal soldeDette = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0m);
                        _lblInfo.Text = "Dette globale (Solde CompteClient) : " + soldeDette.ToString("N2");
                    }

                    using (var da = new SqlDataAdapter(@"
SELECT 
    IdCredit,
    DateCredit,
    Total,
    Reste,
    DateEcheance,
    Statut,
    RefVente
FROM CreditVente
WHERE IdClient=@id
ORDER BY DateCredit DESC;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = _idClient;
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgvCredits.DataSource = dt;
                    }
                }

                LoadPaymentsForSelected();
            }

            private int GetSelectedCreditId()
            {
                if (_dgvCredits.CurrentRow == null) return 0;
                object v = _dgvCredits.CurrentRow.Cells["IdCredit"]?.Value;
                int id = 0;
                int.TryParse(v == null ? "" : v.ToString(), out id);
                return id;
            }

            private void LoadPaymentsForSelected()
            {
                int idCredit = GetSelectedCreditId();
                if (idCredit <= 0)
                {
                    _dgvPays.DataSource = null;
                    return;
                }

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT IdPaiement, DatePaiement, Montant, ModePaiement, Note
FROM CreditPaiement
WHERE IdCredit=@c
ORDER BY DatePaiement DESC;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@c", SqlDbType.Int).Value = idCredit;
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgvPays.DataSource = dt;
                    }
                }
            }

            private void PaySelectedCredit()
            {
                int idCredit = GetSelectedCreditId();
                if (idCredit <= 0)
                {
                    MessageBox.Show("Sélectionne un crédit.");
                    return;
                }

                string sMontant = Prompt("Montant à encaisser :", "Paiement Crédit");
                if (string.IsNullOrWhiteSpace(sMontant)) return;

                if (!decimal.TryParse(sMontant, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal montant) || montant <= 0m)
                {
                    MessageBox.Show("Montant invalide.");
                    return;
                }

                string mode = Prompt("Mode paiement (CASH/MOMO/CARTE...):", "Paiement Crédit");
                string note = Prompt("Note (optionnel):", "Paiement Crédit");

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var tx = cn.BeginTransaction())
                    {
                        try
                        {
                            var creditSvc = new CreditService(ConfigSysteme.ConnectionString);
                            creditSvc.PayCredit(idCredit, montant, mode, note, cn, tx);
                            tx.Commit();
                        }
                        catch (Exception ex)
                        {
                            try { tx.Rollback(); } catch { }
                            MessageBox.Show("Erreur paiement crédit: " + ex.Message);
                            return;
                        }
                    }
                }

                LoadCredits();
                MessageBox.Show("Paiement enregistré.");
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

        private class FormHistoriqueAchats : Form
        {
            private readonly int _idClient;
            private DataGridView _dgvVentes;
            private DataGridView _dgvDetails;
            private Button _btnDeleteVente;

            public FormHistoriqueAchats(int idClient)
            {
                _idClient = idClient;
                Text = "Historique Achats - Client " + idClient;
                Width = 1100;
                Height = 650;
                StartPosition = FormStartPosition.CenterParent;

                _dgvVentes = new DataGridView { Dock = DockStyle.Top, Height = 260, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
                _dgvDetails = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };

                Controls.Add(_dgvDetails);
                Controls.Add(_dgvVentes);

                _dgvVentes.SelectionChanged += (s, e) => LoadDetails();
                _btnDeleteVente = new Button { Text = "🗑 Supprimer Vente sélectionnée", Dock = DockStyle.Bottom, Height = 40 };
                Controls.Add(_btnDeleteVente);

                _btnDeleteVente.Click += (s, e) => DeleteSelectedVente();

                LoadVentes();
            }

            private void DeleteSelectedVente()
            {
                int idVente = GetSelectedVenteId();
                if (idVente <= 0)
                {
                    MessageBox.Show("Sélectionne une vente.");
                    return;
                }

                if (MessageBox.Show($"Supprimer la vente ID={idVente} (et ses détails) ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                    {
                        cn.Open();
                        using (var tx = cn.BeginTransaction())
                        {
                            // 1) Détails
                            using (var cmd = new SqlCommand("DELETE FROM DetailsVente WHERE ID_Vente=@v", cn, tx))
                            {
                                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                                cmd.ExecuteNonQuery();
                            }

                            // 2) Vente
                            using (var cmd = new SqlCommand("DELETE FROM Vente WHERE ID_Vente=@v", cn, tx))
                            {
                                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                                cmd.ExecuteNonQuery();
                            }

                            tx.Commit();
                        }
                    }

                    LoadVentes();
                    MessageBox.Show("Vente supprimée.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur suppression vente: " + ex.Message);
                }
            }

            private void LoadVentes()
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT ID_Vente, DateVente, MontantTotal, Devise, ModePaiement, CodeFacture, Statut
FROM Vente
WHERE ID_Client=@id
ORDER BY DateVente DESC;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = _idClient;
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgvVentes.DataSource = dt;
                    }
                }
                LoadDetails();
            }

            private int GetSelectedVenteId()
            {
                if (_dgvVentes.CurrentRow == null) return 0;
                object v = _dgvVentes.CurrentRow.Cells["ID_Vente"]?.Value;
                int id = 0; int.TryParse(v == null ? "" : v.ToString(), out id);
                return id;
            }

            private void LoadDetails()
            {
                int idVente = GetSelectedVenteId();
                if (idVente <= 0) { _dgvDetails.DataSource = null; return; }

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT ID_Produit, NomProduit, RefProduit, Quantite, PrixUnitaire, Remise, TVA, Montant, Devise
FROM DetailsVente
WHERE ID_Vente=@v
ORDER BY NomProduit;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgvDetails.DataSource = dt;
                    }
                }
            }
        }
        private class FormCoupons : Form
        {
            private DataGridView _dgv;
            private Button _btnRefresh;
            private Button _btnAdd;
            private Button _btnDisable;
            private Button _btnDelete;

            public FormCoupons()
            {
                Text = "Coupons";
                Width = 900;
                Height = 550;
                StartPosition = FormStartPosition.CenterParent;

                _dgv = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false
                };

                _btnDisable = new Button { Text = "Désactiver coupon sélectionné", Dock = DockStyle.Bottom, Height = 40 };
                _btnAdd = new Button { Text = "Ajouter coupon", Dock = DockStyle.Bottom, Height = 40 };
                _btnRefresh = new Button { Text = "Actualiser", Dock = DockStyle.Bottom, Height = 40 };

                Controls.Add(_dgv);
                Controls.Add(_btnDisable);
                Controls.Add(_btnAdd);
                Controls.Add(_btnRefresh);

                _btnRefresh.Click += (s, e) => LoadCoupons();
                _btnAdd.Click += (s, e) => AddCoupon();
                _btnDisable.Click += (s, e) => DisableSelected();
                _btnDelete = new Button { Text = "🗑 Supprimer coupon sélectionné", Dock = DockStyle.Bottom, Height = 40 };

                Controls.Add(_btnDelete);
                _btnDelete.Click += (s, e) => DeleteSelected();

                LoadCoupons();
            }

            private void DeleteSelected()
            {
                string code = GetSelectedCode();
                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Sélectionne un coupon.");
                    return;
                }

                if (MessageBox.Show($"Supprimer le coupon '{code}' ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                    {
                        cn.Open();
                        using (var cmd = new SqlCommand("DELETE FROM Coupon WHERE Code=@c", cn))
                        {
                            cmd.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = code.Trim().ToUpperInvariant();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    LoadCoupons();
                    MessageBox.Show("Coupon supprimé.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Impossible de supprimer : " + ex.Message);
                }
            }

            private void LoadCoupons()
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var da = new SqlDataAdapter(@"
SELECT Code, Type, Valeur, DateDebut, DateFin, MinAchat,
       UtilisationsMax, UtilisationsClientMax, Actif,
       IdPartenaire, PartenaireSharePct
FROM Coupon
ORDER BY DateFin DESC;", cn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgv.DataSource = dt;
                    }
                }

                // Optionnel : renommer les en-têtes
                if (_dgv.Columns.Contains("IdPartenaire")) _dgv.Columns["IdPartenaire"].HeaderText = "Partenaire";
                if (_dgv.Columns.Contains("PartenaireSharePct")) _dgv.Columns["PartenaireSharePct"].HeaderText = "Share %";
            }

            private string GetSelectedCode()
            {
                if (_dgv.CurrentRow == null) return null;
                return (_dgv.CurrentRow.Cells["Code"]?.Value ?? "").ToString();
            }

            private void DisableSelected()
            {
                string code = GetSelectedCode();
                if (string.IsNullOrWhiteSpace(code)) { MessageBox.Show("Sélectionne un coupon."); return; }

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("UPDATE Coupon SET Actif=0 WHERE Code=@c", cn))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = code.Trim().ToUpperInvariant();
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadCoupons();
            }

            private void AddCoupon()
            {
                string code = Prompt("Code coupon (ex: PROMO10) :", "Ajouter Coupon");
                if (string.IsNullOrWhiteSpace(code)) return;

                string type = Prompt("Type (FIXE / POURCENT) :", "Ajouter Coupon");
                string sValeur = Prompt("Valeur (ex: 10 ou 5000) :", "Ajouter Coupon");
                string sMin = Prompt("Min Achat (0 si aucun) :", "Ajouter Coupon");
                string sDays = Prompt("Durée en jours (ex: 30) :", "Ajouter Coupon");

                // ✅ NOUVEAU
                string sPart = Prompt("IdPartenaire (vide si aucun) :", "Ajouter Coupon");
                string sPct = Prompt("PartenaireSharePct (ex: 50) :", "Ajouter Coupon");

                if (!decimal.TryParse(sValeur, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal valeur))
                {
                    MessageBox.Show("Valeur invalide."); return;
                }
                if (!decimal.TryParse(sMin, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal min)) min = 0m;
                if (!int.TryParse(sDays, out int days) || days <= 0) days = 30;

                int? idPartenaire = null;
                if (!string.IsNullOrWhiteSpace(sPart) && int.TryParse(sPart.Trim(), out int pid) && pid > 0)
                    idPartenaire = pid;

                decimal sharePct = 50m;
                if (!decimal.TryParse((sPct ?? "").Trim(), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out sharePct))
                    sharePct = 50m;

                if (sharePct < 0m) sharePct = 0m;
                if (sharePct > 100m) sharePct = 100m;

                DateTime deb = DateTime.Today;
                DateTime fin = DateTime.Today.AddDays(days);

                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand(@"
INSERT INTO Coupon
    (Code,Type,Valeur,DateDebut,DateFin,MinAchat,UtilisationsMax,UtilisationsClientMax,Actif,IdPartenaire,PartenaireSharePct)
VALUES
    (@c,@t,@v,@d1,@d2,@m,NULL,NULL,1,@p,@s);", cn))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = code.Trim().ToUpperInvariant();
                        cmd.Parameters.Add("@t", SqlDbType.NVarChar, 10).Value = (type ?? "FIXE").Trim().ToUpperInvariant();

                        var pV = cmd.Parameters.Add("@v", SqlDbType.Decimal);
                        pV.Precision = 18; pV.Scale = 2; pV.Value = valeur;

                        cmd.Parameters.Add("@d1", SqlDbType.Date).Value = deb;
                        cmd.Parameters.Add("@d2", SqlDbType.Date).Value = fin;

                        var pMin = cmd.Parameters.Add("@m", SqlDbType.Decimal);
                        pMin.Precision = 18; pMin.Scale = 2; pMin.Value = min;

                        cmd.Parameters.Add("@p", SqlDbType.Int).Value =
                            (object)(idPartenaire.HasValue ? idPartenaire.Value : (object)DBNull.Value);

                        var pS = cmd.Parameters.Add("@s", SqlDbType.Decimal);
                        pS.Precision = 5; pS.Scale = 2; pS.Value = sharePct;

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadCoupons();
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
        private class FormLoyaltyMouvements : Form
        {
            private readonly int _idClient;
            private Label _lbl;
            private DataGridView _dgv;
            private Button _btnDelete;

            public FormLoyaltyMouvements(int idClient)
            {
                _idClient = idClient;
                Text = "Loyalty - Mouvements Client " + idClient;
                Width = 900;
                Height = 550;
                StartPosition = FormStartPosition.CenterParent;

                _lbl = new Label { Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleLeft };
                _dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
                _btnDelete = new Button { Text = "🗑 Supprimer mouvement sélectionné", Dock = DockStyle.Bottom, Height = 40 };
                Controls.Add(_btnDelete);

                _btnDelete.Click += (s, e) => DeleteSelectedMvt();

                Controls.Add(_dgv);
                Controls.Add(_lbl);

                LoadData();
            }

            private int GetSelectedMvtId()
            {
                if (_dgv.CurrentRow == null) return 0;
                object v = _dgv.CurrentRow.Cells["IdMvt"]?.Value;
                return (v != null && int.TryParse(v.ToString(), out int id)) ? id : 0;
            }

            private void DeleteSelectedMvt()
            {
                int idMvt = GetSelectedMvtId();
                if (idMvt <= 0)
                {
                    MessageBox.Show("Sélectionne un mouvement.");
                    return;
                }

                if (MessageBox.Show($"Supprimer le mouvement ID={idMvt} ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                try
                {
                    using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                    {
                        cn.Open();
                        using (var tx = cn.BeginTransaction())
                        {
                            using (var cmd = new SqlCommand("DELETE FROM LoyaltyMouvement WHERE IdMvt=@m", cn, tx))
                            {
                                cmd.Parameters.Add("@m", SqlDbType.Int).Value = idMvt;
                                cmd.ExecuteNonQuery();
                            }

                            // ⚠️ Idéal : recalculer Points/CashbackSolde ici (somme des mouvements)
                            tx.Commit();
                        }
                    }

                    LoadData();
                    MessageBox.Show("Mouvement supprimé.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur suppression mouvement: " + ex.Message);
                }
            }
            private void LoadData()
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    using (var cmd = new SqlCommand(@"
SELECT Statut, Points, CashbackSolde
FROM LoyaltyCompte WHERE IdClient=@id;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = _idClient;
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string statut = r.IsDBNull(0) ? "BRONZE" : r.GetString(0);
                                int pts = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                                decimal cb = r.IsDBNull(2) ? 0m : r.GetDecimal(2);
                                _lbl.Text = "Statut: " + statut + " | Points: " + pts + " | Cashback: " + cb.ToString("N2");
                            }
                            else
                            {
                                _lbl.Text = "Aucun compte fidélité pour ce client.";
                            }
                        }
                    }

                    using (var da = new SqlDataAdapter(@"
SELECT IdMvt, DateMvt, Type, PointsDelta, CashbackDelta, RefVente, Note
FROM LoyaltyMouvement
WHERE IdClient=@id
ORDER BY DateMvt DESC;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = _idClient;
                        var dt = new DataTable();
                        da.Fill(dt);
                        _dgv.DataSource = dt;
                    }
                }
            }
        }

        private bool TryGetSelectedClientRow(out DataGridViewRow row)
        {
            row = null;

            if (dgvClients == null) return false;

            if (dgvClients.SelectedRows != null && dgvClients.SelectedRows.Count > 0)
                row = dgvClients.SelectedRows[0];
            else if (dgvClients.CurrentRow != null)
                row = dgvClients.CurrentRow;

            return row != null;
        }

        private bool TryGetSelectedClientId(out int id)
        {
            id = 0;

            if (!TryGetSelectedClientRow(out DataGridViewRow row))
                return false;

            object v = row.Cells["ID_Clients"]?.Value;
            return v != null && v != DBNull.Value && int.TryParse(v.ToString(), out id) && id > 0;
        }


        private List<ClientCardInfo> _cardsToPrint = new List<ClientCardInfo>();

        // Couleurs Premium
        private Color _premiumBase = Color.FromArgb(16, 24, 40);       // bleu nuit
        private Color _premiumAccent = Color.FromArgb(198, 169, 107);  // doré
        private Color _premiumInk = Color.FromArgb(245, 245, 245);     // texte clair

        public FormClients()
        {
            InitializeComponent();

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FormClients_Load(object sender, EventArgs e)
        {
            AfficherClients();
            InitialiserMenuContextuelClients();

            dgvClients.MouseDown += dgvClients_MouseDown;

            // ✅ Garde celui-ci
            dgvClients.SelectionChanged += dgvClients_SelectionChanged;

            // ❌ Enlève celui-ci (ou commente)
            // dgvClients.CellClick += dgvClients_CellClick;

            RafraichirLangue();
            RafraichirTheme();
        }
        private bool TryGetSelectedClient(out int idClient, out string codeCarte)
        {
            idClient = 0;
            codeCarte = "";

            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Sélectionnez un client dans la liste.");
                return false;
            }

            if (!int.TryParse(txtID.Text.Trim(), out idClient) || idClient <= 0)
            {
                MessageBox.Show("ID client invalide.");
                return false;
            }

            codeCarte = (txtCodeCarte.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(codeCarte))
            {
                MessageBox.Show("Ce client n'a pas de CodeCarte. Clique d'abord sur 'Générer Carte'.");
                return false;
            }

            return true;
        }

        // ===================== LECTURE INFO CLIENT (NOM + CODE) =====================
        private ClientCardInfo LoadClientCardInfo(int idClient)
        {
            string nomComplet = "";
            string code = "";

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    ID_Clients,
    LTRIM(RTRIM(ISNULL(Nom,''))) 
    + CASE 
        WHEN NULLIF(LTRIM(RTRIM(ISNULL(Prenom,''))), '') IS NULL THEN '' 
        ELSE ' ' + LTRIM(RTRIM(ISNULL(Prenom,''))) 
      END AS NomComplet,
    ISNULL(CodeCarte,'') AS CodeCarte
FROM Clients
WHERE ID_Clients=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;

                        nomComplet = r.IsDBNull(1) ? "" : r.GetString(1);
                        code = r.IsDBNull(2) ? "" : r.GetString(2);
                    }
                }
            }

            return new ClientCardInfo
            {
                IdClient = idClient,
                NomComplet = (nomComplet ?? "").Trim(),
                CodeCarte = (code ?? "").Trim()
            };
        }

        // ===================== GENERATION QR / BARCODE (ZXing) =====================
        private Bitmap GenerateQr(string text, int size = 240)
        {
            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 1
                }
            };
            return writer.Write(text);
        }

        private Bitmap GenerateCode128(string text, int width = 900, int height = 200)
        {
            var writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 1,
                    PureBarcode = true
                }
            };
            return writer.Write(text);
        }
        private List<int> GetSelectedClientIds(int max = 8)
        {
            if (dgvClients == null) return new List<int>();

            // ✅ Si multi-select : trier par Index => ordre stable
            if (dgvClients.SelectedRows != null && dgvClients.SelectedRows.Count > 0)
            {
                return dgvClients.SelectedRows.Cast<DataGridViewRow>()
                    .OrderBy(r => r.Index)
                    .Select(r => r.Cells["ID_Clients"]?.Value)
                    .Where(v => v != null && v != DBNull.Value)
                    .Select(v => int.TryParse(v.ToString(), out int id) ? id : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .Take(max)
                    .ToList();
            }

            // fallback : CurrentRow
            if (dgvClients.CurrentRow != null)
            {
                object v = dgvClients.CurrentRow.Cells["ID_Clients"]?.Value;
                if (v != null && v != DBNull.Value && int.TryParse(v.ToString(), out int id) && id > 0)
                    return new List<int> { id };
            }

            return new List<int>();
        }

        // ===================== HELPERS DESIGN =====================
        private void FillLinearGradient(Graphics g, Rectangle r, Color c1, Color c2, float angle = 35f)
        {
            using (var br = new LinearGradientBrush(r, c1, c2, angle))
                g.FillRectangle(br, r);
        }
        private void DrawCardVersoPremium(Graphics g, Rectangle card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fond premium clair (léger dégradé)
            FillLinearGradient(g, card, Color.White, Color.FromArgb(242, 244, 248), 90f);

            // Motif logos répétés (plus visible que l'ancien verso)
            DrawLogoPattern(g, card, logoW: 52, logoH: 36, gapX: 26, gapY: 22, alpha: 38);

            // Cadre doré
            DrawPremiumFrame(g, card);

            // Bande centrale légère
            Rectangle band = new Rectangle(card.Left + 18, card.Top + (card.Height / 2) - 34, card.Width - 36, 68);
            using (var brBand = new SolidBrush(Color.FromArgb(110, _premiumAccent)))
                g.FillRectangle(brBand, band);

            // Grand nom (effet 3D simple : ombre + texte)
            string title = "ZAIRE MODE SARL";

            using (var f = new Font("Segoe UI Black", 16, FontStyle.Bold))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // Ombre (effet relief)
                using (var brShadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                    g.DrawString(title, f, brShadow, new RectangleF(band.Left + 2, band.Top + 2, band.Width, band.Height), fmt);

                // Texte principal
                using (var brText = new SolidBrush(Color.FromArgb(25, 25, 25)))
                    g.DrawString(title, f, brText, band, fmt);
            }

            // Petit slogan sous la bande
            using (var f2 = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var br2 = new SolidBrush(Color.FromArgb(120, 20, 20, 20)))
            {
                var fmt2 = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("CARTE FIDELITE • 0,5% SUR CHAQUE ACHAT", f2, br2,
                    new RectangleF(card.Left, band.Bottom + 8, card.Width, 18), fmt2);
            }

            // Logo principal en haut (optionnel mais très pro)
            Rectangle logoTop = new Rectangle(card.Left + 18, card.Top + 14, 90, 60);
            DrawLogoSafe(g, logoTop);
        }

        private void DrawShadow(Graphics g, Rectangle r, int spread = 6, int alpha = 18)
        {
            Rectangle s = Rectangle.Inflate(r, spread, spread);
            using (var br = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                g.FillRectangle(br, s);
        }

        private int GetClientId()
        {
            return TryGetSelectedClientId(out int id) ? id : 0;
        }
        private void DrawPremiumFrame(Graphics g, Rectangle card)
        {
            using (var penOuter = new Pen(_premiumAccent, 3f))
            using (var penInner = new Pen(Color.FromArgb(180, _premiumAccent), 1.2f))
            {
                penOuter.Alignment = PenAlignment.Inset;
                penInner.Alignment = PenAlignment.Inset;

                g.DrawRectangle(penOuter, card);
                var inner = Rectangle.Inflate(card, -10, -10);
                g.DrawRectangle(penInner, inner);
            }
        }

        private void DrawPremiumRibbon(Graphics g, Rectangle card, string text)
        {
            // ✅ On descend un peu le ruban pour libérer encore plus la zone du haut
            var ribbon = new Rectangle(
                card.Right - 210,
                card.Top + 38,   // ↓ plus bas
                190,
                30
            );

            using (var br = new SolidBrush(Color.FromArgb(220, _premiumAccent)))
                g.FillRectangle(br, ribbon);

            using (var f = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(25, 25, 25)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text ?? "", f, brT, ribbon, fmt);
            }
        }

        private void DrawLogoSafe(Graphics g, Rectangle r)
        {
            var img = GetLogoCached();
            if (img == null) return;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, r);
        }

        private void DrawLogoPattern(Graphics g, Rectangle area, int logoW = 44, int logoH = 30, int gapX = 22, int gapY = 18, int alpha = 24)
        {
            var img = GetLogoCached();
            if (img == null) return;

            try
            {
                using (var ia = new ImageAttributes())
                {
                    var cm = new ColorMatrix
                    {
                        Matrix00 = 1f,
                        Matrix11 = 1f,
                        Matrix22 = 1f,
                        Matrix33 = Math.Max(0f, Math.Min(1f, alpha / 255f))
                    };
                    ia.SetColorMatrix(cm);

                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    for (int y = area.Top; y < area.Bottom; y += (logoH + gapY))
                    {
                        for (int x = area.Left; x < area.Right; x += (logoW + gapX))
                        {
                            var r = new Rectangle(x, y, logoW, logoH);
                            g.DrawImage(img, r, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
                        }
                    }
                }
            }
            catch { }
        }


        private Rectangle DrawWhiteSafeBox(Graphics g, Rectangle inner, string title, int boxWidth)
        {
            // ✅ Plus petit + plus bas + moins haut (protège texte du bas)
            int topOffset = 64;          // ↓
            int bottomMargin = 92;       // ↑ laisse place aux textes du bas

            int boxH = inner.Height - (topOffset + bottomMargin);
            if (boxH < 120) boxH = 120;

            Rectangle box = new Rectangle(
                inner.Right - boxWidth,
                inner.Top + topOffset,
                boxWidth,
                boxH
            );

            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, box);

            using (var p = new Pen(Color.FromArgb(210, 210, 210), 1f))
                g.DrawRectangle(p, box);

            using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(60, 60, 60)))
                g.DrawString(title ?? "", f, brT, box.Left + 10, box.Top + 8);

            return box;
        }

        private void DrawDivider(Graphics g, int x, int y1, int y2)
        {
            using (var p = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                g.DrawLine(p, x, y1, x, y2);
        }

        // ===================== DESSIN RECTO / VERSO =====================
        private void DrawCardRecto(Graphics g, Rectangle card, ClientCardInfo info)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawShadow(g, card, spread: 6, alpha: 18);
            FillLinearGradient(g, card, _premiumBase, Color.FromArgb(30, 58, 90), 35f);
            DrawPremiumFrame(g, card);

            // ✅ scale: PVC grand => 1.0 / Plastification plus petit => 0.70-0.85
            float scale = card.Width / 860f;
            if (scale < 0.65f) scale = 0.65f;
            if (scale > 1.05f) scale = 1.05f;

            int pad = (int)(18 * scale);
            if (pad < 10) pad = 10;
            Rectangle inner = Rectangle.Inflate(card, -pad, -pad);

            DrawPremiumRibbon(g, card, "FIDELITE 0,5%");

            // ✅ Zone scan responsive: en petit format, on réduit la largeur
            int safeW = (int)(inner.Width * 0.33f);
            if (safeW < (int)(210 * scale)) safeW = (int)(210 * scale);
            if (safeW > (int)(290 * scale)) safeW = (int)(290 * scale);

            // Divider
            int dividerX = inner.Right - safeW - (int)(16 * scale);
            DrawDivider(g, dividerX, inner.Top + (int)(10 * scale), inner.Bottom - (int)(10 * scale));

            // ✅ Box blanche scan à droite (ne doit JAMAIS manger le texte)
            Rectangle safeBox = DrawWhiteSafeBox(g, inner, "SCAN", 230);

            // Logo en haut à gauche
            Rectangle logoRect = new Rectangle(inner.Left + (int)(4 * scale), inner.Top + (int)(6 * scale),
                                               (int)(92 * scale), (int)(62 * scale));
            DrawLogoSafe(g, logoRect);

            // ✅ Zone texte = tout l’espace à gauche du divider
            int textLeft = inner.Left + (int)(110 * scale);
            int textRight = dividerX - (int)(12 * scale);
            int textWidth = Math.Max(10, textRight - textLeft);

            // Fonts responsive
            using (var fBrand = new Font("Segoe UI", 13f * scale, FontStyle.Bold))
            using (var fTitle = new Font("Segoe UI", 10.5f * scale, FontStyle.Bold))
            using (var fSmall = new Font("Segoe UI", 9.2f * scale, FontStyle.Regular))
            using (var fCode = new Font("Consolas", 10.5f * scale, FontStyle.Bold))
            using (var brText = new SolidBrush(_premiumInk))
            using (var brSoft = new SolidBrush(Color.FromArgb(215, _premiumInk)))
            {
                int y = inner.Top + (int)(6 * scale);

                // Marque
                g.DrawString("ZAIRE MODE SARL", fBrand, brText, textLeft, y);
                y += (int)(22 * scale);

                g.DrawString("CARTE FIDELITE", fTitle, brSoft, textLeft, y);
                y += (int)(18 * scale);

                string nom = (info?.NomComplet ?? "CLIENT").Trim();
                if (nom.Length > 28) nom = nom.Substring(0, 28) + "...";

                g.DrawString(nom, fSmall, brText, new RectangleF(textLeft, y, textWidth, 999));
                y += (int)(18 * scale);

                string code = (info?.CodeCarte ?? "").Trim();
                g.DrawString("Code : " + code, fCode, brText, new RectangleF(textLeft, y, textWidth, 999));

                // ✅ Infos du bas : on les remonte pour éviter qu’elles se fassent couper
                int yInfo = inner.Bottom - (int)(52 * scale);
                if (yInfo < y + (int)(26 * scale)) yInfo = y + (int)(26 * scale);

                g.DrawString("• Cumulez 0,5% à chaque achat", fSmall, brSoft, inner.Left + (int)(4 * scale), yInfo);
                yInfo += (int)(16 * scale);
                g.DrawString("• Présentez la carte à chaque passage", fSmall, brSoft, inner.Left + (int)(4 * scale), yInfo);
            }

            // ✅ QR + Barcode responsive (ne pas déborder)
            string codeCarte = (info?.CodeCarte ?? "").Trim();

            // QR
            int qrSize = (int)(118 * scale);
            if (qrSize < 75) qrSize = 75;
            if (qrSize > 125) qrSize = 125;

            Rectangle qrRect = new Rectangle(
                safeBox.Left + (int)(12 * scale),
                safeBox.Top + (int)(23 * scale),
                qrSize,
                qrSize
            );

            using (var qr = GenerateQr(codeCarte, 240))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(qr, qrRect);
            }

            // Barcode
            int barH = (int)(40 * scale);
            if (barH < 25) barH = 25;
            if (barH > 52) barH = 52;

            Rectangle barImgRect = new Rectangle(
                safeBox.Left + (int)(12 * scale),
                qrRect.Bottom + (int)(12 * scale),
                safeBox.Width - (int)(20 * scale),
                barH
            );

            using (var barcode = GenerateCode128(codeCarte, 900, 200))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(barcode, barImgRect);
            }

            // Code texte sous barcode
            using (var fMini = new Font("Segoe UI", 8f * scale, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(codeCarte, fMini, br,
                    new RectangleF(safeBox.Left, barImgRect.Bottom + (int)(2 * scale), safeBox.Width, (int)(16 * scale)), fmt);
            }
        }

        private void DrawCardVerso(Graphics g, Rectangle card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fond premium clair (léger dégradé)
            FillLinearGradient(g, card, Color.White, Color.FromArgb(242, 244, 248), 90f);

            // Motif logos répétés (plus visible que l'ancien verso)
            DrawLogoPattern(g, card, logoW: 52, logoH: 36, gapX: 26, gapY: 22, alpha: 38);

            // Cadre doré
            DrawPremiumFrame(g, card);

            // Bande centrale légère
            Rectangle band = new Rectangle(card.Left + 18, card.Top + (card.Height / 2) - 34, card.Width - 36, 68);
            using (var brBand = new SolidBrush(Color.FromArgb(110, _premiumAccent)))
                g.FillRectangle(brBand, band);

            // Grand nom (effet 3D simple : ombre + texte)
            string title = "ZAIRE MODE SARL";

            using (var f = new Font("Segoe UI Black", 16, FontStyle.Bold))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                // Ombre (effet relief)
                using (var brShadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                    g.DrawString(title, f, brShadow, new RectangleF(band.Left + 2, band.Top + 2, band.Width, band.Height), fmt);

                // Texte principal
                using (var brText = new SolidBrush(Color.FromArgb(25, 25, 25)))
                    g.DrawString(title, f, brText, band, fmt);
            }

            // Petit slogan sous la bande
            using (var f2 = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var br2 = new SolidBrush(Color.FromArgb(120, 20, 20, 20)))
            {
                var fmt2 = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("CARTE FIDELITE • 0,5% SUR CHAQUE ACHAT", f2, br2,
                    new RectangleF(card.Left, band.Bottom + 8, card.Width, 18), fmt2);
            }

            // Logo principal en haut (optionnel mais très pro)
            Rectangle logoTop = new Rectangle(card.Left + 18, card.Top + 14, 90, 60);
            DrawLogoSafe(g, logoTop);
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
            // 🔥 OBLIGATOIRE : éviter fuite mémoire (déjà bon)
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;

            // ✅ Dispose print
            try
            {
                if (_printDoc != null)
                {
                    _printDoc.PrintPage -= PrintDoc_Plastification_PrintPage;
                    _printDoc.PrintPage -= PrintDoc_PVC_PrintPage;
                    _printDoc.Dispose();
                    _printDoc = null;
                }
            }
            catch { }

            // ✅ libère logo cache
            DisposeLogoCache();

            try { _gridHeaderFont?.Dispose(); } catch { }
            try { _gridFont?.Dispose(); } catch { }

            base.OnFormClosed(e);
        }
        private void InitialiserMenuContextuelClients()
        {
            menuClients = new ContextMenuStrip();

            ToolStripMenuItem mnuNouveau = new ToolStripMenuItem("➕ Nouveau");
            ToolStripMenuItem mnuModifier = new ToolStripMenuItem("✏ Modifier");
            ToolStripMenuItem mnuSupprimer = new ToolStripMenuItem("🗑 Supprimer");
            ToolStripMenuItem mnuActualiser = new ToolStripMenuItem("🔄 Actualiser");
            ToolStripMenuItem mnuImprimerPlast = new ToolStripMenuItem("🖨 Imprimer carte (Plastification)");
            ToolStripMenuItem mnuImprimerPVC = new ToolStripMenuItem("🖨 Imprimer carte (PVC)");

            mnuNouveau.Click += MnuNouveau_Click;
            mnuModifier.Click += MnuModifier_Click;
            mnuSupprimer.Click += MnuSupprimer_Click;
            mnuActualiser.Click += MnuActualiser_Click;
            mnuImprimerPlast.Click += (s, e) => btnImprimerPlastification_Click(s, e);
            mnuImprimerPVC.Click += (s, e) => btnImprimerPVC_Click(s, e);

            menuClients.Items.AddRange(new ToolStripItem[]
{
    mnuNouveau,
    new ToolStripSeparator(),
    mnuModifier,
    mnuSupprimer,
    new ToolStripSeparator(),
    mnuImprimerPlast,
    mnuImprimerPVC,
    new ToolStripSeparator(),
    mnuActualiser
});

            dgvClients.ContextMenuStrip = menuClients;
        }
        private void dgvClients_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dgvClients.HitTest(e.X, e.Y);

                if (hit.RowIndex >= 0)
                {
                    dgvClients.ClearSelection();

                    DataGridViewRow row = dgvClients.Rows[hit.RowIndex];
                    row.Selected = true;

                    // Trouver la première cellule VISIBLE
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Visible)
                        {
                            dgvClients.CurrentCell = cell;
                            dgvClients.Rows[hit.RowIndex].Selected = true;
                            break;
                        }
                    }
                }
            }
        }
        private void MnuNouveau_Click(object sender, EventArgs e)
        {
            ViderChampsClient();
            txtNom.Focus();
        }
        private void nouveauToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtID.Clear();
            txtNom.Clear();
            txtPrenom.Clear();
            txtAdresse.Clear();
            txtTelephone.Clear();
            txtEmail.Clear();

            txtNom.Focus();
        }
        private void MnuModifier_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un client à modifier");
                return;
            }

            // Les champs sont déjà remplis via CellClick
            txtNom.Focus();
        }
        private void modifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvClients.CurrentRow == null)
            {
                MessageBox.Show("Aucun client sélectionné");
                return;
            }

            txtID.Text = dgvClients.CurrentRow.Cells["ID_Clients"].Value.ToString();
            txtNom.Text = dgvClients.CurrentRow.Cells["Nom"].Value.ToString();
            txtPrenom.Text = dgvClients.CurrentRow.Cells["Prenom"].Value.ToString();
            txtAdresse.Text = dgvClients.CurrentRow.Cells["Adresse"].Value.ToString();
            txtTelephone.Text = dgvClients.CurrentRow.Cells["Telephone"].Value.ToString();
            txtEmail.Text = dgvClients.CurrentRow.Cells["Email"].Value.ToString();
        }
        private void MnuSupprimer_Click(object sender, EventArgs e)
        {
            SupprimerClientSelectionne();
        }
        private void supprimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SupprimerClientSelectionne();
        }
        private void MnuActualiser_Click(object sender, EventArgs e)
        {
            AfficherClients();
        }

        private void SupprimerClientSelectionne()
        {
            if (!TryGetSelectedClientRow(out DataGridViewRow row))
            {
                MessageBox.Show("Sélectionnez un client.");
                return;
            }

            if (!TryGetSelectedClientId(out int idClient))
            {
                MessageBox.Show("ID client invalide.");
                return;
            }

            string nom = (row.Cells["Nom"]?.Value ?? "").ToString();
            string prenom = (row.Cells["Prenom"]?.Value ?? "").ToString();

            var confirm = MessageBox.Show(
                $"Désactiver ce client ?\n\nID: {idClient}\nNom: {nom} {prenom}\n\n" +
                "➡️ Action: désactivation (soft-delete)\n(Le client ne sera plus visible, mais l'historique reste.)",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();
                    using (var tx = cn.BeginTransaction())
                    {
                        int n;
                        using (var cmd = new SqlCommand(@"
UPDATE dbo.Clients
SET Actif = 0
WHERE ID_Clients = @id;", cn, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                            n = cmd.ExecuteNonQuery();
                        }

                        if (n <= 0)
                        {
                            tx.Rollback();
                            MessageBox.Show("Aucune action effectuée (client introuvable).");
                            return;
                        }

                        try
                        {
                            ConfigSysteme.AjouterAuditLog(
                                "Client Désactivé",
                                $"Désactivation client ID={idClient} | {nom} {prenom}",
                                "Succès");
                        }
                        catch { }

                        tx.Commit();
                    }
                }

                MessageBox.Show("✅ Client désactivé.");
                AfficherClients();
                ViderChampsClient();
            }
            catch (SqlException exSql)
            {
                MessageBox.Show("Erreur SQL : " + exSql.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }




        private void dgvClients_SelectionChanged(object sender, EventArgs e)
        {
            var row = (dgvClients.SelectedRows.Count > 0) ? dgvClients.SelectedRows[0] : dgvClients.CurrentRow;
            if (row == null) return;

            txtID.Text = row.Cells["ID_Clients"]?.Value?.ToString() ?? "";
            txtNom.Text = row.Cells["Nom"]?.Value?.ToString() ?? "";
            txtPrenom.Text = row.Cells["Prenom"]?.Value?.ToString() ?? "";
            txtAdresse.Text = row.Cells["Adresse"]?.Value?.ToString() ?? "";
            txtTelephone.Text = row.Cells["Telephone"]?.Value?.ToString() ?? "";
            txtEmail.Text = row.Cells["Email"]?.Value?.ToString() ?? "";
            txtCodeCarte.Text = row.Cells["CodeCarte"]?.Value?.ToString() ?? "";
        }
        private void AfficherClients()
        {
            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();

                    string sql = USE_SOFT_DELETE
                        ? @"
SELECT ID_Clients, Nom, Prenom, Adresse, Telephone, Email, CodeCarte
FROM dbo.Clients
WHERE Actif = 1
ORDER BY Nom, Prenom;"
                        : @"
SELECT ID_Clients, Nom, Prenom, Adresse, Telephone, Email, CodeCarte
FROM dbo.Clients
ORDER BY Nom, Prenom;";

                    using (var cmd = new SqlCommand(sql, cn))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        dgvClients.DataSource = null;
                        dgvClients.DataSource = dt;
                    }
                }

                // ✅ Config grid (à chaque refresh : ok)
                dgvClients.MultiSelect = true;
                dgvClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvClients.ScrollBars = ScrollBars.Both;
                dgvClients.ReadOnly = true;
                dgvClients.AllowUserToAddRows = false;
                dgvClients.AllowUserToResizeRows = false;
                dgvClients.RowHeadersVisible = false;
                dgvClients.EnableHeadersVisualStyles = false;

                // ✅ Styling "one-shot" (évite recréer Font à chaque fois)
                if (!_gridStyledOnce)
                {
                    dgvClients.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    dgvClients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);

                    dgvClients.ColumnHeadersDefaultCellStyle.Font = _gridHeaderFont;
                    dgvClients.DefaultCellStyle.Font = _gridFont;

                    dgvClients.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvClients.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    _gridStyledOnce = true;
                }

                // ✅ Headers + hide
                if (dgvClients.Columns.Contains("ID_Clients"))
                {
                    dgvClients.Columns["ID_Clients"].HeaderText = "ID";
                    dgvClients.Columns["ID_Clients"].Visible = false;
                }
                if (dgvClients.Columns.Contains("Telephone")) dgvClients.Columns["Telephone"].HeaderText = "Téléphone";
                if (dgvClients.Columns.Contains("CodeCarte")) dgvClients.Columns["CodeCarte"].HeaderText = "Carte";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des clients : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Implémentation de ITraduisible
        public void AppliquerLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        // Méthode exemple pour appliquer thème (à adapter selon ta logique)
        public void AppliquerTheme(string theme)
        {
            Color backColor;
            Color foreColor;

            if (theme == "Sombre")
            {
                backColor = Color.FromArgb(30, 30, 30);
                foreColor = Color.White;
            }
            else
            {
                backColor = SystemColors.Control;
                foreColor = Color.Black;
            }

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            foreach (Control ctrl in this.Controls)
            {
                if (!(ctrl is Button))
                {
                    ctrl.BackColor = backColor;
                    ctrl.ForeColor = foreColor;
                }
            }
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();

                    string sql = @"
INSERT INTO dbo.Clients (Nom, Prenom, Adresse, Telephone, Email)
VALUES (@Nom, @Prenom, @Adresse, @Telephone, @Email);";

                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 120).Value = (txtNom.Text ?? "").Trim();
                        cmd.Parameters.Add("@Prenom", SqlDbType.NVarChar, 120).Value = (txtPrenom.Text ?? "").Trim();
                        cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 200).Value = (txtAdresse.Text ?? "").Trim();
                        cmd.Parameters.Add("@Telephone", SqlDbType.NVarChar, 40).Value = (txtTelephone.Text ?? "").Trim();

                        string email = (txtEmail.Text ?? "").Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value =
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog(
                    "Ajout Client",
                    $"Client ajouté : {(txtNom.Text ?? "").Trim()} {(txtPrenom.Text ?? "").Trim()} | Tel={(txtTelephone.Text ?? "").Trim()}",
                    "Succès"
                );

                MessageBox.Show("✅ Client ajouté avec succès !");
                AfficherClients();
                ViderChampsClient();
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog(
                    "Ajout Client",
                    $"Erreur ajout client : {(txtNom.Text ?? "").Trim()} {(txtPrenom.Text ?? "").Trim()} | {ex.Message}",
                    "Échec"
                );

                MessageBox.Show("Erreur lors de l'ajout : " + ex.Message);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || !int.TryParse(txtID.Text.Trim(), out int id) || id <= 0)
            {
                MessageBox.Show("Sélectionnez un client à modifier");
                return;
            }

            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();

                    string sql = @"
UPDATE dbo.Clients
SET Nom=@Nom,
    Prenom=@Prenom,
    Adresse=@Adresse,
    Telephone=@Telephone,
    Email=@Email
WHERE ID_Clients=@ID;";

                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

                        cmd.Parameters.Add("@Nom", SqlDbType.NVarChar, 120).Value = (txtNom.Text ?? "").Trim();
                        cmd.Parameters.Add("@Prenom", SqlDbType.NVarChar, 120).Value = (txtPrenom.Text ?? "").Trim();
                        cmd.Parameters.Add("@Adresse", SqlDbType.NVarChar, 200).Value = (txtAdresse.Text ?? "").Trim();
                        cmd.Parameters.Add("@Telephone", SqlDbType.NVarChar, 40).Value = (txtTelephone.Text ?? "").Trim();

                        string email = (txtEmail.Text ?? "").Trim();
                        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value =
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("✅ Client modifié avec succès !");
                AfficherClients();
                ViderChampsClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la modification : " + ex.Message);
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            SupprimerClientSelectionne();
        }


        private void btnRafraichir_Click(object sender, EventArgs e)
        {
            AfficherClients();
        }

        private void dgvClients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
        private string BuildCodeCarte(int idClient)
        {
            return "FID-" + idClient.ToString("D6"); // FID-000025
        }
        private void ViderChampsClient()
        {
            txtID.Clear();
            txtNom.Clear();
            txtPrenom.Clear();
            txtAdresse.Clear();
            txtTelephone.Clear();
            txtEmail.Clear();

            // ✅ Nouveau
            if (txtCodeCarte != null)
                txtCodeCarte.Clear();
        }

        private void btnTestconnexion_Click(object sender, EventArgs e)
        {
            try
            {
                using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    cn.Open();

                    // Optionnel : petit ping SQL pour confirmer que tout marche
                    using (var cmd = new SqlCommand("SELECT 1;", cn))
                    {
                        cmd.CommandTimeout = 5;
                        cmd.ExecuteScalar();
                    }
                }

                MessageBox.Show("✅ Connexion SQL réussie !");
            }
            catch (SqlException exSql)
            {
                MessageBox.Show("❌ Échec connexion SQL : " + exSql.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Échec de la connexion : " + ex.Message);
            }
        }

        private void btnGenererCarte_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedClientFromGrid(out int idClient, out string codeCarteFromGrid))
                return;

            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();

                    // 1) déjà un code ?
                    string existing = "";
                    using (var cmd = new SqlCommand("SELECT ISNULL(CodeCarte,'') FROM dbo.Clients WHERE ID_Clients=@id;", cn))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                        existing = (cmd.ExecuteScalar() ?? "").ToString().Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(existing))
                    {
                        txtCodeCarte.Text = existing;
                        MessageBox.Show("Ce client a déjà une carte : " + existing);
                        return;
                    }

                    // 2) Génère
                    string codeCarte = BuildCodeCarte(idClient);

                    // 3) Sauvegarde seulement si vide
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Clients
SET CodeCarte=@c
WHERE ID_Clients=@id
  AND (CodeCarte IS NULL OR LTRIM(RTRIM(CodeCarte))='');", cn))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = codeCarte;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                        cmd.ExecuteNonQuery();
                    }

                    ConfigSysteme.AjouterAuditLog(
                        "Carte Fidélité",
                        $"Génération carte client ID={idClient} | Code={codeCarte}",
                        "Succès"
                    );

                    txtCodeCarte.Text = codeCarte;
                    MessageBox.Show("✅ Carte générée : " + codeCarte);

                    AfficherClients();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private bool TryGetSelectedClientFromGrid(out int idClient, out string codeCarte)
        {
            idClient = 0;
            codeCarte = "";

            DataGridViewRow row = null;

            // ✅ 1) Priorité : SelectedRows
            if (dgvClients.SelectedRows != null && dgvClients.SelectedRows.Count > 0)
                row = dgvClients.SelectedRows[0];
            else if (dgvClients.CurrentRow != null) // ✅ 2) sinon CurrentRow
                row = dgvClients.CurrentRow;

            if (row == null)
            {
                MessageBox.Show("Sélectionnez un client dans la liste.");
                return false;
            }

            // ✅ lire ID directement depuis la grille
            object vId = row.Cells["ID_Clients"]?.Value;
            if (vId == null || vId == DBNull.Value || !int.TryParse(vId.ToString(), out idClient) || idClient <= 0)
            {
                MessageBox.Show("ID client invalide (ligne sélectionnée).");
                return false;
            }

            // ✅ lire CodeCarte depuis la grille (ou txtCodeCarte si tu veux)
            object vCode = row.Cells["CodeCarte"]?.Value;
            codeCarte = (vCode == null || vCode == DBNull.Value) ? "" : vCode.ToString().Trim();

            // Si vide -> essaie txtCodeCarte (si déjà rempli)
            if (string.IsNullOrWhiteSpace(codeCarte))
                codeCarte = (txtCodeCarte.Text ?? "").Trim();

            // Pour impression on exige CodeCarte (mais pour "Générer", non)
            return true;
        }

        private void btnImprimerPlastification_Click(object sender, EventArgs e)
        {
            ResetPrintDoc(); // ✅ AJOUTE ÇA
            // 1) Récupère jusqu’à 8 clients sélectionnés
            var ids = GetSelectedClientIds(8);

            if (ids.Count == 0)
            {
                MessageBox.Show("Sélectionnez 1 à 8 clients dans la liste (Ctrl+Click / Shift+Click).");
                return;
            }

            // 2) Charger infos + vérifier CodeCarte
            _cardsToPrint.Clear();

            foreach (int idClient in ids)
            {
                var info = LoadClientCardInfo(idClient);
                if (info == null)
                {
                    MessageBox.Show("Client introuvable ID=" + idClient);
                    return;
                }

                if (string.IsNullOrWhiteSpace(info.CodeCarte))
                {
                    MessageBox.Show($"Le client '{info.NomComplet}' n'a pas de CodeCarte. Générez d'abord sa carte.");
                    return;
                }

                _cardsToPrint.Add(info);
            }

            // 3) Remplir la page à 8 cartes : si moins de 8, on ajoute des cartes "vides"
            while (_cardsToPrint.Count < 8)
                _cardsToPrint.Add(null);

            _printMode = "PLAST";
            _printIndex = 0;

            _printDoc = new PrintDocument();
            _printDoc.DocumentName = "Cartes Fidelite - Plastification (A4) - Batch";
            _printDoc.DefaultPageSettings.Landscape = false;
            _printDoc.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);

            _printDoc.PrintPage -= PrintDoc_Plastification_PrintPage; // sécurité
            _printDoc.PrintPage += PrintDoc_Plastification_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        ConfigSysteme.AjouterAuditLog(
                            "IMPRESSION_CARTE",
                            $"Mode={_printMode} | Clients={string.Join(",", ids)}",
                            "OK"
                        );
                    }
                    catch { }
                {
                    _printDoc.Print();
                }
            }
        }

        private void PrintDoc_Plastification_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle page = e.MarginBounds;

            int cols = 2;
            int rows = 4;

            int cellW = page.Width / cols;
            int cellH = page.Height / rows;

            // ✅ Carte ISO ratio (comme PVC) => évite écrasement
            int maxW = (int)(cellW * 0.95);
            int maxH = (int)(cellH * 0.92);

            int cardW = maxW;
            int cardH = (int)(cardW / 1.593);
            if (cardH > maxH)
            {
                cardH = maxH;
                cardW = (int)(cardH * 1.593);
            }

            int idx = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (idx >= 8) break;

                    int x = page.Left + c * cellW + (cellW - cardW) / 2;
                    int y = page.Top + r * cellH + (cellH - cardH) / 2;

                    Rectangle card = new Rectangle(x, y, cardW, cardH);

                    // Page 1 = Recto, Page 2 = Verso
                    if (_printIndex == 0)
                    {
                        var info = _cardsToPrint[idx];
                        if (info != null)
                            DrawCardRecto(e.Graphics, card, info);
                        else
                            DrawCardVide(e.Graphics, card); // optionnel (voir méthode en bas)
                    }
                    else
                    {
                        DrawCardVerso(e.Graphics, card);
                    }

                    idx++;
                }
            }

            // ✅ 2 pages : recto puis verso
            if (_printIndex == 0)
            {
                _printIndex = 1;
                e.HasMorePages = true;
            }
            else
            {
                _printIndex = 0;
                e.HasMorePages = false;
            }
        }
        private void DrawCardVide(Graphics g, Rectangle card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, card);

            using (var p = new Pen(Color.FromArgb(220, 220, 220), 2f))
                g.DrawRectangle(p, card);

            using (var f = new Font("Segoe UI", 10, FontStyle.Italic))
            using (var brT = new SolidBrush(Color.FromArgb(150, 150, 150)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("CASE VIDE", f, brT, card, fmt);
            }
        }

        private void btnImprimerPVC_Click(object sender, EventArgs e)
        {
            ResetPrintDoc(); // ✅ AJOUTE ÇA
            if (!TryGetSelectedClientFromGrid(out int idClient, out string codeCarte)) return;
            if (string.IsNullOrWhiteSpace(codeCarte))
            {
                MessageBox.Show("Ce client n'a pas encore de CodeCarte. Cliquez d'abord sur 'Générer Carte'.");
                return;
            }

            var info = LoadClientCardInfo(idClient);
            if (info == null || string.IsNullOrWhiteSpace(info.CodeCarte))
            {
                MessageBox.Show("Impossible de charger le client / CodeCarte.");
                return;
            }

            _cardsToPrint.Clear();
            _cardsToPrint.Add(info);

            _printMode = "PVC";
            _printIndex = 0;

            _printDoc = new PrintDocument();
            _printDoc.DocumentName = "Carte Fidelite - PVC";
            _printDoc.DefaultPageSettings.Landscape = true;

            // Marges petites (PVC imprimante)
            _printDoc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

            _printDoc.PrintPage -= PrintDoc_PVC_PrintPage;
            _printDoc.PrintPage += PrintDoc_PVC_PrintPage;

            using (var dlg = new PrintDialog())
            {
                dlg.Document = _printDoc;
                if (dlg.ShowDialog() == DialogResult.OK)
                    try
                    {
                        ConfigSysteme.AjouterAuditLog(
                            "IMPRESSION_CARTE",
                            $"Mode={_printMode} | Client={info.IdClient} | Code={info.CodeCarte}",
                            "OK"
                        );
                    }
                    catch { }
                {
                    
                    _printDoc.Print();
                }
            }
        }

        private Bitmap _logoCache;     // cache en mémoire
        private byte[] _logoBytesCache;

        // Charge le logo une seule fois en mémoire (byte[]) puis Bitmap cloné
        private Bitmap GetLogoCached()
        {
            try
            {
                if (_logoCache != null) return _logoCache;
                if (!System.IO.File.Exists(_logoPath)) return null;

                // Lire en mémoire => pas de lock du fichier
                _logoBytesCache = System.IO.File.ReadAllBytes(_logoPath);

                using (var ms = new System.IO.MemoryStream(_logoBytesCache))
                using (var img = Image.FromStream(ms))
                {
                    _logoCache = new Bitmap(img); // clone
                }

                return _logoCache;
            }
            catch
            {
                return null;
            }
        }

        // Libérer correctement
        private void DisposeLogoCache()
        {
            try
            {
                if (_logoCache != null)
                {
                    _logoCache.Dispose();
                    _logoCache = null;
                }
                _logoBytesCache = null;
            }
            catch { }
        }

        private void PrintDoc_PVC_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var info = _cardsToPrint[0];

            Rectangle page = e.MarginBounds;

            // Carte ISO (approx) : 86mm x 54mm (ratio 1.593)
            // On adapte au max dans la page
            int cardW = page.Width;
            int cardH = (int)(cardW / 1.593);

            if (cardH > page.Height)
            {
                cardH = page.Height;
                cardW = (int)(cardH * 1.593);
            }

            int x = page.Left + (page.Width - cardW) / 2;
            int y = page.Top + (page.Height - cardH) / 2;

            Rectangle card = new Rectangle(x, y, cardW, cardH);

            // Page 1 = Recto, Page 2 = Verso
            if (_printIndex == 0)
            {
                DrawCardRecto(e.Graphics, card, info);
                _printIndex++;
                e.HasMorePages = true; // demande une 2e page
            }
            else
            {
                DrawCardVerso(e.Graphics, card);
                e.HasMorePages = false;
                _printIndex = 0;
            }
        }

        private void btnCompteCredit_Click(object sender, EventArgs e)
        {
            int idClient = GetClientId();
            if (idClient <= 0)
            {
                MessageBox.Show("Sélectionne un client d'abord.");
                return;
            }

            using (var f = new FormCreditManager(idClient))
                f.ShowDialog(this);
        }

        private void btnHistoriqueAchats_Click(object sender, EventArgs e)
        {
            int idClient = GetClientId();
            if (idClient <= 0)
            {
                MessageBox.Show("Sélectionne un client d'abord.");
                return;
            }

            using (var f = new FormHistoriqueAchats(idClient))
                f.ShowDialog(this);
        }

        private void btnCoupons_Click(object sender, EventArgs e)
        {
            using (var f = new FormCoupons())
                f.ShowDialog(this);
        }

        private void btnLoyaltyMouvements_Click(object sender, EventArgs e)
        {
            int idClient = GetClientId();
            if (idClient <= 0)
            {
                MessageBox.Show("Sélectionne un client d'abord.");
                return;
            }

            using (var f = new FormLoyaltyMouvements(idClient))
                f.ShowDialog(this);
        }

        private void btnCreditClients_Click(object sender, EventArgs e)
        {
            try
            {
                using (var f = new FrmCreditsClients()) // ✅ ton formulaire liste crédits
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ouverture crédits clients : " + ex.Message);
            }
        }

        private void btnEncaisserCredit_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fList = new FrmCreditsClients())
                {
                    fList.StartPosition = FormStartPosition.CenterParent;

                    if (fList.ShowDialog(this) != DialogResult.OK)
                        return;

                    int idCredit = fList.SelectedCreditId;
                    decimal reste = fList.SelectedReste;
                    string clientNom = fList.SelectedClientNom;

                    if (idCredit <= 0)
                    {
                        MessageBox.Show("Aucun crédit sélectionné.");
                        return;
                    }

                    // ✅ ordre corrigé (int, string, decimal)
                    using (var fPay = new FrmEncaisserCredit(idCredit, clientNom, reste))
                    {
                        fPay.StartPosition = FormStartPosition.CenterParent;
                        fPay.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ouverture encaissement crédit : " + ex.Message);
            }
        }
    }
}


