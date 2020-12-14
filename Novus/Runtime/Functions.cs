using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Novus.Interop;
using Novus.Runtime.VM;
using Novus.Runtime.VM.EE;

// ReSharper disable UnassignedGetOnlyAutoProperty

// ReSharper disable InconsistentNaming

namespace Novus.Runtime
{
	/// <summary>
	/// Contains internal runtime functions.
	/// </summary>
	public static unsafe class Functions
	{
		/*
		 * Native internal CLR functions
		 *
		 * Originally, IL had to be used to call native functions as the calli opcode was needed.
		 *
		 * Now, we can use C# 9 unmanaged function pointers because they are implemented using
		 * the calli opcode.
		 *
		 * Unmanaged function pointers are backed by IntPtr.
		 *
		 * https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/function-pointers
		 * https://github.com/dotnet/csharplang/blob/master/proposals/csharp-9.0/function-pointers.md
		 * https://devblogs.microsoft.com/dotnet/improvements-in-native-code-interop-in-net-5-0/
		 *
		 * Normal delegates using the UnmanagedFunctionPointer attribute is also possible, but it's
		 * better to use the new unmanaged function pointers.
		 */


		/// <summary>
		/// <see cref="TypeHandle.MethodTable"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetMethodTable")]
		internal static delegate* unmanaged<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }

		/// <summary>
		/// <see cref="FieldDesc.Size"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetSize")]
		internal static delegate* unmanaged<FieldDesc*, int> Func_GetSize { get; }

		/// <summary>
		/// <see cref="MethodDesc.IsPointingToNativeCode"/>
		/// </summary>
		[field: ImportClrFunction("Sig_IsPointingToNativeCode")]
		internal static delegate* unmanaged<MethodDesc*, int> Func_IsPointingToNativeCode { get; }

		/// <summary>
		/// <see cref="MethodDesc.NativeCode"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetNativeCode")]
		internal static delegate* unmanaged<MethodDesc*, void*> Func_GetNativeCode { get; }

		/// <summary>
		/// <see cref="MethodDesc.Token"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetMemberDef")]
		internal static delegate* unmanaged<MethodDesc*, int> Func_GetToken { get; }

		/// <summary>
		/// <see cref="MethodDesc.RVA"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetRVA")]
		internal static delegate* unmanaged<MethodDesc*, long> Func_GetRVA { get; }

		/// <summary>
		/// <see cref="MethodTable.EEClass"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetEEClass")]
		internal static delegate* unmanaged<MethodTable*, EEClass*> Func_GetClass { get; }

		/// <summary>
		/// <see cref="MethodTable.NativeLayoutInfo"/>
		/// </summary>
		[field: ImportClrFunction("Sig_GetNativeLayoutInfo")]
		internal static delegate* unmanaged<MethodTable*, EEClassNativeLayoutInfo*> Func_GetNativeLayoutInfo { get; }


		/*
		 * Managed internal functions
		 */

		
		
		/// <summary>
		/// <see cref="RuntimeInfo.ResolveType"/>
		/// </summary>
		[field: ImportManagedFunction(typeof(Type), "GetTypeFromHandleUnsafe")]
		internal static delegate* managed<IntPtr, Type> Func_GetTypeFromHandle { get; }

		/// <summary>
		/// <see cref="RuntimeInfo.IsPinnable"/>
		/// </summary>
		[field: ImportManagedFunction(typeof(Marshal), "IsPinnable")]
		internal static delegate* managed<object, bool> Func_IsPinnable { get; }


		static Functions()
		{
			Debug.WriteLine($"Loading {nameof(Functions)}");


			/*
			 * Load imports
			 */


			Resource.LoadImports(typeof(Functions));
		}
	}
}