using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Kantan.Text;
using Novus.Utilities;

namespace Novus.FileTypes;

//TODO: WIP

public sealed class UniFile : IDisposable
{
	private UniFile() { }

	public string Value { get; internal init; }

	public bool IsFile { get; internal init; }

	public bool IsUri { get; internal init; }

	public Stream Stream { get; internal init; }

	public bool IsValid => IsFile || IsUri;

	public FileType[] FileTypes { get; private init; }

	public static readonly UniFile Null = new();

	public static (bool IsFile, bool IsUri) IsUriOrFile(string value) => (File.Exists(value), Url.IsValid(value));

	public static async Task<UniFile> GetAsync(string value, IFileTypeResolver resolver = null,
	                                           params FileType[] whitelist)
	{
		// TODO: null or throw exception?

		value = value.CleanString();

		var (isFile, isUrl) = IsUriOrFile(value);
		var stream = Stream.Null;

		if (isFile) {
			stream = File.OpenRead(value);
			isUrl  = false;
		}
		else {
			var res = await value.AllowAnyHttpStatus()
			                     .WithHeaders(new
			                     {
				                     User_Agent = ER.UserAgent,

			                     })
			                     .GetAsync();

			/*if (!res.ResponseMessage.IsSuccessStatusCode) {
					Debug.WriteLine($"invalid status code {res.ResponseMessage.StatusCode} {value}");
					return null;
				}*/

			stream = await res.GetStreamAsync();
			isUrl  = true;

		}

		// Trace.Assert((isFile || isUrl) && !(isFile && isUrl));

		var types = (await IFileTypeResolver.Default.ResolveAsync(stream)).ToArray();

		if (whitelist.Any()) {
			var inter = types.Intersect(whitelist);

			if (!inter.Any()) {
				// var e = new ArgumentException("Invalid file types", nameof(value));
				// return await Task.FromException<SearchQuery>(e);
				// Debug.WriteLine($"Invalid file types: {value} {types.QuickJoin()}", nameof(TryGetAsync));
				// return null;
				throw new ArgumentException($"Invalid file types: {types.QuickJoin()}", nameof(value));
			}

		}

		var uf = new UniFile()
		{
			Value     = value,
			Stream    = stream,
			IsFile    = isFile,
			IsUri     = isUrl,
			FileTypes = types
		};

		if (uf.Stream.CanSeek) {
			uf.Stream.Position = 0;
		}

		return uf;
	}

	public static async Task<UniFile> TryGetAsync(string value, IFileTypeResolver resolver = null,
	                                              params FileType[] whitelist)
	{
		try {
			return await GetAsync(value, resolver, whitelist);
		}
		catch (FlurlHttpException e) {
			Debug.WriteLine($"HTTP: {e.Message}", nameof(TryGetAsync));
		}
		catch (ArgumentException e) {
			Debug.WriteLine($"Argument: {e.Message}", nameof(TryGetAsync));

		}
		catch (Exception e) { }
		finally { }

		return null;
	}

	public override string ToString()
	{
		return $"{Value} :: {(IsFile ? "File" : "Uri")} [{FileTypes.QuickJoin()}]";
	}

	public void Dispose()
	{
		Stream?.Dispose();
	}

	public async Task<Memory<byte>> ToMemoryAsync()
	{
		var m = new byte[Stream.Length];
		var i = await Stream.ReadAsync(m);
		return m;
	}

	public async Task<string> DownloadAsync()
	{
		var async    = await ToMemoryAsync();
		var fileName = Path.GetTempFileName();
		await File.WriteAllBytesAsync(fileName, async.ToArray());
		return fileName;
	}
}