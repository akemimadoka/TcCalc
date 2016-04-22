using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tccalc.Properties;

namespace Tccalc
{
    public partial class Form2 : Form
    {
        private readonly List<List<string>> _asresult;
        private static readonly AspectCalc Tascalc = new AspectCalc("Aspects.txt");

        private static string ParseFilename(string filename)
        {
            string[] tmpfile = filename.Split('\\');
            string retfilename = tmpfile[tmpfile.Length - 1];
            tmpfile = retfilename.Split('.');
            retfilename = tmpfile[0];

            return retfilename;
        }
        public Form2(ICollection<List<string>> result, int step, ImageList.ImageCollection asimagelist)
        {
            if (result.Count <= 0)
                throw new ArgumentException(Resources.noresult, nameof(result));

            _asresult = new List<List<string>>(result);

            string tmpresult = "";
            InitializeComponent();
            for (int i = 0;i < asimagelist.Count;++i)
            {
                imageList1.Images.Add(Tascalc.GetAspectNameFromId(Tascalc.FindAspectFromStr(ParseFilename(asimagelist.Keys[i]))), asimagelist[i]);
            }
            Text = $"共有 {result.Count} 组结果，步数为 {step}";
            
            foreach(List<string> x in result)
            {
                tmpresult = x.Aggregate(tmpresult, (current, y) => current + (y + " "));
                listBox1.Items.Add(tmpresult);
                tmpresult = "";
            }
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Clear();
            if (listBox1.SelectedItems.Count > 0)
            {
                foreach (string x in _asresult[listBox1.SelectedIndex])
                {
                    listView1.Items.Add(x, x);
                }
            }
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

        private string FindAspect(string asname)
        {
            return imageList1.Images.Keys.Cast<string>().FirstOrDefault(asname.Contains);
        }

        private Point _pointView = new Point(0, 0);//定义外部存储变量
        private readonly frmAstip _astip = new frmAstip();
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ActiveForm != FindForm())
                return;
            ListViewItem lv = listView1.GetItemAt(e.X, e.Y);
            if (lv != null)
            {
                if (!Tascalc.IsBasicAspect(lv.Text))
                {
                    if (_pointView.X != e.X || _pointView.Y != e.Y)//防止tooltip不停闪烁问题
                    {
                        List<string> tmpAsname = new List<string> {lv.Text};
                        tmpAsname.AddRange(Tascalc.GetAspectRecipe(Tascalc.FindAspectFromStr(lv.Text)));
                        List<Image> tmpImage = tmpAsname.Select(x => imageList1.Images.ContainsKey(FindAspect(x)) ? imageList1.Images[imageList1.Images.Keys.IndexOf(FindAspect(x))] : null).ToList();

                        _astip.UpdateAstip(tmpImage, tmpAsname);
                        if (ActiveForm != null)
                            _astip.Location = new Point(e.X + ActiveForm.Left + 30, e.Y + ActiveForm.Top + 100);

                        _astip.Visible = true;
                        Focus();
                    }
                }
            }
            else
            {
                _astip.Visible = false;//没有取到item自动隐藏
            }
            _pointView = new Point(e.X, e.Y);
        }

        private void listView1_MouseLeave(object sender, EventArgs e)
        {
            _astip.Visible = false;
        }
    }
}
