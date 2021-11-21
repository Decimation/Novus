#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Utilities;

// ReSharper disable UnusedMember.Global


namespace Novus.Memory;

/// <summary>
///     Represents a native <c>readonly</c> pointer.
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
///             <description>Supports pointer arithmetic, reading/writing different any type, and other pointer operations.</description>
///         </item>
///         <item>
///             <description>No bounds checking</description>
///         </item>
///         <item>
///             <description>Minimum type safety</description>
///         </item>
///     </list>
/// </remarks>
/// <seealso cref="Span{T}" />
/// <seealso cref="Memory{T}" />
/// <seealso cref="IntPtr" />
/// <seealso cref="UIntPtr" />
/// <seealso cref="Unsafe" />
public readonly unsafe struct ReadOnlyPointer<T> : IFormattable
{
	/// <summary>
	///     Internal pointer value.
	/// </summary>
	private readonly void* m_value;

	/// <summary>
	///     Size of element type <typeparamref name="T" />.
	/// </summary>
	public int ElementSize => Mem.SizeOf<T>();

	/// <summary>
	///     Indexes <see cref="Address" /> as a reference.
	/// </summary>
	public ref T this[int index] => ref AsRef(index);

	/// <summary>
	///     Returns the current value as a reference.
	/// </summary>
	public ref T Reference => ref AsRef();


	/// <summary>
	///     Dereferences the pointer as the specified type.
	/// </summary>
	public T Value
	{
		get => Read();
		set => Write(value);
	}

	/// <summary>
	///     Address being pointed to.
	/// </summary>
	public IntPtr Address => (IntPtr) m_value;


	/// <summary>
	///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
	/// </summary>
	public bool IsNull => this == Mem.Nullptr;


	public ReadOnlyPointer(void* value)
	{
		m_value = value;
	}

	public ReadOnlyPointer(IntPtr value) : this(value.ToPointer()) { }

	public ReadOnlyPointer(ref T value) : this(Unsafe.AsPointer(ref value)) { }


	#region Conversion

	public static explicit operator ReadOnlyPointer<T>(ulong ul) => new((void*) ul);

	public static explicit operator Pointer<T>(ReadOnlyPointer<T> ptr) => ptr.Address;

	public static explicit operator IntPtr(ReadOnlyPointer<T> ptr) => ptr.Address;

	public static explicit operator void*(ReadOnlyPointer<T> ptr) => ptr.ToPointer();

	public static explicit operator long(ReadOnlyPointer<T> ptr) => ptr.ToInt64();

	public static explicit operator ulong(ReadOnlyPointer<T> ptr) => ptr.ToUInt64();

	public static implicit operator ReadOnlyPointer<byte>(ReadOnlyPointer<T> ptr) => ptr.ToPointer();

	public static explicit operator ReadOnlyPointer<T>(long value) => new((void*) value);

	public static implicit operator ReadOnlyPointer<T>(void* value) => new(value);

	public static implicit operator ReadOnlyPointer<T>(IntPtr value) => new(value);

	public static implicit operator ReadOnlyPointer<T>(ReadOnlyPointer<byte> ptr) => ptr.Address;

	public static implicit operator ReadOnlyPointer<T>(Span<T> ptr) => new(ref ptr.GetPinnableReference());


	/// <summary>
	///     Creates a new <see cref="ReadOnlyPointer{T}" /> of type <typeparamref name="TNew" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <typeparam name="TNew">Type to point to</typeparam>
	/// <returns>A new <see cref="ReadOnlyPointer{T}" /> of type <typeparamref name="TNew" /></returns>
	public ReadOnlyPointer<TNew> Cast<TNew>() => m_value;

	/// <summary>
	///     Creates a new <see cref="ReadOnlyPointer{T}" /> of type <see cref="Byte" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <returns>A new <see cref="ReadOnlyPointer{T}" /> of type <see cref="Byte" /></returns>
	public ReadOnlyPointer<byte> Cast() => Cast<byte>();

	public Pointer<T> ConstCast()
	{
		return m_value;
	}

	/// <summary>
	///     Creates a native pointer of type <typeparamref name="TUnmanaged" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <returns>A native pointer of type <typeparamref name="TUnmanaged" /></returns>
	[Pure]
	public TUnmanaged* ToPointer<TUnmanaged>() where TUnmanaged : unmanaged => (TUnmanaged*) m_value;

	/// <summary>
	///     Creates a native <c>void*</c> pointer, pointing to <see cref="Address" />
	/// </summary>
	/// <returns>A native <c>void*</c> pointer</returns>
	[Pure]
	public void* ToPointer() => m_value;

	[Pure]
	public nint ToNativeInt() => (nint) m_value;

	[Pure]
	public ulong ToUInt64() => (ulong) m_value;

	[Pure]
	public long ToInt64() => (long) m_value;

	[Pure]
	public int ToInt32() => (int) m_value;

	[Pure]
	public uint ToUInt32() => (uint) m_value;

	[Pure]
	public Span<T> AsSpan(int elemCnt) => new(m_value, elemCnt);

	#endregion

	#region Comparison

	/// <summary>
	///     Checks to see if <paramref name="other" /> is equal to the current instance.
	/// </summary>
	/// <param name="other">Other <see cref="ReadOnlyPointer{T}" />.</param>
	/// <returns></returns>
	public bool Equals(ReadOnlyPointer<T> other)
	{
		return Address == other.Address;
	}

	public override bool Equals(object? obj)
	{
		return obj is ReadOnlyPointer<T> pointer && Equals(pointer);
	}


	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return unchecked((int) (long) m_value);
	}

	public static bool operator ==(ReadOnlyPointer<T> left, ReadOnlyPointer<byte> right) => left.Equals(right);

	public static bool operator !=(ReadOnlyPointer<T> left, ReadOnlyPointer<byte> right) => !left.Equals(right);

	public static bool operator ==(ReadOnlyPointer<T> left, Pointer<T> right) =>
		left.ConstCast().Equals(right);

	public static bool operator !=(ReadOnlyPointer<T> left, Pointer<T> right) =>
		!left.ConstCast().Equals(right);

	public static bool operator ==(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right) => left.Equals(right);

	public static bool operator !=(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right) => !left.Equals(right);

	public static bool operator >(ReadOnlyPointer<T> ptr, ReadOnlyPointer<T> b) => ptr.ToInt64() > b.ToInt64();

	public static bool operator >=(ReadOnlyPointer<T> ptr, ReadOnlyPointer<T> b) => ptr.ToInt64() >= b.ToInt64();

	public static bool operator <(ReadOnlyPointer<T> ptr, ReadOnlyPointer<T> b) => ptr.ToInt64() < b.ToInt64();

	public static bool operator <=(ReadOnlyPointer<T> ptr, ReadOnlyPointer<T> b) => ptr.ToInt64() <= b.ToInt64();

	#endregion

	#region Arithmetic

	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void* Offset(int elemCnt)
	{
		return (void*) ((long) m_value + (long)Mem.GetByteCount(ElementSize, elemCnt));
	}
		

	#endregion

	#region Read/write

	/// <summary>
	///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />.
	/// </summary>
	/// <param name="value">Value to write.</param>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	public void Write(T value, int elemOffset = OFFSET) => Unsafe.Write(Offset(elemOffset), value);


	/// <summary>
	///     Reads a value of type <typeparamref name="T" /> from <see cref="Address" />.
	/// </summary>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	/// <returns>The value read from the offset <see cref="Address" />.</returns>
	[Pure]
	public T Read(int elemOffset = OFFSET) => Unsafe.Read<T>(Offset(elemOffset));


	/// <summary>
	///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />.
	/// </summary>
	/// <param name="elemOffset">Element offset (in terms of type <typeparamref name="T" />).</param>
	/// <returns>A reference to a value of type <typeparamref name="T" />.</returns>
	[Pure]
	public ref T AsRef(int elemOffset = OFFSET) => ref Unsafe.AsRef<T>(Offset(elemOffset));

	/// <summary>
	///     Zeros <paramref name="elemCnt" /> elements.
	/// </summary>
	/// <param name="elemCnt">Number of elements to zero</param>
	public void Clear(int elemCnt = ELEM_CNT)
	{
		for (int i = 0; i < elemCnt; i++)
			this[i] = default!;
	}

	/// <summary>
	///     Writes all elements of <paramref name="rg" /> to the current pointer.
	/// </summary>
	/// <param name="rg">Values to write</param>
	public void WriteAll(T[] rg)
	{
		for (int j = 0; j < rg.Length; j++) {
			this[j] = rg[j];
		}
	}


	[Pure]
	public Pointer<byte> ReadPointer(int elemOffset = OFFSET) => 
		ReadPointer<byte>(elemOffset);

	[Pure]
	public Pointer<TType> ReadPointer<TType>(int elemOffset = OFFSET) => 
		Cast<Pointer<TType>>().Read(elemOffset);

	public void WritePointer<TType>(Pointer<TType> ptr, int elemOffset = OFFSET) =>
		Cast<Pointer<TType>>().Write(ptr, elemOffset);


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
	public T[] Copy(int startIndex, int elemCnt)
	{
		var rg = new T[elemCnt];

		for (int i = startIndex; i < elemCnt + startIndex; i++)
			rg[i - startIndex] = this[i];

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
	public T[] Copy(int elemCnt) => Copy(OFFSET, elemCnt);

	public void CopyTo(T[] rg)
	{
		for (int i = 0; i < rg.Length; i++) {
			rg[i] = this[i];
		}
	}

	#endregion

	#region Format

	public override string ToString() => ToString(Strings.HexFormatter.FMT_P);


	public string ToString(string format) => ToString(format, null);

	public string ToString(string? format, IFormatProvider? provider)
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

		return Strings.ToHexString(Address, format);

	}

	#endregion

	/// <summary>
	///     Default offset for <see cref="ReadOnlyPointer{T}" />
	/// </summary>
	private const int OFFSET = 0;

	/// <summary>
	///     Default increment/decrement/element count for <see cref="ReadOnlyPointer{T}" />
	/// </summary>
	private const int ELEM_CNT = 1;
}