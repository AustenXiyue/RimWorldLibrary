namespace System.Windows.Media.TextFormatting;

/// <summary>Represents the bounding rectangle of a text run.</summary>
public sealed class TextRunBounds
{
	private int _cpFirst;

	private int _cch;

	private Rect _bounds;

	private TextRun _textRun;

	/// <summary>Gets the character index of the first character in the bounded text run.</summary>
	/// <returns>The index representing the first character of the bounded text run.</returns>
	public int TextSourceCharacterIndex => _cpFirst;

	/// <summary>Gets the character length of bounded text run.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the character length.</returns>
	public int Length => _cch;

	/// <summary>Gets the bounding rectangle for the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> value that represents the bounding rectangle of the text run.</returns>
	public Rect Rectangle => _bounds;

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> object that represents the text run.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> value that represents the text run.</returns>
	public TextRun TextRun => _textRun;

	internal TextRunBounds(Rect bounds, int cpFirst, int cpEnd, TextRun textRun)
	{
		_cpFirst = cpFirst;
		_cch = cpEnd - cpFirst;
		_bounds = bounds;
		_textRun = textRun;
	}
}
