using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Novus.Utilities;

namespace Novus.FileTypes;

public class QFileHandle : IDisposable
{
	private QFileHandle() { }

	public string Value { get; private set; }

	public Stream Stream { get; private set; }

	public bool IsFile { get; private set; }

	public bool IsUri { get; private set; }

	public FileType[] FileTypes { get; private set; }

	public static async Task<QFileHandle> GetHandleAsync(string s, IFileTypeResolver resolver = null)
	{
		var b = Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u);

		var stream = Stream.Null;

		bool isFile = false, isUri = false;

		if (b) {
			isFile = (u.IsFile && File.Exists(s));
			isUri  = !isFile; //todo

			if (isFile) {
				stream = File.OpenRead(s);
			}
			else if (isUri) {
				using var client = new HttpClient();
				stream = await client.GetStreamAsync(s);
			}
			else { }

		}

		using var copy = stream.Copy();

		resolver ??= IFileTypeResolver.Default;

		var types = await resolver.ResolveAsync(copy);

		if (stream.CanSeek) {
			stream.Position = 0;
		}

		return new QFileHandle()
		{
			Stream    = stream,
			Value     = s,
			IsFile    = isFile,
			IsUri     = isUri,
			FileTypes = types.ToArray()
		};
	}

	#region IDisposable

	public void Dispose()
	{
		Stream?.Dispose();
	}

	#endregion
}