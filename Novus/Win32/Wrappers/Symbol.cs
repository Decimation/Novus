using System;
using Novus.Win32.Structures;

namespace Novus.Win32.Wrappers
{
	public unsafe class Symbol
	{
		internal Symbol(DebugSymbol* pSymInfo)
		{
			Name = pSymInfo->ReadSymbolName();

			SizeOfStruct = (int) pSymInfo->SizeOfStruct;
			TypeIndex    = (int) pSymInfo->TypeIndex;
			Index        = (int) pSymInfo->Index;
			Size         = (int) pSymInfo->Size;
			ModBase      = pSymInfo->ModBase;
			Flags        = pSymInfo->Flags;
			Value        = pSymInfo->Value;
			Address      = pSymInfo->Address;
			Register     = (int) pSymInfo->Register;
			Scope        = (int) pSymInfo->Scope;
			Tag          = pSymInfo->Tag;
		}


		public string Name { get; }

		public int SizeOfStruct { get; }

		public int TypeIndex { get; }

		public int Index { get; }

		public int Size { get; }

		public ulong ModBase { get; }

		public ulong Value { get; }

		public ulong Address { get; }

		public int Register { get; }

		public int Scope { get; }

		public SymbolTag Tag { get; }

		public SymbolFlag Flags { get; }

		public long Offset => (long) (Address - ModBase);


		public override string ToString()
		{
			return String.Format("Name: {0} | Offset: {1:X} | Address: {2:X} | Tag: {3} | Flags: {4}",
				Name, Offset, Address, Tag, Flags);
		}
	}
}