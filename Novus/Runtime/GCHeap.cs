using System;
using Novus.Imports;
using Novus.Memory;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Novus.Runtime
{
	/// <summary>
	/// GC heap
	/// </summary>
	/// <seealso cref="GC"/>
	public static unsafe class GCHeap
	{
		/// <summary>
		/// <c>g_pGCHeap</c>
		/// </summary>
		[field: ImportClrComponent("g_pGCHeap", UnmanagedType.Offset)]
		public static Pointer<byte> GlobalHeap { get; }

		public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class
		{
			return IsHeapPointer(Mem.AddressOfHeap(t), smallHeapOnly);
		}

		public static bool IsHeapPointer(Pointer<byte> ptr, bool smallHeapOnly = false)
		{
			return Functions.Func_IsHeapPointer(GlobalHeap.ToPointer(), ptr.ToPointer(), smallHeapOnly);
		}

		static GCHeap()
		{
			Resource.LoadImports(typeof(GCHeap));
		}
	}
}