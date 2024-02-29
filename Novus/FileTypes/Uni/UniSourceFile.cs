// Deci Novus UniSourceFile.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Microsoft;

namespace Novus.FileTypes.Uni;

public class UniSourceFile : UniSource, IUniSource
{

	public override UniSourceType SourceType => UniSourceType.File;

	public override bool IsUri => false;

	public override bool IsFile => true;

	public override bool IsStream => false;

	internal UniSourceFile(Stream stream, object value) : base(stream, value)
	{
		Name     = Path.GetFileName(Value.ToString());
		FileName = Value.ToString();
	}

	public string FileName { get; }

	public override Task<string> TryDownloadAsync()
	{
		var fileName = Value.ToString();
		
		if (!File.Exists(fileName)) {
			throw new FileNotFoundException(fileName: fileName, message: "Not found");
		}

		return Task.FromResult(fileName);
	}

	public static bool IsType(object o, out object f)
	{
		f = null;

		if (o is string { } s && File.Exists(s)) {
			f = new FileInfo(s);
		}

		return f != null;
	}

}