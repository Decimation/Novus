using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Novus.Imports;
using Novus.Memory;

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Novus.Runtime.Meta
{
	/// <summary>
	/// GC heap
	/// </summary>
	/// <seealso cref="GC"/>
	/// <seealso cref="FormatterServices"/>
	/// <seealso cref="Activator"/>
	/// <seealso cref="RuntimeHelpers"/>
	/// <seealso cref="System.Runtime.InteropServices.GCHandle"/>
	public static unsafe class GCHeap
	{
		/// <summary>
		/// <c>g_pGCHeap</c>
		/// </summary>
		[field: ImportClr("g_pGCHeap", UnmanagedType.Offset)]
		public static Pointer<byte> GlobalHeap { get; }

		public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class =>
			IsHeapPointer(Mem.AddressOfHeap(t), smallHeapOnly);

		public static bool IsHeapPointer(Pointer<byte> ptr, bool smallHeapOnly = false) =>
			Func_IsHeapPointer(GlobalHeap.ToPointer(), ptr.ToPointer(), smallHeapOnly);


		static GCHeap()
		{

			Resource.LoadImports(typeof(GCHeap));
		}

		/// <summary>
		/// <see cref="IsHeapPointer"/>
		/// </summary>
		[field: ImportClr("Ofs_IsHeapPointer", UnmanagedType.Offset)]
		private static delegate* unmanaged<void*, void*, bool, bool> Func_IsHeapPointer { get; }
	}
}