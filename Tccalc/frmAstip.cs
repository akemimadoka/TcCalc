using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tccalc
{
    public partial class frmAstip : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }
        public frmAstip(IList<Image> aspects, IList<string> aspectsName, bool enableWallpaper)
        {
            Visible = false;

            if (!enableWallpaper)
            {
                if (ActiveForm != null)
                    ActiveForm.BackgroundImage = null;
            }

            InitializeComponent();

            pictureBox1.Image = aspects[0];
            pictureBox2.Image = aspects[1];
            pictureBox3.Image = aspects[2];

            label1.Text = aspectsName[0];
            label2.Text = aspectsName[1];
            label3.Text = aspectsName[2];
        }

        public frmAstip()
        {
            Visible = false;
            InitializeComponent();
        }
        public void DisableWallpaper()
        {
            BackgroundImage = null;
        }
        public void UpdateAstip(IList<Image> aspects, IList<string> aspectsName)
        {
            if (aspects.Count > 0)
            {
                pictureBox1.Image = aspects[0];
                pictureBox2.Image = aspects[1];
                pictureBox3.Image = aspects[2];
            }

            label1.Text = aspectsName[0];
            label2.Text = aspectsName[1];
            label3.Text = aspectsName[2];
            toolTip1.SetToolTip(label1, label1.Text);
            toolTip1.SetToolTip(label2, label2.Text);
            toolTip1.SetToolTip(label3, label3.Text);
        }

        private void frmAstip_Load(object sender, EventArgs e)
        {

        }
    }
}
