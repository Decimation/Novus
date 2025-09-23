// Deci Novus UniSourceUri.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using System.Diagnostics;
using System.Net;
using Flurl;
using Flurl.Http;
using Kantan.Net.Utilities;
using Novus.OS;

namespace Novus.FileTypes.Uni;

public class UniSourceUrl : UniSource, IUniSource
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

	/*public static IFlurlClient Client { get; set; }

	static UniSourceUrl()
	{
		Client = FlurlHttp.Clients.GetOrAdd(nameof(UniSourceUrl), null, builder => { });
	}*/

#region Overrides of UniSource

	public override async ValueTask<bool> AllocStream(CancellationToken ct = default)
	{
		bool ok = true;

		if (Stream != null) {
			goto ret;
		}


		// value = value.CleanString();

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

		/*if (stream.CanSeek && stream.Length < FileTypes.FileType.RSRC_HEADER_LEN) {

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