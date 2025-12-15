// Author: Deci | Project: Novus | Name: MethodTableAuxiliaryData.cs
// Date: 2025/12/14 @ 11:12:53

using System.Runtime.InteropServices;
using Novus.Imports.Attributes;

namespace Novus.Runtime.VM;

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct MethodTableAuxiliaryData
{

	public enum AuxiliaryFlags
	{

		// AS YOU ADD NEW FLAGS PLEASE CONSIDER WHETHER Generics::NewInstantiation NEEDS
		// TO BE UPDATED IN ORDER TO ENSURE THAT METHODTABLES DUPLICATED FOR GENERIC INSTANTIATIONS
		// CARRY THE CORRECT INITIAL FLAGS.

		Initialized = 0x0001,

		HasCheckedCanCompareBitsOrUseFastGetHashCode = 0x0002, // Whether we have checked the overridden Equals or GetHashCode

		CanCompareBitsOrUseFastGetHashCode = 0x0004, // Is any field type or sub field type overridden Equals or GetHashCode
		IsTlsIndexAllocated                = 0x0008,
		HasApproxParent                    = 0x0010,
		MayHaveOpenInterfaceInInterfaceMap = 0x0020,
		IsNotFullyLoaded                   = 0x0040,
		DependenciesLoaded                 = 0x0080, // class and all dependencies loaded up to CLASS_LOADED_BUT_NOT_VERIFIED

		IsInitError = 0x0100,

		IsStaticDataAllocated =
			0x0200, // When this is set, if the class can be marked as initialized without any further code execution it will be.
		HasCheckedStreamOverride = 0x0400,
		StreamOverriddenRead     = 0x0800,
		StreamOverriddenWrite    = 0x1000,
		EnsuredInstanceActive    = 0x2000,

		// unused enum                      = 0x4000,
		// unused enum                      = 0x8000,

	};

	public uint Flags { get; set; }

	public void* LoaderModule { get; set; }

	public void* ExposedClassObject { get; set; }

}