namespace BoutiqueRebuildFixed
{
    partial class FormProduits
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
            this.Label1 = new System.Windows.Forms.Label();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.cmbCategorie = new System.Windows.Forms.ComboBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.cmbTaille = new System.Windows.Forms.ComboBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.txtCouleur = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.numQuantite = new System.Windows.Forms.NumericUpDown();
            this.Label7 = new System.Windows.Forms.Label();
            this.txtPrix = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.dtpDateAjout = new System.Windows.Forms.DateTimePicker();
            this.Label10 = new System.Windows.Forms.Label();
            this.picProduit = new System.Windows.Forms.PictureBox();
            this.btnChangerImage = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnModifier = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtScanCode = new System.Windows.Forms.TextBox();
            this.lblDevise = new System.Windows.Forms.Label();
            this.cmbDevise = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtIDProduit = new System.Windows.Forms.TextBox();
            this.dgvProduits = new System.Windows.Forms.DataGridView();
            this.btnImprimerEtiquettes = new System.Windows.Forms.Button();
            this.btnGenererCodeBarre = new System.Windows.Forms.Button();
            this.txtCodeBarreProduit = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cmbFormatEtiquettes = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.cmbDepotStockInitial = new System.Windows.Forms.ComboBox();
            this.btnStockInitial = new System.Windows.Forms.Button();
            this.btnAjouterEquivalence = new System.Windows.Forms.Button();
            this.btnSupprimerEquivalence = new System.Windows.Forms.Button();
            this.cmbEquivalent = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduits)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(18, 14);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(166, 27);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Nom du Produit :";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(183, 14);
            this.txtNomProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(209, 27);
            this.txtNomProduit.TabIndex = 2;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(18, 49);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(111, 27);
            this.Label2.TabIndex = 3;
            this.Label2.Text = "Reference :";
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(183, 50);
            this.txtReference.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(209, 27);
            this.txtReference.TabIndex = 4;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(13, 86);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(110, 27);
            this.Label3.TabIndex = 5;
            this.Label3.Text = "Categorie :";
            // 
            // cmbCategorie
            // 
            this.cmbCategorie.FormattingEnabled = true;
            this.cmbCategorie.Location = new System.Drawing.Point(183, 87);
            this.cmbCategorie.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbCategorie.Name = "cmbCategorie";
            this.cmbCategorie.Size = new System.Drawing.Size(209, 28);
            this.cmbCategorie.TabIndex = 6;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(13, 126);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(71, 27);
            this.Label4.TabIndex = 7;
            this.Label4.Text = "Taille :";
            // 
            // cmbTaille
            // 
            this.cmbTaille.FormattingEnabled = true;
            this.cmbTaille.Location = new System.Drawing.Point(183, 125);
            this.cmbTaille.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbTaille.Name = "cmbTaille";
            this.cmbTaille.Size = new System.Drawing.Size(209, 28);
            this.cmbTaille.TabIndex = 8;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(13, 162);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(94, 27);
            this.Label5.TabIndex = 9;
            this.Label5.Text = "Couleur :";
            // 
            // txtCouleur
            // 
            this.txtCouleur.Location = new System.Drawing.Point(183, 160);
            this.txtCouleur.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCouleur.Name = "txtCouleur";
            this.txtCouleur.Size = new System.Drawing.Size(209, 27);
            this.txtCouleur.TabIndex = 10;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(13, 198);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(102, 27);
            this.Label6.TabIndex = 11;
            this.Label6.Text = "Quantite :";
            // 
            // numQuantite
            // 
            this.numQuantite.Location = new System.Drawing.Point(183, 196);
            this.numQuantite.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numQuantite.Name = "numQuantite";
            this.numQuantite.Size = new System.Drawing.Size(62, 27);
            this.numQuantite.TabIndex = 12;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(18, 239);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(58, 27);
            this.Label7.TabIndex = 13;
            this.Label7.Text = "Prix :";
            // 
            // txtPrix
            // 
            this.txtPrix.Location = new System.Drawing.Point(183, 233);
            this.txtPrix.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPrix.Name = "txtPrix";
            this.txtPrix.Size = new System.Drawing.Size(209, 27);
            this.txtPrix.TabIndex = 14;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(13, 341);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(125, 27);
            this.Label9.TabIndex = 16;
            this.Label9.Text = "Description :";
            // 
            // rtbDescription
            // 
            this.rtbDescription.Location = new System.Drawing.Point(180, 332);
            this.rtbDescription.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.Size = new System.Drawing.Size(294, 78);
            this.rtbDescription.TabIndex = 18;
            this.rtbDescription.Text = "";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(7, 521);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(131, 27);
            this.Label8.TabIndex = 19;
            this.Label8.Text = "Date d\'ajout :";
            // 
            // dtpDateAjout
            // 
            this.dtpDateAjout.Location = new System.Drawing.Point(136, 521);
            this.dtpDateAjout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpDateAjout.Name = "dtpDateAjout";
            this.dtpDateAjout.Size = new System.Drawing.Size(109, 27);
            this.dtpDateAjout.TabIndex = 20;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label10.Location = new System.Drawing.Point(13, 488);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(76, 27);
            this.Label10.TabIndex = 21;
            this.Label10.Text = "Image :";
            // 
            // picProduit
            // 
            this.picProduit.Location = new System.Drawing.Point(253, 488);
            this.picProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picProduit.Name = "picProduit";
            this.picProduit.Size = new System.Drawing.Size(203, 154);
            this.picProduit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picProduit.TabIndex = 22;
            this.picProduit.TabStop = false;
            // 
            // btnChangerImage
            // 
            this.btnChangerImage.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangerImage.ForeColor = System.Drawing.Color.Blue;
            this.btnChangerImage.Location = new System.Drawing.Point(4, 558);
            this.btnChangerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnChangerImage.Name = "btnChangerImage";
            this.btnChangerImage.Size = new System.Drawing.Size(149, 39);
            this.btnChangerImage.TabIndex = 23;
            this.btnChangerImage.Text = "Changer Image";
            this.btnChangerImage.UseVisualStyleBackColor = true;
            this.btnChangerImage.Click += new System.EventHandler(this.btnChangerImage_Click);
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("HP Simplified", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.Black;
            this.btnOk.Location = new System.Drawing.Point(3, 652);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(73, 38);
            this.btnOk.TabIndex = 24;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btnAnnuler.Location = new System.Drawing.Point(217, 652);
            this.btnAnnuler.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(99, 38);
            this.btnAnnuler.TabIndex = 25;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSupprimer.Location = new System.Drawing.Point(359, 652);
            this.btnSupprimer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(115, 38);
            this.btnSupprimer.TabIndex = 26;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnDetails
            // 
            this.btnDetails.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetails.ForeColor = System.Drawing.Color.Black;
            this.btnDetails.Location = new System.Drawing.Point(4, 700);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(88, 38);
            this.btnDetails.TabIndex = 27;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnModifier
            // 
            this.btnModifier.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModifier.ForeColor = System.Drawing.Color.Black;
            this.btnModifier.Location = new System.Drawing.Point(100, 652);
            this.btnModifier.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnModifier.Name = "btnModifier";
            this.btnModifier.Size = new System.Drawing.Size(93, 38);
            this.btnModifier.TabIndex = 28;
            this.btnModifier.Text = "Modifier";
            this.btnModifier.UseVisualStyleBackColor = true;
            this.btnModifier.Click += new System.EventHandler(this.btnModifier_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(17, 303);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(116, 27);
            this.label11.TabIndex = 29;
            this.label11.Text = "Scan Code :";
            // 
            // txtScanCode
            // 
            this.txtScanCode.Location = new System.Drawing.Point(180, 303);
            this.txtScanCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtScanCode.Name = "txtScanCode";
            this.txtScanCode.Size = new System.Drawing.Size(212, 27);
            this.txtScanCode.TabIndex = 30;
            // 
            // lblDevise
            // 
            this.lblDevise.AutoSize = true;
            this.lblDevise.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDevise.Location = new System.Drawing.Point(20, 272);
            this.lblDevise.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDevise.Name = "lblDevise";
            this.lblDevise.Size = new System.Drawing.Size(80, 27);
            this.lblDevise.TabIndex = 31;
            this.lblDevise.Text = "Devise :";
            // 
            // cmbDevise
            // 
            this.cmbDevise.BackColor = System.Drawing.Color.White;
            this.cmbDevise.FormattingEnabled = true;
            this.cmbDevise.Location = new System.Drawing.Point(183, 270);
            this.cmbDevise.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbDevise.Name = "cmbDevise";
            this.cmbDevise.Size = new System.Drawing.Size(97, 28);
            this.cmbDevise.TabIndex = 32;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(288, 273);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 23);
            this.label12.TabIndex = 33;
            this.label12.Text = "ID";
            // 
            // txtIDProduit
            // 
            this.txtIDProduit.Location = new System.Drawing.Point(326, 270);
            this.txtIDProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtIDProduit.Name = "txtIDProduit";
            this.txtIDProduit.Size = new System.Drawing.Size(66, 27);
            this.txtIDProduit.TabIndex = 34;
            // 
            // dgvProduits
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvProduits.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProduits.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvProduits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvProduits.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvProduits.Location = new System.Drawing.Point(481, 3);
            this.dgvProduits.Name = "dgvProduits";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProduits.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvProduits.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvProduits.Size = new System.Drawing.Size(884, 498);
            this.dgvProduits.TabIndex = 35;
            // 
            // btnImprimerEtiquettes
            // 
            this.btnImprimerEtiquettes.BackColor = System.Drawing.Color.Blue;
            this.btnImprimerEtiquettes.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImprimerEtiquettes.ForeColor = System.Drawing.Color.Transparent;
            this.btnImprimerEtiquettes.Location = new System.Drawing.Point(289, 701);
            this.btnImprimerEtiquettes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnImprimerEtiquettes.Name = "btnImprimerEtiquettes";
            this.btnImprimerEtiquettes.Size = new System.Drawing.Size(187, 38);
            this.btnImprimerEtiquettes.TabIndex = 36;
            this.btnImprimerEtiquettes.Text = "Imprimer Etiquettes";
            this.btnImprimerEtiquettes.UseVisualStyleBackColor = false;
            this.btnImprimerEtiquettes.Click += new System.EventHandler(this.btnImprimerEtiquettes_Click);
            // 
            // btnGenererCodeBarre
            // 
            this.btnGenererCodeBarre.BackColor = System.Drawing.Color.Blue;
            this.btnGenererCodeBarre.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenererCodeBarre.ForeColor = System.Drawing.Color.Transparent;
            this.btnGenererCodeBarre.Location = new System.Drawing.Point(100, 701);
            this.btnGenererCodeBarre.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGenererCodeBarre.Name = "btnGenererCodeBarre";
            this.btnGenererCodeBarre.Size = new System.Drawing.Size(187, 38);
            this.btnGenererCodeBarre.TabIndex = 37;
            this.btnGenererCodeBarre.Text = "Générer CodeBarre";
            this.btnGenererCodeBarre.UseVisualStyleBackColor = false;
            this.btnGenererCodeBarre.Click += new System.EventHandler(this.btnGenererCodeBarre_Click);
            // 
            // txtCodeBarreProduit
            // 
            this.txtCodeBarreProduit.Location = new System.Drawing.Point(136, 420);
            this.txtCodeBarreProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCodeBarreProduit.Name = "txtCodeBarreProduit";
            this.txtCodeBarreProduit.Size = new System.Drawing.Size(338, 27);
            this.txtCodeBarreProduit.TabIndex = 38;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(13, 419);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(125, 27);
            this.label13.TabIndex = 39;
            this.label13.Text = "Code Barre :";
            // 
            // cmbFormatEtiquettes
            // 
            this.cmbFormatEtiquettes.BackColor = System.Drawing.Color.White;
            this.cmbFormatEtiquettes.FormattingEnabled = true;
            this.cmbFormatEtiquettes.Location = new System.Drawing.Point(240, 450);
            this.cmbFormatEtiquettes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbFormatEtiquettes.Name = "cmbFormatEtiquettes";
            this.cmbFormatEtiquettes.Size = new System.Drawing.Size(234, 28);
            this.cmbFormatEtiquettes.TabIndex = 40;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(13, 451);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(185, 27);
            this.label14.TabIndex = 41;
            this.label14.Text = "Format Etiquettes :";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(248, 197);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(110, 23);
            this.label15.TabIndex = 42;
            this.label15.Text = "DépôtStock :";
            // 
            // cmbDepotStockInitial
            // 
            this.cmbDepotStockInitial.FormattingEnabled = true;
            this.cmbDepotStockInitial.Location = new System.Drawing.Point(366, 195);
            this.cmbDepotStockInitial.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbDepotStockInitial.Name = "cmbDepotStockInitial";
            this.cmbDepotStockInitial.Size = new System.Drawing.Size(110, 28);
            this.cmbDepotStockInitial.TabIndex = 43;
            // 
            // btnStockInitial
            // 
            this.btnStockInitial.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStockInitial.ForeColor = System.Drawing.Color.Black;
            this.btnStockInitial.Location = new System.Drawing.Point(4, 604);
            this.btnStockInitial.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStockInitial.Name = "btnStockInitial";
            this.btnStockInitial.Size = new System.Drawing.Size(164, 38);
            this.btnStockInitial.TabIndex = 44;
            this.btnStockInitial.Text = "Stock Initial";
            this.btnStockInitial.UseVisualStyleBackColor = true;
            this.btnStockInitial.Click += new System.EventHandler(this.btnStockInitial_Click);
            // 
            // btnAjouterEquivalence
            // 
            this.btnAjouterEquivalence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnAjouterEquivalence.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAjouterEquivalence.ForeColor = System.Drawing.Color.Transparent;
            this.btnAjouterEquivalence.Location = new System.Drawing.Point(520, 700);
            this.btnAjouterEquivalence.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAjouterEquivalence.Name = "btnAjouterEquivalence";
            this.btnAjouterEquivalence.Size = new System.Drawing.Size(187, 38);
            this.btnAjouterEquivalence.TabIndex = 45;
            this.btnAjouterEquivalence.Text = "Ajouter Equivalence";
            this.btnAjouterEquivalence.UseVisualStyleBackColor = false;
            this.btnAjouterEquivalence.Click += new System.EventHandler(this.btnAjouterEquivalence_Click);
            // 
            // btnSupprimerEquivalence
            // 
            this.btnSupprimerEquivalence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSupprimerEquivalence.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimerEquivalence.ForeColor = System.Drawing.Color.Transparent;
            this.btnSupprimerEquivalence.Location = new System.Drawing.Point(1046, 701);
            this.btnSupprimerEquivalence.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSupprimerEquivalence.Name = "btnSupprimerEquivalence";
            this.btnSupprimerEquivalence.Size = new System.Drawing.Size(219, 38);
            this.btnSupprimerEquivalence.TabIndex = 46;
            this.btnSupprimerEquivalence.Text = "Supprimer Equivalence";
            this.btnSupprimerEquivalence.UseVisualStyleBackColor = false;
            this.btnSupprimerEquivalence.Click += new System.EventHandler(this.btnSupprimerEquivalence_Click);
            // 
            // cmbEquivalent
            // 
            this.cmbEquivalent.BackColor = System.Drawing.Color.White;
            this.cmbEquivalent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEquivalent.FormattingEnabled = true;
            this.cmbEquivalent.Location = new System.Drawing.Point(838, 707);
            this.cmbEquivalent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbEquivalent.Name = "cmbEquivalent";
            this.cmbEquivalent.Size = new System.Drawing.Size(200, 28);
            this.cmbEquivalent.TabIndex = 47;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(715, 708);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(115, 27);
            this.label16.TabIndex = 48;
            this.label16.Text = "Equivalent :";
            // 
            // FormProduits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Teal;
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.cmbEquivalent);
            this.Controls.Add(this.btnSupprimerEquivalence);
            this.Controls.Add(this.btnAjouterEquivalence);
            this.Controls.Add(this.btnStockInitial);
            this.Controls.Add(this.cmbDepotStockInitial);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.cmbFormatEtiquettes);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtCodeBarreProduit);
            this.Controls.Add(this.btnGenererCodeBarre);
            this.Controls.Add(this.btnImprimerEtiquettes);
            this.Controls.Add(this.dgvProduits);
            this.Controls.Add(this.txtIDProduit);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.cmbDevise);
            this.Controls.Add(this.lblDevise);
            this.Controls.Add(this.txtScanCode);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnChangerImage);
            this.Controls.Add(this.picProduit);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.dtpDateAjout);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.rtbDescription);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.txtPrix);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.numQuantite);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.txtCouleur);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.cmbTaille);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.cmbCategorie);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.txtNomProduit);
            this.Controls.Add(this.Label1);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormProduits";
            this.Text = "FormProduits";
            this.Load += new System.EventHandler(this.FormProduits_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduits)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox txtNomProduit;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox txtReference;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ComboBox cmbCategorie;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.ComboBox cmbTaille;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.TextBox txtCouleur;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.NumericUpDown numQuantite;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.TextBox txtPrix;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.RichTextBox rtbDescription;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.DateTimePicker dtpDateAjout;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.PictureBox picProduit;
        internal System.Windows.Forms.Button btnChangerImage;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Button btnAnnuler;
        internal System.Windows.Forms.Button btnSupprimer;
        internal System.Windows.Forms.Button btnDetails;
        internal System.Windows.Forms.Button btnModifier;
        internal System.Windows.Forms.Label label11;
        internal System.Windows.Forms.TextBox txtScanCode;
        internal System.Windows.Forms.Label lblDevise;
        internal System.Windows.Forms.ComboBox cmbDevise;
        internal System.Windows.Forms.Label label12;
        internal System.Windows.Forms.TextBox txtIDProduit;
        private System.Windows.Forms.DataGridView dgvProduits;
        internal System.Windows.Forms.Button btnImprimerEtiquettes;
        internal System.Windows.Forms.Button btnGenererCodeBarre;
        internal System.Windows.Forms.TextBox txtCodeBarreProduit;
        internal System.Windows.Forms.Label label13;
        internal System.Windows.Forms.ComboBox cmbFormatEtiquettes;
        internal System.Windows.Forms.Label label14;
        internal System.Windows.Forms.Label label15;
        internal System.Windows.Forms.ComboBox cmbDepotStockInitial;
        internal System.Windows.Forms.Button btnStockInitial;
        internal System.Windows.Forms.Button btnAjouterEquivalence;
        internal System.Windows.Forms.Button btnSupprimerEquivalence;
        internal System.Windows.Forms.ComboBox cmbEquivalent;
        internal System.Windows.Forms.Label label16;
    }
}