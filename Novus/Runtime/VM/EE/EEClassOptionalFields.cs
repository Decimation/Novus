// Author: Deci | Project: Novus | Name: EEClassOptionalFields.cs
// Date: 2025/04/09 @ 17:04:24

using System.Runtime.InteropServices;
using Novus.Imports.Attributes;

// ReSharper disable InconsistentNaming

namespace Novus.Runtime.VM.EE;
#pragma warning disable CA1069

[NativeStructure]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EEClassOptionalFields
{

/*
 * class EEClassOptionalFields
   {
	   class DictionaryLayout* m_pDictLayout;
	   uint8_t* m_pVarianceInfo;
	   class SparseVTableMap* m_pSparseVTableMap;
	   class TypeHandle m_pCoClassForIntf;
	   class ClassFactoryBase* m_pClassFactory;
	   uint8_t m_requiredFieldAlignment;
   };
 *
 */

	internal void* DictLayout { get; set; }

	internal byte* VarianceInfo { get; set; }

	internal void* SparseVTableMap { get; set; }

	internal TypeHandle CoClassForIntf { get; set; }

	internal void* ClassFactory { get; set; }

	internal byte RequiredFieldAlignment { get; set; }
}