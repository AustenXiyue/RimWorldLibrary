using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace MS.Internal.Shaping;

internal class GlyphingCache
{
	private SizeLimitedCache<Typeface, TypefaceMap> _sizeLimitedCache;

	internal GlyphingCache(int capacity)
	{
		_sizeLimitedCache = new SizeLimitedCache<Typeface, TypefaceMap>(capacity);
	}

	internal void GetShapeableText(Typeface typeface, CharacterBufferReference characterBufferReference, int stringLength, TextRunProperties textRunProperties, CultureInfo digitCulture, bool isRightToLeftParagraph, IList<TextShapeableSymbols> shapeableList, IShapeableTextCollector collector, TextFormattingMode textFormattingMode)
	{
		if (!typeface.Symbol)
		{
			Lookup(typeface).GetShapeableText(characterBufferReference, stringLength, textRunProperties, digitCulture, isRightToLeftParagraph, shapeableList, collector, textFormattingMode);
			return;
		}
		ShapeTypeface shapeTypeface = new ShapeTypeface(typeface.TryGetGlyphTypeface(), null);
		collector.Add(shapeableList, new CharacterBufferRange(characterBufferReference, stringLength), textRunProperties, new ItemProps(), shapeTypeface, 1.0, nullShape: false, textFormattingMode);
	}

	private TypefaceMap Lookup(Typeface key)
	{
		TypefaceMap typefaceMap = _sizeLimitedCache.Get(key);
		if (typefaceMap == null)
		{
			typefaceMap = new TypefaceMap(key.FontFamily, key.FallbackFontFamily, key.CanonicalStyle, key.CanonicalWeight, key.CanonicalStretch, key.NullFont);
			_sizeLimitedCache.Add(key, typefaceMap, isPermanent: false);
		}
		return typefaceMap;
	}
}
