namespace System.Windows.Controls;

internal class MatchedTextInfo
{
	private readonly string _matchedText;

	private readonly int _matchedItemIndex;

	private readonly int _matchedPrefixLength;

	private readonly int _textExcludingPrefixLength;

	private static MatchedTextInfo s_NoMatch;

	internal static MatchedTextInfo NoMatch => s_NoMatch;

	internal string MatchedText => _matchedText;

	internal int MatchedItemIndex => _matchedItemIndex;

	internal int MatchedPrefixLength => _matchedPrefixLength;

	internal int TextExcludingPrefixLength => _textExcludingPrefixLength;

	static MatchedTextInfo()
	{
		s_NoMatch = new MatchedTextInfo(-1, null, 0, 0);
	}

	internal MatchedTextInfo(int matchedItemIndex, string matchedText, int matchedPrefixLength, int textExcludingPrefixLength)
	{
		_matchedItemIndex = matchedItemIndex;
		_matchedText = matchedText;
		_matchedPrefixLength = matchedPrefixLength;
		_textExcludingPrefixLength = textExcludingPrefixLength;
	}
}
