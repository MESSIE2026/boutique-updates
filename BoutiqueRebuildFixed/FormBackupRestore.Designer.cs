namespace BoutiqueRebuildFixed
{
    partial class FormBackupRestore
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
            this.groupBoxBackup = new System.Windows.Forms.GroupBox();
            this.progressBarBackup = new System.Windows.Forms.ProgressBar();
            this.lblBackupStatus = new System.Windows.Forms.Label();
            this.btnStartBackup = new System.Windows.Forms.Button();
            this.btnBrowseBackup = new System.Windows.Forms.Button();
            this.lblChemin = new System.Windows.Forms.Label();
            this.txtBackupPath = new System.Windows.Forms.TextBox();
            this.groupBoxRestore = new System.Windows.Forms.GroupBox();
            this.progressBarRestore = new System.Windows.Forms.ProgressBar();
            this.lblRestoreStatus = new System.Windows.Forms.Label();
            this.cbDatabases = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStartRestore = new System.Windows.Forms.Button();
            this.btnBrowseRestore = new System.Windows.Forms.Button();
            this.txtRestorePath = new System.Windows.Forms.TextBox();
            this.lblFichierSauvegarde = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.cbLogs = new System.Windows.Forms.ComboBox();
            this.btnPlanifierBackup = new System.Windows.Forms.Button();
            this.groupBoxBackup.SuspendLayout();
            this.groupBoxRestore.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxBackup
            // 
            this.groupBoxBackup.Controls.Add(this.progressBarBackup);
            this.groupBoxBackup.Controls.Add(this.lblBackupStatus);
            this.groupBoxBackup.Controls.Add(this.btnStartBackup);
            this.groupBoxBackup.Controls.Add(this.btnBrowseBackup);
            this.groupBoxBackup.Controls.Add(this.lblChemin);
            this.groupBoxBackup.Controls.Add(this.txtBackupPath);
            this.groupBoxBackup.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxBackup.ForeColor = System.Drawing.Color.White;
            this.groupBoxBackup.Location = new System.Drawing.Point(53, 25);
            this.groupBoxBackup.Name = "groupBoxBackup";
            this.groupBoxBackup.Size = new System.Drawing.Size(532, 451);
            this.groupBoxBackup.TabIndex = 0;
            this.groupBoxBackup.TabStop = false;
            this.groupBoxBackup.Text = "Sauvegarde des Données";
            this.groupBoxBackup.Enter += new System.EventHandler(this.groupBoxBackup_Enter);
            // 
            // progressBarBackup
            // 
            this.progressBarBackup.Location = new System.Drawing.Point(61, 317);
            this.progressBarBackup.Name = "progressBarBackup";
            this.progressBarBackup.Size = new System.Drawing.Size(356, 28);
            this.progressBarBackup.TabIndex = 5;
            // 
            // lblBackupStatus
            // 
            this.lblBackupStatus.AutoSize = true;
            this.lblBackupStatus.ForeColor = System.Drawing.Color.Black;
            this.lblBackupStatus.Location = new System.Drawing.Point(32, 272);
            this.lblBackupStatus.Name = "lblBackupStatus";
            this.lblBackupStatus.Size = new System.Drawing.Size(271, 25);
            this.lblBackupStatus.TabIndex = 4;
            this.lblBackupStatus.Text = "Statut: Sauvegarde en cours...";
            // 
            // btnStartBackup
            // 
            this.btnStartBackup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnStartBackup.Font = new System.Drawing.Font("Segoe UI Black", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartBackup.ForeColor = System.Drawing.Color.White;
            this.btnStartBackup.Location = new System.Drawing.Point(167, 192);
            this.btnStartBackup.Name = "btnStartBackup";
            this.btnStartBackup.Size = new System.Drawing.Size(171, 52);
            this.btnStartBackup.TabIndex = 3;
            this.btnStartBackup.Text = "Lancer Sauvegarde";
            this.btnStartBackup.UseVisualStyleBackColor = false;
            this.btnStartBackup.Click += new System.EventHandler(this.btnStartBackup_Click);
            // 
            // btnBrowseBackup
            // 
            this.btnBrowseBackup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnBrowseBackup.Font = new System.Drawing.Font("Segoe UI", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseBackup.ForeColor = System.Drawing.Color.White;
            this.btnBrowseBackup.Location = new System.Drawing.Point(382, 111);
            this.btnBrowseBackup.Name = "btnBrowseBackup";
            this.btnBrowseBackup.Size = new System.Drawing.Size(117, 33);
            this.btnBrowseBackup.TabIndex = 2;
            this.btnBrowseBackup.Text = "\"Parcourir...\"";
            this.btnBrowseBackup.UseVisualStyleBackColor = false;
            this.btnBrowseBackup.Click += new System.EventHandler(this.btnBrowseBackup_Click);
            // 
            // lblChemin
            // 
            this.lblChemin.AutoSize = true;
            this.lblChemin.ForeColor = System.Drawing.Color.Black;
            this.lblChemin.Location = new System.Drawing.Point(32, 67);
            this.lblChemin.Name = "lblChemin";
            this.lblChemin.Size = new System.Drawing.Size(223, 25);
            this.lblChemin.TabIndex = 1;
            this.lblChemin.Text = "Chemin de Sauvegarde :";
            // 
            // txtBackupPath
            // 
            this.txtBackupPath.Location = new System.Drawing.Point(37, 111);
            this.txtBackupPath.Name = "txtBackupPath";
            this.txtBackupPath.Size = new System.Drawing.Size(314, 33);
            this.txtBackupPath.TabIndex = 0;
            // 
            // groupBoxRestore
            // 
            this.groupBoxRestore.Controls.Add(this.progressBarRestore);
            this.groupBoxRestore.Controls.Add(this.lblRestoreStatus);
            this.groupBoxRestore.Controls.Add(this.cbDatabases);
            this.groupBoxRestore.Controls.Add(this.label1);
            this.groupBoxRestore.Controls.Add(this.btnStartRestore);
            this.groupBoxRestore.Controls.Add(this.btnBrowseRestore);
            this.groupBoxRestore.Controls.Add(this.txtRestorePath);
            this.groupBoxRestore.Controls.Add(this.lblFichierSauvegarde);
            this.groupBoxRestore.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxRestore.ForeColor = System.Drawing.Color.White;
            this.groupBoxRestore.Location = new System.Drawing.Point(623, 25);
            this.groupBoxRestore.Name = "groupBoxRestore";
            this.groupBoxRestore.Size = new System.Drawing.Size(532, 451);
            this.groupBoxRestore.TabIndex = 1;
            this.groupBoxRestore.TabStop = false;
            this.groupBoxRestore.Text = "Restauration des Données";
            // 
            // progressBarRestore
            // 
            this.progressBarRestore.Location = new System.Drawing.Point(41, 386);
            this.progressBarRestore.Name = "progressBarRestore";
            this.progressBarRestore.Size = new System.Drawing.Size(356, 28);
            this.progressBarRestore.TabIndex = 9;
            // 
            // lblRestoreStatus
            // 
            this.lblRestoreStatus.AutoSize = true;
            this.lblRestoreStatus.ForeColor = System.Drawing.Color.Black;
            this.lblRestoreStatus.Location = new System.Drawing.Point(34, 358);
            this.lblRestoreStatus.Name = "lblRestoreStatus";
            this.lblRestoreStatus.Size = new System.Drawing.Size(189, 25);
            this.lblRestoreStatus.TabIndex = 8;
            this.lblRestoreStatus.Text = "Statut restauration :";
            // 
            // cbDatabases
            // 
            this.cbDatabases.FormattingEnabled = true;
            this.cbDatabases.Location = new System.Drawing.Point(39, 297);
            this.cbDatabases.Name = "cbDatabases";
            this.cbDatabases.Size = new System.Drawing.Size(358, 33);
            this.cbDatabases.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(34, 260);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(232, 25);
            this.label1.TabIndex = 6;
            this.label1.Text = "Base des données Cibles :";
            // 
            // btnStartRestore
            // 
            this.btnStartRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnStartRestore.Font = new System.Drawing.Font("Segoe UI Black", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartRestore.ForeColor = System.Drawing.Color.White;
            this.btnStartRestore.Location = new System.Drawing.Point(191, 192);
            this.btnStartRestore.Name = "btnStartRestore";
            this.btnStartRestore.Size = new System.Drawing.Size(171, 52);
            this.btnStartRestore.TabIndex = 5;
            this.btnStartRestore.Text = "Lancer Restauration";
            this.btnStartRestore.UseVisualStyleBackColor = false;
            this.btnStartRestore.Click += new System.EventHandler(this.btnStartRestore_Click);
            // 
            // btnBrowseRestore
            // 
            this.btnBrowseRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.btnBrowseRestore.Font = new System.Drawing.Font("Segoe UI", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseRestore.ForeColor = System.Drawing.Color.White;
            this.btnBrowseRestore.Location = new System.Drawing.Point(396, 110);
            this.btnBrowseRestore.Name = "btnBrowseRestore";
            this.btnBrowseRestore.Size = new System.Drawing.Size(117, 33);
            this.btnBrowseRestore.TabIndex = 4;
            this.btnBrowseRestore.Text = "\"Parcourir...\"";
            this.btnBrowseRestore.UseVisualStyleBackColor = false;
            this.btnBrowseRestore.Click += new System.EventHandler(this.btnBrowseRestore_Click);
            // 
            // txtRestorePath
            // 
            this.txtRestorePath.Location = new System.Drawing.Point(39, 110);
            this.txtRestorePath.Name = "txtRestorePath";
            this.txtRestorePath.Size = new System.Drawing.Size(342, 33);
            this.txtRestorePath.TabIndex = 3;
            // 
            // lblFichierSauvegarde
            // 
            this.lblFichierSauvegarde.AutoSize = true;
            this.lblFichierSauvegarde.ForeColor = System.Drawing.Color.Black;
            this.lblFichierSauvegarde.Location = new System.Drawing.Point(34, 67);
            this.lblFichierSauvegarde.Name = "lblFichierSauvegarde";
            this.lblFichierSauvegarde.Size = new System.Drawing.Size(214, 25);
            this.lblFichierSauvegarde.TabIndex = 2;
            this.lblFichierSauvegarde.Text = "Fichier de sauvegarde :";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Red;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(924, 505);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(109, 44);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Fermer";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.White;
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHelp.ForeColor = System.Drawing.Color.Black;
            this.btnHelp.Location = new System.Drawing.Point(1057, 505);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(98, 44);
            this.btnHelp.TabIndex = 6;
            this.btnHelp.Text = "Aide";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // cbLogs
            // 
            this.cbLogs.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLogs.FormattingEnabled = true;
            this.cbLogs.Location = new System.Drawing.Point(28, 520);
            this.cbLogs.Name = "cbLogs";
            this.cbLogs.Size = new System.Drawing.Size(121, 33);
            this.cbLogs.TabIndex = 7;
            this.cbLogs.Text = "Logs";
            // 
            // btnPlanifierBackup
            // 
            this.btnPlanifierBackup.BackColor = System.Drawing.Color.White;
            this.btnPlanifierBackup.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlanifierBackup.ForeColor = System.Drawing.Color.Black;
            this.btnPlanifierBackup.Location = new System.Drawing.Point(180, 513);
            this.btnPlanifierBackup.Name = "btnPlanifierBackup";
            this.btnPlanifierBackup.Size = new System.Drawing.Size(191, 44);
            this.btnPlanifierBackup.TabIndex = 8;
            this.btnPlanifierBackup.Text = "Planifier Backup";
            this.btnPlanifierBackup.UseVisualStyleBackColor = false;
            this.btnPlanifierBackup.Click += new System.EventHandler(this.btnPlanifierBackup_Click);
            // 
            // FormBackupRestore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Teal;
            this.ClientSize = new System.Drawing.Size(1184, 561);
            this.Controls.Add(this.btnPlanifierBackup);
            this.Controls.Add(this.cbLogs);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBoxRestore);
            this.Controls.Add(this.groupBoxBackup);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FormBackupRestore";
            this.Text = "Backup / Restauration";
            this.Load += new System.EventHandler(this.FormBackupRestore_Load);
            this.groupBoxBackup.ResumeLayout(false);
            this.groupBoxBackup.PerformLayout();
            this.groupBoxRestore.ResumeLayout(false);
            this.groupBoxRestore.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxBackup;
        private System.Windows.Forms.GroupBox groupBoxRestore;
        private System.Windows.Forms.Label lblChemin;
        private System.Windows.Forms.TextBox txtBackupPath;
        private System.Windows.Forms.Button btnStartBackup;
        private System.Windows.Forms.Button btnBrowseBackup;
        private System.Windows.Forms.ProgressBar progressBarBackup;
        private System.Windows.Forms.Label lblBackupStatus;
        private System.Windows.Forms.Label lblFichierSauvegarde;
        private System.Windows.Forms.ComboBox cbDatabases;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStartRestore;
        private System.Windows.Forms.Button btnBrowseRestore;
        private System.Windows.Forms.TextBox txtRestorePath;
        private System.Windows.Forms.ProgressBar progressBarRestore;
        private System.Windows.Forms.Label lblRestoreStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.ComboBox cbLogs;
        private System.Windows.Forms.Button btnPlanifierBackup;
    }
}