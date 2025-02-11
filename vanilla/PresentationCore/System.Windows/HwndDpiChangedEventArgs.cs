using System.ComponentModel;
using System.Runtime.InteropServices;
using MS.Win32;

namespace System.Windows;

public sealed class HwndDpiChangedEventArgs : HandledEventArgs
{
	public DpiScale OldDpi { get; private set; }

	public DpiScale NewDpi { get; private set; }

	public Rect SuggestedRect { get; private set; }

	[Obsolete]
	internal HwndDpiChangedEventArgs(double oldDpiX, double oldDpiY, double newDpiX, double newDpiY, nint lParam)
		: base(defaultHandledValue: false)
	{
		OldDpi = new DpiScale(oldDpiX / 96.0, oldDpiY / 96.0);
		NewDpi = new DpiScale(newDpiX / 96.0, newDpiY / 96.0);
		MS.Win32.NativeMethods.RECT rECT = Marshal.PtrToStructure<MS.Win32.NativeMethods.RECT>(lParam);
		SuggestedRect = new Rect(rECT.left, rECT.top, rECT.Width, rECT.Height);
	}

	internal HwndDpiChangedEventArgs(DpiScale oldDpi, DpiScale newDpi, Rect suggestedRect)
		: base(defaultHandledValue: false)
	{
		OldDpi = oldDpi;
		NewDpi = newDpi;
		SuggestedRect = suggestedRect;
	}
}
