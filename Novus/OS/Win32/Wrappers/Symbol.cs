using Novus.OS.Win32.Structures;
using Novus.OS.Win32.Structures.DbgHelp;

namespace Novus.OS.Win32.Wrappers;

public unsafe class Symbol
{
	internal Symbol(SymbolInfo* pSymInfo)
	{
		Name = pSymInfo->ReadSymbolName();
		//Name         = new string(pSymInfo->Name);
		SizeOfStruct = (int) pSymInfo->SizeOfStruct;
		TypeIndex    = (int) pSymInfo->TypeIndex;
		Index        = (int) pSymInfo->Index;
		Size         = (int) pSymInfo->Size;
		ModBase      = pSymInfo->ModBase;
		Flags        = pSymInfo->Flags;
		Value        = pSymInfo->Value;
		Address      = pSymInfo->Address;
		Register     = (int) pSymInfo->Register;
		Scope        = (int) pSymInfo->Scope;
		Tag          = pSymInfo->Tag;
	}


	public string Name { get; }

	public int SizeOfStruct { get; }

	public int TypeIndex { get; }

	public int Index { get; }

	public int Size { get; }

	public ulong ModBase { get; }

	public ulong Value { get; }

	public ulong Address { get; }

	public int Register { get; }

	public int Scope { get; }

	public SymbolTag Tag { get; }

	public SymbolFlag Flags { get; }

	public long Offset => (long) (Address - ModBase);


	public override string ToString()
	{
		return $"Name: {Name} | Offset: {Offset:X} | Address: {Address:X} | Tag: {Tag} | Flags: {Flags}";
	}
}