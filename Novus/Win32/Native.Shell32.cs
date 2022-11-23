using System.Runtime.InteropServices;
using System.Text;

namespace Novus.Win32
{
	public static unsafe partial class Native
	{
		[DllImport(SHELL32_DLL, CharSet = CharSet.Unicode)]
		public static extern int DragQueryFile(nint hDrop, uint iFile,
		                                     [Out] StringBuilder lpszFile, int cch);

		[DllImport(SHELL32_DLL, CharSet = CharSet.Ansi)]
		public static extern void DragAcceptFiles(nint hDrop, [MA(UT.Bool)] bool fAccept);
	}
}