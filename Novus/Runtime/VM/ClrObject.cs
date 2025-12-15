// Author: Deci | Project: Novus | Name: ClrObject.cs
// Date: 2024/12/19 @ 13:12:50

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Imports.Attributes;
using Novus.Memory;

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ClrObject
{

	static ClrObject()
	{
		Global.Clr.LoadImports(typeof(ClrObject));
	}

	/*[field: ImportClr("Sig_ObjGetSize")]
	private static delegate* unmanaged[Thiscall]<void*, ulong> Func_ObjGetSize { get; }

	public ulong Size
	{
		get
		{
			fixed (ClrObject* __this = &this) {
				return Func_ObjGetSize(__this);
			}
		}
	}*/

	public const int MARKED_BIT = 0x1;

	[UnscopedRef]
	public ref ClrObjHeader Header
	{
		get
		{
			ref var hdr = ref Unsafe.SubtractByteOffset(ref this, OffsetOptions.Header.GetOffsetValue());
			return ref Unsafe.As<ClrObject, ClrObjHeader>(ref hdr);

		}
	}

	public TypeHandle TypeHandle { get; set; }

	public byte Data { get; set; }


	/*public ref byte Data
	{
		get
		{
			fixed (ClrObject* p = &this) {
				return ref Unsafe.AsRef<byte>(p);

			}
		}
	}*/

}