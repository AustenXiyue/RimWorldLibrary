using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;

namespace MS.Internal.FontFace;

internal interface ITypefaceMetrics
{
	double XHeight { get; }

	double CapsHeight { get; }

	double UnderlinePosition { get; }

	double UnderlineThickness { get; }

	double StrikethroughPosition { get; }

	double StrikethroughThickness { get; }

	bool Symbol { get; }

	StyleSimulations StyleSimulations { get; }

	IDictionary<XmlLanguage, string> AdjustedFaceNames { get; }
}
