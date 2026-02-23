namespace BoutiqueRebuildFixed
{
    partial class FormSalairesAgents
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblTitre = new System.Windows.Forms.Label();
            this.groupBoxPaiement = new System.Windows.Forms.GroupBox();
            this.txtObservations = new System.Windows.Forms.TextBox();
            this.lblObservations = new System.Windows.Forms.Label();
            this.cboStatut = new System.Windows.Forms.ComboBox();
            this.lblStatut = new System.Windows.Forms.Label();
            this.cboDevise = new System.Windows.Forms.ComboBox();
            this.lblDevise = new System.Windows.Forms.Label();
            this.txtMontant = new System.Windows.Forms.TextBox();
            this.lblMontant = new System.Windows.Forms.Label();
            this.txtNomEmploye = new System.Windows.Forms.TextBox();
            this.lblNomEmploye = new System.Windows.Forms.Label();
            this.lblIdEmploye = new System.Windows.Forms.Label();
            this.dtpDatePaiement = new System.Windows.Forms.DateTimePicker();
            this.txtIdEmploye = new System.Windows.Forms.TextBox();
            this.lblDatePaiement = new System.Windows.Forms.Label();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnNouveau = new System.Windows.Forms.Button();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            this.btnFermer = new System.Windows.Forms.Button();
            this.dgvSalairesAgents = new System.Windows.Forms.DataGridView();
            this.btnLikelemba = new System.Windows.Forms.Button();
            this.groupBoxPaiement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSalairesAgents)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitre.Font = new System.Drawing.Font("Gill Sans Ultra Bold", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.Yellow;
            this.lblTitre.Location = new System.Drawing.Point(519, 9);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(329, 41);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "SALAIRES AGENTS";
            // 
            // groupBoxPaiement
            // 
            this.groupBoxPaiement.Controls.Add(this.txtObservations);
            this.groupBoxPaiement.Controls.Add(this.lblObservations);
            this.groupBoxPaiement.Controls.Add(this.cboStatut);
            this.groupBoxPaiement.Controls.Add(this.lblStatut);
            this.groupBoxPaiement.Controls.Add(this.cboDevise);
            this.groupBoxPaiement.Controls.Add(this.lblDevise);
            this.groupBoxPaiement.Controls.Add(this.txtMontant);
            this.groupBoxPaiement.Controls.Add(this.lblMontant);
            this.groupBoxPaiement.Controls.Add(this.txtNomEmploye);
            this.groupBoxPaiement.Controls.Add(this.lblNomEmploye);
            this.groupBoxPaiement.Controls.Add(this.lblIdEmploye);
            this.groupBoxPaiement.Controls.Add(this.dtpDatePaiement);
            this.groupBoxPaiement.Controls.Add(this.txtIdEmploye);
            this.groupBoxPaiement.Controls.Add(this.lblDatePaiement);
            this.groupBoxPaiement.Font = new System.Drawing.Font("Comic Sans MS", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxPaiement.ForeColor = System.Drawing.Color.White;
            this.groupBoxPaiement.Location = new System.Drawing.Point(26, 53);
            this.groupBoxPaiement.Name = "groupBoxPaiement";
            this.groupBoxPaiement.Size = new System.Drawing.Size(1058, 359);
            this.groupBoxPaiement.TabIndex = 1;
            this.groupBoxPaiement.TabStop = false;
            this.groupBoxPaiement.Text = "Informations du paiement";
            // 
            // txtObservations
            // 
            this.txtObservations.Location = new System.Drawing.Point(295, 284);
            this.txtObservations.Multiline = true;
            this.txtObservations.Name = "txtObservations";
            this.txtObservations.Size = new System.Drawing.Size(499, 65);
            this.txtObservations.TabIndex = 13;
            // 
            // lblObservations
            // 
            this.lblObservations.AutoSize = true;
            this.lblObservations.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblObservations.Location = new System.Drawing.Point(24, 283);
            this.lblObservations.Name = "lblObservations";
            this.lblObservations.Size = new System.Drawing.Size(151, 30);
            this.lblObservations.TabIndex = 12;
            this.lblObservations.Text = "Observations :";
            // 
            // cboStatut
            // 
            this.cboStatut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatut.FormattingEnabled = true;
            this.cboStatut.Location = new System.Drawing.Point(295, 241);
            this.cboStatut.Name = "cboStatut";
            this.cboStatut.Size = new System.Drawing.Size(245, 37);
            this.cboStatut.TabIndex = 11;
            // 
            // lblStatut
            // 
            this.lblStatut.AutoSize = true;
            this.lblStatut.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatut.Location = new System.Drawing.Point(24, 243);
            this.lblStatut.Name = "lblStatut";
            this.lblStatut.Size = new System.Drawing.Size(81, 30);
            this.lblStatut.TabIndex = 10;
            this.lblStatut.Text = "Statut :";
            // 
            // cboDevise
            // 
            this.cboDevise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDevise.FormattingEnabled = true;
            this.cboDevise.Location = new System.Drawing.Point(295, 198);
            this.cboDevise.Name = "cboDevise";
            this.cboDevise.Size = new System.Drawing.Size(245, 37);
            this.cboDevise.TabIndex = 9;
            // 
            // lblDevise
            // 
            this.lblDevise.AutoSize = true;
            this.lblDevise.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDevise.Location = new System.Drawing.Point(24, 198);
            this.lblDevise.Name = "lblDevise";
            this.lblDevise.Size = new System.Drawing.Size(86, 30);
            this.lblDevise.TabIndex = 8;
            this.lblDevise.Text = "Devise :";
            // 
            // txtMontant
            // 
            this.txtMontant.Location = new System.Drawing.Point(295, 155);
            this.txtMontant.Name = "txtMontant";
            this.txtMontant.Size = new System.Drawing.Size(245, 37);
            this.txtMontant.TabIndex = 7;
            this.txtMontant.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblMontant
            // 
            this.lblMontant.AutoSize = true;
            this.lblMontant.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMontant.Location = new System.Drawing.Point(24, 157);
            this.lblMontant.Name = "lblMontant";
            this.lblMontant.Size = new System.Drawing.Size(107, 30);
            this.lblMontant.TabIndex = 6;
            this.lblMontant.Text = "Montant :";
            // 
            // txtNomEmploye
            // 
            this.txtNomEmploye.Location = new System.Drawing.Point(295, 112);
            this.txtNomEmploye.Name = "txtNomEmploye";
            this.txtNomEmploye.Size = new System.Drawing.Size(499, 37);
            this.txtNomEmploye.TabIndex = 5;
            // 
            // lblNomEmploye
            // 
            this.lblNomEmploye.AutoSize = true;
            this.lblNomEmploye.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomEmploye.Location = new System.Drawing.Point(24, 114);
            this.lblNomEmploye.Name = "lblNomEmploye";
            this.lblNomEmploye.Size = new System.Drawing.Size(201, 30);
            this.lblNomEmploye.TabIndex = 4;
            this.lblNomEmploye.Text = "Nom de l\'Employé :";
            // 
            // lblIdEmploye
            // 
            this.lblIdEmploye.AutoSize = true;
            this.lblIdEmploye.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIdEmploye.Location = new System.Drawing.Point(24, 72);
            this.lblIdEmploye.Name = "lblIdEmploye";
            this.lblIdEmploye.Size = new System.Drawing.Size(134, 30);
            this.lblIdEmploye.TabIndex = 3;
            this.lblIdEmploye.Text = "ID Employé :";
            // 
            // dtpDatePaiement
            // 
            this.dtpDatePaiement.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpDatePaiement.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDatePaiement.Location = new System.Drawing.Point(295, 30);
            this.dtpDatePaiement.Name = "dtpDatePaiement";
            this.dtpDatePaiement.Size = new System.Drawing.Size(245, 33);
            this.dtpDatePaiement.TabIndex = 2;
            // 
            // txtIdEmploye
            // 
            this.txtIdEmploye.Location = new System.Drawing.Point(295, 69);
            this.txtIdEmploye.Name = "txtIdEmploye";
            this.txtIdEmploye.Size = new System.Drawing.Size(245, 37);
            this.txtIdEmploye.TabIndex = 1;
            // 
            // lblDatePaiement
            // 
            this.lblDatePaiement.AutoSize = true;
            this.lblDatePaiement.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatePaiement.Location = new System.Drawing.Point(24, 33);
            this.lblDatePaiement.Name = "lblDatePaiement";
            this.lblDatePaiement.Size = new System.Drawing.Size(195, 30);
            this.lblDatePaiement.TabIndex = 0;
            this.lblDatePaiement.Text = "Date de paiement :";
            // 
            // btnEnregistrer
            // 
            this.btnEnregistrer.BackColor = System.Drawing.Color.Fuchsia;
            this.btnEnregistrer.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnregistrer.ForeColor = System.Drawing.Color.Black;
            this.btnEnregistrer.Location = new System.Drawing.Point(1144, 136);
            this.btnEnregistrer.Name = "btnEnregistrer";
            this.btnEnregistrer.Size = new System.Drawing.Size(129, 38);
            this.btnEnregistrer.TabIndex = 2;
            this.btnEnregistrer.Text = "Enregistrer";
            this.btnEnregistrer.UseVisualStyleBackColor = false;
            this.btnEnregistrer.Click += new System.EventHandler(this.btnEnregistrer_Click);
            // 
            // btnNouveau
            // 
            this.btnNouveau.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnNouveau.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNouveau.ForeColor = System.Drawing.Color.White;
            this.btnNouveau.Location = new System.Drawing.Point(1144, 190);
            this.btnNouveau.Name = "btnNouveau";
            this.btnNouveau.Size = new System.Drawing.Size(129, 38);
            this.btnNouveau.TabIndex = 3;
            this.btnNouveau.Text = "Nouveau";
            this.btnNouveau.UseVisualStyleBackColor = false;
            this.btnNouveau.Click += new System.EventHandler(this.btnNouveau_Click);
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.BackColor = System.Drawing.Color.Blue;
            this.btnExporterPDF.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporterPDF.ForeColor = System.Drawing.Color.White;
            this.btnExporterPDF.Location = new System.Drawing.Point(1144, 307);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(129, 38);
            this.btnExporterPDF.TabIndex = 4;
            this.btnExporterPDF.Text = "Exporter PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = false;
            this.btnExporterPDF.Click += new System.EventHandler(this.btnExporterPDF_Click);
            // 
            // btnFermer
            // 
            this.btnFermer.BackColor = System.Drawing.Color.Red;
            this.btnFermer.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFermer.ForeColor = System.Drawing.Color.White;
            this.btnFermer.Location = new System.Drawing.Point(1144, 251);
            this.btnFermer.Name = "btnFermer";
            this.btnFermer.Size = new System.Drawing.Size(129, 38);
            this.btnFermer.TabIndex = 5;
            this.btnFermer.Text = "Fermer";
            this.btnFermer.UseVisualStyleBackColor = false;
            this.btnFermer.Click += new System.EventHandler(this.btnFermer_Click);
            // 
            // dgvSalairesAgents
            // 
            this.dgvSalairesAgents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSalairesAgents.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvSalairesAgents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSalairesAgents.Location = new System.Drawing.Point(26, 418);
            this.dgvSalairesAgents.Name = "dgvSalairesAgents";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvSalairesAgents.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvSalairesAgents.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvSalairesAgents.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSalairesAgents.Size = new System.Drawing.Size(1288, 327);
            this.dgvSalairesAgents.TabIndex = 6;
            // 
            // btnLikelemba
            // 
            this.btnLikelemba.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnLikelemba.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLikelemba.ForeColor = System.Drawing.Color.White;
            this.btnLikelemba.Location = new System.Drawing.Point(1144, 374);
            this.btnLikelemba.Name = "btnLikelemba";
            this.btnLikelemba.Size = new System.Drawing.Size(129, 38);
            this.btnLikelemba.TabIndex = 7;
            this.btnLikelemba.Text = "LIKELEMBA";
            this.btnLikelemba.UseVisualStyleBackColor = false;
            this.btnLikelemba.Click += new System.EventHandler(this.btnLikelemba_Click);
            // 
            // FormSalairesAgents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkSlateBlue;
            this.ClientSize = new System.Drawing.Size(1326, 749);
            this.Controls.Add(this.btnLikelemba);
            this.Controls.Add(this.dgvSalairesAgents);
            this.Controls.Add(this.btnFermer);
            this.Controls.Add(this.btnExporterPDF);
            this.Controls.Add(this.btnNouveau);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.groupBoxPaiement);
            this.Controls.Add(this.lblTitre);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormSalairesAgents";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Paiement des salaires des agents";
            this.Load += new System.EventHandler(this.FormSalairesAgents_Load);
            this.groupBoxPaiement.ResumeLayout(false);
            this.groupBoxPaiement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSalairesAgents)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.GroupBox groupBoxPaiement;
        private System.Windows.Forms.Label lblDatePaiement;
        private System.Windows.Forms.Label lblIdEmploye;
        private System.Windows.Forms.DateTimePicker dtpDatePaiement;
        private System.Windows.Forms.TextBox txtIdEmploye;
        private System.Windows.Forms.ComboBox cboStatut;
        private System.Windows.Forms.Label lblStatut;
        private System.Windows.Forms.ComboBox cboDevise;
        private System.Windows.Forms.Label lblDevise;
        private System.Windows.Forms.TextBox txtMontant;
        private System.Windows.Forms.Label lblMontant;
        private System.Windows.Forms.TextBox txtNomEmploye;
        private System.Windows.Forms.Label lblNomEmploye;
        private System.Windows.Forms.TextBox txtObservations;
        private System.Windows.Forms.Label lblObservations;
        private System.Windows.Forms.Button btnEnregistrer;
        private System.Windows.Forms.Button btnNouveau;
        private System.Windows.Forms.Button btnExporterPDF;
        private System.Windows.Forms.Button btnFermer;
        private System.Windows.Forms.DataGridView dgvSalairesAgents;
        private System.Windows.Forms.Button btnLikelemba;
    }
}