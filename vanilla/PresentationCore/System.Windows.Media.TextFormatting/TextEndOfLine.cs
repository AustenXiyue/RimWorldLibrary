using MS.Internal.PresentationCore;

namespace System.Windows.Media.TextFormatting;

/// <summary>Defines a specialized text run that is used to mark the end of a line.</summary>
public class TextEndOfLine : TextRun
{
	private int _length;

	private TextRunProperties _textRunProperties;

	/// <summary>Gets a reference to the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> character buffer.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> value.</returns>
	public sealed override CharacterBufferReference CharacterBufferReference => default(CharacterBufferReference);

	/// <summary>Gets the character length of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> character buffer.</summary>
	/// <returns>An <see cref="T:System.Int32" /> object that represents the length of the character buffer.</returns>
	public sealed override int Length => _length;

	/// <summary>Gets the set of properties shared by every text character of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> character buffer.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value that represents the properties shared by every text character.</returns>
	public sealed override TextRunProperties Properties => _textRunProperties;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> class using a specified character length.</summary>
	/// <param name="length">The number of characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> buffer.</param>
	public TextEndOfLine(int length)
		: this(length, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> class using a specified character length and <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value.</summary>
	/// <param name="length">The number of characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> buffer.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in the <see cref="T:System.Windows.Media.TextFormatting.TextEndOfLine" /> buffer.</param>
	public TextEndOfLine(int length, TextRunProperties textRunProperties)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterMustBeGreaterThanZero);
		}
		if (textRunProperties != null && textRunProperties.Typeface == null)
		{
			throw new ArgumentNullException("textRunProperties.Typeface");
		}
		_length = length;
		_textRunProperties = textRunProperties;
	}
}
