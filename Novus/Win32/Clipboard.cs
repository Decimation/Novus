using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Novus.Win32;

//TODO: WIP
//https://github.com/aayush-pokharel/ClipboardMonitor

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

	public static bool IsFormatAvailable(uint n) => Native.IsClipboardFormatAvailable(n);

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

	public static object GetData(uint? f = null)
	{

		f ??= ((EnumFormats().FirstOrDefault<uint>(Native.IsClipboardFormatAvailable)));
		var fn = ClipboardFormatToObject(f);

		var d = Native.GetClipboardData(f.Value);

		var v = fn?.Invoke(d) ?? d;

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

	[CBN]
	public static Func<nint, object> ClipboardFormatToObject(uint? f)
	{
		switch (f) {
			case (uint) ClipboardFormat.FileNameW or (uint) ClipboardFormat.CF_OEMTEXT:
				return Marshal.PtrToStringUni;
			case (uint) ClipboardFormat.PNG:
				return (u) =>
				{
					var size = Native.GlobalSize(u);
					var rg   = new byte[size];
					Marshal.Copy(u, rg, 0, (int) size);
					return rg;
				};
			case (uint) ClipboardFormat.FileName or (uint) ClipboardFormat.CF_TEXT:
				return Marshal.PtrToStringAnsi;
			default:
				return _ => default;
		}

	}

	[CBN]
	public static Func<object, nint> ClipboardFormatFromObject(uint? f)
	{
		Func<object, nint> fn = f switch
		{
			(uint) ClipboardFormat.FileNameW or
				(uint) ClipboardFormat.CF_OEMTEXT => s => Marshal.StringToHGlobalUni((string) s),

			(uint) ClipboardFormat.FileName or
				(uint) ClipboardFormat.CF_TEXT => s => Marshal.StringToHGlobalAnsi((string) s),

			_ => null
			// _ => n => nint.Parse((string) n),
		};

		return fn;
	}

	#endregion

	public static uint DefaultFormat { get; set; } = (uint) ClipboardFormat.CF_TEXT;

	public static int SequenceNumber => Native.GetClipboardSequenceNumber();
}