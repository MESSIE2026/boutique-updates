using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace BoutiqueRebuildFixed
{
    public partial class FormProduits : Form
    {
        private readonly string connectionString = ConfigSysteme.ConnectionString;

        // =================== IMPRESSION ETIQUETTES A4 (AUTOCOLLANT) ===================
        private PrintDocument _labelsPrintDoc;
        private List<ProductLabelInfo> _labelsToPrint = new List<ProductLabelInfo>();
        private int _labelIndex = 0;
        private DataGridView dgvEquivalences;
        private SplitContainer splitGrids;
        private bool _askEquivalenceAfterAdd = true; // tu peux le lier à une CheckBox plus tard
        private Button btnSignatureManager;
        private bool _loadingProduits = false;
        private int _lastProduitIdLoaded = -1;
        private CheckBox chkIsReglemente;
        private CheckBox chkSignatureRequired;
        private Label lblPermission;
        private ComboBox cmbPermissionCode;
        private Label lblAgeMin;
        private NumericUpDown nudAgeMin;
        private Label lblNiveau;
        private ComboBox cmbNiveauRestriction;


        // Modèle de données minimal
        private class ProductLabelInfo
        {
            public int IdProduit { get; set; }
            public string NomProduit { get; set; }
            public string RefProduit { get; set; }
            public string CodeBarre { get; set; }
            public decimal Prix { get; set; }
            public string Devise { get; set; }
            public string ImagePath { get; set; }
        }

        private class LabelSheet
        {
            public string Name { get; set; }
            public int Cols { get; set; }
            public int Rows { get; set; }
            public int MarginLeft { get; set; } = 12;
            public int MarginRight { get; set; } = 12;
            public int MarginTop { get; set; } = 12;
            public int MarginBottom { get; set; } = 12;
            public int CellPadding { get; set; } = 6; // marge interne dans chaque etiquette
            public override string ToString() => Name;
        }

        private class ProduitInfo
        {
            public int Id { get; set; }
            public string Nom { get; set; }
            public string Ref { get; set; }
            public string CodeBarre { get; set; }
        }

        private class ProduitDeps
        {
            public int Ventes { get; set; }
            public int StockMoves { get; set; }
            public int Equivalences { get; set; }
            public int Unites { get; set; }
        }


        private LabelSheet _sheet = null;


        public FormProduits()
        {
            InitializeComponent();

            cmbCategorie.SelectedIndexChanged += cmbCategorie_SelectedIndexChanged;

            cmbTaille.DropDownStyle = ComboBoxStyle.DropDown;
            cmbTaille.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbTaille.AutoCompleteSource = AutoCompleteSource.ListItems;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;
        }

        private void FormProduits_Load(object sender, EventArgs e)
        {
            ChargerCategories();
            ChargerDevises();

            dtpDateAjout.Value = DateTime.Now;
            numQuantite.Value = 1;

            // ✅ init formats (évite aussi double event)
            InitLabelFormats();

            ConfigurerDgvProduits();
            InitEquivalencesUI_BelowProduits();

            dgvProduits.CellClick -= dgvProduits_CellClick;
            dgvProduits.CellClick += dgvProduits_CellClick;

            dgvProduits.SelectionChanged -= DgvProduits_SelectionChanged;
            dgvProduits.SelectionChanged += DgvProduits_SelectionChanged;

            btnAjouterEquivalence.Click -= btnAjouterEquivalence_Click;
            btnAjouterEquivalence.Click += btnAjouterEquivalence_Click;

            // ✅ PRO: éviter double click si designer a déjà branché
            btnGenererCodeBarre.Click -= btnGenererCodeBarre_Click;
            btnGenererCodeBarre.Click += btnGenererCodeBarre_Click;

            btnImprimerEtiquettes.Click -= btnImprimerEtiquettes_Click;
            btnImprimerEtiquettes.Click += btnImprimerEtiquettes_Click;

            btnStockInitial.Click -= btnStockInitial_Click;
            btnStockInitial.Click += btnStockInitial_Click;

            ChargerProduits();
            if (dgvProduits.Rows.Count > 0)
            {
                dgvProduits.ClearSelection();
                dgvProduits.ClearSelection();
                var r0 = dgvProduits.Rows[0];
                r0.Selected = true;

                // ✅ surtout ne pas setter CurrentCell ici
                dgvProduits.CurrentCell = null;

                int idP = Convert.ToInt32(r0.Cells["ID_Produit"].Value);
                RemplirChampsDepuisRow(r0);
                ChargerEquivalencesDuProduit(idP);
                ChargerComboEquivalents(idP);
            }

            btnSignatureManager = new Button
            {
                Name = "btnSignatureManager",
                Text = "Signature Manager",
                Width = 170,
                Height = btnSupprimer.Height,
                Left = btnSupprimer.Right + 10,   // ✅ juste après Supprimer
                Top = btnSupprimer.Top,
                Anchor = btnSupprimer.Anchor
            };

            btnSignatureManager.Click += btnSignatureManager_Click;

            // ================== Bouton Modifier Prix (Manager) ==================
            var parent = btnSupprimerEquivalence.Parent; // ✅ même zone que "Supprimer Equivalence"

            var btnModifierPrix = new Button
            {
                Name = "btnModifierPrix",
                Text = "Modifier Prix (Manager)",
                Width = 190,
                Height = btnSupprimerEquivalence.Height,
                Left = btnSupprimerEquivalence.Right + 10,   // ✅ juste après Supprimer Equivalence
                Top = btnSupprimerEquivalence.Top,
                Anchor = btnSupprimerEquivalence.Anchor
            };

            btnModifierPrix.Click += btnModifierPrix_Click;

            // ✅ important : ajouter au même parent
            parent.Controls.Add(btnModifierPrix);

            // ✅ l'amener devant
            btnModifierPrix.BringToFront();


            // ✅ important : l’ajouter au même parent que btnSupprimer
            btnSupprimer.Parent.Controls.Add(btnSignatureManager);
            btnSignatureManager.BringToFront();

            AjouterUI_Reglementation_ApresSignatureManager();

            // ✅ Charger permissions depuis dbo.Modules
            ChargerPermissionsModules();

            // ✅ Ajuster l'UI selon l'état
            SyncPermissionUi();
            // ✅ ICI
            ChargerDepotsStockInitial();

            RafraichirLangue();
            RafraichirTheme();
        }

        private ProduitInfo GetProduitInfo(int idProduit)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 ID_Produit, NomProduit, RefProduit, CodeBarre
FROM dbo.Produit
WHERE ID_Produit = @id;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                con.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new ProduitInfo
                    {
                        Id = Convert.ToInt32(rd["ID_Produit"]),
                        Nom = (rd["NomProduit"] ?? "").ToString().Trim(),
                        Ref = (rd["RefProduit"] ?? "").ToString().Trim(),
                        CodeBarre = (rd["CodeBarre"] ?? "").ToString().Trim()
                    };
                }
            }
        }

        private ProduitDeps CheckProduitDependencies(int idProduit)
        {
            var d = new ProduitDeps();

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                // 1) Ventes (ex: LigneVente)
                using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.LigneVente
WHERE ID_Produit = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                    d.Ventes = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2) Mouvements Stock (ex: StockOperation / StockMouvement)
                using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.StockOperation
WHERE ID_Produit = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                    d.StockMoves = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 3) Équivalences : liens vers/depuis
                using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.ProduitEquivalence
WHERE ID_Produit = @id OR ID_ProduitEquivalent = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                    d.Equivalences = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 4) Unités produit
                using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM dbo.ProduitUnite
WHERE ID_Produit = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                    d.Unites = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return d;
        }

        private void SoftDeleteProduit(int idProduit, ProduitInfo info)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Produit
SET IsActif = 0
WHERE ID_Produit = @id;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                con.Open();

                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                    throw new Exception("Aucune ligne modifiée (produit introuvable).");
            }

            ConfigSysteme.AjouterAuditLog(
                "Suppression Produit (Soft)",
                $"Désactivation produit ID={info.Id} | Nom={info.Nom} | Ref={info.Ref} | Code={info.CodeBarre}",
                "Succès"
            );
        }

        private void HardDeleteProduit(int idProduit, ProduitInfo info)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        // Nettoyer tables "faibles" (si elles existent chez toi)
                        // 1) Equivalences (liens vers et depuis)
                        using (var cmd = new SqlCommand(@"
DELETE FROM dbo.ProduitEquivalence
WHERE ID_Produit = @id OR ID_ProduitEquivalent = @id;", con, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                            cmd.ExecuteNonQuery();
                        }

                        // 2) Unités produit
                        using (var cmd = new SqlCommand(@"
DELETE FROM dbo.ProduitUnite
WHERE ID_Produit = @id;", con, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                            cmd.ExecuteNonQuery();
                        }

                        // 3) Enfin le produit
                        using (var cmd = new SqlCommand(@"
DELETE FROM dbo.Produit
WHERE ID_Produit = @id;", con, tx))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                throw new Exception("Suppression échouée : produit introuvable.");
                        }

                        tx.Commit();

                        ConfigSysteme.AjouterAuditLog(
                            "Suppression Produit (Hard)",
                            $"DELETE produit ID={info.Id} | Nom={info.Nom} | Ref={info.Ref} | Code={info.CodeBarre}",
                            "Succès"
                        );
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }



        private void AjouterUI_Reglementation_ApresSignatureManager()
        {
            var parent = btnSignatureManager.Parent;

            chkIsReglemente = new CheckBox
            {
                Name = "chkIsReglemente",
                Text = "Réglementé",
                AutoSize = true,
                Left = btnSignatureManager.Right + 12,
                Top = btnSignatureManager.Top + 6,
                Anchor = btnSignatureManager.Anchor
            };

            chkSignatureRequired = new CheckBox
            {
                Name = "chkSignatureRequired",
                Text = "Signature requise",
                AutoSize = true,
                Left = chkIsReglemente.Right + 14,
                Top = chkIsReglemente.Top,
                Anchor = btnSignatureManager.Anchor
            };

            lblPermission = new Label
            {
                Name = "lblPermission",
                Text = "Permission :",
                AutoSize = true,
                Left = chkSignatureRequired.Right + 40,
                Top = chkIsReglemente.Top + 2,
                Anchor = btnSignatureManager.Anchor
            };

            cmbPermissionCode = new ComboBox
            {
                Name = "cmbPermissionCode",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 220,
                Left = lblPermission.Right + 6,
                Top = btnSignatureManager.Top + 2,
                Anchor = btnSignatureManager.Anchor
            };

            lblAgeMin = new Label
            {
                Name = "lblAgeMin",
                Text = "Âge min :",
                AutoSize = true,
                Left = cmbPermissionCode.Right + 12,
                Top = lblPermission.Top,
                Anchor = btnSignatureManager.Anchor
            };

            nudAgeMin = new NumericUpDown
            {
                Name = "nudAgeMin",
                Minimum = 0,
                Maximum = 120,
                Width = 60,
                Left = lblAgeMin.Right + 6,
                Top = btnSignatureManager.Top + 2,
                Anchor = btnSignatureManager.Anchor
            };

            lblNiveau = new Label
            {
                Name = "lblNiveau",
                Text = "Niveau :",
                AutoSize = true,
                Left = nudAgeMin.Right + 10,
                Top = lblPermission.Top,
                Anchor = btnSignatureManager.Anchor
            };

            cmbNiveauRestriction = new ComboBox
            {
                Name = "cmbNiveauRestriction",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Left = lblNiveau.Right + 4,
                Top = btnSignatureManager.Top + 2,
                Anchor = btnSignatureManager.Anchor
            };

            // Exemples de niveaux (tu peux adapter)
            cmbNiveauRestriction.Items.Clear();
            cmbNiveauRestriction.Items.AddRange(new object[]
            {
        "0 - Aucun",
        "1 - Faible",
        "2 - Moyen",
        "3 - Élevé"
            });
            cmbNiveauRestriction.SelectedIndex = 0;

            // Events
            chkIsReglemente.CheckedChanged += (s, e) => SyncPermissionUi();
            chkSignatureRequired.CheckedChanged += (s, e) => SyncPermissionUi();

            parent.Controls.Add(chkIsReglemente);
            parent.Controls.Add(chkSignatureRequired);
            parent.Controls.Add(lblPermission);
            parent.Controls.Add(cmbPermissionCode);
            parent.Controls.Add(lblAgeMin);
            parent.Controls.Add(nudAgeMin);
            parent.Controls.Add(lblNiveau);
            parent.Controls.Add(cmbNiveauRestriction);

            // Au-dessus
            chkIsReglemente.BringToFront();
            chkSignatureRequired.BringToFront();
            lblPermission.BringToFront();
            cmbPermissionCode.BringToFront();
            lblAgeMin.BringToFront();
            nudAgeMin.BringToFront();
            lblNiveau.BringToFront();
            cmbNiveauRestriction.BringToFront();
        }

        private void ChargerPermissionsModules()
        {
            try
            {
                using (var con = new SqlConnection(connectionString))
                using (var da = new SqlDataAdapter(@"
SELECT CodeModule, NomModule
FROM dbo.Modules
WHERE CodeModule LIKE 'VENTE_%'
ORDER BY CodeModule;", con))
                {
                    var dt = new DataTable();
                    da.Fill(dt);

                    cmbPermissionCode.DisplayMember = "NomModule";
                    cmbPermissionCode.ValueMember = "CodeModule";
                    cmbPermissionCode.DataSource = dt;
                    cmbPermissionCode.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement permissions (Modules) : " + ex.Message);
                cmbPermissionCode.DataSource = null;
            }
        }

        private string PermissionFromNiveau(int niveau)
        {
            switch (niveau)
            {
                case 1: return "VENTE_REGLEMENTE_N1";
                case 2: return "VENTE_REGLEMENTE_N2";
                case 3: return "VENTE_REGLEMENTE_N3";
                default: return "VENTE_PRODUIT_REGLEMENTE";
            }
        }

        private void SyncPermissionUi()
        {
            bool needsPerm = (chkIsReglemente != null && chkIsReglemente.Checked)
                             && (chkSignatureRequired != null && chkSignatureRequired.Checked);

            if (cmbPermissionCode != null)
                cmbPermissionCode.Enabled = needsPerm;

            if (lblPermission != null)
                lblPermission.Enabled = needsPerm;

            // AgeMin / Niveau seulement utile si réglementé (même sans signature)
            bool reg = chkIsReglemente != null && chkIsReglemente.Checked;

            if (nudAgeMin != null) nudAgeMin.Enabled = reg;
            if (lblAgeMin != null) lblAgeMin.Enabled = reg;
            if (cmbNiveauRestriction != null) cmbNiveauRestriction.Enabled = reg;
            if (lblNiveau != null) lblNiveau.Enabled = reg;

            // Si pas besoin de permission => reset
            if (!needsPerm && cmbPermissionCode != null)
                cmbPermissionCode.SelectedIndex = -1;
        }

        private string GetPermissionCodeAuto()
        {
            bool isReg = chkIsReglemente != null && chkIsReglemente.Checked;
            bool sigReq = chkSignatureRequired != null && chkSignatureRequired.Checked;

            if (!(isReg && sigReq))
                return null; // produit normal

            // 1) Si user a choisi une permission manuellement -> respecter
            string chosen = cmbPermissionCode?.SelectedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(chosen))
                return chosen.Trim();

            // 2) Sinon -> auto via niveau
            int niveau = GetNiveauRestrictionValue();
            return PermissionFromNiveau(niveau);
        }

        private int GetNiveauRestrictionValue()
        {
            if (cmbNiveauRestriction == null || cmbNiveauRestriction.SelectedIndex < 0) return 0;

            var s = cmbNiveauRestriction.SelectedItem.ToString();
            if (!string.IsNullOrWhiteSpace(s) && char.IsDigit(s[0]))
                return int.Parse(s.Substring(0, 1));

            return 0;
        }


        private void ProposerAjoutEquivalences(int idProduit)
        {
            if (!_askEquivalenceAfterAdd) return;

            // Si tu veux demander une confirmation :
            if (MessageBox.Show("Voulez-vous ajouter des équivalences à ce produit maintenant ?",
                "Équivalences", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            while (true)
            {
                using (var f = new FrmSelectProduitEquivalent(connectionString, idProduit))
                {
                    if (f.ShowDialog() != DialogResult.OK)
                        break;

                    try
                    {
                        AjouterEquivalence(idProduit, f.SelectedProduitId, f.SelectedType, f.SelectedPriorite);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur ajout équivalence : " + ex.Message);
                    }
                }

                // Continuer à ajouter d'autres équivalences ?
                var again = MessageBox.Show("Ajouter une autre équivalence ?", "Équivalences",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (again != DialogResult.Yes)
                    break;
            }

            // Rafraîchir la grille des équivalences
            ChargerEquivalencesDuProduit(idProduit);
        }


        private void DgvProduits_SelectionChanged(object sender, EventArgs e)
        {
            if (_loadingProduits) return;
            if (dgvProduits.CurrentRow == null) return;
            if (dgvProduits.SelectedRows != null && dgvProduits.SelectedRows.Count > 1) return;

            object v = dgvProduits.CurrentRow.Cells["ID_Produit"]?.Value;
            if (v == null || v == DBNull.Value) return;

            int idProduit = Convert.ToInt32(v);
            if (idProduit == _lastProduitIdLoaded) return;
            _lastProduitIdLoaded = idProduit;

            RemplirChampsDepuisRow(dgvProduits.CurrentRow);

            // ✅ sauver scroll du panel parent (panelContenu)
            var host = this.Parent as ScrollableControl;
            int savedY = host != null ? -host.AutoScrollPosition.Y : 0;

            this.BeginInvoke(new Action(() =>
            {
                ChargerEquivalencesDuProduit(idProduit);
                ChargerComboEquivalents(idProduit);

                // ✅ restaurer scroll du panel parent
                if (host != null)
                    host.AutoScrollPosition = new Point(0, savedY);
            }));
        }


        private void InitEquivalencesUI_BelowProduits()
        {
            if (dgvProduits == null) return;
            if (dgvEquivalences != null) return; // déjà créé

            Control parent = dgvProduits.Parent;
            if (parent == null) return;

            // Sauver l'emplacement / taille actuels du dgvProduits
            var oldDock = dgvProduits.Dock;
            var oldLocation = dgvProduits.Location;
            var oldSize = dgvProduits.Size;
            var oldAnchor = dgvProduits.Anchor;

            // Créer un split horizontal qui prend la place de dgvProduits
            splitGrids = new SplitContainer
            {
                Orientation = Orientation.Horizontal,
                Dock = oldDock,                // si dgvProduits était Dock=Fill, le split sera Fill aussi
                Location = oldLocation,        // utile si Dock=None
                Size = oldSize,
                Anchor = oldAnchor,
                SplitterWidth = 6
            };

            // Créer la grille des équivalences
            dgvEquivalences = new DataGridView
            {
                Name = "dgvEquivalences",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Remplacer dgvProduits par le SplitContainer dans le parent
            parent.Controls.Add(splitGrids);
            splitGrids.BringToFront();

            parent.Controls.Remove(dgvProduits);

            // Mettre dgvProduits en haut et dgvEquivalences en bas
            dgvProduits.Dock = DockStyle.Fill;
            splitGrids.Panel1.Controls.Add(dgvProduits);
            splitGrids.Panel2.Controls.Add(dgvEquivalences);

            // ✅ Taille propre : équivalences visible mais pas énorme
            // ✅ Réglages Panel2 (équivalences)
            int panel2H = 180; // hauteur souhaitée en bas

            splitGrids.Panel1MinSize = 220;
            splitGrids.Panel2MinSize = 120;
            splitGrids.FixedPanel = FixedPanel.Panel2;     // Panel2 garde sa taille
            splitGrids.IsSplitterFixed = false;

            // ✅ Fixer la hauteur du bas
            splitGrids.SplitterDistance = Math.Max(splitGrids.Panel1MinSize, splitGrids.Height - panel2H);

            // ✅ Cacher au départ (tant qu'il n'y a pas de données)
            splitGrids.Panel2Collapsed = true;

            // ✅ Garder ~180px en bas quand on resize (si visible)
            splitGrids.Resize += (s, e) =>
            {
                if (!splitGrids.Panel2Collapsed)
                    splitGrids.SplitterDistance = Math.Max(splitGrids.Panel1MinSize, splitGrids.Height - panel2H);
            };
        }

        private string BuildCodeBarreProduit(int idProduit)
        {
            // Exemple: PRD-000123
            return "PRD-" + idProduit.ToString("D6");
        }

        private void InitLabelFormats()
        {
            cmbFormatEtiquettes.SelectedIndexChanged -= CmbFormatEtiquettes_SelectedIndexChanged;

            cmbFormatEtiquettes.Items.Clear();

            cmbFormatEtiquettes.Items.Add(new LabelSheet { Name = "A4 24 étiquettes (3x8)", Cols = 3, Rows = 8 });
            cmbFormatEtiquettes.Items.Add(new LabelSheet { Name = "A4 21 étiquettes (3x7)", Cols = 3, Rows = 7 });
            cmbFormatEtiquettes.Items.Add(new LabelSheet { Name = "A4 14 étiquettes (2x7)", Cols = 2, Rows = 7 });
            cmbFormatEtiquettes.Items.Add(new LabelSheet { Name = "A4 40 étiquettes (4x10)", Cols = 4, Rows = 10 });
            cmbFormatEtiquettes.Items.Add(new LabelSheet { Name = "A4 12 étiquettes (2x6)", Cols = 2, Rows = 6 });

            cmbFormatEtiquettes.SelectedIndexChanged += CmbFormatEtiquettes_SelectedIndexChanged;

            if (cmbFormatEtiquettes.Items.Count > 0)
                cmbFormatEtiquettes.SelectedIndex = 0;
        }

        private void CmbFormatEtiquettes_SelectedIndexChanged(object sender, EventArgs e)
        {
            _sheet = cmbFormatEtiquettes.SelectedItem as LabelSheet;
        }

        private void ChargerDepotsStockInitial()
        {
            try
            {
                cmbDepotStockInitial.Items.Clear();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT ID_Depot, NomDepot FROM dbo.Depot ORDER BY NomDepot;", con))
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            cmbDepotStockInitial.Items.Add(
                                new ComboboxItem(rd["NomDepot"].ToString(), Convert.ToInt32(rd["ID_Depot"]))
                            );
                        }
                    }
                }

                if (cmbDepotStockInitial.Items.Count > 0)
                    cmbDepotStockInitial.SelectedIndex = 0;
                else
                    MessageBox.Show("Aucun dépôt trouvé. Créez un dépôt avant de faire un stock initial.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement dépôts : " + ex.Message);
            }
        }
        private void HookPrintLabelsButton()
        {
            btnImprimerEtiquettes.Click += btnImprimerEtiquettes_Click;
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

        private void ChargerCategories()
        {
            cmbCategorie.Items.AddRange(new[]
            {
                "Vêtements","Chaussures","Accessoires","Électronique",
                "Maison","Pharmacie","Quincaillerie","Imprimerie","FoodsMarket"
            });
        }
        

        private void ChargerDevises()
        {
            cmbDevise.Items.Clear();
            cmbDevise.Items.AddRange(new[] { "USD", "CDF", "EUR", "ZAR" });
            cmbDevise.SelectedIndex = 0;
        }

        private void ConfigurerDgvProduits()
        {
            dgvProduits.AutoGenerateColumns = true;
            dgvProduits.ReadOnly = true;
            dgvProduits.AllowUserToAddRows = false;
            dgvProduits.AllowUserToDeleteRows = false;

            // Activer barres de défilement horizontale et verticale
            dgvProduits.ScrollBars = ScrollBars.Both;

            // Ne pas auto-redimensionner les colonnes, pour activer le scroll horizontal
            dgvProduits.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvProduits.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            dgvProduits.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProduits.MultiSelect = true;

            dgvProduits.RowHeadersVisible = false;

            // Empêche le retour à la ligne dans les cellules (hauteur constante)
            dgvProduits.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            // Désactive le redimensionnement manuel par l'utilisateur pour plus de contrôle
            dgvProduits.AllowUserToResizeColumns = false;
        }

        private void ChargerProduits()
        {
            int firstRow = -1;
            int firstCol = -1;
            int selectedId = 0;
            int selectedColIndex = 0;

            try
            {
                if (dgvProduits.Rows.Count > 0)
                {
                    if (dgvProduits.FirstDisplayedScrollingRowIndex >= 0)
                        firstRow = dgvProduits.FirstDisplayedScrollingRowIndex;

                    if (dgvProduits.FirstDisplayedScrollingColumnIndex >= 0)
                        firstCol = dgvProduits.FirstDisplayedScrollingColumnIndex;

                    if (dgvProduits.CurrentRow != null)
                    {
                        object v = dgvProduits.CurrentRow.Cells["ID_Produit"]?.Value;
                        if (v != null && v != DBNull.Value) selectedId = Convert.ToInt32(v);
                    }

                    if (dgvProduits.CurrentCell != null)
                        selectedColIndex = dgvProduits.CurrentCell.ColumnIndex;
                }

                _loadingProduits = true;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sql = @"
SELECT
    ID_Produit, NomProduit, RefProduit, CodeBarre, Prix, Quantite, StockInitial, StockActuel,
    Devise, Categorie, Taille, Couleur, Description, ImagePath,
    OrdonnanceObligatoire, IsReglemente, NiveauRestriction, AgeMin, SignatureManagerRequired, PermissionCode
FROM dbo.Produit
ORDER BY NomProduit;";

                    SqlDataAdapter da = new SqlDataAdapter(sql, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvProduits.DataSource = dt;
                }

                // ✅ restaurer sélection (par ID)
                if (selectedId > 0)
                {
                    foreach (DataGridViewRow r in dgvProduits.Rows)
                    {
                        if (Convert.ToInt32(r.Cells["ID_Produit"].Value) == selectedId)
                        {
                            r.Selected = true;

                            int col = Math.Max(0, Math.Min(selectedColIndex, dgvProduits.ColumnCount - 1));

                            // ✅ ne force CurrentCell QUE si le DGV est déjà focalisé
                            if (dgvProduits.Focused || dgvProduits.ContainsFocus)
                                dgvProduits.CurrentCell = r.Cells[col];
                            else
                                dgvProduits.CurrentCell = null;

                            break;
                        }
                    }
                }

                // ✅ restaurer scroll vertical + horizontal
                if (firstRow >= 0 && firstRow < dgvProduits.Rows.Count)
                    dgvProduits.FirstDisplayedScrollingRowIndex = firstRow;

                if (firstCol >= 0 && firstCol < dgvProduits.ColumnCount)
                    dgvProduits.FirstDisplayedScrollingColumnIndex = firstCol;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des produits : " + ex.Message);
            }
            finally
            {
                _loadingProduits = false;
            }
        }


        private void KeepHostScroll(Action action)
        {
            var host = this.Parent as ScrollableControl;
            if (host == null) { action(); return; }

            int savedX = -host.AutoScrollPosition.X;
            int savedY = -host.AutoScrollPosition.Y;

            host.SuspendLayout();
            action();
            host.ResumeLayout(true);

            // ✅ double BeginInvoke = après layout + après focus changes
            host.BeginInvoke(new Action(() => host.AutoScrollPosition = new Point(savedX, savedY)));
            host.BeginInvoke(new Action(() => host.AutoScrollPosition = new Point(savedX, savedY)));
        }

        private void ChargerComboEquivalents(int idProduit)
        {
            cmbEquivalent.Items.Clear();

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT p.ID_Produit, p.NomProduit
FROM dbo.ProduitEquivalence e
JOIN dbo.Produit p ON p.ID_Produit = e.ID_ProduitEquivalent
WHERE e.ID_Produit = @id AND e.Actif = 1
ORDER BY e.Priorite ASC, p.NomProduit ASC;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;

                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        cmbEquivalent.Items.Add(
                            new ComboboxItem(rd["NomProduit"].ToString(), Convert.ToInt32(rd["ID_Produit"]))
                        );
                    }
                }
            }

            cmbEquivalent.Enabled = cmbEquivalent.Items.Count > 0;
            if (cmbEquivalent.Items.Count > 0) cmbEquivalent.SelectedIndex = 0;
        }

        private void ChargerEquivalencesDuProduit(int idProduit)
        {
            if (dgvEquivalences == null) return;

            using (var con = new SqlConnection(connectionString))
            using (var da = new SqlDataAdapter(@"
SELECT 
    e.ID_Equivalence,
    e.ID_ProduitEquivalent,
    p.NomProduit,
    p.RefProduit,
    p.CodeBarre,
    p.Prix,
    p.Devise,
    e.Type,
    e.Priorite,
    e.Commentaire,
    e.Actif
FROM dbo.ProduitEquivalence e
JOIN dbo.Produit p ON p.ID_Produit = e.ID_ProduitEquivalent
WHERE e.ID_Produit = @id AND e.Actif = 1
ORDER BY e.Priorite ASC, p.NomProduit ASC;", con))
            {
                da.SelectCommand.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;

                var dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    dgvEquivalences.DataSource = null;
                    dgvEquivalences.Rows.Clear();
                    dgvEquivalences.Refresh();

                    if (splitGrids != null)
                        KeepHostScroll(() => splitGrids.Panel2Collapsed = true);

                    return;
                }

                dgvEquivalences.DataSource = dt;

                if (splitGrids != null)
                    KeepHostScroll(() => splitGrids.Panel2Collapsed = false);
            }
        }



        // ================= VALIDATION =================
        private bool ValiderProduit()
        {
            if (string.IsNullOrWhiteSpace(txtNomProduit.Text))
            {
                MessageBox.Show("Nom du produit requis");
                return false;
            }

            if (!decimal.TryParse(txtPrix.Text, out _))
            {
                MessageBox.Show("Prix invalide");
                return false;
            }

            if (cmbDevise.SelectedIndex == -1)
            {
                MessageBox.Show("Sélectionnez une devise");
                return false;
            }

            return true;
        }

        private void dgvProduits_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            RemplirChampsDepuisRow(dgvProduits.Rows[e.RowIndex]);
            
        }

        private void RemplirChampsDepuisRow(DataGridViewRow row)
        {
            if (row == null) return;

            // Helper local : lit une cellule si la colonne existe
            object Cell(string col)
            {
                if (row.DataGridView == null) return null;
                if (!row.DataGridView.Columns.Contains(col)) return null;
                return row.Cells[col]?.Value;
            }

            string S(string col) => (Cell(col) == null || Cell(col) == DBNull.Value) ? "" : Cell(col).ToString();
            int I(string col, int def = 0)
            {
                var v = Cell(col);
                if (v == null || v == DBNull.Value) return def;
                return int.TryParse(v.ToString(), out int n) ? n : def;
            }
            bool B(string col, bool def = false)
            {
                var v = Cell(col);
                if (v == null || v == DBNull.Value) return def;
                try { return Convert.ToBoolean(v); } catch { return def; }
            }

            // Champs simples
            txtIDProduit.Text = S("ID_Produit");
            txtNomProduit.Text = S("NomProduit");
            txtReference.Text = S("RefProduit");
            cmbCategorie.Text = S("Categorie");
            cmbTaille.Text = S("Taille");
            txtCouleur.Text = S("Couleur");
            txtPrix.Text = S("Prix");
            rtbDescription.Text = S("Description");
            picProduit.ImageLocation = S("ImagePath");
            txtCodeBarreProduit.Text = S("CodeBarre");

            // ✅ Réglementation
            bool isReg = B("IsReglemente");
            bool sigReq = B("SignatureManagerRequired");

            if (chkIsReglemente != null) chkIsReglemente.Checked = isReg;
            if (chkSignatureRequired != null) chkSignatureRequired.Checked = sigReq;

            int age = I("AgeMin", 0);
            if (nudAgeMin != null)
                nudAgeMin.Value = Math.Max(nudAgeMin.Minimum, Math.Min(nudAgeMin.Maximum, age));

            int niv = I("NiveauRestriction", 0);
            if (cmbNiveauRestriction != null)
                cmbNiveauRestriction.SelectedIndex = Math.Max(0, Math.Min(3, niv));

            // ✅ PermissionCode (IMPORTANT : ne jamais mettre SelectedValue = null)
            string perm = S("PermissionCode").Trim();
            if (cmbPermissionCode != null)
            {
                if (string.IsNullOrWhiteSpace(perm))
                {
                    cmbPermissionCode.SelectedIndex = -1;   // ✅ safe
                }
                else
                {
                    // ✅ safe : si la valeur n'existe pas dans la liste, on remet -1
                    try
                    {
                        cmbPermissionCode.SelectedValue = perm;
                        if (cmbPermissionCode.SelectedIndex < 0)
                            cmbPermissionCode.SelectedIndex = -1;
                    }
                    catch
                    {
                        cmbPermissionCode.SelectedIndex = -1;
                    }
                }
            }

            SyncPermissionUi();
        }

        private void AjouterEquivalence(int idProduit, int idEquivalent, string type, int priorite)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                using (var cmd = new SqlCommand(@"
IF EXISTS(
    SELECT 1
    FROM dbo.ProduitEquivalence
    WHERE ID_Produit = @p
      AND ID_ProduitEquivalent = @eq
)
BEGIN
    UPDATE dbo.ProduitEquivalence
    SET Actif = 1,
        Type = @t,
        Priorite = @prio,
        CreePar = @user
    WHERE ID_Produit = @p
      AND ID_ProduitEquivalent = @eq;
END
ELSE
BEGIN
    INSERT INTO dbo.ProduitEquivalence
    (ID_Produit, ID_ProduitEquivalent, Type, Priorite, Actif, CreePar)
    VALUES
    (@p, @eq, @t, @prio, 1, @user);
END
", con))
                {
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                    cmd.Parameters.Add("@eq", SqlDbType.Int).Value = idEquivalent;
                    cmd.Parameters.Add("@t", SqlDbType.NVarChar, 30).Value = (type ?? "EQUIVALENT").Trim();
                    cmd.Parameters.Add("@prio", SqlDbType.Int).Value = priorite;

                    string user = (SessionEmploye.Nom + " " + SessionEmploye.Prenom).Trim();
                    if (string.IsNullOrWhiteSpace(user)) user = Environment.UserName;
                    cmd.Parameters.Add("@user", SqlDbType.NVarChar, 100).Value = user;

                    if (idProduit == idEquivalent)
                    {
                        MessageBox.Show("Un produit ne peut pas être équivalent à lui-même.");
                        return;
                    }

                    cmd.ExecuteNonQuery(); // ✅ une seule fois
                }
            }

            ChargerEquivalencesDuProduit(idProduit);
        }

        private void DesactiverEquivalence(int idEquivalence, int idProduit)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                "UPDATE dbo.ProduitEquivalence SET Actif=0 WHERE ID_Equivalence=@id;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEquivalence;
                con.Open();
                cmd.ExecuteNonQuery();
            }

            ChargerEquivalencesDuProduit(idProduit);
        }


        // ✅ 1) Lire le produit sélectionné dans dgvProduits
        private bool TryGetSelectedProduit(out int idProduit)
        {
            idProduit = 0;

            DataGridViewRow row = null;

            if (dgvProduits.SelectedRows != null && dgvProduits.SelectedRows.Count > 0)
                row = dgvProduits.SelectedRows[0];
            else if (dgvProduits.CurrentRow != null)
                row = dgvProduits.CurrentRow;

            if (row == null)
            {
                MessageBox.Show("Sélectionnez un produit dans la liste.");
                return false;
            }

            object vId = row.Cells["ID_Produit"]?.Value;
            if (vId == null || vId == DBNull.Value || !int.TryParse(vId.ToString(), out idProduit) || idProduit <= 0)
            {
                MessageBox.Show("ID produit invalide (ligne sélectionnée).");
                return false;
            }

            return true;
        }

        private int GetUniteBaseIdForProduit(SqlConnection con, int idProduit)
        {
            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 pu.ID_Unite
FROM dbo.ProduitUnite pu
WHERE pu.ID_Produit = @p AND pu.IsBase = 1;", con))
            {
                cmd.Parameters.Add("@p", SqlDbType.Int).Value = idProduit;
                object v = cmd.ExecuteScalar();
                if (v == null || v == DBNull.Value)
                    throw new Exception("Unité de base introuvable pour ce produit (ProduitUnite.IsBase=1).");

                return Convert.ToInt32(v);
            }
        }

        // ✅ 3) (Optionnel PRO) Mettre à jour le textbox depuis la grille sélectionnée
        private void RemplirTxtCodeBarreDepuisSelection()
        {
            if (dgvProduits.CurrentRow == null) return;

            object v = dgvProduits.CurrentRow.Cells["CodeBarre"]?.Value;
            txtCodeBarreProduit.Text = (v == null || v == DBNull.Value) ? "" : v.ToString();
        }

        // ================= AJOUT =================
        private void AjouterProduit()
        {
            if (!ValiderProduit()) return;

            try
            {
                // ✅ état réglementation/permission
                bool isReg = chkIsReglemente != null && chkIsReglemente.Checked;
                bool sigReq = chkSignatureRequired != null && chkSignatureRequired.Checked;

                string perm = GetPermissionCodeAuto();
                int niveau = isReg ? GetNiveauRestrictionValue() : 0;
                int ageMin = isReg ? (int)(nudAgeMin?.Value ?? 0) : 0;

                // Ordonnance (si tu n’as pas encore le checkbox, reste false)
                bool ordonnance = false;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    int qte = 0;

                    string cb = (txtScanCode.Text ?? "").Trim();
                    bool userProvidedCode = !string.IsNullOrWhiteSpace(cb);

                    // ✅ unicité si user scanne un code
                    if (userProvidedCode)
                    {
                        using (var chk = new SqlCommand("SELECT COUNT(1) FROM dbo.Produit WHERE CodeBarreTrim = LTRIM(RTRIM(@c));", con))
                        {
                            chk.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = cb;
                            int exists = Convert.ToInt32(chk.ExecuteScalar());
                            if (exists > 0)
                            {
                                MessageBox.Show("Ce CodeBarre existe déjà. Scannez un autre code ou laissez vide pour auto-générer.");
                                return;
                            }
                        }
                    }

                    string sql = @"
INSERT INTO dbo.Produit
(NomProduit, Prix, Quantite, StockInitial, StockActuel, Devise,
 Categorie, Taille, Couleur, Description, CodeBarre, ImagePath, RefProduit,
 OrdonnanceObligatoire, IsReglemente, NiveauRestriction, AgeMin, SignatureManagerRequired, PermissionCode)
OUTPUT INSERTED.ID_Produit
VALUES
(@NomProduit,@Prix,@Quantite,@StockInitial,@StockActuel,@Devise,
 @Categorie,@Taille,@Couleur,@Description,@CodeBarre,@ImagePath,@RefProduit,
 @OrdonnanceObligatoire, @IsReglemente, @NiveauRestriction, @AgeMin, @SignatureManagerRequired, @PermissionCode);";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@NomProduit", SqlDbType.NVarChar, 200).Value = (txtNomProduit.Text ?? "").Trim();

                        var pPrix = cmd.Parameters.Add("@Prix", SqlDbType.Decimal);
                        pPrix.Precision = 18;
                        pPrix.Scale = 2;
                        pPrix.Value = decimal.Parse(txtPrix.Text);

                        cmd.Parameters.Add("@Quantite", SqlDbType.Int).Value = qte;
                        cmd.Parameters.Add("@StockInitial", SqlDbType.Int).Value = qte;
                        cmd.Parameters.Add("@StockActuel", SqlDbType.Int).Value = qte;

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = cmbDevise.Text;
                        cmd.Parameters.Add("@Categorie", SqlDbType.NVarChar, 80).Value = cmbCategorie.Text;
                        cmd.Parameters.Add("@Taille", SqlDbType.NVarChar, 40).Value = cmbTaille.Text;
                        cmd.Parameters.Add("@Couleur", SqlDbType.NVarChar, 80).Value = (txtCouleur.Text ?? "").Trim();
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (rtbDescription.Text ?? "").Trim();

                        cmd.Parameters.Add("@CodeBarre", SqlDbType.NVarChar, 50).Value =
                            userProvidedCode ? (object)cb : DBNull.Value;

                        cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 400).Value = (picProduit.ImageLocation ?? "").Trim();
                        cmd.Parameters.Add("@RefProduit", SqlDbType.NVarChar, 100).Value = (txtReference.Text ?? "").Trim();

                        // ✅ réglementation
                        cmd.Parameters.Add("@OrdonnanceObligatoire", SqlDbType.Bit).Value = ordonnance;
                        cmd.Parameters.Add("@IsReglemente", SqlDbType.Bit).Value = isReg;
                        cmd.Parameters.Add("@NiveauRestriction", SqlDbType.Int).Value = niveau;
                        cmd.Parameters.Add("@AgeMin", SqlDbType.Int).Value = ageMin;
                        cmd.Parameters.Add("@SignatureManagerRequired", SqlDbType.Bit).Value = sigReq;
                        cmd.Parameters.Add("@PermissionCode", SqlDbType.NVarChar, 80).Value = (object)perm ?? DBNull.Value;

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());

                        // ✅ Auto-générer si vide
                        if (!userProvidedCode)
                        {
                            cb = BuildCodeBarreProduit(newId);

                            using (SqlCommand cmd2 = new SqlCommand(
                                "UPDATE dbo.Produit SET CodeBarre=@c WHERE ID_Produit=@id;", con))
                            {
                                cmd2.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = cb;
                                cmd2.Parameters.Add("@id", SqlDbType.Int).Value = newId;
                                cmd2.ExecuteNonQuery();
                            }
                        }

                        txtIDProduit.Text = newId.ToString();
                        txtCodeBarreProduit.Text = cb;

                        ProposerAjoutEquivalences(newId);

                        ConfigSysteme.AjouterAuditLog(
                            "Ajout Produit",
                            $"Produit ajouté: ID={newId} | Nom={txtNomProduit.Text.Trim()} | Ref={txtReference.Text.Trim()} | CodeBarre={cb} | Reg={isReg} | Niv={niveau} | AgeMin={ageMin} | Perm={perm}",
                            "Succès"
                        );
                    }
                }

                ChargerProduits();
                ReinitialiserFormulaire();
                MessageBox.Show("Produit ajouté avec succès ✅");
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Ajout Produit", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL ajout produit : " + ex.Message);
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Ajout Produit", ex.Message, "Échec");
                MessageBox.Show("Erreur ajout produit : " + ex.Message);
            }
        }

        // ================= MODIFICATION =================
        private void ModifierProduit()
        {
            if (string.IsNullOrWhiteSpace(txtIDProduit.Text))
            {
                MessageBox.Show("Aucun produit sélectionné");
                return;
            }

            try
            {
                // ✅ état réglementation/permission
                bool isReg = chkIsReglemente != null && chkIsReglemente.Checked;
                bool sigReq = chkSignatureRequired != null && chkSignatureRequired.Checked;

                string perm = GetPermissionCodeAuto();

                object niv = isReg ? (object)GetNiveauRestrictionValue() : DBNull.Value;
                object age = isReg ? (object)(int)(nudAgeMin?.Value ?? 0) : DBNull.Value;

                // Ordonnance (si tu n’as pas encore le checkbox, reste false)
                bool ordonnance = false;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sql = @"
UPDATE dbo.Produit SET
    NomProduit=@NomProduit,
    Prix=@Prix,
    Devise=@Devise,
    Categorie=@Categorie,
    Taille=@Taille,
    Couleur=@Couleur,
    Description=@Description,
    StockActuel=@StockActuel,
    Quantite=@Quantite,
    ImagePath=@ImagePath,
    RefProduit=@RefProduit,
    OrdonnanceObligatoire=@OrdonnanceObligatoire,
    IsReglemente=@IsReglemente,
    NiveauRestriction=@NiveauRestriction,
    AgeMin=@AgeMin,
    SignatureManagerRequired=@SignatureManagerRequired,
    PermissionCode=@PermissionCode
WHERE ID_Produit=@ID;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = int.Parse(txtIDProduit.Text);
                        cmd.Parameters.Add("@NomProduit", SqlDbType.NVarChar, 200).Value = (txtNomProduit.Text ?? "").Trim();

                        var pPrix = cmd.Parameters.Add("@Prix", SqlDbType.Decimal);
                        pPrix.Precision = 18;
                        pPrix.Scale = 2;
                        pPrix.Value = decimal.Parse(txtPrix.Text);

                        cmd.Parameters.Add("@Devise", SqlDbType.NVarChar, 10).Value = cmbDevise.Text;
                        cmd.Parameters.Add("@Categorie", SqlDbType.NVarChar, 80).Value = cmbCategorie.Text;
                        cmd.Parameters.Add("@Taille", SqlDbType.NVarChar, 40).Value = cmbTaille.Text;
                        cmd.Parameters.Add("@Couleur", SqlDbType.NVarChar, 80).Value = (txtCouleur.Text ?? "").Trim();
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (rtbDescription.Text ?? "").Trim();

                        cmd.Parameters.Add("@StockActuel", SqlDbType.Int).Value = (int)numQuantite.Value;
                        cmd.Parameters.Add("@Quantite", SqlDbType.Int).Value = (int)numQuantite.Value;

                        cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 400).Value = (picProduit.ImageLocation ?? "").Trim();
                        cmd.Parameters.Add("@RefProduit", SqlDbType.NVarChar, 100).Value = (txtReference.Text ?? "").Trim();

                        // ✅ NOUVEAU: réglementation/permission
                        cmd.Parameters.Add("@OrdonnanceObligatoire", SqlDbType.Bit).Value = ordonnance;
                        cmd.Parameters.Add("@IsReglemente", SqlDbType.Bit).Value = isReg;
                        cmd.Parameters.Add("@NiveauRestriction", SqlDbType.Int).Value = niv;
                        cmd.Parameters.Add("@AgeMin", SqlDbType.Int).Value = age;
                        cmd.Parameters.Add("@SignatureManagerRequired", SqlDbType.Bit).Value = sigReq;
                        cmd.Parameters.Add("@PermissionCode", SqlDbType.NVarChar, 80).Value = (object)perm ?? DBNull.Value;

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ChargerProduits();
                MessageBox.Show("Produit modifié ✏️");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL modification produit : " + ex.Message);
                ConfigSysteme.AjouterAuditLog("Modification Produit", ex.Message, "Échec");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification produit : " + ex.Message);
                ConfigSysteme.AjouterAuditLog("Modification Produit", ex.Message, "Échec");
            }
        }

        // ================= SUPPRESSION =================
        private void SupprimerProduit()
        {
            if (string.IsNullOrWhiteSpace(txtIDProduit.Text)) return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Produit WHERE ID_Produit=@ID", con);

                cmd.Parameters.AddWithValue("@ID", int.Parse(txtIDProduit.Text));
                con.Open();
                cmd.ExecuteNonQuery();
            }

            ChargerProduits();
            ReinitialiserFormulaire();
            MessageBox.Show("Produit supprimé ❌");
        }

        // ================= SCAN =================
        private void txtScanCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ScannerProduitParCode(txtScanCode.Text.Trim());
                txtScanCode.Clear();
            }
        }

        private void ScannerProduitParCode(string codeBarre)
        {
            codeBarre = (codeBarre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codeBarre))
            {
                MessageBox.Show("Scannez un code barre.");
                txtScanCode.Focus();
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // 1) contrôle doublons
                using (var cntCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Produit WHERE LTRIM(RTRIM(CodeBarre)) = LTRIM(RTRIM(@c));", con))
                {
                    cntCmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = codeBarre;
                    int cnt = Convert.ToInt32(cntCmd.ExecuteScalar());

                    if (cnt == 0)
                    {
                        MessageBox.Show("Aucun produit trouvé pour ce code barre.");
                        txtScanCode.Focus();
                        return;
                    }
                    if (cnt > 1)
                    {
                        MessageBox.Show("⚠️ Code barre dupliqué en base. Corrige dbo.Produit.CodeBarre (doit être unique).");
                        txtScanCode.Focus();
                        return;
                    }
                }

                // 2) lecture unique (✅ inclut réglementé + permission)
                using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1
    ID_Produit, NomProduit, RefProduit, CodeBarre, Prix, StockActuel,
    Devise, Categorie, Taille, Couleur, Description, ImagePath,
    OrdonnanceObligatoire, IsReglemente, NiveauRestriction, AgeMin, SignatureManagerRequired, PermissionCode
FROM dbo.Produit
WHERE LTRIM(RTRIM(CodeBarre)) = LTRIM(RTRIM(@Code));", con))
                {
                    cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = codeBarre;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            MessageBox.Show("Aucun produit trouvé pour ce code barre.");
                            txtScanCode.Focus();
                            return;
                        }

                        txtIDProduit.Text = dr["ID_Produit"]?.ToString() ?? "";
                        txtNomProduit.Text = dr["NomProduit"]?.ToString() ?? "";
                        txtReference.Text = dr["RefProduit"]?.ToString() ?? "";
                        cmbCategorie.Text = dr["Categorie"]?.ToString() ?? "";
                        cmbTaille.Text = dr["Taille"]?.ToString() ?? "";
                        txtCouleur.Text = dr["Couleur"]?.ToString() ?? "";
                        txtPrix.Text = dr["Prix"]?.ToString() ?? "";
                        rtbDescription.Text = dr["Description"]?.ToString() ?? "";
                        picProduit.ImageLocation = dr["ImagePath"]?.ToString() ?? "";

                        int stock = 0;
                        int.TryParse((dr["StockActuel"] ?? "0").ToString(), out stock);
                        numQuantite.Value = stock;

                        txtCodeBarreProduit.Text = dr["CodeBarre"]?.ToString() ?? "";

                        // ✅ NOUVEAU: remplir UI réglementation
                        bool isReg = dr["IsReglemente"] != DBNull.Value && Convert.ToBoolean(dr["IsReglemente"]);
                        bool sigReq = dr["SignatureManagerRequired"] != DBNull.Value && Convert.ToBoolean(dr["SignatureManagerRequired"]);

                        if (chkIsReglemente != null) chkIsReglemente.Checked = isReg;
                        if (chkSignatureRequired != null) chkSignatureRequired.Checked = sigReq;

                        int age = dr["AgeMin"] == DBNull.Value ? 0 : Convert.ToInt32(dr["AgeMin"]);
                        if (nudAgeMin != null)
                            nudAgeMin.Value = Math.Max(nudAgeMin.Minimum, Math.Min(nudAgeMin.Maximum, age));

                        int niv = dr["NiveauRestriction"] == DBNull.Value ? 0 : Convert.ToInt32(dr["NiveauRestriction"]);
                        if (cmbNiveauRestriction != null)
                            cmbNiveauRestriction.SelectedIndex = Math.Max(0, Math.Min(3, niv));

                        string perm = dr["PermissionCode"] == DBNull.Value ? null : dr["PermissionCode"].ToString();
                        if (cmbPermissionCode != null)
                        {
                            cmbPermissionCode.SelectedValue = perm;
                            if (cmbPermissionCode.SelectedIndex < 0) cmbPermissionCode.SelectedIndex = -1;
                        }

                        SyncPermissionUi();
                    }
                }
            }
        }

        // ================= UI =================
        private void ReinitialiserFormulaire()
        {
            txtIDProduit.Clear();
            txtNomProduit.Clear();
            txtPrix.Clear();
            txtCouleur.Clear();
            txtReference.Clear();
            rtbDescription.Clear();
            cmbCategorie.SelectedIndex = -1;
            cmbTaille.Text = "";
            numQuantite.Value = 1;
            picProduit.Image = null;
            picProduit.ImageLocation = null;

            // ✅ reset réglementation
            if (chkIsReglemente != null) chkIsReglemente.Checked = false;
            if (chkSignatureRequired != null) chkSignatureRequired.Checked = false;
            if (nudAgeMin != null) nudAgeMin.Value = 0;
            if (cmbNiveauRestriction != null) cmbNiveauRestriction.SelectedIndex = 0;
            if (cmbPermissionCode != null) cmbPermissionCode.SelectedIndex = -1;

            SyncPermissionUi();
        }

        private bool CodeBarreExiste(SqlConnection con, string code, int? excludeIdProduit = null)
        {
            code = (code ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) return false;

            string sql = @"
SELECT COUNT(1)
FROM dbo.Produit
WHERE LTRIM(RTRIM(CodeBarre)) = LTRIM(RTRIM(@c))
" + (excludeIdProduit.HasValue ? " AND ID_Produit <> @id" : "") + ";";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;
                if (excludeIdProduit.HasValue)
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = excludeIdProduit.Value;

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void cmbCategorie_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbTaille.Items.Clear();

            if (cmbCategorie.Text == "Chaussures")
                for (int i = 30; i <= 50; i++) cmbTaille.Items.Add(i.ToString());
            else if (cmbCategorie.Text == "Vêtements")
                cmbTaille.Items.AddRange(new[] { "XS", "XXXL", "XXL", "S", "M", "L", "XL" });
            else
                cmbTaille.Items.Add("Unique");
        }

        private void AppliquerLangue()
        {
            if (ConfigSysteme.Langue == "EN")
                this.Text = "Product Management";
        }

        private void AppliquerTheme()
        {
            if (ConfigSysteme.Theme == "Sombre")
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                foreach (Control c in Controls) c.ForeColor = Color.White;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNomProduit.Text) ||
                string.IsNullOrWhiteSpace(txtReference.Text))
            {
                MessageBox.Show(
                    "Veuillez remplir le nom et la référence du produit.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                AjouterProduit();  // cette méthode affiche déjà un MessageBox succès

                // Audit log succès
                string messageAudit = $"Produit ajouté : Nom = {txtNomProduit.Text}, Référence = {txtReference.Text}";
                ConfigSysteme.AjouterAuditLog("Produits", messageAudit, "Succès");
            }
            catch (Exception ex)
            {
                // Audit log échec
                string messageAudit = $"Erreur ajout produit : Nom = {txtNomProduit.Text}, Référence = {txtReference.Text}, Erreur = {ex.Message}";
                ConfigSysteme.AjouterAuditLog("Produits", messageAudit, "Échec");

                MessageBox.Show("Erreur lors de l'ajout du produit : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIDProduit.Text))
            {
                MessageBox.Show(
                    "Veuillez sélectionner un produit à modifier.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ModifierProduit();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedProduit(out int idProduit))
                return;

            try
            {
                // 1) Charger infos produit (affichage)
                var info = GetProduitInfo(idProduit);
                if (info == null)
                {
                    MessageBox.Show("Produit introuvable (il a peut-être été supprimé).");
                    return;
                }

                // 2) Vérifier dépendances
                var deps = CheckProduitDependencies(idProduit);

                // 3) Message confirmation détaillé
                string msg =
                    $"Voulez-vous supprimer ce produit ?\n\n" +
                    $"ID : {info.Id}\n" +
                    $"Nom : {info.Nom}\n" +
                    $"Réf : {info.Ref}\n" +
                    $"CodeBarre : {info.CodeBarre}\n\n" +
                    $"Dépendances détectées :\n" +
                    $"- Ventes : {deps.Ventes}\n" +
                    $"- Mouvements Stock : {deps.StockMoves}\n" +
                    $"- Équivalences (liens) : {deps.Equivalences}\n" +
                    $"- Unités Produit : {deps.Unites}\n\n" +
                    $"✅ Recommandation : si Ventes/Stock > 0, fais une suppression LOGIQUE (désactivation).";

                var confirm = MessageBox.Show(msg, "Confirmation suppression",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                // 4) Si dépendances -> suppression logique obligatoire
                bool hasHardRefs = deps.Ventes > 0 || deps.StockMoves > 0;
                if (hasHardRefs)
                {
                    SoftDeleteProduit(idProduit, info);
                    MessageBox.Show("Produit désactivé (suppression logique) ✅\n\nIl reste dans l’historique (ventes/stock).");
                }
                else
                {
                    // 5) Sans dépendances -> proposer le choix
                    var choice = MessageBox.Show(
                        "Aucune vente/mouvement stock trouvé.\n\n" +
                        "Oui = suppression RÉELLE (DELETE)\n" +
                        "Non = suppression LOGIQUE (désactivation)\n\n" +
                        "Que souhaitez-vous faire ?",
                        "Choix suppression",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (choice == DialogResult.Cancel)
                        return;

                    if (choice == DialogResult.No)
                    {
                        SoftDeleteProduit(idProduit, info);
                        MessageBox.Show("Produit désactivé (suppression logique) ✅");
                    }
                    else
                    {
                        HardDeleteProduit(idProduit, info);
                        MessageBox.Show("Produit supprimé définitivement ✅");
                    }
                }

                // 6) Refresh UI
                ChargerProduits();
                ReinitialiserFormulaire();
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Suppression Produit", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL suppression produit : " + ex.Message);
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Suppression Produit", ex.Message, "Échec");
                MessageBox.Show("Erreur suppression produit : " + ex.Message);
            }
        }
        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnChangerImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Choisir une image du produit";
                ofd.Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picProduit.Image?.Dispose(); // évite fuite mémoire
                    picProduit.Image = Image.FromFile(ofd.FileName);
                    picProduit.ImageLocation = ofd.FileName;
                }
            }
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Fonction Détails à implémenter.",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnImprimerEtiquettes_Click(object sender, EventArgs e)
        {
            try
            {
                var list = GetSelectedProductsFromGrid();
                if (list.Count == 0)
                {
                    MessageBox.Show("Sélectionnez au moins un produit dans la liste (Ctrl + clic).");
                    return;
                }

                // Si certains n'ont pas de CodeBarre → on bloque (ou tu peux auto-générer)
                var noCode = list.FindAll(p => string.IsNullOrWhiteSpace(p.CodeBarre));
                if (noCode.Count > 0)
                {
                    MessageBox.Show("Certains produits n'ont pas de CodeBarre. Ajoutez d'abord un code barre pour chaque produit.");
                    return;
                }

                _labelsToPrint = list;
                _labelIndex = 0;

                _labelsPrintDoc = new PrintDocument();
                _labelsPrintDoc.DocumentName = "Etiquettes Produits (QR + Barcode)";
                _labelsPrintDoc.DefaultPageSettings.Landscape = false;
                _labelsPrintDoc.DefaultPageSettings.Margins = new Margins(12, 12, 12, 12);

                _labelsPrintDoc.PrintPage -= Labels_PrintPage;
                _labelsPrintDoc.PrintPage += Labels_PrintPage;

                using (var dlg = new PrintDialog())
                {
                    dlg.Document = _labelsPrintDoc;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _labelsPrintDoc.Print();
                        _labelsPrintDoc.Dispose();
                        _labelsPrintDoc = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur impression étiquettes : " + ex.Message);
            }
        }

        // ✅ Récupère les produits depuis sélection DGV (MultiSelect)
        private List<ProductLabelInfo> GetSelectedProductsFromGrid()
        {
            var list = new List<ProductLabelInfo>();

            // Priorité SelectedRows
            if (dgvProduits.SelectedRows != null && dgvProduits.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvProduits.SelectedRows)
                {
                    var p = RowToProductLabel(row);
                    if (p != null) list.Add(p);
                }
            }
            else if (dgvProduits.CurrentRow != null)
            {
                var p = RowToProductLabel(dgvProduits.CurrentRow);
                if (p != null) list.Add(p);
            }

            // ✅ tri par nom pour impression propre
            list = list.OrderBy(x => x.NomProduit).ToList();
            return list;
        }

        private ProductLabelInfo RowToProductLabel(DataGridViewRow row)
        {
            if (row == null) return null;

            int id = 0;
            int.TryParse(row.Cells["ID_Produit"]?.Value?.ToString(), out id);

            string nom = row.Cells["NomProduit"]?.Value?.ToString() ?? "";
            string refp = row.Cells["RefProduit"]?.Value?.ToString() ?? "";
            string code = row.Cells["CodeBarre"]?.Value?.ToString() ?? "";
            string devise = row.Cells["Devise"]?.Value?.ToString() ?? "";
            string img = row.Cells["ImagePath"]?.Value?.ToString() ?? "";

            decimal prix = 0m;
            decimal.TryParse(row.Cells["Prix"]?.Value?.ToString(), out prix);

            return new ProductLabelInfo
            {
                IdProduit = id,
                NomProduit = nom.Trim(),
                RefProduit = refp.Trim(),
                CodeBarre = code.Trim(),
                Prix = prix,
                Devise = devise.Trim(),
                ImagePath = img.Trim()
            };
        }

        // =================== Zxing génération QR + Code128 ===================
        private Bitmap GenerateQr(string text, int size)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 0
                }
            };
            return writer.Write(text);
        }

        private void DrawProductImageSafe(Graphics g, string path, Rectangle target)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                g.DrawRectangle(Pens.Gainsboro, target);
                return;
            }

            try
            {
                using (var imgTemp = Image.FromFile(path))
                using (var img = new Bitmap(imgTemp))
                {
                    Rectangle fit = GetFitRect(img.Width, img.Height, target);
                    g.DrawImage(img, fit);
                }
            }
            catch
            {
                g.DrawRectangle(Pens.Gainsboro, target);
            }
        }

        private Rectangle GetFitRect(int imgW, int imgH, Rectangle box)
        {
            if (imgW <= 0 || imgH <= 0) return box;

            float rImg = imgW / (float)imgH;
            float rBox = box.Width / (float)box.Height;

            int w, h;
            if (rImg > rBox) { w = box.Width; h = (int)(w / rImg); }
            else { h = box.Height; w = (int)(h * rImg); }

            int x = box.Left + (box.Width - w) / 2;
            int y = box.Top + (box.Height - h) / 2;

            return new Rectangle(x, y, w, h);
        }

        private Bitmap GenerateCode128(string text, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 0,
                    PureBarcode = true
                }
            };
            return writer.Write(text);
        }

        // =================== PRINT PAGE (A4) ===================
        // ✅ Planche A4 : 3 colonnes x 8 lignes = 24 étiquettes par page (format autocollant courant)
        // Tu peux ajuster cols/rows si ton autocollant est différent.
        private void Labels_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // ✅ sécurité C# 7.3
            if (_sheet == null)
                _sheet = new LabelSheet { Name = "A4 24 étiquettes (3x8)", Cols = 3, Rows = 8, CellPadding = 6 };

            Rectangle page = e.MarginBounds;

            int cols = _sheet.Cols;
            int rows = _sheet.Rows;

            if (cols <= 0) cols = 3;
            if (rows <= 0) rows = 8;

            int cellW = page.Width / cols;
            int cellH = page.Height / rows;

            int pad = _sheet.CellPadding;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_labelIndex >= _labelsToPrint.Count)
                        break;

                    int x = page.Left + c * cellW;
                    int y = page.Top + r * cellH;

                    Rectangle cell = new Rectangle(x, y, cellW, cellH);
                    Rectangle label = Rectangle.Inflate(cell, -pad, -pad);

                    DrawProductLabel(e.Graphics, label, _labelsToPrint[_labelIndex]);

                    _labelIndex++;
                }

                if (_labelIndex >= _labelsToPrint.Count)
                    break;
            }

            e.HasMorePages = (_labelIndex < _labelsToPrint.Count);
        }

        // =================== DESSIN D'UNE ÉTIQUETTE ===================
        private void DrawProductLabel(Graphics g, Rectangle r, ProductLabelInfo p)
        {
            // fond blanc
            using (var br = new SolidBrush(Color.White))
                g.FillRectangle(br, r);

            // cadre fin
            using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1f))
                g.DrawRectangle(pen, r);

            string code = (p.CodeBarre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(code)) code = "NO-CODE";

            string qrText = "PRD:" + code;
            string nom = (p.NomProduit ?? "").Trim();
            string refp = (p.RefProduit ?? "").Trim();
            string prixTxt = $"{p.Prix:N2} {p.Devise}".Trim();

            int left = r.Left + 6;
            int top = r.Top + 6;

            // QR
            int qrSize = Math.Min(54, r.Height - 14);
            Rectangle qrRect = new Rectangle(left, top, qrSize, qrSize);

            // ✅ image à droite
            Rectangle imgRect = new Rectangle(r.Right - qrSize - 6, top, qrSize, qrSize);
            DrawProductImageSafe(g, p.ImagePath, imgRect);

            // ✅ Texte ENTRE QR et image => pas de superposition
            int textX = qrRect.Right + 6;
            int textW = imgRect.Left - textX - 6; // ✅ correction importante
            if (textW < 10) textW = 10;

            // Barcode
            int barH = 22;
            Rectangle barRect = new Rectangle(left, r.Bottom - barH - 8, r.Width - 12, barH);

            // QR
            using (var qr = GenerateQr(qrText, 140))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(qr, qrRect);
            }

            // Textes
            using (var fName = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var fSmall = new Font("Segoe UI", 7f, FontStyle.Regular))
            using (var fPrice = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (var brT = new SolidBrush(Color.FromArgb(25, 25, 25)))
            {
                // Nom (2 lignes max)
                var nameRect = new Rectangle(textX, top, textW, 28);
                DrawTextEllipsis(g, nom, fName, brT, nameRect);

                // Ref
                g.DrawString("Ref: " + refp, fSmall, brT, textX, top + 30);

                // Prix
                g.DrawString(prixTxt, fPrice, brT, textX, top + 44);
            }

            // Barcode
            using (var bc = GenerateCode128(code, 560, 140))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bc, barRect);
            }

            // Code dessous
            using (var fCode = new Font("Consolas", 7.5f, FontStyle.Bold))
            using (var br = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                Rectangle codeRect = new Rectangle(left, barRect.Bottom + 1, r.Width - 12, 14);
                g.DrawString(code, fCode, br, codeRect, fmt);
            }
        }

        // Helper : texte avec ellipsis
        private void DrawTextEllipsis(Graphics g, string text, Font f, Brush br, Rectangle rect)
        {
            var fmt = new StringFormat
            {
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.LineLimit
            };
            g.DrawString(text ?? "", f, br, rect, fmt);
        }

        private void btnGenererCodeBarre_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedProduit(out int idProduit))
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1) Vérifier si déjà un CodeBarre
                    string existing;
                    using (var cmd = new SqlCommand(
                        "SELECT ISNULL(CodeBarre,'') FROM dbo.Produit WHERE ID_Produit=@id;", con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                        existing = (cmd.ExecuteScalar() ?? "").ToString().Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(existing))
                    {
                        txtCodeBarreProduit.Text = existing;
                        MessageBox.Show("Ce produit a déjà un CodeBarre : " + existing);
                        return;
                    }

                    // 2) Générer
                    string code = (BuildCodeBarreProduit(idProduit) ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        MessageBox.Show("Erreur génération code barre (vide).");
                        return;
                    }

                    // 2bis) Contrôle collision (unicité)
                    if (CodeBarreExiste(con, code, excludeIdProduit: idProduit))
                    {
                        MessageBox.Show("⚠️ Collision : ce CodeBarre existe déjà. Corrige la règle de génération.");
                        return;
                    }

                    // 3) Sauvegarder (trim-safe)
                    using (var cmd = new SqlCommand(@"
UPDATE dbo.Produit
SET CodeBarre = @c
WHERE ID_Produit = @id
  AND (CodeBarre IS NULL OR LTRIM(RTRIM(CodeBarre)) = '');", con))
                    {
                        cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            MessageBox.Show("Impossible de générer : le produit a été modifié ou a déjà un code.");
                            return;
                        }
                    }

                    // Audit
                    ConfigSysteme.AjouterAuditLog(
                        "Produit CodeBarre",
                        $"Génération CodeBarre ProduitID={idProduit} | Code={code}",
                        "Succès"
                    );

                    // 4) UI
                    txtCodeBarreProduit.Text = code;
                    MessageBox.Show("✅ CodeBarre généré : " + code);

                    // 5) Refresh
                    ChargerProduits();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erreur SQL : " + ex.Message);
                ConfigSysteme.AjouterAuditLog(
                    "Produit CodeBarre",
                    $"Erreur génération CodeBarre ProduitID={idProduit} | {ex.Message}",
                    "Échec"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void btnStockInitial_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedProduit(out int idProduit)) return;

            int qte = (int)numQuantite.Value;
            if (qte <= 0)
            {
                MessageBox.Show("Quantité invalide.");
                return;
            }

            if (!(cmbDepotStockInitial.SelectedItem is ComboboxItem it))
            {
                MessageBox.Show("Choisissez le dépôt destination.");
                return;
            }
            int depotDest = it.Value;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1) empêcher stock initial si déjà stock > 0
                    int stockActuel;
                    using (var s = new SqlCommand(
                        "SELECT ISNULL(StockActuel,0) FROM dbo.Produit WHERE ID_Produit=@id;", con))
                    {
                        s.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                        stockActuel = Convert.ToInt32(s.ExecuteScalar());
                    }

                    if (stockActuel > 0)
                    {
                        MessageBox.Show("Ce produit a déjà du stock. Utilise plutôt une ENTREE/Réception.");
                        return;
                    }

                    // 2) ✅ Unité de base du produit (au lieu de PIECE)
                    int uniteBaseId = GetUniteBaseIdForProduit(con, idProduit);

                    using (SqlCommand cmd = new SqlCommand("dbo.sp_ApplyStockOperationV2", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ID_Produit", idProduit);
                        cmd.Parameters.AddWithValue("@Type", "ENTREE");

                        // ✅ Decimal typé (évite AddWithValue)
                        var pQ = cmd.Parameters.Add("@Quantite", SqlDbType.Decimal);
                        pQ.Precision = 18;
                        pQ.Scale = 3; // adapte si tu veux 0 ou 2
                        pQ.Value = qte;

                        cmd.Parameters.AddWithValue("@ID_Unite", uniteBaseId);

                        cmd.Parameters.AddWithValue("@ID_DepotSource", DBNull.Value);
                        cmd.Parameters.AddWithValue("@ID_DepotDestination", depotDest);

                        cmd.Parameters.AddWithValue("@ID_Variante", DBNull.Value);

                        string nom = (SessionEmploye.Nom ?? "").Trim();
                        string prenom = (SessionEmploye.Prenom ?? "").Trim();
                        string user = (!string.IsNullOrWhiteSpace(nom) || !string.IsNullOrWhiteSpace(prenom))
                            ? (nom + " " + prenom).Trim()
                            : Environment.UserName;

                        cmd.Parameters.AddWithValue("@Utilisateur", user);

                        cmd.Parameters.AddWithValue("@Motif", "STOCK INITIAL");
                        cmd.Parameters.AddWithValue("@Reference", "INIT");
                        cmd.Parameters.AddWithValue("@Emplacement", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Remarques", DBNull.Value);

                        cmd.Parameters.AddWithValue("@LotNumero", DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateExpiration", DBNull.Value);

                        cmd.Parameters.AddWithValue("@DateOperation", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                ConfigSysteme.AjouterAuditLog(
                    "Stock Initial",
                    $"ENTREE ProduitID={idProduit} Qte={qte} DepotDest={depotDest}",
                    "Succès"
                );

                MessageBox.Show("Stock initial créé ✅");
                ChargerProduits();
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Stock Initial", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL : " + ex.Message);
            }
            catch (Exception ex)
            {
                ConfigSysteme.AjouterAuditLog("Stock Initial", ex.Message, "Échec");
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void btnModifierPrix_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedProduit(out int idProduit))
                return;

            // Charger infos produit (nom + prix actuel)
            ProduitInfo info;
            decimal prixActuel;

            try
            {
                info = GetProduitInfo(idProduit);
                if (info == null)
                {
                    MessageBox.Show("Produit introuvable.");
                    return;
                }

                prixActuel = GetPrixProduit(idProduit);

                // 1) Signature Manager obligatoire
                string typeAction = "PRODUIT_MODIF_PRIX";
                string permissionCode = "PRODUIT_MODIF_PRIX"; // OU un code VENTE_REGLEMENTE... si tu veux réutiliser Modules
                string reference = idProduit.ToString();
                string details = $"Demande modification prix produit: {info.Nom} | Ref={info.Ref} | Code={info.CodeBarre} | PrixActuel={prixActuel:N2}";

                using (var f = new FrmSignatureManager(
                    ConfigSysteme.ConnectionString,
                    typeAction,
                    permissionCode,
                    reference,
                    details,
                    SessionEmploye.ID_Employe,
                    SessionEmploye.Poste
                ))
                {
                    if (f.ShowDialog(this) != DialogResult.OK || !f.Approved)
                    {
                        MessageBox.Show("❌ Modification prix refusée / annulée.");
                        return;
                    }

                    // 2) Demander nouveau prix
                    if (!TryAskNewPrice(prixActuel, out decimal nouveauPrix, out string motif))
                        return;

                    // 3) Bloquer baisse si tu veux uniquement descendre
                    if (nouveauPrix > prixActuel)
                    {
                        var up = MessageBox.Show(
                            "Le nouveau prix est supérieur au prix actuel. Continuer quand même ?",
                            "Confirmation",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (up != DialogResult.Yes) return;
                    }

                    // 4) Update DB
                    UpdatePrixProduit(idProduit, nouveauPrix);

                    // 5) Audit
                    ConfigSysteme.AjouterAuditLog(
                        "Produit - Modification Prix",
                        $"ProduitID={idProduit} | Nom={info.Nom} | Prix: {prixActuel:N2} -> {nouveauPrix:N2} | Motif={motif} | Manager={f.ManagerNom}",
                        "Succès"
                    );

                    // 6) Refresh UI
                    ChargerProduits();
                    txtPrix.Text = nouveauPrix.ToString("N2");

                    MessageBox.Show($"✅ Prix modifié.\n\nAncien: {prixActuel:N2}\nNouveau: {nouveauPrix:N2}");
                }
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Produit - Modification Prix", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL modification prix : " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur modification prix : " + ex.Message);
            }
        }

        private decimal GetPrixProduit(int idProduit)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT ISNULL(Prix,0) FROM dbo.Produit WHERE ID_Produit=@id;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;
                con.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private void UpdatePrixProduit(int idProduit, decimal nouveauPrix)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Produit
SET Prix = @p
WHERE ID_Produit = @id;", con))
            {
                var p = cmd.Parameters.Add("@p", SqlDbType.Decimal);
                p.Precision = 18;
                p.Scale = 2;
                p.Value = nouveauPrix;

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProduit;

                con.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0) throw new Exception("Produit introuvable (update 0 ligne).");
            }
        }

        private bool TryAskNewPrice(decimal prixActuel, out decimal nouveauPrix, out string motif)
        {
            nouveauPrix = 0m;
            motif = "";

            // Mini dialog simple
            using (var f = new Form())
            {
                f.Text = "Modifier le prix (Manager)";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.StartPosition = FormStartPosition.CenterParent;
                f.Width = 420;
                f.Height = 220;
                f.MaximizeBox = false;
                f.MinimizeBox = false;

                var lbl1 = new Label { Left = 15, Top = 15, Width = 380, Text = $"Prix actuel : {prixActuel:N2}" };
                var lbl2 = new Label { Left = 15, Top = 50, Width = 120, Text = "Nouveau prix :" };
                var txt = new TextBox { Left = 140, Top = 46, Width = 240, Text = prixActuel.ToString("N2") };

                var lbl3 = new Label { Left = 15, Top = 85, Width = 120, Text = "Motif :" };
                var txtMotif = new TextBox { Left = 140, Top = 81, Width = 240 };

                var ok = new Button { Text = "OK", Left = 220, Width = 75, Top = 125, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Annuler", Left = 305, Width = 75, Top = 125, DialogResult = DialogResult.Cancel };

                f.Controls.AddRange(new Control[] { lbl1, lbl2, txt, lbl3, txtMotif, ok, cancel });
                f.AcceptButton = ok;
                f.CancelButton = cancel;

                if (f.ShowDialog(this) != DialogResult.OK)
                    return false;

                if (!decimal.TryParse(txt.Text.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal p))
                {
                    MessageBox.Show("Prix invalide.");
                    return false;
                }

                if (p <= 0)
                {
                    MessageBox.Show("Le prix doit être > 0.");
                    return false;
                }

                string m = (txtMotif.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(m))
                {
                    MessageBox.Show("Motif obligatoire.");
                    return false;
                }

                nouveauPrix = Math.Round(p, 2);
                motif = m;
                return true;
            }
        }


        private void btnAjouterEquivalence_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedProduit(out int idProduit)) return;

            using (var f = new FrmSelectProduitEquivalent(connectionString, idProduit))
            {
                if (f.ShowDialog() == DialogResult.OK)
                    AjouterEquivalence(idProduit, f.SelectedProduitId, f.SelectedType, f.SelectedPriorite);
            }
        }

        private void btnSignatureManager_Click(object sender, EventArgs e)
        {
            string typeAction = "PRODUIT_SIGNATURE_TEST";
            string permissionCode = "PRODUIT_SIGNATURE_TEST";
            string reference = (txtIDProduit.Text ?? "").Trim();
            string details = "Accès signature depuis FormProduits";

            using (var f = new FrmSignatureManager(
                ConfigSysteme.ConnectionString,
                typeAction,
                permissionCode,
                reference,
                details,
                SessionEmploye.ID_Employe,
                SessionEmploye.Poste // ✅ IMPORTANT : rôle demandeur
            ))
            {
                if (f.ShowDialog(this) == DialogResult.OK && f.Approved)
                    MessageBox.Show("✅ Signature manager validée : " + f.ManagerNom);
                else
                    MessageBox.Show("❌ Signature refusée / annulée.");
            }
        }
        private void btnSupprimerEquivalence_Click(object sender, EventArgs e)
        {
            try
            {
                // 1) Produit source sélectionné
                if (!TryGetSelectedProduit(out int idProduit))
                    return;

                // 2) Vérifier la sélection dans la grille des équivalences
                if (dgvEquivalences == null || dgvEquivalences.DataSource == null || dgvEquivalences.Rows.Count == 0)
                {
                    MessageBox.Show("Aucune équivalence à supprimer.");
                    return;
                }

                DataGridViewRow row = dgvEquivalences.CurrentRow;
                if (row == null)
                {
                    MessageBox.Show("Sélectionnez une équivalence dans la liste.");
                    return;
                }

                object vIdEq = row.Cells["ID_Equivalence"]?.Value;
                if (vIdEq == null || vIdEq == DBNull.Value || !int.TryParse(vIdEq.ToString(), out int idEquivalence) || idEquivalence <= 0)
                {
                    MessageBox.Show("ID_Equivalence invalide (ligne sélectionnée).");
                    return;
                }

                // Info affichée dans la confirmation
                string nomEq = row.Cells["NomProduit"]?.Value?.ToString() ?? "";
                string refEq = row.Cells["RefProduit"]?.Value?.ToString() ?? "";
                string typeEq = row.Cells["Type"]?.Value?.ToString() ?? "";
                string prioEq = row.Cells["Priorite"]?.Value?.ToString() ?? "";

                // 3) Confirmation
                var confirm = MessageBox.Show(
                    $"Voulez-vous supprimer (désactiver) cette équivalence ?\n\n" +
                    $"Produit équivalent : {nomEq}\n" +
                    $"Réf : {refEq}\n" +
                    $"Type : {typeEq} | Priorité : {prioEq}",
                    "Confirmation suppression",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                // 4) Désactivation (soft delete)
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(@"
UPDATE dbo.ProduitEquivalence
SET Actif = 0
WHERE ID_Equivalence = @id;", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idEquivalence;

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        MessageBox.Show("Aucune ligne modifiée. L'équivalence n'existe plus ou a déjà été désactivée.");
                        return;
                    }
                }

                // 5) Audit log
                ConfigSysteme.AjouterAuditLog(
                    "Produit Equivalence",
                    $"Désactivation équivalence ID_Equivalence={idEquivalence} | ProduitID={idProduit} | Eq={nomEq} ({refEq})",
                    "Succès"
                );

                // 6) Refresh UI
                ChargerEquivalencesDuProduit(idProduit);

                MessageBox.Show("Équivalence supprimée (désactivée) ✅");
            }
            catch (SqlException ex)
            {
                ConfigSysteme.AjouterAuditLog("Produit Equivalence", ex.Message, "Échec");
                MessageBox.Show("Erreur SQL suppression équivalence : " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur suppression équivalence : " + ex.Message);
            }
        }
    }
    }
