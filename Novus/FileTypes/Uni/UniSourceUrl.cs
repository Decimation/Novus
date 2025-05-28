// Deci Novus UniSourceUri.cs
// $File.CreatedYear-$File.CreatedMonth-9 @ 2:39

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

#region Overrides of UniSource

	public override async ValueTask<bool> AllocStream(CancellationToken ct = default)
	{
		if (Stream != null) {
			goto ret;
		}


		// value = value.CleanString();

		var res = await Url.AllowAnyHttpStatus()
			          .WithHeaders(new
			          {
				          User_Agent = ER.UserAgent,
			          })
			          .GetAsync(cancellationToken: ct);

		if (res.ResponseMessage.StatusCode == HttpStatusCode.NotFound) {
			throw new ArgumentException($"{Url} returned {HttpStatusCode.NotFound}");
		}

		Stream = await res.GetStreamAsync();

		/*if (stream.CanSeek && stream.Length < FileTypes.FileType.RSRC_HEADER_LEN) {

		}*/

		ret:
		return true;
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