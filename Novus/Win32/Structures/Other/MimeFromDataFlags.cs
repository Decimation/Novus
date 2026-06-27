// Author: Deci | Project: Novus | Name: MimeFromDataFlags.cs
// Date: 2026/06/27 @ 13:06:18

namespace Novus.Win32.Structures.Other;

#pragma warning disable 649
/// <see cref="Native.FindMimeFromData"/>
[Flags]
public enum MimeFromDataFlags
{
	DEFAULT                  = 0x00000000,
	URL_AS_FILENAME          = 0x00000001,
	ENABLE_MIME_SNIFFING     = 0x00000002,
	IGNORE_MIME_TEXT_PLAIN   = 0x00000004,
	SERVER_MIME              = 0x00000008,
	RESPECT_TEXT_PLAIN       = 0x00000010,
	RETURN_UPDATED_IMG_MIMES = 0x00000020,
}