using System.Windows.Media.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class LineBreakpoint : UnmanagedHandle
{
	private TextBreakpoint _textBreakpoint;

	private OptimalBreakSession _optimalBreakSession;

	internal OptimalBreakSession OptimalBreakSession => _optimalBreakSession;

	internal LineBreakpoint(OptimalBreakSession optimalBreakSession, TextBreakpoint textBreakpoint)
		: base(optimalBreakSession.PtsContext)
	{
		_textBreakpoint = textBreakpoint;
		_optimalBreakSession = optimalBreakSession;
	}

	public override void Dispose()
	{
		if (_textBreakpoint != null)
		{
			_textBreakpoint.Dispose();
		}
		base.Dispose();
	}
}
