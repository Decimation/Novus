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
		var m = await IsValid(s, true);

		using var copy = m.Stream.Copy();

		resolver ??= IFileTypeResolver.Default;

		//TODO

		var types = await resolver.ResolveAsync(copy);

		if (m.Stream.CanSeek) {
			m.Stream.Position = 0;
		}

		return new QFileHandle()
		{
			Stream    = m.Stream,
			Value     = s,
			IsFile    = m.IsFile,
			IsUri     = m.IsUri,
			FileTypes = types.ToArray()
		};
	}

	private static async Task<QInputInfo> IsValid(string s, bool auto = false)
	{
		var b = Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u);

		QInputInfo m = default;

		if (b) {
			bool isFile = (u.IsFile && File.Exists(s));

			m = new()
			{
				IsFile = isFile,
				IsUri  = !isFile //todo

			};

			if (isFile) {
				if (auto) {
					m.Stream = File.OpenRead(s);

				}
			}
			else if (m.IsUri) {
				if (auto) {
					var handler = new HttpClientHandler()
						{ };

					using var client = new HttpClient(handler)
						{ };

					m.Stream = await client.GetStreamAsync(s);

				}
			}
			else { }

		}

		return m;
	}

	#region IDisposable

	public void Dispose()
	{
		Stream?.Dispose();
	}

	#endregion
}

public struct QInputInfo
{
	public bool IsFile { get; internal set; }

	public bool IsUri { get; internal set; }

	public Stream Stream { get; internal set; }

	public QInputInfo()
	{
		IsFile = false;
		IsUri  = false;

		Stream = Stream.Null;
	}
}