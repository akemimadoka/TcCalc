using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcCalc.Properties;

namespace TcCalc
{
	public sealed partial class Form2 : Form
	{
		private readonly AspectCalc _aspectCalc;
		private readonly List<List<AspectCalc.Aspect>> _asresult;
		private readonly frmAstip _astip;

		public Form2(AspectCalc aspectCalc, IEnumerable<List<AspectCalc.Aspect>> result, int step, ImageList.ImageCollection aspectImageList, frmAstip astip)
		{
			_aspectCalc = aspectCalc;
			_asresult = new List<List<AspectCalc.Aspect>>(result);
			_astip = astip;
			if (_asresult.Count == 0)
			{
				throw new ArgumentException(Resources.NoResult, nameof(result));
			}

			InitializeComponent();
			for (var i = 0; i < aspectImageList.Count;++i)
			{
				imageList1.Images.Add(aspectImageList.Keys[i], aspectImageList[i]);
			}

			Text = string.Format(Resources.ResultWindowTitle, _asresult.Count, step);

			foreach (var r in _asresult)
			{
				listBox1.Items.Add(string.Join(" ", r.Select(a => a.FullName)));
			}
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			listView1.Clear();
			if (listBox1.SelectedItems.Count <= 0)
			{
				return;
			}
			listView1.Items.AddRange((from item in _asresult[listBox1.SelectedIndex] select new ListViewItem(item.FullName, item.Name)).ToArray());
		}

		private void Form2_Resize(object sender, EventArgs e)
		{
			var findForm = listBox1.FindForm();
			if (findForm != null)
			{
				listBox1.Width = findForm.Width - 60;
				listBox1.Height = findForm.Height - 176;
				listView1.Location = new Point(findForm.Width > 22 ? 22 : listView1.Location.X,
					findForm.Height > listView1.Height ? listBox1.Height + 46 : listView1.Location.Y);
			}
			listView1.Width = listBox1.Width;
		}

		private Point _pointView = new Point(0, 0);//定义外部存储变量
		private void listView1_MouseMove(object sender, MouseEventArgs e)
		{
			var lv = listView1.GetItemAt(e.X, e.Y);
			if (lv != null)
			{
				var aspect = _aspectCalc.FindAspect(lv.ImageKey);
				aspect.Recipe?.Let(recipe =>
				{
					if (_pointView.X != e.X || _pointView.Y != e.Y) //防止tooltip不停闪烁问题
					{
						var aspects = new[]
							{ aspect, _aspectCalc.FindAspect(recipe.Item1), _aspectCalc.FindAspect(recipe.Item2) };
						var aspectsName = aspects.Select(a => a.FullName);
						var tmpImage = aspects.Select(a => imageList1.Images[imageList1.Images.Keys.IndexOf(a.Name)]);

						_astip.UpdateAstip(tmpImage, aspectsName);
						if (ActiveForm != null)
							_astip.Location = new Point(e.X + listView1.Left + ActiveForm.Left + 10,
								e.Y + listView1.Top + ActiveForm.Top - 70);

						_astip.Visible = true;
						Focus();
					}
				});
			}
			else
			{
				_astip.Visible = false; //没有取到item自动隐藏
			}

			_pointView = new Point(e.X, e.Y);
		}

		private void listView1_MouseLeave(object sender, EventArgs e)
		{
			_astip.Visible = false;
		}
	}
}
