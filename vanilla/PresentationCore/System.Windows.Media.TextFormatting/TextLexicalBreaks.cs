using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

[FriendAccessAllowed]
internal abstract class TextLexicalBreaks
{
	public abstract int Length { get; }

	public abstract int GetNextBreak(int currentIndex);

	public abstract int GetPreviousBreak(int currentIndex);
}
