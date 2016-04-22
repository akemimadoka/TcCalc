using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Collections;
using Tccalc.Properties;

namespace Tccalc
{
    // 很早以前写的了，求别吐槽，求别吐槽，求别吐槽 orz
    public class Setting
    {
        public List<string> Settings { get; } = new List<string>();

        public Setting(string settingfile)
        {
            StreamReader objReader = new StreamReader(settingfile);

            while (!objReader.EndOfStream)
            {
                var readLine = objReader.ReadLine();
                if (readLine == null)
                    continue;
                string tmpline = readLine.ToLower();
                if (tmpline.StartsWith("@"))
                {
                    Settings.Add(tmpline.Split('@')[1]);
                }
            }
        }
    }
    public class AspectCalc
    {
        public static string Version { get; private set; }

        public static int Change { get; private set; }

        private static readonly List<List<string>> Aspects = new List<List<string>>();
        private static readonly List<string> BasicAspects = new List<string>();
        private static bool _hasLoaded;

        //验证元素计算是否出错
        //已被移除，仅供参考
        /*
        private bool VerifyAspects()
        {
            foreach (List<string> x in Aspects)
            {
                if (!Aspects[FindAspectFromStr(x[1])].Contains(x[0]) ||
                    !Aspects[FindAspectFromStr(x[2])].Contains(x[0]))
                {
                    MessageBox.Show("数据校验错误 " + x[0]);
                    return false;
                }
            }

            return true;
        }
        */

        //计算元素连接表
        private void CalcAspectLinkList()
        {
            for (int asindex = 0; asindex < Aspects.Count; ++asindex)
            {
                switch (Aspects[asindex].Count)
                {
                    case 3:
                        for (int index = 1; index < 3; ++index)
                        {
                            if (!Aspects[FindAspectFromStr(Aspects[asindex][index])].Contains(Aspects[asindex][0]))
                            {
                                Aspects[FindAspectFromStr(Aspects[asindex][index])].Add(Aspects[asindex][0]);
                            }
                        }
                        break;
                    case 1:
                        if (!BasicAspects.Contains(Aspects[asindex][0]))
                        {
                            BasicAspects.Add(Aspects[asindex][0]);
                        }
                        continue;
                    default:
                        Aspects.Remove(Aspects[asindex]);
                        break;
                }
            }
        }
        //加载元素描述文件
        public AspectCalc(string aspectfile)
        {
            if (_hasLoaded)
                return;
            Aspects.Clear();
            List<string> tmpAspects = new List<string>();
            string tmpAsp = "";
            StreamReader objReader = new StreamReader(aspectfile);

            while (tmpAsp != null && !objReader.EndOfStream)
            {
                tmpAsp = objReader.ReadLine();
                if (tmpAsp == null || tmpAsp.Equals(""))
                    continue;
                if (tmpAsp[0] == '#' || tmpAsp[0] == '⑨')
                {
                    continue;
                }
                if (tmpAsp[0] == '@')
                {
                    string command = tmpAsp.Split('@')[1];
                    command = command.ToLower();
                    if (command.StartsWith("version"))
                    {
                        Version = command.Split('=')[1];
                    }
                    else if (command.StartsWith("change"))
                    {
                        int tmpChange;
                        if (int.TryParse(command.Split('=')[1], out tmpChange))
                        {
                            Change = tmpChange;
                        }
                    }

                    continue;
                }
                tmpAspects.AddRange(tmpAsp.Split(' '));

                Aspects.Add(new List<string>(tmpAspects));
                tmpAspects.Clear();
            }

            objReader.Close();

            CalcAspectLinkList();

            /*
            if (!VerifyAspects())
            {
                MessageBox.Show("元素文件无效！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */

            _hasLoaded = true;
        }
        // 刷新元素描述文件
        // 暂时无效
        // 谁帮我改下啊【
        public void RefreshAspects(string filename)
        {
            Aspects.Clear();
            List<string> tmpAspects = new List<string>();
            string tmpAsp = "";
            StreamReader objReader = new StreamReader(filename);

            while (tmpAsp != null && !objReader.EndOfStream)
            {
                tmpAsp = objReader.ReadLine();
                if (tmpAsp == null || tmpAsp.Equals(""))
                    continue;
                switch (tmpAsp[0])
                {
                    case '#':
                    case '⑨':
                        continue;
                    case '@':
                        string command = tmpAsp.Split('@')[1];
                        command = command.ToLower();
                        if (command.StartsWith("version"))
                        {
                            Version = command.Split('=')[1];
                        }
                        else if (command.StartsWith("change"))
                        {
                            int tmpChange;
                            if (int.TryParse(command.Split('=')[1], out tmpChange))
                            {
                                Change = tmpChange;
                            }
                        }

                        continue;
                }
                tmpAspects.AddRange(tmpAsp.Split(' '));

                Aspects.Add(new List<string>(tmpAspects));
                tmpAspects.Clear();
            }

            objReader.Close();

            CalcAspectLinkList();
        }
        //从元素ID获得元素名
        public string GetAspectNameFromId(int id)
        {
            return Aspects[id][0];
        }
        //从元素名查找元素ID
        public int FindAspectFromStr(string name)
        {
            for(int index = 0;index < Aspects.Count;++index)
            {
                if(Aspects[index][0].Contains(name) || name.Contains(Aspects[index][0]))
                {
                    return index;
                }
            }

            return -1;
        }
        //标准化元素名称
        public string ParseAspectName(string aspectName)
        {
            return GetAspectNameFromId(FindAspectFromStr(aspectName));
        }
        //判断是否为基本元素
        public bool IsBasicAspect(int aspectId)
        {
            return IsBasicAspect(GetAspectNameFromId(aspectId));
        }
        //判断是否为基本元素
        public bool IsBasicAspect(string aspectName)
        {
            return BasicAspects.Contains(ParseAspectName(aspectName));
        }
        public int BasicAspectCount => BasicAspects.Count;
        //获得该元素的合成
        public List<string> GetAspectRecipe(int aspectId)
        {
            if (IsBasicAspect(aspectId))
                return null;
            List<string> retRecipe = new List<string> {Aspects[aspectId][1], Aspects[aspectId][2]};

            return new List<string>(retRecipe);
        }
        //获得该元素的合成
        public List<string> GetAspectRecipe(string aspectName)
        {
            return GetAspectRecipe(FindAspectFromStr(aspectName));
        }

        //排除元素表
        private List<int> _exceptAspects;
        //判断是否可连接
        private bool IsLinkAvailable(int @from, int to)
        {
            if(_exceptAspects.Contains(@from) || _exceptAspects.Contains(to))
            {
                return false;
            }

            if(@from < 0 || to < 0 || @from >= Aspects.Count || to >= Aspects.Count)
            {
                return false;
            }

            for(int i = 1;i<Aspects[@from].Count;++i)
            {
                if(Aspects[@from][i].Equals(Aspects[to][0]))
                {
                    return true;
                }
            }

            return false;
        }
        //判断是否可连接
        //并没有用到
/*
        private bool IsLinkAvailable(string @from, string to)
        {
            return IsLinkAvailable(FindAspectFromStr(@from), FindAspectFromStr(to));
        }
*/

        //全部连接方式
        private readonly List<List<string>> _tmpCalcWays = new List<List<string>>();
        //单个连接方式
        private readonly List<string> _tmpCalcSWay = new List<string>();

        static AspectCalc()
        {
            Change = 0;
            Version = null;
        }

        //用递归计算连接方式
        private void TCalc(int aspect, int depth, int faspect)
        {
            _tmpCalcSWay.Add(Aspects[aspect][0]);
            List<string> tempw = new List<string>(_tmpCalcSWay);

            if (depth == 0)
            {
                for (int tmpasp = 1; tmpasp < Aspects[aspect].Count; ++tmpasp)
                {
                    if (!IsLinkAvailable(FindAspectFromStr(Aspects[aspect][tmpasp]), faspect))
                        continue;
                    tempw.Add(Aspects[aspect][tmpasp]);
                    tempw.Add(Aspects[faspect][0]);
                    _tmpCalcWays.Add(new List<string>(tempw));
                    tempw = new List<string>(_tmpCalcSWay);
                }
            }
            else if(depth > 0)
            {
                for(int tmpasp = 1;tmpasp < Aspects[aspect].Count;++tmpasp)
                {
                    if (_exceptAspects.Contains(FindAspectFromStr(Aspects[aspect][tmpasp])))
                    {
                        continue;
                    }

                    TCalc(FindAspectFromStr(Aspects[aspect][tmpasp]), depth - 1, faspect);
                    _tmpCalcSWay.RemoveAt(_tmpCalcSWay.Count - 1);
                }
            }
            else if(depth < 0)
            {
                MessageBox.Show(Resources.calcerror, Resources.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //计算连接方式
        public List<List<string>> Calc(int start, int end, int minWays, List<int> except)
        {
            _tmpCalcWays.Clear();
            _tmpCalcSWay.Clear();
            _exceptAspects = new List<int>(except);
            TCalc(start, minWays - 1 , end);
            return _tmpCalcWays;
        }
    }
    // 自动升级类
    // 其实没测试过
    public class AutoUpdate
    {
        private static string _updateUrl;
        public AutoUpdate(string url)
        {
            _updateUrl = url;
        }
        public List<string> GetRawVersionInfo()
        {
            List<string> rawverinfo = new List<string>();
            Uri u = new Uri(_updateUrl);
            HttpWebRequest twr = (HttpWebRequest)WebRequest.Create(u);
            twr.Method = "GET";
            twr.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse wr = (HttpWebResponse)twr.GetResponse();

            Stream sIn = wr.GetResponseStream();
            if (sIn == null)
                return rawverinfo;
            StreamReader objReader = new StreamReader(sIn);

            while (!objReader.EndOfStream)
            {
                string tmpline = objReader.ReadLine();
                if (tmpline != null && !tmpline.Equals(""))
                {
                    rawverinfo.Add(tmpline);
                }
            }

            return rawverinfo;
        }
        public void DownloadFile(string url, string filename)
        {
            Uri u = new Uri(url);
            HttpWebRequest twr = (HttpWebRequest)WebRequest.Create(u);
            twr.Method = "GET";
            twr.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse wr = (HttpWebResponse)twr.GetResponse();

            Stream sIn = wr.GetResponseStream();
            if (sIn == null)
                return;
            StreamReader sRead = new StreamReader(sIn);
            StreamWriter sWrite = new StreamWriter(filename);

            while (!sRead.EndOfStream)
            {
                sWrite.Write(sRead.ReadToEnd());
            }
        }
    }

    // 本来是给结果窗口的列表排序的
    // 一不小心把工程弄坏了，所以没用到了
    // 谁有时间试试吧_(:з」∠)_
    public class AspectResultSort : IComparer
    {
        private static AspectCalc _asCalc;
        private static List<List<string>> _result;
        private static ListBox.ObjectCollection _objects;
        public AspectResultSort(AspectCalc ascalc, List<List<string>> result, ListBox.ObjectCollection objects)
        {
            _asCalc = ascalc;
            _result = result;
            _objects = objects;
        }
        private static int AspectCount(int index)
        {
            List<string> temp = new List<string>();
            foreach (string x in _result[index].Where(x => !temp.Contains(x)))
            {
                temp.Add(x);
            }

            return temp.Count;
        }
        public int Compare(object x, object y)
        {
            int counta = AspectCount(_objects.IndexOf(x));
            int countb = AspectCount(_objects.IndexOf(y));

            if (counta < countb)
            {
                return -1;
            }

            return counta == countb ? 0 : 1;
        }
    }

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
