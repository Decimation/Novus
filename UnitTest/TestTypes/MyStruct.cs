using System.Runtime.InteropServices;

namespace UnitTest.TestTypes;

[StructLayout(LayoutKind.Explicit)]
public struct MyStruct
{
	[FieldOffset(default)] public int a;

	[FieldOffset(sizeof(int))] public int b;
}