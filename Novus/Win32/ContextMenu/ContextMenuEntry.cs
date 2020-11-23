using System.Collections.Generic;

namespace Novus.Win32.ContextMenu
{ 
	// WIP: WIP
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
