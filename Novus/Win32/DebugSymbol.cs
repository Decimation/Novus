using System;
using System.Runtime.InteropServices;
using Novus.Imports;

namespace Novus.Win32
{
	/// <summary>
	/// Contains symbol information.
	/// </summary>
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct DebugSymbol
	{
		/// <summary>
		///     Max string length for <see cref="DebugSymbol.Name" />
		/// </summary>
		internal const int MaxNameLength = 2000;

		/// <summary>
		///     Size of <see cref="DebugSymbol" />
		/// </summary>
		internal static readonly int SizeOf = Marshal.SizeOf<DebugSymbol>();

		internal static readonly int FullSize =
			SizeOf + MaxNameLength * sizeof(byte) + sizeof(ulong) - 1 / sizeof(ulong);


		// https://docs.microsoft.com/en-us/windows/win32/api/dbghelp/ns-dbghelp-symbol_info

		/// <summary>
		///     The size of the structure, in bytes. This member must be set to <see cref="SizeOf" />.
		///     Note that the total size of the data is <see cref="GetSymbolInfoSize" />. The reason to subtract one is
		///     that the first character in the name is accounted for in the size of the structure.
		/// </summary>
		internal uint SizeOfStruct { get; set; }

		/// <summary>
		///     A unique value that identifies the type data that describes the symbol.
		///     This value does not persist between sessions.
		/// </summary>
		internal uint TypeIndex { get; set; }


		/// <summary>
		///     Reserved.
		/// </summary>
		private fixed ulong Reserved[2];


		/// <summary>
		///     The unique value for the symbol. The value associated with a symbol is not guaranteed to be the same
		///     each time you run the process. For PDB symbols, the index value for a symbol is not generated until the
		///     symbol is enumerated or retrieved through a search by name or address. The index values for all
		///     CodeView and COFF symbols are generated when the symbols are loaded.
		/// </summary>
		internal uint Index { get; set; }

		/// <summary>
		///     The symbol size, in bytes. This value is meaningful only if the module symbols are from a pdb file;
		///     otherwise, this value is typically zero and should be ignored.
		/// </summary>
		internal uint Size { get; set; }

		/// <summary>
		///     The base address of the module that contains the symbol.
		/// </summary>
		internal ulong ModBase { get; set; }

		/// <summary>
		/// <see cref="SymbolFlag"/>
		/// </summary>
		internal SymbolFlag Flags { get; set; }

		/// <summary>
		///     The value of a constant.
		/// </summary>
		internal ulong Value { get; set; }

		/// <summary>
		///     The virtual address of the start of the symbol.
		/// </summary>
		internal ulong Address { get; set; }


		/// <summary>
		///     The register.
		/// </summary>
		internal uint Register { get; set; }

		/// <summary>
		///     DIA scope.
		/// </summary>
		internal uint Scope { get; set; }

		/// <summary>
		///     PDB classification.
		/// </summary>
		internal SymbolTag Tag { get; set; }

		/// <summary>
		///     The length of the name, in characters, not including the null-terminating character.
		/// </summary>
		internal uint NameLen { get; set; }

		/// <summary>
		///     The size of the <see cref="Name" /> buffer, in characters. If this member is 0,
		///     the <see cref="Name" /> member is not used.
		/// </summary>
		internal uint MaxNameLen { get; set; }

		/// <summary>
		///     The name of the symbol. The name can be undecorated if the <see cref="SymbolOptions.UNDNAME" /> option is
		///     used with the <see cref="Symbols.SetOptions" /> function.
		/// </summary>
		internal fixed sbyte Name[1];

		// !
		//internal static int GetSymbolInfoSize(DebugSymbol* pSym)
		//{
		//	// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
		//	return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
		//}

		internal string ReadSymbolName()
		{
			/*while( (native.NameLen > 0) &&
			       (0 == Marshal.ReadInt16( pNative + ((int) (native.NameLen - 1) * sizeof( char )) )) )
			{
				native.NameLen = native.NameLen - 1; // don't pull in trailing NULLs
			}
			Name = Marshal.PtrToStringUni( pNative, (int) native.NameLen );*/
			//fixed (DebugSymbol* pSym = &this)
			//{
			//	sbyte** namePtr = &pSym->Name;
			//	return Mem.ReadString(*namePtr, (int)pSym->NameLen);
			//}
			//fixed (DebugSymbol* pSym = &this) {
			//	sbyte* namePtr = pSym->Name;
			//	return Marshal.PtrToStringUni((IntPtr) namePtr, (int) NameLen);
			//}

			fixed (DebugSymbol* pSym = &this) {
				sbyte* namePtr = pSym->Name;
				return Marshal.PtrToStringUni((IntPtr) namePtr, (int) NameLen);
			}
		}
	}
}