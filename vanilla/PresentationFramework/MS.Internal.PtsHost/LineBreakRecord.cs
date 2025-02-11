using System.Windows.Media.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class LineBreakRecord : UnmanagedHandle
{
	private TextLineBreak _textLineBreak;

	internal TextLineBreak TextLineBreak => _textLineBreak;

	internal LineBreakRecord(PtsContext ptsContext, TextLineBreak textLineBreak)
		: base(ptsContext)
	{
		_textLineBreak = textLineBreak;
	}

	public override void Dispose()
	{
		if (_textLineBreak != null)
		{
			_textLineBreak.Dispose();
		}
		base.Dispose();
	}

	internal LineBreakRecord Clone()
	{
		return new LineBreakRecord(base.PtsContext, _textLineBreak.Clone());
	}
}
