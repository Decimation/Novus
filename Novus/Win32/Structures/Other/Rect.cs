using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32.Structures.Other;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;
}