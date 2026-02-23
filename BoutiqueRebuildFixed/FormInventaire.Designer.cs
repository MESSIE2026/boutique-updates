namespace BoutiqueRebuildFixed
{
    partial class FormInventaire
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
            this.btDetails = new System.Windows.Forms.Button();
            this.btSupprimer = new System.Windows.Forms.Button();
            this.btAnnuler = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.btChangerImage = new System.Windows.Forms.Button();
            this.picProduit = new System.Windows.Forms.PictureBox();
            this.txtFournisseur = new System.Windows.Forms.TextBox();
            this.dtpDateAjout = new System.Windows.Forms.DateTimePicker();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.txtPrixUnitaire = new System.Windows.Forms.TextBox();
            this.numQuantite = new System.Windows.Forms.NumericUpDown();
            this.cmbCategorie = new System.Windows.Forms.ComboBox();
            this.txtReference = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.txtNomProduit = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.lblQuantiteRestante = new System.Windows.Forms.Label();
            this.dgvInventaire = new System.Windows.Forms.DataGridView();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            this.lblJour = new System.Windows.Forms.Label();
            this.lblSemaine = new System.Windows.Forms.Label();
            this.lblMois = new System.Windows.Forms.Label();
            this.lblAnnee = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.btOuvrirScanner = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventaire)).BeginInit();
            this.SuspendLayout();
            // 
            // btDetails
            // 
            this.btDetails.Font = new System.Drawing.Font("Rockwell Extra Bold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btDetails.ForeColor = System.Drawing.Color.Navy;
            this.btDetails.Location = new System.Drawing.Point(375, 608);
            this.btDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btDetails.Name = "btDetails";
            this.btDetails.Size = new System.Drawing.Size(101, 43);
            this.btDetails.TabIndex = 47;
            this.btDetails.Text = "Details";
            this.btDetails.UseVisualStyleBackColor = true;
            this.btDetails.Click += new System.EventHandler(this.btDetails_Click);
            // 
            // btSupprimer
            // 
            this.btSupprimer.Font = new System.Drawing.Font("Rockwell Extra Bold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSupprimer.ForeColor = System.Drawing.Color.Red;
            this.btSupprimer.Location = new System.Drawing.Point(225, 608);
            this.btSupprimer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btSupprimer.Name = "btSupprimer";
            this.btSupprimer.Size = new System.Drawing.Size(142, 43);
            this.btSupprimer.TabIndex = 46;
            this.btSupprimer.Text = "Supprimer";
            this.btSupprimer.UseVisualStyleBackColor = true;
            this.btSupprimer.Click += new System.EventHandler(this.btSupprimer_Click);
            // 
            // btAnnuler
            // 
            this.btAnnuler.Font = new System.Drawing.Font("Rockwell Extra Bold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btAnnuler.Location = new System.Drawing.Point(94, 608);
            this.btAnnuler.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btAnnuler.Name = "btAnnuler";
            this.btAnnuler.Size = new System.Drawing.Size(123, 43);
            this.btAnnuler.TabIndex = 45;
            this.btAnnuler.Text = "Annuler";
            this.btAnnuler.UseVisualStyleBackColor = true;
            this.btAnnuler.Click += new System.EventHandler(this.btAnnuler_Click);
            // 
            // btOk
            // 
            this.btOk.Font = new System.Drawing.Font("Rockwell Extra Bold", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOk.ForeColor = System.Drawing.Color.Black;
            this.btOk.Location = new System.Drawing.Point(6, 608);
            this.btOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(80, 43);
            this.btOk.TabIndex = 44;
            this.btOk.Text = "Ok";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btChangerImage
            // 
            this.btChangerImage.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btChangerImage.ForeColor = System.Drawing.Color.Navy;
            this.btChangerImage.Location = new System.Drawing.Point(13, 424);
            this.btChangerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btChangerImage.Name = "btChangerImage";
            this.btChangerImage.Size = new System.Drawing.Size(135, 38);
            this.btChangerImage.TabIndex = 43;
            this.btChangerImage.Text = "Changer Image";
            this.btChangerImage.UseVisualStyleBackColor = true;
            this.btChangerImage.Click += new System.EventHandler(this.btChangerImage_Click);
            // 
            // picProduit
            // 
            this.picProduit.Location = new System.Drawing.Point(261, 382);
            this.picProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picProduit.Name = "picProduit";
            this.picProduit.Size = new System.Drawing.Size(219, 205);
            this.picProduit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picProduit.TabIndex = 42;
            this.picProduit.TabStop = false;
            // 
            // txtFournisseur
            // 
            this.txtFournisseur.Location = new System.Drawing.Point(192, 340);
            this.txtFournisseur.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtFournisseur.Name = "txtFournisseur";
            this.txtFournisseur.Size = new System.Drawing.Size(288, 27);
            this.txtFournisseur.TabIndex = 41;
            // 
            // dtpDateAjout
            // 
            this.dtpDateAjout.Location = new System.Drawing.Point(192, 300);
            this.dtpDateAjout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpDateAjout.Name = "dtpDateAjout";
            this.dtpDateAjout.Size = new System.Drawing.Size(187, 27);
            this.dtpDateAjout.TabIndex = 40;
            // 
            // rtbDescription
            // 
            this.rtbDescription.Location = new System.Drawing.Point(192, 189);
            this.rtbDescription.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.Size = new System.Drawing.Size(288, 96);
            this.rtbDescription.TabIndex = 39;
            this.rtbDescription.Text = "";
            // 
            // txtPrixUnitaire
            // 
            this.txtPrixUnitaire.Location = new System.Drawing.Point(194, 152);
            this.txtPrixUnitaire.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPrixUnitaire.Name = "txtPrixUnitaire";
            this.txtPrixUnitaire.Size = new System.Drawing.Size(163, 27);
            this.txtPrixUnitaire.TabIndex = 38;
            // 
            // numQuantite
            // 
            this.numQuantite.Location = new System.Drawing.Point(194, 115);
            this.numQuantite.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numQuantite.Name = "numQuantite";
            this.numQuantite.Size = new System.Drawing.Size(91, 27);
            this.numQuantite.TabIndex = 37;
            // 
            // cmbCategorie
            // 
            this.cmbCategorie.FormattingEnabled = true;
            this.cmbCategorie.Location = new System.Drawing.Point(192, 81);
            this.cmbCategorie.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbCategorie.Name = "cmbCategorie";
            this.cmbCategorie.Size = new System.Drawing.Size(288, 28);
            this.cmbCategorie.TabIndex = 36;
            // 
            // txtReference
            // 
            this.txtReference.Location = new System.Drawing.Point(194, 47);
            this.txtReference.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtReference.Name = "txtReference";
            this.txtReference.Size = new System.Drawing.Size(286, 27);
            this.txtReference.TabIndex = 35;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(18, 115);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(102, 27);
            this.Label2.TabIndex = 34;
            this.Label2.Text = "Quantite :";
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(18, 382);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(77, 25);
            this.Label9.TabIndex = 33;
            this.Label9.Text = "Image :";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(13, 340);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(127, 25);
            this.Label8.TabIndex = 32;
            this.Label8.Text = "Fournisseur :";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(16, 300);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(132, 25);
            this.Label7.TabIndex = 31;
            this.Label7.Text = "Date d\'ajout :";
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(18, 188);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(125, 27);
            this.Label6.TabIndex = 30;
            this.Label6.Text = "Description :";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(18, 152);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(142, 27);
            this.Label5.TabIndex = 29;
            this.Label5.Text = "Prix Unitaire : ";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(16, 81);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(110, 27);
            this.Label4.TabIndex = 28;
            this.Label4.Text = "Categorie :";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(18, 47);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(111, 27);
            this.Label3.TabIndex = 27;
            this.Label3.Text = "Reference :";
            // 
            // txtNomProduit
            // 
            this.txtNomProduit.Location = new System.Drawing.Point(194, 10);
            this.txtNomProduit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNomProduit.Name = "txtNomProduit";
            this.txtNomProduit.Size = new System.Drawing.Size(286, 27);
            this.txtNomProduit.TabIndex = 26;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(18, 9);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(168, 25);
            this.Label1.TabIndex = 25;
            this.Label1.Text = "Nom du Produit :";
            // 
            // lblQuantiteRestante
            // 
            this.lblQuantiteRestante.AutoSize = true;
            this.lblQuantiteRestante.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantiteRestante.Location = new System.Drawing.Point(293, 114);
            this.lblQuantiteRestante.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblQuantiteRestante.Name = "lblQuantiteRestante";
            this.lblQuantiteRestante.Size = new System.Drawing.Size(170, 28);
            this.lblQuantiteRestante.TabIndex = 48;
            this.lblQuantiteRestante.Text = "Quantite Restant:";
            // 
            // dgvInventaire
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvInventaire.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvInventaire.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgvInventaire.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvInventaire.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvInventaire.Location = new System.Drawing.Point(498, 12);
            this.dgvInventaire.Name = "dgvInventaire";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvInventaire.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvInventaire.RowsDefaultCellStyle = dataGridViewCellStyle10;
            this.dgvInventaire.Size = new System.Drawing.Size(860, 660);
            this.dgvInventaire.TabIndex = 49;
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporterPDF.ForeColor = System.Drawing.Color.Black;
            this.btnExporterPDF.Location = new System.Drawing.Point(6, 671);
            this.btnExporterPDF.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(154, 43);
            this.btnExporterPDF.TabIndex = 50;
            this.btnExporterPDF.Text = "Exporter PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = true;
            // 
            // lblJour
            // 
            this.lblJour.AutoSize = true;
            this.lblJour.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJour.Location = new System.Drawing.Point(476, 689);
            this.lblJour.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblJour.Name = "lblJour";
            this.lblJour.Size = new System.Drawing.Size(34, 25);
            this.lblJour.TabIndex = 51;
            this.lblJour.Text = "00";
            // 
            // lblSemaine
            // 
            this.lblSemaine.AutoSize = true;
            this.lblSemaine.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSemaine.Location = new System.Drawing.Point(764, 689);
            this.lblSemaine.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSemaine.Name = "lblSemaine";
            this.lblSemaine.Size = new System.Drawing.Size(34, 25);
            this.lblSemaine.TabIndex = 52;
            this.lblSemaine.Text = "00";
            // 
            // lblMois
            // 
            this.lblMois.AutoSize = true;
            this.lblMois.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMois.Location = new System.Drawing.Point(1021, 689);
            this.lblMois.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMois.Name = "lblMois";
            this.lblMois.Size = new System.Drawing.Size(34, 25);
            this.lblMois.TabIndex = 53;
            this.lblMois.Text = "00";
            // 
            // lblAnnee
            // 
            this.lblAnnee.AutoSize = true;
            this.lblAnnee.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnnee.Location = new System.Drawing.Point(1250, 689);
            this.lblAnnee.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAnnee.Name = "lblAnnee";
            this.lblAnnee.Size = new System.Drawing.Size(34, 25);
            this.lblAnnee.TabIndex = 54;
            this.lblAnnee.Text = "00";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(350, 689);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(128, 25);
            this.label10.TabIndex = 55;
            this.label10.Text = "Inv. du Jour :";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(575, 689);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(181, 25);
            this.label11.TabIndex = 56;
            this.label11.Text = "Inv. de la Semaine :";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(882, 689);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(131, 25);
            this.label12.TabIndex = 57;
            this.label12.Text = "Inv. du Mois :";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(1119, 689);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(123, 25);
            this.label13.TabIndex = 58;
            this.label13.Text = "Inv. Annuel :";
            // 
            // btOuvrirScanner
            // 
            this.btOuvrirScanner.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btOuvrirScanner.ForeColor = System.Drawing.Color.Black;
            this.btOuvrirScanner.Location = new System.Drawing.Point(168, 671);
            this.btOuvrirScanner.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btOuvrirScanner.Name = "btOuvrirScanner";
            this.btOuvrirScanner.Size = new System.Drawing.Size(174, 43);
            this.btOuvrirScanner.TabIndex = 59;
            this.btOuvrirScanner.Text = "Ouvrir Scanner";
            this.btOuvrirScanner.UseVisualStyleBackColor = true;
            this.btOuvrirScanner.Click += new System.EventHandler(this.btOuvrirScanner_Click);
            // 
            // FormInventaire
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.btOuvrirScanner);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblAnnee);
            this.Controls.Add(this.lblMois);
            this.Controls.Add(this.lblSemaine);
            this.Controls.Add(this.lblJour);
            this.Controls.Add(this.btnExporterPDF);
            this.Controls.Add(this.dgvInventaire);
            this.Controls.Add(this.lblQuantiteRestante);
            this.Controls.Add(this.btDetails);
            this.Controls.Add(this.btSupprimer);
            this.Controls.Add(this.btAnnuler);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.btChangerImage);
            this.Controls.Add(this.picProduit);
            this.Controls.Add(this.txtFournisseur);
            this.Controls.Add(this.dtpDateAjout);
            this.Controls.Add(this.rtbDescription);
            this.Controls.Add(this.txtPrixUnitaire);
            this.Controls.Add(this.numQuantite);
            this.Controls.Add(this.cmbCategorie);
            this.Controls.Add(this.txtReference);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.txtNomProduit);
            this.Controls.Add(this.Label1);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormInventaire";
            this.Text = "FormInventaire";
            this.Load += new System.EventHandler(this.FormInventaire_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picProduit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQuantite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventaire)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btDetails;
        internal System.Windows.Forms.Button btSupprimer;
        internal System.Windows.Forms.Button btAnnuler;
        internal System.Windows.Forms.Button btOk;
        internal System.Windows.Forms.Button btChangerImage;
        internal System.Windows.Forms.PictureBox picProduit;
        internal System.Windows.Forms.TextBox txtFournisseur;
        internal System.Windows.Forms.DateTimePicker dtpDateAjout;
        internal System.Windows.Forms.RichTextBox rtbDescription;
        internal System.Windows.Forms.TextBox txtPrixUnitaire;
        internal System.Windows.Forms.NumericUpDown numQuantite;
        internal System.Windows.Forms.ComboBox cmbCategorie;
        internal System.Windows.Forms.TextBox txtReference;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.TextBox txtNomProduit;
        internal System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label lblQuantiteRestante;
        private System.Windows.Forms.DataGridView dgvInventaire;
        internal System.Windows.Forms.Button btnExporterPDF;
        internal System.Windows.Forms.Label lblJour;
        internal System.Windows.Forms.Label lblSemaine;
        internal System.Windows.Forms.Label lblMois;
        internal System.Windows.Forms.Label lblAnnee;
        internal System.Windows.Forms.Label label10;
        internal System.Windows.Forms.Label label11;
        internal System.Windows.Forms.Label label12;
        internal System.Windows.Forms.Label label13;
        internal System.Windows.Forms.Button btOuvrirScanner;
    }
}