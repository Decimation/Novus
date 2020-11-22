using System.Runtime.InteropServices;
using Novus.Memory;
using Novus.Native;
using Novus.Utilities;

// ReSharper disable UnusedMember.Global

// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable IDE0044

namespace Novus.CoreClr.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct TypeHandle
	{
		private void* m_asPtr;

		internal Pointer<MethodTable> MethodTable
		{
			get
			{
				var sig = "48 8B 01 A8 02 75 ? C3";
				var fn  = (void*) Resources.Clr.Scanner.FindPattern(sig);

				fixed (TypeHandle* p = &this) {
					var mt = (MethodTable*) Functions.CallReturnPointer(fn, (ulong) p);

					return mt;
				}
			}
		}
	}
}