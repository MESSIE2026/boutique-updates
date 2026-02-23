namespace BoutiqueRebuildFixed
{
    partial class Utilisateurs
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnChangerAvatar = new System.Windows.Forms.Button();
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.lblAvatar = new System.Windows.Forms.Label();
            this.lblDateCreation = new System.Windows.Forms.Label();
            this.txtTelephone = new System.Windows.Forms.TextBox();
            this.lblTelephone = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.chkActif = new System.Windows.Forms.CheckBox();
            this.cmbRole = new System.Windows.Forms.ComboBox();
            this.txtPrenom = new System.Windows.Forms.TextBox();
            this.txtNom = new System.Windows.Forms.TextBox();
            this.txtConfirmationMotDePasse = new System.Windows.Forms.TextBox();
            this.txtMotDePasse = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.lblActif = new System.Windows.Forms.Label();
            this.lblReference = new System.Windows.Forms.Label();
            this.lblPrenom = new System.Windows.Forms.Label();
            this.lblNom = new System.Windows.Forms.Label();
            this.lblConfirmationMotDePasse = new System.Windows.Forms.Label();
            this.lblMotDePasse = new System.Windows.Forms.Label();
            this.txtNomUtilisateur = new System.Windows.Forms.TextBox();
            this.lblNomUtilisateur = new System.Windows.Forms.Label();
            this.dptDateCreation = new System.Windows.Forms.DateTimePicker();
            this.dgvUtilisateurs = new System.Windows.Forms.DataGridView();
            this.cmbMagasin = new System.Windows.Forms.ComboBox();
            this.cmbEntreprise = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDesactiverUser = new System.Windows.Forms.Button();
            this.btnReactiverUser = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUtilisateurs)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDetails
            // 
            this.btnDetails.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnDetails.Location = new System.Drawing.Point(21, 684);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(141, 39);
            this.btnDetails.TabIndex = 53;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnSupprimer.Location = new System.Drawing.Point(17, 622);
            this.btnSupprimer.Margin = new System.Windows.Forms.Padding(4);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(141, 39);
            this.btnSupprimer.TabIndex = 52;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btnAnnuler.Location = new System.Drawing.Point(12, 556);
            this.btnAnnuler.Margin = new System.Windows.Forms.Padding(4);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(141, 39);
            this.btnAnnuler.TabIndex = 51;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            this.btnAnnuler.Click += new System.EventHandler(this.btnAnnuler_Click);
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.Black;
            this.btnOk.Location = new System.Drawing.Point(21, 491);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(120, 39);
            this.btnOk.TabIndex = 50;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnChangerAvatar
            // 
            this.btnChangerAvatar.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangerAvatar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnChangerAvatar.Location = new System.Drawing.Point(13, 390);
            this.btnChangerAvatar.Margin = new System.Windows.Forms.Padding(4);
            this.btnChangerAvatar.Name = "btnChangerAvatar";
            this.btnChangerAvatar.Size = new System.Drawing.Size(171, 39);
            this.btnChangerAvatar.TabIndex = 49;
            this.btnChangerAvatar.Text = "Changer Avatar";
            this.btnChangerAvatar.UseVisualStyleBackColor = true;
            this.btnChangerAvatar.Click += new System.EventHandler(this.btnChangerAvatar_Click);
            // 
            // picAvatar
            // 
            this.picAvatar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picAvatar.Location = new System.Drawing.Point(192, 346);
            this.picAvatar.Margin = new System.Windows.Forms.Padding(4);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(259, 154);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picAvatar.TabIndex = 48;
            this.picAvatar.TabStop = false;
            // 
            // lblAvatar
            // 
            this.lblAvatar.AutoSize = true;
            this.lblAvatar.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvatar.Location = new System.Drawing.Point(18, 348);
            this.lblAvatar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAvatar.Name = "lblAvatar";
            this.lblAvatar.Size = new System.Drawing.Size(152, 27);
            this.lblAvatar.TabIndex = 47;
            this.lblAvatar.Text = "Avatar / Photo :";
            // 
            // lblDateCreation
            // 
            this.lblDateCreation.AutoSize = true;
            this.lblDateCreation.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDateCreation.Location = new System.Drawing.Point(16, 308);
            this.lblDateCreation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDateCreation.Name = "lblDateCreation";
            this.lblDateCreation.Size = new System.Drawing.Size(175, 27);
            this.lblDateCreation.TabIndex = 45;
            this.lblDateCreation.Text = "Date de Creation :";
            // 
            // txtTelephone
            // 
            this.txtTelephone.Location = new System.Drawing.Point(254, 280);
            this.txtTelephone.Margin = new System.Windows.Forms.Padding(4);
            this.txtTelephone.Name = "txtTelephone";
            this.txtTelephone.Size = new System.Drawing.Size(197, 25);
            this.txtTelephone.TabIndex = 44;
            // 
            // lblTelephone
            // 
            this.lblTelephone.AutoSize = true;
            this.lblTelephone.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTelephone.Location = new System.Drawing.Point(14, 278);
            this.lblTelephone.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTelephone.Name = "lblTelephone";
            this.lblTelephone.Size = new System.Drawing.Size(115, 27);
            this.lblTelephone.TabIndex = 43;
            this.lblTelephone.Text = "Telephone :";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(254, 243);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(4);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(197, 25);
            this.txtEmail.TabIndex = 42;
            // 
            // chkActif
            // 
            this.chkActif.AutoSize = true;
            this.chkActif.Location = new System.Drawing.Point(358, 214);
            this.chkActif.Margin = new System.Windows.Forms.Padding(4);
            this.chkActif.Name = "chkActif";
            this.chkActif.Size = new System.Drawing.Size(93, 21);
            this.chkActif.TabIndex = 41;
            this.chkActif.Text = "CheckBox1";
            this.chkActif.UseVisualStyleBackColor = true;
            // 
            // cmbRole
            // 
            this.cmbRole.FormattingEnabled = true;
            this.cmbRole.Location = new System.Drawing.Point(254, 181);
            this.cmbRole.Margin = new System.Windows.Forms.Padding(4);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Size = new System.Drawing.Size(197, 25);
            this.cmbRole.TabIndex = 40;
            // 
            // txtPrenom
            // 
            this.txtPrenom.Location = new System.Drawing.Point(254, 148);
            this.txtPrenom.Margin = new System.Windows.Forms.Padding(4);
            this.txtPrenom.Name = "txtPrenom";
            this.txtPrenom.Size = new System.Drawing.Size(197, 25);
            this.txtPrenom.TabIndex = 39;
            // 
            // txtNom
            // 
            this.txtNom.Location = new System.Drawing.Point(254, 117);
            this.txtNom.Margin = new System.Windows.Forms.Padding(4);
            this.txtNom.Name = "txtNom";
            this.txtNom.Size = new System.Drawing.Size(197, 25);
            this.txtNom.TabIndex = 38;
            // 
            // txtConfirmationMotDePasse
            // 
            this.txtConfirmationMotDePasse.Location = new System.Drawing.Point(254, 85);
            this.txtConfirmationMotDePasse.Margin = new System.Windows.Forms.Padding(4);
            this.txtConfirmationMotDePasse.Name = "txtConfirmationMotDePasse";
            this.txtConfirmationMotDePasse.Size = new System.Drawing.Size(197, 25);
            this.txtConfirmationMotDePasse.TabIndex = 37;
            this.txtConfirmationMotDePasse.UseSystemPasswordChar = true;
            // 
            // txtMotDePasse
            // 
            this.txtMotDePasse.Location = new System.Drawing.Point(254, 51);
            this.txtMotDePasse.Margin = new System.Windows.Forms.Padding(4);
            this.txtMotDePasse.Name = "txtMotDePasse";
            this.txtMotDePasse.Size = new System.Drawing.Size(197, 25);
            this.txtMotDePasse.TabIndex = 36;
            this.txtMotDePasse.UseSystemPasswordChar = true;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmail.Location = new System.Drawing.Point(16, 246);
            this.lblEmail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(72, 27);
            this.lblEmail.TabIndex = 35;
            this.lblEmail.Text = "Email :";
            // 
            // lblActif
            // 
            this.lblActif.AutoSize = true;
            this.lblActif.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActif.Location = new System.Drawing.Point(16, 214);
            this.lblActif.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblActif.Name = "lblActif";
            this.lblActif.Size = new System.Drawing.Size(66, 27);
            this.lblActif.TabIndex = 34;
            this.lblActif.Text = "Actif :";
            // 
            // lblReference
            // 
            this.lblReference.AutoSize = true;
            this.lblReference.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblReference.Location = new System.Drawing.Point(16, 181);
            this.lblReference.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblReference.Name = "lblReference";
            this.lblReference.Size = new System.Drawing.Size(111, 27);
            this.lblReference.TabIndex = 33;
            this.lblReference.Text = "Reference :";
            // 
            // lblPrenom
            // 
            this.lblPrenom.AutoSize = true;
            this.lblPrenom.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrenom.Location = new System.Drawing.Point(16, 148);
            this.lblPrenom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPrenom.Name = "lblPrenom";
            this.lblPrenom.Size = new System.Drawing.Size(93, 27);
            this.lblPrenom.TabIndex = 32;
            this.lblPrenom.Text = "Prenom :";
            // 
            // lblNom
            // 
            this.lblNom.AutoSize = true;
            this.lblNom.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNom.Location = new System.Drawing.Point(16, 118);
            this.lblNom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNom.Name = "lblNom";
            this.lblNom.Size = new System.Drawing.Size(72, 27);
            this.lblNom.TabIndex = 31;
            this.lblNom.Text = "Nom : ";
            // 
            // lblConfirmationMotDePasse
            // 
            this.lblConfirmationMotDePasse.AutoSize = true;
            this.lblConfirmationMotDePasse.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConfirmationMotDePasse.Location = new System.Drawing.Point(13, 83);
            this.lblConfirmationMotDePasse.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConfirmationMotDePasse.Name = "lblConfirmationMotDePasse";
            this.lblConfirmationMotDePasse.Size = new System.Drawing.Size(223, 23);
            this.lblConfirmationMotDePasse.TabIndex = 30;
            this.lblConfirmationMotDePasse.Text = "Confirmation Mot de Passe :";
            // 
            // lblMotDePasse
            // 
            this.lblMotDePasse.AutoSize = true;
            this.lblMotDePasse.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMotDePasse.Location = new System.Drawing.Point(16, 48);
            this.lblMotDePasse.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMotDePasse.Name = "lblMotDePasse";
            this.lblMotDePasse.Size = new System.Drawing.Size(137, 27);
            this.lblMotDePasse.TabIndex = 29;
            this.lblMotDePasse.Text = "Mot de Passe :";
            // 
            // txtNomUtilisateur
            // 
            this.txtNomUtilisateur.Location = new System.Drawing.Point(254, 17);
            this.txtNomUtilisateur.Margin = new System.Windows.Forms.Padding(4);
            this.txtNomUtilisateur.Name = "txtNomUtilisateur";
            this.txtNomUtilisateur.Size = new System.Drawing.Size(197, 25);
            this.txtNomUtilisateur.TabIndex = 28;
            // 
            // lblNomUtilisateur
            // 
            this.lblNomUtilisateur.AutoSize = true;
            this.lblNomUtilisateur.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNomUtilisateur.Location = new System.Drawing.Point(16, 12);
            this.lblNomUtilisateur.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNomUtilisateur.Name = "lblNomUtilisateur";
            this.lblNomUtilisateur.Size = new System.Drawing.Size(178, 27);
            this.lblNomUtilisateur.TabIndex = 27;
            this.lblNomUtilisateur.Text = "Nom d\'utilisateur :";
            // 
            // dptDateCreation
            // 
            this.dptDateCreation.Location = new System.Drawing.Point(254, 313);
            this.dptDateCreation.Margin = new System.Windows.Forms.Padding(4);
            this.dptDateCreation.Name = "dptDateCreation";
            this.dptDateCreation.Size = new System.Drawing.Size(197, 25);
            this.dptDateCreation.TabIndex = 54;
            // 
            // dgvUtilisateurs
            // 
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvUtilisateurs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle17;
            this.dgvUtilisateurs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvUtilisateurs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle18;
            this.dgvUtilisateurs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle19.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvUtilisateurs.DefaultCellStyle = dataGridViewCellStyle19;
            this.dgvUtilisateurs.Location = new System.Drawing.Point(458, 17);
            this.dgvUtilisateurs.Name = "dgvUtilisateurs";
            dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvUtilisateurs.RowsDefaultCellStyle = dataGridViewCellStyle20;
            this.dgvUtilisateurs.Size = new System.Drawing.Size(844, 719);
            this.dgvUtilisateurs.TabIndex = 55;
            // 
            // cmbMagasin
            // 
            this.cmbMagasin.FormattingEnabled = true;
            this.cmbMagasin.Location = new System.Drawing.Point(265, 521);
            this.cmbMagasin.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMagasin.Name = "cmbMagasin";
            this.cmbMagasin.Size = new System.Drawing.Size(186, 25);
            this.cmbMagasin.TabIndex = 56;
            // 
            // cmbEntreprise
            // 
            this.cmbEntreprise.FormattingEnabled = true;
            this.cmbEntreprise.Location = new System.Drawing.Point(295, 562);
            this.cmbEntreprise.Margin = new System.Windows.Forms.Padding(4);
            this.cmbEntreprise.Name = "cmbEntreprise";
            this.cmbEntreprise.Size = new System.Drawing.Size(156, 25);
            this.cmbEntreprise.TabIndex = 57;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(172, 519);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 27);
            this.label1.TabIndex = 58;
            this.label1.Text = "Magasin :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(172, 562);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 27);
            this.label2.TabIndex = 59;
            this.label2.Text = "Entreprise :";
            // 
            // btnDesactiverUser
            // 
            this.btnDesactiverUser.BackColor = System.Drawing.Color.Red;
            this.btnDesactiverUser.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDesactiverUser.ForeColor = System.Drawing.Color.White;
            this.btnDesactiverUser.Location = new System.Drawing.Point(204, 622);
            this.btnDesactiverUser.Margin = new System.Windows.Forms.Padding(4);
            this.btnDesactiverUser.Name = "btnDesactiverUser";
            this.btnDesactiverUser.Size = new System.Drawing.Size(198, 39);
            this.btnDesactiverUser.TabIndex = 60;
            this.btnDesactiverUser.Text = "Desactiver Users";
            this.btnDesactiverUser.UseVisualStyleBackColor = false;
            this.btnDesactiverUser.Click += new System.EventHandler(this.btnDesactiverUser_Click);
            // 
            // btnReactiverUser
            // 
            this.btnReactiverUser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnReactiverUser.Font = new System.Drawing.Font("HP Simplified", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReactiverUser.ForeColor = System.Drawing.Color.White;
            this.btnReactiverUser.Location = new System.Drawing.Point(204, 684);
            this.btnReactiverUser.Margin = new System.Windows.Forms.Padding(4);
            this.btnReactiverUser.Name = "btnReactiverUser";
            this.btnReactiverUser.Size = new System.Drawing.Size(198, 39);
            this.btnReactiverUser.TabIndex = 61;
            this.btnReactiverUser.Text = "Reactiver Users";
            this.btnReactiverUser.UseVisualStyleBackColor = false;
            this.btnReactiverUser.Click += new System.EventHandler(this.btnReactiverUser_Click);
            // 
            // Utilisateurs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.btnReactiverUser);
            this.Controls.Add(this.btnDesactiverUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbEntreprise);
            this.Controls.Add(this.cmbMagasin);
            this.Controls.Add(this.dgvUtilisateurs);
            this.Controls.Add(this.dptDateCreation);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnChangerAvatar);
            this.Controls.Add(this.picAvatar);
            this.Controls.Add(this.lblAvatar);
            this.Controls.Add(this.lblDateCreation);
            this.Controls.Add(this.txtTelephone);
            this.Controls.Add(this.lblTelephone);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.chkActif);
            this.Controls.Add(this.cmbRole);
            this.Controls.Add(this.txtPrenom);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.txtConfirmationMotDePasse);
            this.Controls.Add(this.txtMotDePasse);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.lblActif);
            this.Controls.Add(this.lblReference);
            this.Controls.Add(this.lblPrenom);
            this.Controls.Add(this.lblNom);
            this.Controls.Add(this.lblConfirmationMotDePasse);
            this.Controls.Add(this.lblMotDePasse);
            this.Controls.Add(this.txtNomUtilisateur);
            this.Controls.Add(this.lblNomUtilisateur);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Utilisateurs";
            this.Text = "Utilisateurs";
            this.Load += new System.EventHandler(this.FormUtilisateurs_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUtilisateurs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnDetails;
        internal System.Windows.Forms.Button btnSupprimer;
        internal System.Windows.Forms.Button btnAnnuler;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Button btnChangerAvatar;
        internal System.Windows.Forms.PictureBox picAvatar;
        internal System.Windows.Forms.Label lblAvatar;
        internal System.Windows.Forms.Label lblDateCreation;
        internal System.Windows.Forms.TextBox txtTelephone;
        internal System.Windows.Forms.Label lblTelephone;
        internal System.Windows.Forms.TextBox txtEmail;
        internal System.Windows.Forms.CheckBox chkActif;
        internal System.Windows.Forms.ComboBox cmbRole;
        internal System.Windows.Forms.TextBox txtPrenom;
        internal System.Windows.Forms.TextBox txtNom;
        internal System.Windows.Forms.TextBox txtConfirmationMotDePasse;
        internal System.Windows.Forms.TextBox txtMotDePasse;
        internal System.Windows.Forms.Label lblEmail;
        internal System.Windows.Forms.Label lblActif;
        internal System.Windows.Forms.Label lblReference;
        internal System.Windows.Forms.Label lblPrenom;
        internal System.Windows.Forms.Label lblNom;
        internal System.Windows.Forms.Label lblConfirmationMotDePasse;
        internal System.Windows.Forms.Label lblMotDePasse;
        internal System.Windows.Forms.TextBox txtNomUtilisateur;
        internal System.Windows.Forms.Label lblNomUtilisateur;
        private System.Windows.Forms.DateTimePicker dptDateCreation;
        private System.Windows.Forms.DataGridView dgvUtilisateurs;
        internal System.Windows.Forms.ComboBox cmbMagasin;
        internal System.Windows.Forms.ComboBox cmbEntreprise;
        internal System.Windows.Forms.Label label1;
        internal System.Windows.Forms.Label label2;
        internal System.Windows.Forms.Button btnDesactiverUser;
        internal System.Windows.Forms.Button btnReactiverUser;
    }
}