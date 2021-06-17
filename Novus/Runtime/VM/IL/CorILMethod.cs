// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;
using Novus.Imports;

// ReSharper disable ArrangeAccessorOwnerBody

// ReSharper disable UnusedMember.Global
#pragma warning disable 169
#pragma warning disable IDE0044
#pragma warning disable 649

namespace Novus.Runtime.VM.IL
{
	[NativeStructure]
	[StructLayout(LayoutKind.Explicit)]
	public struct CorILMethod
	{
		/*
		 * https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/corhdr.h
		 * https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/corhlpr.cpp
		 * https://github.com/dotnet/runtime/blob/main/src/coreclr/inc/corhlpr.h
		 *
		 * H:\Archives & Backups\Computer Science\Code\Jit
		 *
		 * https://github.com/Decimation/NeoCore/blob/master/NeoCore/CoreClr/VM/Jit/CorMethod.cs
		 */


		[FieldOffset(0)]
		internal CorILMethodFat Fat;

		[FieldOffset(0)]
		internal CorILMethodTiny Tiny;
	}

	[Flags]
	public enum CorILMethodFlags : ushort
	{
		/// <summary>
		/// Call default constructor on all local vars
		/// </summary>
		InitLocals = 0x0010,

		/// <summary>
		/// There is another attribute after this one
		/// </summary>
		MoreSects = 0x0008,

		/// <summary>
		/// Not used/
		/// </summary>
		CompressedIL = 0x0040,

		// Indicates the format for the COR_ILMETHOD header
		FormatShift = 3,

		FormatMask = (1 << FormatShift) - 1,

		/// <summary>
		/// Use this code if the code size is even
		/// </summary>
		TinyFormat = 0x0002,

		SmallFormat = 0x0000,

		FatFormat = 0x0003,

		/// <summary>
		/// Use this code if the code size is odd
		/// </summary>
		TinyFormat1 = 0x0006,
	}
}