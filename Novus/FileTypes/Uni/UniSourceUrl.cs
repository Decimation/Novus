// Deci Novus UniSourceUri.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using Flurl;
using Flurl.Http;
using Kantan.Net.Utilities;
using Microsoft.Net.Http.Headers;
using Novus.OS;
using System.Diagnostics;
using System.Net;
using Novus.FileTypes.Media;

namespace Novus.FileTypes.Uni;

public class UniSourceUrl : UniSource, IUniSource
{

	public Url Url { get; }

	internal UniSourceUrl(Url value) : base(value, UniSourceType.Url)
	{
		Url  = (Url) value;
		Name = Url.GetFileName();
	}

	public override ValueTask<string> TryWriteToFileAsync(string fn = null, string ext = null)
	{
		fn ??= Name;
		return base.TryWriteToFileAsync(fn, ext);
	}


	public override async ValueTask<bool> AllocBuffer(CancellationToken ct = default)
	{
		bool ok = HasBuffer;

		IFlurlResponse res = null;

		if (ok) {
			goto ret;
		}

		res = await Url.AllowAnyHttpStatus()
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
		               }).GetAsync(cancellationToken: ct);

		if (res is null or { ResponseMessage.IsSuccessStatusCode: false }) {

			ok = false;
			goto ret;
		}

		Buffer = await res.GetBytesAsync();

		if (!HasMediaType && res.TryGetMediaType(out var hdr)) {
			MediaType = new MediaType(hdr, []);
		}

	ret:
		res?.Dispose();
		return ok;
	}

	

	public static bool IsUrlType(object input, out Url url)
	{
		url = input switch
		{
			string { } s when Url.IsValid(s) => Url.Parse(s),
			Url u                            => u,
			_                                => null
		};

		return url != null && url.Scheme != "file";
	}

}