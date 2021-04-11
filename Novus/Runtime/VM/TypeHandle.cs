using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Memory;
// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable IDE0044


namespace Novus.Runtime.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct TypeHandle
	{
		static TypeHandle()
		{
			Resource.LoadImports(typeof(TypeHandle));
		}

		private void* Value { get; }


		internal Pointer<MethodTable> MethodTable
		{
			get
			{
				fixed (TypeHandle* p = &this) {

					return Func_GetMethodTable(p);
				}
			}
		}

		/// <summary>
		/// <see cref="TypeHandle.MethodTable"/>
		/// </summary>
		[field: ImportClr("Sig_GetMethodTable")]
		private static delegate* unmanaged<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }
	}
}