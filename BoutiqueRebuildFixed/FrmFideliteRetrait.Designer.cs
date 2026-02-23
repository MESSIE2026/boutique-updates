namespace BoutiqueRebuildFixed
{
    partial class FrmFideliteRetrait
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
            this.txtScanCarte = new System.Windows.Forms.TextBox();
            this.lblTitre = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSoldeCDF = new System.Windows.Forms.Label();
            this.lblClientNom = new System.Windows.Forms.Label();
            this.lblSoldeUSD = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMontantAUtiliser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbDevise = new System.Windows.Forms.ComboBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnAppliquer = new System.Windows.Forms.Button();
            this.dgvHistorique = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorique)).BeginInit();
            this.SuspendLayout();
            // 
            // txtScanCarte
            // 
            this.txtScanCarte.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtScanCarte.Location = new System.Drawing.Point(341, 91);
            this.txtScanCarte.Name = "txtScanCarte";
            this.txtScanCarte.Size = new System.Drawing.Size(352, 29);
            this.txtScanCarte.TabIndex = 0;
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblTitre.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitre.Font = new System.Drawing.Font("Impact", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.White;
            this.lblTitre.Location = new System.Drawing.Point(419, 18);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(336, 41);
            this.lblTitre.TabIndex = 1;
            this.lblTitre.Text = "RETRAIT CARTE FIDELITE";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(125, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "Code Scan Carte :";
            // 
            // lblSoldeCDF
            // 
            this.lblSoldeCDF.AutoSize = true;
            this.lblSoldeCDF.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSoldeCDF.ForeColor = System.Drawing.Color.White;
            this.lblSoldeCDF.Location = new System.Drawing.Point(336, 174);
            this.lblSoldeCDF.Name = "lblSoldeCDF";
            this.lblSoldeCDF.Size = new System.Drawing.Size(125, 30);
            this.lblSoldeCDF.TabIndex = 3;
            this.lblSoldeCDF.Text = "Solde CDF :";
            // 
            // lblClientNom
            // 
            this.lblClientNom.AutoSize = true;
            this.lblClientNom.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClientNom.ForeColor = System.Drawing.Color.White;
            this.lblClientNom.Location = new System.Drawing.Point(336, 132);
            this.lblClientNom.Name = "lblClientNom";
            this.lblClientNom.Size = new System.Drawing.Size(137, 30);
            this.lblClientNom.TabIndex = 4;
            this.lblClientNom.Text = "Nom Client :";
            // 
            // lblSoldeUSD
            // 
            this.lblSoldeUSD.AutoSize = true;
            this.lblSoldeUSD.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSoldeUSD.ForeColor = System.Drawing.Color.White;
            this.lblSoldeUSD.Location = new System.Drawing.Point(336, 209);
            this.lblSoldeUSD.Name = "lblSoldeUSD";
            this.lblSoldeUSD.Size = new System.Drawing.Size(128, 30);
            this.lblSoldeUSD.TabIndex = 5;
            this.lblSoldeUSD.Text = "Solde USD :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(122, 252);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(203, 30);
            this.label2.TabIndex = 6;
            this.label2.Text = "Montant à Utiliser :";
            // 
            // txtMontantAUtiliser
            // 
            this.txtMontantAUtiliser.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMontantAUtiliser.Location = new System.Drawing.Point(341, 252);
            this.txtMontantAUtiliser.Name = "txtMontantAUtiliser";
            this.txtMontantAUtiliser.Size = new System.Drawing.Size(352, 29);
            this.txtMontantAUtiliser.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(120, 291);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 30);
            this.label3.TabIndex = 8;
            this.label3.Text = "Devise :";
            // 
            // cmbDevise
            // 
            this.cmbDevise.FormattingEnabled = true;
            this.cmbDevise.Location = new System.Drawing.Point(341, 291);
            this.cmbDevise.Name = "cmbDevise";
            this.cmbDevise.Size = new System.Drawing.Size(136, 29);
            this.cmbDevise.TabIndex = 9;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.White;
            this.lblMessage.Location = new System.Drawing.Point(120, 332);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(109, 30);
            this.lblMessage.TabIndex = 10;
            this.lblMessage.Text = "Message :";
            // 
            // btnAppliquer
            // 
            this.btnAppliquer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnAppliquer.Font = new System.Drawing.Font("Britannic Bold", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAppliquer.ForeColor = System.Drawing.Color.White;
            this.btnAppliquer.Location = new System.Drawing.Point(551, 373);
            this.btnAppliquer.Name = "btnAppliquer";
            this.btnAppliquer.Size = new System.Drawing.Size(142, 33);
            this.btnAppliquer.TabIndex = 11;
            this.btnAppliquer.Text = "Appliquer";
            this.btnAppliquer.UseVisualStyleBackColor = false;
            this.btnAppliquer.Click += new System.EventHandler(this.btnAppliquer_Click);
            // 
            // dgvHistorique
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvHistorique.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHistorique.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHistorique.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvHistorique.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvHistorique.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvHistorique.Location = new System.Drawing.Point(12, 413);
            this.dgvHistorique.Name = "dgvHistorique";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHistorique.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvHistorique.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvHistorique.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistorique.Size = new System.Drawing.Size(1176, 302);
            this.dgvHistorique.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(125, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 30);
            this.label4.TabIndex = 13;
            this.label4.Text = "Nom Client :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(122, 177);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 30);
            this.label5.TabIndex = 14;
            this.label5.Text = "Solde CDF :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(120, 209);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 30);
            this.label6.TabIndex = 15;
            this.label6.Text = "Solde USD :";
            // 
            // FrmFideliteRetrait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1200, 727);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgvHistorique);
            this.Controls.Add(this.btnAppliquer);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.cmbDevise);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtMontantAUtiliser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblSoldeUSD);
            this.Controls.Add(this.lblClientNom);
            this.Controls.Add(this.lblSoldeCDF);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTitre);
            this.Controls.Add(this.txtScanCarte);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmFideliteRetrait";
            this.Text = "Carte Fidelité Retrait";
            this.Load += new System.EventHandler(this.FrmFideliteRetrait_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistorique)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtScanCarte;
        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSoldeCDF;
        private System.Windows.Forms.Label lblClientNom;
        private System.Windows.Forms.Label lblSoldeUSD;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtMontantAUtiliser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbDevise;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnAppliquer;
        private System.Windows.Forms.DataGridView dgvHistorique;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}