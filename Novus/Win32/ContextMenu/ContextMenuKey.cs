using System.Collections.Generic;

#nullable enable
namespace Novus.Win32.ContextMenu
{
	// WIP: WIP
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