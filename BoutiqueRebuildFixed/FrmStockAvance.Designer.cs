namespace BoutiqueRebuildFixed
{
    partial class FrmStockAvance
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmbProduit = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numQuantite = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpDateEntree = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpDateSortie = new System.Windows.Forms.DateTimePicker();
            this.BtnRechercher = new System.Windows.Forms.Button();
            this.dgvStocks = new System.Windows.Forms.DataGridView();
            this.BtnAjouter = new System.Windows.Forms.Button();
            this.BtnModifier = new System.Windows.Forms.Button();
            this.BtnSupprimer = new System.Windows.Forms.Button();
            this.BtnValider = new System.Windows.Forms.Button();
            this.BtnExporterPDF = new System.Windows.Forms.Button();
            this.txtRechercher = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtGerant = new System.Windows.Forms.TextBox();
            this.txtFournisseur = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(22, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Produit : ";
            // 
            // cmbProduit
            // 
            this.cmbProduit.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbProduit.FormattingEnabled = true;
            this.cmbProduit.Location = new System.Drawing.Point(113, 53);
            this.cmbProduit.Name = "cmbProduit";
            this.cmbProduit.Size = new System.Drawing.Size(233, 33);
            this.cmbProduit.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(365, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Quantité : ";
            // 
            // numQuantite
            // 
            this.numQuantite.Location = new System.Drawing.Point(490, 57);
            this.numQuantite.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numQuantite.Name = "numQuantite";
            this.numQuantite.Size = new System.Drawing.Size(120, 29);
            this.numQuantite.TabIndex = 3;
            this.numQuantite.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(902, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 25);
            this.label3.TabIndex = 4;
            this.label3.Text = "Date d\'entrée :";
            // 
            // dtpDateEntree
            // 
            this.dtpDateEntree.Location = new System.Drawing.Point(1060, 51);
            this.dtpDateEntree.Name = "dtpDateEntree";
            this.dtpDateEntree.Size = new System.Drawing.Size(200, 29);
            this.dtpDateEntree.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(902, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 25);
            this.label4.TabIndex = 6;
            this.label4.Text = "Date de sortie :";
            // 
            // dtpDateSortie
            // 
            this.dtpDateSortie.Location = new System.Drawing.Point(1060, 98);
            this.dtpDateSortie.Name = "dtpDateSortie";
            this.dtpDateSortie.Size = new System.Drawing.Size(200, 29);
            this.dtpDateSortie.TabIndex = 7;
            // 
            // BtnRechercher
            // 
            this.BtnRechercher.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnRechercher.ForeColor = System.Drawing.Color.Blue;
            this.BtnRechercher.Location = new System.Drawing.Point(1151, 5);
            this.BtnRechercher.Name = "BtnRechercher";
            this.BtnRechercher.Size = new System.Drawing.Size(121, 36);
            this.BtnRechercher.TabIndex = 8;
            this.BtnRechercher.Text = "Rechercher";
            this.BtnRechercher.UseVisualStyleBackColor = true;
            // 
            // dgvStocks
            // 
            this.dgvStocks.AllowUserToAddRows = false;
            this.dgvStocks.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvStocks.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvStocks.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvStocks.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvStocks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvStocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvStocks.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvStocks.Location = new System.Drawing.Point(27, 140);
            this.dgvStocks.MultiSelect = false;
            this.dgvStocks.Name = "dgvStocks";
            this.dgvStocks.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvStocks.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvStocks.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvStocks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStocks.Size = new System.Drawing.Size(1233, 529);
            this.dgvStocks.TabIndex = 9;
            // 
            // BtnAjouter
            // 
            this.BtnAjouter.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAjouter.ForeColor = System.Drawing.Color.Blue;
            this.BtnAjouter.Location = new System.Drawing.Point(27, 688);
            this.BtnAjouter.Name = "BtnAjouter";
            this.BtnAjouter.Size = new System.Drawing.Size(121, 36);
            this.BtnAjouter.TabIndex = 10;
            this.BtnAjouter.Text = "Ajouter";
            this.BtnAjouter.UseVisualStyleBackColor = true;
            this.BtnAjouter.Click += new System.EventHandler(this.BtnAjouter_Click);
            // 
            // BtnModifier
            // 
            this.BtnModifier.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnModifier.ForeColor = System.Drawing.Color.Black;
            this.BtnModifier.Location = new System.Drawing.Point(286, 688);
            this.BtnModifier.Name = "BtnModifier";
            this.BtnModifier.Size = new System.Drawing.Size(121, 36);
            this.BtnModifier.TabIndex = 11;
            this.BtnModifier.Text = "Modifier";
            this.BtnModifier.UseVisualStyleBackColor = true;
            this.BtnModifier.Click += new System.EventHandler(this.BtnModifier_Click);
            // 
            // BtnSupprimer
            // 
            this.BtnSupprimer.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnSupprimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.BtnSupprimer.Location = new System.Drawing.Point(551, 688);
            this.BtnSupprimer.Name = "BtnSupprimer";
            this.BtnSupprimer.Size = new System.Drawing.Size(121, 36);
            this.BtnSupprimer.TabIndex = 12;
            this.BtnSupprimer.Text = "Supprimer";
            this.BtnSupprimer.UseVisualStyleBackColor = true;
            this.BtnSupprimer.Click += new System.EventHandler(this.BtnSupprimer_Click);
            // 
            // BtnValider
            // 
            this.BtnValider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnValider.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnValider.ForeColor = System.Drawing.Color.LightGray;
            this.BtnValider.Location = new System.Drawing.Point(825, 688);
            this.BtnValider.Name = "BtnValider";
            this.BtnValider.Size = new System.Drawing.Size(121, 36);
            this.BtnValider.TabIndex = 13;
            this.BtnValider.Text = "Valider";
            this.BtnValider.UseVisualStyleBackColor = false;
            this.BtnValider.Click += new System.EventHandler(this.BtnValider_Click);
            // 
            // BtnExporterPDF
            // 
            this.BtnExporterPDF.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnExporterPDF.ForeColor = System.Drawing.Color.Black;
            this.BtnExporterPDF.Location = new System.Drawing.Point(1073, 688);
            this.BtnExporterPDF.Name = "BtnExporterPDF";
            this.BtnExporterPDF.Size = new System.Drawing.Size(174, 36);
            this.BtnExporterPDF.TabIndex = 14;
            this.BtnExporterPDF.Text = "Exporter PDF";
            this.BtnExporterPDF.UseVisualStyleBackColor = true;
            this.BtnExporterPDF.Click += new System.EventHandler(this.BtnExporterPDF_Click);
            // 
            // txtRechercher
            // 
            this.txtRechercher.Location = new System.Drawing.Point(907, 11);
            this.txtRechercher.Name = "txtRechercher";
            this.txtRechercher.Size = new System.Drawing.Size(238, 29);
            this.txtRechercher.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Font = new System.Drawing.Font("Showcard Gothic", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(388, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(422, 38);
            this.label5.TabIndex = 16;
            this.label5.Text = "ENTREES ET SORTIES STOCKS";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(22, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 25);
            this.label6.TabIndex = 17;
            this.label6.Text = "Gérant : ";
            // 
            // txtGerant
            // 
            this.txtGerant.Location = new System.Drawing.Point(111, 95);
            this.txtGerant.Name = "txtGerant";
            this.txtGerant.Size = new System.Drawing.Size(238, 29);
            this.txtGerant.TabIndex = 18;
            // 
            // txtFournisseur
            // 
            this.txtFournisseur.Location = new System.Drawing.Point(490, 97);
            this.txtFournisseur.Name = "txtFournisseur";
            this.txtFournisseur.Size = new System.Drawing.Size(238, 29);
            this.txtFournisseur.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(365, 98);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(124, 25);
            this.label7.TabIndex = 20;
            this.label7.Text = "Fournisseur : ";
            // 
            // FrmStockAvance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1284, 749);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtFournisseur);
            this.Controls.Add(this.txtGerant);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtRechercher);
            this.Controls.Add(this.BtnExporterPDF);
            this.Controls.Add(this.BtnValider);
            this.Controls.Add(this.BtnSupprimer);
            this.Controls.Add(this.BtnModifier);
            this.Controls.Add(this.BtnAjouter);
            this.Controls.Add(this.dgvStocks);
            this.Controls.Add(this.BtnRechercher);
            this.Controls.Add(this.dtpDateSortie);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dtpDateEntree);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numQuantite);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbProduit);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmStockAvance";
            this.Text = "Gestion Stock Avancé";
            this.Load += new System.EventHandler(this.FrmStockAvance_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStocks)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbProduit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numQuantite;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpDateEntree;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpDateSortie;
        private System.Windows.Forms.Button BtnRechercher;
        private System.Windows.Forms.DataGridView dgvStocks;
        private System.Windows.Forms.Button BtnAjouter;
        private System.Windows.Forms.Button BtnModifier;
        private System.Windows.Forms.Button BtnSupprimer;
        private System.Windows.Forms.Button BtnValider;
        private System.Windows.Forms.Button BtnExporterPDF;
        private System.Windows.Forms.TextBox txtRechercher;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtGerant;
        private System.Windows.Forms.TextBox txtFournisseur;
        private System.Windows.Forms.Label label7;
    }
}