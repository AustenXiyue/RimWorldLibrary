using System.Windows.Media;

namespace MS.Internal.PtsHost;

internal sealed class LineVisual : DrawingVisual
{
	internal double WidthIncludingTrailingWhitespace;

	internal DrawingContext Open()
	{
		return RenderOpen();
	}
}
