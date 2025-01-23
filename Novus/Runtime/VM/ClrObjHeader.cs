// using System.Runtime.InteropServices;

using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public struct ClrObjHeader
{
	public nint Value { get; set; }

}