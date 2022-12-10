using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32;

public static class Clipboard
{
	public static bool IsOpen { get; private set; }

	public static bool Open()
	{
		return IsOpen = Win32.Native.OpenClipboard(IntPtr.Zero);
	}

	public static bool Close()
	{
		return IsOpen = !Native.CloseClipboard();
	}

	public static object Cache { get; private set; }

	public static bool IsFormatAvailable(uint n)
	{
		return Native.IsClipboardFormatAvailable(n);
	}

	public static bool SetData(object s, uint fmt)
	{
		bool b = false;

		unsafe {
			switch (s) {
				case string str:
					var ptr = ClipboardFormatFromString(fmt)(str);
					b = Native.SetClipboardData(fmt, ptr.ToPointer()) != IntPtr.Zero;
					break;
			}

		}

		return b;
	}

	public static string GetFileName()
	{
		return (string) GetData((uint) ClipboardFormat.FileNameW);
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

	public static object GetData(uint? f = null)
	{
		var fn = ClipboardFormatToString(f);

		f ??= ((EnumFormats().FirstOrDefault<uint>(Native.IsClipboardFormatAvailable)));

		var d = Native.GetClipboardData(f.Value);

		var v = fn(d);

		//todo

		return v;
	}

	public static uint[] EnumFormats()
	{
		var rg = new List<uint>();

		uint u = Native.EnumClipboardFormats(Native.ZERO_U);

		while ((u = Native.EnumClipboardFormats(u)) != Native.ZERO_U) {
			rg.Add(u);
		}

		return rg.ToArray();
	}

	#region 

	public static Func<nint, string> ClipboardFormatToString(uint? f)
	{
		Func<nint, string> fn = f switch
		{
			(uint) ClipboardFormat.FileNameW or
				(uint) ClipboardFormat.CF_OEMTEXT => Marshal.PtrToStringUni,

			(uint) ClipboardFormat.FileName or
				(uint) ClipboardFormat.CF_TEXT => Marshal.PtrToStringAnsi,
				
			_ => Marshal.PtrToStringAuto

		};
		return fn;
	}

	public static Func<string, nint> ClipboardFormatFromString(uint? f)
	{
		Func<string, nint> fn = f switch
		{
			(uint) ClipboardFormat.FileNameW or
				(uint) ClipboardFormat.CF_OEMTEXT => Marshal.StringToHGlobalUni,

			(uint) ClipboardFormat.FileName or
				(uint) ClipboardFormat.CF_TEXT => Marshal.StringToHGlobalAnsi,

			_ => Marshal.StringToHGlobalAuto,
		};

		return fn;
	}

	#endregion

	public static uint DefaultFormat { get; set; } = (uint) ClipboardFormat.CF_TEXT;
}