namespace BoutiqueRebuildFixed
{
    partial class FrmClotureJournaliere
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
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.lblCaissier = new System.Windows.Forms.Label();
            this.txtCaissier = new System.Windows.Forms.TextBox();
            this.grpFC = new System.Windows.Forms.GroupBox();
            this.txtBalanceFC = new System.Windows.Forms.TextBox();
            this.txtVenteFC = new System.Windows.Forms.TextBox();
            this.txtPhotoFC = new System.Windows.Forms.TextBox();
            this.txtSortiesFC = new System.Windows.Forms.TextBox();
            this.txtEntreesFC = new System.Windows.Forms.TextBox();
            this.lblBalanceFC = new System.Windows.Forms.Label();
            this.lblVenteFC = new System.Windows.Forms.Label();
            this.lblPhotoFC = new System.Windows.Forms.Label();
            this.lblSortiesFC = new System.Windows.Forms.Label();
            this.lblEntreesFC = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBalanceUSD = new System.Windows.Forms.TextBox();
            this.txtVenteUSD = new System.Windows.Forms.TextBox();
            this.txtPhotoUSD = new System.Windows.Forms.TextBox();
            this.txtSortiesUSD = new System.Windows.Forms.TextBox();
            this.txtEntreesUSD = new System.Windows.Forms.TextBox();
            this.lblBalanceUSD = new System.Windows.Forms.Label();
            this.lblVenteUSD = new System.Windows.Forms.Label();
            this.lblPhotoUSD = new System.Windows.Forms.Label();
            this.lblSortiesUSD = new System.Windows.Forms.Label();
            this.lblEntreesUSD = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtObservation = new System.Windows.Forms.TextBox();
            this.BtnValiderCloture = new System.Windows.Forms.Button();
            this.BtnAnnuler = new System.Windows.Forms.Button();
            this.BtnExporterPDF = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtEntreesFC_Semaine = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSortiesFC_Semaine = new System.Windows.Forms.TextBox();
            this.txtEntreesUSD_Semaine = new System.Windows.Forms.TextBox();
            this.txtSortiesUSD_Semaine = new System.Windows.Forms.TextBox();
            this.txtBalanceFC_Semaine = new System.Windows.Forms.TextBox();
            this.txtBalanceUSD_Semaine = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtEntreesFC_Mois = new System.Windows.Forms.TextBox();
            this.txtEntreesUSD_Mois = new System.Windows.Forms.TextBox();
            this.txtSortiesFC_Mois = new System.Windows.Forms.TextBox();
            this.txtSortiesUSD_Mois = new System.Windows.Forms.TextBox();
            this.txtBalanceFC_Mois = new System.Windows.Forms.TextBox();
            this.txtBalanceUSD_Mois = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.grpFC.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblTitre.Font = new System.Drawing.Font("Rockwell Extra Bold", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblTitre.Location = new System.Drawing.Point(261, 9);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(840, 40);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "CLOTURE JOURNALIÈRE DE CAISSE";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Location = new System.Drawing.Point(304, 70);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(49, 21);
            this.lblDate.TabIndex = 1;
            this.lblDate.Text = "Date :";
            // 
            // txtDate
            // 
            this.txtDate.Location = new System.Drawing.Point(380, 67);
            this.txtDate.Name = "txtDate";
            this.txtDate.ReadOnly = true;
            this.txtDate.Size = new System.Drawing.Size(200, 29);
            this.txtDate.TabIndex = 2;
            // 
            // lblCaissier
            // 
            this.lblCaissier.AutoSize = true;
            this.lblCaissier.Location = new System.Drawing.Point(878, 65);
            this.lblCaissier.Name = "lblCaissier";
            this.lblCaissier.Size = new System.Drawing.Size(75, 21);
            this.lblCaissier.TabIndex = 3;
            this.lblCaissier.Text = "Caissier : ";
            // 
            // txtCaissier
            // 
            this.txtCaissier.Location = new System.Drawing.Point(982, 59);
            this.txtCaissier.Name = "txtCaissier";
            this.txtCaissier.ReadOnly = true;
            this.txtCaissier.Size = new System.Drawing.Size(250, 29);
            this.txtCaissier.TabIndex = 4;
            // 
            // grpFC
            // 
            this.grpFC.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.grpFC.Controls.Add(this.txtBalanceFC);
            this.grpFC.Controls.Add(this.txtVenteFC);
            this.grpFC.Controls.Add(this.txtPhotoFC);
            this.grpFC.Controls.Add(this.txtSortiesFC);
            this.grpFC.Controls.Add(this.txtEntreesFC);
            this.grpFC.Controls.Add(this.lblBalanceFC);
            this.grpFC.Controls.Add(this.lblVenteFC);
            this.grpFC.Controls.Add(this.lblPhotoFC);
            this.grpFC.Controls.Add(this.lblSortiesFC);
            this.grpFC.Controls.Add(this.lblEntreesFC);
            this.grpFC.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpFC.Location = new System.Drawing.Point(69, 102);
            this.grpFC.Name = "grpFC";
            this.grpFC.Size = new System.Drawing.Size(511, 243);
            this.grpFC.TabIndex = 5;
            this.grpFC.TabStop = false;
            this.grpFC.Text = "RÉCAPITULATIF FC";
            // 
            // txtBalanceFC
            // 
            this.txtBalanceFC.BackColor = System.Drawing.Color.White;
            this.txtBalanceFC.Location = new System.Drawing.Point(191, 189);
            this.txtBalanceFC.Name = "txtBalanceFC";
            this.txtBalanceFC.ReadOnly = true;
            this.txtBalanceFC.Size = new System.Drawing.Size(289, 33);
            this.txtBalanceFC.TabIndex = 9;
            this.txtBalanceFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtVenteFC
            // 
            this.txtVenteFC.BackColor = System.Drawing.Color.White;
            this.txtVenteFC.Location = new System.Drawing.Point(191, 150);
            this.txtVenteFC.Name = "txtVenteFC";
            this.txtVenteFC.ReadOnly = true;
            this.txtVenteFC.Size = new System.Drawing.Size(289, 33);
            this.txtVenteFC.TabIndex = 8;
            this.txtVenteFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtPhotoFC
            // 
            this.txtPhotoFC.BackColor = System.Drawing.Color.White;
            this.txtPhotoFC.Location = new System.Drawing.Point(191, 108);
            this.txtPhotoFC.Name = "txtPhotoFC";
            this.txtPhotoFC.ReadOnly = true;
            this.txtPhotoFC.Size = new System.Drawing.Size(289, 33);
            this.txtPhotoFC.TabIndex = 7;
            this.txtPhotoFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSortiesFC
            // 
            this.txtSortiesFC.BackColor = System.Drawing.Color.White;
            this.txtSortiesFC.Location = new System.Drawing.Point(191, 69);
            this.txtSortiesFC.Name = "txtSortiesFC";
            this.txtSortiesFC.ReadOnly = true;
            this.txtSortiesFC.Size = new System.Drawing.Size(289, 33);
            this.txtSortiesFC.TabIndex = 6;
            this.txtSortiesFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtEntreesFC
            // 
            this.txtEntreesFC.BackColor = System.Drawing.Color.White;
            this.txtEntreesFC.Location = new System.Drawing.Point(191, 30);
            this.txtEntreesFC.Name = "txtEntreesFC";
            this.txtEntreesFC.ReadOnly = true;
            this.txtEntreesFC.Size = new System.Drawing.Size(289, 33);
            this.txtEntreesFC.TabIndex = 5;
            this.txtEntreesFC.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblBalanceFC
            // 
            this.lblBalanceFC.AutoSize = true;
            this.lblBalanceFC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalanceFC.Location = new System.Drawing.Point(10, 192);
            this.lblBalanceFC.Name = "lblBalanceFC";
            this.lblBalanceFC.Size = new System.Drawing.Size(107, 25);
            this.lblBalanceFC.TabIndex = 4;
            this.lblBalanceFC.Text = "Balance FC";
            // 
            // lblVenteFC
            // 
            this.lblVenteFC.AutoSize = true;
            this.lblVenteFC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVenteFC.Location = new System.Drawing.Point(10, 150);
            this.lblVenteFC.Name = "lblVenteFC";
            this.lblVenteFC.Size = new System.Drawing.Size(148, 25);
            this.lblVenteFC.TabIndex = 3;
            this.lblVenteFC.Text = "Argent Vente FC";
            // 
            // lblPhotoFC
            // 
            this.lblPhotoFC.AutoSize = true;
            this.lblPhotoFC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPhotoFC.Location = new System.Drawing.Point(10, 111);
            this.lblPhotoFC.Name = "lblPhotoFC";
            this.lblPhotoFC.Size = new System.Drawing.Size(150, 25);
            this.lblPhotoFC.TabIndex = 2;
            this.lblPhotoFC.Text = "Argent Photo FC";
            // 
            // lblSortiesFC
            // 
            this.lblSortiesFC.AutoSize = true;
            this.lblSortiesFC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSortiesFC.Location = new System.Drawing.Point(10, 72);
            this.lblSortiesFC.Name = "lblSortiesFC";
            this.lblSortiesFC.Size = new System.Drawing.Size(95, 25);
            this.lblSortiesFC.TabIndex = 1;
            this.lblSortiesFC.Text = "Sorties FC";
            // 
            // lblEntreesFC
            // 
            this.lblEntreesFC.AutoSize = true;
            this.lblEntreesFC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEntreesFC.Location = new System.Drawing.Point(6, 38);
            this.lblEntreesFC.Name = "lblEntreesFC";
            this.lblEntreesFC.Size = new System.Drawing.Size(100, 25);
            this.lblEntreesFC.TabIndex = 0;
            this.lblEntreesFC.Text = "Entrées FC";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.groupBox1.Controls.Add(this.txtBalanceUSD);
            this.groupBox1.Controls.Add(this.txtVenteUSD);
            this.groupBox1.Controls.Add(this.txtPhotoUSD);
            this.groupBox1.Controls.Add(this.txtSortiesUSD);
            this.groupBox1.Controls.Add(this.txtEntreesUSD);
            this.groupBox1.Controls.Add(this.lblBalanceUSD);
            this.groupBox1.Controls.Add(this.lblVenteUSD);
            this.groupBox1.Controls.Add(this.lblPhotoUSD);
            this.groupBox1.Controls.Add(this.lblSortiesUSD);
            this.groupBox1.Controls.Add(this.lblEntreesUSD);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(804, 102);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(428, 243);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "RÉCAPITULATIF USD";
            // 
            // txtBalanceUSD
            // 
            this.txtBalanceUSD.BackColor = System.Drawing.Color.White;
            this.txtBalanceUSD.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.txtBalanceUSD.Location = new System.Drawing.Point(191, 192);
            this.txtBalanceUSD.Name = "txtBalanceUSD";
            this.txtBalanceUSD.ReadOnly = true;
            this.txtBalanceUSD.Size = new System.Drawing.Size(231, 33);
            this.txtBalanceUSD.TabIndex = 9;
            this.txtBalanceUSD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtVenteUSD
            // 
            this.txtVenteUSD.BackColor = System.Drawing.Color.White;
            this.txtVenteUSD.Location = new System.Drawing.Point(191, 147);
            this.txtVenteUSD.Name = "txtVenteUSD";
            this.txtVenteUSD.ReadOnly = true;
            this.txtVenteUSD.Size = new System.Drawing.Size(231, 33);
            this.txtVenteUSD.TabIndex = 8;
            this.txtVenteUSD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtPhotoUSD
            // 
            this.txtPhotoUSD.BackColor = System.Drawing.Color.White;
            this.txtPhotoUSD.Location = new System.Drawing.Point(191, 108);
            this.txtPhotoUSD.Name = "txtPhotoUSD";
            this.txtPhotoUSD.ReadOnly = true;
            this.txtPhotoUSD.Size = new System.Drawing.Size(231, 33);
            this.txtPhotoUSD.TabIndex = 7;
            this.txtPhotoUSD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtSortiesUSD
            // 
            this.txtSortiesUSD.BackColor = System.Drawing.Color.White;
            this.txtSortiesUSD.Location = new System.Drawing.Point(191, 69);
            this.txtSortiesUSD.Name = "txtSortiesUSD";
            this.txtSortiesUSD.ReadOnly = true;
            this.txtSortiesUSD.Size = new System.Drawing.Size(231, 33);
            this.txtSortiesUSD.TabIndex = 6;
            this.txtSortiesUSD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtEntreesUSD
            // 
            this.txtEntreesUSD.BackColor = System.Drawing.Color.White;
            this.txtEntreesUSD.Location = new System.Drawing.Point(191, 30);
            this.txtEntreesUSD.Name = "txtEntreesUSD";
            this.txtEntreesUSD.ReadOnly = true;
            this.txtEntreesUSD.Size = new System.Drawing.Size(231, 33);
            this.txtEntreesUSD.TabIndex = 5;
            this.txtEntreesUSD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblBalanceUSD
            // 
            this.lblBalanceUSD.AutoSize = true;
            this.lblBalanceUSD.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalanceUSD.Location = new System.Drawing.Point(10, 192);
            this.lblBalanceUSD.Name = "lblBalanceUSD";
            this.lblBalanceUSD.Size = new System.Drawing.Size(124, 25);
            this.lblBalanceUSD.TabIndex = 4;
            this.lblBalanceUSD.Text = "Balance USD";
            // 
            // lblVenteUSD
            // 
            this.lblVenteUSD.AutoSize = true;
            this.lblVenteUSD.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVenteUSD.Location = new System.Drawing.Point(10, 150);
            this.lblVenteUSD.Name = "lblVenteUSD";
            this.lblVenteUSD.Size = new System.Drawing.Size(163, 25);
            this.lblVenteUSD.TabIndex = 3;
            this.lblVenteUSD.Text = "Argent Vente USD";
            // 
            // lblPhotoUSD
            // 
            this.lblPhotoUSD.AutoSize = true;
            this.lblPhotoUSD.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPhotoUSD.Location = new System.Drawing.Point(10, 111);
            this.lblPhotoUSD.Name = "lblPhotoUSD";
            this.lblPhotoUSD.Size = new System.Drawing.Size(165, 25);
            this.lblPhotoUSD.TabIndex = 2;
            this.lblPhotoUSD.Text = "Argent Photo USD";
            // 
            // lblSortiesUSD
            // 
            this.lblSortiesUSD.AutoSize = true;
            this.lblSortiesUSD.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSortiesUSD.Location = new System.Drawing.Point(10, 72);
            this.lblSortiesUSD.Name = "lblSortiesUSD";
            this.lblSortiesUSD.Size = new System.Drawing.Size(110, 25);
            this.lblSortiesUSD.TabIndex = 1;
            this.lblSortiesUSD.Text = "Sorties USD";
            // 
            // lblEntreesUSD
            // 
            this.lblEntreesUSD.AutoSize = true;
            this.lblEntreesUSD.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEntreesUSD.Location = new System.Drawing.Point(6, 38);
            this.lblEntreesUSD.Name = "lblEntreesUSD";
            this.lblEntreesUSD.Size = new System.Drawing.Size(115, 25);
            this.lblEntreesUSD.TabIndex = 0;
            this.lblEntreesUSD.Text = "Entrées USD";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe Print", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(52, 512);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 33);
            this.label1.TabIndex = 7;
            this.label1.Text = "Observation :";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtObservation
            // 
            this.txtObservation.Location = new System.Drawing.Point(58, 548);
            this.txtObservation.Multiline = true;
            this.txtObservation.Name = "txtObservation";
            this.txtObservation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtObservation.Size = new System.Drawing.Size(1174, 126);
            this.txtObservation.TabIndex = 8;
            // 
            // BtnValiderCloture
            // 
            this.BtnValiderCloture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.BtnValiderCloture.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnValiderCloture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.BtnValiderCloture.Location = new System.Drawing.Point(138, 697);
            this.BtnValiderCloture.Name = "BtnValiderCloture";
            this.BtnValiderCloture.Size = new System.Drawing.Size(202, 40);
            this.BtnValiderCloture.TabIndex = 9;
            this.BtnValiderCloture.Text = "VALIDER LA CLÔTURE";
            this.BtnValiderCloture.UseVisualStyleBackColor = false;
            this.BtnValiderCloture.Click += new System.EventHandler(this.BtnValiderCloture_Click);
            // 
            // BtnAnnuler
            // 
            this.BtnAnnuler.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BtnAnnuler.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnAnnuler.ForeColor = System.Drawing.Color.Red;
            this.BtnAnnuler.Location = new System.Drawing.Point(1002, 697);
            this.BtnAnnuler.Name = "BtnAnnuler";
            this.BtnAnnuler.Size = new System.Drawing.Size(202, 40);
            this.BtnAnnuler.TabIndex = 10;
            this.BtnAnnuler.Text = "Annuler";
            this.BtnAnnuler.UseVisualStyleBackColor = false;
            this.BtnAnnuler.Click += new System.EventHandler(this.BtnAnnuler_Click);
            // 
            // BtnExporterPDF
            // 
            this.BtnExporterPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.BtnExporterPDF.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnExporterPDF.ForeColor = System.Drawing.Color.Black;
            this.BtnExporterPDF.Location = new System.Drawing.Point(583, 697);
            this.BtnExporterPDF.Name = "BtnExporterPDF";
            this.BtnExporterPDF.Size = new System.Drawing.Size(202, 40);
            this.BtnExporterPDF.TabIndex = 11;
            this.BtnExporterPDF.Text = "Exporter PDF";
            this.BtnExporterPDF.UseVisualStyleBackColor = false;
            this.BtnExporterPDF.Click += new System.EventHandler(this.BtnExporterPDF_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(64, 346);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 25);
            this.label2.TabIndex = 12;
            this.label2.Text = "E/S  SEMAINE FC : ";
            // 
            // txtEntreesFC_Semaine
            // 
            this.txtEntreesFC_Semaine.Location = new System.Drawing.Point(69, 384);
            this.txtEntreesFC_Semaine.Name = "txtEntreesFC_Semaine";
            this.txtEntreesFC_Semaine.Size = new System.Drawing.Size(184, 29);
            this.txtEntreesFC_Semaine.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(341, 346);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 25);
            this.label3.TabIndex = 14;
            this.label3.Text = "E/S  SEMAINE USD : ";
            // 
            // txtSortiesFC_Semaine
            // 
            this.txtSortiesFC_Semaine.Location = new System.Drawing.Point(69, 428);
            this.txtSortiesFC_Semaine.Name = "txtSortiesFC_Semaine";
            this.txtSortiesFC_Semaine.Size = new System.Drawing.Size(184, 29);
            this.txtSortiesFC_Semaine.TabIndex = 15;
            // 
            // txtEntreesUSD_Semaine
            // 
            this.txtEntreesUSD_Semaine.Location = new System.Drawing.Point(346, 384);
            this.txtEntreesUSD_Semaine.Name = "txtEntreesUSD_Semaine";
            this.txtEntreesUSD_Semaine.Size = new System.Drawing.Size(188, 29);
            this.txtEntreesUSD_Semaine.TabIndex = 16;
            // 
            // txtSortiesUSD_Semaine
            // 
            this.txtSortiesUSD_Semaine.Location = new System.Drawing.Point(346, 428);
            this.txtSortiesUSD_Semaine.Name = "txtSortiesUSD_Semaine";
            this.txtSortiesUSD_Semaine.Size = new System.Drawing.Size(188, 29);
            this.txtSortiesUSD_Semaine.TabIndex = 17;
            // 
            // txtBalanceFC_Semaine
            // 
            this.txtBalanceFC_Semaine.Location = new System.Drawing.Point(69, 480);
            this.txtBalanceFC_Semaine.Name = "txtBalanceFC_Semaine";
            this.txtBalanceFC_Semaine.Size = new System.Drawing.Size(184, 29);
            this.txtBalanceFC_Semaine.TabIndex = 18;
            // 
            // txtBalanceUSD_Semaine
            // 
            this.txtBalanceUSD_Semaine.Location = new System.Drawing.Point(346, 480);
            this.txtBalanceUSD_Semaine.Name = "txtBalanceUSD_Semaine";
            this.txtBalanceUSD_Semaine.Size = new System.Drawing.Size(188, 29);
            this.txtBalanceUSD_Semaine.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(799, 363);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 25);
            this.label4.TabIndex = 20;
            this.label4.Text = "E/S  MOIS FC : ";
            // 
            // txtEntreesFC_Mois
            // 
            this.txtEntreesFC_Mois.Location = new System.Drawing.Point(804, 397);
            this.txtEntreesFC_Mois.Name = "txtEntreesFC_Mois";
            this.txtEntreesFC_Mois.Size = new System.Drawing.Size(185, 29);
            this.txtEntreesFC_Mois.TabIndex = 21;
            this.txtEntreesFC_Mois.TextChanged += new System.EventHandler(this.txtEntreesFC_Mois_TextChanged);
            // 
            // txtEntreesUSD_Mois
            // 
            this.txtEntreesUSD_Mois.Location = new System.Drawing.Point(1037, 398);
            this.txtEntreesUSD_Mois.Name = "txtEntreesUSD_Mois";
            this.txtEntreesUSD_Mois.Size = new System.Drawing.Size(193, 29);
            this.txtEntreesUSD_Mois.TabIndex = 22;
            // 
            // txtSortiesFC_Mois
            // 
            this.txtSortiesFC_Mois.Location = new System.Drawing.Point(804, 444);
            this.txtSortiesFC_Mois.Name = "txtSortiesFC_Mois";
            this.txtSortiesFC_Mois.Size = new System.Drawing.Size(185, 29);
            this.txtSortiesFC_Mois.TabIndex = 23;
            this.txtSortiesFC_Mois.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtSortiesUSD_Mois
            // 
            this.txtSortiesUSD_Mois.Location = new System.Drawing.Point(1037, 444);
            this.txtSortiesUSD_Mois.Name = "txtSortiesUSD_Mois";
            this.txtSortiesUSD_Mois.Size = new System.Drawing.Size(193, 29);
            this.txtSortiesUSD_Mois.TabIndex = 25;
            // 
            // txtBalanceFC_Mois
            // 
            this.txtBalanceFC_Mois.Location = new System.Drawing.Point(804, 490);
            this.txtBalanceFC_Mois.Name = "txtBalanceFC_Mois";
            this.txtBalanceFC_Mois.Size = new System.Drawing.Size(185, 29);
            this.txtBalanceFC_Mois.TabIndex = 26;
            // 
            // txtBalanceUSD_Mois
            // 
            this.txtBalanceUSD_Mois.Location = new System.Drawing.Point(1037, 490);
            this.txtBalanceUSD_Mois.Name = "txtBalanceUSD_Mois";
            this.txtBalanceUSD_Mois.Size = new System.Drawing.Size(191, 29);
            this.txtBalanceUSD_Mois.TabIndex = 27;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(1032, 363);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(155, 25);
            this.label6.TabIndex = 28;
            this.label6.Text = "E/S  MOIS USD : ";
            // 
            // FrmClotureJournaliere
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtBalanceUSD_Mois);
            this.Controls.Add(this.txtBalanceFC_Mois);
            this.Controls.Add(this.txtSortiesUSD_Mois);
            this.Controls.Add(this.txtSortiesFC_Mois);
            this.Controls.Add(this.txtEntreesUSD_Mois);
            this.Controls.Add(this.txtEntreesFC_Mois);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtBalanceUSD_Semaine);
            this.Controls.Add(this.txtBalanceFC_Semaine);
            this.Controls.Add(this.txtSortiesUSD_Semaine);
            this.Controls.Add(this.txtEntreesUSD_Semaine);
            this.Controls.Add(this.txtSortiesFC_Semaine);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtEntreesFC_Semaine);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BtnExporterPDF);
            this.Controls.Add(this.BtnAnnuler);
            this.Controls.Add(this.BtnValiderCloture);
            this.Controls.Add(this.txtObservation);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpFC);
            this.Controls.Add(this.txtCaissier);
            this.Controls.Add(this.lblCaissier);
            this.Controls.Add(this.txtDate);
            this.Controls.Add(this.lblDate);
            this.Controls.Add(this.lblTitre);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmClotureJournaliere";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clôture Journalière de Caisse";
            this.Load += new System.EventHandler(this.FrmClotureJournaliere_Load);
            this.grpFC.ResumeLayout(false);
            this.grpFC.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.Label lblCaissier;
        private System.Windows.Forms.TextBox txtCaissier;
        private System.Windows.Forms.GroupBox grpFC;
        private System.Windows.Forms.Label lblEntreesFC;
        private System.Windows.Forms.TextBox txtVenteFC;
        private System.Windows.Forms.TextBox txtPhotoFC;
        private System.Windows.Forms.TextBox txtSortiesFC;
        private System.Windows.Forms.TextBox txtEntreesFC;
        private System.Windows.Forms.Label lblBalanceFC;
        private System.Windows.Forms.Label lblVenteFC;
        private System.Windows.Forms.Label lblPhotoFC;
        private System.Windows.Forms.Label lblSortiesFC;
        private System.Windows.Forms.TextBox txtBalanceFC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtBalanceUSD;
        private System.Windows.Forms.TextBox txtVenteUSD;
        private System.Windows.Forms.TextBox txtPhotoUSD;
        private System.Windows.Forms.TextBox txtSortiesUSD;
        private System.Windows.Forms.TextBox txtEntreesUSD;
        private System.Windows.Forms.Label lblBalanceUSD;
        private System.Windows.Forms.Label lblVenteUSD;
        private System.Windows.Forms.Label lblPhotoUSD;
        private System.Windows.Forms.Label lblSortiesUSD;
        private System.Windows.Forms.Label lblEntreesUSD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtObservation;
        private System.Windows.Forms.Button BtnValiderCloture;
        private System.Windows.Forms.Button BtnAnnuler;
        private System.Windows.Forms.Button BtnExporterPDF;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtEntreesFC_Semaine;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSortiesFC_Semaine;
        private System.Windows.Forms.TextBox txtEntreesUSD_Semaine;
        private System.Windows.Forms.TextBox txtSortiesUSD_Semaine;
        private System.Windows.Forms.TextBox txtBalanceFC_Semaine;
        private System.Windows.Forms.TextBox txtBalanceUSD_Semaine;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtEntreesFC_Mois;
        private System.Windows.Forms.TextBox txtEntreesUSD_Mois;
        private System.Windows.Forms.TextBox txtSortiesFC_Mois;
        private System.Windows.Forms.TextBox txtSortiesUSD_Mois;
        private System.Windows.Forms.TextBox txtBalanceFC_Mois;
        private System.Windows.Forms.TextBox txtBalanceUSD_Mois;
        private System.Windows.Forms.Label label6;
    }
}