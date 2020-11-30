using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Novus.Interop;

// ReSharper disable InconsistentNaming

namespace Novus.CoreClr.VM.EE
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