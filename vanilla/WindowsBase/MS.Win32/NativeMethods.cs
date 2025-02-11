using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using MS.Internal;
using MS.Internal.WindowsBase;
using MS.Utility;

namespace MS.Win32;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class NativeMethods
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class XFORM
	{
		public float eM11;

		public float eM12;

		public float eM21;

		public float eM22;

		public float eDx;

		public float eDy;

		public XFORM()
		{
			eM11 = (eM22 = 1f);
		}

		public XFORM(float em11, float em12, float em21, float em22, float edx, float edy)
		{
			eM11 = em11;
			eM12 = em12;
			eM21 = em21;
			eM22 = em22;
			eDx = edx;
			eDy = edy;
		}

		public XFORM(float[] elements)
		{
			eM11 = elements[0];
			eM12 = elements[1];
			eM21 = elements[2];
			eM22 = elements[3];
			eDx = elements[4];
			eDy = elements[5];
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "[{0}, {1}, {2}, {3}, {4}, {5}]", eM11, eM12, eM21, eM22, eDx, eDy);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is XFORM xFORM))
			{
				return false;
			}
			if (eM11 == xFORM.eM11 && eM12 == xFORM.eM12 && eM21 == xFORM.eM21 && eM22 == xFORM.eM22 && eDx == xFORM.eDx)
			{
				return eDy == xFORM.eDy;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
	}

	public struct CANDIDATEFORM
	{
		public int dwIndex;

		public int dwStyle;

		public POINT ptCurrentPos;

		public RECT rcArea;
	}

	public struct COMPOSITIONFORM
	{
		public int dwStyle;

		public POINT ptCurrentPos;

		public RECT rcArea;
	}

	public struct RECONVERTSTRING
	{
		public int dwSize;

		public int dwVersion;

		public int dwStrLen;

		public int dwStrOffset;

		public int dwCompStrLen;

		public int dwCompStrOffset;

		public int dwTargetStrLen;

		public int dwTargetStrOffset;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct REGISTERWORD
	{
		public string lpReading;

		public string lpWord;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	public class MONITORINFOEX
	{
		internal int cbSize = SizeOf();

		internal RECT rcMonitor;

		internal RECT rcWork;

		internal int dwFlags;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		internal char[] szDevice = new char[32];

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(MONITORINFOEX));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class TRACKMOUSEEVENT
	{
		public int cbSize = SizeOf();

		public int dwFlags;

		public nint hwndTrack = IntPtr.Zero;

		public int dwHoverTime = 100;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
		}
	}

	public struct POINT
	{
		public int x;

		public int y;

		public POINT(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public delegate nint WndProc(nint hWnd, int msg, nint wParam, nint lParam);

	public struct MARGINS
	{
		public int cxLeftWidth;

		public int cxRightWidth;

		public int cyTopHeight;

		public int cyBottomHeight;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class NONCLIENTMETRICS
	{
		public int cbSize = SizeOf();

		public int iBorderWidth;

		public int iScrollWidth;

		public int iScrollHeight;

		public int iCaptionWidth;

		public int iCaptionHeight;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfCaptionFont;

		public int iSmCaptionWidth;

		public int iSmCaptionHeight;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfSmCaptionFont;

		public int iMenuWidth;

		public int iMenuHeight;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfMenuFont;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfStatusFont;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfMessageFont;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(NONCLIENTMETRICS));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ICONMETRICS
	{
		public int cbSize = SizeOf();

		public int iHorzSpacing;

		public int iVertSpacing;

		public int iTitleWrap;

		[MarshalAs(UnmanagedType.Struct)]
		public LOGFONT lfFont;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(ICONMETRICS));
		}
	}

	public struct PAINTSTRUCT
	{
		public nint hdc;

		public bool fErase;

		public int rcPaint_left;

		public int rcPaint_top;

		public int rcPaint_right;

		public int rcPaint_bottom;

		public bool fRestore;

		public bool fIncUpdate;

		public int reserved1;

		public int reserved2;

		public int reserved3;

		public int reserved4;

		public int reserved5;

		public int reserved6;

		public int reserved7;

		public int reserved8;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class SIZE
	{
		public int cx;

		public int cy;

		public SIZE()
		{
		}

		public SIZE(int cx, int cy)
		{
			this.cx = cx;
			this.cy = cy;
		}
	}

	public struct WINDOWPLACEMENT
	{
		public int length;

		public int flags;

		public int showCmd;

		public int ptMinPosition_x;

		public int ptMinPosition_y;

		public int ptMaxPosition_x;

		public int ptMaxPosition_y;

		public int rcNormalPosition_left;

		public int rcNormalPosition_top;

		public int rcNormalPosition_right;

		public int rcNormalPosition_bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class BITMAP
	{
		public int bmType;

		public int bmWidth;

		public int bmHeight;

		public int bmWidthBytes;

		public short bmPlanes;

		public short bmBitsPixel;

		public int bmBits;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LOGFONT
	{
		public int lfHeight;

		public int lfWidth;

		public int lfEscapement;

		public int lfOrientation;

		public int lfWeight;

		public byte lfItalic;

		public byte lfUnderline;

		public byte lfStrikeOut;

		public byte lfCharSet;

		public byte lfOutPrecision;

		public byte lfClipPrecision;

		public byte lfQuality;

		public byte lfPitchAndFamily;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName;

		public LOGFONT()
		{
		}

		public LOGFONT(LOGFONT lf)
		{
			if (lf == null)
			{
				throw new ArgumentNullException("lf");
			}
			lfHeight = lf.lfHeight;
			lfWidth = lf.lfWidth;
			lfEscapement = lf.lfEscapement;
			lfOrientation = lf.lfOrientation;
			lfWeight = lf.lfWeight;
			lfItalic = lf.lfItalic;
			lfUnderline = lf.lfUnderline;
			lfStrikeOut = lf.lfStrikeOut;
			lfCharSet = lf.lfCharSet;
			lfOutPrecision = lf.lfOutPrecision;
			lfClipPrecision = lf.lfClipPrecision;
			lfQuality = lf.lfQuality;
			lfPitchAndFamily = lf.lfPitchAndFamily;
			lfFaceName = lf.lfFaceName;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class MENUITEMINFO_T
	{
		public int cbSize = SizeOf();

		public int fMask;

		public int fType;

		public int fState;

		public int wID;

		public nint hSubMenu = IntPtr.Zero;

		public nint hbmpChecked = IntPtr.Zero;

		public nint hbmpUnchecked = IntPtr.Zero;

		public int dwItemData;

		public string dwTypeData;

		public int cch;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(MENUITEMINFO_T));
		}
	}

	public delegate bool EnumThreadWindowsCallback(nint hWnd, nint lParam);

	[StructLayout(LayoutKind.Sequential)]
	internal class OFNOTIFY
	{
		internal nint hdr_hwndFrom;

		internal nint hdr_idFrom;

		internal int hdr_code;

		internal nint lpOFN;

		internal nint pszFile;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class OPENFILENAME_I
	{
		public int lStructSize = SizeOf();

		public nint hwndOwner;

		public nint hInstance;

		public string lpstrFilter;

		public nint lpstrCustomFilter;

		public int nMaxCustFilter;

		public int nFilterIndex;

		public nint lpstrFile;

		public int nMaxFile = 260;

		public nint lpstrFileTitle;

		public int nMaxFileTitle = 260;

		public string lpstrInitialDir;

		public string lpstrTitle;

		public int Flags;

		public short nFileOffset;

		public short nFileExtension;

		public string lpstrDefExt;

		public nint lCustData;

		public WndProc lpfnHook;

		public string lpTemplateName;

		public nint pvReserved;

		public int dwReserved;

		public int FlagsEx;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(OPENFILENAME_I));
		}
	}

	public struct STYLESTRUCT
	{
		public int styleOld;

		public int styleNew;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class STATSTG
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwcsName;

		public int type;

		[MarshalAs(UnmanagedType.I8)]
		public long cbSize;

		[MarshalAs(UnmanagedType.I8)]
		public long mtime;

		[MarshalAs(UnmanagedType.I8)]
		public long ctime;

		[MarshalAs(UnmanagedType.I8)]
		public long atime;

		[MarshalAs(UnmanagedType.I4)]
		public int grfMode;

		[MarshalAs(UnmanagedType.I4)]
		public int grfLocksSupported;

		public int clsid_data1;

		[MarshalAs(UnmanagedType.I2)]
		public short clsid_data2;

		[MarshalAs(UnmanagedType.I2)]
		public short clsid_data3;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b0;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b1;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b2;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b3;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b4;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b5;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b6;

		[MarshalAs(UnmanagedType.U1)]
		public byte clsid_b7;

		[MarshalAs(UnmanagedType.I4)]
		public int grfStateBits;

		[MarshalAs(UnmanagedType.I4)]
		public int reserved;
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public class COMRECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public bool IsEmpty
		{
			get
			{
				if (left == right)
				{
					return top == bottom;
				}
				return false;
			}
		}

		public COMRECT(int x, int y, int right, int bottom)
		{
			left = x;
			top = y;
			this.right = right;
			this.bottom = bottom;
		}

		public COMRECT(RECT rect)
		{
			left = rect.left;
			top = rect.top;
			bottom = rect.bottom;
			right = rect.right;
		}

		public void CopyTo(COMRECT destRect)
		{
			destRect.left = left;
			destRect.right = right;
			destRect.top = top;
			destRect.bottom = bottom;
		}

		public override string ToString()
		{
			return "Left = " + left + " Top " + top + " Right = " + right + " Bottom = " + bottom;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class tagOleMenuGroupWidths
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public int[] widths = new int[6];
	}

	public struct POINTF
	{
		public float x;

		public float y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class OLEINPLACEFRAMEINFO
	{
		[MarshalAs(UnmanagedType.U4)]
		public uint cb;

		public bool fMDIApp;

		public nint hwndFrame;

		public nint hAccel;

		[MarshalAs(UnmanagedType.U4)]
		public uint cAccelEntries;
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class tagOLEVERB
	{
		public int lVerb;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszVerbName;

		[MarshalAs(UnmanagedType.U4)]
		public uint fuFlags;

		[MarshalAs(UnmanagedType.U4)]
		public uint grfAttribs;
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class tagLOGPALETTE
	{
		[MarshalAs(UnmanagedType.U2)]
		public ushort palVersion;

		[MarshalAs(UnmanagedType.U2)]
		public ushort palNumEntries;
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class tagCONTROLINFO
	{
		[MarshalAs(UnmanagedType.U4)]
		public uint cb = (uint)SizeOf();

		public nint hAccel;

		[MarshalAs(UnmanagedType.U2)]
		public ushort cAccel;

		[MarshalAs(UnmanagedType.U4)]
		public uint dwFlags;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(tagCONTROLINFO));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class VARIANT
	{
		[MarshalAs(UnmanagedType.I2)]
		public short vt;

		[MarshalAs(UnmanagedType.I2)]
		public short reserved1;

		[MarshalAs(UnmanagedType.I2)]
		public short reserved2;

		[MarshalAs(UnmanagedType.I2)]
		public short reserved3;

		public SecurityCriticalDataForSet<nint> data1;

		public SecurityCriticalDataForSet<nint> data2;

		public bool Byref => (vt & 0x4000) != 0;

		public void Clear()
		{
			if ((vt == 13 || vt == 9) && data1.Value != IntPtr.Zero)
			{
				Marshal.Release(data1.Value);
			}
			if (vt == 8 && data1.Value != IntPtr.Zero)
			{
				SysFreeString(data1.Value);
			}
			ref SecurityCriticalDataForSet<nint> reference = ref data1;
			nint value = (data2.Value = IntPtr.Zero);
			reference.Value = value;
			vt = 0;
		}

		~VARIANT()
		{
			Clear();
		}

		public void SuppressFinalize()
		{
			GC.SuppressFinalize(this);
		}

		[DllImport("oleaut32.dll", CharSet = CharSet.Auto)]
		private static extern nint SysAllocString([In][MarshalAs(UnmanagedType.LPWStr)] string s);

		[DllImport("oleaut32.dll", CharSet = CharSet.Auto)]
		private static extern void SysFreeString(nint pbstr);

		public void SetLong(long lVal)
		{
			data1.Value = (nint)(lVal & 0xFFFFFFFFu);
			data2.Value = (nint)((lVal >> 32) & 0xFFFFFFFFu);
		}

		public nint ToCoTaskMemPtr()
		{
			nint num = Marshal.AllocCoTaskMem(16);
			Marshal.WriteInt16(num, vt);
			Marshal.WriteInt16(num, 2, reserved1);
			Marshal.WriteInt16(num, 4, reserved2);
			Marshal.WriteInt16(num, 6, reserved3);
			Marshal.WriteInt32(num, 8, (int)data1.Value);
			Marshal.WriteInt32(num, 12, (int)data2.Value);
			return num;
		}

		public object ToObject()
		{
			nint num = data1.Value;
			int num2 = vt & 0xFFF;
			switch (num2)
			{
			case 0:
				return null;
			case 1:
				return Convert.DBNull;
			case 16:
				if (Byref)
				{
					num = Marshal.ReadByte(num);
				}
				return (sbyte)(0xFF & (sbyte)num);
			case 17:
				if (Byref)
				{
					num = Marshal.ReadByte(num);
				}
				return (byte)(0xFF & (byte)num);
			case 2:
				if (Byref)
				{
					num = Marshal.ReadInt16(num);
				}
				return (short)(0xFFFF & (short)num);
			case 18:
				if (Byref)
				{
					num = Marshal.ReadInt16(num);
				}
				return (ushort)(0xFFFF & (ushort)num);
			case 3:
			case 22:
				if (Byref)
				{
					num = Marshal.ReadInt32(num);
				}
				return (int)num;
			case 19:
			case 23:
				if (Byref)
				{
					num = Marshal.ReadInt32(num);
				}
				return (uint)num;
			case 20:
			case 21:
			{
				long ticks = ((!Byref) ? ((uint)(((int)data1.Value & -1) | (int)data2.Value)) : Marshal.ReadInt64(num));
				if (vt == 20)
				{
					return ticks;
				}
				return (ulong)ticks;
			}
			default:
				if (Byref)
				{
					num = GetRefInt(num);
				}
				switch (num2)
				{
				case 4:
				case 5:
					throw new FormatException();
				case 6:
				{
					long ticks = (uint)(((int)data1.Value & -1) | (int)data2.Value);
					return new decimal(ticks);
				}
				case 7:
					throw new FormatException();
				case 8:
				case 31:
					return Marshal.PtrToStringUni(num);
				case 30:
					return Marshal.PtrToStringAnsi(num);
				case 9:
				case 13:
					return Marshal.GetObjectForIUnknown(num);
				case 25:
					return num;
				case 14:
				{
					long ticks = (uint)(((int)data1.Value & -1) | (int)data2.Value);
					return new decimal(ticks);
				}
				case 11:
					return num != IntPtr.Zero;
				case 12:
					return Marshal.PtrToStructure<VARIANT>(num).ToObject();
				case 72:
					return Marshal.PtrToStructure<Guid>(num);
				case 64:
				{
					long ticks = (uint)(((int)data1.Value & -1) | (int)data2.Value);
					return new DateTime(ticks);
				}
				default:
					return null;
				}
			}
		}

		private static nint GetRefInt(nint value)
		{
			return Marshal.ReadIntPtr(value);
		}
	}

	public enum tagVT
	{
		VT_EMPTY = 0,
		VT_NULL = 1,
		VT_I2 = 2,
		VT_I4 = 3,
		VT_R4 = 4,
		VT_R8 = 5,
		VT_CY = 6,
		VT_DATE = 7,
		VT_BSTR = 8,
		VT_DISPATCH = 9,
		VT_ERROR = 10,
		VT_BOOL = 11,
		VT_VARIANT = 12,
		VT_UNKNOWN = 13,
		VT_DECIMAL = 14,
		VT_I1 = 16,
		VT_UI1 = 17,
		VT_UI2 = 18,
		VT_UI4 = 19,
		VT_I8 = 20,
		VT_UI8 = 21,
		VT_INT = 22,
		VT_UINT = 23,
		VT_VOID = 24,
		VT_HRESULT = 25,
		VT_PTR = 26,
		VT_SAFEARRAY = 27,
		VT_CARRAY = 28,
		VT_USERDEFINED = 29,
		VT_LPSTR = 30,
		VT_LPWSTR = 31,
		VT_RECORD = 36,
		VT_FILETIME = 64,
		VT_BLOB = 65,
		VT_STREAM = 66,
		VT_STORAGE = 67,
		VT_STREAMED_OBJECT = 68,
		VT_STORED_OBJECT = 69,
		VT_BLOB_OBJECT = 70,
		VT_CF = 71,
		VT_CLSID = 72,
		VT_BSTR_BLOB = 4095,
		VT_VECTOR = 4096,
		VT_ARRAY = 8192,
		VT_BYREF = 16384,
		VT_RESERVED = 32768,
		VT_ILLEGAL = 65535,
		VT_ILLEGALMASKED = 4095,
		VT_TYPEMASK = 4095
	}

	public delegate void TimerProc(nint hWnd, int msg, nint wParam, nint lParam);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct HIGHCONTRAST_I
	{
		public int cbSize;

		public int dwFlags;

		public nint lpszDefaultScheme;
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class DISPPARAMS
	{
		public nint rgvarg;

		public nint rgdispidNamedArgs;

		[MarshalAs(UnmanagedType.U4)]
		public uint cArgs;

		[MarshalAs(UnmanagedType.U4)]
		public uint cNamedArgs;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class EXCEPINFO
	{
		[MarshalAs(UnmanagedType.U2)]
		public ushort wCode;

		[MarshalAs(UnmanagedType.U2)]
		public ushort wReserved;

		[MarshalAs(UnmanagedType.BStr)]
		public string bstrSource;

		[MarshalAs(UnmanagedType.BStr)]
		public string bstrDescription;

		[MarshalAs(UnmanagedType.BStr)]
		public string bstrHelpFile;

		[MarshalAs(UnmanagedType.U4)]
		public uint dwHelpContext;

		public nint pvReserved;

		public nint pfnDeferredFillIn;

		[MarshalAs(UnmanagedType.I4)]
		public int scode;
	}

	public struct WINDOWPOS
	{
		public nint hwnd;

		public nint hwndInsertAfter;

		public int x;

		public int y;

		public int cx;

		public int cy;

		public int flags;
	}

	public struct MINMAXINFO
	{
		public POINT ptReserved;

		public POINT ptMaxSize;

		public POINT ptMaxPosition;

		public POINT ptMinTrackSize;

		public POINT ptMaxTrackSize;
	}

	internal abstract class CharBuffer
	{
		internal abstract int Length { get; }

		internal static CharBuffer CreateBuffer(int size)
		{
			if (Marshal.SystemDefaultCharSize == 1)
			{
				return new AnsiCharBuffer(size);
			}
			return new UnicodeCharBuffer(size);
		}

		internal abstract nint AllocCoTaskMem();

		internal abstract string GetString();

		internal abstract void PutCoTaskMem(nint ptr);

		internal abstract void PutString(string s);
	}

	internal class AnsiCharBuffer : CharBuffer
	{
		internal byte[] buffer;

		internal int offset;

		internal override int Length => buffer.Length;

		internal AnsiCharBuffer(int size)
		{
			buffer = new byte[size];
		}

		internal override nint AllocCoTaskMem()
		{
			nint num = Marshal.AllocCoTaskMem(buffer.Length);
			Marshal.Copy(buffer, 0, num, buffer.Length);
			return num;
		}

		internal override string GetString()
		{
			int i;
			for (i = offset; i < buffer.Length && buffer[i] != 0; i++)
			{
			}
			string @string = Encoding.Default.GetString(buffer, offset, i - offset);
			if (i < buffer.Length)
			{
				i++;
			}
			offset = i;
			return @string;
		}

		internal override void PutCoTaskMem(nint ptr)
		{
			Marshal.Copy(ptr, buffer, 0, buffer.Length);
			offset = 0;
		}

		internal override void PutString(string s)
		{
			byte[] bytes = Encoding.Default.GetBytes(s);
			int num = Math.Min(bytes.Length, buffer.Length - offset);
			Array.Copy(bytes, 0, buffer, offset, num);
			offset += num;
			if (offset < buffer.Length)
			{
				buffer[offset++] = 0;
			}
		}
	}

	internal class UnicodeCharBuffer : CharBuffer
	{
		internal char[] buffer;

		internal int offset;

		internal override int Length => buffer.Length;

		internal UnicodeCharBuffer(int size)
		{
			buffer = new char[size];
		}

		internal override nint AllocCoTaskMem()
		{
			nint num = Marshal.AllocCoTaskMem(buffer.Length * 2);
			Marshal.Copy(buffer, 0, num, buffer.Length);
			return num;
		}

		internal override string GetString()
		{
			int i;
			for (i = offset; i < buffer.Length && buffer[i] != 0; i++)
			{
			}
			string result = new string(buffer, offset, i - offset);
			if (i < buffer.Length)
			{
				i++;
			}
			offset = i;
			return result;
		}

		internal override void PutCoTaskMem(nint ptr)
		{
			Marshal.Copy(ptr, buffer, 0, buffer.Length);
			offset = 0;
		}

		internal override void PutString(string s)
		{
			int num = Math.Min(s.Length, buffer.Length - offset);
			s.CopyTo(0, buffer, offset, num);
			offset += num;
			if (offset < buffer.Length)
			{
				buffer[offset++] = '\0';
			}
		}
	}

	public static class CommonHandles
	{
		public static readonly int Accelerator;

		public static readonly int Cursor;

		public static readonly int EMF;

		public static readonly int Find;

		public static readonly int GDI;

		public static readonly int HDC;

		public static readonly int Icon;

		public static readonly int Kernel;

		public static readonly int Menu;

		public static readonly int Window;

		static CommonHandles()
		{
			Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);
			Cursor = HandleCollector.RegisterType("Cursor", 20, 500);
			EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);
			Find = HandleCollector.RegisterType("Find", 0, 1000);
			GDI = HandleCollector.RegisterType("GDI", 50, 500);
			HDC = HandleCollector.RegisterType("HDC", 100, 2);
			Icon = HandleCollector.RegisterType("Icon", 20, 500);
			Kernel = HandleCollector.RegisterType("Kernel", 0, 1000);
			Menu = HandleCollector.RegisterType("Menu", 30, 1000);
			Window = HandleCollector.RegisterType("Window", 5, 1000);
		}
	}

	public struct SYSTEM_POWER_STATUS
	{
		public byte ACLineStatus;

		public byte BatteryFlag;

		public byte BatteryLifePercent;

		public byte Reserved1;

		public int BatteryLifeTime;

		public int BatteryFullLifeTime;
	}

	internal delegate void WinEventProcDef(int winEventHook, int eventId, nint hwnd, int idObject, int idChild, int eventThread, int eventTime);

	public enum WebBrowserReadyState
	{
		UnInitialized,
		Loading,
		Loaded,
		Interactive,
		Complete
	}

	public struct RAWINPUTDEVICELIST
	{
		public nint hDevice;

		public uint dwType;
	}

	public struct RID_DEVICE_INFO_MOUSE
	{
		public uint dwId;

		public uint dwNumberOfButtons;

		public uint dwSampleRate;
	}

	public struct RID_DEVICE_INFO_KEYBOARD
	{
		public uint dwType;

		public uint dwSubType;

		public uint dwKeyboardMode;

		public uint dwNumberOfFunctionKeys;

		public uint dwNumberOfIndicators;

		public uint dwNumberOfKeysTotal;
	}

	public struct RID_DEVICE_INFO_HID
	{
		public uint dwVendorId;

		public uint dwProductId;

		public uint dwVersionNumber;

		public ushort usUsagePage;

		public ushort usUsage;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct RID_DEVICE_INFO
	{
		[FieldOffset(0)]
		public uint cbSize;

		[FieldOffset(4)]
		public uint dwType;

		[FieldOffset(8)]
		public RID_DEVICE_INFO_MOUSE mouse;

		[FieldOffset(8)]
		public RID_DEVICE_INFO_KEYBOARD keyboard;

		[FieldOffset(8)]
		public RID_DEVICE_INFO_HID hid;
	}

	public struct BLENDFUNCTION
	{
		public byte BlendOp;

		public byte BlendFlags;

		public byte SourceConstantAlpha;

		public byte AlphaFormat;
	}

	public enum WTS_INFO_CLASS
	{
		WTSInitialProgram,
		WTSApplicationName,
		WTSWorkingDirectory,
		WTSOEMId,
		WTSSessionId,
		WTSUserName,
		WTSWinStationName,
		WTSDomainName,
		WTSConnectState,
		WTSClientBuildNumber,
		WTSClientName,
		WTSClientDirectory,
		WTSClientProductId,
		WTSClientHardwareId,
		WTSClientAddress,
		WTSClientDisplay,
		WTSClientProtocolType,
		WTSIdleTime,
		WTSLogonTime,
		WTSIncomingBytes,
		WTSOutgoingBytes,
		WTSIncomingFrames,
		WTSOutgoingFrames,
		WTSClientInfo,
		WTSSessionInfo,
		WTSSessionInfoEx,
		WTSConfigInfo,
		WTSValidationInfo,
		WTSSessionAddressV4,
		WTSIsRemoteSession
	}

	public enum WTS_CONNECTSTATE_CLASS
	{
		WTSActive,
		WTSConnected,
		WTSConnectQuery,
		WTSShadow,
		WTSDisconnected,
		WTSIdle,
		WTSListen,
		WTSReset,
		WTSDown,
		WTSInit
	}

	internal enum PROCESS_DPI_AWARENESS
	{
		PROCESS_DPI_UNAWARE,
		PROCESS_SYSTEM_DPI_AWARE,
		PROCESS_PER_MONITOR_DPI_AWARE
	}

	internal enum DPI_AWARENESS
	{
		DPI_AWARENESS_INVALID = -1,
		DPI_AWARENESS_UNAWARE,
		DPI_AWARENESS_SYSTEM_AWARE,
		DPI_AWARENESS_PER_MONITOR_AWARE
	}

	internal enum DPI_HOSTING_BEHAVIOR
	{
		DPI_HOSTING_BEHAVIOR_INVALID = -1,
		DPI_HOSTING_BEHAVIOR_DEFAULT,
		DPI_HOSTING_BEHAVIOR_MIXED
	}

	[return: MarshalAs(UnmanagedType.Bool)]
	internal delegate bool MonitorEnumProc(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData);

	internal enum MONITOR_DPI_TYPE
	{
		MDT_EFFECTIVE_DPI = 0,
		MDT_ANGULAR_DPI = 1,
		MDT_RAW_DPI = 2,
		MDT_DEFAULT = 0
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal sealed class OSVERSIONINFOEX
	{
		public int osVersionInfoSize = SizeOf();

		public int majorVersion;

		public int minorVersion;

		public int buildNumber;

		public int platformId;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string csdVersion;

		public short servicePackMajor;

		public short servicePackMinor;

		public short suiteMask;

		public byte productType;

		public byte reserved;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(OSVERSIONINFOEX));
		}
	}

	[ComImport]
	[Guid("79eac9ee-baf9-11ce-8c82-00aa004ba90b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IInternetSecurityMgrSite
	{
		void GetWindow(ref nint phwnd);

		void EnableModeless(bool fEnable);
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class OLECMD
	{
		[MarshalAs(UnmanagedType.U4)]
		public int cmdID;

		[MarshalAs(UnmanagedType.U4)]
		public int cmdf;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class GUID
	{
		public Guid guid;

		public GUID(Guid guid)
		{
			this.guid = guid;
		}
	}

	[ComImport]
	[ComVisible(true)]
	[Guid("B722BCCB-4E68-101B-A2BC-00AA00404770")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IOleCommandTarget
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.I4)]
		int QueryStatus(GUID pguidCmdGroup, int cCmds, [In][Out] OLECMD prgCmds, [In][Out] nint pCmdText);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.I4)]
		int Exec(GUID pguidCmdGroup, int nCmdID, int nCmdexecopt, [In][MarshalAs(UnmanagedType.LPArray)] object[] pvaIn, int pvaOut);
	}

	[StructLayout(LayoutKind.Sequential)]
	[ComVisible(true)]
	internal class DOCHOSTUIINFO
	{
		[MarshalAs(UnmanagedType.U4)]
		internal int cbSize = SizeOf();

		[MarshalAs(UnmanagedType.I4)]
		internal int dwFlags;

		[MarshalAs(UnmanagedType.I4)]
		internal int dwDoubleClick;

		[MarshalAs(UnmanagedType.I4)]
		internal int dwReserved1;

		[MarshalAs(UnmanagedType.I4)]
		internal int dwReserved2;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(DOCHOSTUIINFO));
		}
	}

	public enum DOCHOSTUIFLAG
	{
		DIALOG = 1,
		DISABLE_HELP_MENU = 2,
		NO3DBORDER = 4,
		SCROLL_NO = 8,
		DISABLE_SCRIPT_INACTIVE = 16,
		OPENNEWWIN = 32,
		DISABLE_OFFSCREEN = 64,
		FLAT_SCROLLBAR = 128,
		DIV_BLOCKDEFAULT = 256,
		ACTIVATE_CLIENTHIT_ONLY = 512,
		NO3DOUTERBORDER = 2097152,
		ENABLE_FORMS_AUTOCOMPLETE = 16384,
		ENABLE_INPLACE_NAVIGATION = 65536,
		IME_ENABLE_RECONVERSION = 131072,
		THEME = 262144,
		NOTHEME = 524288,
		DISABLE_COOKIE = 1024,
		NOPICS = 1048576,
		DISABLE_EDIT_NS_FIXUP = 4194304,
		LOCAL_MACHINE_ACCESS_CHECK = 8388608,
		DISABLE_UNTRUSTEDPROTOCOL = 16777216,
		HOST_NAVIGATES = 33554432,
		ENABLE_REDIRECT_NOTIFICATION = 67108864
	}

	public enum DOCHOSTUIDBLCLICK
	{
		DEFAULT,
		SHOWPROPERTIES,
		SHOWCODE
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class ICONINFO
	{
		public bool fIcon;

		public int xHotspot;

		public int yHotspot;

		public BitmapHandle hbmMask;

		public BitmapHandle hbmColor;
	}

	internal abstract class WpfSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private int _collectorId;

		protected WpfSafeHandle(bool ownsHandle, int collectorId)
			: base(ownsHandle)
		{
			HandleCollector.Add(collectorId);
			_collectorId = collectorId;
		}

		protected override void Dispose(bool disposing)
		{
			HandleCollector.Remove(_collectorId);
			base.Dispose(disposing);
		}
	}

	internal sealed class BitmapHandle : WpfSafeHandle
	{
		private BitmapHandle()
			: this(ownsHandle: true)
		{
		}

		private BitmapHandle(bool ownsHandle)
			: base(ownsHandle, CommonHandles.GDI)
		{
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.DeleteObject(handle);
		}

		internal HandleRef MakeHandleRef(object obj)
		{
			return new HandleRef(obj, handle);
		}

		internal static BitmapHandle CreateFromHandle(nint hbitmap, bool ownsHandle = true)
		{
			return new BitmapHandle(ownsHandle)
			{
				handle = hbitmap
			};
		}
	}

	internal sealed class IconHandle : WpfSafeHandle
	{
		private IconHandle()
			: base(ownsHandle: true, CommonHandles.Icon)
		{
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.DestroyIcon(handle);
		}

		internal static IconHandle GetInvalidIcon()
		{
			return new IconHandle();
		}

		internal nint CriticalGetHandle()
		{
			return handle;
		}
	}

	internal sealed class CursorHandle : WpfSafeHandle
	{
		private CursorHandle()
			: base(ownsHandle: true, CommonHandles.Cursor)
		{
		}

		protected override bool ReleaseHandle()
		{
			return UnsafeNativeMethods.DestroyCursor(handle);
		}

		internal static CursorHandle GetInvalidCursor()
		{
			return new CursorHandle();
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class WNDCLASSEX_I
	{
		public int cbSize;

		public int style;

		public nint lpfnWndProc = IntPtr.Zero;

		public int cbClsExtra;

		public int cbWndExtra;

		public nint hInstance = IntPtr.Zero;

		public nint hIcon = IntPtr.Zero;

		public nint hCursor = IntPtr.Zero;

		public nint hbrBackground = IntPtr.Zero;

		public nint lpszMenuName = IntPtr.Zero;

		public nint lpszClassName = IntPtr.Zero;

		public nint hIconSm = IntPtr.Zero;
	}

	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public int Width => right - left;

		public int Height => bottom - top;

		public bool IsEmpty
		{
			get
			{
				if (left < right)
				{
					return top >= bottom;
				}
				return true;
			}
		}

		public RECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public void Offset(int dx, int dy)
		{
			left += dx;
			top += dy;
			right += dx;
			bottom += dy;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class RefRECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public int Width => right - left;

		public int Height => bottom - top;

		public bool IsEmpty
		{
			get
			{
				if (left < right)
				{
					return top >= bottom;
				}
				return true;
			}
		}

		public RefRECT()
		{
		}

		public RefRECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public void Offset(int dx, int dy)
		{
			left += dx;
			top += dy;
			right += dx;
			bottom += dy;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct BITMAPINFO
	{
		public int bmiHeader_biSize;

		public int bmiHeader_biWidth;

		public int bmiHeader_biHeight;

		public short bmiHeader_biPlanes;

		public short bmiHeader_biBitCount;

		public int bmiHeader_biCompression;

		public int bmiHeader_biSizeImage;

		public int bmiHeader_biXPelsPerMeter;

		public int bmiHeader_biYPelsPerMeter;

		public int bmiHeader_biClrUsed;

		public int bmiHeader_biClrImportant;

		public BITMAPINFO(int width, int height, short bpp)
		{
			bmiHeader_biSize = SizeOf();
			bmiHeader_biWidth = width;
			bmiHeader_biHeight = height;
			bmiHeader_biPlanes = 1;
			bmiHeader_biBitCount = bpp;
			bmiHeader_biCompression = 0;
			bmiHeader_biSizeImage = 0;
			bmiHeader_biXPelsPerMeter = 0;
			bmiHeader_biYPelsPerMeter = 0;
			bmiHeader_biClrUsed = 0;
			bmiHeader_biClrImportant = 0;
		}

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(BITMAPINFO));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal class SECURITY_ATTRIBUTES
	{
		public int nLength = SizeOf();

		public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle();

		public bool bInheritHandle;

		public SECURITY_ATTRIBUTES()
		{
			lpSecurityDescriptor = new SafeLocalMemHandle();
		}

		public void Release()
		{
			if (lpSecurityDescriptor != null)
			{
				lpSecurityDescriptor.Dispose();
				lpSecurityDescriptor = new SafeLocalMemHandle();
			}
		}

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
		}
	}

	internal sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public SafeLocalMemHandle()
			: base(ownsHandle: true)
		{
		}

		public SafeLocalMemHandle(nint existingHandle, bool ownsHandle)
			: base(ownsHandle)
		{
			SetHandle(existingHandle);
		}

		protected override bool ReleaseHandle()
		{
			return LocalFree(handle) == IntPtr.Zero;
		}

		[DllImport("kernel32.dll")]
		private static extern nint LocalFree(nint hMem);
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class WNDCLASSEX_D
	{
		public int cbSize;

		public int style;

		public WndProc lpfnWndProc;

		public int cbClsExtra;

		public int cbWndExtra;

		public nint hInstance = IntPtr.Zero;

		public nint hIcon = IntPtr.Zero;

		public nint hCursor = IntPtr.Zero;

		public nint hbrBackground = IntPtr.Zero;

		public string lpszMenuName;

		public string lpszClassName;

		public nint hIconSm = IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class ANIMATIONINFO
	{
		public int cbSize = SizeOf();

		public int iMinAnimate;

		private static int SizeOf()
		{
			return Marshal.SizeOf(typeof(ANIMATIONINFO));
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class STATDATA
	{
		[MarshalAs(UnmanagedType.U4)]
		public int advf;

		[MarshalAs(UnmanagedType.U4)]
		public int dwConnection;
	}

	public enum WINDOWTHEMEATTRIBUTETYPE
	{
		WTA_NONCLIENT = 1
	}

	public struct HWND
	{
		public nint h;

		public static HWND Cast(nint h)
		{
			HWND result = default(HWND);
			result.h = h;
			return result;
		}

		public HandleRef MakeHandleRef(object wrapper)
		{
			return new HandleRef(wrapper, h);
		}

		public static implicit operator nint(HWND h)
		{
			return h.h;
		}

		public static bool operator ==(HWND hl, HWND hr)
		{
			return hl.h == hr.h;
		}

		public static bool operator !=(HWND hl, HWND hr)
		{
			return hl.h != hr.h;
		}

		public override bool Equals(object oCompare)
		{
			HWND hWND = Cast((nint)(HWND)oCompare);
			return h == hWND.h;
		}

		public override int GetHashCode()
		{
			return (int)h;
		}
	}

	public struct HDC
	{
		public nint h;

		public static HDC NULL
		{
			get
			{
				HDC result = default(HDC);
				result.h = IntPtr.Zero;
				return result;
			}
		}

		public static HDC Cast(nint h)
		{
			HDC result = default(HDC);
			result.h = h;
			return result;
		}

		public HandleRef MakeHandleRef(object wrapper)
		{
			return new HandleRef(wrapper, h);
		}
	}

	public struct PrinterEscape
	{
		public int cbInput;

		public uint cbOutput;

		public uint opcode;

		public int cbSize;

		public unsafe void* buffer;
	}

	public struct DocInfo
	{
		internal int cbSize;

		internal string lpszName;

		internal string lpszOutput;

		internal string lpszDatatype;

		internal int fwType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct MOUSEMOVEPOINT
	{
		public int x;

		public int y;

		public int time;

		public nint dwExtraInfo;
	}

	internal enum Win32SystemColors
	{
		ActiveBorder = 10,
		ActiveCaption = 2,
		ActiveCaptionText = 9,
		AppWorkspace = 12,
		Control = 15,
		ControlDark = 16,
		ControlDarkDark = 21,
		ControlLight = 22,
		ControlLightLight = 20,
		ControlText = 18,
		Desktop = 1,
		GradientActiveCaption = 27,
		GradientInactiveCaption = 28,
		GrayText = 17,
		Highlight = 13,
		HighlightText = 14,
		HotTrack = 26,
		InactiveBorder = 11,
		InactiveCaption = 3,
		InactiveCaptionText = 19,
		Info = 24,
		InfoText = 23,
		Menu = 4,
		MenuBar = 30,
		MenuHighlight = 29,
		MenuText = 7,
		ScrollBar = 0,
		Window = 5,
		WindowFrame = 6,
		WindowText = 8
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct InternetCacheConfigInfo
	{
		internal uint dwStructSize;

		internal uint dwContainer;

		internal uint dwQuota;

		internal uint dwReserved4;

		[MarshalAs(UnmanagedType.Bool)]
		internal bool fPerUser;

		internal uint dwSyncMode;

		internal uint dwNumCachePaths;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		internal string CachePath;

		internal uint dwCacheSize;

		internal uint dwNormalUsage;

		internal uint dwExemptUsage;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct POWERBROADCAST_SETTING
	{
		public Guid PowerSetting;

		public int DataLength;

		public byte Data;
	}

	public enum ProfileType : uint
	{
		PROFILE_FILENAME = 1u,
		PROFILE_MEMBUFFER
	}

	public enum COLORTYPE : uint
	{
		COLOR_GRAY = 1u,
		COLOR_RGB = 2u,
		COLOR_XYZ = 3u,
		COLOR_Yxy = 4u,
		COLOR_Lab = 5u,
		COLOR_3_CHANNEL = 6u,
		COLOR_CMYK = 7u,
		COLOR_5_CHANNEL = 8u,
		COLOR_6_CHANNEL = 9u,
		COLOR_7_CHANNEL = 10u,
		COLOR_8_CHANNEL = 11u,
		COLOR_NAMED = 12u,
		COLOR_UNDEFINED = 255u
	}

	public enum ColorSpace : uint
	{
		SPACE_XYZ = 1482250784u,
		SPACE_Lab = 1281450528u,
		SPACE_Luv = 1282766368u,
		SPACE_YCbCr = 1497588338u,
		SPACE_Yxy = 1501067552u,
		SPACE_RGB = 1380401696u,
		SPACE_GRAY = 1196573017u,
		SPACE_HSV = 1213421088u,
		SPACE_HLS = 1212961568u,
		SPACE_CMYK = 1129142603u,
		SPACE_CMY = 1129142560u,
		SPACE_2_CHANNEL = 843271250u,
		SPACE_3_CHANNEL = 860048466u,
		SPACE_4_CHANNEL = 876825682u,
		SPACE_5_CHANNEL = 893602898u,
		SPACE_6_CHANNEL = 910380114u,
		SPACE_7_CHANNEL = 927157330u,
		SPACE_8_CHANNEL = 943934546u,
		SPACE_9_CHANNEL = 960711762u,
		SPACE_A_CHANNEL = 1094929490u,
		SPACE_B_CHANNEL = 1111706706u,
		SPACE_C_CHANNEL = 1128483922u,
		SPACE_D_CHANNEL = 1145261138u,
		SPACE_E_CHANNEL = 1162038354u,
		SPACE_F_CHANNEL = 1178815570u,
		SPACE_sRGB = 1934772034u
	}

	public static nint InvalidIntPtr = -1;

	public static nint LPSTR_TEXTCALLBACK = -1;

	public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

	public const int ERROR = 0;

	public const int BITMAPINFO_MAX_COLORSIZE = 256;

	public const int PAGE_READWRITE = 4;

	public const int FILE_MAP_READ = 4;

	public const int APPCOMMAND_BROWSER_BACKWARD = 1;

	public const int APPCOMMAND_BROWSER_FORWARD = 2;

	public const int APPCOMMAND_BROWSER_REFRESH = 3;

	public const int APPCOMMAND_BROWSER_STOP = 4;

	public const int APPCOMMAND_BROWSER_SEARCH = 5;

	public const int APPCOMMAND_BROWSER_FAVORITES = 6;

	public const int APPCOMMAND_BROWSER_HOME = 7;

	public const int APPCOMMAND_VOLUME_MUTE = 8;

	public const int APPCOMMAND_VOLUME_DOWN = 9;

	public const int APPCOMMAND_VOLUME_UP = 10;

	public const int APPCOMMAND_MEDIA_NEXTTRACK = 11;

	public const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;

	public const int APPCOMMAND_MEDIA_STOP = 13;

	public const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;

	public const int APPCOMMAND_LAUNCH_MAIL = 15;

	public const int APPCOMMAND_LAUNCH_MEDIA_SELECT = 16;

	public const int APPCOMMAND_LAUNCH_APP1 = 17;

	public const int APPCOMMAND_LAUNCH_APP2 = 18;

	public const int APPCOMMAND_BASS_DOWN = 19;

	public const int APPCOMMAND_BASS_BOOST = 20;

	public const int APPCOMMAND_BASS_UP = 21;

	public const int APPCOMMAND_TREBLE_DOWN = 22;

	public const int APPCOMMAND_TREBLE_UP = 23;

	public const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 24;

	public const int APPCOMMAND_MICROPHONE_VOLUME_DOWN = 25;

	public const int APPCOMMAND_MICROPHONE_VOLUME_UP = 26;

	public const int APPCOMMAND_HELP = 27;

	public const int APPCOMMAND_FIND = 28;

	public const int APPCOMMAND_NEW = 29;

	public const int APPCOMMAND_OPEN = 30;

	public const int APPCOMMAND_CLOSE = 31;

	public const int APPCOMMAND_SAVE = 32;

	public const int APPCOMMAND_PRINT = 33;

	public const int APPCOMMAND_UNDO = 34;

	public const int APPCOMMAND_REDO = 35;

	public const int APPCOMMAND_COPY = 36;

	public const int APPCOMMAND_CUT = 37;

	public const int APPCOMMAND_PASTE = 38;

	public const int APPCOMMAND_REPLY_TO_MAIL = 39;

	public const int APPCOMMAND_FORWARD_MAIL = 40;

	public const int APPCOMMAND_SEND_MAIL = 41;

	public const int APPCOMMAND_SPELL_CHECK = 42;

	public const int APPCOMMAND_DICTATE_OR_COMMAND_CONTROL_TOGGLE = 43;

	public const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 44;

	public const int APPCOMMAND_CORRECTION_LIST = 45;

	public const int APPCOMMAND_MEDIA_PLAY = 46;

	public const int APPCOMMAND_MEDIA_PAUSE = 47;

	public const int APPCOMMAND_MEDIA_RECORD = 48;

	public const int APPCOMMAND_MEDIA_FAST_FORWARD = 49;

	public const int APPCOMMAND_MEDIA_REWIND = 50;

	public const int APPCOMMAND_MEDIA_CHANNEL_UP = 51;

	public const int APPCOMMAND_MEDIA_CHANNEL_DOWN = 52;

	public const int FAPPCOMMAND_MOUSE = 32768;

	public const int FAPPCOMMAND_KEY = 0;

	public const int FAPPCOMMAND_OEM = 4096;

	public const int FAPPCOMMAND_MASK = 61440;

	public const int BI_RGB = 0;

	public const int BITSPIXEL = 12;

	public const int cmb4 = 1139;

	public const int CS_DBLCLKS = 8;

	public const int CS_DROPSHADOW = 131072;

	public const int CS_SAVEBITS = 2048;

	public const int CF_TEXT = 1;

	public const int CF_BITMAP = 2;

	public const int CF_METAFILEPICT = 3;

	public const int CF_SYLK = 4;

	public const int CF_DIF = 5;

	public const int CF_TIFF = 6;

	public const int CF_OEMTEXT = 7;

	public const int CF_DIB = 8;

	public const int CF_PALETTE = 9;

	public const int CF_PENDATA = 10;

	public const int CF_RIFF = 11;

	public const int CF_WAVE = 12;

	public const int CF_UNICODETEXT = 13;

	public const int CF_ENHMETAFILE = 14;

	public const int CF_HDROP = 15;

	public const int CF_LOCALE = 16;

	public const int CLSCTX_INPROC_SERVER = 1;

	public const int CLSCTX_LOCAL_SERVER = 4;

	public const int CW_USEDEFAULT = int.MinValue;

	public const int CWP_SKIPINVISIBLE = 1;

	public const int COLOR_WINDOW = 5;

	public const int CB_ERR = -1;

	public const int CBN_SELCHANGE = 1;

	public const int CBN_DBLCLK = 2;

	public const int CBN_EDITCHANGE = 5;

	public const int CBN_EDITUPDATE = 6;

	public const int CBN_DROPDOWN = 7;

	public const int CBN_CLOSEUP = 8;

	public const int CBN_SELENDOK = 9;

	public const int CBS_SIMPLE = 1;

	public const int CBS_DROPDOWN = 2;

	public const int CBS_DROPDOWNLIST = 3;

	public const int CBS_OWNERDRAWFIXED = 16;

	public const int CBS_OWNERDRAWVARIABLE = 32;

	public const int CBS_AUTOHSCROLL = 64;

	public const int CBS_HASSTRINGS = 512;

	public const int CBS_NOINTEGRALHEIGHT = 1024;

	public const int CB_GETEDITSEL = 320;

	public const int CB_LIMITTEXT = 321;

	public const int CB_SETEDITSEL = 322;

	public const int CB_ADDSTRING = 323;

	public const int CB_DELETESTRING = 324;

	public const int CB_GETCURSEL = 327;

	public const int CB_GETLBTEXT = 328;

	public const int CB_GETLBTEXTLEN = 329;

	public const int CB_INSERTSTRING = 330;

	public const int CB_RESETCONTENT = 331;

	public const int CB_FINDSTRING = 332;

	public const int CB_SETCURSEL = 334;

	public const int CB_SHOWDROPDOWN = 335;

	public const int CB_GETITEMDATA = 336;

	public const int CB_SETITEMHEIGHT = 339;

	public const int CB_GETITEMHEIGHT = 340;

	public const int CB_GETDROPPEDSTATE = 343;

	public const int CB_FINDSTRINGEXACT = 344;

	public const int CB_SETDROPPEDWIDTH = 352;

	public const int CDRF_DODEFAULT = 0;

	public const int CDRF_NEWFONT = 2;

	public const int CDRF_SKIPDEFAULT = 4;

	public const int CDRF_NOTIFYPOSTPAINT = 16;

	public const int CDRF_NOTIFYITEMDRAW = 32;

	public const int CDRF_NOTIFYSUBITEMDRAW = 32;

	public const int CDDS_PREPAINT = 1;

	public const int CDDS_POSTPAINT = 2;

	public const int CDDS_ITEM = 65536;

	public const int CDDS_SUBITEM = 131072;

	public const int CDDS_ITEMPREPAINT = 65537;

	public const int CDDS_ITEMPOSTPAINT = 65538;

	public const int CDIS_SELECTED = 1;

	public const int CDIS_GRAYED = 2;

	public const int CDIS_DISABLED = 4;

	public const int CDIS_CHECKED = 8;

	public const int CDIS_FOCUS = 16;

	public const int CDIS_DEFAULT = 32;

	public const int CDIS_HOT = 64;

	public const int CDIS_MARKED = 128;

	public const int CDIS_INDETERMINATE = 256;

	public const int CDIS_SHOWKEYBOARDCUES = 512;

	public const int CLR_NONE = -1;

	public const int CLR_DEFAULT = -16777216;

	public const int CCM_SETVERSION = 8199;

	public const int CCM_GETVERSION = 8200;

	public const int CCS_NORESIZE = 4;

	public const int CCS_NOPARENTALIGN = 8;

	public const int CCS_NODIVIDER = 64;

	public const int CBEM_INSERTITEMA = 1025;

	public const int CBEM_GETITEMA = 1028;

	public const int CBEM_SETITEMA = 1029;

	public const int CBEM_INSERTITEMW = 1035;

	public const int CBEM_SETITEMW = 1036;

	public const int CBEM_GETITEMW = 1037;

	public const int CBEN_ENDEDITA = -805;

	public const int CBEN_ENDEDITW = -806;

	public const int CONNECT_E_NOCONNECTION = -2147220992;

	public const int CONNECT_E_CANNOTCONNECT = -2147220990;

	public const int CTRLINFO_EATS_RETURN = 1;

	public const int CTRLINFO_EATS_ESCAPE = 2;

	public const int CSIDL_DESKTOP = 0;

	public const int CSIDL_INTERNET = 1;

	public const int CSIDL_PROGRAMS = 2;

	public const int CSIDL_PERSONAL = 5;

	public const int CSIDL_FAVORITES = 6;

	public const int CSIDL_STARTUP = 7;

	public const int CSIDL_RECENT = 8;

	public const int CSIDL_SENDTO = 9;

	public const int CSIDL_STARTMENU = 11;

	public const int CSIDL_DESKTOPDIRECTORY = 16;

	public const int CSIDL_TEMPLATES = 21;

	public const int CSIDL_APPDATA = 26;

	public const int CSIDL_LOCAL_APPDATA = 28;

	public const int CSIDL_INTERNET_CACHE = 32;

	public const int CSIDL_COOKIES = 33;

	public const int CSIDL_HISTORY = 34;

	public const int CSIDL_COMMON_APPDATA = 35;

	public const int CSIDL_SYSTEM = 37;

	public const int CSIDL_PROGRAM_FILES = 38;

	public const int CSIDL_PROGRAM_FILES_COMMON = 43;

	public const int DUPLICATE = 6;

	public const int DISPID_VALUE = 0;

	public const int DISPID_UNKNOWN = -1;

	public const int DISPID_PROPERTYPUT = -3;

	public const int DISPATCH_METHOD = 1;

	public const int DISPATCH_PROPERTYGET = 2;

	public const int DISPATCH_PROPERTYPUT = 4;

	public const int DISPATCH_PROPERTYPUTREF = 8;

	public const int DV_E_DVASPECT = -2147221397;

	public const int DEFAULT_GUI_FONT = 17;

	public const int DIB_RGB_COLORS = 0;

	public const int DRAGDROP_E_NOTREGISTERED = -2147221248;

	public const int DRAGDROP_E_ALREADYREGISTERED = -2147221247;

	public const int DUPLICATE_SAME_ACCESS = 2;

	public const int DFC_CAPTION = 1;

	public const int DFC_MENU = 2;

	public const int DFC_SCROLL = 3;

	public const int DFC_BUTTON = 4;

	public const int DFCS_CAPTIONCLOSE = 0;

	public const int DFCS_CAPTIONMIN = 1;

	public const int DFCS_CAPTIONMAX = 2;

	public const int DFCS_CAPTIONRESTORE = 3;

	public const int DFCS_CAPTIONHELP = 4;

	public const int DFCS_MENUARROW = 0;

	public const int DFCS_MENUCHECK = 1;

	public const int DFCS_MENUBULLET = 2;

	public const int DFCS_SCROLLUP = 0;

	public const int DFCS_SCROLLDOWN = 1;

	public const int DFCS_SCROLLLEFT = 2;

	public const int DFCS_SCROLLRIGHT = 3;

	public const int DFCS_SCROLLCOMBOBOX = 5;

	public const int DFCS_BUTTONCHECK = 0;

	public const int DFCS_BUTTONRADIO = 4;

	public const int DFCS_BUTTON3STATE = 8;

	public const int DFCS_BUTTONPUSH = 16;

	public const int DFCS_INACTIVE = 256;

	public const int DFCS_PUSHED = 512;

	public const int DFCS_CHECKED = 1024;

	public const int DFCS_FLAT = 16384;

	public const int DT_LEFT = 0;

	public const int DT_RIGHT = 2;

	public const int DT_VCENTER = 4;

	public const int DT_SINGLELINE = 32;

	public const int DT_NOCLIP = 256;

	public const int DT_CALCRECT = 1024;

	public const int DT_NOPREFIX = 2048;

	public const int DT_EDITCONTROL = 8192;

	public const int DT_EXPANDTABS = 64;

	public const int DT_END_ELLIPSIS = 32768;

	public const int DT_RTLREADING = 131072;

	public const int DT_WORDBREAK = 16;

	public const int DCX_WINDOW = 1;

	public const int DCX_CACHE = 2;

	public const int DCX_LOCKWINDOWUPDATE = 1024;

	public const int DI_NORMAL = 3;

	public const int DLGC_WANTARROWS = 1;

	public const int DLGC_WANTTAB = 2;

	public const int DLGC_WANTALLKEYS = 4;

	public const int DLGC_WANTCHARS = 128;

	public const int DTM_GETSYSTEMTIME = 4097;

	public const int DTM_SETSYSTEMTIME = 4098;

	public const int DTM_SETRANGE = 4100;

	public const int DTM_SETFORMATA = 4101;

	public const int DTM_SETFORMATW = 4146;

	public const int DTM_SETMCCOLOR = 4102;

	public const int DTM_SETMCFONT = 4105;

	public const int DTS_UPDOWN = 1;

	public const int DTS_SHOWNONE = 2;

	public const int DTS_LONGDATEFORMAT = 4;

	public const int DTS_TIMEFORMAT = 9;

	public const int DTS_RIGHTALIGN = 32;

	public const int DTN_DATETIMECHANGE = -759;

	public const int DTN_USERSTRINGA = -758;

	public const int DTN_USERSTRINGW = -745;

	public const int DTN_WMKEYDOWNA = -757;

	public const int DTN_WMKEYDOWNW = -744;

	public const int DTN_FORMATA = -756;

	public const int DTN_FORMATW = -743;

	public const int DTN_FORMATQUERYA = -755;

	public const int DTN_FORMATQUERYW = -742;

	public const int DTN_DROPDOWN = -754;

	public const int DTN_CLOSEUP = -753;

	public const int DVASPECT_CONTENT = 1;

	public const int DVASPECT_TRANSPARENT = 32;

	public const int DVASPECT_OPAQUE = 16;

	public const int E_NOTIMPL = -2147467263;

	public const int E_OUTOFMEMORY = -2147024882;

	public const int E_INVALIDARG = -2147024809;

	public const int E_NOINTERFACE = -2147467262;

	public const int E_FAIL = -2147467259;

	public const int E_ABORT = -2147467260;

	public const int E_ACCESSDENIED = -2147024891;

	public const int E_UNEXPECTED = -2147418113;

	public const int INET_E_DEFAULT_ACTION = -2146697199;

	public const int ETO_OPAQUE = 2;

	public const int ETO_CLIPPED = 4;

	public const int EMR_POLYTEXTOUTA = 96;

	public const int EMR_POLYTEXTOUTW = 97;

	public const int EDGE_RAISED = 5;

	public const int EDGE_SUNKEN = 10;

	public const int EDGE_ETCHED = 6;

	public const int EDGE_BUMP = 9;

	public const int ES_LEFT = 0;

	public const int ES_CENTER = 1;

	public const int ES_RIGHT = 2;

	public const int ES_MULTILINE = 4;

	public const int ES_UPPERCASE = 8;

	public const int ES_LOWERCASE = 16;

	public const int ES_AUTOVSCROLL = 64;

	public const int ES_AUTOHSCROLL = 128;

	public const int ES_NOHIDESEL = 256;

	public const int ES_READONLY = 2048;

	public const int ES_PASSWORD = 32;

	public const int EN_CHANGE = 768;

	public const int EN_UPDATE = 1024;

	public const int EN_HSCROLL = 1537;

	public const int EN_VSCROLL = 1538;

	public const int EN_ALIGN_LTR_EC = 1792;

	public const int EN_ALIGN_RTL_EC = 1793;

	public const int EC_LEFTMARGIN = 1;

	public const int EC_RIGHTMARGIN = 2;

	public const int EM_GETSEL = 176;

	public const int EM_SETSEL = 177;

	public const int EM_SCROLL = 181;

	public const int EM_SCROLLCARET = 183;

	public const int EM_GETMODIFY = 184;

	public const int EM_SETMODIFY = 185;

	public const int EM_GETLINECOUNT = 186;

	public const int EM_REPLACESEL = 194;

	public const int EM_GETLINE = 196;

	public const int EM_LIMITTEXT = 197;

	public const int EM_CANUNDO = 198;

	public const int EM_UNDO = 199;

	public const int EM_SETPASSWORDCHAR = 204;

	public const int EM_GETPASSWORDCHAR = 210;

	public const int EM_EMPTYUNDOBUFFER = 205;

	public const int EM_SETREADONLY = 207;

	public const int EM_SETMARGINS = 211;

	public const int EM_POSFROMCHAR = 214;

	public const int EM_CHARFROMPOS = 215;

	public const int EM_LINEFROMCHAR = 201;

	public const int EM_LINEINDEX = 187;

	public const int FNERR_SUBCLASSFAILURE = 12289;

	public const int FNERR_INVALIDFILENAME = 12290;

	public const int FNERR_BUFFERTOOSMALL = 12291;

	public const int GMEM_MOVEABLE = 2;

	public const int GMEM_ZEROINIT = 64;

	public const int GMEM_DDESHARE = 8192;

	public const int GCL_WNDPROC = -24;

	public const int GWL_WNDPROC = -4;

	public const int GWL_HWNDPARENT = -8;

	public const int GWL_STYLE = -16;

	public const int GWL_EXSTYLE = -20;

	public const int GWL_ID = -12;

	public const int GW_HWNDFIRST = 0;

	public const int GW_HWNDLAST = 1;

	public const int GW_HWNDNEXT = 2;

	public const int GW_HWNDPREV = 3;

	public const int GW_OWNER = 4;

	public const int GW_CHILD = 5;

	public const int GMR_VISIBLE = 0;

	public const int GMR_DAYSTATE = 1;

	public const int GDI_ERROR = -1;

	public const int GDTR_MIN = 1;

	public const int GDTR_MAX = 2;

	public const int GDT_VALID = 0;

	public const int GDT_NONE = 1;

	public const int GA_PARENT = 1;

	public const int GA_ROOT = 2;

	public const int GCS_COMPREADSTR = 1;

	public const int GCS_COMPREADATTR = 2;

	public const int GCS_COMPREADCLAUSE = 4;

	public const int GCS_COMPSTR = 8;

	public const int GCS_COMPATTR = 16;

	public const int GCS_COMPCLAUSE = 32;

	public const int GCS_CURSORPOS = 128;

	public const int GCS_DELTASTART = 256;

	public const int GCS_RESULTREADSTR = 512;

	public const int GCS_RESULTREADCLAUSE = 1024;

	public const int GCS_RESULTSTR = 2048;

	public const int GCS_RESULTCLAUSE = 4096;

	public const int ATTR_INPUT = 0;

	public const int ATTR_TARGET_CONVERTED = 1;

	public const int ATTR_CONVERTED = 2;

	public const int ATTR_TARGET_NOTCONVERTED = 3;

	public const int ATTR_INPUT_ERROR = 4;

	public const int ATTR_FIXEDCONVERTED = 5;

	public const int NI_COMPOSITIONSTR = 21;

	public const int IMN_CLOSESTATUSWINDOW = 1;

	public const int IMN_OPENSTATUSWINDOW = 2;

	public const int IMN_CHANGECANDIDATE = 3;

	public const int IMN_CLOSECANDIDATE = 4;

	public const int IMN_OPENCANDIDATE = 5;

	public const int IMN_SETCONVERSIONMODE = 6;

	public const int IMN_SETSENTENCEMODE = 7;

	public const int IMN_SETOPENSTATUS = 8;

	public const int IMN_SETCANDIDATEPOS = 9;

	public const int IMN_SETCOMPOSITIONFONT = 10;

	public const int IMN_SETCOMPOSITIONWINDOW = 11;

	public const int IMN_SETSTATUSWINDOWPOS = 12;

	public const int IMN_GUIDELINE = 13;

	public const int IMN_PRIVATE = 14;

	public const int CPS_COMPLETE = 1;

	public const int CPS_CANCEL = 4;

	public const int CFS_DEFAULT = 0;

	public const int CFS_RECT = 1;

	public const int CFS_POINT = 2;

	public const int CFS_FORCE_POSITION = 32;

	public const int CFS_CANDIDATEPOS = 64;

	public const int CFS_EXCLUDE = 128;

	public const int IME_CMODE_ALPHANUMERIC = 0;

	public const int IME_CMODE_NATIVE = 1;

	public const int IME_CMODE_CHINESE = 1;

	public const int IME_CMODE_HANGEUL = 1;

	public const int IME_CMODE_HANGUL = 1;

	public const int IME_CMODE_JAPANESE = 1;

	public const int IME_CMODE_KATAKANA = 2;

	public const int IME_CMODE_LANGUAGE = 3;

	public const int IME_CMODE_FULLSHAPE = 8;

	public const int IME_CMODE_ROMAN = 16;

	public const int IME_CMODE_CHARCODE = 32;

	public const int IME_CMODE_HANJACONVERT = 64;

	public const int IME_CMODE_SOFTKBD = 128;

	public const int IME_CMODE_NOCONVERSION = 256;

	public const int IME_CMODE_EUDC = 512;

	public const int IME_CMODE_SYMBOL = 1024;

	public const int IME_CMODE_FIXED = 2048;

	public const int IME_CMODE_RESERVED = -268435456;

	public const int IME_SMODE_NONE = 0;

	public const int IME_SMODE_PLAURALCLAUSE = 1;

	public const int IME_SMODE_SINGLECONVERT = 2;

	public const int IME_SMODE_AUTOMATIC = 4;

	public const int IME_SMODE_PHRASEPREDICT = 8;

	public const int IME_SMODE_CONVERSATION = 16;

	public const int IME_SMODE_RESERVED = 61440;

	public const int IME_CAND_UNKNOWN = 0;

	public const int IME_CAND_READ = 1;

	public const int IME_CAND_CODE = 2;

	public const int IME_CAND_MEANING = 3;

	public const int IME_CAND_RADICAL = 4;

	public const int IME_CAND_STROKE = 5;

	public const int IMR_COMPOSITIONWINDOW = 1;

	public const int IMR_CANDIDATEWINDOW = 2;

	public const int IMR_COMPOSITIONFONT = 3;

	public const int IMR_RECONVERTSTRING = 4;

	public const int IMR_CONFIRMRECONVERTSTRING = 5;

	public const int IMR_QUERYCHARPOSITION = 6;

	public const int IMR_DOCUMENTFEED = 7;

	public const int IME_CONFIG_GENERAL = 1;

	public const int IME_CONFIG_REGISTERWORD = 2;

	public const int IME_CONFIG_SELECTDICTIONARY = 3;

	public const int IGP_GETIMEVERSION = -4;

	public const int IGP_PROPERTY = 4;

	public const int IGP_CONVERSION = 8;

	public const int IGP_SENTENCE = 12;

	public const int IGP_UI = 16;

	public const int IGP_SETCOMPSTR = 20;

	public const int IGP_SELECT = 24;

	public const int IME_PROP_AT_CARET = 65536;

	public const int IME_PROP_SPECIAL_UI = 131072;

	public const int IME_PROP_CANDLIST_START_FROM_1 = 262144;

	public const int IME_PROP_UNICODE = 524288;

	public const int IME_PROP_COMPLETE_ON_UNSELECT = 1048576;

	public const int HC_ACTION = 0;

	public const int HC_GETNEXT = 1;

	public const int HC_SKIP = 2;

	public const int HTNOWHERE = 0;

	public const int HTCLIENT = 1;

	public const int HTBOTTOM = 15;

	public const int HTTRANSPARENT = -1;

	public const int HTBOTTOMLEFT = 16;

	public const int HTBOTTOMRIGHT = 17;

	public const int HELPINFO_WINDOW = 1;

	public const int HCF_HIGHCONTRASTON = 1;

	public const int HDI_ORDER = 128;

	public const int HDI_WIDTH = 1;

	public const int HDM_GETITEMCOUNT = 4608;

	public const int HDM_INSERTITEMA = 4609;

	public const int HDM_INSERTITEMW = 4618;

	public const int HDM_GETITEMA = 4611;

	public const int HDM_GETITEMW = 4619;

	public const int HDM_SETITEMA = 4612;

	public const int HDM_SETITEMW = 4620;

	public const int HDN_ITEMCHANGINGA = -300;

	public const int HDN_ITEMCHANGINGW = -320;

	public const int HDN_ITEMCHANGEDA = -301;

	public const int HDN_ITEMCHANGEDW = -321;

	public const int HDN_ITEMCLICKA = -302;

	public const int HDN_ITEMCLICKW = -322;

	public const int HDN_ITEMDBLCLICKA = -303;

	public const int HDN_ITEMDBLCLICKW = -323;

	public const int HDN_DIVIDERDBLCLICKA = -305;

	public const int HDN_DIVIDERDBLCLICKW = -325;

	public const int HDN_BEGINTDRAG = -310;

	public const int HDN_BEGINTRACKA = -306;

	public const int HDN_BEGINTRACKW = -326;

	public const int HDN_ENDDRAG = -311;

	public const int HDN_ENDTRACKA = -307;

	public const int HDN_ENDTRACKW = -327;

	public const int HDN_TRACKA = -308;

	public const int HDN_TRACKW = -328;

	public const int HDN_GETDISPINFOA = -309;

	public const int HDN_GETDISPINFOW = -329;

	public static HandleRef HWND_TOP = new HandleRef(null, 0);

	public static HandleRef HWND_BOTTOM = new HandleRef(null, 1);

	public static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));

	public static HandleRef HWND_NOTOPMOST = new HandleRef(null, new IntPtr(-2));

	public const int INPLACE_E_NOTOOLSPACE = -2147221087;

	public const int ICON_SMALL = 0;

	public const int ICON_BIG = 1;

	public const int IDC_ARROW = 32512;

	public const int IDC_IBEAM = 32513;

	public const int IDC_WAIT = 32514;

	public const int IDC_CROSS = 32515;

	public const int IDC_SIZEALL = 32646;

	public const int IDC_SIZENWSE = 32642;

	public const int IDC_SIZENESW = 32643;

	public const int IDC_SIZEWE = 32644;

	public const int IDC_SIZENS = 32645;

	public const int IDC_UPARROW = 32516;

	public const int IDC_NO = 32648;

	public const int IDC_APPSTARTING = 32650;

	public const int IDC_HELP = 32651;

	public const int IMAGE_ICON = 1;

	public const int IMAGE_CURSOR = 2;

	public const int ICC_LISTVIEW_CLASSES = 1;

	public const int ICC_TREEVIEW_CLASSES = 2;

	public const int ICC_BAR_CLASSES = 4;

	public const int ICC_TAB_CLASSES = 8;

	public const int ICC_PROGRESS_CLASS = 32;

	public const int ICC_DATE_CLASSES = 256;

	public const int ILC_MASK = 1;

	public const int ILC_COLOR = 0;

	public const int ILC_COLOR4 = 4;

	public const int ILC_COLOR8 = 8;

	public const int ILC_COLOR16 = 16;

	public const int ILC_COLOR24 = 24;

	public const int ILC_COLOR32 = 32;

	public const int ILC_MIRROR = 8192;

	public const int ILD_NORMAL = 0;

	public const int ILD_TRANSPARENT = 1;

	public const int ILD_MASK = 16;

	public const int ILD_ROP = 64;

	public const int ILP_NORMAL = 0;

	public const int ILP_DOWNLEVEL = 1;

	public const int ILS_NORMAL = 0;

	public const int ILS_GLOW = 1;

	public const int ILS_SHADOW = 2;

	public const int ILS_SATURATE = 4;

	public const int ILS_ALPHA = 8;

	public const int CSC_NAVIGATEFORWARD = 1;

	public const int CSC_NAVIGATEBACK = 2;

	public const int STG_E_CANTSAVE = -2147286781;

	public const int LOGPIXELSX = 88;

	public const int LOGPIXELSY = 90;

	public const int LB_ERR = -1;

	public const int LB_ERRSPACE = -2;

	public const int LBN_SELCHANGE = 1;

	public const int LBN_DBLCLK = 2;

	public const int LB_ADDSTRING = 384;

	public const int LB_INSERTSTRING = 385;

	public const int LB_DELETESTRING = 386;

	public const int LB_RESETCONTENT = 388;

	public const int LB_SETSEL = 389;

	public const int LB_SETCURSEL = 390;

	public const int LB_GETSEL = 391;

	public const int LB_GETCARETINDEX = 415;

	public const int LB_GETCURSEL = 392;

	public const int LB_GETTEXT = 393;

	public const int LB_GETTEXTLEN = 394;

	public const int LB_GETTOPINDEX = 398;

	public const int LB_FINDSTRING = 399;

	public const int LB_GETSELCOUNT = 400;

	public const int LB_GETSELITEMS = 401;

	public const int LB_SETTABSTOPS = 402;

	public const int LB_SETHORIZONTALEXTENT = 404;

	public const int LB_SETCOLUMNWIDTH = 405;

	public const int LB_SETTOPINDEX = 407;

	public const int LB_GETITEMRECT = 408;

	public const int LB_SETITEMHEIGHT = 416;

	public const int LB_GETITEMHEIGHT = 417;

	public const int LB_FINDSTRINGEXACT = 418;

	public const int LB_ITEMFROMPOINT = 425;

	public const int LB_SETLOCALE = 421;

	public const int LWA_ALPHA = 2;

	public const int MEMBERID_NIL = -1;

	public const int MAX_PATH = 260;

	public const int MA_ACTIVATE = 1;

	public const int MA_ACTIVATEANDEAT = 2;

	public const int MA_NOACTIVATE = 3;

	public const int MA_NOACTIVATEANDEAT = 4;

	public const int MM_TEXT = 1;

	public const int MM_ANISOTROPIC = 8;

	public const int MK_LBUTTON = 1;

	public const int MK_RBUTTON = 2;

	public const int MK_SHIFT = 4;

	public const int MK_CONTROL = 8;

	public const int MK_MBUTTON = 16;

	public const int MNC_EXECUTE = 2;

	public const int MNC_SELECT = 3;

	public const int MIIM_STATE = 1;

	public const int MIIM_ID = 2;

	public const int MIIM_SUBMENU = 4;

	public const int MIIM_TYPE = 16;

	public const int MIIM_DATA = 32;

	public const int MIIM_STRING = 64;

	public const int MIIM_BITMAP = 128;

	public const int MIIM_FTYPE = 256;

	public const int MB_OK = 0;

	public const int MF_BYCOMMAND = 0;

	public const int MF_BYPOSITION = 1024;

	public const int MF_ENABLED = 0;

	public const int MF_GRAYED = 1;

	public const int MF_POPUP = 16;

	public const int MF_SYSMENU = 8192;

	public const int MFS_DISABLED = 3;

	public const int MFT_MENUBREAK = 64;

	public const int MFT_SEPARATOR = 2048;

	public const int MFT_RIGHTORDER = 8192;

	public const int MFT_RIGHTJUSTIFY = 16384;

	public const int MDIS_ALLCHILDSTYLES = 1;

	public const int MDITILE_VERTICAL = 0;

	public const int MDITILE_HORIZONTAL = 1;

	public const int MDITILE_SKIPDISABLED = 2;

	public const int MCM_SETMAXSELCOUNT = 4100;

	public const int MCM_SETSELRANGE = 4102;

	public const int MCM_GETMONTHRANGE = 4103;

	public const int MCM_GETMINREQRECT = 4105;

	public const int MCM_SETCOLOR = 4106;

	public const int MCM_SETTODAY = 4108;

	public const int MCM_GETTODAY = 4109;

	public const int MCM_HITTEST = 4110;

	public const int MCM_SETFIRSTDAYOFWEEK = 4111;

	public const int MCM_SETRANGE = 4114;

	public const int MCM_SETMONTHDELTA = 4116;

	public const int MCM_GETMAXTODAYWIDTH = 4117;

	public const int MCHT_TITLE = 65536;

	public const int MCHT_CALENDAR = 131072;

	public const int MCHT_TODAYLINK = 196608;

	public const int MCHT_TITLEBK = 65536;

	public const int MCHT_TITLEMONTH = 65537;

	public const int MCHT_TITLEYEAR = 65538;

	public const int MCHT_TITLEBTNNEXT = 16842755;

	public const int MCHT_TITLEBTNPREV = 33619971;

	public const int MCHT_CALENDARBK = 131072;

	public const int MCHT_CALENDARDATE = 131073;

	public const int MCHT_CALENDARDATENEXT = 16908289;

	public const int MCHT_CALENDARDATEPREV = 33685505;

	public const int MCHT_CALENDARDAY = 131074;

	public const int MCHT_CALENDARWEEKNUM = 131075;

	public const int MCSC_TEXT = 1;

	public const int MCSC_TITLEBK = 2;

	public const int MCSC_TITLETEXT = 3;

	public const int MCSC_MONTHBK = 4;

	public const int MCSC_TRAILINGTEXT = 5;

	public const int MCN_SELCHANGE = -749;

	public const int MCN_GETDAYSTATE = -747;

	public const int MCN_SELECT = -746;

	public const int MCS_DAYSTATE = 1;

	public const int MCS_MULTISELECT = 2;

	public const int MCS_WEEKNUMBERS = 4;

	public const int MCS_NOTODAYCIRCLE = 8;

	public const int MCS_NOTODAY = 16;

	public const int MSAA_MENU_SIG = -1441927155;

	public const int OLECONTF_EMBEDDINGS = 1;

	public const int OLECONTF_LINKS = 2;

	public const int OLECONTF_OTHERS = 4;

	public const int OLECONTF_ONLYUSER = 8;

	public const int OLECONTF_ONLYIFRUNNING = 16;

	public const int OLEMISC_RECOMPOSEONRESIZE = 1;

	public const int OLEMISC_INSIDEOUT = 128;

	public const int OLEMISC_ACTIVATEWHENVISIBLE = 256;

	public const int OLEMISC_ACTSLIKEBUTTON = 4096;

	public const int OLEMISC_SETCLIENTSITEFIRST = 131072;

	public const int OLEIVERB_PRIMARY = 0;

	public const int OLEIVERB_SHOW = -1;

	public const int OLEIVERB_HIDE = -3;

	public const int OLEIVERB_UIACTIVATE = -4;

	public const int OLEIVERB_INPLACEACTIVATE = -5;

	public const int OLEIVERB_DISCARDUNDOSTATE = -6;

	public const int OLEIVERB_PROPERTIES = -7;

	public const int XFORMCOORDS_POSITION = 1;

	public const int XFORMCOORDS_SIZE = 2;

	public const int XFORMCOORDS_HIMETRICTOCONTAINER = 4;

	public const int XFORMCOORDS_CONTAINERTOHIMETRIC = 8;

	public const int OFN_READONLY = 1;

	public const int OFN_OVERWRITEPROMPT = 2;

	public const int OFN_HIDEREADONLY = 4;

	public const int OFN_NOCHANGEDIR = 8;

	public const int OFN_ENABLEHOOK = 32;

	public const int OFN_NOVALIDATE = 256;

	public const int OFN_ALLOWMULTISELECT = 512;

	public const int OFN_PATHMUSTEXIST = 2048;

	public const int OFN_FILEMUSTEXIST = 4096;

	public const int OFN_CREATEPROMPT = 8192;

	public const int OFN_EXPLORER = 524288;

	public const int OFN_NODEREFERENCELINKS = 1048576;

	public const int OFN_ENABLESIZING = 8388608;

	public const int OFN_USESHELLITEM = 16777216;

	public const int PDERR_SETUPFAILURE = 4097;

	public const int PDERR_PARSEFAILURE = 4098;

	public const int PDERR_RETDEFFAILURE = 4099;

	public const int PDERR_LOADDRVFAILURE = 4100;

	public const int PDERR_GETDEVMODEFAIL = 4101;

	public const int PDERR_INITFAILURE = 4102;

	public const int PDERR_NODEVICES = 4103;

	public const int PDERR_NODEFAULTPRN = 4104;

	public const int PDERR_DNDMMISMATCH = 4105;

	public const int PDERR_CREATEICFAILURE = 4106;

	public const int PDERR_PRINTERNOTFOUND = 4107;

	public const int PDERR_DEFAULTDIFFERENT = 4108;

	public const int PD_ALLPAGES = 0;

	public const int PD_SELECTION = 1;

	public const int PD_PAGENUMS = 2;

	public const int PD_NOSELECTION = 4;

	public const int PD_NOPAGENUMS = 8;

	public const int PD_COLLATE = 16;

	public const int PD_PRINTTOFILE = 32;

	public const int PD_PRINTSETUP = 64;

	public const int PD_NOWARNING = 128;

	public const int PD_RETURNDC = 256;

	public const int PD_RETURNIC = 512;

	public const int PD_RETURNDEFAULT = 1024;

	public const int PD_SHOWHELP = 2048;

	public const int PD_ENABLEPRINTHOOK = 4096;

	public const int PD_ENABLESETUPHOOK = 8192;

	public const int PD_ENABLEPRINTTEMPLATE = 16384;

	public const int PD_ENABLESETUPTEMPLATE = 32768;

	public const int PD_ENABLEPRINTTEMPLATEHANDLE = 65536;

	public const int PD_ENABLESETUPTEMPLATEHANDLE = 131072;

	public const int PD_USEDEVMODECOPIES = 262144;

	public const int PD_USEDEVMODECOPIESANDCOLLATE = 262144;

	public const int PD_DISABLEPRINTTOFILE = 524288;

	public const int PD_HIDEPRINTTOFILE = 1048576;

	public const int PD_NONETWORKBUTTON = 2097152;

	public const int PD_CURRENTPAGE = 4194304;

	public const int PD_NOCURRENTPAGE = 8388608;

	public const int PD_EXCLUSIONFLAGS = 16777216;

	public const int PD_USELARGETEMPLATE = 268435456;

	public const int PSD_MINMARGINS = 1;

	public const int PSD_MARGINS = 2;

	public const int PSD_INHUNDREDTHSOFMILLIMETERS = 8;

	public const int PSD_DISABLEMARGINS = 16;

	public const int PSD_DISABLEPRINTER = 32;

	public const int PSD_DISABLEORIENTATION = 256;

	public const int PSD_DISABLEPAPER = 512;

	public const int PSD_SHOWHELP = 2048;

	public const int PSD_ENABLEPAGESETUPHOOK = 8192;

	public const int PSD_NONETWORKBUTTON = 2097152;

	public const int PS_SOLID = 0;

	public const int PS_DOT = 2;

	public const int PLANES = 14;

	public const int PRF_CHECKVISIBLE = 1;

	public const int PRF_NONCLIENT = 2;

	public const int PRF_CLIENT = 4;

	public const int PRF_ERASEBKGND = 8;

	public const int PRF_CHILDREN = 16;

	public const int PM_NOREMOVE = 0;

	public const int PM_REMOVE = 1;

	public const int PM_NOYIELD = 2;

	public const int PBM_SETRANGE = 1025;

	public const int PBM_SETPOS = 1026;

	public const int PBM_SETSTEP = 1028;

	public const int PBM_SETRANGE32 = 1030;

	public const int PBM_SETBARCOLOR = 1033;

	public const int PBM_SETBKCOLOR = 8193;

	public const int PSM_SETTITLEA = 1135;

	public const int PSM_SETTITLEW = 1144;

	public const int PSM_SETFINISHTEXTA = 1139;

	public const int PSM_SETFINISHTEXTW = 1145;

	public const int PATCOPY = 15728673;

	public const int PATINVERT = 5898313;

	public const int QS_KEY = 1;

	public const int QS_MOUSEMOVE = 2;

	public const int QS_MOUSEBUTTON = 4;

	public const int QS_POSTMESSAGE = 8;

	public const int QS_TIMER = 16;

	public const int QS_PAINT = 32;

	public const int QS_SENDMESSAGE = 64;

	public const int QS_HOTKEY = 128;

	public const int QS_ALLPOSTMESSAGE = 256;

	public const int QS_MOUSE = 6;

	public const int QS_INPUT = 7;

	public const int QS_ALLEVENTS = 191;

	public const int QS_ALLINPUT = 255;

	public const int RDW_INVALIDATE = 1;

	public const int RDW_ALLCHILDREN = 128;

	public const int stc4 = 1091;

	public const int SHGFP_TYPE_CURRENT = 0;

	public const int STGM_READ = 0;

	public const int STGM_WRITE = 1;

	public const int STGM_READWRITE = 2;

	public const int STGM_SHARE_EXCLUSIVE = 16;

	public const int STGM_CREATE = 4096;

	public const int STGM_TRANSACTED = 65536;

	public const int STGM_CONVERT = 131072;

	public const int STGM_DELETEONRELEASE = 67108864;

	public const int STGTY_STORAGE = 1;

	public const int STGTY_STREAM = 2;

	public const int STGTY_LOCKBYTES = 3;

	public const int STGTY_PROPERTY = 4;

	public const int STARTF_USESHOWWINDOW = 1;

	public const int SB_HORZ = 0;

	public const int SB_VERT = 1;

	public const int SB_CTL = 2;

	public const int SB_LINEUP = 0;

	public const int SB_LINELEFT = 0;

	public const int SB_LINEDOWN = 1;

	public const int SB_LINERIGHT = 1;

	public const int SB_PAGEUP = 2;

	public const int SB_PAGELEFT = 2;

	public const int SB_PAGEDOWN = 3;

	public const int SB_PAGERIGHT = 3;

	public const int SB_THUMBPOSITION = 4;

	public const int SB_THUMBTRACK = 5;

	public const int SB_LEFT = 6;

	public const int SB_RIGHT = 7;

	public const int SB_ENDSCROLL = 8;

	public const int SB_TOP = 6;

	public const int SB_BOTTOM = 7;

	public const int SIZE_MAXIMIZED = 2;

	public const int ESB_ENABLE_BOTH = 0;

	public const int ESB_DISABLE_BOTH = 3;

	public const int SORT_DEFAULT = 0;

	public const int SUBLANG_DEFAULT = 1;

	public const int SW_HIDE = 0;

	public const int SW_NORMAL = 1;

	public const int SW_SHOWMINIMIZED = 2;

	public const int SW_SHOWMAXIMIZED = 3;

	public const int SW_MAXIMIZE = 3;

	public const int SW_SHOWNOACTIVATE = 4;

	public const int SW_SHOW = 5;

	public const int SW_MINIMIZE = 6;

	public const int SW_SHOWMINNOACTIVE = 7;

	public const int SW_SHOWNA = 8;

	public const int SW_RESTORE = 9;

	public const int SW_MAX = 10;

	public const int SWP_NOSIZE = 1;

	public const int SWP_NOMOVE = 2;

	public const int SWP_NOZORDER = 4;

	public const int SWP_NOACTIVATE = 16;

	public const int SWP_SHOWWINDOW = 64;

	public const int SWP_HIDEWINDOW = 128;

	public const int SWP_DRAWFRAME = 32;

	public const int MB_ICONHAND = 16;

	public const int MB_ICONQUESTION = 32;

	public const int MB_ICONEXCLAMATION = 48;

	public const int MB_ICONASTERISK = 64;

	public const int SW_SCROLLCHILDREN = 1;

	public const int SW_INVALIDATE = 2;

	public const int SW_ERASE = 4;

	public const int SW_SMOOTHSCROLL = 16;

	public const int SC_SIZE = 61440;

	public const int SC_MINIMIZE = 61472;

	public const int SC_MAXIMIZE = 61488;

	public const int SC_CLOSE = 61536;

	public const int SC_KEYMENU = 61696;

	public const int SC_RESTORE = 61728;

	public const int SC_MOVE = 61456;

	public const int SS_LEFT = 0;

	public const int SS_CENTER = 1;

	public const int SS_RIGHT = 2;

	public const int SS_OWNERDRAW = 13;

	public const int SS_NOPREFIX = 128;

	public const int SS_SUNKEN = 4096;

	public const int SBS_HORZ = 0;

	public const int SBS_VERT = 1;

	public const int SIF_RANGE = 1;

	public const int SIF_PAGE = 2;

	public const int SIF_POS = 4;

	public const int SIF_TRACKPOS = 16;

	public const int SIF_ALL = 23;

	public const int SPI_GETFONTSMOOTHING = 74;

	public const int SPI_GETDROPSHADOW = 4132;

	public const int SPI_GETFLATMENU = 4130;

	public const int SPI_GETFONTSMOOTHINGTYPE = 8202;

	public const int SPI_GETFONTSMOOTHINGCONTRAST = 8204;

	public const int SPI_ICONHORIZONTALSPACING = 13;

	public const int SPI_ICONVERTICALSPACING = 24;

	public const int SPI_GETICONMETRICS = 45;

	public const int SPI_GETICONTITLEWRAP = 25;

	public const int SPI_GETICONTITLELOGFONT = 31;

	public const int SPI_GETKEYBOARDCUES = 4106;

	public const int SPI_GETKEYBOARDDELAY = 22;

	public const int SPI_GETKEYBOARDPREF = 68;

	public const int SPI_GETKEYBOARDSPEED = 10;

	public const int SPI_GETMOUSEHOVERWIDTH = 98;

	public const int SPI_GETMOUSEHOVERHEIGHT = 100;

	public const int SPI_GETMOUSEHOVERTIME = 102;

	public const int SPI_GETMOUSESPEED = 112;

	public const int SPI_GETMENUDROPALIGNMENT = 27;

	public const int SPI_GETMENUFADE = 4114;

	public const int SPI_GETMENUSHOWDELAY = 106;

	public const int SPI_GETCOMBOBOXANIMATION = 4100;

	public const int SPI_GETCLIENTAREAANIMATION = 4162;

	public const int SPI_GETGRADIENTCAPTIONS = 4104;

	public const int SPI_GETHOTTRACKING = 4110;

	public const int SPI_GETLISTBOXSMOOTHSCROLLING = 4102;

	public const int SPI_GETMENUANIMATION = 4098;

	public const int SPI_GETSELECTIONFADE = 4116;

	public const int SPI_GETTOOLTIPANIMATION = 4118;

	public const int SPI_GETUIEFFECTS = 4158;

	public const int SPI_GETACTIVEWINDOWTRACKING = 4096;

	public const int SPI_GETACTIVEWNDTRKTIMEOUT = 8194;

	public const int SPI_GETANIMATION = 72;

	public const int SPI_GETBORDER = 5;

	public const int SPI_GETCARETWIDTH = 8198;

	public const int SPI_GETMOUSEVANISH = 4128;

	public const int SPI_GETDRAGFULLWINDOWS = 38;

	public const int SPI_GETNONCLIENTMETRICS = 41;

	public const int SPI_GETWORKAREA = 48;

	public const int SPI_GETHIGHCONTRAST = 66;

	public const int SPI_GETDEFAULTINPUTLANG = 89;

	public const int SPI_GETSNAPTODEFBUTTON = 95;

	public const int SPI_GETWHEELSCROLLLINES = 104;

	public const int SBARS_SIZEGRIP = 256;

	public const int SB_SETTEXTA = 1025;

	public const int SB_SETTEXTW = 1035;

	public const int SB_GETTEXTA = 1026;

	public const int SB_GETTEXTW = 1037;

	public const int SB_GETTEXTLENGTHA = 1027;

	public const int SB_GETTEXTLENGTHW = 1036;

	public const int SB_SETPARTS = 1028;

	public const int SB_SIMPLE = 1033;

	public const int SB_GETRECT = 1034;

	public const int SB_SETICON = 1039;

	public const int SB_SETTIPTEXTA = 1040;

	public const int SB_SETTIPTEXTW = 1041;

	public const int SB_GETTIPTEXTA = 1042;

	public const int SB_GETTIPTEXTW = 1043;

	public const int SBT_OWNERDRAW = 4096;

	public const int SBT_NOBORDERS = 256;

	public const int SBT_POPOUT = 512;

	public const int SBT_RTLREADING = 1024;

	public const int SRCCOPY = 13369376;

	public const int SRCAND = 8913094;

	public const int SRCPAINT = 15597702;

	public const int NOTSRCCOPY = 3342344;

	public const int STATFLAG_DEFAULT = 0;

	public const int STATFLAG_NONAME = 1;

	public const int STATFLAG_NOOPEN = 2;

	public const int STGC_DEFAULT = 0;

	public const int STGC_OVERWRITE = 1;

	public const int STGC_ONLYIFCURRENT = 2;

	public const int STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4;

	public const int STREAM_SEEK_SET = 0;

	public const int STREAM_SEEK_CUR = 1;

	public const int STREAM_SEEK_END = 2;

	public const int S_OK = 0;

	public const int S_FALSE = 1;

	public const int TRANSPARENT = 1;

	public const int OPAQUE = 2;

	public const int TME_HOVER = 1;

	public const int TME_LEAVE = 2;

	public const int TPM_LEFTBUTTON = 0;

	public const int TPM_RIGHTBUTTON = 2;

	public const int TPM_LEFTALIGN = 0;

	public const int TPM_RIGHTALIGN = 8;

	public const int TPM_VERTICAL = 64;

	public const int TV_FIRST = 4352;

	public const int TBSTATE_CHECKED = 1;

	public const int TBSTATE_ENABLED = 4;

	public const int TBSTATE_HIDDEN = 8;

	public const int TBSTATE_INDETERMINATE = 16;

	public const int TBSTYLE_BUTTON = 0;

	public const int TBSTYLE_SEP = 1;

	public const int TBSTYLE_CHECK = 2;

	public const int TBSTYLE_DROPDOWN = 8;

	public const int TBSTYLE_TOOLTIPS = 256;

	public const int TBSTYLE_FLAT = 2048;

	public const int TBSTYLE_LIST = 4096;

	public const int TBSTYLE_EX_DRAWDDARROWS = 1;

	public const int TB_ENABLEBUTTON = 1025;

	public const int TB_ISBUTTONCHECKED = 1034;

	public const int TB_ISBUTTONINDETERMINATE = 1037;

	public const int TB_ADDBUTTONSA = 1044;

	public const int TB_ADDBUTTONSW = 1092;

	public const int TB_INSERTBUTTONA = 1045;

	public const int TB_INSERTBUTTONW = 1091;

	public const int TB_DELETEBUTTON = 1046;

	public const int TB_GETBUTTON = 1047;

	public const int TB_SAVERESTOREA = 1050;

	public const int TB_SAVERESTOREW = 1100;

	public const int TB_ADDSTRINGA = 1052;

	public const int TB_ADDSTRINGW = 1101;

	public const int TB_BUTTONSTRUCTSIZE = 1054;

	public const int TB_SETBUTTONSIZE = 1055;

	public const int TB_AUTOSIZE = 1057;

	public const int TB_GETROWS = 1064;

	public const int TB_GETBUTTONTEXTA = 1069;

	public const int TB_GETBUTTONTEXTW = 1099;

	public const int TB_SETIMAGELIST = 1072;

	public const int TB_GETRECT = 1075;

	public const int TB_GETBUTTONSIZE = 1082;

	public const int TB_GETBUTTONINFOW = 1087;

	public const int TB_SETBUTTONINFOW = 1088;

	public const int TB_GETBUTTONINFOA = 1089;

	public const int TB_SETBUTTONINFOA = 1090;

	public const int TB_MAPACCELERATORA = 1102;

	public const int TB_SETEXTENDEDSTYLE = 1108;

	public const int TB_MAPACCELERATORW = 1114;

	public const int TB_GETTOOLTIPS = 1059;

	public const int TB_SETTOOLTIPS = 1060;

	public const int TBIF_IMAGE = 1;

	public const int TBIF_TEXT = 2;

	public const int TBIF_STATE = 4;

	public const int TBIF_STYLE = 8;

	public const int TBIF_COMMAND = 32;

	public const int TBIF_SIZE = 64;

	public const int TBN_GETBUTTONINFOA = -700;

	public const int TBN_GETBUTTONINFOW = -720;

	public const int TBN_QUERYINSERT = -706;

	public const int TBN_DROPDOWN = -710;

	public const int TBN_HOTITEMCHANGE = -713;

	public const int TBN_GETDISPINFOA = -716;

	public const int TBN_GETDISPINFOW = -717;

	public const int TBN_GETINFOTIPA = -718;

	public const int TBN_GETINFOTIPW = -719;

	public const int TTS_ALWAYSTIP = 1;

	public const int TTS_NOPREFIX = 2;

	public const int TTS_NOANIMATE = 16;

	public const int TTS_NOFADE = 32;

	public const int TTS_BALLOON = 64;

	public const int TTI_WARNING = 2;

	public const int TTF_IDISHWND = 1;

	public const int TTF_RTLREADING = 4;

	public const int TTF_TRACK = 32;

	public const int TTF_CENTERTIP = 2;

	public const int TTF_SUBCLASS = 16;

	public const int TTF_TRANSPARENT = 256;

	public const int TTF_ABSOLUTE = 128;

	public const int TTDT_AUTOMATIC = 0;

	public const int TTDT_RESHOW = 1;

	public const int TTDT_AUTOPOP = 2;

	public const int TTDT_INITIAL = 3;

	public const int TTM_TRACKACTIVATE = 1041;

	public const int TTM_TRACKPOSITION = 1042;

	public const int TTM_ACTIVATE = 1025;

	public const int TTM_POP = 1052;

	public const int TTM_ADJUSTRECT = 1055;

	public const int TTM_SETDELAYTIME = 1027;

	public const int TTM_SETTITLEA = 1056;

	public const int TTM_SETTITLEW = 1057;

	public const int TTM_ADDTOOLA = 1028;

	public const int TTM_ADDTOOLW = 1074;

	public const int TTM_DELTOOLA = 1029;

	public const int TTM_DELTOOLW = 1075;

	public const int TTM_NEWTOOLRECTA = 1030;

	public const int TTM_NEWTOOLRECTW = 1076;

	public const int TTM_RELAYEVENT = 1031;

	public const int TTM_GETTIPBKCOLOR = 1046;

	public const int TTM_SETTIPBKCOLOR = 1043;

	public const int TTM_SETTIPTEXTCOLOR = 1044;

	public const int TTM_GETTIPTEXTCOLOR = 1047;

	public const int TTM_GETTOOLINFOA = 1032;

	public const int TTM_GETTOOLINFOW = 1077;

	public const int TTM_SETTOOLINFOA = 1033;

	public const int TTM_SETTOOLINFOW = 1078;

	public const int TTM_HITTESTA = 1034;

	public const int TTM_HITTESTW = 1079;

	public const int TTM_GETTEXTA = 1035;

	public const int TTM_GETTEXTW = 1080;

	public const int TTM_UPDATE = 1053;

	public const int TTM_UPDATETIPTEXTA = 1036;

	public const int TTM_UPDATETIPTEXTW = 1081;

	public const int TTM_ENUMTOOLSA = 1038;

	public const int TTM_ENUMTOOLSW = 1082;

	public const int TTM_GETCURRENTTOOLA = 1039;

	public const int TTM_GETCURRENTTOOLW = 1083;

	public const int TTM_WINDOWFROMPOINT = 1040;

	public const int TTM_GETDELAYTIME = 1045;

	public const int TTM_SETMAXTIPWIDTH = 1048;

	public const int TTN_GETDISPINFOA = -520;

	public const int TTN_GETDISPINFOW = -530;

	public const int TTN_SHOW = -521;

	public const int TTN_POP = -522;

	public const int TTN_NEEDTEXTA = -520;

	public const int TTN_NEEDTEXTW = -530;

	public const int TBS_AUTOTICKS = 1;

	public const int TBS_VERT = 2;

	public const int TBS_TOP = 4;

	public const int TBS_BOTTOM = 0;

	public const int TBS_BOTH = 8;

	public const int TBS_NOTICKS = 16;

	public const int TBM_GETPOS = 1024;

	public const int TBM_SETTIC = 1028;

	public const int TBM_SETPOS = 1029;

	public const int TBM_SETRANGE = 1030;

	public const int TBM_SETRANGEMIN = 1031;

	public const int TBM_SETRANGEMAX = 1032;

	public const int TBM_SETTICFREQ = 1044;

	public const int TBM_SETPAGESIZE = 1045;

	public const int TBM_SETLINESIZE = 1047;

	public const int TB_LINEUP = 0;

	public const int TB_LINEDOWN = 1;

	public const int TB_PAGEUP = 2;

	public const int TB_PAGEDOWN = 3;

	public const int TB_THUMBPOSITION = 4;

	public const int TB_THUMBTRACK = 5;

	public const int TB_TOP = 6;

	public const int TB_BOTTOM = 7;

	public const int TB_ENDTRACK = 8;

	public const int TVS_HASBUTTONS = 1;

	public const int TVS_HASLINES = 2;

	public const int TVS_LINESATROOT = 4;

	public const int TVS_EDITLABELS = 8;

	public const int TVS_SHOWSELALWAYS = 32;

	public const int TVS_RTLREADING = 64;

	public const int TVS_CHECKBOXES = 256;

	public const int TVS_TRACKSELECT = 512;

	public const int TVS_FULLROWSELECT = 4096;

	public const int TVS_NONEVENHEIGHT = 16384;

	public const int TVS_INFOTIP = 2048;

	public const int TVS_NOTOOLTIPS = 128;

	public const int TVIF_TEXT = 1;

	public const int TVIF_IMAGE = 2;

	public const int TVIF_PARAM = 4;

	public const int TVIF_STATE = 8;

	public const int TVIF_HANDLE = 16;

	public const int TVIF_SELECTEDIMAGE = 32;

	public const int TVIS_SELECTED = 2;

	public const int TVIS_EXPANDED = 32;

	public const int TVIS_EXPANDEDONCE = 64;

	public const int TVIS_STATEIMAGEMASK = 61440;

	public const int TVI_ROOT = -65536;

	public const int TVI_FIRST = -65535;

	public const int TVM_INSERTITEMA = 4352;

	public const int TVM_INSERTITEMW = 4402;

	public const int TVM_DELETEITEM = 4353;

	public const int TVM_EXPAND = 4354;

	public const int TVE_COLLAPSE = 1;

	public const int TVE_EXPAND = 2;

	public const int TVM_GETITEMRECT = 4356;

	public const int TVM_GETINDENT = 4358;

	public const int TVM_SETINDENT = 4359;

	public const int TVM_SETIMAGELIST = 4361;

	public const int TVM_GETNEXTITEM = 4362;

	public const int TVGN_NEXT = 1;

	public const int TVGN_PREVIOUS = 2;

	public const int TVGN_FIRSTVISIBLE = 5;

	public const int TVGN_NEXTVISIBLE = 6;

	public const int TVGN_PREVIOUSVISIBLE = 7;

	public const int TVGN_CARET = 9;

	public const int TVM_SELECTITEM = 4363;

	public const int TVM_GETITEMA = 4364;

	public const int TVM_GETITEMW = 4414;

	public const int TVM_SETITEMA = 4365;

	public const int TVM_SETITEMW = 4415;

	public const int TVM_EDITLABELA = 4366;

	public const int TVM_EDITLABELW = 4417;

	public const int TVM_GETEDITCONTROL = 4367;

	public const int TVM_GETVISIBLECOUNT = 4368;

	public const int TVM_HITTEST = 4369;

	public const int TVM_ENSUREVISIBLE = 4372;

	public const int TVM_ENDEDITLABELNOW = 4374;

	public const int TVM_GETISEARCHSTRINGA = 4375;

	public const int TVM_GETISEARCHSTRINGW = 4416;

	public const int TVM_SETITEMHEIGHT = 4379;

	public const int TVM_GETITEMHEIGHT = 4380;

	public const int TVN_SELCHANGINGA = -401;

	public const int TVN_SELCHANGINGW = -450;

	public const int TVN_GETINFOTIPA = -413;

	public const int TVN_GETINFOTIPW = -414;

	public const int TVN_SELCHANGEDA = -402;

	public const int TVN_SELCHANGEDW = -451;

	public const int TVC_UNKNOWN = 0;

	public const int TVC_BYMOUSE = 1;

	public const int TVC_BYKEYBOARD = 2;

	public const int TVN_GETDISPINFOA = -403;

	public const int TVN_GETDISPINFOW = -452;

	public const int TVN_SETDISPINFOA = -404;

	public const int TVN_SETDISPINFOW = -453;

	public const int TVN_ITEMEXPANDINGA = -405;

	public const int TVN_ITEMEXPANDINGW = -454;

	public const int TVN_ITEMEXPANDEDA = -406;

	public const int TVN_ITEMEXPANDEDW = -455;

	public const int TVN_BEGINDRAGA = -407;

	public const int TVN_BEGINDRAGW = -456;

	public const int TVN_BEGINRDRAGA = -408;

	public const int TVN_BEGINRDRAGW = -457;

	public const int TVN_BEGINLABELEDITA = -410;

	public const int TVN_BEGINLABELEDITW = -459;

	public const int TVN_ENDLABELEDITA = -411;

	public const int TVN_ENDLABELEDITW = -460;

	public const int TCS_BOTTOM = 2;

	public const int TCS_RIGHT = 2;

	public const int TCS_FLATBUTTONS = 8;

	public const int TCS_HOTTRACK = 64;

	public const int TCS_VERTICAL = 128;

	public const int TCS_TABS = 0;

	public const int TCS_BUTTONS = 256;

	public const int TCS_MULTILINE = 512;

	public const int TCS_RIGHTJUSTIFY = 0;

	public const int TCS_FIXEDWIDTH = 1024;

	public const int TCS_RAGGEDRIGHT = 2048;

	public const int TCS_OWNERDRAWFIXED = 8192;

	public const int TCS_TOOLTIPS = 16384;

	public const int TCM_SETIMAGELIST = 4867;

	public const int TCIF_TEXT = 1;

	public const int TCIF_IMAGE = 2;

	public const int TCM_GETITEMA = 4869;

	public const int TCM_GETITEMW = 4924;

	public const int TCM_SETITEMA = 4870;

	public const int TCM_SETITEMW = 4925;

	public const int TCM_INSERTITEMA = 4871;

	public const int TCM_INSERTITEMW = 4926;

	public const int TCM_DELETEITEM = 4872;

	public const int TCM_DELETEALLITEMS = 4873;

	public const int TCM_GETITEMRECT = 4874;

	public const int TCM_GETCURSEL = 4875;

	public const int TCM_SETCURSEL = 4876;

	public const int TCM_ADJUSTRECT = 4904;

	public const int TCM_SETITEMSIZE = 4905;

	public const int TCM_SETPADDING = 4907;

	public const int TCM_GETROWCOUNT = 4908;

	public const int TCM_GETTOOLTIPS = 4909;

	public const int TCM_SETTOOLTIPS = 4910;

	public const int TCN_SELCHANGE = -551;

	public const int TCN_SELCHANGING = -552;

	public const int TBSTYLE_WRAPPABLE = 512;

	public const int TVM_SETBKCOLOR = 4381;

	public const int TVM_SETTEXTCOLOR = 4382;

	public const int TYMED_NULL = 0;

	public const int TVM_GETLINECOLOR = 4393;

	public const int TVM_SETLINECOLOR = 4392;

	public const int TVM_SETTOOLTIPS = 4376;

	public const int TVSIL_STATE = 2;

	public const int TVM_SORTCHILDRENCB = 4373;

	public const int UIS_SET = 1;

	public const int UIS_CLEAR = 2;

	public const int UIS_INITIALIZE = 3;

	public const int UISF_HIDEFOCUS = 1;

	public const int UISF_HIDEACCEL = 2;

	public const int UISF_ACTIVE = 4;

	public const int VK_TAB = 9;

	public const int VK_SHIFT = 16;

	public const int VK_CONTROL = 17;

	public const int VK_MENU = 18;

	public const int WH_JOURNALPLAYBACK = 1;

	public const int WH_GETMESSAGE = 3;

	public const int WH_MOUSE = 7;

	public const int WSF_VISIBLE = 1;

	public const int WA_INACTIVE = 0;

	public const int WA_ACTIVE = 1;

	public const int WA_CLICKACTIVE = 2;

	public const int WHEEL_DELTA = 120;

	public const int WM_REFLECT = 8192;

	public const int WM_CHOOSEFONT_GETLOGFONT = 1025;

	public const int WS_OVERLAPPED = 0;

	public const int WS_POPUP = int.MinValue;

	public const int WS_CHILD = 1073741824;

	public const int WS_MINIMIZE = 536870912;

	public const int WS_VISIBLE = 268435456;

	public const int WS_DISABLED = 134217728;

	public const int WS_CLIPSIBLINGS = 67108864;

	public const int WS_CLIPCHILDREN = 33554432;

	public const int WS_MAXIMIZE = 16777216;

	public const int WS_CAPTION = 12582912;

	public const int WS_BORDER = 8388608;

	public const int WS_DLGFRAME = 4194304;

	public const int WS_VSCROLL = 2097152;

	public const int WS_HSCROLL = 1048576;

	public const int WS_SYSMENU = 524288;

	public const int WS_THICKFRAME = 262144;

	public const int WS_TABSTOP = 65536;

	public const int WS_MINIMIZEBOX = 131072;

	public const int WS_MAXIMIZEBOX = 65536;

	public const int WS_EX_DLGMODALFRAME = 1;

	public const int WS_EX_TRANSPARENT = 32;

	public const int WS_EX_MDICHILD = 64;

	public const int WS_EX_TOOLWINDOW = 128;

	public const int WS_EX_WINDOWEDGE = 256;

	public const int WS_EX_CLIENTEDGE = 512;

	public const int WS_EX_CONTEXTHELP = 1024;

	public const int WS_EX_RIGHT = 4096;

	public const int WS_EX_LEFT = 0;

	public const int WS_EX_RTLREADING = 8192;

	public const int WS_EX_LEFTSCROLLBAR = 16384;

	public const int WS_EX_CONTROLPARENT = 65536;

	public const int WS_EX_STATICEDGE = 131072;

	public const int WS_EX_APPWINDOW = 262144;

	public const int WS_EX_LAYERED = 524288;

	public const int WS_EX_TOPMOST = 8;

	public const int WS_EX_LAYOUTRTL = 4194304;

	public const int WS_EX_NOINHERITLAYOUT = 1048576;

	public const int WS_EX_COMPOSITED = 33554432;

	public const int WPF_SETMINPOSITION = 1;

	public const int WPF_RESTORETOMAXIMIZED = 2;

	public const int WHITE_BRUSH = 0;

	public const int NULL_BRUSH = 5;

	public const int XBUTTON1 = 1;

	public const int XBUTTON2 = 2;

	public const int CDN_FIRST = -601;

	public const int CDN_INITDONE = -601;

	public const int CDN_SELCHANGE = -602;

	public const int CDN_SHAREVIOLATION = -604;

	public const int CDN_FILEOK = -606;

	public const int CDM_FIRST = 1124;

	public const int CDM_GETSPEC = 1124;

	public const int CDM_GETFILEPATH = 1125;

	public const int DWL_MSGRESULT = 0;

	public const int PBT_APMPOWERSTATUSCHANGE = 10;

	public const int EVENT_SYSTEM_MOVESIZESTART = 10;

	public const int EVENT_SYSTEM_MOVESIZEEND = 11;

	public const int EVENT_OBJECT_STATECHANGE = 32778;

	public const int EVENT_OBJECT_FOCUS = 32773;

	public const int OBJID_CLIENT = -4;

	public const int WINEVENT_OUTOFCONTEXT = 0;

	public const uint RIDI_DEVICEINFO = 536870923u;

	public const uint RIM_TYPEHID = 2u;

	public const ushort HID_USAGE_PAGE_DIGITIZER = 13;

	public const ushort HID_USAGE_DIGITIZER_DIGITIZER = 1;

	public const ushort HID_USAGE_DIGITIZER_PEN = 2;

	public const ushort HID_USAGE_DIGITIZER_LIGHTPEN = 3;

	public const ushort HID_USAGE_DIGITIZER_TOUCHSCREEN = 4;

	public const int AC_SRC_OVER = 0;

	public const int ULW_COLORKEY = 1;

	public const int ULW_ALPHA = 2;

	public const int ULW_OPAQUE = 4;

	public static readonly nint WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

	public const int WTS_CURRENT_SESSION = -1;

	internal static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_UNAWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_UNAWARE;

	internal static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE;

	internal static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE;

	internal static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2;

	public const int FEATURE_OBJECT_CACHING = 0;

	public const int FEATURE_ZONE_ELEVATION = 1;

	public const int FEATURE_MIME_HANDLING = 2;

	public const int FEATURE_MIME_SNIFFING = 3;

	public const int FEATURE_WINDOW_RESTRICTIONS = 4;

	public const int FEATURE_WEBOC_POPUPMANAGEMENT = 5;

	public const int FEATURE_BEHAVIORS = 6;

	public const int FEATURE_DISABLE_MK_PROTOCOL = 7;

	public const int FEATURE_LOCALMACHINE_LOCKDOWN = 8;

	public const int FEATURE_SECURITYBAND = 9;

	public const int FEATURE_RESTRICT_ACTIVEXINSTALL = 10;

	public const int FEATURE_VALIDATE_NAVIGATE_URL = 11;

	public const int FEATURE_RESTRICT_FILEDOWNLOAD = 12;

	public const int FEATURE_ADDON_MANAGEMENT = 13;

	public const int FEATURE_PROTOCOL_LOCKDOWN = 14;

	public const int FEATURE_HTTP_USERNAME_PASSWORD_DISABLE = 15;

	public const int FEATURE_SAFE_BINDTOOBJECT = 16;

	public const int FEATURE_UNC_SAVEDFILECHECK = 17;

	public const int FEATURE_GET_URL_DOM_FILEPATH_UNENCODED = 18;

	public const int FEATURE_TABBED_BROWSING = 19;

	public const int FEATURE_SSLUX = 20;

	public const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;

	public const int FEATURE_DISABLE_LEGACY_COMPRESSION = 22;

	public const int FEATURE_FORCE_ADDR_AND_STATUS = 23;

	public const int FEATURE_XMLHTTP = 24;

	public const int FEATURE_DISABLE_TELNET_PROTOCOL = 25;

	public const int FEATURE_FEEDS = 26;

	public const int FEATURE_BLOCK_INPUT_PROMPTS = 27;

	public const int GET_FEATURE_FROM_PROCESS = 2;

	public const int SET_FEATURE_ON_PROCESS = 2;

	public const int URLZONE_LOCAL_MACHINE = 0;

	public const int URLZONE_INTRANET = 1;

	public const int URLZONE_TRUSTED = 2;

	public const int URLZONE_INTERNET = 3;

	public const int URLZONE_UNTRUSTED = 4;

	public const byte URLPOLICY_ALLOW = 0;

	public const byte URLPOLICY_QUERY = 1;

	public const byte URLPOLICY_DISALLOW = 3;

	public const int URLACTION_FEATURE_ZONE_ELEVATION = 8449;

	public const int PUAF_NOUI = 1;

	public const int MUTZ_NOSAVEDFILECHECK = 1;

	public const int SIZE_RESTORED = 0;

	public const int SIZE_MINIMIZED = 1;

	public const int WS_EX_NOACTIVATE = 134217728;

	public const int VK_LSHIFT = 160;

	public const int VK_RMENU = 165;

	public const int VK_LMENU = 164;

	public const int VK_LCONTROL = 162;

	public const int VK_RCONTROL = 163;

	public const int VK_LBUTTON = 1;

	public const int VK_RBUTTON = 2;

	public const int VK_MBUTTON = 4;

	public const int VK_XBUTTON1 = 5;

	public const int VK_XBUTTON2 = 6;

	public const int PM_QS_SENDMESSAGE = 4194304;

	public const int PM_QS_POSTMESSAGE = 9961472;

	public const int MWMO_WAITALL = 1;

	public const int MWMO_ALERTABLE = 2;

	public const int MWMO_INPUTAVAILABLE = 4;

	public static nint HWND_MESSAGE = new IntPtr(-3);

	internal const uint DELETE = 65536u;

	internal const uint READ_CONTROL = 131072u;

	internal const uint WRITE_DAC = 262144u;

	internal const uint WRITE_OWNER = 524288u;

	internal const uint SYNCHRONIZE = 1048576u;

	internal const uint STANDARD_RIGHTS_REQUIRED = 983040u;

	internal const uint STANDARD_RIGHTS_READ = 131072u;

	internal const uint STANDARD_RIGHTS_WRITE = 131072u;

	internal const uint STANDARD_RIGHTS_EXECUTE = 131072u;

	internal const uint STANDARD_RIGHTS_ALL = 2031616u;

	internal const uint SPECIFIC_RIGHTS_ALL = 65535u;

	internal const uint ACCESS_SYSTEM_SECURITY = 16777216u;

	internal const uint MAXIMUM_ALLOWED = 33554432u;

	internal const uint GENERIC_READ = 2147483648u;

	internal const uint GENERIC_WRITE = 1073741824u;

	internal const uint GENERIC_EXECUTE = 536870912u;

	internal const uint GENERIC_ALL = 268435456u;

	internal const uint FILE_READ_DATA = 1u;

	internal const uint FILE_LIST_DIRECTORY = 1u;

	internal const uint FILE_WRITE_DATA = 2u;

	internal const uint FILE_ADD_FILE = 2u;

	internal const uint FILE_APPEND_DATA = 4u;

	internal const uint FILE_ADD_SUBDIRECTORY = 4u;

	internal const uint FILE_CREATE_PIPE_INSTANCE = 4u;

	internal const uint FILE_READ_EA = 8u;

	internal const uint FILE_WRITE_EA = 16u;

	internal const uint FILE_EXECUTE = 32u;

	internal const uint FILE_TRAVERSE = 32u;

	internal const uint FILE_DELETE_CHILD = 64u;

	internal const uint FILE_READ_ATTRIBUTES = 128u;

	internal const uint FILE_WRITE_ATTRIBUTES = 256u;

	internal const uint FILE_ALL_ACCESS = 2032127u;

	internal const uint FILE_GENERIC_READ = 1179785u;

	internal const uint FILE_GENERIC_WRITE = 1179926u;

	internal const uint FILE_GENERIC_EXECUTE = 1179808u;

	internal const uint FILE_SHARE_READ = 1u;

	internal const uint FILE_SHARE_WRITE = 2u;

	internal const uint FILE_SHARE_DELETE = 4u;

	internal const int ERROR_ALREADY_EXISTS = 183;

	internal const int OPEN_EXISTING = 3;

	internal const int PAGE_READONLY = 2;

	internal const int SECTION_MAP_READ = 4;

	internal const int FILE_ATTRIBUTE_NORMAL = 128;

	internal const int FILE_ATTRIBUTE_TEMPORARY = 256;

	internal const int FILE_FLAG_DELETE_ON_CLOSE = 67108864;

	internal const int CREATE_ALWAYS = 2;

	internal const int PROCESS_ALL_ACCESS = 2035711;

	public const int QS_EVENT = 8192;

	public const int VK_CANCEL = 3;

	public const int VK_BACK = 8;

	public const int VK_CLEAR = 12;

	public const int VK_RETURN = 13;

	public const int VK_PAUSE = 19;

	public const int VK_CAPITAL = 20;

	public const int VK_KANA = 21;

	public const int VK_HANGEUL = 21;

	public const int VK_HANGUL = 21;

	public const int VK_JUNJA = 23;

	public const int VK_FINAL = 24;

	public const int VK_HANJA = 25;

	public const int VK_KANJI = 25;

	public const int VK_ESCAPE = 27;

	public const int VK_CONVERT = 28;

	public const int VK_NONCONVERT = 29;

	public const int VK_ACCEPT = 30;

	public const int VK_MODECHANGE = 31;

	public const int VK_SPACE = 32;

	public const int VK_PRIOR = 33;

	public const int VK_NEXT = 34;

	public const int VK_END = 35;

	public const int VK_HOME = 36;

	public const int VK_LEFT = 37;

	public const int VK_UP = 38;

	public const int VK_RIGHT = 39;

	public const int VK_DOWN = 40;

	public const int VK_SELECT = 41;

	public const int VK_PRINT = 42;

	public const int VK_EXECUTE = 43;

	public const int VK_SNAPSHOT = 44;

	public const int VK_INSERT = 45;

	public const int VK_DELETE = 46;

	public const int VK_HELP = 47;

	public const int VK_0 = 48;

	public const int VK_1 = 49;

	public const int VK_2 = 50;

	public const int VK_3 = 51;

	public const int VK_4 = 52;

	public const int VK_5 = 53;

	public const int VK_6 = 54;

	public const int VK_7 = 55;

	public const int VK_8 = 56;

	public const int VK_9 = 57;

	public const int VK_A = 65;

	public const int VK_B = 66;

	public const int VK_C = 67;

	public const int VK_D = 68;

	public const int VK_E = 69;

	public const int VK_F = 70;

	public const int VK_G = 71;

	public const int VK_H = 72;

	public const int VK_I = 73;

	public const int VK_J = 74;

	public const int VK_K = 75;

	public const int VK_L = 76;

	public const int VK_M = 77;

	public const int VK_N = 78;

	public const int VK_O = 79;

	public const int VK_P = 80;

	public const int VK_Q = 81;

	public const int VK_R = 82;

	public const int VK_S = 83;

	public const int VK_T = 84;

	public const int VK_U = 85;

	public const int VK_V = 86;

	public const int VK_W = 87;

	public const int VK_X = 88;

	public const int VK_Y = 89;

	public const int VK_Z = 90;

	public const int VK_LWIN = 91;

	public const int VK_RWIN = 92;

	public const int VK_APPS = 93;

	public const int VK_POWER = 94;

	public const int VK_SLEEP = 95;

	public const int VK_NUMPAD0 = 96;

	public const int VK_NUMPAD1 = 97;

	public const int VK_NUMPAD2 = 98;

	public const int VK_NUMPAD3 = 99;

	public const int VK_NUMPAD4 = 100;

	public const int VK_NUMPAD5 = 101;

	public const int VK_NUMPAD6 = 102;

	public const int VK_NUMPAD7 = 103;

	public const int VK_NUMPAD8 = 104;

	public const int VK_NUMPAD9 = 105;

	public const int VK_MULTIPLY = 106;

	public const int VK_ADD = 107;

	public const int VK_SEPARATOR = 108;

	public const int VK_SUBTRACT = 109;

	public const int VK_DECIMAL = 110;

	public const int VK_DIVIDE = 111;

	public const int VK_F1 = 112;

	public const int VK_F2 = 113;

	public const int VK_F3 = 114;

	public const int VK_F4 = 115;

	public const int VK_F5 = 116;

	public const int VK_F6 = 117;

	public const int VK_F7 = 118;

	public const int VK_F8 = 119;

	public const int VK_F9 = 120;

	public const int VK_F10 = 121;

	public const int VK_F11 = 122;

	public const int VK_F12 = 123;

	public const int VK_F13 = 124;

	public const int VK_F14 = 125;

	public const int VK_F15 = 126;

	public const int VK_F16 = 127;

	public const int VK_F17 = 128;

	public const int VK_F18 = 129;

	public const int VK_F19 = 130;

	public const int VK_F20 = 131;

	public const int VK_F21 = 132;

	public const int VK_F22 = 133;

	public const int VK_F23 = 134;

	public const int VK_F24 = 135;

	public const int VK_NUMLOCK = 144;

	public const int VK_SCROLL = 145;

	public const int VK_RSHIFT = 161;

	public const int VK_BROWSER_BACK = 166;

	public const int VK_BROWSER_FORWARD = 167;

	public const int VK_BROWSER_REFRESH = 168;

	public const int VK_BROWSER_STOP = 169;

	public const int VK_BROWSER_SEARCH = 170;

	public const int VK_BROWSER_FAVORITES = 171;

	public const int VK_BROWSER_HOME = 172;

	public const int VK_VOLUME_MUTE = 173;

	public const int VK_VOLUME_DOWN = 174;

	public const int VK_VOLUME_UP = 175;

	public const int VK_MEDIA_NEXT_TRACK = 176;

	public const int VK_MEDIA_PREV_TRACK = 177;

	public const int VK_MEDIA_STOP = 178;

	public const int VK_MEDIA_PLAY_PAUSE = 179;

	public const int VK_LAUNCH_MAIL = 180;

	public const int VK_LAUNCH_MEDIA_SELECT = 181;

	public const int VK_LAUNCH_APP1 = 182;

	public const int VK_LAUNCH_APP2 = 183;

	public const int VK_PROCESSKEY = 229;

	public const int VK_PACKET = 231;

	public const int VK_ATTN = 246;

	public const int VK_CRSEL = 247;

	public const int VK_EXSEL = 248;

	public const int VK_EREOF = 249;

	public const int VK_PLAY = 250;

	public const int VK_ZOOM = 251;

	public const int VK_NONAME = 252;

	public const int VK_PA1 = 253;

	public const int VK_OEM_CLEAR = 254;

	internal const int ENDSESSION_LOGOFF = int.MinValue;

	internal const int ERROR_SUCCESS = 0;

	public const int LOCALE_FONTSIGNATURE = 88;

	public const int SWP_NOREDRAW = 8;

	public const int SWP_FRAMECHANGED = 32;

	public const int SWP_NOCOPYBITS = 256;

	public const int SWP_NOOWNERZORDER = 512;

	public const int SWP_NOSENDCHANGING = 1024;

	public const int SWP_NOREPOSITION = 512;

	public const int SWP_DEFERERASE = 8192;

	public const int SWP_ASYNCWINDOWPOS = 16384;

	public const int SPI_GETCURSORSHADOW = 4122;

	public const int SPI_SETCURSORSHADOW = 4123;

	public const int SPI_GETFOCUSBORDERWIDTH = 8206;

	public const int SPI_SETFOCUSBORDERWIDTH = 8207;

	public const int SPI_GETFOCUSBORDERHEIGHT = 8208;

	public const int SPI_SETFOCUSBORDERHEIGHT = 8209;

	public const int SPI_GETSTYLUSHOTTRACKING = 4112;

	public const int SPI_SETSTYLUSHOTTRACKING = 4113;

	public const int SPI_GETTOOLTIPFADE = 4120;

	public const int SPI_SETTOOLTIPFADE = 4121;

	public const int SPI_GETFOREGROUNDFLASHCOUNT = 8196;

	public const int SPI_SETFOREGROUNDFLASHCOUNT = 8197;

	public const int SPI_SETCARETWIDTH = 8199;

	public const int SPI_SETMOUSEVANISH = 4129;

	public const int SPI_SETHIGHCONTRAST = 67;

	public const int SPI_SETKEYBOARDPREF = 69;

	public const int SPI_SETFLATMENU = 4131;

	public const int SPI_SETDROPSHADOW = 4133;

	public const int SPI_SETWORKAREA = 47;

	public const int SPI_SETICONMETRICS = 46;

	public const int SPI_SETDRAGWIDTH = 76;

	public const int SPI_SETDRAGHEIGHT = 77;

	public const int SPI_SETPENWINDOWS = 49;

	public const int SPI_SETMOUSEBUTTONSWAP = 33;

	public const int SPI_SETSHOWSOUNDS = 57;

	public const int SPI_SETKEYBOARDCUES = 4107;

	public const int SPI_SETKEYBOARDDELAY = 23;

	public const int SPI_SETSNAPTODEFBUTTON = 96;

	public const int SPI_SETWHEELSCROLLLINES = 105;

	public const int SPI_SETMOUSEHOVERWIDTH = 99;

	public const int SPI_SETMOUSEHOVERHEIGHT = 101;

	public const int SPI_SETMOUSEHOVERTIME = 103;

	public const int SPI_SETMENUDROPALIGNMENT = 28;

	public const int SPI_SETMENUFADE = 4115;

	public const int SPI_SETMENUSHOWDELAY = 107;

	public const int SPI_SETCOMBOBOXANIMATION = 4101;

	public const int SPI_SETCLIENTAREAANIMATION = 4163;

	public const int SPI_SETGRADIENTCAPTIONS = 4105;

	public const int SPI_SETHOTTRACKING = 4111;

	public const int SPI_SETLISTBOXSMOOTHSCROLLING = 4103;

	public const int SPI_SETMENUANIMATION = 4099;

	public const int SPI_SETSELECTIONFADE = 4117;

	public const int SPI_SETTOOLTIPANIMATION = 4119;

	public const int SPI_SETUIEFFECTS = 4159;

	public const int SPI_SETANIMATION = 73;

	public const int SPI_SETDRAGFULLWINDOWS = 37;

	public const int SPI_SETBORDER = 6;

	public const int SPI_SETNONCLIENTMETRICS = 42;

	public const int LANG_KOREAN = 18;

	public const int MB_YESNO = 4;

	public const int MB_SYSTEMMODAL = 4096;

	public const int IDYES = 6;

	public const int PM_QS_INPUT = 458752;

	public const int PM_QS_PAINT = 2097152;

	public const int SW_PARENTCLOSING = 1;

	public const int SW_PARENTOPENING = 3;

	public const int SC_MOUSEMOVE = 61458;

	public const int SPI_SETKEYBOARDSPEED = 11;

	internal const int TYMED_HGLOBAL = 1;

	internal const int TYMED_FILE = 2;

	internal const int TYMED_ISTREAM = 4;

	internal const int TYMED_ISTORAGE = 8;

	internal const int TYMED_GDI = 16;

	internal const int TYMED_MFPICT = 32;

	internal const int TYMED_ENHMF = 64;

	public const int WS_OVERLAPPEDWINDOW = 13565952;

	public const int KEYEVENTF_EXTENDEDKEY = 1;

	public const int KEYEVENTF_KEYUP = 2;

	public const int KEYEVENTF_UNICODE = 4;

	public const int KEYEVENTF_SCANCODE = 8;

	public const int MOUSEEVENTF_MOVE = 1;

	public const int MOUSEEVENTF_LEFTDOWN = 2;

	public const int MOUSEEVENTF_LEFTUP = 4;

	public const int MOUSEEVENTF_RIGHTDOWN = 8;

	public const int MOUSEEVENTF_RIGHTUP = 16;

	public const int MOUSEEVENTF_MIDDLEDOWN = 32;

	public const int MOUSEEVENTF_MIDDLEUP = 64;

	public const int MOUSEEVENTF_XDOWN = 128;

	public const int MOUSEEVENTF_XUP = 256;

	public const int MOUSEEVENTF_WHEEL = 2048;

	public const int MOUSEEVENTF_VIRTUALDESK = 16384;

	public const int MOUSEEVENTF_ABSOLUTE = 32768;

	public const int MOUSEEVENTF_ACTUAL = 65536;

	public const int GWL_HINSTANCE = -6;

	public const int GWL_USERDATA = -21;

	public const int GCL_MENUNAME = -8;

	public const int GCL_HBRBACKGROUND = -10;

	public const int GCL_HCURSOR = -12;

	public const int GCL_HICON = -14;

	public const int GCL_HMODULE = -16;

	public const int GCL_CBWNDEXTRA = -18;

	public const int GCL_CBCLSEXTRA = -20;

	public const int GCL_STYLE = -26;

	public const int GCW_ATOM = -32;

	public const int GCL_HICONSM = -34;

	public const int MONITOR_DEFAULTTONULL = 0;

	public const int MONITOR_DEFAULTTOPRIMARY = 1;

	public const int MONITOR_DEFAULTTONEAREST = 2;

	public const uint WTNCA_NODRAWCAPTION = 1u;

	public const uint WTNCA_NODRAWICON = 2u;

	public const uint WTNCA_NOSYSMENU = 4u;

	public const uint WTNCA_VALIDBITS = 7u;

	internal const int NO_ERROR = 0;

	public const int VK_OEM_1 = 186;

	public const int VK_OEM_PLUS = 187;

	public const int VK_OEM_COMMA = 188;

	public const int VK_OEM_MINUS = 189;

	public const int VK_OEM_PERIOD = 190;

	public const int VK_OEM_2 = 191;

	public const int VK_OEM_3 = 192;

	public const int VK_C1 = 193;

	public const int VK_C2 = 194;

	public const int VK_OEM_4 = 219;

	public const int VK_OEM_5 = 220;

	public const int VK_OEM_6 = 221;

	public const int VK_OEM_7 = 222;

	public const int VK_OEM_8 = 223;

	public const int VK_OEM_AX = 225;

	public const int VK_OEM_102 = 226;

	public const int VK_OEM_RESET = 233;

	public const int VK_OEM_JUMP = 234;

	public const int VK_OEM_PA1 = 235;

	public const int VK_OEM_PA2 = 236;

	public const int VK_OEM_PA3 = 237;

	public const int VK_OEM_WSCTRL = 238;

	public const int VK_OEM_CUSEL = 239;

	public const int VK_OEM_ATTN = 240;

	public const int VK_OEM_FINISH = 241;

	public const int VK_OEM_COPY = 242;

	public const int VK_OEM_AUTO = 243;

	public const int VK_OEM_ENLW = 244;

	public const int VK_OEM_BACKTAB = 245;

	public const int DRAGDROP_S_DROP = 262400;

	public const int DRAGDROP_S_CANCEL = 262401;

	public const int DRAGDROP_S_USEDEFAULTCURSORS = 262402;

	public const int TME_CANCEL = int.MinValue;

	public const int IDC_HAND = 32649;

	public const int DM_ORIENTATION = 1;

	public const int DM_PAPERSIZE = 2;

	public const int DM_PAPERLENGTH = 4;

	public const int DM_PAPERWIDTH = 8;

	public const int DM_PRINTQUALITY = 1024;

	public const int DM_YRESOLUTION = 8192;

	public const int MM_ISOTROPIC = 7;

	public const int DM_OUT_BUFFER = 2;

	public const int E_HANDLE = -2147024890;

	public const int SPI_SETFONTSMOOTHING = 75;

	public const int SPI_SETFONTSMOOTHINGTYPE = 8203;

	public const int SPI_SETFONTSMOOTHINGCONTRAST = 8205;

	public const int SPI_SETFONTSMOOTHINGORIENTATION = 8211;

	public const int SPI_SETDISPLAYPIXELSTRUCTURE = 8213;

	public const int SPI_SETDISPLAYGAMMA = 8215;

	public const int SPI_SETDISPLAYCLEARTYPELEVEL = 8217;

	public const int SPI_SETDISPLAYTEXTCONTRASTLEVEL = 8219;

	public const int GMMP_USE_DISPLAY_POINTS = 1;

	public const int GMMP_USE_HIGH_RESOLUTION_POINTS = 2;

	public const int ERROR_FILE_NOT_FOUND = 2;

	public const int ERROR_PATH_NOT_FOUND = 3;

	public const int ERROR_ACCESS_DENIED = 5;

	public const int ERROR_INVALID_DRIVE = 15;

	public const int ERROR_SHARING_VIOLATION = 32;

	public const int ERROR_FILE_EXISTS = 80;

	public const int ERROR_INVALID_PARAMETER = 87;

	public const int ERROR_FILENAME_EXCED_RANGE = 206;

	public const int ERROR_NO_MORE_ITEMS = 259;

	public const int ERROR_OPERATION_ABORTED = 995;

	public const int LR_DEFAULTCOLOR = 0;

	public const int LR_MONOCHROME = 1;

	public const int LR_COLOR = 2;

	public const int LR_COPYRETURNORG = 4;

	public const int LR_COPYDELETEORG = 8;

	public const int LR_LOADFROMFILE = 16;

	public const int LR_LOADTRANSPARENT = 32;

	public const int LR_DEFAULTSIZE = 64;

	public const int LR_VGACOLOR = 128;

	public const int LR_LOADMAP3DCOLORS = 4096;

	public const int LR_CREATEDIBSECTION = 8192;

	public const int LR_COPYFROMRESOURCE = 16384;

	public const int LR_SHARED = 32768;

	public const int WTS_CONSOLE_CONNECT = 1;

	public const int WTS_CONSOLE_DISCONNECT = 2;

	public const int WTS_REMOTE_CONNECT = 3;

	public const int WTS_REMOTE_DISCONNECT = 4;

	public const int WTS_SESSION_LOCK = 7;

	public const int WTS_SESSION_UNLOCK = 8;

	public const uint NOTIFY_FOR_THIS_SESSION = 0u;

	public const int PBT_APMSUSPEND = 4;

	public const int PBT_APMRESUMECRITICAL = 6;

	public const int PBT_APMRESUMESUSPEND = 7;

	public const int PBT_APMRESUMEAUTOMATIC = 18;

	public const int PBT_POWERSETTINGCHANGE = 32787;

	public static readonly Guid GUID_MONITOR_POWER_ON = new Guid(41095189, 17680, 17702, 153, 230, 229, 161, 126, 189, 26, 234);

	public const uint PROFILE_READ = 1u;

	public static bool Succeeded(int hr)
	{
		return hr >= 0;
	}

	public static bool Failed(int hr)
	{
		return hr < 0;
	}

	public static int SignedHIWORD(int n)
	{
		return (short)((n >> 16) & 0xFFFF);
	}

	public static int SignedLOWORD(int n)
	{
		return (short)(n & 0xFFFF);
	}

	public static int MakeHRFromErrorCode(int errorCode)
	{
		return -2147024896 | errorCode;
	}

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	internal static extern nint SetEnhMetaFileBits(uint cbBuffer, byte[] buffer);

	public static int SignedHIWORD(nint intPtr)
	{
		return SignedHIWORD(IntPtrToInt32(intPtr));
	}

	public static int SignedLOWORD(nint intPtr)
	{
		return SignedLOWORD(IntPtrToInt32(intPtr));
	}

	public static int IntPtrToInt32(nint intPtr)
	{
		return (int)((IntPtr)intPtr).ToInt64();
	}

	[DllImport("gdi32.dll")]
	public static extern int EndDoc(HDC hdc);

	[DllImport("gdi32.dll")]
	public unsafe static extern int ExtEscape(HDC hdc, int nEscape, int cbInput, PrinterEscape* lpvInData, int cbOutput, [Out] void* lpvOutData);

	[DllImport("gdi32.dll")]
	public static extern int StartDoc(HDC hdc, ref DocInfo docInfo);

	[DllImport("winspool.drv", BestFitMapping = false, ThrowOnUnmappableChar = true)]
	public unsafe static extern int OpenPrinterA(string printerName, nint* phPrinter, void* pDefaults);

	[DllImport("winspool.drv")]
	public static extern int ClosePrinter(nint hPrinter);

	[DllImport("gdi32.dll")]
	public static extern int EndPage(HDC hdc);

	[DllImport("gdi32.dll")]
	public static extern int StartPage(HDC hdc);
}
