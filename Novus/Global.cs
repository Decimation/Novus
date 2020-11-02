using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using SimpleCore.Diagnostics;

[assembly: InternalsVisibleTo("Test")]

namespace Novus
{
	public static class Global
	{
		public static void Setup()
		{
			bool c = Compat();

			if (!c) {
				Guard.Fail<Exception>();
			}
		}

		public static readonly Version ClrVersion = new Version(3,1,9);

		public static bool Compat()
		{
			bool ver = Environment.Version == ClrVersion;
			bool b   = !GCSettings.IsServerGC;

			return ver && b;
		}
	}
}
