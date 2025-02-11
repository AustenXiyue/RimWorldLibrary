using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal sealed class TypographyProperties : TextRunTypographyProperties
{
	private enum PropertyId
	{
		StandardLigatures,
		ContextualLigatures,
		DiscretionaryLigatures,
		HistoricalLigatures,
		CaseSensitiveForms,
		ContextualAlternates,
		HistoricalForms,
		Kerning,
		CapitalSpacing,
		StylisticSet1,
		StylisticSet2,
		StylisticSet3,
		StylisticSet4,
		StylisticSet5,
		StylisticSet6,
		StylisticSet7,
		StylisticSet8,
		StylisticSet9,
		StylisticSet10,
		StylisticSet11,
		StylisticSet12,
		StylisticSet13,
		StylisticSet14,
		StylisticSet15,
		StylisticSet16,
		StylisticSet17,
		StylisticSet18,
		StylisticSet19,
		StylisticSet20,
		SlashedZero,
		MathematicalGreek,
		EastAsianExpertForms,
		PropertyCount
	}

	private uint _idPropertySetFlags;

	private int _standardSwashes;

	private int _contextualSwashes;

	private int _stylisticAlternates;

	private int _annotationAlternates;

	private FontVariants _variant;

	private FontCapitals _capitals;

	private FontFraction _fraction;

	private FontNumeralStyle _numeralStyle;

	private FontNumeralAlignment _numeralAlignment;

	private FontEastAsianWidths _eastAsianWidths;

	private FontEastAsianLanguage _eastAsianLanguage;

	public override bool StandardLigatures => IsBooleanPropertySet(PropertyId.StandardLigatures);

	public override bool ContextualLigatures => IsBooleanPropertySet(PropertyId.ContextualLigatures);

	public override bool DiscretionaryLigatures => IsBooleanPropertySet(PropertyId.DiscretionaryLigatures);

	public override bool HistoricalLigatures => IsBooleanPropertySet(PropertyId.HistoricalLigatures);

	public override bool CaseSensitiveForms => IsBooleanPropertySet(PropertyId.CaseSensitiveForms);

	public override bool ContextualAlternates => IsBooleanPropertySet(PropertyId.ContextualAlternates);

	public override bool HistoricalForms => IsBooleanPropertySet(PropertyId.HistoricalForms);

	public override bool Kerning => IsBooleanPropertySet(PropertyId.Kerning);

	public override bool CapitalSpacing => IsBooleanPropertySet(PropertyId.CapitalSpacing);

	public override bool StylisticSet1 => IsBooleanPropertySet(PropertyId.StylisticSet1);

	public override bool StylisticSet2 => IsBooleanPropertySet(PropertyId.StylisticSet2);

	public override bool StylisticSet3 => IsBooleanPropertySet(PropertyId.StylisticSet3);

	public override bool StylisticSet4 => IsBooleanPropertySet(PropertyId.StylisticSet4);

	public override bool StylisticSet5 => IsBooleanPropertySet(PropertyId.StylisticSet5);

	public override bool StylisticSet6 => IsBooleanPropertySet(PropertyId.StylisticSet6);

	public override bool StylisticSet7 => IsBooleanPropertySet(PropertyId.StylisticSet7);

	public override bool StylisticSet8 => IsBooleanPropertySet(PropertyId.StylisticSet8);

	public override bool StylisticSet9 => IsBooleanPropertySet(PropertyId.StylisticSet9);

	public override bool StylisticSet10 => IsBooleanPropertySet(PropertyId.StylisticSet10);

	public override bool StylisticSet11 => IsBooleanPropertySet(PropertyId.StylisticSet11);

	public override bool StylisticSet12 => IsBooleanPropertySet(PropertyId.StylisticSet12);

	public override bool StylisticSet13 => IsBooleanPropertySet(PropertyId.StylisticSet13);

	public override bool StylisticSet14 => IsBooleanPropertySet(PropertyId.StylisticSet14);

	public override bool StylisticSet15 => IsBooleanPropertySet(PropertyId.StylisticSet15);

	public override bool StylisticSet16 => IsBooleanPropertySet(PropertyId.StylisticSet16);

	public override bool StylisticSet17 => IsBooleanPropertySet(PropertyId.StylisticSet17);

	public override bool StylisticSet18 => IsBooleanPropertySet(PropertyId.StylisticSet18);

	public override bool StylisticSet19 => IsBooleanPropertySet(PropertyId.StylisticSet19);

	public override bool StylisticSet20 => IsBooleanPropertySet(PropertyId.StylisticSet20);

	public override FontFraction Fraction => _fraction;

	public override bool SlashedZero => IsBooleanPropertySet(PropertyId.SlashedZero);

	public override bool MathematicalGreek => IsBooleanPropertySet(PropertyId.MathematicalGreek);

	public override bool EastAsianExpertForms => IsBooleanPropertySet(PropertyId.EastAsianExpertForms);

	public override FontVariants Variants => _variant;

	public override FontCapitals Capitals => _capitals;

	public override FontNumeralStyle NumeralStyle => _numeralStyle;

	public override FontNumeralAlignment NumeralAlignment => _numeralAlignment;

	public override FontEastAsianWidths EastAsianWidths => _eastAsianWidths;

	public override FontEastAsianLanguage EastAsianLanguage => _eastAsianLanguage;

	public override int StandardSwashes => _standardSwashes;

	public override int ContextualSwashes => _contextualSwashes;

	public override int StylisticAlternates => _stylisticAlternates;

	public override int AnnotationAlternates => _annotationAlternates;

	public TypographyProperties()
	{
		ResetProperties();
	}

	public void SetStandardLigatures(bool value)
	{
		SetBooleanProperty(PropertyId.StandardLigatures, value);
	}

	public void SetContextualLigatures(bool value)
	{
		SetBooleanProperty(PropertyId.ContextualLigatures, value);
	}

	public void SetDiscretionaryLigatures(bool value)
	{
		SetBooleanProperty(PropertyId.DiscretionaryLigatures, value);
	}

	public void SetHistoricalLigatures(bool value)
	{
		SetBooleanProperty(PropertyId.HistoricalLigatures, value);
	}

	public void SetCaseSensitiveForms(bool value)
	{
		SetBooleanProperty(PropertyId.CaseSensitiveForms, value);
	}

	public void SetContextualAlternates(bool value)
	{
		SetBooleanProperty(PropertyId.ContextualAlternates, value);
	}

	public void SetHistoricalForms(bool value)
	{
		SetBooleanProperty(PropertyId.HistoricalForms, value);
	}

	public void SetKerning(bool value)
	{
		SetBooleanProperty(PropertyId.Kerning, value);
	}

	public void SetCapitalSpacing(bool value)
	{
		SetBooleanProperty(PropertyId.CapitalSpacing, value);
	}

	public void SetStylisticSet1(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet1, value);
	}

	public void SetStylisticSet2(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet2, value);
	}

	public void SetStylisticSet3(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet3, value);
	}

	public void SetStylisticSet4(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet4, value);
	}

	public void SetStylisticSet5(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet5, value);
	}

	public void SetStylisticSet6(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet6, value);
	}

	public void SetStylisticSet7(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet7, value);
	}

	public void SetStylisticSet8(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet8, value);
	}

	public void SetStylisticSet9(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet9, value);
	}

	public void SetStylisticSet10(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet10, value);
	}

	public void SetStylisticSet11(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet11, value);
	}

	public void SetStylisticSet12(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet12, value);
	}

	public void SetStylisticSet13(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet13, value);
	}

	public void SetStylisticSet14(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet14, value);
	}

	public void SetStylisticSet15(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet15, value);
	}

	public void SetStylisticSet16(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet16, value);
	}

	public void SetStylisticSet17(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet17, value);
	}

	public void SetStylisticSet18(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet18, value);
	}

	public void SetStylisticSet19(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet19, value);
	}

	public void SetStylisticSet20(bool value)
	{
		SetBooleanProperty(PropertyId.StylisticSet20, value);
	}

	public void SetFraction(FontFraction value)
	{
		_fraction = value;
		OnPropertiesChanged();
	}

	public void SetSlashedZero(bool value)
	{
		SetBooleanProperty(PropertyId.SlashedZero, value);
	}

	public void SetMathematicalGreek(bool value)
	{
		SetBooleanProperty(PropertyId.MathematicalGreek, value);
	}

	public void SetEastAsianExpertForms(bool value)
	{
		SetBooleanProperty(PropertyId.EastAsianExpertForms, value);
	}

	public void SetVariants(FontVariants value)
	{
		_variant = value;
		OnPropertiesChanged();
	}

	public void SetCapitals(FontCapitals value)
	{
		_capitals = value;
		OnPropertiesChanged();
	}

	public void SetNumeralStyle(FontNumeralStyle value)
	{
		_numeralStyle = value;
		OnPropertiesChanged();
	}

	public void SetNumeralAlignment(FontNumeralAlignment value)
	{
		_numeralAlignment = value;
		OnPropertiesChanged();
	}

	public void SetEastAsianWidths(FontEastAsianWidths value)
	{
		_eastAsianWidths = value;
		OnPropertiesChanged();
	}

	public void SetEastAsianLanguage(FontEastAsianLanguage value)
	{
		_eastAsianLanguage = value;
		OnPropertiesChanged();
	}

	public void SetStandardSwashes(int value)
	{
		_standardSwashes = value;
		OnPropertiesChanged();
	}

	public void SetContextualSwashes(int value)
	{
		_contextualSwashes = value;
		OnPropertiesChanged();
	}

	public void SetStylisticAlternates(int value)
	{
		_stylisticAlternates = value;
		OnPropertiesChanged();
	}

	public void SetAnnotationAlternates(int value)
	{
		_annotationAlternates = value;
		OnPropertiesChanged();
	}

	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (GetType() != other.GetType())
		{
			return false;
		}
		TypographyProperties typographyProperties = (TypographyProperties)other;
		if (_idPropertySetFlags == typographyProperties._idPropertySetFlags && _variant == typographyProperties._variant && _capitals == typographyProperties._capitals && _fraction == typographyProperties._fraction && _numeralStyle == typographyProperties._numeralStyle && _numeralAlignment == typographyProperties._numeralAlignment && _eastAsianWidths == typographyProperties._eastAsianWidths && _eastAsianLanguage == typographyProperties._eastAsianLanguage && _standardSwashes == typographyProperties._standardSwashes && _contextualSwashes == typographyProperties._contextualSwashes && _stylisticAlternates == typographyProperties._stylisticAlternates)
		{
			return _annotationAlternates == typographyProperties._annotationAlternates;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(_idPropertySetFlags ^ (_idPropertySetFlags & 0xFFFFFFFFu) ^ (uint)((int)_variant << 28) ^ (uint)((int)_capitals << 24) ^ (uint)((int)_numeralStyle << 20) ^ (uint)((int)_numeralAlignment << 18) ^ (uint)((int)_eastAsianWidths << 14) ^ (uint)((int)_eastAsianLanguage << 10) ^ (uint)(_standardSwashes << 6) ^ (uint)(_contextualSwashes << 2) ^ (uint)_stylisticAlternates ^ (uint)((int)_fraction << 16)) ^ (_annotationAlternates << 12);
	}

	public static bool operator ==(TypographyProperties first, TypographyProperties second)
	{
		return first?.Equals(second) ?? ((object)second == null);
	}

	public static bool operator !=(TypographyProperties first, TypographyProperties second)
	{
		return !(first == second);
	}

	private void ResetProperties()
	{
		_idPropertySetFlags = 0u;
		_standardSwashes = 0;
		_contextualSwashes = 0;
		_stylisticAlternates = 0;
		_annotationAlternates = 0;
		_variant = FontVariants.Normal;
		_capitals = FontCapitals.Normal;
		_numeralStyle = FontNumeralStyle.Normal;
		_numeralAlignment = FontNumeralAlignment.Normal;
		_eastAsianWidths = FontEastAsianWidths.Normal;
		_eastAsianLanguage = FontEastAsianLanguage.Normal;
		_fraction = FontFraction.Normal;
		OnPropertiesChanged();
	}

	private bool IsBooleanPropertySet(PropertyId propertyId)
	{
		uint num = (uint)(1 << (int)propertyId);
		return (_idPropertySetFlags & num) != 0;
	}

	private void SetBooleanProperty(PropertyId propertyId, bool flagValue)
	{
		uint num = (uint)(1 << (int)propertyId);
		if (flagValue)
		{
			_idPropertySetFlags |= num;
		}
		else
		{
			_idPropertySetFlags &= ~num;
		}
		OnPropertiesChanged();
	}
}
