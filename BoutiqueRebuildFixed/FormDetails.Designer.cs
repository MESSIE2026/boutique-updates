namespace BoutiqueRebuildFixed
{
    partial class FormDetails
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
            this.txtPrixUnitaire = new System.Windows.Forms.TextBox();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnChangerImage = new System.Windows.Forms.Button();
            this.picProduit = new System.Windows.Forms.PictureBox();
            this.Label10 = new System.Windows.Forms.Label();
            this.dtpDateAjout = new System.Windows.Forms.DateTimePicker();
            this.Label9 = new System.Windows.Forms.Label();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.numQuantite = new System.Windows.Forms.NumericUpDown();
            this.Label6 = new System.Windows.Forms.Label();
            this.txtCouleur = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.cmbTaille = new System.Windows.Forms.ComboBox();
            this.Label4 = new System.Windows.Forms.Label();
            this.txtRefProduit = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.cmbCategorie = new System.Windows.Forms.ComboBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.dgvDetailsVente = new System.Windows.Forms.DataGridView();
            this.txtIDVente = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtIDProduit = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtRemise = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtTVA = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtDevise = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtNomCaissier = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailsVente)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPrixUnitaire
            // 
            this.txtPrixUnitaire.Location = new System.Drawing.Point(192, 241);
            this.txtPrixUnitaire.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPrixUnitaire.Name = "txtPrixUnitaire";
            this.txtPrixUnitaire.Size = new System.Drawing.Size(155, 27);
            this.txtPrixUnitaire.TabIndex = 51;
            // 
            // btnDetails
            // 
            this.btnDetails.Font = new System.Drawing.Font("Microsoft New Tai Lue", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetails.ForeColor = System.Drawing.Color.Navy;
            this.btnDetails.Location = new System.Drawing.Point(430, 685);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(121, 35);
            this.btnDetails.TabIndex = 50;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSupprimer.Location = new System.Drawing.Point(287, 685);
            this.btnSupprimer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(135, 35);
            this.btnSupprimer.TabIndex = 49;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btnAnnuler.Location = new System.Drawing.Point(154, 685);
            this.btnAnnuler.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(125, 35);
            this.btnAnnuler.TabIndex = 48;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.Black;
            this.btnOk.Location = new System.Drawing.Point(12, 686);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 35);
            this.btnOk.TabIndex = 47;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnChangerImage
            // 
            this.btnChangerImage.Font = new System.Drawing.Font("Microsoft New Tai Lue", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangerImage.ForeColor = System.Drawing.Color.Navy;
            this.btnChangerImage.Location = new System.Drawing.Point(12, 572);
            this.btnChangerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnChangerImage.Name = "btnChangerImage";
            this.btnChangerImage.Size = new System.Drawing.Size(160, 35);
            this.btnChangerImage.TabIndex = 46;
            this.btnChangerImage.Text = "Changer Image";
            this.btnChangerImage.UseVisualStyleBackColor = true;
            this.btnChangerImage.Click += new System.EventHandler(this.btnChangerImage_Click);
            // 
            // picProduit
            // 
            this.picProduit.Location = new System.Drawing.Point(309, 528);
            this.picProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picProduit.Name = "picProduit";
            this.picProduit.Size = new System.Drawing.Size(240, 147);
            this.picProduit.TabIndex = 45;
            this.picProduit.TabStop = false;
            this.picProduit.Click += new System.EventHandler(this.picProduit_Click);
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label10.Location = new System.Drawing.Point(18, 528);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(76, 27);
            this.Label10.TabIndex = 44;
            this.Label10.Text = "Image :";
            // 
            // dtpDateAjout
            // 
            this.dtpDateAjout.Location = new System.Drawing.Point(320, 485);
            this.dtpDateAjout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpDateAjout.Name = "dtpDateAjout";
            this.dtpDateAjout.Size = new System.Drawing.Size(229, 27);
            this.dtpDateAjout.TabIndex = 43;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(18, 486);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(120, 27);
            this.Label9.TabIndex = 42;
            this.Label9.Text = "Date Ajout :";
            // 
            // rtbDescription
            // 
            this.rtbDescription.Location = new System.Drawing.Point(198, 380);
            this.rtbDescription.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.Size = new System.Drawing.Size(351, 95);
            this.rtbDescription.TabIndex = 41;
            this.rtbDescription.Text = "";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(18, 379);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(125, 27);
            this.Label8.TabIndex = 40;
            this.Label8.Text = "Description :";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(18, 240);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(58, 27);
            this.Label7.TabIndex = 39;
            this.Label7.Text = "Prix :";
            // 
            // numQuantite
            // 
            this.numQuantite.Location = new System.Drawing.Point(193, 204);
            this.numQuantite.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numQuantite.Name = "numQuantite";
            this.numQuantite.Size = new System.Drawing.Size(155, 27);
            this.numQuantite.TabIndex = 38;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(18, 204);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(102, 27);
            this.Label6.TabIndex = 37;
            this.Label6.Text = "Quantite :";
            // 
            // txtCouleur
            // 
            this.txtCouleur.Location = new System.Drawing.Point(192, 168);
            this.txtCouleur.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtCouleur.Name = "txtCouleur";
            this.txtCouleur.Size = new System.Drawing.Size(247, 27);
            this.txtCouleur.TabIndex = 36;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(18, 165);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(94, 27);
            this.Label5.TabIndex = 35;
            this.Label5.Text = "Couleur :";
            // 
            // cmbTaille
            // 
            this.cmbTaille.FormattingEnabled = true;
            this.cmbTaille.Location = new System.Drawing.Point(193, 130);
            this.cmbTaille.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbTaille.Name = "cmbTaille";
            this.cmbTaille.Size = new System.Drawing.Size(247, 28);
            this.cmbTaille.TabIndex = 34;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(18, 129);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(71, 27);
            this.Label4.TabIndex = 33;
            this.Label4.Text = "Taille :";
            // 
            // txtRefProduit
            // 
            this.txtRefProduit.Location = new System.Drawing.Point(192, 90);
            this.txtRefProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRefProduit.Name = "txtRefProduit";
            this.txtRefProduit.Size = new System.Drawing.Size(247, 27);
            this.txtRefProduit.TabIndex = 32;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(18, 91);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(111, 27);
            this.Label3.TabIndex = 31;
            this.Label3.Text = "Reference :";
            // 
            // cmbCategorie
            // 
            this.cmbCategorie.FormattingEnabled = true;
            this.cmbCategorie.Location = new System.Drawing.Point(192, 52);
            this.cmbCategorie.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbCategorie.Name = "cmbCategorie";
            this.cmbCategorie.Size = new System.Drawing.Size(247, 28);
            this.cmbCategorie.TabIndex = 30;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(13, 53);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(110, 27);
            this.Label2.TabIndex = 29;
            this.Label2.Text = "Categorie :";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(193, 13);
            this.txtNomProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(247, 27);
            this.txtNomProduit.TabIndex = 28;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(18, 13);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(166, 27);
            this.Label1.TabIndex = 27;
            this.Label1.Text = "Nom du Produit :";
            // 
            // dgvDetailsVente
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvDetailsVente.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDetailsVente.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetailsVente.BackgroundColor = System.Drawing.Color.Silver;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDetailsVente.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvDetailsVente.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDetailsVente.Location = new System.Drawing.Point(556, 8);
            this.dgvDetailsVente.Name = "dgvDetailsVente";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDetailsVente.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvDetailsVente.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvDetailsVente.Size = new System.Drawing.Size(802, 729);
            this.dgvDetailsVente.TabIndex = 52;
            // 
            // txtIDVente
            // 
            this.txtIDVente.Location = new System.Drawing.Point(124, 278);
            this.txtIDVente.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtIDVente.Name = "txtIDVente";
            this.txtIDVente.Size = new System.Drawing.Size(155, 27);
            this.txtIDVente.TabIndex = 53;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(18, 277);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 27);
            this.label11.TabIndex = 54;
            this.label11.Text = "ID Vente :";
            // 
            // txtIDProduit
            // 
            this.txtIDProduit.Location = new System.Drawing.Point(402, 276);
            this.txtIDProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtIDProduit.Name = "txtIDProduit";
            this.txtIDProduit.Size = new System.Drawing.Size(140, 27);
            this.txtIDProduit.TabIndex = 55;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(285, 276);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(115, 27);
            this.label12.TabIndex = 56;
            this.label12.Text = "ID Produit :";
            // 
            // txtRemise
            // 
            this.txtRemise.Location = new System.Drawing.Point(106, 315);
            this.txtRemise.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtRemise.Name = "txtRemise";
            this.txtRemise.Size = new System.Drawing.Size(92, 27);
            this.txtRemise.TabIndex = 57;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(15, 314);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(86, 27);
            this.label13.TabIndex = 58;
            this.label13.Text = "Remise :";
            // 
            // txtTVA
            // 
            this.txtTVA.Location = new System.Drawing.Point(268, 316);
            this.txtTVA.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTVA.Name = "txtTVA";
            this.txtTVA.Size = new System.Drawing.Size(92, 27);
            this.txtTVA.TabIndex = 59;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(206, 315);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 27);
            this.label14.TabIndex = 60;
            this.label14.Text = "TVA :";
            // 
            // txtDevise
            // 
            this.txtDevise.Location = new System.Drawing.Point(450, 316);
            this.txtDevise.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDevise.Name = "txtDevise";
            this.txtDevise.Size = new System.Drawing.Size(92, 27);
            this.txtDevise.TabIndex = 61;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(368, 316);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(80, 27);
            this.label15.TabIndex = 62;
            this.label15.Text = "Devise :";
            // 
            // txtNomCaissier
            // 
            this.txtNomCaissier.Location = new System.Drawing.Point(165, 347);
            this.txtNomCaissier.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNomCaissier.Name = "txtNomCaissier";
            this.txtNomCaissier.Size = new System.Drawing.Size(195, 27);
            this.txtNomCaissier.TabIndex = 63;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(15, 346);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(142, 27);
            this.label16.TabIndex = 64;
            this.label16.Text = "Nom Caissier :";
            // 
            // FormDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.txtNomCaissier);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.txtDevise);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtTVA);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtRemise);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtIDProduit);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtIDVente);
            this.Controls.Add(this.dgvDetailsVente);
            this.Controls.Add(this.txtPrixUnitaire);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnChangerImage);
            this.Controls.Add(this.picProduit);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.dtpDateAjout);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.rtbDescription);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.numQuantite);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.txtCouleur);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.cmbTaille);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.txtRefProduit);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.cmbCategorie);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.txtNomProduit);
            this.Controls.Add(this.Label1);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormDetails";
            this.Text = "FormDetails";
            this.Load += new System.EventHandler(this.FormDetails_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailsVente)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.TextBox txtPrixUnitaire;
        internal System.Windows.Forms.Button btnDetails;
        internal System.Windows.Forms.Button btnSupprimer;
        internal System.Windows.Forms.Button btnAnnuler;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Button btnChangerImage;
        internal System.Windows.Forms.PictureBox picProduit;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.DateTimePicker dtpDateAjout;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.RichTextBox rtbDescription;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.NumericUpDown numQuantite;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.TextBox txtCouleur;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.ComboBox cmbTaille;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.TextBox txtRefProduit;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.ComboBox cmbCategorie;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox txtNomProduit;
        internal System.Windows.Forms.Label Label1;
        private System.Windows.Forms.DataGridView dgvDetailsVente;
        internal System.Windows.Forms.TextBox txtIDVente;
        internal System.Windows.Forms.Label label11;
        internal System.Windows.Forms.TextBox txtIDProduit;
        internal System.Windows.Forms.Label label12;
        internal System.Windows.Forms.TextBox txtRemise;
        internal System.Windows.Forms.Label label13;
        internal System.Windows.Forms.TextBox txtTVA;
        internal System.Windows.Forms.Label label14;
        internal System.Windows.Forms.TextBox txtDevise;
        internal System.Windows.Forms.Label label15;
        internal System.Windows.Forms.TextBox txtNomCaissier;
        internal System.Windows.Forms.Label label16;
    }
}