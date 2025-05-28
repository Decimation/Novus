// Deci Novus UniSourceFile.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Microsoft;

namespace Novus.FileTypes.Uni;

public class UniSourceFile : UniSource, IUniSource
{

	internal UniSourceFile(FileInfo value) : base(UniSourceType.File, value)
	{
		FileInfo = value;
		Name     = FileInfo.Name;
	}


	public FileInfo FileInfo { get; }

	public override ValueTask<bool> AllocStream(CancellationToken ct = default)
	{
		var b = Stream != null;

		if (b) {
			goto ret;
		}

		Stream = FileInfo.OpenRead();

	ret:
		return ValueTask.FromResult(true);
	}

	public override ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		return ValueTask.FromResult(FileInfo.FullName);
	}

	/*public static bool IsType(object o, out object f)
	{
		f = null;

		if (o is string { } s && File.Exists(s)) {
			f = new FileInfo(s);
		}

		return f != null;
	}*/

}