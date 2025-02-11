using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.FontFace;

internal interface IFontFamily
{
	IDictionary<XmlLanguage, string> Names { get; }

	double BaselineDesign { get; }

	double LineSpacingDesign { get; }

	double Baseline(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode);

	double LineSpacing(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode);

	ITypefaceMetrics GetTypefaceMetrics(FontStyle style, FontWeight weight, FontStretch stretch);

	IDeviceFont GetDeviceFont(FontStyle style, FontWeight weight, FontStretch stretch);

	bool GetMapTargetFamilyNameAndScale(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, double defaultSizeInEm, out int cchAdvance, out string targetFamilyName, out double scaleInEm);

	ICollection<Typeface> GetTypefaces(FontFamilyIdentifier familyIdentifier);
}
