using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Memory;
using Novus.Win32;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeAccessorOwnerBody

namespace Novus.Runtime.VM.IL;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct CorILMethodTiny
{

	/*
	 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/CoreClr/VM/Jit/CorMethodTiny.cs
	 */

	private byte m_flagsAndCodeSize;

	internal bool IsTiny
	{
		get
		{
			// return((Flags_CodeSize & (CorILMethod_FormatMask >> 1)) == CorILMethod_TinyFormat);
			return (m_flagsAndCodeSize & (((int) CorILMethodFlags.FormatMask) >> 1))
			       == (int) CorILMethodFlags.TinyFormat;
		}
	}

	internal int CodeSize
	{
		get
		{
			// return(((unsigned) Flags_CodeSize) >> (CorILMethod_FormatShift-1));
			return (m_flagsAndCodeSize) >> ((int) (CorILMethodFlags.FormatShift - 1));
		}
	}

	internal Pointer<byte> Code
	{
		get
		{
			// return(((BYTE*) this) + sizeof(struct tagCOR_ILMETHOD_TINY));

			fixed (CorILMethodTiny* value = &this) {
				return (((byte*) value) + sizeof(CorILMethodTiny));
			}
		}
	}

	internal int MaxStackSize
	{
		get
		{
			const int MAX_STACK_SIZE_TINY = 8;
			return MAX_STACK_SIZE_TINY;
		}
	}

	internal int LocalVarSigToken
	{
		get
		{
			const int LOCAL_VAR_SIG_TOKEN_TINY = 0;
			return LOCAL_VAR_SIG_TOKEN_TINY;
		}
	}

	internal byte[] GetCodeIL()
		=> Code.ToArray(CodeSize);

}