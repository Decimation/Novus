using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Novus.Memory;
using Novus.Runtime.VM;
using SimpleCore.Diagnostics;

// ReSharper disable UnusedMember.Global

#pragma warning disable CS0618
namespace Novus.Runtime
{
	/// <summary>
	///     Utilities for runtime info.
	/// </summary>
	/// <seealso cref="Mem" />
	public static unsafe class RuntimeInfo
	{
		// https://github.com/dotnet/runtime/blob/master/src/coreclr/src/vm/object.h


		/// <summary>
		///     Size of the length field and first character
		///     <list type="bullet">
		///         <item>
		///             <description>+ sizeof <see cref="char" />: First character</description>
		///         </item>
		///         <item>
		///             <description>+ sizeof <see cref="int" />: String length</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int StringOverhead = sizeof(char) + sizeof(int);

		/// <summary>
		///     Size of the length field and padding (x64)
		///     <list type="bullet">
		///         <item>
		///             <description>+ sizeof <see cref="int" />: Length field</description>
		///         </item>
		///         <item>
		///             <description>+ sizeof <see cref="int" />: Padding (x64)</description>
		///         </item>
		///     </list>
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
		///     (<see cref="Mem.Size" /> + <see cref="int" />)
		/// </summary>
		public static readonly int OffsetToStringData = RuntimeHelpers.OffsetToStringData;


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


		internal static Pointer<byte> FieldOffset<TField>(TField* field, int offset) where TField : unmanaged
		{
			// Alias: PTR_HOST_MEMBER_TADDR

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
		///     Reads inherited substructure <typeparamref name="TSub" /> from parent <typeparamref name="TSuper" />.
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
		///     Reads <see cref="TypeHandle" /> as <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from
		///     <paramref name="value" />
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
		///     Resolves the <see cref="Type" /> from a <see cref="Pointer{T}" /> to the internal <see cref="MethodTable" />.
		/// </summary>
		/// <remarks>Inverse of <see cref="ResolveTypeHandle" /></remarks>
		public static Type ResolveType(Pointer<MethodTable> handle)
		{
			return Functions.Func_GetTypeFromHandle(handle.Address);
		}


		/// <summary>
		///     Resolves the <see cref="Pointer{T}" /> to <see cref="MethodTable" /> from <paramref name="t" />.
		/// </summary>
		/// <remarks>Inverse of <see cref="ResolveType" /></remarks>
		public static Pointer<MethodTable> ResolveTypeHandle(Type t)
		{
			var handle          = t.TypeHandle.Value;
			var typeHandleValue = *(TypeHandle*) &handle;
			return typeHandleValue.MethodTable;
		}

		/// <summary>
		///     Determines whether the value of <paramref name="value" /> is <c>nil</c>.
		///     <remarks><c>Nil</c> is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNil<T>([CanBeNull] T value)
		{
			return EqualityComparer<T>.Default.Equals(value, default);
		}

		public static bool IsStruct<T>([NotNull] T value)
		{
			return value.GetType().IsValueType;
		}

		public static bool IsArray<T>([NotNull] T value)
		{
			return value is Array;
		}

		public static bool IsString<T>([NotNull] T value)
		{
			return value is string;
		}

		/// <summary>
		///     Determines whether <paramref name="value" /> is boxed.
		/// </summary>
		public static bool IsBoxed<T>([CanBeNull] T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
		}

		/// <summary>
		///     Determines whether <paramref name="value" /> is pinnable.
		/// </summary>
		public static bool IsPinnable([CanBeNull] object value)
		{
			return Functions.Func_IsPinnable(value);
		}

		/// <summary>
		///     Heuristically determines whether <paramref name="value" /> is blank.
		///     This always returns <c>true</c> if <paramref name="value" /> is <c>null</c> or nil.
		/// </summary>
		/// <remarks>
		///     Blank is defined as one of the following: <c>null</c>, nil (<see cref="IsNilFast{T}" />),
		///     non-unique, or unmodified
		/// </remarks>
		/// <example>
		///     If <paramref name="value" /> is a <see cref="string" />, this function returns <c>true</c> if the
		///     <see cref="string" /> is <c>null</c> or whitespace (<see cref="string.IsNullOrWhiteSpace" />).
		/// </example>
		/// <param name="value">Value to check for</param>
		/// <typeparam name="T">Type of <paramref name="value" /></typeparam>
		/// <returns>
		///     <c>true</c> if <paramref name="value" /> is <c>null</c> or nil; or
		///     if <paramref name="value" /> is heuristically determined to be blank.
		/// </returns>
		public static bool IsBlank<T>([CanBeNull] T value)
		{
			if (IsNil(value)) {
				return true;
			}

			if (IsBoxed(value)) {
				return false;
			}


			// As for strings, IsNullOrWhiteSpace should always be true when
			// IsNullOrEmpty is true, and vise versa

			bool test = value switch
			{
				IList list => list.Count == 0,
				string str => String.IsNullOrWhiteSpace(str),
				_          => false
			};

			return test;
		}
	}
}