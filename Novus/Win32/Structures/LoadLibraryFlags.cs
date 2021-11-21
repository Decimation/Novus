using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace Novus.Win32.Structures;

[Flags]
public enum LoadLibraryFlags : uint
{
	AsDataFile      = 0x00000002,
	AsImageResource = 0x00000020
}