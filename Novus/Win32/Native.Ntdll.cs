using System.Runtime.InteropServices;
using Novus.Win32.Structures.Ntdll;
using HANDLE = System.IntPtr;

namespace Novus.Win32;

public static partial class Native
{
	/// <summary>
	/// <c>ZwDuplicateObject</c>
	/// </summary>
	[DllImport(NTDLL_DLL)]
	public static extern unsafe NtStatus NtDuplicateObject(HANDLE sourceProcessHandle, HANDLE sourceHandle,
	                                                       HANDLE targetProcessHandle, HANDLE* targetHandle,
	                                                       ulong desiredAccess, ulong handleAttributes,
	                                                       ulong options);

	/// <summary>Retrieves the specified system information.</summary>
	/// <param name="systemInformationClass">indicate the kind of system information to be retrieved</param>
	/// <param name="systemInformation">a buffer that receives the requested information</param>
	/// <param name="systemInformationLength">The allocation size of the buffer pointed to by Info</param>
	/// <param name="returnLength">If null, ignored.  Otherwise tells you the size of the information returned by the kernel.</param>
	/// <returns>Status Information</returns>
	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtQuerySystemInformation(SystemInformationClass systemInformationClass,
	                                                       IntPtr systemInformation, uint systemInformationLength,
	                                                       out uint returnLength);

	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtQueryObject(IntPtr objectHandle, ObjectInformationClass informationClass,
	                                            IntPtr informationPtr, uint informationLength,
	                                            out uint returnLength);
}

public enum ObjectInformationClass : int
{
	ObjectBasicInformation    = 0,
	ObjectNameInformation     = 1,
	ObjectTypeInformation     = 2,
	ObjectAllTypesInformation = 3,
	ObjectHandleInformation   = 4
}