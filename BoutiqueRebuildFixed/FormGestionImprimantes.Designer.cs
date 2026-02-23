namespace BoutiqueRebuildFixed
{
    partial class FormGestionImprimantes
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblTicket = new System.Windows.Forms.Label();
            this.cboTicket = new System.Windows.Forms.ComboBox();
            this.btnTestTicket = new System.Windows.Forms.Button();
            this.lblA4 = new System.Windows.Forms.Label();
            this.cboA4 = new System.Windows.Forms.ComboBox();
            this.btnTestA4 = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lblTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTitle.Font = new System.Drawing.Font("Gill Sans Ultra Bold", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(279, 28);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(478, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "GESTION DES IMPRIMANTES";
            // 
            // lblTicket
            // 
            this.lblTicket.AutoSize = true;
            this.lblTicket.Font = new System.Drawing.Font("Segoe UI Black", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTicket.ForeColor = System.Drawing.Color.White;
            this.lblTicket.Location = new System.Drawing.Point(87, 152);
            this.lblTicket.Name = "lblTicket";
            this.lblTicket.Size = new System.Drawing.Size(244, 32);
            this.lblTicket.TabIndex = 1;
            this.lblTicket.Text = "Imprimante Ticket :";
            // 
            // cboTicket
            // 
            this.cboTicket.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTicket.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboTicket.FormattingEnabled = true;
            this.cboTicket.Location = new System.Drawing.Point(377, 146);
            this.cboTicket.Name = "cboTicket";
            this.cboTicket.Size = new System.Drawing.Size(286, 38);
            this.cboTicket.TabIndex = 2;
            // 
            // btnTestTicket
            // 
            this.btnTestTicket.BackColor = System.Drawing.Color.Blue;
            this.btnTestTicket.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestTicket.ForeColor = System.Drawing.Color.White;
            this.btnTestTicket.Location = new System.Drawing.Point(707, 147);
            this.btnTestTicket.Name = "btnTestTicket";
            this.btnTestTicket.Size = new System.Drawing.Size(140, 37);
            this.btnTestTicket.TabIndex = 3;
            this.btnTestTicket.Text = "Tester Ticket";
            this.btnTestTicket.UseVisualStyleBackColor = false;
            this.btnTestTicket.Click += new System.EventHandler(this.btnTestTicket_Click);
            // 
            // lblA4
            // 
            this.lblA4.AutoSize = true;
            this.lblA4.Font = new System.Drawing.Font("Segoe UI Black", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblA4.ForeColor = System.Drawing.Color.White;
            this.lblA4.Location = new System.Drawing.Point(87, 238);
            this.lblA4.Name = "lblA4";
            this.lblA4.Size = new System.Drawing.Size(206, 32);
            this.lblA4.TabIndex = 4;
            this.lblA4.Text = "Imprimante A4 :";
            // 
            // cboA4
            // 
            this.cboA4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboA4.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboA4.FormattingEnabled = true;
            this.cboA4.Location = new System.Drawing.Point(377, 232);
            this.cboA4.Name = "cboA4";
            this.cboA4.Size = new System.Drawing.Size(286, 38);
            this.cboA4.TabIndex = 5;
            // 
            // btnTestA4
            // 
            this.btnTestA4.BackColor = System.Drawing.Color.Blue;
            this.btnTestA4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestA4.ForeColor = System.Drawing.Color.White;
            this.btnTestA4.Location = new System.Drawing.Point(707, 233);
            this.btnTestA4.Name = "btnTestA4";
            this.btnTestA4.Size = new System.Drawing.Size(140, 37);
            this.btnTestA4.TabIndex = 6;
            this.btnTestA4.Text = "Tester A4";
            this.btnTestA4.UseVisualStyleBackColor = false;
            this.btnTestA4.Click += new System.EventHandler(this.btnTestA4_Click);
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Black", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Blue;
            this.btnSave.Location = new System.Drawing.Point(279, 325);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(144, 43);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Enregistrer";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Black", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.Red;
            this.btnClose.Location = new System.Drawing.Point(532, 325);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(144, 43);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Fermer";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FormGestionImprimantes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Purple;
            this.ClientSize = new System.Drawing.Size(934, 410);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnTestA4);
            this.Controls.Add(this.cboA4);
            this.Controls.Add(this.lblA4);
            this.Controls.Add(this.btnTestTicket);
            this.Controls.Add(this.cboTicket);
            this.Controls.Add(this.lblTicket);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormGestionImprimantes";
            this.Text = "FormGestionImprimantes";
            this.Load += new System.EventHandler(this.FormGestionImprimantes_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTicket;
        private System.Windows.Forms.ComboBox cboTicket;
        private System.Windows.Forms.Button btnTestTicket;
        private System.Windows.Forms.Label lblA4;
        private System.Windows.Forms.ComboBox cboA4;
        private System.Windows.Forms.Button btnTestA4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}