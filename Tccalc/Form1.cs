using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Tccalc.Properties;

namespace Tccalc
{
    public partial class Form1 : Form
    {
        private readonly AspectCalc _ascalc = new AspectCalc("Aspects.txt");
        // 前略，天国的主机m(_ _)m
        // 炸了，谁有主机帮我续个呗【不
        private readonly AutoUpdate _update = new AutoUpdate("http://homuhomu.web3v.net/aspectupdate.html");
        private readonly Setting _settings = new Setting("Settings.ini");

        private List<string> _rawupdatedata = new List<string>();
        private bool _basicfirst = true;
        private bool _checkupdate = true;
        public Form1()
        {
            InitializeComponent();
        }

        private static void GetAllFileByDir(string dirPath, List<string> fl)
        {
            fl.AddRange(Directory.GetFiles(dirPath));

            foreach (string dir in Directory.GetDirectories(dirPath))
            {
                GetAllFileByDir(dir, fl);
            }
        }

        private static string ParseFilename(string filename)
        {
            return filename.Substring(filename.LastIndexOf('\\') + 1, filename.LastIndexOf('.') - filename.LastIndexOf('\\') - 1);
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> settings = _settings.Settings;

            if (AspectCalc.Version != null)
            {
                label8.Text = Resources.asver + AspectCalc.Version;
            }
            foreach (string x in settings)
            {
                if (x.StartsWith("enablewallpaper"))
                {
                    if (x.EndsWith("false"))
                    {
                        _astip.DisableWallpaper();
                    }
                }
                else if (x.StartsWith("basicfirst"))
                {
                    if (x.EndsWith("false"))
                    {
                        _basicfirst = false;
                    }
                }
                else if (x.StartsWith("checkupdate"))
                {
                    if (x.EndsWith("false"))
                    {
                        _checkupdate = false;
                    }
                }
            }

            if (_checkupdate)
            {
                // 未测试
                _rawupdatedata = _update.GetRawVersionInfo();
                foreach (string x in from x in _rawupdatedata
                    where x.StartsWith("@")
                    where x.Split('@')[1] == AspectCalc.Version
                    let tmpChange = int.Parse(_rawupdatedata[_rawupdatedata.IndexOf(x) + 1])
                    where tmpChange > AspectCalc.Change
                    where _rawupdatedata[_rawupdatedata.IndexOf(x) + 2] != "null"
                    where MessageBox.Show(Resources.updateavailable, Resources.prompt, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes
                    select x)
                {
                    _update.DownloadFile(_rawupdatedata[_rawupdatedata.IndexOf(x) + 2], "Aspects.txt");
                    MessageBox.Show(Resources.updatecompleted, Resources.success);
                }
            }

            List<string> filelist = new List<string>();
            GetAllFileByDir(@"pictures\color\", filelist);
            foreach (string filename in filelist)
            {
                if (_ascalc.FindAspectFromStr(ParseFilename(filename)) > -1)
                {
                    imageList1.Images.Add(ParseFilename(filename), Image.FromFile(filename));
                    listView1.Items.Add(new ListViewItem(_ascalc.ParseAspectName(ParseFilename(filename)), ParseFilename(filename)));
                }
                else
                {
                    listView1.Items.Add(_ascalc.ParseAspectName(ParseFilename(filename)), null);
                }
            }

            if (_basicfirst)
            {
                List<ListViewItem> basicaspect = new List<ListViewItem>();

                foreach (ListViewItem item in from ListViewItem item in listView1.Items let asindex = _ascalc.FindAspectFromStr(item.Text) where _ascalc.IsBasicAspect(asindex) select item)
                {
                    basicaspect.Add(item);
                    //ListViewItem tmpitem = (ListViewItem)item.Clone();
                    listView1.Items.Remove(item);
                }
                if (basicaspect.Count > 0)
                {
                    for (int index = 0; index < basicaspect.Count; ++index)
                    {
                        listView1.Items.Insert(index, basicaspect[index]);
                    }
                }
                // <Bug Fix>
                List<ListViewItem> bugfix = (from ListViewItem item in listView1.Items select (ListViewItem) item.Clone()).ToList();
                listView1.Clear();
                foreach (ListViewItem item in bugfix)
                {
                    listView1.Items.Add(item);
                }
            }

            foreach (ListViewItem item in listView1.Items)
            {
                listView2.Items.Add((ListViewItem)item.Clone());
                listView3.Items.Add((ListViewItem)item.Clone());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            while (true)
            {
                int step;

                List<int> except = new List<int>();
                if (listView3.SelectedItems.Count > 0)
                {
                    except.AddRange(from ListViewItem x in listView3.SelectedItems select _ascalc.FindAspectFromStr(x.Text));
                }

                // 异常处理部分
                // ↑哈？
                if (textBox1.Text.Equals(""))
                {
                    MessageBox.Show(Resources.steprequested, Resources.prompt, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (listView1.SelectedItems.Count != 1 || listView2.SelectedItems.Count != 1)
                {
                    MessageBox.Show(Resources.checkas, Resources.prompt, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (listView3.SelectedItems.Cast<ListViewItem>().Any(x => x.Text == listView1.SelectedItems[0].Text || x.Text == listView2.SelectedItems[0].Text))
                {
                    MessageBox.Show(Resources.startendasrequested, Resources.prompt, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int.TryParse(textBox1.Text, out step);
                if (step < 0)
                {
                    MessageBox.Show(Resources.stepshouldbepositivenum, Resources.prompt, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (step > 9)
                {
                    if (MessageBox.Show(Resources.steptoobig, Resources.prompt, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }
                }
                List<List<string>> result = _ascalc.Calc(_ascalc.FindAspectFromStr(listView1.SelectedItems[0].Text), _ascalc.FindAspectFromStr(listView2.SelectedItems[0].Text), step, except);

                if (result.Count > 0)
                {
                    Form2 f2 = new Form2(new List<List<string>>(result), step, imageList1.Images);
                    f2.Show();
                }
                else
                {
                    if (MessageBox.Show(Resources.cannotfindlink, Resources.cannotfindlink_, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
            if (listView1.SelectedItems.Count > 0)
            {
                label2.Text = Resources.selectedup + listView1.SelectedItems[0].Text;
            }
            else
            {
                label2.Text = Resources.selectedup + Resources.nullas;
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                label3.Text = Resources.selecteddown + listView2.SelectedItems[0].Text;
            }
            else
            {
                label3.Text = Resources.selecteddown + Resources.nullas;
            }
        }

        private bool _isFold = true;
        private void button3_Click(object sender, EventArgs e)
        {
            if (_isFold)
            {
                if (ActiveForm != null) 
                    ActiveForm.Height = 600;
                button3.Text = Resources.closead;
                _isFold = false;
            }
            else
            {
                if (ActiveForm != null) 
                    ActiveForm.Height = 380;
                button3.Text = Resources.openad;
                _isFold = true;
            }
        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            label5.Text = Resources.selectedas;

            if (listView3.SelectedItems.Count > 0)
            {
                foreach (ListViewItem x in listView3.SelectedItems)
                {
                    label5.Text += x.Text + @" ";
                }
            }
            else
            {
                label5.Text += Resources.nullas;
            }

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
                if (!_ascalc.IsBasicAspect(lv.Text))
                {
                    if (_pointView.X != e.X || _pointView.Y != e.Y)//防止tooltip不停闪烁问题
                    {
                        List<string> tmpAsname = new List<string> {lv.Text};
                        tmpAsname.AddRange(_ascalc.GetAspectRecipe(_ascalc.FindAspectFromStr(lv.Text)));
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

        private void label6_Click(object sender, EventArgs e)
        {
            int tmpstep = 3;
            int.TryParse(textBox1.Text, out tmpstep);
            if (tmpstep - 1 > 0)
            {
                textBox1.Text = (tmpstep - 1).ToString(CultureInfo.InvariantCulture);
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            int tmpstep = 3;
            int.TryParse(textBox1.Text, out tmpstep);
            textBox1.Text = (tmpstep + 1).ToString(CultureInfo.InvariantCulture);
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (ListViewItem item in from ListViewItem item in listView1.Items where !_basicfirst || !_ascalc.IsBasicAspect(item.Text) where item.Text[0] == e.KeyChar select item)
            {
                listView1.FocusedItem = item;
                break;
            }
        }

        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (ListViewItem item in from ListViewItem item in listView2.Items where !_basicfirst || !_ascalc.IsBasicAspect(item.Text) where item.Text[0] == e.KeyChar select item)
            {
                listView2.FocusedItem = item;
                break;
            }
        }

        // 是的我基本只是复制了一遍上面的代码【
        private void listView2_MouseMove(object sender, MouseEventArgs e)
        {
            if (ActiveForm != FindForm())
                return;
            ListViewItem lv = listView2.GetItemAt(e.X, e.Y);
            if (lv != null)
            {
                if (!_ascalc.IsBasicAspect(lv.Text))
                {
                    if (_pointView.X != e.X || _pointView.Y != e.Y)//防止tooltip不停闪烁问题
                    {
                        List<string> tmpAsname = new List<string> {lv.Text};
                        tmpAsname.AddRange(_ascalc.GetAspectRecipe(_ascalc.FindAspectFromStr(lv.Text)));
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

        private void listView2_MouseLeave(object sender, EventArgs e)
        {
            _astip.Visible = false;
        }
    }
}
