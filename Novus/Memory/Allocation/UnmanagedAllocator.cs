using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.Win32;

// ReSharper disable UnusedMember.Global

namespace Novus.Memory.Allocation;

/// <summary>
/// Unmanaged memory allocator. Uses <em>HGLOBAL</em> memory.
/// </summary>
public sealed class UnmanagedAllocator : IAllocator
{
	/*
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/NativeMemory.Windows.cs
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/Interop/Windows/Ucrtbase/Interop.MemAlloc.cs
	 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/NativeMemory.cs
	 */

	public nuint AllocSize(Pointer ptr) => (nuint) Native.LocalSize(ptr.Address);

	[MustUseReturnValue]
	public Pointer ReAlloc(Pointer ptr, nuint cb)
	{
		ptr = Marshal.ReAllocHGlobal(ptr.Address, (IntPtr) (uint) cb);
		return ptr;
	}

	public void Free(Pointer ptr) => Marshal.FreeHGlobal(ptr.Address);


	/// <summary>
	/// Allocates memory for <paramref name="cb"></paramref> elements of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Element type</typeparam>
	/// <param name="cb">Number of elements</param>
	[MustUseReturnValue]
	public Pointer Alloc(nuint cb) => Marshal.AllocHGlobal((int) cb);
}