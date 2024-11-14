// Author: Deci | Project: Novus | Name: CodePages.cs
// Date: 2024/11/13 @ 23:11:21

namespace Novus.Win32.Structures.Other;

#pragma warning disable CS1573
public enum CodePages
{

	CP_IBM437 = 437,

	/// <summary>The system default Windows ANSI code page.</summary>
	CP_ACP = 0,

	/// <summary>The current system Macintosh code page.</summary>
	CP_MACCP = 2,

	/// <summary>The current system OEM code page.</summary>
	CP_OEMCP = 1,

	/// <summary>Symbol code page (42).</summary>
	CP_SYMBOL = 42,

	/// <summary>The Windows ANSI code page for the current thread.</summary>
	CP_THREAD_ACP = 3,

	/// <summary>UTF-7. Use this value only when forced by a 7-bit transport mechanism. Use of UTF-8 is preferred.</summary>
	CP_UTF7 = 65000,

	/// <summary>UTF-8.</summary>
	CP_UTF8 = 65001,

}