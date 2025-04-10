using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EEClassNativeLayoutInfo
{

	internal byte Alignment { get; set; }

	internal bool IsMarshalable { get; set; }

	// private  ushort Padding       { get; set; }
	internal uint Size { get; set; }

	internal uint NumFields { get; set; }

	internal void* Descriptor { get; set; }

}