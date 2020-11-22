using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Novus.Win32.ContextMenu
{
	// WIP: WIP

	public class CascadingContextMenuEntry
	{
		public ContextMenuKey Base { get; }

		public List<ContextMenuEntry> Items { get; }

		private const string shell = @"HKEY_CLASSES_ROOT\*\shell\";

		public string Name { get; }

		public CascadingContextMenuEntry(string kn)
		{
			Name = Path.Combine(shell, kn);
			Base = new ContextMenuKey(Name);


			Base.Values.Add("subcommands", "");

			Items = new List<ContextMenuEntry>
			{
				new ContextMenuEntry()
				{
					Base = new ContextMenuKey(GetName().Item1)
					{
						Main   = "My main action",
						Values = new Dictionary<string, object>() {{"CommandFlags", "dword:00000040"},}
					},
					Command = new ContextMenuKey(GetName().Item2)
					{
						Main = @"C:\\Users\\Deci\\Desktop\\SmartImage.exe " + "\\\"%1\\\""
					}
				}
			};

		}

		public ContextMenuEntry GetStub()
		{
			var e = new ContextMenuEntry()
			{
				Base = new ContextMenuKey(GetName().Item1)
					{ },
				Command = new ContextMenuKey(GetName().Item2)
					{ }
			};

			return e;
		}

		public (string,string) GetName()
		{
			string s;

			s = Path.Combine(Name, "shell", $"{(Items == null ? 0 : Items.Count)}_action");

			

			return (s,Path.Combine(s,"command"));
		}


		public string[] ToRegistry()
		{
			var rg = new List<string>()
				{ };

			rg.AddRange(Base.ToRegistry());

			foreach (var item in Items) {
				rg.AddRange(item.ToRegistry());
			}

			return rg.ToArray();
		}
	}
}