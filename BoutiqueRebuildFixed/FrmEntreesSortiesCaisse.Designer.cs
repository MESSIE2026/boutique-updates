namespace BoutiqueRebuildFixed
{
    partial class FrmEntreesSortiesCaisse
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblMontant = new System.Windows.Forms.Label();
            this.lblMotif = new System.Windows.Forms.Label();
            this.cmbMotif = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.BtnEnregistrer = new System.Windows.Forms.Button();
            this.BtnAnnuler = new System.Windows.Forms.Button();
            this.lblTableau = new System.Windows.Forms.Label();
            this.dgvMouvements = new System.Windows.Forms.DataGridView();
            this.colDateHeure = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMontant = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMotif = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Colcaisier = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColAutorisePar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTotalEntrees = new System.Windows.Forms.Label();
            this.lblTotalSorties = new System.Windows.Forms.Label();
            this.lblBalanceNette = new System.Windows.Forms.Label();
            this.lblDateHeure = new System.Windows.Forms.Label();
            this.TimerDateHeure = new System.Windows.Forms.Timer(this.components);
            this.BtnExporterPDF = new System.Windows.Forms.Button();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbDevise = new System.Windows.Forms.ComboBox();
            this.txtMontant = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMouvements)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.lblTitre.Font = new System.Drawing.Font("HP Simplified", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lblTitre.Location = new System.Drawing.Point(470, 3);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(360, 44);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "Entrée / Sorties de Caisse";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblType.Location = new System.Drawing.Point(80, 63);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(148, 28);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Entrée / Sortie : ";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Entrée, Sortie"});
            this.comboBox1.Location = new System.Drawing.Point(234, 67);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(120, 21);
            this.comboBox1.TabIndex = 3;
            // 
            // lblMontant
            // 
            this.lblMontant.AutoSize = true;
            this.lblMontant.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMontant.Location = new System.Drawing.Point(432, 64);
            this.lblMontant.Name = "lblMontant";
            this.lblMontant.Size = new System.Drawing.Size(93, 28);
            this.lblMontant.TabIndex = 4;
            this.lblMontant.Text = "Montant :";
            // 
            // lblMotif
            // 
            this.lblMotif.AutoSize = true;
            this.lblMotif.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotif.Location = new System.Drawing.Point(904, 100);
            this.lblMotif.Name = "lblMotif";
            this.lblMotif.Size = new System.Drawing.Size(70, 28);
            this.lblMotif.TabIndex = 6;
            this.lblMotif.Text = "Motif : ";
            // 
            // cmbMotif
            // 
            this.cmbMotif.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMotif.Font = new System.Drawing.Font("Palatino Linotype", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbMotif.FormattingEnabled = true;
            this.cmbMotif.Items.AddRange(new object[] {
            "Fond de caisse",
            "Remboursement",
            "Avance",
            "Sortie interne",
            "Autres…"});
            this.cmbMotif.Location = new System.Drawing.Point(980, 99);
            this.cmbMotif.Name = "cmbMotif";
            this.cmbMotif.Size = new System.Drawing.Size(203, 34);
            this.cmbMotif.TabIndex = 7;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("HP Simplified", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.Location = new System.Drawing.Point(81, 105);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(142, 31);
            this.lblDescription.TabIndex = 8;
            this.lblDescription.Text = "Description : ";
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(234, 108);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDescription.Size = new System.Drawing.Size(664, 120);
            this.txtDescription.TabIndex = 9;
            // 
            // BtnEnregistrer
            // 
            this.BtnEnregistrer.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnEnregistrer.ForeColor = System.Drawing.Color.Blue;
            this.BtnEnregistrer.Location = new System.Drawing.Point(980, 192);
            this.BtnEnregistrer.Name = "BtnEnregistrer";
            this.BtnEnregistrer.Size = new System.Drawing.Size(125, 36);
            this.BtnEnregistrer.TabIndex = 10;
            this.BtnEnregistrer.Text = "Enregistrer";
            this.BtnEnregistrer.UseVisualStyleBackColor = true;
            this.BtnEnregistrer.Click += new System.EventHandler(this.BtnEnregistrer_Click);
            // 
            // BtnAnnuler
            // 
            this.BtnAnnuler.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.BtnAnnuler.Location = new System.Drawing.Point(1141, 191);
            this.BtnAnnuler.Name = "BtnAnnuler";
            this.BtnAnnuler.Size = new System.Drawing.Size(103, 36);
            this.BtnAnnuler.TabIndex = 11;
            this.BtnAnnuler.Text = "Annuler";
            this.BtnAnnuler.UseVisualStyleBackColor = true;
            this.BtnAnnuler.Click += new System.EventHandler(this.BtnAnnuler_Click);
            // 
            // lblTableau
            // 
            this.lblTableau.AutoSize = true;
            this.lblTableau.Font = new System.Drawing.Font("HP Simplified", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTableau.Location = new System.Drawing.Point(507, 238);
            this.lblTableau.Name = "lblTableau";
            this.lblTableau.Size = new System.Drawing.Size(326, 29);
            this.lblTableau.TabIndex = 12;
            this.lblTableau.Text = "Tableau des mouvements de caisse";
            // 
            // dgvMouvements
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvMouvements.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvMouvements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Rockwell", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMouvements.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvMouvements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMouvements.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDateHeure,
            this.ColType,
            this.ColMontant,
            this.ColMotif,
            this.ColDescription,
            this.Colcaisier,
            this.ColAutorisePar});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Rockwell", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMouvements.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvMouvements.Location = new System.Drawing.Point(12, 272);
            this.dgvMouvements.Name = "dgvMouvements";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Rockwell", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMouvements.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvMouvements.Size = new System.Drawing.Size(1296, 357);
            this.dgvMouvements.TabIndex = 13;
            // 
            // colDateHeure
            // 
            this.colDateHeure.HeaderText = "DateHeure";
            this.colDateHeure.Name = "colDateHeure";
            // 
            // ColType
            // 
            this.ColType.HeaderText = "Type";
            this.ColType.Name = "ColType";
            // 
            // ColMontant
            // 
            this.ColMontant.HeaderText = "Montant";
            this.ColMontant.Name = "ColMontant";
            // 
            // ColMotif
            // 
            this.ColMotif.HeaderText = "Motif";
            this.ColMotif.Name = "ColMotif";
            // 
            // ColDescription
            // 
            this.ColDescription.HeaderText = "Description";
            this.ColDescription.Name = "ColDescription";
            // 
            // Colcaisier
            // 
            this.Colcaisier.HeaderText = "Caissier";
            this.Colcaisier.Name = "Colcaisier";
            // 
            // ColAutorisePar
            // 
            this.ColAutorisePar.HeaderText = "AutorisePar";
            this.ColAutorisePar.Name = "ColAutorisePar";
            // 
            // lblTotalEntrees
            // 
            this.lblTotalEntrees.AutoSize = true;
            this.lblTotalEntrees.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalEntrees.Location = new System.Drawing.Point(470, 684);
            this.lblTotalEntrees.Name = "lblTotalEntrees";
            this.lblTotalEntrees.Size = new System.Drawing.Size(51, 28);
            this.lblTotalEntrees.TabIndex = 18;
            this.lblTotalEntrees.Text = "0.00";
            // 
            // lblTotalSorties
            // 
            this.lblTotalSorties.AutoSize = true;
            this.lblTotalSorties.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalSorties.Location = new System.Drawing.Point(470, 712);
            this.lblTotalSorties.Name = "lblTotalSorties";
            this.lblTotalSorties.Size = new System.Drawing.Size(51, 28);
            this.lblTotalSorties.TabIndex = 19;
            this.lblTotalSorties.Text = "0.00";
            // 
            // lblBalanceNette
            // 
            this.lblBalanceNette.AutoSize = true;
            this.lblBalanceNette.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalanceNette.Location = new System.Drawing.Point(832, 703);
            this.lblBalanceNette.Name = "lblBalanceNette";
            this.lblBalanceNette.Size = new System.Drawing.Size(51, 28);
            this.lblBalanceNette.TabIndex = 21;
            this.lblBalanceNette.Text = "0.00";
            // 
            // lblDateHeure
            // 
            this.lblDateHeure.AutoSize = true;
            this.lblDateHeure.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateHeure.Location = new System.Drawing.Point(1107, 27);
            this.lblDateHeure.Name = "lblDateHeure";
            this.lblDateHeure.Size = new System.Drawing.Size(76, 18);
            this.lblDateHeure.TabIndex = 22;
            this.lblDateHeure.Text = "Date Time";
            // 
            // BtnExporterPDF
            // 
            this.BtnExporterPDF.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnExporterPDF.ForeColor = System.Drawing.Color.Blue;
            this.BtnExporterPDF.Location = new System.Drawing.Point(1141, 684);
            this.BtnExporterPDF.Name = "BtnExporterPDF";
            this.BtnExporterPDF.Size = new System.Drawing.Size(143, 38);
            this.BtnExporterPDF.TabIndex = 23;
            this.BtnExporterPDF.Text = "Exporter PDF";
            this.BtnExporterPDF.UseVisualStyleBackColor = true;
            this.BtnExporterPDF.Click += new System.EventHandler(this.BtnExporterPDF_Click);
            // 
            // cmbType
            // 
            this.cmbType.Font = new System.Drawing.Font("Palatino Linotype", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(980, 142);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(146, 29);
            this.cmbType.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(904, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 28);
            this.label4.TabIndex = 25;
            this.label4.Text = "Type :";
            // 
            // cmbDevise
            // 
            this.cmbDevise.Font = new System.Drawing.Font("Rockwell", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDevise.FormattingEnabled = true;
            this.cmbDevise.Location = new System.Drawing.Point(719, 63);
            this.cmbDevise.Name = "cmbDevise";
            this.cmbDevise.Size = new System.Drawing.Size(94, 29);
            this.cmbDevise.TabIndex = 26;
            // 
            // txtMontant
            // 
            this.txtMontant.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMontant.Location = new System.Drawing.Point(531, 61);
            this.txtMontant.Name = "txtMontant";
            this.txtMontant.Size = new System.Drawing.Size(173, 33);
            this.txtMontant.TabIndex = 27;
            // 
            // FrmEntreesSortiesCaisse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.txtMontant);
            this.Controls.Add(this.cmbDevise);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.BtnExporterPDF);
            this.Controls.Add(this.lblDateHeure);
            this.Controls.Add(this.lblBalanceNette);
            this.Controls.Add(this.lblTotalSorties);
            this.Controls.Add(this.lblTotalEntrees);
            this.Controls.Add(this.dgvMouvements);
            this.Controls.Add(this.lblTableau);
            this.Controls.Add(this.BtnAnnuler);
            this.Controls.Add(this.BtnEnregistrer);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.cmbMotif);
            this.Controls.Add(this.lblMotif);
            this.Controls.Add(this.lblMontant);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblTitre);
            this.Name = "FrmEntreesSortiesCaisse";
            this.Text = "FrmEntreesSortiesCaisse";
            this.Load += new System.EventHandler(this.FrmEntreesSortiesCaisse_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMouvements)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label lblMontant;
        private System.Windows.Forms.Label lblMotif;
        private System.Windows.Forms.ComboBox cmbMotif;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button BtnEnregistrer;
        private System.Windows.Forms.Button BtnAnnuler;
        private System.Windows.Forms.Label lblTableau;
        private System.Windows.Forms.DataGridView dgvMouvements;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateHeure;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColMontant;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColMotif;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn Colcaisier;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColAutorisePar;
        private System.Windows.Forms.Label lblTotalEntrees;
        private System.Windows.Forms.Label lblTotalSorties;
        private System.Windows.Forms.Label lblBalanceNette;
        private System.Windows.Forms.Label lblDateHeure;
        private System.Windows.Forms.Timer TimerDateHeure;
        private System.Windows.Forms.Button BtnExporterPDF;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbDevise;
        private System.Windows.Forms.TextBox txtMontant;
    }
}