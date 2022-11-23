using System.Runtime.InteropServices;
using Kantan.Diagnostics;

namespace Novus.FileTypes.Impl;

public sealed class UrlmonResolver : IFileTypeResolver
{
	private UrlmonResolver() { }

	public static readonly IFileTypeResolver Instance = new UrlmonResolver();

	public void Dispose() { }

	public static string ResolveFromData(byte[] dataBytes, string mimeProposed = null)
	{
		//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
		//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
		//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
		//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

		Require.ArgumentNotNull(dataBytes, nameof(dataBytes));

		string mimeRet = String.Empty;

		if (!String.IsNullOrEmpty(mimeProposed)) {
			//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
			mimeRet = mimeProposed;
		}

		const MimeFromDataFlags flags = MimeFromDataFlags.ENABLE_MIME_SNIFFING |
		                                MimeFromDataFlags.RETURN_UPDATED_IMG_MIMES |
		                                MimeFromDataFlags.IGNORE_MIME_TEXT_PLAIN;

		int ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length,
		                           mimeProposed, flags, out nint outPtr, 0);

		if (ret == 0 && outPtr != IntPtr.Zero) {
			string str = Marshal.PtrToStringUni(outPtr);

			Marshal.FreeCoTaskMem(outPtr);

			return str;
		}

		return mimeRet;
	}

	public IEnumerable<FileType> Resolve(byte[] buf)
	{
		var data = ResolveFromData(buf);
		return new[] { new FileType() { MediaType = data } };
	}

	[DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
	private static extern int FindMimeFromData(nint pBC, [MA(UT.LPWStr)] string pwzUrl,
	                                           [MA(UT.LPArray, ArraySubType = UT.I1, SizeParamIndex = 3)]
	                                           byte[] pBuffer, int cbSize,
	                                           [MA(UT.LPWStr)] string pwzMimeProposed,
	                                           MimeFromDataFlags dwMimeFlags, out nint ppwzMimeOut,
	                                           int dwReserved);

	/// <see cref="FindMimeFromData"/>
	[Flags]
	private enum MimeFromDataFlags
	{
		DEFAULT                  = 0x00000000,
		URL_AS_FILENAME          = 0x00000001,
		ENABLE_MIME_SNIFFING     = 0x00000002,
		IGNORE_MIME_TEXT_PLAIN   = 0x00000004,
		SERVER_MIME              = 0x00000008,
		RESPECT_TEXT_PLAIN       = 0x00000010,
		RETURN_UPDATED_IMG_MIMES = 0x00000020,
	}
}