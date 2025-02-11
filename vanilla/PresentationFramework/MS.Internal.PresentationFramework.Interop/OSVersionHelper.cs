using System;
using System.Runtime.InteropServices;

namespace MS.Internal.PresentationFramework.Interop;

internal static class OSVersionHelper
{
	internal static bool IsOsWindows10RS5OrGreater { get; set; }

	internal static bool IsOsWindows10RS4OrGreater { get; set; }

	internal static bool IsOsWindows10RS3OrGreater { get; set; }

	internal static bool IsOsWindows10RS2OrGreater { get; set; }

	internal static bool IsOsWindows10RS1OrGreater { get; set; }

	internal static bool IsOsWindows10TH2OrGreater { get; set; }

	internal static bool IsOsWindows10TH1OrGreater { get; set; }

	internal static bool IsOsWindows10OrGreater { get; set; }

	internal static bool IsOsWindows8Point1OrGreater { get; set; }

	internal static bool IsOsWindows8OrGreater { get; set; }

	internal static bool IsOsWindows7SP1OrGreater { get; set; }

	internal static bool IsOsWindows7OrGreater { get; set; }

	internal static bool IsOsWindowsVistaSP2OrGreater { get; set; }

	internal static bool IsOsWindowsVistaSP1OrGreater { get; set; }

	internal static bool IsOsWindowsVistaOrGreater { get; set; }

	internal static bool IsOsWindowsXPSP3OrGreater { get; set; }

	internal static bool IsOsWindowsXPSP2OrGreater { get; set; }

	internal static bool IsOsWindowsXPSP1OrGreater { get; set; }

	internal static bool IsOsWindowsXPOrGreater { get; set; }

	internal static bool IsOsWindowsServer { get; set; }

	static OSVersionHelper()
	{
		IsOsWindows10RS5OrGreater = IsWindows10RS5OrGreater();
		IsOsWindows10RS4OrGreater = IsWindows10RS4OrGreater();
		IsOsWindows10RS3OrGreater = IsWindows10RS3OrGreater();
		IsOsWindows10RS2OrGreater = IsWindows10RS2OrGreater();
		IsOsWindows10RS1OrGreater = IsWindows10RS1OrGreater();
		IsOsWindows10TH2OrGreater = IsWindows10TH2OrGreater();
		IsOsWindows10TH1OrGreater = IsWindows10TH1OrGreater();
		IsOsWindows10OrGreater = IsWindows10OrGreater();
		IsOsWindows8Point1OrGreater = IsWindows8Point1OrGreater();
		IsOsWindows8OrGreater = IsWindows8OrGreater();
		IsOsWindows7SP1OrGreater = IsWindows7SP1OrGreater();
		IsOsWindows7OrGreater = IsWindows7OrGreater();
		IsOsWindowsVistaSP2OrGreater = IsWindowsVistaSP2OrGreater();
		IsOsWindowsVistaSP1OrGreater = IsWindowsVistaSP1OrGreater();
		IsOsWindowsVistaOrGreater = IsWindowsVistaOrGreater();
		IsOsWindowsXPSP3OrGreater = IsWindowsXPSP3OrGreater();
		IsOsWindowsXPSP2OrGreater = IsWindowsXPSP2OrGreater();
		IsOsWindowsXPSP1OrGreater = IsWindowsXPSP1OrGreater();
		IsOsWindowsXPOrGreater = IsWindowsXPOrGreater();
		IsOsWindowsServer = IsWindowsServer();
	}

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10RS5OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10RS4OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10RS3OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10RS2OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10RS1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10TH2OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10TH1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows10OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows8Point1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows8OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows7SP1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindows7OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsVistaSP2OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsVistaSP1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsVistaOrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsXPSP3OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsXPSP2OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsXPSP1OrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsXPOrGreater();

	[DllImport("PresentationNative_cor3.dll", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool IsWindowsServer();

	internal static bool IsOsVersionOrGreater(OperatingSystemVersion osVer)
	{
		return osVer switch
		{
			OperatingSystemVersion.Windows10RS5 => IsOsWindows10RS5OrGreater, 
			OperatingSystemVersion.Windows10RS4 => IsOsWindows10RS4OrGreater, 
			OperatingSystemVersion.Windows10RS3 => IsOsWindows10RS3OrGreater, 
			OperatingSystemVersion.Windows10RS2 => IsOsWindows10RS2OrGreater, 
			OperatingSystemVersion.Windows10RS1 => IsOsWindows10RS1OrGreater, 
			OperatingSystemVersion.Windows10TH2 => IsOsWindows10TH2OrGreater, 
			OperatingSystemVersion.Windows10 => IsOsWindows10OrGreater, 
			OperatingSystemVersion.Windows8Point1 => IsOsWindows8Point1OrGreater, 
			OperatingSystemVersion.Windows8 => IsOsWindows8OrGreater, 
			OperatingSystemVersion.Windows7SP1 => IsOsWindows7SP1OrGreater, 
			OperatingSystemVersion.Windows7 => IsOsWindows7OrGreater, 
			OperatingSystemVersion.WindowsVistaSP2 => IsOsWindowsVistaSP2OrGreater, 
			OperatingSystemVersion.WindowsVistaSP1 => IsOsWindowsVistaSP1OrGreater, 
			OperatingSystemVersion.WindowsVista => IsOsWindowsVistaOrGreater, 
			OperatingSystemVersion.WindowsXPSP3 => IsOsWindowsXPSP3OrGreater, 
			OperatingSystemVersion.WindowsXPSP2 => IsOsWindowsXPSP2OrGreater, 
			_ => throw new ArgumentException($"{osVer.ToString()} is not a valid OS!", "osVer"), 
		};
	}

	internal static OperatingSystemVersion GetOsVersion()
	{
		if (IsOsWindows10RS5OrGreater)
		{
			return OperatingSystemVersion.Windows10RS5;
		}
		if (IsOsWindows10RS4OrGreater)
		{
			return OperatingSystemVersion.Windows10RS4;
		}
		if (IsOsWindows10RS3OrGreater)
		{
			return OperatingSystemVersion.Windows10RS3;
		}
		if (IsOsWindows10RS2OrGreater)
		{
			return OperatingSystemVersion.Windows10RS2;
		}
		if (IsOsWindows10RS1OrGreater)
		{
			return OperatingSystemVersion.Windows10RS1;
		}
		if (IsOsWindows10TH2OrGreater)
		{
			return OperatingSystemVersion.Windows10TH2;
		}
		if (IsOsWindows10OrGreater)
		{
			return OperatingSystemVersion.Windows10;
		}
		if (IsOsWindows8Point1OrGreater)
		{
			return OperatingSystemVersion.Windows8Point1;
		}
		if (IsOsWindows8OrGreater)
		{
			return OperatingSystemVersion.Windows8;
		}
		if (IsOsWindows7SP1OrGreater)
		{
			return OperatingSystemVersion.Windows7SP1;
		}
		if (IsOsWindows7OrGreater)
		{
			return OperatingSystemVersion.Windows7;
		}
		if (IsOsWindowsVistaSP2OrGreater)
		{
			return OperatingSystemVersion.WindowsVistaSP2;
		}
		if (IsOsWindowsVistaSP1OrGreater)
		{
			return OperatingSystemVersion.WindowsVistaSP1;
		}
		if (IsOsWindowsVistaOrGreater)
		{
			return OperatingSystemVersion.WindowsVista;
		}
		if (IsOsWindowsXPSP3OrGreater)
		{
			return OperatingSystemVersion.WindowsXPSP3;
		}
		if (IsOsWindowsXPSP2OrGreater)
		{
			return OperatingSystemVersion.WindowsXPSP2;
		}
		throw new Exception("OSVersionHelper.GetOsVersion Could not detect OS!");
	}
}
