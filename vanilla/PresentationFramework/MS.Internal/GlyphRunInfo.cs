using System.Windows;

namespace MS.Internal;

internal abstract class GlyphRunInfo
{
	internal abstract Point StartPosition { get; }

	internal abstract Point EndPosition { get; }

	internal abstract double WidthEmFontSize { get; }

	internal abstract double HeightEmFontSize { get; }

	internal abstract bool GlyphsHaveSidewaysOrientation { get; }

	internal abstract int BidiLevel { get; }

	internal abstract uint LanguageID { get; }

	internal abstract string UnicodeString { get; }
}
