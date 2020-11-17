using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32.ContextMenu
{
	public class CascadingContextMenuEntry
	{
		public ContextMenuKey Base { get; init; }

		public ContextMenuEntry[] Items { get; init; }


		public string[] ToRegistry()
		{
			var rg = new List<string>()
			{
				
			};

			rg.AddRange(Base.ToRegistry());

			foreach (var item in Items) {
				rg.AddRange(item.ToRegistry());
			}

			return rg.ToArray();
		}
	}
}