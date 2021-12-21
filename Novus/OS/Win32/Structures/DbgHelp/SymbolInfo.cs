using System;
using System.Runtime.InteropServices;
using Novus.Imports;

namespace Novus.OS.Win32.Structures.DbgHelp;

/// <summary>
/// Contains symbol information.
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct SymbolInfo
{
	/// <summary>
	///     Max string length for <see cref="SymbolInfo.Name" />
	/// </summary>
	internal const int MaxNameLength = 2000;

	/// <summary>
	///     Size of <see cref="SymbolInfo" />
	/// </summary>
	internal static readonly int SizeOf = Marshal.SizeOf<SymbolInfo>();

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
	///     used with the <see cref="Native.SymSetOptions" /> function.
	/// </summary>
	internal fixed sbyte Name[1];

	internal static int GetSymbolInfoSize(SymbolInfo* pSym)
	{
		// SizeOfStruct + (MaxNameLen - 1) * sizeof(TCHAR)
		return (int) (pSym->SizeOfStruct + (pSym->MaxNameLen - 1) * sizeof(byte));
	}

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

		fixed (SymbolInfo* pSym = &this) {
			sbyte* namePtr = pSym->Name;
			return Marshal.PtrToStringUni((IntPtr) namePtr, (int) NameLen);
		}
	}
}

[Flags]
public enum SymbolFlag
{
	/// <summary>
	/// The symbol is a CLR token.
	/// </summary>
	CLR_TOKEN = 0x00040000,


	/// <summary>
	/// The symbol is a constant.
	/// </summary>
	CONSTANT = 0x00000100,


	/// <summary>
	/// The symbol is from the export table.
	/// </summary>
	EXPORT = 0x00000200,


	/// <summary>
	/// The symbol is a forwarder.
	/// </summary>
	FORWARDER = 0x00000400,


	/// <summary>
	/// Offsets are frame relative.
	/// </summary>
	FRAMEREL = 0x00000020,


	/// <summary>
	/// The symbol is a known function.
	/// </summary>
	FUNCTION = 0x00000800,


	/// <summary>
	/// The symbol address is an offset relative to the beginning of the intermediate language block. This applies to managed code only.
	/// </summary>
	ILREL = 0x00010000,


	/// <summary>
	/// The symbol is a local variable.
	/// </summary>
	LOCAL = 0x00000080,


	/// <summary>
	/// The symbol is managed metadata.
	/// </summary>
	METADATA = 0x00020000,


	/// <summary>
	/// The symbol is a parameter.
	/// </summary>
	PARAMETER = 0x00000040,


	/// <summary>
	/// The symbol is a register. The Register member is used.
	/// </summary>
	REGISTER = 0x00000008,

	/// <summary>
	/// Offsets are register relative.
	/// </summary>
	REGREL = 0x00000010,

	/// <summary>
	/// The symbol is a managed code slot.
	/// </summary>
	SLOT = 0x00008000,

	/// <summary>
	/// The symbol is a thunk.
	/// </summary>
	THUNK = 0x00002000,

	/// <summary>
	/// The symbol is an offset into the TLS data area.
	/// </summary>
	TLSREL = 0x00004000,

	/// <summary>
	/// The Value member is used.
	/// </summary>
	VALUEPRESENT = 0x00000001,

	/// <summary>
	/// The symbol is a virtual symbol created by the SymAddSymbol function.
	/// </summary>
	VIRTUAL = 0x00001000,
}

[Flags]
public enum SymbolOptions : uint
{
	ALLOW_ABSOLUTE_SYMBOLS = 0x00000800,

	ALLOW_ZERO_ADDRESS = 0x01000000,

	AUTO_PUBLICS = 0x00010000,

	CASE_INSENSITIVE = 0x00000001,

	DEBUG = 0x80000000,

	DEFERRED_LOADS = 0x00000004,

	DISABLE_SYMSRV_AUTODETECT = 0x02000000,

	EXACT_SYMBOLS = 0x00000400,

	FAIL_CRITICAL_ERRORS = 0x00000200,

	FAVOR_COMPRESSED = 0x00800000,

	FLAT_DIRECTORY = 0x00400000,

	IGNORE_CVREC = 0x00000080,

	IGNORE_IMAGEDIR = 0x00200000,

	IGNORE_NT_SYMPATH = 0x00001000,

	INCLUDE_32_BIT_MODULES = 0x00002000,

	LOAD_ANYTHING = 0x00000040,

	LOAD_LINES = 0x00000010,

	NO_CPP = 0x00000008,

	NO_IMAGE_SEARCH = 0x00020000,

	NO_PROMPTS = 0x00080000,

	NO_PUBLICS = 0x00008000,

	NO_UNQUALIFIED_LOADS = 0x00000100,

	OVERWRITE = 0x00100000,

	PUBLICS_ONLY = 0x00004000,

	SECURE = 0x00040000,

	UNDNAME = 0x00000002,
}

public enum SymbolTag
{
	Null,
	Exe,
	Compiland,
	CompilandDetails,
	CompilandEnv,
	Function,
	Block,
	Data,
	Annotation,
	Label,
	PublicSymbol,
	UDT,
	Enum,
	FunctionType,
	PointerType,
	ArrayType,
	BaseType,
	Typedef,
	BaseClass,
	Friend,
	FunctionArgType,
	FuncDebugStart,
	FuncDebugEnd,
	UsingNamespace,
	VTableShape,
	VTable,
	Custom,
	Thunk,
	CustomType,
	ManagedType,
	Dimension,
	CallSite,
	InlineSite,
	BaseInterface,
	VectorType,
	MatrixType,
	HLSLType,
	Caller,
	Callee,
	Export,
	HeapAllocationSite,
	CoffGroup,
	Max
}