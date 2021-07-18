using Novus.Imports;
using Novus.Memory;
using System;
using System.Runtime.InteropServices;
using Novus.Runtime.VM.IL;
using Kantan.Diagnostics;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Novus.Runtime.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MethodDesc
	{
		static MethodDesc()
		{
			Global.Clr.LoadImports(typeof(MethodDesc));
		}

		internal MethodClassification Classification =>
			(MethodClassification) ((ushort) Properties & (ushort) MethodProperties.Classification);

		internal ParamFlags Flags3AndTokenRemainder { get; }

		internal byte ChunkIndex { get; }

		internal CodeFlags CodeFlags { get; }

		internal ushort SlotNumber { get; }

		internal MethodProperties Properties { get; }

		/// <summary>
		///     Valid only if the function is non-virtual,
		///     non-abstract, non-generic (size of this <see cref="MethodDesc"/> <c>== 16</c>)
		/// </summary>
		internal void* Function { get; }

		internal bool IsPointingToNativeCode
		{
			get
			{
				fixed (MethodDesc* p = &this) {
					var mt = Func_IsPointingToNativeCode(p);
					return Convert.ToBoolean(mt);
				}
			}
		}

		internal void* NativeCode
		{
			get
			{
				fixed (MethodDesc* p = &this) {
					return Func_GetNativeCode(p);
				}
			}
		}

		internal int Token
		{
			get
			{
				fixed (MethodDesc* p = &this) {
					return Func_GetToken(p);
				}
			}
		}


		internal long RVA
		{
			get
			{
				fixed (MethodDesc* p = &this) {
					return Func_GetRVA(p);
				}
			}
		}

		private static int Alignment
		{
			get
			{
				//int alignmentShift = 3;
				int alignmentShift = Mem.Is64Bit ? 3 : 2;
				int alignment      = 1 << alignmentShift;
				int alignmentMask  = alignment - 1;

				return alignment;
			}
		}

		internal CorILMethod* ILHeader
		{
			get
			{
				fixed (MethodDesc* p = &this) {
					const int fAllowOverrides = 0;
					return Func_GetILHeader(p, fAllowOverrides);
				}
			}
		}

		internal Pointer<MethodDescChunk> MethodDescChunk
		{
			get
			{
				// PTR_MethodDescChunk(dac_cast<TADDR>(this) -(sizeof(MethodDescChunk) + (GetMethodDescIndex() * MethodDesc::ALIGNMENT)));

				var thisptr = Mem.AddressOf(ref this).Cast();
				return (thisptr - (sizeof(MethodDescChunk) + (ChunkIndex * Alignment)));
			}
		}

		internal void Reset()
		{
			fixed (MethodDesc* p = &this) {
				Func_Reset(p);
			}
		}

		internal bool SetEntryPoint(void* v)
		{
			fixed (MethodDesc* p = &this) {
				return Func_SetEntryPoint(p, v) != 0;
			}
		}

		internal Pointer<MethodTable> MethodTable => MethodDescChunk.Reference.MethodTable;

		/// <summary>
		/// <see cref="SetEntryPoint"/>
		/// </summary>
		[field: ImportClr("Sig_SetEntryPoint")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, void*, int> Func_SetEntryPoint { get; }

		/// <summary>
		/// <see cref="MethodDesc.IsPointingToNativeCode"/>
		/// </summary>
		[field: ImportClr("Sig_IsPointingToNativeCode")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, int> Func_IsPointingToNativeCode { get; }

		/// <summary>
		/// <see cref="MethodDesc.NativeCode"/>
		/// </summary>
		[field: ImportClr("Sig_GetNativeCode")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, void*> Func_GetNativeCode { get; }

		/// <summary>
		/// <see cref="MethodDesc.Token"/>
		/// </summary>
		[field: ImportClr("Sig_GetMemberDef")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, int> Func_GetToken { get; }

		/// <summary>
		/// <see cref="MethodDesc.RVA"/>
		/// </summary>
		[field: ImportClr("Sig_GetRVA")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, int> Func_GetRVA { get; }


		/// <summary>
		/// <see cref="MethodDesc.Reset"/>
		/// </summary>
		[field: ImportClr("Sig_Reset")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, void> Func_Reset { get; }

		/// <summary>
		/// <see cref="ILHeader"/>
		/// </summary>
		[field: ImportClr("Sig_GetIL")]
		private static delegate* unmanaged[Thiscall]<MethodDesc*, int, CorILMethod*> Func_GetILHeader { get; }
	}

	/// <summary>
	///     Describes <see cref="MethodDesc" /> JIT/entry point status
	/// </summary>
	[Flags]
	public enum CodeFlags : byte
	{
		/// <summary>
		///     The method entry point is stable (either precode or actual code)
		/// </summary>
		HasStableEntryPoint = 0x01,

		/// <summary>
		///     implies that HasStableEntryPoint is set.
		///     Precode has been allocated for this method
		/// </summary>
		HasPrecode = 0x02,

		IsUnboxingStub = 0x04,

		/// <summary>
		///     Has slot for native code
		/// </summary>
		HasNativeCodeSlot = 0x08,

		/// <summary>
		///     Jit may expand method as an intrinsic
		/// </summary>
		IsJitIntrinsic = 0x10
	}

	/// <summary>
	///     Describes <see cref="MethodDesc" /> parameters
	/// </summary>
	[Flags]
	public enum ParamFlags : ushort
	{
		TokenRemainderMask = 0x3FFF,

		// These are separate to allow the flags space available and used to be obvious here
		// and for the logic that splits the token to be algorithmically generated based on the
		// #define

		/// <summary>
		///     Indicates that a type-forwarded type is used as a valuetype parameter (this flag is only valid for ngenned items)
		/// </summary>
		HasForwardedValueTypeParameter = 0x4000,

		/// <summary>
		///     Indicates that all typeref's in the signature of the method have been resolved to typedefs (or that process failed)
		///     (this flag is only valid for non-ngenned methods)
		/// </summary>
		ValueTypeParametersWalked = 0x4000,

		/// <summary>
		///     Indicates that we have verified that there are no equivalent valuetype parameters for this method
		/// </summary>
		DoesNotHaveEquivalentValueTypeParameters = 0x8000
	}

	/// <summary>
	///     Describes the type of <see cref="MethodDesc" />
	/// </summary>
	public enum MethodClassification
	{
		/// <summary>
		///     IL
		/// </summary>
		IL = 0,

		/// <summary>
		///     FCall(also includes tlbimped ctor, Delegate ctor)
		/// </summary>
		FCall = 1,

		/// <summary>
		///     N/Direct
		/// </summary>
		NDirect = 2,

		/// <summary>
		///     Special method; implementation provided by EE (like Delegate Invoke)
		/// </summary>
		EEImpl = 3,

		/// <summary>
		///     Array ECall
		/// </summary>
		Array = 4,

		/// <summary>
		///     Instantiated generic methods, including descriptors
		///     for both shared and unshared code (see InstantiatedMethodDesc)
		/// </summary>
		Instantiated = 5,

		//#ifdef FEATURE_COMINTEROP
		// This needs a little explanation.  There are MethodDescs on MethodTables
		// which are Interfaces.  These have the mdcInterface bit set.  Then there
		// are MethodDescs on MethodTables that are Classes, where the method is
		// exposed through an interface.  These do not have the mdcInterface bit set.
		//
		// So, today, a dispatch through an 'mdcInterface' MethodDesc is either an
		// error (someone forgot to look up the method in a class' VTable) or it is
		// a case of COM Interop.

		ComInterop = 6,

		//#endif                 // FEATURE_COMINTEROP

		/// <summary>
		///     For <see cref="MethodDesc" /> with no metadata behind
		/// </summary>
		Dynamic = 7,

		Count
	}

	/// <summary>
	///     Describes the type and properties of a <see cref="MethodDesc" />
	/// </summary>
	[Flags]
	public enum MethodProperties : ushort
	{
		/// <summary>
		///     Method is <see cref="MethodClassification.IL" />, <see cref="MethodClassification.FCall" /> etc., see
		///     <see cref="MethodClassification" /> above.
		/// </summary>
		Classification = 0x0007,

		ClassificationCount = Classification + 1,

		// Note that layout of code:MethodDesc::s_ClassificationSizeTable depends on the exact values
		// of mdcHasNonVtableSlot and mdcMethodImpl

		/// <summary>
		///     Has local slot (vs. has real slot in MethodTable)
		/// </summary>
		HasNonVtableSlot = 0x0008,

		/// <summary>
		///     Method is a body for a method impl (MI_MethodDesc, MI_NDirectMethodDesc, etc)
		///     where the function explicitly implements IInterface.foo() instead of foo().
		/// </summary>
		MethodImpl = 0x0010,

		/// <summary>
		///     Method is static
		/// </summary>
		Static = 0x0020,

		// unused                           = 0x0040,
		// unused                           = 0x0080,
		// unused                           = 0x0100,
		// unused                           = 0x0200,

		// Duplicate method. When a method needs to be placed in multiple slots in the
		// method table, because it could not be packed into one slot. For eg, a method
		// providing implementation for two interfaces, MethodImpl, etc
		Duplicate = 0x0400,

		/// <summary>
		///     Has this method been verified?
		/// </summary>
		VerifiedState = 0x0800,

		/// <summary>
		///     Is the method verifiable? It needs to be verified first to determine this
		/// </summary>
		Verifiable = 0x1000,

		/// <summary>
		///     Is this method ineligible for inlining?
		/// </summary>
		NotInline = 0x2000,

		/// <summary>
		///     Is the method synchronized
		/// </summary>
		Synchronized = 0x4000,

		/// <summary>
		///     Does the method's slot number require all 16 bits
		/// </summary>
		RequiresFullSlotNumber = 0x8000
	}
}