using System.Windows.Media.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class OptimalBreakSession : UnmanagedHandle
{
	private TextParagraphCache _textParagraphCache;

	private TextParagraph _textParagraph;

	private TextParaClient _textParaClient;

	private OptimalTextSource _optimalTextSource;

	internal TextParagraphCache TextParagraphCache => _textParagraphCache;

	internal TextParagraph TextParagraph => _textParagraph;

	internal TextParaClient TextParaClient => _textParaClient;

	internal OptimalTextSource OptimalTextSource => _optimalTextSource;

	internal OptimalBreakSession(TextParagraph textParagraph, TextParaClient textParaClient, TextParagraphCache TextParagraphCache, OptimalTextSource optimalTextSource)
		: base(textParagraph.PtsContext)
	{
		_textParagraph = textParagraph;
		_textParaClient = textParaClient;
		_textParagraphCache = TextParagraphCache;
		_optimalTextSource = optimalTextSource;
	}

	public override void Dispose()
	{
		try
		{
			if (_textParagraphCache != null)
			{
				_textParagraphCache.Dispose();
			}
			if (_optimalTextSource != null)
			{
				_optimalTextSource.Dispose();
			}
		}
		finally
		{
			_textParagraphCache = null;
			_optimalTextSource = null;
		}
		base.Dispose();
	}
}
