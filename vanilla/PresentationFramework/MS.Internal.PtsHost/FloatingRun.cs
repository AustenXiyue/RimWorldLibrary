using System.Windows.Media.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class FloatingRun : TextHidden
{
	private readonly bool _figure;

	internal bool Figure => _figure;

	internal FloatingRun(int length, bool figure)
		: base(length)
	{
		_figure = figure;
	}
}
