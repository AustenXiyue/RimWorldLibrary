namespace System.Windows;

internal class DpiRecursiveChangeArgs
{
	internal bool DpiScaleFlag1 { get; set; }

	internal bool DpiScaleFlag2 { get; set; }

	internal int Index { get; set; }

	internal DpiScale OldDpiScale { get; set; }

	internal DpiScale NewDpiScale { get; set; }

	internal DpiRecursiveChangeArgs(DpiFlags dpiFlags, DpiScale oldDpiScale, DpiScale newDpiScale)
	{
		DpiScaleFlag1 = dpiFlags.DpiScaleFlag1;
		DpiScaleFlag2 = dpiFlags.DpiScaleFlag2;
		Index = dpiFlags.Index;
		OldDpiScale = oldDpiScale;
		NewDpiScale = newDpiScale;
	}
}
