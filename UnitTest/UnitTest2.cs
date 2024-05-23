// Read S UnitTest UnitTest2.cs
// 2023-08-03 @ 1:10 PM

using Novus.FileTypes;
using Novus.Memory;
using Novus.Win32;
using Novus.Win32.Structures.User32;
using NUnit.Framework;
using System;
using System.Diagnostics;
using Novus;
using Novus.Properties;
using Novus.Utilities;

// ReSharper disable InconsistentNaming
#pragma warning disable 0649, CS1998, CS0612
#pragma warning disable CA1416

namespace UnitTest;

[TestFixture]
[Parallelizable]
public class Tests_Other
{

	[Test]
	public static void SendInputTest()
	{
		var a = Native.SendInput(new[]
		{
			new InputRecord
			{
				type = InputType.Keyboard,
				Unsafe = new InputUnion
				{
					ki = new KeyboardInput
					{
						// dwFlags     = (KeyEventFlags.KeyDown | KeyEventFlags.SCANCODE),
						// wScan       = ScanCodeShort.KEY_W,

						wVk         = VirtualKey.MEDIA_PLAY_PAUSE,
						dwFlags     = 0,
						dwExtraInfo = new nuint((uint) Native.GetMessageExtraInfo().ToInt64())
					},
				}
			}
		});

		if (a != 0) {
			Assert.Pass();
		}
	}

}

[TestFixture]
[Parallelizable]
public class Tests_FileTypes
{

	[Test]
	public void Test1()
	{
		foreach (var ft in FileType.Image) {
			TestContext.WriteLine($"{ft}");
		}
	}

}

[TestFixture]
public class Tests_Runtime2
{

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTest2(object s)
	{
		var p = Mem.AddressOfHeap(s);

		Mem.InvokeWhilePinned(s, o =>
		{
			Assert.False(AddPressure(p, o));
		});
	}

	[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTest(object s)
	{

		//var g = GCHandle.Alloc(s, GCHandleType.Pinned);

		var p = Mem.AddressOfHeap(s);

		Mem.Pin(s);
		Assert.False(AddPressure(p, s));

		Mem.Unpin(s);

		// Assert.True(AddPressure(p, s));

	}

	/*[Test]
	[TestCase("foo")]
	[TestCase(new[] { 1, 2, 3 })]
	public void PinTestInv(object s)
	{

		//var g = GCHandle.Alloc(s, GCHandleType.Pinned);

		var p = Mem.AddressOfHeap(s);

		Mem.Pin(s);
		Assert.False(AddPressure(p, s));

		Mem.Unpin(s);
		Assert.True(AddPressure(p, s));

	}*/

	private static bool AddPressure(Pointer<byte> p, object s, long i1 = 5_000)
	{
		var random = new Random();

		for (int i = 0; i < i1; i++) {
			GC.AddMemoryPressure(i1);

			//GC.AddMemoryPressure(100000);
			var r = new object[i1];

			for (int j = 0; j < r.Length; j++) {
				r[j] = random.Next();

				if (p != Mem.AddressOfHeap(s)) {
					return true;
				}
			}

			GC.Collect();

			if (p != Mem.AddressOfHeap(s)) {
				return true;
			}

			// GC.RemoveMemoryPressure(i1);
		}

		return p != Mem.AddressOfHeap(s);

		// return false;
	}

}

[TestFixture]
public class Tests_SigScanner
{

	[Test]
	public void Test1()
	{
		Process proc = Process.GetCurrentProcess();
		var     s    = SigScanner.FromProcess(proc, proc.FindModule(Global.CLR_MODULE));

		Assert.AreEqual(s.FindSignature(EmbeddedResources.Sig_GetIL),
		                s.FindSignature(SigScanner.ReadSignature(EmbeddedResources.Sig_GetIL)));
	}

}