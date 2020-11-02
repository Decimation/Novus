using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Novus.CoreClr.VM;
using Novus.Memory;
using SimpleCore.Diagnostics;

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



		#region Constructors

		internal MetaField(Pointer<FieldDesc> ptr) : base(ptr) { }
		internal MetaField(FieldInfo ptr) : this(RuntimeInfo.ResolveHandle(ptr)) { }

		#endregion

		#region Accessors

		public FieldInfo FieldInfo => (FieldInfo)Info;

		public CorElementType Element => Value.Reference.Element;

		public AccessModifiers Access => Value.Reference.Access;

		public int Offset => Value.Reference.Offset;

		public override MemberInfo Info => EnclosingType.RuntimeType.Module.ResolveField(Token);

		public MetaType FieldType => FieldInfo.FieldType;

		public override MetaType EnclosingType => Value.Reference.ApproxEnclosingMethodTable;

		/// <summary>
		/// <remarks>Equals <see cref="System.Reflection.FieldInfo.MetadataToken"/></remarks>
		/// </summary>
		public override int Token => Value.Reference.Token;

		public FieldAttributes Attributes => FieldInfo.Attributes;

		#region bool

		public bool IsPointer => Value.Reference.IsPointer;

		public FieldBitFlags BitFlags => Value.Reference.BitFlags;

		public bool IsStatic => BitFlags.HasFlag(FieldBitFlags.Static);

		#endregion

		#region Delegates

		public int Size => Value.Reference.Size;

		/// <summary>
		/// <remarks>Ensure the enclosing type is loaded!</remarks>
		/// </summary>
		public Pointer<byte> GetStaticAddress()
		{
			throw new NotImplementedException();
			//return Value.Reference.GetCurrentStaticAddress();
		}

		#endregion

		#endregion

		#region Operators

		public static implicit operator MetaField(Pointer<FieldDesc> ptr) => new MetaField(ptr);

		public static implicit operator MetaField(FieldInfo t) => new MetaField(t);

		#region Equality

		//		public static bool operator ==(MetaField left, MetaField right) => Equals(left, right);

		//		public static bool operator !=(MetaField left, MetaField right) => !Equals(left, right);

		#endregion

		#endregion
	}
}
