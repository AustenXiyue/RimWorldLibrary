using MS.Internal.Text;

namespace System.Windows.Documents;

/// <summary>Provides access to a rich set of OpenType typography properties.</summary>
public sealed class Typography
{
	private static readonly Type _typeofThis;

	private static readonly Type _typeofBool;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> attached property.</returns>
	public static readonly DependencyProperty StandardLigaturesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> attached property.</returns>
	public static readonly DependencyProperty ContextualLigaturesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> attached property.</returns>
	public static readonly DependencyProperty DiscretionaryLigaturesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> attached property.</returns>
	public static readonly DependencyProperty HistoricalLigaturesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> attached property.</returns>
	public static readonly DependencyProperty AnnotationAlternatesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> attached property.</returns>
	public static readonly DependencyProperty ContextualAlternatesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> attached property.</returns>
	public static readonly DependencyProperty HistoricalFormsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.Kerning" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.Kerning" /> attached property.</returns>
	public static readonly DependencyProperty KerningProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> attached property.</returns>
	public static readonly DependencyProperty CapitalSpacingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> attached property.</returns>
	public static readonly DependencyProperty CaseSensitiveFormsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet1Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet2Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet3Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet4Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet5Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet6Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet7Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet8Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet9" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet9" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet9Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet10Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet11Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet12Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet13Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet14Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet15Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet16Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet17Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet18Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet19Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> attached property.</returns>
	public static readonly DependencyProperty StylisticSet20Property;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.Fraction" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.Fraction" /> attached property.</returns>
	public static readonly DependencyProperty FractionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> attached property.</returns>
	public static readonly DependencyProperty SlashedZeroProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> attached property.</returns>
	public static readonly DependencyProperty MathematicalGreekProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> attached property.</returns>
	public static readonly DependencyProperty EastAsianExpertFormsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.Variants" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.Variants" /> attached property.</returns>
	public static readonly DependencyProperty VariantsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.Capitals" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.Capitals" /> attached property.</returns>
	public static readonly DependencyProperty CapitalsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> attached property.</returns>
	public static readonly DependencyProperty NumeralStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> attached property.</returns>
	public static readonly DependencyProperty NumeralAlignmentProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> attached property.</returns>
	public static readonly DependencyProperty EastAsianWidthsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> attached property.</returns>
	public static readonly DependencyProperty EastAsianLanguageProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> attached property.</returns>
	public static readonly DependencyProperty StandardSwashesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> attached property.</returns>
	public static readonly DependencyProperty ContextualSwashesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> attached property.</summary>
	/// <returns>The identifier for <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> attached property.</returns>
	public static readonly DependencyProperty StylisticAlternatesProperty;

	internal static DependencyProperty[] TypographyPropertiesList;

	internal static readonly TypographyProperties Default;

	private DependencyObject _owner;

	/// <summary>Gets or sets a value that indicates whether standard ligatures are enabled. </summary>
	/// <returns>true if standard ligatures are enabled; otherwise, false. The default value is true.</returns>
	public bool StandardLigatures
	{
		get
		{
			return (bool)_owner.GetValue(StandardLigaturesProperty);
		}
		set
		{
			_owner.SetValue(StandardLigaturesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether contextual ligatures are enabled. </summary>
	/// <returns>true if contextual ligatures are enabled; otherwise, false. The default value is true.</returns>
	public bool ContextualLigatures
	{
		get
		{
			return (bool)_owner.GetValue(ContextualLigaturesProperty);
		}
		set
		{
			_owner.SetValue(ContextualLigaturesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether discretionary ligatures are enabled. </summary>
	/// <returns>true if discretionary ligatures are enabled; otherwise, false. The default value is false.</returns>
	public bool DiscretionaryLigatures
	{
		get
		{
			return (bool)_owner.GetValue(DiscretionaryLigaturesProperty);
		}
		set
		{
			_owner.SetValue(DiscretionaryLigaturesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether historical ligatures are enabled. </summary>
	/// <returns>true if historical ligatures are enabled; otherwise, false. The default value is false.</returns>
	public bool HistoricalLigatures
	{
		get
		{
			return (bool)_owner.GetValue(HistoricalLigaturesProperty);
		}
		set
		{
			_owner.SetValue(HistoricalLigaturesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the index of an alternate annotation form. </summary>
	/// <returns>The index of the alternate annotation form. The default value is 0 (zero).</returns>
	public int AnnotationAlternates
	{
		get
		{
			return (int)_owner.GetValue(AnnotationAlternatesProperty);
		}
		set
		{
			_owner.SetValue(AnnotationAlternatesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether custom glyph forms can be used based upon the context of the text being rendered. </summary>
	/// <returns>true if custom glyph forms can be used; otherwise, false. The default value is true.</returns>
	public bool ContextualAlternates
	{
		get
		{
			return (bool)_owner.GetValue(ContextualAlternatesProperty);
		}
		set
		{
			_owner.SetValue(ContextualAlternatesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether historical forms are enabled. </summary>
	/// <returns>true if historical forms are enabled; otherwise, false. The default value is false.</returns>
	public bool HistoricalForms
	{
		get
		{
			return (bool)_owner.GetValue(HistoricalFormsProperty);
		}
		set
		{
			_owner.SetValue(HistoricalFormsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether kerning is enabled. </summary>
	/// <returns>true if kerning is enabled; otherwise, false. The default value is true.</returns>
	public bool Kerning
	{
		get
		{
			return (bool)_owner.GetValue(KerningProperty);
		}
		set
		{
			_owner.SetValue(KerningProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether inter-glyph spacing for all-capital text is globally adjusted to improve readability. </summary>
	/// <returns>true if spacing is adjusted; otherwise, false. The default value is false.</returns>
	public bool CapitalSpacing
	{
		get
		{
			return (bool)_owner.GetValue(CapitalSpacingProperty);
		}
		set
		{
			_owner.SetValue(CapitalSpacingProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether glyphs adjust their vertical position to better align with uppercase glyphs. </summary>
	/// <returns>true if the vertical position is adjusted; otherwise, false. The default value is false.</returns>
	public bool CaseSensitiveForms
	{
		get
		{
			return (bool)_owner.GetValue(CaseSensitiveFormsProperty);
		}
		set
		{
			_owner.SetValue(CaseSensitiveFormsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet1
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet1Property);
		}
		set
		{
			_owner.SetValue(StylisticSet1Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet2
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet2Property);
		}
		set
		{
			_owner.SetValue(StylisticSet2Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet3
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet3Property);
		}
		set
		{
			_owner.SetValue(StylisticSet3Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet4
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet4Property);
		}
		set
		{
			_owner.SetValue(StylisticSet4Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet5
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet5Property);
		}
		set
		{
			_owner.SetValue(StylisticSet5Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet6
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet6Property);
		}
		set
		{
			_owner.SetValue(StylisticSet6Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet7
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet7Property);
		}
		set
		{
			_owner.SetValue(StylisticSet7Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet8
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet8Property);
		}
		set
		{
			_owner.SetValue(StylisticSet8Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet9
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet9Property);
		}
		set
		{
			_owner.SetValue(StylisticSet9Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet10
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet10Property);
		}
		set
		{
			_owner.SetValue(StylisticSet10Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet11
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet11Property);
		}
		set
		{
			_owner.SetValue(StylisticSet11Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet12
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet12Property);
		}
		set
		{
			_owner.SetValue(StylisticSet12Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet13
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet13Property);
		}
		set
		{
			_owner.SetValue(StylisticSet13Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet14
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet14Property);
		}
		set
		{
			_owner.SetValue(StylisticSet14Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet15
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet15Property);
		}
		set
		{
			_owner.SetValue(StylisticSet15Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet16
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet16Property);
		}
		set
		{
			_owner.SetValue(StylisticSet16Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet17
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet17Property);
		}
		set
		{
			_owner.SetValue(StylisticSet17Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet18
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet18Property);
		}
		set
		{
			_owner.SetValue(StylisticSet18Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet19
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet19Property);
		}
		set
		{
			_owner.SetValue(StylisticSet19Property, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a stylistic set of a font form is enabled. </summary>
	/// <returns>true if the stylistic set of the font form is enabled; otherwise, false. The default value is false.</returns>
	public bool StylisticSet20
	{
		get
		{
			return (bool)_owner.GetValue(StylisticSet20Property);
		}
		set
		{
			_owner.SetValue(StylisticSet20Property, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontFraction" /> enumerated value that indicates the fraction style. </summary>
	/// <returns>A <see cref="T:System.Windows.FontFraction" /> enumerated value. The default value is <see cref="F:System.Windows.FontFraction.Normal" />.</returns>
	public FontFraction Fraction
	{
		get
		{
			return (FontFraction)_owner.GetValue(FractionProperty);
		}
		set
		{
			_owner.SetValue(FractionProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a nominal zero font form should be replaced with a slashed zero. </summary>
	/// <returns>true if slashed zero forms are enabled; otherwise, false. The default value is false.</returns>
	public bool SlashedZero
	{
		get
		{
			return (bool)_owner.GetValue(SlashedZeroProperty);
		}
		set
		{
			_owner.SetValue(SlashedZeroProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether standard typographic font forms of Greek glyphs have been replaced with corresponding font forms commonly used in mathematical notation. </summary>
	/// <returns>true if mathematical Greek forms are enabled; otherwise, false. The default value is false.</returns>
	public bool MathematicalGreek
	{
		get
		{
			return (bool)_owner.GetValue(MathematicalGreekProperty);
		}
		set
		{
			_owner.SetValue(MathematicalGreekProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines whether the standard Japanese font forms have been replaced with the corresponding preferred typographic forms. </summary>
	/// <returns>true if standard Japanese font forms have been replaced with the corresponding preferred typographic forms; otherwise, false. The default value is false.</returns>
	public bool EastAsianExpertForms
	{
		get
		{
			return (bool)_owner.GetValue(EastAsianExpertFormsProperty);
		}
		set
		{
			_owner.SetValue(EastAsianExpertFormsProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontVariants" /> enumerated value that indicates a variation of the standard typographic form to be used. </summary>
	/// <returns>A <see cref="T:System.Windows.FontVariants" /> enumerated value. The default value is <see cref="F:System.Windows.FontVariants.Normal" />.</returns>
	public FontVariants Variants
	{
		get
		{
			return (FontVariants)_owner.GetValue(VariantsProperty);
		}
		set
		{
			_owner.SetValue(VariantsProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontCapitals" /> enumerated value that indicates the capital form of the selected font. </summary>
	/// <returns>A <see cref="T:System.Windows.FontCapitals" /> enumerated value. The default value is <see cref="F:System.Windows.FontCapitals.Normal" />.</returns>
	public FontCapitals Capitals
	{
		get
		{
			return (FontCapitals)_owner.GetValue(CapitalsProperty);
		}
		set
		{
			_owner.SetValue(CapitalsProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontNumeralStyle" /> enumerated value that determines the set of glyphs that are used to render numeric alternate font forms. </summary>
	/// <returns>A <see cref="T:System.Windows.FontNumeralStyle" /> enumerated value. The default value is <see cref="F:System.Windows.FontNumeralStyle.Normal" />.</returns>
	public FontNumeralStyle NumeralStyle
	{
		get
		{
			return (FontNumeralStyle)_owner.GetValue(NumeralStyleProperty);
		}
		set
		{
			_owner.SetValue(NumeralStyleProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontNumeralAlignment" /> enumerated value that indicates the alighnment of widths when using numerals. </summary>
	/// <returns>A <see cref="T:System.Windows.FontNumeralAlignment" /> enumerated value. The default value is <see cref="F:System.Windows.FontNumeralAlignment.Normal" />.</returns>
	public FontNumeralAlignment NumeralAlignment
	{
		get
		{
			return (FontNumeralAlignment)_owner.GetValue(NumeralAlignmentProperty);
		}
		set
		{
			_owner.SetValue(NumeralAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontEastAsianWidths" /> enumerated value that indicates the proportional width to be used for Latin characters in an East Asian font. </summary>
	/// <returns>A <see cref="T:System.Windows.FontEastAsianWidths" /> enumerated value. The default value is <see cref="F:System.Windows.FontEastAsianWidths.Normal" />.</returns>
	public FontEastAsianWidths EastAsianWidths
	{
		get
		{
			return (FontEastAsianWidths)_owner.GetValue(EastAsianWidthsProperty);
		}
		set
		{
			_owner.SetValue(EastAsianWidthsProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.FontEastAsianLanguage" /> enumerated value that indicates the version of glyphs to be used for a specific writing system or language. </summary>
	/// <returns>A <see cref="T:System.Windows.FontEastAsianLanguage" /> enumerated value. The default value is <see cref="F:System.Windows.FontEastAsianLanguage.Normal" />.</returns>
	public FontEastAsianLanguage EastAsianLanguage
	{
		get
		{
			return (FontEastAsianLanguage)_owner.GetValue(EastAsianLanguageProperty);
		}
		set
		{
			_owner.SetValue(EastAsianLanguageProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the index of a standard swashes form. </summary>
	/// <returns>The index of the standard swashes form. The default value is 0 (zero).</returns>
	public int StandardSwashes
	{
		get
		{
			return (int)_owner.GetValue(StandardSwashesProperty);
		}
		set
		{
			_owner.SetValue(StandardSwashesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the index of a contextual swashes form. </summary>
	/// <returns>The index of the standard swashes form. The default value is 0 (zero).</returns>
	public int ContextualSwashes
	{
		get
		{
			return (int)_owner.GetValue(ContextualSwashesProperty);
		}
		set
		{
			_owner.SetValue(ContextualSwashesProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the index of a stylistic alternates form. </summary>
	/// <returns>The index of the stylistic alternates form. The default value is 0 (zero).</returns>
	public int StylisticAlternates
	{
		get
		{
			return (int)_owner.GetValue(StylisticAlternatesProperty);
		}
		set
		{
			_owner.SetValue(StylisticAlternatesProperty, value);
		}
	}

	internal Typography(DependencyObject owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
	}

	static Typography()
	{
		_typeofThis = typeof(Typography);
		_typeofBool = typeof(bool);
		StandardLigaturesProperty = DependencyProperty.RegisterAttached("StandardLigatures", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		ContextualLigaturesProperty = DependencyProperty.RegisterAttached("ContextualLigatures", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		DiscretionaryLigaturesProperty = DependencyProperty.RegisterAttached("DiscretionaryLigatures", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		HistoricalLigaturesProperty = DependencyProperty.RegisterAttached("HistoricalLigatures", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		AnnotationAlternatesProperty = DependencyProperty.RegisterAttached("AnnotationAlternates", typeof(int), _typeofThis, new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		ContextualAlternatesProperty = DependencyProperty.RegisterAttached("ContextualAlternates", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		HistoricalFormsProperty = DependencyProperty.RegisterAttached("HistoricalForms", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		KerningProperty = DependencyProperty.RegisterAttached("Kerning", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		CapitalSpacingProperty = DependencyProperty.RegisterAttached("CapitalSpacing", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		CaseSensitiveFormsProperty = DependencyProperty.RegisterAttached("CaseSensitiveForms", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet1Property = DependencyProperty.RegisterAttached("StylisticSet1", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet2Property = DependencyProperty.RegisterAttached("StylisticSet2", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet3Property = DependencyProperty.RegisterAttached("StylisticSet3", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet4Property = DependencyProperty.RegisterAttached("StylisticSet4", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet5Property = DependencyProperty.RegisterAttached("StylisticSet5", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet6Property = DependencyProperty.RegisterAttached("StylisticSet6", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet7Property = DependencyProperty.RegisterAttached("StylisticSet7", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet8Property = DependencyProperty.RegisterAttached("StylisticSet8", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet9Property = DependencyProperty.RegisterAttached("StylisticSet9", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet10Property = DependencyProperty.RegisterAttached("StylisticSet10", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet11Property = DependencyProperty.RegisterAttached("StylisticSet11", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet12Property = DependencyProperty.RegisterAttached("StylisticSet12", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet13Property = DependencyProperty.RegisterAttached("StylisticSet13", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet14Property = DependencyProperty.RegisterAttached("StylisticSet14", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet15Property = DependencyProperty.RegisterAttached("StylisticSet15", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet16Property = DependencyProperty.RegisterAttached("StylisticSet16", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet17Property = DependencyProperty.RegisterAttached("StylisticSet17", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet18Property = DependencyProperty.RegisterAttached("StylisticSet18", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet19Property = DependencyProperty.RegisterAttached("StylisticSet19", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticSet20Property = DependencyProperty.RegisterAttached("StylisticSet20", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		FractionProperty = DependencyProperty.RegisterAttached("Fraction", typeof(FontFraction), _typeofThis, new FrameworkPropertyMetadata(FontFraction.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		SlashedZeroProperty = DependencyProperty.RegisterAttached("SlashedZero", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		MathematicalGreekProperty = DependencyProperty.RegisterAttached("MathematicalGreek", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		EastAsianExpertFormsProperty = DependencyProperty.RegisterAttached("EastAsianExpertForms", _typeofBool, _typeofThis, new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		VariantsProperty = DependencyProperty.RegisterAttached("Variants", typeof(FontVariants), _typeofThis, new FrameworkPropertyMetadata(FontVariants.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		CapitalsProperty = DependencyProperty.RegisterAttached("Capitals", typeof(FontCapitals), _typeofThis, new FrameworkPropertyMetadata(FontCapitals.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		NumeralStyleProperty = DependencyProperty.RegisterAttached("NumeralStyle", typeof(FontNumeralStyle), _typeofThis, new FrameworkPropertyMetadata(FontNumeralStyle.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		NumeralAlignmentProperty = DependencyProperty.RegisterAttached("NumeralAlignment", typeof(FontNumeralAlignment), _typeofThis, new FrameworkPropertyMetadata(FontNumeralAlignment.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		EastAsianWidthsProperty = DependencyProperty.RegisterAttached("EastAsianWidths", typeof(FontEastAsianWidths), _typeofThis, new FrameworkPropertyMetadata(FontEastAsianWidths.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		EastAsianLanguageProperty = DependencyProperty.RegisterAttached("EastAsianLanguage", typeof(FontEastAsianLanguage), _typeofThis, new FrameworkPropertyMetadata(FontEastAsianLanguage.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StandardSwashesProperty = DependencyProperty.RegisterAttached("StandardSwashes", typeof(int), _typeofThis, new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		ContextualSwashesProperty = DependencyProperty.RegisterAttached("ContextualSwashes", typeof(int), _typeofThis, new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		StylisticAlternatesProperty = DependencyProperty.RegisterAttached("StylisticAlternates", typeof(int), _typeofThis, new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
		TypographyPropertiesList = new DependencyProperty[43]
		{
			StandardLigaturesProperty, ContextualLigaturesProperty, DiscretionaryLigaturesProperty, HistoricalLigaturesProperty, AnnotationAlternatesProperty, ContextualAlternatesProperty, HistoricalFormsProperty, KerningProperty, CapitalSpacingProperty, CaseSensitiveFormsProperty,
			StylisticSet1Property, StylisticSet2Property, StylisticSet3Property, StylisticSet4Property, StylisticSet5Property, StylisticSet6Property, StylisticSet7Property, StylisticSet8Property, StylisticSet9Property, StylisticSet10Property,
			StylisticSet11Property, StylisticSet12Property, StylisticSet13Property, StylisticSet14Property, StylisticSet15Property, StylisticSet16Property, StylisticSet17Property, StylisticSet18Property, StylisticSet19Property, StylisticSet20Property,
			FractionProperty, SlashedZeroProperty, MathematicalGreekProperty, EastAsianExpertFormsProperty, VariantsProperty, CapitalsProperty, NumeralStyleProperty, NumeralAlignmentProperty, EastAsianWidthsProperty, EastAsianLanguageProperty,
			StandardSwashesProperty, ContextualSwashesProperty, StylisticAlternatesProperty
		};
		Default = new TypographyProperties();
		Default.SetStandardLigatures(value: true);
		Default.SetContextualAlternates(value: true);
		Default.SetContextualLigatures(value: true);
		Default.SetKerning(value: true);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStandardLigatures(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StandardLigaturesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StandardLigatures" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStandardLigatures(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StandardLigaturesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetContextualLigatures(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ContextualLigaturesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.ContextualLigatures" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetContextualLigatures(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(ContextualLigaturesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetDiscretionaryLigatures(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(DiscretionaryLigaturesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.DiscretionaryLigatures" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetDiscretionaryLigatures(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(DiscretionaryLigaturesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetHistoricalLigatures(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HistoricalLigaturesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalLigatures" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetHistoricalLigatures(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(HistoricalLigaturesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetAnnotationAlternates(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(AnnotationAlternatesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.TextElement.FontFamily" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.AnnotationAlternates" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetAnnotationAlternates(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(AnnotationAlternatesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetContextualAlternates(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ContextualAlternatesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.ContextualAlternates" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetContextualAlternates(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(ContextualAlternatesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetHistoricalForms(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HistoricalFormsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.HistoricalForms" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetHistoricalForms(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(HistoricalFormsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.Kerning" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.Kerning" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetKerning(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(KerningProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.Kerning" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.Kerning" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.Kerning" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetKerning(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(KerningProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetCapitalSpacing(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CapitalSpacingProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.CapitalSpacing" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetCapitalSpacing(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(CapitalSpacingProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetCaseSensitiveForms(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CaseSensitiveFormsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.CaseSensitiveForms" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetCaseSensitiveForms(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(CaseSensitiveFormsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet1(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet1Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet1" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet1(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet1Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet2(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet2Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet2" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet2(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet2Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet3(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet3Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet3" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet3(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet3Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet4(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet4Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet4" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet4(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet4Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet5(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet5Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet5" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet5(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet5Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet6(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet6Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet6" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet6(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet6Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet7(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet7Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet7" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet7(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet7Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet8(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet8Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet8(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet8Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet9" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet9" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet9(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet9Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet8" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet9(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet9Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet10(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet10Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet10" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet10(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet10Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet11(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet11Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet11" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet11(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet11Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet12(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet12Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet12" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet12(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet12Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet13(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet13Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet13" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet13(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet13Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet14(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet14Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet14" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet14(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet14Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet15(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet15Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet15" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet15(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet15Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet16(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet16Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet16" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet16(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet16Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet17(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet17Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet17" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet17(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet17Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet18(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet18Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet18" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet18(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet18Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet19(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet19Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet19" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet19(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet19Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticSet20(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticSet20Property, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticSet20" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetStylisticSet20(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(StylisticSet20Property);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.Fraction" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.Fraction" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetFraction(DependencyObject element, FontFraction value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FractionProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.Fraction" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.Fraction" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.Fraction" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontFraction GetFraction(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontFraction)element.GetValue(FractionProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetSlashedZero(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(SlashedZeroProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.SlashedZero" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetSlashedZero(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(SlashedZeroProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetMathematicalGreek(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(MathematicalGreekProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.MathematicalGreek" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetMathematicalGreek(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(MathematicalGreekProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetEastAsianExpertForms(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(EastAsianExpertFormsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianExpertForms" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetEastAsianExpertForms(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(EastAsianExpertFormsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.Variants" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.Variants" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetVariants(DependencyObject element, FontVariants value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(VariantsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.Variants" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.Variants" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.Variants" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontVariants GetVariants(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontVariants)element.GetValue(VariantsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.Capitals" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.Capitals" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetCapitals(DependencyObject element, FontCapitals value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CapitalsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.Capitals" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.Capitals" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.Capitals" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontCapitals GetCapitals(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontCapitals)element.GetValue(CapitalsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetNumeralStyle(DependencyObject element, FontNumeralStyle value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(NumeralStyleProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.NumeralStyle" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontNumeralStyle GetNumeralStyle(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontNumeralStyle)element.GetValue(NumeralStyleProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetNumeralAlignment(DependencyObject element, FontNumeralAlignment value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(NumeralAlignmentProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.NumeralAlignment" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontNumeralAlignment GetNumeralAlignment(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontNumeralAlignment)element.GetValue(NumeralAlignmentProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetEastAsianWidths(DependencyObject element, FontEastAsianWidths value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(EastAsianWidthsProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianWidths" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontEastAsianWidths GetEastAsianWidths(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontEastAsianWidths)element.GetValue(EastAsianWidthsProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetEastAsianLanguage(DependencyObject element, FontEastAsianLanguage value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(EastAsianLanguageProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.EastAsianLanguage" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static FontEastAsianLanguage GetEastAsianLanguage(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FontEastAsianLanguage)element.GetValue(EastAsianLanguageProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStandardSwashes(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StandardSwashesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StandardSwashes" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetStandardSwashes(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(StandardSwashesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetContextualSwashes(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ContextualSwashesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.ContextualSwashes" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetContextualSwashes(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(ContextualSwashesProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> attached property for a specified dependency object.</summary>
	/// <param name="element">The dependency object for which to set the value of the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> property.</param>
	/// <param name="value">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised when <paramref name="element" /> is null.</exception>
	public static void SetStylisticAlternates(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(StylisticAlternatesProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> attached property on the specified dependency object.</returns>
	/// <param name="element">The dependency object for which to retrieve the value of the <see cref="P:System.Windows.Documents.Typography.StylisticAlternates" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetStylisticAlternates(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(StylisticAlternatesProperty);
	}
}
