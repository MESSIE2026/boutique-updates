namespace BoutiqueRebuildFixed
{
    partial class SessionCaisse
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnOuvrirSession = new System.Windows.Forms.Button();
            this.BtnCloreSession = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDateOuverture = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CmbCaissier = new System.Windows.Forms.ComboBox();
            this.DataGridViewSessions = new System.Windows.Forms.DataGridView();
            this.BtnExporterPDF = new System.Windows.Forms.Button();
            this.BtnImprimerZ = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblVenteEspece = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblVenteCarte = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblRemboursements = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblCashReel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblMobileMoney = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewSessions)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Font = new System.Drawing.Font("Impact", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(478, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(389, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "OUVERTURE ET FERMETURE SESSION";
            // 
            // BtnOuvrirSession
            // 
            this.BtnOuvrirSession.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnOuvrirSession.ForeColor = System.Drawing.Color.Blue;
            this.BtnOuvrirSession.Location = new System.Drawing.Point(20, 63);
            this.BtnOuvrirSession.Margin = new System.Windows.Forms.Padding(4);
            this.BtnOuvrirSession.Name = "BtnOuvrirSession";
            this.BtnOuvrirSession.Size = new System.Drawing.Size(158, 53);
            this.BtnOuvrirSession.TabIndex = 1;
            this.BtnOuvrirSession.Text = "Ouvrir Session";
            this.BtnOuvrirSession.UseVisualStyleBackColor = true;
            this.BtnOuvrirSession.Click += new System.EventHandler(this.BtnOuvrirSession_Click);
            // 
            // BtnCloreSession
            // 
            this.BtnCloreSession.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnCloreSession.ForeColor = System.Drawing.Color.Blue;
            this.BtnCloreSession.Location = new System.Drawing.Point(20, 137);
            this.BtnCloreSession.Margin = new System.Windows.Forms.Padding(4);
            this.BtnCloreSession.Name = "BtnCloreSession";
            this.BtnCloreSession.Size = new System.Drawing.Size(158, 45);
            this.BtnCloreSession.TabIndex = 2;
            this.BtnCloreSession.Text = "Clore Session";
            this.BtnCloreSession.UseVisualStyleBackColor = true;
            this.BtnCloreSession.Click += new System.EventHandler(this.BtnCloreSession_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtDateOuverture);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.CmbCaissier);
            this.groupBox1.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(204, 63);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(738, 160);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info Session";
            // 
            // txtDateOuverture
            // 
            this.txtDateOuverture.Location = new System.Drawing.Point(346, 99);
            this.txtDateOuverture.Margin = new System.Windows.Forms.Padding(4);
            this.txtDateOuverture.Name = "txtDateOuverture";
            this.txtDateOuverture.Size = new System.Drawing.Size(284, 35);
            this.txtDateOuverture.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(90, 99);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(165, 28);
            this.label3.TabIndex = 2;
            this.label3.Text = "Date Ouverture : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(90, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 28);
            this.label2.TabIndex = 1;
            this.label2.Text = "Caissier : ";
            // 
            // CmbCaissier
            // 
            this.CmbCaissier.FormattingEnabled = true;
            this.CmbCaissier.Location = new System.Drawing.Point(346, 39);
            this.CmbCaissier.Margin = new System.Windows.Forms.Padding(4);
            this.CmbCaissier.Name = "CmbCaissier";
            this.CmbCaissier.Size = new System.Drawing.Size(284, 36);
            this.CmbCaissier.TabIndex = 0;
            // 
            // DataGridViewSessions
            // 
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.DataGridViewSessions.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle9;
            this.DataGridViewSessions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridViewSessions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.DataGridViewSessions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DataGridViewSessions.DefaultCellStyle = dataGridViewCellStyle11;
            this.DataGridViewSessions.Location = new System.Drawing.Point(20, 249);
            this.DataGridViewSessions.Margin = new System.Windows.Forms.Padding(4);
            this.DataGridViewSessions.Name = "DataGridViewSessions";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridViewSessions.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.DataGridViewSessions.Size = new System.Drawing.Size(1281, 395);
            this.DataGridViewSessions.TabIndex = 4;
            // 
            // BtnExporterPDF
            // 
            this.BtnExporterPDF.Font = new System.Drawing.Font("HP Simplified", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnExporterPDF.ForeColor = System.Drawing.Color.Blue;
            this.BtnExporterPDF.Location = new System.Drawing.Point(1131, 652);
            this.BtnExporterPDF.Margin = new System.Windows.Forms.Padding(4);
            this.BtnExporterPDF.Name = "BtnExporterPDF";
            this.BtnExporterPDF.Size = new System.Drawing.Size(170, 38);
            this.BtnExporterPDF.TabIndex = 5;
            this.BtnExporterPDF.Text = "Exporter PDF";
            this.BtnExporterPDF.UseVisualStyleBackColor = true;
            this.BtnExporterPDF.Click += new System.EventHandler(this.BtnExporterPDF_Click);
            // 
            // BtnImprimerZ
            // 
            this.BtnImprimerZ.Font = new System.Drawing.Font("HP Simplified", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnImprimerZ.ForeColor = System.Drawing.Color.Blue;
            this.BtnImprimerZ.Location = new System.Drawing.Point(1131, 698);
            this.BtnImprimerZ.Margin = new System.Windows.Forms.Padding(4);
            this.BtnImprimerZ.Name = "BtnImprimerZ";
            this.BtnImprimerZ.Size = new System.Drawing.Size(170, 38);
            this.BtnImprimerZ.TabIndex = 6;
            this.BtnImprimerZ.Text = "Imprimer Z";
            this.BtnImprimerZ.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(69, 657);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 28);
            this.label4.TabIndex = 7;
            this.label4.Text = "Vente Espece :";
            // 
            // lblVenteEspece
            // 
            this.lblVenteEspece.AutoSize = true;
            this.lblVenteEspece.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVenteEspece.Location = new System.Drawing.Point(217, 657);
            this.lblVenteEspece.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVenteEspece.Name = "lblVenteEspece";
            this.lblVenteEspece.Size = new System.Drawing.Size(51, 28);
            this.lblVenteEspece.TabIndex = 8;
            this.lblVenteEspece.Text = "0.00";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(459, 657);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 28);
            this.label5.TabIndex = 9;
            this.label5.Text = "Vente Carte :";
            // 
            // lblVenteCarte
            // 
            this.lblVenteCarte.AutoSize = true;
            this.lblVenteCarte.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVenteCarte.Location = new System.Drawing.Point(594, 657);
            this.lblVenteCarte.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVenteCarte.Name = "lblVenteCarte";
            this.lblVenteCarte.Size = new System.Drawing.Size(51, 28);
            this.lblVenteCarte.TabIndex = 10;
            this.lblVenteCarte.Text = "0.00";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(280, 708);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(179, 28);
            this.label6.TabIndex = 11;
            this.label6.Text = "Remboursements :";
            // 
            // lblRemboursements
            // 
            this.lblRemboursements.AutoSize = true;
            this.lblRemboursements.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemboursements.Location = new System.Drawing.Point(473, 708);
            this.lblRemboursements.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRemboursements.Name = "lblRemboursements";
            this.lblRemboursements.Size = new System.Drawing.Size(51, 28);
            this.lblRemboursements.TabIndex = 12;
            this.lblRemboursements.Text = "0.00";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(720, 708);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 28);
            this.label7.TabIndex = 13;
            this.label7.Text = "Cash reel :";
            // 
            // lblCashReel
            // 
            this.lblCashReel.AutoSize = true;
            this.lblCashReel.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCashReel.Location = new System.Drawing.Point(832, 708);
            this.lblCashReel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCashReel.Name = "lblCashReel";
            this.lblCashReel.Size = new System.Drawing.Size(51, 28);
            this.lblCashReel.TabIndex = 14;
            this.lblCashReel.Text = "0.00";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(720, 657);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(200, 28);
            this.label8.TabIndex = 15;
            this.label8.Text = "Vente Mobile Money :";
            // 
            // lblMobileMoney
            // 
            this.lblMobileMoney.AutoSize = true;
            this.lblMobileMoney.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMobileMoney.Location = new System.Drawing.Point(928, 657);
            this.lblMobileMoney.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMobileMoney.Name = "lblMobileMoney";
            this.lblMobileMoney.Size = new System.Drawing.Size(51, 28);
            this.lblMobileMoney.TabIndex = 16;
            this.lblMobileMoney.Text = "0.00";
            // 
            // SessionCaisse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.lblMobileMoney);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblCashReel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblRemboursements);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblVenteCarte);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblVenteEspece);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BtnImprimerZ);
            this.Controls.Add(this.BtnExporterPDF);
            this.Controls.Add(this.DataGridViewSessions);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.BtnCloreSession);
            this.Controls.Add(this.BtnOuvrirSession);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SessionCaisse";
            this.Text = "SessionCaisse";
            this.Load += new System.EventHandler(this.SessionCaisse_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridViewSessions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnOuvrirSession;
        private System.Windows.Forms.Button BtnCloreSession;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtDateOuverture;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView DataGridViewSessions;
        private System.Windows.Forms.Button BtnExporterPDF;
        private System.Windows.Forms.Button BtnImprimerZ;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblVenteEspece;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblVenteCarte;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblRemboursements;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblCashReel;
        private System.Windows.Forms.ComboBox CmbCaissier;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblMobileMoney;
    }
}