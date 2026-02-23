using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public partial class FormTest : FormBase
    {
        public FormTest()
        {
            InitializeComponent();

            this.BackColor = Color.LightGreen;

            Label lbl = new Label();
            lbl.Text = "Test Affichage";
            lbl.AutoSize = true;
            lbl.Location = new Point(20, 20);

            this.Controls.Add(lbl);
        }
    }
}
