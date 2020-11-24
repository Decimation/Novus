using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Novus.CoreClr.VM;
using Novus.CoreClr.VM.EE;
using Novus.Properties;
using Novus.Utilities;

// ReSharper disable InconsistentNaming

namespace Novus.CoreClr
{
	internal static unsafe class ClrFunctions
	{
		/// <summary>
		/// <seealso cref="TypeHandle.MethodTable"/>
		/// </summary>
		internal static delegate* unmanaged<TypeHandle*, MethodTable*> Func_GetMethodTable { get; }

		/// <summary>
		/// <seealso cref="FieldDesc.Size"/>
		/// </summary>
		internal static delegate* unmanaged<FieldDesc*, int> Func_GetSize { get; }

		/// <summary>
		/// <seealso cref="MethodDesc.IsPointingToNativeCode"/>
		/// </summary>
		internal static delegate* unmanaged<MethodDesc*, int> Func_IsPointingToNativeCode { get; }

		/// <summary>
		/// <seealso cref="MethodDesc.NativeCode"/>
		/// </summary>
		internal static delegate* unmanaged<MethodDesc*, void*> Func_GetNativeCode { get; }

		/// <summary>
		/// <seealso cref="MethodDesc.Token"/>
		/// </summary>
		internal static delegate* unmanaged<MethodDesc*, int> Func_GetToken { get; }

		/// <summary>
		/// <seealso cref="MethodDesc.RVA"/>
		/// </summary>
		internal static delegate* unmanaged<MethodDesc*, long> Func_GetRVA { get; }

		/// <summary>
		/// <seealso cref="MethodTable.EEClass"/>
		/// </summary>
		internal static delegate* unmanaged<MethodTable*, EEClass*> Func_GetClass { get; }

		
		internal static GetTypeFromHandleDelegate Func_GetTypeFromHandle { get; }


		internal static IsPinnableDelegate Func_IsPinnable { get; }


		internal delegate bool IsPinnableDelegate(object o);

		internal delegate Type GetTypeFromHandleDelegate(IntPtr i);

		static ClrFunctions()
		{
			Debug.WriteLine($"{nameof(ClrFunctions)}");


			Func_GetMethodTable = (delegate* unmanaged<TypeHandle*, MethodTable*>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetMethodTable);

			Func_GetSize = (delegate* unmanaged<FieldDesc*, int>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetSize);

			Func_IsPointingToNativeCode = (delegate* unmanaged<MethodDesc*, int>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_IsPointingToNativeCode);


			Func_GetNativeCode = (delegate* unmanaged<MethodDesc*, void*>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetNativeCode);


			Func_GetToken = (delegate* unmanaged<MethodDesc*, int>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetMemberDef);

			Func_GetRVA = (delegate* unmanaged<MethodDesc*, long>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetRVA);

			Func_GetClass = (delegate* unmanaged<MethodTable*, EEClass*>) Resources.Clr.Scanner.FindSignature(
				EmbeddedResources.Sig_GetEEClass);


			Func_GetTypeFromHandle = typeof(Type).GetAnyMethod("GetTypeFromHandleUnsafe")
				.CreateDelegate<GetTypeFromHandleDelegate>();


			Func_IsPinnable = typeof(Marshal).GetAnyMethod("IsPinnable")
				.CreateDelegate<IsPinnableDelegate>();
		}
	}
}