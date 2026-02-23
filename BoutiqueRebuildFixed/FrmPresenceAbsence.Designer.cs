namespace BoutiqueRebuildFixed
{
    partial class FrmPresenceAbsence
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblNomPrenom = new System.Windows.Forms.Label();
            this.txtNomPrenom = new System.Windows.Forms.TextBox();
            this.lblSexe = new System.Windows.Forms.Label();
            this.cmbSexe = new System.Windows.Forms.ComboBox();
            this.lblDateJour = new System.Windows.Forms.Label();
            this.dtpJourDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpHeureEntree = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpHeureSortie = new System.Windows.Forms.DateTimePicker();
            this.chkAbsent = new System.Windows.Forms.CheckBox();
            this.chkPresent = new System.Windows.Forms.CheckBox();
            this.chkRetard = new System.Windows.Forms.CheckBox();
            this.chkRepos = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtObservations = new System.Windows.Forms.TextBox();
            this.btnEnregistrer = new System.Windows.Forms.Button();
            this.btnExporterPDF = new System.Windows.Forms.Button();
            this.btnCalculerTotals = new System.Windows.Forms.Button();
            this.dgvPresenceAbsence = new System.Windows.Forms.DataGridView();
            this.lblTotalPresent = new System.Windows.Forms.Label();
            this.lblTotalAbsent = new System.Windows.Forms.Label();
            this.lblTotalRetard = new System.Windows.Forms.Label();
            this.lblTotalRepos = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbEmploye = new System.Windows.Forms.ComboBox();
            this.btnValider = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPresenceAbsence)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label1.Font = new System.Drawing.Font("Impact", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(371, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(556, 36);
            this.label1.TabIndex = 0;
            this.label1.Text = "LISTE DES PRESENCES ET ABSENCES PERSONNELS";
            // 
            // lblNomPrenom
            // 
            this.lblNomPrenom.AutoSize = true;
            this.lblNomPrenom.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomPrenom.Location = new System.Drawing.Point(26, 80);
            this.lblNomPrenom.Name = "lblNomPrenom";
            this.lblNomPrenom.Size = new System.Drawing.Size(154, 25);
            this.lblNomPrenom.TabIndex = 1;
            this.lblNomPrenom.Text = "Nom et Prenom :";
            // 
            // txtNomPrenom
            // 
            this.txtNomPrenom.Location = new System.Drawing.Point(205, 80);
            this.txtNomPrenom.Name = "txtNomPrenom";
            this.txtNomPrenom.Size = new System.Drawing.Size(602, 27);
            this.txtNomPrenom.TabIndex = 3;
            // 
            // lblSexe
            // 
            this.lblSexe.AutoSize = true;
            this.lblSexe.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSexe.Location = new System.Drawing.Point(813, 83);
            this.lblSexe.Name = "lblSexe";
            this.lblSexe.Size = new System.Drawing.Size(60, 25);
            this.lblSexe.TabIndex = 4;
            this.lblSexe.Text = "Sexe :";
            // 
            // cmbSexe
            // 
            this.cmbSexe.FormattingEnabled = true;
            this.cmbSexe.Items.AddRange(new object[] {
            "\"M\", \"F\""});
            this.cmbSexe.Location = new System.Drawing.Point(879, 80);
            this.cmbSexe.Name = "cmbSexe";
            this.cmbSexe.Size = new System.Drawing.Size(135, 28);
            this.cmbSexe.TabIndex = 5;
            this.cmbSexe.SelectedIndexChanged += new System.EventHandler(this.cmbSexe_SelectedIndexChanged);
            // 
            // lblDateJour
            // 
            this.lblDateJour.AutoSize = true;
            this.lblDateJour.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateJour.Location = new System.Drawing.Point(26, 117);
            this.lblDateJour.Name = "lblDateJour";
            this.lblDateJour.Size = new System.Drawing.Size(122, 25);
            this.lblDateJour.TabIndex = 6;
            this.lblDateJour.Text = "Jour et Date :";
            this.lblDateJour.Click += new System.EventHandler(this.lblDateJour_Click);
            // 
            // dtpJourDate
            // 
            this.dtpJourDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpJourDate.Location = new System.Drawing.Point(205, 116);
            this.dtpJourDate.Name = "dtpJourDate";
            this.dtpJourDate.Size = new System.Drawing.Size(147, 27);
            this.dtpJourDate.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(373, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 25);
            this.label2.TabIndex = 8;
            this.label2.Text = "Heure d\'entrée :";
            // 
            // dtpHeureEntree
            // 
            this.dtpHeureEntree.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpHeureEntree.Location = new System.Drawing.Point(525, 117);
            this.dtpHeureEntree.Name = "dtpHeureEntree";
            this.dtpHeureEntree.ShowUpDown = true;
            this.dtpHeureEntree.Size = new System.Drawing.Size(162, 27);
            this.dtpHeureEntree.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(710, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 25);
            this.label3.TabIndex = 10;
            this.label3.Text = "Heure de Sortie :";
            // 
            // dtpHeureSortie
            // 
            this.dtpHeureSortie.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpHeureSortie.Location = new System.Drawing.Point(879, 117);
            this.dtpHeureSortie.Name = "dtpHeureSortie";
            this.dtpHeureSortie.ShowUpDown = true;
            this.dtpHeureSortie.Size = new System.Drawing.Size(135, 27);
            this.dtpHeureSortie.TabIndex = 11;
            // 
            // chkAbsent
            // 
            this.chkAbsent.AutoSize = true;
            this.chkAbsent.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkAbsent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.chkAbsent.Location = new System.Drawing.Point(369, 145);
            this.chkAbsent.Name = "chkAbsent";
            this.chkAbsent.Size = new System.Drawing.Size(82, 27);
            this.chkAbsent.TabIndex = 13;
            this.chkAbsent.Text = "Absent";
            this.chkAbsent.UseVisualStyleBackColor = true;
            // 
            // chkPresent
            // 
            this.chkPresent.AutoSize = true;
            this.chkPresent.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPresent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.chkPresent.Location = new System.Drawing.Point(535, 145);
            this.chkPresent.Name = "chkPresent";
            this.chkPresent.Size = new System.Drawing.Size(86, 27);
            this.chkPresent.TabIndex = 14;
            this.chkPresent.Text = "Present";
            this.chkPresent.UseVisualStyleBackColor = true;
            // 
            // chkRetard
            // 
            this.chkRetard.AutoSize = true;
            this.chkRetard.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRetard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.chkRetard.Location = new System.Drawing.Point(715, 145);
            this.chkRetard.Name = "chkRetard";
            this.chkRetard.Size = new System.Drawing.Size(79, 27);
            this.chkRetard.TabIndex = 15;
            this.chkRetard.Text = "Retard";
            this.chkRetard.UseVisualStyleBackColor = true;
            // 
            // chkRepos
            // 
            this.chkRepos.AutoSize = true;
            this.chkRepos.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkRepos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.chkRepos.Location = new System.Drawing.Point(863, 148);
            this.chkRepos.Name = "chkRepos";
            this.chkRepos.Size = new System.Drawing.Size(75, 27);
            this.chkRepos.TabIndex = 16;
            this.chkRepos.Text = "Repos";
            this.chkRepos.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(26, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(135, 25);
            this.label4.TabIndex = 17;
            this.label4.Text = "Observations: ";
            // 
            // txtObservations
            // 
            this.txtObservations.Location = new System.Drawing.Point(31, 189);
            this.txtObservations.Multiline = true;
            this.txtObservations.Name = "txtObservations";
            this.txtObservations.Size = new System.Drawing.Size(1327, 79);
            this.txtObservations.TabIndex = 18;
            // 
            // btnEnregistrer
            // 
            this.btnEnregistrer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.btnEnregistrer.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnregistrer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnEnregistrer.Location = new System.Drawing.Point(298, 274);
            this.btnEnregistrer.Name = "btnEnregistrer";
            this.btnEnregistrer.Size = new System.Drawing.Size(126, 35);
            this.btnEnregistrer.TabIndex = 19;
            this.btnEnregistrer.Text = "Enregistrer";
            this.btnEnregistrer.UseVisualStyleBackColor = false;
            this.btnEnregistrer.Click += new System.EventHandler(this.btnEnregistrer_Click);
            // 
            // btnExporterPDF
            // 
            this.btnExporterPDF.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnExporterPDF.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExporterPDF.ForeColor = System.Drawing.Color.Black;
            this.btnExporterPDF.Location = new System.Drawing.Point(535, 274);
            this.btnExporterPDF.Name = "btnExporterPDF";
            this.btnExporterPDF.Size = new System.Drawing.Size(177, 35);
            this.btnExporterPDF.TabIndex = 20;
            this.btnExporterPDF.Text = "Exporter PDF";
            this.btnExporterPDF.UseVisualStyleBackColor = false;
            this.btnExporterPDF.Click += new System.EventHandler(this.btnExporterPDF_Click);
            // 
            // btnCalculerTotals
            // 
            this.btnCalculerTotals.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCalculerTotals.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCalculerTotals.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnCalculerTotals.Location = new System.Drawing.Point(761, 274);
            this.btnCalculerTotals.Name = "btnCalculerTotals";
            this.btnCalculerTotals.Size = new System.Drawing.Size(177, 35);
            this.btnCalculerTotals.TabIndex = 21;
            this.btnCalculerTotals.Text = "Calculer Totals";
            this.btnCalculerTotals.UseVisualStyleBackColor = false;
            this.btnCalculerTotals.Click += new System.EventHandler(this.btnCalculerTotals_Click);
            // 
            // dgvPresenceAbsence
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvPresenceAbsence.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvPresenceAbsence.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvPresenceAbsence.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPresenceAbsence.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvPresenceAbsence.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvPresenceAbsence.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvPresenceAbsence.Location = new System.Drawing.Point(31, 315);
            this.dgvPresenceAbsence.Name = "dgvPresenceAbsence";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvPresenceAbsence.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvPresenceAbsence.Size = new System.Drawing.Size(1327, 390);
            this.dgvPresenceAbsence.StandardTab = true;
            this.dgvPresenceAbsence.TabIndex = 22;
            // 
            // lblTotalPresent
            // 
            this.lblTotalPresent.AutoSize = true;
            this.lblTotalPresent.Font = new System.Drawing.Font("Segoe UI", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalPresent.Location = new System.Drawing.Point(55, 708);
            this.lblTotalPresent.Name = "lblTotalPresent";
            this.lblTotalPresent.Size = new System.Drawing.Size(125, 23);
            this.lblTotalPresent.TabIndex = 23;
            this.lblTotalPresent.Text = "Total Present :";
            // 
            // lblTotalAbsent
            // 
            this.lblTotalAbsent.AutoSize = true;
            this.lblTotalAbsent.Font = new System.Drawing.Font("Segoe UI", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalAbsent.Location = new System.Drawing.Point(419, 708);
            this.lblTotalAbsent.Name = "lblTotalAbsent";
            this.lblTotalAbsent.Size = new System.Drawing.Size(120, 23);
            this.lblTotalAbsent.TabIndex = 24;
            this.lblTotalAbsent.Text = "Total Absent :";
            // 
            // lblTotalRetard
            // 
            this.lblTotalRetard.AutoSize = true;
            this.lblTotalRetard.Font = new System.Drawing.Font("Segoe UI", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalRetard.Location = new System.Drawing.Point(768, 708);
            this.lblTotalRetard.Name = "lblTotalRetard";
            this.lblTotalRetard.Size = new System.Drawing.Size(119, 23);
            this.lblTotalRetard.TabIndex = 25;
            this.lblTotalRetard.Text = "Total Retard :";
            // 
            // lblTotalRepos
            // 
            this.lblTotalRepos.AutoSize = true;
            this.lblTotalRepos.Font = new System.Drawing.Font("Segoe UI", 12.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalRepos.Location = new System.Drawing.Point(1143, 708);
            this.lblTotalRepos.Name = "lblTotalRepos";
            this.lblTotalRepos.Size = new System.Drawing.Size(113, 23);
            this.lblTotalRepos.TabIndex = 26;
            this.lblTotalRepos.Text = "Total Repos :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1020, 83);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 25);
            this.label5.TabIndex = 27;
            this.label5.Text = "Employés :";
            // 
            // cmbEmploye
            // 
            this.cmbEmploye.DisplayMember = "NomPrenom (Prenom Nom)";
            this.cmbEmploye.FormattingEnabled = true;
            this.cmbEmploye.Items.AddRange(new object[] {
            "\"M\", \"F\""});
            this.cmbEmploye.Location = new System.Drawing.Point(1121, 84);
            this.cmbEmploye.Name = "cmbEmploye";
            this.cmbEmploye.Size = new System.Drawing.Size(163, 28);
            this.cmbEmploye.TabIndex = 28;
            this.cmbEmploye.ValueMember = "ID_Employe";
            // 
            // btnValider
            // 
            this.btnValider.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnValider.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnValider.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnValider.Location = new System.Drawing.Point(975, 274);
            this.btnValider.Name = "btnValider";
            this.btnValider.Size = new System.Drawing.Size(177, 35);
            this.btnValider.TabIndex = 29;
            this.btnValider.Text = "Valider";
            this.btnValider.UseVisualStyleBackColor = false;
            this.btnValider.Click += new System.EventHandler(this.btnValider_Click);
            // 
            // FrmPresenceAbsence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.btnValider);
            this.Controls.Add(this.cmbEmploye);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblTotalRepos);
            this.Controls.Add(this.lblTotalRetard);
            this.Controls.Add(this.lblTotalAbsent);
            this.Controls.Add(this.lblTotalPresent);
            this.Controls.Add(this.dgvPresenceAbsence);
            this.Controls.Add(this.btnCalculerTotals);
            this.Controls.Add(this.btnExporterPDF);
            this.Controls.Add(this.btnEnregistrer);
            this.Controls.Add(this.txtObservations);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkRepos);
            this.Controls.Add(this.chkRetard);
            this.Controls.Add(this.chkPresent);
            this.Controls.Add(this.chkAbsent);
            this.Controls.Add(this.dtpHeureSortie);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dtpHeureEntree);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dtpJourDate);
            this.Controls.Add(this.lblDateJour);
            this.Controls.Add(this.cmbSexe);
            this.Controls.Add(this.lblSexe);
            this.Controls.Add(this.txtNomPrenom);
            this.Controls.Add(this.lblNomPrenom);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FrmPresenceAbsence";
            this.Text = "LISTE DES PRESENCES ET ABSENCES AGENTS";
            this.Load += new System.EventHandler(this.FrmPresenceAbsence_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPresenceAbsence)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblNomPrenom;
        private System.Windows.Forms.TextBox txtNomPrenom;
        private System.Windows.Forms.Label lblSexe;
        private System.Windows.Forms.ComboBox cmbSexe;
        private System.Windows.Forms.Label lblDateJour;
        private System.Windows.Forms.DateTimePicker dtpJourDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpHeureEntree;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpHeureSortie;
        private System.Windows.Forms.CheckBox chkAbsent;
        private System.Windows.Forms.CheckBox chkPresent;
        private System.Windows.Forms.CheckBox chkRetard;
        private System.Windows.Forms.CheckBox chkRepos;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtObservations;
        private System.Windows.Forms.Button btnEnregistrer;
        private System.Windows.Forms.Button btnExporterPDF;
        private System.Windows.Forms.Button btnCalculerTotals;
        private System.Windows.Forms.DataGridView dgvPresenceAbsence;
        private System.Windows.Forms.Label lblTotalPresent;
        private System.Windows.Forms.Label lblTotalAbsent;
        private System.Windows.Forms.Label lblTotalRetard;
        private System.Windows.Forms.Label lblTotalRepos;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbEmploye;
        private System.Windows.Forms.Button btnValider;
    }
}