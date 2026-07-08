using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Novus.OS;
using Novus.Runtime;

namespace Novus.Win32;

//TODO: WIP
//https://github.com/aayush-pokharel/ClipboardMonitor

[SupportedOSPlatform(RuntimeInformationExtensions.OS_WIN)]
public static class Clipboard
{

	public static bool IsOpen { get; private set; }

	public static bool Open()
	{
		return IsOpen = Native.OpenClipboard(IntPtr.Zero);

	}

	public static bool Close()
	{
		return IsOpen = !Native.CloseClipboard();
	}

	public static bool IsFormatAvailable(uint fmt)
		=> Native.IsClipboardFormatAvailable(fmt);

	[CBN]
	public static string GetFormatName(uint f)
	{
		const int CAPACITY = 0xFFF;
		var       sb       = new StringBuilder(CAPACITY);
		int       l        = Native.GetClipboardFormatName(f, sb, sb.Capacity);

		if (l == 0) {
			return Enum.GetName((ClipboardFormat) f);
		}

		return sb.ToString();
	}


	public static uint[] EnumFormats()
	{
		var rg = new uint[Native.CountClipboardFormats()];

		uint u = Native.ZERO_U;
		int  i = 0;

		while ((u = Native.EnumClipboardFormats(u)) != Native.ZERO_U) {
			rg[i++] = u;
		}

		return [.. rg];
	}

	public static T ParseFormatData<T>(uint format, Func<nint, T> conv)
	{
		if (IsFormatAvailable(format)) {
			var d = Native.GetClipboardData(format);
			return conv(d);
		}


		return default;
	}

	public static string[] GetDragQueryList()
	{
		var h = Native.GetClipboardData((uint) ClipboardFormat.CF_HDROP);

		var cn = Native.DragQueryFile(h, UInt32.MaxValue, null, 0);
		var rg = new List<string>();

		for (int i = 0; i < cn; i++) {
			var l    = Native.DragQueryFile(h, (uint) i, null, 0) + 1;
			var file = new StringBuilder(l);
			l = Native.DragQueryFile(h, (uint) i, file, l);
			rg.Add(file.ToString());
		}

		return [.. rg];
	}

	[CBN]
	public static object GetData(uint f)
	{
		// f ??= ((EnumFormats().FirstOrDefault<uint>(Native.IsClipboardFormatAvailable)));
		
		var data = Native.GetClipboardData(f);

		if (data == IntPtr.Zero) {
			return null;
		}

		if (FormatToObjectConverters.TryGetValue([(ClipboardFormat)f], out var converter)) {
			var val  = converter(data);

			return val;
		}

		return null;
	}

	public static bool SetData(object s, uint fmt)
	{
		bool b = false;
		unsafe {
			ClipboardFormat[] fmtKey = [(ClipboardFormat) fmt];

			if (!ObjectToFormatConverters.TryGetValue(fmtKey, out Func<object, nint> converter)) {
				return false;
			}

			var               ptr              = converter(s);
			b = Native.SetClipboardData(fmt, ptr.ToPointer()) != IntPtr.Zero;

		}

		return b;
	}

#region

	public static readonly Dictionary<ClipboardFormat[], Func<nint, object>> FormatToObjectConverters = new()
	{
		{ [ClipboardFormat.FileNameW, ClipboardFormat.CF_OEMTEXT, ClipboardFormat.CF_UNICODETEXT], Marshal.PtrToStringUni },
		{ [ClipboardFormat.FileName, ClipboardFormat.CF_TEXT], Marshal.PtrToStringAnsi },
		{ [ClipboardFormat.PNG, ClipboardFormat.PNG2, ClipboardFormat.PNG3, ClipboardFormat.BMP2], Native.CopyGlobalObject },
		{
			[], arg =>
			{
				Trace.WriteLine($"No handler found for format -> {arg}");
				return Native.CopyGlobalObject(arg);
			}
		}
	};

	public static readonly Dictionary<ClipboardFormat[], Func<object, nint>> ObjectToFormatConverters = new()
	{
		{ [ClipboardFormat.FileNameW, ClipboardFormat.CF_OEMTEXT, ClipboardFormat.CF_UNICODETEXT], static s => Marshal.StringToHGlobalUni((string) s) },
		{ [ClipboardFormat.FileName, ClipboardFormat.CF_TEXT], static s => Marshal.StringToHGlobalAnsi((string) s) },
		{
			[], arg =>
			{
				Trace.WriteLine($"No handler found for format -> {arg}");
				return IntPtr.Zero;
			}
		}
	};


#endregion

	public static uint DefaultFormat { get; set; } = (uint) ClipboardFormat.CF_TEXT;

	public static int SequenceNumber => Native.GetClipboardSequenceNumber();

}