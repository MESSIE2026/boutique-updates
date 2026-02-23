namespace BoutiqueRebuildFixed
{
    partial class FormAnnulations
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
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblNomClient = new System.Windows.Forms.Label();
            this.txtNomClient = new System.Windows.Forms.TextBox();
            this.lblNumCommande = new System.Windows.Forms.Label();
            this.txtNumCommande = new System.Windows.Forms.TextBox();
            this.lblDateAchat = new System.Windows.Forms.Label();
            this.dtpDateAchat = new System.Windows.Forms.DateTimePicker();
            this.lblNomProduit = new System.Windows.Forms.Label();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.lblQuantite = new System.Windows.Forms.Label();
            this.nudQuantite = new System.Windows.Forms.NumericUpDown();
            this.lblPrixUnitaire = new System.Windows.Forms.Label();
            this.txtPrixUnitaire = new System.Windows.Forms.TextBox();
            this.lblPrixTotal = new System.Windows.Forms.Label();
            this.txtPrixTotal = new System.Windows.Forms.TextBox();
            this.lblMotifRetour = new System.Windows.Forms.Label();
            this.cmbMotifRetour = new System.Windows.Forms.ComboBox();
            this.lblCommentaires = new System.Windows.Forms.Label();
            this.txtCommentaires = new System.Windows.Forms.TextBox();
            this.grpTypeRetour = new System.Windows.Forms.GroupBox();
            this.rbEchange = new System.Windows.Forms.RadioButton();
            this.rbRemboursement = new System.Windows.Forms.RadioButton();
            this.btnValider = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            this.dgvAnnulationsRetours = new System.Windows.Forms.DataGridView();
            this.lblDevise = new System.Windows.Forms.Label();
            this.cbDevise = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).BeginInit();
            this.grpTypeRetour.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAnnulationsRetours)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblTitre.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitre.Font = new System.Drawing.Font("Britannic Bold", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.Maroon;
            this.lblTitre.Location = new System.Drawing.Point(380, 6);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(327, 38);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "Annulations / Retours";
            // 
            // lblNomClient
            // 
            this.lblNomClient.AutoSize = true;
            this.lblNomClient.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomClient.Location = new System.Drawing.Point(48, 42);
            this.lblNomClient.Name = "lblNomClient";
            this.lblNomClient.Size = new System.Drawing.Size(156, 30);
            this.lblNomClient.TabIndex = 1;
            this.lblNomClient.Text = "Nom du client :";
            // 
            // txtNomClient
            // 
            this.txtNomClient.Location = new System.Drawing.Point(324, 47);
            this.txtNomClient.Name = "txtNomClient";
            this.txtNomClient.Size = new System.Drawing.Size(541, 29);
            this.txtNomClient.TabIndex = 2;
            // 
            // lblNumCommande
            // 
            this.lblNumCommande.AutoSize = true;
            this.lblNumCommande.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNumCommande.Location = new System.Drawing.Point(48, 75);
            this.lblNumCommande.Name = "lblNumCommande";
            this.lblNumCommande.Size = new System.Drawing.Size(239, 30);
            this.lblNumCommande.TabIndex = 3;
            this.lblNumCommande.Text = "Numéro de commande :";
            // 
            // txtNumCommande
            // 
            this.txtNumCommande.Location = new System.Drawing.Point(324, 80);
            this.txtNumCommande.Name = "txtNumCommande";
            this.txtNumCommande.Size = new System.Drawing.Size(246, 29);
            this.txtNumCommande.TabIndex = 4;
            // 
            // lblDateAchat
            // 
            this.lblDateAchat.AutoSize = true;
            this.lblDateAchat.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateAchat.Location = new System.Drawing.Point(48, 107);
            this.lblDateAchat.Name = "lblDateAchat";
            this.lblDateAchat.Size = new System.Drawing.Size(164, 30);
            this.lblDateAchat.TabIndex = 5;
            this.lblDateAchat.Text = "Date de l\'achat :";
            // 
            // dtpDateAchat
            // 
            this.dtpDateAchat.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateAchat.Location = new System.Drawing.Point(324, 114);
            this.dtpDateAchat.Name = "dtpDateAchat";
            this.dtpDateAchat.Size = new System.Drawing.Size(200, 29);
            this.dtpDateAchat.TabIndex = 6;
            // 
            // lblNomProduit
            // 
            this.lblNomProduit.AutoSize = true;
            this.lblNomProduit.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomProduit.Location = new System.Drawing.Point(48, 140);
            this.lblNomProduit.Name = "lblNomProduit";
            this.lblNomProduit.Size = new System.Drawing.Size(173, 30);
            this.lblNomProduit.TabIndex = 7;
            this.lblNomProduit.Text = "Nom du produit :";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(324, 148);
            this.txtNomProduit.Multiline = true;
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(541, 29);
            this.txtNomProduit.TabIndex = 8;
            // 
            // lblQuantite
            // 
            this.lblQuantite.AutoSize = true;
            this.lblQuantite.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantite.Location = new System.Drawing.Point(48, 175);
            this.lblQuantite.Name = "lblQuantite";
            this.lblQuantite.Size = new System.Drawing.Size(105, 30);
            this.lblQuantite.TabIndex = 9;
            this.lblQuantite.Text = "Quantité :";
            // 
            // nudQuantite
            // 
            this.nudQuantite.Location = new System.Drawing.Point(324, 181);
            this.nudQuantite.Name = "nudQuantite";
            this.nudQuantite.Size = new System.Drawing.Size(120, 29);
            this.nudQuantite.TabIndex = 10;
            // 
            // lblPrixUnitaire
            // 
            this.lblPrixUnitaire.AutoSize = true;
            this.lblPrixUnitaire.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrixUnitaire.Location = new System.Drawing.Point(48, 212);
            this.lblPrixUnitaire.Name = "lblPrixUnitaire";
            this.lblPrixUnitaire.Size = new System.Drawing.Size(136, 30);
            this.lblPrixUnitaire.TabIndex = 11;
            this.lblPrixUnitaire.Text = "Prix Unitaire :";
            // 
            // txtPrixUnitaire
            // 
            this.txtPrixUnitaire.Location = new System.Drawing.Point(324, 216);
            this.txtPrixUnitaire.Name = "txtPrixUnitaire";
            this.txtPrixUnitaire.Size = new System.Drawing.Size(200, 29);
            this.txtPrixUnitaire.TabIndex = 12;
            // 
            // lblPrixTotal
            // 
            this.lblPrixTotal.AutoSize = true;
            this.lblPrixTotal.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrixTotal.Location = new System.Drawing.Point(48, 247);
            this.lblPrixTotal.Name = "lblPrixTotal";
            this.lblPrixTotal.Size = new System.Drawing.Size(108, 30);
            this.lblPrixTotal.TabIndex = 13;
            this.lblPrixTotal.Text = "Prix Total :";
            // 
            // txtPrixTotal
            // 
            this.txtPrixTotal.Location = new System.Drawing.Point(324, 251);
            this.txtPrixTotal.Name = "txtPrixTotal";
            this.txtPrixTotal.Size = new System.Drawing.Size(200, 29);
            this.txtPrixTotal.TabIndex = 14;
            // 
            // lblMotifRetour
            // 
            this.lblMotifRetour.AutoSize = true;
            this.lblMotifRetour.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotifRetour.Location = new System.Drawing.Point(44, 278);
            this.lblMotifRetour.Name = "lblMotifRetour";
            this.lblMotifRetour.Size = new System.Drawing.Size(243, 30);
            this.lblMotifRetour.TabIndex = 15;
            this.lblMotifRetour.Text = "Motif retour/annulation :";
            // 
            // cmbMotifRetour
            // 
            this.cmbMotifRetour.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMotifRetour.FormattingEnabled = true;
            this.cmbMotifRetour.Items.AddRange(new object[] {
            "Produit défectueux",
            "Erreur de commande",
            "Non conforme"});
            this.cmbMotifRetour.Location = new System.Drawing.Point(324, 287);
            this.cmbMotifRetour.Name = "cmbMotifRetour";
            this.cmbMotifRetour.Size = new System.Drawing.Size(200, 29);
            this.cmbMotifRetour.TabIndex = 16;
            // 
            // lblCommentaires
            // 
            this.lblCommentaires.AutoSize = true;
            this.lblCommentaires.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommentaires.Location = new System.Drawing.Point(48, 315);
            this.lblCommentaires.Name = "lblCommentaires";
            this.lblCommentaires.Size = new System.Drawing.Size(149, 30);
            this.lblCommentaires.TabIndex = 17;
            this.lblCommentaires.Text = "Commentaire :";
            // 
            // txtCommentaires
            // 
            this.txtCommentaires.Location = new System.Drawing.Point(324, 321);
            this.txtCommentaires.Multiline = true;
            this.txtCommentaires.Name = "txtCommentaires";
            this.txtCommentaires.Size = new System.Drawing.Size(541, 64);
            this.txtCommentaires.TabIndex = 18;
            // 
            // grpTypeRetour
            // 
            this.grpTypeRetour.BackColor = System.Drawing.Color.Cyan;
            this.grpTypeRetour.Controls.Add(this.rbEchange);
            this.grpTypeRetour.Controls.Add(this.rbRemboursement);
            this.grpTypeRetour.Location = new System.Drawing.Point(53, 391);
            this.grpTypeRetour.Name = "grpTypeRetour";
            this.grpTypeRetour.Size = new System.Drawing.Size(812, 97);
            this.grpTypeRetour.TabIndex = 19;
            this.grpTypeRetour.TabStop = false;
            this.grpTypeRetour.Text = "Souhaitez-vous :";
            // 
            // rbEchange
            // 
            this.rbEchange.AutoSize = true;
            this.rbEchange.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbEchange.Location = new System.Drawing.Point(15, 59);
            this.rbEchange.Name = "rbEchange";
            this.rbEchange.Size = new System.Drawing.Size(163, 25);
            this.rbEchange.TabIndex = 1;
            this.rbEchange.TabStop = true;
            this.rbEchange.Text = "Échanger le produit";
            this.rbEchange.UseVisualStyleBackColor = true;
            // 
            // rbRemboursement
            // 
            this.rbRemboursement.AutoSize = true;
            this.rbRemboursement.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbRemboursement.Location = new System.Drawing.Point(15, 28);
            this.rbRemboursement.Name = "rbRemboursement";
            this.rbRemboursement.Size = new System.Drawing.Size(219, 25);
            this.rbRemboursement.TabIndex = 0;
            this.rbRemboursement.TabStop = true;
            this.rbRemboursement.Text = "Obtenir un remboursement";
            this.rbRemboursement.UseVisualStyleBackColor = true;
            // 
            // btnValider
            // 
            this.btnValider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnValider.ForeColor = System.Drawing.Color.White;
            this.btnValider.Location = new System.Drawing.Point(408, 492);
            this.btnValider.Name = "btnValider";
            this.btnValider.Size = new System.Drawing.Size(98, 32);
            this.btnValider.TabIndex = 20;
            this.btnValider.Text = "Valider";
            this.btnValider.UseVisualStyleBackColor = false;
            this.btnValider.Click += new System.EventHandler(this.btnValider_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.BackColor = System.Drawing.Color.Black;
            this.btnAnnuler.ForeColor = System.Drawing.Color.White;
            this.btnAnnuler.Location = new System.Drawing.Point(512, 492);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(98, 32);
            this.btnAnnuler.TabIndex = 21;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = false;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnFermer
            // 
            this.btnFermer.BackColor = System.Drawing.Color.Red;
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.Location = new System.Drawing.Point(633, 492);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(98, 32);
            this.btnFermer.TabIndex = 22;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = false;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.BackColor = System.Drawing.Color.Blue;
            this.btnExporterPDF.ForeColor = System.Drawing.Color.White;
            this.btnExporterPDF.Location = new System.Drawing.Point(750, 492);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(115, 32);
            this.btnExporterPDF.TabIndex = 23;
            this.btnExporterPDF.Text = "Exporter PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = false;
            this.btnExporterPDF.Click += new System.EventHandler(this.btnExporterPDF_Click);
            // 
            // dgvAnnulationsRetours
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvAnnulationsRetours.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAnnulationsRetours.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAnnulationsRetours.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAnnulationsRetours.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvAnnulationsRetours.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAnnulationsRetours.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvAnnulationsRetours.Location = new System.Drawing.Point(0, 530);
            this.dgvAnnulationsRetours.Name = "dgvAnnulationsRetours";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAnnulationsRetours.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvAnnulationsRetours.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvAnnulationsRetours.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAnnulationsRetours.Size = new System.Drawing.Size(1200, 219);
            this.dgvAnnulationsRetours.TabIndex = 24;
            // 
            // lblDevise
            // 
            this.lblDevise.AutoSize = true;
            this.lblDevise.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDevise.Location = new System.Drawing.Point(530, 216);
            this.lblDevise.Name = "lblDevise";
            this.lblDevise.Size = new System.Drawing.Size(85, 30);
            this.lblDevise.TabIndex = 25;
            this.lblDevise.Text = "Devise :";
            // 
            // cbDevise
            // 
            this.cbDevise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevise.FormattingEnabled = true;
            this.cbDevise.Items.AddRange(new object[] {
            "Produit défectueux",
            "Erreur de commande",
            "Non conforme"});
            this.cbDevise.Location = new System.Drawing.Point(621, 220);
            this.cbDevise.Name = "cbDevise";
            this.cbDevise.Size = new System.Drawing.Size(135, 29);
            this.cbDevise.TabIndex = 26;
            // 
            // FormAnnulations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1200, 749);
            this.Controls.Add(this.cbDevise);
            this.Controls.Add(this.lblDevise);
            this.Controls.Add(this.dgvAnnulationsRetours);
            this.Controls.Add(this.btnExporterPDF);
            this.Controls.Add(this.btnFermer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnValider);
            this.Controls.Add(this.grpTypeRetour);
            this.Controls.Add(this.txtCommentaires);
            this.Controls.Add(this.lblCommentaires);
            this.Controls.Add(this.cmbMotifRetour);
            this.Controls.Add(this.lblMotifRetour);
            this.Controls.Add(this.txtPrixTotal);
            this.Controls.Add(this.lblPrixTotal);
            this.Controls.Add(this.txtPrixUnitaire);
            this.Controls.Add(this.lblPrixUnitaire);
            this.Controls.Add(this.nudQuantite);
            this.Controls.Add(this.lblQuantite);
            this.Controls.Add(this.txtNomProduit);
            this.Controls.Add(this.lblNomProduit);
            this.Controls.Add(this.dtpDateAchat);
            this.Controls.Add(this.lblDateAchat);
            this.Controls.Add(this.txtNumCommande);
            this.Controls.Add(this.lblNumCommande);
            this.Controls.Add(this.txtNomClient);
            this.Controls.Add(this.lblNomClient);
            this.Controls.Add(this.lblTitre);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormAnnulations";
            this.Text = "Annulations / Retours";
            this.Load += new System.EventHandler(this.FormAnnulations_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).EndInit();
            this.grpTypeRetour.ResumeLayout(false);
            this.grpTypeRetour.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAnnulationsRetours)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblNomClient;
        private System.Windows.Forms.TextBox txtNomClient;
        private System.Windows.Forms.Label lblNumCommande;
        private System.Windows.Forms.TextBox txtNumCommande;
        private System.Windows.Forms.Label lblDateAchat;
        private System.Windows.Forms.DateTimePicker dtpDateAchat;
        private System.Windows.Forms.Label lblNomProduit;
        private System.Windows.Forms.TextBox txtNomProduit;
        private System.Windows.Forms.Label lblQuantite;
        private System.Windows.Forms.NumericUpDown nudQuantite;
        private System.Windows.Forms.Label lblPrixUnitaire;
        private System.Windows.Forms.TextBox txtPrixUnitaire;
        private System.Windows.Forms.Label lblPrixTotal;
        private System.Windows.Forms.TextBox txtPrixTotal;
        private System.Windows.Forms.Label lblMotifRetour;
        private System.Windows.Forms.ComboBox cmbMotifRetour;
        private System.Windows.Forms.Label lblCommentaires;
        private System.Windows.Forms.TextBox txtCommentaires;
        private System.Windows.Forms.GroupBox grpTypeRetour;
        private System.Windows.Forms.RadioButton rbEchange;
        private System.Windows.Forms.RadioButton rbRemboursement;
        private System.Windows.Forms.Button btnValider;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.Button btnExporterPDF;
        private System.Windows.Forms.DataGridView dgvAnnulationsRetours;
        private System.Windows.Forms.Label lblDevise;
        private System.Windows.Forms.ComboBox cbDevise;
    }
}