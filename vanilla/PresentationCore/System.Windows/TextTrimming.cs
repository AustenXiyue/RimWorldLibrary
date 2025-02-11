namespace System.Windows;

/// <summary>Describes how text is trimmed when it overflows the edge of its containing box.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public enum TextTrimming
{
	/// <summary>Text is not trimmed.</summary>
	None,
	/// <summary>Text is trimmed at a character boundary. An ellipsis (...) is drawn in place of remaining text.</summary>
	CharacterEllipsis,
	/// <summary>Text is trimmed at a word boundary. An ellipsis (...) is drawn in place of remaining text.</summary>
	WordEllipsis
}
