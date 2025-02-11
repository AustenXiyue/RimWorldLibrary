using System.Windows.Media;

namespace System.Windows.Documents;

internal static class SelectionHighlightInfo
{
	private static readonly Brush _objectMaskBrush;

	internal static Brush ForegroundBrush => SystemColors.HighlightTextBrush;

	internal static Brush BackgroundBrush => SystemColors.HighlightBrush;

	internal static Brush ObjectMaskBrush => _objectMaskBrush;

	static SelectionHighlightInfo()
	{
		_objectMaskBrush = new SolidColorBrush(SystemColors.HighlightColor);
		_objectMaskBrush.Opacity = 0.5;
		_objectMaskBrush.Freeze();
	}
}
