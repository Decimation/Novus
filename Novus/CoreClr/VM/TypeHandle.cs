using System.Reflection;
using System.Runtime.InteropServices;
using Novus.CoreClr.VM;
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