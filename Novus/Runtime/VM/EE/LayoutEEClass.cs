using System.Runtime.InteropServices;
using Novus.Imports;
using Novus.Imports.Attributes;
using Novus.Win32;

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;

/// <summary>
/// Substructure of <see cref="EEClass"/>
/// </summary>
[NativeStructure]
[StructLayout(LayoutKind.Explicit)]
public unsafe struct LayoutEEClass
{

	public const int OFFSET_LAYOUTINFO       = 0x58;
	public const int OFFSET_NATIVELAYOUTINFO = 0x60;

	static LayoutEEClass()
	{
		if (RuntimeProperties.EEClassSize != OFFSET_LAYOUTINFO) {
			throw new ImportException($"{nameof(RuntimeProperties.EEClassSize)} != {nameof(OFFSET_LAYOUTINFO)}");
		}
	}

	/*
 *
 *class __base(EEClass, 0) LayoutEEClass
   {
	   __inherited struct GuidInfo* EEClass::m_pGuidInfo;
	   __inherited class EEClassOptionalFields* EEClass::
		   m_rpOptionalFields;
	   __inherited class MethodTable* EEClass::m_pMethodTable;
	   __inherited class FieldDesc* EEClass::m_pFieldDescList;
	   __inherited class MethodDescChunk* EEClass::m_pChunks;
	   __inherited union
	   {
		   struct OBJECTHANDLE__* m_ohDelegate;
		   enum CorIfaceAttr m_ComInterfaceType;
	   } __inner5;
	   __inherited class ComCallWrapperTemplate* EEClass::m_pccwTemplate;
	   __inherited uint32_t EEClass::m_dwAttrClass;
	   __inherited uint32_t EEClass::m_VMFlags;
	   __inherited uint8_t EEClass::m_NormType;
	   __inherited uint8_t EEClass::m_cbBaseSizePadding;
	   __inherited uint16_t EEClass::m_NumInstanceFields;
	   __inherited uint16_t EEClass::m_NumMethods;
	   __inherited uint16_t EEClass::m_NumStaticFields;
	   __inherited uint16_t EEClass::m_NumHandleStatics;
	   __inherited uint16_t EEClass::m_NumThreadStaticFields;
	   __inherited uint16_t EEClass::m_NumHandleThreadStatics;
	   __inherited uint16_t EEClass::m_NumNonVirtualSlots;
	   __inherited uint32_t EEClass::m_NonGCStaticFieldBytes;
	   __inherited uint32_t EEClass::m_NonGCThreadStaticFieldBytes;
	   class EEClassLayoutInfo m_LayoutInfo;
	   class Volatile<EEClassNativeLayoutInfo *> m_nativeLayoutInfo;
   };
 *
 *
 *
 */

	[field: FieldOffset(OFFSET_LAYOUTINFO)]
	internal EEClassLayoutInfo LayoutInfo;

	[field: FieldOffset(OFFSET_NATIVELAYOUTINFO)]
	internal EEClassNativeLayoutInfo* NativeLayoutInfo;

}