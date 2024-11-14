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

namespace Novus.Win32;

//TODO: WIP
//https://github.com/aayush-pokharel/ClipboardMonitor

[SupportedOSPlatform(Global.OS_WIN)]
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

	public static bool IsFormatAvailable(uint n)
		=> Native.IsClipboardFormatAvailable(n);

	public static bool SetData(object s, uint fmt)
	{
		bool b = false;

		unsafe {
			switch (s) {
				case string str:
					var ptr = ClipboardFormatFromObject(fmt)(str);
					b = Native.SetClipboardData(fmt, ptr.ToPointer()) != IntPtr.Zero;
					break;

			}

		}

		return b;
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

		return rg.ToArray();
	}

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


	public static object GetData(uint f)
	{

		// f ??= ((EnumFormats().FirstOrDefault<uint>(Native.IsClipboardFormatAvailable)));
		var fn = ClipboardFormatToObject(f);

		var d = Native.GetClipboardData(f);

		var v = fn?.Invoke(d) ?? d;

		//todo

		return v;
	}

	internal static T FormatConvert<T>(uint format, Func<nint, T> conv)
	{
		if (IsFormatAvailable(format)) {
			var d = Native.GetClipboardData(format);
			return conv(d);
		}

		return default;
	}

	public static string GetFileName()
	{
		return FormatConvert((uint) ClipboardFormat.FileNameW, Marshal.PtrToStringUni);
	}

	public static uint[] EnumFormats()
	{
		var rg = new List<uint>();

		uint u = Native.ZERO_U;

		while ((u = Native.EnumClipboardFormats(u)) != Native.ZERO_U) {
			rg.Add(u);
		}

		return rg.ToArray();
	}

	#region

	[CBN]
	public static Func<nint, object> ClipboardFormatToObject(uint? f)
	{
		switch (f) {
			case (uint) ClipboardFormat.FileNameW or (uint) ClipboardFormat.CF_OEMTEXT
				or (uint) ClipboardFormat.CF_UNICODETEXT:
				return Marshal.PtrToStringUni;

			case (uint) ClipboardFormat.FileName or (uint) ClipboardFormat.CF_TEXT:
				return Marshal.PtrToStringAnsi;

			case (uint) ClipboardFormat.PNG or (uint) ClipboardFormat.PNG2 or (uint) ClipboardFormat.BMP2:
				return Native.CopyGlobalObject;


			default:
				return _ => default;
		}

	}


	[CBN]
	public static Func<object, nint> ClipboardFormatFromObject(uint? f)
	{
		Func<object, nint> fn = f switch
		{
			(uint) ClipboardFormat.FileNameW or (uint) ClipboardFormat.CF_OEMTEXT
				or (uint) ClipboardFormat.CF_UNICODETEXT
				=> static s => Marshal.StringToHGlobalUni((string) s),

			(uint) ClipboardFormat.FileName or (uint) ClipboardFormat.CF_TEXT
				=> static s => Marshal.StringToHGlobalAnsi((string) s),

			_ => default

			// _ => n => nint.Parse((string) n),
		};

		return fn;
	}

	#endregion

	public static uint DefaultFormat { get; set; } = (uint) ClipboardFormat.CF_TEXT;

	public static int SequenceNumber => Native.GetClipboardSequenceNumber();

}