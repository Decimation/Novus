using System;
using System.Collections.Generic;
using System.Diagnostics;
using Novus.Memory;
using Novus.Utilities;

#nullable enable
namespace Novus
{
	public class Resource
	{
		public string Name { get; }

		public ProcessModule Module { get; }

		public SigScanner Scanner { get; }

		public Dictionary<string, Pointer<byte>> Map { get; }

		

		public Resource(string name)
		{
			Name = name;
			
			var module = Mem.FindModule(name);

			Module = module ?? throw new NullReferenceException();

			Scanner = new SigScanner(Module);

			Map = new Dictionary<string, Pointer<byte>>();
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Module.ModuleName, Scanner.Address);
		}
	}
}