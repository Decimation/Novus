using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Novus.OS.Win32.Structures;

[StructLayout(LayoutKind.Sequential)]
public struct MemoryBasicInformation
{
	public IntPtr           BaseAddress;
	public IntPtr           AllocationBase;
	public MemoryProtection AllocationProtect;
	public IntPtr           RegionSize;
	public AllocationType   State;
	public MemoryProtection Protect;
	public MemType          Type;


	/// <summary>Experimental</summary>
	public bool IsAccessible
	{
		get
		{
			/*var b = m.State == AllocationType.Commit &&
			        m.Type is MemType.MEM_MAPPED or MemType.MEM_PRIVATE;*/

			return State == AllocationType.Commit && Type is MemType.MEM_MAPPED or MemType.MEM_PRIVATE;
		}
	}

	/// <summary>Experimental</summary>
	public bool IsWritable
	{
		get
		{
			const MemoryProtection mask = MemoryProtection.ExecuteReadWrite | MemoryProtection.ExecuteWriteCopy |
			                              MemoryProtection.ReadWrite | MemoryProtection.WriteCopy;

			return !((Protect & MemoryProtection.GuardOrNoAccess) != 0 || (Protect & mask) == 0);
		}
	}

	/// <summary>Experimental</summary>
	public bool IsReadable
	{
		get
		{
			const MemoryProtection mask = MemoryProtection.ExecuteRead | MemoryProtection.ExecuteReadWrite |
			                              MemoryProtection.ReadOnly | MemoryProtection.ReadWrite;

			return !((Protect & MemoryProtection.GuardOrNoAccess) != 0 || (Protect & mask) == 0);
		}
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{nameof(BaseAddress)}: {BaseAddress:X},\n" +
		       $"{nameof(AllocationBase)}: {AllocationBase:X},\n" +
		       $"{nameof(AllocationProtect)}: {AllocationProtect},\n" +
		       $"{nameof(RegionSize)}: {RegionSize},\n" +
		       $"{nameof(State)}: {State},\n" +
		       $"{nameof(Protect)}: {Protect},\n" +
		       $"{nameof(Type)}: {Type}";
	}
}

public enum MemType : uint
{
	MEM_IMAGE   = 0x1000000,
	MEM_MAPPED  = 0x40000,
	MEM_PRIVATE = 0x20000
}