using System.Runtime.InteropServices;
using Novus.Imports;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct EEClassNativeLayoutInfo
	{
		internal byte   Alignment     { get; }
		internal bool   IsMarshalable { get; }
		private  ushort Padding       { get; }
		internal uint   Size          { get; }
		internal uint   NumFields     { get; }
		internal void*  Descriptor    { get; }
	}
}