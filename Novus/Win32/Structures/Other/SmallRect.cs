﻿using System.Runtime.InteropServices;

namespace Novus.Win32.Structures.Other;

[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
	public short Left;
	public short Top;
	public short Right;
	public short Bottom;
}