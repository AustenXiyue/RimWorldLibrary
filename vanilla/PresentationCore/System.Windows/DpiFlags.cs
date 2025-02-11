namespace System.Windows;

internal class DpiFlags
{
	internal bool DpiScaleFlag1 { get; set; }

	internal bool DpiScaleFlag2 { get; set; }

	internal int Index { get; set; }

	internal DpiFlags(bool dpiScaleFlag1, bool dpiScaleFlag2, int index)
	{
		DpiScaleFlag1 = dpiScaleFlag1;
		DpiScaleFlag2 = dpiScaleFlag2;
		Index = index;
	}
}
