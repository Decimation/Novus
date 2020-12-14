using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Novus.Runtime;
using Novus.Runtime.Meta;
using Novus.Runtime.VM;
using Novus.Runtime.VM.EE;

namespace Novus.Memory
{
	/// <summary>
	/// Specifies how sizes are calculated
	/// </summary>
	public enum SizeOfOptions
	{
		/// <summary>
		///     <para>Returns the native (<see cref="Marshal" />) size of a type.</para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		///     <para> Returned from <see cref="EEClassNativeLayoutInfo.Size" /> </para>
		///     <para> Equals <see cref="Marshal.SizeOf(System.Type)" /></para>
		///     <para> Equals <see cref="StructLayoutAttribute.Size" /> when type isn't zero-sized.</para>
		/// </remarks>
		/// <returns>The native size if the type has a native representation; <see cref="Win32.Native.INVALID" /> otherwise</returns>
		Native,

		/// <summary>
		///     Returns the managed size of an object.
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		///     <para>Returned from <see cref="EEClassLayoutInfo.ManagedSize" /></para>
		/// </remarks>
		/// <returns>
		///     Managed size if the type has an <see cref="EEClassLayoutInfo" />; <see cref="Win32.Native.INVALID" />
		///     otherwise
		/// </returns>
		Managed,


		/// <summary>
		///     <para>Returns the normal size of a type in memory.</para>
		///     <para>Call to <see cref="Mem.SizeOf{T}()" /></para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		/// </remarks>
		/// <returns><see cref="Mem.Size" /> for reference types, size for value types</returns>
		Intrinsic,

		/// <summary>
		///     <para>Returns the base size of the fields (data) in the heap.</para>
		///     <para>This follows the formula of:</para>
		///     <para><see cref="MetaType.BaseSize" /> - <see cref="MetaType.BaseSizePadding" /></para>
		/// <para>
		///         If a value is supplied, this manually reads the <c>MethodTable*</c>, making
		///         this work for boxed values.
		///     </para>
		/// <code>
		/// object o = 123;
		/// Unsafe.BaseFieldsSize(ref o);  // == 4 (boxed int, base fields size of int (sizeof(int)))
		/// Unsafe.BaseFieldsSize&lt;object&gt;; // == 0 (base fields size of object)
		/// </code>
		///     <remarks>
		/// <para>Only a type parameter is needed, or a value can be supplied</para>
		///         <para>Supply a value if the value may be boxed.</para>
		///         <para>Returned from <see cref="MetaType.InstanceFieldsSize" /></para>
		///         <para>This includes field padding.</para>
		///     </remarks>
		/// </summary>
		/// <returns><see cref="RuntimeInfo.MinObjectSize" /> if type is an array, fields size otherwise</returns>
		BaseFields,

		/// <summary>
		///     <para>Returns the base instance size according to the TypeHandle (<c>MethodTable</c>).</para>
		///     <para>This is the minimum heap size of a type.</para>
		///     <para>By default, this equals <see cref="RuntimeInfo.MinObjectSize" /> (<c>24</c> (x64) or <c>12</c> (x84)).</para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed, or a value can be supplied</para>
		///     <para>Returned from <see cref="MetaType.BaseSize" /></para>
		/// </remarks>
		/// <returns>
		///     <see cref="MetaType.BaseSize" />
		/// </returns>
		BaseInstance,

		/// <summary>
		///     <para>Calculates the complete size of a reference type in heap memory.</para>
		///     <para>This is the most accurate size calculation.</para>
		///     <para>
		///         This follows the size formula of: (<see cref="MetaType.BaseSize" />) + (length) *
		///         (<see cref="MetaType.ComponentSize" />)
		///     </para>
		///     <para>where:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 <see cref="MetaType.BaseSize" /> = The base instance size of a type
		///                 (<c>24</c> (x64) or <c>12</c> (x86) by default) (<see cref="RuntimeInfo.MinObjectSize" />)
		///             </description>
		///         </item>
		///         <item>
		///             <description>length	= array or string length; <c>1</c> otherwise</description>
		///         </item>
		///         <item>
		///             <description><see cref="MetaType.ComponentSize" /> = element size, if available; <c>0</c> otherwise</description>
		///         </item>
		///     </list>
		/// </summary>
		/// <remarks>
		/// <para>A value must be supplied.</para>
		///     <para>Source: /src/vm/object.inl: 45</para>
		///     <para>Equals the Son Of Strike "!do" command.</para>
		///     <para>Equals <see cref="BaseInstance" /> for objects that aren't arrays or strings.</para>
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
		/// </remarks>
		/// <returns>The size of the type in heap memory, in bytes</returns>
		Heap,

		

		/// <summary>
		/// Requires a value.
		/// Returns the size of the data in the value. If the type is a reference type,
		/// this returns the size of the value not occupied by the <see cref="MethodTable" /> pointer and the
		/// <see cref="ObjHeader" />.
		/// If the type is a value type, this returns <see cref="Mem.SizeOf{T}()" />.
		/// </summary>
		Data,

		/// <summary>
		/// Does not require a value.
		/// Returns the base size of the data in the type specified by the value. If the value is a
		/// reference type,
		/// this returns the size of data not occupied by the <see cref="MethodTable" /> pointer, <see cref="ObjHeader" />,
		/// padding, and overhead.
		/// If the value is a value type, this returns <see cref="Mem.SizeOf{T}()" />.
		/// 
		/// </summary>
		BaseData,

		/// <summary>
		///     Calculates the complete size of the value's data. If the type parameter is
		///     a value type, this is equal to option <see cref="Intrinsic"/>. If the type parameter is a
		///     reference type, this is equal to <see cref="Heap"/>.
		/// </summary>
		Auto,
	}
}