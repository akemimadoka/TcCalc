using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TcCalc.Properties;

namespace TcCalc
{
	public partial class Form1 : Form
	{
		private readonly AspectCalc _aspectCalc = new AspectCalc("Aspects.txt");

		private readonly Setting _settings = new Setting("Settings.ini");

		private List<string> _rawUpdateData = new List<string>();
		private bool _basicFirst = true;

		public Form1()
		{
			InitializeComponent();
		}

		private static void GetAllFileByDir(string dirPath, List<string> fl)
		{
			fl.AddRange(Directory.GetFiles(dirPath));

			foreach (var dir in Directory.GetDirectories(dirPath))
			{
				GetAllFileByDir(dir, fl);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var settings = _settings.Settings;

			if (_aspectCalc.Version != null)
			{
				label8.Text = Resources.AspectDescriptionFileVersion + _aspectCalc.Version;
			}

			_astip = new frmAstip(settings.TryGetValue("enablewallpaper", out var enableWallpaper) && enableWallpaper == "true");

			if (settings.TryGetValue("basicfirst", out var basicFirst))
			{
				_basicFirst = basicFirst == "true";
			}

			var fileList = new List<string>();
			GetAllFileByDir(@"pictures\color\", fileList);
			foreach (var filename in fileList)
			{
				var parsedFilename = Path.GetFileNameWithoutExtension(filename);
				_aspectCalc.FindAspect(parsedFilename)?.Let(aspect =>
				{
					imageList1.Images.Add(aspect.Name, Image.FromFile(filename));
					listView1.Items.Add(new ListViewItem(aspect.FullName, aspect.Name));
				});
			}

			if (_basicFirst)
			{
				var basicAspects = new List<ListViewItem>();

				foreach (var item in from ListViewItem item in listView1.Items
					where _aspectCalc.FindAspect(item.ImageKey).IsBasicAspect
					select item)
				{
					basicAspects.Add(item);
					listView1.Items.Remove(item);
				}

				if (basicAspects.Count > 0)
				{
					for (var index = 0; index < basicAspects.Count; ++index)
					{
						listView1.Items.Insert(index, basicAspects[index]);
					}
				}

				// Bug Fix
				var items = (from ListViewItem item in listView1.Items select (ListViewItem) item.Clone()).ToArray();
				listView1.Clear();
				listView1.Items.AddRange(items);
			}

			listView2.Items.AddRange((from ListViewItem item in listView1.Items select (ListViewItem) item.Clone())
				.ToArray());
			listView3.Items.AddRange((from ListViewItem item in listView1.Items select (ListViewItem) item.Clone())
				.ToArray());
		}

		private void button1_Click(object sender, EventArgs e)
		{
			while (true)
			{
				var exceptedAspects = new List<AspectCalc.Aspect>();
				if (listView3.SelectedItems.Count > 0)
				{
					exceptedAspects.AddRange(from ListViewItem x in listView3.SelectedItems
						select _aspectCalc.FindAspect(x.ImageKey));
				}

				if (textBox1.Text == "")
				{
					MessageBox.Show(Resources.StepRequested, Resources.Prompt, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}

				if (listView1.SelectedItems.Count != 1 || listView2.SelectedItems.Count != 1)
				{
					MessageBox.Show(Resources.CheckAspect, Resources.Prompt, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}

				if (listView3.SelectedItems.Cast<ListViewItem>().Any(x =>
					x.Text == listView1.SelectedItems[0].Text || x.Text == listView2.SelectedItems[0].Text))
				{
					MessageBox.Show(Resources.StartEndAspectRequested, Resources.Prompt, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}

				if (!int.TryParse(textBox1.Text, out var step))
				{
					MessageBox.Show(Resources.StepShouldBeNumber, Resources.Prompt, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}

				if (step < 0)
				{
					MessageBox.Show(Resources.StepShouldBePositiveNum, Resources.Prompt, MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					return;
				}

				if (step > 9)
				{
					if (MessageBox.Show(Resources.StepTooBig, Resources.Prompt, MessageBoxButtons.YesNo,
						    MessageBoxIcon.Question) == DialogResult.No)
					{
						return;
					}
				}

				var result = _aspectCalc.CalculateLink(_aspectCalc.FindAspect(listView1.SelectedItems[0].ImageKey),
					_aspectCalc.FindAspect(listView2.SelectedItems[0].ImageKey), step, exceptedAspects);

				if (result.Count > 0)
				{
					var f2 = new Form2(_aspectCalc, result, step, imageList1.Images, _astip);
					f2.Show();
				}
				else
				{
					if (MessageBox.Show(Resources.CannotFindLink, Resources.CannotFindLink_, MessageBoxButtons.YesNo,
						    MessageBoxIcon.Question) == DialogResult.Yes)
					{
						++step;
						textBox1.Text = step.ToString(CultureInfo.InvariantCulture);
						continue;
					}
				}

				break;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("Readme.txt");
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			label2.Text = Resources.SelectedUp +
			              (listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0].Text : Resources.NullAspect);
		}

		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
			label3.Text = Resources.SelectedDown +
			              (listView2.SelectedItems.Count > 0 ? listView2.SelectedItems[0].Text : Resources.NullAspect);
		}

		private bool _isFold = true;

		private void button3_Click(object sender, EventArgs e)
		{
			if (_isFold)
			{
				if (ActiveForm != null)
					ActiveForm.Height = 600;
				button3.Text = Resources.CloseAdvance;
				_isFold = false;
			}
			else
			{
				if (ActiveForm != null)
					ActiveForm.Height = 380;
				button3.Text = Resources.OpenAdvance;
				_isFold = true;
			}
		}

		private void listView3_SelectedIndexChanged(object sender, EventArgs e)
		{
			label5.Text = Resources.SelectedAspect + (listView3.SelectedItems.Count > 0
				              ? string.Join(" ", listView3.SelectedItems.Cast<ListViewItem>().Select(i => i.Text))
				              : Resources.NullAspect);

			toolTip1.SetToolTip(label5, label5.Text);
		}

		private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
		{
			// 什么都没有
			// 为什么什么都没有还要弄这个方法？
			// 因为我点错了【
		}

		private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
		{
			// 同上
		}

		private Point _pointView = new Point(0, 0); //定义外部存储变量
		private frmAstip _astip;

		private void ShowAspectTip(ListView listView, MouseEventArgs e)
		{
			if (ActiveForm != FindForm())
			{
				return;
			}

			var lv = listView.GetItemAt(e.X, e.Y);
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
							_astip.Location = new Point(e.X + listView.Left + ActiveForm.Left + 10,
								e.Y + listView.Top + ActiveForm.Top - 70);

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

		private void listView1_MouseMove(object sender, MouseEventArgs e)
		{
			ShowAspectTip(listView1, e);
		}

		private void listView1_MouseLeave(object sender, EventArgs e)
		{
			_astip.Visible = false;
		}

		private void label6_Click(object sender, EventArgs e)
		{
			int.TryParse(textBox1.Text, out var step);
			if (step - 1 > 0)
			{
				textBox1.Text = (step - 1).ToString(CultureInfo.InvariantCulture);
			}
		}

		private void label7_Click(object sender, EventArgs e)
		{
			int.TryParse(textBox1.Text, out var step);
			textBox1.Text = (step + 1).ToString(CultureInfo.InvariantCulture);
		}

		private void listView1_KeyPress(object sender, KeyPressEventArgs e)
		{
			listView1.FocusedItem = (from ListViewItem item in listView1.Items
				                        where !_basicFirst || !_aspectCalc.FindAspect(item.Text).IsBasicAspect
				                        where item.Text[0] == e.KeyChar
				                        select item).FirstOrDefault() ?? listView1.FocusedItem;
		}

		private void listView2_KeyPress(object sender, KeyPressEventArgs e)
		{
			listView2.FocusedItem = (from ListViewItem item in listView2.Items
				                        where !_basicFirst || !_aspectCalc.FindAspect(item.Text).IsBasicAspect
				                        where item.Text[0] == e.KeyChar
				                        select item).FirstOrDefault() ?? listView2.FocusedItem;
		}

		private void listView2_MouseMove(object sender, MouseEventArgs e)
		{
			ShowAspectTip(listView2, e);
		}

		private void listView2_MouseLeave(object sender, EventArgs e)
		{
			_astip.Visible = false;
		}
	}
}
