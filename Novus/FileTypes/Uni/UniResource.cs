using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Flurl;
using Flurl.Http;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Novus.FileTypes.Uni;

public class UniResource
{

	public MediaTypeHeaderValue SuppliedType { get; protected set; }

	public string Input { get; }

	public MediaTypeFlags Flags { get; protected set; }

	public UniResourceType Type { get; private set; }

	protected internal UniResource() { }

	protected internal UniResource(string input)
	{
		Input = input;
	}

	public static async Task<UniResource> LoadAsync(string input, CancellationToken ct = default)
	{
		UniResource ur = new();

		if (Url.IsValid(input)) {
			var osAsUrl = Url.Parse(input);

			if (osAsUrl.Scheme == "file" && File.Exists(input)) {
				ur.Type = UniResourceType.File;
			}
			else {
				ur.Type = UniResourceType.Http;
			}
		}
		else if (File.Exists(input)) {
			ur.Type = UniResourceType.File;
		}

		return ur;
	}

}

public class UniHttpResource : UniResource
{

	public static async Task<UniHttpResource> FromResponse(IFlurlResponse response, CancellationToken ct = default)
	{
		string               suppliedType       = null;
		MediaTypeHeaderValue suppliedTypeVal    = null;
		bool                 hasSuppliedTypeVal = false;
		MediaTypeFlags     flags              = default;

		if (response.Headers.TryGetFirst(HeaderNames.ContentType, out string contentType)) {
			suppliedType = contentType;

			hasSuppliedTypeVal = MediaTypeHeaderValue.TryParse(suppliedType, out suppliedTypeVal);
		}

		if (hasSuppliedTypeVal && MediaTypeUtilities.ApacheBugContentTypes.Any(hv => hv.Equals(suppliedTypeVal))) {
			flags |= MediaTypeFlags.CheckForApacheBug;
		}


		var ur = new UniHttpResource() { SuppliedType = suppliedTypeVal, Flags = flags };
		return ur;
	}

}

public enum UniResourceType
{

	None   = 0,
	File   = 1,
	Http   = 2,
	Stream = 3,

}

[Flags]
public enum MediaTypeFlags
{

	None              = 0,
	CheckForApacheBug = 1 << 0,

}