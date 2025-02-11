namespace System.Windows.Media.TextFormatting;

/// <summary>Defines a specialized text run that is used to mark the end of a paragraph.</summary>
public class TextEndOfParagraph : TextEndOfLine
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfParagraph" /> class using a specified character length.</summary>
	/// <param name="length">The number of characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfParagraph" /> buffer.</param>
	public TextEndOfParagraph(int length)
		: base(length)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfParagraph" /> class using a specified character length and <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value.</summary>
	/// <param name="length">The number of characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfParagraph" /> buffer.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfParagraph" /> buffer.</param>
	public TextEndOfParagraph(int length, TextRunProperties textRunProperties)
		: base(length, textRunProperties)
	{
	}
}
