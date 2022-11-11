using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Novus.Utilities;

namespace Novus.FileTypes;

//TODO: WIP

public class UniFile : IDisposable
{
	private UniFile() { }

	public UniFileInfo Info { get; private init; }

	public FileType[] FileTypes { get; private init; }

	public static async Task<UniFile> GetHandleAsync(string s, IFileTypeResolver resolver = null)
	{
		var m = await GetInfoAsync(s, true);

		using var copy = m.Stream.Copy();

		resolver ??= IFileTypeResolver.Default;

		//TODO

		var types = await resolver.ResolveAsync(copy);

		if (m.Stream.CanSeek) {
			m.Stream.Position = 0;
		}

		return new UniFile()
		{
			Info      = m,
			FileTypes = types.ToArray()
		};
	}

	public static async Task<UniFileInfo> GetInfoAsync([NN] string s, bool auto = false)
	{
		// var b = Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u);
		var isFile = File.Exists(s);
		var isUrl  = Url.IsValid(s);

		UniFileInfo m;

		if (isFile || isUrl) {

			m = new()
			{
				IsFile = isFile,
				IsUri  = isUrl

			};

			try {
				if (m.IsFile) {
					if (auto) {

						m.Stream = File.OpenRead(s);

					}
				}
				else if (m.IsUri) {
					if (auto) {
						/*var handler = new HttpClientHandler()
							{ };

						using var client = new HttpClient(handler)
							{ };
						var req = new HttpRequestMessage(HttpMethod.Get, s);

						var res = await client.SendAsync(req);

						m.Stream = await res.Content.ReadAsStreamAsync();*/
						m.Stream = await s.GetStreamAsync();
					}
				}
				else { }
			}
			catch (Exception e) { }

		}
		else {
			m = default;
		}

		return m;
	}

	#region IDisposable

	public void Dispose()
	{
		Info.Dispose();
	}

	#endregion
}

public struct UniFileInfo : IEquatable<UniFileInfo>, IDisposable
{
	public string Value { get; internal set; }

	public bool IsFile { get; internal set; }

	public bool IsUri { get; internal set; }

	public Stream Stream { get; internal set; }

	public bool IsValid => IsFile || IsUri;

	public UniFileInfo()
	{
		IsFile = false;
		IsUri  = false;
		Value  = null;
		Stream = Stream.Null;
	}

	#region Equality members

	public override int GetHashCode()
	{
		unchecked {
			int hashCode = (Value != null ? Value.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ IsFile.GetHashCode();
			hashCode = (hashCode * 397) ^ IsUri.GetHashCode();
			hashCode = (hashCode * 397) ^ (Stream != null ? Stream.GetHashCode() : 0);
			return hashCode;
		}
	}

	public bool Equals(UniFileInfo other)
	{
		return Value == other.Value && IsFile == other.IsFile && IsUri == other.IsUri && Equals(Stream, other.Stream);
	}

	public override bool Equals(object obj)
	{
		return obj is UniFileInfo other && Equals(other);
	}

	public static bool operator ==(UniFileInfo left, UniFileInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(UniFileInfo left, UniFileInfo right)
	{
		return !left.Equals(right);
	}

	#endregion

	#region IDisposable

	public void Dispose()
	{
		Stream?.Dispose();
	}

	#endregion
}