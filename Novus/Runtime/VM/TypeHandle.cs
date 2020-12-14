using System.Runtime.InteropServices;
using Novus.Interop;
using Novus.Memory;

// ReSharper disable UnusedMember.Global

// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable IDE0044


namespace Novus.Runtime.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct TypeHandle
	{
		private void* Value { get; }


		internal Pointer<MethodTable> MethodTable
		{
			get
			{
				fixed (TypeHandle* p = &this) {

					return Functions.Func_GetMethodTable(p);
				}
			}
		}
	}
}