using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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


		/*
		 *	| Method |     Mean |    Error |   StdDev |
			|------- |---------:|---------:|---------:|
			|  Test1 | 43.62 ns | 0.791 ns | 0.740 ns |
			|  Test2 | 42.70 ns | 0.316 ns | 0.280 ns |
			|  Test3 | 24.76 ns | 0.446 ns | 0.417 ns |
		 */


		private static T AllocObject<T>(Type t, params object[] args)
		{
			var ptr = Func_AllocObj(t.TypeHandle.Value.ToPointer(), false);

			T obj = Unsafe.Read<T>(&ptr);

			ReflectionHelper.CallConstructor(obj, args);
			

			return obj;
		}

		public static T AllocObject<T>(params object[] args) => AllocObject<T>(typeof(T), args);

		public static object AllocObject(Type t, params object[] args) => AllocObject<object>(t, args);


		public static bool IsHeapPointer<T>(T t, bool smallHeapOnly = false) where T : class =>
			IsHeapPointer(Mem.AddressOfHeap(t), smallHeapOnly);

		public static bool IsHeapPointer(Pointer<byte> ptr, bool smallHeapOnly = false) =>
			Func_IsHeapPointer(GlobalHeap.ToPointer(), ptr.ToPointer(), smallHeapOnly);

		/// <summary>
		/// <c>g_pGCHeap</c>
		/// </summary>
		[field: ImportClr("Ofs_GCHeap", UnmanagedImportType.Offset)]
		public static Pointer<byte> GlobalHeap { get; }

		[field: ImportClr("Sig_AllocObj")]
		private static delegate* unmanaged<void*, bool, void*> Func_AllocObj { get; }

		/// <summary>
		/// <see cref="IsHeapPointer"/>
		/// </summary>
		[field: ImportClr("Ofs_IsHeapPointer", UnmanagedImportType.Offset)]
		private static delegate* unmanaged[Thiscall]<void*, void*, bool, bool> Func_IsHeapPointer { get; }
	}
}