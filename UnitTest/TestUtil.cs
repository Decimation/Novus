using System;
using NUnit.Framework;
// ReSharper disable UnusedMember.Global

namespace UnitTest;

public static class TestUtil
{
	public static void AssertAll<T>(T t, params Func<T, bool>[] fn)
	{
		foreach (var func in fn) {
			Assert.True(func(t));
		}
	}

	public static void AssertAll(params bool[] cond)
	{
		foreach (bool b in cond) {
			Assert.True(b);
		}
	}
}