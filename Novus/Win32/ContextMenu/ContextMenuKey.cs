using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Novus.Utilities;

#nullable enable
namespace Novus.Win32.ContextMenu
{
	public class ContextMenuKey
	{
		public string Root { get; set; }

		public string? Main { get; set; }

		// ...

		public Dictionary<string, object> Values { get; set; }

		public ContextMenuKey(string root)
		{
			Root   = root;
			Values = new Dictionary<string, object>();
		}

		private static string GetValStr(object o)
		{
			var t = o.ToString();

			if (t.StartsWith("dword")) {


				return t;
			}


			return $"\"{o}\"";
		}

		public string[] ToRegistry()
		{
			var rg = new List<string>()
			{
				$"[{Root}]",
			};

			if (!string.IsNullOrWhiteSpace(Main)) {
				rg.Add($"@=\"{Main}\"");
			}

			if (Values != null) {
				foreach (var value in Values) {


					string valStr = GetValStr(value.Value);


					rg.Add($"\"{value.Key}\"={valStr}");
				}
			}


			rg.Add("\n");

			return rg.ToArray();
		}
	}
}