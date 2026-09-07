// Deci Novus UniSourceUri.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Flurl;
using Flurl.Http;
using Kantan.Net.Utilities;
using Microsoft.Net.Http.Headers;
using Novus.OS;
using System.Diagnostics;
using System.Net;

namespace Novus.FileTypes.Uni;

internal class UniSourceUrl : UniSource, IUniSource
{

	public Url Url { get; }

	internal UniSourceUrl(Url value) : base(UniSourceType.Uri, value)
	{
		Url  = (Url) value;
		Name = Url.GetFileName();
	}

	public override ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		fn ??= Name;
		return base.TryWriteToFileAsync(fn, ext);
	}

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


		return new MediaType(suppliedType) { Value = suppliedTypeVal}
	}

#region Overrides of UniSource

	public override async ValueTask<bool> AllocStream(CancellationToken ct = default)
	{
		bool ok = true;

		if (Stream != null) {
			goto ret;
		}

		var res = await Url.AllowAnyHttpStatus()
			          .WithHeaders(new
			          {
				          User_Agent = ER.UserAgent,
			          })
			          .WithSettings(act =>
			          {
				          act.Redirects.Enabled               = true;
				          act.Redirects.AllowSecureToInsecure = true;
				          act.Redirects.MaxAutoRedirects      = 3;
				          act.HttpVersion                     = "2.0";
			          })
			          .WithCookies(out CookieJar jar)
			          .OnError(err =>
			          {
				          Trace.WriteLine($"{err} {err.Exception}");
				          err.ExceptionHandled = true;
			          })
			          .GetAsync(cancellationToken: ct);

		/*var res2 = await new HttpClient().GetAsync(Url, ct);
		Trace.WriteLine(res2);*/

		if (res is null or
		    {
			    ResponseMessage.IsSuccessStatusCode: false
			    /*ResponseMessage.StatusCode: HttpStatusCode.NotFound or HttpStatusCode.Moved */
		    }) {
			// throw new ArgumentException($"{Url} returned {HttpStatusCode.NotFound}");

			ok = false;
			goto ret;
		}

		Stream = await res.GetStreamAsync();

		/*if (stream.CanSeek && stream.Length < FileTypes.MediaType.RSRC_HEADER_LEN) {

		}*/

	ret:
		return ok;
	}

#endregion


	/*public static bool IsType(object o, out object u)
	{
		Url ux2 = o switch
		{
			Url u2   => u2,
			string s => s,
			_        => null
		};
		u = ux2;
		return Url.IsValid(ux2);
	}*/

}