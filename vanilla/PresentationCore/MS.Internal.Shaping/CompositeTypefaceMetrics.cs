using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.FontFace;

namespace MS.Internal.Shaping;

internal class CompositeTypefaceMetrics : ITypefaceMetrics
{
	private double _underlinePosition;

	private double _underlineThickness;

	private double _strikethroughPosition;

	private double _strikethroughThickenss;

	private double _capsHeight;

	private double _xHeight;

	private FontStyle _style;

	private FontWeight _weight;

	private FontStretch _stretch;

	private const double UnderlineOffsetDefaultInEm = -5.0 / 32.0;

	private const double UnderlineSizeDefaultInEm = 5.0 / 64.0;

	private const double StrikethroughOffsetDefaultInEm = 0.3125;

	private const double StrikethroughSizeDefaultInEm = 5.0 / 64.0;

	private const double CapsHeightDefaultInEm = 1.0;

	private const double XHeightDefaultInEm = 43.0 / 64.0;

	public double XHeight => _xHeight;

	public double CapsHeight => _capsHeight;

	public double UnderlinePosition => _underlinePosition;

	public double UnderlineThickness => _underlineThickness;

	public double StrikethroughPosition => _strikethroughPosition;

	public double StrikethroughThickness => _strikethroughThickenss;

	public bool Symbol => false;

	public StyleSimulations StyleSimulations => StyleSimulations.None;

	public IDictionary<XmlLanguage, string> AdjustedFaceNames => FontDifferentiator.ConstructFaceNamesByStyleWeightStretch(_style, _weight, _stretch);

	internal CompositeTypefaceMetrics(double underlinePosition, double underlineThickness, double strikethroughPosition, double strikethroughThickness, double capsHeight, double xHeight, FontStyle style, FontWeight weight, FontStretch stretch)
	{
		_underlinePosition = ((underlinePosition != 0.0) ? underlinePosition : (-5.0 / 32.0));
		_underlineThickness = ((underlineThickness > 0.0) ? underlineThickness : (5.0 / 64.0));
		_strikethroughPosition = ((strikethroughPosition != 0.0) ? strikethroughPosition : 0.3125);
		_strikethroughThickenss = ((strikethroughThickness > 0.0) ? strikethroughThickness : (5.0 / 64.0));
		_capsHeight = ((capsHeight > 0.0) ? capsHeight : 1.0);
		_xHeight = ((xHeight > 0.0) ? xHeight : (43.0 / 64.0));
		_style = style;
		_weight = weight;
		_stretch = stretch;
	}

	internal CompositeTypefaceMetrics()
		: this(-5.0 / 32.0, 5.0 / 64.0, 0.3125, 5.0 / 64.0, 1.0, 43.0 / 64.0, FontStyles.Normal, FontWeights.Regular, FontStretches.Normal)
	{
	}
}
