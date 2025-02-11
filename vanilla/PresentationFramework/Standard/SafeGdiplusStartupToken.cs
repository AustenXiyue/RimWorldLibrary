using System;
using Microsoft.Win32.SafeHandles;

namespace Standard;

internal sealed class SafeGdiplusStartupToken : SafeHandleZeroOrMinusOneIsInvalid
{
	private SafeGdiplusStartupToken()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return NativeMethods.GdiplusShutdown(handle) == Status.Ok;
	}

	public static SafeGdiplusStartupToken Startup()
	{
		SafeGdiplusStartupToken safeGdiplusStartupToken = new SafeGdiplusStartupToken();
		if (NativeMethods.GdiplusStartup(out var token, new StartupInput(), out var _) == Status.Ok)
		{
			safeGdiplusStartupToken.handle = token;
			return safeGdiplusStartupToken;
		}
		safeGdiplusStartupToken.Dispose();
		throw new Exception("Unable to initialize GDI+");
	}
}
