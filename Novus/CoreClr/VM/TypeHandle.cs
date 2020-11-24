using System.Runtime.InteropServices;
using Novus.Interop;
using Novus.Memory;
using Novus.Properties;

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

				var sig = EmbeddedResources.Sig_GetMethodTable;
				var fn  = (void*) Resources.Clr.Scanner.FindSignature(sig);

				fixed (TypeHandle* p = &this) {
					var mt = (MethodTable*) Functions.CallReturnPointer(fn, (ulong) p);

					return mt;
				}
			}
		}
	}
}