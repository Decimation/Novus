// Author: Deci | Project: Novus | Name: SC_HANDLE.cs
// Date: 2025/02/05 @ 14:02:22

using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Novus.Win32.Structures.AdvApi32;

/// <summary>Provides a handle to a service.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct ScHandle
{

	private readonly nint handle;

	/// <summary>Initializes a new instance of the <see cref="ScHandle" /> struct.</summary>
	/// <param name="preexistingHandle">An <see cref="nint" /> object that represents the pre-existing handle to use.</param>
	public ScHandle(nint preexistingHandle)
	{
		handle = preexistingHandle;
	}

	/// <summary>Returns an invalid handle by instantiating a <see cref="ScHandle" /> object with <see cref="nint.Zero" />.</summary>
	public static ScHandle NULL => new(nint.Zero);

	/// <summary>Gets a value indicating whether this instance is a null handle.</summary>
	public bool IsNull => handle == nint.Zero;

	/// <summary>Performs an explicit conversion from <see cref="ScHandle" /> to <see cref="nint" />.</summary>
	/// <param name="h">The handle.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator nint(ScHandle h)
	{
		return h.handle;
	}

	/// <summary>Performs an implicit conversion from <see cref="nint" /> to <see cref="ScHandle" />.</summary>
	/// <param name="h">The pointer to a handle.</param>
	/// <returns>The result of the conversion.</returns>
	public static implicit operator ScHandle(nint h)
	{
		return new ScHandle(h);
	}

	/// <summary>Implements the operator !=.</summary>
	/// <param name="h1">The first handle.</param>
	/// <param name="h2">The second handle.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator !=(ScHandle h1, ScHandle h2)
	{
		return !(h1 == h2);
	}

	/// <summary>Implements the operator ==.</summary>
	/// <param name="h1">The first handle.</param>
	/// <param name="h2">The second handle.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator ==(ScHandle h1, ScHandle h2)
	{
		return h1.Equals(h2);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj is ScHandle h && handle == h.handle;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return handle.GetHashCode();
	}

	/// <inheritdoc />
	public nint DangerousGetHandle()
	{
		return handle;
	}

}