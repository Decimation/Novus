using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Novus.Win32;
using Novus.Win32.Structures;

namespace Novus.Runtime
{
	public interface ICorStaticInfo
	{
		CorStaticInfo.CorInfoFlag GetMethodAttribs(IntPtr thisPtr, [In] IntPtr methodHandle);
		CorStaticInfo.CorInfoFlag GetMethodAttribsInternal(IntPtr thisPtr, [In] IntPtr methodHandle);
	}

	public unsafe class CorStaticInfo
	{
		[StructLayout(layoutKind: LayoutKind.Sequential)]
		public unsafe struct CorStaticInfoNative
		{
			public getMethodAttribsDel GetMethodAttribs;
			public getMethodAttribsInternalDel GetMethodAttribsInternal;
		}

		public static ICorStaticInfo GetCorStaticInfoInterface(IntPtr ptr)
		{
			var corStaticInfoNative = Marshal.PtrToStructure<CorStaticInfoNative>(ptr);
			return new CorStaticInfoNativeWrapper(ptr, corStaticInfoNative.GetMethodAttribs, corStaticInfoNative.GetMethodAttribsInternal);
		}

		private sealed class CorStaticInfoNativeWrapper : ICorStaticInfo
		{
			private IntPtr _pThis;
			private getMethodAttribsDel _getMethodAttribs;
			private getMethodAttribsInternalDel _getMethodAttribsInternal;

			public CorStaticInfoNativeWrapper(IntPtr pThis, getMethodAttribsDel getMethodAttribs, getMethodAttribsInternalDel getMethodAttribsInternal)
			{
				_pThis = pThis;
				_getMethodAttribs = getMethodAttribs;
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

		//from coreinfo.h
		// these are the attribute flags for fields and methods (getMethodAttribs)
		public enum CorInfoFlag : UInt32
		{
			//  CORINFO_FLG_UNUSED                = 0x00000001,
			//  CORINFO_FLG_UNUSED                = 0x00000002,
			CORINFO_FLG_PROTECTED = 0x00000004,
			CORINFO_FLG_STATIC = 0x00000008,
			CORINFO_FLG_FINAL = 0x00000010,
			CORINFO_FLG_SYNCH = 0x00000020,
			CORINFO_FLG_VIRTUAL = 0x00000040,
			//  CORINFO_FLG_UNUSED                = 0x00000080,
			CORINFO_FLG_NATIVE = 0x00000100,
			CORINFO_FLG_INTRINSIC_TYPE = 0x00000200, // This type is marked by [Intrinsic]
			CORINFO_FLG_ABSTRACT = 0x00000400,

			CORINFO_FLG_EnC = 0x00000800, // member was added by Edit'n'Continue

			// These are internal flags that can only be on methods
			CORINFO_FLG_FORCEINLINE = 0x00010000, // The method should be inlined if possible.
			CORINFO_FLG_SHAREDINST = 0x00020000, // the code for this method is shared between different generic instantiations (also set on classes/types)
			CORINFO_FLG_DELEGATE_INVOKE = 0x00040000, // "Delegate
			CORINFO_FLG_PINVOKE = 0x00080000, // Is a P/Invoke call
			CORINFO_FLG_SECURITYCHECK = 0x00100000, // Is one of the security routines that does a stackwalk (e.g. Assert, Demand)
			CORINFO_FLG_NOGCCHECK = 0x00200000, // This method is FCALL that has no GC check.  Don't put alone in loops
			CORINFO_FLG_INTRINSIC = 0x00400000, // This method MAY have an intrinsic ID
			CORINFO_FLG_CONSTRUCTOR = 0x00800000, // This method is an instance or type initializer
												  //  CORINFO_FLG_UNUSED                = 0x01000000,
												  //  CORINFO_FLG_UNUSED                = 0x02000000,
			CORINFO_FLG_NOSECURITYWRAP = 0x04000000, // The method requires no security checks
			CORINFO_FLG_DONT_INLINE = 0x10000000, // The method should not be inlined
			CORINFO_FLG_DONT_INLINE_CALLER = 0x20000000, // The method should not be inlined, nor should its callers. It cannot be tail called.
			CORINFO_FLG_JIT_INTRINSIC = 0x40000000, // Method is a potential jit intrinsic; verify identity by name check

			// These are internal flags that can only be on Classes
			CORINFO_FLG_VALUECLASS = 0x00010000, // is the class a value class
												 //  This flag is define din the Methods section, but is also valid on classes.
												 //  CORINFO_FLG_SHAREDINST            = 0x00020000, // This class is satisfies TypeHandle::IsCanonicalSubtype
			CORINFO_FLG_VAROBJSIZE = 0x00040000, // the object size varies depending of constructor args
			CORINFO_FLG_ARRAY = 0x00080000, // class is an array class (initialized differently)
			CORINFO_FLG_OVERLAPPING_FIELDS = 0x00100000, // struct or class has fields that overlap (aka union)
			CORINFO_FLG_INTERFACE = 0x00200000, // it is an interface
			CORINFO_FLG_CONTEXTFUL = 0x00400000, // is this a contextful class?
			CORINFO_FLG_CUSTOMLAYOUT = 0x00800000, // does this struct have custom layout?
			CORINFO_FLG_CONTAINS_GC_PTR = 0x01000000, // does the class contain a gc ptr ?
			CORINFO_FLG_DELEGATE = 0x02000000, // is this a subclass of delegate or multicast delegate ?
			CORINFO_FLG_MARSHAL_BYREF = 0x04000000, // is this a subclass of MarshalByRef ?
			CORINFO_FLG_CONTAINS_STACK_PTR = 0x08000000, // This class has a stack pointer inside it
			CORINFO_FLG_VARIANCE = 0x10000000, // MethodTable::HasVariance (sealed does *not* mean uncast-able)
			CORINFO_FLG_BEFOREFIELDINIT = 0x20000000, // Additional flexibility for when to run .cctor (see code:#ClassConstructionFlags)
			CORINFO_FLG_GENERIC_TYPE_VARIABLE = 0x40000000, // This is really a handle for a variable type
			CORINFO_FLG_UNSAFE_VALUECLASS = 0x80000000, // Unsafe (C++'s /GS) value type
			FLG_CCTOR = (CORINFO_FLG_CONSTRUCTOR | CORINFO_FLG_STATIC)
		};

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorInfoFlag getMethodAttribsDel(IntPtr thisPtr, [In] IntPtr methodHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorInfoFlag getMethodAttribsInternalDel(IntPtr thisPtr, [In] IntPtr methodHandle);
	}
	public class CompiledMethodInfo<T>
	{
		public uint CodeSize;
		public uint PrologSize;
		public string ILCode { get; private set; }
		public bool IsBlendedCode
		{
			get
			{
				if (_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_DEBUG_CODE))
				{
					var compCodeOpt = CorJitCompiler.CodeOptimize.BLENDED_CODE;
					// If the EE sets SIZE_OPT or if we are compiling a Class constructor
					// we will optimize for code size at the expense of speed
					//
					if (_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_SIZE_OPT) || ((_compFlags & CorStaticInfo.CorInfoFlag.FLG_CCTOR) == CorStaticInfo.CorInfoFlag.FLG_CCTOR))
					{
						compCodeOpt = CorJitCompiler.CodeOptimize.SMALL_CODE;
					}
					//
					// If the EE sets SPEED_OPT we will optimize for speed at the expense of code size
					//
					else if (_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_SPEED_OPT) ||
							 (_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_TIER1) && !_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_MIN_OPT)))
					{
						compCodeOpt = CorJitCompiler.CodeOptimize.FAST_CODE;
						if (_corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_SIZE_OPT))
							throw new Exception("Seems CorJitFlags corrupted");
					}
					return compCodeOpt == CorJitCompiler.CodeOptimize.BLENDED_CODE;
				}
				//TODO: track https://github.com/dotnet/coreclr/blob/dbd533372e41b029398839056450c0fcac2b91f0/src/jit/compiler.h#L8533
				return true;
			}
		}
		public bool IsOptimizedCode => _corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_SIZE_OPT) || _corJitFlags.IsSet(CorJitFlags.CorJitFlag.CORJIT_FLAG_SPEED_OPT);
		public bool IsRbpBasedFrame { get; private set; }
		public bool IsPartiallyInterruptible { get; private set; }
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
		private CorJitFlags.CorJitFlag    _corJitFlag;
		private CorStaticInfo.CorInfoFlag _compFlags;

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
				| System.Reflection.BindingFlags.Public).MethodHandle, new[] { typeof(T).TypeHandle });
			RuntimeHelpers.PrepareMethod(GetType().GetMethod("NativeDump", System.Reflection.BindingFlags.Instance
				| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).MethodHandle, new[] { typeof(T).TypeHandle });

			RuntimeHelpers.PrepareMethod(typeof(CorJitCompiler).GetMethod("DumpMethodInfo",
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).MethodHandle, null);
			hook = new CompilerHook();
			hook.Hook(CompileMethodDel);
		}

		public void Release() => hook.RemoveHook();
		public static unsafe void NativeDump(IntPtr corJitInfoPtr, CorInfo* methodInfo,
			CorJitFlags.CorJitFlag flags, IntPtr nativeEntry, IntPtr nativeSizeOfCode) => CorJitCompiler.DumpMethodInfo(corJitInfoPtr, methodInfo, flags, nativeEntry, nativeSizeOfCode);

		internal static unsafe CorJitCompiler.CorJitResult CompileMethodDel(IntPtr thisPtr, [In] IntPtr corJitInfoPtr, [In] CorInfo* methodInfo,
			CorJitFlags.CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode)
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
		[Flags]
		public enum CorJitFlag : UInt32
		{
			CORJIT_FLAG_CALL_GETJITFLAGS = 0xffffffff, // Indicates that the JIT should retrieve flags in the form of a

			// pointer to a CORJIT_FLAGS value via ICorJitInfo::getJitFlags().
			CORJIT_FLAG_SPEED_OPT        = 0,
			CORJIT_FLAG_SIZE_OPT         = 1,
			CORJIT_FLAG_DEBUG_CODE       = 2, // generate "debuggable" code (no code-mangling optimizations)
			CORJIT_FLAG_DEBUG_EnC        = 3, // We are in Edit-n-Continue mode
			CORJIT_FLAG_DEBUG_INFO       = 4, // generate line and local-var info
			CORJIT_FLAG_MIN_OPT          = 5, // disable all jit optimizations (not necesarily debuggable code)
			CORJIT_FLAG_GCPOLL_CALLS     = 6, // Emit calls to JIT_POLLGC for thread suspension.
			CORJIT_FLAG_MCJIT_BACKGROUND = 7, // Calling from multicore JIT background thread, do not call JitComplete

#if (_TARGET_X64_)
		CORJIT_FLAG_PINVOKE_RESTORE_ESP = 8, // Restore ESP after returning from inlined PInvoke
		CORJIT_FLAG_TARGET_P4 = 9,
		CORJIT_FLAG_USE_FCOMI = 10, // Generated code may use fcomi(p) instruction
		CORJIT_FLAG_USE_CMOV = 11, // Generated code may use cmov instruction
		CORJIT_FLAG_USE_SSE2 = 12, // Generated code may use SSE-2 instructions

#elif (_TARGET_X86_)
			CORJIT_FLAG_UNUSED1 = 8,
			CORJIT_FLAG_UNUSED2 = 9,
			CORJIT_FLAG_UNUSED3 = 10,
			CORJIT_FLAG_UNUSED4 = 11,
			CORJIT_FLAG_UNUSED5 = 12,

#else
			CORJIT_FLAG_UNUSED6 = 13,
#endif

#if (_TARGET_X86_ || _TARGET_AMD64_) //_TARGET_AMD64_ not implemented
		CORJIT_FLAG_USE_AVX = 14,
		CORJIT_FLAG_USE_AVX2 = 15,
		CORJIT_FLAG_USE_AVX_512 = 16,

#else // !defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

			CORJIT_FLAG_UNUSED7 = 14,
			CORJIT_FLAG_UNUSED8 = 15,
			CORJIT_FLAG_UNUSED9 = 16,

#endif // !defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

#if (_TARGET_X86_ || _TARGET_AMD64_ || _TARGET_ARM64_) // _TARGET_AMD64_ and _TARGET_ARM64_ not implemented
		CORJIT_FLAG_FEATURE_SIMD = 17,
#else
			CORJIT_FLAG_UNUSED10 = 17,
#endif // !(defined(_TARGET_X86_) || defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_))

			CORJIT_FLAG_MAKEFINALCODE          = 18, // Use the final code generator, i.e., not the interpreter.
			CORJIT_FLAG_READYTORUN             = 19, // Use version-resilient code generation
			CORJIT_FLAG_PROF_ENTERLEAVE        = 20, // Instrument prologues/epilogues
			CORJIT_FLAG_PROF_REJIT_NOPS        = 21, // Insert NOPs to ensure code is re-jitable
			CORJIT_FLAG_PROF_NO_PINVOKE_INLINE = 22, // Disables PInvoke inlining

			CORJIT_FLAG_SKIP_VERIFICATION =
				23, // (lazy) skip verification - determined without doing a full resolve. See comment below
			CORJIT_FLAG_PREJIT               = 24, // jit or prejit is the execution engine.
			CORJIT_FLAG_RELOC                = 25, // Generate relocatable code
			CORJIT_FLAG_IMPORT_ONLY          = 26, // Only import the function
			CORJIT_FLAG_IL_STUB              = 27, // method is an IL stub
			CORJIT_FLAG_PROCSPLIT            = 28, // JIT should separate code into hot and cold sections
			CORJIT_FLAG_BBINSTR              = 29, // Collect basic block profile information
			CORJIT_FLAG_BBOPT                = 30, // Optimize method based on profile information
			CORJIT_FLAG_FRAMED               = 31, // All methods have an EBP frame
			CORJIT_FLAG_ALIGN_LOOPS          = 32, // add NOPs before loops to align them at 16 byte boundaries
			CORJIT_FLAG_PUBLISH_SECRET_PARAM = 33, // JIT must place stub secret param into local 0.  (used by IL stubs)
			CORJIT_FLAG_GCPOLL_INLINE        = 34, // JIT must inline calls to GCPoll when possible

			CORJIT_FLAG_SAMPLING_JIT_BACKGROUND =
				35, // JIT is being invoked as a result of stack sampling for hot methods in the background

			CORJIT_FLAG_USE_PINVOKE_HELPERS =
				36, // The JIT should use the PINVOKE_{BEGIN,END} helpers instead of emitting inline transitions

			CORJIT_FLAG_REVERSE_PINVOKE =
				37, // The JIT should insert REVERSE_PINVOKE_{ENTER,EXIT} helpers into method prolog/epilog
			CORJIT_FLAG_DESKTOP_QUIRKS = 38, // The JIT should generate desktop-quirk-compatible code

			CORJIT_FLAG_TIER0 =
				39, // This is the initial tier for tiered compilation which should generate code as quickly as possible

			CORJIT_FLAG_TIER1 =
				40, // This is the final tier (for now) for tiered compilation which should generate high quality code

#if _TARGET_ARM_ //not implemented
		CORJIT_FLAG_RELATIVE_CODE_RELOCS =
 41, // JIT should generate PC-relative address computations instead of EE relocation records
#else // !defined(_TARGET_ARM_)
			CORJIT_FLAG_UNUSED11 = 41,
#endif // !defined(_TARGET_ARM_)

			CORJIT_FLAG_NO_INLINING = 42, // JIT should not inline any called method into this method

#if _TARGET_ARM64_ //not implemented
		CORJIT_FLAG_HAS_ARM64_AES = 43, // ID_AA64ISAR0_EL1.AES is 1 or better
		CORJIT_FLAG_HAS_ARM64_ATOMICS = 44, // ID_AA64ISAR0_EL1.Atomic is 2 or better
		CORJIT_FLAG_HAS_ARM64_CRC32 = 45, // ID_AA64ISAR0_EL1.CRC32 is 1 or better
		CORJIT_FLAG_HAS_ARM64_DCPOP = 46, // ID_AA64ISAR1_EL1.DPB is 1 or better
		CORJIT_FLAG_HAS_ARM64_DP = 47, // ID_AA64ISAR0_EL1.DP is 1 or better
		CORJIT_FLAG_HAS_ARM64_FCMA = 48, // ID_AA64ISAR1_EL1.FCMA is 1 or better
		CORJIT_FLAG_HAS_ARM64_FP = 49, // ID_AA64PFR0_EL1.FP is 0 or better
		CORJIT_FLAG_HAS_ARM64_FP16 = 50, // ID_AA64PFR0_EL1.FP is 1 or better
		CORJIT_FLAG_HAS_ARM64_JSCVT = 51, // ID_AA64ISAR1_EL1.JSCVT is 1 or better
		CORJIT_FLAG_HAS_ARM64_LRCPC = 52, // ID_AA64ISAR1_EL1.LRCPC is 1 or better
		CORJIT_FLAG_HAS_ARM64_PMULL = 53, // ID_AA64ISAR0_EL1.AES is 2 or better
		CORJIT_FLAG_HAS_ARM64_SHA1 = 54, // ID_AA64ISAR0_EL1.SHA1 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SHA2 = 55, // ID_AA64ISAR0_EL1.SHA2 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SHA512 = 56, // ID_AA64ISAR0_EL1.SHA2 is 2 or better
		CORJIT_FLAG_HAS_ARM64_SHA3 = 57, // ID_AA64ISAR0_EL1.SHA3 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SIMD = 58, // ID_AA64PFR0_EL1.AdvSIMD is 0 or better
		CORJIT_FLAG_HAS_ARM64_SIMD_V81 = 59, // ID_AA64ISAR0_EL1.RDM is 1 or better
		CORJIT_FLAG_HAS_ARM64_SIMD_FP16 = 60, // ID_AA64PFR0_EL1.AdvSIMD is 1 or better
		CORJIT_FLAG_HAS_ARM64_SM3 = 61, // ID_AA64ISAR0_EL1.SM3 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SM4 = 62, // ID_AA64ISAR0_EL1.SM4 is 1 or better
		CORJIT_FLAG_HAS_ARM64_SVE = 63  // ID_AA64PFR0_EL1.SVE is 1 or better

#elif (_TARGET_X86_ || _TARGET_AMD64_) //_TARGET_AMD64_ not implemented
		CORJIT_FLAG_USE_SSE3 = 43,
		CORJIT_FLAG_USE_SSSE3 = 44,
		CORJIT_FLAG_USE_SSE41 = 45,
		CORJIT_FLAG_USE_SSE42 = 46,
		CORJIT_FLAG_USE_AES = 47,
		CORJIT_FLAG_USE_BMI1 = 48,
		CORJIT_FLAG_USE_BMI2 = 49,
		CORJIT_FLAG_USE_FMA = 50,
		CORJIT_FLAG_USE_LZCNT = 51,
		CORJIT_FLAG_USE_PCLMULQDQ = 52,
		CORJIT_FLAG_USE_POPCNT = 53,
		CORJIT_FLAG_UNUSED23 = 54,
		CORJIT_FLAG_UNUSED24 = 55,
		CORJIT_FLAG_UNUSED25 = 56,
		CORJIT_FLAG_UNUSED26 = 57,
		CORJIT_FLAG_UNUSED27 = 58,
		CORJIT_FLAG_UNUSED28 = 59,
		CORJIT_FLAG_UNUSED29 = 60,
		CORJIT_FLAG_UNUSED30 = 61,
		CORJIT_FLAG_UNUSED31 = 62,
		CORJIT_FLAG_UNUSED32 = 63


#else // !defined(_TARGET_ARM64_) &&!defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)

			CORJIT_FLAG_UNUSED12 = 43,
			CORJIT_FLAG_UNUSED13 = 44,
			CORJIT_FLAG_UNUSED14 = 45,
			CORJIT_FLAG_UNUSED15 = 46,
			CORJIT_FLAG_UNUSED16 = 47,
			CORJIT_FLAG_UNUSED17 = 48,
			CORJIT_FLAG_UNUSED18 = 49,
			CORJIT_FLAG_UNUSED19 = 50,
			CORJIT_FLAG_UNUSED20 = 51,
			CORJIT_FLAG_UNUSED21 = 52,
			CORJIT_FLAG_UNUSED22 = 53,
			CORJIT_FLAG_UNUSED23 = 54,
			CORJIT_FLAG_UNUSED24 = 55,
			CORJIT_FLAG_UNUSED25 = 56,
			CORJIT_FLAG_UNUSED26 = 57,
			CORJIT_FLAG_UNUSED27 = 58,
			CORJIT_FLAG_UNUSED28 = 59,
			CORJIT_FLAG_UNUSED29 = 60,
			CORJIT_FLAG_UNUSED30 = 61,
			CORJIT_FLAG_UNUSED31 = 62,
			CORJIT_FLAG_UNUSED32 = 63

#endif // !defined(_TARGET_ARM64_) &&!defined(_TARGET_X86_) && !defined(_TARGET_AMD64_)
		};

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

	public enum CorInfoOptions : UInt32
	{
		CORINFO_OPT_INIT_LOCALS = 0x00000010, // zero initialize all variables

		CORINFO_GENERICS_CTXT_FROM_THIS =
			0x00000020, // is this shared generic code that access the generic context from the this pointer?  If so, then if the method has SEH then the 'this' pointer must always be reported and kept alive.

		CORINFO_GENERICS_CTXT_FROM_METHODDESC =
			0x00000040, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodDesc)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_FROM_METHODTABLE =
			0x00000080, // is this shared generic code that access the generic context from the ParamTypeArg(that is a MethodTable)?  If so, then if the method has SEH then the 'ParamTypeArg' must always be reported and kept alive. Same as CORINFO_CALLCONV_PARAMTYPE

		CORINFO_GENERICS_CTXT_MASK = (CORINFO_GENERICS_CTXT_FROM_THIS       |
									  CORINFO_GENERICS_CTXT_FROM_METHODDESC |
									  CORINFO_GENERICS_CTXT_FROM_METHODTABLE),

		CORINFO_GENERICS_CTXT_KEEP_ALIVE =
			0x00000100, // Keep the generics context alive throughout the method even if there is no explicit use, and report its location to the CLR
	};

	public enum CorInfoRegionKind : UInt32
	{
		CORINFO_REGION_NONE,
		CORINFO_REGION_HOT,
		CORINFO_REGION_COLD,
		CORINFO_REGION_JIT,
	};

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

	public enum CorInfoCallConv
	{
		// These correspond to CorCallingConvention

		CORINFO_CALLCONV_DEFAULT      = 0x0,
		CORINFO_CALLCONV_C            = 0x1,
		CORINFO_CALLCONV_STDCALL      = 0x2,
		CORINFO_CALLCONV_THISCALL     = 0x3,
		CORINFO_CALLCONV_FASTCALL     = 0x4,
		CORINFO_CALLCONV_VARARG       = 0x5,
		CORINFO_CALLCONV_FIELD        = 0x6,
		CORINFO_CALLCONV_LOCAL_SIG    = 0x7,
		CORINFO_CALLCONV_PROPERTY     = 0x8,
		CORINFO_CALLCONV_NATIVEVARARG = 0xb, // used ONLY for IL stub PInvoke vararg calls

		CORINFO_CALLCONV_MASK         = 0x0f, // Calling convention is bottom 4 bits
		CORINFO_CALLCONV_GENERIC      = 0x10,
		CORINFO_CALLCONV_HASTHIS      = 0x20,
		CORINFO_CALLCONV_EXPLICITTHIS = 0x40,
		CORINFO_CALLCONV_PARAMTYPE    = 0x80, // Passed last. Same as CORINFO_GENERICS_CTXT_FROM_PARAMTYPEARG
	};

	// The enumeration is returned in 'getSig','getType', getArgType methods
	public enum CorInfoType
	{
		CORINFO_TYPE_UNDEF      = 0x0,
		CORINFO_TYPE_VOID       = 0x1,
		CORINFO_TYPE_BOOL       = 0x2,
		CORINFO_TYPE_CHAR       = 0x3,
		CORINFO_TYPE_BYTE       = 0x4,
		CORINFO_TYPE_UBYTE      = 0x5,
		CORINFO_TYPE_SHORT      = 0x6,
		CORINFO_TYPE_USHORT     = 0x7,
		CORINFO_TYPE_INT        = 0x8,
		CORINFO_TYPE_UINT       = 0x9,
		CORINFO_TYPE_LONG       = 0xa,
		CORINFO_TYPE_ULONG      = 0xb,
		CORINFO_TYPE_NATIVEINT  = 0xc,
		CORINFO_TYPE_NATIVEUINT = 0xd,
		CORINFO_TYPE_FLOAT      = 0xe,
		CORINFO_TYPE_DOUBLE     = 0xf,
		CORINFO_TYPE_STRING     = 0x10, // Not used, should remove
		CORINFO_TYPE_PTR        = 0x11,
		CORINFO_TYPE_BYREF      = 0x12,
		CORINFO_TYPE_VALUECLASS = 0x13,
		CORINFO_TYPE_CLASS      = 0x14,
		CORINFO_TYPE_REFANY     = 0x15,

		// CORINFO_TYPE_VAR is for a generic type variable.
		// Generic type variables only appear when the JIT is doing
		// verification (not NOT compilation) of generic code
		// for the EE, in which case we're running
		// the JIT in "import only" mode.

		CORINFO_TYPE_VAR = 0x16,
		CORINFO_TYPE_COUNT, // number of jit types
	};

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

	public enum CorInfoTokenKind
	{
		CORINFO_TOKENKIND_Class  = 0x01,
		CORINFO_TOKENKIND_Method = 0x02,
		CORINFO_TOKENKIND_Field  = 0x04,
		CORINFO_TOKENKIND_Mask   = 0x07,

		// token comes from CEE_LDTOKEN
		CORINFO_TOKENKIND_Ldtoken = 0x10 | CORINFO_TOKENKIND_Class | CORINFO_TOKENKIND_Method | CORINFO_TOKENKIND_Field,

		// token comes from CEE_CASTCLASS or CEE_ISINST
		CORINFO_TOKENKIND_Casting = 0x20 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWARR
		CORINFO_TOKENKIND_Newarr = 0x40 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_BOX
		CORINFO_TOKENKIND_Box = 0x80 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_CONSTRAINED
		CORINFO_TOKENKIND_Constrained = 0x100 | CORINFO_TOKENKIND_Class,

		// token comes from CEE_NEWOBJ
		CORINFO_TOKENKIND_NewObj = 0x200 | CORINFO_TOKENKIND_Method,

		// token comes from CEE_LDVIRTFTN
		CORINFO_TOKENKIND_Ldvirtftn = 0x400 | CORINFO_TOKENKIND_Method,
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

	public enum CorInfoHelpFunc
	{
		CORINFO_HELP_UNDEF, // invalid value. This should never be used

		/* Arithmetic helpers */

		CORINFO_HELP_DIV, // For the ARM 32-bit integer divide uses a helper call :-(
		CORINFO_HELP_MOD,
		CORINFO_HELP_UDIV,
		CORINFO_HELP_UMOD,

		CORINFO_HELP_LLSH,
		CORINFO_HELP_LRSH,
		CORINFO_HELP_LRSZ,
		CORINFO_HELP_LMUL,
		CORINFO_HELP_LMUL_OVF,
		CORINFO_HELP_ULMUL_OVF,
		CORINFO_HELP_LDIV,
		CORINFO_HELP_LMOD,
		CORINFO_HELP_ULDIV,
		CORINFO_HELP_ULMOD,
		CORINFO_HELP_LNG2DBL,  // Convert a signed int64 to a double
		CORINFO_HELP_ULNG2DBL, // Convert a unsigned int64 to a double
		CORINFO_HELP_DBL2INT,
		CORINFO_HELP_DBL2INT_OVF,
		CORINFO_HELP_DBL2LNG,
		CORINFO_HELP_DBL2LNG_OVF,
		CORINFO_HELP_DBL2UINT,
		CORINFO_HELP_DBL2UINT_OVF,
		CORINFO_HELP_DBL2ULNG,
		CORINFO_HELP_DBL2ULNG_OVF,
		CORINFO_HELP_FLTREM,
		CORINFO_HELP_DBLREM,
		CORINFO_HELP_FLTROUND,
		CORINFO_HELP_DBLROUND,

		/* Allocating a new object. Always use ICorClassInfo::getNewHelper() to decide 
		   which is the right helper to use to allocate an object of a given type. */

		CORINFO_HELP_NEW_CROSSCONTEXT, // cross context new object
		CORINFO_HELP_NEWFAST,
		CORINFO_HELP_NEWSFAST, // allocator for small, non-finalizer, non-array object
		CORINFO_HELP_NEWSFAST_ALIGN8, // allocator for small, non-finalizer, non-array object, 8 byte aligned
		CORINFO_HELP_NEW_MDARR, // multi-dim array helper (with or without lower bounds - dimensions passed in as vararg)
		CORINFO_HELP_NEW_MDARR_NONVARARG, // multi-dim array helper (with or without lower bounds - dimensions passed in as unmanaged array)
		CORINFO_HELP_NEWARR_1_DIRECT, // helper for any one dimensional array creation
		CORINFO_HELP_NEWARR_1_R2R_DIRECT, // wrapper for R2R direct call, which extracts method table from ArrayTypeDesc
		CORINFO_HELP_NEWARR_1_OBJ, // optimized 1-D object arrays
		CORINFO_HELP_NEWARR_1_VC, // optimized 1-D value class arrays
		CORINFO_HELP_NEWARR_1_ALIGN8, // like VC, but aligns the array start

		CORINFO_HELP_STRCNS,                // create a new string literal
		CORINFO_HELP_STRCNS_CURRENT_MODULE, // create a new string literal from the current module (used by NGen code)

		/* Object model */

		CORINFO_HELP_INITCLASS,     // Initialize class if not already initialized
		CORINFO_HELP_INITINSTCLASS, // Initialize class for instantiated type

		// Use ICorClassInfo::getCastingHelper to determine
		// the right helper to use

		CORINFO_HELP_ISINSTANCEOFINTERFACE, // Optimized helper for interfaces
		CORINFO_HELP_ISINSTANCEOFARRAY,     // Optimized helper for arrays
		CORINFO_HELP_ISINSTANCEOFCLASS,     // Optimized helper for classes
		CORINFO_HELP_ISINSTANCEOFANY,       // Slow helper for any type

		CORINFO_HELP_CHKCASTINTERFACE,
		CORINFO_HELP_CHKCASTARRAY,
		CORINFO_HELP_CHKCASTCLASS,
		CORINFO_HELP_CHKCASTANY,
		CORINFO_HELP_CHKCASTCLASS_SPECIAL, // Optimized helper for classes. Assumes that the trivial cases 
		// has been taken care of by the inlined check

		CORINFO_HELP_BOX,
		CORINFO_HELP_BOX_NULLABLE, // special form of boxing for Nullable<T>
		CORINFO_HELP_UNBOX,
		CORINFO_HELP_UNBOX_NULLABLE, // special form of unboxing for Nullable<T>
		CORINFO_HELP_GETREFANY,      // Extract the byref from a TypedReference, checking that it is the expected type

		CORINFO_HELP_ARRADDR_ST,  // assign to element of object array with type-checking
		CORINFO_HELP_LDELEMA_REF, // does a precise type comparision and returns address

		/* Exceptions */

		CORINFO_HELP_THROW,           // Throw an exception object
		CORINFO_HELP_RETHROW,         // Rethrow the currently active exception
		CORINFO_HELP_USER_BREAKPOINT, // For a user program to break to the debugger
		CORINFO_HELP_RNGCHKFAIL,      // array bounds check failed
		CORINFO_HELP_OVERFLOW,        // throw an overflow exception
		CORINFO_HELP_THROWDIVZERO,    // throw a divide by zero exception
		CORINFO_HELP_THROWNULLREF,    // throw a null reference exception

		CORINFO_HELP_INTERNALTHROW, // Support for really fast jit
		CORINFO_HELP_VERIFICATION, // Throw a VerificationException
		CORINFO_HELP_SEC_UNMGDCODE_EXCPT, // throw a security unmanaged code exception
		CORINFO_HELP_FAIL_FAST, // Kill the process avoiding any exceptions or stack and data dependencies (use for GuardStack unsafe buffer checks)

		CORINFO_HELP_METHOD_ACCESS_EXCEPTION, //Throw an access exception due to a failed member/class access check.
		CORINFO_HELP_FIELD_ACCESS_EXCEPTION,
		CORINFO_HELP_CLASS_ACCESS_EXCEPTION,

		CORINFO_HELP_ENDCATCH, // call back into the EE at the end of a catch block

		/* Synchronization */

		CORINFO_HELP_MON_ENTER,
		CORINFO_HELP_MON_EXIT,
		CORINFO_HELP_MON_ENTER_STATIC,
		CORINFO_HELP_MON_EXIT_STATIC,

		CORINFO_HELP_GETCLASSFROMMETHODPARAM, // Given a generics method handle, returns a class handle
		CORINFO_HELP_GETSYNCFROMCLASSHANDLE,  // Given a generics class handle, returns the sync monitor 
		// in its ManagedClassObject

		/* Security callout support */

		CORINFO_HELP_SECURITY_PROLOG, // Required if CORINFO_FLG_SECURITYCHECK is set, or CORINFO_FLG_NOSECURITYWRAP is not set
		CORINFO_HELP_SECURITY_PROLOG_FRAMED, // Slow version of CORINFO_HELP_SECURITY_PROLOG. Used for instrumentation.

		CORINFO_HELP_METHOD_ACCESS_CHECK, // Callouts to runtime security access checks
		CORINFO_HELP_FIELD_ACCESS_CHECK,
		CORINFO_HELP_CLASS_ACCESS_CHECK,

		CORINFO_HELP_DELEGATE_SECURITY_CHECK, // Callout to delegate security transparency check

		/* Verification runtime callout support */

		CORINFO_HELP_VERIFICATION_RUNTIME_CHECK, // Do a Demand for UnmanagedCode permission at runtime

		/* GC support */

		CORINFO_HELP_STOP_FOR_GC, // Call GC (force a GC)
		CORINFO_HELP_POLL_GC,     // Ask GC if it wants to collect

		CORINFO_HELP_STRESS_GC, // Force a GC, but then update the JITTED code to be a noop call
		CORINFO_HELP_CHECK_OBJ, // confirm that ECX is a valid object pointer (debugging only)

		/* GC Write barrier support */

		CORINFO_HELP_ASSIGN_REF, // universal helpers with F_CALL_CONV calling convention
		CORINFO_HELP_CHECKED_ASSIGN_REF,
		CORINFO_HELP_ASSIGN_REF_ENSURE_NONHEAP, // Do the store, and ensure that the target was not in the heap.

		CORINFO_HELP_ASSIGN_BYREF,
		CORINFO_HELP_ASSIGN_STRUCT,


		/* Accessing fields */

		// For COM object support (using COM get/set routines to update object)
		// and EnC and cross-context support
		CORINFO_HELP_GETFIELD8,
		CORINFO_HELP_SETFIELD8,
		CORINFO_HELP_GETFIELD16,
		CORINFO_HELP_SETFIELD16,
		CORINFO_HELP_GETFIELD32,
		CORINFO_HELP_SETFIELD32,
		CORINFO_HELP_GETFIELD64,
		CORINFO_HELP_SETFIELD64,
		CORINFO_HELP_GETFIELDOBJ,
		CORINFO_HELP_SETFIELDOBJ,
		CORINFO_HELP_GETFIELDSTRUCT,
		CORINFO_HELP_SETFIELDSTRUCT,
		CORINFO_HELP_GETFIELDFLOAT,
		CORINFO_HELP_SETFIELDFLOAT,
		CORINFO_HELP_GETFIELDDOUBLE,
		CORINFO_HELP_SETFIELDDOUBLE,

		CORINFO_HELP_GETFIELDADDR,

		CORINFO_HELP_GETSTATICFIELDADDR_CONTEXT, // Helper for context-static fields
		CORINFO_HELP_GETSTATICFIELDADDR_TLS,     // Helper for PE TLS fields

		// There are a variety of specialized helpers for accessing static fields. The JIT should use 
		// ICorClassInfo::getSharedStaticsOrCCtorHelper to determine which helper to use

		// Helpers for regular statics
		CORINFO_HELP_GETGENERICS_GCSTATIC_BASE,
		CORINFO_HELP_GETGENERICS_NONGCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_GCSTATIC_BASE_DYNAMICCLASS,
		CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS,

		// Helper to class initialize shared generic with dynamicclass, but not get static field address
		CORINFO_HELP_CLASSINIT_SHARED_DYNAMICCLASS,

		// Helpers for thread statics
		CORINFO_HELP_GETGENERICS_GCTHREADSTATIC_BASE,
		CORINFO_HELP_GETGENERICS_NONGCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR,
		CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS,
		CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS,

		/* Debugger */

		CORINFO_HELP_DBG_IS_JUST_MY_CODE, // Check if this is "JustMyCode" and needs to be stepped through.

		/* Profiling enter/leave probe addresses */
		CORINFO_HELP_PROF_FCN_ENTER,    // record the entry to a method (caller)
		CORINFO_HELP_PROF_FCN_LEAVE,    // record the completion of current method (caller)
		CORINFO_HELP_PROF_FCN_TAILCALL, // record the completionof current method through tailcall (caller)

		/* Miscellaneous */

		CORINFO_HELP_BBT_FCN_ENTER, // record the entry to a method for collecting Tuning data

		CORINFO_HELP_PINVOKE_CALLI, // Indirect pinvoke call
		CORINFO_HELP_TAILCALL,      // Perform a tail call

		CORINFO_HELP_GETCURRENTMANAGEDTHREADID,

		CORINFO_HELP_INIT_PINVOKE_FRAME, // initialize an inlined PInvoke Frame for the JIT-compiler

		CORINFO_HELP_MEMSET, // Init block of memory
		CORINFO_HELP_MEMCPY, // Copy block of memory

		CORINFO_HELP_RUNTIMEHANDLE_METHOD,     // determine a type/field/method handle at run-time
		CORINFO_HELP_RUNTIMEHANDLE_METHOD_LOG, // determine a type/field/method handle at run-time, with IBC logging
		CORINFO_HELP_RUNTIMEHANDLE_CLASS,      // determine a type/field/method handle at run-time
		CORINFO_HELP_RUNTIMEHANDLE_CLASS_LOG,  // determine a type/field/method handle at run-time, with IBC logging

		// These helpers are required for MDIL backward compatibility only. They are not used by current JITed code.
		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPEHANDLE_OBSOLETE, // Convert from a TypeHandle (native structure pointer) to RuntimeTypeHandle at run-time
		CORINFO_HELP_METHODDESC_TO_RUNTIMEMETHODHANDLE_OBSOLETE, // Convert from a MethodDesc (native structure pointer) to RuntimeMethodHandle at run-time
		CORINFO_HELP_FIELDDESC_TO_RUNTIMEFIELDHANDLE_OBSOLETE, // Convert from a FieldDesc (native structure pointer) to RuntimeFieldHandle at run-time

		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE, // Convert from a TypeHandle (native structure pointer) to RuntimeType at run-time
		CORINFO_HELP_TYPEHANDLE_TO_RUNTIMETYPE_MAYBENULL, // Convert from a TypeHandle (native structure pointer) to RuntimeType at run-time, the type may be null
		CORINFO_HELP_METHODDESC_TO_STUBRUNTIMEMETHOD, // Convert from a MethodDesc (native structure pointer) to RuntimeMethodHandle at run-time
		CORINFO_HELP_FIELDDESC_TO_STUBRUNTIMEFIELD, // Convert from a FieldDesc (native structure pointer) to RuntimeFieldHandle at run-time

		CORINFO_HELP_VIRTUAL_FUNC_PTR, // look up a virtual method at run-time
		//CORINFO_HELP_VIRTUAL_FUNC_PTR_LOG,  // look up a virtual method at run-time, with IBC logging

		// Not a real helpers. Instead of taking handle arguments, these helpers point to a small stub that loads the handle argument and calls the static helper.
		CORINFO_HELP_READYTORUN_NEW,
		CORINFO_HELP_READYTORUN_NEWARR_1,
		CORINFO_HELP_READYTORUN_ISINSTANCEOF,
		CORINFO_HELP_READYTORUN_CHKCAST,
		CORINFO_HELP_READYTORUN_STATIC_BASE,
		CORINFO_HELP_READYTORUN_VIRTUAL_FUNC_PTR,
		CORINFO_HELP_READYTORUN_GENERIC_HANDLE,
		CORINFO_HELP_READYTORUN_DELEGATE_CTOR,
		CORINFO_HELP_READYTORUN_GENERIC_STATIC_BASE,

		CORINFO_HELP_EE_PRESTUB, // Not real JIT helper. Used in native images.

		CORINFO_HELP_EE_PRECODE_FIXUP, // Not real JIT helper. Used for Precode fixup in native images.
		CORINFO_HELP_EE_PINVOKE_FIXUP, // Not real JIT helper. Used for PInvoke target fixup in native images.
		CORINFO_HELP_EE_VSD_FIXUP, // Not real JIT helper. Used for VSD cell fixup in native images.
		CORINFO_HELP_EE_EXTERNAL_FIXUP, // Not real JIT helper. Used for to fixup external method thunks in native images.
		CORINFO_HELP_EE_VTABLE_FIXUP, // Not real JIT helper. Used for inherited vtable slot fixup in native images.

		CORINFO_HELP_EE_REMOTING_THUNK, // Not real JIT helper. Used for remoting precode in native images.

		CORINFO_HELP_EE_PERSONALITY_ROUTINE, // Not real JIT helper. Used in native images.
		CORINFO_HELP_EE_PERSONALITY_ROUTINE_FILTER_FUNCLET, // Not real JIT helper. Used in native images to detect filter funclets.

		// ASSIGN_REF_EAX - CHECKED_ASSIGN_REF_EBP: NOGC_WRITE_BARRIERS JIT helper calls
		//
		// For unchecked versions EDX is required to point into GC heap.
		//
		// NOTE: these helpers are only used for x86.
		CORINFO_HELP_ASSIGN_REF_EAX, // EAX holds GC ptr, do a 'mov [EDX], EAX' and inform GC
		CORINFO_HELP_ASSIGN_REF_EBX, // EBX holds GC ptr, do a 'mov [EDX], EBX' and inform GC
		CORINFO_HELP_ASSIGN_REF_ECX, // ECX holds GC ptr, do a 'mov [EDX], ECX' and inform GC
		CORINFO_HELP_ASSIGN_REF_ESI, // ESI holds GC ptr, do a 'mov [EDX], ESI' and inform GC
		CORINFO_HELP_ASSIGN_REF_EDI, // EDI holds GC ptr, do a 'mov [EDX], EDI' and inform GC
		CORINFO_HELP_ASSIGN_REF_EBP, // EBP holds GC ptr, do a 'mov [EDX], EBP' and inform GC

		CORINFO_HELP_CHECKED_ASSIGN_REF_EAX, // These are the same as ASSIGN_REF above ...
		CORINFO_HELP_CHECKED_ASSIGN_REF_EBX, // ... but also check if EDX points into heap.
		CORINFO_HELP_CHECKED_ASSIGN_REF_ECX,
		CORINFO_HELP_CHECKED_ASSIGN_REF_ESI,
		CORINFO_HELP_CHECKED_ASSIGN_REF_EDI,
		CORINFO_HELP_CHECKED_ASSIGN_REF_EBP,

		CORINFO_HELP_LOOP_CLONE_CHOICE_ADDR, // Return the reference to a counter to decide to take cloned path in debug stress.
		CORINFO_HELP_DEBUG_LOG_LOOP_CLONING, // Print a message that a loop cloning optimization has occurred in debug mode.

		CORINFO_HELP_THROW_ARGUMENTEXCEPTION,           // throw ArgumentException
		CORINFO_HELP_THROW_ARGUMENTOUTOFRANGEEXCEPTION, // throw ArgumentOutOfRangeException
		CORINFO_HELP_THROW_PLATFORM_NOT_SUPPORTED,      // throw PlatformNotSupportedException
		CORINFO_HELP_THROW_TYPE_NOT_SUPPORTED,          // throw TypeNotSupportedException

		CORINFO_HELP_JIT_PINVOKE_BEGIN, // Transition to preemptive mode before a P/Invoke, frame is the first argument
		CORINFO_HELP_JIT_PINVOKE_END,   // Transition to cooperative mode after a P/Invoke, frame is the first argument

		CORINFO_HELP_JIT_REVERSE_PINVOKE_ENTER, // Transition to cooperative mode in reverse P/Invoke prolog, frame is the first argument
		CORINFO_HELP_JIT_REVERSE_PINVOKE_EXIT, // Transition to preemptive mode in reverse P/Invoke epilog, frame is the first argument

		CORINFO_HELP_GVMLOOKUP_FOR_SLOT, // Resolve a generic virtual method target from this pointer and runtime method handle 

		CORINFO_HELP_COUNT,
	}

	public enum CorInfoIsAccessAllowedResult
	{
		CORINFO_ACCESS_ALLOWED       = 0, // Call allowed
		CORINFO_ACCESS_ILLEGAL       = 1, // Call not allowed
		CORINFO_ACCESS_RUNTIME_CHECK = 2, // Ask at runtime whether to allow the call or not
	}

	public enum CorinfoThisTransform
	{
		CORINFO_NO_THIS_TRANSFORM,
		CORINFO_BOX_THIS,
		CORINFO_DEREF_THIS
	}

	public enum CorinfoCallKind
	{
		CORINFO_CALL,
		CORINFO_CALL_CODE_POINTER,
		CORINFO_VIRTUALCALL_STUB,
		CORINFO_VIRTUALCALL_LDVIRTFTN,
		CORINFO_VIRTUALCALL_VTABLE
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

	/// <summary>
	/// corjit.h
	/// </summary>
	/// 
	public unsafe interface ICorJitCompiler
	{
		CorJitCompiler.CorJitResult CompileMethod(IntPtr thisPtr, [In] IntPtr corJitInfo, [In] CorInfo* methodInfo,
			CorJitFlags.CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode);

		void ProcessShutdownWork(IntPtr thisPtr, [In] IntPtr corStaticInfo);
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
				CorJitFlags.CorJitFlag flags, [Out] IntPtr nativeEntry, [Out] IntPtr nativeSizeOfCode)
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
		public static extern void DumpMethodInfo(IntPtr corJitInfo, CorInfo* methodInfo, CorJitFlags.CorJitFlag flags,
			IntPtr nativeEntry, IntPtr nativeSizeOfCode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		public unsafe delegate CorJitResult CompileMethodDel(IntPtr thisPtr, [In] IntPtr corJitInfo,
			[In] CorInfo* methodInfo, CorJitFlags.CorJitFlag flags, [Out] IntPtr nativeEntry,
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