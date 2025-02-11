using System;
using System.Runtime.InteropServices;

namespace MS.Internal.WindowsRuntime.Windows.UI.ViewManagement;

internal static class NativeMethods
{
	internal const int E_NOINTERFACE = -2147467262;

	internal const int REGDB_E_CLASSNOTREG = -2147221164;

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	internal static extern int WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString, int length, out nint hstring);

	[DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	internal static extern int WindowsDeleteString(nint hstring);

	[DllImport("api-ms-win-core-winrt-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
	internal static extern int RoGetActivationFactory(nint runtimeClassId, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object factory);
}
