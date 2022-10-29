using System.Runtime.InteropServices;
using System.Text;

namespace Novus.Win32
{
	public static unsafe partial class Native
	{
		[DllImport(SHELL32_DLL, CharSet = CharSet.Unicode)]
		public static extern int DragQueryFile(IntPtr hDrop, uint iFile,
		                                     [Out] StringBuilder lpszFile, int cch);

		[DllImport(SHELL32_DLL, CharSet = CharSet.Ansi)]
		public static extern void DragAcceptFiles(IntPtr hDrop, [MA(UT.Bool)] bool fAccept);
	}
}