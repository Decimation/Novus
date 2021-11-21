global using Pointer = Novus.Memory.Pointer<byte>;

#nullable enable
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Utilities;
using Novus.Win32.Structures;

// ReSharper disable UnusedMember.Global


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
public unsafe struct Pointer<T> : IFormattable
{
	/// <summary>
	///     Internal pointer value.
	/// </summary>
	private void* m_value;

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
	public IntPtr Address
	{
		get => (IntPtr) m_value;
		set => m_value = (void*) value;
	}


	/// <summary>
	///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
	/// </summary>
	public bool IsNull => this == Mem.Nullptr;

	public Pointer(void* value)
	{
		m_value = value;
	}

	public Pointer(IntPtr value) : this(value.ToPointer()) { }

	public Pointer(ref T value) : this(Unsafe.AsPointer(ref value)) { }


	#region Conversion

	public static explicit operator Pointer<T>(ulong ul) => new((void*) ul);

	public static explicit operator IntPtr(Pointer<T> ptr) => ptr.Address;

	public static explicit operator void*(Pointer<T> ptr) => ptr.ToPointer();

	public static explicit operator long(Pointer<T> ptr) => ptr.ToInt64();

	public static explicit operator ulong(Pointer<T> ptr) => ptr.ToUInt64();

	public static implicit operator Pointer<byte>(Pointer<T> ptr) => ptr.ToPointer();

	public static explicit operator Pointer<T>(long value) => new((void*) value);

	public static implicit operator Pointer<T>(void* value) => new(value);

	public static implicit operator Pointer<T>(IntPtr value) => new(value);

	public static implicit operator Pointer<T>(Pointer<byte> ptr) => ptr.Address;

	public static implicit operator Pointer<T>(Span<T> ptr) => new(ref ptr.GetPinnableReference());
	

	/// <summary>
	///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <typeparam name="TNew">Type to point to</typeparam>
	/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
	public Pointer<TNew> Cast<TNew>() => m_value;

	/// <summary>
	///     Creates a new <see cref="Pointer{T}" /> of type <see cref="Byte" />, pointing to
	///     <see cref="Address" />
	/// </summary>
	/// <returns>A new <see cref="Pointer{T}" /> of type <see cref="Byte" /></returns>
	public Pointer<byte> Cast() => Cast<byte>();

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
	/// <param name="other">Other <see cref="Pointer{T}" />.</param>
	/// <returns></returns>
	public bool Equals(Pointer<T> other)
	{
		return Address == other.Address;
	}

	public override bool Equals(object? obj)
	{
		return obj is Pointer<T> pointer && Equals(pointer);
	}


	public override int GetHashCode()
	{
		// ReSharper disable once NonReadonlyMemberInGetHashCode
		return unchecked((int) (long) m_value);
	}

	public static bool operator ==(Pointer<T> left, Pointer<byte> right) => left.Equals(right);

	public static bool operator !=(Pointer<T> left, Pointer<byte> right) => !left.Equals(right);

	public static bool operator ==(Pointer<T> left, Pointer<T> right) => left.Equals(right);

	public static bool operator !=(Pointer<T> left, Pointer<T> right) => !left.Equals(right);

	public static bool operator >(Pointer<T> ptr, Pointer<T> b) => ptr.ToInt64() > b.ToInt64();

	public static bool operator >=(Pointer<T> ptr, Pointer<T> b) => ptr.ToInt64() >= b.ToInt64();

	public static bool operator <(Pointer<T> ptr, Pointer<T> b) => ptr.ToInt64() < b.ToInt64();

	public static bool operator <=(Pointer<T> ptr, Pointer<T> b) => ptr.ToInt64() <= b.ToInt64();

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
	public Pointer<T> Add(long byteCnt = ELEM_CNT)
	{
		long val = ToInt64() + byteCnt;
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
	public Pointer<T> Subtract(long byteCnt = ELEM_CNT) => Add(-byteCnt);

	public static Pointer<T> operator +(Pointer<T> left, long right) => (void*) (left.ToInt64() + right);

	public static Pointer<T> operator -(Pointer<T> left, long right) => (void*) (left.ToInt64() - right);

	public static Pointer<T> operator +(Pointer<T> left, Pointer<T> right)
		=> (void*) (left.ToInt64() + right.ToInt64());

	public static Pointer<T> operator -(Pointer<T> left, Pointer<T> right)
		=> (void*) (left.ToInt64() - right.ToInt64());

	/// <summary>
	///     Increments the <see cref="Address" /> by the specified number of elements.
	///     <remarks>
	///         Equal to <see cref="Pointer{T}.Increment" />
	///     </remarks>
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
	public static Pointer<T> operator +(Pointer<T> ptr, int i) => ptr.Increment(i);

	/// <summary>
	///     Decrements the <see cref="Address" /> by the specified number of elements.
	///     <remarks>
	///         Equal to <see cref="Pointer{T}.Decrement" />
	///     </remarks>
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
	public static Pointer<T> operator -(Pointer<T> ptr, int i) => ptr.Decrement(i);

	/// <summary>
	///     Increments the <see cref="Pointer{T}" /> by one element.
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <returns>The offset <see cref="Address" /></returns>
	public static Pointer<T> operator ++(Pointer<T> ptr) => ptr.Increment();

	/// <summary>
	///     Decrements the <see cref="Pointer{T}" /> by one element.
	/// </summary>
	/// <param name="ptr">
	///     <see cref="Pointer{T}" />
	/// </param>
	/// <returns>The offset <see cref="Address" /></returns>
	public static Pointer<T> operator --(Pointer<T> ptr) => ptr.Decrement();

	/// <summary>
	///     Increment <see cref="Address" /> by the specified number of elements
	/// </summary>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="elemCnt" /> elements incremented
	/// </returns>
	[Pure]
	public Pointer<T> Increment(int elemCnt = ELEM_CNT) => Offset(elemCnt);


	/// <summary>
	///     Decrement <see cref="Address" /> by the specified number of elements
	/// </summary>
	/// <param name="elemCnt">Number of elements</param>
	/// <returns>
	///     A new <see cref="Pointer{T}" /> with <paramref name="elemCnt" /> elements decremented
	/// </returns>
	[Pure]
	public Pointer<T> Decrement(int elemCnt = ELEM_CNT) => Increment(-elemCnt);

	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void* Offset(int elemCnt)
	{
		return (void*) ((long) m_value + Mem.FlatSize(ElementSize, elemCnt));
	}

	[Pure]
	public Pointer<T> AddressOfIndex(int index) => Offset(index);

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
	public Pointer<byte> ReadPointer(int elemOffset = OFFSET) => ReadPointer<byte>(elemOffset);

	[Pure]
	public Pointer<TType> ReadPointer<TType>(int elemOffset = OFFSET)
	{
		return Cast<Pointer<TType>>().Read(elemOffset);
	}

	public void WritePointer<TType>(Pointer<TType> ptr, int elemOffset = OFFSET)
	{
		Cast<Pointer<TType>>().Write(ptr, elemOffset);
	}


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

	public MemoryBasicInformation Query() => Mem.QueryMemoryPage(this);

	/// <summary>
	///     Default offset for <see cref="Pointer{T}" />
	/// </summary>
	private const int OFFSET = 0;

	/// <summary>
	///     Default increment/decrement/element count for <see cref="Pointer{T}" />
	/// </summary>
	private const int ELEM_CNT = 1;
}