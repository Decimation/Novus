using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Novus.CoreClr;
using Novus.CoreClr.VM;
using Novus.Memory;
using Novus.Utilities;
using SimpleCore.Diagnostics;

#pragma warning disable CS0618
namespace Novus
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Mem"/>
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
		///             <description>+ 4 for length (<see cref="uint" />) </description>
		///         </item>
		///         <item>
		///             <description>+ 4 for padding (<see cref="uint" />) (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToArrayData = OffsetToData + ArrayOverhead;

		/// <summary>
		///     Heap offset to the first string character.
		/// On 64 bit platforms, this should be 12 (8 + 4) and on 32 bit 8 (4 + 4).
		/// (<see cref="Mem.Size"/> + <see cref="int"/>)
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
		///             <description>+ sizeof <see cref="TypeHandle" />: <see cref="TypeHandle"/></description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int ObjectBaseSize = ObjHeaderSize + sizeof(TypeHandle);

		/// <summary>
		///     <para>Minimum GC object heap size</para>
		/// </summary>
		public static readonly int MinObjectSize = (Mem.Size * 2) + ObjHeaderSize;

		/// <summary>
		/// Alias: PTR_HOST_MEMBER_TADDR
		/// </summary>
		internal static Pointer<byte> FieldOffset<TField>(TField* field, int offset) where TField : unmanaged
		{
			// m_methodTable.GetValue(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_methodTable));

			//const int MT_FIELD_OFS = 0;
			//return (MethodTable*) (MT_FIELD_OFS + ((long) MethodTableRaw));

			// // Construct a pointer to a member of the given type.
			// #define PTR_HOST_MEMBER_TADDR(type, host, memb) \
			//     (PTR_HOST_TO_TADDR(host) + (TADDR)offsetof(type, memb))

			return (Pointer<byte>) (offset + ((long) field));
		}

		/// <summary>
		/// Alias: PTR_HOST_MEMBER_TADDR (alt)
		/// </summary>
		internal static Pointer<byte> FieldOffsetAlt<T>(ref T value, long ofs, Pointer<byte> fieldValue)
			where T : unmanaged
		{
			return Mem.AddressOf(ref value).Add((long) fieldValue).Add(ofs).Cast();
		}

		internal static Pointer<TSub> ReadSubStructure<TSuper, TSub>(Pointer<TSuper> super)

		{
			int size = Mem.SizeOf<TSuper>();
			return super.Add(size).Cast<TSub>();
		}

		internal static Pointer<MethodTable> ReadTypeHandle<T>(T value)
		{
			var type = value.GetType();


			return ReadTypeHandle(type);
		}

		/// <summary>
		/// Returns a pointer to the internal CLR metadata structure of <paramref name="member"/>
		/// </summary>
		/// <param name="member">Reflection type</param>
		/// <returns>A pointer to the corresponding structure</returns>
		/// <exception cref="InvalidOperationException">The type of <see cref="MemberInfo"/> doesn't have a handle</exception>
		internal static Pointer<byte> ResolveHandle(MemberInfo member)
		{
			Guard.AssertNotNull(member, nameof(member));

			return member switch
			{
				Type t            => ReadTypeHandle(t).Cast(),
				FieldInfo field   => field.FieldHandle.Value,
				MethodInfo method => method.MethodHandle.Value,
				_                 => throw new InvalidOperationException()
			};
		}

		internal static Type ResolveType(Pointer<byte> handle)
		{
			//return GetTypeFromHandle(handle.Address);
			//todo
			var t = typeof(Type).GetAnyMethod("GetTypeFromHandleUnsafe");

			var mb = (MethodBase) t;

			var o = mb.Invoke(null, new object[] {handle.Address});

			var type = (Type) o;

			return type;
		}

		public static Assembly GetAssemblyByName(string name)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SingleOrDefault(assembly => assembly.GetName().FullName.Contains(name));
		}

		public static IEnumerable<AssemblyName> GetUserDependencies(Assembly asm)
		{
			const string SYSTEM = "System";

			return asm.GetReferencedAssemblies().Where(a => a.Name != null && !a.Name.Contains(SYSTEM));
		}

		internal static Pointer<MethodTable> ReadTypeHandle(Type t)
		{
			var handle          = t.TypeHandle.Value;
			var typeHandleValue = *(TypeHandle*) &handle;
			return (typeHandleValue.MethodTable);
		}
	}
}