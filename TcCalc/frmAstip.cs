using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcCalc
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

		public frmAstip(bool enableWallpaper)
		{
			if (!enableWallpaper)
			{
				if (ActiveForm != null)
				{
					ActiveForm.BackgroundImage = null;
				}
			}

			Visible = false;
			InitializeComponent();
		}

		public void UpdateAstip(IEnumerable<Image> aspectsImages, IEnumerable<string> aspectsNames)
		{
			using (var aspectImageEnumerator = aspectsImages.GetEnumerator())
			{
				aspectImageEnumerator.MoveNext();
				pictureBox1.Image = aspectImageEnumerator.Current;
				aspectImageEnumerator.MoveNext();
				pictureBox2.Image = aspectImageEnumerator.Current;
				aspectImageEnumerator.MoveNext();
				pictureBox3.Image = aspectImageEnumerator.Current;
			}

			using (var aspectNameEnumerator = aspectsNames.GetEnumerator())
			{
				aspectNameEnumerator.MoveNext();
				label1.Text = aspectNameEnumerator.Current;
				aspectNameEnumerator.MoveNext();
				label2.Text = aspectNameEnumerator.Current;
				aspectNameEnumerator.MoveNext();
				label3.Text = aspectNameEnumerator.Current;
			}

			toolTip1.SetToolTip(label1, label1.Text);
			toolTip1.SetToolTip(label2, label2.Text);
			toolTip1.SetToolTip(label3, label3.Text);
		}

		private void frmAstip_Load(object sender, EventArgs e)
		{

		}
	}
}
