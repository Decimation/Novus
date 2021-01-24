using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novus.Interop;
using Novus.Memory;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM
{
	public static unsafe class GCHeap
	{
		/// <summary>
		/// <c>g_pGCHeap</c>
		/// </summary>
		[field: ImportClrComponent("g_pGCHeap", UnmanagedType.Offset)]
		public static Pointer<byte> GlobalHeap { get; internal set; }

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
			//todo
			// const int g_pGCHeapOffset = 0x4A4D98;
			//
			// var ptr = Global.Clr.Address + g_pGCHeapOffset;
			// var gc  = ptr.ReadPointer();
			//
			// GCHeap.GlobalHeap = gc;

			Resource.LoadImports(typeof(GCHeap));
		}
	}
}