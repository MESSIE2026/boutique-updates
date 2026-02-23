namespace BoutiqueRebuildFixed
{
    partial class FormMarketing
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblTitre = new System.Windows.Forms.Label();
            this.lblNomCampagne = new System.Windows.Forms.Label();
            this.txtNomCampagne = new System.Windows.Forms.TextBox();
            this.lblTypeCampagne = new System.Windows.Forms.Label();
            this.cmbTypeCampagne = new System.Windows.Forms.ComboBox();
            this.lblDateDebut = new System.Windows.Forms.Label();
            this.dateDebut = new System.Windows.Forms.DateTimePicker();
            this.lblDateFin = new System.Windows.Forms.Label();
            this.dateFin = new System.Windows.Forms.DateTimePicker();
            this.lblBudget = new System.Windows.Forms.Label();
            this.txtBudget = new System.Windows.Forms.TextBox();
            this.lblStatut = new System.Windows.Forms.Label();
            this.cmbStatut = new System.Windows.Forms.ComboBox();
            this.lblCommentaires = new System.Windows.Forms.Label();
            this.txtCommentaires = new System.Windows.Forms.TextBox();
            this.btnAjouter = new System.Windows.Forms.Button();
            this.btnModifier = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.dgvMarketing = new System.Windows.Forms.DataGridView();
            this.lblConversationsMessages = new System.Windows.Forms.Label();
            this.txtConversationsMessages = new System.Windows.Forms.TextBox();
            this.lblVues = new System.Windows.Forms.Label();
            this.txtVues = new System.Windows.Forms.TextBox();
            this.lblSpectateurs = new System.Windows.Forms.Label();
            this.txtSpectateurs = new System.Windows.Forms.TextBox();
            this.lblBudgetQuotidien = new System.Windows.Forms.Label();
            this.txtBudgetQuotidien = new System.Windows.Forms.TextBox();
            this.lblResultat = new System.Windows.Forms.Label();
            this.lblNombreVentes = new System.Windows.Forms.Label();
            this.txtNombreVentes = new System.Windows.Forms.TextBox();
            this.lblMontantVendus = new System.Windows.Forms.Label();
            this.txtMontantVendus = new System.Windows.Forms.TextBox();
            this.cbDevise = new System.Windows.Forms.ComboBox();
            this.lblDevise = new System.Windows.Forms.Label();
            this.btnObjectifs = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketing)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblTitre.Font = new System.Drawing.Font("Broadway", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.Location = new System.Drawing.Point(432, 9);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(523, 40);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "FORMULAIRE MARKETING";
            // 
            // lblNomCampagne
            // 
            this.lblNomCampagne.AutoSize = true;
            this.lblNomCampagne.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomCampagne.Location = new System.Drawing.Point(306, 65);
            this.lblNomCampagne.Name = "lblNomCampagne";
            this.lblNomCampagne.Size = new System.Drawing.Size(184, 30);
            this.lblNomCampagne.TabIndex = 1;
            this.lblNomCampagne.Text = "Nom Campagne :";
            // 
            // txtNomCampagne
            // 
            this.txtNomCampagne.Location = new System.Drawing.Point(496, 66);
            this.txtNomCampagne.Name = "txtNomCampagne";
            this.txtNomCampagne.Size = new System.Drawing.Size(402, 33);
            this.txtNomCampagne.TabIndex = 2;
            // 
            // lblTypeCampagne
            // 
            this.lblTypeCampagne.AutoSize = true;
            this.lblTypeCampagne.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTypeCampagne.Location = new System.Drawing.Point(301, 115);
            this.lblTypeCampagne.Name = "lblTypeCampagne";
            this.lblTypeCampagne.Size = new System.Drawing.Size(181, 30);
            this.lblTypeCampagne.TabIndex = 3;
            this.lblTypeCampagne.Text = "Type Campagne :";
            // 
            // cmbTypeCampagne
            // 
            this.cmbTypeCampagne.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypeCampagne.FormattingEnabled = true;
            this.cmbTypeCampagne.Location = new System.Drawing.Point(498, 114);
            this.cmbTypeCampagne.Name = "cmbTypeCampagne";
            this.cmbTypeCampagne.Size = new System.Drawing.Size(225, 33);
            this.cmbTypeCampagne.TabIndex = 4;
            // 
            // lblDateDebut
            // 
            this.lblDateDebut.AutoSize = true;
            this.lblDateDebut.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateDebut.Location = new System.Drawing.Point(302, 155);
            this.lblDateDebut.Name = "lblDateDebut";
            this.lblDateDebut.Size = new System.Drawing.Size(136, 30);
            this.lblDateDebut.TabIndex = 5;
            this.lblDateDebut.Text = "Date Début :";
            // 
            // dateDebut
            // 
            this.dateDebut.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateDebut.Location = new System.Drawing.Point(498, 155);
            this.dateDebut.Name = "dateDebut";
            this.dateDebut.Size = new System.Drawing.Size(225, 33);
            this.dateDebut.TabIndex = 6;
            // 
            // lblDateFin
            // 
            this.lblDateFin.AutoSize = true;
            this.lblDateFin.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateFin.Location = new System.Drawing.Point(302, 198);
            this.lblDateFin.Name = "lblDateFin";
            this.lblDateFin.Size = new System.Drawing.Size(106, 30);
            this.lblDateFin.TabIndex = 7;
            this.lblDateFin.Text = "Date Fin :";
            // 
            // dateFin
            // 
            this.dateFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateFin.Location = new System.Drawing.Point(499, 198);
            this.dateFin.Name = "dateFin";
            this.dateFin.Size = new System.Drawing.Size(225, 33);
            this.dateFin.TabIndex = 8;
            // 
            // lblBudget
            // 
            this.lblBudget.AutoSize = true;
            this.lblBudget.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBudget.Location = new System.Drawing.Point(300, 239);
            this.lblBudget.Name = "lblBudget";
            this.lblBudget.Size = new System.Drawing.Size(160, 30);
            this.lblBudget.TabIndex = 9;
            this.lblBudget.Text = "Budget (USD) :";
            // 
            // txtBudget
            // 
            this.txtBudget.Location = new System.Drawing.Point(499, 240);
            this.txtBudget.Name = "txtBudget";
            this.txtBudget.Size = new System.Drawing.Size(225, 33);
            this.txtBudget.TabIndex = 10;
            // 
            // lblStatut
            // 
            this.lblStatut.AutoSize = true;
            this.lblStatut.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatut.Location = new System.Drawing.Point(300, 282);
            this.lblStatut.Name = "lblStatut";
            this.lblStatut.Size = new System.Drawing.Size(84, 30);
            this.lblStatut.TabIndex = 11;
            this.lblStatut.Text = "Statut :";
            // 
            // cmbStatut
            // 
            this.cmbStatut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatut.FormattingEnabled = true;
            this.cmbStatut.Location = new System.Drawing.Point(498, 283);
            this.cmbStatut.Name = "cmbStatut";
            this.cmbStatut.Size = new System.Drawing.Size(225, 33);
            this.cmbStatut.TabIndex = 12;
            // 
            // lblCommentaires
            // 
            this.lblCommentaires.AutoSize = true;
            this.lblCommentaires.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCommentaires.Location = new System.Drawing.Point(294, 322);
            this.lblCommentaires.Name = "lblCommentaires";
            this.lblCommentaires.Size = new System.Drawing.Size(166, 30);
            this.lblCommentaires.TabIndex = 13;
            this.lblCommentaires.Text = "Commentaires :";
            // 
            // txtCommentaires
            // 
            this.txtCommentaires.Location = new System.Drawing.Point(453, 354);
            this.txtCommentaires.Multiline = true;
            this.txtCommentaires.Name = "txtCommentaires";
            this.txtCommentaires.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCommentaires.Size = new System.Drawing.Size(905, 55);
            this.txtCommentaires.TabIndex = 14;
            // 
            // btnAjouter
            // 
            this.btnAjouter.BackColor = System.Drawing.Color.Blue;
            this.btnAjouter.ForeColor = System.Drawing.Color.White;
            this.btnAjouter.Location = new System.Drawing.Point(903, 415);
            this.btnAjouter.Name = "btnAjouter";
            this.btnAjouter.Size = new System.Drawing.Size(109, 34);
            this.btnAjouter.TabIndex = 15;
            this.btnAjouter.Text = "Ajouter";
            this.btnAjouter.UseVisualStyleBackColor = false;
            this.btnAjouter.Click += new System.EventHandler(this.btnAjouter_Click);
            // 
            // btnModifier
            // 
            this.btnModifier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnModifier.ForeColor = System.Drawing.Color.White;
            this.btnModifier.Location = new System.Drawing.Point(1017, 415);
            this.btnModifier.Name = "btnModifier";
            this.btnModifier.Size = new System.Drawing.Size(109, 34);
            this.btnModifier.TabIndex = 16;
            this.btnModifier.Text = "Modifier";
            this.btnModifier.UseVisualStyleBackColor = false;
            this.btnModifier.Click += new System.EventHandler(this.btnModifier_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSupprimer.ForeColor = System.Drawing.Color.White;
            this.btnSupprimer.Location = new System.Drawing.Point(1132, 415);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(109, 34);
            this.btnSupprimer.TabIndex = 17;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = false;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.BackColor = System.Drawing.Color.Red;
            this.btnAnnuler.ForeColor = System.Drawing.Color.White;
            this.btnAnnuler.Location = new System.Drawing.Point(1247, 415);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(109, 34);
            this.btnAnnuler.TabIndex = 18;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = false;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // dgvMarketing
            // 
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvMarketing.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle11;
            this.dgvMarketing.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMarketing.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.dgvMarketing.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle13.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvMarketing.DefaultCellStyle = dataGridViewCellStyle13;
            this.dgvMarketing.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvMarketing.Location = new System.Drawing.Point(0, 455);
            this.dgvMarketing.Name = "dgvMarketing";
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle14.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvMarketing.RowHeadersDefaultCellStyle = dataGridViewCellStyle14;
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvMarketing.RowsDefaultCellStyle = dataGridViewCellStyle15;
            this.dgvMarketing.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvMarketing.Size = new System.Drawing.Size(1370, 294);
            this.dgvMarketing.TabIndex = 19;
            // 
            // lblConversationsMessages
            // 
            this.lblConversationsMessages.AutoSize = true;
            this.lblConversationsMessages.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConversationsMessages.Location = new System.Drawing.Point(861, 100);
            this.lblConversationsMessages.Name = "lblConversationsMessages";
            this.lblConversationsMessages.Size = new System.Drawing.Size(265, 30);
            this.lblConversationsMessages.TabIndex = 20;
            this.lblConversationsMessages.Text = "Conversations/Messages :";
            // 
            // txtConversationsMessages
            // 
            this.txtConversationsMessages.Location = new System.Drawing.Point(1132, 101);
            this.txtConversationsMessages.Name = "txtConversationsMessages";
            this.txtConversationsMessages.Size = new System.Drawing.Size(225, 33);
            this.txtConversationsMessages.TabIndex = 21;
            // 
            // lblVues
            // 
            this.lblVues.AutoSize = true;
            this.lblVues.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVues.Location = new System.Drawing.Point(861, 136);
            this.lblVues.Name = "lblVues";
            this.lblVues.Size = new System.Drawing.Size(71, 30);
            this.lblVues.TabIndex = 22;
            this.lblVues.Text = "Vues :";
            // 
            // txtVues
            // 
            this.txtVues.Location = new System.Drawing.Point(1132, 138);
            this.txtVues.Name = "txtVues";
            this.txtVues.Size = new System.Drawing.Size(225, 33);
            this.txtVues.TabIndex = 23;
            // 
            // lblSpectateurs
            // 
            this.lblSpectateurs.AutoSize = true;
            this.lblSpectateurs.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpectateurs.Location = new System.Drawing.Point(861, 176);
            this.lblSpectateurs.Name = "lblSpectateurs";
            this.lblSpectateurs.Size = new System.Drawing.Size(139, 30);
            this.lblSpectateurs.TabIndex = 24;
            this.lblSpectateurs.Text = "Spectateurs :";
            // 
            // txtSpectateurs
            // 
            this.txtSpectateurs.Location = new System.Drawing.Point(1132, 175);
            this.txtSpectateurs.Name = "txtSpectateurs";
            this.txtSpectateurs.Size = new System.Drawing.Size(225, 33);
            this.txtSpectateurs.TabIndex = 25;
            // 
            // lblBudgetQuotidien
            // 
            this.lblBudgetQuotidien.AutoSize = true;
            this.lblBudgetQuotidien.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBudgetQuotidien.Location = new System.Drawing.Point(861, 214);
            this.lblBudgetQuotidien.Name = "lblBudgetQuotidien";
            this.lblBudgetQuotidien.Size = new System.Drawing.Size(201, 30);
            this.lblBudgetQuotidien.TabIndex = 26;
            this.lblBudgetQuotidien.Text = "Budget Quotidien :";
            // 
            // txtBudgetQuotidien
            // 
            this.txtBudgetQuotidien.Location = new System.Drawing.Point(1132, 213);
            this.txtBudgetQuotidien.Name = "txtBudgetQuotidien";
            this.txtBudgetQuotidien.Size = new System.Drawing.Size(225, 33);
            this.txtBudgetQuotidien.TabIndex = 27;
            // 
            // lblResultat
            // 
            this.lblResultat.AutoSize = true;
            this.lblResultat.BackColor = System.Drawing.Color.Red;
            this.lblResultat.Font = new System.Drawing.Font("Segoe Print", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultat.Location = new System.Drawing.Point(1006, 51);
            this.lblResultat.Name = "lblResultat";
            this.lblResultat.Size = new System.Drawing.Size(236, 37);
            this.lblResultat.TabIndex = 28;
            this.lblResultat.Text = "Résultat Campagne :";
            // 
            // lblNombreVentes
            // 
            this.lblNombreVentes.AutoSize = true;
            this.lblNombreVentes.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreVentes.Location = new System.Drawing.Point(861, 249);
            this.lblNombreVentes.Name = "lblNombreVentes";
            this.lblNombreVentes.Size = new System.Drawing.Size(177, 30);
            this.lblNombreVentes.TabIndex = 29;
            this.lblNombreVentes.Text = "Nombre Ventes :";
            // 
            // txtNombreVentes
            // 
            this.txtNombreVentes.Location = new System.Drawing.Point(1132, 251);
            this.txtNombreVentes.Name = "txtNombreVentes";
            this.txtNombreVentes.Size = new System.Drawing.Size(225, 33);
            this.txtNombreVentes.TabIndex = 30;
            // 
            // lblMontantVendus
            // 
            this.lblMontantVendus.AutoSize = true;
            this.lblMontantVendus.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMontantVendus.Location = new System.Drawing.Point(861, 290);
            this.lblMontantVendus.Name = "lblMontantVendus";
            this.lblMontantVendus.Size = new System.Drawing.Size(189, 30);
            this.lblMontantVendus.TabIndex = 31;
            this.lblMontantVendus.Text = "Montant Vendus :";
            // 
            // txtMontantVendus
            // 
            this.txtMontantVendus.Location = new System.Drawing.Point(1132, 288);
            this.txtMontantVendus.Name = "txtMontantVendus";
            this.txtMontantVendus.Size = new System.Drawing.Size(225, 33);
            this.txtMontantVendus.TabIndex = 32;
            // 
            // cbDevise
            // 
            this.cbDevise.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevise.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDevise.FormattingEnabled = true;
            this.cbDevise.Location = new System.Drawing.Point(1132, 324);
            this.cbDevise.Name = "cbDevise";
            this.cbDevise.Size = new System.Drawing.Size(225, 28);
            this.cbDevise.TabIndex = 33;
            // 
            // lblDevise
            // 
            this.lblDevise.AutoSize = true;
            this.lblDevise.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDevise.Location = new System.Drawing.Point(861, 319);
            this.lblDevise.Name = "lblDevise";
            this.lblDevise.Size = new System.Drawing.Size(88, 30);
            this.lblDevise.TabIndex = 34;
            this.lblDevise.Text = "Devise :";
            // 
            // btnObjectifs
            // 
            this.btnObjectifs.BackColor = System.Drawing.Color.Green;
            this.btnObjectifs.ForeColor = System.Drawing.Color.White;
            this.btnObjectifs.Location = new System.Drawing.Point(453, 415);
            this.btnObjectifs.Name = "btnObjectifs";
            this.btnObjectifs.Size = new System.Drawing.Size(213, 34);
            this.btnObjectifs.TabIndex = 35;
            this.btnObjectifs.Text = "Objectifs Marketings";
            this.btnObjectifs.UseVisualStyleBackColor = false;
            this.btnObjectifs.Click += new System.EventHandler(this.btnObjectifs_Click);
            // 
            // FormMarketing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.btnObjectifs);
            this.Controls.Add(this.lblDevise);
            this.Controls.Add(this.cbDevise);
            this.Controls.Add(this.txtMontantVendus);
            this.Controls.Add(this.lblMontantVendus);
            this.Controls.Add(this.txtNombreVentes);
            this.Controls.Add(this.lblNombreVentes);
            this.Controls.Add(this.lblResultat);
            this.Controls.Add(this.txtBudgetQuotidien);
            this.Controls.Add(this.lblBudgetQuotidien);
            this.Controls.Add(this.txtSpectateurs);
            this.Controls.Add(this.lblSpectateurs);
            this.Controls.Add(this.txtVues);
            this.Controls.Add(this.lblVues);
            this.Controls.Add(this.txtConversationsMessages);
            this.Controls.Add(this.lblConversationsMessages);
            this.Controls.Add(this.dgvMarketing);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnModifier);
            this.Controls.Add(this.btnAjouter);
            this.Controls.Add(this.txtCommentaires);
            this.Controls.Add(this.lblCommentaires);
            this.Controls.Add(this.cmbStatut);
            this.Controls.Add(this.lblStatut);
            this.Controls.Add(this.txtBudget);
            this.Controls.Add(this.lblBudget);
            this.Controls.Add(this.dateFin);
            this.Controls.Add(this.lblDateFin);
            this.Controls.Add(this.dateDebut);
            this.Controls.Add(this.lblDateDebut);
            this.Controls.Add(this.cmbTypeCampagne);
            this.Controls.Add(this.lblTypeCampagne);
            this.Controls.Add(this.txtNomCampagne);
            this.Controls.Add(this.lblNomCampagne);
            this.Controls.Add(this.lblTitre);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FormMarketing";
            this.Text = "Marketing";
            this.Load += new System.EventHandler(this.FormMarketing_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketing)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Label lblNomCampagne;
        private System.Windows.Forms.TextBox txtNomCampagne;
        private System.Windows.Forms.Label lblTypeCampagne;
        private System.Windows.Forms.ComboBox cmbTypeCampagne;
        private System.Windows.Forms.Label lblDateDebut;
        private System.Windows.Forms.DateTimePicker dateDebut;
        private System.Windows.Forms.Label lblDateFin;
        private System.Windows.Forms.DateTimePicker dateFin;
        private System.Windows.Forms.Label lblBudget;
        private System.Windows.Forms.TextBox txtBudget;
        private System.Windows.Forms.Label lblStatut;
        private System.Windows.Forms.ComboBox cmbStatut;
        private System.Windows.Forms.Label lblCommentaires;
        private System.Windows.Forms.TextBox txtCommentaires;
        private System.Windows.Forms.Button btnAjouter;
        private System.Windows.Forms.Button btnModifier;
        private System.Windows.Forms.Button btnSupprimer;
        private System.Windows.Forms.Button btnAnnuler;
        private System.Windows.Forms.DataGridView dgvMarketing;
        private System.Windows.Forms.Label lblConversationsMessages;
        private System.Windows.Forms.TextBox txtConversationsMessages;
        private System.Windows.Forms.Label lblVues;
        private System.Windows.Forms.TextBox txtVues;
        private System.Windows.Forms.Label lblSpectateurs;
        private System.Windows.Forms.TextBox txtSpectateurs;
        private System.Windows.Forms.Label lblBudgetQuotidien;
        private System.Windows.Forms.TextBox txtBudgetQuotidien;
        private System.Windows.Forms.Label lblResultat;
        private System.Windows.Forms.Label lblNombreVentes;
        private System.Windows.Forms.TextBox txtNombreVentes;
        private System.Windows.Forms.Label lblMontantVendus;
        private System.Windows.Forms.TextBox txtMontantVendus;
        private System.Windows.Forms.ComboBox cbDevise;
        private System.Windows.Forms.Label lblDevise;
        private System.Windows.Forms.Button btnObjectifs;
    }
}