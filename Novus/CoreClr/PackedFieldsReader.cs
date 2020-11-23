using System;
using Novus.Memory;

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable BuiltInTypeReferenceStyle

namespace Novus.CoreClr
{
	using DWORD = UInt32;

	/// <summary>
	/// Packed DWORD fields
	/// </summary>
	internal readonly struct PackedFieldsReader
	{
		private const int PACKED_FIELDS_RG_LEN = 1;

		private const int MAX_LENGTH_BITS = 5;

		private readonly Pointer<DWORD> m_ptr;

		//		[FieldOffset(0)]
		//		private fixed DWORD m_rgUnpackedFields[EECLASS_LENGTH];
		// 		DWORD   m_rgUnpackedFields[FIELD_COUNT];

		//		[FieldOffset(0)]
		//		private DWORD m_rgPackedFields;
		// 		DWORD   m_rgPackedFields[1];

		private readonly int m_unpackedFieldsLength;

		internal PackedFieldsReader(Pointer<DWORD> p, int l)
		{
			m_ptr = p;
			m_unpackedFieldsLength = l;
		}

		/// <summary>
		/// The first DWORD block of fields in the packed state.
		/// There should only be <see cref="PACKED_FIELDS_RG_LEN"/> elements.
		/// </summary>
		private Pointer<DWORD> PackedFields => m_ptr;

		/// <summary>
		/// The fields in their unpacked state
		/// </summary>
		private Pointer<DWORD> UnpackedFields => m_ptr;



		/// <summary>
		///     Get the value of the given field when the structure is in its unpacked state.
		/// </summary>
		internal DWORD GetUnpackedField(DWORD dwFieldIndex)
		{
			return UnpackedFields[(int)dwFieldIndex];
		}

		internal DWORD GetPackedField(DWORD dwFieldIndex)
		{
			DWORD dwOffset = 0;

			for (DWORD i = 0; i < dwFieldIndex; i++)
			{
				// +1 since size is [1,32] not [0,31]
				dwOffset += MAX_LENGTH_BITS + BitVectorGet(dwOffset, MAX_LENGTH_BITS) + 1;
			}


			// The next kMaxLengthBits bits contain the length in bits of the field we want (-1 due to the way we
			// encode the length).
			DWORD dwFieldLength = BitVectorGet(dwOffset, MAX_LENGTH_BITS) + 1;
			dwOffset += MAX_LENGTH_BITS;

			// Grab the field value.
			DWORD dwReturn = BitVectorGet(dwOffset, dwFieldLength);

			return dwReturn;
		}

		private const int BITS_PER_DWORD = 32;
		private DWORD BitVectorGet(DWORD dwOffset, DWORD dwLength)
		{
			// Calculate the start and end naturally aligned DWORDs from which the value will come.
			DWORD dwStartBlock = dwOffset / BITS_PER_DWORD;
			DWORD dwEndBlock = (dwOffset + dwLength - 1) / BITS_PER_DWORD;

			if (dwStartBlock == dwEndBlock)
			{
				// Easy case: the new value fits entirely within one aligned DWORD. Compute the number of bits
				// we'll need to shift the extracted value (to the right) and a mask of the bits that will be
				// extracted in the destination DWORD.
				DWORD dwValueShift = dwOffset % BITS_PER_DWORD;
				DWORD dwValueMask = ((1U << (int)dwLength) - 1) << (int)dwValueShift;

				// Mask out the bits we want and shift them down into the bottom of the result DWORD.


				return (PackedFields[(int)dwStartBlock] & dwValueMask) >> (int)dwValueShift;
			}

			// Hard case: the return value is split across two DWORDs (two DWORDs is the max as the new value
			// can be at most DWORD-sized itself). For simplicity we'll simply break this into two separate
			// non-spanning gets and stitch the result together from that. We can revisit this in the future
			// if the perf is a problem.
			DWORD dwInitialBits = BITS_PER_DWORD -
								  dwOffset % BITS_PER_DWORD; // Number of bits to get in the first DWORD
			DWORD dwReturn;

			// Get the initial (low-order) bits from the first DWORD.
			dwReturn = BitVectorGet(dwOffset, dwInitialBits);

			// Get the remaining bits from the second DWORD. These bits will need to be shifted to the left
			// (past the bits we've already read) before being OR'd into the result.
			dwReturn |= BitVectorGet(dwOffset + dwInitialBits, dwLength - dwInitialBits)
						<< (int)dwInitialBits;

			return dwReturn;
		}
	}
}
