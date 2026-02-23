namespace BoutiqueRebuildFixed
{
    partial class FormEmployes
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
            this.btnDetails = new System.Windows.Forms.Button();
            this.btnSupprimer = new System.Windows.Forms.Button();
            this.btnAnnuler = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnChangerPhoto = new System.Windows.Forms.Button();
            this.picPhoto = new System.Windows.Forms.PictureBox();
            this.dtpDateEmbauche = new System.Windows.Forms.DateTimePicker();
            this.Label12 = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.rtbAdresse = new System.Windows.Forms.RichTextBox();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.txtTelephone = new System.Windows.Forms.TextBox();
            this.dtpDateNaissance = new System.Windows.Forms.DateTimePicker();
            this.Label10 = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.cmbSexe = new System.Windows.Forms.ComboBox();
            this.cmbDepartement = new System.Windows.Forms.ComboBox();
            this.cmbPoste = new System.Windows.Forms.ComboBox();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.txtPrenom = new System.Windows.Forms.TextBox();
            this.txtMatricule = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.txtNom = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.dgvEmployes = new System.Windows.Forms.DataGridView();
            this.btnSavePin = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.txtPin = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtNomUtilisateur = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPhoto)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployes)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDetails
            // 
            this.btnDetails.Font = new System.Drawing.Font("Gill Sans MT", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDetails.ForeColor = System.Drawing.Color.Navy;
            this.btnDetails.Location = new System.Drawing.Point(166, 715);
            this.btnDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(93, 32);
            this.btnDetails.TabIndex = 57;
            this.btnDetails.Text = "Details";
            this.btnDetails.UseVisualStyleBackColor = true;
            this.btnDetails.Click += new System.EventHandler(this.btnDetails_Click);
            // 
            // btnSupprimer
            // 
            this.btnSupprimer.Font = new System.Drawing.Font("Gill Sans MT", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSupprimer.ForeColor = System.Drawing.Color.Maroon;
            this.btnSupprimer.Location = new System.Drawing.Point(387, 703);
            this.btnSupprimer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSupprimer.Name = "btnSupprimer";
            this.btnSupprimer.Size = new System.Drawing.Size(134, 44);
            this.btnSupprimer.TabIndex = 56;
            this.btnSupprimer.Text = "Supprimer";
            this.btnSupprimer.UseVisualStyleBackColor = true;
            this.btnSupprimer.Click += new System.EventHandler(this.btnSupprimer_Click);
            // 
            // btnAnnuler
            // 
            this.btnAnnuler.Font = new System.Drawing.Font("Gill Sans MT", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnnuler.ForeColor = System.Drawing.Color.Black;
            this.btnAnnuler.Location = new System.Drawing.Point(279, 715);
            this.btnAnnuler.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnAnnuler.Name = "btnAnnuler";
            this.btnAnnuler.Size = new System.Drawing.Size(100, 32);
            this.btnAnnuler.TabIndex = 55;
            this.btnAnnuler.Text = "Annuler";
            this.btnAnnuler.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("Gill Sans MT", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.Black;
            this.btnOk.Location = new System.Drawing.Point(442, 662);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(79, 40);
            this.btnOk.TabIndex = 54;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnChangerPhoto
            // 
            this.btnChangerPhoto.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangerPhoto.ForeColor = System.Drawing.Color.Navy;
            this.btnChangerPhoto.Location = new System.Drawing.Point(26, 506);
            this.btnChangerPhoto.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnChangerPhoto.Name = "btnChangerPhoto";
            this.btnChangerPhoto.Size = new System.Drawing.Size(167, 37);
            this.btnChangerPhoto.TabIndex = 53;
            this.btnChangerPhoto.Text = "Changer Photo";
            this.btnChangerPhoto.UseVisualStyleBackColor = true;
            this.btnChangerPhoto.Click += new System.EventHandler(this.btnChangerPhoto_Click);
            // 
            // picPhoto
            // 
            this.picPhoto.Location = new System.Drawing.Point(309, 451);
            this.picPhoto.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.picPhoto.Name = "picPhoto";
            this.picPhoto.Size = new System.Drawing.Size(212, 181);
            this.picPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPhoto.TabIndex = 52;
            this.picPhoto.TabStop = false;
            this.picPhoto.UseWaitCursor = true;
            // 
            // dtpDateEmbauche
            // 
            this.dtpDateEmbauche.Location = new System.Drawing.Point(201, 405);
            this.dtpDateEmbauche.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpDateEmbauche.Name = "dtpDateEmbauche";
            this.dtpDateEmbauche.Size = new System.Drawing.Size(298, 27);
            this.dtpDateEmbauche.TabIndex = 51;
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label12.Location = new System.Drawing.Point(16, 403);
            this.Label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(177, 27);
            this.Label12.TabIndex = 50;
            this.Label12.Text = "Date d\'embauche :";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label11.Location = new System.Drawing.Point(24, 475);
            this.Label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(65, 27);
            this.Label11.TabIndex = 49;
            this.Label11.Text = "Photo";
            // 
            // rtbAdresse
            // 
            this.rtbAdresse.Location = new System.Drawing.Point(201, 338);
            this.rtbAdresse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbAdresse.Name = "rtbAdresse";
            this.rtbAdresse.Size = new System.Drawing.Size(320, 27);
            this.rtbAdresse.TabIndex = 48;
            this.rtbAdresse.Text = "";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(201, 305);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(320, 27);
            this.txtEmail.TabIndex = 47;
            // 
            // txtTelephone
            // 
            this.txtTelephone.Location = new System.Drawing.Point(201, 269);
            this.txtTelephone.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtTelephone.Name = "txtTelephone";
            this.txtTelephone.Size = new System.Drawing.Size(217, 27);
            this.txtTelephone.TabIndex = 46;
            // 
            // dtpDateNaissance
            // 
            this.dtpDateNaissance.Location = new System.Drawing.Point(201, 230);
            this.dtpDateNaissance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dtpDateNaissance.Name = "dtpDateNaissance";
            this.dtpDateNaissance.Size = new System.Drawing.Size(217, 27);
            this.dtpDateNaissance.TabIndex = 45;
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label10.Location = new System.Drawing.Point(16, 338);
            this.Label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(93, 27);
            this.Label10.TabIndex = 44;
            this.Label10.Text = "Adresse :";
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.Location = new System.Drawing.Point(16, 304);
            this.Label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(72, 27);
            this.Label9.TabIndex = 43;
            this.Label9.Text = "Email :";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.Location = new System.Drawing.Point(16, 269);
            this.Label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(115, 27);
            this.Label8.TabIndex = 42;
            this.Label8.Text = "Telephone :";
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(16, 231);
            this.Label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(184, 27);
            this.Label7.TabIndex = 41;
            this.Label7.Text = "Date de Naissance :";
            // 
            // cmbSexe
            // 
            this.cmbSexe.FormattingEnabled = true;
            this.cmbSexe.Location = new System.Drawing.Point(201, 192);
            this.cmbSexe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbSexe.Name = "cmbSexe";
            this.cmbSexe.Size = new System.Drawing.Size(217, 28);
            this.cmbSexe.TabIndex = 40;
            // 
            // cmbDepartement
            // 
            this.cmbDepartement.FormattingEnabled = true;
            this.cmbDepartement.Location = new System.Drawing.Point(201, 157);
            this.cmbDepartement.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbDepartement.Name = "cmbDepartement";
            this.cmbDepartement.Size = new System.Drawing.Size(217, 28);
            this.cmbDepartement.TabIndex = 39;
            // 
            // cmbPoste
            // 
            this.cmbPoste.FormattingEnabled = true;
            this.cmbPoste.Location = new System.Drawing.Point(201, 122);
            this.cmbPoste.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbPoste.Name = "cmbPoste";
            this.cmbPoste.Size = new System.Drawing.Size(217, 28);
            this.cmbPoste.TabIndex = 38;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(16, 196);
            this.Label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(63, 27);
            this.Label6.TabIndex = 37;
            this.Label6.Text = "Sexe :";
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(16, 158);
            this.Label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(142, 27);
            this.Label5.TabIndex = 36;
            this.Label5.Text = "Departement :";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(16, 124);
            this.Label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(71, 27);
            this.Label4.TabIndex = 35;
            this.Label4.Text = "Poste :";
            // 
            // txtPrenom
            // 
            this.txtPrenom.Location = new System.Drawing.Point(201, 49);
            this.txtPrenom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPrenom.Name = "txtPrenom";
            this.txtPrenom.Size = new System.Drawing.Size(217, 27);
            this.txtPrenom.TabIndex = 34;
            // 
            // txtMatricule
            // 
            this.txtMatricule.Location = new System.Drawing.Point(201, 86);
            this.txtMatricule.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMatricule.Name = "txtMatricule";
            this.txtMatricule.Size = new System.Drawing.Size(217, 27);
            this.txtMatricule.TabIndex = 33;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.Location = new System.Drawing.Point(16, 87);
            this.Label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(107, 27);
            this.Label3.TabIndex = 32;
            this.Label3.Text = "Matricule :";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(16, 50);
            this.Label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(93, 27);
            this.Label2.TabIndex = 31;
            this.Label2.Text = "Prenom :";
            // 
            // txtNom
            // 
            this.txtNom.Location = new System.Drawing.Point(201, 15);
            this.txtNom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNom.Name = "txtNom";
            this.txtNom.Size = new System.Drawing.Size(217, 27);
            this.txtNom.TabIndex = 30;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(16, 14);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(67, 27);
            this.Label1.TabIndex = 29;
            this.Label1.Text = "Nom :";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("HP Simplified", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(13, 715);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 32);
            this.button1.TabIndex = 58;
            this.button1.Text = "Lister Tables";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dgvEmployes
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvEmployes.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvEmployes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEmployes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvEmployes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvEmployes.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvEmployes.Location = new System.Drawing.Point(528, 12);
            this.dgvEmployes.Name = "dgvEmployes";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEmployes.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvEmployes.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvEmployes.Size = new System.Drawing.Size(830, 708);
            this.dgvEmployes.TabIndex = 59;
            // 
            // btnSavePin
            // 
            this.btnSavePin.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSavePin.ForeColor = System.Drawing.Color.Black;
            this.btnSavePin.Location = new System.Drawing.Point(340, 665);
            this.btnSavePin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSavePin.Name = "btnSavePin";
            this.btnSavePin.Size = new System.Drawing.Size(94, 36);
            this.btnSavePin.TabIndex = 60;
            this.btnSavePin.Text = "Save Pin";
            this.btnSavePin.UseVisualStyleBackColor = true;
            this.btnSavePin.Click += new System.EventHandler(this.btnSavePin_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Gill Sans MT", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(22, 442);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(48, 23);
            this.label13.TabIndex = 61;
            this.label13.Text = "PIN :";
            // 
            // txtPin
            // 
            this.txtPin.Location = new System.Drawing.Point(78, 442);
            this.txtPin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPin.Name = "txtPin";
            this.txtPin.Size = new System.Drawing.Size(150, 27);
            this.txtPin.TabIndex = 62;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Gill Sans MT", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(13, 376);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(167, 27);
            this.label14.TabIndex = 63;
            this.label14.Text = "Nom Utilisateur :";
            // 
            // txtNomUtilisateur
            // 
            this.txtNomUtilisateur.Location = new System.Drawing.Point(201, 375);
            this.txtNomUtilisateur.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtNomUtilisateur.Name = "txtNomUtilisateur";
            this.txtNomUtilisateur.Size = new System.Drawing.Size(320, 27);
            this.txtNomUtilisateur.TabIndex = 64;
            // 
            // FormEmployes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.txtNomUtilisateur);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtPin);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.btnSavePin);
            this.Controls.Add(this.dgvEmployes);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnSupprimer);
            this.Controls.Add(this.btnAnnuler);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnChangerPhoto);
            this.Controls.Add(this.picPhoto);
            this.Controls.Add(this.dtpDateEmbauche);
            this.Controls.Add(this.Label12);
            this.Controls.Add(this.Label11);
            this.Controls.Add(this.rtbAdresse);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.txtTelephone);
            this.Controls.Add(this.dtpDateNaissance);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.cmbSexe);
            this.Controls.Add(this.cmbDepartement);
            this.Controls.Add(this.cmbPoste);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.txtPrenom);
            this.Controls.Add(this.txtMatricule);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.txtNom);
            this.Controls.Add(this.Label1);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormEmployes";
            this.Text = "FormEmployes";
            this.Load += new System.EventHandler(this.FormEmployes_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picPhoto)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmployes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnDetails;
        internal System.Windows.Forms.Button btnSupprimer;
        internal System.Windows.Forms.Button btnAnnuler;
        internal System.Windows.Forms.Button btnOk;
        internal System.Windows.Forms.Button btnChangerPhoto;
        internal System.Windows.Forms.PictureBox picPhoto;
        internal System.Windows.Forms.DateTimePicker dtpDateEmbauche;
        internal System.Windows.Forms.Label Label12;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.RichTextBox rtbAdresse;
        internal System.Windows.Forms.TextBox txtEmail;
        internal System.Windows.Forms.TextBox txtTelephone;
        internal System.Windows.Forms.DateTimePicker dtpDateNaissance;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.ComboBox cmbSexe;
        internal System.Windows.Forms.ComboBox cmbDepartement;
        internal System.Windows.Forms.ComboBox cmbPoste;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.TextBox txtPrenom;
        internal System.Windows.Forms.TextBox txtMatricule;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox txtNom;
        internal System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dgvEmployes;
        internal System.Windows.Forms.Button btnSavePin;
        internal System.Windows.Forms.Label label13;
        internal System.Windows.Forms.TextBox txtPin;
        internal System.Windows.Forms.Label label14;
        internal System.Windows.Forms.TextBox txtNomUtilisateur;
    }
}