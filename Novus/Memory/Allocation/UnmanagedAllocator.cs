using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Novus.Win32;
#pragma warning disable CA1416

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

	public nint GetSize(Pointer ptr) => (nint) Native.LocalSize(ptr.Address);

	public Pointer ReAlloc(Pointer ptr, nuint cb)
	{
		ptr = Marshal.ReAllocHGlobal(ptr.Address, (nint) (uint) cb);
		return ptr;
	}

	public void Free(Pointer ptr) => Marshal.FreeHGlobal(ptr.Address);

	public Pointer Alloc(nuint cb) => Marshal.AllocHGlobal((int) cb);
}