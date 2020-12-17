using System.Runtime.InteropServices;

// ReSharper disable StructCanBeMadeReadOnly
namespace Novus.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ImageFileHeader
	{
		/// WORD->unsigned short
		public MachineType Machine { get; }

		/// WORD->unsigned short
		public ushort NumberOfSections { get; }

		/// DWORD->unsigned int
		public uint TimeDateStamp { get; }

		/// DWORD->unsigned int
		public uint PointerToSymbolTable { get; }

		/// DWORD->unsigned int
		public uint NumberOfSymbols { get; }

		/// WORD->unsigned short
		public ushort SizeOfOptionalHeader { get; }

		/// WORD->unsigned short
		public ushort Characteristics { get; }
	}
}