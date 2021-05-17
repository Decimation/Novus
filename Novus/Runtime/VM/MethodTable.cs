using System;
using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Memory;
using Novus.Runtime.VM.EE;

// ReSharper disable StructCanBeMadeReadOnly
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Novus.Runtime.VM
{
	
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MethodTable
	{
		static MethodTable()
		{
			Resource.LoadImports(typeof(MethodTable));
		}

		internal short ComponentSize { get; }

		internal GenericsFlags GenericsFlags { get; }

		internal int BaseSize { get; }

		internal OptionalSlotsFlags SlotsFlags { get; }

		internal short RawToken { get; }

		internal short NumVirtuals { get; }

		internal short NumInterfaces { get; }

		internal void* Parent { get; }

		internal void* Module { get; }

		internal void* WriteableData { get; }

		internal TypeFlags TypeFlags
		{
			get
			{
				fixed (MethodTable* ptr = &this) {
					return (TypeFlags) (*(int*) ptr);
				}
			}
		}

		/// <summary>
		///     Describes what the union at offset of <see cref="Union1" />
		///     contains.
		/// </summary>
		internal UnionType UnionType
		{
			get
			{
				const long UNION_MASK = 3;

				long l = (long) Union1;
				return (UnionType) (l & UNION_MASK);
			}
		}

		internal Pointer<EEClass> EEClass
		{
			get
			{

				fixed (MethodTable* p = &this) {

					return Func_GetClass(p);
				}
			}
		}


		/// <summary>
		///     <para>Union 1</para>
		///     <para><see cref="EEClass" /></para>
		///     <para><see cref="CanonicalMethodTable" /></para>
		/// </summary>
		private void* Union1 { get; }


		internal Pointer<MethodTable> CanonicalMethodTable
		{
			get
			{
				//todo
				throw new NotImplementedException();
			}
		}

		internal Pointer<EEClassNativeLayoutInfo> NativeLayoutInfo
		{
			get
			{
				fixed (MethodTable* p = &this) {
					return Func_GetNativeLayoutInfo(p);
				}
			}
		}

		/// <summary>
		///     <para>Union 2</para>
		///     <para><see cref="PerInstInfo" /></para>
		///     <para><see cref="ElementTypeHandle" /></para>
		///     <para><see cref="MultipurposeSlot1" /></para>
		/// </summary>
		private void* Union2 { get; }

		internal Pointer<byte> PerInstInfo => Union2;

		internal Pointer<MethodTable> ElementTypeHandle => Union2;

		internal Pointer<byte> MultipurposeSlot1 => Union2;

		/// <summary>
		///     <para>Union 3</para>
		///     <para><see cref="InterfaceMap" /></para>
		///     <para><see cref="MultipurposeSlot2" /></para>
		/// </summary>
		private void* Union3 { get; }

		internal Pointer<byte> InterfaceMap => Union3;

		internal Pointer<byte> MultipurposeSlot2 => Union3;

		/// <summary>
		/// <see cref="MethodTable.EEClass"/>
		/// </summary>
		[field: ImportClr("Sig_GetEEClass")]
		private static delegate* unmanaged<MethodTable*, EEClass*> Func_GetClass { get; }

		/// <summary>
		/// <see cref="MethodTable.NativeLayoutInfo"/>
		/// </summary>
		[field: ImportClr("Sig_GetNativeLayoutInfo")]
		private static delegate* unmanaged<MethodTable*, EEClassNativeLayoutInfo*> Func_GetNativeLayoutInfo { get; }
	}


	/// <summary>
	///     <remarks>
	///         <para>Alias: High flags</para>
	///         <para>Use with <see cref="MethodTable.TypeFlags" /></para>
	///     </remarks>
	/// </summary>
	[Flags]
	public enum TypeFlags : uint
	{
		Mask             = 0x000F0000,
		Class            = 0x00000000,
		Unused1          = 0x00010000,
		MarshalByRefMask = 0x000E0000,
		MarshalByRef     = 0x00020000,

		/// <summary>
		///     sub-category of MarshalByRef
		/// </summary>
		Contextful = 0x00030000,

		ValueType     = 0x00040000,
		ValueTypeMask = 0x000C0000,

		/// <summary>
		///     sub-category of ValueType
		/// </summary>
		Nullable = 0x00050000,

		/// <summary>
		///     sub-category of ValueType, Enum or primitive value type
		/// </summary>
		PrimitiveValueType = 0x00060000,

		/// <summary>
		///     sub-category of ValueType, Primitive (ELEMENT_TYPE_I, etc.)
		/// </summary>
		TruePrimitive = 0x00070000,

		Array     = 0x00080000,
		ArrayMask = 0x000C0000,

		/// <summary>
		///     sub-category of Array
		/// </summary>
		IfArrayThenSzArray = 0x00020000,

		Interface        = 0x000C0000,
		Unused2          = 0x000D0000,
		TransparentProxy = 0x000E0000,
		AsyncPin         = 0x000F0000,

		/// <summary>
		///     bits that matter for element type mask
		/// </summary>
		ElementTypeMask = 0x000E0000,

		/// <summary>
		///     instances require finalization
		/// </summary>
		HasFinalizer = 0x00100000,

		/// <summary>
		///     Is this type marshalable by the pinvoke marshalling layer
		/// </summary>
		IfNotInterfaceThenMarshalable = 0x00200000,

		/// <summary>
		///     Does the type has optional GuidInfo
		/// </summary>
		IfInterfaceThenHasGuidInfo = 0x00200000,

		/// <summary>
		///     class implements ICastable interface
		/// </summary>
		ICastable = 0x00400000,

		/// <summary>
		///     m_pParentMethodTable has double indirection
		/// </summary>
		HasIndirectParent = 0x00800000,

		ContainsPointers = 0x01000000,

		/// <summary>
		///     can be equivalent to another type
		/// </summary>
		HasTypeEquivalence = 0x02000000,

		/// <summary>
		///     has optional pointer to RCWPerTypeData
		/// </summary>
		HasRCWPerTypeData = 0x04000000,

		/// <summary>
		///     finalizer must be run on Appdomain Unload
		/// </summary>
		HasCriticalFinalizer = 0x08000000,

		Collectible              = 0x10000000,
		ContainsGenericVariables = 0x20000000,

		/// <summary>
		///     class is a com object
		/// </summary>
		ComObject = 0x40000000,

		/// <summary>
		///     This is set if component size is used for flags.
		/// </summary>
		HasComponentSize = 0x80000000,

		/// <summary>
		///     Types that require non-trivial interface cast have this bit set in the category
		/// </summary>
		NonTrivialInterfaceCast = Array | ComObject | ICastable
	}

	/// <summary>
	///     <remarks>
	///         <para>Alias: flags 2</para>
	///         <para>Use with <see cref="MethodTable.SlotsFlags" /></para>
	///     </remarks>
	/// </summary>
	[Flags]
	public enum OptionalSlotsFlags : ushort
	{
		MultipurposeSlotsMask    = 0x001F,
		HasPerInstInfo           = 0x0001,
		HasInterfaceMap          = 0x0002,
		HasDispatchMapSlot       = 0x0004,
		HasNonVirtualSlots       = 0x0008,
		HasModuleOverride        = 0x0010,
		IsZapped                 = 0x0020,
		IsPreRestored            = 0x0040,
		HasModuleDependencies    = 0x0080,
		IsIntrinsicType          = 0x0100,
		RequiresDispatchTokenFat = 0x0200,
		HasCctor                 = 0x0400,
		HasCCWTemplate           = 0x0800,

		/// <summary>
		///     Type requires 8-byte alignment (only set on platforms that require this and don't get it implicitly)
		/// </summary>
		RequiresAlign8 = 0x1000,

		HasBoxedRegularStatics                = 0x2000,
		HasSingleNonVirtualSlot               = 0x4000,
		DependsOnEquivalentOrForwardedStructs = 0x8000
	}

	/// <summary>
	///     <remarks>
	///         <para>Alias: low flags</para>
	///         <para>Use with <see cref="MethodTable.GenericsFlags" /></para>
	///     </remarks>
	/// </summary>
	[Flags]
	public enum GenericsFlags : ushort
	{
		// We are overloading the low 2 bytes of m_dwFlags to be a component size for Strings
		// and Arrays and some set of flags which we can be assured are of a specified state
		// for Strings / Arrays, currently these will be a bunch of generics flags which don't
		// apply to Strings / Arrays.

		UnusedComponentSize1 = 0x00000001,

		StaticsMask                           = 0x00000006,
		StaticsMask_NonDynamic                = 0x00000000,
		StaticsMask_Dynamic                   = 0x00000002, // dynamic statics (EnC, reflection.emit)
		StaticsMask_Generics                  = 0x00000004, // generics statics
		StaticsMask_CrossModuleGenerics       = 0x00000006, // cross module generics statics (NGen)
		StaticsMask_IfGenericsThenCrossModule = 0x00000002, // helper constant to get rid of unnecessary check

		NotInPZM = 0x00000008, // True if this type is not in its PreferredZapModule

		GenericsMask             = 0x00000030,
		GenericsMask_NonGeneric  = 0x00000000, // no instantiation
		GenericsMask_GenericInst = 0x00000010, // regular instantiation, e.g. List<String>

		GenericsMask_SharedInst  = 0x00000020, // shared instantiation, e.g. List<__Canon> or List<MyValueType<__Canon>>
		GenericsMask_TypicalInst = 0x00000030, // the type instantiated at its formal parameters, e.g. List<T>

		HasRemotingVtsInfo = 0x00000080, // Optional data present indicating VTS methods and optional fields

		HasVariance = 0x00000100, // This is an instantiated type some of whose type parameters are co or contra-variant

		HasDefaultCtor = 0x00000200,

		HasPreciseInitCctors =
			0x00000400, // Do we need to run class constructors at allocation time? (Not perf important, could be moved to EEClass

		IsHFA = 0x00000800, // This type is an HFA (Homogenous Floating-point Aggregate)

		IsRegStructPassed = 0x00000800, // This type is a System V register passed struct.

		IsByRefLike = 0x00001000,

		// In a perfect world we would fill these flags using other flags that we already have
		// which have a constant value for something which has a component size.
		UnusedComponentSize5 = 0x00002000,
		UnusedComponentSize6 = 0x00004000,
		UnusedComponentSize7 = 0x00008000,

		StringArrayValues = (StaticsMask_NonDynamic  & 0xFFFF) |
		                    (NotInPZM                & 0)      |
		                    (GenericsMask_NonGeneric & 0xFFFF) |
		                    (HasVariance             & 0)      |
		                    (HasDefaultCtor          & 0)      |
		                    (HasPreciseInitCctors    & 0)
	}

	/// <summary>
	///     The value of lowest two bits describe what the union contains
	///     <remarks>
	///         Use with <see cref="UnionType" />
	///     </remarks>
	/// </summary>
	public enum UnionType
	{
		/// <summary>
		///     0 - pointer to <see cref="EEClass" />
		///     This <see cref="MethodTable" /> is the canonical method table.
		/// </summary>
		EEClass = 0,

		/// <summary>
		///     1 - not used
		/// </summary>
		Invalid = 1,

		/// <summary>
		///     2 - pointer to canonical <see cref="MethodTable" />.
		/// </summary>
		MethodTable = 2,

		/// <summary>
		///     3 - pointer to indirection cell that points to canonical <see cref="MethodTable" />.
		///     (used only if FEATURE_PREJIT is defined)
		/// </summary>
		Indirection = 3
	}
}