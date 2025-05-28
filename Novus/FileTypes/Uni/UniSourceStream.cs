// Deci Novus UniSourceStream.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Novus.OS;

namespace Novus.FileTypes.Uni;

public class UniSourceStream : UniSource, IUniSource
{

	internal UniSourceStream(Stream stream) : base(UniSourceType.Stream, stream)
	{
		Stream = stream;
		Name   = $"<stream {Stream.GetHashCode()}>";
	}


	public override ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		return base.TryWriteToFileAsync(fn, ext ?? FileType.Subtype);
	}

	public override ValueTask<bool> AllocStream(CancellationToken ct = default)
	{
		return ValueTask.FromResult(true);
	}

	/*public static bool IsType(object o, out object t2)
	{
		t2 = Stream.Null;

		if (o is Stream sz) {
			t2 = sz;
		}

		return t2 != Stream.Null;
	}*/

}