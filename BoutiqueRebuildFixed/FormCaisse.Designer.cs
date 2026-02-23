namespace BoutiqueRebuildFixed
{
    partial class FormCaisse
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.Label1 = new System.Windows.Forms.Label();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.numQuantite = new System.Windows.Forms.NumericUpDown();
            this.Label4 = new System.Windows.Forms.Label();
            this.txtPrix = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.txtMontantTotal = new System.Windows.Forms.TextBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.dtpDateTransaction = new System.Windows.Forms.DateTimePicker();
            this.Label7 = new System.Windows.Forms.Label();
            this.cmbModePaiement = new System.Windows.Forms.ComboBox();
            this.Label8 = new System.Windows.Forms.Label();
            this.txtNomClient = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.txtIDEmploye = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnDetails = new System.Windows.Forms.Button();
            this.dgvCaisse = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCaisse)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(12, 9);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(147, 23);
            this.Label1.TabIndex = 3;
            this.Label1.Text = "Reference Produit";
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(180, 12);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(277, 20);
            this.txtReference.TabIndex = 4;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(12, 49);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(110, 23);
            this.Label2.TabIndex = 5;
            this.Label2.Text = "Nom Produit";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(180, 49);
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(277, 20);
            this.txtNomProduit.TabIndex = 6;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(12, 86);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(78, 23);
            this.Label3.TabIndex = 7;
            this.Label3.Text = "Quantite";
            // 
            // numQuantite
            // 
            this.numQuantite.Location = new System.Drawing.Point(257, 86);
            this.numQuantite.Name = "numQuantite";
            this.numQuantite.Size = new System.Drawing.Size(200, 20);
            this.numQuantite.TabIndex = 8;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(15, 124);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(107, 23);
            this.Label4.TabIndex = 9;
            this.Label4.Text = "Prix Unitaire";
            // 
            // txtPrix
            // 
            this.txtPrix.Location = new System.Drawing.Point(180, 124);
            this.txtPrix.Name = "txtPrix";
            this.txtPrix.Size = new System.Drawing.Size(277, 20);
            this.txtPrix.TabIndex = 10;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(15, 164);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(121, 23);
            this.Label5.TabIndex = 11;
            this.Label5.Text = "Montant Total";
            // 
            // txtMontantTotal
            // 
            this.txtMontantTotal.Location = new System.Drawing.Point(180, 164);
            this.txtMontantTotal.Name = "txtMontantTotal";
            this.txtMontantTotal.Size = new System.Drawing.Size(277, 20);
            this.txtMontantTotal.TabIndex = 12;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(15, 204);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(120, 23);
            this.Label6.TabIndex = 13;
            this.Label6.Text = "Date de Vente";
            // 
            // dtpDateTransaction
            // 
            this.dtpDateTransaction.Location = new System.Drawing.Point(257, 204);
            this.dtpDateTransaction.Name = "dtpDateTransaction";
            this.dtpDateTransaction.Size = new System.Drawing.Size(200, 20);
            this.dtpDateTransaction.TabIndex = 14;
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(15, 239);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(151, 23);
            this.Label7.TabIndex = 15;
            this.Label7.Text = "Mode de Paiement";
            // 
            // cmbModePaiement
            // 
            this.cmbModePaiement.FormattingEnabled = true;
            this.cmbModePaiement.Location = new System.Drawing.Point(257, 239);
            this.cmbModePaiement.Name = "cmbModePaiement";
            this.cmbModePaiement.Size = new System.Drawing.Size(200, 21);
            this.cmbModePaiement.TabIndex = 16;
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(15, 278);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(120, 23);
            this.Label8.TabIndex = 17;
            this.Label8.Text = "Nom du Client";
            // 
            // txtNomClient
            // 
            this.txtNomClient.Location = new System.Drawing.Point(180, 278);
            this.txtNomClient.Name = "txtNomClient";
            this.txtNomClient.Size = new System.Drawing.Size(277, 20);
            this.txtNomClient.TabIndex = 18;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(15, 319);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(97, 23);
            this.Label9.TabIndex = 19;
            this.Label9.Text = "ID Employe";
            // 
            // txtIDEmploye
            // 
            this.txtIDEmploye.Location = new System.Drawing.Point(180, 319);
            this.txtIDEmploye.Name = "txtIDEmploye";
            this.txtIDEmploye.Size = new System.Drawing.Size(277, 20);
            this.txtIDEmploye.TabIndex = 20;
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("HP Simplified", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(19, 386);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 28);
            this.btnOk.TabIndex = 21;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Font = new System.Drawing.Font("HP Simplified", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.Location = new System.Drawing.Point(135, 386);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(75, 28);
            this.btnAnnuler.TabIndex = 22;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Font = new System.Drawing.Font("HP Simplified", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.Red;
            this.btnSupprimer.Location = new System.Drawing.Point(243, 386);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(93, 28);
            this.btnSupprimer.TabIndex = 23;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click_1);
            // 
            // btnDetails
            // 
            this.btnDetails.Font = new System.Drawing.Font("HP Simplified", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnDetails.Location = new System.Drawing.Point(382, 386);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(75, 28);
            this.btnDetails.TabIndex = 24;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // dgvCaisse
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvCaisse.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvCaisse.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCaisse.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvCaisse.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCaisse.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvCaisse.Location = new System.Drawing.Point(478, 9);
            this.dgvCaisse.Name = "dgvCaisse";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCaisse.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvCaisse.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dgvCaisse.Size = new System.Drawing.Size(824, 728);
            this.dgvCaisse.TabIndex = 25;
            // 
            // FormCaisse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.dgvCaisse);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtIDEmploye);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.txtNomClient);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.cmbModePaiement);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.dtpDateTransaction);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.txtMontantTotal);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.txtPrix);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.numQuantite);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.txtNomProduit);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.Label1);
            this.Name = "FormCaisse";
            this.Text = "FormCaisse";
            this.Load += new System.EventHandler(this.FormCaisse_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCaisse)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox txtReference;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox txtNomProduit;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.NumericUpDown numQuantite;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.TextBox txtPrix;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.TextBox txtMontantTotal;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.DateTimePicker dtpDateTransaction;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.ComboBox cmbModePaiement;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.TextBox txtNomClient;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.TextBox txtIDEmploye;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Button btnAnnuler;
        internal System.Windows.Forms.Button btnSupprimer;
        internal System.Windows.Forms.Button btnDetails;
        private System.Windows.Forms.DataGridView dgvCaisse;
    }
}