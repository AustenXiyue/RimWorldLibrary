using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal static class Context
{
	[DllImport("api-ms-win-core-com-l1-1-0.dll")]
	private static extern int CoGetObjectContext(ref Guid riid, out nint ppv);

	public static nint GetContextCallback()
	{
		Guid riid = typeof(IContextCallback).GUID;
		Marshal.ThrowExceptionForHR(CoGetObjectContext(ref riid, out var ppv));
		return ppv;
	}
}
