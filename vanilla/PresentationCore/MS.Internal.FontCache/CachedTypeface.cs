using System.Windows;
using MS.Internal.FontFace;

namespace MS.Internal.FontCache;

internal class CachedTypeface
{
	private FontStyle _canonicalStyle;

	private FontWeight _canonicalWeight;

	private FontStretch _canonicalStretch;

	private IFontFamily _firstFontFamily;

	private ITypefaceMetrics _typefaceMetrics;

	private bool _nullFont;

	internal FontStyle CanonicalStyle => _canonicalStyle;

	internal FontWeight CanonicalWeight => _canonicalWeight;

	internal FontStretch CanonicalStretch => _canonicalStretch;

	internal IFontFamily FirstFontFamily => _firstFontFamily;

	internal ITypefaceMetrics TypefaceMetrics => _typefaceMetrics;

	internal bool NullFont => _nullFont;

	internal CachedTypeface(FontStyle canonicalStyle, FontWeight canonicalWeight, FontStretch canonicalStretch, IFontFamily firstFontFamily, ITypefaceMetrics typefaceMetrics, bool nullFont)
	{
		_canonicalStyle = canonicalStyle;
		_canonicalWeight = canonicalWeight;
		_canonicalStretch = canonicalStretch;
		Invariant.Assert(firstFontFamily != null && typefaceMetrics != null);
		_firstFontFamily = firstFontFamily;
		_typefaceMetrics = typefaceMetrics;
		_nullFont = nullFont;
	}
}
