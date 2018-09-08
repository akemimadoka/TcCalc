using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using TcCalc.Properties;

namespace TcCalc
{
	public static class UtilityExtensions
	{
		public static TR Let<T, TR>(this T obj, Func<T, TR> func)
		{
			return func(obj);
		}

		public static void Let<T>(this T obj, Action<T> func)
		{
			func(obj);
		}

		public static T Apply<T>(this T obj, Action<T> func)
		{
			func(obj);
			return obj;
		}

		public delegate void ApplyDelegate<T>(ref T obj);

		public static T Apply<T>(this T obj, ApplyDelegate<T> func)
		{
			func(ref obj);
			return obj;
		}
	}

// 很早以前写的了，求别吐槽，求别吐槽，求别吐槽 orz
	public class Setting
	{
		public Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();

		public Setting(string settingFile)
		{
			using (var objReader = new StreamReader(settingFile))
			{
				while (!objReader.EndOfStream)
				{
					var readLine = objReader.ReadLine();
					if (readLine == null)
					{
						continue;
					}

					var line = readLine.ToLower();
					if (!line.StartsWith("@"))
					{
						continue;
					}

					var equalPos = line.IndexOf('=');
					if (equalPos == -1)
					{
						throw new ArgumentException(Resources.InvalidSettingFile, nameof(settingFile));
					}
					Settings.Add(line.Substring(1, equalPos - 1), line.Substring(equalPos + 1));
				}
			}
		}
	}

	public class AspectCalc
	{
		public string Version { get; }
		public int Change { get; }

		public class Aspect
		{
			public string Name { get; }
			public string Translation { get; }

			public string FullName => $"{Name}({Translation})";

			public (string, string)? Recipe { get; }
			public bool IsBasicAspect => Recipe == null;

			public Aspect(string name, string translation, (string, string)? recipe = null)
			{
				Contract.Assert(!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(translation));
				Contract.Assert(recipe?.Let(r => r.Item1 != null && r.Item2 != null) ?? true);

				Name = name;
				Translation = translation;
				Recipe = recipe;
			}
		}

		private readonly Dictionary<string, (Aspect, HashSet<Aspect>)> _aspects = new Dictionary<string, (Aspect, HashSet<Aspect>)>();

		//计算元素连接表
		private void CalcAspectLinkList()
		{
			foreach (var (aspect, linkList) in _aspects.Values)
			{
				Contract.Assert(aspect != null && linkList != null);

				aspect.Recipe?.Let(r =>
				{
					var recipeAspect = (_aspects[r.Item1], _aspects[r.Item2]);
					linkList.Add(recipeAspect.Item1.Item1);
					linkList.Add(recipeAspect.Item2.Item1);
					recipeAspect.Item1.Item2.Add(aspect);
					recipeAspect.Item2.Item2.Add(aspect);
				});
			}
		}

		private static IEnumerable<(int, string)> ReadLines(TextReader streamReader)
		{
			var lineCount = 1;
			string line;
			while ((line = streamReader.ReadLine()) != null)
			{
				yield return (lineCount++, line);
			}
		}

		//加载元素描述文件
		public AspectCalc(string aspectFile)
		{
			using (var objReader = new StreamReader(aspectFile))
			{
				foreach (var (lineCount, line) in ReadLines(objReader))
				{
					if (line.Equals(""))
					{
						continue;
					}

					switch (line.First())
					{
					case '#':
					case '⑨':
						continue;
					case '@':
						var command = line.Substring(line.IndexOf('@') + 1);
						command = command.ToLower();
						var equalPos = command.IndexOf('=');
						if (command.StartsWith("version"))
						{
							Version = command.Substring(equalPos + 1);
						}
						else if (command.StartsWith("change"))
						{
							if (int.TryParse(command.Substring(equalPos + 1), out var tmpChange))
							{
								Change = tmpChange;
							}
						}

						continue;
					}

					var aspectDescription = line.Split(' ');
					switch (aspectDescription.Length)
					{
					case 4:
						_aspects.Add(aspectDescription[0],
							(new Aspect(aspectDescription[0], aspectDescription[1], (aspectDescription[2], aspectDescription[3])),
								new HashSet<Aspect>()));
						break;
					case 2:
						_aspects.Add(aspectDescription[0], (new Aspect(aspectDescription[0], aspectDescription[1]), new HashSet<Aspect>()));
						break;
					default:
						throw new ArgumentException(string.Format(Resources.InvalidAspectDescription, lineCount), nameof(aspectFile));
					}
				}
			}

			CalcAspectLinkList();
		}

		public Aspect FindAspect(string name)
		{
			_aspects.TryGetValue(name, out var aspect);
			return aspect.Item1;
		}

		//判断是否可连接
		private bool IsLinkable(Aspect fromAspect, Aspect toAspect, ICollection<Aspect> exceptedAspects = null)
		{
			return (exceptedAspects == null ||
			        (!exceptedAspects.Contains(fromAspect) && !exceptedAspects.Contains(toAspect))) &&
			       _aspects[fromAspect.Name].Item2.Contains(toAspect);
		}

		//用递归计算连接方式
		private void CalculateLinkImpl(ICollection<List<Aspect>> allLinks, IList<Aspect> currentLink, ICollection<Aspect> exceptedAspects, Aspect toAspect, int depth)
		{
			var fromAspect = currentLink[currentLink.Count - 1];

			if (depth == 0)
			{
				foreach (var linkableAspect in _aspects[fromAspect.Name].Item2)
				{
					if (!IsLinkable(linkableAspect, toAspect, exceptedAspects))
					{
						continue;
					}

					allLinks.Add(new List<Aspect>(currentLink){ linkableAspect, toAspect });
				}
			}
			else if (depth > 0)
			{
				foreach (var linkableAspect in _aspects[fromAspect.Name].Item2)
				{
					if (exceptedAspects.Contains(linkableAspect))
					{
						continue;
					}

					CalculateLinkImpl(allLinks, new List<Aspect>(currentLink) { linkableAspect }, exceptedAspects, toAspect, depth - 1);
				}
			}
			else
			{
				throw new Exception(Resources.CalculationError);
			}
		}

		//计算连接方式
		public HashSet<List<Aspect>> CalculateLink(Aspect fromAspect, Aspect toAspect, int minWays, IEnumerable<Aspect> exceptedAspects)
		{
			var result = new HashSet<List<Aspect>>();
			CalculateLinkImpl(result, new List<Aspect> { fromAspect }, new HashSet<Aspect>(exceptedAspects), toAspect, minWays - 1);
			return result;
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
