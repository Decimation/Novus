using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novus.Win32.Structures;

namespace Novus.Runtime.Meta.Jit
{
	//todo

	/*
	 * https://georgeplotnikov.github.io/articles/just-in-time-hooking.html
	 * https://github.com/GeorgePlotnikov/ClrAnalyzer
	 */

	/*
	 * https://github.com/Decimation/RazorSharp/blob/acb4e213b938d0f3f9a952fe7f43d2f6f63a0f00/Test/Program.cs
	 * https://www.codeproject.com/Articles/463508/NET-CLR-Injection-Modify-IL-Code-during-Run-time
	 */

	public interface ICorStaticInfo
	{
		CorInfoFlag GetMethodAttribs(IntPtr thisPtr, [In] IntPtr methodHandle);
		CorInfoFlag GetMethodAttribsInternal(IntPtr thisPtr, [In] IntPtr methodHandle);
	}

	/// <summary>
	/// corjit.h
	/// </summary>
	/// 
	public unsafe interface ICorJitCompiler
	{
		CorJitCompiler.CorJitResult CompileMethod(IntPtr thisPtr, [In] IntPtr corJitInfo, [In] CorInfo* methodInfo,
			CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode);

		void ProcessShutdownWork(IntPtr thisPtr, [In] IntPtr corStaticInfo);
	}

	public unsafe class CorStaticInfo
	{
		[StructLayout(layoutKind: LayoutKind.Sequential)]
		public unsafe struct CorStaticInfoNative
		{
			public getMethodAttribsDel         GetMethodAttribs;
			public getMethodAttribsInternalDel GetMethodAttribsInternal;
		}

		public static ICorStaticInfo GetCorStaticInfoInterface(IntPtr ptr)
		{
			var corStaticInfoNative = Marshal.PtrToStructure<CorStaticInfoNative>(ptr);

			return new CorStaticInfoNativeWrapper(ptr, corStaticInfoNative.GetMethodAttribs,
				corStaticInfoNative.GetMethodAttribsInternal);
		}

		private sealed class CorStaticInfoNativeWrapper : ICorStaticInfo
		{
			private IntPtr _pThis;

			private getMethodAttribsDel         _getMethodAttribs;
			private getMethodAttribsInternalDel _getMethodAttribsInternal;

			public CorStaticInfoNativeWrapper(IntPtr pThis, getMethodAttribsDel getMethodAttribs,
				getMethodAttribsInternalDel getMethodAttribsInternal)
			{
				_pThis                    = pThis;
				_getMethodAttribs         = getMethodAttribs;
				_getMethodAttribsInternal = getMethodAttribsInternal;
			}

			public CorInfoFlag GetMethodAttribs(IntPtr thisPtr, [In] IntPtr methodHandle)
			{
				return _getMethodAttribs(thisPtr, methodHandle);
			}

			public CorInfoFlag GetMethodAttribsInternal(IntPtr thisPtr, [In] IntPtr methodHandle)
			{
				return _getMethodAttribsInternal(thisPtr, methodHandle);
			}
		}


		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorInfoFlag getMethodAttribsDel(IntPtr thisPtr, [In] IntPtr methodHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorInfoFlag getMethodAttribsInternalDel(IntPtr thisPtr, [In] IntPtr methodHandle);
	}

	public class CompiledMethodInfo<T>
	{
		public uint   CodeSize;
		public uint   PrologSize;
		public string ILCode { get; private set; }

		public bool IsBlendedCode
		{
			get
			{
				if (_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_DEBUG_CODE)) {
					var compCodeOpt = CorJitCompiler.CodeOptimize.BLENDED_CODE;

					// If the EE sets SIZE_OPT or if we are compiling a Class constructor
					// we will optimize for code size at the expense of speed
					//
					if (_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_SIZE_OPT) ||
					    ((_compFlags & CorInfoFlag.FLG_CCTOR) == CorInfoFlag.FLG_CCTOR)) {
						compCodeOpt = CorJitCompiler.CodeOptimize.SMALL_CODE;
					}
					//
					// If the EE sets SPEED_OPT we will optimize for speed at the expense of code size
					//
					else if (_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_SPEED_OPT) ||
					         (_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_TIER1) &&
					          !_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_MIN_OPT))) {
						compCodeOpt = CorJitCompiler.CodeOptimize.FAST_CODE;

						if (_corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_SIZE_OPT))
							throw new Exception("Seems CorJitFlags corrupted");
					}

					return compCodeOpt == CorJitCompiler.CodeOptimize.BLENDED_CODE;
				}

				//TODO: track https://github.com/dotnet/coreclr/blob/dbd533372e41b029398839056450c0fcac2b91f0/src/jit/compiler.h#L8533
				return true;
			}
		}

		public bool IsOptimizedCode => _corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_SIZE_OPT) ||
		                               _corJitFlags.IsSet(CorJitFlag.CORJIT_FLAG_SPEED_OPT);

		public bool IsRbpBasedFrame                 { get; private set; }
		public bool IsPartiallyInterruptible        { get; private set; }
		public bool IsFinalLocalVariableAssignments { get; private set; }

		private CorJitFlags __corJitFlags;

		private CorJitFlags _corJitFlags
		{
			get
			{
				if (__corJitFlags == null) __corJitFlags = new CorJitFlags(_corJitFlag);
				return __corJitFlags;
			}
		}

		private CorJitFlag  _corJitFlag;
		private CorInfoFlag _compFlags;

		public void Build(string path)
		{
			//TODO: parse file to fill the fields
		}

		private static CompilerHook hook;

#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
		public unsafe void SetUp()
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type
		{
			RuntimeHelpers.PrepareMethod(GetType().GetMethod("Release", System.Reflection.BindingFlags.Instance
			                                                            | System.Reflection.BindingFlags.Public)
				.MethodHandle, new[] {typeof(T).TypeHandle});

			RuntimeHelpers.PrepareMethod(GetType().GetMethod("NativeDump", System.Reflection.BindingFlags.Instance
			                                                               | System.Reflection.BindingFlags.Public |
			                                                               System.Reflection.BindingFlags.Static)
				.MethodHandle, new[] {typeof(T).TypeHandle});

			RuntimeHelpers.PrepareMethod(typeof(CorJitCompiler).GetMethod("DumpMethodInfo",
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).MethodHandle, null);
			hook = new CompilerHook();
			hook.Hook(CompileMethodDel);
		}

		public void Release() => hook.RemoveHook();

		public static unsafe void NativeDump(IntPtr corJitInfoPtr, CorInfo* methodInfo,
			CorJitFlag flags, IntPtr nativeEntry, IntPtr nativeSizeOfCode) =>
			CorJitCompiler.DumpMethodInfo(corJitInfoPtr, methodInfo, flags, nativeEntry, nativeSizeOfCode);

		internal static unsafe CorJitCompiler.CorJitResult CompileMethodDel(IntPtr thisPtr, [In] IntPtr corJitInfoPtr,
			[In] CorInfo* methodInfo,
			CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode)
		{
			hook.RemoveHook();
			var res = hook.Compile(thisPtr, corJitInfoPtr, methodInfo, flags, nativeEntry, nativeSizeOfCode);
			NativeDump(corJitInfoPtr, methodInfo, flags, nativeEntry, nativeSizeOfCode);
			return res;
		}
	}

	public unsafe class CompilerHook
	{
		public CorJitCompiler.CompileMethodDel Compile = null;

		private          IntPtr                              pJit;
		private          IntPtr                              pVTable;
		private          bool                                isHooked = false;
		private readonly CorJitCompiler.CorJitCompilerNative compiler;
		private          uint                                lpflOldProtect;

		public CompilerHook()
		{
			if (pJit == IntPtr.Zero) pJit = CorJitCompiler.GetJit();
			Debug.Assert(pJit != null);
			compiler = Marshal.PtrToStructure<CorJitCompiler.CorJitCompilerNative>(Marshal.ReadIntPtr(pJit));
			Debug.Assert(compiler.CompileMethod != null);
			pVTable = Marshal.ReadIntPtr(pJit);

			RuntimeHelpers.PrepareMethod(GetType().GetMethod("RemoveHook").MethodHandle);

			RuntimeHelpers.PrepareMethod(GetType().GetMethod("LockpVTable",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).MethodHandle);
		}

		/// <summary>
		/// From Windows.h
		/// https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa366898(v=vs.85).aspx
		/// </summary>
		[DllImport("kernel32.dll", BestFitMapping = true, CallingConvention = CallingConvention.Winapi,
			SetLastError                          = true, ExactSpelling     = true)]
		public static extern bool VirtualProtect(IntPtr lpAddress, UInt32 dwSize, MemoryProtection flNewProtect,
			out UInt32 lpflOldProtect);

		private bool UnlockpVTable()
		{
			if (!VirtualProtect(pVTable, (uint) IntPtr.Size, MemoryProtection.ExecuteReadWrite, out lpflOldProtect)) {
				Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				return false;
			}

			return true;
		}

		private bool LockpVTable()
		{
			return VirtualProtect(pVTable, (uint) IntPtr.Size, (MemoryProtection) lpflOldProtect, out lpflOldProtect);
		}

		public bool Hook(CorJitCompiler.CompileMethodDel hook)
		{
			if (!UnlockpVTable()) return false;

			Compile = compiler.CompileMethod;
			Debug.Assert(Compile != null);

			RuntimeHelpers.PrepareDelegate(hook);
			RuntimeHelpers.PrepareDelegate(Compile);

			Marshal.WriteIntPtr(pVTable, Marshal.GetFunctionPointerForDelegate(hook));

			return isHooked = LockpVTable();
		}

		public bool RemoveHook()
		{
			if (!isHooked) throw new InvalidOperationException("Impossible unhook not hooked compiler");
			if (!UnlockpVTable()) return false;

			Marshal.WriteIntPtr(pVTable, Marshal.GetFunctionPointerForDelegate(Compile));

			return LockpVTable();
		}
	}

	public class CorJitFlags
	{
		private CorJitFlag _corJitFlags;

		public CorJitFlags()
		{
			_corJitFlags = 0;
		}

		public CorJitFlags(CorJitFlag corJitFlags)
		{
			Set(corJitFlags);
		}

		public void Reset() => _corJitFlags = 0;
		public void Set(CorJitFlag flag) => _corJitFlags |= (CorJitFlag) ((UInt32) 1 << (Int32) flag);
		public void Clear(CorJitFlag flag) => _corJitFlags &= ~(CorJitFlag) ((UInt32) 1 << (Int32) flag);
		public void Add(CorJitFlag flag) => _corJitFlags |= flag;
		public void Remove(CorJitFlag flag) => _corJitFlags &= ~flag;
		public bool IsSet(CorJitFlag flag) => _corJitFlags.HasFlag(flag);
		public bool IsEmpty() => _corJitFlags == 0;
	}

	[StructLayout(layoutKind: LayoutKind.Sequential, Pack = 1, Size = 0x88)]
	public unsafe struct CorInfo
	{
		//ftn CORINFO_METHOD_HANDLE
		public IntPtr methodHandle;

		//scope CORINFO_MODULE_HANDLE
		public IntPtr moduleHandle;

		//BYTE*
		public IntPtr ILCode;
		public UInt32 ILCodeSize;
		public UInt16 maxStack;

		public UInt16 EHcount;

		//options CorInfoOptions
		public CorInfoOptions options;

		//regionKind CorInfoRegionKind
		public CorInfoRegionKind regionKind;

		//CORINFO_SIG_INFO
		public CorInfoSigInfo args;

		//CORINFO_SIG_INFO
		public CorInfoSigInfo locals;
	}


	//CORINFO_SIG_INFO
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorInfoSigInfo
	{
		//CorInfoCallConv
		public CorInfoCallConv callConv;

		//CORINFO_CLASS_HANDLE
		public IntPtr retTypeClass; // if the return type is a value class, this is its handle (enums are normalized)

		public IntPtr
			retTypeSigClass; // returns the value class as it is in the sig (enums are not converted to primitives)

		public CorInfoType    retType;
		public byte           flags; // used by IL stubs code
		public UInt16         numArgs;
		public CorinfoSigInst sigInst; // information about how type variables are being instantiated in generic code
		public IntPtr         args;
		public IntPtr         pSig;

		public UInt64 cbSig;

		//scope CORINFO_MODULE_HANDLE
		public IntPtr moduleHandle; // passed to getArgClass
		public UInt32 token;
	}


	//CORINFO_SIG_INST
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorinfoSigInst
	{
		public UInt64  classInstCount;
		public IntPtr* classInst; // (representative, not exact) instantiation for class type variables in signature
		public UInt64  methInstCount;
		public IntPtr* methInst; // (representative, not exact) instantiation for method type variables in signature
	}

	//corinfo.h

	//CORINFO_RESOLVED_TOKEN
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorinfoResolvedToken
	{
		//
		// [In] arguments of resolveToken
		//
		public IntPtr           tokenContext; //Context for resolution of generic arguments
		public IntPtr           tokenScope;
		public UInt32           token; //The source token
		public CorInfoTokenKind tokenType;

		//
		// [Out] arguments of resolveToken. 
		// - Type handle is always non-NULL.
		// - At most one of method and field handles is non-NULL (according to the token type).
		// - Method handle is an instantiating stub only for generic methods. Type handle 
		//   is required to provide the full context for methods in generic types.
		//
		public IntPtr hClass;
		public IntPtr hMethod;
		public IntPtr hField;

		//
		// [Out] TypeSpec and MethodSpec signatures for generics. NULL otherwise.
		//
		public Byte   pTypeSpec;
		public UInt32 cbTypeSpec;
		public Byte   pMethodSpec;
		public UInt32 cbMethodSpec;
	}


	//CORINFO_CALL_INFO
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorinfoCallInfo
	{
		public IntPtr hMethod;     //target method handle
		public UInt32 methodFlags; //flags for the target method

		public UInt32 classFlags; //flags for CORINFO_RESOLVED_TOKEN::hClass

		public CorInfoSigInfo sig;

		//Verification information
		public UInt32 verMethodFlags; // flags for CORINFO_RESOLVED_TOKEN::hMethod

		public CorInfoSigInfo verSig;
		//All of the regular method data is the same... hMethod might not be the same as CORINFO_RESOLVED_TOKEN::hMethod


		//If set to:
		//  - CORINFO_ACCESS_ALLOWED - The access is allowed.
		//  - CORINFO_ACCESS_ILLEGAL - This access cannot be allowed (i.e. it is public calling private).  The
		//      JIT may either insert the callsiteCalloutHelper into the code (as per a verification error) or
		//      call throwExceptionFromHelper on the callsiteCalloutHelper.  In this case callsiteCalloutHelper
		//      is guaranteed not to return.
		//  - CORINFO_ACCESS_RUNTIME_CHECK - The jit must insert the callsiteCalloutHelper at the call site.
		//      the helper may return
		public CorInfoIsAccessAllowedResult accessAllowed;
		public CorinfoHelperDesc            callsiteCalloutHelper;

		// See above section on constraintCalls to understand when these are set to unusual values.
		public CorinfoThisTransform thisTransform;

		public CorinfoCallKind kind;
		public bool            nullInstanceCheck;

		// Context for inlining and hidden arg
		public IntPtr contextHandle;

		public bool
			exactContextNeedsRuntimeLookup; // Set if contextHandle is approx handle. Runtime lookup is required to get the exact handle.

		// If kind.CORINFO_VIRTUALCALL_STUB then stubLookup will be set.
		// If kind.CORINFO_CALL_CODE_POINTER then entryPointLookup will be set.
		[StructLayout(LayoutKind.Explicit)]
		public struct lookup
		{
			[FieldOffset(0)] CorinfoLookup stubLookup;
			[FieldOffset(0)] CorinfoLookup codePointerLookup;
		}

		public CorinfoConstLookup instParamLookup; // Used by Ready-to-Run

		public bool secureDelegateInvoke;
	}

	//CORINFO_HELPER_DESC
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorinfoHelperDesc
	{
		public CorInfoHelpFunc helperNum;
		public UInt16          numArgs;

		[StructLayout(LayoutKind.Explicit)]
		public struct args
		{
			[FieldOffset(0)] UInt32 fieldHandle;
			[FieldOffset(0)] UInt32 methodHandle;
			[FieldOffset(0)] UInt32 classHandle;
			[FieldOffset(0)] UInt32 moduleHandle;
			[FieldOffset(0)] UInt32 constant;
		};
	}


	//CORINFO_CONST_LOOKUP
	[StructLayout(layoutKind: LayoutKind.Explicit)]
	public unsafe struct CorinfoConstLookup
	{
		// If the handle is obtained at compile-time, then this handle is the "exact" handle (class, method, or field)
		// Otherwise, it's a representative... 
		// If accessType is
		//     IAT_VALUE   --> "handle" stores the real handle or "addr " stores the computed address
		//     IAT_PVALUE  --> "addr" stores a pointer to a location which will hold the real handle
		//     IAT_PPVALUE --> "addr" stores a double indirection to a location which will hold the real handle
		[FieldOffset(0)]             public InfoAccessType accessType;
		[FieldOffset(sizeof(Int32))] public IntPtr         handle;
		[FieldOffset(sizeof(Int32))] public void*          addr;
	}

	// Can a value be accessed directly from JITed code.
	public enum InfoAccessType
	{
		IAT_VALUE,  // The info value is directly available
		IAT_PVALUE, // The value needs to be accessed via an       indirection
		IAT_PPVALUE // The value needs to be accessed via a double indirection
	}

	// Result of calling embedGenericHandle
	//CORINFO_LOOKUP
	[StructLayout(layoutKind: LayoutKind.Explicit)]
	public unsafe struct CorinfoLookup
	{
		[FieldOffset(0)] public CORINFO_LOOKUP_KIND lookupKind;

		// If kind.needsRuntimeLookup then this indicates how to do the lookup
		[FieldOffset(sizeof(bool) + sizeof(UInt32) + sizeof(UInt16) +
#if _TARGET_X64_
			4
#else
		             2
#endif
		)]
		public CorinfoRuntimeLookup runtimeLookup;

		// If the handle is obtained at compile-time, then this handle is the "exact" handle (class, method, or field)
		// Otherwise, it's a representative...  If accessType is
		//     IAT_VALUE --> "handle" stores the real handle or "addr " stores the computed address
		//     IAT_PVALUE --> "addr" stores a pointer to a location which will hold the real handle
		//     IAT_PPVALUE --> "addr" stores a double indirection to a location which will hold the real handle
		[FieldOffset(sizeof(bool) + sizeof(UInt32) + sizeof(UInt16) +
#if _TARGET_X64_
			4
#else
		             2
#endif
		)]
		public CorinfoConstLookup constLookup;
	}

	//CORINFO_LOOKUP_KIND
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CORINFO_LOOKUP_KIND
	{
		public bool                     needsRuntimeLookup;
		public CorinfoRuntimeLookupKind runtimeLookupKind;

		// The 'runtimeLookupFlags' and 'runtimeLookupArgs' fields
		// are just for internal VM / ZAP communication, not to be used by the JIT.
		public UInt16 runtimeLookupFlags;
		public void*  runtimeLookupArgs;
	}

	public enum CorinfoRuntimeLookupKind
	{
		CORINFO_LOOKUP_THISOBJ,
		CORINFO_LOOKUP_METHODPARAM,
		CORINFO_LOOKUP_CLASSPARAM,
	}

	//CORINFO_RUNTIME_LOOKUP
	[StructLayout(layoutKind: LayoutKind.Sequential)]
	public unsafe struct CorinfoRuntimeLookup
	{
		// This is signature you must pass back to the runtime lookup helper
		public void* signature;

		// Here is the helper you must call. It is one of CORINFO_HELP_RUNTIMEHANDLE_* helpers.
		public CorInfoHelpFunc helper;

		// Number of indirections to get there
		// CORINFO_USEHELPER = don't know how to get it, so use helper function at run-time instead
		// 0 = use the this pointer itself (e.g. token is C<!0> inside code in sealed class C)
		//     or method desc itself (e.g. token is method void M::mymeth<!!0>() inside code in M::mymeth)
		// Otherwise, follow each byte-offset stored in the "offsets[]" array (may be negative)
		public UInt16 indirections;

		// If set, test for null and branch to helper if null
		public bool testForNull;

		// If set, test the lowest bit and dereference if set (see code:FixupPointer)
		public bool testForFixup;

		public IntPtr offsets; //UInt32[#define CORINFO_MAXINDIRECTIONS 4]

		// If set, first offset is indirect.
		// 0 means that value stored at first offset (offsets[0]) from pointer is next pointer, to which the next offset
		// (offsets[1]) is added and so on.
		// 1 means that value stored at first offset (offsets[0]) from pointer is offset1, and the next pointer is
		// stored at pointer+offsets[0]+offset1.
		public bool indirectFirstOffset;

		// If set, second offset is indirect.
		// 0 means that value stored at second offset (offsets[1]) from pointer is next pointer, to which the next offset
		// (offsets[2]) is added and so on.
		// 1 means that value stored at second offset (offsets[1]) from pointer is offset2, and the next pointer is
		// stored at pointer+offsets[1]+offset2.
		public bool indirectSecondOffset;
	}

	public unsafe class CorJitCompiler
	{
		public unsafe struct CorJitCompilerNative
		{
			public CompileMethodDel          CompileMethod;
			public ProcessShutdownWorkDel    ProcessShutdownWork;
			public isCacheCleanupRequiredDel isCacheCleanupRequired;
			public getMethodAttribs          getMethodAttribs;
		}

		public static ICorJitCompiler GetCorJitCompilerInterface()
		{
			var pJit           = GetJit();
			var nativeCompiler = Marshal.PtrToStructure<CorJitCompilerNative>(pJit);

			return new CorJitCompilerNativeWrapper(pJit, nativeCompiler.CompileMethod,
				nativeCompiler.ProcessShutdownWork, nativeCompiler.getMethodAttribs);
		}

		private sealed class CorJitCompilerNativeWrapper : ICorJitCompiler
		{
			private IntPtr                 _pThis;
			private CompileMethodDel       _compileMethod;
			private ProcessShutdownWorkDel _processShutdownWork;
			private getMethodAttribs       _getMethodAttribs;

			public CorJitCompilerNativeWrapper(IntPtr pThis, CompileMethodDel compileMethodDel,
				ProcessShutdownWorkDel processShutdownWork, getMethodAttribs getMethodAttribs)
			{
				_pThis               = pThis;
				_compileMethod       = compileMethodDel;
				_processShutdownWork = processShutdownWork;
				_getMethodAttribs    = getMethodAttribs;
			}

			public CorJitResult CompileMethod(IntPtr thisPtr, [In] IntPtr corJitInfo, [In] CorInfo* methodInfo,
				CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode)
			{
				return _compileMethod(thisPtr, corJitInfo, methodInfo, flags, nativeEntry, nativeSizeOfCode);
			}

			public void ProcessShutdownWork(IntPtr thisPtr, [In] IntPtr corStaticInfo)
			{
				_processShutdownWork(thisPtr, corStaticInfo);
			}

			public UInt32 getMethodAttribs(IntPtr methodHandle)
			{
				return _getMethodAttribs(methodHandle);
			}
		}

		[DllImport(
			"Clrjit.dll"
			, CallingConvention = CallingConvention.StdCall, SetLastError = true, EntryPoint = "getJit",
			BestFitMapping      = true)]
		public static extern IntPtr GetJit();

		[DllImport("Win32Native.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true,
			EntryPoint                                  = "DumpMethodInfo", BestFitMapping        = true)]
		public static extern void DumpMethodInfo(IntPtr corJitInfo, CorInfo* methodInfo, CorJitFlag flags,
			IntPtr nativeEntry, IntPtr nativeSizeOfCode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorJitResult CompileMethodDel(IntPtr thisPtr, [In] IntPtr corJitInfo,
			[In] CorInfo* methodInfo, CorJitFlag flags, [Out] IntPtr nativeEntry,
			[Out] IntPtr nativeSizeOfCode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate void ProcessShutdownWorkDel(IntPtr thisPtr, [Out] IntPtr corStaticInfo);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate Byte isCacheCleanupRequiredDel();

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate UInt32 getMethodAttribs(IntPtr methodHandle);

		// These are error codes returned by CompileMethod
		public const Int32 SEVERITY_ERROR = 1;
		public const Int32 FACILITY_NULL  = 0;

		public enum CorJitResult : Int32
		{
			CORJIT_OK = 0,

			CORJIT_BADCODE = unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                                    ((UInt32) (1)))),

			CORJIT_OUTOFMEM = unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                                     ((UInt32) (2)))),

			CORJIT_INTERNALERROR =
				unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
				                   ((UInt32) (3)))),

			CORJIT_SKIPPED = unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                                    ((UInt32) (4)))),

			CORJIT_RECOVERABLEERROR =
				unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
				                   ((UInt32) (5)))),
		};

		public enum CodeOptimize
		{
			BLENDED_CODE,
			SMALL_CODE,
			FAST_CODE,
			COUNT_OPT_CODE
		};
	}
}