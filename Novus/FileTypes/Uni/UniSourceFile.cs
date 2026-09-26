// Deci Novus UniSourceFile.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using System.Diagnostics.CodeAnalysis;
using Microsoft;

namespace Novus.FileTypes.Uni;

public class UniSourceFile : UniSource, IUniSource
{

	internal UniSourceFile(FileInfo value) : base(value, UniSourceType.File)
	{
		FileInfo = value;
		Name     = FileInfo.Name;
	}


	public FileInfo FileInfo { get; }

	public override ValueTask<bool> AllocBuffer(CancellationToken ct = default)
	{
		if (!HasBuffer) {
			Buffer = File.ReadAllBytes(FileInfo.FullName);
		}

		return ValueTask.FromResult(HasBuffer);
	}

	public override ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		return ValueTask.FromResult(FileInfo.FullName);
	}

	public static bool IsFileType(object input, out FileInfo file)
	{
		file = input switch
		{
			string { } s when File.Exists(s) => new FileInfo(s),
			_                                => null
		};

		// todo: check for Stream.Null
		return file != null;
	}

}