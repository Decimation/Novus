global using MImpl = System.Runtime.CompilerServices.MethodImplAttribute;
global using Pointer = Novus.Memory.Pointer<byte>;
using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using Novus.Runtime.Meta;
using Novus.Win32;
using Novus.Win32.Structures;
using Novus.Win32.Structures.Kernel32;
#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Utilities;
using Novus.Streams;

// ReSharper disable UseSymbolAlias

// ReSharper disable UnusedMember.Global
// ReSharper disable StaticMemberInGenericType

namespace Novus.Memory;

/// <summary>
///     Represents a native pointer.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>
///             <description>Can be represented as a native pointer in memory. </description>
///         </item>
///         <item>
///             <description>Equals the size of <see cref="P:System.IntPtr.Size" /></description>
///         </item>
///         <item>
///             <description>
/// Supports pointer arithmetic, reading/writing any type,
/// and other pointer operations.
/// </description>
///         </item>
///         <item>
///             <description>No bounds checking</description>
///         </item>
///         <item>
///             <description>Minimum type safety</description>
///         </item>
/// <item>
///             <description>Pointer semantics</description>
///         </item>
///     </list>
/// </remarks>
/// <seealso cref="Span{T}" />
/// <seealso cref="Memory{T}" />
/// <seealso cref="IntPtr" />
/// <seealso cref="UIntPtr" />
/// <seealso cref="Unsafe" />
public unsafe struct Pointer<T> : IFormattable, IPinnable
{

	private static readonly nuint s_ElementSize;

	static Pointer()
		=> s_ElementSize = (nuint) Mem.SizeOf<T>();

	/// <summary>
	///     Internal pointer value.
	/// </summary>
	private void* m_value;

	/// <summary>
	///     Size of element type <typeparamref name="T" />.
	/// </summary>
	public readonly nuint ElementSize => s_ElementSize;

	/// <summary>
	///     Indexes <see cref="Address" /> as a reference.
	/// </summary>
	public ref T this[nint index]
	{
		[method: MImpl(Global.IMPL_OPTIONS)]
		get => ref AsRef(index);
	}

	/// <summary>
	///     Returns the current value as a reference.
	/// </summary>
	public ref T Reference
	{
		[method: MImpl(Global.IMPL_OPTIONS)]
		get => ref AsRef();
	}

	/// <summary>
	///     Dereferences the pointer as the specified type.
	/// </summary>
	public T Value
	{
		readonly get => Read();
		set => Write(value);
	}

	/// <summary>
	///     Address being pointed to.
	/// </summary>
	public nint Address
	{
		readonly get => (nint) m_value;
		set => m_value = (void*) value;
	}

	/// <summary>
	///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
	/// </summary>
	public readonly bool IsNull => this == Mem.Nullptr;

	public Pointer() : this(value: null) { }

	public Pointer(void* value)
	{
		m_value = value;
	}

	public Pointer(nint value) : this(value.ToPointer()) { }

	public Pointer(ref T value) : this(Unsafe.AsPointer(ref value)) { }

	#region Conversion

	public static explicit operator Pointer<T>(ulong ul)
		=> new((void*) ul);

	public static explicit operator nint(Pointer<T> ptr)
		=> ptr.Address;

	public static explicit operator void*(Pointer<T> ptr)
		=> ptr.ToPointer();

	public static explicit operator long(Pointer<T> ptr)
		=> ptr.ToInt64();

	public static explicit operator ulong(Pointer<T> ptr)
		=> ptr.ToUInt64();

	public static explicit operator Pointer<T>(long value)
		=> new((void*) value);

	public static implicit operator Pointer<byte>(Pointer<T> ptr)
		=> ptr.ToPointer();

	public static implicit operator Pointer<T>(void* value)
		=> new(value);

	public static implicit operator Pointer<T>(nint value)
		=> new(value);

	public static /*implicit*/ explicit operator Pointer<T>(Pointer<byte> ptr)
		=> ptr.Address;

	/*public static explicit operator Pointer<T>(Span<T> s)
		=> s.ToPointer();*/

	/// <summary>
	///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <typeparam name="TNew">Type to point to</typeparam>
	/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
	public readonly Pointer<TNew> Cast<TNew>()
		=> m_value;

	/// <summary>
	///     Creates a new <see cref="Pointer{T}" /> of type <see cref="Byte" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <returns>A new <see cref="Pointer{T}" /> of type <see cref="Byte" /></returns>
	public readonly Pointer<byte> Cast()
		=> Cast<byte>();

	public readonly Span<T> ToSpan(int n)
		=> new(m_value, n);

	/// <summary>
	///     Creates a native pointer of type <typeparamref name="TUnmanaged" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <returns>A native pointer of type <typeparamref name="TUnmanaged" /></returns>
	[Pure]
	public readonly TUnmanaged* ToPointer<TUnmanaged>() where TUnmanaged : unmanaged
		=> (TUnmanaged*) m_value;

	/// <summary>
	///     Creates a native <c>void*</c> pointer, pointing to <see cref="Address" />
	/// </summary>
	/// <returns>A native <c>void*</c> pointer</returns>
	[Pure]
	public readonly void* ToPointer()
		=> m_value;

	[Pure]
	public readonly ulong ToUInt64()
		=> (ulong) m_value;

	[Pure]
	public readonly long ToInt64()
		=> Address.ToInt64();

	[Pure]
	public readonly int ToInt32()
		=> Address.ToInt32();

	[Pure]
	public readonly uint ToUInt32()
		=> (uint) m_value;

	#endregion

	#region Comparison

	/// <summary>
	///     Checks to see if <paramref name="other" /> is equal to the current instance.
	/// </summary>
	/// <param name="other">Other <see cref="Pointer{T}" />.</param>
	/// <returns></returns>
	public readonly bool Equals(Pointer<T> other)
	{
		return Address == other.Address;
	}

	public readonly override bool Equals(object? obj)
	{
		return obj is Pointer<T> pointer && Equals(pointer);

		/*if (obj is Pointer p && Equals(p)) {
			return true;
		}
		else if (obj == null) {
			return IsNull;
		}
		else {
			return object.Equals(this, obj);
		}*/
	}

	public readonly override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return unchecked((int) (long) m_value);
	}

	public static bool operator ==(Pointer<T> left, Pointer<byte> right)
		=> left.Equals(right);

	public static bool operator !=(Pointer<T> left, Pointer<byte> right)
		=> !left.Equals(right);

	public static bool operator ==(Pointer<T> left, Pointer<T> right)
		=> left.Equals(right);

	public static bool operator !=(Pointer<T> left, Pointer<T> right)
		=> !left.Equals(right);

	public static bool operator >(Pointer<T> ptr, Pointer<T> b)
		=> ptr.ToInt64() > b.ToInt64();

	public static bool operator >=(Pointer<T> ptr, Pointer<T> b)
		=> ptr.ToInt64() >= b.ToInt64();

	public static bool operator <(Pointer<T> ptr, Pointer<T> b)
		=> ptr.ToInt64() < b.ToInt64();

	public static bool operator <=(Pointer<T> ptr, Pointer<T> b)
		=> ptr.ToInt64() <= b.ToInt64();

	#endregion

	#region Arithmetic

	/// <summary>
	///     Increment <see cref="Address" /> by the specified number of bytes
	/// </summary>
	/// <param name="byteCnt">Number of bytes to add</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="byteCnt" /> bytes added
	/// </returns>
	[Pure]
	public readonly Pointer<T> AddBytes(nint byteCnt = ELEM_CNT)
	{
		nint val = Address + byteCnt;
		return (void*) val;
	}

	/// <summary>
	///     Decrement <see cref="Address" /> by the specified number of bytes
	/// </summary>
	/// <param name="byteCnt">Number of bytes to subtract</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="byteCnt" /> bytes subtracted
	/// </returns>
	[Pure]
	public readonly Pointer<T> SubtractBytes(nint byteCnt = ELEM_CNT)
		=> AddBytes(-byteCnt);

	public static Pointer<T> operator +(Pointer<T> left, nint right)
		=> left.Add(right);

	public static Pointer<T> operator -(Pointer<T> left, nint right)
		=> left.Subtract(right);

	public static Pointer<T> operator +(Pointer<T> left, Pointer<T> right)
		=> (void*) (left.ToInt64() + right.ToInt64());

	public static Pointer<T> operator -(Pointer<T> left, Pointer<T> right)
		=> (void*) (left.ToInt64() - right.ToInt64());

	/// <summary>
	///     Increments the <see cref="Address" /> by the specified number of elements.
	///     <remarks>
	///         Equal to <see cref="Add" />
	///     </remarks>
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
	public static Pointer<T> operator +(Pointer<T> ptr, int i)
		=> ptr.Add(i);

	/// <summary>
	///     Decrements the <see cref="Address" /> by the specified number of elements.
	///     <remarks>
	///         Equal to <see cref="Subtract" />
	///     </remarks>
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
	public static Pointer<T> operator -(Pointer<T> ptr, int i)
		=> ptr.Subtract(i);

	/// <summary>
	///     Increments the <see cref="Pointer{T}" /> by one element.
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <returns>The offset <see cref="Address" /></returns>
	public static Pointer<T> operator ++(Pointer<T> ptr)
		=> ptr.Add();

	/// <summary>
	///     Decrements the <see cref="Pointer{T}" /> by one element.
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <returns>The offset <see cref="Address" /></returns>
	public static Pointer<T> operator --(Pointer<T> ptr)
		=> ptr.Subtract();

	/// <summary>
	///     Increment <see cref="Address" /> by the specified number of elements
	/// </summary>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="elemCnt" /> elements incremented
	/// </returns>
	[Pure]
	public readonly Pointer<T> Add(nint elemCnt = ELEM_CNT)
		=> Offset(elemCnt);

	/// <summary>
	///     Decrement <see cref="Address" /> by the specified number of elements
	/// </summary>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="elemCnt" /> elements decremented
	/// </returns>
	[Pure]
	public readonly Pointer<T> Subtract(nint elemCnt = ELEM_CNT)
		=> Add(-elemCnt);

	[Pure]
	[MImpl(MImplO.AggressiveInlining)]
	private readonly void* Offset(nint elemCnt)
	{
		// return (void*) ((long) m_value + (long) Mem.GetByteCount(ElementSize, elemCnt));
		return Unsafe.Add<T>(m_value, (int) elemCnt);
	}

	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly Pointer<T> AddressOfIndex(nint index)
		=> Offset(index);

	#endregion

	#region Read/write

	/// <summary>
	///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	[method: MImpl(Global.IMPL_OPTIONS)]
	public readonly void Write(T value, nint elemOffset = OFFSET)
		=> Unsafe.Write(Offset(elemOffset), value);

	/// <summary>
	///     Reads a value of type <typeparamref name="T" /> from <see cref="Address" />.
	/// </summary>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	/// <returns>The value read from the offset <see cref="Address" />.</returns>
	[Pure]
	[method: MImpl(Global.IMPL_OPTIONS)]
	public readonly T Read(nint elemOffset = OFFSET)
		=> Unsafe.Read<T>(Offset(elemOffset));

	/// <summary>
	///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />.
	/// </summary>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	/// <returns>A reference to a value of type <typeparamref name="T" />.</returns>
	[Pure]
	[method: MImpl(Global.IMPL_OPTIONS)]
	public ref T AsRef(nint elemOffset = OFFSET)
		=> ref Unsafe.AsRef<T>(Offset(elemOffset));

	/// <summary>
	///     Zeros <paramref name="elemCnt" /> elements.
	/// </summary>
	/// <param name="elemCnt">Number of elements to zero</param>
	public void Clear(nint elemCnt = ELEM_CNT)
	{
		for (int i = 0; i < elemCnt; i++) {
			this[i] = default!;
		}
	}

	/// <summary>
	///     Writes all elements of <paramref name="rg" /> to the current pointer.
	/// </summary>
	/// <param name="rg">Values to write</param>
	public void WriteAll(params T[] rg)
	{
		for (int j = 0; j < rg.Length; j++) {
			this[j] = rg[j];
		}
	}

	[Pure]
	public readonly Pointer<byte> ReadPointer(nint elemOffset = OFFSET)
		=> ReadPointer<byte>(elemOffset);

	[Pure]
	public readonly Pointer<TType> ReadPointer<TType>(nint elemOffset = OFFSET)
		=> Cast<Pointer<TType>>().Read(elemOffset);

	public readonly void WritePointer<TType>(Pointer<TType> ptr, nint elemOffset = OFFSET)
		=> Cast<Pointer<TType>>().Write(ptr, elemOffset);

	public readonly void CopyTo(Pointer<T> dest, nint startIndex, nint elemCnt)
	{
		//|  Copy3 | 7.428 ns | 0.0473 ns | 0.0395 ns |

		var count = (long) Mem.GetByteCount((nuint) elemCnt, ElementSize);

		Buffer.MemoryCopy((void*) (this + startIndex), (void*) dest,
		                  count, count);

		/*for (int i = startIndex; i < elemCnt + startIndex; i++) {
			dest[i - startIndex] = this[i];
		}*/
	}

	public readonly void CopyTo(Pointer<T> dest, nint elemCnt)
		=> CopyTo(dest, OFFSET, elemCnt);

	public void CopyTo(T[] rg, nint startIndex, nint elemCnt)
	{
		/*var       s   = rg.AsMemory();
		using var pin = s.Pin();
		Copy(pin.Pointer, elemCnt, startIndex);*/

		// |  Copy3 | 3.690 ns | 0.0655 ns | 0.0580 ns |

		for (nint i = startIndex; i < elemCnt + startIndex; i++) {
			rg[i - startIndex] = this[(int) i];
		}
	}

	public void CopyTo(T[] rg)
		=> CopyTo(rg, OFFSET, rg.Length);

	/// <summary>
	///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="T" />,
	///     starting from index <paramref name="startIndex" />
	/// </summary>
	/// <param name="startIndex">Index to begin copying from</param>
	/// <param name="elemCnt">Number of elements to copy</param>
	/// <returns>
	///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="T" /> copied from
	///     the current pointer
	/// </returns>
	[Pure]
	public T[] ToArray(nint startIndex, nint elemCnt)
	{
		var rg = new T[elemCnt];

		CopyTo(rg, startIndex, elemCnt);

		return rg;
	}

	/// <summary>
	///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="T" />,
	///     starting from index 0.
	/// </summary>
	/// <param name="elemCnt">Number of elements to copy</param>
	/// <returns>
	///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="T" /> copied from
	///     the current pointer
	/// </returns>
	[Pure]
	public T[] ToArray(nint elemCnt)
		=> ToArray(OFFSET, elemCnt);

	#endregion

	#region Format

	public readonly override string ToString()
		=> ToString(FormatHelper.HexFormatter.FMT_P);

	public readonly string ToString(string format)
		=> ToString(format, null);

	public readonly string ToString(string? format, IFormatProvider? provider)
	{
		//if (String.IsNullOrEmpty(format))
		//	format = FMT_HEX;

		//provider ??= CultureInfo.CurrentCulture;

		//return format.ToUpperInvariant() switch
		//{
		//	FMT_HEX => Address.ToInt64().ToString(FMT_HEX, provider),
		//	FMT_PTR => Strings.HexFormatter.HEX_PREFIX + ToString(FMT_HEX),
		//	_       => throw new FormatException()
		//};

		return FormatHelper.ToHexString(Address, format);

	}

	#endregion

	[MURV]
	public readonly MemoryHandle Pin(int elementIndex = OFFSET_I)
	{
		var handle = new MemoryHandle(Offset(elementIndex));

		return handle;
	}

	public readonly void Unpin()
	{
		// ...
	}

	/*public readonly MemoryBasicInformation Query()
		=> Native.QueryMemoryPage(this);*/

	/// <summary>
	///     Default offset for <see cref="Pointer{T}" />
	/// </summary>
	private const nint OFFSET = OFFSET_I;

	private const int OFFSET_I = 0;

	/// <summary>
	///     Default increment/decrement/element count for <see cref="Pointer{T}" />
	/// </summary>
	private const nint ELEM_CNT = 1;

}