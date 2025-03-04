﻿// Deci Novus UniSourceStream.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Novus.OS;

namespace Novus.FileTypes.Uni;

public class UniSourceStream : UniSource, IUniSource
{

	public override bool IsUri => false;

	public override bool IsFile => false;

	public override bool IsStream => true;

	public override UniSourceType SourceType => UniSourceType.Stream;

	internal UniSourceStream(Stream stream) : base(stream, stream)
	{
		Name = $"<stream {Stream.GetHashCode()}>";
	}

	public override async Task<string> TryDownloadAsync()
	{
		string fn = null, ext = null;

		ext = FileType.Subtype;
		var tmp = FileSystem.GetTempFileName(fn, ext);

		// tmp = FileSystem.SanitizeFilename(tmp);

		var path = await WriteStreamToFileAsync(tmp);
		return path;
	}

	public static bool IsType(object o, out object t2)
	{
		t2 = Stream.Null;

		if (o is Stream sz) {
			t2 = sz;
		}

		return t2 != Stream.Null;
	}

}