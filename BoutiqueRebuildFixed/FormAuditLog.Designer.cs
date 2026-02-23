namespace BoutiqueRebuildFixed
{
    partial class FormAuditLog
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
            this.lblTitre = new System.Windows.Forms.Label();
            this.panelFiltres = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnFiltrer = new System.Windows.Forms.Button();
            this.cmbAction = new System.Windows.Forms.ComboBox();
            this.lblAction = new System.Windows.Forms.Label();
            this.dtpFin = new System.Windows.Forms.DateTimePicker();
            this.lblAu = new System.Windows.Forms.Label();
            this.lblDu = new System.Windows.Forms.Label();
            this.dtpDebut = new System.Windows.Forms.DateTimePicker();
            this.lblPeriode = new System.Windows.Forms.Label();
            this.dgvAuditLog = new System.Windows.Forms.DataGridView();
            this.btnSuivant = new System.Windows.Forms.Button();
            this.btnPrecedent = new System.Windows.Forms.Button();
            this.panelFiltres.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLog)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitre
            // 
            this.lblTitre.AutoSize = true;
            this.lblTitre.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitre.Font = new System.Drawing.Font("Segoe UI Black", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitre.ForeColor = System.Drawing.Color.White;
            this.lblTitre.Location = new System.Drawing.Point(0, 0);
            this.lblTitre.Name = "lblTitre";
            this.lblTitre.Size = new System.Drawing.Size(531, 37);
            this.lblTitre.TabIndex = 0;
            this.lblTitre.Text = "JOURNAL DES ACTIVITÉS (AUDIT LOG)";
            this.lblTitre.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelFiltres
            // 
            this.panelFiltres.Controls.Add(this.btnReset);
            this.panelFiltres.Controls.Add(this.btnFiltrer);
            this.panelFiltres.Controls.Add(this.cmbAction);
            this.panelFiltres.Controls.Add(this.lblAction);
            this.panelFiltres.Controls.Add(this.dtpFin);
            this.panelFiltres.Controls.Add(this.lblAu);
            this.panelFiltres.Controls.Add(this.lblDu);
            this.panelFiltres.Controls.Add(this.dtpDebut);
            this.panelFiltres.Controls.Add(this.lblPeriode);
            this.panelFiltres.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltres.Location = new System.Drawing.Point(0, 37);
            this.panelFiltres.Name = "panelFiltres";
            this.panelFiltres.Size = new System.Drawing.Size(1314, 65);
            this.panelFiltres.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.Blue;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.ForeColor = System.Drawing.Color.White;
            this.btnReset.Location = new System.Drawing.Point(1100, 13);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(126, 31);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "Réinitialiser";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnFiltrer
            // 
            this.btnFiltrer.BackColor = System.Drawing.Color.Blue;
            this.btnFiltrer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltrer.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFiltrer.ForeColor = System.Drawing.Color.White;
            this.btnFiltrer.Location = new System.Drawing.Point(984, 14);
            this.btnFiltrer.Name = "btnFiltrer";
            this.btnFiltrer.Size = new System.Drawing.Size(89, 29);
            this.btnFiltrer.TabIndex = 5;
            this.btnFiltrer.Text = "Filtrer";
            this.btnFiltrer.UseVisualStyleBackColor = false;
            this.btnFiltrer.Click += new System.EventHandler(this.btnFiltrer_Click);
            // 
            // cmbAction
            // 
            this.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAction.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbAction.FormattingEnabled = true;
            this.cmbAction.Location = new System.Drawing.Point(747, 13);
            this.cmbAction.Name = "cmbAction";
            this.cmbAction.Size = new System.Drawing.Size(207, 29);
            this.cmbAction.TabIndex = 4;
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAction.ForeColor = System.Drawing.Color.White;
            this.lblAction.Location = new System.Drawing.Point(657, 16);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(75, 25);
            this.lblAction.TabIndex = 2;
            this.lblAction.Text = "Action :";
            // 
            // dtpFin
            // 
            this.dtpFin.CalendarFont = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpFin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpFin.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFin.Location = new System.Drawing.Point(489, 13);
            this.dtpFin.Name = "dtpFin";
            this.dtpFin.Size = new System.Drawing.Size(148, 29);
            this.dtpFin.TabIndex = 2;
            // 
            // lblAu
            // 
            this.lblAu.AutoSize = true;
            this.lblAu.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAu.ForeColor = System.Drawing.Color.White;
            this.lblAu.Location = new System.Drawing.Point(434, 15);
            this.lblAu.Name = "lblAu";
            this.lblAu.Size = new System.Drawing.Size(49, 25);
            this.lblAu.TabIndex = 3;
            this.lblAu.Text = "Au :";
            // 
            // lblDu
            // 
            this.lblDu.AutoSize = true;
            this.lblDu.Font = new System.Drawing.Font("Segoe UI Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDu.ForeColor = System.Drawing.Color.White;
            this.lblDu.Location = new System.Drawing.Point(200, 16);
            this.lblDu.Name = "lblDu";
            this.lblDu.Size = new System.Drawing.Size(49, 25);
            this.lblDu.TabIndex = 2;
            this.lblDu.Text = "Du :";
            // 
            // dtpDebut
            // 
            this.dtpDebut.CalendarFont = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpDebut.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpDebut.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDebut.Location = new System.Drawing.Point(266, 14);
            this.dtpDebut.Name = "dtpDebut";
            this.dtpDebut.Size = new System.Drawing.Size(149, 29);
            this.dtpDebut.TabIndex = 1;
            // 
            // lblPeriode
            // 
            this.lblPeriode.AutoSize = true;
            this.lblPeriode.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPeriode.ForeColor = System.Drawing.Color.White;
            this.lblPeriode.Location = new System.Drawing.Point(109, 17);
            this.lblPeriode.Name = "lblPeriode";
            this.lblPeriode.Size = new System.Drawing.Size(85, 25);
            this.lblPeriode.TabIndex = 0;
            this.lblPeriode.Text = "Période :";
            // 
            // dgvAuditLog
            // 
            this.dgvAuditLog.AllowUserToAddRows = false;
            this.dgvAuditLog.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.AliceBlue;
            this.dgvAuditLog.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAuditLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAuditLog.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAuditLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvAuditLog.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvAuditLog.Location = new System.Drawing.Point(0, 102);
            this.dgvAuditLog.Name = "dgvAuditLog";
            this.dgvAuditLog.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAuditLog.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvAuditLog.RowHeadersVisible = false;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvAuditLog.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dgvAuditLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAuditLog.Size = new System.Drawing.Size(1311, 591);
            this.dgvAuditLog.TabIndex = 2;
            // 
            // btnSuivant
            // 
            this.btnSuivant.BackColor = System.Drawing.Color.Blue;
            this.btnSuivant.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSuivant.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSuivant.ForeColor = System.Drawing.Color.White;
            this.btnSuivant.Location = new System.Drawing.Point(686, 708);
            this.btnSuivant.Name = "btnSuivant";
            this.btnSuivant.Size = new System.Drawing.Size(93, 29);
            this.btnSuivant.TabIndex = 6;
            this.btnSuivant.Text = "Suivant";
            this.btnSuivant.UseVisualStyleBackColor = false;
            this.btnSuivant.Click += new System.EventHandler(this.btnSuivant_Click);
            // 
            // btnPrecedent
            // 
            this.btnPrecedent.BackColor = System.Drawing.Color.Blue;
            this.btnPrecedent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrecedent.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrecedent.ForeColor = System.Drawing.Color.White;
            this.btnPrecedent.Location = new System.Drawing.Point(504, 708);
            this.btnPrecedent.Name = "btnPrecedent";
            this.btnPrecedent.Size = new System.Drawing.Size(113, 29);
            this.btnPrecedent.TabIndex = 7;
            this.btnPrecedent.Text = "Precedent";
            this.btnPrecedent.UseVisualStyleBackColor = false;
            this.btnPrecedent.Click += new System.EventHandler(this.btnPrecedent_Click);
            // 
            // FormAuditLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1314, 749);
            this.Controls.Add(this.btnPrecedent);
            this.Controls.Add(this.btnSuivant);
            this.Controls.Add(this.dgvAuditLog);
            this.Controls.Add(this.panelFiltres);
            this.Controls.Add(this.lblTitre);
            this.Name = "FormAuditLog";
            this.Text = "FormAuditLog";
            this.Load += new System.EventHandler(this.FormAuditLog_Load);
            this.panelFiltres.ResumeLayout(false);
            this.panelFiltres.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLog)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitre;
        private System.Windows.Forms.Panel panelFiltres;
        private System.Windows.Forms.DateTimePicker dtpDebut;
        private System.Windows.Forms.Label lblPeriode;
        private System.Windows.Forms.ComboBox cmbAction;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.DateTimePicker dtpFin;
        private System.Windows.Forms.Label lblAu;
        private System.Windows.Forms.Label lblDu;
        private System.Windows.Forms.Button btnFiltrer;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.DataGridView dgvAuditLog;
        private System.Windows.Forms.Button btnSuivant;
        private System.Windows.Forms.Button btnPrecedent;
    }
}