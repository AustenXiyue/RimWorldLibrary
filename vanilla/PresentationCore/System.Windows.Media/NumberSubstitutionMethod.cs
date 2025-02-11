namespace System.Windows.Media;

/// <summary>Defines an enumerator class that specifies the type of number substitution to perform on numbers in a text run.</summary>
public enum NumberSubstitutionMethod
{
	/// <summary>Default. Specifies that the substitution method should be determined based on the number culture's <see cref="P:System.Globalization.NumberFormatInfo.DigitSubstitution" /> property value.</summary>
	AsCulture,
	/// <summary>If the number culture is an Arabic or Farsi culture, specifies that the digits depend on the context. Either traditional or Latin digits are used depending on the nearest preceding strong character, or, if there is none, the text direction of the paragraph.</summary>
	Context,
	/// <summary>Specifies that code points 0x30-0x39 are always rendered as European digits, in which case, no number substitution is performed.</summary>
	European,
	/// <summary>Specifies that numbers are rendered using the national digits for the number culture, as specified by the culture's <see cref="P:System.Globalization.NumberFormatInfo.NativeDigits" /> property value.</summary>
	NativeNational,
	/// <summary>Specifies that numbers are rendered using the traditional digits for the number culture. For most cultures, this is the same as the <see cref="F:System.Globalization.DigitShapes.NativeNational" /> enumeration value. However, using <see cref="F:System.Windows.Media.NumberSubstitutionMethod.NativeNational" /> can result in Latin digits for some Arabic cultures, whereas using <see cref="F:System.Windows.Media.NumberSubstitutionMethod.Traditional" /> results in Arabic digits for all Arabic cultures.</summary>
	Traditional
}
