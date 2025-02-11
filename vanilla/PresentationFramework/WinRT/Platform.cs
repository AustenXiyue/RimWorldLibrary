using System;
using System.Runtime.InteropServices;

namespace WinRT;

internal class Platform
{
	[DllImport("api-ms-win-core-com-l1-1-0.dll")]
	public static extern int CoDecrementMTAUsage(nint cookie);

	[DllImport("api-ms-win-core-com-l1-1-0.dll")]
	public unsafe static extern int CoIncrementMTAUsage(nint* cookie);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool FreeLibrary(nint moduleHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint GetProcAddress(nint moduleHandle, [MarshalAs(UnmanagedType.LPStr)] string functionName);

	public static T GetProcAddress<T>(nint moduleHandle)
	{
		nint procAddress = GetProcAddress(moduleHandle, typeof(T).Name);
		if (procAddress == IntPtr.Zero)
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
		return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nint LoadLibraryExW([MarshalAs(UnmanagedType.LPWStr)] string fileName, nint fileHandle, uint flags);

	[DllImport("api-ms-win-core-winrt-l1-1-0.dll")]
	public unsafe static extern int RoGetActivationFactory(nint runtimeClassId, ref Guid iid, nint* factory);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	public unsafe static extern int WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString, int length, nint* hstring);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	public unsafe static extern int WindowsCreateStringReference(char* sourceString, int length, nint* hstring_header, nint* hstring);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int WindowsDeleteString(nint hstring);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	public unsafe static extern int WindowsDuplicateString(nint sourceString, nint* hstring);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	public unsafe static extern char* WindowsGetStringRawBuffer(nint hstring, uint* length);
}
