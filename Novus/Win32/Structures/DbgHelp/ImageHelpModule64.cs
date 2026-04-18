// Author: Deci | Project: Novus | Name: ImageHelpModule64.cs
// Date: 2026/04/18 @ 02:04:16

using System.Runtime.InteropServices;
using Novus.Imports.Attributes;

namespace Novus.Win32.Structures.DbgHelp;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
[NativeStructure]
public struct ImageHelpModule64
{

	/// <summary>
	/// The size of the structure, in bytes. The caller must set this member to
	/// <c>sizeof(ImageHelpModule64)</c>
	/// </summary>
	public uint SizeOfStruct;

	/// <summary>The base virtual address where the image is loaded.</summary>
	public ulong BaseOfImage;

	/// <summary>The size of the image, in bytes.</summary>
	public uint ImageSize;

	/// <summary>
	/// The date and timestamp value. The value is represented in the number of seconds elapsed since midnight (00:00:00), January
	/// 1, 1970, Universal Coordinated Time, according to the system clock. The timestamp can be printed using the C run-time (CRT)
	/// function <c>ctime</c>.
	/// </summary>
	public uint TimeDateStamp;

	/// <summary>The checksum of the image. This value can be zero.</summary>
	public uint CheckSum;

	/// <summary>
	/// The number of symbols in the symbol table. The value of this parameter is not meaningful when <c>SymPdb</c> is specified as
	/// the value of the SymType parameter.
	/// </summary>
	public uint NumSyms;

	public ImageHelpModule64()
	{
		SizeOfStruct = (uint) Marshal.SizeOf<ImageHelpModule64>();
	}

	/// <summary>
	/// <para>The type of symbols that are loaded. This member can be one of the following values.</para>
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <term>Meaning</term>
	/// </listheader>
	/// <item>
	/// <term>SymCoff</term>
	/// <term>COFF symbols.</term>
	/// </item>
	/// <item>
	/// <term>SymCv</term>
	/// <term>CodeView symbols.</term>
	/// </item>
	/// <item>
	/// <term>SymDeferred</term>
	/// <term>Symbol loading deferred.</term>
	/// </item>
	/// <item>
	/// <term>SymDia</term>
	/// <term>DIA symbols.</term>
	/// </item>
	/// <item>
	/// <term>SymExport</term>
	/// <term>Symbols generated from a DLL export table.</term>
	/// </item>
	/// <item>
	/// <term>SymNone</term>
	/// <term>No symbols are loaded.</term>
	/// </item>
	/// <item>
	/// <term>SymPdb</term>
	/// <term>PDB symbols.</term>
	/// </item>
	/// <item>
	/// <term>SymSym</term>
	/// <term>.sym file.</term>
	/// </item>
	/// <item>
	/// <term>SymVirtual</term>
	/// <term>The virtual module created by SymLoadModuleEx with SLMFLAG_VIRTUAL.</term>
	/// </item>
	/// </list>
	/// </summary>
	public SymbolType SymType;

	/// <summary>The module name.</summary>
	[MA(UT.ByValTStr, SizeConst = 32)]
	public string ModuleName;

	/// <summary>The image name. The name may or may not contain a full path.</summary>
	[MA(UT.ByValTStr, SizeConst = 256)]
	public string ImageName;

	/// <summary>The full path and file name of the file from which symbols were loaded.</summary>
	[MA(UT.ByValTStr, SizeConst = 256)]
	public string LoadedImageName;

	/// <summary>The full path and file name of the .pdb file.</summary>
	[MA(UT.ByValTStr, SizeConst = 256)]
	public string LoadedPdbName;

	/// <summary>The signature of the CV record in the debug directories.</summary>
	public uint CVSig;

	/// <summary>The contents of the CV record.</summary>
	[MA(UT.ByValTStr, SizeConst = 260 * 3)]
	public string CVData;

	/// <summary>The PDB signature.</summary>
	public uint PdbSig;

	/// <summary>The PDB signature (Visual C/C++ 7.0 and later)</summary>
	public Guid PdbSig70;

	/// <summary>The DBI age of PDB.</summary>
	public uint PdbAge;

	/// <summary>A value that indicates whether the loaded PDB is unmatched.</summary>
	[MA(UT.Bool)]
	public bool PdbUnmatched;

	/// <summary>A value that indicates whether the loaded DBG is unmatched.</summary>
	[MA(UT.Bool)]
	public bool DbgUnmatched;

	/// <summary>A value that indicates whether line number information is available.</summary>
	[MA(UT.Bool)]
	public bool LineNumbers;

	/// <summary>A value that indicates whether symbol information is available.</summary>
	[MA(UT.Bool)]
	public bool GlobalSymbols;

	/// <summary>A value that indicates whether type information is available.</summary>
	[MA(UT.Bool)]
	public bool TypeInfo;

	/// <summary>
	/// <para>A value that indicates whether the .pdb supports the source server.</para>
	/// <para><c>DbgHelp 6.1 and earlier:</c> This member is not supported.</para>
	/// </summary>
	[MA(UT.Bool)]
	public bool SourceIndexed;

	/// <summary>
	/// <para>A value that indicates whether the module contains public symbols.</para>
	/// <para><c>DbgHelp 6.1 and earlier:</c> This member is not supported.</para>
	/// </summary>
	[MA(UT.Bool)]
	public bool Publics;

	/// <summary/>
	public uint MachineType;

	/// <summary/>
	public uint Reserved;

}

public enum SymbolType : uint
{

	SymNone     = 0,
	SymCoff     = 1,
	SymCv       = 2,
	SymPdb      = 3,
	SymExport   = 4,
	SymDeferred = 5,
	SymSym      = 6,
	SymDia      = 7,
	SymVirtual  = 8,

}