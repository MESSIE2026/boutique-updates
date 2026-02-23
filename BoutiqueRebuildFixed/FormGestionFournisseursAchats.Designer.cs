namespace BoutiqueRebuildFixed
{
    partial class FormGestionFournisseursAchats
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grpInfosFournisseur = new System.Windows.Forms.GroupBox();
            this.txtAdresse = new System.Windows.Forms.TextBox();
            this.lblAdresse = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtTelephone = new System.Windows.Forms.TextBox();
            this.lblTelephone = new System.Windows.Forms.Label();
            this.txtContact = new System.Windows.Forms.TextBox();
            this.lblContact = new System.Windows.Forms.Label();
            this.lblNomFournisseur = new System.Windows.Forms.Label();
            this.grpInfosAchat = new System.Windows.Forms.GroupBox();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.lblNomProduit = new System.Windows.Forms.Label();
            this.txtPrixUnitaire = new System.Windows.Forms.TextBox();
            this.lblPrixUnitaire = new System.Windows.Forms.Label();
            this.txtQuantite = new System.Windows.Forms.TextBox();
            this.lblQuantite = new System.Windows.Forms.Label();
            this.cmbDevise = new System.Windows.Forms.ComboBox();
            this.lblDevise = new System.Windows.Forms.Label();
            this.btnAnnulerAchat = new System.Windows.Forms.Button();
            this.btnAjouterAchat = new System.Windows.Forms.Button();
            this.cmbStatut = new System.Windows.Forms.ComboBox();
            this.cmbModePaiement = new System.Windows.Forms.ComboBox();
            this.dtpDateAchat = new System.Windows.Forms.DateTimePicker();
            this.lblStatut = new System.Windows.Forms.Label();
            this.txtMontantTotal = new System.Windows.Forms.TextBox();
            this.lblMontantTotal = new System.Windows.Forms.Label();
            this.lblModePaiement = new System.Windows.Forms.Label();
            this.lblDateAchat = new System.Windows.Forms.Label();
            this.txtRefCommande = new System.Windows.Forms.TextBox();
            this.lblRefCommande = new System.Windows.Forms.Label();
            this.grpHistoriqueAchats = new System.Windows.Forms.GroupBox();
            this.dgvHistoriqueAchats = new System.Windows.Forms.DataGridView();
            this.grpDetailsCommande = new System.Windows.Forms.GroupBox();
            this.dgvDetailsCommande = new System.Windows.Forms.DataGridView();
            this.btnModifier = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnExporter = new System.Windows.Forms.Button();
            this.btnFermer1 = new System.Windows.Forms.Button();
            this.btnFermer2 = new System.Windows.Forms.Button();
            this.cmbNomFournisseur = new System.Windows.Forms.ComboBox();
            this.grpInfosFournisseur.SuspendLayout();
            this.grpInfosAchat.SuspendLayout();
            this.grpHistoriqueAchats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoriqueAchats)).BeginInit();
            this.grpDetailsCommande.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailsCommande)).BeginInit();
            this.SuspendLayout();
            // 
            // grpInfosFournisseur
            // 
            this.grpInfosFournisseur.Controls.Add(this.cmbNomFournisseur);
            this.grpInfosFournisseur.Controls.Add(this.txtAdresse);
            this.grpInfosFournisseur.Controls.Add(this.lblAdresse);
            this.grpInfosFournisseur.Controls.Add(this.txtEmail);
            this.grpInfosFournisseur.Controls.Add(this.lblEmail);
            this.grpInfosFournisseur.Controls.Add(this.txtTelephone);
            this.grpInfosFournisseur.Controls.Add(this.lblTelephone);
            this.grpInfosFournisseur.Controls.Add(this.txtContact);
            this.grpInfosFournisseur.Controls.Add(this.lblContact);
            this.grpInfosFournisseur.Controls.Add(this.lblNomFournisseur);
            this.grpInfosFournisseur.Font = new System.Drawing.Font("Segoe UI Semibold", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpInfosFournisseur.ForeColor = System.Drawing.Color.White;
            this.grpInfosFournisseur.Location = new System.Drawing.Point(46, 12);
            this.grpInfosFournisseur.Name = "grpInfosFournisseur";
            this.grpInfosFournisseur.Size = new System.Drawing.Size(579, 248);
            this.grpInfosFournisseur.TabIndex = 0;
            this.grpInfosFournisseur.TabStop = false;
            this.grpInfosFournisseur.Text = "Informations Fournisseur";
            // 
            // txtAdresse
            // 
            this.txtAdresse.Location = new System.Drawing.Point(184, 162);
            this.txtAdresse.Multiline = true;
            this.txtAdresse.Name = "txtAdresse";
            this.txtAdresse.Size = new System.Drawing.Size(379, 79);
            this.txtAdresse.TabIndex = 9;
            // 
            // lblAdresse
            // 
            this.lblAdresse.AutoSize = true;
            this.lblAdresse.Location = new System.Drawing.Point(6, 161);
            this.lblAdresse.Name = "lblAdresse";
            this.lblAdresse.Size = new System.Drawing.Size(78, 23);
            this.lblAdresse.TabIndex = 8;
            this.lblAdresse.Text = "Adresse :";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(184, 127);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(379, 30);
            this.txtEmail.TabIndex = 7;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(6, 132);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(63, 23);
            this.lblEmail.TabIndex = 6;
            this.lblEmail.Text = "Email :";
            // 
            // txtTelephone
            // 
            this.txtTelephone.Location = new System.Drawing.Point(184, 92);
            this.txtTelephone.Name = "txtTelephone";
            this.txtTelephone.Size = new System.Drawing.Size(379, 30);
            this.txtTelephone.TabIndex = 5;
            // 
            // lblTelephone
            // 
            this.lblTelephone.AutoSize = true;
            this.lblTelephone.Location = new System.Drawing.Point(6, 95);
            this.lblTelephone.Name = "lblTelephone";
            this.lblTelephone.Size = new System.Drawing.Size(99, 23);
            this.lblTelephone.TabIndex = 4;
            this.lblTelephone.Text = "Telephone :";
            // 
            // txtContact
            // 
            this.txtContact.Location = new System.Drawing.Point(184, 57);
            this.txtContact.Name = "txtContact";
            this.txtContact.Size = new System.Drawing.Size(379, 30);
            this.txtContact.TabIndex = 3;
            // 
            // lblContact
            // 
            this.lblContact.AutoSize = true;
            this.lblContact.Location = new System.Drawing.Point(6, 60);
            this.lblContact.Name = "lblContact";
            this.lblContact.Size = new System.Drawing.Size(79, 23);
            this.lblContact.TabIndex = 2;
            this.lblContact.Text = "Contact :";
            // 
            // lblNomFournisseur
            // 
            this.lblNomFournisseur.AutoSize = true;
            this.lblNomFournisseur.Location = new System.Drawing.Point(6, 25);
            this.lblNomFournisseur.Name = "lblNomFournisseur";
            this.lblNomFournisseur.Size = new System.Drawing.Size(149, 23);
            this.lblNomFournisseur.TabIndex = 0;
            this.lblNomFournisseur.Text = "Nom Fournisseur :";
            // 
            // grpInfosAchat
            // 
            this.grpInfosAchat.Controls.Add(this.txtNomProduit);
            this.grpInfosAchat.Controls.Add(this.lblNomProduit);
            this.grpInfosAchat.Controls.Add(this.txtPrixUnitaire);
            this.grpInfosAchat.Controls.Add(this.lblPrixUnitaire);
            this.grpInfosAchat.Controls.Add(this.txtQuantite);
            this.grpInfosAchat.Controls.Add(this.lblQuantite);
            this.grpInfosAchat.Controls.Add(this.cmbDevise);
            this.grpInfosAchat.Controls.Add(this.lblDevise);
            this.grpInfosAchat.Controls.Add(this.btnAnnulerAchat);
            this.grpInfosAchat.Controls.Add(this.btnAjouterAchat);
            this.grpInfosAchat.Controls.Add(this.cmbStatut);
            this.grpInfosAchat.Controls.Add(this.cmbModePaiement);
            this.grpInfosAchat.Controls.Add(this.dtpDateAchat);
            this.grpInfosAchat.Controls.Add(this.lblStatut);
            this.grpInfosAchat.Controls.Add(this.txtMontantTotal);
            this.grpInfosAchat.Controls.Add(this.lblMontantTotal);
            this.grpInfosAchat.Controls.Add(this.lblModePaiement);
            this.grpInfosAchat.Controls.Add(this.lblDateAchat);
            this.grpInfosAchat.Controls.Add(this.txtRefCommande);
            this.grpInfosAchat.Controls.Add(this.lblRefCommande);
            this.grpInfosAchat.Font = new System.Drawing.Font("Segoe UI Semibold", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpInfosAchat.ForeColor = System.Drawing.Color.White;
            this.grpInfosAchat.Location = new System.Drawing.Point(672, 12);
            this.grpInfosAchat.Name = "grpInfosAchat";
            this.grpInfosAchat.Size = new System.Drawing.Size(650, 248);
            this.grpInfosAchat.TabIndex = 1;
            this.grpInfosAchat.TabStop = false;
            this.grpInfosAchat.Text = "Informations Achat";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(154, 57);
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(217, 30);
            this.txtNomProduit.TabIndex = 22;
            // 
            // lblNomProduit
            // 
            this.lblNomProduit.AutoSize = true;
            this.lblNomProduit.Location = new System.Drawing.Point(6, 59);
            this.lblNomProduit.Name = "lblNomProduit";
            this.lblNomProduit.Size = new System.Drawing.Size(117, 23);
            this.lblNomProduit.TabIndex = 21;
            this.lblNomProduit.Text = "Nom Produit :";
            // 
            // txtPrixUnitaire
            // 
            this.txtPrixUnitaire.Location = new System.Drawing.Point(495, 93);
            this.txtPrixUnitaire.Name = "txtPrixUnitaire";
            this.txtPrixUnitaire.Size = new System.Drawing.Size(149, 30);
            this.txtPrixUnitaire.TabIndex = 20;
            // 
            // lblPrixUnitaire
            // 
            this.lblPrixUnitaire.AutoSize = true;
            this.lblPrixUnitaire.Location = new System.Drawing.Point(377, 96);
            this.lblPrixUnitaire.Name = "lblPrixUnitaire";
            this.lblPrixUnitaire.Size = new System.Drawing.Size(45, 23);
            this.lblPrixUnitaire.TabIndex = 19;
            this.lblPrixUnitaire.Text = "P.U :";
            // 
            // txtQuantite
            // 
            this.txtQuantite.Location = new System.Drawing.Point(495, 56);
            this.txtQuantite.Name = "txtQuantite";
            this.txtQuantite.Size = new System.Drawing.Size(149, 30);
            this.txtQuantite.TabIndex = 18;
            // 
            // lblQuantite
            // 
            this.lblQuantite.AutoSize = true;
            this.lblQuantite.Location = new System.Drawing.Point(377, 59);
            this.lblQuantite.Name = "lblQuantite";
            this.lblQuantite.Size = new System.Drawing.Size(88, 23);
            this.lblQuantite.TabIndex = 17;
            this.lblQuantite.Text = "Quantite :";
            // 
            // cmbDevise
            // 
            this.cmbDevise.FormattingEnabled = true;
            this.cmbDevise.Items.AddRange(new object[] {
            "[\"Reçu\", \"En Attente\", \"Annulé\"]"});
            this.cmbDevise.Location = new System.Drawing.Point(495, 128);
            this.cmbDevise.Name = "cmbDevise";
            this.cmbDevise.Size = new System.Drawing.Size(149, 31);
            this.cmbDevise.TabIndex = 16;
            // 
            // lblDevise
            // 
            this.lblDevise.AutoSize = true;
            this.lblDevise.Location = new System.Drawing.Point(377, 133);
            this.lblDevise.Name = "lblDevise";
            this.lblDevise.Size = new System.Drawing.Size(70, 23);
            this.lblDevise.TabIndex = 15;
            this.lblDevise.Text = "Devise :";
            // 
            // btnAnnulerAchat
            // 
            this.btnAnnulerAchat.BackColor = System.Drawing.Color.Red;
            this.btnAnnulerAchat.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnulerAchat.ForeColor = System.Drawing.Color.White;
            this.btnAnnulerAchat.Location = new System.Drawing.Point(480, 199);
            this.btnAnnulerAchat.Name = "btnAnnulerAchat";
            this.btnAnnulerAchat.Size = new System.Drawing.Size(154, 42);
            this.btnAnnulerAchat.TabIndex = 14;
            this.btnAnnulerAchat.Text = "Annuler Achat";
            this.btnAnnulerAchat.UseVisualStyleBackColor = false;
            this.btnAnnulerAchat.Click += new System.EventHandler(this.btnAnnulerAchat_Click);
            // 
            // btnAjouterAchat
            // 
            this.btnAjouterAchat.BackColor = System.Drawing.Color.Blue;
            this.btnAjouterAchat.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAjouterAchat.ForeColor = System.Drawing.Color.White;
            this.btnAjouterAchat.Location = new System.Drawing.Point(288, 203);
            this.btnAjouterAchat.Name = "btnAjouterAchat";
            this.btnAjouterAchat.Size = new System.Drawing.Size(177, 39);
            this.btnAjouterAchat.TabIndex = 13;
            this.btnAjouterAchat.Text = "Ajouter Achat";
            this.btnAjouterAchat.UseVisualStyleBackColor = false;
            this.btnAjouterAchat.Click += new System.EventHandler(this.btnAjouterAchat_Click);
            // 
            // cmbStatut
            // 
            this.cmbStatut.FormattingEnabled = true;
            this.cmbStatut.Items.AddRange(new object[] {
            "Reçu",
            "En Attente",
            "Annulé"});
            this.cmbStatut.Location = new System.Drawing.Point(154, 166);
            this.cmbStatut.Name = "cmbStatut";
            this.cmbStatut.Size = new System.Drawing.Size(217, 31);
            this.cmbStatut.TabIndex = 12;
            // 
            // cmbModePaiement
            // 
            this.cmbModePaiement.FormattingEnabled = true;
            this.cmbModePaiement.Items.AddRange(new object[] {
            "Espèces",
            "Virement Bancaire",
            "Chèque ",
            "Carte Bancaire"});
            this.cmbModePaiement.Location = new System.Drawing.Point(154, 94);
            this.cmbModePaiement.Name = "cmbModePaiement";
            this.cmbModePaiement.Size = new System.Drawing.Size(217, 31);
            this.cmbModePaiement.TabIndex = 11;
            // 
            // dtpDateAchat
            // 
            this.dtpDateAchat.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateAchat.Location = new System.Drawing.Point(495, 23);
            this.dtpDateAchat.Name = "dtpDateAchat";
            this.dtpDateAchat.Size = new System.Drawing.Size(149, 30);
            this.dtpDateAchat.TabIndex = 10;
            // 
            // lblStatut
            // 
            this.lblStatut.AutoSize = true;
            this.lblStatut.Location = new System.Drawing.Point(6, 169);
            this.lblStatut.Name = "lblStatut";
            this.lblStatut.Size = new System.Drawing.Size(66, 23);
            this.lblStatut.TabIndex = 8;
            this.lblStatut.Text = "Statut :";
            // 
            // txtMontantTotal
            // 
            this.txtMontantTotal.Location = new System.Drawing.Point(154, 129);
            this.txtMontantTotal.Name = "txtMontantTotal";
            this.txtMontantTotal.Size = new System.Drawing.Size(217, 30);
            this.txtMontantTotal.TabIndex = 7;
            // 
            // lblMontantTotal
            // 
            this.lblMontantTotal.AutoSize = true;
            this.lblMontantTotal.Location = new System.Drawing.Point(6, 134);
            this.lblMontantTotal.Name = "lblMontantTotal";
            this.lblMontantTotal.Size = new System.Drawing.Size(128, 23);
            this.lblMontantTotal.TabIndex = 6;
            this.lblMontantTotal.Text = "Montant Total :";
            // 
            // lblModePaiement
            // 
            this.lblModePaiement.AutoSize = true;
            this.lblModePaiement.Location = new System.Drawing.Point(6, 96);
            this.lblModePaiement.Name = "lblModePaiement";
            this.lblModePaiement.Size = new System.Drawing.Size(140, 23);
            this.lblModePaiement.TabIndex = 4;
            this.lblModePaiement.Text = "Mode Paiement :";
            // 
            // lblDateAchat
            // 
            this.lblDateAchat.AutoSize = true;
            this.lblDateAchat.Location = new System.Drawing.Point(377, 26);
            this.lblDateAchat.Name = "lblDateAchat";
            this.lblDateAchat.Size = new System.Drawing.Size(106, 23);
            this.lblDateAchat.TabIndex = 2;
            this.lblDateAchat.Text = "Date Achat :";
            // 
            // txtRefCommande
            // 
            this.txtRefCommande.Location = new System.Drawing.Point(154, 23);
            this.txtRefCommande.Name = "txtRefCommande";
            this.txtRefCommande.Size = new System.Drawing.Size(217, 30);
            this.txtRefCommande.TabIndex = 1;
            // 
            // lblRefCommande
            // 
            this.lblRefCommande.AutoSize = true;
            this.lblRefCommande.Location = new System.Drawing.Point(6, 25);
            this.lblRefCommande.Name = "lblRefCommande";
            this.lblRefCommande.Size = new System.Drawing.Size(142, 23);
            this.lblRefCommande.TabIndex = 0;
            this.lblRefCommande.Text = "Ref. Commande :";
            // 
            // grpHistoriqueAchats
            // 
            this.grpHistoriqueAchats.Controls.Add(this.dgvHistoriqueAchats);
            this.grpHistoriqueAchats.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpHistoriqueAchats.ForeColor = System.Drawing.Color.White;
            this.grpHistoriqueAchats.Location = new System.Drawing.Point(46, 266);
            this.grpHistoriqueAchats.Name = "grpHistoriqueAchats";
            this.grpHistoriqueAchats.Size = new System.Drawing.Size(1283, 215);
            this.grpHistoriqueAchats.TabIndex = 2;
            this.grpHistoriqueAchats.TabStop = false;
            this.grpHistoriqueAchats.Text = "Historique des Achats";
            // 
            // dgvHistoriqueAchats
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvHistoriqueAchats.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHistoriqueAchats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistoriqueAchats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvHistoriqueAchats.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvHistoriqueAchats.Location = new System.Drawing.Point(10, 28);
            this.dgvHistoriqueAchats.Name = "dgvHistoriqueAchats";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHistoriqueAchats.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvHistoriqueAchats.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvHistoriqueAchats.Size = new System.Drawing.Size(1267, 176);
            this.dgvHistoriqueAchats.TabIndex = 0;
            // 
            // grpDetailsCommande
            // 
            this.grpDetailsCommande.Controls.Add(this.dgvDetailsCommande);
            this.grpDetailsCommande.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpDetailsCommande.ForeColor = System.Drawing.Color.White;
            this.grpDetailsCommande.Location = new System.Drawing.Point(46, 487);
            this.grpDetailsCommande.Name = "grpDetailsCommande";
            this.grpDetailsCommande.Size = new System.Drawing.Size(1283, 207);
            this.grpDetailsCommande.TabIndex = 3;
            this.grpDetailsCommande.TabStop = false;
            this.grpDetailsCommande.Text = "Détails de la Commande";
            // 
            // dgvDetailsCommande
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvDetailsCommande.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvDetailsCommande.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetailsCommande.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDetailsCommande.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvDetailsCommande.Location = new System.Drawing.Point(20, 21);
            this.dgvDetailsCommande.Name = "dgvDetailsCommande";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDetailsCommande.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvDetailsCommande.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvDetailsCommande.Size = new System.Drawing.Size(1257, 180);
            this.dgvDetailsCommande.TabIndex = 0;
            // 
            // btnModifier
            // 
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnModifier.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModifier.ForeColor = System.Drawing.Color.Black;
            this.btnModifier.Location = new System.Drawing.Point(136, 700);
            this.btnModifier.Name = "btnModifier";
            this.btnModifier.Size = new System.Drawing.Size(113, 42);
            this.btnModifier.TabIndex = 14;
            this.btnModifier.Text = "Modifier";
            this.btnModifier.UseVisualStyleBackColor = false;
            this.btnModifier.Click += new System.EventHandler(this.btnModifier_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.BackColor = System.Drawing.Color.Maroon;
            this.btnSupprimer.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.Location = new System.Drawing.Point(361, 700);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(128, 42);
            this.btnSupprimer.TabIndex = 15;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = false;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnExporter
            // 
            this.btnExporter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnExporter.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporter.ForeColor = System.Drawing.Color.White;
            this.btnExporter.Location = new System.Drawing.Point(605, 700);
            this.btnExporter.Name = "btnExporter";
            this.btnExporter.Size = new System.Drawing.Size(128, 42);
            this.btnExporter.TabIndex = 16;
            this.btnExporter.Text = "Exporter";
            this.btnExporter.UseVisualStyleBackColor = false;
            this.btnExporter.Click += new System.EventHandler(this.btnExporter_Click);
            // 
            // btnFermer1
            // 
            this.btnFermer1.BackColor = System.Drawing.Color.Black;
            this.btnFermer1.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFermer1.ForeColor = System.Drawing.Color.White;
            this.btnFermer1.Location = new System.Drawing.Point(833, 700);
            this.btnFermer1.Name = "btnFermer1";
            this.btnFermer1.Size = new System.Drawing.Size(128, 42);
            this.btnFermer1.TabIndex = 17;
            this.btnFermer1.Text = "Fermer 1";
            this.btnFermer1.UseVisualStyleBackColor = false;
            this.btnFermer1.Click += new System.EventHandler(this.btnFermer1_Click);
            // 
            // btnFermer2
            // 
            this.btnFermer2.BackColor = System.Drawing.Color.Black;
            this.btnFermer2.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFermer2.ForeColor = System.Drawing.Color.White;
            this.btnFermer2.Location = new System.Drawing.Point(1069, 700);
            this.btnFermer2.Name = "btnFermer2";
            this.btnFermer2.Size = new System.Drawing.Size(128, 42);
            this.btnFermer2.TabIndex = 18;
            this.btnFermer2.Text = "Fermer 2";
            this.btnFermer2.UseVisualStyleBackColor = false;
            this.btnFermer2.Click += new System.EventHandler(this.btnFermer2_Click);
            // 
            // cmbNomFournisseur
            // 
            this.cmbNomFournisseur.FormattingEnabled = true;
            this.cmbNomFournisseur.Items.AddRange(new object[] {
            "Reçu",
            "En Attente",
            "Annulé"});
            this.cmbNomFournisseur.Location = new System.Drawing.Point(184, 22);
            this.cmbNomFournisseur.Name = "cmbNomFournisseur";
            this.cmbNomFournisseur.Size = new System.Drawing.Size(379, 31);
            this.cmbNomFournisseur.TabIndex = 13;
            // 
            // FormGestionFournisseursAchats
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(1334, 749);
            this.Controls.Add(this.btnFermer2);
            this.Controls.Add(this.btnFermer1);
            this.Controls.Add(this.btnExporter);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.grpDetailsCommande);
            this.Controls.Add(this.grpHistoriqueAchats);
            this.Controls.Add(this.grpInfosAchat);
            this.Controls.Add(this.grpInfosFournisseur);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormGestionFournisseursAchats";
            this.Text = "Gestion des Fournisseur / Achats";
            this.Load += new System.EventHandler(this.FormGestionFournisseursAchats_Load);
            this.grpInfosFournisseur.ResumeLayout(false);
            this.grpInfosFournisseur.PerformLayout();
            this.grpInfosAchat.ResumeLayout(false);
            this.grpInfosAchat.PerformLayout();
            this.grpHistoriqueAchats.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoriqueAchats)).EndInit();
            this.grpDetailsCommande.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailsCommande)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpInfosFournisseur;
        private System.Windows.Forms.TextBox txtContact;
        private System.Windows.Forms.Label lblContact;
        private System.Windows.Forms.Label lblNomFournisseur;
        private System.Windows.Forms.TextBox txtAdresse;
        private System.Windows.Forms.Label lblAdresse;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtTelephone;
        private System.Windows.Forms.Label lblTelephone;
        private System.Windows.Forms.GroupBox grpInfosAchat;
        private System.Windows.Forms.Label lblStatut;
        private System.Windows.Forms.TextBox txtMontantTotal;
        private System.Windows.Forms.Label lblMontantTotal;
        private System.Windows.Forms.Label lblModePaiement;
        private System.Windows.Forms.Label lblDateAchat;
        private System.Windows.Forms.TextBox txtRefCommande;
        private System.Windows.Forms.Label lblRefCommande;
        private System.Windows.Forms.DateTimePicker dtpDateAchat;
        private System.Windows.Forms.ComboBox cmbModePaiement;
        private System.Windows.Forms.ComboBox cmbStatut;
        private System.Windows.Forms.Button btnAnnulerAchat;
        private System.Windows.Forms.Button btnAjouterAchat;
        private System.Windows.Forms.GroupBox grpHistoriqueAchats;
        private System.Windows.Forms.DataGridView dgvHistoriqueAchats;
        private System.Windows.Forms.GroupBox grpDetailsCommande;
        private System.Windows.Forms.DataGridView dgvDetailsCommande;
        private System.Windows.Forms.Button btnModifier;
        private System.Windows.Forms.Button btnSupprimer;
        private System.Windows.Forms.Button btnExporter;
        private System.Windows.Forms.Button btnFermer1;
        private System.Windows.Forms.Button btnFermer2;
        private System.Windows.Forms.ComboBox cmbDevise;
        private System.Windows.Forms.Label lblDevise;
        private System.Windows.Forms.TextBox txtPrixUnitaire;
        private System.Windows.Forms.Label lblPrixUnitaire;
        private System.Windows.Forms.TextBox txtQuantite;
        private System.Windows.Forms.Label lblQuantite;
        private System.Windows.Forms.TextBox txtNomProduit;
        private System.Windows.Forms.Label lblNomProduit;
        private System.Windows.Forms.ComboBox cmbNomFournisseur;
    }
}