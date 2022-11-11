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

	public string Value { get; internal set; }

	public bool IsFile { get; internal set; }

	public bool IsUri { get; internal set; }

	public Stream Stream { get; internal set; }

	public bool IsValid => IsFile || IsUri;

	public FileType[] FileTypes { get; private init; }

	public static async Task<UniFile> GetAsync(string value, IFileTypeResolver resolver = null,
	                                           params FileType[] whitelist)
	{
		// TODO: null or throw exception?

		value = value.CleanString();

		bool isFile, isUrl;
		var  stream = Stream.Null;

		isFile = File.Exists(value);

		if (isFile) {
			stream = File.OpenRead(value);
			isUrl  = false;
		}
		else {
			var res = await value.AllowAnyHttpStatus().WithHeaders(new
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

		var sq = new UniFile()
		{
			Value = value,
			Stream = stream,
			IsFile    = isFile,
			IsUri     = isUrl,
			FileTypes = types
		};

		return sq;
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

	#region Overrides of Object

	public override string ToString()
	{
		return $"{Value} :: {(IsFile ? "File" : "Uri")} [{FileTypes.QuickJoin()}]";
	}

	#endregion

	#region IDisposable

	public void Dispose()
	{
		Stream?.Dispose();
	}

	#endregion
}