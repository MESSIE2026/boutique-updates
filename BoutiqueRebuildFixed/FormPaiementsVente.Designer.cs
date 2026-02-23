namespace BoutiqueRebuildFixed
{
    partial class FormPaiementsVente
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvPaiements = new System.Windows.Forms.DataGridView();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.lblTotalPaye = new System.Windows.Forms.Label();
            this.txtTotalTTC = new System.Windows.Forms.TextBox();
            this.lblResteAPayer = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAnnulerPaiement = new System.Windows.Forms.Button();
            this.txtMotifAnnulation = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPaiements)).BeginInit();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Maroon;
            this.label1.Font = new System.Drawing.Font("Broadway", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(297, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(492, 55);
            this.label1.TabIndex = 0;
            this.label1.Text = "PAIEMENT VENTE";
            // 
            // dgvPaiements
            // 
            this.dgvPaiements.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPaiements.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPaiements.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvPaiements.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPaiements.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvPaiements.Location = new System.Drawing.Point(14, 123);
            this.dgvPaiements.Name = "dgvPaiements";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPaiements.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            this.dgvPaiements.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgvPaiements.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPaiements.Size = new System.Drawing.Size(1344, 560);
            this.dgvPaiements.TabIndex = 1;
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.Color.Blue;
            this.btnOk.Font = new System.Drawing.Font("Britannic Bold", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.Location = new System.Drawing.Point(770, 689);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(95, 43);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAnnuler.Font = new System.Drawing.Font("Britannic Bold", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btnAnnuler.Location = new System.Drawing.Point(1219, 689);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(123, 42);
            this.btnAnnuler.TabIndex = 3;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = false;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // lblTotalPaye
            // 
            this.lblTotalPaye.AutoSize = true;
            this.lblTotalPaye.Location = new System.Drawing.Point(124, 699);
            this.lblTotalPaye.Name = "lblTotalPaye";
            this.lblTotalPaye.Size = new System.Drawing.Size(64, 25);
            this.lblTotalPaye.TabIndex = 4;
            this.lblTotalPaye.Text = "label2";
            // 
            // txtTotalTTC
            // 
            this.txtTotalTTC.Location = new System.Drawing.Point(628, 696);
            this.txtTotalTTC.Name = "txtTotalTTC";
            this.txtTotalTTC.Size = new System.Drawing.Size(127, 33);
            this.txtTotalTTC.TabIndex = 5;
            // 
            // lblResteAPayer
            // 
            this.lblResteAPayer.AutoSize = true;
            this.lblResteAPayer.Location = new System.Drawing.Point(413, 699);
            this.lblResteAPayer.Name = "lblResteAPayer";
            this.lblResteAPayer.Size = new System.Drawing.Size(64, 25);
            this.lblResteAPayer.TabIndex = 6;
            this.lblResteAPayer.Text = "label2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(269, 699);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 25);
            this.label2.TabIndex = 7;
            this.label2.Text = "Reste A Payer :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 699);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Total Payer :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(523, 699);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(99, 25);
            this.label4.TabIndex = 9;
            this.label4.Text = "Total TTC :";
            // 
            // btnAnnulerPaiement
            // 
            this.btnAnnulerPaiement.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAnnulerPaiement.Font = new System.Drawing.Font("Britannic Bold", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnulerPaiement.ForeColor = System.Drawing.Color.Black;
            this.btnAnnulerPaiement.Location = new System.Drawing.Point(871, 689);
            this.btnAnnulerPaiement.Name = "btnAnnulerPaiement";
            this.btnAnnulerPaiement.Size = new System.Drawing.Size(217, 42);
            this.btnAnnulerPaiement.TabIndex = 10;
            this.btnAnnulerPaiement.Text = "Annuler Paiement";
            this.btnAnnulerPaiement.UseVisualStyleBackColor = false;
            this.btnAnnulerPaiement.Click += new System.EventHandler(this.btnAnnulerPaiement_Click);
            // 
            // txtMotifAnnulation
            // 
            this.txtMotifAnnulation.Location = new System.Drawing.Point(971, 12);
            this.txtMotifAnnulation.Multiline = true;
            this.txtMotifAnnulation.Name = "txtMotifAnnulation";
            this.txtMotifAnnulation.Size = new System.Drawing.Size(387, 105);
            this.txtMotifAnnulation.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(795, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(170, 25);
            this.label5.TabIndex = 12;
            this.label5.Text = "Motif Annulation :";
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.btnExporterPDF);
            this.panelMain.Controls.Add(this.label5);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.lblResteAPayer);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.label4);
            this.panelMain.Controls.Add(this.lblTotalPaye);
            this.panelMain.Controls.Add(this.btnAnnulerPaiement);
            this.panelMain.Controls.Add(this.txtMotifAnnulation);
            this.panelMain.Controls.Add(this.dgvPaiements);
            this.panelMain.Controls.Add(this.btnAnnuler);
            this.panelMain.Controls.Add(this.txtTotalTTC);
            this.panelMain.Controls.Add(this.btnOk);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1370, 749);
            this.panelMain.TabIndex = 13;
            this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.BackColor = System.Drawing.Color.Blue;
            this.btnExporterPDF.Font = new System.Drawing.Font("Britannic Bold", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporterPDF.ForeColor = System.Drawing.Color.White;
            this.btnExporterPDF.Location = new System.Drawing.Point(1104, 689);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(95, 43);
            this.btnExporterPDF.TabIndex = 13;
            this.btnExporterPDF.Text = "PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = false;
            this.btnExporterPDF.Click += new System.EventHandler(this.btnExporterPDF_Click);
            // 
            // FormPaiementsVente
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FormPaiementsVente";
            this.Text = "FormPaiementsVente";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPaiements)).EndInit();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvPaiements;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.Label lblTotalPaye;
        private System.Windows.Forms.TextBox txtTotalTTC;
        private System.Windows.Forms.Label lblResteAPayer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAnnulerPaiement;
        private System.Windows.Forms.TextBox txtMotifAnnulation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnExporterPDF;
    }
}