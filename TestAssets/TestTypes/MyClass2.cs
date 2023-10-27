using Novus.Imports;
using Novus.Imports.Attributes;

public unsafe class MyClass2
{
	public const string s =
		@"C:\Users\Deci\VSProjects\SandboxLibrary\x64\Release\SandboxLibrary.dll";

	[ImportUnmanaged(s, nameof(doSomething2), ImportType.Offset, Value = "1090")]
	public static readonly delegate* unmanaged<int, void> doSomething2;

	public static readonly RuntimeResource _rr;

	static MyClass2()
	{
		_rr = RuntimeResource.LoadModule(MyClass2.s);
		_rr.LoadImports(typeof(MyClass2));
	}
}