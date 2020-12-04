using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Novus.CoreClr;
using Novus.CoreClr.Meta;
using Novus.CoreClr.VM;
using Novus.Memory;
using SimpleCore.Diagnostics;

// ReSharper disable UnusedMember.Global

#pragma warning disable CS0618
namespace Novus
{
	/// <summary>
	/// Utilities for runtime info.
	/// </summary>
	/// <seealso cref="Mem" />
	public static unsafe class RuntimeInfo
	{
		// https://github.com/dotnet/coreclr/blob/master/src/vm/object.h

		/// <summary>
		///     Size of the length field and first character
		///     <list type="bullet">
		///         <item>
		///             <description>+ 2: First character</description>
		///         </item>
		///         <item>
		///             <description>+ 4: String length</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int StringOverhead = sizeof(char) + sizeof(int);

		/// <summary>
		///     Size of the length field and padding (x64)
		/// </summary>
		public static readonly int ArrayOverhead = Mem.Size;

		/// <summary>
		///     Heap offset to the first field.
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="Mem.Size" /> for <see cref="TypeHandle" /></description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToData = Mem.Size;

		/// <summary>
		///     Heap offset to the first array element.
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="Mem.Size" /> for <see cref="TypeHandle" /></description>
		///         </item>
		///         <item>
		///             <description>+ sizeof (<see cref="uint" />) for length </description>
		///         </item>
		///         <item>
		///             <description>+ sizeof (<see cref="uint" />) for padding (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToArrayData = OffsetToData + ArrayOverhead;

		/// <summary>
		///     Heap offset to the first string character.
		///     On 64 bit platforms, this should be 12 and on 32 bit 8.
		///     (<see cref="Mem.Size"/> + <see cref="int"/>)
		/// </summary>
		public static readonly int OffsetToStringData = RuntimeHelpers.OffsetToStringData;

		// https://github.com/dotnet/coreclr/blob/master/src/vm/object.h


		public static readonly int ObjHeaderSize = sizeof(ObjHeader);

		/// <summary>
		///     Size of <see cref="TypeHandle" /> and <see cref="ObjHeader" />
		///     <list type="bullet">
		///         <item>
		///             <description>+ <see cref="ObjHeaderSize" />: <see cref="ObjHeader" /></description>
		///         </item>
		///         <item>
		///             <description>+ sizeof <see cref="TypeHandle" />: <see cref="TypeHandle" /></description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int ObjectBaseSize = ObjHeaderSize + sizeof(TypeHandle);

		/// <summary>
		///     <para>Minimum GC object heap size</para>
		/// </summary>
		public static readonly int MinObjectSize = Mem.Size * 2 + ObjHeaderSize;

		/// <summary>
		///     Alias: PTR_HOST_MEMBER_TADDR
		/// </summary>
		internal static Pointer<byte> FieldOffset<TField>(TField* field, int offset) where TField : unmanaged
		{
			// m_methodTable.GetValue(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_methodTable));

			//const int MT_FIELD_OFS = 0;
			//return (MethodTable*) (MT_FIELD_OFS + ((long) MethodTableRaw));

			// // Construct a pointer to a member of the given type.
			// #define PTR_HOST_MEMBER_TADDR(type, host, memb) \
			//     (PTR_HOST_TO_TADDR(host) + (TADDR)offsetof(type, memb))

			return (Pointer<byte>) (offset + (long) field);
		}

		
		internal static Pointer<byte> FieldOffset<T>(ref T value, long ofs, Pointer<byte> fieldValue)
			where T : unmanaged
		{
			// Alias: PTR_HOST_MEMBER_TADDR (alt)
			return Mem.AddressOf(ref value).Add((long) fieldValue).Add(ofs).Cast();
		}

		/// <summary>
		/// Reads inherited substructure <typeparamref name="TSub"/> from parent <typeparamref name="TSuper"/>.
		/// </summary>
		/// <typeparam name="TSuper">Superstructure (parent) type</typeparam>
		/// <typeparam name="TSub">Substructure (child) type</typeparam>
		/// <param name="super">Superstructure pointer</param>
		/// <returns>Substructure pointer</returns>
		public static Pointer<TSub> ReadSubStructure<TSuper, TSub>(Pointer<TSuper> super)
		{
			int size = Mem.SizeOf<TSuper>();
			return super.Add(size).Cast<TSub>();
		}

		/// <summary>
		/// Reads <see cref="TypeHandle"/> as <see cref="Pointer{T}"/> to <see cref="MethodTable"/> from <paramref name="value"/> 
		/// </summary>
		public static Pointer<MethodTable> ReadTypeHandle<T>(T value)
		{
			var type = value.GetType();
			return ResolveTypeHandle(type);
		}

		/// <summary>
		///     Returns a handle to the internal CLR metadata structure of <paramref name="member" />
		/// </summary>
		/// <param name="member">Reflection type</param>
		/// <returns>A pointer to the corresponding structure</returns>
		/// <exception cref="InvalidOperationException">The type of <see cref="MemberInfo" /> doesn't have a handle</exception>
		public static Pointer<byte> ResolveHandle(MemberInfo member)
		{
			Guard.AssertArgumentNotNull(member, nameof(member));

			return member switch
			{
				Type t            => ResolveTypeHandle(t).Cast(),
				FieldInfo field   => field.FieldHandle.Value,
				MethodInfo method => method.MethodHandle.Value,
				_                 => throw new InvalidOperationException()
			};
		}

		/// <summary>
		/// Resolves the <see cref="Type"/> from a <see cref="Pointer{T}"/> to the internal <see cref="MethodTable"/>.
		/// </summary>
		/// <remarks>Inverse of <see cref="ResolveTypeHandle"/></remarks>
		public static Type ResolveType(Pointer<MethodTable> handle)
		{
			//return GetTypeFromHandle(handle.Address);
			//todo
			// var t = typeof(Type).GetAnyMethod("GetTypeFromHandleUnsafe");
			//
			// var mb = (MethodBase) t;
			//
			// var o = mb.Invoke(null, new object[] {handle.Address});
			//
			// var type = (Type) o;
			//
			// return type;

			return Functions.Func_GetTypeFromHandle(handle.Address);
		}

		// public static Type Type_Of<T>(T t = default)
		// {
		// 	//todo
		//
		// 	if (Inspector.IsNil(t)) {
		// 		return typeof(T);
		// 	}
		// 	else {
		// 		return t.GetType();
		// 	}
		// }

		/// <summary>
		/// Resolves the <see cref="Pointer{T}"/> to <see cref="MethodTable"/> from <paramref name="t"/>.
		/// </summary>
		/// <remarks>Inverse of <see cref="ResolveType"/></remarks>
		public static Pointer<MethodTable> ResolveTypeHandle(Type t)
		{
			var handle          = t.TypeHandle.Value;
			var typeHandleValue = *(TypeHandle*) &handle;
			return typeHandleValue.MethodTable;
		}
	}
}