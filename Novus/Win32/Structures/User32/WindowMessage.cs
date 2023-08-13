using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novus.Win32.Structures.User32;

public enum WindowMessage
{
	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-null
	WM_NULL = 0x0000,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-create
	WM_CREATE = 0x0001,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-destroy
	WM_DESTROY = 0x0002,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-move
	WM_MOVE = 0x0003,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-size
	WM_SIZE = 0x0005,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-activate
	WM_ACTIVATE = 0x0006,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-setfocus
	WM_SETFOCUS = 0x0007,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-killfocus
	WM_KILLFOCUS = 0x0008,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-enable
	WM_ENABLE = 0x000A,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-setredraw
	WM_SETREDRAW = 0x000B,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-settext
	WM_SETTEXT = 0x000C,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-gettext
	WM_GETTEXT = 0x000D,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-gettextlength
	WM_GETTEXTLENGTH = 0x000E,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-paint
	WM_PAINT = 0x000F,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-close
	WM_CLOSE = 0x0010,

	// https://docs.microsoft.com/en-us/windows/win32/shutdown/wm-queryendsession
	WM_QUERYENDSESSION = 0x0011,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-queryopen
	WM_QUERYOPEN = 0x0013,

	// https://docs.microsoft.com/en-us/windows/win32/shutdown/wm-endsession
	WM_ENDSESSION = 0x0016,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-quit
	WM_QUIT = 0x0012,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-erasebkgnd
	WM_ERASEBKGND = 0x0014,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-syscolorchange
	WM_SYSCOLORCHANGE = 0x0015,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-showwindow
	WM_SHOWWINDOW = 0x0018,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-wininichange
	WM_WININICHANGE = 0x001A,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange
	WM_SETTINGCHANGE = WM_WININICHANGE,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-devmodechange
	WM_DEVMODECHANGE = 0x001B,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-activateapp
	WM_ACTIVATEAPP = 0x001C,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-fontchange
	WM_FONTCHANGE = 0x001D,

	// https://docs.microsoft.com/en-us/windows/win32/sysinfo/wm-timechange
	WM_TIMECHANGE = 0x001E,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-cancelmode
	WM_CANCELMODE = 0x001F,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-setcursor
	WM_SETCURSOR = 0x0020,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mouseactivate
	WM_MOUSEACTIVATE = 0x0021,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-childactivate
	WM_CHILDACTIVATE = 0x0022,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-queuesync
	WM_QUEUESYNC = 0x0023,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-getminmaxinfo
	WM_GETMINMAXINFO = 0x0024,

	WM_PAINTICON = 0x0026,

	WM_ICONERASEBKGND = 0x0027,

	// https://docs.microsoft.com/en-us/windows/win32/dlgbox/wm-nextdlgctl
	WM_NEXTDLGCTL = 0x0028,

	// https://docs.microsoft.com/en-us/windows/win32/printdocs/wm-spoolerstatus
	WM_SPOOLERSTATUS = 0x002A,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-drawitem
	WM_DRAWITEM = 0x002B,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-measureitem
	WM_MEASUREITEM = 0x002C,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-deleteitem
	WM_DELETEITEM = 0x002D,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-vkeytoitem
	WM_VKEYTOITEM = 0x002E,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-chartoitem
	WM_CHARTOITEM = 0x002F,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-setfont
	WM_SETFONT = 0x0030,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-getfont
	WM_GETFONT = 0x0031,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-sethotkey
	WM_SETHOTKEY = 0x0032,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-gethotkey
	WM_GETHOTKEY = 0x0033,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-querydragicon
	WM_QUERYDRAGICON = 0x0037,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-compareitem
	WM_COMPAREITEM = 0x0039,

	WM_GETOBJECT = 0x003D,

	WM_COMPACTING = 0x0041,

	[Obsolete]
	WM_COMMNOTIFY = 0x0044,

	WM_WINDOWPOSCHANGING = 0x0046,

	WM_WINDOWPOSCHANGED = 0x0047,

	// https://docs.microsoft.com/en-us/windows/win32/power/wm-power
	[Obsolete]
	WM_POWER = 0x0048,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-copydata
	WM_COPYDATA = 0x004A,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-canceljournal
	WM_CANCELJOURNAL = 0x004B,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-notify
	WM_NOTIFY = 0x004E,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-inputlangchangerequest
	WM_INPUTLANGCHANGEREQUEST = 0x0050,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-inputlangchange
	WM_INPUTLANGCHANGE = 0x0051,

	// https://docs.microsoft.com/en-us/windows/win32/shell/wm-tcard
	WM_TCARD = 0x0052,

	// https://docs.microsoft.com/en-us/windows/win32/shell/wm-help
	WM_HELP = 0x0053,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-userchanged
	WM_USERCHANGED = 0x0054,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-notifyformat
	WM_NOTIFYFORMAT = 0x0055,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-contextmenu
	WM_CONTEXTMENU = 0x007B,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-stylechanging
	WM_STYLECHANGING = 0x007C,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-stylechanged
	WM_STYLECHANGED = 0x007D,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-displaychange
	WM_DISPLAYCHANGE = 0x007E,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-geticon
	WM_GETICON = 0x007F,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-seticon
	WM_SETICON = 0x0080,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-nccreate
	WM_NCCREATE = 0x0081,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-ncdestroy
	WM_NCDESTROY = 0x0082,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-nccalcsize
	WM_NCCALCSIZE = 0x0083,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest
	WM_NCHITTEST = 0x0084,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-ncpaint
	WM_NCPAINT = 0x0085,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-ncactivate
	WM_NCACTIVATE = 0x0086,

	// https://docs.microsoft.com/en-us/windows/win32/dlgbox/wm-getdlgcode
	WM_GETDLGCODE = 0x0087,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-syncpaint
	WM_SYNCPAINT = 0x0088,

	WM_UAHDESTROYWINDOW = 0x0090,

	WM_UAHDRAWMENU = 0x0091,

	WM_UAHDRAWMENUITEM = 0x0092,

	WM_UAHINITMENU = 0x0093,

	WM_UAHMEASUREMENUITEM = 0x0094,

	WM_UAHNCPAINTMENUPOPUP = 0x0095,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmousemove
	WM_NCMOUSEMOVE = 0x00A0,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nclbuttondown
	WM_NCLBUTTONDOWN = 0x00A1,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nclbuttonup
	WM_NCLBUTTONUP = 0x00A2,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-nclbuttondblclk
	WM_NCLBUTTONDBLCLK = 0x00A3,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncrbuttondown
	WM_NCRBUTTONDOWN = 0x00A4,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncrbuttonup
	WM_NCRBUTTONUP = 0x00A5,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncrbuttondblclk
	WM_NCRBUTTONDBLCLK = 0x00A6,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmbuttondown
	WM_NCMBUTTONDOWN = 0x00A7,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmbuttonup
	WM_NCMBUTTONUP = 0x00A8,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmbuttondblclk
	WM_NCMBUTTONDBLCLK = 0x00A9,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncxbuttondown
	WM_NCXBUTTONDOWN = 0x00AB,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncxbuttonup
	WM_NCXBUTTONUP = 0x00AC,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncxbuttondblclk
	WM_NCXBUTTONDBLCLK = 0x00AD,

	// https://docs.microsoft.com/en-us/windows/win32/controls/bm-click
	WM_BM_CLICK = 0x00F5,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-input-device-change
	WM_INPUT_DEVICE_CHANGE = 0x00FE,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-input
	WM_INPUT = 0x00FF,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
	WM_KEYFIRST = 0x0100,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keydown
	WM_KEYDOWN = 0x0100,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-keyup
	WM_KEYUP = 0x0101,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-char
	WM_CHAR = 0x0102,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-deadchar
	WM_DEADCHAR = 0x0103,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-syskeydown
	WM_SYSKEYDOWN = 0x0104,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-syskeyup
	WM_SYSKEYUP = 0x0105,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syschar
	WM_SYSCHAR = 0x0106,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-sysdeadchar
	WM_SYSDEADCHAR = 0x0107,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-unichar
	WM_UNICHAR = 0x0109,

	WM_KEYLAST = 0x0109,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-startcomposition
	WM_IME_STARTCOMPOSITION = 0x010D,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-endcomposition
	WM_IME_ENDCOMPOSITION = 0x010E,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-composition
	WM_IME_COMPOSITION = 0x010F,

	WM_IME_KEYLAST = 0x010F,

	// https://docs.microsoft.com/en-us/windows/win32/dlgbox/wm-initdialog
	WM_INITDIALOG = 0x0110,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-command
	WM_COMMAND = 0x0111,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand
	WM_SYSCOMMAND = 0x0112,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-timer
	WM_TIMER = 0x0113,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-hscroll
	WM_HSCROLL = 0x0114,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-vscroll
	WM_VSCROLL = 0x0115,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-initmenu
	WM_INITMENU = 0x0116,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-initmenupopup
	WM_INITMENUPOPUP = 0x0117,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menuselect
	WM_MENUSELECT = 0x011F,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menuchar
	WM_MENUCHAR = 0x0120,

	// https://docs.microsoft.com/en-us/windows/win32/dlgbox/wm-enteridle
	WM_ENTERIDLE = 0x0121,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menurbuttonup
	WM_MENURBUTTONUP = 0x0122,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menudrag
	WM_MENUDRAG = 0x0123,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menugetobject
	WM_MENUGETOBJECT = 0x0124,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-uninitmenupopup
	WM_UNINITMENUPOPUP = 0x0125,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-menucommand
	WM_MENUCOMMAND = 0x0126,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-changeuistate
	WM_CHANGEUISTATE = 0x0127,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-updateuistate
	WM_UPDATEUISTATE = 0x0128,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-queryuistate
	WM_QUERYUISTATE = 0x0129,

	WM_CTLCOLORMSGBOX = 0x0132,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-ctlcoloredit
	WM_CTLCOLOREDIT = 0x0133,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-ctlcolorlistbox
	WM_CTLCOLORLISTBOX = 0x0134,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-ctlcolorbtn
	WM_CTLCOLORBTN = 0x0135,

	// https://docs.microsoft.com/en-us/windows/win32/dlgbox/wm-ctlcolordlg
	WM_CTLCOLORDLG = 0x0136,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-ctlcolorscrollbar
	WM_CTLCOLORSCROLLBAR = 0x0137,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-ctlcolorstatic
	WM_CTLCOLORSTATIC = 0x0138,

	WM_MOUSEFIRST = 0x0200,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove
	WM_MOUSEMOVE = 0x0200,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown
	WM_LBUTTONDOWN = 0x0201,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttonup
	WM_LBUTTONUP = 0x0202,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondblclk
	WM_LBUTTONDBLCLK = 0x0203,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttondown
	WM_RBUTTONDOWN = 0x0204,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttonup
	WM_RBUTTONUP = 0x0205,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-rbuttondblclk
	WM_RBUTTONDBLCLK = 0x0206,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondown
	WM_MBUTTONDOWN = 0x0207,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttonup
	WM_MBUTTONUP = 0x0208,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondblclk
	WM_MBUTTONDBLCLK = 0x0209,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousewheel
	WM_MOUSEWHEEL = 0x020A,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttondown
	WM_XBUTTONDOWN = 0x020B,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttonup
	WM_XBUTTONUP = 0x020C,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttondblclk
	WM_XBUTTONDBLCLK = 0x020D,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousehwheel
	WM_MOUSEHWHEEL = 0x020E,

	WM_MOUSELAST = 0x020E,

	// https://docs.microsoft.com/en-us/windows/win32/inputmsg/wm-parentnotify
	WM_PARENTNOTIFY = 0x0210,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-entermenuloop
	WM_ENTERMENULOOP = 0x0211,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-exitmenuloop
	WM_EXITMENULOOP = 0x0212,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-nextmenu
	WM_NEXTMENU = 0x0213,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-sizing
	WM_SIZING = 0x0214,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-capturechanged
	WM_CAPTURECHANGED = 0x0215,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-moving
	WM_MOVING = 0x0216,

	// https://docs.microsoft.com/en-us/windows/win32/power/wm-powerbroadcast
	WM_POWERBROADCAST = 0x0218,

	// https://docs.microsoft.com/en-us/windows/win32/devio/wm-devicechange
	WM_DEVICECHANGE = 0x0219,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdicreate
	WM_MDICREATE = 0x0220,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdidestroy
	WM_MDIDESTROY = 0x0221,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdiactivate
	WM_MDIACTIVATE = 0x0222,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdirestore
	WM_MDIRESTORE = 0x0223,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdinext
	WM_MDINEXT = 0x0224,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdimaximize
	WM_MDIMAXIMIZE = 0x0225,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mditile
	WM_MDITILE = 0x0226,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdicascade
	WM_MDICASCADE = 0x0227,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdiiconarrange
	WM_MDIICONARRANGE = 0x0228,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdigetactive
	WM_MDIGETACTIVE = 0x0229,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdisetmenu
	WM_MDISETMENU = 0x0230,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-entersizemove
	WM_ENTERSIZEMOVE = 0x0231,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-exitsizemove
	WM_EXITSIZEMOVE = 0x0232,

	// https://docs.microsoft.com/en-us/windows/win32/shell/wm-dropfiles
	WM_DROPFILES = 0x0233,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-mdirefreshmenu
	WM_MDIREFRESHMENU = 0x0234,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-setcontext
	WM_IME_SETCONTEXT = 0x0281,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-notify
	WM_IME_NOTIFY = 0x0282,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-control
	WM_IME_CONTROL = 0x0283,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-compositionfull
	WM_IME_COMPOSITIONFULL = 0x0284,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-select
	WM_IME_SELECT = 0x0285,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-char
	WM_IME_CHAR = 0x0286,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-request
	WM_IME_REQUEST = 0x0288,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-keydown
	WM_IME_KEYDOWN = 0x0290,

	// https://docs.microsoft.com/en-us/windows/win32/intl/wm-ime-keyup
	WM_IME_KEYUP = 0x0291,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousehover
	WM_MOUSEHOVER = 0x02A1,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mouseleave
	WM_MOUSELEAVE = 0x02A3,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmousehover
	WM_NCMOUSEHOVER = 0x02A0,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmouseleave
	WM_NCMOUSELEAVE = 0x02A2,

	// https://docs.microsoft.com/en-us/windows/win32/termserv/wm-wtssession-change
	WM_WTSSESSION_CHANGE = 0x02B1,

	WM_TABLET_LAST = 0x02df,

	// https://docs.microsoft.com/en-us/windows/win32/hidpi/wm-dpichanged
	WM_DPICHANGED = 0x02E0,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-cut
	WM_CUT = 0x0300,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-copy
	WM_COPY = 0x0301,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-paste
	WM_PASTE = 0x0302,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-clear
	WM_CLEAR = 0x0303,

	// https://docs.microsoft.com/en-us/windows/win32/controls/wm-undo
	WM_UNDO = 0x0304,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-renderformat
	WM_RENDERFORMAT = 0x0305,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-renderallformats
	WM_RENDERALLFORMATS = 0x0306,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-destroyclipboard
	WM_DESTROYCLIPBOARD = 0x0307,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-drawclipboard
	WM_DRAWCLIPBOARD = 0x0308,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-paintclipboard
	WM_PAINTCLIPBOARD = 0x0309,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-vscrollclipboard
	WM_VSCROLLCLIPBOARD = 0x030A,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-sizeclipboard
	WM_SIZECLIPBOARD = 0x030B,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-askcbformatname
	WM_ASKCBFORMATNAME = 0x030C,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-changecbchain
	WM_CHANGECBCHAIN = 0x030D,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-hscrollclipboard
	WM_HSCROLLCLIPBOARD = 0x030E,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-querynewpalette
	WM_QUERYNEWPALETTE = 0x030F,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-paletteischanging
	WM_PALETTEISCHANGING = 0x0310,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-palettechanged
	WM_PALETTECHANGED = 0x0311,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
	WM_HOTKEY = 0x0312,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-print
	WM_PRINT = 0x0317,

	// https://docs.microsoft.com/en-us/windows/win32/gdi/wm-printclient
	WM_PRINTCLIENT = 0x0318,

	// https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-appcommand
	WM_APPCOMMAND = 0x0319,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-themechanged
	WM_THEMECHANGED = 0x031A,

	// https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-clipboardupdate
	WM_CLIPBOARDUPDATE = 0x031D,

	// https://docs.microsoft.com/en-us/windows/win32/dwm/wm-dwmcompositionchanged
	WM_DWMCOMPOSITIONCHANGED = 0x031E,

	// https://docs.microsoft.com/en-us/windows/win32/dwm/wm-dwmncrenderingchanged
	WM_DWMNCRENDERINGCHANGED = 0x031F,

	// https://docs.microsoft.com/en-us/windows/win32/dwm/wm-dwmcolorizationcolorchanged
	WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,

	// https://docs.microsoft.com/en-us/windows/win32/dwm/wm-dwmwindowmaximizedchange
	WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

	// https://docs.microsoft.com/en-us/windows/win32/menurc/wm-gettitlebarinfoex
	WM_GETTITLEBARINFOEX = 0x033F,

	WM_HANDHELDFIRST = 0x0358,

	WM_HANDHELDLAST = 0x035F,

	WM_AFXFIRST = 0x0360,

	WM_AFXLAST = 0x037F,

	WM_PENWINFIRST = 0x0380,

	WM_PENWINLAST = 0x038F,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-app
	WM_APP = 0x8000,

	// https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-user
	WM_USER = 0x0400,

	WM_CPL_LAUNCH = WM_USER + 0x1000,

	WM_CPL_LAUNCHED = WM_USER + 0x1001,

	WM_REFLECT = WM_USER + 0x1C00,

	WM_SYSTIMER = 0x118,
}