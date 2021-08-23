using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Novus.Imports;
using Novus.Memory;
using Novus.Utilities;

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
		static GCHeap()
		{
			Global.Clr.LoadImports(typeof(GCHeap));
		}

		[MustUseReturnValue]
		[CanBeNull]
		public static T AllocObject<T>(params object[] args) => (T) AllocObject(typeof(T), args);

		[MustUseReturnValue]
		[CanBeNull]
		public static object AllocObject(Type t, params object[] args)
		{
			var obj = Func_TypeHandleAlloc(t.TypeHandle);

			ReflectionHelper.CallConstructor(obj, args);

			return obj;
		}

		public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class =>
			IsHeapPointer(Mem.AddressOfHeap(t), smallHeapOnly);

		public static bool IsHeapPointer(Pointer<byte> ptr, bool smallHeapOnly = false) =>
			Func_IsHeapPointer(GlobalHeap.ToPointer(), ptr.ToPointer(), smallHeapOnly);

		/// <summary>
		/// <c>g_pGCHeap</c>
		/// </summary>
		[field: ImportClr("Ofs_GCHeap", UnmanagedImportType.Offset)]
		public static Pointer<byte> GlobalHeap { get; }

		/// <summary>
		/// <see cref="IsHeapPointer"/>
		/// </summary>
		[field: ImportClr("Ofs_IsHeapPointer", UnmanagedImportType.Offset)]
		private static delegate* unmanaged[Thiscall]<void*, void*, bool, bool> Func_IsHeapPointer { get; }

		[field: ImportManaged(typeof(RuntimeTypeHandle), "Allocate")]
		private static delegate* managed<RuntimeTypeHandle, object> Func_TypeHandleAlloc { get; }
	}
}