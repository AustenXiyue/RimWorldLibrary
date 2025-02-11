using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class TextFormatterHost : TextSource
{
	internal LineBase Context;

	internal TextFormatter TextFormatter;

	internal TextFormatterHost(TextFormatter textFormatter, TextFormattingMode textFormattingMode, double pixelsPerDip)
	{
		if (textFormatter == null)
		{
			TextFormatter = TextFormatter.FromCurrentDispatcher(textFormattingMode);
		}
		else
		{
			TextFormatter = textFormatter;
		}
		base.PixelsPerDip = pixelsPerDip;
	}

	public override TextRun GetTextRun(int textSourceCharacterIndex)
	{
		TextRun textRun = Context.GetTextRun(textSourceCharacterIndex);
		if (textRun.Properties != null)
		{
			textRun.Properties.PixelsPerDip = base.PixelsPerDip;
		}
		return textRun;
	}

	public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
	{
		return Context.GetPrecedingText(textSourceCharacterIndexLimit);
	}

	public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
	{
		return Context.GetTextEffectCharacterIndexFromTextSourceCharacterIndex(textSourceCharacterIndex);
	}
}
