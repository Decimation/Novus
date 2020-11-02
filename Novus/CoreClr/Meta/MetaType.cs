using System;
using System.Reflection;
using Novus.CoreClr.VM;
using Novus.CoreClr.VM.EE;
using Novus.Memory;
using SimpleCore.Diagnostics;


#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0602 // Delegate on struct instance caused a boxing allocation
#pragma warning disable HAA0603 // Delegate allocation from a method group
#pragma warning disable HAA0604 // Delegate allocation from a method group

#pragma warning disable HAA0501 // Explicit new array type allocation
#pragma warning disable HAA0502 // Explicit new reference type allocation
#pragma warning disable HAA0503 // Explicit new reference type allocation
#pragma warning disable HAA0504 // Implicit new array creation allocation
#pragma warning disable HAA0505 // Initializer reference type allocation
#pragma warning disable HAA0506 // Let clause induced allocation

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0302 // Display class allocation to capture closure
#pragma warning disable HAA0303 // Lambda or anonymous method in a generic method allocates a delegate instance


namespace Novus.CoreClr.Meta
{
	public static class MetaExtensions
	{
		public static MetaField AsMetaField(this FieldInfo t) => new MetaField(t);
		public static MetaType AsMetaType(this Type t) => new MetaType(t);
	}

	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="MethodTable"/>, <see cref="EEClass"/>, and
	/// 		<see cref="TypeHandle"/></description></item>
	///         <item><description>Reflection structure: <see cref="Type"/></description></item>
	///     </list>
	/// </summary>
	public unsafe class MetaType : StandardClrStructure<MethodTable>
	{
		#region MethodTable

		public  byte             ArrayRank         => EEClass.Reference.ArrayRank;
		public  int              BaseSize          => Value.Reference.BaseSize;
		public  MetaType          Canon             => Value.Reference.CanonicalMethodTable;
		public  short            ComponentSize     => Value.Reference.ComponentSize;
		private Pointer<EEClass> EEClass           => Value.Reference.EEClass;
		public  MetaType          ElementTypeHandle => Value.Reference.ElementTypeHandle;
		public  GenericsFlags    GenericFlags      => Value.Reference.GenericsFlags;

		public Pointer<byte> InterfaceMap    => Value.Reference.InterfaceMap;
		public short         InterfacesCount => Value.Reference.NumInterfaces;
		public Pointer<byte> Module          => Value.Reference.Module;
		public MetaType       Parent          => (Pointer<MethodTable>) Value.Reference.Parent;

		public Pointer<byte> PerInstInfo => Value.Reference.PerInstInfo;

		//		public CorElementType ArrayElementType => EEClass.Reference.ArrayElementType;
		public Type RuntimeType { get; }

		public OptionalSlotsFlags SlotFlags => Value.Reference.SlotsFlags;


		public OptionalSlotsFlags SlotsFlags    => Value.Reference.SlotsFlags;
		public TypeFlags          TypeFlags     => Value.Reference.TypeFlags;
		public short              VirtualsCount => Value.Reference.NumVirtuals;
		public Pointer<byte>      WriteableData => Value.Reference.WriteableData;

		#endregion

		public TypeAttributes Attributes => EEClass.Reference.Attributes;

		/// <summary>
		/// Size of the padding in <see cref="BaseSize"/>
		/// </summary>
		public int BaseSizePadding => EEClass.Reference.BaseSizePadding;

		public Pointer<byte> Chunks             => EEClass.Reference.Chunks;
		public bool          FieldsArePacked    => EEClass.Reference.FieldsArePacked;
		public int           FieldsCount        => EEClass.Reference.FieldListLength;
		public int           FixedEEClassFields => EEClass.Reference.FixedEEClassFields;
		public Pointer<byte> GuidInfo           => EEClass.Reference.GuidInfo;

		public int InstanceFieldsCount => EEClass.Reference.NumInstanceFields;

		/// <summary>
		/// Size of instance fields
		/// </summary>
		public int InstanceFieldsSize => BaseSize - BaseSizePadding;

		public CorInterfaceType InterfaceType        => EEClass.Reference.InterfaceType;
		public int              MethodsCount         => EEClass.Reference.NumMethods;
		public int              NativeSize           => (int) EEClass.Reference.NativeSize;
		public int              NonVirtualSlotsCount => EEClass.Reference.NumNonVirtualSlots;
		public CorElementType   NormType             => EEClass.Reference.NormType;

		public Pointer<byte> OptionalFields => EEClass.Reference.OptionalFields;

		public MetaLayout LayoutInfo
		{
			get
			{
				Guard.Assert(HasLayout);

				return new MetaLayout(EEClass.Reference.LayoutInfo);
			}
		}

		/// <summary>
		/// Number of fields that are not <see cref="FieldInfo.IsLiteral"/> but <see cref="MetaField.IsStatic"/>
		/// </summary>
		public int StaticFieldsCount => EEClass.Reference.NumStaticFields;

		public VMFlags VMFlags => EEClass.Reference.VMFlags;
		//		public MemberInfo[] GetOriginalMember(string name) => RuntimeType.GetAnyMember(name);

		//		public MemberInfo GetFirstOriginalMember(string name) => GetOriginalMember(name)[0];

		public bool ContainsPointers => TypeFlags.HasFlag(TypeFlags.ContainsPointers);

		public bool HasComponentSize
		{
			get { return GetFlag(TypeFlags.HasComponentSize) != 0; }
		}

		/// <summary>
		///     Whether this <see cref="EEClass" /> has a <see cref="EEClassLayoutInfo" />
		/// </summary>
		public bool HasLayout => VMFlags.HasFlag(VMFlags.HasLayout);

		public bool IsBlittable => HasLayout && LayoutInfo.Flags.HasFlag(LayoutFlags.Blittable);

		public bool IsArray
		{
			get { return ((GetFlag(TypeFlags.ArrayMask)) == (TypeFlags.Array)); }
		}

		public bool IsDelegate                      => VMFlags.HasFlag(VMFlags.Delegate);
		public bool IsReferenceOrContainsReferences => !RuntimeType.IsValueType || ContainsPointers;

		public bool IsString
		{
			get { return HasComponentSize && !IsArray && RawGetComponentSize() == sizeof(char); }
		}

		public bool IsStringOrArray => HasComponentSize;

		internal MetaType(Pointer<MethodTable> mt) : base(mt)
		{
			RuntimeType          = RuntimeInfo.ResolveType(mt.Cast());
		}

		internal MetaType(Type t) : this(RuntimeInfo.ResolveHandle(t)) { }

		internal TypeFlags GetFlag(TypeFlags flag)
		{
			return (TypeFlags) (TypeFlags & (TypeFlags) flag);
		}

		internal OptionalSlotsFlags GetFlag(OptionalSlotsFlags flag)
		{

			return (OptionalSlotsFlags) (SlotsFlags & flag);
		}

		internal GenericsFlags GetFlag(GenericsFlags flag)
		{
			return (GenericsFlags) ((IsStringOrArray
				? (GenericsFlags.StringArrayValues & flag)
				: (GenericFlags                    & flag)));
		}


		// returns random combination of flags if this doesn't have a component size
		internal ushort RawGetComponentSize()
		{
			Guard.Assert(BitConverter.IsLittleEndian);
			return (ushort) ComponentSize;
			// fixed (uint* dw = &m_dwFlags) {
			//
			// 	//BE: return *((WORD*)&m_dwFlags + 1);
			// 	//LE: return *(ushort*) &m_dwFlags;
			//
			// 	return *(ushort*) dw;
			// }
		}

		public static implicit operator MetaType(Pointer<MethodTable> ptr) => new MetaType(ptr);

		public static implicit operator MetaType(Type t) => new MetaType(t);

		public static bool operator !=(MetaType left, MetaType right)
		{
			return !Equals(left, right);
		}

		public static bool operator ==(MetaType left, MetaType right)
		{
			return Equals(left, right);
		}

		public bool Equals(MetaType other)
		{
			return base.Equals(other); /*&& RuntimeType == other.RuntimeType*/
		}

		public override MemberInfo Info => RuntimeType;

		public override int Token => Tokens.TokenFromRid(Value.Reference.RawToken, CorTokenType.TypeDef);

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != this.GetType())
				return false;

			return Equals((MetaType) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return (base.GetHashCode() * 397) ^ RuntimeType.GetHashCode();
			}
		}
	}
}