using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed;
using iTextSharp.text;                 // Pour Rectangle, Document, Paragraph, etc.
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Org.BouncyCastle.Math.EC;
using ZXing;
using static BoutiqueRebuildFixed.ConfigSysteme;
using static BoutiqueRebuildFixed.FormVentes;
using DFont = System.Drawing.Font;
using DRectangle = System.Drawing.Rectangle;
using DRectangleF = System.Drawing.RectangleF;
using DSize = System.Drawing.Size;
using DSizeF = System.Drawing.SizeF;
using IFont = iTextSharp.text.Font;
using IRectangle = iTextSharp.text.Rectangle;
using WinTimer = System.Windows.Forms.Timer;

namespace BoutiqueRebuildFixed
{

    public partial class FormVentes : Form, ConfigSysteme.IContextMenuHandler
    {
        // ====== SERVICES / REPOS ======
        private readonly ProduitRepository _produitRepo;

        // ====== FACTURE basée sur ID_Vente ======
        private int _lastIdVente = 0;
        private string _lastCodeFacture = null;
        private bool _suspendComboEvents = false;
        private decimal _netFromClientRapide = 0m;
        private string _deviseFromClientRapide = "CDF";


        // ====== Recherche scalable pour Combo DGV ======
        private WinTimer _timerProduitSearch;
        private ComboBox _comboProduitEditing;
        private string _pendingPrefix = "";
        private List<ProduitCombo> _lastSearchResults = new List<ProduitCombo>();

        private readonly StockService stockService = new StockService(ConfigSysteme.ConnectionString);
        private readonly string connectionString = ConfigSysteme.ConnectionString;
        private Dictionary<string, string> dictRefProduits = new Dictionary<string, string>();
        private List<string> tousLesProduits = new List<string>();
        private int _idProduitCourant = 0;
        private string _refProduitCourant = "";
        private BindingList<ProduitCombo> _produitsCacheDgv = new BindingList<ProduitCombo>();
        // ====== FIDELITE / CARTE CLIENT ======
        private decimal _lastTauxFidelite = 0.005m;
        private decimal _lastGainFidelite = 0m;
        private decimal _lastSoldeFideliteCDF = 0m;
        private decimal _lastSoldeFideliteUSD = 0m;
        private string _lastCodeCarteClient = "";
        private VentePrintModel _printModel = null;
        private int _printLineIndex = 0; // pour pagination
        private string _codeFactureCourant;
        // ✅ Services
        private readonly CompteClientService _compteSvc;
        private readonly CreditService _creditSvc;
        private readonly CouponService _couponSvc;
        private readonly LoyaltyService _loyalSvc;
        private readonly ClientStatsService _statsSvc;
        private OrdonnanceVenteDTO _ordonnanceCourante = null;
        private bool _comboHooksInstalled = false;
        private bool _updatingRow = false;
        private List<ProduitCombo> _produits;
        private bool _forceFocusQuantite = false;


        private System.Windows.Forms.Timer _tmrCodeBarreDebounce;
        private ListBox _lstCodeSuggestions;
        private bool _loadingSuggestions = false;
        private bool _ignoreTxtChange = false;
        private string _lastScanValue = "";
        private DateTime _lastScanAt = DateTime.MinValue;
        private ComboBox _comboScanEditing;
        private List<FormPaiementsVente.PaiementLine> _payLinesFromClientRapide = null;
        private OrdonnanceVenteDTO _ordonnanceFromClientRapide = null;

        // ✅ code facture "en cours" (généré AVANT l'ouverture ClientRapide)
        private string _codeFactureEnCours = "";



        // 🔥 état pour gérer focus & popup
        private bool _qtyEditStarted = false;
        private bool _openClientAfterQty = false;

        // (optionnel) mémoriser la ligne en cours
        private int _lastRowIndexForQty = -1;

        private enum ScanOutcome
        {
            None,
            NotFoundKeepFocusCodeBarre,     // produit introuvable => rester sur txtCodeBarre
            SuggestionsShownKeepFocus,      // suggestions visibles => attendre choix user
            AddedAndQtyEditStarted          // produit ajouté => édition quantite démarrée
        }

        private sealed class SuggestItem
        {
            public int ID { get; set; }
            public string CodeBarre { get; set; }
            public string Text { get; set; }
        }

        private sealed class CodeSearchItem
        {
            public int ID { get; set; }
            public string Code { get; set; }
            public string Text { get; set; }   // affichage combo

            public override string ToString() => Text;
        }

        public class ClientRapideResult
        {
            public bool Ok { get; set; }

            public string Nom { get; set; } = "";
            public string Telephone { get; set; } = "";

            public string CouponCode { get; set; } = "";
            public bool VenteACredit { get; set; }
            public System.DateTime EcheanceCredit { get; set; }

            public string Emplacement { get; set; } = "";
        }

        // =======================
        // SQL RAPPORTS (TOUT CALCULE)
        // =======================

        private const string SQL_HEBDO = @"
DECLARE @d1 datetime = DATEADD(day, -6, CONVERT(date, GETDATE())); -- 7 jours: aujourd'hui + 6 jours arrière
DECLARE @d2 datetime = DATEADD(day, 1, CONVERT(date, GETDATE()));

;WITH P AS
(
    SELECT
        CONVERT(date, v.DateVente) AS Jour,
        v.ID_Vente,
        pv.IdPaiement,
        UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF')))) AS DevisePay,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN 0 ELSE ISNULL(pv.Montant,0)
        END AS MontantValide,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.Statut,'')))) = 'REGLEMENT_CREDIT'
             AND UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END AS MontantReglementCredit
    FROM dbo.Vente v
    LEFT JOIN dbo.PaiementsVente pv ON pv.IdVente = v.ID_Vente
    WHERE v.DateVente >= @d1 AND v.DateVente < @d2
)
SELECT
    Jour,
    COUNT(DISTINCT ID_Vente) AS NbVentes,
    COUNT(IdPaiement) AS NbPaiements,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantValide ELSE 0 END) AS Encaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantValide ELSE 0 END) AS Encaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantValide ELSE 0 END) AS Encaisse_EUR,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_EUR
FROM P
GROUP BY Jour
ORDER BY Jour DESC;
";

        private const string SQL_MENSUEL = @"
DECLARE @d1 date = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @d2 date = DATEADD(month, 1, @d1);

;WITH P AS
(
    SELECT
        DATEPART(week, v.DateVente) AS Semaine,
        v.ID_Vente,
        pv.IdPaiement,
        UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF')))) AS DevisePay,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN 0 ELSE ISNULL(pv.Montant,0)
        END AS MontantValide,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.Statut,'')))) = 'REGLEMENT_CREDIT'
             AND UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END AS MontantReglementCredit
    FROM dbo.Vente v
    LEFT JOIN dbo.PaiementsVente pv ON pv.IdVente = v.ID_Vente
    WHERE v.DateVente >= @d1 AND v.DateVente < @d2
)
SELECT
    Semaine,
    COUNT(DISTINCT ID_Vente) AS NbVentes,
    COUNT(IdPaiement) AS NbPaiements,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantValide ELSE 0 END) AS Encaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantValide ELSE 0 END) AS Encaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantValide ELSE 0 END) AS Encaisse_EUR,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_EUR
FROM P
GROUP BY Semaine
ORDER BY Semaine DESC;
";

        private const string SQL_ANNUEL = @"
DECLARE @d1 date = DATEFROMPARTS(YEAR(GETDATE()), 1, 1);
DECLARE @d2 date = DATEADD(year, 1, @d1);

;WITH P AS
(
    SELECT
        MONTH(v.DateVente) AS Mois,
        v.ID_Vente,
        pv.IdPaiement,
        UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF')))) AS DevisePay,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN 0 ELSE ISNULL(pv.Montant,0)
        END AS MontantValide,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.Statut,'')))) = 'REGLEMENT_CREDIT'
             AND UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END AS MontantReglementCredit
    FROM dbo.Vente v
    LEFT JOIN dbo.PaiementsVente pv ON pv.IdVente = v.ID_Vente
    WHERE v.DateVente >= @d1 AND v.DateVente < @d2
)
SELECT
    Mois,
    COUNT(DISTINCT ID_Vente) AS NbVentes,
    COUNT(IdPaiement) AS NbPaiements,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantValide ELSE 0 END) AS Encaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantValide ELSE 0 END) AS Encaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantValide ELSE 0 END) AS Encaisse_EUR,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_EUR
FROM P
GROUP BY Mois
ORDER BY Mois DESC;
";

        private const string SQL_CAISSIER = @"
DECLARE @d1 datetime = DATEADD(day, -30, CONVERT(date, GETDATE()));
DECLARE @d2 datetime = DATEADD(day, 1, CONVERT(date, GETDATE()));

;WITH P AS
(
    SELECT
        LTRIM(RTRIM(ISNULL(v.NomCaissier,''))) AS Caissier,
        v.ID_Vente,
        pv.IdPaiement,
        UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF')))) AS DevisePay,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN 0 ELSE ISNULL(pv.Montant,0)
        END AS MontantValide,
        CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.Statut,'')))) = 'REGLEMENT_CREDIT'
             AND UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END AS MontantReglementCredit
    FROM dbo.Vente v
    LEFT JOIN dbo.PaiementsVente pv ON pv.IdVente = v.ID_Vente
    WHERE v.DateVente >= @d1 AND v.DateVente < @d2
)
SELECT
    Caissier,
    COUNT(DISTINCT ID_Vente) AS NbVentes,
    COUNT(IdPaiement) AS NbPaiements,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantValide ELSE 0 END) AS Encaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantValide ELSE 0 END) AS Encaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantValide ELSE 0 END) AS Encaisse_EUR,
    SUM(CASE WHEN DevisePay='CDF' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_CDF,
    SUM(CASE WHEN DevisePay='USD' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_USD,
    SUM(CASE WHEN DevisePay='EUR' THEN MontantReglementCredit ELSE 0 END) AS CreditEncaisse_EUR
FROM P
GROUP BY Caissier
ORDER BY Encaisse_USD DESC, Encaisse_CDF DESC, Encaisse_EUR DESC;
";


        public void ApplyGain(int idClient, int idVente, decimal montantBase, SqlConnection con, SqlTransaction trans)
        {
            if (idClient <= 0 || idVente <= 0) return;

            // 1) ✅ Anti-duplication (à mettre ici)
            using (var cmdChk = new SqlCommand(@"
SELECT COUNT(1) FROM dbo.LoyaltyMouvement
WHERE IdClient=@c AND RefVente=@v AND Type='GAIN';", con, trans))
            {
                cmdChk.Parameters.Add("@c", SqlDbType.Int).Value = idClient;
                cmdChk.Parameters.Add("@v", SqlDbType.Int).Value = idVente;

                if (Convert.ToInt32(cmdChk.ExecuteScalar()) > 0)
                    return; // déjà crédité
            }
        }

        private void TxtCodeBarreParent_Resize(object sender, EventArgs e) => RepositionnerListeSuggestions();
        private void TxtCodeBarre_LayoutChanged(object sender, EventArgs e) => RepositionnerListeSuggestions();

        private void InjectClientRapide(FormClientsRapide f)
        {
            if (f == null) return;

            // ===================== 1) NOM + TEL =====================
            cmbNomClient.Text = (f.NomClientChoisi ?? "").Trim();
            txtTelephone.Text = (f.TelephoneChoisi ?? "").Trim();

            // ===================== 2) COUPON =====================
            txtCouponCode.Text = (f.CouponChoisi ?? "").Trim();

            // ===================== 3) CREDIT + ECHEANCE =====================
            chkVenteCredit.Checked = f.VenteCreditChoisi;

            dtpEcheanceCredit.Value = (f.EcheanceChoisie == default)
                ? DateTime.Today.AddDays(30)
                : f.EcheanceChoisie.Date;

            // ===================== 4) EMPLACEMENT =====================
            if (!string.IsNullOrWhiteSpace(f.EmplacementChoisi) && cmbEmplacement != null)
            {
                string emp = f.EmplacementChoisi.Trim();

                if (cmbEmplacement.Items.Contains(emp))
                {
                    cmbEmplacement.SelectedItem = emp;
                }
                else
                {
                    // si DropDownList -> on ne peut pas mettre Text librement
                    if (cmbEmplacement.DropDownStyle == ComboBoxStyle.DropDown)
                        cmbEmplacement.Text = emp;
                    else if (cmbEmplacement.Items.Count > 0)
                        cmbEmplacement.SelectedIndex = 0; // fallback
                }
            }

            // ===================== 5) CATEGORIE CLIENT (si existe dans FormVentes) =====================
            var found = this.Controls.Find("cmbCategorieClient", true).FirstOrDefault();
            if (found is ComboBox cmbCat)
            {
                string cat = (f.CategorieClientChoisie ?? "OCCASIONNEL").Trim().ToUpperInvariant();

                if (cmbCat.Items.Contains(cat))
                    cmbCat.SelectedItem = cat;
                else if (cmbCat.DropDownStyle == ComboBoxStyle.DropDown)
                    cmbCat.Text = cat;
            }

            // ===================== 6) PAIEMENTS : stocker pour btnFinaliser_Click =====================
            _payLinesFromClientRapide = (f.PaiementsChoisis != null && f.PaiementsChoisis.Count > 0)
                ? f.PaiementsChoisis.ToList()
                : null;

            // ===================== 7) ORDONNANCE : stocker pour btnFinaliser_Click =====================
            _ordonnanceFromClientRapide = f.OrdonnanceChoisie;
        }

        public class VentePrintModel
        {
            public int IdVente { get; set; }
            public int IdClient { get; set; }
            public string CodeFacture { get; set; }
            public DateTime DateVente { get; set; }
            public string Caissier { get; set; }
            public string ClientNom { get; set; }
            public string Devise { get; set; }
            public string ModePaiement { get; set; }
            public decimal MontantTotal { get; set; }
            public string Statut { get; set; }
            public string CategorieClient { get; set; } = "OCCASIONNEL";
            public List<VenteLignePrintModel> Lignes { get; set; } = new List<VenteLignePrintModel>();
        }
        public decimal NetBoutique { get; set; }



        public class VenteLignePrintModel
        {
            public string NomProduit { get; set; }
            public int Quantite { get; set; }
            public decimal PrixUnitaire { get; set; }
            public decimal Montant { get; set; }
        }

        public class CouponCalcResult
        {
            public bool Ok { get; set; }
            public string Message { get; set; }

            public decimal Total { get; set; }           // ✅ AJOUT
            public decimal RemiseTotale { get; set; }
            public decimal RemiseClient { get; set; }
            public decimal CreditPartenaire { get; set; }
            public decimal NetAPayer { get; set; }
        }

        public class ProduitScanInfo
        {
            public int ID { get; set; }
            public string NomProduit { get; set; }
            public string RefProduit { get; set; }
            public string CodeBarre { get; set; }
            public decimal Prix { get; set; }
            public string Devise { get; set; }

            public override string ToString()
            {
                // affichage dans la liste
                return $"{CodeBarre}  |  {NomProduit}  |  {Prix:0.00} {Devise}";
            }
        }

        public static CouponCalcResult CalculerCoupon(decimal total, decimal taux, decimal plafond, bool partenairePartage)
        {
            var res = new CouponCalcResult();

            decimal remiseTotale = Math.Round(Math.Min(total * taux, plafond), 2);

            decimal remiseClient = remiseTotale;
            decimal creditPartenaire = 0m;

            if (partenairePartage)
            {
                remiseClient = Math.Round(remiseTotale * 0.50m, 2);
                creditPartenaire = remiseTotale - remiseClient;
            }

            decimal netClient = total - remiseTotale;

            res.Total = Math.Round(total, 2);
            res.RemiseTotale = remiseTotale;
            res.RemiseClient = Math.Round(remiseClient, 2);
            res.CreditPartenaire = Math.Round(creditPartenaire, 2);
            res.NetAPayer = Math.Round(netClient, 2);
            res.Ok = true;
            res.Message = "OK";

            return res;
        }

        private object GetCellValue(DataGridViewRow r, int index)
        {
            if (r == null) return null;
            if (index < 0 || index >= r.Cells.Count) return null;
            return r.Cells[index]?.Value;
        }

        private object GetCellValue(DataGridViewRow r, params string[] names)
        {
            if (r == null) return null;

            foreach (var n in names)
            {
                if (string.IsNullOrWhiteSpace(n)) continue;
                if (r.DataGridView.Columns.Contains(n))
                    return r.Cells[n]?.Value;
            }
            return null;
        }


        public static CouponCalcResult CalculerCoupon(
    decimal total,
    string type,
    decimal valeur,
    decimal minAchat,
    int? idPartenaire,
    decimal sharePct
)
        {
            if (total <= 0m)
                return new CouponCalcResult { Ok = false, Message = "Total invalide." };

            if (total < minAchat)
                return new CouponCalcResult { Ok = false, Message = $"Achat minimum {minAchat:N2} non atteint." };

            var res = new CouponCalcResult();

            decimal remiseTotale = 0m;

            type = (type ?? "").Trim().ToUpperInvariant();
            if (type == "POURCENT")
                remiseTotale = total * (valeur / 100m);
            else if (type == "FIXE")
                remiseTotale = valeur;
            else
                return new CouponCalcResult { Ok = false, Message = "Type coupon invalide." };

            if (remiseTotale < 0m) remiseTotale = 0m;
            if (remiseTotale > total) remiseTotale = total;

            decimal creditPartenaire = 0m;
            decimal remiseClient = remiseTotale;

            if (idPartenaire.HasValue && idPartenaire.Value > 0)
            {
                if (sharePct < 0m) sharePct = 0m;
                if (sharePct > 100m) sharePct = 100m;

                creditPartenaire = Math.Round(remiseTotale * (sharePct / 100m), 2);
                remiseClient = remiseTotale - creditPartenaire;
            }

            decimal netClient = total - remiseTotale;

            res.Ok = true;
            res.RemiseTotale = Math.Round(remiseTotale, 2);
            res.CreditPartenaire = Math.Round(creditPartenaire, 2);
            res.RemiseClient = Math.Round(remiseClient, 2);
            res.NetAPayer = Math.Round(netClient, 2);
            res.Message = "OK";

            return res;
        }

        private decimal LireDecimalFR(string s)
        {
            s = (s ?? "").Trim().Replace("\u00A0", "").Replace(" ", "");
            decimal v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v)) return v;
            s = s.Replace(".", ",");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v)) return v;
            return 0m;
        }

        private List<BoutiqueRebuildFixed.OrdonnanceLigneDTO> ConstruireLignesOrdonnanceDepuisPanier()
        {
            var list = new List<BoutiqueRebuildFixed.OrdonnanceLigneDTO>();

            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                // ===== ID Produit
                int idProduit = 0;
                int.TryParse(Convert.ToString(r.Cells["ID_Produit"]?.Value), out idProduit);

                // ===== Article (NomProduit affiché, PAS la Value=ID)
                string article = "";

                // 1) le combo NomProduit : FormattedValue = texte affiché
                if (dgvPanier.Columns.Contains("NomProduit"))
                {
                    article = Convert.ToString(r.Cells["NomProduit"]?.FormattedValue ?? "").Trim();
                }

                // 2) fallback : si vide, prendre la Référence (RefProduit)
                if (string.IsNullOrWhiteSpace(article))
                {
                    article = Convert.ToString(r.Cells["RefProduit"]?.Value ?? "").Trim();
                }

                // sécurité finale
                if (string.IsNullOrWhiteSpace(article))
                    article = "ARTICLE";

                // ===== Qté
                int qte = 1;
                int.TryParse(Convert.ToString(r.Cells["Quantite"]?.Value), out qte);
                if (qte <= 0) qte = 1;

                // ===== PU
                decimal pu = LireDecimalFR(Convert.ToString(r.Cells["PrixUnitaire"]?.Value));

                // ===== Devise
                string dev = Convert.ToString(r.Cells["Devise"]?.Value ?? "").Trim();

                list.Add(new BoutiqueRebuildFixed.OrdonnanceLigneDTO
                {
                    IdProduit = idProduit,
                    NomProduit = article,   // ✅ ICI : HERMES
                    Qte = qte,
                    PU = pu,
                    Devise = dev
                });
            }

            return list;
        }




        private void InsererOrdonnanceDansTransaction(SqlConnection con, SqlTransaction trans, int idVente, OrdonnanceVenteDTO o)
        {
            if (o == null) return;

            int idOrd;

            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.OrdonnanceVente
(
    ID_Vente, Numero, Prescripteur, Patient, DateOrdonnance, Note, ScanPath, PdfPath
)
OUTPUT INSERTED.ID_Ordonnance
VALUES
(
    @idVente, @numero, @prescripteur, @patient, @dateOrd, @note, @scan, @pdf
);", con, trans))
            {
                cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@numero", SqlDbType.NVarChar, 50).Value = (o.Numero ?? "").Trim();
                cmd.Parameters.Add("@prescripteur", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(o.Prescripteur) ? (object)DBNull.Value : o.Prescripteur.Trim();
                cmd.Parameters.Add("@patient", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(o.Patient) ? (object)DBNull.Value : o.Patient.Trim();
                cmd.Parameters.Add("@dateOrd", SqlDbType.DateTime).Value = o.DateOrdonnance;
                cmd.Parameters.Add("@note", SqlDbType.NVarChar).Value =
                    string.IsNullOrWhiteSpace(o.Note) ? (object)DBNull.Value : o.Note.Trim(); cmd.Parameters.Add("@scan", SqlDbType.NVarChar, 400).Value = string.IsNullOrWhiteSpace(o.ScanPath) ? (object)DBNull.Value : o.ScanPath;
                cmd.Parameters.Add("@pdf", SqlDbType.NVarChar, 400).Value = string.IsNullOrWhiteSpace(o.PdfPath) ? (object)DBNull.Value : o.PdfPath;

                idOrd = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var cmdL = new SqlCommand(@"
INSERT INTO dbo.OrdonnanceVenteLignes
(
    ID_Ordonnance, ID_Produit, Article, Qte, PU, Devise
)
VALUES
(
    @idOrd, @idProd, @article, @qte, @pu, @dev
);", con, trans))
            {
                cmdL.Parameters.Add("@idOrd", SqlDbType.Int);
                cmdL.Parameters.Add("@idProd", SqlDbType.Int);
                cmdL.Parameters.Add("@article", SqlDbType.NVarChar, 200);
                cmdL.Parameters.Add("@qte", SqlDbType.Int);
                var pPU = cmdL.Parameters.Add("@pu", SqlDbType.Decimal);
                pPU.Precision = 18; pPU.Scale = 2;
                cmdL.Parameters.Add("@dev", SqlDbType.NVarChar, 10);

                foreach (var l in o.Lignes ?? new List<OrdonnanceLigneDTO>())
                {
                    cmdL.Parameters["@idOrd"].Value = idOrd;
                    cmdL.Parameters["@idProd"].Value = l.IdProduit;
                    cmdL.Parameters["@article"].Value = (l.NomProduit ?? "").Trim();
                    cmdL.Parameters["@qte"].Value = l.Qte <= 0 ? 1 : l.Qte;
                    cmdL.Parameters["@pu"].Value = l.PU;
                    cmdL.Parameters["@dev"].Value = string.IsNullOrWhiteSpace(l.Devise) ? (object)DBNull.Value : l.Devise.Trim();

                    cmdL.ExecuteNonQuery();
                }
            }
        }

        private void LoadFideliteForClient(int idClient)
        {
            _lastSoldeFideliteCDF = 0m;
            _lastSoldeFideliteUSD = 0m;
            _lastCodeCarteClient = "";
            _lastTauxFidelite = 0m;

            if (idClient <= 0) return;

            try
            {
                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();
                    ChargerInfosFideliteClient(con, null, idClient);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void LoadFideliteForClient(SqlConnection con, SqlTransaction trans, int idClient)
        {
            _lastSoldeFideliteCDF = 0m;
            _lastSoldeFideliteUSD = 0m;
            _lastCodeCarteClient = "";
            _lastTauxFidelite = 0.005m;

            if (idClient <= 0) return;
            ChargerInfosFideliteClient(con, trans, idClient);
        }

        private string RecalculerEtMajCategorieClient(SqlConnection con, SqlTransaction trans, int idClient)
        {
            // Valeurs par défaut
            string categorie = "OCCASIONNEL";

            // ⚙️ Seuils (à adapter)
            decimal seuilTotalAchats = 1000m; // ex: 1000 (dans la devise de tes stats)
            int seuilNbTickets = 10;          // ex: 10 tickets
            int maxJoursInactif = 60;         // ex: si dernier achat > 60 jours => occasionnel

            decimal totalAchats = 0m;
            int nbTickets = 0;
            DateTime? dernierAchat = null;

            // 1) Lire stats (ta table existe déjà)
            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    ISNULL(TotalAchats,0) AS TotalAchats,
    ISNULL(NbTickets,0) AS NbTickets,
    DernierAchat
FROM dbo.ClientStats
WHERE IdClient=@id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        totalAchats = r["TotalAchats"] == DBNull.Value ? 0m : Convert.ToDecimal(r["TotalAchats"]);
                        nbTickets = r["NbTickets"] == DBNull.Value ? 0 : Convert.ToInt32(r["NbTickets"]);
                        dernierAchat = r["DernierAchat"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DernierAchat"]);
                    }
                }
            }

            // 2) Appliquer règle
            bool actifRecent = !dernierAchat.HasValue || (DateTime.Now - dernierAchat.Value).TotalDays <= maxJoursInactif;

            if (actifRecent && (totalAchats >= seuilTotalAchats || nbTickets >= seuilNbTickets))
                categorie = "FIDELE";
            else
                categorie = "OCCASIONNEL";

            // 3) Mettre à jour Clients (source d’affichage + filtrage)
            using (var cmdUp = new SqlCommand(@"
UPDATE dbo.Clients
SET CategorieClient = @cat
WHERE ID_Clients = @id;", con, trans))
            {
                cmdUp.Parameters.Add("@cat", SqlDbType.NVarChar, 30).Value = categorie;
                cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmdUp.ExecuteNonQuery();
            }

            return categorie;
        }


        private bool RenderFacture(Graphics g, DRectangle bounds, bool isTicket)
        {
            if (_printModel == null) return false;

            int left = bounds.Left;
            int right = bounds.Right;
            int y = bounds.Top;

            // Polices
            var fTitre = new DFont("Trebuchet MS", isTicket ? 10 : 16, FontStyle.Bold);
            var fTxt = new DFont("Trebuchet MS", isTicket ? 8 : 10, FontStyle.Regular);
            var fBold = new DFont("Trebuchet MS", isTicket ? 8 : 10, FontStyle.Bold);
            var fSmall = new DFont("Trebuchet MS", isTicket ? 7 : 9, FontStyle.Regular);

            string devise = string.IsNullOrWhiteSpace(_printModel.Devise) ? "CDF" : _printModel.Devise;
            string codeFacture = _printModel.CodeFacture ?? "";
            string modePaiement = string.IsNullOrWhiteSpace(_printModel.ModePaiement) ? "" : _printModel.ModePaiement;

            // Helper center
            void Center(string text, DFont f, int extra = 2)
            {
                var s = g.MeasureString(text ?? "", f);
                float x = left + (bounds.Width - s.Width) / 2f;
                g.DrawString(text ?? "", f, Brushes.Black, x, y);
                y += (int)s.Height + extra;
            }

            // ENTETE
            if (isTicket)
            {
                Center("ZAIRE MODE SARL", fTitre);
                Center("23, Bld Lumumba / Immeuble Masina Plaza", fTxt);
                Center("+243861507560  |  ZAIRE.CD", fTxt);
                Center("RCCM: 25-B-01497", fSmall);
                Center("IDNAT: 01-F4300-N73258E", fSmall);
            }
            else
            {
                g.DrawString("ZAIRE MODE SARL", fTitre, Brushes.Black, left, y); y += 30;
                g.DrawString("23, Bld Lumumba / Immeuble Masina Plaza", fTxt, Brushes.Black, left, y); y += 18;
                g.DrawString("+243861507560 / E-MAIL: Zaireshop@hotmail.com", fTxt, Brushes.Black, left, y); y += 18;
                g.DrawString("ZAIRE.CD  |  RCCM: 25-B-01497  |  IDNAT: 01-F4300-N73258E", fSmall, Brushes.Black, left, y); y += 18;

                // DUPLICATA / ANNULEE
                string tag = "DUPLICATA";
                if (!string.IsNullOrWhiteSpace(_printModel.Statut) &&
                    _printModel.Statut.Equals("ANNULEE", StringComparison.OrdinalIgnoreCase))
                    tag = "DUPLICATA (VENTE ANNULEE)";

                g.DrawString(tag, new DFont("Trebuchet MS", 11, FontStyle.Bold), Brushes.Black, right - 280, bounds.Top);
            }

            y += 2;
            g.DrawLine(Pens.Black, left, y, right, y); y += 8;

            // Infos
            g.DrawString("Facture : " + codeFacture, fBold, Brushes.Black, left, y); y += 16;
            g.DrawString("Date : " + _printModel.DateVente.ToString("dd/MM/yyyy HH:mm:ss"), fTxt, Brushes.Black, left, y); y += 16;
            g.DrawString("Caissier : " + (_printModel.Caissier ?? "SYSTEM"), fTxt, Brushes.Black, left, y); y += 16;
            g.DrawString("Client : " + (string.IsNullOrWhiteSpace(_printModel.ClientNom) ? "-" : _printModel.ClientNom), fTxt, Brushes.Black, left, y); y += 16;

            if (!string.IsNullOrWhiteSpace(modePaiement))
            {
                g.DrawString("Mode : " + modePaiement + " (" + devise + ")", fTxt, Brushes.Black, left, y);
                y += 16;
            }

            g.DrawLine(Pens.Black, left, y, right, y); y += 8;

            // Colonnes
            int colArticle = left;
            int colQte = left + (int)(bounds.Width * (isTicket ? 0.62 : 0.60));
            int colPU = left + (int)(bounds.Width * (isTicket ? 0.76 : 0.72));
            int colTot = left + (int)(bounds.Width * (isTicket ? 0.88 : 0.85));

            // Header tableau
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), left, y, bounds.Width, 20);
            g.DrawRectangle(Pens.Gray, left, y, bounds.Width, 20);
            g.DrawString("Article", fBold, Brushes.Black, colArticle + 3, y + 2);
            g.DrawString("Qt", fBold, Brushes.Black, colQte + 3, y + 2);
            g.DrawString("PU", fBold, Brushes.Black, colPU + 3, y + 2);
            g.DrawString("Total", fBold, Brushes.Black, colTot + 3, y + 2);
            y += 24;

            // Réserve bas de page (inclut fidelité + barcode)
            int reserveBottom = isTicket ? 260 : 300;
            int bottomLimit = bounds.Bottom - reserveBottom;

            // Lignes produits (wrap + pagination)
            for (; _printLineIndex < _printModel.Lignes.Count; _printLineIndex++)
            {
                var line = _printModel.Lignes[_printLineIndex];

                float articleWidth = (colQte - colArticle - 8);
                var size = g.MeasureString(line.NomProduit ?? "", fTxt, (int)articleWidth);
                int h = Math.Max(16, (int)Math.Ceiling(size.Height));

                if (y + h + 12 > bottomLimit)
                    return true; // encore des pages

                var rect = new RectangleF(colArticle + 2, y, articleWidth, 1000);
                g.DrawString(line.NomProduit ?? "", fTxt, Brushes.Black, rect);

                g.DrawString(line.Quantite.ToString(), fTxt, Brushes.Black, colQte, y);
                g.DrawString(line.PrixUnitaire.ToString("N2"), fTxt, Brushes.Black, colPU, y);
                g.DrawString(line.Montant.ToString("N2"), fTxt, Brushes.Black, colTot, y);

                y += h + 4;
                g.DrawLine(Pens.Gainsboro, left, y, right, y);
                y += 3;
            }

            // Totaux
            y += 6;
            g.DrawLine(Pens.Black, left, y, right, y); y += 10;

            g.DrawString("TOTAL TTC :", new DFont("Trebuchet MS", isTicket ? 10 : 12, FontStyle.Bold),
                Brushes.Black, left, y);
            g.DrawString(_printModel.MontantTotal.ToString("N2") + " " + devise,
                new DFont("Trebuchet MS", isTicket ? 10 : 12, FontStyle.Bold),
                Brushes.Black, right - (isTicket ? 160 : 190), y);
            y += 24;

            // ====== BLOC FIDELITE (comme PDF) ======
            // Afficher seulement si on a un client et un taux > 0
            if (_printModel.IdClient > 0 && _lastTauxFidelite > 0m)
            {
                decimal gain = Math.Round(_printModel.MontantTotal * _lastTauxFidelite, 2);
                string tauxTxt = (_lastTauxFidelite * 100m).ToString("0.##", CultureInfo.GetCultureInfo("fr-FR")) + "%";

                int blockH = isTicket ? 95 : 110;
                var rectBlock = new DRectangle(left, y, bounds.Width, blockH);

                // cadre
                g.FillRectangle(new SolidBrush(Color.FromArgb(252, 252, 252)), rectBlock);
                g.DrawRectangle(Pens.Gray, rectBlock);

                int bx = left + 8;
                int by = y + 6;

                g.DrawString("FIDELITE CLIENT", fBold, Brushes.Black, bx, by); by += 16;
                g.DrawString("Taux : " + tauxTxt, fSmall, Brushes.Black, bx, by); by += 14;
                g.DrawString("Gain sur cette vente : " + gain.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " " + devise, fSmall, Brushes.Black, bx, by); by += 14;

                g.DrawString("Solde Fidelite (CDF) : " + _lastSoldeFideliteCDF.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")), fSmall, Brushes.Black, bx, by); by += 14;
                g.DrawString("Solde Fidelite (USD) : " + _lastSoldeFideliteUSD.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")), fSmall, Brushes.Black, bx, by); by += 14;

                if (!string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                {
                    g.DrawString("Carte Fidelite : " + _lastCodeCarteClient, fSmall, Brushes.Black, bx, by);
                    by += 14;
                }

                // petit message
                g.DrawString("Vous pouvez utiliser vos points si vous depasser 10USD.", fSmall, Brushes.Black, bx, by);

                y += blockH + 10;
            }

            // Message
            if (isTicket)
            {
                Center("Merci pour votre fidélité !", fTxt);
                Center("La Qualité fait la différence.", fTxt);
                Center("Ni repris, ni échangé.", fTxt);
            }
            else
            {
                Center("Merci pour votre fidélité, à la prochaine !", fTxt);
                Center("La Qualité fait la différence.", fTxt);
                Center("Les marchandises vendues ne peuvent être ni reprises, ni échangées.", fSmall);
            }

            // Barcode facture
            try
            {
                var bw = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = Math.Max(200, bounds.Width - 30),
                        Height = isTicket ? 45 : 60,
                        Margin = 1
                    }
                };

                using (Bitmap bmp = bw.Write(codeFacture))
                {
                    int x = left + (bounds.Width - bmp.Width) / 2;
                    int yBarcode = y + 8;
                    g.DrawImage(bmp, x, yBarcode);
                    y = yBarcode + bmp.Height + 6;
                }

                Center("Code Facture : " + codeFacture, fSmall);

                // Barcode carte fidélité (optionnel)
                if (!string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                {
                    var bw2 = new ZXing.BarcodeWriter
                    {
                        Format = ZXing.BarcodeFormat.CODE_128,
                        Options = new ZXing.Common.EncodingOptions
                        {
                            Width = Math.Max(200, bounds.Width - 30),
                            Height = isTicket ? 40 : 55,
                            Margin = 1
                        }
                    };

                    using (Bitmap bmp2 = bw2.Write(_lastCodeCarteClient))
                    {
                        int x2 = left + (bounds.Width - bmp2.Width) / 2;
                        int y2 = y + 6;
                        g.DrawImage(bmp2, x2, y2);
                        y = y2 + bmp2.Height + 6;
                    }

                    Center("Carte Fidelite : " + _lastCodeCarteClient, fSmall);
                }
            }
            catch { }

            return false; // fini
        }

        public FormVentes()
        {
            InitializeComponent();

            InitKeyboardWorkflow();

            cboScanCode.TextUpdate -= cboScanCode_TextUpdate;
            cboScanCode.TextUpdate += cboScanCode_TextUpdate;

            cboScanCode.SelectionChangeCommitted -= cboScanCode_SelectionChangeCommitted;
            cboScanCode.SelectionChangeCommitted += cboScanCode_SelectionChangeCommitted;

            cboScanCode.KeyDown -= txtScanCode_KeyDown;
            cboScanCode.KeyDown += txtScanCode_KeyDown;

            InitAutoSuggestCodeBarre();

            _creditSvc = new CreditService(ConfigSysteme.ConnectionString);


            _compteSvc = new CompteClientService(ConfigSysteme.ConnectionString);
            _creditSvc = new CreditService(ConfigSysteme.ConnectionString);
            _couponSvc = new CouponService(ConfigSysteme.ConnectionString);
            _loyalSvc = new LoyaltyService(ConfigSysteme.ConnectionString);
            _statsSvc = new ClientStatsService(ConfigSysteme.ConnectionString);

            _produitRepo = new ProduitRepository(ConfigSysteme.ConnectionString);

            // timer anti-spam requêtes
            _timerProduitSearch = new WinTimer();
            _timerProduitSearch.Interval = 250; // 250ms
            _timerProduitSearch.Tick += TimerProduitSearch_Tick;

            ConfigSysteme.AppliquerMenuContextuel(this);

            InitialiserComboTypeRapport();
            Load += FormVentes_Load;
            dgvPanier.ContextMenuStrip = ConfigSysteme.MenuContextuel;
            dgvRapport.ContextMenuStrip = ConfigSysteme.MenuContextuel;
            InitialiserComboBoxClient();

            ConfigSysteme.AppliquerTheme(this);
        }

        private void FormVentes_Load(object sender, EventArgs e)
        {
            // Affiche le caissier connecté
            lblCaissier.Text = $"{SessionEmploye.Prenom} {SessionEmploye.Nom}";
            txtNomCaissier.Text = lblCaissier.Text;
            txtIDEmploye.Text = SessionEmploye.ID_Employe.ToString();

            InitialiserDgvPanier();
            InitialiserCombos();
            SetupScanCombo();
            ChargerProduits();
            ChargerClientsComboBox();
            InitialiserComboEmplacement();
            cmbNomClient.DropDownStyle = ComboBoxStyle.DropDown;
            UnhookOriginalClickHandlers();
            ConfigurerSecuriteBoutons_Ventes();
            ConfigSysteme.LoadPrintersConfig();

            cboScanCode.KeyDown -= txtScanCode_KeyDown;
            cboScanCode.KeyDown += txtScanCode_KeyDown;

            int idCaissier = SessionEmploye.ID_Employe;
            if (SessionOuvertePourCaissier(idCaissier, out int sid))
            {
                ConfigSysteme.SessionCaisseId = sid;
                ConfigSysteme.CaissierSessionId = idCaissier;
            }

            chkVenteCredit.CheckedChanged += (s, e2) =>
            {
                dtpEcheanceCredit.Enabled = chkVenteCredit.Checked;
            };
            dtpEcheanceCredit.Enabled = chkVenteCredit.Checked;


            dtpDateTransaction.Value = DateTime.Now;
            numQuantite.Value = 1;
            txtTVApercent.Text = "0";
            txtRemisePercent.Text = "0";
           

            // Charger traductions dynamiques

            RafraichirLangue();
            RafraichirTheme();

        }

        private string GetTicketPrinterName_FromConfigOrDefault()
        {
            // ✅ 1) celle choisie dans Gestion Imprimantes
            string p = (ConfigSysteme.ImprimanteTicketNom ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(p))
            {
                foreach (string inst in PrinterSettings.InstalledPrinters)
                    if (string.Equals(inst, p, StringComparison.OrdinalIgnoreCase))
                        return inst;
            }

            // ✅ 2) fallback imprimante Windows par défaut
            return new PrinterSettings().PrinterName;
        }


        private void dgvPanier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (dgvPanier.IsCurrentCellInEditMode) return;

            e.SuppressKeyPress = true;

            OuvrirClientRapideEtFinaliser();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // 1) Scan / codebarre : laisse ton système existant
                if (IsInScanControls())
                    return base.ProcessCmdKey(ref msg, keyData);

                // 2) DGV en édition : laisse Enter valider la cellule
                if (IsEditingPanier())
                    return base.ProcessCmdKey(ref msg, keyData);

                // 3) Sinon : workflow Pro
                if (HandleEnterWorkflow_Pro())
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool HandleEnterWorkflow_Pro()
        {
            // Panier => ouvrir Client rapide (ou focus client)
            if (dgvPanier != null && (ActiveControl == dgvPanier || dgvPanier.Focused))
            {
                OuvrirClientRapideEtFinaliser();
                return true;
            }

            // Client => aller vers options (coupon/crédit) ou finaliser
            if (cmbNomClient != null && (ActiveControl == cmbNomClient || cmbNomClient.Focused))
            {
                AllerAuxOptionsOuFinaliser(); // tu l'as déjà (ou similaire)
                return true;
            }

            // Coupon => finaliser
            if (txtCouponCode != null && txtCouponCode.Focused)
            {
                FinaliserEtImprimer();
                return true;
            }

            // Crédit => aller échéance ou finaliser
            if (chkVenteCredit != null && chkVenteCredit.Focused)
            {
                if (chkVenteCredit.Checked && dtpEcheanceCredit != null && dtpEcheanceCredit.Enabled)
                {
                    dtpEcheanceCredit.Focus();
                    return true;
                }
                FinaliserEtImprimer();
                return true;
            }

            if (dtpEcheanceCredit != null && dtpEcheanceCredit.Focused)
            {
                FinaliserEtImprimer();
                return true;
            }

            // ailleurs => si panier non vide, finaliser
            if (!PanierVide())
            {
                FinaliserEtImprimer();
                return true;
            }

            return false;
        }

        private bool CanFocusSafe(Control c)
        {
            if (c == null) return false;
            if (c.IsDisposed) return false;
            if (!c.Visible) return false;
            if (!c.Enabled) return false;
            return c.CanFocus; // propriété native
        }

        private void AllerAuxOptionsOuFinaliser()
        {
            // Priorité 1 : Coupon
            if (CanFocusSafe(txtCouponCode))
            {
                txtCouponCode.Focus();
                txtCouponCode.SelectAll();
                return;
            }

            // Priorité 2 : Vente à crédit (case)
            if (CanFocusSafe(chkVenteCredit))
            {
                chkVenteCredit.Focus();
                return;
            }

            // Sinon : finaliser
            FinaliserEtImprimer();
        }

        private bool IsInScanControls()
        {
            var ac = this.ActiveControl;
            if (ac == null) return false;

            if (cboScanCode != null && ac == cboScanCode) return true;
            if (txtCodeBarre != null && ac == txtCodeBarre) return true;

            return false;
        }

        private bool IsEditingPanier()
        {
            if (dgvPanier == null) return false;
            return dgvPanier.IsCurrentCellInEditMode || dgvPanier.EditingControl != null;
        }

        private bool PanierVide()
        {
            if (dgvPanier == null) return true;
            foreach (DataGridViewRow r in dgvPanier.Rows)
                if (r != null && !r.IsNewRow) return false;
            return true;
        }

        private void OuvrirClientRapideEtFinaliser()
        {
            // ✅ 1) Calculer un NET + Devise “provisoires” pour la boite ClientRapide
            decimal totalBrut = 0m;
            decimal remiseTicketMontant = 0m;

            decimal.TryParse(CleanText(txtTotalTTC.Text), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out totalBrut);

            if (decimal.TryParse(CleanText(txtRemiseTicketMontant.Text), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var rm))
                remiseTicketMontant = Math.Max(0m, rm);

            decimal montantNetAPayer = Math.Round(totalBrut - remiseTicketMontant, 2);
            if (montantNetAPayer < 0m) montantNetAPayer = 0m;

            // ✅ devise “provisoire”
            string deviseVente = "CDF";
            try
            {
                string dv = null;
                foreach (DataGridViewRow r in dgvPanier.Rows)
                {
                    if (r.IsNewRow) continue;
                    string d = CleanText(r.Cells["Devise"].Value?.ToString() ?? "");
                    if (string.IsNullOrWhiteSpace(d)) continue;

                    if (dv == null) dv = d;
                    else if (!d.Equals(dv, StringComparison.OrdinalIgnoreCase))
                    {
                        dv = null;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(dv))
                {
                    dv = CleanText(dv).ToUpperInvariant();
                    deviseVente = (dv == "FC") ? "CDF" : dv;
                }
            }
            catch { }

            // ✅ 2) Construire les lignes ordonnance depuis le panier (AVANT ouverture)
            var lignesOrd = BuildOrdonnanceLinesFromPanier();  // ✅ IMPORTANT : passer dgvPanier

            // (optionnel debug)
            // MessageBox.Show("Lignes ordonnance = " + lignesOrd.Count);

            using (var frmClientRapide = new FormClientsRapide(
                connectionString: ConfigSysteme.ConnectionString,

                couponInitial: txtCouponCode.Text,
                creditInitial: chkVenteCredit.Checked,
                echeanceInitial: dtpEcheanceCredit.Value.Date,

                emplacementsSource: cmbEmplacement.Items,
                emplacementInitial: cmbEmplacement.SelectedItem?.ToString(),

                injectToFormVentes: (frm) =>
                {
                    // ✅ pousser valeurs dans FormVentes
                    cmbNomClient.Text = frm.NomClientChoisi;
                    txtTelephone.Text = frm.TelephoneChoisi;

                    txtCouponCode.Text = frm.CouponChoisi;
                    chkVenteCredit.Checked = frm.VenteCreditChoisi;
                    dtpEcheanceCredit.Value = frm.EcheanceChoisie;

                    if (!string.IsNullOrWhiteSpace(frm.EmplacementChoisi))
                        cmbEmplacement.SelectedItem = frm.EmplacementChoisi;

                    _payLinesFromClientRapide = frm.PaiementsChoisis;
                    _ordonnanceFromClientRapide = frm.OrdonnanceChoisie;

                    _netFromClientRapide = montantNetAPayer;
                    _deviseFromClientRapide = deviseVente;

                    ChargerClientsComboBox();
                },

                finaliserAction: () =>
                {
                    BeginInvoke(new Action(() => btnFinaliser.PerformClick()));
                },

                netAPayer: montantNetAPayer,
                deviseVente: deviseVente,
                codeFactureEnCours: _codeFactureEnCours
            ))
            {
                // ✅ 3) ICI EXACTEMENT : envoyer panier -> ordonnance AVANT ShowDialog
                frmClientRapide.LignesPanier = lignesOrd;

                // ✅ (optionnel mais conseillé) : pousser aussi le contexte numéro/patient/prescripteur
                frmClientRapide.ChargerContexteOrdonnance(
                    lignes: lignesOrd,
                    numeroOrd: CleanText(_codeFactureEnCours),
                    prescripteur: GetPrescripteurConnecte(),
                    patient: CleanText(cmbNomClient.Text)
                );

                frmClientRapide.ShowDialog(this);
            }
        }


        private void AppliquerResultClientRapide(ClientRapideResult r)
        {
            // 🔥 Ici tu choisis : soit créer le client en DB, soit juste remplir et créer à la finalisation.
            // Option PRO : créer tout de suite (si tu as une méthode CreateClient).
            // Sinon : remplir les champs + cmbNomClient.Text.

            _suspendComboEvents = true;
            try
            {
                cmbNomClient.Text = r.Nom;
                txtTelephone.Text = r.Telephone;

                if (txtCouponCode != null) txtCouponCode.Text = r.CouponCode ?? "";
                if (chkVenteCredit != null) chkVenteCredit.Checked = r.VenteACredit;
                if (dtpEcheanceCredit != null) dtpEcheanceCredit.Value = r.EcheanceCredit == default
                    ? DateTime.Today.AddDays(30) : r.EcheanceCredit;

                if (cmbEmplacement != null && !string.IsNullOrWhiteSpace(r.Emplacement))
                    cmbEmplacement.Text = r.Emplacement;
            }
            finally
            {
                _suspendComboEvents = false;
            }
        }

        private void FinaliserEtImprimer()
        {
            if (PanierVide())
            {
                FocusScan();
                return;
            }

            // ✅ 1) Finaliser (ta logique DB)
            btnFinaliser?.PerformClick();

            // ✅ 2) Imprimer (ticket)
            btnImprimerTicket?.PerformClick();

            // ✅ 3) Reset + focus scan
            ResetApresVente();
            FocusScan();
        }



        private readonly string _cs = ConfigSysteme.ConnectionString;

        private void InitAutoSuggestCodeBarre()
        {
            // 1) Timer debounce
            if (_tmrCodeBarreDebounce == null)
                _tmrCodeBarreDebounce = new System.Windows.Forms.Timer();

            _tmrCodeBarreDebounce.Interval = 220;
            _tmrCodeBarreDebounce.Tick -= TmrCodeBarreDebounce_Tick;
            _tmrCodeBarreDebounce.Tick += TmrCodeBarreDebounce_Tick;

            // 2) ListBox suggestions
            if (_lstCodeSuggestions == null)
            {
                _lstCodeSuggestions = new ListBox
                {
                    Visible = false,
                    IntegralHeight = false,
                    Height = 170,
                    BorderStyle = BorderStyle.FixedSingle
                };

                txtCodeBarre.Parent.Controls.Add(_lstCodeSuggestions);
                _lstCodeSuggestions.BringToFront();
            }

            // 3) Reposition dynamique
            RepositionnerListeSuggestions();
            txtCodeBarre.Parent.Resize -= TxtCodeBarreParent_Resize;
            txtCodeBarre.Parent.Resize += TxtCodeBarreParent_Resize;

            txtCodeBarre.LocationChanged -= TxtCodeBarre_LayoutChanged;
            txtCodeBarre.LocationChanged += TxtCodeBarre_LayoutChanged;
            txtCodeBarre.SizeChanged -= TxtCodeBarre_LayoutChanged;
            txtCodeBarre.SizeChanged += TxtCodeBarre_LayoutChanged;

            // 4) TextChanged (debounce)
            txtCodeBarre.TextChanged -= TxtCodeBarre_TextChanged;
            txtCodeBarre.TextChanged += TxtCodeBarre_TextChanged;

            // 5) KeyDown textbox
            txtCodeBarre.KeyDown -= txtCodeBarre_KeyDown;
            txtCodeBarre.KeyDown += txtCodeBarre_KeyDown;

            // 6) KeyDown listbox + doubleclick
            _lstCodeSuggestions.KeyDown -= LstCodeSuggestions_KeyDown;
            _lstCodeSuggestions.KeyDown += LstCodeSuggestions_KeyDown;

            _lstCodeSuggestions.DoubleClick -= LstCodeSuggestions_DoubleClick;
            _lstCodeSuggestions.DoubleClick += LstCodeSuggestions_DoubleClick;

            // 7) Clic ailleurs -> masque
            this.Click -= Global_ClickToHide;
            this.Click += Global_ClickToHide;
            dgvPanier.Click -= Global_ClickToHide;
            dgvPanier.Click += Global_ClickToHide;
        }

        private void TxtCodeBarre_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreTxtChange) return;

            var t = (txtCodeBarre.Text ?? "").Trim();
            if (t.Length < 2)
            {
                CacherSuggestions();
                return;
            }

            _tmrCodeBarreDebounce.Stop();
            _tmrCodeBarreDebounce.Start();
        }

        private async void LstCodeSuggestions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                txtCodeBarre.Focus();
                CacherSuggestions();
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await ValiderCodeBarreOuSelectionAsync();
                return;
            }
        }

        private async void LstCodeSuggestions_DoubleClick(object sender, EventArgs e)
        {
            await ValiderCodeBarreOuSelectionAsync();
        }

        private void Global_ClickToHide(object sender, EventArgs e) => CacherSuggestions();


        private async void TmrCodeBarreDebounce_Tick(object sender, EventArgs e)
        {
            _tmrCodeBarreDebounce.Stop();
            await ChargerSuggestionsCodeBarreAsync();
        }

        private async void txtCodeBarre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            // ✅ exécute validation scan
            var outcome = await ValiderCodeBarreOuSelectionAsync();

            // ✅ Si on doit rester sur txtCodeBarre (introuvable ou suggestions)
            if (outcome == ScanOutcome.NotFoundKeepFocusCodeBarre ||
                outcome == ScanOutcome.SuggestionsShownKeepFocus)
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    txtCodeBarre.Focus();
                    txtCodeBarre.SelectAll();
                }));
                return;
            }

            // ✅ Si on a ajouté et démarré l’édition Quantité => NE TOUCHE PAS AU FOCUS
            if (outcome == ScanOutcome.AddedAndQtyEditStarted)
                return;

            // ✅ Sinon, comportement par défaut (si tu veux revenir au scan)
            // (Optionnel : si tu ne veux JAMAIS revenir auto au scan, supprime ce bloc)
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                txtCodeBarre.Focus();
                txtCodeBarre.SelectAll();
            }));
        }

        private void RepositionnerListeSuggestions()
        {
            if (_lstCodeSuggestions == null || txtCodeBarre?.Parent == null) return;

            var p = txtCodeBarre.Parent.PointToClient(txtCodeBarre.PointToScreen(Point.Empty));
            _lstCodeSuggestions.Left = p.X;
            _lstCodeSuggestions.Top = p.Y + txtCodeBarre.Height + 2;
            _lstCodeSuggestions.Width = txtCodeBarre.Width;
        }

        private void CacherSuggestions()
        {
            if (_lstCodeSuggestions == null) return;
            _lstCodeSuggestions.Visible = false;
            _lstCodeSuggestions.DataSource = null;
        }

        private bool ForceOffline
        {
            get
            {
                bool v;
                return bool.TryParse(ConfigurationManager.AppSettings["ForceOffline"], out v) && v;
            }
        }

        private async Task ChargerSuggestionsCodeBarreAsync()
        {
            if (_loadingSuggestions) return;
            if (txtCodeBarre == null) return;

            string prefix = (txtCodeBarre.Text ?? "").Trim();
            if (prefix.Length < 2) { CacherSuggestions(); return; }

            _loadingSuggestions = true;
            try
            {
                var items = new List<SuggestItem>();

                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    await con.OpenAsync();

                    string sql = @"
SELECT TOP 20
    ID_Produit,
    LTRIM(RTRIM(ISNULL(CodeBarre,''))) AS CodeBarre,
    ISNULL(NomProduit,'') AS NomProduit,
    ISNULL(RefProduit,'') AS RefProduit
FROM Produit
WHERE LTRIM(RTRIM(ISNULL(CodeBarre,''))) LIKE @p + '%'
ORDER BY CodeBarre;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@p", SqlDbType.NVarChar, 100).Value = prefix;

                        using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                string cb = dr["CodeBarre"]?.ToString() ?? "";
                                if (string.IsNullOrWhiteSpace(cb)) continue;

                                string nom = dr["NomProduit"]?.ToString() ?? "";
                                string rf = dr["RefProduit"]?.ToString() ?? "";

                                items.Add(new SuggestItem
                                {
                                    ID = Convert.ToInt32(dr["ID_Produit"]),
                                    CodeBarre = cb,
                                    Text = string.IsNullOrWhiteSpace(nom) && string.IsNullOrWhiteSpace(rf)
                                        ? cb
                                        : $"{cb}  —  {nom} {(string.IsNullOrWhiteSpace(rf) ? "" : $"({rf})")}".Trim()
                                });
                            }
                        }
                    }
                }

                if (items.Count == 0)
                {
                    CacherSuggestions();
                    return;
                }

                _lstCodeSuggestions.DataSource = items;
                _lstCodeSuggestions.DisplayMember = "Text";
                _lstCodeSuggestions.ValueMember = "ID";
                _lstCodeSuggestions.Visible = true;
                _lstCodeSuggestions.BringToFront();
                RepositionnerListeSuggestions();
            }
            catch
            {
                CacherSuggestions();
            }
            finally
            {
                _loadingSuggestions = false;
            }
        }

        private void FocusScan()
        {
            if (cboScanCode == null) return;
            cboScanCode.Focus();
            cboScanCode.SelectAll();
        }

        private void UnhookOriginalClickHandlers()
        {
            // 4 boutons OPEN
            if (btnAjouter != null) btnAjouter.Click -= btnAjouter_Click;
            if (btnSupprimerArticle != null) btnSupprimerArticle.Click -= btnSupprimerArticle_Click;
            if (btnAnnuler != null) btnAnnuler.Click -= btnAnnuler_Click;
            if (btnFinaliser != null) btnFinaliser.Click -= btnFinaliser_Click;
            if (btnImprimerTicket != null) btnImprimerTicket.Click -= btnImprimerTicket_Click;

            // Rapports / exports
            if (btnCharger != null) btnCharger.Click -= btnCharger_Click;
            if (btnRechercher != null) btnRechercher.Click -= btnRechercher_Click;
            if (btnExporterPDF != null) btnExporterPDF.Click -= btnExporterPDF_Click;
            if (btnExporterExcel != null) btnExporterExcel.Click -= btnExporterExcel_Click;

            // Impression / aperçu
            if (btnApercu != null) btnApercu.Click -= btnApercu_Click;
            if (btnImprimer != null) btnImprimer.Click -= btnImprimer_Click;
            if (btnDuplicataA4 != null) btnDuplicataA4.Click -= btnDuplicataA4_Click;

            // Annulation vente / paiements
            if (btnAnnulerVente != null) btnAnnulerVente.Click -= btnAnnulerVente_Click;
            if (btnVoirPaiements != null) btnVoirPaiements.Click -= btnVoirPaiements_Click;

            // Inventaire / total jour / rapport
            if (btnInventaireDuJour != null) btnInventaireDuJour.Click -= btnInventaireDuJour_Click;
            if (BtnTotalDuJour != null) BtnTotalDuJour.Click -= BtnTotalDuJour_Click;
            if (btnGenererRapport != null) btnGenererRapport.Click -= btnGenererRapport_Click;

            // Test connexion
            if (btnTesterConnexion != null) btnTesterConnexion.Click -= btnTesterConnexion_Click;
        }


        // FormVentes.cs  (dans la classe FormVentes)
        private List<OrdonnanceLigneDTO> BuildOrdonnanceLinesFromPanier()
        {
            dgvPanier.EndEdit(); // IMPORTANT si user vient de modifier une cellule

            var lignes = new List<OrdonnanceLigneDTO>();

            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                int.TryParse(r.Cells["ID_Produit"]?.Value?.ToString(), out int idProd);
                int.TryParse(r.Cells["Quantite"]?.Value?.ToString(), out int qte);

                string nom = r.Cells["NomProduit"]?.FormattedValue?.ToString() ?? "";

                decimal pu = GetDecimalCell(r, "PrixUnitaire"); // <-- vérifie le vrai nom de colonne !
                string dev = (r.Cells["Devise"]?.Value?.ToString() ?? "").Trim();
                if (string.IsNullOrWhiteSpace(dev)) dev = "CDF";

                if (idProd <= 0 || qte <= 0 || string.IsNullOrWhiteSpace(nom))
                    continue;

                lignes.Add(new OrdonnanceLigneDTO
                {
                    IdProduit = idProd,
                    NomProduit = nom.Trim(),
                    Qte = qte,
                    PU = Math.Round(pu, 2),
                    Devise = dev.ToUpperInvariant()
                });
            }

            return lignes;
        }


        private void SetupScanCombo()
        {
            cboScanCode.DropDownStyle = ComboBoxStyle.DropDown;
            cboScanCode.AutoCompleteMode = AutoCompleteMode.None;
            cboScanCode.AutoCompleteSource = AutoCompleteSource.None;
            cboScanCode.FormattingEnabled = true;

            // Important: on ne veut pas que le dropdown s’ouvre
            cboScanCode.DroppedDown = false;
        }


        private async Task<List<SuggestItem>> BuildSuggestionItemsAsync(string prefix)
        {
            var items = new List<SuggestItem>();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                await con.OpenAsync();

                string sql = @"
SELECT TOP 20
    ID_Produit,
    LTRIM(RTRIM(ISNULL(CodeBarre,''))) AS CodeBarre,
    NomProduit,
    RefProduit
FROM Produit
WHERE LTRIM(RTRIM(ISNULL(CodeBarre,''))) LIKE @p + '%'
ORDER BY CodeBarre;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@p", SqlDbType.NVarChar, 100).Value = prefix;

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            string cb = dr["CodeBarre"].ToString();
                            string nom = dr["NomProduit"].ToString();
                            string rf = dr["RefProduit"].ToString();

                            items.Add(new SuggestItem
                            {
                                ID = Convert.ToInt32(dr["ID_Produit"]),
                                CodeBarre = cb,
                                Text = $"{cb}  —  {nom}  ({rf})"
                            });
                        }
                    }
                }
            }

            return items;
        }

        private async Task<ScanOutcome> ValiderCodeBarreOuSelectionAsync()
        {
            // 1) suggestion sélectionnée
            if (_lstCodeSuggestions != null &&
                _lstCodeSuggestions.Visible &&
                _lstCodeSuggestions.SelectedItem is SuggestItem si)
            {
                await AjouterProduitAuPanierParIdAsync(si.ID, si.CodeBarre);
                CacherSuggestions();
                ClearTxtCodeBarreSafe_Deferred(focusScan: false);

                return _qtyEditStarted ? ScanOutcome.AddedAndQtyEditStarted : ScanOutcome.None;
            }

            string code = (txtCodeBarre.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) return ScanOutcome.None;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    await con.OpenAsync();

                    string sql = @"
SELECT TOP 1 
    ID_Produit,
    LTRIM(RTRIM(ISNULL(CodeBarre,''))) AS CodeBarre
FROM Produit
WHERE LTRIM(RTRIM(ISNULL(CodeBarre,''))) = LTRIM(RTRIM(@code));";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = code;

                        using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            if (!await dr.ReadAsync())
                            {
                                await ChargerSuggestionsCodeBarreAsync();

                                if (_lstCodeSuggestions != null && _lstCodeSuggestions.Visible)
                                    return ScanOutcome.SuggestionsShownKeepFocus;

                                // ✅ ton popup
                                MessageBox.Show("Ok");

                                // ✅ rester sur txtCodeBarre (pas retour scan)
                                return ScanOutcome.NotFoundKeepFocusCodeBarre;
                            }

                            int id = Convert.ToInt32(dr["ID_Produit"]);
                            string codeDb = dr["CodeBarre"]?.ToString() ?? code;

                            await AjouterProduitAuPanierParIdAsync(id, codeDb);
                            CacherSuggestions();
                            ClearTxtCodeBarreSafe_Deferred(focusScan: false);

                            return _qtyEditStarted ? ScanOutcome.AddedAndQtyEditStarted : ScanOutcome.None;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur validation code-barres : " + ex.Message);
                return ScanOutcome.NotFoundKeepFocusCodeBarre;
            }
        }

        private async Task AjouterProduitAuPanierParIdAsync(int idProduit, string codeBarre)
        {
            ProduitCombo p = await ChargerProduitComboParIdAsync(idProduit);
            if (p == null)
            {
                // Produit introuvable => on revient sur scan
                ClearTxtCodeBarreSafe_Deferred(focusScan: true);
                _qtyEditStarted = false;
                return;
            }

            int rowIndex = AddOrIncrementProduitToPanier_ReturnRowIndex(p, 1, codeBarre);

            _qtyEditStarted = false;

            if (rowIndex >= 0 && dgvPanier.Columns.Contains("Quantite"))
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed) return;
                    if (dgvPanier.IsDisposed) return;

                    if (rowIndex < 0 || rowIndex >= dgvPanier.Rows.Count) return;
                    if (dgvPanier.Rows[rowIndex].IsNewRow) return;

                    // ✅ Focus sur la grille + cellule Quantite
                    dgvPanier.Focus();
                    dgvPanier.CurrentCell = dgvPanier.Rows[rowIndex].Cells["Quantite"];
                    dgvPanier.BeginEdit(true);

                    // ✅ Met le curseur dans le TextBox de Quantite
                    if (dgvPanier.EditingControl is TextBox tb)
                    {
                        tb.Focus();
                        tb.SelectAll();
                    }

                    _qtyEditStarted = true;
                    _openClientAfterQty = true; // si tu veux ouvrir ClientRapide après Enter Quantite
                }));
            }

            // ✅ IMPORTANT : vider le scan sans refocus sur txtCodeBarre
            ClearTxtCodeBarreSafe_Deferred(focusScan: false);
        }

        private async Task<ProduitCombo> ChargerProduitComboParIdAsync(int idProduit)
        {
            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                await con.OpenAsync();

                string sql = @"
SELECT TOP 1 
    ID_Produit, NomProduit, RefProduit, Prix, Devise, Categorie, Taille, Couleur
FROM dbo.Produit
WHERE ID_Produit = @id;";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (!await dr.ReadAsync()) return null;

                        return new ProduitCombo
                        {
                            ID = Convert.ToInt32(dr["ID_Produit"]),
                            NomProduit = dr["NomProduit"]?.ToString() ?? "",
                            Ref = dr["RefProduit"]?.ToString() ?? "",
                            Prix = dr["Prix"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["Prix"]),
                            Devise = dr["Devise"]?.ToString() ?? "CDF",
                            Categorie = dr["Categorie"]?.ToString() ?? "",
                            Taille = dr["Taille"]?.ToString() ?? "",
                            Couleur = dr["Couleur"]?.ToString() ?? ""
                        };
                    }
                }
            }
        }


        private async Task AjouterProduitAuPanierAsync(ProduitScanInfo p, decimal quantite)
        {
            if (p == null) return;
            if (quantite <= 0) quantite = 1;

            // ✅ sécuriser combo/cache (si tu utilises la colonne NomProduit ComboBox)
            EnsureProduitIdInCache(p.ID);

            // ✅ ajoute/incrémente via ta méthode "par ID"
            AddOrIncrementProduitToPanierById_ReturnRowIndex(
                p.ID,
                quantite,
                p.Prix,
                p.Devise,
                p.RefProduit
            );

            // Totaux (si tu as la méthode)
            // MettreAJourTotaux();

            await Task.CompletedTask;
        }

        private async Task<ProduitScanInfo> GetProduitParCodeBarreExactAsync(string codeBarre, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(codeBarre)) return null;

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    ID_Produit,
    NomProduit,
    RefProduit,
    CodeBarre,
    Prix,
    Devise
FROM dbo.Produit
WHERE CodeBarre = @q OR RefProduit = @q;", con))
            {
                cmd.Parameters.Add("@q", SqlDbType.NVarChar, 80).Value = codeBarre.Trim();

                await con.OpenAsync(token);

                using (var rd = await cmd.ExecuteReaderAsync(token))
                {
                    if (!await rd.ReadAsync(token)) return null;

                    return new ProduitScanInfo
                    {
                        ID = Convert.ToInt32(rd["ID_Produit"]),
                        NomProduit = rd["NomProduit"]?.ToString() ?? "",
                        RefProduit = rd["RefProduit"]?.ToString() ?? "",
                        CodeBarre = rd["CodeBarre"]?.ToString() ?? "",
                        Prix = rd["Prix"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Prix"]),
                        Devise = rd["Devise"]?.ToString() ?? ""
                    };
                }
            }
        }

        private async Task<List<ProduitScanInfo>> RechercherProduitsParCodeBarreOuNomAsync(string q, int max, CancellationToken token)
        {
            var res = new List<ProduitScanInfo>();
            if (string.IsNullOrWhiteSpace(q)) return res;

            // On privilégie:
            // - CodeBarre qui commence par q
            // - RefProduit qui commence par q
            // - NomProduit qui contient q
            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP (@top)
    ID_Produit,
    NomProduit,
    RefProduit,
    CodeBarre,
    Prix,
    Devise
FROM dbo.Produit
WHERE
    CodeBarre LIKE @qPrefix
    OR RefProduit LIKE @qPrefix
    OR NomProduit LIKE @qLike
ORDER BY
    CASE WHEN CodeBarre LIKE @qPrefix THEN 0 ELSE 1 END,
    CASE WHEN RefProduit LIKE @qPrefix THEN 0 ELSE 1 END,
    NomProduit;", con))
            {
                cmd.Parameters.Add("@top", SqlDbType.Int).Value = Math.Max(1, max);
                cmd.Parameters.Add("@qPrefix", SqlDbType.NVarChar, 80).Value = q.Trim() + "%";
                cmd.Parameters.Add("@qLike", SqlDbType.NVarChar, 120).Value = "%" + q.Trim() + "%";

                await con.OpenAsync(token);

                using (var rd = await cmd.ExecuteReaderAsync(token))
                {
                    while (await rd.ReadAsync(token))
                    {
                        res.Add(new ProduitScanInfo
                        {
                            ID = Convert.ToInt32(rd["ID_Produit"]),
                            NomProduit = rd["NomProduit"]?.ToString() ?? "",
                            RefProduit = rd["RefProduit"]?.ToString() ?? "",
                            CodeBarre = rd["CodeBarre"]?.ToString() ?? "",
                            Prix = rd["Prix"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Prix"]),
                            Devise = rd["Devise"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return res;
        }


        private void RemplacerLignePanierParProduit(
    SqlConnection con, SqlTransaction trans,
    DataGridViewRow r, int idProduitNew, int quantite)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));

            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ID_Produit, NomProduit, RefProduit, Prix, Devise
FROM dbo.Produit
WHERE ID_Produit = @id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduitNew;

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read())
                        throw new Exception("Produit équivalent introuvable en base.");

                    int id = Convert.ToInt32(rd["ID_Produit"]);
                    string refp = rd["RefProduit"]?.ToString() ?? "";
                    string dev = NormalizeDevise(rd["Devise"]?.ToString() ?? "");
                    decimal prix = rd["Prix"] == DBNull.Value ? 0m : Convert.ToDecimal(rd["Prix"]);

                    EnsureProduitIdInCache(id);

                    if (HasCol(r, "ID_Produit")) r.Cells["ID_Produit"].Value = id;
                    if (HasCol(r, "NomProduit")) r.Cells["NomProduit"].Value = id;

                    if (HasCol(r, "RefProduit")) r.Cells["RefProduit"].Value = refp;

                    if (HasCol(r, "Devise"))
                        SetDeviseOnRow(r, dev);

                    if (HasCol(r, "PrixUnitaire")) r.Cells["PrixUnitaire"].Value = prix;
                    if (HasCol(r, "Quantite")) r.Cells["Quantite"].Value = quantite;

                    if (HasCol(r, "Montant") && HasCol(r, "Quantite") && HasCol(r, "PrixUnitaire"))
                    {
                        decimal q = GetDecimalCell(r, "Quantite");
                        decimal pu = GetDecimalCell(r, "PrixUnitaire");
                        r.Cells["Montant"].Value = Math.Round(q * pu, 2);
                    }
                }
            }
        }


        private string OfflineFolder
        {
            get
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var sub = ConfigurationManager.AppSettings["OfflineFolder"] ?? @"Data\Offline";
                return Path.Combine(baseDir, sub);
            }
        }

        private string FacturePrefix
        {
            get { return ConfigurationManager.AppSettings["FacturePrefix"] ?? "FAC"; }
        }


        private bool IsStockInsuffisant(Exception ex)
        {
            string msg = (ex?.Message ?? "").ToLowerInvariant();
            return msg.Contains("stock") && (msg.Contains("insuff") || msg.Contains("insuffisant") || msg.Contains("quantit"));
        }

        private DataTable GetEquivalentsForProduit(SqlConnection con, SqlTransaction trans, int idProduit, string deviseVente)
        {
            using (var da = new SqlDataAdapter(@"
SELECT 
    e.ID_ProduitEquivalent AS ID_Produit,
    p.NomProduit,
    p.RefProduit,
    p.CodeBarre,
    p.Prix,
    p.Devise,
    p.StockActuel,
    e.Type,
    e.Priorite
FROM dbo.ProduitEquivalence e
JOIN dbo.Produit p ON p.ID_Produit = e.ID_ProduitEquivalent
WHERE e.ID_Produit = @p
  AND e.Actif = 1
  AND (@devise IS NULL OR p.Devise = @devise)
  AND (ISNULL(p.StockActuel,0) > 0)
ORDER BY 
    e.Priorite ASC,
    CASE UPPER(e.Type)
        WHEN 'EQUIVALENT' THEN 1
        WHEN 'REMPLACANT' THEN 2
        WHEN 'ALTERNATIVE' THEN 3
        ELSE 9
    END,
    p.NomProduit ASC;", con))
            {
                da.SelectCommand.Transaction = trans;
                da.SelectCommand.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                da.SelectCommand.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value =
                    string.IsNullOrWhiteSpace(deviseVente) ? (object)DBNull.Value : deviseVente.Trim();

                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private bool IsOnline()
        {
            if (ForceOffline) return false;

            try
            {
                using (var cn = new SqlConnection(_cs))
                {
                    cn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private string EnsureDir(string sub)
        {
            var path = Path.Combine(OfflineFolder, sub);
            Directory.CreateDirectory(path);
            return path;
        }

        // ========================
        // GENERATION CodeFacture
        // ========================

        private string GetNextCodeFacture(int magasinId, DateTime dateVente)
        {
            string dateKey = dateVente.ToString("yyyyMMdd");

            int numero = IsOnline()
                ? NextNumeroOnline(dateKey, magasinId)
                : NextNumeroOffline(dateKey, magasinId);

            // Format: FAC-20260121-0001-M01
            return string.Format("{0}-{1}-{2:0000}-M{3:00}", FacturePrefix, dateKey, numero, magasinId);
        }

        private string BuildReferenceTransactionFromPanier(DataGridView dgv)
        {
            if (dgv == null) return "VENTE";

            // Prendre les RefProduit distincts
            var refs = new List<string>();

            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.IsNewRow) continue;

                string refP = Convert.ToString(r.Cells["RefProduit"]?.Value ?? "").Trim();
                if (string.IsNullOrWhiteSpace(refP)) continue;

                // éviter doublons
                if (!refs.Any(x => x.Equals(refP, StringComparison.OrdinalIgnoreCase)))
                    refs.Add(refP);
            }

            if (refs.Count == 0)
                return "VENTE"; // fallback

            // Exemple: "REF1|REF2|REF3"
            string join = string.Join("|", refs);

            // ⚠️ IMPORTANT : adapter à la taille de ta colonne SQL ReferenceTransaction
            // si la colonne est NVARCHAR(120) => limite à 120
            const int MAX = 120;

            if (join.Length <= MAX)
                return join;

            // Si trop long : on garde un résumé
            // Exemple: "MULTI(35):REF1|REF2|REF3..."
            string head = string.Join("|", refs.Take(6));
            string summary = $"MULTI({refs.Count}):{head}...";
            if (summary.Length > MAX)
                summary = summary.Substring(0, MAX);

            return summary;
        }

        

        private int NextNumeroOnline(string dateKey, int magasinId)
        {
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();

                using (var cmd = new SqlCommand("dbo.usp_FactureSequence_Next", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@DateKey", dateKey);
                    cmd.Parameters.AddWithValue("@MagasinId", magasinId);

                    var pOut = cmd.Parameters.Add("@NextNumero", SqlDbType.Int);
                    pOut.Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    return Convert.ToInt32(pOut.Value);
                }
            }
        }

        private int GetNextNumeroFacture(SqlConnection con, SqlTransaction trans, string dateKey, int magasinId)
        {
            using (var cmd = new SqlCommand("dbo.usp_FactureSequence_Next", con, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@DateKey", SqlDbType.Char, 8).Value = dateKey;
                cmd.Parameters.Add("@MagasinId", SqlDbType.Int).Value = magasinId;

                var pOut = cmd.Parameters.Add("@NextNumero", SqlDbType.Int);
                pOut.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();
                return Convert.ToInt32(pOut.Value);
            }
        }

        private string BuildCodeFactureSequence(SqlConnection con, SqlTransaction trans, int magasinId, DateTime dateVente)
        {
            string dateKey = dateVente.ToString("yyyyMMdd");
            int next = GetNextNumeroFacture(con, trans, dateKey, magasinId);

            return string.Format("{0}-{1}-{2:0000}-M{3:00}", FacturePrefix, dateKey, next, magasinId);
        }

        private static readonly object _offlineLock = new object();

        private int NextNumeroOffline(string dateKey, int magasinId)
        {
            var folder = EnsureDir("counters");
            var file = Path.Combine(folder, string.Format("counter_{0}_M{1:00}.txt", dateKey, magasinId));

            lock (_offlineLock)
            {
                int last = 0;
                if (File.Exists(file))
                {
                    int.TryParse(File.ReadAllText(file).Trim(), out last);
                }

                int next = last + 1;
                File.WriteAllText(file, next.ToString());
                return next;
            }
        }

        // ========================
        // A APPELER AU LOAD ou RESET
        // ========================

        private void PreparerNouvelleVente()
        {
            int magasinId = 1; 
            DateTime now = DateTime.Now;

            _codeFactureCourant = GetNextCodeFacture(magasinId, now);

            
        }

        private void SetProduitOnRow(DataGridViewRow row, ProduitCombo p, decimal qte = 1m)
        {
            if (row == null || p == null) return;

            EnsureProduitIdInCache(p.ID);

            if (HasCol(row, "NomProduit")) row.Cells["NomProduit"].Value = p.ID;
            if (HasCol(row, "ID_Produit")) row.Cells["ID_Produit"].Value = p.ID;
            if (HasCol(row, "RefProduit")) row.Cells["RefProduit"].Value = p.Ref;

            if (HasCol(row, "PrixUnitaire")) row.Cells["PrixUnitaire"].Value = p.Prix;
            if (HasCol(row, "Categorie")) row.Cells["Categorie"].Value = p.Categorie;
            if (HasCol(row, "Taille")) row.Cells["Taille"].Value = p.Taille;
            if (HasCol(row, "Couleur")) row.Cells["Couleur"].Value = p.Couleur;

            if (HasCol(row, "Quantite") && qte > 0) row.Cells["Quantite"].Value = qte;

            // ✅ DEVISE (ComboBox) : injecter puis assigner
            if (HasCol(row, "Devise"))
                SetDeviseOnRow(row, p.Devise);

            // recalcul ligne
            if (HasCol(row, "Montant"))
            {
                decimal qty = GetDecimalCell(row, "Quantite");
                decimal pu = GetDecimalCell(row, "PrixUnitaire");
                decimal rem = GetDecimalCell(row, "Remise");
                decimal tva = GetDecimalCell(row, "TVA");

                decimal ht = qty * pu;
                decimal remise = ht * rem / 100m;
                decimal baseTva = ht - remise;
                decimal tvaMont = baseTva * tva / 100m;

                row.Cells["Montant"].Value = Math.Round(baseTva + tvaMont, 2);
            }
        }


        private decimal PreviewCouponDiscount(string codeCoupon, decimal totalAchat, int idClientOrZero)
        {
            if (string.IsNullOrWhiteSpace(codeCoupon)) return 0m;

            using (var cn = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                cn.Open();

                using (var cmd = new SqlCommand(@"
SELECT Type, Valeur, DateDebut, DateFin, MinAchat, UtilisationsMax, UtilisationsClientMax, Actif
FROM Coupon WHERE Code=@c;", cn))
                {
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = codeCoupon.Trim().ToUpperInvariant();

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return 0m;

                        bool actif = !r.IsDBNull(7) && r.GetBoolean(7);
                        if (!actif) return 0m;

                        string type = r.IsDBNull(0) ? "" : r.GetString(0);
                        decimal valeur = r.IsDBNull(1) ? 0m : Convert.ToDecimal(r.GetValue(1));
                        DateTime deb = r.IsDBNull(2) ? DateTime.MinValue : r.GetDateTime(2);
                        DateTime fin = r.IsDBNull(3) ? DateTime.MinValue : r.GetDateTime(3);
                        decimal min = r.IsDBNull(4) ? 0m : Convert.ToDecimal(r.GetValue(4));

                        DateTime today = DateTime.Today;
                        if (today < deb.Date || today > fin.Date) return 0m;
                        if (totalAchat < min) return 0m;
                    }
                }

                int? maxUse = null;
                int? maxClientUse = null;

                using (var cmd2 = new SqlCommand(@"SELECT UtilisationsMax, UtilisationsClientMax FROM Coupon WHERE Code=@c;", cn))
                {
                    cmd2.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = codeCoupon.Trim().ToUpperInvariant();
                    using (var rr = cmd2.ExecuteReader())
                    {
                        if (rr.Read())
                        {
                            if (!rr.IsDBNull(0)) maxUse = rr.GetInt32(0);
                            if (!rr.IsDBNull(1)) maxClientUse = rr.GetInt32(1);
                        }
                    }
                }

                if (maxUse.HasValue)
                {
                    using (var c3 = new SqlCommand("SELECT COUNT(*) FROM CouponUsage WHERE Code=@c;", cn))
                    {
                        c3.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = codeCoupon.Trim().ToUpperInvariant();
                        int used = Convert.ToInt32(c3.ExecuteScalar());
                        if (used >= maxUse.Value) return 0m;
                    }
                }

                if (idClientOrZero > 0 && maxClientUse.HasValue)
                {
                    using (var c4 = new SqlCommand("SELECT COUNT(*) FROM CouponUsage WHERE Code=@c AND IdClient=@id;", cn))
                    {
                        c4.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = codeCoupon.Trim().ToUpperInvariant();
                        c4.Parameters.Add("@id", SqlDbType.Int).Value = idClientOrZero;
                        int used = Convert.ToInt32(c4.ExecuteScalar());
                        if (used >= maxClientUse.Value) return 0m;
                    }
                }

                string t = "FIXE";
                decimal v = 0m;

                using (var cmd3 = new SqlCommand(@"SELECT Type, Valeur FROM Coupon WHERE Code=@c;", cn))
                {
                    cmd3.Parameters.Add("@c", SqlDbType.NVarChar, 30).Value = codeCoupon.Trim().ToUpperInvariant();
                    using (var r3 = cmd3.ExecuteReader())
                    {
                        if (r3.Read())
                        {
                            t = r3.IsDBNull(0) ? "FIXE" : r3.GetString(0);
                            v = r3.IsDBNull(1) ? 0m : Convert.ToDecimal(r3.GetValue(1));
                        }
                    }
                }

                decimal remise = 0m;
                if (t.Equals("FIXE", StringComparison.OrdinalIgnoreCase)) remise = v;
                else if (t.Equals("POURCENT", StringComparison.OrdinalIgnoreCase)) remise = totalAchat * v / 100m;

                if (remise < 0m) remise = 0m;
                if (remise > totalAchat) remise = totalAchat;
                return Math.Round(remise, 2);
            }
        }

        private bool ExecuteSecured(
        string permissionCode,
        string actionTitle,
        string reference,
        string details,
        Action actionSiOk,
        int minutesUnlock = 10)
        {
            if (actionSiOk == null) return false;

            // ✅ Admin = accès direct
            if (RolesSecurite.EstAdmin(SessionEmploye.Poste))
            {
                actionSiOk();
                return true;
            }

            // ✅ Non-admin : boutons OPEN autorisés
            if (IsOpenButton(permissionCode))
            {
                actionSiOk();
                return true;
            }

            // ✅ Permission obligatoire
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                MessageBox.Show("Action non autorisée.", "Blocage",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            permissionCode = permissionCode.Trim();

            // ✅ Déjà débloqué ?
            if (DeblocageManager.EstDebloque(permissionCode))
            {
                actionSiOk();
                return true;
            }

            // ✅ Demander signature manager AVANT toute action
            using (var fSig = new FrmSignatureManager(
                ConfigSysteme.ConnectionString,
                typeAction: permissionCode,
                permissionCode: permissionCode,
                reference: string.IsNullOrWhiteSpace(reference) ? "VENTE_EN_COURS" : reference,
                details: string.IsNullOrWhiteSpace(details) ? actionTitle : details,
                idEmployeDemandeur: SessionEmploye.ID_Employe))
            {
                if (fSig.ShowDialog(this) != DialogResult.OK || !fSig.Approved)
                {
                    MessageBox.Show("Validation refusée : autorisation manager requise.", "Blocage",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            // ✅ OK => débloquer X minutes
            DeblocageManager.Debloquer(permissionCode, minutesUnlock);

            // ✅ seulement maintenant on exécute
            actionSiOk();
            return true;
        }

        private bool IsOpenButton(string permissionCode)
        {
            // ✅ 4 boutons ouverts pour les non-admin
            // (on marque ces boutons comme OPEN_* dans le mapping)
            return string.Equals(permissionCode, "OPEN", StringComparison.OrdinalIgnoreCase);
        }


        private readonly HashSet<string> _boutonsSansSignature = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "btnAjouter",
    "btnSupprimerArticle",
    "btnAnnuler",
    "btnFinaliser",
    "btnImprimerTicket"

};

        private void ConfigurerSecuriteBoutons_Ventes()
        {
            // ====== 4 BOUTONS OUVERTS (NON-ADMIN) ======
            SecureButton(btnAjouter, "OPEN", () => btnAjouter_Click(btnAjouter, EventArgs.Empty), "Ajouter");
            SecureButton(btnSupprimerArticle, "OPEN", () => btnSupprimerArticle_Click(btnSupprimerArticle, EventArgs.Empty), "Supprimer ligne");
            SecureButton(btnAnnuler, "OPEN", () => btnAnnuler_Click(btnAnnuler, EventArgs.Empty), "Annuler");
            SecureButton(btnFinaliser, "OPEN", () => btnFinaliser_Click(btnFinaliser, EventArgs.Empty), "Finaliser");
            SecureButton(btnImprimerTicket, "OPEN", () => btnImprimerTicket_Click(btnImprimerTicket, EventArgs.Empty), "Impression Ticket");

            // ====== TOUT LE RESTE => SIGNATURE MANAGER POUR NON-ADMIN ======
            // Rapports / exports
            SecureButton(btnCharger, "VENTE_RAPPORT_CHARGER", () => btnCharger_Click(btnCharger, EventArgs.Empty), "Charger rapport");
            SecureButton(btnRechercher, "VENTE_RAPPORT_RECHERCHER", () => btnRechercher_Click(btnRechercher, EventArgs.Empty), "Rechercher");
            SecureButton(btnExporterPDF, "VENTE_EXPORT_PDF", () => btnExporterPDF_Click(btnExporterPDF, EventArgs.Empty), "Exporter PDF");
            SecureButton(btnExporterExcel, "VENTE_EXPORT_EXCEL", () => btnExporterExcel_Click(btnExporterExcel, EventArgs.Empty), "Exporter Excel");

            // Impression / aperçu
            SecureButton(btnApercu, "VENTE_APERCU", () => btnApercu_Click(btnApercu, EventArgs.Empty), "Aperçu");
            SecureButton(btnImprimer, "VENTE_IMPRIMER_A4", () => btnImprimer_Click(btnImprimer, EventArgs.Empty), "Imprimer A4");
            SecureButton(btnDuplicataA4, "VENTE_DUPLICATA_A4", () => btnDuplicataA4_Click(btnDuplicataA4, EventArgs.Empty), "Duplicata A4");

            // Annulation vente / paiements
            SecureButton(btnAnnulerVente, "VENTE_ANNULER_VENTE", () => btnAnnulerVente_Click(btnAnnulerVente, EventArgs.Empty), "Annuler Vente");
            SecureButton(btnVoirPaiements, "VENTE_VOIR_PAIEMENTS", () => btnVoirPaiements_Click(btnVoirPaiements, EventArgs.Empty), "Voir paiements");

            // Inventaire / total du jour / rapport mensuel etc.
            SecureButton(btnInventaireDuJour, "VENTE_INVENTAIRE_JOUR", () => btnInventaireDuJour_Click(btnInventaireDuJour, EventArgs.Empty), "Inventaire du jour");
            SecureButton(BtnTotalDuJour, "VENTE_TOTAL_JOUR", () => BtnTotalDuJour_Click(BtnTotalDuJour, EventArgs.Empty), "Total du jour");
            SecureButton(btnGenererRapport, "VENTE_GENERER_RAPPORT", () => btnGenererRapport_Click(btnGenererRapport, EventArgs.Empty), "Générer rapport");

            // Test connexion
            SecureButton(btnTesterConnexion, "VENTE_TEST_CONNEXION", () => btnTesterConnexion_Click(btnTesterConnexion, EventArgs.Empty), "Tester connexion");
        }
        private void OpenButton(Button btn, Action action)
        {
            if (btn == null) return;

            btn.Click -= GenericOpen_Click;
            btn.Click += GenericOpen_Click;

            // stocker l’action dans Tag
            btn.Tag = action;
        }

        private void GenericOpen_Click(object sender, EventArgs e)
        {
            if (sender is Button b && b.Tag is Action act)
                act();
        }

        private void SecureButton(Button btn, string permissionCode, Action action, string title = null)
        {
            if (btn == null) return;

            btn.Click -= GenericSecure_Click;
            btn.Click += GenericSecure_Click;

            btn.Tag = new SecuredBtnInfo
            {
                PermissionCode = permissionCode,
                Action = action,
                Title = title ?? btn.Text
            };
        }


        private void GenericSecure_Click(object sender, EventArgs e)
        {
            if (!(sender is Button b)) return;
            if (!(b.Tag is SecuredBtnInfo info) || info.Action == null)
            {
                MessageBox.Show("Bouton non configuré.", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string reference = BuildReferenceVente();
            string details = BuildDetailsForAction(info.Title);

            ExecuteSecured(
                permissionCode: info.PermissionCode,
                actionTitle: info.Title,
                reference: reference,
                details: details,
                actionSiOk: info.Action,
                minutesUnlock: 10
            );
        }

        private sealed class SecuredBtnInfo
        {
            public string PermissionCode { get; set; }
            public Action Action { get; set; }
            public string Title { get; set; }
        }

        private string BuildReferenceVente()
        {
            // Si tu as un code facture en cours, mets-le
            // sinon “VENTE_EN_COURS”
            return string.IsNullOrWhiteSpace(_lastCodeFacture) ? "VENTE_EN_COURS" : _lastCodeFacture;
        }

        private string BuildDetailsForAction(string actionName)
        {
            string client = (cmbNomClient?.Text ?? "").Trim();
            string total = (txtTotalTTC?.Text ?? "").Trim();
            string caisse = (txtNomCaissier?.Text ?? "").Trim();

            return $"Action: {actionName}\nClient: {client}\nTotal: {total}\nCaissier: {caisse}";
        }


        private void LoadLoyaltySnapshot(SqlConnection con, SqlTransaction trans, int idClient, string deviseVente)
        {
            _lastTauxFidelite = 0.005m;
            _lastSoldeFideliteCDF = 0m;
            _lastSoldeFideliteUSD = 0m;

            using (var cmd = new SqlCommand(@"
SELECT Statut, Points, CashbackSolde
FROM LoyaltyCompte WHERE IdClient=@id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return;

                    string statut = r.IsDBNull(0) ? "BRONZE" : r.GetString(0);
                    decimal cashback = r.IsDBNull(2) ? 0m : Convert.ToDecimal(r.GetValue(2));

                    if (statut.Equals("SILVER", StringComparison.OrdinalIgnoreCase)) _lastTauxFidelite = 0.010m;
                    else if (statut.Equals("GOLD", StringComparison.OrdinalIgnoreCase)) _lastTauxFidelite = 0.015m;

                    if (deviseVente.Equals("USD", StringComparison.OrdinalIgnoreCase)) _lastSoldeFideliteUSD = cashback;
                    else _lastSoldeFideliteCDF = cashback;
                }
            }
        }

        private int InsertVenteCompleteOnline(
    string codeFacture,
    int idClient,
    int idEmploye,
    int idSession,
    string modePaiement,
    string devise,
    string nomCaissier,
    decimal montantTotal,
    decimal remisePct,
    decimal remiseMont,
    decimal netAPayer,
    List<LigneVenteDTO> lignes,
    string referenceTransaction
)
        {
            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();

                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        int idVente;

                        // 1) INSERT Vente
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Vente
(DateVente, ID_Client, IDEmploye, ModePaiement, MontantTotal, NomCaissier, Devise, IdSession,
 CodeFacture, Statut, RemiseTicketPct, RemiseTicketMontant,
 IdEntreprise, IdMagasin, IdPoste,AnnulePar, DateAnnulation, MotifAnnulation)
OUTPUT INSERTED.ID_Vente
VALUES
(GETDATE(), @idClient, @idEmploye, @mode, @total, @caissier, @devise, @idSession,
 @code, 'VALIDEE', @remPct, @remMont, @IdEntreprise, @IdMagasin, @IdPoste,
 '', NULL, '');", cn, tx))
                        {
                            cmd.Parameters.Add("@idClient", SqlDbType.Int).Value = idClient;
                            cmd.Parameters.Add("@idEmploye", SqlDbType.Int).Value = idEmploye;

                            cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 80).Value =
                                string.IsNullOrWhiteSpace(modePaiement) ? "INCONNU" : modePaiement.Trim();

                            var pTotal = cmd.Parameters.Add("@total", SqlDbType.Decimal);
                            pTotal.Precision = 18; pTotal.Scale = 2; pTotal.Value = montantTotal;

                            cmd.Parameters.Add("@caissier", SqlDbType.NVarChar, 120).Value =
                                string.IsNullOrWhiteSpace(nomCaissier) ? "SYSTEM" : nomCaissier.Trim();

                            cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value =
                                string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim();

                            cmd.Parameters.Add("@idSession", SqlDbType.Int).Value = idSession;
                            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 30).Value = codeFacture;

                            var p1 = cmd.Parameters.Add("@remPct", SqlDbType.Decimal); p1.Precision = 18; p1.Scale = 2; p1.Value = remisePct;
                            var p2 = cmd.Parameters.Add("@remMont", SqlDbType.Decimal); p2.Precision = 18; p2.Scale = 2; p2.Value = remiseMont;
                            cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                            cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                            cmd.Parameters.Add("@IdPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                            idVente = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2) INSERT VenteDetails
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.DetailsVente
(ID_Vente, ID_Produit, Quantite, PrixUnitaire, RefProduit, NomProduit,
 Remise, TVA, Montant, Devise, NomCaissier,
 IdEntreprise, IdMagasin, IdPoste, QuantiteRetournee)
VALUES
(@idVente, @idProduit, @qte, @pu, @ref, @nom,
 @remise, @tva, @montant, @devise, @caissier,
 @idEntreprise, @idMagasin, @idPoste, @qteRetournee);", cn, tx))
                        {
                            cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;

                            var pProd = cmd.Parameters.Add("@idProduit", SqlDbType.Int);
                            var pQte = cmd.Parameters.Add("@qte", SqlDbType.Decimal); pQte.Precision = 18; pQte.Scale = 2;

                            var pPU = cmd.Parameters.Add("@pu", SqlDbType.Decimal); pPU.Precision = 18; pPU.Scale = 2;

                            var pRef = cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 60);
                            var pNom = cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 200);

                            var pRem = cmd.Parameters.Add("@remise", SqlDbType.Decimal); pRem.Precision = 18; pRem.Scale = 2;
                            var pTva = cmd.Parameters.Add("@tva", SqlDbType.Decimal); pTva.Precision = 18; pTva.Scale = 2;

                            var pMont = cmd.Parameters.Add("@montant", SqlDbType.Decimal); pMont.Precision = 18; pMont.Scale = 2;

                            var pDev = cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10);
                            var pCais = cmd.Parameters.Add("@caissier", SqlDbType.NVarChar, 80);

                            // ⚠️ adapte ces 3 valeurs selon ton contexte POS (ConfigSysteme / Session / PosContextService)
                            cmd.Parameters.Add("@idEntreprise", SqlDbType.Int).Value = ConfigSysteme.IdEntreprise;
                            cmd.Parameters.Add("@idMagasin", SqlDbType.Int).Value = ConfigSysteme.IdMagasin;
                            cmd.Parameters.Add("@idPoste", SqlDbType.Int).Value = ConfigSysteme.IdPoste;

                            var pQteRet = cmd.Parameters.Add("@qteRetournee", SqlDbType.Decimal);
                            pQteRet.Precision = 18; pQteRet.Scale = 2;

                            foreach (var l in lignes)
                            {
                                pProd.Value = l.ProduitId;
                                pQte.Value = l.Qte;
                                pPU.Value = l.PU;

                                // ✅ pas dans LigneVenteDTO -> on met vide / valeur par défaut
                                pRef.Value = "";              // RefProduit inconnu
                                pNom.Value = l.Libelle ?? ""; // NomProduit = Libelle

                                decimal remise = 0m;
                                decimal tva = 0m;

                                pRem.Value = remise;
                                pTva.Value = tva;

                                decimal montant = (l.PU * l.Qte) - remise + tva;
                                pMont.Value = montant;

                                pDev.Value = "CDF";                 // devise par défaut
                                pCais.Value = SessionEmploye.Nom;    // caissier = employé connecté

                                pQteRet.Value = 0m;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3) INSERT PaiementsVente
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

    DeviseOriginale,
    MontantOriginal,
    TauxApplique,

    IdEntreprise,
    IdMagasin,
    IdPoste,

    AnnulePar,
    DateAnnulation,
    MotifAnnulation
)
VALUES
(
    @idVente,
    @mode,
    @devise,
    @montant,
    GETDATE(),
    @ref,
    'VALIDE',

    @devOrig,
    @montOrig,
    @taux,

    @IdEntreprise,
    @IdMagasin,
    @IdPoste,

    '', NULL, ''
);", cn, tx))
                        {
                            cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;

                            cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 50).Value =
                                string.IsNullOrWhiteSpace(modePaiement) ? "INCONNU" : modePaiement.Trim().ToUpperInvariant();

                            cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value =
                                string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim().ToUpperInvariant();

                            var pM = cmd.Parameters.Add("@montant", SqlDbType.Decimal);
                            pM.Precision = 18; pM.Scale = 2; pM.Value = netAPayer;

                            // ✅ vraie ref transaction (pas note)
                            cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 100).Value =
                                string.IsNullOrWhiteSpace(referenceTransaction) ? (object)DBNull.Value : referenceTransaction.Trim();

                            // ✅ multi-devise: ici Online ne convertit pas -> on enregistre original = même chose
                            cmd.Parameters.Add("@devOrig", SqlDbType.NVarChar, 10).Value =
                                string.IsNullOrWhiteSpace(devise) ? (object)DBNull.Value : devise.Trim().ToUpperInvariant();

                            var pMO = cmd.Parameters.Add("@montOrig", SqlDbType.Decimal);
                            pMO.Precision = 18; pMO.Scale = 2; pMO.Value = netAPayer;

                            var pT = cmd.Parameters.Add("@taux", SqlDbType.Decimal);
                            pT.Precision = 18; pT.Scale = 6; pT.Value = 1m;

                            // ✅ CONTEXTE POS (⚠️ adapte si tu utilises ConfigSysteme au lieu AppContext)
                            cmd.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                            cmd.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                            cmd.Parameters.Add("@IdPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return idVente;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // ========================
        // OFFLINE JSON (System.Text.Json)
        // ========================

        private void SaveVenteOfflineJson(
            string codeFacture,
            int idClient,
            int idEmploye,
            int idSession,
            string modePaiement,
            string devise,
            string nomCaissier,
            decimal montantTotal,
            decimal remisePct,
            decimal remiseMont,
            decimal netAPayer,
            List<LigneVenteDTO> lignes,
            string referenceTransaction
        )
        {
            var folder = EnsureDir("pending_sales");
            var path = Path.Combine(folder, codeFacture + ".json");

            var payload = new
            {
                CodeFacture = codeFacture,
                DateVente = DateTime.Now,
                IdClient = idClient,
                IdEmploye = idEmploye,
                IdSession = idSession,
                ModePaiement = string.IsNullOrWhiteSpace(modePaiement) ? "INCONNU" : modePaiement.Trim(),
                Devise = string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim(),
                NomCaissier = string.IsNullOrWhiteSpace(nomCaissier) ? "SYSTEM" : nomCaissier.Trim(),
                MontantTotal = montantTotal,
                RemiseTicketPct = remisePct,
                RemiseTicketMontant = remiseMont,
                NetAPayer = netAPayer,
                ReferenceTransaction = string.IsNullOrWhiteSpace(referenceTransaction) ? "" : referenceTransaction.Trim(),
                Lignes = lignes
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }



        private string GenererPdfFactureDepuisDb(int idVente, string codeFacture, string dossierSortie = null)
        {
            if (idVente <= 0) throw new Exception("ID vente invalide.");

            if (string.IsNullOrWhiteSpace(dossierSortie))
                dossierSortie = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "FacturesPDF");

            Directory.CreateDirectory(dossierSortie);

            // =======================
            // 1) Charger données DB
            // =======================
            DateTime dateVente = DateTime.Now;
            string devise = "CDF";
            string caissier = "SYSTEM";
            string clientNomComplet = "";
            string telephone = "";
            string modePaiement = "INCONNU";
            decimal montantTotal = 0m;

            int idClient = 0;

            // ✅ Catégorie client (nouveau)
            string categorieClient = "OCCASIONNEL";
            bool clientEstFidele = false;

            DataTable dtLignes = new DataTable();

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                // Header vente + client (✅ AJOUT CategorieClient)
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    v.ID_Client,
    v.DateVente,
    ISNULL(v.Devise,'CDF') AS Devise,
    ISNULL(v.NomCaissier,'SYSTEM') AS NomCaissier,
    ISNULL(v.ModePaiement,'INCONNU') AS ModePaiement,
    ISNULL(v.MontantTotal,0) AS MontantTotal,
    ISNULL(c.Prenom,'') AS Prenom,
    ISNULL(c.Nom,'') AS Nom,
    ISNULL(c.Telephone,'') AS Telephone,
    ISNULL(NULLIF(LTRIM(RTRIM(c.CategorieClient)),''),'OCCASIONNEL') AS CategorieClient
FROM dbo.Vente v
LEFT JOIN dbo.Clients c ON c.ID_Clients = v.ID_Client
WHERE v.ID_Vente = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            throw new Exception("Vente introuvable (ID_Vente=" + idVente + ").");

                        idClient = r["ID_Client"] == DBNull.Value ? 0 : Convert.ToInt32(r["ID_Client"]);
                        dateVente = Convert.ToDateTime(r["DateVente"]);
                        devise = (r["Devise"] ?? "CDF").ToString().Trim();
                        caissier = (r["NomCaissier"] ?? "SYSTEM").ToString().Trim();
                        modePaiement = (r["ModePaiement"] ?? "INCONNU").ToString().Trim();
                        montantTotal = Convert.ToDecimal(r["MontantTotal"]);

                        string prenom = (r["Prenom"] ?? "").ToString().Trim();
                        string nom = (r["Nom"] ?? "").ToString().Trim();
                        clientNomComplet = (prenom + " " + nom).Trim();
                        telephone = (r["Telephone"] ?? "").ToString().Trim();

                        // ✅ lecture catégorie client (nouveau)
                        categorieClient = (r["CategorieClient"] ?? "OCCASIONNEL").ToString().Trim().ToUpperInvariant();
                        clientEstFidele = categorieClient.Equals("FIDELE", StringComparison.OrdinalIgnoreCase);
                    }
                }

                // ✅ Détails vente (CORRIGÉ : ORDER BY ID_DetailsVente)
                using (var cmd = new SqlCommand(@"
SELECT
    ISNULL(NomProduit,'') AS NomProduit,
    ISNULL(Quantite,0) AS Quantite,
    ISNULL(PrixUnitaire,0) AS PrixUnitaire,
    ISNULL(Remise,0) AS RemisePct,
    ISNULL(TVA,0) AS TVAPct,
    ISNULL(Montant,0) AS Montant,
    ISNULL(RefProduit,'') AS RefProduit
FROM dbo.DetailsVente
WHERE ID_Vente = @id
ORDER BY ID_Details;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                    using (var da = new SqlDataAdapter(cmd))
                        da.Fill(dtLignes);
                }
            }

            if (dtLignes.Rows.Count == 0)
                throw new Exception("Aucune ligne trouvée dans DetailsVente pour cette vente.");

            // =======================
            // 2) Préparer CodeFacture + PATH
            // =======================
            string safeCode = (codeFacture ?? "").Trim();
            if (string.IsNullOrWhiteSpace(safeCode))
                safeCode = "FAC_" + idVente;

            // dossierSortie peut être un dossier OU un filepath
            string path;
            if (!string.IsNullOrWhiteSpace(dossierSortie) && dossierSortie.Trim().EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                // ✅ c'est un fichier complet
                path = dossierSortie.Trim();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            else
            {
                // ✅ c'est un dossier
                if (string.IsNullOrWhiteSpace(dossierSortie))
                    dossierSortie = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "FacturesPDF");

                Directory.CreateDirectory(dossierSortie);
                path = Path.Combine(dossierSortie, safeCode + ".pdf");
            }

            // =======================
            // 3) Générer PDF iTextSharp (DESIGN PRO)
            // =======================
            var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
            var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
            var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
            var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);
            var fontCellBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9f);

            BaseColor grayHeader = new BaseColor(240, 240, 240);
            BaseColor grayBorder = new BaseColor(200, 200, 200);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 36f, 36f, 28f, 30f))
            {
                var writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                // ===== ENTÊTE ENTREPRISE + LOGO =====
                PdfPTable top = new PdfPTable(2) { WidthPercentage = 100 };
                top.SetWidths(new float[] { 70f, 30f });

                PdfPCell left = new PdfPCell { Border = PdfPCell.NO_BORDER };
                left.AddElement(new Paragraph("ZAIRE MODE SARL", fontTitre));
                left.AddElement(new Paragraph("23, Bld Lumumba / Immeuble Masina Plaza", fontSmall));
                left.AddElement(new Paragraph("+243861507560 / E-MAIL: Zaireshop@hotmail.com", fontSmall));
                left.AddElement(new Paragraph("PAGE: ZAIRE.CD", fontSmall));
                left.AddElement(new Paragraph("RCCM: 25-B-01497", fontSmall));
                left.AddElement(new Paragraph("IDNAT: 01-F4300-N73258E", fontSmall));
                top.AddCell(left);

                PdfPCell right = new PdfPCell { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

                string logoPath = @"D:\ZAIRE\LOGO1.png";
                if (File.Exists(logoPath))
                {
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(90f, 90f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    right.AddElement(logo);
                }
                top.AddCell(right);

                doc.Add(top);
                doc.Add(new Paragraph(" "));

                doc.Add(new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(
                    1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0))));

                // ===== INFOS FACTURE =====
                PdfPTable info = new PdfPTable(2)
                {
                    WidthPercentage = 100f,
                    SpacingBefore = 8f,
                    SpacingAfter = 10f
                };
                info.SetWidths(new float[] { 50f, 50f });

                PdfPCell InfoCell(string t)
                {
                    return new PdfPCell(new Phrase(t ?? "", fontSmall))
                    {
                        Border = PdfPCell.NO_BORDER,
                        PaddingBottom = 3f
                    };
                }

                string leftInfo =
                    "Ticket N°: " + dateVente.ToString("yyMMddHHmmss") + "\n" +
                    "Date : " + dateVente.ToString("dd/MM/yyyy HH:mm:ss");

                string rightInfo =
                    "Facture N° : " + safeCode + "\n" +
                    "Caissier : " + (string.IsNullOrWhiteSpace(caissier) ? "SYSTEM" : caissier) + "\n" +
                    "Client : " + (string.IsNullOrWhiteSpace(clientNomComplet) ? "-" : clientNomComplet) +
                    (string.IsNullOrWhiteSpace(telephone) ? "" : ("\nTél : " + telephone));

                info.AddCell(InfoCell(leftInfo));
                info.AddCell(InfoCell(rightInfo));
                doc.Add(info);

                // ===== TABLEAU LIGNES =====
                PdfPTable table = new PdfPTable(4)
                {
                    WidthPercentage = 100f,
                    SpacingBefore = 5f,
                    SpacingAfter = 10f
                };
                table.SetWidths(new float[] { 52f, 10f, 18f, 20f });

                PdfPCell MakeCell(string text, iTextSharp.text.Font font, int align, BaseColor bg, bool boldBorder)
                {
                    return new PdfPCell(new Phrase(text ?? "", font))
                    {
                        HorizontalAlignment = align,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        BackgroundColor = bg,
                        PaddingTop = 6f,
                        PaddingBottom = 6f,
                        PaddingLeft = 5f,
                        PaddingRight = 5f,
                        BorderColor = grayBorder,
                        BorderWidth = boldBorder ? 1.2f : 0.6f,
                        NoWrap = false
                    };
                }

                table.AddCell(MakeCell("Article", fontHeader, Element.ALIGN_LEFT, grayHeader, true));
                table.AddCell(MakeCell("Qté", fontHeader, Element.ALIGN_CENTER, grayHeader, true));
                table.AddCell(MakeCell("PU", fontHeader, Element.ALIGN_RIGHT, grayHeader, true));
                table.AddCell(MakeCell("Total", fontHeader, Element.ALIGN_RIGHT, grayHeader, true));

                decimal totalTTC = 0m, totalRemise = 0m, totalTVA = 0m;

                foreach (DataRow row in dtLignes.Rows)
                {
                    string nomProduit = (row["NomProduit"] ?? "").ToString();

                    int qte = Convert.ToInt32(row["Quantite"]);
                    decimal pu = Convert.ToDecimal(row["PrixUnitaire"]);
                    decimal remisePct = Convert.ToDecimal(row["RemisePct"]);
                    decimal tvaPct = Convert.ToDecimal(row["TVAPct"]);

                    decimal sousTotal = qte * pu;
                    decimal montantRemise = sousTotal * remisePct / 100m;
                    decimal baseTVA = sousTotal - montantRemise;
                    decimal montantTVA = baseTVA * tvaPct / 100m;
                    decimal totalLigne = baseTVA + montantTVA;

                    totalTTC += totalLigne;
                    totalRemise += montantRemise;
                    totalTVA += montantTVA;

                    table.AddCell(MakeCell(nomProduit, fontCell, Element.ALIGN_LEFT, null, false));
                    table.AddCell(MakeCell(qte.ToString(), fontCell, Element.ALIGN_CENTER, null, false));
                    table.AddCell(MakeCell(FormatMontant(pu, devise), fontCell, Element.ALIGN_RIGHT, null, false));
                    table.AddCell(MakeCell(FormatMontant(totalLigne, devise), fontCell, Element.ALIGN_RIGHT, null, false));
                }

                doc.Add(table);

                // ===== TOTAUX =====
                PdfPTable totals = new PdfPTable(2)
                {
                    WidthPercentage = 45f,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                totals.SetWidths(new float[] { 55f, 45f });

                PdfPCell TotLabel(string t) => new PdfPCell(new Phrase(t, fontCellBold))
                {
                    BorderColor = grayBorder,
                    Padding = 6f,
                    BackgroundColor = new BaseColor(250, 250, 250)
                };

                PdfPCell TotVal(string t) => new PdfPCell(new Phrase(t, fontCell))
                {
                    BorderColor = grayBorder,
                    Padding = 6f,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };

                totals.AddCell(TotLabel("Remise"));
                totals.AddCell(TotVal(FormatMontant(totalRemise, devise)));

                totals.AddCell(TotLabel("TVA"));
                totals.AddCell(TotVal(FormatMontant(totalTVA, devise)));

                totals.AddCell(TotLabel("TOTAL TTC"));
                totals.AddCell(TotVal(FormatMontant(totalTTC, devise)));

                doc.Add(totals);

                doc.Add(new Paragraph("Mode : " + (string.IsNullOrWhiteSpace(modePaiement) ? "INCONNU" : modePaiement) + " (" + devise + ")",
                    fontSmall)
                { SpacingBefore = 10f });

                // ===== FIDELITE (UNIQUEMENT CLIENT FIDELE) =====
                if (idClient > 0 && clientEstFidele)
                {
                    try
                    {
                        using (var con2 = new SqlConnection(ConfigSysteme.ConnectionString))
                        {
                            con2.Open();

                            // ✅ relire le bon client depuis la vente
                            int idClientVente = 0;
                            using (var cmdC = new SqlCommand("SELECT ID_Client FROM dbo.Vente WHERE ID_Vente=@v", con2))
                            {
                                cmdC.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                                var o = cmdC.ExecuteScalar();
                                idClientVente = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                            }

                            if (idClientVente > 0)
                                ChargerInfosFideliteClient(con2, null, idClientVente);
                        }

                        // ✅ gain affiché (info)
                        decimal gain = Math.Round(totalTTC * _lastTauxFidelite, 2);
                        string tauxTxt = (_lastTauxFidelite * 100m).ToString("0.##", CultureInfo.GetCultureInfo("fr-FR")) + "%";

                        PdfPTable fid = new PdfPTable(1)
                        {
                            WidthPercentage = 100f,
                            SpacingBefore = 10f,
                            SpacingAfter = 6f
                        };

                        PdfPCell block = new PdfPCell
                        {
                            BorderColor = grayBorder,
                            Padding = 8f,
                            BackgroundColor = new BaseColor(252, 252, 252)
                        };

                        block.AddElement(new Paragraph("FIDELITE CLIENT", fontCellBold));
                        block.AddElement(new Paragraph("Catégorie : FIDELE", fontSmall));
                        block.AddElement(new Paragraph("Taux : " + tauxTxt, fontSmall));
                        block.AddElement(new Paragraph("Gain sur cette vente : " + FormatMontant(gain, devise), fontSmall));
                        block.AddElement(new Paragraph(
     "Solde Cashback : " + _lastSoldeFideliteUSD.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " USD",
     fontSmall));

                        if (!string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                            block.AddElement(new Paragraph("Carte Fidelite : " + _lastCodeCarteClient, fontSmall));

                        block.AddElement(new Paragraph("Vous pouvez utiliser vos points si vous depasser 10USD.", fontSmall));

                        fid.AddCell(block);
                        doc.Add(fid);
                    }
                    catch
                    {
                        // ignore fidélité si erreur
                    }
                }
                // =================================================

                // ===== MESSAGE =====
                doc.Add(new Paragraph("Merci pour votre fidélité, à la prochaine !\nLa Qualité fait la différence.", fontSmall)
                { Alignment = Element.ALIGN_CENTER, SpacingBefore = 12f });

                doc.Add(new Paragraph("Les marchandises vendues ne peuvent être ni reprises, ni échangées.", fontSmall)
                { Alignment = Element.ALIGN_CENTER, SpacingBefore = 6f });

                // ===== BARCODE FACTURE =====
                Barcode128 barcode = new Barcode128 { CodeType = Barcode.CODE128, Code = safeCode };
                var barcodeImage = barcode.CreateImageWithBarcode(writer.DirectContent, null, null);
                barcodeImage.Alignment = Element.ALIGN_CENTER;
                barcodeImage.ScaleToFit(240f, 60f);
                barcodeImage.SpacingBefore = 12f;
                doc.Add(barcodeImage);

                doc.Add(new Paragraph("Code Facture : " + safeCode, fontSmall)
                { Alignment = Element.ALIGN_CENTER });

                // Optionnel: barcode carte fidélité (✅ uniquement si fidèle + code carte non vide)
                if (clientEstFidele && !string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                {
                    Barcode128 card = new Barcode128 { CodeType = Barcode.CODE128, Code = _lastCodeCarteClient };
                    var imgCard = card.CreateImageWithBarcode(writer.DirectContent, null, null);
                    imgCard.Alignment = Element.ALIGN_CENTER;
                    imgCard.ScaleToFit(240f, 55f);
                    imgCard.SpacingBefore = 8f;
                    doc.Add(imgCard);

                    doc.Add(new Paragraph("Carte Fidelite : " + _lastCodeCarteClient, fontSmall)
                    { Alignment = Element.ALIGN_CENTER });
                }

                doc.Close();
            }

            return path;
        }



        // ========================
        // PDF (QuestPDF)
        // ========================


        private void OuvrirPdf(string path)
        {
            if (!File.Exists(path)) return;

            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }

        private sealed class LigneStockRetour
        {
            public int IdProduit;
            public string RefProduit;
            public int Qte;
        }

        // ========================
        // DTO
        // ========================

        private sealed class LigneVenteDTO
        {
            public int ProduitId { get; set; }
            public string Libelle { get; set; }
            public decimal PU { get; set; }
            public decimal Qte { get; set; }
        }


        private bool TryGetSelectedVenteFromRapport(out int idVente)
        {
            idVente = 0;

            DataGridViewRow row = null;

            if (dgvRapport.SelectedRows != null && dgvRapport.SelectedRows.Count > 0)
                row = dgvRapport.SelectedRows[0];
            else if (dgvRapport.CurrentRow != null)
                row = dgvRapport.CurrentRow;

            if (row == null)
            {
                // fallback: dernière vente finalisée
                if (_lastIdVente > 0)
                {
                    idVente = _lastIdVente;
                    return true;
                }

                MessageBox.Show("Sélectionne une vente dans le rapport.");
                return false;
            }

            // ✅ Mets ici le nom EXACT de ta colonne ID dans dgvRapport :
            object vId = row.Cells["ID_Vente"]?.Value;   // <-- si chez toi c’est "IdVente" change ici

            if (vId == null || vId == DBNull.Value || !int.TryParse(vId.ToString(), out idVente) || idVente <= 0)
            {
                // fallback dernière vente
                if (_lastIdVente > 0)
                {
                    idVente = _lastIdVente;
                    return true;
                }

                MessageBox.Show("ID_Vente invalide dans la ligne sélectionnée.");
                return false;
            }

            return true;
        }

        private VentePrintModel ChargerVentePourImpression(int idVente)
        {
            var m = new VentePrintModel();

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                // 1) entête vente (✅ AJOUT CategorieClient)
                using (var cmd = new SqlCommand(@"
SELECT TOP 1
    v.ID_Vente,
    ISNULL(v.CodeFacture,'') AS CodeFacture,
    v.DateVente,
    ISNULL(v.NomCaissier,'') AS NomCaissier,
    ISNULL(v.Devise,'') AS Devise,
    ISNULL(v.MontantTotal,0) AS MontantTotal,
    ISNULL(v.Statut,'VALIDEE') AS Statut,
    ISNULL(c.Nom,'') AS ClientNom,
    ISNULL(NULLIF(LTRIM(RTRIM(c.CategorieClient)),''),'OCCASIONNEL') AS CategorieClient
FROM dbo.Vente v
LEFT JOIN dbo.Clients c ON c.ID_Clients = v.ID_Client
WHERE v.ID_Vente = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            throw new Exception("Vente introuvable.");

                        m.IdVente = Convert.ToInt32(r["ID_Vente"]);
                        m.CodeFacture = r["CodeFacture"].ToString();
                        m.DateVente = Convert.ToDateTime(r["DateVente"]);
                        m.Caissier = r["NomCaissier"].ToString();
                        m.ClientNom = r["ClientNom"].ToString();
                        m.Devise = r["Devise"].ToString();
                        m.MontantTotal = Convert.ToDecimal(r["MontantTotal"]);
                        m.Statut = r["Statut"].ToString();

                        // ✅ nouveau
                        m.CategorieClient = r["CategorieClient"].ToString().Trim().ToUpperInvariant();
                    }
                }

                // 2) lignes
                using (var cmd = new SqlCommand(@"
SELECT
    ID_Produit, NomProduit, RefProduit,
    Quantite,
    ISNULL(PrixUnitaire,0) AS PrixUnitaire,
    ISNULL(Remise,0) AS Remise,
    ISNULL(TVA,0) AS TVA,
    ISNULL(Montant,0) AS Montant
FROM dbo.DetailsVente
WHERE ID_Vente = @id
ORDER BY [ID_Details];", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            m.Lignes.Add(new VenteLignePrintModel
                            {
                                NomProduit = r["NomProduit"].ToString(),
                                Quantite = Convert.ToInt32(r["Quantite"]),
                                PrixUnitaire = Convert.ToDecimal(r["PrixUnitaire"]),
                                Montant = Convert.ToDecimal(r["Montant"])
                            });
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(m.CodeFacture))
                m.CodeFacture = BuildCodeFacture(m.IdVente); // fallback

            return m;
        }

        private string GetPrescripteurConnecte()
        {
            string p = $"{SessionEmploye.Nom} {SessionEmploye.Prenom}".Trim();
            return string.IsNullOrWhiteSpace(p) ? "SYSTEM" : p;
        }

        // ✅ Génère un code facture AVANT finalisation (pour ordonnance dans ClientRapide)
        private void EnsureCodeFactureEnCours_DB()
        {
            if (!string.IsNullOrWhiteSpace(_codeFactureEnCours)) return;

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    int magasinId = 1;
                    _codeFactureEnCours = FactureHelper.BuildCodeFactureSequence(con, trans, magasinId, DateTime.Now);
                    trans.Commit();
                }
            }
        }

        private bool SessionOuvertePourCaissier(int idCaissier, out int idSession)
        {
            idSession = 0;

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 IdSession
FROM dbo.SessionsCaisse
WHERE IdCaissier = @id
  AND Etat = 'OUVERTE'
ORDER BY DateOuverture DESC;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCaissier;

                    object r = cmd.ExecuteScalar();
                    if (r == null || r == DBNull.Value) return false;

                    idSession = Convert.ToInt32(r);
                    return idSession > 0;
                }
            }
        }
        private void cmbDevise_SelectedIndexChanged(object sender, EventArgs e)
        {
            string devise = cmbDevise.Text.Trim();

            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells["Devise"].Value = devise;
            }

            MettreAJourTotaux();
        }

        private void ClearTxtCodeBarreSafe(bool focusScan = false)
        {
            if (txtCodeBarre == null) return;

            _ignoreTxtChange = true;   // évite debounce/suggestions
            txtCodeBarre.Clear();
            _ignoreTxtChange = false;

            CacherSuggestions();

            if (focusScan) FocusScan();
        }

        private void ClearTxtCodeBarreSafe_Deferred(bool focusScan)
        {
            if (IsDisposed) return;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;

                txtCodeBarre.Text = "";

                if (focusScan)
                {
                    txtCodeBarre.Focus();
                    txtCodeBarre.SelectAll();
                }
            }));
        }

        private DataTable BuildPaiementsDataTable(List<FormPaiementsVente.PaiementLine> payLines)
        {
            var dt = new DataTable();
            dt.Columns.Add("Mode", typeof(string));
            dt.Columns.Add("Devise", typeof(string));
            dt.Columns.Add("Montant", typeof(decimal));
            string refTransaction = BuildReferenceTransactionFromPanier(dgvPanier);
            dt.Columns.Add("Reference", typeof(string));

            foreach (var p in payLines)
                dt.Rows.Add(p.ModePaiement, p.Devise, p.Montant, p.Reference);

            return dt;
        }


        private string NormalizeModePaiement(string mode)
        {
            mode = (mode ?? "").Trim().ToUpperInvariant();

            // accents
            mode = mode.Replace("È", "E").Replace("É", "E").Replace("Ê", "E").Replace("Ë", "E");

            // espaces multiples
            while (mode.Contains("  ")) mode = mode.Replace("  ", " ");

            if (mode == "") return "CASH";

            // ✅ Normalisation espèces
            if (mode == "ESPECES" || mode == "ESPECE" || mode == "CASH")
                return "CASH";

            if (mode == "LIQUIDE" || mode == "LIQUIDES")
                return "CASH";

            // ✅ CARTE
            if (mode == "CB" || mode == "CARTE" || mode == "CARD")
                return "CARTE";

            // ✅ MOBILE MONEY (beaucoup d’alias)
            if (mode == "MOBILE MONEY" || mode == "MOBILEMONEY" || mode == "MOMO" || mode == "MM" ||
                mode == "ORANGE MONEY" || mode == "AIRTEL MONEY" || mode == "MPESA")
                return "MOBILE MONEY";

            // ✅ FIDELITE
            if (mode == "FIDELITE" || mode == "FIDELITY")
                return "FIDELITE";

            // ✅ PARTENAIRE (nouveau)
            if (mode == "PARTENAIRE" || mode == "PARTNER" || mode == "PARTENAIRE FIDELITE" || mode == "PARTENAIRE_FIDELITE")
                return "PARTENAIRE";

            // ✅ CREDIT (si tu l'utilises en paiement)
            if (mode == "CREDIT")
                return "CREDIT";

            return mode;
        }


        private static string NormalizeDevise(string d)
        {
            d = (d ?? "").Trim().ToUpperInvariant();
            if (d == "FC") d = "CDF";
            if (string.IsNullOrWhiteSpace(d)) d = "CDF";
            return d;
        }

        private void InsertPaiementVente(
    SqlConnection cn,
    SqlTransaction tx,
    int idVente,
    dynamic p,                 // ou ton type réel: PaiementLine / PaymentLine
    string refTransaction,
    string statut = "VALIDE"
)
        {
            if (cn == null) throw new ArgumentNullException(nameof(cn));
            if (idVente <= 0) throw new Exception("IdVente invalide (InsertPaiementVente).");
            if (p == null) throw new ArgumentNullException(nameof(p));

            string mode = (p.ModePaiement ?? p.Mode ?? "CASH").ToString().Trim().ToUpperInvariant();
            string devise = (p.Devise ?? "CDF").ToString().Trim().ToUpperInvariant();
            if (devise == "FC") devise = "CDF";

            decimal montant = Convert.ToDecimal(p.Montant);
            montant = Math.Round(montant, 2);

            // ✅ Original + taux (si pas fourni => fallback)
            string deviseOrig = null;
            decimal montantOrig = 0m;
            decimal taux = 1m;

            try
            {
                deviseOrig = (p.DeviseOriginale ?? "").ToString().Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(deviseOrig)) deviseOrig = devise;
                if (deviseOrig == "FC") deviseOrig = "CDF";

                montantOrig = Convert.ToDecimal(p.MontantOriginal);
                montantOrig = Math.Round(montantOrig, 2);

                taux = Convert.ToDecimal(p.TauxApplique);
                if (taux <= 0m) taux = 1m;
            }
            catch
            {
                // si ton objet p ne contient pas ces champs
                deviseOrig = devise;
                montantOrig = montant;
                taux = 1m;
            }

            // ✅ CONTEXTE POS : adapte si tu utilises ConfigSysteme au lieu AppContext
            int idEntreprise = AppContext.IdEntreprise;
            int idMagasin = AppContext.IdMagasin;
            int idPoste = AppContext.IdPoste;

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

    DeviseOriginale,
    MontantOriginal,
    TauxApplique,

    IdEntreprise,
    IdMagasin,
    IdPoste
)
VALUES
(
    @v,
    @mode,
    @dev,
    @m,
    GETDATE(),
    @ref,
    @statut,

    @devOrig,
    @mOrig,
    @taux,

    @e,
    @mag,
    @poste
);", cn, tx))
            {
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;

                cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 30).Value = mode;
                cmd.Parameters.Add("@dev", SqlDbType.NVarChar, 10).Value = devise;

                var pm = cmd.Parameters.Add("@m", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;

                // ✅ vraie ref transaction (mobile money / banque / ticket)
                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(refTransaction) ? (object)DBNull.Value : refTransaction.Trim();

                cmd.Parameters.Add("@statut", SqlDbType.NVarChar, 20).Value =
                    string.IsNullOrWhiteSpace(statut) ? "VALIDE" : statut.Trim().ToUpperInvariant();

                cmd.Parameters.Add("@devOrig", SqlDbType.NVarChar, 10).Value =
                    string.IsNullOrWhiteSpace(deviseOrig) ? (object)DBNull.Value : deviseOrig;

                var pmo = cmd.Parameters.Add("@mOrig", SqlDbType.Decimal);
                pmo.Precision = 18; pmo.Scale = 2; pmo.Value = montantOrig;

                var pt = cmd.Parameters.Add("@taux", SqlDbType.Decimal);
                pt.Precision = 18; pt.Scale = 6; pt.Value = taux;

                cmd.Parameters.Add("@e", SqlDbType.Int).Value = idEntreprise;
                cmd.Parameters.Add("@mag", SqlDbType.Int).Value = idMagasin;
                cmd.Parameters.Add("@poste", SqlDbType.Int).Value = idPoste;

                cmd.ExecuteNonQuery();
            }
        }
        private static string BuildCodeCarteClient(int idClient)
        {
            // Simple, stable, scannable
            return "FID-" + idClient.ToString("D6"); // FID-000025
        }

        private void ChargerInfosFideliteClient(SqlConnection con, int idClient)
        {
            ChargerInfosFideliteClient(con, null, idClient);
        }

        private void ChargerInfosFideliteClient(SqlConnection con, SqlTransaction trans, int idClient)
        {
            _lastTauxFidelite = 0.005m;
            _lastGainFidelite = 0m;
            _lastSoldeFideliteCDF = 0m;
            _lastSoldeFideliteUSD = 0m;
            _lastCodeCarteClient = "";

            if (idClient <= 0) return;

            // 1) Code carte
            using (var cmdCard = new SqlCommand(
                @"SELECT TOP 1 ISNULL(CodeCarte,'') FROM dbo.Clients WHERE ID_Clients=@id;", con))
            {
                if (trans != null) cmdCard.Transaction = trans;
                cmdCard.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                _lastCodeCarteClient = (cmdCard.ExecuteScalar() ?? "").ToString().Trim();
            }

            // 2) Ensure LoyaltyCompte
            using (var cmdEnsure = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.LoyaltyCompte WHERE IdClient=@id)
BEGIN
    INSERT INTO dbo.LoyaltyCompte(IdClient, Points, CashbackSolde, Statut, DateMaj)
    VALUES(@id, 0, 0, 'BRONZE', GETDATE());
END", con))
            {
                if (trans != null) cmdEnsure.Transaction = trans;
                cmdEnsure.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                cmdEnsure.ExecuteNonQuery();
            }

            // 3) Lire solde officiel
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ISNULL(Points,0) AS Points, ISNULL(CashbackSolde,0) AS CashbackSolde
FROM dbo.LoyaltyCompte
WHERE IdClient=@id;", con))
            {
                if (trans != null) cmd.Transaction = trans;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idClient;

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        // ✅ Mono-solde cashback
                        _lastSoldeFideliteUSD = Convert.ToDecimal(r["CashbackSolde"]);
                        _lastSoldeFideliteCDF = 0m; // plus utilisé
                    }
                }
            }

            // ✅ 3) Code carte (si tu l'utilises ailleurs)
            try
            {
                using (var cmdCard = new SqlCommand(@"SELECT TOP 1 ISNULL(CodeCarte,'') FROM dbo.Clients WHERE ID_Clients=@id;", con))
                {
                    if (trans != null) cmdCard.Transaction = trans;
                    cmdCard.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                    var o = cmdCard.ExecuteScalar();
                    _lastCodeCarteClient = (o == null || o == DBNull.Value) ? "" : o.ToString().Trim();
                }
            }
            catch
            {
                // ignore
            }
        }
        private void EnsureProduitInCache(ProduitCombo p)
        {
            if (p == null) return;

            for (int i = 0; i < _produitsCacheDgv.Count; i++)
            {
                if (_produitsCacheDgv[i].ID == p.ID)
                    return;
            }
            _produitsCacheDgv.Add(p);
        }
        private void InitialiserComboEmplacement()
        {
            cmbEmplacement.Items.Clear();

            // Rayons 1 à 20
            for (int i = 1; i <= 20; i++)
                cmbEmplacement.Items.Add("Rayon " + i);

            // Tables 1 à 10
            for (int i = 1; i <= 10; i++)
                cmbEmplacement.Items.Add("Table " + i);

            // Coins
            cmbEmplacement.Items.Add("Coin Gauche");
            cmbEmplacement.Items.Add("Coin Droite");

            cmbEmplacement.DropDownStyle = ComboBoxStyle.DropDownList;

            if (cmbEmplacement.Items.Count > 0)
                cmbEmplacement.SelectedIndex = 0;
        }



        private async void txtScanCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Tab) return;

            e.SuppressKeyPress = true;

            string code = (cboScanCode.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) { FocusScan(); return; }

            var now = DateTime.Now;
            if (code == _lastScanValue && (now - _lastScanAt).TotalMilliseconds < 250)
            {
                cboScanCode.Text = "";
                cboScanCode.SelectedIndex = -1;
                FocusScan();
                return;
            }
            _lastScanValue = code;
            _lastScanAt = now;

            _qtyEditStarted = false;
            ScanOutcome outcome = ScanOutcome.None;

            try
            {
                _ignoreTxtChange = true;
                txtCodeBarre.Text = code;
                _ignoreTxtChange = false;

                outcome = await ValiderCodeBarreOuSelectionAsync();
            }
            finally
            {
                // on nettoie le scan combo (toujours)
                cboScanCode.Text = "";
                cboScanCode.SelectedIndex = -1;
            }

            // ✅ gestion focus ICI (hors finally)
            if (outcome == ScanOutcome.NotFoundKeepFocusCodeBarre)
            {
                txtCodeBarre.Focus();
                txtCodeBarre.SelectAll();
                return;
            }

            if (outcome == ScanOutcome.SuggestionsShownKeepFocus)
                return;

            if (outcome == ScanOutcome.AddedAndQtyEditStarted || _qtyEditStarted)
                return;

            FocusScan();
        }

        private bool _updatingCombo = false;
        private CancellationTokenSource _ctsSearch;

        private async void cboScanCode_TextUpdate(object sender, EventArgs e)
        {
            if (_updatingCombo) return;

            string q = (cboScanCode.Text ?? "").Trim();
            if (q.Length < 2) { cboScanCode.DroppedDown = false; return; }

            _ctsSearch?.Cancel();
            _ctsSearch = new CancellationTokenSource();
            var ct = _ctsSearch.Token;

            try
            {
                var list = await RechercherCodesAsync(q, ct);
                if (ct.IsCancellationRequested) return;

                _updatingCombo = true;

                int selStart = cboScanCode.SelectionStart;
                string typed = cboScanCode.Text;

                cboScanCode.BeginUpdate();

                // ✅ IMPORTANT: reset DataSource propre
                cboScanCode.DataSource = null;
                cboScanCode.Items.Clear();

                // ✅ Remplir avec objets (ToString => Text)
                foreach (var it in list)
                    cboScanCode.Items.Add(it);

                cboScanCode.EndUpdate();

                // restore texte
                cboScanCode.Text = typed;
                cboScanCode.SelectionStart = Math.Min(selStart, cboScanCode.Text.Length);
                cboScanCode.SelectionLength = 0;

                cboScanCode.DroppedDown = (cboScanCode.Items.Count > 0);
            }
            catch (OperationCanceledException)
            {
                // normal
            }
            catch
            {
                cboScanCode.DroppedDown = false;
            }
            finally
            {
                _updatingCombo = false;
            }
        }

        private async void cboScanCode_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (!(cboScanCode.SelectedItem is CodeSearchItem chosen)) return;

            string code = (chosen.Code ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) return;

            _ignoreTxtChange = true;
            txtCodeBarre.Text = code;
            _ignoreTxtChange = false;

            await ValiderCodeBarreOuSelectionAsync();

            cboScanCode.Text = "";
            cboScanCode.SelectedIndex = -1;
            FocusScan();
        }

        private void ComboProduit_TextUpdate(object sender, EventArgs e)
        {
            _pendingPrefix = (_comboProduitEditing != null ? _comboProduitEditing.Text : "");
            if (_pendingPrefix == null) _pendingPrefix = "";
            _pendingPrefix = _pendingPrefix.Trim();
            RestartProduitTimer();
        }

        private void RestartProduitTimer()
        {
            if (_timerProduitSearch == null) return;
            _timerProduitSearch.Stop();
            _timerProduitSearch.Start();
        }

        private void TimerProduitSearch_Tick(object sender, EventArgs e)
        {
            _timerProduitSearch.Stop();
            if (_suspendComboEvents) return;
            if (_comboProduitEditing == null) return;

            string prefix = (_pendingPrefix ?? "").Trim();
            string typed = _comboProduitEditing.Text ?? "";
            int caret = _comboProduitEditing.SelectionStart;

            int take = string.IsNullOrWhiteSpace(prefix) ? 50 : 25;
            var results = _produitRepo.SearchProduitsByPrefix(prefix, take);
            _lastSearchResults = results;

            // Inject results into cache (BindingList)
            foreach (var p in results)
                EnsureProduitInCache(p);

            _suspendComboEvents = true;
            try
            {
                _comboProduitEditing.BeginUpdate();

                // ✅ REBIND + restore typed
                RefreshEditingComboPreserveText(_comboProduitEditing, typed, caret);

                // Dropdown
                if (!_comboProduitEditing.DroppedDown)
                    _comboProduitEditing.DroppedDown = (results.Count > 0);
            }
            finally
            {
                _comboProduitEditing.EndUpdate();
                _suspendComboEvents = false;
            }
        }
        private void MettreAJourTotauxSession(SqlConnection con, SqlTransaction trans, int idSession)
        {
            // ✅ vérifier si colonnes EUR existent
            bool hasEur = false;
            using (var chk = new SqlCommand(@"
SELECT CASE WHEN COL_LENGTH('dbo.SessionsCaisse','TotalEspecesEUR') IS NULL THEN 0 ELSE 1 END;", con, trans))
            {
                hasEur = Convert.ToInt32(chk.ExecuteScalar()) == 1;
            }

            string sql = hasEur ? @"
;WITH P AS
(
    SELECT
        UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'')))) AS Devise,
        UPPER(LTRIM(RTRIM(ISNULL(p.ModePaiement,'')))) AS ModePaiement,
        ISNULL(p.Montant,0) AS Montant
    FROM dbo.PaiementsVente p
    INNER JOIN dbo.Vente v ON v.ID_Vente = p.IdVente
    WHERE v.IdSession = @idSession
),
Agg AS
(
    SELECT
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyCDF,

        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyUSD,

        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesEUR,
        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteEUR,
        SUM(CASE WHEN Devise='EUR' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyEUR
    FROM P
)
UPDATE s
SET
    TotalEspecesCDF     = ISNULL(a.TotalEspecesCDF,0),
    TotalCarteCDF       = ISNULL(a.TotalCarteCDF,0),
    TotalMobileMoneyCDF = ISNULL(a.TotalMobileMoneyCDF,0),

    TotalEspecesUSD     = ISNULL(a.TotalEspecesUSD,0),
    TotalCarteUSD       = ISNULL(a.TotalCarteUSD,0),
    TotalMobileMoneyUSD = ISNULL(a.TotalMobileMoneyUSD,0),

    TotalEspecesEUR     = ISNULL(a.TotalEspecesEUR,0),
    TotalCarteEUR       = ISNULL(a.TotalCarteEUR,0),
    TotalMobileMoneyEUR = ISNULL(a.TotalMobileMoneyEUR,0)
FROM dbo.SessionsCaisse s
CROSS JOIN Agg a
WHERE s.IdSession = @idSession;
" : @"
;WITH P AS
(
    SELECT
        UPPER(LTRIM(RTRIM(ISNULL(p.Devise,'')))) AS Devise,
        UPPER(LTRIM(RTRIM(ISNULL(p.ModePaiement,'')))) AS ModePaiement,
        ISNULL(p.Montant,0) AS Montant
    FROM dbo.PaiementsVente p
    INNER JOIN dbo.Vente v ON v.ID_Vente = p.IdVente
    WHERE v.IdSession = @idSession
),
Agg AS
(
    SELECT
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteCDF,
        SUM(CASE WHEN Devise='CDF' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyCDF,

        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('ESPECES','CASH','LIQUIDE') THEN Montant ELSE 0 END) AS TotalEspecesUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('CARTE','CARD') THEN Montant ELSE 0 END) AS TotalCarteUSD,
        SUM(CASE WHEN Devise='USD' AND ModePaiement IN('MOBILE MONEY','MOBILEMONEY','MOMO') THEN Montant ELSE 0 END) AS TotalMobileMoneyUSD
    FROM P
)
UPDATE s
SET
    TotalEspecesCDF     = ISNULL(a.TotalEspecesCDF,0),
    TotalCarteCDF       = ISNULL(a.TotalCarteCDF,0),
    TotalMobileMoneyCDF = ISNULL(a.TotalMobileMoneyCDF,0),

    TotalEspecesUSD     = ISNULL(a.TotalEspecesUSD,0),
    TotalCarteUSD       = ISNULL(a.TotalCarteUSD,0),
    TotalMobileMoneyUSD = ISNULL(a.TotalMobileMoneyUSD,0)
FROM dbo.SessionsCaisse s
CROSS JOIN Agg a
WHERE s.IdSession = @idSession;
";

            using (var cmd = new SqlCommand(sql, con, trans))
            {
                cmd.Parameters.Add("@idSession", SqlDbType.Int).Value = idSession;
                cmd.ExecuteNonQuery();
            }
        }
        private void InitialiserComboBoxClient()
        {
            cmbNomClient.DropDownStyle = ComboBoxStyle.DropDown;

            ChargerClientsComboBox();

            cmbNomClient.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbNomClient.AutoCompleteSource = AutoCompleteSource.CustomSource;

            cmbNomClient.SelectedIndexChanged -= cmbNomClient_SelectedIndexChanged;
            cmbNomClient.SelectedIndexChanged += cmbNomClient_SelectedIndexChanged;

            // ✅ Désactivation définitive
            cmbNomClient.TextChanged -= cmbNomClient_TextChanged;
        }
        private void RafraichirAutoCompleteClients()
        {
            var ac = new AutoCompleteStringCollection();

            foreach (Client c in cmbNomClient.Items)
            {
                // tu peux choisir Nom seul
                ac.Add(c.Nom);

                // ou Nom + Prénom (si tu veux)
                // ac.Add($"{c.Nom} {c.Prenom}".Trim());
            }

            cmbNomClient.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbNomClient.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cmbNomClient.AutoCompleteCustomSource = ac;
        }
        private void ResetApresVente()
        {
            // Panier
            dgvPanier.Rows.Clear();
            MettreAJourTotaux();

            // Client (combo + champs)
            _suspendComboEvents = true;
            try
            {
                cmbNomClient.SelectedIndex = -1;
                cmbNomClient.Text = "";
            }
            finally
            {
                _suspendComboEvents = false;
            }

            txtIDClient.Text = "";
            txtPrenomClient.Text = "";
            txtAdresseClient.Text = "";
            txtTelephone.Text = "";
            txtEmail.Text = "";

            // (Optionnel) remise/TVA et quantité
            txtTVApercent.Text = "0";
            txtRemisePercent.Text = "0";
            numQuantite.Value = 1;

            // (Optionnel) mode paiement / emplacement
            if (cmbModePaiement.Items.Count > 0) cmbModePaiement.SelectedIndex = 0;
            if (cmbEmplacement.Items.Count > 0) cmbEmplacement.SelectedIndex = 0;

            // Recharger la liste clients si tu veux proposer le nouveau client
            ChargerClientsComboBox();

            // Focus sur le client pour prochaine vente
            cmbNomClient.Focus();
        }
        private void EnsureProduitIdInCache(int idProduit)
        {
            if (idProduit <= 0) return;

            for (int i = 0; i < _produitsCacheDgv.Count; i++)
                if (_produitsCacheDgv[i].ID == idProduit)
                    return;

            var p = _produitRepo.GetProduitById(idProduit);
            if (p != null)
                _produitsCacheDgv.Add(p);
        }

        private void RafraichirLangue()
        {
            ConfigSysteme.AppliquerTraductions(this);
        }

        private void RafraichirTheme()
        {
            ConfigSysteme.AppliquerTheme(this);
        }

        private string BuildCodeFacture(int idVente)
        {
            return "FAC" + idVente.ToString("D8"); // FAC00000012
        }
        private void dgvPanier_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;

            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string col = dgvPanier.Columns[e.ColumnIndex].Name;

            // ✅ Si NomProduit plante, injecter l'ID dans le cache puis refresh cellule
            if (col == "NomProduit")
            {
                var v = dgvPanier.Rows[e.RowIndex].Cells["NomProduit"]?.Value;
                if (v != null && int.TryParse(v.ToString(), out int pid) && pid > 0)
                {
                    EnsureProduitIdInCache(pid);
                    ForceRebindNomProduitColumn(); // ✅
                    dgvPanier.InvalidateCell(e.ColumnIndex, e.RowIndex);
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_tmrCodeBarreDebounce != null)
            {
                _tmrCodeBarreDebounce.Stop();
                _tmrCodeBarreDebounce.Tick -= TmrCodeBarreDebounce_Tick;
                _tmrCodeBarreDebounce.Dispose();
                _tmrCodeBarreDebounce = null;
            }

            base.OnFormClosed(e);
        }

        private decimal ParseDecimalCell(DataGridViewRow r, string col)
        {
            var s = r.Cells[col].Value?.ToString();
            if (string.IsNullOrWhiteSpace(s)) return 0m;

            decimal v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out v)) return v;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out v)) return v;

            return 0m;
        }

        private int GetStockDisponible(SqlConnection con, SqlTransaction trans, int idProduit, string emplacement)
        {
            // emplacement ignoré car pas géré dans dbo.Produit (pas de stock par emplacement)
            using (var cmd = new SqlCommand(@"
SELECT ISNULL(StockActuel, 0)
FROM dbo.Produit
WHERE ID_Produit = @id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                var v = cmd.ExecuteScalar();
                return v == null || v == DBNull.Value ? 0 : Convert.ToInt32(v);
            }
        }

        private void VerifierStockAvantVente(SqlConnection con, SqlTransaction trans, string emplacementChoisi)
        {
            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                int idProduit = 0;
                int.TryParse(r.Cells["ID_Produit"].Value?.ToString(), out idProduit);

                int qte = 0;
                int.TryParse(r.Cells["Quantite"].Value?.ToString(), out qte);

                if (idProduit <= 0 || qte <= 0) continue;

                int dispo = GetStockDisponible(con, trans, idProduit, emplacementChoisi);

                if (dispo < qte)
                {
                    string nom = r.Cells["NomProduit"].FormattedValue?.ToString() ?? "";
                    nom = nom.Trim();
                    throw new Exception($"Stock insuffisant pour [{nom}]. Disponible: {dispo} | Demandé: {qte}");
                }
            }
        }
        private void SortieStockAtomique(
    SqlConnection con,
    SqlTransaction trans,
    int idProduit,
    string reference,
    int qte,
    string utilisateur,
    string motif = "VENTE",
    string emplacement = null,
    string remarques = null)
        {
            if (idProduit <= 0) throw new Exception("ID_Produit invalide pour stock.");
            if (string.IsNullOrWhiteSpace(reference)) throw new Exception("Référence invalide pour stock.");
            if (qte <= 0) throw new Exception("Quantité invalide pour stock.");

            utilisateur = (utilisateur ?? "").Trim();
            if (utilisateur.Length == 0) utilisateur = "SYSTEM";

            string emp = string.IsNullOrWhiteSpace(emplacement) ? "" : emplacement.Trim();
            string mot = string.IsNullOrWhiteSpace(motif) ? "VENTE" : motif.Trim();
            string rem = string.IsNullOrWhiteSpace(remarques) ? "" : remarques.Trim();

            // ✅ 1) Décrément atomique Produit.StockActuel (bloque si insuffisant)
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Produit WITH (UPDLOCK, HOLDLOCK)
SET StockActuel = ISNULL(StockActuel,0) - @qte
WHERE ID_Produit = @idProduit
  AND ISNULL(StockActuel,0) >= @qte;

IF (@@ROWCOUNT = 0)
BEGIN
    DECLARE @dispo INT = (SELECT ISNULL(StockActuel,0) FROM dbo.Produit WHERE ID_Produit=@idProduit);
    RAISERROR(N'Stock insuffisant. Disponible: %d | Demandé: %d.', 16, 1, @dispo, @qte);
END
", con, trans))
            {
                cmd.Parameters.Add("@idProduit", SqlDbType.Int).Value = idProduit;
                cmd.Parameters.Add("@qte", SqlDbType.Int).Value = qte;
                cmd.ExecuteNonQuery();
            }

            // ✅ 2) Log mouvement dans OperationsStock
            using (var cmd2 = new SqlCommand(@"
INSERT INTO dbo.OperationsStock
(ID_Produit, TypeOperation, Quantite, DateOperation, Utilisateur, Motif, Reference, Emplacement, Remarques)
VALUES
(@idProduit, 'SORTIE', @qte, GETDATE(), @utilisateur, @motif, @ref, @emplacement, @remarques);
", con, trans))
            {
                cmd2.Parameters.Add("@idProduit", SqlDbType.Int).Value = idProduit;
                cmd2.Parameters.Add("@qte", SqlDbType.Int).Value = qte;
                cmd2.Parameters.Add("@ref", SqlDbType.NVarChar, 50).Value = reference;

                cmd2.Parameters.Add("@utilisateur", SqlDbType.NVarChar, 120).Value = utilisateur;
                cmd2.Parameters.Add("@motif", SqlDbType.NVarChar, 100).Value = mot;

                cmd2.Parameters.Add("@emplacement", SqlDbType.NVarChar, 100).Value = emp;
                cmd2.Parameters.Add("@remarques", SqlDbType.NVarChar, 200).Value = rem;

                cmd2.ExecuteNonQuery();
            }
        }

        private string GetTicketPrinterName_Auto()
        {
            // 1) si mémorisée et installée
            if (!string.IsNullOrWhiteSpace(ConfigSysteme.ImprimanteTicketNom))
            {
                foreach (string p in PrinterSettings.InstalledPrinters)
                {
                    if (string.Equals(p, ConfigSysteme.ImprimanteTicketNom, StringComparison.OrdinalIgnoreCase))
                        return p;
                }
            }

            // 2) heuristique
            string[] hints = { "ticket", "pos", "thermal", "receipt", "80", "58" };
            foreach (string p in PrinterSettings.InstalledPrinters)
            {
                string low = (p ?? "").ToLowerInvariant();
                if (hints.Any(h => low.Contains(h)))
                    return p;
            }

            // 3) fallback imprimante Windows par défaut
            var ps = new PrinterSettings();
            return ps.PrinterName;
        }

        private void ImprimerTicket_AutoSansDialog(int idVente)
        {
            if (idVente <= 0) throw new Exception("ID vente invalide.");

            _printModel = ChargerVentePourImpression(idVente);
            _printLineIndex = 0;

            LoadFideliteForClient(_printModel.IdClient);

            string printer = GetTicketPrinterName_FromConfigOrDefault();
            if (string.IsNullOrWhiteSpace(printer))
                throw new Exception("Aucune imprimante ticket disponible.");

            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printer;

            pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 315, 2500);
            pd.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

            pd.PrintPage += PrintPageHandlerTicket_FromModel;

            pd.Print();
        }
        private void PrintPageHandlerTicket(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int left = e.MarginBounds.Left;
            int right = e.MarginBounds.Right;
            int y = e.MarginBounds.Top;

            var fTitle = new DFont("Arial", 10, FontStyle.Bold);
            var fTxt = new DFont("Arial", 8, FontStyle.Regular);
            var fBold = new DFont("Arial", 8, FontStyle.Bold);

            string devise = string.IsNullOrWhiteSpace(cmbDevise.Text) ? "CDF" : cmbDevise.Text;

            string codeFacture = (!string.IsNullOrWhiteSpace(_lastCodeFacture))
                ? _lastCodeFacture
                : "FAC" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // ✅ Catégorie client (pour afficher fidélité seulement si fidèle)
            bool clientEstFidele = false;

            try
            {
                int idVente = _lastIdVente; // ✅ supposé existant chez toi

                if (idVente > 0)
                {
                    using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                    {
                        con.Open();
                        using (var cmd = new SqlCommand(@"
SELECT ISNULL(NULLIF(LTRIM(RTRIM(c.CategorieClient)),''),'OCCASIONNEL') AS Cat
FROM dbo.Vente v
JOIN dbo.Clients c ON c.ID_Clients = v.ID_Client
WHERE v.ID_Vente=@id;", con))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                            var o = cmd.ExecuteScalar();
                            string cat = (o == null || o == DBNull.Value) ? "OCCASIONNEL" : o.ToString().Trim().ToUpperInvariant();
                            clientEstFidele = (cat == "FIDELE");
                        }
                    }
                }
            }
            catch { }

            // Helper center
            void Center(string text, DFont f)
            {
                SizeF s = g.MeasureString(text, f);
                float x = left + (e.MarginBounds.Width - s.Width) / 2f;
                g.DrawString(text, f, Brushes.Black, x, y);
                y += (int)s.Height + 2;
            }

            Center("ZAIRE MODE SARL", fTitle);
            Center("23, Bld Lumumba / Immeuble Masina Plaza", fTxt);
            Center("+243861507560  |  ZAIRE.CD", fTxt);
            Center("RCCM: 25-B-01497", fTxt);
            Center("IDNAT: 01-F4300-N73258E", fTxt);

            y += 6;
            g.DrawLine(Pens.Black, left, y, right, y); y += 6;

            g.DrawString("Facture: " + codeFacture, fBold, Brushes.Black, left, y); y += 14;
            g.DrawString("Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fTxt, Brushes.Black, left, y); y += 14;
            g.DrawString("Caissier: " + txtNomCaissier.Text, fTxt, Brushes.Black, left, y); y += 14;
            g.DrawString("Client: " + txtPrenomClient.Text + " " + cmbNomClient.Text, fTxt, Brushes.Black, left, y); y += 16;

            g.DrawLine(Pens.Black, left, y, right, y); y += 6;

            g.DrawString("Article", fBold, Brushes.Black, left, y);
            g.DrawString("Qt", fBold, Brushes.Black, right - 90, y);
            g.DrawString("Total", fBold, Brushes.Black, right - 45, y);
            y += 14;

            g.DrawLine(Pens.Black, left, y, right, y); y += 6;

            decimal totalTTC = 0m;

            foreach (DataGridViewRow row in dgvPanier.Rows)
            {
                if (row.IsNewRow) continue;

                string nom = row.Cells["NomProduit"].FormattedValue?.ToString() ?? "";
                int.TryParse(row.Cells["Quantite"].Value?.ToString(), out int qte);
                decimal.TryParse(row.Cells["Montant"].Value?.ToString(), out decimal totalLigne);

                totalTTC += totalLigne;

                // Article (wrap)
                RectangleF rNom = new RectangleF(left, y, e.MarginBounds.Width - 2, 40);
                g.DrawString(nom, fTxt, Brushes.Black, rNom);
                y += 14;

                g.DrawString(qte.ToString(), fTxt, Brushes.Black, right - 90, y);
                g.DrawString(totalLigne.ToString("N2"), fTxt, Brushes.Black, right - 45, y);
                y += 16;
            }

            y += 4;
            g.DrawLine(Pens.Black, left, y, right, y); y += 8;

            g.DrawString("TOTAL TTC:", fBold, Brushes.Black, left, y);
            g.DrawString(totalTTC.ToString("N2") + " " + devise, fBold, Brushes.Black, right - 120, y);
            y += 18;

            g.DrawString("Mode: " + cmbModePaiement.Text, fTxt, Brushes.Black, left, y); y += 16;

            // ✅ Bloc fidélité sur ticket (UNIQUEMENT si FIDELE)
            if (clientEstFidele)
            {
                y += 6;
                g.DrawLine(Pens.Black, left, y, right, y); y += 6;

                g.DrawString("FIDELITE CLIENT", fBold, Brushes.Black, left, y); y += 14;

                try
                {
                    int idVente = _lastIdVente;
                    if (idVente > 0)
                    {
                        int idClient = 0;
                        using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                        {
                            con.Open();

                            using (var cmd = new SqlCommand("SELECT ID_Client FROM dbo.Vente WHERE ID_Vente=@v", con))
                            {
                                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                                var o = cmd.ExecuteScalar();
                                idClient = (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
                            }

                            if (idClient > 0)
                            {
                                ChargerInfosFideliteClient(con, null, idClient);

                                string tauxTxt = (_lastTauxFidelite * 100m).ToString("0.##", CultureInfo.GetCultureInfo("fr-FR")) + "%";
                                decimal gain = Math.Round(totalTTC * _lastTauxFidelite, 2);

                                g.DrawString("Taux: " + tauxTxt, fTxt, Brushes.Black, left, y); y += 14;
                                g.DrawString("Gain vente: " + gain.ToString("N2") + " " + devise, fTxt, Brushes.Black, left, y); y += 14;
                                g.DrawString("Solde Cashback: " + _lastSoldeFideliteUSD.ToString("N2") + " USD", fTxt, Brushes.Black, left, y); y += 14;

                                if (!string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                                {
                                    g.DrawString("Carte: " + _lastCodeCarteClient, fTxt, Brushes.Black, left, y); y += 14;
                                }

                                g.DrawString("Utilisable si > 10 USD", fTxt, Brushes.Black, left, y); y += 14;
                            }
                        }
                    }
                }
                catch { }
            }

            Center("Merci pour votre fidélité !", fTxt);
            Center("La Qualité fait la différence.", fTxt);
            Center("Ni repris, ni échangé.", fTxt);

            y += 8;

            // Barcode
            try
            {
                var bw = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions { Width = e.MarginBounds.Width - 10, Height = 40 }
                };
                using (Bitmap bmp = bw.Write(codeFacture))
                {
                    int x = left + (e.MarginBounds.Width - bmp.Width) / 2; // ✅ centré
                    int yBarcode = y + 8;                                   // ✅ descend un peu
                    g.DrawImage(bmp, x, yBarcode);
                    y = yBarcode + bmp.Height + 6;
                }
                Center(codeFacture, fTxt);
            }
            catch { }

            e.HasMorePages = false;
        }


        private void ForceRebindNomProduitColumn()
        {
            if (!(dgvPanier.Columns["NomProduit"] is DataGridViewComboBoxColumn col)) return;

            var src = col.DataSource;
            col.DataSource = null;
            col.DisplayMember = "NomProduit";
            col.ValueMember = "ID";
            col.DataSource = _produitsCacheDgv;
        }
        private void Combo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_suspendComboEvents) return;
            if (_timerProduitSearch != null) _timerProduitSearch.Stop();

            var combo = sender as ComboBox;
            if (combo == null) return;
            if (dgvPanier.CurrentCell == null) return;

            var produit = combo.SelectedItem as ProduitCombo;
            if (produit == null) return;

            EnsureProduitInCache(produit);

            var row = dgvPanier.Rows[dgvPanier.CurrentCell.RowIndex];

            row.Cells["NomProduit"].Value = produit.ID;
            row.Cells["ID_Produit"].Value = produit.ID;
            row.Cells["RefProduit"].Value = produit.Ref;
            row.Cells["PrixUnitaire"].Value = produit.Prix;
            row.Cells["Categorie"].Value = produit.Categorie;
            row.Cells["Taille"].Value = produit.Taille;
            row.Cells["Couleur"].Value = produit.Couleur;

            // ✅ Devise (ComboBox colonne)
            if (HasCol(row, "Devise"))
                SetDeviseOnRow(row, produit.Devise);

            MettreAJourTotaux();
        }

        // ===================== PANIER =====================
        private void InitialiserDgvPanier()
{
    dgvPanier.Columns.Clear();
    dgvPanier.AutoGenerateColumns = false;

            dgvPanier.DataError -= dgvPanier_DataError;
            dgvPanier.DataError += dgvPanier_DataError;

            // ID PRODUIT (caché)
            dgvPanier.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "ID_Produit",
        HeaderText = "ID Produit",
        Visible = false
    });

    // RÉFÉRENCE
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "RefProduit",
        HeaderText = "Référence"
    });
            dgvPanier.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CodeBarre",
                HeaderText = "CodeBarre",
                Visible = false // mets true si tu veux voir
            });

            // ✅ Charge seulement un petit cache initial (ex: 200)
            _produitsCacheDgv = new BindingList<ProduitCombo>(_produitRepo.SearchProduitsByPrefix("", 200));

            var colDesignation = new DataGridViewComboBoxColumn
            {
                Name = "NomProduit",
                HeaderText = "Désignation",
                DataSource = _produitsCacheDgv,       // ✅ cache léger
                DisplayMember = "NomProduit",
                ValueMember = "ID",
                FlatStyle = FlatStyle.Flat
            };
            dgvPanier.Columns.Add(colDesignation);

            // AUTRES COLONNES
            dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantite", HeaderText = "Qté" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "PrixUnitaire", HeaderText = "PU" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remise", HeaderText = "Remise %" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "TVA", HeaderText = "TVA %" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categorie", HeaderText = "Catégorie" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "Taille", HeaderText = "Taille" });
    dgvPanier.Columns.Add(new DataGridViewTextBoxColumn { Name = "Couleur", HeaderText = "Couleur" });
            var colDevise = new DataGridViewComboBoxColumn
            {
                Name = "Devise",
                HeaderText = "Devise",
                FlatStyle = FlatStyle.Flat,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            colDevise.Items.AddRange("CDF", "USD", "EUR");
            dgvPanier.Columns.Add(colDevise);

            dgvPanier.Columns.Add(new DataGridViewTextBoxColumn
    {
        Name = "Montant",
        HeaderText = "Montant TTC",
        ReadOnly = true
    });

            dgvPanier.Columns["Quantite"].ValueType = typeof(decimal);
            dgvPanier.Columns["PrixUnitaire"].ValueType = typeof(decimal);
            dgvPanier.Columns["Remise"].ValueType = typeof(decimal);
            dgvPanier.Columns["TVA"].ValueType = typeof(decimal);
            dgvPanier.Columns["Montant"].ValueType = typeof(decimal);

            dgvPanier.Columns["Quantite"].DefaultCellStyle.Format = "N2";
            dgvPanier.Columns["PrixUnitaire"].DefaultCellStyle.Format = "N2";
            dgvPanier.Columns["Remise"].DefaultCellStyle.Format = "N2";
            dgvPanier.Columns["TVA"].DefaultCellStyle.Format = "N2";
            dgvPanier.Columns["Montant"].DefaultCellStyle.Format = "N2";

            colDesignation.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            colDesignation.DisplayStyleForCurrentCellOnly = true;
            colDesignation.AutoComplete = false; // (si dispo selon version) sinon ignore

            EnsureDeviseColumn_DgvPanier();

            // Événements (prévention doublons)
            dgvPanier.CellEndEdit -= DgvPanier_CellEndEdit;
    dgvPanier.CellEndEdit += DgvPanier_CellEndEdit;

    dgvPanier.EditingControlShowing -= dgvPanier_EditingControlShowing;
    dgvPanier.EditingControlShowing += dgvPanier_EditingControlShowing;

    dgvPanier.CurrentCellDirtyStateChanged -= DgvPanier_CurrentCellDirtyStateChanged;
    dgvPanier.CurrentCellDirtyStateChanged += DgvPanier_CurrentCellDirtyStateChanged;

            dgvPanier.ContextMenuStrip = ConfigSysteme.MenuContextuel;
}

        private void EnsureDeviseColumn_DgvPanier()
        {
            if (!dgvPanier.Columns.Contains("Devise"))
                return; // ou throw si tu veux forcer la présence

            if (dgvPanier.Columns["Devise"] is DataGridViewComboBoxColumn colDevise)
            {
                // ✅ look & feel
                colDevise.FlatStyle = FlatStyle.Flat;
                colDevise.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

                // ✅ valeurs autorisées (base)
                string[] baseItems = { "CDF", "USD", "EUR" };
                foreach (var dv in baseItems)
                {
                    if (!colDevise.Items.Contains(dv))
                        colDevise.Items.Add(dv);
                }

                // optionnel : empêcher l’utilisateur de changer
                // colDevise.ReadOnly = true;
                // colDevise.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            }
            else
            {
                // Si Devise n'est pas ComboBox, rien à faire
            }
        }

        private void DebugColonnesPanier()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Colonnes dgvPanier :");

            foreach (DataGridViewColumn c in dgvPanier.Columns)
                sb.AppendLine($"Name='{c.Name}'  Header='{c.HeaderText}'  Index={c.Index}");

            MessageBox.Show(sb.ToString());
        }

        private List<ProduitCombo> ChargerProduitsPourDgv()
        {
            List<ProduitCombo> liste = new List<ProduitCombo>();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT ID_Produit, NomProduit, RefProduit, Prix, Categorie, Taille, Couleur
            FROM Produit", con);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        liste.Add(new ProduitCombo
                        {
                            ID = Convert.ToInt32(dr["ID_Produit"]),
                            NomProduit = dr["NomProduit"].ToString(),
                            Ref = dr["RefProduit"].ToString(),
                            Prix = Convert.ToDecimal(dr["Prix"]),
                            Categorie = dr["Categorie"].ToString(),
                            Taille = dr["Taille"].ToString(),
                            Couleur = dr["Couleur"].ToString()
                        });
                    }
                }
            }

            return liste;
        }
        private void dgvPanier_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvPanier.CurrentCell == null) return;

            string colName = dgvPanier.Columns[dgvPanier.CurrentCell.ColumnIndex].Name;

            // =========================
            // 0) cboScanCode
            // =========================
            if (colName == "cboScanCode" && e.Control is ComboBox comboScan)
            {
                _comboScanEditing = comboScan;

                // éviter doublons
                comboScan.SelectionChangeCommitted -= ComboScan_SelectionChangeCommitted;
                comboScan.KeyDown -= ComboScan_KeyDown;

                comboScan.SelectionChangeCommitted += ComboScan_SelectionChangeCommitted;
                comboScan.KeyDown += ComboScan_KeyDown;

                return; // ✅ important: on ne veut pas tomber sur les autres blocs
            }

            // =========================
            // 1) Quantite
            // =========================
            if (colName == "Quantite" && e.Control is TextBox tb)
            {
                tb.KeyDown -= TbQuantite_KeyDown;
                tb.KeyDown += TbQuantite_KeyDown;
                return;
            }

            // =========================
            // 2) NomProduit
            // =========================
            if (colName == "NomProduit" && e.Control is ComboBox comboProd)
            {
                _comboProduitEditing = comboProd;

                comboProd.DropDownStyle = ComboBoxStyle.DropDown;
                comboProd.AutoCompleteMode = AutoCompleteMode.None;
                comboProd.AutoCompleteSource = AutoCompleteSource.None;

                if (!ReferenceEquals(comboProd.DataSource, _produitsCacheDgv))
                {
                    comboProd.DisplayMember = "NomProduit";
                    comboProd.ValueMember = "ID";
                    comboProd.DataSource = _produitsCacheDgv;
                }

                if (!(comboProd.Tag is string t) || t != "HOOKED")
                {
                    comboProd.TextUpdate -= ComboProduit_TextUpdate;
                    comboProd.SelectionChangeCommitted -= Combo_SelectionChangeCommitted;
                    comboProd.KeyDown -= ComboProduit_KeyDown;

                    comboProd.TextUpdate += ComboProduit_TextUpdate;
                    comboProd.SelectionChangeCommitted += Combo_SelectionChangeCommitted;
                    comboProd.KeyDown += ComboProduit_KeyDown;

                    comboProd.Tag = "HOOKED";
                }

                var cellVal = dgvPanier.CurrentCell.Value;
                if (cellVal != null && int.TryParse(cellVal.ToString(), out int pid) && pid > 0)
                    EnsureProduitIdInCache(pid);

                return;
            }
        }

        private void ComboScan_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // ✅ commit propre de la sélection
            dgvPanier.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dgvPanier.EndEdit();

            _forceFocusQuantite = true;

            FocusQuantiteDgv(dgvPanier.CurrentCell?.RowIndex ?? -1);
        }

        private void ComboScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                dgvPanier.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dgvPanier.EndEdit();

                FocusQuantiteDgv(dgvPanier.CurrentCell?.RowIndex ?? -1);
            }
        }

        private void TbQuantite_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            // ✅ commit propre
            try
            {
                dgvPanier.EndEdit();
                dgvPanier.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch { }

            // ✅ ouvrir client rapide si demandé
            if (_openClientAfterQty)
            {
                _openClientAfterQty = false;

                BeginInvoke(new Action(() =>
                {
                    OuvrirClientRapideEtFinaliser();

                    // après fermeture du popup => on revient au scan
                    FocusScan();
                }));
            }
            else
            {
                BeginInvoke(new Action(FocusScan));
            }
        }

        private void InitialiserDgvRapport()
        {
            dgvRapport.Columns.Clear();
            dgvRapport.AutoGenerateColumns = false;

            // Barres de défilement
            dgvRapport.ScrollBars = ScrollBars.Both;

            // Comportement général
            dgvRapport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRapport.MultiSelect = false;
            dgvRapport.ReadOnly = true;
            dgvRapport.AllowUserToAddRows = false;
            dgvRapport.AllowUserToDeleteRows = false;

            // ❌ NE PAS utiliser Fill (ça écrase les colonnes)
            dgvRapport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Alignement des en-têtes
            dgvRapport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRapport.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 📅 Date de vente
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DateVente",
                HeaderText = "Date de vente",
                DataPropertyName = "DateVente",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy" }
            });

            // 🧾 ID Vente
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ID_Vente",
                HeaderText = "N° Vente",
                DataPropertyName = "ID_Vente",
                Width = 90
            });

            // 👤 Client
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ID_Client",
                HeaderText = "Client",
                DataPropertyName = "ID_Client",
                Width = 90
            });

            // 👨‍💼 Caissier
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NomCaissier",
                HeaderText = "Caissier",
                DataPropertyName = "NomCaissier",
                Width = 140
            });

            // 👷 Employé
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IDEmploye",
                HeaderText = "Employé",
                DataPropertyName = "IDEmploye",
                Width = 100
            });

            // 💳 Mode paiement
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ModePaiement",
                HeaderText = "Mode de paiement",
                DataPropertyName = "ModePaiement",
                Width = 140
            });

            // 💰 Montant
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MontantTotal",
                HeaderText = "Montant total",
                DataPropertyName = "MontantTotal",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            // 💱 Devise
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Devise",
                HeaderText = "Devise",
                DataPropertyName = "Devise",
                Width = 70
            });

            // 💱 CodeFacture
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CodeFacture",
                HeaderText = "Facture",
                DataPropertyName = "CodeFacture",
                Width = 140
            });

            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Statut",
                HeaderText = "Statut",
                DataPropertyName = "Statut",
                Width = 90
            });

            // Remise %
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RemiseTicketPct",
                HeaderText = "Remise %",
                DataPropertyName = "RemiseTicketPct",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            // Remise montant
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RemiseTicketMontant",
                HeaderText = "Remise Montant",
                DataPropertyName = "RemiseTicketMontant",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
            });

            // AnnulePar
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AnnulePar",
                HeaderText = "Annulé par",
                DataPropertyName = "AnnulePar",
                Width = 110
            });

            // DateAnnulation
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DateAnnulation",
                HeaderText = "Date annulation",
                DataPropertyName = "DateAnnulation",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
            });

            // MotifAnnulation
            dgvRapport.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MotifAnnulation",
                HeaderText = "Motif",
                DataPropertyName = "MotifAnnulation",
                Width = 180
            });
        }
        
        private void ComboProduit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            var combo = sender as ComboBox;
            if (combo == null) return;
            if (dgvPanier.CurrentCell == null) return;

            ProduitCombo p = combo.SelectedItem as ProduitCombo;

            if (p == null)
            {
                string typed = (combo.Text ?? "").Trim();
                if (_lastSearchResults != null && _lastSearchResults.Count > 0)
                {
                    p = _lastSearchResults
                            .FirstOrDefault(x => (x.NomProduit ?? "").StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                        ?? _lastSearchResults[0];
                }
            }

            if (p == null) return;

            var row = dgvPanier.Rows[dgvPanier.CurrentCell.RowIndex];

            // ✅ injecter dans cache AVANT Value
            EnsureProduitIdInCache(p.ID);

            row.Cells["NomProduit"].Value = p.ID;
            row.Cells["ID_Produit"].Value = p.ID;
            row.Cells["RefProduit"].Value = p.Ref;

            // ✅ DECIMAL (PAS string)
            row.Cells["PrixUnitaire"].Value = p.Prix;

            row.Cells["Categorie"].Value = p.Categorie;
            row.Cells["Taille"].Value = p.Taille;
            row.Cells["Couleur"].Value = p.Couleur;

            dgvPanier.EndEdit();
            dgvPanier.CurrentCell = row.Cells["Quantite"];
            dgvPanier.BeginEdit(true);

            MettreAJourTotaux();
        }

        private void InitKeyboardWorkflow()
        {
            dgvPanier.EditingControlShowing -= dgvPanier_EditingControlShowing;
            dgvPanier.EditingControlShowing += dgvPanier_EditingControlShowing;

            dgvPanier.KeyDown -= dgvPanier_KeyDown;
            dgvPanier.KeyDown += dgvPanier_KeyDown;
        }



        private int AddOrIncrementProduitToPanierById_ReturnRowIndex(int produitId, decimal qty, decimal prix, string devise, string refProduit, string codeBarre = "")
        {
            if (produitId <= 0) return -1;
            if (qty <= 0) qty = 1;

            string dev = NormalizeDevise(devise);

            // ComboBox NomProduit => ID doit exister dans cache
            EnsureProduitIdInCache(produitId);

            // 1) si déjà présent => incrémenter
            for (int i = 0; i < dgvPanier.Rows.Count; i++)
            {
                var row = dgvPanier.Rows[i];
                if (row == null || row.IsNewRow) continue;

                int idRow = GetIntCell(row, "ID_Produit");
                if (idRow != produitId) continue;

                decimal oldQ = GetDecimalCell(row, "Quantite");
                decimal newQ = oldQ + qty;

                if (HasCol(row, "Quantite")) row.Cells["Quantite"].Value = newQ;
                if (HasCol(row, "PrixUnitaire")) row.Cells["PrixUnitaire"].Value = prix;
                if (HasCol(row, "RefProduit")) row.Cells["RefProduit"].Value = refProduit;
                if (HasCol(row, "CodeBarre")) row.Cells["CodeBarre"].Value = codeBarre ?? "";

                if (HasCol(row, "Devise")) SetDeviseOnRow(row, dev);

                RecalcMontantRow(row);
                return i;
            }

            // 2) sinon ajouter ligne
            int idx = dgvPanier.Rows.Add();
            var r = dgvPanier.Rows[idx];

            if (HasCol(r, "ID_Produit")) r.Cells["ID_Produit"].Value = produitId;
            if (HasCol(r, "NomProduit")) r.Cells["NomProduit"].Value = produitId; // VALUE = ID
            if (HasCol(r, "RefProduit")) r.Cells["RefProduit"].Value = refProduit;
            if (HasCol(r, "CodeBarre")) r.Cells["CodeBarre"].Value = codeBarre ?? "";

            if (HasCol(r, "PrixUnitaire")) r.Cells["PrixUnitaire"].Value = prix;
            if (HasCol(r, "Quantite")) r.Cells["Quantite"].Value = qty;

            // Valeurs par défaut si vides
            if (HasCol(r, "Remise") && (r.Cells["Remise"].Value == null || r.Cells["Remise"].Value == DBNull.Value))
                r.Cells["Remise"].Value = 0m;

            if (HasCol(r, "TVA") && (r.Cells["TVA"].Value == null || r.Cells["TVA"].Value == DBNull.Value))
                r.Cells["TVA"].Value = 0m;

            if (HasCol(r, "Devise")) SetDeviseOnRow(r, dev);

            RecalcMontantRow(r);
            return idx;
        }

        private async Task<List<CodeSearchItem>> RechercherCodesAsync(string q, CancellationToken token)
{
    var res = new List<CodeSearchItem>();
    if (string.IsNullOrWhiteSpace(q)) return res;

    q = q.Trim();

    // ✅ AJOUT : activer suffixe seulement si 2 ou 3 caractères
    bool shortKey = (q.Length == 2 || q.Length == 3);

    using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
    using (var cmd = new SqlCommand(@"
SELECT TOP 20
    ID_Produit,
    LTRIM(RTRIM(ISNULL(CodeBarre,''))) AS CodeBarre,
    ISNULL(NomProduit,'') AS NomProduit,
    ISNULL(RefProduit,'') AS RefProduit
FROM dbo.Produit
WHERE
    LTRIM(RTRIM(ISNULL(CodeBarre,''))) LIKE @pPrefix
    OR LTRIM(RTRIM(ISNULL(RefProduit,''))) LIKE @pPrefix

    -- ✅ AJOUT : suffixe (2 ou 3 derniers chiffres) seulement si shortKey
    OR (@shortKey = 1 AND LTRIM(RTRIM(ISNULL(CodeBarre,''))) LIKE @pSuffix)
    OR (@shortKey = 1 AND LTRIM(RTRIM(ISNULL(RefProduit,''))) LIKE @pSuffix)

    OR ISNULL(NomProduit,'') LIKE @pLike
ORDER BY
    CASE WHEN LTRIM(RTRIM(ISNULL(CodeBarre,''))) LIKE @pPrefix THEN 0 ELSE 1 END,
    CASE WHEN LTRIM(RTRIM(ISNULL(RefProduit,''))) LIKE @pPrefix THEN 0 ELSE 1 END,
    NomProduit;", con))
    {
        cmd.Parameters.Add("@pPrefix", SqlDbType.NVarChar, 100).Value = q + "%";
        cmd.Parameters.Add("@pLike", SqlDbType.NVarChar, 120).Value = "%" + q + "%";

        // ✅ AJOUT : paramètres suffixe + shortKey
        cmd.Parameters.Add("@pSuffix", SqlDbType.NVarChar, 100).Value = "%" + q;
        cmd.Parameters.Add("@shortKey", SqlDbType.Bit).Value = shortKey ? 1 : 0;

        await con.OpenAsync(token);

        using (var rd = await cmd.ExecuteReaderAsync(token))
        {
            while (await rd.ReadAsync(token))
            {
                token.ThrowIfCancellationRequested();

                int id = Convert.ToInt32(rd["ID_Produit"]);
                string cb = (rd["CodeBarre"] ?? "").ToString().Trim();
                string nom = (rd["NomProduit"] ?? "").ToString().Trim();
                string rf = (rd["RefProduit"] ?? "").ToString().Trim();

                // Si CodeBarre vide, on fallback RefProduit
                string code = !string.IsNullOrWhiteSpace(cb) ? cb : rf;

                if (string.IsNullOrWhiteSpace(code))
                    continue;

                string label = $"{code}  |  {nom}{(string.IsNullOrWhiteSpace(rf) ? "" : $" ({rf})")}";

                res.Add(new CodeSearchItem
                {
                    ID = id,
                    Code = code,
                    Text = label.Trim()
                });
            }
        }
    }

    return res;
}



        private int GetIntCell(DataGridViewRow row, string colName)
        {
            if (row?.DataGridView?.Columns.Contains(colName) != true) return 0;
            var v = row.Cells[colName]?.Value;
            if (v == null || v == DBNull.Value) return 0;
            return int.TryParse(Convert.ToString(v), out int n) ? n : 0;
        }

        private string GetTextCell(DataGridViewRow row, string colName)
        {
            if (row?.DataGridView?.Columns.Contains(colName) != true) return "";

            // ComboBoxColumn => texte visible ici
            string fv = Convert.ToString(row.Cells[colName]?.FormattedValue ?? "")?.Trim();
            if (!string.IsNullOrWhiteSpace(fv)) return fv;

            string v = Convert.ToString(row.Cells[colName]?.Value ?? "")?.Trim();
            return v ?? "";
        }

        private decimal GetDecCell(DataGridViewRow row, string colName)
        {
            if (row?.DataGridView?.Columns.Contains(colName) != true) return 0m;

            var v = row.Cells[colName]?.Value;
            if (v == null || v == DBNull.Value) return 0m;

            string s = Convert.ToString(v)?.Trim() ?? "0";
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var d)) return d;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;

            s = s.Replace(".", ",");
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out d)) return d;

            return 0m;
        }

        private void SelectRowErrorPanier(int rowIndex, string message)
        {
            try
            {
                if (rowIndex >= 0 && rowIndex < dgvPanier.Rows.Count)
                {
                    dgvPanier.ClearSelection();
                    dgvPanier.Rows[rowIndex].Selected = true;
                    dgvPanier.CurrentCell = dgvPanier.Rows[rowIndex].Cells
                        .Cast<DataGridViewCell>()
                        .FirstOrDefault(c => c.Visible) ?? dgvPanier.Rows[rowIndex].Cells[0];
                }
            }
            catch { }

            MessageBox.Show(message, "Ligne panier invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void EnsureDeviseInColumn(string devise)
        {
            devise = NormalizeDevise(devise);

            if (!(dgvPanier.Columns["Devise"] is DataGridViewComboBoxColumn col))
                return;

            // si Items vide -> on met au moins CDF
            if (col.Items.Count == 0)
                col.Items.Add("CDF");

            // ✅ ajoute la devise si absente (sinon DataError)
            bool exists = false;
            foreach (var it in col.Items)
            {
                if (string.Equals(it?.ToString(), devise, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
                col.Items.Add(devise);
        }

        private void SetDeviseOnRow(DataGridViewRow row, string dev)
        {
            dev = NormalizeDevise(dev);

            if (!dgvPanier.Columns.Contains("Devise"))
            {
                if (HasCol(row, "Devise")) row.Cells["Devise"].Value = dev;
                return;
            }

            // si colonne devise est ComboBox, s'assurer que l'item existe
            if (dgvPanier.Columns["Devise"] is DataGridViewComboBoxColumn col)
            {
                if (!col.Items.Contains(dev))
                    col.Items.Add(dev);

                row.Cells["Devise"].Value = dev;
            }
            else
            {
                row.Cells["Devise"].Value = dev;
            }
        }

        private void FocusQuantiteDgv(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvPanier.Rows.Count) return;
            if (dgvPanier.Rows[rowIndex].IsNewRow) return;

            // ⚠️ Important: après EndEdit/Commit, utiliser BeginInvoke
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;

                string colQte = dgvPanier.Columns.Contains("QuantiteDgvPanier")
                    ? "QuantiteDgvPanier"
                    : "Quantite";

                if (!dgvPanier.Columns.Contains(colQte)) return;

                dgvPanier.ClearSelection();
                dgvPanier.CurrentCell = dgvPanier.Rows[rowIndex].Cells[colQte];
                dgvPanier.BeginEdit(true);

                // Optionnel: sélectionne tout le texte si c’est un TextBox
                if (dgvPanier.EditingControl is TextBox tb)
                    tb.SelectAll();
            }));
        }

        private void RecalcMontantRow(DataGridViewRow row)
        {
            if (row == null) return;
            if (!HasCol(row, "Montant")) return;

            decimal qty = GetDecimalCell(row, "Quantite");
            decimal pu = GetDecimalCell(row, "PrixUnitaire");
            decimal remPct = HasCol(row, "Remise") ? GetDecimalCell(row, "Remise") : 0m;
            decimal tvaPct = HasCol(row, "TVA") ? GetDecimalCell(row, "TVA") : 0m;

            decimal ht = qty * pu;
            decimal rem = ht * remPct / 100m;
            decimal baseTva = ht - rem;
            decimal tva = baseTva * tvaPct / 100m;

            row.Cells["Montant"].Value = Math.Round(baseTva + tva, 2);
        }


        private decimal GetDecimalCell(DataGridViewRow r, string colName)
        {
            if (r?.Cells[colName] == null) return 0m;

            object v = r.Cells[colName].Value;
            string s = "";

            if (v != null && v != DBNull.Value) s = v.ToString();
            if (string.IsNullOrWhiteSpace(s))
                s = r.Cells[colName].FormattedValue?.ToString() ?? "";

            s = (s ?? "").Trim();

            // enlever devise + espaces
            s = s.Replace("CDF", "").Replace("FC", "").Replace("USD", "").Replace("EUR", "");
            s = s.Replace(" ", "").Replace("\u00A0", ""); // espace insécable

            // sécurité
            decimal x;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out x))
                return x;

            // fallback culture invariant
            if (decimal.TryParse(s.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                return x;

            return 0m;
        }

        public enum TypePeriodeRapport
        {
            Jour,
            Hebdomadaire,
            Mensuel,
            Annuel,
            ParCaissier
        }

        // Méthode pour récupérer les totaux
        private (decimal totalCDF, decimal totalUSD, decimal totalEUR) ObtenirTotauxVentes(
    TypePeriodeRapport periode,
    string nomCaissier = null)
        {
            decimal totalCDF = 0m, totalUSD = 0m, totalEUR = 0m;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                var sb = new StringBuilder();
                sb.AppendLine("SELECT");
                sb.AppendLine("  SUM(CASE WHEN Devise = 'CDF' THEN MontantTotal ELSE 0 END) AS TotalCDF,");
                sb.AppendLine("  SUM(CASE WHEN Devise = 'USD' THEN MontantTotal ELSE 0 END) AS TotalUSD,");
                sb.AppendLine("  SUM(CASE WHEN Devise = 'EUR' THEN MontantTotal ELSE 0 END) AS TotalEUR");
                sb.AppendLine("FROM [dbo].[Vente]");
                sb.AppendLine("WHERE 1=1");

                List<SqlParameter> parameters = new List<SqlParameter>();

                switch (periode)
                {
                    case TypePeriodeRapport.Jour:
                        sb.AppendLine("AND CAST(DateVente AS DATE) = CAST(GETDATE() AS DATE)");
                        break;
                    case TypePeriodeRapport.Hebdomadaire:
                        sb.AppendLine("AND DATEPART(WEEK, DateVente) = DATEPART(WEEK, GETDATE())");
                        sb.AppendLine("AND DATEPART(YEAR, DateVente) = DATEPART(YEAR, GETDATE())");
                        break;
                    case TypePeriodeRapport.Mensuel:
                        sb.AppendLine("AND DATEPART(MONTH, DateVente) = DATEPART(MONTH, GETDATE())");
                        sb.AppendLine("AND DATEPART(YEAR, DateVente) = DATEPART(YEAR, GETDATE())");
                        break;
                    case TypePeriodeRapport.Annuel:
                        sb.AppendLine("AND DATEPART(YEAR, DateVente) = DATEPART(YEAR, GETDATE())");
                        break;
                }

                if (!string.IsNullOrEmpty(nomCaissier))
                {
                    sb.AppendLine("AND NomCaissier = @nomCaissier");
                    parameters.Add(new SqlParameter("@nomCaissier", nomCaissier));
                }

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), con))
                {
                    if (parameters.Count > 0)
                        cmd.Parameters.AddRange(parameters.ToArray());

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalCDF = reader["TotalCDF"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCDF"]) : 0m;
                            totalUSD = reader["TotalUSD"] != DBNull.Value ? Convert.ToDecimal(reader["TotalUSD"]) : 0m;
                            totalEUR = reader["TotalEUR"] != DBNull.Value ? Convert.ToDecimal(reader["TotalEUR"]) : 0m;
                        }
                    }
                }
            }

            return (totalCDF, totalUSD, totalEUR);
        }


        private void CalculerTotauxVente()
        {
            decimal totalHT = 0m;

            foreach (DataGridViewRow row in dgvPanier.Rows)
            {
                if (row.IsNewRow) continue;

                // ✅ lis avec la même logique que partout (fr-FR / invariant fallback)
                totalHT += ParseDecimalCell(row, "Montant");
            }

            decimal tvaPercent = 0m;
            decimal.TryParse((txtTVApercent.Text ?? "").Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out tvaPercent);

            decimal remisePercent = 0m;
            decimal.TryParse((txtRemisePercent.Text ?? "").Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out remisePercent);

            decimal totalTVA = totalHT * tvaPercent / 100m;
            decimal totalRemise = totalHT * remisePercent / 100m;
            decimal totalTTC = totalHT + totalTVA - totalRemise;

            txtTotalHT.Text = totalHT.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
            txtTotalTVA.Text = totalTVA.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
            txtTotalRemise.Text = totalRemise.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
            txtTotalTTC.Text = totalTTC.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
            txtTotal.Text = totalTTC.ToString("N2", CultureInfo.GetCultureInfo("fr-FR"));
        }
        private void txtTVApercent_TextChanged(object sender, EventArgs e)
        {
            CalculerTotauxVente();
        }
        private void txtRemisepercent_TextChanged(object sender, EventArgs e)
        {
            CalculerTotauxVente();
        }
        // Charger la liste des produits dans cmbNomProduit


        private decimal CalculerMontant(decimal qte, decimal pu, decimal remPercent, decimal tvaPercent)
        {
            decimal ht = qte * pu;
            decimal remise = ht * remPercent / 100m;
            decimal baseTva = ht - remise;
            decimal tvaMontant = baseTva * tvaPercent / 100m;
            decimal total = baseTva + tvaMontant;
            return Math.Round(total, 2);
        }

        private void ExporterDataTableEnPdf(DataTable dt, string cheminFichier, string titre)
        {
            using (var fs = new FileStream(cheminFichier, FileMode.Create))
            {
                using (var doc = new Document(PageSize.A4, 30f, 30f, 40f, 30f))
                {
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    var fontTitre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                    var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10f);
                    var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9f);

                    Paragraph p = new Paragraph(titre, fontTitre) { Alignment = Element.ALIGN_CENTER };
                    doc.Add(p);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(dt.Columns.Count)
                    {
                        WidthPercentage = 100f
                    };

                    // Répéter les en-têtes sur chaque nouvelle page
                    table.HeaderRows = 1;

                    float[] widths = Enumerable.Repeat(1f, dt.Columns.Count).ToArray();
                    table.SetWidths(widths);

                    // En-têtes
                    foreach (DataColumn col in dt.Columns)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, fontHeader))
                        {
                            BackgroundColor = BaseColor.LIGHT_GRAY,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 5
                        };
                        table.AddCell(cell);
                    }

                    // Données
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (var item in row.ItemArray)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(item?.ToString() ?? "", fontCell))
                            {
                                Padding = 5
                            };
                            table.AddCell(cell);
                        }
                    }

                    doc.Add(table);
                    doc.Close();
                }
            }
        }
        public class Client
        {
            public int ID { get; set; }
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Adresse { get; set; }
            public string Telephone { get; set; }
            public string Email { get; set; }

            public override string ToString()
            {
                return Nom; // Ce qui s'affiche dans le ComboBox
            }
        }
        private void ChargerClientsComboBox()
        {
            cmbNomClient.Items.Clear();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                string requete = @"
            SELECT ID_Clients, Nom, Prenom, Adresse, Telephone, Email 
            FROM Clients
            ORDER BY Nom";

                using (SqlCommand cmd = new SqlCommand(requete, con))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var client = new Client
                        {
                            ID = Convert.ToInt32(dr["ID_Clients"]),
                            Nom = dr["Nom"].ToString(),
                            Prenom = dr["Prenom"].ToString(),
                            Adresse = dr["Adresse"].ToString(),
                            Telephone = dr["Telephone"].ToString(),
                            Email = dr["Email"].ToString()
                        };
                        cmbNomClient.Items.Add(client);
                    }
                }
            }

            cmbNomClient.DisplayMember = "Nom";
            cmbNomClient.ValueMember = "ID";

            // ✅ Important : mettre à jour l’auto-complétion après chargement
            RafraichirAutoCompleteClients();
        }
        private void cmbNomClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbNomClient.SelectedItem is Client client)
            {
                txtIDClient.Text = client.ID.ToString();
                txtPrenomClient.Text = client.Prenom;
                txtAdresseClient.Text = client.Adresse;
                txtTelephone.Text = client.Telephone;
                txtEmail.Text = client.Email;
            }
            else
            {
                ClearClientFields();
            }
            
        }
        private void ClearClientFields()
        {
            txtIDClient.Text = "";
            txtPrenomClient.Text = "";
            txtAdresseClient.Text = "";
            txtTelephone.Text = "";
            txtEmail.Text = "";
        }
        private void cmbNomClient_TextChanged(object sender, EventArgs e)
        {
            
        }
        private async void cmbNomClient_Leave(object sender, EventArgs e)
        {
            if (_suspendComboEvents) return;

            string nomSaisi = (cmbNomClient.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nomSaisi))
                return;

            // 1) Si déjà sélectionné et correspond au texte -> ok
            if (cmbNomClient.SelectedItem is Client sel &&
                string.Equals(sel.Nom?.Trim(), nomSaisi, StringComparison.OrdinalIgnoreCase))
                return;

            // 2) Chercher dans la liste déjà chargée (Items)
            Client inCombo = cmbNomClient.Items
                .Cast<object>()
                .OfType<Client>()
                .FirstOrDefault(c => string.Equals((c.Nom ?? "").Trim(), nomSaisi, StringComparison.OrdinalIgnoreCase));

            if (inCombo != null)
            {
                _suspendComboEvents = true;
                try { cmbNomClient.SelectedItem = inCombo; }
                finally { _suspendComboEvents = false; }
                return;
            }

            // 3) Chercher en DB (évite doublon si la combo n’est pas à jour)
            try
            {
                using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    await con.OpenAsync();

                    // ✅ Normalisation simple (évite " Paul " vs "Paul")
                    string nomDb = nomSaisi;

                    // 3a) Existe déjà ?
                    using (var cmdFind = new SqlCommand(@"
SELECT TOP 1 ID_Clients, Nom, Prenom, Adresse, Telephone, Email
FROM dbo.Clients
WHERE LTRIM(RTRIM(Nom)) = LTRIM(RTRIM(@nom));", con))
                    {
                        cmdFind.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomDb;

                        using (var rd = await cmdFind.ExecuteReaderAsync())
                        {
                            if (await rd.ReadAsync())
                            {
                                var clientDb = new Client
                                {
                                    ID = Convert.ToInt32(rd["ID_Clients"]),
                                    Nom = rd["Nom"]?.ToString() ?? "",
                                    Prenom = rd["Prenom"]?.ToString() ?? "",
                                    Adresse = rd["Adresse"]?.ToString() ?? "",
                                    Telephone = rd["Telephone"]?.ToString() ?? "",
                                    Email = rd["Email"]?.ToString() ?? ""
                                };

                                cmbNomClient.Items.Add(clientDb);

                                _suspendComboEvents = true;
                                try { cmbNomClient.SelectedItem = clientDb; }
                                finally { _suspendComboEvents = false; }

                                RafraichirAutoCompleteClients();
                                return;
                            }
                        }
                    }

                    // 3b) Sinon INSERT (client minimal)
                    using (var cmdIns = new SqlCommand(@"
INSERT INTO dbo.Clients (Nom, Prenom, Adresse, Telephone, Email)
OUTPUT INSERTED.ID_Clients
VALUES (@nom, N'', N'', N'', N'');", con))
                    {
                        cmdIns.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomDb;

                        int newId = Convert.ToInt32(await cmdIns.ExecuteScalarAsync());

                        var nouveauClient = new Client
                        {
                            ID = newId,
                            Nom = nomDb,
                            Prenom = "",
                            Adresse = "",
                            Telephone = "",
                            Email = ""
                        };

                        cmbNomClient.Items.Add(nouveauClient);

                        _suspendComboEvents = true;
                        try { cmbNomClient.SelectedItem = nouveauClient; }
                        finally { _suspendComboEvents = false; }

                        RafraichirAutoCompleteClients();

                        MessageBox.Show("Nouveau client ajouté : " + nomDb,
                            "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l’ajout du client : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitialiserComboTypeRapport()
        {
            cmbTypeRapport.Items.Clear();
            cmbTypeRapport.Items.Add("Hebdomadaire");
            cmbTypeRapport.Items.Add("Mensuel");
            cmbTypeRapport.Items.Add("Annuel");
            cmbTypeRapport.Items.Add("Par Caissier");
            cmbTypeRapport.SelectedIndex = 0; // Sélection par défaut
        }
        private void DgvPanier_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var colName = dgvPanier.Columns[e.ColumnIndex].Name;
            DataGridViewRow row = dgvPanier.Rows[e.RowIndex];

            // ✅ Après validation du scan => focus Quantité
            if (colName == "cboScanCode")
                FocusQuantiteDgv(e.RowIndex);

            // ✅ Anti DataError : si NomProduit contient un ID, l'injecter dans le cache
            if (row.Cells["NomProduit"].Value != null)
            {
                if (int.TryParse(row.Cells["NomProduit"].Value.ToString(), out int pid))
                    EnsureProduitIdInCache(pid);
            }

            decimal qte = GetDecimalCell(row, "Quantite");
            decimal pu = GetDecimalCell(row, "PrixUnitaire");
            decimal remise = GetDecimalCell(row, "Remise");
            decimal tva = GetDecimalCell(row, "TVA");

            decimal ht = qte * pu;
            decimal montantRemise = ht * remise / 100m;
            decimal baseTva = ht - montantRemise;
            decimal montantTva = baseTva * tva / 100m;
            decimal montant = baseTva + montantTva;

            row.Cells["Montant"].Value = Math.Round(montant, 2);
            MettreAJourTotaux();
        }

        private bool HasCol(DataGridViewRow row, string colName)
        {
            return row?.DataGridView?.Columns?.Contains(colName) == true;
        }
        private void ChargerProduits()
        {
            cmbNomProduit.Items.Clear();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(@"
SELECT ID_Produit, NomProduit, RefProduit, Prix, Devise, Categorie, Taille, Couleur
FROM dbo.Produit
WHERE ISNULL(IsActif,1)=1
ORDER BY NomProduit;", con))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        cmbNomProduit.Items.Add(new ProduitCombo
                        {
                            ID = Convert.ToInt32(dr["ID_Produit"]),
                            NomProduit = dr["NomProduit"]?.ToString() ?? "",
                            Ref = dr["RefProduit"]?.ToString() ?? "",
                            Prix = dr["Prix"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["Prix"]),
                            Devise = dr["Devise"]?.ToString() ?? "CDF",
                            Categorie = dr["Categorie"]?.ToString() ?? "",
                            Taille = dr["Taille"]?.ToString() ?? "",
                            Couleur = dr["Couleur"]?.ToString() ?? ""
                        });
                    }
                }
            }
        }

        // Classe modifiée pour plus de propriétés
        public class ProduitCombo
        {
            public int ID { get; set; }
            public string NomProduit { get; set; }
            public string Ref { get; set; }
            public decimal Prix { get; set; }
            public string Devise { get; set; }      // ✅ AJOUT

            public string Categorie { get; set; }
            public string Taille { get; set; }
            public string Couleur { get; set; }

            public override string ToString() => NomProduit;
        }
        private void DgvPanier_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (!dgvPanier.IsCurrentCellDirty) return;
            if (dgvPanier.CurrentCell == null) return;

            if (_suspendComboEvents) return;

            // ✅ Ne pas commit sur la colonne combo "NomProduit" (saisie)
            if (dgvPanier.CurrentCell.OwningColumn?.Name == "NomProduit") return;

            dgvPanier.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        public void MettreAJourTotaux()
        {
            decimal totalHT = 0m;
            decimal totalRemise = 0m;
            decimal totalTVA = 0m;
            decimal totalTTC = 0m;

            foreach (DataGridViewRow row in dgvPanier.Rows)
            {
                if (row == null || row.IsNewRow) continue;

                decimal qte = GetDecimalCell(row, "Quantite");
                decimal pu = GetDecimalCell(row, "PrixUnitaire");
                decimal remisePct = GetDecimalCell(row, "Remise");
                decimal tvaPct = GetDecimalCell(row, "TVA");

                if (qte <= 0) qte = 1;

                decimal ht = qte * pu;
                decimal rem = ht * remisePct / 100m;
                decimal baseTva = ht - rem;
                decimal tva = baseTva * tvaPct / 100m;
                decimal ttc = baseTva + tva;

                // ✅ écrire le montant TTC ligne
                if (HasCol(row, "Montant"))
                    row.Cells["Montant"].Value = Math.Round(ttc, 2);

                totalHT += ht;
                totalRemise += rem;
                totalTVA += tva;
                totalTTC += ttc;
            }

            // ✅ update UI si tes champs existent
            var fr = CultureInfo.GetCultureInfo("fr-FR");

            if (txtTotalHT != null) txtTotalHT.Text = totalHT.ToString("N2", fr);
            if (txtTotalRemise != null) txtTotalRemise.Text = totalRemise.ToString("N2", fr);
            if (txtTotalTVA != null) txtTotalTVA.Text = totalTVA.ToString("N2", fr);

            if (txtTotalTTC != null) txtTotalTTC.Text = totalTTC.ToString("N2", fr);
            if (txtTotal != null) txtTotal.Text = totalTTC.ToString("N2", fr);
        }

        private string ObtenirReferenceProduit(string idProduit)
        {
            if (dictRefProduits.TryGetValue(idProduit, out string reference))
                return reference;
            return "";
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            // 1) Produit sélectionné
            var p = cmbNomProduit.SelectedItem as ProduitCombo;
            if (p == null || p.ID <= 0)
            {
                MessageBox.Show("Sélection produit invalide.");
                return;
            }

            // 2) Quantité
            decimal quantite = numQuantite.Value;
            if (quantite <= 0)
            {
                MessageBox.Show("Veuillez saisir une quantité valide (>0).");
                return;
            }

            // 3) Prix unitaire (textbox si valide, sinon prix produit)
            decimal prixUnitaire;
            if (!decimal.TryParse((txtPrixUnitaire.Text ?? "").Trim(), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out prixUnitaire))
                prixUnitaire = p.Prix;

            if (prixUnitaire <= 0)
            {
                MessageBox.Show("Prix unitaire invalide.");
                return;
            }

            // 4) Remise / TVA
            decimal remise = 0m, tva = 0m;
            decimal.TryParse((txtRemisePercent.Text ?? "0").Trim(), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out remise);
            decimal.TryParse((txtTVApercent.Text ?? "0").Trim(), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out tva);

            // ✅ éviter DataError Combo NomProduit
            EnsureProduitIdInCache(p.ID);

            // 5) Si déjà dans panier -> incrémenter
            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                int id = 0;
                int.TryParse(r.Cells["ID_Produit"].Value?.ToString(), out id);

                if (id == p.ID)
                {
                    _updatingRow = true;
                    try
                    {
                        decimal oldQ = GetDecimalCell(r, "Quantite");
                        r.Cells["Quantite"].Value = oldQ + quantite;

                        // MAJ champs (si besoin)
                        r.Cells["PrixUnitaire"].Value = prixUnitaire;
                        r.Cells["Remise"].Value = remise;
                        r.Cells["TVA"].Value = tva;

                        if (dgvPanier.Columns.Contains("RefProduit")) r.Cells["RefProduit"].Value = p.Ref;
                        if (dgvPanier.Columns.Contains("Categorie")) r.Cells["Categorie"].Value = p.Categorie;
                        if (dgvPanier.Columns.Contains("Taille")) r.Cells["Taille"].Value = p.Taille;
                        if (dgvPanier.Columns.Contains("Couleur")) r.Cells["Couleur"].Value = p.Couleur;

                        SetDeviseOnRow(r, p.Devise);

                        RecalcMontantRow(r);
                        MettreAJourTotaux();
                    }
                    finally { _updatingRow = false; }

                    ResetUiAjout();
                    return;
                }
            }

            // 6) Sinon nouvelle ligne
            int idx = dgvPanier.Rows.Add();
            var row = dgvPanier.Rows[idx];

            _updatingRow = true;
            try
            {
                row.Cells["NomProduit"].Value = p.ID;   // ✅ ValueMember = ID
                row.Cells["ID_Produit"].Value = p.ID;
                row.Cells["RefProduit"].Value = p.Ref;

                row.Cells["Quantite"].Value = quantite;
                row.Cells["PrixUnitaire"].Value = prixUnitaire;
                row.Cells["Remise"].Value = remise;
                row.Cells["TVA"].Value = tva;

                if (dgvPanier.Columns.Contains("Categorie")) row.Cells["Categorie"].Value = p.Categorie;
                if (dgvPanier.Columns.Contains("Taille")) row.Cells["Taille"].Value = p.Taille;
                if (dgvPanier.Columns.Contains("Couleur")) row.Cells["Couleur"].Value = p.Couleur;

                SetDeviseOnRow(row, p.Devise);

                RecalcMontantRow(row);
                MettreAJourTotaux();
            }
            finally { _updatingRow = false; }

            ResetUiAjout();
        }

        private void ResetUiAjout()
        {
            numQuantite.Value = 1;
            txtRemisePercent.Text = "0";
            txtTVApercent.Text = "0";
        }


        private void btnSupprimerArticle_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dgvPanier.CurrentRow;
            if (row == null || row.IsNewRow)
                row = dgvPanier.SelectedRows.Count > 0 ? dgvPanier.SelectedRows[0] : null;

            if (row == null || row.IsNewRow)
            {
                MessageBox.Show("Veuillez sélectionner une ligne valide à supprimer.");
                return;
            }

            var designation = dgvPanier.Columns.Contains("NomProduit")
                ? (row.Cells["NomProduit"].FormattedValue?.ToString() ?? "")
                : "";

            if (MessageBox.Show(
                $"Supprimer cet article ?\n{designation}",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            dgvPanier.Rows.RemoveAt(row.Index);
            MettreAJourTotaux();
        }

        private void btnFinaliser_Click(object sender, EventArgs e)
        {
            // ===================== PRE-CHECKS =====================
            if (dgvPanier.Rows.Count == 0)
            {
                MessageBox.Show("Panier vide");
                return;
            }

            if (!ConfigSysteme.SessionOuverte)
            {
                MessageBox.Show("Aucune session caisse ouverte. Ouvre d'abord une session avant de vendre.");
                return;
            }

            if (AppContext.IdEntreprise <= 0 || AppContext.IdMagasin <= 0 || AppContext.IdPoste <= 0)
            {
                MessageBox.Show("Contexte POS invalide. Recharge le contexte (Entreprise/Magasin/Poste).");
                return;
            }

            int idSessionCourante = ConfigSysteme.SessionCaisseId;
            int idEmploye = SessionEmploye.ID_Employe;
            if (idEmploye <= 0)
            {
                MessageBox.Show("Employé non connecté.");
                return;
            }

            if (!decimal.TryParse(CleanText(txtTotalTTC.Text), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out decimal totalBrut))
            {
                MessageBox.Show("Le total TTC est invalide");
                return;
            }

            // ✅ client obligatoire (nom + tel)
            string nomClientUI = CleanText(cmbNomClient.Text);
            string telClientUI = CleanText(txtTelephone.Text);

            if (string.IsNullOrWhiteSpace(nomClientUI))
            {
                MessageBox.Show("Le nom du client est obligatoire");
                return;
            }
            if (string.IsNullOrWhiteSpace(telClientUI))
            {
                MessageBox.Show("Le téléphone du client est obligatoire");
                return;
            }

            string emplacementChoisi = cmbEmplacement.SelectedItem != null
                ? CleanText(cmbEmplacement.SelectedItem.ToString())
                : null;

            // ================== ✅ RÈGLE DE DEVISE (UNE SEULE PAR VENTE) ==================
            string deviseVente = null;

            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                string deviseLigne = CleanText(r.Cells["Devise"].Value?.ToString() ?? "");
                if (string.IsNullOrWhiteSpace(deviseLigne)) continue;

                if (deviseVente == null) deviseVente = deviseLigne;
                else if (!deviseLigne.Equals(deviseVente, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Une vente ne peut contenir qu'une seule devise. Corrige les lignes du panier.");
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(deviseVente)) deviseVente = "CDF";
            deviseVente = CleanText(deviseVente).ToUpperInvariant();
            if (deviseVente == "FC") deviseVente = "CDF";

            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                string deviseLigne = CleanText(r.Cells["Devise"].Value?.ToString() ?? "");
                if (string.IsNullOrWhiteSpace(deviseLigne))
                    r.Cells["Devise"].Value = deviseVente;
                else if (!deviseLigne.Equals(deviseVente, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Une vente ne peut contenir qu'une seule devise. Corrige les lignes du panier.");
                    return;
                }
            }
            // ============================================================================

            // ================== ✅ REMISE TICKET (SUR LE TOTAL) ==================
            decimal remiseTicketPct = 0m;
            decimal remiseTicketMontant = 0m;

            if (decimal.TryParse(CleanText(txtRemiseTicketPct.Text), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var pct))
                remiseTicketPct = Math.Max(0m, pct);

            if (decimal.TryParse(CleanText(txtRemiseTicketMontant.Text), NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var rm))
                remiseTicketMontant = Math.Max(0m, rm);

            if (remiseTicketPct > 0m && remiseTicketMontant <= 0m)
                remiseTicketMontant = Math.Round(totalBrut * (remiseTicketPct / 100m), 2);

            if (remiseTicketMontant > 0m && remiseTicketPct <= 0m && totalBrut > 0m)
                remiseTicketPct = Math.Round((remiseTicketMontant / totalBrut) * 100m, 2);

            if (remiseTicketMontant > totalBrut)
            {
                MessageBox.Show("La remise ticket ne peut pas dépasser le total.");
                return;
            }

            decimal montantNetAPayer = Math.Round(totalBrut - remiseTicketMontant, 2);
            if (montantNetAPayer < 0m) montantNetAPayer = 0m;
            // ============================================================================

            // ================== ✅ COUPON + CREDIT ==================
            string couponCode = CleanText(txtCouponCode.Text);
            bool isCredit = chkVenteCredit.Checked;
            DateTime? echeance = isCredit ? (DateTime?)dtpEcheanceCredit.Value.Date : null;

            decimal remiseCoupon = 0m;       // ✅ remise totale (client+partenaire) qui baisse le net
            int idPartenaireCoupon = 0;      // ✅ partenaire final (priorité vente sinon coupon)
                                             // ============================================================================

            decimal netFinal = montantNetAPayer;

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction();

                int idVente = 0;
                string codeFactureGeneree = "";

                // ✅ NOM CAISSIER (une seule fois)
                string nomCaissier = (SessionEmploye.Prenom + " " + SessionEmploye.Nom).Trim();
                if (string.IsNullOrWhiteSpace(nomCaissier)) nomCaissier = "SYSTEM";

                _lastTauxFidelite = 0.005m;
                _lastGainFidelite = 0m;
                _lastSoldeFideliteCDF = 0m;
                _lastSoldeFideliteUSD = 0m;
                _lastCodeCarteClient = "";

                try
                {
                    // ================= CLIENT (ROBUSTE) =================
                    int idClient = 0;

                    string nomClient = CleanText(cmbNomClient.Text);
                    string telClient = CleanText(txtTelephone.Text);

                    using (SqlCommand cmdCheckClient = new SqlCommand(@"
SELECT TOP 1 ID_Clients, ISNULL(CodeCarte,'') AS CodeCarte
FROM dbo.Clients
WHERE LTRIM(RTRIM(Nom)) = LTRIM(RTRIM(@nom))
  AND LTRIM(RTRIM(ISNULL(Telephone,''))) = LTRIM(RTRIM(ISNULL(@tel,'')))
ORDER BY ID_Clients DESC;", con, trans))
                    {
                        cmdCheckClient.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomClient;
                        cmdCheckClient.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telClient;

                        using (var rd = cmdCheckClient.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                idClient = Convert.ToInt32(rd["ID_Clients"]);
                                _lastCodeCarteClient = rd["CodeCarte"]?.ToString() ?? "";
                                txtIDClient.Text = idClient.ToString();
                            }
                        }
                    }

                    if (idClient <= 0)
                    {
                        using (SqlCommand cmdInsertClient = new SqlCommand(@"
INSERT INTO dbo.Clients (Nom, Prenom, Adresse, Telephone, Email, CodeCarte)
OUTPUT INSERTED.ID_Clients
VALUES (@nom, @prenom, @adresse, @tel, @email, NULL);", con, trans))
                        {
                            cmdInsertClient.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = nomClient;
                            cmdInsertClient.Parameters.Add("@prenom", SqlDbType.NVarChar, 120).Value = CleanText(txtPrenomClient.Text);
                            cmdInsertClient.Parameters.Add("@adresse", SqlDbType.NVarChar, 200).Value = CleanText(txtAdresseClient.Text);
                            cmdInsertClient.Parameters.Add("@tel", SqlDbType.NVarChar, 50).Value = telClient;

                            string email = CleanText(txtEmail.Text);
                            cmdInsertClient.Parameters.Add("@email", SqlDbType.NVarChar, 120).Value =
                                string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;

                            idClient = Convert.ToInt32(cmdInsertClient.ExecuteScalar());
                            txtIDClient.Text = idClient.ToString();

                            _lastCodeCarteClient = ""; // on forcera la création dessous
                        }
                    }

                    string categorieClient = "OCCASIONNEL";

                    using (var cmdCat = new SqlCommand(@"
SELECT ISNULL(NULLIF(LTRIM(RTRIM(CategorieClient)),''),'OCCASIONNEL')
FROM dbo.Clients
WHERE ID_Clients=@id;", con, trans))
                    {
                        cmdCat.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                        var oCat = cmdCat.ExecuteScalar();
                        if (oCat != null && oCat != DBNull.Value)
                            categorieClient = oCat.ToString().Trim().ToUpperInvariant();
                    }

                    bool clientEstFideleAuMomentDeLaVente =
                        categorieClient.Equals("FIDELE", StringComparison.OrdinalIgnoreCase);

                    if (string.IsNullOrWhiteSpace(_lastCodeCarteClient))
                    {
                        _lastCodeCarteClient = BuildCodeCarteClient(idClient);

                        using (var cmdUp = new SqlCommand(@"
UPDATE dbo.Clients SET CodeCarte=@c
WHERE ID_Clients=@id AND (CodeCarte IS NULL OR LTRIM(RTRIM(CodeCarte))='');", con, trans))
                        {
                            cmdUp.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = _lastCodeCarteClient;
                            cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = idClient;
                            cmdUp.ExecuteNonQuery();
                        }
                    }

                    _compteSvc.EnsureClientModules(idClient, con, trans);
                    // ======================================================

                    // ================== ✅ COUPON (PREVIEW VIA CouponService) ==================
                    CouponService.CouponSplitResult couponPrev = null;

                    if (!string.IsNullOrWhiteSpace(couponCode))
                    {
                        // si tu as un partenaire "de la vente", mets-le ici, sinon 0
                        int idPartenaireVente = 0;

                        couponPrev = _couponSvc.PreviewCouponSplitTx(
                            code: couponCode,
                            montantBase: montantNetAPayer,
                            idClient: idClient,
                            idPartenaire: idPartenaireVente,
                            deviseVente: deviseVente,
                            con: con,
                            trans: trans
                        );

                        if (couponPrev == null || !couponPrev.IsValid)
                        {
                            MessageBox.Show(couponPrev?.Message ?? "Coupon invalide.");
                            trans.Rollback();
                            return;
                        }

                        // ✅ remise totale (client+partenaire) => baisse le net
                        remiseCoupon = Math.Round(couponPrev.PromoTotale, 2);
                        netFinal = Math.Round(montantNetAPayer - remiseCoupon, 2);
                        if (netFinal < 0m) netFinal = 0m;

                        idPartenaireCoupon = couponPrev.IdPartenaire ?? 0;
                    }
                    else
                    {
                        remiseCoupon = 0m;
                        netFinal = montantNetAPayer;
                    }
                    // ============================================================================

                    if (!string.IsNullOrWhiteSpace(_codeFactureEnCours))
                        codeFactureGeneree = _codeFactureEnCours;
                    else
                    {
                        int magasinId = 1;
                        codeFactureGeneree = FactureHelper.BuildCodeFactureSequence(con, trans, magasinId, DateTime.Now);
                    }

                    // ================== ✅ SIGNATURE MANAGER (AUTORISATIONS PAR PRODUIT) ==================
                    {
                        var mapDetails = new Dictionary<string, StringBuilder>(StringComparer.OrdinalIgnoreCase);

                        foreach (DataGridViewRow rr in dgvPanier.Rows)
                        {
                            if (rr.IsNewRow) continue;

                            int.TryParse(rr.Cells["ID_Produit"].Value?.ToString(), out int idProd);
                            if (idProd <= 0) continue;

                            int q = 0; int.TryParse(rr.Cells["Quantite"].Value?.ToString(), out q);
                            if (q <= 0) continue;

                            using (var cmdP = new SqlCommand(@"
SELECT TOP 1
    ISNULL(IsReglemente,0) AS IsReg,
    ISNULL(SignatureManagerRequired,0) AS SigReq,
    ISNULL(PermissionCode,'') AS PermissionCode,
    ISNULL(NomProduit,'') AS NomProduit,
    ISNULL(RefProduit,'') AS RefProduit
FROM dbo.Produit
WHERE ID_Produit=@id;", con, trans))
                            {
                                cmdP.Parameters.Add("@id", SqlDbType.Int).Value = idProd;

                                using (var rdP = cmdP.ExecuteReader())
                                {
                                    if (!rdP.Read()) continue;

                                    bool isReg = Convert.ToInt32(rdP["IsReg"]) == 1;
                                    bool sigReq = Convert.ToInt32(rdP["SigReq"]) == 1;

                                    if (!(isReg && sigReq)) continue;

                                    string perm = (rdP["PermissionCode"]?.ToString() ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(perm))
                                        perm = "VENTE_PRODUIT_REGLEMENTE";

                                    if (!mapDetails.TryGetValue(perm, out var sb))
                                    {
                                        sb = new StringBuilder();
                                        mapDetails[perm] = sb;
                                    }

                                    string np = rdP["NomProduit"].ToString();
                                    string rp = rdP["RefProduit"].ToString();
                                    sb.AppendLine($"{np} ({rp}) x{q}");
                                }
                            }
                        }

                        foreach (var kv in mapDetails)
                        {
                            string permCode = kv.Key;
                            string det = kv.Value.ToString().Trim();

                            using (var fSig = new FrmSignatureManager(
                                ConfigSysteme.ConnectionString,
                                typeAction: permCode,
                                permissionCode: permCode,
                                reference: codeFactureGeneree ?? "VENTE_EN_COURS",
                                details: det,
                                idEmployeDemandeur: SessionEmploye.ID_Employe))
                            {
                                if (fSig.ShowDialog(this) != DialogResult.OK || !fSig.Approved)
                                {
                                    MessageBox.Show("Validation refusée : autorisation manager requise.", "Blocage",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    trans.Rollback();
                                    return;
                                }
                            }
                        }
                    }
                    // ============================================================================

                    // ================== ✅ STOCK : BLOCAGE AVANT VENTE ==================
                    VerifierStockAvantVente(con, trans, emplacementChoisi);
                    // ====================================================================

                    // ================== ✅ PAIEMENTS (SANS OUVRIR FormPaiementsVente) ==================
                    List<FormPaiementsVente.PaiementLine> payLines = new List<FormPaiementsVente.PaiementLine>();

                    if (_payLinesFromClientRapide != null && _payLinesFromClientRapide.Count > 0)
                    {
                        payLines = _payLinesFromClientRapide;
                    }
                    else
                    {
                        if (isCredit)
                        {
                            payLines.Add(new FormPaiementsVente.PaiementLine
                            {
                                ModePaiement = "CREDIT",
                                Devise = deviseVente,
                                Montant = 0m,
                                Reference = ""
                            });
                        }
                        else
                        {
                            payLines.Add(new FormPaiementsVente.PaiementLine
                            {
                                ModePaiement = "CASH",
                                Devise = deviseVente,
                                Montant = netFinal,
                                Reference = ""
                            });
                        }
                    }

                    // Ordonnance : uniquement depuis ClientRapide
                    OrdonnanceVenteDTO ordDto = _ordonnanceFromClientRapide;

                    // Référence transaction
                    string refTransaction = BuildReferenceTransactionFromPanier(dgvPanier);

                    // Harmoniser devise + mode + contrôle devise unique
                    foreach (var p in payLines)
                    {
                        p.ModePaiement = NormalizeModePaiement(p.ModePaiement);

                        string devisePay = NormalizeDevise(p.Devise ?? deviseVente);   // saisi (caisse)
                        string deviseSale = NormalizeDevise(deviseVente);              // devise vente

                        // ✅ mémoriser original
                        if (string.IsNullOrWhiteSpace(p.DeviseOriginale)) p.DeviseOriginale = devisePay;
                        if (p.MontantOriginal <= 0m) p.MontantOriginal = Math.Round(p.Montant, 2);
                        p.TauxApplique = 1m;

                        if (!devisePay.Equals(deviseSale, StringComparison.OrdinalIgnoreCase))
                        {
                            decimal taux;
                            decimal montantConverti = TauxChangeService.Convertir(con, trans, p.MontantOriginal, devisePay, deviseSale, out taux);

                            p.TauxApplique = taux;

                            // ✅ comptable (devise vente)
                            p.Devise = deviseSale;
                            p.Montant = Math.Round(montantConverti, 2);
                        }
                        else
                        {
                            p.Devise = deviseSale;
                            p.Montant = Math.Round(p.MontantOriginal, 2);
                        }
                    }

                    // Calcul total paiements
                    decimal sumPay = payLines.Sum(x => Math.Round(x.Montant, 2));

                    // Contrôle cohérence
                    if (!isCredit)
                    {
                        if (Math.Round(sumPay, 2) != Math.Round(netFinal, 2))
                        {
                            MessageBox.Show($"Total paiements ({sumPay:N2}) ≠ Montant à payer ({netFinal:N2}). Vérifie ClientRapide.");
                            trans.Rollback();
                            return;
                        }
                    }
                    else
                    {
                        if (sumPay < 0m || Math.Round(sumPay, 2) > Math.Round(netFinal, 2))
                        {
                            MessageBox.Show($"Paiements ({sumPay:N2}) doivent être ≤ Net ({netFinal:N2}) en mode crédit.");
                            trans.Rollback();
                            return;
                        }
                    }
                    // ====================================================================

                    // ================= VENTE =================
                    string mpResume = string.Join(" + ",
                        payLines.Select(p => NormalizeModePaiement(p.ModePaiement))
                                .Where(x => x.Length > 0)
                                .Distinct());
                    if (string.IsNullOrWhiteSpace(mpResume)) mpResume = "INCONNU";

                    using (SqlCommand cmdVente = new SqlCommand(@"
INSERT INTO Vente
(
    DateVente, ID_Client, IDEmploye, ModePaiement, MontantTotal, NomCaissier, Devise, IdSession, CodeFacture, Statut,
    AnnulePar, DateAnnulation, MotifAnnulation,
    RemiseTicketPct, RemiseTicketMontant,
    IdEntreprise, IdMagasin, IdPoste
)
OUTPUT INSERTED.ID_Vente
VALUES
(
    GETDATE(), @idClient, @idEmploye, @modePaiement, @montantNet, @nomCaissier, @devise, @idSession, @codeFacture, 'VALIDEE',
    NULL, NULL, NULL,
    @remisePct, @remiseMontant,
    @IdEntreprise, @IdMagasin, @IdPoste
);", con, trans))
                    {
                        cmdVente.Parameters.Add("@idClient", SqlDbType.Int).Value = idClient;
                        cmdVente.Parameters.Add("@idEmploye", SqlDbType.Int).Value = idEmploye;
                        cmdVente.Parameters.Add("@modePaiement", SqlDbType.NVarChar, 80).Value = mpResume;

                        var pNet = cmdVente.Parameters.Add("@montantNet", SqlDbType.Decimal);
                        pNet.Precision = 18; pNet.Scale = 2; pNet.Value = netFinal;

                        cmdVente.Parameters.Add("@nomCaissier", SqlDbType.NVarChar, 120).Value = nomCaissier;
                        cmdVente.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value = deviseVente;
                        cmdVente.Parameters.Add("@idSession", SqlDbType.Int).Value = idSessionCourante;
                        cmdVente.Parameters.Add("@codeFacture", SqlDbType.NVarChar, 30).Value = codeFactureGeneree;

                        var pPct = cmdVente.Parameters.Add("@remisePct", SqlDbType.Decimal);
                        pPct.Precision = 6; pPct.Scale = 2; pPct.Value = remiseTicketPct;

                        var pRm = cmdVente.Parameters.Add("@remiseMontant", SqlDbType.Decimal);
                        pRm.Precision = 18; pRm.Scale = 2; pRm.Value = remiseTicketMontant;

                        // ✅ CONTEXTE POS
                        cmdVente.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                        cmdVente.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                        cmdVente.Parameters.Add("@IdPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                        idVente = Convert.ToInt32(cmdVente.ExecuteScalar());

                        if (ordDto != null && ordDto.Lignes != null && ordDto.Lignes.Count > 0)
                        {
                            ordDto.CodeFacture = codeFactureGeneree;
                            ordDto.CodeCarteClient = _lastCodeCarteClient;
                            InsererOrdonnanceDansTransaction(con, trans, idVente, ordDto);
                        }
                    }

                    // ================== ✅ FIDELITE (APRES creation Vente) ==================
                    {
                        decimal montantFidelite = payLines
                            .Where(x => NormalizeModePaiement(x.ModePaiement) == "FIDELITE")
                            .Sum(x => Math.Round(x.Montant, 2));

                        montantFidelite = Math.Round(montantFidelite, 2);

                        bool isFidele = clientEstFideleAuMomentDeLaVente;

                        if (montantFidelite > 0m && !isFidele)
                        {
                            MessageBox.Show("Paiement fidélité réservé aux clients FIDÈLES. Modifie la catégorie client ou retire le paiement fidélité.");
                            trans.Rollback();
                            return;
                        }

                        if (montantFidelite > 0m)
                        {
                            UtiliserFideliteDansVente(con, trans, idClient, idVente, deviseVente, montantFidelite);
                        }
                    }

                    // ================== ✅ PARTENAIRE (APRES creation Vente) ==================
                    {
                        decimal montantPartenaire = payLines
                            .Where(x => NormalizeModePaiement(x.ModePaiement) == "PARTENAIRE")
                            .Sum(x => Math.Round(x.Montant, 2));

                        montantPartenaire = Math.Round(montantPartenaire, 2);

                        if (montantPartenaire > 0m)
                        {
                            if (idPartenaireCoupon <= 0) // ou ton idPartenaireVente si tu l’as
                            {
                                MessageBox.Show("Paiement PARTENAIRE: partenaire introuvable (id=0).");
                                trans.Rollback();
                                return;
                            }

                            UtiliserPartenaireFideliteDansVente(con, trans,
                                idPartenaire: idPartenaireCoupon,   // ✅ partenaire qui finance
                                idVente: idVente,
                                deviseVente: deviseVente,
                                montantUtilise: montantPartenaire,
                                refTransaction: refTransaction
                            );
                        }
                    }
                    // =======================================================================

                    // ======================================================================

                    foreach (var p in payLines)
                        InsertPaiementVente(con, trans, idVente, p, refTransaction);

                    foreach (DataGridViewRow r in dgvPanier.Rows)
                    {
                        if (r.IsNewRow) continue;

                        int idProduit = 0;
                        int.TryParse(r.Cells["ID_Produit"].Value?.ToString(), out idProduit);

                        string nomProduit = r.Cells["NomProduit"].FormattedValue != null
                            ? CleanText(r.Cells["NomProduit"].FormattedValue.ToString())
                            : "";

                        string refProduit = r.Cells["RefProduit"].Value != null
                            ? CleanText(r.Cells["RefProduit"].Value.ToString())
                            : "";

                        int quantite = 0;
                        int.TryParse(r.Cells["Quantite"].Value?.ToString(), out quantite);

                        decimal prixUnitaire = ParseDecimalCell(r, "PrixUnitaire");
                        decimal remise = ParseDecimalCell(r, "Remise");
                        decimal tva = ParseDecimalCell(r, "TVA");
                        decimal montant = ParseDecimalCell(r, "Montant");

                        // ✅ valider l'édition en cours avant lecture
                        dgvPanier.EndEdit();
                        this.Validate();

                        foreach (DataGridViewRow rowPanier in dgvPanier.Rows)
                        {
                            if (rowPanier == null || rowPanier.IsNewRow) continue;

                            int idx = rowPanier.Index;

                            // ================== LECTURE ROBUSTE ==================
                            int idProd = GetIntCell(rowPanier, "ID_Produit");

                            // NomProduit = ComboBox => Value = ID, FormattedValue = texte affiché
                            // Ici on prend le texte affiché pour "nom"
                            string nomProd = GetTextCell(rowPanier, "NomProduit");

                            string refProd = GetTextCell(rowPanier, "RefProduit");

                            // Qté (ta colonne est TextBox)
                            int qte = 0;
                            {
                                string sQ = Convert.ToString(rowPanier.Cells["Quantite"]?.Value ?? "")?.Trim();
                                if (!int.TryParse(sQ, out qte))
                                {
                                    // parfois c'est "1,00"
                                    if (decimal.TryParse(sQ, NumberStyles.Any, CultureInfo.GetCultureInfo("fr-FR"), out var dq))
                                        qte = (int)Math.Round(dq, 0);
                                }
                            }
                            if (qte <= 0) qte = 1;

                            decimal pu = GetDecCell(rowPanier, "PrixUnitaire");
                            decimal remisePct = GetDecCell(rowPanier, "Remise");
                            decimal tvaPct = GetDecCell(rowPanier, "TVA");
                            decimal montantTtc = GetDecCell(rowPanier, "Montant");

                            // fallback si montant vide
                            if (montantTtc <= 0m)
                            {
                                decimal baseHt = Math.Round(pu * qte, 2);
                                decimal rem = Math.Round(baseHt * (remisePct / 100m), 2);
                                decimal htApresRem = Math.Max(0m, baseHt - rem);

                                decimal tvaMontant = Math.Round(htApresRem * (tvaPct / 100m), 2); // ✅ nom unique
                                montantTtc = Math.Round(htApresRem + tvaMontant, 2);
                            }

                            // Si Nom vide, fallback Ref
                            if (string.IsNullOrWhiteSpace(nomProd)) nomProd = refProd;

                            // ================== VALIDATIONS ==================
                            if (idProd <= 0 || string.IsNullOrWhiteSpace(nomProd))
                            {
                                string debug =
                                    $"Ligne #{idx + 1} invalide.\n\n" +
                                    $"ID_Produit={idProd}\n" +
                                    $"NomProduit(affiché)='{nomProd}'\n" +
                                    $"RefProduit='{refProd}'\n" +
                                    $"Quantite='{Convert.ToString(rowPanier.Cells["Quantite"]?.Value ?? "")}'\n\n" +
                                    $"➡️ Sélectionne un produit dans la colonne Désignation puis réessaie.";

                                SelectRowErrorPanier(idx, debug);
                                throw new Exception("Ligne panier invalide : produit/désignation manquants.");
                            }

                            // ================== STOCK ==================
                            SortieStockAtomique(
                                con, trans,
                                idProd,
                                string.IsNullOrWhiteSpace(refProd) ? null : refProd,
                                qte,
                                CleanText(txtNomCaissier.Text),
                                "VENTE",
                                emplacementChoisi,
                                null
                            );

                            // ================== INSERT DETAILS ==================
                            using (SqlCommand cmdDetail = new SqlCommand(@"
INSERT INTO DetailsVente
(
 ID_Vente, ID_Produit, Quantite, PrixUnitaire, RefProduit, NomProduit,
 Remise, TVA, Montant, Devise, NomCaissier,
 IdEntreprise, IdMagasin, IdPoste
)
VALUES
(
 @idVente, @idProduit, @quantite, @prixUnitaire, @refProduit, @nomProduit,
 @remise, @tva, @montant, @devise, @nomCaissier,
 @IdEntreprise, @IdMagasin, @IdPoste
)", con, trans))
                            {
                                cmdDetail.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;
                                cmdDetail.Parameters.Add("@idProduit", SqlDbType.Int).Value = idProd;
                                cmdDetail.Parameters.Add("@quantite", SqlDbType.Int).Value = qte;

                                var pPU2 = cmdDetail.Parameters.Add("@prixUnitaire", SqlDbType.Decimal);
                                pPU2.Precision = 18; pPU2.Scale = 2; pPU2.Value = pu;

                                cmdDetail.Parameters.Add("@refProduit", SqlDbType.NVarChar, 50).Value =
                                    string.IsNullOrWhiteSpace(refProd) ? (object)DBNull.Value : refProd;

                                cmdDetail.Parameters.Add("@nomProduit", SqlDbType.NVarChar, 200).Value = nomProd;

                                var pRem2 = cmdDetail.Parameters.Add("@remise", SqlDbType.Decimal);
                                pRem2.Precision = 18; pRem2.Scale = 2; pRem2.Value = remisePct;

                                var pTva2 = cmdDetail.Parameters.Add("@tva", SqlDbType.Decimal);
                                pTva2.Precision = 18; pTva2.Scale = 2; pTva2.Value = tvaPct;

                                var pMont2 = cmdDetail.Parameters.Add("@montant", SqlDbType.Decimal);
                                pMont2.Precision = 18; pMont2.Scale = 2; pMont2.Value = montantTtc;

                                cmdDetail.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value = deviseVente;
                                cmdDetail.Parameters.Add("@nomCaissier", SqlDbType.NVarChar, 120).Value = nomCaissier;

                                cmdDetail.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                                cmdDetail.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                                cmdDetail.Parameters.Add("@IdPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                                cmdDetail.ExecuteNonQuery();
                            }
                        }

                        // ================== ✅ COUPON (APPLY + SAVE) ==================
                        if (!string.IsNullOrWhiteSpace(couponCode) && remiseCoupon > 0m)
                    {
                        var mappedPays = payLines.Select(p => new CouponService.CouponPaiementLine
                        {
                            Mode = p.ModePaiement,
                            Devise = p.Devise,
                            Montant = p.Montant,
                            Reference = p.Reference
                        }).ToList();

                        var applyRes = _couponSvc.ApplyCouponSplitAndSaveTx(
                            idClient: idClient,
                            codeCoupon: couponCode,
                            baseAmountAvantCoupon: montantNetAPayer,
                            remiseCouponReelle: remiseCoupon,
                            idVente: idVente,
                            deviseVente: deviseVente,
                            idPartenaire: idPartenaireCoupon,
                            refTransaction: refTransaction,
                            payLines: mappedPays,
                            con: con,
                            trans: trans
                        );

                        if (applyRes == null || !applyRes.Applied)
                        {
                            MessageBox.Show("Coupon non appliqué : " + (applyRes?.Message ?? "Erreur inconnue"));
                            trans.Rollback();
                            return;
                        }
                    }
                    // =====================================================================

                    decimal acompte = Math.Round(sumPay, 2);
                    decimal resteCredit = Math.Round(netFinal - acompte, 2);
                    if (resteCredit < 0m) resteCredit = 0m;

                    if (isCredit && resteCredit > 0m)
                    {
                        int idCredit = _creditSvc.CreateCreditVente(
                            idClient: idClient,
                            idVente: idVente,
                            refVente: codeFactureGeneree,
                            total: netFinal,
                            acompte: acompte,
                            reste: resteCredit,
                            echeance: echeance,
                            con: con,
                            trans: trans
                        );

                        using (var cmd = new SqlCommand("UPDATE Vente SET Statut='CREDIT' WHERE ID_Vente=@v", con, trans))
                        {
                            cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    _compteSvc.EnsureClientModules(idClient, con, trans);

                    // ✅ Déterminer catégorie client (FIDELITE oui/non) DANS CE CONTEXTE
                    bool clientEstFidele = false;
                    try
                    {
                        using (var cmdCat2 = new SqlCommand(@"
SELECT ISNULL(NULLIF(LTRIM(RTRIM(CategorieClient)),''),'OCCASIONNEL')
FROM dbo.Clients
WHERE ID_Clients = @idClient;", con, trans))
                        {
                            cmdCat2.Parameters.Add("@idClient", SqlDbType.Int).Value = idClient;

                            var o = cmdCat2.ExecuteScalar();
                            string cat = (o == null || o == DBNull.Value) ? "OCCASIONNEL" : o.ToString().Trim().ToUpperInvariant();
                            clientEstFidele = cat.Equals("FIDELE", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch
                    {
                        clientEstFidele = false;
                    }

                    // ✅ Gain fidélité UNIQUEMENT pour clients FIDELE
                    if (clientEstFideleAuMomentDeLaVente)
                    {
                        _loyalSvc.ApplyGain(idClient, idVente, netFinal, con, trans);
                        LoadFideliteForClient(con, trans, idClient);
                    }

                    _statsSvc.Recompute(idClient, con, trans);
                    MettreAJourTotauxSession(con, trans, idSessionCourante);

                    using (var cmd = new SqlCommand("dbo.Client_RecalculerFidelite", con, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdClient", SqlDbType.Int).Value = idClient;
                        cmd.ExecuteNonQuery();
                    }

                    // ✅ COMMIT DB (la vente est validée ici)
                    trans.Commit();

                    // ✅ Nettoyer (évite réutilisation sur vente suivante)
                    _payLinesFromClientRapide = null;
                    _ordonnanceFromClientRapide = null;
                    _codeFactureEnCours = "";

                    _lastIdVente = idVente;
                    _lastCodeFacture = codeFactureGeneree;

                    SelectVenteInRapport(idVente);

                    ConfigSysteme.LoadPrintersConfig();
                    if (!IsPrinterValid(ConfigSysteme.ImprimanteTicketNom))
                    {
                        EnsureTicketPrinterIsReady(this);
                    }

                    // ======================== PDF ========================
                    string chosenFilePath = DemanderCheminPdf_Fichier(codeFactureGeneree);

                    if (string.IsNullOrWhiteSpace(chosenFilePath))
                    {
                        TryPrintTicket_AfterPdf_NoBlock(idVente);

                        MessageBox.Show("✅ Vente enregistrée.\nFacture: " + codeFactureGeneree,
                            "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        ResetApresVente();
                        MettreAJourTotaux();
                        ChargerClientsComboBox();
                        return;
                    }

                    bool pdfOk = false;

                    try
                    {
                        string dossier = Path.GetDirectoryName(chosenFilePath);
                        string generatedPath = GenererPdfFactureDepuisDb(idVente, codeFactureGeneree, dossier);

                        if (!string.Equals(generatedPath, chosenFilePath, StringComparison.OrdinalIgnoreCase))
                        {
                            if (File.Exists(chosenFilePath))
                                File.Delete(chosenFilePath);

                            File.Move(generatedPath, chosenFilePath);
                        }

                        OuvrirPdf(chosenFilePath);
                        pdfOk = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show("Accès refusé. Choisis un autre dossier (Documents, D:\\, USB...).",
                            "PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show("Fichier bloqué/ouvert : " + ioEx.Message,
                            "PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception pdfEx)
                    {
                        MessageBox.Show("Vente validée, mais erreur PDF : " + pdfEx.Message,
                            "PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        TryPrintTicket_AfterPdf_NoBlock(idVente);
                    }

                    // ======================== FIN ========================
                    MessageBox.Show("✅ Vente enregistrée.\nFacture: " + codeFactureGeneree +
                                    (pdfOk ? "\nPDF : OK" : "\nPDF : Non généré"),
                        "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ResetApresVente();
                    MettreAJourTotaux();
                    ChargerClientsComboBox();
                }
             }
                catch (SqlException ex)
                {
                    try { trans.Rollback(); } catch { }

                    _payLinesFromClientRapide = null;
                    _ordonnanceFromClientRapide = null;

                    _lastIdVente = 0;
                    _lastCodeFacture = "";

                    var sb = new StringBuilder();
                    sb.AppendLine("Erreur SQL : " + ex.Message);
                    sb.AppendLine("Client=" + CleanText(cmbNomClient.Text) + " / Tel=" + CleanText(txtTelephone.Text));
                    sb.AppendLine("Devise=" + (deviseVente ?? "") + " / Net=" + netFinal.ToString("N2"));

                    if (ex.Errors != null && ex.Errors.Count > 0)
                    {
                        var e0 = ex.Errors[0];
                        sb.AppendLine("SQL Number: " + e0.Number);
                        sb.AppendLine("SQL Procedure: " + (string.IsNullOrWhiteSpace(e0.Procedure) ? "(n/a)" : e0.Procedure));
                        sb.AppendLine("SQL Line: " + e0.LineNumber);
                        sb.AppendLine("SQL State: " + e0.State);
                        sb.AppendLine("SQL Class: " + e0.Class);
                    }

                    MessageBox.Show(sb.ToString(), "Finalisation - SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Exception ex)
                {
                    try { trans.Rollback(); } catch { }

                    _payLinesFromClientRapide = null;
                    _ordonnanceFromClientRapide = null;

                    _lastIdVente = 0;
                    _lastCodeFacture = "";

                    MessageBox.Show(
                        "Erreur lors de l'enregistrement : " + ex.Message + Environment.NewLine +
                        "Client=" + CleanText(cmbNomClient.Text) + " / Tel=" + CleanText(txtTelephone.Text) + Environment.NewLine +
                        "Devise=" + (deviseVente ?? "") + " / Net=" + netFinal.ToString("N2"),
                        "Finalisation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }
            }
        }

        private void UtiliserPartenaireFideliteDansVente(SqlConnection con, SqlTransaction trans,
    int idPartenaire, int idVente, string deviseVente, decimal montantUtilise, string refTransaction)
        {
            if (idPartenaire <= 0) throw new Exception("Partenaire invalide.");
            if (idVente <= 0) throw new Exception("Vente invalide.");
            if (montantUtilise <= 0m) return;

            deviseVente = (deviseVente ?? "CDF").Trim().ToUpperInvariant();
            if (deviseVente == "FC") deviseVente = "CDF";

            // 1) SP : débite le solde partenaire + historise
            using (var cmd = new SqlCommand("dbo.PartenairesFidelite_Utiliser", con, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@IdPartenaire", SqlDbType.Int).Value = idPartenaire;
                cmd.Parameters.Add("@IdVente", SqlDbType.Int).Value = idVente;          // ✅ lien vente
                cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = deviseVente;

                var p = cmd.Parameters.Add("@MontantUtilise", SqlDbType.Decimal);
                p.Precision = 18; p.Scale = 2; p.Value = Math.Round(montantUtilise, 2);

                cmd.ExecuteNonQuery();
            }

            // 2) Trace comptable : PaiementsVente
            using (var cmdPay = new SqlCommand(@"
INSERT INTO dbo.PaiementsVente
(
    IdVente, ModePaiement, Devise, Montant, DatePaiement,
    ReferenceTransaction, Statut,
    DeviseOriginale, MontantOriginal, TauxApplique,
    IdEntreprise, IdMagasin, IdPoste
)
VALUES
(
    @IdVente, @Mode, @Devise, @Montant, GETDATE(),
    @Ref, 'VALIDE',
    @DevOrig, @MontOrig, @Taux,
    @IdEntreprise, @IdMagasin, @IdPoste
);", con, trans))
            {
                cmdPay.Parameters.Add("@IdVente", SqlDbType.Int).Value = idVente;
                cmdPay.Parameters.Add("@Mode", SqlDbType.NVarChar, 50).Value = "PARTENAIRE"; // ou "PARTENAIRE_FIDELITE"
                cmdPay.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = deviseVente;

                var pM = cmdPay.Parameters.Add("@Montant", SqlDbType.Decimal);
                pM.Precision = 18; pM.Scale = 2; pM.Value = Math.Round(montantUtilise, 2);

                cmdPay.Parameters.Add("@Ref", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(refTransaction) ? (object)DBNull.Value : refTransaction;

                // compta conversion (si tu ne convertis pas ici)
                cmdPay.Parameters.Add("@DevOrig", SqlDbType.NVarChar, 10).Value = deviseVente;
                var pMO = cmdPay.Parameters.Add("@MontOrig", SqlDbType.Decimal);
                pMO.Precision = 18; pMO.Scale = 2; pMO.Value = Math.Round(montantUtilise, 2);

                var pT = cmdPay.Parameters.Add("@Taux", SqlDbType.Decimal);
                pT.Precision = 18; pT.Scale = 6; pT.Value = 1m;

                // ✅ contexte POS
                cmdPay.Parameters.Add("@IdEntreprise", SqlDbType.Int).Value = AppContext.IdEntreprise;
                cmdPay.Parameters.Add("@IdMagasin", SqlDbType.Int).Value = AppContext.IdMagasin;
                cmdPay.Parameters.Add("@IdPoste", SqlDbType.Int).Value = AppContext.IdPoste;

                cmdPay.ExecuteNonQuery();
            }
        }


        private static string CleanText(string s)
        {
            if (s == null) return "";
            s = s.Replace("\u00A0", " ");          // NBSP -> space
            s = s.Trim();
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return s;
        }

        private bool IsPrinterReady(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName)) return false;

            foreach (string p in PrinterSettings.InstalledPrinters)
                if (string.Equals(p, printerName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var ps = new PrinterSettings { PrinterName = printerName };
                        if (!ps.IsValid) return false;

                        // tentative simple : si ça jette exception => pas prête
                        using (var pd = new PrintDocument())
                        {
                            pd.PrinterSettings.PrinterName = printerName;
                            // Ne lance pas pd.Print() ici, juste valider settings
                        }
                        return true;
                    }
                    catch { return false; }
                }

            return false;
        }



        private bool IsPrinterValid(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName)) return false;

            try
            {
                var ps = new PrinterSettings { PrinterName = printerName.Trim() };
                return ps.IsValid; // ✅ installé + reconnu Windows
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ouvre la boîte centrale si imprimante absente/invalide.
        /// Retourne true si une imprimante valide est disponible après.
        /// </summary>
        private bool EnsureTicketPrinterIsReady(IWin32Window owner)
        {
            // ✅ charger config au cas où
            ConfigSysteme.LoadPrintersConfig();

            // ✅ si déjà valide -> ok
            if (IsPrinterValid(ConfigSysteme.ImprimanteTicketNom))
                return true;

            // ✅ sinon, ouvrir le formulaire central de sélection
            using (var f = new FormGestionImprimantes())
            {
                f.StartPosition = FormStartPosition.CenterParent;
                var dr = f.ShowDialog(owner);
                // Même si Cancel, on re-teste derrière (au cas où Save a été fait)
            }

            ConfigSysteme.LoadPrintersConfig();
            return IsPrinterValid(ConfigSysteme.ImprimanteTicketNom);
        }

        /// <summary>
        /// Impression non bloquante : si imprimante pas prête => on ignore.
        /// IMPORTANT: à appeler APRÈS commit et APRÈS PDF.
        /// </summary>
        private void TryPrintTicket_AfterPdf_NoBlock(int idVente)
        {
            // ✅ si pas d'imprimante valide => on ne bloque pas
            if (!IsPrinterValid(ConfigSysteme.ImprimanteTicketNom))
                return;

            // ✅ impression en arrière-plan (ne ralentit pas la finalisation)
            Task.Run(() =>
            {
                try
                {
                    // ⚠️ ici adapte si ta méthode prend déjà un nom d'imprimante
                    // Si ta méthode actuelle n'accepte pas le nom, alors modifie-la (voir plus bas)
                    ImprimerTicket_AutoSansDialog(idVente, ConfigSysteme.ImprimanteTicketNom);
                }
                catch
                {
                    // ✅ on ignore totalement (pas de MessageBox pour ne pas "relancer" / déranger la finalisation)
                }
            });
        }

        private void ImprimerTicket_AutoSansDialog(int idVente, string printerName)
        {
           

            PrintDocument doc = new PrintDocument();
            doc.PrinterSettings.PrinterName = printerName;

            
            if (!doc.PrinterSettings.IsValid)
                return;

            doc.PrintController = new StandardPrintController(); // ✅ pas de popup

            doc.PrintPage += (s, e) =>
            {
                
            };

            doc.Print();
        }


        private void OuvrirClientRapidePuisFinaliser()
        {
            try
            {
                // ===================== 0) PRECHECK =====================
                if (dgvPanier.Rows.Count == 0 || dgvPanier.Rows.Cast<DataGridViewRow>().All(r => r.IsNewRow))
                {
                    MessageBox.Show("Panier vide.");
                    return;
                }

                // ===================== 1) DEVISE VENTE (une seule) =====================
                string deviseVente = null;

                foreach (DataGridViewRow r in dgvPanier.Rows)
                {
                    if (r.IsNewRow) continue;

                    string d = CleanText(r.Cells["Devise"]?.Value?.ToString() ?? "");
                    if (string.IsNullOrWhiteSpace(d)) continue;

                    if (deviseVente == null) deviseVente = d;
                    else if (!d.Equals(deviseVente, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Une vente ne peut contenir qu'une seule devise. Corrige les lignes du panier.");
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(deviseVente)) deviseVente = "CDF";
                deviseVente = CleanText(deviseVente).ToUpperInvariant();
                if (deviseVente == "FC") deviseVente = "CDF";

                // ===================== 2) NET A PAYER (juste pour afficher/paiements rapides) =====================
                // Le net FINAL sera recalculé dans btnFinaliser_Click (remise ticket/coupon/etc.)
                if (!decimal.TryParse(
                        CleanText(txtTotalTTC.Text),
                        NumberStyles.Any,
                        CultureInfo.GetCultureInfo("fr-FR"),
                        out decimal netAPayer))
                    netAPayer = 0m;

                if (netAPayer < 0m) netAPayer = 0m;

                // ===================== 3) CODE FACTURE EN COURS (pour N° ordonnance) =====================
                EnsureCodeFactureEnCours_DB(); // doit remplir _codeFactureEnCours

                // ===================== 4) CONTEXTE ORDONNANCE =====================
                var lignesOrd = ConstruireLignesOrdonnanceDepuisPanier();   // si tu l’as
                if (lignesOrd == null || lignesOrd.Count == 0)
                    lignesOrd = BuildOrdonnanceLinesFromPanier();           // fallback si tu as l’autre

                string patient = CleanText(cmbNomClient.Text);
                string prescripteur = GetPrescripteurConnecte();
                string numeroOrd = CleanText(_codeFactureEnCours);

                // ===================== 5) OUVRIR CLIENT RAPIDE =====================
                using (var dlg = new FormClientsRapide(
                    connectionString: ConfigSysteme.ConnectionString,
                    couponInitial: txtCouponCode.Text,
                    creditInitial: chkVenteCredit.Checked,
                    echeanceInitial: dtpEcheanceCredit.Value.Date,
                    emplacementsSource: cmbEmplacement.Items,
                    emplacementInitial: cmbEmplacement.SelectedItem?.ToString(),
                    injectToFormVentes: InjectClientRapide,
                    finaliserAction: () => BeginInvoke(new Action(() => btnFinaliser.PerformClick())),
                    netAPayer: netAPayer,
                    deviseVente: deviseVente,
                    codeFactureEnCours: _codeFactureEnCours
                ))
                {
                    // Optionnel (si tu utilises ordonnance)
                    dlg.LignesPanier = BuildOrdonnanceLinesFromPanier();

                    // IMPORTANT : charger avant ShowDialog
                    dlg.ChargerContexteOrdonnance(lignesOrd, numeroOrd, prescripteur, patient);

                    // ===================== 6) SHOW =====================
                    dlg.ShowDialog(this);

                    // NOTE :
                    // Tu n’as pas besoin de récupérer ici _payLinesFromClientRapide / _ordonnanceFromClientRapide
                    // car InjectClientRapide() les stocke déjà.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ClientRapide : " + ex.Message);
            }
        }

        private string DemanderCheminPdf_Fichier(string codeFacture)
        {
            string nom = (codeFacture ?? "FACTURE") + ".pdf";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Enregistrer la facture";
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = nom;
                sfd.AddExtension = true;
                sfd.DefaultExt = "pdf";
                sfd.OverwritePrompt = true;

                // DesktopDirectory est plus fiable que Desktop
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (!Directory.Exists(desktop))
                    desktop = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                sfd.InitialDirectory = desktop;

                return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
            }
        }


        private void SelectVenteInRapport(int idVente)
        {
            if (dgvRapport == null || dgvRapport.Rows.Count == 0) return;
            if (!dgvRapport.Columns.Contains("ID_Vente")) return;

            foreach (DataGridViewRow r in dgvRapport.Rows)
            {
                if (r.IsNewRow) continue;

                int id = 0;
                int.TryParse(Convert.ToString(r.Cells["ID_Vente"].Value), out id);

                if (id == idVente)
                {
                    dgvRapport.ClearSelection();
                    r.Selected = true;
                    dgvRapport.FirstDisplayedScrollingRowIndex = r.Index;
                    return;
                }
            }
        }


        // Initialiser les combos mode paiement et devise
        private void InitialiserCombos()
        {
            cmbModePaiement.Items.Clear();

            // ✅ Ajout PARTENAIRE
            cmbModePaiement.Items.AddRange(new[]
            {
        "ESPECES",
        "CARTE",
        "Mobile Money",
        "FIDELITE",
        "PARTENAIRE"
    });

            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new[] { "CDF", "USD", "EUR" });

            if (cmbModePaiement.Items.Count > 0)
                cmbModePaiement.SelectedIndex = 0;

            if (cmbDevise.Items.Count > 0)
                cmbDevise.SelectedIndex = 0;
        }

        private void UtiliserFideliteDansVente(
    SqlConnection con, SqlTransaction trans,
    int idClient, int idVente,
    string devise, decimal montantFidelite)
        {
            if (idClient <= 0) throw new Exception("Client requis pour paiement fidélité.");
            if (idVente <= 0) throw new Exception("IdVente invalide pour paiement fidélité.");
            if (montantFidelite <= 0m) throw new Exception("Montant fidélité invalide.");

            devise = (devise ?? "CDF").Trim().ToUpperInvariant();
            if (devise == "FC") devise = "CDF";

            using (var cmd = new SqlCommand("dbo.Fidelite_Utiliser", con, trans))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@IdClient", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@IdVente", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = devise;

                var p = cmd.Parameters.Add("@MontantUtilise", SqlDbType.Decimal);
                p.Precision = 18; p.Scale = 2;
                p.Value = Math.Round(montantFidelite, 2);

                cmd.ExecuteNonQuery();
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Effacer le panier ?", "Confirmer", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                dgvPanier.Rows.Clear();        // Vide toutes les lignes du DataGridView

                cmbNomClient.Items.Clear();    // Vider tous les éléments du ComboBox

                txtIDEmploye.Text = SessionEmploye.ID_Employe.ToString(); // Réinitialiser l'ID Employé (ou autre valeur)

                MettreAJourTotaux();           // Mise à jour des totaux (fonction que tu as déjà)
            }
        }

        private void btnImprimer_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

            pd.PrintPage += PrintPageHandlerA4_Pro;

            using (PrintDialog dlg = new PrintDialog { Document = pd })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    pd.Print();
            }
        }

        private void btnApercu_Click(object sender, EventArgs e)
        {
            if (dgvRapport.CurrentRow == null && _lastIdVente <= 0)
            {
                MessageBox.Show("Sélectionne une vente dans le rapport ou finalise une vente d'abord.");
                return;
            }

            int idVente = 0;
            if (dgvRapport.CurrentRow != null)
                idVente = Convert.ToInt32(dgvRapport.CurrentRow.Cells["ID_Vente"].Value);
            else
                idVente = _lastIdVente;

            try
            {
                // ✅ modèle d'impression (le même que duplicata)
                _printModel = ChargerVentePourImpression(idVente);
                if (_printModel == null)
                {
                    MessageBox.Show("Impossible de charger la vente.");
                    return;
                }

                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                // ✅ IMPORTANT : utiliser ton handler A4 depuis le modèle
                pd.PrintPage += PrintPageHandlerA4_FromModel;

                using (PrintPreviewDialog preview = new PrintPreviewDialog())
                {
                    preview.Document = pd;
                    preview.Width = 1100;
                    preview.Height = 850;

                    // Certains PC exigent ça sinon aperçu blanc
                    preview.UseAntiAlias = true;

                    preview.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur aperçu : " + ex.Message);
            }
        }

        private void PrintPageHandlerA4_Pro(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int left = e.MarginBounds.Left;
            int top = e.MarginBounds.Top;
            int right = e.MarginBounds.Right;
            int y = top;

            var fTitre = new DFont("Trebuchet MS", 16, FontStyle.Bold);
            var fTxt = new DFont("Trebuchet MS", 10, FontStyle.Regular);
            var fSmall = new DFont("Trebuchet MS", 9, FontStyle.Regular);
            var fBold = new DFont("Trebuchet MS", 10, FontStyle.Bold);

            // Code facture
            string codeFacture = (!string.IsNullOrWhiteSpace(_lastCodeFacture))
                ? _lastCodeFacture
                : "FAC" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Entête (mêmes textes)
            g.DrawString("ZAIRE MODE SARL", fTitre, Brushes.Black, left, y); y += 30;
            g.DrawString("23, Bld Lumumba / Immeuble Masina Plaza", fTxt, Brushes.Black, left, y); y += 18;
            g.DrawString("+243861507560 / E-MAIL: Zaireshop@hotmail.com", fTxt, Brushes.Black, left, y); y += 18;
            g.DrawString("PAGE: ZAIRE.CD   RCCM: 25-B-01497   IDNAT: 01-F4300-N73258E", fSmall, Brushes.Black, left, y); y += 25;

            // Logo à droite
            string logoPath = @"D:\ZAIRE\LOGO1.png";
            if (File.Exists(logoPath))
            {
                try
                {
                    using (var logo = System.Drawing.Image.FromFile(logoPath))
                        g.DrawImage(logo, right - 140, top, 120, 120);
                }
                catch { }
            }

            g.DrawLine(Pens.Black, left, y, right, y); y += 10;

            // Infos facture
            g.DrawString("Facture N° : " + codeFacture, fBold, Brushes.Black, left, y); y += 18;
            g.DrawString("Date : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), fTxt, Brushes.Black, left, y); y += 18;
            g.DrawString("Caissier : " + txtNomCaissier.Text, fTxt, Brushes.Black, left, y); y += 18;
            g.DrawString("Client : " + txtPrenomClient.Text + " " + cmbNomClient.Text, fTxt, Brushes.Black, left, y); y += 20;

            g.DrawLine(Pens.Black, left, y, right, y); y += 12;

            // Colonnes (calculées)
            int colArticle = left;
            int colQte = left + (int)(e.MarginBounds.Width * 0.60);
            int colPU = left + (int)(e.MarginBounds.Width * 0.72);
            int colTotal = left + (int)(e.MarginBounds.Width * 0.85);

            // Header tableau
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), left, y, e.MarginBounds.Width, 22);
            g.DrawRectangle(Pens.Gray, left, y, e.MarginBounds.Width, 22);

            g.DrawString("Article", fBold, Brushes.Black, colArticle + 4, y + 3);
            g.DrawString("Qté", fBold, Brushes.Black, colQte + 4, y + 3);
            g.DrawString("PU", fBold, Brushes.Black, colPU + 4, y + 3);
            g.DrawString("Total", fBold, Brushes.Black, colTotal + 4, y + 3);
            y += 28;

            // Lignes
            foreach (DataGridViewRow row in dgvPanier.Rows)
            {
                if (row.IsNewRow) continue;

                string nom = row.Cells["NomProduit"].FormattedValue?.ToString() ?? "";
                string qte = row.Cells["Quantite"].Value?.ToString() ?? "0";
                string pu = row.Cells["PrixUnitaire"].Value?.ToString() ?? "0";
                string total = row.Cells["Montant"].Value?.ToString() ?? "0";

                // wrap article
                RectangleF rNom = new RectangleF(colArticle + 2, y, colQte - colArticle - 10, 40);
                g.DrawString(nom, fTxt, Brushes.Black, rNom);

                g.DrawString(qte, fTxt, Brushes.Black, colQte, y);
                g.DrawString(pu, fTxt, Brushes.Black, colPU, y);
                g.DrawString(total, fTxt, Brushes.Black, colTotal, y);

                y += 22;
                g.DrawLine(Pens.Gainsboro, left, y, right, y);
                y += 3;
            }

            y += 10;
            g.DrawLine(Pens.Black, left, y, right, y); y += 14;

            // Totaux
            string devise = string.IsNullOrWhiteSpace(cmbDevise.Text) ? "CDF" : cmbDevise.Text;
            g.DrawString("TOTAL TTC : " + txtTotal.Text + " " + devise, new DFont("Trebuchet MS", 12, FontStyle.Bold),
                Brushes.Black, colPU, y);
            y += 30;

            g.DrawString("Mode de paiement : " + cmbModePaiement.Text, fTxt, Brushes.Black, left, y); y += 25;

            g.DrawString("Merci pour votre fidélité, à la prochaine !", fTxt, Brushes.Black, left, y); y += 18;
            g.DrawString("La Qualité fait la différence.", fTxt, Brushes.Black, left, y); y += 25;

            g.DrawString("Les marchandises vendues ne peuvent être ni reprises, ni échangées.", fSmall, Brushes.Black, left, y); y += 25;

            // Barcode
            try
            {
                var barcodeWriter = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions { Width = 320, Height = 60 }
                };
                using (Bitmap barcode = barcodeWriter.Write(codeFacture))
                {
                    int bw = 300;   // ✅ réduit
                    int bh = 55;    // ✅ réduit

                    using (Bitmap resized = new Bitmap(barcode, new Size(bw, bh)))
                    {
                        int x = left + (e.MarginBounds.Width - bw) / 2; // ✅ centré
                        int yBarcode = y + 10;                          // ✅ descend un peu
                        g.DrawImage(resized, x, yBarcode);
                        y = yBarcode + bh + 10;
                    }
                }

                g.DrawString("Code Facture : " + codeFacture, fTxt, Brushes.Black,
                    left + (e.MarginBounds.Width - 250) / 2, y);
            }
            catch
            {
                g.DrawString("(Erreur code-barres)", fSmall, Brushes.Gray, left, y);
            }

            e.HasMorePages = false;
        }

        private void btnRechercher_Click(object sender, EventArgs e)
        {
            string filtre = txtRechercheProduit.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filtre))
            {
                // Si vide, afficher tous les produits
                cmbNomProduit.DataSource = null;
                cmbNomProduit.DataSource = tousLesProduits;
                cmbNomProduit.SelectedIndex = -1;
                return;
            }

            var listeFiltree = tousLesProduits
                .Where(p => p.ToLower().Contains(filtre))
                .ToList();

            cmbNomProduit.DataSource = null;
            cmbNomProduit.DataSource = listeFiltree;
            cmbNomProduit.SelectedIndex = -1;
        }


        private void btnCharger_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    string sql = @"
                SELECT 
    ID_Vente,
    DateVente,
    ID_Client,
    IDEmploye,
    ModePaiement,
    MontantTotal,
    NomCaissier,
    Devise,
    CodeFacture
FROM Vente
                WHERE DateVente BETWEEN @Debut AND @Fin
                ";

                    // Si txtIDEmploye vide, on n'applique pas ce filtre
                    if (!string.IsNullOrWhiteSpace(txtIDEmploye.Text))
                    {
                        sql += " AND CAST(IDEmploye AS NVARCHAR) LIKE @IDEmploye ";
                    }

                    sql += " ORDER BY DateVente DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@Debut", SqlDbType.DateTime).Value = dtpDateDebut.Value.Date;
                        cmd.Parameters.Add("@Fin", SqlDbType.DateTime).Value = dtpDateFin.Value.Date.AddDays(1).AddSeconds(-1);

                        if (!string.IsNullOrWhiteSpace(txtIDEmploye.Text))
                        {
                            cmd.Parameters.Add("@IDEmploye", SqlDbType.NVarChar).Value = "%" + txtIDEmploye.Text.Trim() + "%";
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        InitialiserDgvRapport();

                        dgvRapport.DataSource = dt;
                        dgvRapport.ClearSelection();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ConfigSysteme.Traduire("Erreur chargement ventes : ") + ex.Message,
                    ConfigSysteme.Traduire("Erreur"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string FormatMontant(decimal montant, string devise)
        {
            return string.Format(CultureInfo.GetCultureInfo("fr-FR"), "{0:N2} {1}", montant, devise);
        }

        private bool VerifierStockDisponible(string reference, int quantiteDemandee)
        {
            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();

                string sql = @"
            SELECT 
                ISNULL(SUM(CASE WHEN TypeOperation = 'ENTREE' THEN Quantite ELSE 0 END), 0) -
                ISNULL(SUM(CASE WHEN TypeOperation = 'SORTIE' THEN Quantite ELSE 0 END), 0) AS StockActuel
            FROM OperationsStock
            WHERE Reference = @ref
            GROUP BY Reference";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ref", reference);

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return false;

                    int stockActuel = Convert.ToInt32(result);

                    return stockActuel >= quantiteDemandee;
                }
            }
        }

        private void btnExporterExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Charger les données depuis la base
                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();
                    string requete = @"
                SELECT [ID_Details], [ID_Vente], [ID_Produit], [Quantite], [PrixUnitaire], [RefProduit], 
                       [NomProduit], [Remise], [TVA], [Montant], [Devise], [NomCaissier]
                FROM [MaBaseSQL2019].[dbo].[DetailsVente]";

                    using (SqlCommand cmd = new SqlCommand(requete, con))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show(ConfigSysteme.Traduire("Aucune donnée à exporter !"),
                        ConfigSysteme.Traduire("Information"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog()
                {
                    Filter = "CSV (*.csv)|*.csv",
                    Title = ConfigSysteme.Traduire("Exporter en CSV"),
                    FileName = "DetailsVente_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv"
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return;

                    using (StreamWriter sw = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        // Écrire l'entête CSV (noms des colonnes)
                        var entetes = dt.Columns.Cast<DataColumn>()
                            .Select(col => QuoteCsv(ConfigSysteme.Traduire(col.ColumnName)));
                        sw.WriteLine(string.Join(",", entetes));

                        // Écrire les lignes
                        foreach (DataRow row in dt.Rows)
                        {
                            var valeurs = dt.Columns.Cast<DataColumn>()
                                .Select(col => QuoteCsv(row[col]?.ToString() ?? ""));
                            sw.WriteLine(string.Join(",", valeurs));
                        }
                    }
                }

                MessageBox.Show(ConfigSysteme.Traduire("✅ Export CSV terminé !"),
                    ConfigSysteme.Traduire("Succès"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ConfigSysteme.Traduire("Erreur lors de l'export CSV : ") + ex.Message,
                    ConfigSysteme.Traduire("Erreur"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Méthode privée pour échapper les chaînes dans CSV
        private string QuoteCsv(string s)
        {
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }


        private void btnExporterPDF_Click(object sender, EventArgs e)
        {
            int idVente = 0;
            string codeFacture = null;

            // 1) Essayer depuis le rapport
            if (dgvRapport.Rows.Count > 0)
            {
                DataGridViewRow row = dgvRapport.CurrentRow;
                if (row == null && dgvRapport.SelectedRows.Count > 0) row = dgvRapport.SelectedRows[0];
                if (row == null) row = dgvRapport.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => !r.IsNewRow);

                if (row != null)
                {
                    int.TryParse(Convert.ToString(row.Cells["ID_Vente"]?.Value), out idVente);
                    codeFacture = Convert.ToString(row.Cells["CodeFacture"]?.Value);
                }
            }

            // 2) Fallback dernière vente
            if (idVente <= 0)
            {
                idVente = _lastIdVente;
                codeFacture = _lastCodeFacture;
            }

            if (idVente <= 0)
            {
                MessageBox.Show("Aucune vente trouvée. Finalise une vente ou sélectionne-la dans le rapport.");
                return;
            }

            if (string.IsNullOrWhiteSpace(codeFacture))
                codeFacture = BuildCodeFacture(idVente);

            // ✅ demander un fichier de sortie (chemin complet)
            string chosenFilePath = DemanderCheminPdf_Fichier(codeFacture);
            if (string.IsNullOrWhiteSpace(chosenFilePath)) return;

            try
            {
                // Ta méthode accepte soit un dossier soit un filepath (tu gères déjà .EndsWith(".pdf"))
                string generatedPath = GenererPdfFactureDepuisDb(idVente, codeFacture, chosenFilePath);

                // Si ta méthode a généré ailleurs (dossier + code.pdf), on renomme vers le fichier choisi
                if (!string.Equals(generatedPath, chosenFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    if (File.Exists(chosenFilePath))
                        File.Delete(chosenFilePath);

                    File.Move(generatedPath, chosenFilePath);
                }

                OuvrirPdf(chosenFilePath);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Accès refusé. Choisis un autre dossier (Documents, D:\\, USB...).");
            }
            catch (IOException ioEx)
            {
                MessageBox.Show("Fichier bloqué/ouvert : " + ioEx.Message);
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Erreur export PDF : " + ex2.Message);
            }
        }


        private void dgvRapport_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private int AddOrIncrementProduitToPanier_ReturnRowIndex(ProduitCombo p, int qte, string codeBarre)
        {
            if (p == null) return -1;
            if (qte <= 0) qte = 1;

            EnsureProduitInCache(p);

            // 1) déjà dans panier -> incrémenter
            foreach (DataGridViewRow r in dgvPanier.Rows)
            {
                if (r.IsNewRow) continue;

                int id = 0;
                int.TryParse(r.Cells["ID_Produit"].Value?.ToString(), out id);

                if (id == p.ID)
                {
                    decimal oldQte = GetDecimalCell(r, "Quantite");
                    r.Cells["Quantite"].Value = (oldQte + qte).ToString("0.##", CultureInfo.GetCultureInfo("fr-FR"));

                    if (dgvPanier.Columns.Contains("CodeBarre"))
                        r.Cells["CodeBarre"].Value = codeBarre;

                    // ✅ Devise = produit (et injection Items)
                    if (HasCol(r, "Devise"))
                        SetDeviseOnRow(r, p.Devise);

                    DgvPanier_CellEndEdit(dgvPanier,
                        new DataGridViewCellEventArgs(dgvPanier.Columns["Quantite"].Index, r.Index));

                    MettreAJourTotaux();
                    return r.Index;
                }
            }

            // 2) nouvelle ligne
            int idx = dgvPanier.Rows.Add();
            var row = dgvPanier.Rows[idx];

            EnsureProduitIdInCache(p.ID);

            row.Cells["NomProduit"].Value = p.ID;
            row.Cells["ID_Produit"].Value = p.ID;
            row.Cells["RefProduit"].Value = p.Ref;

            if (dgvPanier.Columns.Contains("CodeBarre"))
                row.Cells["CodeBarre"].Value = codeBarre;

            row.Cells["Quantite"].Value = qte;
            row.Cells["PrixUnitaire"].Value = p.Prix; // ✅ decimal (pas string)
            row.Cells["Remise"].Value = 0;
            row.Cells["TVA"].Value = 0;

            row.Cells["Categorie"].Value = p.Categorie;
            row.Cells["Taille"].Value = p.Taille;
            row.Cells["Couleur"].Value = p.Couleur;

            // ✅ Devise = produit
            if (HasCol(row, "Devise"))
                SetDeviseOnRow(row, p.Devise);

            DgvPanier_CellEndEdit(dgvPanier,
                new DataGridViewCellEventArgs(dgvPanier.Columns["Quantite"].Index, idx));

            MettreAJourTotaux();
            return idx;
        }

        private void btnInventaireDuJour_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    string requete = @"
SELECT 
    d.NomProduit,
    d.Devise,
    SUM(d.Quantite) AS QuantiteTotale,
    SUM(d.Montant) AS MontantTotal
FROM [dbo].[DetailsVente] d
INNER JOIN [dbo].[Vente] v ON d.ID_Vente = v.ID_Vente
WHERE CAST(v.DateVente AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY d.NomProduit, d.Devise
ORDER BY d.Devise, d.NomProduit";

                    DataTable dt = new DataTable();

                    using (SqlCommand cmd = new SqlCommand(requete, con))
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    // ✅ Si aucune vente aujourd'hui
                    if (dt.Rows.Count == 0)
                    {
                        // IMPORTANT : supprimer le schéma créé par Fill (sinon "Devise" existe déjà)
                        dt.Clear();
                        dt.Columns.Clear();

                        dt.Columns.Add("Produit", typeof(string));
                        dt.Columns.Add("Devise", typeof(string));
                        dt.Columns.Add("Quantité", typeof(int));
                        dt.Columns.Add("Montant", typeof(decimal));

                        dt.Rows.Add("Aucun produit vendu", "-", 0, 0m);
                    }
                    else
                    {
                        // ✅ Renommer colonnes pour affichage
                        if (dt.Columns.Contains("NomProduit")) dt.Columns["NomProduit"].ColumnName = "Produit";
                        if (dt.Columns.Contains("Devise")) dt.Columns["Devise"].ColumnName = "Devise";
                        if (dt.Columns.Contains("QuantiteTotale")) dt.Columns["QuantiteTotale"].ColumnName = "Quantité";
                        if (dt.Columns.Contains("MontantTotal")) dt.Columns["MontantTotal"].ColumnName = "Montant";
                    }

                    // Affichage dans DataGridView
                    dgvRapport.DataSource = dt;
                    dgvRapport.ReadOnly = true;
                    dgvRapport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvRapport.ClearSelection();

                    // Sauvegarde PDF
                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "Fichier PDF (*.pdf)|*.pdf",
                        Title = "Enregistrer l'inventaire du jour",
                        FileName = "InventaireDuJour_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExporterDataTableEnPdf(dt, saveDialog.FileName, "Inventaire du jour - " + DateTime.Now.ToString("dd/MM/yyyy"));
                        MessageBox.Show("Inventaire exporté avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'exportation de l'inventaire :\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetSqlRapport(string typeRapport)
        {
            switch (typeRapport)
            {
                case "Hebdomadaire": return SQL_HEBDO;
                case "Mensuel": return SQL_MENSUEL;
                case "Annuel": return SQL_ANNUEL;
                case "Par Caissier": return SQL_CAISSIER;
                default: throw new Exception("Type rapport non pris en charge.");
            }
        }

        private void btnGenererRapport_Click(object sender, EventArgs e)
        {
            if (cmbTypeRapport.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner un type de rapport.");
                return;
            }

            string typeRapport = cmbTypeRapport.SelectedItem.ToString();
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;

                        switch (typeRapport)
                        {
                            case "Hebdomadaire":
                                cmd.CommandText = @"
SELECT ID_Vente, DateVente, NomCaissier, MontantTotal, Devise
FROM dbo.Vente
WHERE DateVente >= DATEADD(day, -7, CONVERT(date, GETDATE()))
ORDER BY DateVente DESC;";
                                break;

                            case "Mensuel":
                                cmd.CommandText = @"
SELECT ID_Vente, DateVente, NomCaissier, MontantTotal, Devise
FROM dbo.Vente
WHERE YEAR(DateVente) = YEAR(GETDATE())
  AND MONTH(DateVente) = MONTH(GETDATE())
ORDER BY DateVente DESC;";
                                break;

                            case "Annuel":
                                cmd.CommandText = @"
SELECT ID_Vente, DateVente, NomCaissier, MontantTotal, Devise
FROM dbo.Vente
WHERE YEAR(DateVente) = YEAR(GETDATE())
ORDER BY DateVente DESC;";
                                break;

                            case "Par Caissier":
                                cmd.CommandText = @"
DECLARE @d1 datetime = DATEADD(day, -30, CONVERT(date, GETDATE()));
DECLARE @d2 datetime = DATEADD(day, 1, CONVERT(date, GETDATE()));

SELECT
    LTRIM(RTRIM(ISNULL(v.NomCaissier,''))) AS Caissier,
    COUNT(DISTINCT v.ID_Vente) AS NbVentes,
    COUNT(pv.IdPaiement) AS NbPaiements,
    SUM(CASE WHEN UPPER(ISNULL(pv.Devise,''))='CDF'
              AND UPPER(ISNULL(pv.Statut,'VALIDE')) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
             THEN pv.Montant ELSE 0 END) AS EncaisseCDF,
    SUM(CASE WHEN UPPER(ISNULL(pv.Devise,''))='USD'
              AND UPPER(ISNULL(pv.Statut,'VALIDE')) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
             THEN pv.Montant ELSE 0 END) AS EncaisseUSD,
    SUM(CASE WHEN UPPER(ISNULL(pv.Devise,''))='EUR'
              AND UPPER(ISNULL(pv.Statut,'VALIDE')) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
             THEN pv.Montant ELSE 0 END) AS EncaisseEUR,
    SUM(CASE WHEN UPPER(ISNULL(v.Statut,''))='REGLEMENT_CREDIT'
              AND UPPER(ISNULL(pv.Statut,'VALIDE')) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
             THEN pv.Montant ELSE 0 END) AS EncaisseCredit_Total
FROM dbo.Vente v
LEFT JOIN dbo.PaiementsVente pv ON pv.IdVente = v.ID_Vente
WHERE v.DateVente >= @d1 AND v.DateVente < @d2
GROUP BY LTRIM(RTRIM(ISNULL(v.NomCaissier,'')))
ORDER BY EncaisseUSD DESC, EncaisseCDF DESC, EncaisseEUR DESC;";
                                break;

                            default:
                                MessageBox.Show("Type de rapport non pris en charge.");
                                return;
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    dt.Columns.Clear();
                    dt.Columns.Add("Message", typeof(string));
                    dt.Rows.Add("Aucune donnée trouvée pour ce rapport");
                }

                // ✅ Affichage DataGridView
                dgvRapport.DataSource = dt;
                dgvRapport.ReadOnly = true;
                dgvRapport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvRapport.ClearSelection();

                // ✅ IMPORTANT : changer seulement le TITRE affiché (pas ColumnName)
                ApplyRapportHeaders(typeRapport);

                // ✅ Export PDF (si tu veux)
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Fichier PDF|*.pdf",
                    Title = "Enregistrer le rapport " + typeRapport,
                    FileName = "Rapport_" + typeRapport + "_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExporterDataTableEnPdf(dt, saveDialog.FileName, "Rapport " + typeRapport + " du " + DateTime.Now.ToString("dd/MM/yyyy"));
                    MessageBox.Show("Rapport exporté avec succès !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du rapport : " + ex.Message);
            }
        }

        public int CurrentIdVente
        {
            get
            {
                if (dgvRapport == null || dgvRapport.CurrentRow == null) return 0;

                if (dgvRapport.Columns.Contains("ID_Vente") && dgvRapport.CurrentRow.Cells["ID_Vente"].Value != null)
                    return Convert.ToInt32(dgvRapport.CurrentRow.Cells["ID_Vente"].Value);

                if (dgvRapport.Columns.Contains("IdVente") && dgvRapport.CurrentRow.Cells["IdVente"].Value != null)
                    return Convert.ToInt32(dgvRapport.CurrentRow.Cells["IdVente"].Value);

                return 0;
            }
        }

        private void ApplyRapportHeaders(string typeRapport)
        {
            if (dgvRapport == null) return;

            if (typeRapport == "Par Caissier")
            {
                if (dgvRapport.Columns["Caissier"] != null) dgvRapport.Columns["Caissier"].HeaderText = "Caissier";
                if (dgvRapport.Columns["NbVentes"] != null) dgvRapport.Columns["NbVentes"].HeaderText = "Nb ventes";
                if (dgvRapport.Columns["NbPaiements"] != null) dgvRapport.Columns["NbPaiements"].HeaderText = "Nb paiements";
                if (dgvRapport.Columns["EncaisseCDF"] != null) dgvRapport.Columns["EncaisseCDF"].HeaderText = "Encaissé (CDF)";
                if (dgvRapport.Columns["EncaisseUSD"] != null) dgvRapport.Columns["EncaisseUSD"].HeaderText = "Encaissé (USD)";
                if (dgvRapport.Columns["EncaisseEUR"] != null) dgvRapport.Columns["EncaisseEUR"].HeaderText = "Encaissé (EUR)";
                if (dgvRapport.Columns["EncaisseCredit_Total"] != null) dgvRapport.Columns["EncaisseCredit_Total"].HeaderText = "Crédit encaissé";
            }
            else
            {
                if (dgvRapport.Columns["ID_Vente"] != null) dgvRapport.Columns["ID_Vente"].HeaderText = "ID Vente";
                if (dgvRapport.Columns["DateVente"] != null) dgvRapport.Columns["DateVente"].HeaderText = "Date de vente";
                if (dgvRapport.Columns["NomCaissier"] != null) dgvRapport.Columns["NomCaissier"].HeaderText = "Caissier";
                if (dgvRapport.Columns["MontantTotal"] != null) dgvRapport.Columns["MontantTotal"].HeaderText = "Montant";
                if (dgvRapport.Columns["Devise"] != null) dgvRapport.Columns["Devise"].HeaderText = "Devise";
            }
        }







        private void btnTesterConnexion_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();
                    MessageBox.Show(ConfigSysteme.Traduire("ConnexionSQLReussie"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ConfigSysteme.Traduire("ConnexionECHEC") + " : " + ex.Message);
            }
        }


        private void BtnTotalDuJour_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ Normalisation du nom caissier
                string caissier = (txtNomCaissier.Text ?? "").Trim();
                caissier = string.Join(" ", caissier.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(caissier)) caissier = "CAISSIER";

                var res = ObtenirTotauxEncaissementsDuJour(caissier);

                string msg =
                    $"Encaissements du jour pour : {caissier}\n\n" +
                    $"CDF : {res.TotalCDF:N2} (Crédit: {res.CreditCDF:N2})\n" +
                    $"USD : {res.TotalUSD:N2} (Crédit: {res.CreditUSD:N2})\n" +
                    $"EUR : {res.TotalEUR:N2} (Crédit: {res.CreditEUR:N2})";

                MessageBox.Show(msg, "Total encaissements (PaiementsVente)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private (decimal TotalCDF, decimal TotalUSD, decimal TotalEUR, decimal CreditCDF, decimal CreditUSD, decimal CreditEUR)
    ObtenirTotauxEncaissementsDuJour(string caissier)
        {
            decimal tCDF = 0m, tUSD = 0m, tEUR = 0m;
            decimal cCDF = 0m, cUSD = 0m, cEUR = 0m;

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            using (var cmd = new SqlCommand(@"
DECLARE @d1 datetime = CONVERT(date, GETDATE());
DECLARE @d2 datetime = DATEADD(day, 1, @d1);

SELECT
    UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF')))) AS Devise,
    SUM(CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END) AS TotalEncaisse,
    SUM(CASE
            WHEN UPPER(LTRIM(RTRIM(ISNULL(v.Statut,'')))) = 'REGLEMENT_CREDIT'
             AND UPPER(LTRIM(RTRIM(ISNULL(pv.Statut,'VALIDE')))) NOT IN ('ANNULE','ANNULÉ','ANNULEE','ANNULÉE')
            THEN ISNULL(pv.Montant,0) ELSE 0
        END) AS TotalReglementCredit
FROM dbo.PaiementsVente pv
JOIN dbo.Vente v ON v.ID_Vente = pv.IdVente
WHERE pv.DatePaiement >= @d1 AND pv.DatePaiement < @d2
  AND LTRIM(RTRIM(ISNULL(v.NomCaissier,''))) = LTRIM(RTRIM(@caissier))
GROUP BY UPPER(LTRIM(RTRIM(ISNULL(pv.Devise,'CDF'))));", con))
            {
                cmd.Parameters.Add("@caissier", SqlDbType.NVarChar, 120).Value = caissier;

                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string dev = (rd["Devise"]?.ToString() ?? "CDF").Trim().ToUpperInvariant();
                        decimal tot = Convert.ToDecimal(rd["TotalEncaisse"]);
                        decimal cred = Convert.ToDecimal(rd["TotalReglementCredit"]);

                        if (dev == "CDF") { tCDF += tot; cCDF += cred; }
                        else if (dev == "USD") { tUSD += tot; cUSD += cred; }
                        else if (dev == "EUR") { tEUR += tot; cEUR += cred; }
                    }
                }
            }

            return (tCDF, tUSD, tEUR, cCDF, cUSD, cEUR);
        }

        private void txtPrenomClient_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnImprimerTicket_Click(object sender, EventArgs e)
        {
            int idVente = 0;

            if (dgvRapport.CurrentRow != null)
                idVente = Convert.ToInt32(dgvRapport.CurrentRow.Cells["ID_Vente"].Value);

            if (idVente <= 0) idVente = _lastIdVente;

            if (idVente <= 0)
            {
                MessageBox.Show("Aucune vente trouvée.");
                return;
            }

            try
            {
                _printModel = ChargerVentePourImpression(idVente);
                _printLineIndex = 0;

                LoadFideliteForClient(_printModel.IdClient);

                string printer = GetTicketPrinterName_Auto();
                if (!string.IsNullOrWhiteSpace(printer))
                    ConfigSysteme.ImprimanteTicketNom = printer;

                PrintDocument pd = new PrintDocument();

                if (!string.IsNullOrWhiteSpace(printer))
                    pd.PrinterSettings.PrinterName = printer;

                pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket80mm", 315, 2500);
                pd.DefaultPageSettings.Margins = new Margins(5, 5, 5, 5);

                pd.PrintPage += PrintPageHandlerTicket_FromModel;

                using (PrintDialog dlg = new PrintDialog { Document = pd, UseEXDialog = true })
                {
                    // ✅ l’imprimante ticket est déjà sélectionnée dans le dialog
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        // ✅ si user change, mémoriser
                        if (!string.IsNullOrWhiteSpace(pd.PrinterSettings.PrinterName))
                            ConfigSysteme.ImprimanteTicketNom = pd.PrinterSettings.PrinterName;

                        pd.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur ticket : " + ex.Message);
            }
        }

        private void PrintPageHandlerTicket_FromModel(object sender, PrintPageEventArgs e)
        {
            if (_printModel == null) { e.HasMorePages = false; return; }

            bool hasMore = RenderFacture(e.Graphics, e.MarginBounds, isTicket: true);
            e.HasMorePages = hasMore;

            if (!hasMore) _printLineIndex = 0;
        }


        private void btnDuplicataA4_Click(object sender, EventArgs e)
        {
            if (dgvRapport.CurrentRow == null && _lastIdVente <= 0)
            {
                MessageBox.Show("Sélectionne une vente ou finalise une vente.");
                return;
            }

            int idVente = dgvRapport.CurrentRow != null
                ? Convert.ToInt32(dgvRapport.CurrentRow.Cells["ID_Vente"].Value)
                : _lastIdVente;

            try
            {
                _printModel = ChargerVentePourImpression(idVente);

                PrintDocument pd = new PrintDocument();
                pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);

                pd.PrintPage += PrintPageHandlerA4_FromModel;

                using (PrintDialog dlg = new PrintDialog { Document = pd })
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                        pd.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur duplicata : " + ex.Message);
            }
        }

        private void PrintPageHandlerA4_FromModel(object sender, PrintPageEventArgs e)
        {
            if (_printModel == null) { e.HasMorePages = false; return; }

            bool hasMore = RenderFacture(e.Graphics, e.MarginBounds, isTicket: false);
            e.HasMorePages = hasMore;

            if (!hasMore) _printLineIndex = 0;
        }

        private void AnnulerVente(int idVente, string motif, string managerValide = null)
        {
            if (idVente <= 0) throw new Exception("IdVente invalide.");
            motif = (motif ?? "").Trim();
            if (motif.Length < 5) throw new Exception("Motif trop court.");

            string user = ((SessionEmploye.Prenom + " " + SessionEmploye.Nom) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(user)) user = "SYSTEM";

            using (var con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        // 1) vérifier statut
                        string statut = "";
                        using (var cmd = new SqlCommand(
                            "SELECT ISNULL(Statut,'VALIDEE') FROM dbo.Vente WHERE ID_Vente=@id",
                            con, trans))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                            statut = (cmd.ExecuteScalar() ?? "VALIDEE").ToString();
                        }

                        if (statut.Equals("ANNULEE", StringComparison.OrdinalIgnoreCase))
                            throw new Exception("Cette vente est déjà annulée.");

                        // 2) marquer vente annulée
                        using (var cmd = new SqlCommand(@"
UPDATE dbo.Vente
SET Statut='ANNULEE',
    AnnulePar=@u,
    DateAnnulation=GETDATE(),
    MotifAnnulation=@m
WHERE ID_Vente=@id;", con, trans))
                        {
                            cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = user;
                            cmd.Parameters.Add("@m", SqlDbType.NVarChar, 250).Value = motif;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                            cmd.ExecuteNonQuery();
                        }

                        // ✅ 2bis) annuler aussi PaiementsVente
                        using (var cmd = new SqlCommand(@"
UPDATE dbo.PaiementsVente
SET Statut = 'ANNULE',
    AnnulePar = @u,
    DateAnnulation = GETDATE(),
    MotifAnnulation = @m
WHERE IdVente = @id
  AND ISNULL(Statut,'VALIDE') <> 'ANNULE';", con, trans))
                        {
                            cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = user;
                            cmd.Parameters.Add("@m", SqlDbType.NVarChar, 250).Value = motif;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                            cmd.ExecuteNonQuery();
                        }

                        // 3) charger détails vente (ton table = DetailsVente)
                        var lignes = new List<LigneStockRetour>();
                        using (var cmd = new SqlCommand(@"
SELECT ID_Produit, RefProduit, Quantite
FROM dbo.DetailsVente
WHERE ID_Vente=@id;", con, trans))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;

                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    var l = new LigneStockRetour();
                                    l.IdProduit = Convert.ToInt32(r["ID_Produit"]);
                                    l.RefProduit = (r["RefProduit"] == DBNull.Value ? "" : r["RefProduit"].ToString());
                                    l.Qte = Convert.ToInt32(r["Quantite"]);
                                    lignes.Add(l);
                                }
                            }
                        }

                        // 4) stock inverse = ENTREE
                        foreach (var l in lignes)
                        {
                            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.OperationsStock
(ID_Produit, TypeOperation, Quantite, DateOperation, Utilisateur, Motif, Reference, Emplacement, Remarques)
VALUES
(@idProduit, 'ENTREE', @qte, GETDATE(), @u, 'ANNULATION', @ref, NULL, @rem);", con, trans))
                            {
                                cmd.Parameters.Add("@idProduit", SqlDbType.Int).Value = l.IdProduit;
                                cmd.Parameters.Add("@qte", SqlDbType.Int).Value = l.Qte;
                                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = user;
                                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 50).Value = (l.RefProduit ?? "");
                                cmd.Parameters.Add("@rem", SqlDbType.NVarChar, 200).Value = "Annulation vente ID=" + idVente;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 5) log annulation
                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.AnnulationVente(IdVente, AnnulePar, Motif, ManagerValide, DateValidation)
VALUES(@id, @u, @m, @mgr, CASE WHEN @mgr IS NULL OR LTRIM(RTRIM(@mgr))='' THEN NULL ELSE GETDATE() END);", con, trans))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idVente;
                            cmd.Parameters.Add("@u", SqlDbType.NVarChar, 120).Value = user;
                            cmd.Parameters.Add("@m", SqlDbType.NVarChar, 250).Value = motif;

                            cmd.Parameters.Add("@mgr", SqlDbType.NVarChar, 120).Value =
                                string.IsNullOrWhiteSpace(managerValide) ? (object)DBNull.Value : managerValide.Trim();

                            cmd.ExecuteNonQuery();
                        }

                        // 6) audit log
                        ConfigSysteme.AjouterAuditLog("Vente",
                            "ANNULATION Vente ID=" + idVente + " | Motif=" + motif + " | Par=" + user,
                            "Succès");

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        private void btnAnnulerVente_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedVenteFromRapport(out int idVente))
                return;

            string motif = Prompt.ShowDialog("Motif d'annulation (obligatoire):", "Annulation Vente");
            if (string.IsNullOrWhiteSpace(motif)) return;

            try
            {
                AnnulerVente(idVente, motif, managerValide: null);
                MessageBox.Show("✅ Vente annulée + paiements annulés + stock réintégré.");
                btnCharger_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur annulation : " + ex.Message);
            }
        }

        private void RefreshEditingComboPreserveText(ComboBox combo, string typed, int caret)
        {
            if (combo == null) return;

            // force refresh binding
            var src = combo.DataSource;
            combo.DataSource = null;
            combo.DisplayMember = "NomProduit";
            combo.ValueMember = "ID";
            combo.DataSource = _produitsCacheDgv;

            combo.Text = typed ?? "";
            combo.SelectionStart = Math.Min(caret, combo.Text.Length);
            combo.SelectionLength = 0;
        }

        private void btnVoirPaiements_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedVenteFromRapport(out int idVente))
                return;

            string user = (txtNomCaissier.Text ?? "SYSTEM").Trim();

            using (var f = new FormPaiementsVente(idVente, user))
            {
                f.ShowDialog(this);
            }
        }

        private void btnRetour_Click(object sender, EventArgs e)
        {
            // 1) vérifier qu'une vente est sélectionnée dans ton grid de ventes
            if (_lastIdVente > 0)
            {
                // ✅ Si tu veux retourner la DERNIÈRE vente directement (cas caisse)
                int idVente = _lastIdVente;

                // ⚠️ Pas de idDetails ici car on n'a pas choisi un produit précis
                using (var f = new FormAnnulations(idVente, null))
                {
                    f.StartPosition = FormStartPosition.CenterParent;
                    f.ShowDialog(this);
                }
                return;
            }

            // 2) Sinon : si tu as un grid de ventes (ex: dgvRapport)
            if (dgvRapport.CurrentRow == null)
            {
                MessageBox.Show("Sélectionne une vente dans la liste.");
                return;
            }

            int idVenteSel = Convert.ToInt32(dgvRapport.CurrentRow.Cells["ID_Vente"].Value);

            using (var f = new FormAnnulations(idVenteSel, null))
            {
                f.StartPosition = FormStartPosition.CenterParent;
                f.ShowDialog(this);
            }
        }
    }
}

