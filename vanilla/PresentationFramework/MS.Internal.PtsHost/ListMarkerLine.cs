using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class ListMarkerLine : LineBase
{
	private readonly TextFormatterHost _host;

	internal ListMarkerLine(TextFormatterHost host, ListParaClient paraClient)
		: base(paraClient)
	{
		_host = host;
	}

	internal override TextRun GetTextRun(int dcp)
	{
		return new ParagraphBreakRun(1, PTS.FSFLRES.fsflrEndOfParagraph);
	}

	internal override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
	{
		return new TextSpan<CultureSpecificCharacterBufferRange>(0, new CultureSpecificCharacterBufferRange(null, CharacterBufferRange.Empty));
	}

	internal override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int dcp)
	{
		return dcp;
	}

	internal void FormatAndDrawVisual(DrawingContext ctx, LineProperties lineProps, int ur, int vrBaseline)
	{
		bool flag = lineProps.FlowDirection == FlowDirection.RightToLeft;
		_host.Context = this;
		try
		{
			TextLine textLine = _host.TextFormatter.FormatLine(_host, 0, 0.0, lineProps.FirstLineProps, null, new TextRunCache());
			Point origin = new Point(TextDpi.FromTextDpi(ur), TextDpi.FromTextDpi(vrBaseline) - textLine.Baseline);
			textLine.Draw(ctx, origin, flag ? InvertAxes.Horizontal : InvertAxes.None);
			textLine.Dispose();
		}
		finally
		{
			_host.Context = null;
		}
	}
}
