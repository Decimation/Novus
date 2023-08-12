using System.Reflection;
using System.Runtime.CompilerServices;
using Novus.Memory;
using Novus.Numerics;
using Novus.Runtime.Meta.Base;
using Novus.Runtime.VM;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0051
namespace Novus.Runtime.Meta;

/// <summary>
///     <list type="bullet">
///         <item><description>CLR structure: <see cref="FieldDesc"/></description></item>
///         <item><description>Reflection structure: <see cref="FieldInfo"/></description></item>
///     </list>
/// </summary>
public unsafe class MetaField : EmbeddedClrStructure<FieldDesc>
{
	private const int FIELD_OFFSET_MAX = (1 << 27) - 1;

	private const int FIELD_OFFSET_NEW_ENC = FIELD_OFFSET_MAX - 4;

	public MetaField(Pointer<FieldDesc> ptr) : base(ptr) { }

	public MetaField(FieldInfo ptr) : base(ptr) { }

	public CorElementType Element => Value.Reference.Element;

	public AccessModifiers Access => Value.Reference.Access;

	public int Offset => Value.Reference.Offset;

	public override FieldInfo Info => EnclosingType.RuntimeType.Module.ResolveField(Token);

	public MetaType FieldType => Info.FieldType;

	public override MetaType EnclosingType => Value.Reference.ApproxEnclosingMethodTable;

	/// <summary>
	/// <remarks>Equals <see cref="MemberInfo.MetadataToken"/></remarks>
	/// </summary>
	public override int Token => Value.Reference.Token;

	public FieldAttributes Attributes => Info.Attributes;

	public bool IsPointer => Value.Reference.IsPointer;

	public FieldBitFlags BitFlags => Value.Reference.BitFlags;

	public bool IsStatic => BitFlags.HasFlag(FieldBitFlags.Static);

	public int Size => Value.Reference.Size;

	public Pointer<byte> StaticAddress
	{
		get
		{
			// NOTE: important
			RuntimeHelpers.RunClassConstructor(EnclosingType.RuntimeType.TypeHandle);

			return Value.Reference.StaticAddress;
		}
	}

	public static implicit operator MetaField(Pointer<FieldDesc> ptr) => new(ptr);

	public static implicit operator MetaField(FieldInfo t) => new(t);
}