using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Novus.FileTypes;

public class DynamicResource
{
	private DynamicResource() { }

	public string Value { get; private set; }

	public Stream Stream { get; private set; }

	public static async Task<DynamicResource> Get(string s)
	{
		var b = Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u);

		using var client = new HttpClient();

		var stream = b
			             ? (u.IsFile && File.Exists(s)
				                ? File.OpenRead(s)
				                : await client.GetStreamAsync(s))
			             : Stream.Null;

		return new DynamicResource()
		{
			Stream = stream,
			Value  = s
		};
	}
}

public static class DynamicResourceExtensions
{
	public static async Task<DynamicResource> get(this string s)
	{
		return await DynamicResource.Get(s);
	}
}