// Read S UnitTest UnitTest2.cs
// 2023-08-03 @ 1:10 PM

using Novus.FileTypes;
using Novus.Win32;
using Novus.Win32.Structures.User32;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
#pragma warning disable 0649, CS1998, CS0612

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
				U = new InputUnion
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