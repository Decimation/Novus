using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Novus.CoreClr.Meta.Base;
using Novus.CoreClr.VM;
using Novus.Memory;
using SimpleCore.Diagnostics;
// ReSharper disable UnusedMember.Global

namespace Novus.CoreClr.Meta
{
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


		//public object GetValue(object value) => FieldInfo.GetValue(value);


		internal MetaField(Pointer<FieldDesc> ptr) : base(ptr) { }

		internal MetaField(FieldInfo ptr) : this(RuntimeInfo.ResolveHandle(ptr)) { }


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

		

		public static implicit operator MetaField(Pointer<FieldDesc> ptr) => new(ptr);

		public static implicit operator MetaField(FieldInfo t) => new(t);

		//		public static bool operator ==(MetaField left, MetaField right) => Equals(left, right);

		//		public static bool operator !=(MetaField left, MetaField right) => !Equals(left, right);
	}
}