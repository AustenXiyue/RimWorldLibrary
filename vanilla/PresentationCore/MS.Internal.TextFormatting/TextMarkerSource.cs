using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class TextMarkerSource : TextSource
{
	private char[] _characterArray;

	private TextRunProperties _textRunProperties;

	private TextParagraphProperties _textParagraphProperties;

	private const char NumberSuffix = '.';

	private const string DecimalNumerics = "0123456789";

	private const string LowerLatinNumerics = "abcdefghijklmnopqrstuvwxyz";

	private const string UpperLatinNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private static string[][] RomanNumerics = new string[2][]
	{
		new string[4] { "m??", "cdm", "xlc", "ivx" },
		new string[4] { "M??", "CDM", "XLC", "IVX" }
	};

	internal TextMarkerSource(TextParagraphProperties textParagraphProperties, TextMarkerStyle markerStyle, int autoNumberingIndex)
	{
		_textParagraphProperties = textParagraphProperties;
		TextRunProperties defaultTextRunProperties = _textParagraphProperties.DefaultTextRunProperties;
		base.PixelsPerDip = defaultTextRunProperties.PixelsPerDip;
		string text = null;
		if (IsKnownSymbolMarkerStyle(markerStyle))
		{
			switch (markerStyle)
			{
			case TextMarkerStyle.Disc:
				text = "\u009f";
				break;
			case TextMarkerStyle.Circle:
				text = "¡";
				break;
			case TextMarkerStyle.Square:
				text = "q";
				break;
			case TextMarkerStyle.Box:
				text = "§";
				break;
			}
			Typeface typeface = defaultTextRunProperties.Typeface;
			_textRunProperties = new GenericTextRunProperties(new Typeface(new FontFamily("Wingdings"), typeface.Style, typeface.Weight, typeface.Stretch), defaultTextRunProperties.FontRenderingEmSize, defaultTextRunProperties.FontHintingEmSize, base.PixelsPerDip, defaultTextRunProperties.TextDecorations, defaultTextRunProperties.ForegroundBrush, defaultTextRunProperties.BackgroundBrush, defaultTextRunProperties.BaselineAlignment, CultureMapper.GetSpecificCulture(defaultTextRunProperties.CultureInfo), null);
		}
		else if (IsKnownIndexMarkerStyle(markerStyle))
		{
			_textRunProperties = defaultTextRunProperties;
			switch (markerStyle)
			{
			case TextMarkerStyle.Decimal:
				_characterArray = ConvertNumberToString(autoNumberingIndex, oneBased: false, "0123456789");
				break;
			case TextMarkerStyle.LowerLatin:
				_characterArray = ConvertNumberToString(autoNumberingIndex, oneBased: true, "abcdefghijklmnopqrstuvwxyz");
				break;
			case TextMarkerStyle.UpperLatin:
				_characterArray = ConvertNumberToString(autoNumberingIndex, oneBased: true, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
				break;
			case TextMarkerStyle.LowerRoman:
				text = ConvertNumberToRomanString(autoNumberingIndex, uppercase: false);
				break;
			case TextMarkerStyle.UpperRoman:
				text = ConvertNumberToRomanString(autoNumberingIndex, uppercase: true);
				break;
			}
		}
		if (text != null)
		{
			_characterArray = text.ToCharArray();
		}
	}

	public override TextRun GetTextRun(int textSourceCharacterIndex)
	{
		if (textSourceCharacterIndex < _characterArray.Length)
		{
			_textRunProperties.PixelsPerDip = base.PixelsPerDip;
			return new TextCharacters(_characterArray, textSourceCharacterIndex, _characterArray.Length - textSourceCharacterIndex, _textRunProperties);
		}
		return new TextEndOfParagraph(1);
	}

	public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
	{
		CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
		if (textSourceCharacterIndexLimit > 0)
		{
			characterBufferRange = new CharacterBufferRange(new CharacterBufferReference(_characterArray, 0), Math.Min(_characterArray.Length, textSourceCharacterIndexLimit));
		}
		return new TextSpan<CultureSpecificCharacterBufferRange>(textSourceCharacterIndexLimit, new CultureSpecificCharacterBufferRange(CultureMapper.GetSpecificCulture(_textRunProperties.CultureInfo), characterBufferRange));
	}

	public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
	{
		throw new NotSupportedException();
	}

	private static char[] ConvertNumberToString(int number, bool oneBased, string numericSymbols)
	{
		if (oneBased)
		{
			number--;
		}
		int length = numericSymbols.Length;
		char[] array;
		if (number < length)
		{
			array = new char[2]
			{
				numericSymbols[number],
				'.'
			};
		}
		else
		{
			int num = (oneBased ? 1 : 0);
			int num2 = 1;
			long num3 = length;
			long num4 = length;
			while (number >= num3)
			{
				num4 *= length;
				num3 = num4 + num3 * num;
				num2++;
			}
			array = new char[num2 + 1];
			array[num2] = '.';
			for (int num5 = num2 - 1; num5 >= 0; num5--)
			{
				array[num5] = numericSymbols[number % length];
				number = number / length - num;
			}
		}
		return array;
	}

	private static string ConvertNumberToRomanString(int number, bool uppercase)
	{
		if (number > 3999)
		{
			return number.ToString(CultureInfo.InvariantCulture);
		}
		StringBuilder stringBuilder = new StringBuilder();
		AddRomanNumeric(stringBuilder, number / 1000, RomanNumerics[uppercase ? 1u : 0u][0]);
		number %= 1000;
		AddRomanNumeric(stringBuilder, number / 100, RomanNumerics[uppercase ? 1u : 0u][1]);
		number %= 100;
		AddRomanNumeric(stringBuilder, number / 10, RomanNumerics[uppercase ? 1u : 0u][2]);
		number %= 10;
		AddRomanNumeric(stringBuilder, number, RomanNumerics[uppercase ? 1u : 0u][3]);
		stringBuilder.Append('.');
		return stringBuilder.ToString();
	}

	private static void AddRomanNumeric(StringBuilder builder, int number, string oneFiveTen)
	{
		switch (number)
		{
		case 4:
		case 9:
			builder.Append(oneFiveTen[0]);
			break;
		case 1:
		case 2:
		case 3:
		case 5:
		case 6:
		case 7:
		case 8:
			break;
		default:
			return;
		}
		if (number == 9)
		{
			builder.Append(oneFiveTen[2]);
			return;
		}
		if (number >= 4)
		{
			builder.Append(oneFiveTen[1]);
		}
		int num = number % 5;
		while (num > 0 && num < 4)
		{
			builder.Append(oneFiveTen[0]);
			num--;
		}
	}

	internal static bool IsKnownSymbolMarkerStyle(TextMarkerStyle markerStyle)
	{
		if (markerStyle != TextMarkerStyle.Disc && markerStyle != TextMarkerStyle.Circle && markerStyle != TextMarkerStyle.Square)
		{
			return markerStyle == TextMarkerStyle.Box;
		}
		return true;
	}

	internal static bool IsKnownIndexMarkerStyle(TextMarkerStyle markerStyle)
	{
		if (markerStyle != TextMarkerStyle.Decimal && markerStyle != TextMarkerStyle.LowerLatin && markerStyle != TextMarkerStyle.UpperLatin && markerStyle != TextMarkerStyle.LowerRoman)
		{
			return markerStyle == TextMarkerStyle.UpperRoman;
		}
		return true;
	}
}
