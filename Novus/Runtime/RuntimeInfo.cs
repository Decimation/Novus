using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Kantan.Diagnostics;

// ReSharper disable UnusedVariable

// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ClassCannotBeInstantiated
// ReSharper disable UnusedMember.Global

#pragma warning disable CS0618, CS1574
namespace Novus.Runtime
{
	/// <summary>
	///     Utilities for runtime info.
	/// </summary>
	/// <seealso cref="Mem" />
	/// <seealso cref="RuntimeHelpers" />
	/// <seealso cref="RuntimeEnvironment" />
	/// <seealso cref="RuntimeInformation" />
	public static unsafe class RuntimeInfo
	{
		// https://github.com/dotnet/runtime/blob/master/src/coreclr/src/vm/object.h


		static RuntimeInfo()
		{
			Global.Clr.LoadImports(typeof(RuntimeInfo));
		}

		/// <summary>
		///     <see cref="ResolveType" />
		/// </summary>
		[field: ImportManaged(typeof(Type), "GetTypeFromHandleUnsafe")]
		private static delegate* managed<IntPtr, Type> Func_GetTypeFromHandle { get; }

		/// <summary>
		///     <see cref="IsPinnable" />
		/// </summary>
		[field: ImportManaged(typeof(Marshal), "IsPinnable")]
		private static delegate* managed<object, bool> Func_IsPinnable { get; }


		private static readonly Action<object, Action<object>> PinImpl = CreatePinImpl();

		private static Action<object, Action<object>> CreatePinImpl()
		{
			var method = new DynamicMethod("InvokeWhilePinnedImpl", typeof(void),
			                               new[] {typeof(object), typeof(Action<object>)}, typeof(RuntimeInfo).Module);

			var il = method.GetILGenerator();

			// create a pinned local variable of type object
			// this wouldn't be valid in C#, but the runtime doesn't complain about the IL
			var local = il.DeclareLocal(typeof(object), pinned: true);

			// store first argument obj in the pinned local variable
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc_0);
			// invoke the delegate
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_0);
			il.EmitCall(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke")!, null);

			il.Emit(OpCodes.Ret);

			return (Action<object, Action<object>>) method.CreateDelegate(typeof(Action<object, Action<object>>));
		}

		// obj will be *temporarily* pinned while action is being invoked  
		public static void InvokeWhilePinned(object obj, Action<object> action)
		{
			PinImpl(obj, action);
		}

		/// <summary>
		///     Used for unsafe pinning of arbitrary objects.
		/// This allows for pinning of unblittable objects, with the <c>fixed</c> statement.
		/// </summary>
		public static PinningHelper GetPinningHelper(object value) => Unsafe.As<PinningHelper>(value);

		private static Dictionary<object, ManualResetEvent> PinResetEvents { get; } = new();


		public static void Pin(object obj)
		{
			var value = new ManualResetEvent(false);


			PinResetEvents.Add(obj, value);

			ThreadPool.QueueUserWorkItem(o =>
			{
				fixed (byte* p = &GetPinningHelper(obj).Data) {
					value.WaitOne();
				}
			});

			Debug.WriteLine($"Pinned obj: {obj.GetHashCode()}");
		}

		public static void Unpin(object obj)
		{
			PinResetEvents[obj].Set();

			Debug.WriteLine($"Unpinned obj: {obj.GetHashCode()}");
		}


		/// <summary>
		///     <para>Helper class to assist with unsafe pinning of arbitrary objects. The typical usage pattern is:</para>
		///     <code>
		///  fixed (byte* pData = &amp;PinHelper.GetPinningHelper(value).Data)
		///  {
		///  }
		///  </code>
		///     <remarks>
		///         <para><c>pData</c> is what <c>Object::GetData()</c> returns in VM.</para>
		///         <para><c>pData</c> is also equal to offsetting the pointer by <see cref="OffsetOptions.Fields" />. </para>
		///         <para>From <see cref="System.Runtime.CompilerServices.JitHelpers" />. </para>
		///     </remarks>
		/// </summary>
		[UsedImplicitly]
		public sealed class PinningHelper
		{
			/// <summary>
			///     Represents the first field in an object.
			/// </summary>
			/// <remarks>Equals <see cref="Mem.AddressOfHeap{T}(T,OffsetOptions)" /> with <see cref="OffsetOptions.Fields" />.</remarks>
			public byte Data;

			private PinningHelper() { }
		}

		#region Constants

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

		#endregion

		#region Metadata

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
		/// <remarks>Inverse of <see cref="ResolveTypeHandle(System.Type)" /></remarks>
		public static Type ResolveType(Pointer<MethodTable> handle)
		{
			return Func_GetTypeFromHandle(handle.Address);
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

		#endregion

		#region Properties

		/// <summary>
		///     Determines whether <paramref name="obj" /> is blittable; that is, whether it has identical data representation in
		///     both
		///     managed and unmanaged memory.
		/// </summary>
		/// <returns><c>true</c> if blittable; <c>false</c> otherwise</returns>
		public static bool IsBlittable<T>(T obj) => obj.GetMetaType().IsBlittable;

		/// <summary>
		///     Determines whether <paramref name="value" /> is pinnable.
		/// </summary>
		/// <returns><c>true</c> if pinnable; <c>false</c> otherwise</returns>
		public static bool IsPinnable([CanBeNull] object value) => Func_IsPinnable(value);

		public static bool IsNullable<T>(T obj)
		{
			//https://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable

			if (obj == null) {
				return true; // obvious
			}

			var type = typeof(T);

			if (!type.IsValueType) {
				return true; // ref-type
			}

			if (Nullable.GetUnderlyingType(type) != null) {
				return true; // Nullable<T>
			}

			return false; // value-type
		}

		/// <summary>
		///     Determines whether the value of <paramref name="value" /> is <c>nil</c>.
		///     <remarks><c>Nil</c> is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNil<T>([CanBeNull] T value) => EqualityComparer<T>.Default.Equals(value, default);

		public static bool IsStruct<T>([NotNull] T value) => value.GetType().IsValueType;

		public static bool IsArray<T>([NotNull] T value) => value is Array;

		public static bool IsString<T>([NotNull] T value) => value is string;

		/// <summary>
		///     Determines whether <paramref name="value" /> is boxed.
		/// </summary>
		/// <returns><c>true</c> if boxed; <c>false</c> otherwise</returns>
		public static bool IsBoxed<T>([CanBeNull] T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
		}

		/// <summary>
		/// Determines whether <paramref name="t"/> is uninitialized; that is,
		/// all of its fields are <c>null</c>.
		/// </summary>
		public static bool IsUninitialized<T>(T t)
		{
			var ptr = Mem.AddressOfData(ref t);
			int s   = Mem.SizeOf(t, SizeOfOptions.BaseFields);

			bool b = ptr.Copy(s).All(x => x == 0);

			return b;
		}

		/// <summary>
		///     Heuristically determines whether <paramref name="value" /> is blank.
		///     This always returns <c>true</c> if <paramref name="value" /> is <c>null</c> or nil.
		/// </summary>
		/// <remarks>
		///     Blank is defined as one of the following: <c>null</c>, nil (<see cref="IsNil{T}" />),
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

			//if (IsBoxed(value)) {
			//	return false;
			//}


			// As for strings, IsNullOrWhiteSpace should always be true when
			// IsNullOrEmpty is true

			bool test = value switch
			{
				IList list => list.Count == 0,
				string str => String.IsNullOrWhiteSpace(str),
				_          => false
			};

			return test;
		}

		#endregion
	}
}