using System.Globalization;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.FontFace;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace System.Windows.Media;

/// <summary>Represents a combination of <see cref="T:System.Windows.Media.FontFamily" />, <see cref="T:System.Windows.FontWeight" />, <see cref="T:System.Windows.FontStyle" />, and <see cref="T:System.Windows.FontStretch" />.</summary>
public class Typeface
{
	private FontFamily _fontFamily;

	private FontStyle _style;

	private FontWeight _weight;

	private FontStretch _stretch;

	private FontFamily _fallbackFontFamily;

	private CachedTypeface _cachedTypeface;

	/// <summary>Gets the name of the font family from which the typeface was constructed. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.FontFamily" /> from which the typeface was constructed.</returns>
	public FontFamily FontFamily => _fontFamily;

	/// <summary>Gets the relative weight of the typeface.</summary>
	/// <returns>A <see cref="T:System.Windows.FontWeight" /> value that represents the relative weight of the typeface.</returns>
	public FontWeight Weight => _weight;

	/// <summary>Gets the style of the <see cref="T:System.Windows.Media.Typeface" />.</summary>
	/// <returns>A <see cref="T:System.Windows.FontStyle" /> value that represents the style value for the typeface.</returns>
	public FontStyle Style => _style;

	/// <summary>Gets the stretch value for the <see cref="T:System.Windows.Media.Typeface" />. The stretch value determines whether a typeface is expanded or condensed when it is displayed.</summary>
	/// <returns>A <see cref="T:System.Windows.FontStretch" /> value that represents the stretch value for the typeface.</returns>
	public FontStretch Stretch => _stretch;

	/// <summary>Determines whether to simulate an italic style for the glyphs represented by the <see cref="T:System.Windows.Media.Typeface" />.</summary>
	/// <returns>true if italic simulation is used for glyphs; otherwise, false.</returns>
	public bool IsObliqueSimulated => (CachedTypeface.TypefaceMetrics.StyleSimulations & StyleSimulations.ItalicSimulation) != 0;

	/// <summary>Determines whether to simulate a bold weight for the glyphs represented by the <see cref="T:System.Windows.Media.Typeface" />.</summary>
	/// <returns>true if bold simulation is used for glyphs; otherwise, false.</returns>
	public bool IsBoldSimulated => (CachedTypeface.TypefaceMetrics.StyleSimulations & StyleSimulations.BoldSimulation) != 0;

	internal FontFamily FallbackFontFamily => _fallbackFontFamily;

	/// <summary>Gets the distance from the baseline to the top of an English lowercase letter for a typeface. The distance excludes ascenders.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the distance from the baseline to the top of an English lowercase letter (excluding ascenders), expressed as a fraction of the font em size.</returns>
	public double XHeight => CachedTypeface.TypefaceMetrics.XHeight;

	/// <summary>Gets the distance from the baseline to the top of an English capital letter for the typeface.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the distance from the baseline to the top of an English capital letter, expressed as a fraction of the font em size.</returns>
	public double CapsHeight => CachedTypeface.TypefaceMetrics.CapsHeight;

	/// <summary>Gets a value that indicates the distance of the underline from the baseline for the typeface.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the underline position, measured from the baseline and expressed as a fraction of the font em size.</returns>
	public double UnderlinePosition => CachedTypeface.TypefaceMetrics.UnderlinePosition;

	/// <summary>Gets a value that indicates the thickness of the underline relative to the font em size for the typeface.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the underline thickness, expressed as a fraction of the font em size.</returns>
	public double UnderlineThickness => CachedTypeface.TypefaceMetrics.UnderlineThickness;

	/// <summary>Gets a value that indicates the distance from the baseline to the strikethrough for the typeface.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the strikethrough position, measured from the baseline and expressed as a fraction of the font em size.</returns>
	public double StrikethroughPosition => CachedTypeface.TypefaceMetrics.StrikethroughPosition;

	/// <summary>Gets a value that indicates the thickness of the strikethrough relative to the font em size.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the strikethrough thickness, expressed as a fraction of the font em size.</returns>
	public double StrikethroughThickness => CachedTypeface.TypefaceMetrics.StrikethroughThickness;

	/// <summary>Gets a collection of culture-specific names for the <see cref="T:System.Windows.Media.Typeface" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> value that represents the culture-specific names for the typeface.</returns>
	public LanguageSpecificStringDictionary FaceNames => new LanguageSpecificStringDictionary(CachedTypeface.TypefaceMetrics.AdjustedFaceNames);

	internal bool Symbol => CachedTypeface.TypefaceMetrics.Symbol;

	internal bool NullFont => CachedTypeface.NullFont;

	internal FontStyle CanonicalStyle => CachedTypeface.CanonicalStyle;

	internal FontWeight CanonicalWeight => CachedTypeface.CanonicalWeight;

	internal FontStretch CanonicalStretch => CachedTypeface.CanonicalStretch;

	private CachedTypeface CachedTypeface
	{
		get
		{
			if (_cachedTypeface == null)
			{
				CachedTypeface cachedTypeface = TypefaceMetricsCache.ReadonlyLookup(this) as CachedTypeface;
				if (cachedTypeface == null)
				{
					cachedTypeface = ConstructCachedTypeface();
					TypefaceMetricsCache.Add(this, cachedTypeface);
				}
				_cachedTypeface = cachedTypeface;
			}
			return _cachedTypeface;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Typeface" /> class for the specified font family typeface name.</summary>
	/// <param name="typefaceName">The typeface name for the specified font family.</param>
	public Typeface(string typefaceName)
		: this(new FontFamily(typefaceName), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Typeface" /> class for the specified font family name, <see cref="P:System.Windows.Media.Typeface.Style" />, <see cref="P:System.Windows.Media.Typeface.Weight" />, and <see cref="P:System.Windows.Media.Typeface.Stretch" /> values.</summary>
	/// <param name="fontFamily">The font family of the typeface.</param>
	/// <param name="style">The style of the typeface.</param>
	/// <param name="weight">The relative weight of the typeface.</param>
	/// <param name="stretch">The degree to which the typeface is stretched.</param>
	public Typeface(FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch)
		: this(fontFamily, style, weight, stretch, FontFamily.FontFamilyGlobalUI)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Typeface" /> class for the specified font family name, <see cref="P:System.Windows.Media.Typeface.Style" />, <see cref="P:System.Windows.Media.Typeface.Weight" />, and <see cref="P:System.Windows.Media.Typeface.Stretch" /> values. In addition, a fallback font family is specified. </summary>
	/// <param name="fontFamily">The font family of the typeface.</param>
	/// <param name="style">The style of the typeface.</param>
	/// <param name="weight">The relative weight of the typeface.</param>
	/// <param name="stretch">The degree to which the typeface is stretched.</param>
	/// <param name="fallbackFontFamily">The font family that is used when a character is encountered that the primary font family (specified by the <paramref name="fontFamily" /> parameter) cannot display.</param>
	public Typeface(FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, FontFamily fallbackFontFamily)
	{
		if (fontFamily == null)
		{
			throw new ArgumentNullException("fontFamily");
		}
		_fontFamily = fontFamily;
		_style = style;
		_weight = weight;
		_stretch = stretch;
		_fallbackFontFamily = fallbackFontFamily;
	}

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.GlyphTypeface" /> that corresponds to the <see cref="T:System.Windows.Media.Typeface" />.</summary>
	/// <returns>true if the out parameter is set to a <see cref="T:System.Windows.Media.GlyphTypeface" /> value; otherwise, false.</returns>
	/// <param name="glyphTypeface">
	///   <see cref="T:System.Windows.Media.GlyphTypeface" /> object that corresponds to this typeface, or null if the typeface was constructed from a composite font.</param>
	public bool TryGetGlyphTypeface(out GlyphTypeface glyphTypeface)
	{
		glyphTypeface = CachedTypeface.TypefaceMetrics as GlyphTypeface;
		return glyphTypeface != null;
	}

	internal double Baseline(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		return CachedTypeface.FirstFontFamily.Baseline(emSize, toReal, pixelsPerDip, textFormattingMode);
	}

	internal double LineSpacing(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		return CachedTypeface.FirstFontFamily.LineSpacing(emSize, toReal, pixelsPerDip, textFormattingMode);
	}

	internal GlyphTypeface TryGetGlyphTypeface()
	{
		return CachedTypeface.TypefaceMetrics as GlyphTypeface;
	}

	internal bool CheckFastPathNominalGlyphs(CharacterBufferRange charBufferRange, double emSize, float pixelsPerDip, double scalingFactor, double widthMax, bool keepAWord, bool numberSubstitution, CultureInfo cultureInfo, TextFormattingMode textFormattingMode, bool isSideways, bool breakOnTabs, out int stringLengthFit)
	{
		stringLengthFit = 0;
		if (CachedTypeface.NullFont)
		{
			return false;
		}
		GlyphTypeface glyphTypeface = TryGetGlyphTypeface();
		if (glyphTypeface == null)
		{
			return false;
		}
		double num = 0.0;
		int num2 = 0;
		ushort blankGlyphIndex = glyphTypeface.BlankGlyphIndex;
		ushort num3 = blankGlyphIndex;
		ushort num4 = (ushort)((!numberSubstitution) ? 1 : 257);
		ushort num5 = 0;
		ushort num6 = 48;
		bool symbol = glyphTypeface.Symbol;
		if (symbol)
		{
			num4 = 0;
		}
		bool flag = widthMax == double.MaxValue;
		ushort[] uShorts = BufferCache.GetUShorts(charBufferRange.Length);
		GlyphMetrics[] array = (flag ? null : BufferCache.GetGlyphMetrics(charBufferRange.Length));
		glyphTypeface.GetGlyphMetricsOptimized(charBufferRange, emSize, pixelsPerDip, uShorts, array, textFormattingMode, isSideways);
		double num7 = emSize / (double)(int)glyphTypeface.DesignEmHeight;
		if (keepAWord)
		{
			do
			{
				char c = charBufferRange[num2++];
				if (c == '\n' || c == '\r' || (breakOnTabs && c == '\t'))
				{
					num2--;
					break;
				}
				num5 = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(c)).Flags;
				num6 &= num5;
				num3 = uShorts[num2 - 1];
				if (!flag)
				{
					num += TextFormatterImp.RoundDip((double)array[num2 - 1].AdvanceWidth * num7, pixelsPerDip, textFormattingMode) * scalingFactor;
				}
			}
			while (num2 < charBufferRange.Length && (num5 & num4) == 0 && (num3 != 0 || symbol) && num3 != blankGlyphIndex);
		}
		while (num2 < charBufferRange.Length && (flag || num <= widthMax) && (num5 & num4) == 0 && (num3 != 0 || symbol))
		{
			char c2 = charBufferRange[num2++];
			if (c2 == '\n' || c2 == '\r' || (breakOnTabs && c2 == '\t'))
			{
				num2--;
				break;
			}
			num5 = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(c2)).Flags;
			num6 &= num5;
			num3 = uShorts[num2 - 1];
			if (!flag)
			{
				num += TextFormatterImp.RoundDip((double)array[num2 - 1].AdvanceWidth * num7, pixelsPerDip, textFormattingMode) * scalingFactor;
			}
		}
		BufferCache.ReleaseUShorts(uShorts);
		uShorts = null;
		BufferCache.ReleaseGlyphMetrics(array);
		array = null;
		if (symbol)
		{
			stringLengthFit = num2;
			return true;
		}
		if (num3 == 0)
		{
			return false;
		}
		if ((num5 & num4) != 0 && --num2 <= 0)
		{
			return false;
		}
		stringLengthFit = num2;
		TypographyAvailabilities typographyAvailabilities = glyphTypeface.FontFaceLayoutInfo.TypographyAvailabilities;
		if ((num6 & 0x10) != 0)
		{
			if ((typographyAvailabilities & (TypographyAvailabilities.FastTextTypographyAvailable | TypographyAvailabilities.FastTextMajorLanguageLocalizedFormAvailable)) != 0)
			{
				return false;
			}
			if ((typographyAvailabilities & TypographyAvailabilities.FastTextExtraLanguageLocalizedFormAvailable) != 0)
			{
				return MajorLanguages.Contains(cultureInfo);
			}
			return true;
		}
		if ((num6 & 0x20) != 0)
		{
			return (typographyAvailabilities & TypographyAvailabilities.IdeoTypographyAvailable) == 0;
		}
		return (typographyAvailabilities & TypographyAvailabilities.Available) == 0;
	}

	internal void GetCharacterNominalWidthsAndIdealWidth(CharacterBufferRange charBufferRange, double emSize, float pixelsPerDip, double toIdeal, TextFormattingMode textFormattingMode, bool isSideways, out int[] nominalWidths)
	{
		GetCharacterNominalWidthsAndIdealWidth(charBufferRange, emSize, pixelsPerDip, toIdeal, textFormattingMode, isSideways, out nominalWidths, out var _);
	}

	internal void GetCharacterNominalWidthsAndIdealWidth(CharacterBufferRange charBufferRange, double emSize, float pixelsPerDip, double toIdeal, TextFormattingMode textFormattingMode, bool isSideways, out int[] nominalWidths, out int idealWidth)
	{
		GlyphTypeface glyphTypeface = TryGetGlyphTypeface();
		Invariant.Assert(glyphTypeface != null);
		GlyphMetrics[] glyphMetrics = BufferCache.GetGlyphMetrics(charBufferRange.Length);
		glyphTypeface.GetGlyphMetricsOptimized(charBufferRange, emSize, pixelsPerDip, textFormattingMode, isSideways, glyphMetrics);
		nominalWidths = new int[charBufferRange.Length];
		idealWidth = 0;
		if (TextFormattingMode.Display == textFormattingMode)
		{
			double num = emSize / (double)(int)glyphTypeface.DesignEmHeight;
			for (int i = 0; i < charBufferRange.Length; i++)
			{
				nominalWidths[i] = (int)Math.Round(TextFormatterImp.RoundDipForDisplayMode((double)glyphMetrics[i].AdvanceWidth * num, pixelsPerDip) * toIdeal);
				idealWidth += nominalWidths[i];
			}
		}
		else
		{
			double num2 = emSize * toIdeal / (double)(int)glyphTypeface.DesignEmHeight;
			for (int j = 0; j < charBufferRange.Length; j++)
			{
				nominalWidths[j] = (int)Math.Round((double)glyphMetrics[j].AdvanceWidth * num2);
				idealWidth += nominalWidths[j];
			}
		}
		BufferCache.ReleaseGlyphMetrics(glyphMetrics);
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.Typeface" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int hash = _fontFamily.GetHashCode();
		if (_fallbackFontFamily != null)
		{
			hash = HashFn.HashMultiply(hash) + _fallbackFontFamily.GetHashCode();
		}
		hash = HashFn.HashMultiply(hash) + _style.GetHashCode();
		hash = HashFn.HashMultiply(hash) + _weight.GetHashCode();
		hash = HashFn.HashMultiply(hash) + _stretch.GetHashCode();
		return HashFn.HashScramble(hash);
	}

	/// <summary>Gets a value that indicates whether the current typeface and the specified typeface have the same <see cref="P:System.Windows.Media.Typeface.FontFamily" />, <see cref="P:System.Windows.Media.Typeface.Style" />, <see cref="P:System.Windows.Media.Typeface.Weight" />, <see cref="P:System.Windows.Media.Typeface.Stretch" />, and fallback font values. </summary>
	/// <returns>true if <paramref name="o" /> is equal to the current <see cref="T:System.Windows.Media.Typeface" /> object; otherwise, false. If <paramref name="o" /> is not a <see cref="T:System.Windows.Media.Typeface" /> object, false is returned.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Media.Typeface" /> to compare.</param>
	public override bool Equals(object o)
	{
		if (!(o is Typeface typeface))
		{
			return false;
		}
		if (_style == typeface._style && _weight == typeface._weight && _stretch == typeface._stretch && _fontFamily.Equals(typeface._fontFamily))
		{
			return CompareFallbackFontFamily(typeface._fallbackFontFamily);
		}
		return false;
	}

	internal bool CompareFallbackFontFamily(FontFamily fallbackFontFamily)
	{
		if (fallbackFontFamily == null || _fallbackFontFamily == null)
		{
			return fallbackFontFamily == _fallbackFontFamily;
		}
		return _fallbackFontFamily.Equals(fallbackFontFamily);
	}

	private CachedTypeface ConstructCachedTypeface()
	{
		FontStyle style = _style;
		FontWeight weight = _weight;
		FontStretch stretch = _stretch;
		FontFamily fontFamily = FontFamily;
		IFontFamily fontFamily2 = fontFamily.FindFirstFontFamilyAndFace(ref style, ref weight, ref stretch);
		if (fontFamily2 == null)
		{
			if (FallbackFontFamily != null)
			{
				fontFamily = FallbackFontFamily;
				fontFamily2 = fontFamily.FindFirstFontFamilyAndFace(ref style, ref weight, ref stretch);
			}
			if (fontFamily2 == null)
			{
				fontFamily = null;
				fontFamily2 = FontFamily.LookupFontFamily(FontFamily.NullFontFamilyCanonicalName);
			}
		}
		if (fontFamily != null && fontFamily.Source != null)
		{
			if (TypefaceMetricsCache.ReadonlyLookup(fontFamily.FamilyIdentifier) is IFontFamily fontFamily3)
			{
				fontFamily2 = fontFamily3;
			}
			else
			{
				TypefaceMetricsCache.Add(fontFamily.FamilyIdentifier, fontFamily2);
			}
		}
		ITypefaceMetrics typefaceMetrics = fontFamily2.GetTypefaceMetrics(style, weight, stretch);
		return new CachedTypeface(style, weight, stretch, fontFamily2, typefaceMetrics, fontFamily == null);
	}
}
