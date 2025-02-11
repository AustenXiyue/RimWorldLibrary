using System;
using System.Runtime.InteropServices;

namespace MS.Internal.Printing;

internal static class NativeMethods
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	internal class PRINTDLGEX32
	{
		public int lStructSize = Marshal.SizeOf(typeof(PRINTDLGEX32));

		public nint hwndOwner = IntPtr.Zero;

		public nint hDevMode = IntPtr.Zero;

		public nint hDevNames = IntPtr.Zero;

		public nint hDC = IntPtr.Zero;

		public uint Flags;

		public uint Flags2;

		public uint ExclusionFlags;

		public uint nPageRanges;

		public uint nMaxPageRanges;

		public nint lpPageRanges = IntPtr.Zero;

		public uint nMinPage;

		public uint nMaxPage;

		public uint nCopies;

		public nint hInstance = IntPtr.Zero;

		public nint lpPrintTemplateName = IntPtr.Zero;

		public nint lpCallback = IntPtr.Zero;

		public uint nPropertyPages;

		public nint lphPropertyPages = IntPtr.Zero;

		public uint nStartPage = uint.MaxValue;

		public uint dwResultAction;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
	internal class PRINTDLGEX64
	{
		public int lStructSize = Marshal.SizeOf(typeof(PRINTDLGEX64));

		public nint hwndOwner = IntPtr.Zero;

		public nint hDevMode = IntPtr.Zero;

		public nint hDevNames = IntPtr.Zero;

		public nint hDC = IntPtr.Zero;

		public uint Flags;

		public uint Flags2;

		public uint ExclusionFlags;

		public uint nPageRanges;

		public uint nMaxPageRanges;

		public nint lpPageRanges = IntPtr.Zero;

		public uint nMinPage;

		public uint nMaxPage;

		public uint nCopies;

		public nint hInstance = IntPtr.Zero;

		public nint lpPrintTemplateName = IntPtr.Zero;

		public nint lpCallback = IntPtr.Zero;

		public uint nPropertyPages;

		public nint lphPropertyPages = IntPtr.Zero;

		public uint nStartPage = uint.MaxValue;

		public uint dwResultAction;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	internal struct DEVMODE
	{
		private const int CCHDEVICENAME = 32;

		private const int CCHFORMNAME = 32;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public ushort dmSpecVersion;

		public ushort dmDriverVersion;

		public ushort dmSize;

		public ushort dmDriverExtra;

		public uint dmFields;

		public int dmPositionX;

		public int dmPositionY;

		public uint dmDisplayOrientation;

		public uint dmDisplayFixedOutput;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public ushort dmLogPixels;

		public uint dmBitsPerPel;

		public uint dmPelsWidth;

		public uint dmPelsHeight;

		public uint dmDisplayFlags;

		public uint dmDisplayFrequency;

		public uint dmICMMethod;

		public uint dmICMIntent;

		public uint dmMediaType;

		public uint dmDitherType;

		public uint dmReserved1;

		public uint dmReserved2;

		public uint dmPanningWidth;

		public uint dmPanningHeight;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	internal struct DEVNAMES
	{
		public ushort wDriverOffset;

		public ushort wDeviceOffset;

		public ushort wOutputOffset;

		public ushort wDefault;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
	internal struct PRINTPAGERANGE
	{
		public uint nFromPage;

		public uint nToPage;
	}

	internal const uint PD_ALLPAGES = 0u;

	internal const uint PD_SELECTION = 1u;

	internal const uint PD_PAGENUMS = 2u;

	internal const uint PD_NOSELECTION = 4u;

	internal const uint PD_NOPAGENUMS = 8u;

	internal const uint PD_USEDEVMODECOPIESANDCOLLATE = 262144u;

	internal const uint PD_DISABLEPRINTTOFILE = 524288u;

	internal const uint PD_HIDEPRINTTOFILE = 1048576u;

	internal const uint PD_CURRENTPAGE = 4194304u;

	internal const uint PD_NOCURRENTPAGE = 8388608u;

	internal const uint PD_RESULT_CANCEL = 0u;

	internal const uint PD_RESULT_PRINT = 1u;

	internal const uint PD_RESULT_APPLY = 2u;

	internal const uint START_PAGE_GENERAL = uint.MaxValue;
}
