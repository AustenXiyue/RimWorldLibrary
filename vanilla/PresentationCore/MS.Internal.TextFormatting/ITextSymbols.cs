using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Shaping;

namespace MS.Internal.TextFormatting;

internal interface ITextSymbols
{
	IList<TextShapeableSymbols> GetTextShapeableSymbols(GlyphingCache glyphingCache, CharacterBufferReference characterBufferReference, int characterLength, bool rightToLeft, bool isRightToLeftParagraph, CultureInfo digitCulture, TextModifierScope textModifierScope, TextFormattingMode textFormattingMode, bool isSideways);
}
