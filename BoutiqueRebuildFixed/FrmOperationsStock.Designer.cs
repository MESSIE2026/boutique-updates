namespace BoutiqueRebuildFixed
{
    partial class FrmOperationsStock
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
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblProduit = new System.Windows.Forms.Label();
            this.cmbProduit = new System.Windows.Forms.ComboBox();
            this.lblTypeOperation = new System.Windows.Forms.Label();
            this.cmbTypeOperation = new System.Windows.Forms.ComboBox();
            this.lblQuantite = new System.Windows.Forms.Label();
            this.nudQuantite = new System.Windows.Forms.NumericUpDown();
            this.lblDateOperation = new System.Windows.Forms.Label();
            this.dtpDateOperation = new System.Windows.Forms.DateTimePicker();
            this.lblUtilisateur = new System.Windows.Forms.Label();
            this.txtUtilisateur = new System.Windows.Forms.TextBox();
            this.lblMotif = new System.Windows.Forms.Label();
            this.txtMotif = new System.Windows.Forms.TextBox();
            this.lblReference = new System.Windows.Forms.Label();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.lblEmplacement = new System.Windows.Forms.Label();
            this.txtEmplacement = new System.Windows.Forms.TextBox();
            this.lblRemarques = new System.Windows.Forms.Label();
            this.txtRemarques = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnEffacer = new System.Windows.Forms.Button();
            this.dgvOperations = new System.Windows.Forms.DataGridView();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbDepotSource = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbDepotDestination = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOperations)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblTitre.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitre.Font = new System.Drawing.Font("Matura MT Script Capitals", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.Location = new System.Drawing.Point(510, 9);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(335, 41);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "Gestion avancée du stock";
            // 
            // lblProduit
            // 
            this.lblProduit.AutoSize = true;
            this.lblProduit.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProduit.Location = new System.Drawing.Point(34, 72);
            this.lblProduit.Name = "lblProduit";
            this.lblProduit.Size = new System.Drawing.Size(80, 23);
            this.lblProduit.TabIndex = 1;
            this.lblProduit.Text = "Produit : ";
            // 
            // cmbProduit
            // 
            this.cmbProduit.FormattingEnabled = true;
            this.cmbProduit.Location = new System.Drawing.Point(211, 67);
            this.cmbProduit.Name = "cmbProduit";
            this.cmbProduit.Size = new System.Drawing.Size(316, 28);
            this.cmbProduit.TabIndex = 2;
            // 
            // lblTypeOperation
            // 
            this.lblTypeOperation.AutoSize = true;
            this.lblTypeOperation.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTypeOperation.Location = new System.Drawing.Point(33, 114);
            this.lblTypeOperation.Name = "lblTypeOperation";
            this.lblTypeOperation.Size = new System.Drawing.Size(152, 23);
            this.lblTypeOperation.TabIndex = 3;
            this.lblTypeOperation.Text = "Type d\'opération : ";
            // 
            // cmbTypeOperation
            // 
            this.cmbTypeOperation.FormattingEnabled = true;
            this.cmbTypeOperation.Location = new System.Drawing.Point(211, 109);
            this.cmbTypeOperation.Name = "cmbTypeOperation";
            this.cmbTypeOperation.Size = new System.Drawing.Size(316, 28);
            this.cmbTypeOperation.TabIndex = 4;
            // 
            // lblQuantite
            // 
            this.lblQuantite.AutoSize = true;
            this.lblQuantite.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantite.Location = new System.Drawing.Point(33, 151);
            this.lblQuantite.Name = "lblQuantite";
            this.lblQuantite.Size = new System.Drawing.Size(91, 23);
            this.lblQuantite.TabIndex = 5;
            this.lblQuantite.Text = "Quantité : ";
            // 
            // nudQuantite
            // 
            this.nudQuantite.Location = new System.Drawing.Point(211, 147);
            this.nudQuantite.Name = "nudQuantite";
            this.nudQuantite.Size = new System.Drawing.Size(170, 27);
            this.nudQuantite.TabIndex = 6;
            // 
            // lblDateOperation
            // 
            this.lblDateOperation.AutoSize = true;
            this.lblDateOperation.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateOperation.Location = new System.Drawing.Point(34, 189);
            this.lblDateOperation.Name = "lblDateOperation";
            this.lblDateOperation.Size = new System.Drawing.Size(171, 23);
            this.lblDateOperation.TabIndex = 7;
            this.lblDateOperation.Text = "Date de l\'opération : ";
            // 
            // dtpDateOperation
            // 
            this.dtpDateOperation.Location = new System.Drawing.Point(211, 189);
            this.dtpDateOperation.Name = "dtpDateOperation";
            this.dtpDateOperation.Size = new System.Drawing.Size(316, 27);
            this.dtpDateOperation.TabIndex = 8;
            // 
            // lblUtilisateur
            // 
            this.lblUtilisateur.AutoSize = true;
            this.lblUtilisateur.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUtilisateur.Location = new System.Drawing.Point(33, 226);
            this.lblUtilisateur.Name = "lblUtilisateur";
            this.lblUtilisateur.Size = new System.Drawing.Size(101, 23);
            this.lblUtilisateur.TabIndex = 9;
            this.lblUtilisateur.Text = "Utilisateur : ";
            // 
            // txtUtilisateur
            // 
            this.txtUtilisateur.Location = new System.Drawing.Point(211, 222);
            this.txtUtilisateur.Name = "txtUtilisateur";
            this.txtUtilisateur.Size = new System.Drawing.Size(316, 27);
            this.txtUtilisateur.TabIndex = 10;
            // 
            // lblMotif
            // 
            this.lblMotif.AutoSize = true;
            this.lblMotif.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotif.Location = new System.Drawing.Point(646, 67);
            this.lblMotif.Name = "lblMotif";
            this.lblMotif.Size = new System.Drawing.Size(121, 23);
            this.lblMotif.TabIndex = 11;
            this.lblMotif.Text = "Motif/Raison : ";
            // 
            // txtMotif
            // 
            this.txtMotif.Location = new System.Drawing.Point(791, 63);
            this.txtMotif.Multiline = true;
            this.txtMotif.Name = "txtMotif";
            this.txtMotif.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMotif.Size = new System.Drawing.Size(531, 74);
            this.txtMotif.TabIndex = 12;
            // 
            // lblReference
            // 
            this.lblReference.AutoSize = true;
            this.lblReference.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReference.Location = new System.Drawing.Point(646, 146);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(99, 23);
            this.lblReference.TabIndex = 13;
            this.lblReference.Text = "Reference : ";
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(791, 146);
            this.txtReference.Multiline = true;
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(531, 28);
            this.txtReference.TabIndex = 14;
            // 
            // lblEmplacement
            // 
            this.lblEmplacement.AutoSize = true;
            this.lblEmplacement.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmplacement.Location = new System.Drawing.Point(646, 181);
            this.lblEmplacement.Name = "lblEmplacement";
            this.lblEmplacement.Size = new System.Drawing.Size(128, 23);
            this.lblEmplacement.TabIndex = 15;
            this.lblEmplacement.Text = "Emplacement : ";
            // 
            // txtEmplacement
            // 
            this.txtEmplacement.Location = new System.Drawing.Point(791, 180);
            this.txtEmplacement.Multiline = true;
            this.txtEmplacement.Name = "txtEmplacement";
            this.txtEmplacement.Size = new System.Drawing.Size(531, 28);
            this.txtEmplacement.TabIndex = 16;
            // 
            // lblRemarques
            // 
            this.lblRemarques.AutoSize = true;
            this.lblRemarques.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemarques.Location = new System.Drawing.Point(646, 222);
            this.lblRemarques.Name = "lblRemarques";
            this.lblRemarques.Size = new System.Drawing.Size(109, 23);
            this.lblRemarques.TabIndex = 17;
            this.lblRemarques.Text = "Remarques : ";
            // 
            // txtRemarques
            // 
            this.txtRemarques.Location = new System.Drawing.Point(791, 214);
            this.txtRemarques.Multiline = true;
            this.txtRemarques.Name = "txtRemarques";
            this.txtRemarques.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemarques.Size = new System.Drawing.Size(531, 74);
            this.txtRemarques.TabIndex = 18;
            // 
            // btnEnregistrer
            // 
            this.btnEnregistrer.BackColor = System.Drawing.Color.Green;
            this.btnEnregistrer.Font = new System.Drawing.Font("Comic Sans MS", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnregistrer.ForeColor = System.Drawing.Color.White;
            this.btnEnregistrer.Location = new System.Drawing.Point(791, 304);
            this.btnEnregistrer.Name = "btnEnregistrer";
            this.btnEnregistrer.Size = new System.Drawing.Size(140, 32);
            this.btnEnregistrer.TabIndex = 19;
            this.btnEnregistrer.Text = "Enregistrer";
            this.btnEnregistrer.UseVisualStyleBackColor = false;
            this.btnEnregistrer.Click += new System.EventHandler(this.btnEnregistrer_Click);
            // 
            // btnEffacer
            // 
            this.btnEffacer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnEffacer.Font = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEffacer.ForeColor = System.Drawing.Color.Black;
            this.btnEffacer.Location = new System.Drawing.Point(1189, 302);
            this.btnEffacer.Name = "btnEffacer";
            this.btnEffacer.Size = new System.Drawing.Size(124, 32);
            this.btnEffacer.TabIndex = 20;
            this.btnEffacer.Text = "Effacer";
            this.btnEffacer.UseVisualStyleBackColor = false;
            this.btnEffacer.Click += new System.EventHandler(this.btnEffacer_Click);
            // 
            // dgvOperations
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvOperations.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvOperations.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvOperations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvOperations.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvOperations.Location = new System.Drawing.Point(37, 357);
            this.dgvOperations.Name = "dgvOperations";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvOperations.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvOperations.Size = new System.Drawing.Size(1285, 380);
            this.dgvOperations.StandardTab = true;
            this.dgvOperations.TabIndex = 21;
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.BackColor = System.Drawing.Color.White;
            this.btnExporterPDF.Font = new System.Drawing.Font("Rockwell", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporterPDF.ForeColor = System.Drawing.Color.Blue;
            this.btnExporterPDF.Location = new System.Drawing.Point(978, 304);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(163, 32);
            this.btnExporterPDF.TabIndex = 22;
            this.btnExporterPDF.Text = "Exporter PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = false;
            this.btnExporterPDF.Click += new System.EventHandler(this.btnExporterPDF_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 265);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 23);
            this.label1.TabIndex = 23;
            this.label1.Text = "Depôt Source : ";
            // 
            // cmbDepotSource
            // 
            this.cmbDepotSource.FormattingEnabled = true;
            this.cmbDepotSource.Location = new System.Drawing.Point(211, 260);
            this.cmbDepotSource.Name = "cmbDepotSource";
            this.cmbDepotSource.Size = new System.Drawing.Size(316, 28);
            this.cmbDepotSource.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(34, 302);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 23);
            this.label2.TabIndex = 25;
            this.label2.Text = "Depôt Destination : ";
            // 
            // cmbDepotDestination
            // 
            this.cmbDepotDestination.FormattingEnabled = true;
            this.cmbDepotDestination.Location = new System.Drawing.Point(211, 297);
            this.cmbDepotDestination.Name = "cmbDepotDestination";
            this.cmbDepotDestination.Size = new System.Drawing.Size(316, 28);
            this.cmbDepotDestination.TabIndex = 26;
            // 
            // FrmOperationsStock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(1334, 749);
            this.Controls.Add(this.cmbDepotDestination);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbDepotSource);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnExporterPDF);
            this.Controls.Add(this.dgvOperations);
            this.Controls.Add(this.btnEffacer);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.txtRemarques);
            this.Controls.Add(this.lblRemarques);
            this.Controls.Add(this.txtEmplacement);
            this.Controls.Add(this.lblEmplacement);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.lblReference);
            this.Controls.Add(this.txtMotif);
            this.Controls.Add(this.lblMotif);
            this.Controls.Add(this.txtUtilisateur);
            this.Controls.Add(this.lblUtilisateur);
            this.Controls.Add(this.dtpDateOperation);
            this.Controls.Add(this.lblDateOperation);
            this.Controls.Add(this.nudQuantite);
            this.Controls.Add(this.lblQuantite);
            this.Controls.Add(this.cmbTypeOperation);
            this.Controls.Add(this.lblTypeOperation);
            this.Controls.Add(this.cmbProduit);
            this.Controls.Add(this.lblProduit);
            this.Controls.Add(this.lblTitre);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmOperationsStock";
            this.Text = "Gestion Avancée du Stock";
            this.Load += new System.EventHandler(this.FrmOperationsStock_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOperations)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblProduit;
        private System.Windows.Forms.ComboBox cmbProduit;
        private System.Windows.Forms.Label lblTypeOperation;
        private System.Windows.Forms.ComboBox cmbTypeOperation;
        private System.Windows.Forms.Label lblQuantite;
        private System.Windows.Forms.NumericUpDown nudQuantite;
        private System.Windows.Forms.Label lblDateOperation;
        private System.Windows.Forms.DateTimePicker dtpDateOperation;
        private System.Windows.Forms.Label lblUtilisateur;
        private System.Windows.Forms.TextBox txtUtilisateur;
        private System.Windows.Forms.Label lblMotif;
        private System.Windows.Forms.TextBox txtMotif;
        private System.Windows.Forms.Label lblReference;
        private System.Windows.Forms.TextBox txtReference;
        private System.Windows.Forms.Label lblEmplacement;
        private System.Windows.Forms.TextBox txtEmplacement;
        private System.Windows.Forms.Label lblRemarques;
        private System.Windows.Forms.TextBox txtRemarques;
        private System.Windows.Forms.Button btnEnregistrer;
        private System.Windows.Forms.Button btnEffacer;
        private System.Windows.Forms.DataGridView dgvOperations;
        private System.Windows.Forms.Button btnExporterPDF;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbDepotSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDepotDestination;
    }
}