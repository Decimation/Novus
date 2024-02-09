// Deci Novus UniSourceUri.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

using System.Net;
using Flurl;
using Flurl.Http;
using Kantan.Net.Utilities;

namespace Novus.FileTypes.Uni;

public class UniSourceUrl : UniSource, IUniSource
{

	public override bool IsUri => true;

	public override bool IsFile => false;

	public override bool IsStream => false;

	public override UniSourceType SourceType => UniSourceType.Uri;

	public Url Url { get; }

	internal UniSourceUrl(Stream stream, object value) : base(stream, value)
	{
		Url  = (Url) value;
		Name = Url.GetFileName();
	}

	public override async Task<string> TryDownloadAsync()
	{
		string fn  = null, ext = null;
		var    url = (Url) Value;
		fn = url.GetFileName();

		// var tmp = Path.Combine(Path.GetTempPath(), fn);
		var tmp = FS.GetTempFileName(fn, ext);

		// tmp = FS.SanitizeFilename(tmp);

		var path = await WriteStreamToFileAsync(tmp);
		return path;
	}

	public static async Task<UniSource> HandleUri(Url value, CancellationToken ct)
	{
		// value = value.CleanString();

		var res = await value.AllowAnyHttpStatus()
			          .WithHeaders(new
			          {
				          User_Agent = ER.UserAgent,
			          })
			          .GetAsync(cancellationToken: ct);

		if (res.ResponseMessage.StatusCode == HttpStatusCode.NotFound) {
			throw new ArgumentException($"{value} returned {HttpStatusCode.NotFound}");
		}

		var buf = new UniSourceUrl(await res.GetStreamAsync(), value)
			{ };
		return buf;
	}

	public static bool IsType(object o, out object u)
	{
		Url ux2 = o switch
		{
			Url u2   => u2,
			string s => s,
			_        => null
		};
		u = ux2;
		return Url.IsValid(ux2);
	}

}