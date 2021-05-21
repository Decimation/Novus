using System;
using Novus.Memory;
using Novus.Win32;
// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

namespace Novus.Runtime
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

			if (type >= PRIMITIVE_TABLE_SIZE) {
				return CorElementType.I == type || CorElementType.U == type;
			}

			return (PT_PRIMITIVE & PrimitiveAttributes[(byte) type]) != 0;
		}

		public static bool IsNilToken(int tk) => RidFromToken(tk) == 0;

		public static int RidToToken(int rid, CorTokenType tktype)
		{
			// #define RidToToken(rid,tktype) ((rid) |= (tktype))
			(rid) |= ((int) tktype);
			return rid;
		}

		public static int TokenFromRid(int rid, CorTokenType tktype)
		{
			// #define TokenFromRid(rid,tktype) ((rid) | (tktype))
			return rid | (int) tktype;
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
			       || cet == CorElementType.I   || cet == CorElementType.U
			       || cet == CorElementType.Ptr || cet == CorElementType.FnPtr;
		}

		/// <summary>
		///     <exception cref="Exception">If size is unknown</exception>
		/// </summary>
		public static int SizeOfElementType(CorElementType t)
		{
			switch (t) {
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
					return Mem.Size * 2;

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
		End  = 0x00,
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

		Ptr   = 0x0F,
		ByRef = 0x10,

		/// <summary>
		///     Struct type
		/// </summary>
		ValueType = 0x11,

		/// <summary>
		///     Reference type (i.e. string, object)
		/// </summary>
		Class = 0x12,

		Var         = 0x13,
		Array       = 0x14,
		GenericInst = 0x15,
		TypedByRef  = 0x16,
		I           = 0x18,
		U           = 0x19,
		FnPtr       = 0x1B,
		Object      = 0x1C,
		SzArray     = 0x1D,
		MVar        = 0x1E,
		CModReqd    = 0x1F,
		CModOpt     = 0x20,
		Internal    = 0x21,
		Max         = 0x22,
		Modifier    = 0x40,
		Sentinel    = 0x41,
		Pinned      = 0x45
	}


	public enum CorTokenType : uint
	{
		Module                 = 0x00000000,
		TypeRef                = 0x01000000,
		TypeDef                = 0x02000000,
		FieldDef               = 0x04000000,
		MethodDef              = 0x06000000,
		ParamDef               = 0x08000000,
		InterfaceImpl          = 0x09000000,
		MemberRef              = 0x0a000000,
		CustomAttribute        = 0x0c000000,
		Permission             = 0x0e000000,
		Signature              = 0x11000000,
		Event                  = 0x14000000,
		Property               = 0x17000000,
		MethodImpl             = 0x19000000,
		ModuleRef              = 0x1a000000,
		TypeSpec               = 0x1b000000,
		Assembly               = 0x20000000,
		AssemblyRef            = 0x23000000,
		File                   = 0x26000000,
		ExportedType           = 0x27000000,
		ManifestResource       = 0x28000000,
		GenericParam           = 0x2a000000,
		MethodSpec             = 0x2b000000,
		GenericParamConstraint = 0x2c000000,
		String                 = 0x70000000,
		Name                   = 0x71000000,

		BaseType = 0x72000000 // Leave this on the high end value. This does not correspond to metadata table
	}

	// The enumeration is returned in 'getSig','getType', getArgType methods
	public enum CorInfoType
	{
		CORINFO_TYPE_UNDEF      = 0x0,
		CORINFO_TYPE_VOID       = 0x1,
		CORINFO_TYPE_BOOL       = 0x2,
		CORINFO_TYPE_CHAR       = 0x3,
		CORINFO_TYPE_BYTE       = 0x4,
		CORINFO_TYPE_UBYTE      = 0x5,
		CORINFO_TYPE_SHORT      = 0x6,
		CORINFO_TYPE_USHORT     = 0x7,
		CORINFO_TYPE_INT        = 0x8,
		CORINFO_TYPE_UINT       = 0x9,
		CORINFO_TYPE_LONG       = 0xa,
		CORINFO_TYPE_ULONG      = 0xb,
		CORINFO_TYPE_NATIVEINT  = 0xc,
		CORINFO_TYPE_NATIVEUINT = 0xd,
		CORINFO_TYPE_FLOAT      = 0xe,
		CORINFO_TYPE_DOUBLE     = 0xf,
		CORINFO_TYPE_STRING     = 0x10, // Not used, should remove
		CORINFO_TYPE_PTR        = 0x11,
		CORINFO_TYPE_BYREF      = 0x12,
		CORINFO_TYPE_VALUECLASS = 0x13,
		CORINFO_TYPE_CLASS      = 0x14,
		CORINFO_TYPE_REFANY     = 0x15,

		// CORINFO_TYPE_VAR is for a generic type variable.
		// Generic type variables only appear when the JIT is doing
		// verification (not NOT compilation) of generic code
		// for the EE, in which case we're running
		// the JIT in "import only" mode.

		CORINFO_TYPE_VAR = 0x16,
		CORINFO_TYPE_COUNT, // number of jit types
	};

	public enum CorInfoOptions : UInt32
	{
		CORINFO_OPT_INIT_LOCALS = 0x00000010, // zero initialize all variables

		CORINFO_GENERICS_CTXT_FROM_THIS =
			0x00000020, // is this shared generic code that access the generic context from the this pointer?  If so, then if the method has SEH then the 'this' pointer must always be reported and kept alive.

		CORINFO_GENERICS_CTXT_FROM_METHODDESC =
			0x00000040, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodDesc)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_FROM_METHODTABLE =
			0x00000080, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodTable)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_MASK = (CORINFO_GENERICS_CTXT_FROM_THIS       |
		                              CORINFO_GENERICS_CTXT_FROM_METHODDESC |
		                              CORINFO_GENERICS_CTXT_FROM_METHODTABLE),

		CORINFO_GENERICS_CTXT_KEEP_ALIVE =
			0x00000100, // Keep the generics context alive throughout the method even if there is no explicit use, and report its location to the CLR
	};

	public enum CorInfoHelpFunc
	{
		CORINFO_HELP_UNDEF, // invalid value. This should never be used

		/* Arithmetic helpers */

		CORINFO_HELP_DIV, // For the ARM 32-bit integer divide uses a helper call :-(
		CORINFO_HELP_MOD,
		CORINFO_HELP_UDIV,
		CORINFO_HELP_UMOD,

		CORINFO_HELP_LLSH,
		CORINFO_HELP_LRSH,
		CORINFO_HELP_LRSZ,
		CORINFO_HELP_LMUL,
		CORINFO_HELP_LMUL_OVF,
		CORINFO_HELP_ULMUL_OVF,
		CORINFO_HELP_LDIV,
		CORINFO_HELP_LMOD,
		CORINFO_HELP_ULDIV,
		CORINFO_HELP_ULMOD,
		CORINFO_HELP_LNG2DBL,  // Convert a signed int64 to a double
		CORINFO_HELP_ULNG2DBL, // Convert a unsigned int64 to a double
		CORINFO_HELP_DBL2INT,
		CORINFO_HELP_DBL2INT_OVF,
		CORINFO_HELP_DBL2LNG,
		CORINFO_HELP_DBL2LNG_OVF,
		CORINFO_HELP_DBL2UINT,
		CORINFO_HELP_DBL2UINT_OVF,
		CORINFO_HELP_DBL2ULNG,
		CORINFO_HELP_DBL2ULNG_OVF,
		CORINFO_HELP_FLTREM,
		CORINFO_HELP_DBLREM,
		CORINFO_HELP_FLTROUND,
		CORINFO_HELP_DBLROUND,

		/* Allocating a new object. Always use ICorClassInfo::getNewHelper() to decide 
		   which is the right helper to use to allocate an object of a given type. */

		CORINFO_HELP_NEW_CROSSCONTEXT, // cross context new object
		CORINFO_HELP_NEWFAST,
		CORINFO_HELP_NEWSFAST, // allocator for small, non-finalizer, non-array object
		CORINFO_HELP_NEWSFAST_ALIGN8, // allocator for small, non-finalizer, non-array object, 8 byte aligned
		CORINFO_HELP_NEW_MDARR, // multi-dim array helper (with or without lower bounds - dimensions passed in as vararg)
		CORINFO_HELP_NEW_MDARR_NONVARARG, // multi-dim array helper (with or without lower bounds - dimensions passed in as unmanaged array)
		CORINFO_HELP_NEWARR_1_DIRECT, // helper for any one dimensional array creation
		CORINFO_HELP_NEWARR_1_R2R_DIRECT, // wrapper for R2R direct call, which extracts method table from ArrayTypeDesc
		CORINFO_HELP_NEWARR_1_OBJ, // optimized 1-D object arrays
		CORINFO_HELP_NEWARR_1_VC, // optimized 1-D value class arrays
		CORINFO_HELP_NEWARR_1_ALIGN8, // like VC, but aligns the array start

		CORINFO_HELP_STRCNS,                // create a new string literal
		CORINFO_HELP_STRCNS_CURRENT_MODULE, // create a new string literal from the current module (used by NGen code)

		/* Object model */

		CORINFO_HELP_INITCLASS,     // Initialize class if not already initialized
		CORINFO_HELP_INITINSTCLASS, // Initialize class for instantiated type

		// Use ICorClassInfo::getCastingHelper to determine
		// the right helper to use

		CORINFO_HELP_ISINSTANCEOFINTERFACE, // Optimized helper for interfaces
		CORINFO_HELP_ISINSTANCEOFARRAY,     // Optimized helper for arrays
		CORINFO_HELP_ISINSTANCEOFCLASS,     // Optimized helper for classes
		CORINFO_HELP_ISINSTANCEOFANY,       // Slow helper for any type

		CORINFO_HELP_CHKCASTINTERFACE,
		CORINFO_HELP_CHKCASTARRAY,
		CORINFO_HELP_CHKCASTCLASS,
		CORINFO_HELP_CHKCASTANY,
		CORINFO_HELP_CHKCASTCLASS_SPECIAL, // Optimized helper for classes. Assumes that the trivial cases 
		// has been taken care of by the inlined check

		CORINFO_HELP_BOX,
		CORINFO_HELP_BOX_NULLABLE, // special form of boxing for Nullable<T>
		CORINFO_HELP_UNBOX,
		CORINFO_HELP_UNBOX_NULLABLE, // special form of unboxing for Nullable<T>
		CORINFO_HELP_GETREFANY,      // Extract the byref from a TypedReference, checking that it is the expected type

		CORINFO_HELP_ARRADDR_ST,  // assign to element of object array with type-checking
		CORINFO_HELP_LDELEMA_REF, // does a precise type comparision and returns address

		/* Exceptions */

		CORINFO_HELP_THROW,           // Throw an exception object
		CORINFO_HELP_RETHROW,         // Rethrow the currently active exception
		CORINFO_HELP_USER_BREAKPOINT, // For a user program to break to the debugger
		CORINFO_HELP_RNGCHKFAIL,      // array bounds check failed
		CORINFO_HELP_OVERFLOW,        // throw an overflow exception
		CORINFO_HELP_THROWDIVZERO,    // throw a divide by zero exception
		CORINFO_HELP_THROWNULLREF,    // throw a null reference exception

		CORINFO_HELP_INTERNALTHROW, // Support for really fast jit
		CORINFO_HELP_VERIFICATION, // Throw a VerificationException
		CORINFO_HELP_SEC_UNMGDCODE_EXCPT, // throw a security unmanaged code exception
		CORINFO_HELP_FAIL_FAST, // Kill the process avoiding any exceptions or stack and data dependencies (use for GuardStack unsafe buffer checks)

		CORINFO_HELP_METHOD_ACCESS_EXCEPTION, //Throw an access exception due to a failed member/class access check.
		CORINFO_HELP_FIELD_ACCESS_EXCEPTION,
		CORINFO_HELP_CLASS_ACCESS_EXCEPTION,

		CORINFO_HELP_ENDCATCH, // call back into the EE at the end of a catch block

		/* Synchronization */

		CORINFO_HELP_MON_ENTER,
		CORINFO_HELP_MON_EXIT,
		CORINFO_HELP_MON_ENTER_STATIC,
		CORINFO_HELP_MON_EXIT_STATIC,

		CORINFO_HELP_GETCLASSFROMMETHODPARAM, // Given a generics method handle, returns a class handle
		CORINFO_HELP_GETSYNCFROMCLASSHANDLE,  // Given a generics class handle, returns the sync monitor 
		// in its ManagedClassObject

		/* Security callout support */

		CORINFO_HELP_SECURITY_PROLOG, // Required if CORINFO_FLG_SECURITYCHECK is set, or CORINFO_FLG_NOSECURITYWRAP is not set
		CORINFO_HELP_SECURITY_PROLOG_FRAMED, // Slow version of CORINFO_HELP_SECURITY_PROLOG. Used for instrumentation.

		CORINFO_HELP_METHOD_ACCESS_CHECK, // Callouts to runtime security access checks
		CORINFO_HELP_FIELD_ACCESS_CHECK,
		CORINFO_HELP_CLASS_ACCESS_CHECK,

		CORINFO_HELP_DELEGATE_SECURITY_CHECK, // Callout to delegate security transparency check

		/* Verification runtime callout support */

		CORINFO_HELP_VERIFICATION_RUNTIME_CHECK, // Do a Demand for UnmanagedCode permission at runtime

		/* GC support */

		CORINFO_HELP_STOP_FOR_GC, // Call GC (force a GC)
		CORINFO_HELP_POLL_GC,     // Ask GC if it wants to collect

		CORINFO_HELP_STRESS_GC, // Force a GC, but then update the JITTED code to be a noop call
		CORINFO_HELP_CHECK_OBJ, // confirm that ECX is a valid object pointer (debugging only)

		/* GC Write barrier support */

		CORINFO_HELP_ASSIGN_REF, // universal helpers with F_CALL_CONV calling convention
		CORINFO_HELP_CHECKED_ASSIGN_REF,
		CORINFO_HELP_ASSIGN_REF_ENSURE_NONHEAP, // Do the store, and ensure that the target was not in the heap.

		CORINFO_HELP_ASSIGN_BYREF,
		CORINFO_HELP_ASSIGN_STRUCT,


		/* Accessing fields */

		// For COM object support (using COM get/set routines to update object)
		// and EnC and cross-context support
		CORINFO_HELP_GETFIELD8,
		CORINFO_HELP_SETFIELD8,
		CORINFO_HELP_GETFIELD16,
		CORINFO_HELP_SETFIELD16,
		CORINFO_HELP_GETFIELD32,
		CORINFO_HELP_SETFIELD32,
		CORINFO_HELP_GETFIELD64,
		CORINFO_HELP_SETFIELD64,
		CORINFO_HELP_GETFIELDOBJ,
		CORINFO_HELP_SETFIELDOBJ,
		CORINFO_HELP_GETFIELDSTRUCT,
		CORINFO_HELP_SETFIELDSTRUCT,
		CORINFO_HELP_GETFIELDFLOAT,
		CORINFO_HELP_SETFIELDFLOAT,
		CORINFO_HELP_GETFIELDDOUBLE,
		CORINFO_HELP_SETFIELDDOUBLE,

		CORINFO_HELP_GETFIELDADDR,

		CORINFO_HELP_GETSTATICFIELDADDR_CONTEXT, // Helper for context-static fields
		CORINFO_HELP_GETSTATICFIELDADDR_TLS,     // Helper for PE TLS fields

		// There are a variety of specialized helpers for accessing static fields. The JIT should use 
		// ICorClassInfo::getSharedStaticsOrCCtorHelper to determine which helper to use

		// Helpers for regular statics
		CORINFO_HELP_GETGENERICS_GCSTATIC_BASE,
		CORINFO_HELP_GETGENERICS_NONGCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE_DYNAMICCLASS,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS,

		// Helper to class initialize shared generic with dynamicclass, but not get static field address
		CORINFO_HELP_CLASSINIT_SHARED_DYNAMICCLASS,

		// Helpers for thread statics
		CORINFO_HELP_GETGENERICS_GCTHREADSTATIC_BASE,
		CORINFO_HELP_GETGENERICS_NONGCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS,

		/* Debugger */

		CORINFO_HELP_DBG_IS_JUST_MY_CODE, // Check if this is "JustMyCode" and needs to be stepped through.

		/* Profiling enter/leave probe addresses */
		CORINFO_HELP_PROF_FCN_ENTER,    // record the entry to a method (caller)
		CORINFO_HELP_PROF_FCN_LEAVE,    // record the completion of current method (caller)
		CORINFO_HELP_PROF_FCN_TAILCALL, // record the completionof current method through tailcall (caller)

		/* Miscellaneous */

		CORINFO_HELP_BBT_FCN_ENTER, // record the entry to a method for collecting Tuning data

		CORINFO_HELP_PINVOKE_CALLI, // Indirect pinvoke call
		CORINFO_HELP_TAILCALL,      // Perform a tail call

		CORINFO_HELP_GETCURRENTMANAGEDTHREADID,

		CORINFO_HELP_INIT_PINVOKE_FRAME, // initialize an inlined PInvoke Frame for the JIT-compiler

		CORINFO_HELP_MEMSET, // Init block of memory
		CORINFO_HELP_MEMCPY, // Copy block of memory

		CORINFO_HELP_RUNTIMEHANDLE_METHOD,     // determine a type/field/method handle at run-time
		CORINFO_HELP_RUNTIMEHANDLE_METHOD_LOG, // determine a type/field/method handle at run-time, with IBC logging
		CORINFO_HELP_RUNTIMEHANDLE_CLASS,      // determine a type/field/method handle at run-time
		CORINFO_HELP_RUNTIMEHANDLE_CLASS_LOG,  // determine a type/field/method handle at run-time, with IBC logging

		// These helpers are required for MDIL backward compatibility only. They are not used by current JITed code.
		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPEHANDLE_OBSOLETE, // Convert from a TypeHandle (native structure pointer) to RuntimeTypeHandle at run-time
		CORINFO_HELP_METHODDESC_TO_RUNTIMEMETHODHANDLE_OBSOLETE, // Convert from a MethodDesc (native structure pointer) to RuntimeMethodHandle at run-time
		CORINFO_HELP_FIELDDESC_TO_RUNTIMEFIELDHANDLE_OBSOLETE, // Convert from a FieldDesc (native structure pointer) to RuntimeFieldHandle at run-time

		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE, // Convert from a TypeHandle (native structure pointer) to RuntimeType at run-time
		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL, // Convert from a TypeHandle (native structure pointer) to RuntimeType at run-time, the type may be null
		CORINFO_HELP_METHODDESC_TO_STUBRUNTIMEMETHOD, // Convert from a MethodDesc (native structure pointer) to RuntimeMethodHandle at run-time
		CORINFO_HELP_FIELDDESC_TO_STUBRUNTIMEFIELD, // Convert from a FieldDesc (native structure pointer) to RuntimeFieldHandle at run-time

		CORINFO_HELP_VIRTUAL_FUNC_PTR, // look up a virtual method at run-time
		//CORINFO_HELP_VIRTUAL_FUNC_PTR_LOG,  // look up a virtual method at run-time, with IBC logging

		// Not a real helpers. Instead of taking handle arguments, these helpers point to a small stub that loads the handle argument and calls the static helper.
		CORINFO_HELP_READYTORUN_NEW,
		CORINFO_HELP_READYTORUN_NEWARR_1,
		CORINFO_HELP_READYTORUN_ISINSTANCEOF,
		CORINFO_HELP_READYTORUN_CHKCAST,
		CORINFO_HELP_READYTORUN_STATIC_BASE,
		CORINFO_HELP_READYTORUN_VIRTUAL_FUNC_PTR,
		CORINFO_HELP_READYTORUN_GENERIC_HANDLE,
		CORINFO_HELP_READYTORUN_DELEGATE_CTOR,
		CORINFO_HELP_READYTORUN_GENERIC_STATIC_BASE,

		CORINFO_HELP_EE_PRESTUB, // Not real JIT helper. Used in native images.

		CORINFO_HELP_EE_PRECODE_FIXUP, // Not real JIT helper. Used for Precode fixup in native images.
		CORINFO_HELP_EE_PINVOKE_FIXUP, // Not real JIT helper. Used for PInvoke target fixup in native images.
		CORINFO_HELP_EE_VSD_FIXUP, // Not real JIT helper. Used for VSD cell fixup in native images.
		CORINFO_HELP_EE_EXTERNAL_FIXUP, // Not real JIT helper. Used for to fixup external method thunks in native images.
		CORINFO_HELP_EE_VTABLE_FIXUP, // Not real JIT helper. Used for inherited vtable slot fixup in native images.

		CORINFO_HELP_EE_REMOTING_THUNK, // Not real JIT helper. Used for remoting precode in native images.

		CORINFO_HELP_EE_PERSONALITY_ROUTINE, // Not real JIT helper. Used in native images.
		CORINFO_HELP_EE_PERSONALITY_ROUTINE_FILTER_FUNCLET, // Not real JIT helper. Used in native images to detect filter funclets.

		// ASSIGN_REF_EAX - CHECKED_ASSIGN_REF_EBP: NOGC_WRITE_BARRIERS JIT helper calls
		//
		// For unchecked versions EDX is required to point into GC heap.
		//
		// NOTE: these helpers are only used for x86.
		CORINFO_HELP_ASSIGN_REF_EAX, // EAX holds GC ptr, do a 'mov [EDX], EAX' and inform GC
		CORINFO_HELP_ASSIGN_REF_EBX, // EBX holds GC ptr, do a 'mov [EDX], EBX' and inform GC
		CORINFO_HELP_ASSIGN_REF_ECX, // ECX holds GC ptr, do a 'mov [EDX], ECX' and inform GC
		CORINFO_HELP_ASSIGN_REF_ESI, // ESI holds GC ptr, do a 'mov [EDX], ESI' and inform GC
		CORINFO_HELP_ASSIGN_REF_EDI, // EDI holds GC ptr, do a 'mov [EDX], EDI' and inform GC
		CORINFO_HELP_ASSIGN_REF_EBP, // EBP holds GC ptr, do a 'mov [EDX], EBP' and inform GC

		CORINFO_HELP_CHECKED_ASSIGN_REF_EAX, // These are the same as ASSIGN_REF above ...
		CORINFO_HELP_CHECKED_ASSIGN_REF_EBX, // ... but also check if EDX points into heap.
		CORINFO_HELP_CHECKED_ASSIGN_REF_ECX,
		CORINFO_HELP_CHECKED_ASSIGN_REF_ESI,
		CORINFO_HELP_CHECKED_ASSIGN_REF_EDI,
		CORINFO_HELP_CHECKED_ASSIGN_REF_EBP,

		CORINFO_HELP_LOOP_CLONE_CHOICE_ADDR, // Return the reference to a counter to decide to take cloned path in debug stress.
		CORINFO_HELP_DEBUG_LOG_LOOP_CLONING, // Print a message that a loop cloning optimization has occurred in debug mode.

		CORINFO_HELP_THROW_ARGUMENTEXCEPTION,           // throw ArgumentException
		CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION, // throw ArgumentOutOfRangeException
		CORINFO_HELP_THROW_PLATFORM_NOT_SUPPORTED,      // throw PlatformNotSupportedException
		CORINFO_HELP_THROW_TYPE_NOT_SUPPORTED,          // throw TypeNotSupportedException

		CORINFO_HELP_JIT_PINVOKE_BEGIN, // Transition to preemptive mode before a P/Invoke, frame is the first argument
		CORINFO_HELP_JIT_PINVOKE_END,   // Transition to cooperative mode after a P/Invoke, frame is the first argument

		CORINFO_HELP_JIT_REVERSE_PINVOKE_ENTER, // Transition to cooperative mode in reverse P/Invoke prolog, frame is the first argument
		CORINFO_HELP_JIT_REVERSE_PINVOKE_EXIT, // Transition to preemptive mode in reverse P/Invoke epilog, frame is the first argument

		CORINFO_HELP_GVMLOOKUP_FOR_SLOT, // Resolve a generic virtual method target from this pointer and runtime method handle 

		CORINFO_HELP_COUNT,
	}

	[Flags]
	public enum CorJitFlag : UInt32
	{
		CORJIT_FLAG_CALL_GETJITFLAGS = 0xffffffff, // Indicates that the JIT should retrieve flags in the form of a

		// pointer to a CORJIT_FLAGS value via ICorJitInfo::getJitFlags().
		CORJIT_FLAG_SPEED_OPT        = 0,
		CORJIT_FLAG_SIZE_OPT         = 1,
		CORJIT_FLAG_DEBUG_CODE       = 2, // generate "debuggable" code (no code-mangling optimizations)
		CORJIT_FLAG_DEBUG_EnC        = 3, // We are in Edit-n-Continue mode
		CORJIT_FLAG_DEBUG_INFO       = 4, // generate line and local-var info
		CORJIT_FLAG_MIN_OPT          = 5, // disable all jit optimizations (not necesarily debuggable code)
		CORJIT_FLAG_GCPOLL_CALLS     = 6, // Emit calls to JIT_POLLGC for thread suspension.
		CORJIT_FLAG_MCJIT_BACKGROUND = 7, // Calling from multicore JIT background thread, do not call JitComplete

#if (_TARGET_X64_)
		CORJIT_FLAG_PINVOKE_RESTORE_ESP = 8, // Restore ESP after returning from inlined PInvoke
		CORJIT_FLAG_TARGET_P4 = 9,
		CORJIT_FLAG_USE_FCOMI = 10, // Generated code may use fcomi(p) instruction
		CORJIT_FLAG_USE_CMOV = 11, // Generated code may use cmov instruction
		CORJIT_FLAG_USE_SSE2 = 12, // Generated code may use SSE-2 instructions

#elif (_TARGET_X86_)
			CORJIT_FLAG_UNUSED1 = 8,
			CORJIT_FLAG_UNUSED2 = 9,
			CORJIT_FLAG_UNUSED3 = 10,
			CORJIT_FLAG_UNUSED4 = 11,
			CORJIT_FLAG_UNUSED5 = 12,

#else
		CORJIT_FLAG_UNUSED6 = 13,
#endif

#if (_TARGET_X86_ || _TARGET_AMD64_) //_TARGET_AMD64_ not implemented
		CORJIT_FLAG_USE_AVX = 14,
		CORJIT_FLAG_USE_AVX2 = 15,
		CORJIT_FLAG_USE_AVX_512 = 16,

#else // !defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

		CORJIT_FLAG_UNUSED7 = 14,
		CORJIT_FLAG_UNUSED8 = 15,
		CORJIT_FLAG_UNUSED9 = 16,

#endif // !defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

#if (_TARGET_X86_ || _TARGET_AMD64_ || _TARGET_ARM64_) // _TARGET_AMD64_ and _TARGET_ARM64_ not implemented
		CORJIT_FLAG_FEATURE_SIMD = 17,
#else
		CORJIT_FLAG_UNUSED10 = 17,
#endif // !(defined(_TARGET_X86_) || defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))

		CORJIT_FLAG_MAKEFINALCODE          = 18, // Use the final code generator, i.e., not the interpreter.
		CORJIT_FLAG_READYTORUN             = 19, // Use version-resilient code generation
		CORJIT_FLAG_PROF_ENTERLEAVE        = 20, // Instrument prologues/epilogues
		CORJIT_FLAG_PROF_REJIT_NOPS        = 21, // Insert NOPs to ensure code is re-jitable
		CORJIT_FLAG_PROF_NO_PINVOKE_INLINE = 22, // Disables PInvoke inlining

		CORJIT_FLAG_SKIP_VERIFICATION =
			23, // (lazy) skip verification - determined without doing a full resolve. See comment below
		CORJIT_FLAG_PREJIT               = 24, // jit or prejit is the execution engine.
		CORJIT_FLAG_RELOC                = 25, // Generate relocatable code
		CORJIT_FLAG_IMPORT_ONLY          = 26, // Only import the function
		CORJIT_FLAG_IL_STUB              = 27, // method is an IL stub
		CORJIT_FLAG_PROCSPLIT            = 28, // JIT should separate code into hot and cold sections
		CORJIT_FLAG_BBINSTR              = 29, // Collect basic block profile information
		CORJIT_FLAG_BBOPT                = 30, // Optimize method based on profile information
		CORJIT_FLAG_FRAMED               = 31, // All methods have an EBP frame
		CORJIT_FLAG_ALIGN_LOOPS          = 32, // add NOPs before loops to align them at 16 byte boundaries
		CORJIT_FLAG_PUBLISH_SECRET_PARAM = 33, // JIT must place stub secret param into local 0.  (used by IL stubs)
		CORJIT_FLAG_GCPOLL_INLINE        = 34, // JIT must inline calls to GCPoll when possible

		CORJIT_FLAG_SAMPLING_JIT_BACKGROUND =
			35, // JIT is being invoked as a result of stack sampling for hot methods in the background

		CORJIT_FLAG_USE_PINVOKE_HELPERS =
			36, // The JIT should use the PINVOKE_{BEGIN,END} helpers instead of emitting inline transitions

		CORJIT_FLAG_REVERSE_PINVOKE =
			37, // The JIT should insert REVERSE_PINVOKE_{ENTER,EXIT} helpers into method prolog/epilog
		CORJIT_FLAG_DESKTOP_QUIRKS = 38, // The JIT should generate desktop-quirk-compatible code

		CORJIT_FLAG_TIER0 =
			39, // This is the initial tier for tiered compilation which should generate code as quickly as possible

		CORJIT_FLAG_TIER1 =
			40, // This is the final tier (for now) for tiered compilation which should generate high quality code

#if _TARGET_ARM_ //not implemented
		CORJIT_FLAG_RELATIVE_CODE_RELOCS =
 41, // JIT should generate PC-relative address computations instead of EE relocation records
#else // !defined(_TARGET_ARM_)
		CORJIT_FLAG_UNUSED11 = 41,
#endif // !defined(_TARGET_ARM_)

		CORJIT_FLAG_NO_INLINING = 42, // JIT should not inline any called method into this method

#if _TARGET_ARM64_ //not implemented
		CORJIT_FLAG_HAS_ARM64_AES = 43, // ID_AA64ISAR0_EL1.AES is 1 or better
		CORJIT_FLAG_HAS_ARM64_ATOMICS = 44, // ID_AA64ISAR0_EL1.Atomic is 2 or better
		CORJIT_FLAG_HAS_ARM64_CRC32 = 45, // ID_AA64ISAR0_EL1.CRC32 is 1 or better
		CORJIT_FLAG_HAS_ARM64_DCPOP = 46, // ID_AA64ISAR1_EL1.DPB is 1 or better
		CORJIT_FLAG_HAS_ARM64_DP = 47, // ID_AA64ISAR0_EL1.DP is 1 or better
		CORJIT_FLAG_HAS_ARM64_FCMA = 48, // ID_AA64ISAR1_EL1.FCMA is 1 or better
		CORJIT_FLAG_HAS_ARM64_FP = 49, // ID_AA64PFR0_EL1.FP is 0 or better
		CORJIT_FLAG_HAS_ARM64_FP16 = 50, // ID_AA64PFR0_EL1.FP is 1 or better
		CORJIT_FLAG_HAS_ARM64_JSCVT = 51, // ID_AA64ISAR1_EL1.JSCVT is 1 or better
		CORJIT_FLAG_HAS_ARM64_LRCPC = 52, // ID_AA64ISAR1_EL1.LRCPC is 1 or better
		CORJIT_FLAG_HAS_ARM64_PMULL = 53, // ID_AA64ISAR0_EL1.AES is 2 or better
		CORJIT_FLAG_HAS_ARM64_SHA1 = 54, // ID_AA64ISAR0_EL1.SHA1 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SHA2 = 55, // ID_AA64ISAR0_EL1.SHA2 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SHA512 = 56, // ID_AA64ISAR0_EL1.SHA2 is 2 or better
		CORJIT_FLAG_HAS_ARM64_SHA3 = 57, // ID_AA64ISAR0_EL1.SHA3 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SIMD = 58, // ID_AA64PFR0_EL1.AdvSIMD is 0 or better
		CORJIT_FLAG_HAS_ARM64_SIMD_V81 = 59, // ID_AA64ISAR0_EL1.RDM is 1 or better
		CORJIT_FLAG_HAS_ARM64_SIMD_FP16 = 60, // ID_AA64PFR0_EL1.AdvSIMD is 1 or better
		CORJIT_FLAG_HAS_ARM64_SM3 = 61, // ID_AA64ISAR0_EL1.SM3 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SM4 = 62, // ID_AA64ISAR0_EL1.SM4 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SVE = 63  // ID_AA64PFR0_EL1.SVE is 1 or better

#elif (_TARGET_X86_ || _TARGET_AMD64_) //_TARGET_AMD64_ not implemented
		CORJIT_FLAG_USE_SSE3 = 43,
		CORJIT_FLAG_USE_SSSE3 = 44,
		CORJIT_FLAG_USE_SSE41 = 45,
		CORJIT_FLAG_USE_SSE42 = 46,
		CORJIT_FLAG_USE_AES = 47,
		CORJIT_FLAG_USE_BMI1 = 48,
		CORJIT_FLAG_USE_BMI2 = 49,
		CORJIT_FLAG_USE_FMA = 50,
		CORJIT_FLAG_USE_LZCNT = 51,
		CORJIT_FLAG_USE_PCLMULQDQ = 52,
		CORJIT_FLAG_USE_POPCNT = 53,
		CORJIT_FLAG_UNUSED23 = 54,
		CORJIT_FLAG_UNUSED24 = 55,
		CORJIT_FLAG_UNUSED25 = 56,
		CORJIT_FLAG_UNUSED26 = 57,
		CORJIT_FLAG_UNUSED27 = 58,
		CORJIT_FLAG_UNUSED28 = 59,
		CORJIT_FLAG_UNUSED29 = 60,
		CORJIT_FLAG_UNUSED30 = 61,
		CORJIT_FLAG_UNUSED31 = 62,
		CORJIT_FLAG_UNUSED32 = 63


#else // !defined(_TARGET_ARM64_) &&!defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

		CORJIT_FLAG_UNUSED12 = 43,
		CORJIT_FLAG_UNUSED13 = 44,
		CORJIT_FLAG_UNUSED14 = 45,
		CORJIT_FLAG_UNUSED15 = 46,
		CORJIT_FLAG_UNUSED16 = 47,
		CORJIT_FLAG_UNUSED17 = 48,
		CORJIT_FLAG_UNUSED18 = 49,
		CORJIT_FLAG_UNUSED19 = 50,
		CORJIT_FLAG_UNUSED20 = 51,
		CORJIT_FLAG_UNUSED21 = 52,
		CORJIT_FLAG_UNUSED22 = 53,
		CORJIT_FLAG_UNUSED23 = 54,
		CORJIT_FLAG_UNUSED24 = 55,
		CORJIT_FLAG_UNUSED25 = 56,
		CORJIT_FLAG_UNUSED26 = 57,
		CORJIT_FLAG_UNUSED27 = 58,
		CORJIT_FLAG_UNUSED28 = 59,
		CORJIT_FLAG_UNUSED29 = 60,
		CORJIT_FLAG_UNUSED30 = 61,
		CORJIT_FLAG_UNUSED31 = 62,
		CORJIT_FLAG_UNUSED32 = 63

#endif // !defined(_TARGET_ARM64_) &&!defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)
	};

	//from coreinfo.h
	// these are the attribute flags for fields and methods (getMethodAttribs)
	public enum CorInfoFlag : UInt32
	{
		//  CORINFO_FLG_UNUSED                = 0x00000001,
		//  CORINFO_FLG_UNUSED                = 0x00000002,
		CORINFO_FLG_PROTECTED = 0x00000004,
		CORINFO_FLG_STATIC    = 0x00000008,
		CORINFO_FLG_FINAL     = 0x00000010,
		CORINFO_FLG_SYNCH     = 0x00000020,
		CORINFO_FLG_VIRTUAL   = 0x00000040,

		//  CORINFO_FLG_UNUSED                = 0x00000080,
		CORINFO_FLG_NATIVE         = 0x00000100,
		CORINFO_FLG_INTRINSIC_TYPE = 0x00000200, // This type is marked by [Intrinsic]
		CORINFO_FLG_ABSTRACT       = 0x00000400,

		CORINFO_FLG_EnC = 0x00000800, // member was added by Edit'n'Continue

		// These are internal flags that can only be on methods
		CORINFO_FLG_FORCEINLINE = 0x00010000, // The method should be inlined if possible.

		CORINFO_FLG_SHAREDINST =
			0x00020000, // the code for this method is shared between different generic instantiations (also set on classes/types)
		CORINFO_FLG_DELEGATE_INVOKE = 0x00040000, // "Delegate
		CORINFO_FLG_PINVOKE         = 0x00080000, // Is a P/Invoke call

		CORINFO_FLG_SECURITYCHECK =
			0x00100000, // Is one of the security routines that does a stackwalk (e.g. Assert, Demand)
		CORINFO_FLG_NOGCCHECK   = 0x00200000, // This method is FCALL that has no GC check.  Don't put alone in loops
		CORINFO_FLG_INTRINSIC   = 0x00400000, // This method MAY have an intrinsic ID
		CORINFO_FLG_CONSTRUCTOR = 0x00800000, // This method is an instance or type initializer

		//  CORINFO_FLG_UNUSED                = 0x01000000,
		//  CORINFO_FLG_UNUSED                = 0x02000000,
		CORINFO_FLG_NOSECURITYWRAP = 0x04000000, // The method requires no security checks
		CORINFO_FLG_DONT_INLINE    = 0x10000000, // The method should not be inlined

		CORINFO_FLG_DONT_INLINE_CALLER =
			0x20000000, // The method should not be inlined, nor should its callers. It cannot be tail called.
		CORINFO_FLG_JIT_INTRINSIC = 0x40000000, // Method is a potential jit intrinsic; verify identity by name check

		// These are internal flags that can only be on Classes
		CORINFO_FLG_VALUECLASS = 0x00010000, // is the class a value class

		//  This flag is define din the Methods section, but is also valid on classes.
		//  CORINFO_FLG_SHAREDINST            = 0x00020000, // This class is satisfies TypeHandle::IsCanonicalSubtype
		CORINFO_FLG_VAROBJSIZE         = 0x00040000, // the object size varies depending of constructor args
		CORINFO_FLG_ARRAY              = 0x00080000, // class is an array class (initialized differently)
		CORINFO_FLG_OVERLAPPING_FIELDS = 0x00100000, // struct or class has fields that overlap (aka union)
		CORINFO_FLG_INTERFACE          = 0x00200000, // it is an interface
		CORINFO_FLG_CONTEXTFUL         = 0x00400000, // is this a contextful class?
		CORINFO_FLG_CUSTOMLAYOUT       = 0x00800000, // does this struct have custom layout?
		CORINFO_FLG_CONTAINS_GC_PTR    = 0x01000000, // does the class contain a gc ptr ?
		CORINFO_FLG_DELEGATE           = 0x02000000, // is this a subclass of delegate or multicast delegate ?
		CORINFO_FLG_MARSHAL_BYREF      = 0x04000000, // is this a subclass of MarshalByRef ?
		CORINFO_FLG_CONTAINS_STACK_PTR = 0x08000000, // This class has a stack pointer inside it
		CORINFO_FLG_VARIANCE           = 0x10000000, // MethodTable::HasVariance (sealed does *not* mean uncast-able)

		CORINFO_FLG_BEFOREFIELDINIT =
			0x20000000, // Additional flexibility for when to run .cctor (see code:#ClassConstructionFlags)
		CORINFO_FLG_GENERIC_TYPE_VARIABLE = 0x40000000, // This is really a handle for a variable type
		CORINFO_FLG_UNSAFE_VALUECLASS     = 0x80000000, // Unsafe (C++'s /GS) value type
		FLG_CCTOR                         = (CORINFO_FLG_CONSTRUCTOR | CORINFO_FLG_STATIC)
	};

	public enum CorInfoIsAccessAllowedResult
	{
		CORINFO_ACCESS_ALLOWED       = 0, // Call allowed
		CORINFO_ACCESS_ILLEGAL       = 1, // Call not allowed
		CORINFO_ACCESS_RUNTIME_CHECK = 2, // Ask at runtime whether to allow the call or not
	}

	public enum CorinfoThisTransform
	{
		CORINFO_NO_THIS_TRANSFORM,
		CORINFO_BOX_THIS,
		CORINFO_DEREF_THIS
	}

	public enum CorinfoCallKind
	{
		CORINFO_CALL,
		CORINFO_CALL_CODE_POINTER,
		CORINFO_VIRTUALCALL_STUB,
		CORINFO_VIRTUALCALL_LDVIRTFTN,
		CORINFO_VIRTUALCALL_VTABLE
	}

	public enum CorInfoCallConv
	{
		// These correspond to CorCallingConvention

		CORINFO_CALLCONV_DEFAULT      = 0x0,
		CORINFO_CALLCONV_C            = 0x1,
		CORINFO_CALLCONV_STDCALL      = 0x2,
		CORINFO_CALLCONV_THISCALL     = 0x3,
		CORINFO_CALLCONV_FASTCALL     = 0x4,
		CORINFO_CALLCONV_VARARG       = 0x5,
		CORINFO_CALLCONV_FIELD        = 0x6,
		CORINFO_CALLCONV_LOCAL_SIG    = 0x7,
		CORINFO_CALLCONV_PROPERTY     = 0x8,
		CORINFO_CALLCONV_NATIVEVARARG = 0xb, // used ONLY for IL stub PInvoke vararg calls

		CORINFO_CALLCONV_MASK         = 0x0f, // Calling convention is bottom 4 bits
		CORINFO_CALLCONV_GENERIC      = 0x10,
		CORINFO_CALLCONV_HASTHIS      = 0x20,
		CORINFO_CALLCONV_EXPLICITTHIS = 0x40,
		CORINFO_CALLCONV_PARAMTYPE    = 0x80, // Passed last. Same as CORINFO_GENERICS_CTXT_FROM_PARAMTYPEARG
	};

	public enum CorInfoRegionKind : UInt32
	{
		CORINFO_REGION_NONE,
		CORINFO_REGION_HOT,
		CORINFO_REGION_COLD,
		CORINFO_REGION_JIT,
	};

	public enum CorInfoTokenKind
	{
		CORINFO_TOKENKIND_Class  = 0x01,
		CORINFO_TOKENKIND_Method = 0x02,
		CORINFO_TOKENKIND_Field  = 0x04,
		CORINFO_TOKENKIND_Mask   = 0x07,

		// token comes from CEE_LDTOKEN
		CORINFO_TOKENKIND_Ldtoken = 0x10 | CORINFO_TOKENKIND_Class | CORINFO_TOKENKIND_Method | CORINFO_TOKENKIND_Field,

		// token comes from CEE_CASTCLASS or CEE_ISINST
		CORINFO_TOKENKIND_Casting = 0x20 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWARR
		CORINFO_TOKENKIND_Newarr = 0x40 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_BOX
		CORINFO_TOKENKIND_Box = 0x80 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_CONSTRAINED
		CORINFO_TOKENKIND_Constrained = 0x100 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWOBJ
		CORINFO_TOKENKIND_NewObj = 0x200 | CORINFO_TOKENKIND_Method,

		// token comes from CEE_LDVIRTFTN
		CORINFO_TOKENKIND_Ldvirtftn = 0x400 | CORINFO_TOKENKIND_Method,
	}

	[Flags]
	public enum CorCallingConvention
	{
		// IMAGE_CEE_CS_CALLCONV

		Default = 0x0,

		Vararg    = 0x5,
		Field     = 0x6,
		LocalSig  = 0x7,
		Property  = 0x8,
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