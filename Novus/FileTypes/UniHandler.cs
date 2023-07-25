// Read S Novus UniHandler.cs
// 2023-07-19 @ 1:13 AM

using Flurl;

namespace Novus.FileTypes
{
	public static class UniHandler
	{
		public static bool IsStream(object o) => o is Stream;

		public static bool IsFile(object o) => o is string s && File.Exists(s);

		public static UniSourceType GetUniType(object o, out object o2)
		{
			// lol
			o2 = o;
			if (IsStream(o)) {
				return UniSourceType.Stream;
			}

			if (IsFile(o)) {
				return UniSourceType.File;
			}

			if (IsUrl(o, out var uu)) {
				o2 = uu;
				return UniSourceType.Uri;
			}

			return UniSourceType.NA;

		}

		public static bool IsUrl(object o, out Url u)
		{
			u = o switch
			{
				Url u2   => u2,
				string s => s,
				_        => null
			};

			return Url.IsValid(u);
		}
	}
}