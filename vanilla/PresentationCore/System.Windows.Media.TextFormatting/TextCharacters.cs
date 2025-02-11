using System.Collections.Generic;
using System.Globalization;
using MS.Internal.PresentationCore;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace System.Windows.Media.TextFormatting;

/// <summary>Represents a collection of character glyphs from distinct physical typefaces.</summary>
public class TextCharacters : TextRun, ITextSymbols, IShapeableTextCollector
{
	private CharacterBufferReference _characterBufferReference;

	private int _length;

	private TextRunProperties _textRunProperties;

	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> for the <see cref="T:System.Windows.Media.TextFormatting.TextCharacters" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> that represents the text characters.</returns>
	public sealed override CharacterBufferReference CharacterBufferReference => _characterBufferReference;

	/// <summary>Gets the length of the text characters.</summary>
	/// <returns>An <see cref="T:System.Int32" /> object that represents the length of the text characters.</returns>
	public sealed override int Length => _length;

	/// <summary>Gets the set of properties shared by every text character.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value that represents the properties shared by every text character.</returns>
	public sealed override TextRunProperties Properties => _textRunProperties;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextCharacters" /> class using a specified character array.</summary>
	/// <param name="characterArray">The <see cref="T:System.Char" /> array.</param>
	/// <param name="offsetToFirstChar">The offset to the first character to use in <paramref name="characterArray" />.</param>
	/// <param name="length">The length of the characters to use in <paramref name="characterArray" />.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in <paramref name="characterArray" />.</param>
	public TextCharacters(char[] characterArray, int offsetToFirstChar, int length, TextRunProperties textRunProperties)
		: this(new CharacterBufferReference(characterArray, offsetToFirstChar), length, textRunProperties)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextCharacters" /> class using a specified character string.</summary>
	/// <param name="characterString">The <see cref="T:System.String" /> containing the text characters.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in <paramref name="characterString" />.</param>
	public TextCharacters(string characterString, TextRunProperties textRunProperties)
		: this(characterString, 0, characterString?.Length ?? 0, textRunProperties)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextCharacters" /> class using a specified character substring.</summary>
	/// <param name="characterString">The <see cref="T:System.String" /> containing the text characters.</param>
	/// <param name="offsetToFirstChar">The offset to the first character to use in <paramref name="characterString" />.</param>
	/// <param name="length">The length of the characters to use in <paramref name="characterString" />.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in <paramref name="characterString" />.</param>
	public TextCharacters(string characterString, int offsetToFirstChar, int length, TextRunProperties textRunProperties)
		: this(new CharacterBufferReference(characterString, offsetToFirstChar), length, textRunProperties)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextCharacters" /> class using a specified unsafe character string.</summary>
	/// <param name="unsafeCharacterString">Pointer to character string.</param>
	/// <param name="length">The length of the characters to use in <paramref name="unsafeCharacterString" />.</param>
	/// <param name="textRunProperties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> value to use for the characters in <paramref name="unsafeCharacterString" />.</param>
	[CLSCompliant(false)]
	public unsafe TextCharacters(char* unsafeCharacterString, int length, TextRunProperties textRunProperties)
		: this(new CharacterBufferReference(unsafeCharacterString, length), length, textRunProperties)
	{
	}

	private TextCharacters(CharacterBufferReference characterBufferReference, int length, TextRunProperties textRunProperties)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterMustBeGreaterThanZero);
		}
		if (textRunProperties == null)
		{
			throw new ArgumentNullException("textRunProperties");
		}
		if (textRunProperties.Typeface == null)
		{
			throw new ArgumentNullException("textRunProperties.Typeface");
		}
		if (textRunProperties.CultureInfo == null)
		{
			throw new ArgumentNullException("textRunProperties.CultureInfo");
		}
		if (textRunProperties.FontRenderingEmSize <= 0.0)
		{
			throw new ArgumentOutOfRangeException("textRunProperties.FontRenderingEmSize", SR.ParameterMustBeGreaterThanZero);
		}
		_characterBufferReference = characterBufferReference;
		_length = length;
		_textRunProperties = textRunProperties;
	}

	IList<TextShapeableSymbols> ITextSymbols.GetTextShapeableSymbols(GlyphingCache glyphingCache, CharacterBufferReference characterBufferReference, int length, bool rightToLeft, bool isRightToLeftParagraph, CultureInfo digitCulture, TextModifierScope textModifierScope, TextFormattingMode textFormattingMode, bool isSideways)
	{
		if (characterBufferReference.CharacterBuffer == null)
		{
			throw new ArgumentNullException("characterBufferReference.CharacterBuffer");
		}
		int num = characterBufferReference.OffsetToFirstChar - _characterBufferReference.OffsetToFirstChar;
		if (length < 0 || num + length > _length)
		{
			length = _length - num;
		}
		TextRunProperties textRunProperties = _textRunProperties;
		if (textModifierScope != null)
		{
			textRunProperties = textModifierScope.ModifyProperties(textRunProperties);
		}
		if (!rightToLeft && textRunProperties.Typeface.CheckFastPathNominalGlyphs(new CharacterBufferRange(characterBufferReference, length), textRunProperties.FontRenderingEmSize, (float)textRunProperties.PixelsPerDip, 1.0, double.MaxValue, keepAWord: true, digitCulture != null, CultureMapper.GetSpecificCulture(textRunProperties.CultureInfo), textFormattingMode, isSideways, breakOnTabs: false, out var stringLengthFit) && length == stringLengthFit)
		{
			return new TextShapeableCharacters[1]
			{
				new TextShapeableCharacters(new CharacterBufferRange(characterBufferReference, stringLengthFit), textRunProperties, textRunProperties.FontRenderingEmSize, new ItemProps(), null, nullShape: false, textFormattingMode, isSideways)
			};
		}
		IList<TextShapeableSymbols> list = new List<TextShapeableSymbols>(2);
		glyphingCache.GetShapeableText(textRunProperties.Typeface, characterBufferReference, length, textRunProperties, digitCulture, isRightToLeftParagraph, list, this, textFormattingMode);
		return list;
	}

	void IShapeableTextCollector.Add(IList<TextShapeableSymbols> shapeables, CharacterBufferRange characterBufferRange, TextRunProperties textRunProperties, ItemProps textItem, ShapeTypeface shapeTypeface, double emScale, bool nullShape, TextFormattingMode textFormattingMode)
	{
		shapeables.Add(new TextShapeableCharacters(characterBufferRange, textRunProperties, textRunProperties.FontRenderingEmSize * emScale, textItem, shapeTypeface, nullShape, textFormattingMode, isSideways: false));
	}
}
