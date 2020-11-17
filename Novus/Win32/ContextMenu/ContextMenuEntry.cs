using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32.ContextMenu
{
	public class ContextMenuEntry
	{
		public ContextMenuKey Base { get; set; }

		public ContextMenuKey Command { get; set; }


		public string[] ToRegistry()
		{

			var rg = new List<string>();

			rg.AddRange(Base.ToRegistry());
			
			rg.AddRange(Command.ToRegistry());
			
			return rg.ToArray();
		}
	}
}
