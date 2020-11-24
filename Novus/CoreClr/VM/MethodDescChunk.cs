using System;
using System.Runtime.InteropServices;
using Novus.Interop;
using Novus.Memory;
// ReSharper disable UnusedMember.Global

namespace Novus.CoreClr.VM
{
	[NativeStructure]
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MethodDescChunk
	{
		/// <summary>
		/// <see cref="RelativeFixupPointer{T}"/>
		/// </summary>
		private Pointer<MethodTable> MethodTableStub { get; }

		/// <summary>
		/// <see cref="RelativePointer{T}"/> to <see cref="MethodDescChunk"/>
		/// </summary>
		private Pointer<byte> Next { get; }

		/// <summary>
		/// The size of this chunk minus 1 (in multiples of MethodDesc::ALIGNMENT)
		/// </summary>
		internal byte Size { get; }

		/// <summary>
		/// The number of MethodDescs in this chunk minus 1
		/// </summary>
		internal byte Count { get; }

		internal ChunkFlags FlagsAndTokenRange { get; }

		// Followed by array of method descs...

		internal Pointer<MethodTable> MethodTable
		{
			get
			{
				// for MDC: m_methodTable.GetValue(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_methodTable));

				const int MT_FIELD_OFS = 0;
				return RuntimeInfo.FieldOffset((MethodTable*)MethodTableStub.ToPointer(), MT_FIELD_OFS);
			}
		}

	}

	[Flags]
	public enum ChunkFlags : ushort
	{
		/// <summary>
		///     This must equal METHOD_TOKEN_RANGE_MASK calculated higher in this file.
		///     These are separate to allow the flags space available and used to be obvious here
		///     and for the logic that splits the token to be algorithmically generated based on the #define
		/// </summary>
		
		TokenRangeMask = 0x03FF,

		/// <summary>
		///     Compact temporary entry points
		/// </summary>
		HasCompactEntryPoints = 0x4000,

		/// <summary>
		///     This chunk lives in NGen module
		/// </summary>
		IsZapped = 0x8000
	}
}
