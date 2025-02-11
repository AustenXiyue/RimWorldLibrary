using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal struct LineMetrics
{
	private uint _packedData;

	private double _width;

	private double _height;

	private double _start;

	private double _baseline;

	private TextLineBreak _textLineBreak;

	private static readonly uint HasBeenUpdatedMask = 1073741824u;

	private static readonly uint LengthMask = 1073741823u;

	private static readonly uint HasInlineObjectsMask = 2147483648u;

	internal int Length => (int)(_packedData & LengthMask);

	internal double Width => _width;

	internal double Height => _height;

	internal double Start => _start;

	internal double Baseline => _baseline;

	internal bool HasInlineObjects => (_packedData & HasInlineObjectsMask) != 0;

	internal TextLineBreak TextLineBreak => _textLineBreak;

	internal LineMetrics(int length, double width, double height, double baseline, bool hasInlineObjects, TextLineBreak textLineBreak)
	{
		_start = 0.0;
		_width = width;
		_height = height;
		_baseline = baseline;
		_textLineBreak = textLineBreak;
		_packedData = ((uint)length & LengthMask) | (hasInlineObjects ? HasInlineObjectsMask : 0);
	}

	internal LineMetrics(LineMetrics source, double start, double width)
	{
		_start = start;
		_width = width;
		_height = source.Height;
		_baseline = source.Baseline;
		_textLineBreak = source.TextLineBreak;
		_packedData = source._packedData | HasBeenUpdatedMask;
	}

	internal LineMetrics Dispose(bool returnUpdatedMetrics)
	{
		if (_textLineBreak != null)
		{
			_textLineBreak.Dispose();
			if (returnUpdatedMetrics)
			{
				return new LineMetrics(Length, _width, _height, _baseline, HasInlineObjects, null);
			}
		}
		return this;
	}
}
