using System.ComponentModel;

namespace System.Windows;

internal class HwndDpiChangedAfterParentEventArgs : HandledEventArgs
{
	internal DpiScale OldDpi { get; }

	internal DpiScale NewDpi { get; }

	internal Rect SuggestedRect { get; }

	internal HwndDpiChangedAfterParentEventArgs(DpiScale oldDpi, DpiScale newDpi, Rect suggestedRect)
		: base(defaultHandledValue: false)
	{
		OldDpi = oldDpi;
		NewDpi = newDpi;
		SuggestedRect = suggestedRect;
	}

	internal HwndDpiChangedAfterParentEventArgs(HwndDpiChangedEventArgs e)
		: this(e.OldDpi, e.NewDpi, e.SuggestedRect)
	{
	}

	public static explicit operator HwndDpiChangedEventArgs(HwndDpiChangedAfterParentEventArgs e)
	{
		return new HwndDpiChangedEventArgs(e.OldDpi, e.NewDpi, e.SuggestedRect);
	}
}
