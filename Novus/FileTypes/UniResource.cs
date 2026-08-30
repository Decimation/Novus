using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Flurl.Http;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace Novus.FileTypes;

public abstract class UniResource
{

	public MediaTypeHeaderValue SuppliedType { get; protected set; }

	public string Input { get; }

	public UniResourceFlags Flags { get; protected set; }

	protected UniResource() { }

	public static async Task<UniResource> LoadAsync(string input, CancellationToken ct = default)
	{
		UniResource ur = default;
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
		UniResourceFlags     flags              = default;

		if (response.Headers.TryGetFirst(HeaderNames.ContentType, out string contentType)) {
			suppliedType = contentType;

			hasSuppliedTypeVal = MediaTypeHeaderValue.TryParse(suppliedType, out suppliedTypeVal);
		}

		if (hasSuppliedTypeVal && ResourceTypeUtilities.ApacheBugContentTypes.Any(hv => hv.Equals(suppliedTypeVal))) {
			flags |= UniResourceFlags.CheckForApacheBug;
		}


		var ur = new UniHttpResource() { SuppliedType = suppliedTypeVal, Flags = flags };
		return ur;
	}

}

[Flags]
public enum UniResourceFlags
{

	None              = 0,
	CheckForApacheBug = 1 << 0,

}