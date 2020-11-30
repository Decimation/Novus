using System;
using Novus.Win32;
// ReSharper disable UnusedMember.Global

namespace Novus.CoreClr
{
	public static class Tokens
	{
		// src/inc/corhdr.h

		public const CorElementType PRIMITIVE_TABLE_SIZE = CorElementType.String;

		public const int PT_PRIMITIVE = 0x01000000;

		/// <summary>
		/// <para>The Attributes Table</para>
		/// <para>20 bits for built in types and 12 bits for Properties</para>
		/// <para>The properties are followed by the widening mask. All types widen to themselves.</para>
		/// <para>https://github.com/dotnet/coreclr/blob/master/src/vm/invokeutil.cpp</para>
		/// <para>https://github.com/dotnet/coreclr/blob/master/src/vm/invokeutil.h</para>
		/// </summary>
		public static readonly int[] PrimitiveAttributes =
		{
			0x00,                  // ELEMENT_TYPE_END
			0x00,                  // ELEMENT_TYPE_VOID
			PT_PRIMITIVE | 0x0004, // ELEMENT_TYPE_BOOLEAN
			PT_PRIMITIVE | 0x3F88, // ELEMENT_TYPE_CHAR (W = U2, CHAR, I4, U4, I8, U8, R4, R8) (U2 == Char)
			PT_PRIMITIVE | 0x3550, // ELEMENT_TYPE_I1   (W = I1, I2, I4, I8, R4, R8) 
			PT_PRIMITIVE | 0x3FE8, // ELEMENT_TYPE_U1   (W = CHAR, U1, I2, U2, I4, U4, I8, U8, R4, R8)
			PT_PRIMITIVE | 0x3540, // ELEMENT_TYPE_I2   (W = I2, I4, I8, R4, R8)
			PT_PRIMITIVE | 0x3F88, // ELEMENT_TYPE_U2   (W = U2, CHAR, I4, U4, I8, U8, R4, R8)
			PT_PRIMITIVE | 0x3500, // ELEMENT_TYPE_I4   (W = I4, I8, R4, R8)
			PT_PRIMITIVE | 0x3E00, // ELEMENT_TYPE_U4   (W = U4, I8, R4, R8)
			PT_PRIMITIVE | 0x3400, // ELEMENT_TYPE_I8   (W = I8, R4, R8)
			PT_PRIMITIVE | 0x3800, // ELEMENT_TYPE_U8   (W = U8, R4, R8)
			PT_PRIMITIVE | 0x3000, // ELEMENT_TYPE_R4   (W = R4, R8)
			PT_PRIMITIVE | 0x2000, // ELEMENT_TYPE_R8   (W = R8) 
		};


		public static bool IsPrimitiveType(CorElementType type)
		{
			// if (type >= PRIMITIVE_TABLE_SIZE)
			// {
			//     if (ELEMENT_TYPE_I==type || ELEMENT_TYPE_U==type)
			//     {
			//         return TRUE;
			//     }
			//     return 0;
			// }

			// return (PT_Primitive & PrimitiveAttributes[type]);

			if (type >= PRIMITIVE_TABLE_SIZE)
			{
				return CorElementType.I == type || CorElementType.U == type;
			}

			return (PT_PRIMITIVE & PrimitiveAttributes[(byte)type]) != 0;
		}

		public static bool IsNilToken(int tk) => RidFromToken(tk) == 0;

		public static int RidToToken(int rid, CorTokenType tktype)
		{
			// #define RidToToken(rid,tktype) ((rid) |= (tktype))
			(rid) |= ((int)tktype);
			return rid;
		}

		public static int TokenFromRid(int rid, CorTokenType tktype)
		{
			// #define TokenFromRid(rid,tktype) ((rid) | (tktype))
			return rid | (int)tktype;
		}

		public static int RidFromToken(int tk)
		{
			// #define RidFromToken(tk) ((RID) ((tk) & 0x00ffffff))
			const int RID_FROM_TOKEN = 0x00FFFFFF;
			return tk & RID_FROM_TOKEN;
		}

		public static long TypeFromToken(int tk)
		{
			// #define TypeFromToken(tk) ((ULONG32)((tk) & 0xff000000))

			const uint TYPE_FROM_TOKEN = 0xFF000000;
			return tk & TYPE_FROM_TOKEN;
		}

		public static bool IsPrimitive(this CorElementType cet)
		{
			return cet >= CorElementType.Boolean && cet <= CorElementType.R8
				   || cet == CorElementType.I || cet == CorElementType.U
				   || cet == CorElementType.Ptr || cet == CorElementType.FnPtr;
		}

		/// <summary>
		///     <exception cref="Exception">If size is unknown</exception>
		/// </summary>
		public static int SizeOfElementType(CorElementType t)
		{
			switch (t)
			{
				case CorElementType.Void:
					return default;

				case CorElementType.Boolean:
					return sizeof(bool);

				case CorElementType.Char:
					return sizeof(char);

				case CorElementType.I1:
					return sizeof(sbyte);
				case CorElementType.U1:
					return sizeof(byte);

				case CorElementType.I2:
					return sizeof(short);
				case CorElementType.U2:
					return sizeof(ushort);

				case CorElementType.I4:
					return sizeof(int);
				case CorElementType.U4:
					return sizeof(uint);

				case CorElementType.I8:
					return sizeof(long);
				case CorElementType.U8:
					return sizeof(ulong);

				case CorElementType.R4:
					return sizeof(float);
				case CorElementType.R8:
					return sizeof(double);

				case CorElementType.String:
				case CorElementType.Ptr:
				case CorElementType.ByRef:
				case CorElementType.Class:
				case CorElementType.Array:
				case CorElementType.I:
				case CorElementType.U:
				case CorElementType.FnPtr:
				case CorElementType.Object:
				case CorElementType.SzArray:
				case CorElementType.End:
					return IntPtr.Size;


				case CorElementType.ValueType:
				case CorElementType.Var:
				case CorElementType.GenericInst:
				case CorElementType.CModReqd:
				case CorElementType.CModOpt:
				case CorElementType.Internal:
				case CorElementType.MVar:
					return Native.INVALID;

				case CorElementType.TypedByRef:
					return IntPtr.Size * 2;

				case CorElementType.Max:
				case CorElementType.Modifier:
				case CorElementType.Sentinel:
				case CorElementType.Pinned:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(t), t, null);
			}

			throw new InvalidOperationException();
		}
	}


	//	[Flags]
	public enum CorInterfaceType
	{
		/// <summary>
		///     Interface derives from IDispatch.
		/// </summary>
		Dual = 0,

		/// <summary>
		///     Interface derives from IUnknown.
		/// </summary>
		VTable = 1,

		/// <summary>
		///     Interface is a dispinterface.
		/// </summary>
		Dispatch = 2,

		/// <summary>
		///     Interface derives from IInspectable.
		/// </summary>
		Inspectable = 3,

		/// <summary>
		///     The last member of the enum.
		/// </summary>
		Last = 4
	}


	public enum CorElementType : byte
	{
		End = 0x00,
		Void = 0x01,

		/// <summary>
		///     bool
		/// </summary>
		Boolean = 0x02,

		/// <summary>
		///     char
		/// </summary>
		Char = 0x03,

		/// <summary>
		///     sbyte
		/// </summary>
		I1 = 0x04,

		/// <summary>
		///     byte
		/// </summary>
		U1 = 0x05,

		/// <summary>
		///     short
		/// </summary>
		I2 = 0x06,

		/// <summary>
		///     ushort
		/// </summary>
		U2 = 0x07,

		/// <summary>
		///     int
		/// </summary>
		I4 = 0x08,

		/// <summary>
		///     uint
		/// </summary>
		U4 = 0x09,

		/// <summary>
		///     long
		/// </summary>
		I8 = 0x0A,

		/// <summary>
		///     ulong
		/// </summary>
		U8 = 0x0B,

		/// <summary>
		///     float
		/// </summary>
		R4 = 0x0C,

		/// <summary>
		///     double
		/// </summary>
		R8 = 0x0D,

		/// <summary>
		///     Note: strings don't actually map to this. They map to <see cref="Class" />
		/// </summary>
		String = 0x0E,

		Ptr = 0x0F,
		ByRef = 0x10,

		/// <summary>
		///     Struct type
		/// </summary>
		ValueType = 0x11,

		/// <summary>
		///     Reference type (i.e. string, object)
		/// </summary>
		Class = 0x12,

		Var = 0x13,
		Array = 0x14,
		GenericInst = 0x15,
		TypedByRef = 0x16,
		I = 0x18,
		U = 0x19,
		FnPtr = 0x1B,
		Object = 0x1C,
		SzArray = 0x1D,
		MVar = 0x1E,
		CModReqd = 0x1F,
		CModOpt = 0x20,
		Internal = 0x21,
		Max = 0x22,
		Modifier = 0x40,
		Sentinel = 0x41,
		Pinned = 0x45
	}


	public enum CorTokenType : uint
	{
		Module = 0x00000000,
		TypeRef = 0x01000000,
		TypeDef = 0x02000000,
		FieldDef = 0x04000000,
		MethodDef = 0x06000000,
		ParamDef = 0x08000000,
		InterfaceImpl = 0x09000000,
		MemberRef = 0x0a000000,
		CustomAttribute = 0x0c000000,
		Permission = 0x0e000000,
		Signature = 0x11000000,
		Event = 0x14000000,
		Property = 0x17000000,
		MethodImpl = 0x19000000,
		ModuleRef = 0x1a000000,
		TypeSpec = 0x1b000000,
		Assembly = 0x20000000,
		AssemblyRef = 0x23000000,
		File = 0x26000000,
		ExportedType = 0x27000000,
		ManifestResource = 0x28000000,
		GenericParam = 0x2a000000,
		MethodSpec = 0x2b000000,
		GenericParamConstraint = 0x2c000000,
		String = 0x70000000,
		Name = 0x71000000,

		BaseType = 0x72000000 // Leave this on the high end value. This does not correspond to metadata table
	}


	[Flags]
	public enum CorCallingConvention
	{
		// IMAGE_CEE_CS_CALLCONV

		Default = 0x0,

		Vararg = 0x5,
		Field = 0x6,
		LocalSig = 0x7,
		Property = 0x8,
		Unmanaged = 0x9,

		/// <summary>
		///     Generic method instantiation
		/// </summary>
		GenericInst = 0xa,

		/// <summary>
		///     Used ONLY for 64bit vararg PInvoke calls
		/// </summary>
		NativeVararg = 0xb,

		/// <summary>
		///     First invalid calling convention
		/// </summary>
		Max = 0xc,


		// The high bits of the calling convention convey additional info

		/// <summary>
		///     Calling convention is bottom 4 bits
		/// </summary>
		Mask = 0x0f,

		/// <summary>
		///     Top bit indicates a 'this' parameter
		/// </summary>
		HasThis = 0x20,

		/// <summary>
		///     This parameter is explicitly in the signature
		/// </summary>
		ExplicitThis = 0x40,

		/// <summary>
		///     Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
		/// </summary>
		Generic = 0x10

		// 0x80 is reserved for internal use
	}
}
