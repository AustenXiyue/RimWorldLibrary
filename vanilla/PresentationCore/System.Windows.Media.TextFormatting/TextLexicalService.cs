using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

[FriendAccessAllowed]
internal abstract class TextLexicalService
{
	public abstract bool IsCultureSupported(CultureInfo culture);

	public abstract TextLexicalBreaks AnalyzeText(char[] characterSource, int length, CultureInfo textCulture);
}
