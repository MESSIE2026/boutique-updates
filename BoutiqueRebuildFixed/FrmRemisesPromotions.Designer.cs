namespace BoutiqueRebuildFixed
{
    partial class FrmRemisesPromotions
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
            this.grpDetails = new System.Windows.Forms.GroupBox();
            this.lblNom = new System.Windows.Forms.Label();
            this.txtNomPromotion = new System.Windows.Forms.TextBox();
            this.lblType = new System.Windows.Forms.Label();
            this.cboTypeRemise = new System.Windows.Forms.ComboBox();
            this.lblValeur = new System.Windows.Forms.Label();
            this.nudValeur = new System.Windows.Forms.NumericUpDown();
            this.lblPourcent = new System.Windows.Forms.Label();
            this.lblDebut = new System.Windows.Forms.Label();
            this.dtDebut = new System.Windows.Forms.DateTimePicker();
            this.lblFin = new System.Windows.Forms.Label();
            this.dtFin = new System.Windows.Forms.DateTimePicker();
            this.grpProduits = new System.Windows.Forms.GroupBox();
            this.lblProduits = new System.Windows.Forms.Label();
            this.lstProduits = new System.Windows.Forms.ListBox();
            this.btnAjouter = new System.Windows.Forms.Button();
            this.btnRetirer = new System.Windows.Forms.Button();
            this.btnEffacer = new System.Windows.Forms.Button();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboProduits = new System.Windows.Forms.ComboBox();
            this.lblCreerPar = new System.Windows.Forms.Label();
            this.txtCreerPar = new System.Windows.Forms.TextBox();
            this.dgvPromotions = new System.Windows.Forms.DataGridView();
            this.grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudValeur)).BeginInit();
            this.grpProduits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPromotions)).BeginInit();
            this.SuspendLayout();
            // 
            // grpDetails
            // 
            this.grpDetails.Controls.Add(this.txtCreerPar);
            this.grpDetails.Controls.Add(this.lblCreerPar);
            this.grpDetails.Controls.Add(this.dtFin);
            this.grpDetails.Controls.Add(this.lblFin);
            this.grpDetails.Controls.Add(this.dtDebut);
            this.grpDetails.Controls.Add(this.lblDebut);
            this.grpDetails.Controls.Add(this.lblPourcent);
            this.grpDetails.Controls.Add(this.nudValeur);
            this.grpDetails.Controls.Add(this.lblValeur);
            this.grpDetails.Controls.Add(this.cboTypeRemise);
            this.grpDetails.Controls.Add(this.lblType);
            this.grpDetails.Controls.Add(this.txtNomPromotion);
            this.grpDetails.Controls.Add(this.lblNom);
            this.grpDetails.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpDetails.ForeColor = System.Drawing.Color.White;
            this.grpDetails.Location = new System.Drawing.Point(54, 3);
            this.grpDetails.Name = "grpDetails";
            this.grpDetails.Size = new System.Drawing.Size(1118, 221);
            this.grpDetails.TabIndex = 0;
            this.grpDetails.TabStop = false;
            this.grpDetails.Text = "Détails de la Promotion";
            // 
            // lblNom
            // 
            this.lblNom.AutoSize = true;
            this.lblNom.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNom.Location = new System.Drawing.Point(87, 29);
            this.lblNom.Name = "lblNom";
            this.lblNom.Size = new System.Drawing.Size(215, 25);
            this.lblNom.TabIndex = 0;
            this.lblNom.Text = "Nom de la Promotion :";
            // 
            // txtNomPromotion
            // 
            this.txtNomPromotion.Location = new System.Drawing.Point(331, 21);
            this.txtNomPromotion.Name = "txtNomPromotion";
            this.txtNomPromotion.Size = new System.Drawing.Size(726, 33);
            this.txtNomPromotion.TabIndex = 1;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.Location = new System.Drawing.Point(87, 63);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(158, 25);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Type de Remise :";
            // 
            // cboTypeRemise
            // 
            this.cboTypeRemise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTypeRemise.FormattingEnabled = true;
            this.cboTypeRemise.Location = new System.Drawing.Point(331, 60);
            this.cboTypeRemise.Name = "cboTypeRemise";
            this.cboTypeRemise.Size = new System.Drawing.Size(269, 33);
            this.cboTypeRemise.TabIndex = 3;
            // 
            // lblValeur
            // 
            this.lblValeur.AutoSize = true;
            this.lblValeur.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValeur.Location = new System.Drawing.Point(87, 100);
            this.lblValeur.Name = "lblValeur";
            this.lblValeur.Size = new System.Drawing.Size(193, 25);
            this.lblValeur.TabIndex = 4;
            this.lblValeur.Text = "Valeur de la Remise :";
            // 
            // nudValeur
            // 
            this.nudValeur.DecimalPlaces = 2;
            this.nudValeur.Location = new System.Drawing.Point(331, 100);
            this.nudValeur.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudValeur.Name = "nudValeur";
            this.nudValeur.Size = new System.Drawing.Size(122, 33);
            this.nudValeur.TabIndex = 5;
            // 
            // lblPourcent
            // 
            this.lblPourcent.AutoSize = true;
            this.lblPourcent.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPourcent.Location = new System.Drawing.Point(459, 108);
            this.lblPourcent.Name = "lblPourcent";
            this.lblPourcent.Size = new System.Drawing.Size(28, 25);
            this.lblPourcent.TabIndex = 6;
            this.lblPourcent.Text = "%";
            // 
            // lblDebut
            // 
            this.lblDebut.AutoSize = true;
            this.lblDebut.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDebut.Location = new System.Drawing.Point(87, 140);
            this.lblDebut.Name = "lblDebut";
            this.lblDebut.Size = new System.Drawing.Size(150, 25);
            this.lblDebut.TabIndex = 7;
            this.lblDebut.Text = "Date de Début :";
            // 
            // dtDebut
            // 
            this.dtDebut.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtDebut.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtDebut.Location = new System.Drawing.Point(331, 140);
            this.dtDebut.Name = "dtDebut";
            this.dtDebut.Size = new System.Drawing.Size(170, 33);
            this.dtDebut.TabIndex = 8;
            // 
            // lblFin
            // 
            this.lblFin.AutoSize = true;
            this.lblFin.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFin.Location = new System.Drawing.Point(87, 175);
            this.lblFin.Name = "lblFin";
            this.lblFin.Size = new System.Drawing.Size(122, 25);
            this.lblFin.TabIndex = 9;
            this.lblFin.Text = "Date de Fin :";
            // 
            // dtFin
            // 
            this.dtFin.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtFin.Location = new System.Drawing.Point(331, 179);
            this.dtFin.Name = "dtFin";
            this.dtFin.Size = new System.Drawing.Size(170, 33);
            this.dtFin.TabIndex = 10;
            // 
            // grpProduits
            // 
            this.grpProduits.Controls.Add(this.cboProduits);
            this.grpProduits.Controls.Add(this.label1);
            this.grpProduits.Controls.Add(this.btnEffacer);
            this.grpProduits.Controls.Add(this.btnRetirer);
            this.grpProduits.Controls.Add(this.btnAjouter);
            this.grpProduits.Controls.Add(this.lstProduits);
            this.grpProduits.Controls.Add(this.lblProduits);
            this.grpProduits.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpProduits.ForeColor = System.Drawing.Color.White;
            this.grpProduits.Location = new System.Drawing.Point(54, 230);
            this.grpProduits.Name = "grpProduits";
            this.grpProduits.Size = new System.Drawing.Size(1118, 196);
            this.grpProduits.TabIndex = 1;
            this.grpProduits.TabStop = false;
            this.grpProduits.Text = "Produits Selectionnés";
            // 
            // lblProduits
            // 
            this.lblProduits.AutoSize = true;
            this.lblProduits.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProduits.Location = new System.Drawing.Point(18, 54);
            this.lblProduits.Name = "lblProduits";
            this.lblProduits.Size = new System.Drawing.Size(169, 25);
            this.lblProduits.TabIndex = 0;
            this.lblProduits.Text = "Produits remisés :";
            // 
            // lstProduits
            // 
            this.lstProduits.FormattingEnabled = true;
            this.lstProduits.ItemHeight = 25;
            this.lstProduits.Location = new System.Drawing.Point(193, 57);
            this.lstProduits.Name = "lstProduits";
            this.lstProduits.Size = new System.Drawing.Size(348, 129);
            this.lstProduits.TabIndex = 1;
            // 
            // btnAjouter
            // 
            this.btnAjouter.ForeColor = System.Drawing.Color.Black;
            this.btnAjouter.Location = new System.Drawing.Point(925, 47);
            this.btnAjouter.Name = "btnAjouter";
            this.btnAjouter.Size = new System.Drawing.Size(149, 44);
            this.btnAjouter.TabIndex = 2;
            this.btnAjouter.Text = "Ajouter >>";
            this.btnAjouter.UseVisualStyleBackColor = true;
            this.btnAjouter.Click += new System.EventHandler(this.btnAjouter_Click);
            // 
            // btnRetirer
            // 
            this.btnRetirer.ForeColor = System.Drawing.Color.Black;
            this.btnRetirer.Location = new System.Drawing.Point(925, 97);
            this.btnRetirer.Name = "btnRetirer";
            this.btnRetirer.Size = new System.Drawing.Size(149, 44);
            this.btnRetirer.TabIndex = 3;
            this.btnRetirer.Text = "<< Retirer";
            this.btnRetirer.UseVisualStyleBackColor = true;
            this.btnRetirer.Click += new System.EventHandler(this.btnRetirer_Click);
            // 
            // btnEffacer
            // 
            this.btnEffacer.ForeColor = System.Drawing.Color.Black;
            this.btnEffacer.Location = new System.Drawing.Point(925, 147);
            this.btnEffacer.Name = "btnEffacer";
            this.btnEffacer.Size = new System.Drawing.Size(149, 44);
            this.btnEffacer.TabIndex = 4;
            this.btnEffacer.Text = "Effacer la Liste";
            this.btnEffacer.UseVisualStyleBackColor = true;
            this.btnEffacer.Click += new System.EventHandler(this.btnEffacer_Click);
            // 
            // btnEnregistrer
            // 
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnEnregistrer.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.Location = new System.Drawing.Point(690, 432);
            this.btnEnregistrer.Name = "btnEnregistrer";
            this.btnEnregistrer.Size = new System.Drawing.Size(132, 33);
            this.btnEnregistrer.TabIndex = 5;
            this.btnEnregistrer.Text = "Enregistrer";
            this.btnEnregistrer.UseVisualStyleBackColor = false;
            this.btnEnregistrer.Click += new System.EventHandler(this.btnEnregistrer_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.BackColor = System.Drawing.Color.Black;
            this.btnAnnuler.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.White;
            this.btnAnnuler.Location = new System.Drawing.Point(875, 432);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(122, 33);
            this.btnAnnuler.TabIndex = 6;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = false;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnFermer
            // 
            this.btnFermer.BackColor = System.Drawing.Color.Maroon;
            this.btnFermer.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.Location = new System.Drawing.Point(1043, 432);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(129, 33);
            this.btnFermer.TabIndex = 7;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = false;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(547, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Produits  :";
            // 
            // cboProduits
            // 
            this.cboProduits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProduits.FormattingEnabled = true;
            this.cboProduits.Location = new System.Drawing.Point(649, 51);
            this.cboProduits.Name = "cboProduits";
            this.cboProduits.Size = new System.Drawing.Size(253, 33);
            this.cboProduits.TabIndex = 6;
            // 
            // lblCreerPar
            // 
            this.lblCreerPar.AutoSize = true;
            this.lblCreerPar.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCreerPar.Location = new System.Drawing.Point(616, 68);
            this.lblCreerPar.Name = "lblCreerPar";
            this.lblCreerPar.Size = new System.Drawing.Size(110, 25);
            this.lblCreerPar.TabIndex = 11;
            this.lblCreerPar.Text = "Créer Par : ";
            // 
            // txtCreerPar
            // 
            this.txtCreerPar.Location = new System.Drawing.Point(732, 65);
            this.txtCreerPar.Name = "txtCreerPar";
            this.txtCreerPar.Size = new System.Drawing.Size(325, 33);
            this.txtCreerPar.TabIndex = 12;
            // 
            // dgvPromotions
            // 
            this.dgvPromotions.AllowUserToAddRows = false;
            this.dgvPromotions.AllowUserToDeleteRows = false;
            this.dgvPromotions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPromotions.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPromotions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPromotions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPromotions.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPromotions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvPromotions.Location = new System.Drawing.Point(0, 471);
            this.dgvPromotions.MultiSelect = false;
            this.dgvPromotions.Name = "dgvPromotions";
            this.dgvPromotions.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPromotions.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvPromotions.RowHeadersVisible = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvPromotions.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvPromotions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPromotions.Size = new System.Drawing.Size(1200, 242);
            this.dgvPromotions.TabIndex = 8;
            // 
            // FrmRemisesPromotions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1200, 713);
            this.Controls.Add(this.dgvPromotions);
            this.Controls.Add(this.btnFermer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.grpProduits);
            this.Controls.Add(this.grpDetails);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "FrmRemisesPromotions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remises & Promotions";
            this.Load += new System.EventHandler(this.FrmRemisesPromotions_Load);
            this.grpDetails.ResumeLayout(false);
            this.grpDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudValeur)).EndInit();
            this.grpProduits.ResumeLayout(false);
            this.grpProduits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPromotions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDetails;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TextBox txtNomPromotion;
        private System.Windows.Forms.Label lblNom;
        private System.Windows.Forms.NumericUpDown nudValeur;
        private System.Windows.Forms.Label lblValeur;
        private System.Windows.Forms.ComboBox cboTypeRemise;
        private System.Windows.Forms.Label lblPourcent;
        private System.Windows.Forms.DateTimePicker dtDebut;
        private System.Windows.Forms.Label lblDebut;
        private System.Windows.Forms.Label lblFin;
        private System.Windows.Forms.DateTimePicker dtFin;
        private System.Windows.Forms.GroupBox grpProduits;
        private System.Windows.Forms.Label lblProduits;
        private System.Windows.Forms.Button btnRetirer;
        private System.Windows.Forms.Button btnAjouter;
        private System.Windows.Forms.ListBox lstProduits;
        private System.Windows.Forms.Button btnEffacer;
        private System.Windows.Forms.Button btnEnregistrer;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.ComboBox cboProduits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCreerPar;
        private System.Windows.Forms.Label lblCreerPar;
        private System.Windows.Forms.DataGridView dgvPromotions;
    }
}