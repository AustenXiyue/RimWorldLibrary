using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal sealed class SimpleLine : Line
{
	private readonly string _content;

	private readonly TextRunProperties _textProps;

	public override TextRun GetTextRun(int dcp)
	{
		TextRun textRun = ((dcp >= _content.Length) ? ((TextRun)new TextEndOfParagraph(Line._syntheticCharacterLength)) : ((TextRun)new TextCharacters(_content, dcp, _content.Length - dcp, _textProps)));
		if (textRun.Properties != null)
		{
			textRun.Properties.PixelsPerDip = base.PixelsPerDip;
		}
		return textRun;
	}

	public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
	{
		CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
		CultureInfo culture = null;
		if (dcp > 0)
		{
			characterBufferRange = new CharacterBufferRange(_content, 0, Math.Min(dcp, _content.Length));
			culture = _textProps.CultureInfo;
		}
		return new TextSpan<CultureSpecificCharacterBufferRange>(dcp, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
	}

	public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
	{
		return textSourceCharacterIndex;
	}

	internal SimpleLine(TextBlock owner, string content, TextRunProperties textProps)
		: base(owner)
	{
		_content = content;
		_textProps = textProps;
	}
}
