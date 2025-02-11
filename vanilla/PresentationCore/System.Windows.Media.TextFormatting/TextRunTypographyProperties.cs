using MS.Internal.Text.TextInterface;

namespace System.Windows.Media.TextFormatting;

/// <summary>Provides an abstract class for supporting typography properties for <see cref="T:System.Windows.Media.TextFormatting.TextRun" /> objects.</summary>
public abstract class TextRunTypographyProperties
{
	private DWriteFontFeature[] _features;

	/// <summary>Gets a value that indicates whether standard ligatures are enabled.</summary>
	/// <returns>true if standard ligatures are enabled; otherwise, false. The default is true.</returns>
	public abstract bool StandardLigatures { get; }

	/// <summary>Gets a value that indicates whether contextual ligatures are enabled.</summary>
	/// <returns>true if contextual ligatures are enabled; otherwise, false. The default is true.</returns>
	public abstract bool ContextualLigatures { get; }

	/// <summary>Gets a value that indicates whether discretionary ligatures are enabled.</summary>
	/// <returns>true if discretionary ligatures are enabled; otherwise, false. The default is false.</returns>
	public abstract bool DiscretionaryLigatures { get; }

	/// <summary>Gets a value that indicates whether historical ligatures are enabled.</summary>
	/// <returns>true if historical ligatures are enabled; otherwise, false. The default is false.</returns>
	public abstract bool HistoricalLigatures { get; }

	/// <summary>Gets a value that indicates whether custom glyph forms can be used based upon the context of the text being rendered.</summary>
	/// <returns>true if custom glyph forms can be used; otherwise, false. The default is true.</returns>
	public abstract bool ContextualAlternates { get; }

	/// <summary>Gets a value that indicates whether historical forms are enabled.</summary>
	/// <returns>true if historical forms are enabled; otherwise, false. The default is false.</returns>
	public abstract bool HistoricalForms { get; }

	/// <summary>Gets a value that indicates whether kerning is enabled.</summary>
	/// <returns>true if kerning is enabled; otherwise, false. The default is true.</returns>
	public abstract bool Kerning { get; }

	/// <summary>Gets a value that indicates whether inter-glyph spacing for all-capital text is globally adjusted to improve readability.</summary>
	/// <returns>true if spacing is adjusted; otherwise, false. The default is false.</returns>
	public abstract bool CapitalSpacing { get; }

	/// <summary>Gets a value that indicates whether glyphs adjust their vertical position to better align with uppercase glyphs.</summary>
	/// <returns>true if the vertical position is adjusted; otherwise, false. The default is false.</returns>
	public abstract bool CaseSensitiveForms { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet1 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet2 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet3 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet4 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet5 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet6 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet7 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet8 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet9 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet10 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet11 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet12 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet13 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet14 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet15 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet16 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet17 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet18 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet19 { get; }

	/// <summary>Gets a value that indicates whether a stylistic set of a font form is enabled.</summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default is false.</returns>
	public abstract bool StylisticSet20 { get; }

	/// <summary>Gets a value that indicates whether a nominal zero font form should be replaced with a slashed zero.</summary>
	/// <returns>true if slashed zero forms are enabled; otherwise, false. The default is false.</returns>
	public abstract bool SlashedZero { get; }

	/// <summary>Gets a value that indicates whether standard typographic font forms of Greek glyphs have been replaced with corresponding font forms commonly used in mathematical notation.</summary>
	/// <returns>true if mathematical Greek forms are enabled; otherwise, false. The default is false.</returns>
	public abstract bool MathematicalGreek { get; }

	/// <summary>Gets a value that indicates whether the standard Japanese font forms have been replaced with the corresponding preferred typographic forms.</summary>
	/// <returns>true if standard Japanese font forms have been replaced with the corresponding preferred typographic forms; otherwise, false. The default is false.</returns>
	public abstract bool EastAsianExpertForms { get; }

	/// <summary>Gets a value that indicates a variation of the standard typographic form to be used.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontVariants" /> values. The default is <see cref="F:System.Windows.FontVariants.Normal" />.</returns>
	public abstract FontVariants Variants { get; }

	/// <summary>Gets a value that indicates the capital form of the selected font.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontCapitals" /> values. The default is <see cref="F:System.Windows.FontCapitals.Normal" />.</returns>
	public abstract FontCapitals Capitals { get; }

	/// <summary>Gets a value that indicates the fraction style.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontFraction" /> values. The default is <see cref="F:System.Windows.FontFraction.Normal" />.</returns>
	public abstract FontFraction Fraction { get; }

	/// <summary>Gets a value that indicates the set of glyphs that are used to render numeric alternate font forms.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontNumeralStyle" /> values. The default is <see cref="F:System.Windows.FontNumeralStyle.Normal" />.</returns>
	public abstract FontNumeralStyle NumeralStyle { get; }

	/// <summary>Gets the alignment of widths when using numerals.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontNumeralAlignment" /> values. The default is <see cref="F:System.Windows.FontNumeralAlignment.Normal" />.</returns>
	public abstract FontNumeralAlignment NumeralAlignment { get; }

	/// <summary>Gets a value that indicates the proportional width to be used for Latin characters in an East Asian font.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontEastAsianWidths" /> values. The default is <see cref="F:System.Windows.FontEastAsianWidths.Normal" />.</returns>
	public abstract FontEastAsianWidths EastAsianWidths { get; }

	/// <summary>Gets a value that indicates the version of glyphs to be used for a specific writing system or language.</summary>
	/// <returns>One of the <see cref="T:System.Windows.FontEastAsianLanguage" /> values. The default is <see cref="F:System.Windows.FontEastAsianLanguage.Normal" />.</returns>
	public abstract FontEastAsianLanguage EastAsianLanguage { get; }

	/// <summary>Gets the index of a standard swashes form.</summary>
	/// <returns>The index of the standard swashes form. The default is 0 (zero).</returns>
	public abstract int StandardSwashes { get; }

	/// <summary>Gets a value that specifies the index of a contextual swashes form.</summary>
	/// <returns>The index of the standard swashes form. The default is 0 (zero).</returns>
	public abstract int ContextualSwashes { get; }

	/// <summary>Gets the index of a stylistic alternates form.</summary>
	/// <returns>The index of the stylistic alternates form. The default is 0 (zero).</returns>
	public abstract int StylisticAlternates { get; }

	/// <summary>Gets the index of an alternate annotation form.</summary>
	/// <returns>The index of the alternate annotation form. The default is 0 (zero).</returns>
	public abstract int AnnotationAlternates { get; }

	internal DWriteFontFeature[] CachedFeatureSet
	{
		get
		{
			return _features;
		}
		set
		{
			_features = value;
		}
	}

	/// <summary>Corrects internal state for a <see cref="T:System.Windows.Media.TextFormatting.TextRunTypographyProperties" /> derived class whenever any <see cref="T:System.Windows.Media.TextFormatting.TextRunTypographyProperties" /> property changes its value.</summary>
	protected void OnPropertiesChanged()
	{
		_features = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextRunTypographyProperties" /> class.</summary>
	protected TextRunTypographyProperties()
	{
	}
}
