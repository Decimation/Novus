using System.Runtime.InteropServices;
using Novus.Win32.Structures.Ntdll;
using HANDLE = nint;

namespace Novus.Win32;

public static unsafe partial class Native
{

	/// <summary>
	/// <c>ZwDuplicateObject</c>
	/// </summary>
	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtDuplicateObject(nint sourceProcessHandle, nint sourceHandle,
	                                                nint targetProcessHandle, nint* targetHandle,
	                                                ulong desiredAccess, ulong handleAttributes, ulong options);

	/// <summary>Retrieves the specified system information.</summary>
	/// <param name="systemInformationClass">indicate the kind of system information to be retrieved</param>
	/// <param name="systemInformation">a buffer that receives the requested information</param>
	/// <param name="systemInformationLength">The allocation size of the buffer pointed to by Info</param>
	/// <param name="returnLength">If null, ignored.  Otherwise tells you the size of the information returned by the kernel.</param>
	/// <returns>Status Information</returns>
	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtQuerySystemInformation(SystemInformationClass systemInformationClass,
	                                                       nint systemInformation, uint systemInformationLength,
	                                                       out uint returnLength);

	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtQueryObject(nint objectHandle, ObjectInformationClass informationClass,
	                                            nint informationPtr, uint informationLength,
	                                            out uint returnLength);

	[DllImport(NTDLL_DLL)]
	public static extern NtStatus NtQueryInformationProcess(nint processHandle, ProcessInfoClass processInformationClass,
	                                                        void* processInformation, int processInformationLength,
	                                                        out int returnLength);

}

public enum ObjectInformationClass : int
{

	ObjectBasicInformation    = 0,
	ObjectNameInformation     = 1,
	ObjectTypeInformation     = 2,
	ObjectAllTypesInformation = 3,
	ObjectHandleInformation   = 4

}