namespace System.Windows.Controls;

internal static class EditingModeHelper
{
	internal static bool IsDefined(InkCanvasEditingMode InkCanvasEditingMode)
	{
		if (InkCanvasEditingMode >= InkCanvasEditingMode.None)
		{
			return InkCanvasEditingMode <= InkCanvasEditingMode.EraseByStroke;
		}
		return false;
	}
}
