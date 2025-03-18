// Author: Deci | Project: Test | Name: MyClass2b.cs
// Date: 2025/02/26 @ 14:02:14

using System.Diagnostics;
using Novus.Imports;
using Novus.Imports.Attributes;

// ReSharper disable InconsistentNaming

namespace Test.TestTypes;

public unsafe class MyClass2b
{
	public const string s =
		@"C:\Users\Deci\VSProjects\SandboxLibrary\x64\Release\SandboxLibrary.dll";

	[ImportUnmanaged(s, nameof(doSomething2), ImportType.Offset, Value = "1090")]
	public static readonly delegate* unmanaged<int, void> doSomething2;

	public static readonly RuntimeResource _rr;

	[ImportInitializer]
	public static void Init()
	{
		Trace.WriteLine($"IIA {nameof(Init)}");

	}

	static MyClass2b()
	{
		Trace.WriteLine($"{nameof(MyClass2b)}");
		_rr = RuntimeResource.LoadModule(MyClass2b.s, out _);
		_rr.LoadImports(typeof(MyClass2b));
	}

}