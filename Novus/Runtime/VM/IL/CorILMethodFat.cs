using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Win32;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.IL;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct CorILMethodFat
{
	/*
	 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/CoreClr/VM/Jit/CorMethodFat.cs
	 */
	
	// https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/corhlpr.h
	// https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/corhdr.h

	/// <summary>
	/// <c>DWORD</c> #1
	///     <para>unsigned Flags    : 12;</para>
	///     <para>unsigned Size     :  4;</para>
	///     <para>unsigned MaxStack : 16;</para>
	/// </summary>
	private uint m_bf;

	private uint m_codeSize;

	private uint m_localVarSigTok;

	internal bool IsFat
	{
		get
		{
			fixed (CorILMethodFat* value = &this) {
				return (*(byte*) value & (int) CorILMethodFlags.FormatMask) == (int) CorILMethodFlags.FatFormat;
			}
		}
	}

	internal CorILMethodFlags Flags
	{
		get
		{
			fixed (CorILMethodFat* value2 = &this) {
				var value = (byte*) value2;

				return (CorILMethodFlags) (((uint) *(value + 0)) | ((((uint) *(value + 1)) & 0x0F) << 8));
			}
		}
	}

	internal int Size
	{
		get
		{
			fixed (CorILMethodFat* value = &this) {
				var p = (byte*) value;
				return *(p + 1) >> 4;
			}
		}
	}

	internal int CodeSize => (int) (m_codeSize);

	internal Pointer<byte> Code
	{
		get
		{
			fixed (CorILMethodFat* value = &this) {
				var p = (byte*) value;
				return ((p) + 4 * Size);
			}
		}
	}

	internal int MaxStackSize
	{
		get
		{
			// return VAL16(*(USHORT*)((BYTE*)this+2));
			fixed (CorILMethodFat* value = &this) {
				return (*(short*) ((byte*) value + 2));
			}
		}
	}

	internal int LocalVarSigToken => (int) (m_localVarSigTok);

	internal byte[] GetCodeIL()
		=> Code.ToArray(CodeSize);

}