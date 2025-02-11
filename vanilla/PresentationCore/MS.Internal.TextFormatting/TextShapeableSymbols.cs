using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.TextFormatting;

internal abstract class TextShapeableSymbols : TextRun
{
	internal abstract bool IsShapingRequired { get; }

	internal abstract bool NeedsMaxClusterSize { get; }

	internal abstract ushort MaxClusterSize { get; }

	internal abstract bool NeedsCaretInfo { get; }

	internal abstract bool HasExtendedCharacter { get; }

	internal abstract GlyphTypeface GlyphTypeFace { get; }

	internal abstract double EmSize { get; }

	internal abstract ItemProps ItemProps { get; }

	internal abstract double Height { get; }

	internal abstract double Baseline { get; }

	internal abstract double UnderlinePosition { get; }

	internal abstract double UnderlineThickness { get; }

	internal abstract double StrikethroughPosition { get; }

	internal abstract double StrikethroughThickness { get; }

	internal abstract GlyphRun ComputeShapedGlyphRun(Point origin, char[] characterString, ushort[] clusterMap, ushort[] glyphIndices, IList<double> glyphAdvances, IList<Point> glyphOffsets, bool rightToLeft, bool sideways);

	internal abstract bool CanShapeTogether(TextShapeableSymbols shapeable);

	internal unsafe abstract void GetAdvanceWidthsUnshaped(char* characterString, int characterLength, double scalingFactor, int* advanceWidthsUnshaped);

	internal abstract GlyphRun ComputeUnshapedGlyphRun(Point origin, char[] characterString, IList<double> characterAdvances);

	internal abstract void Draw(DrawingContext drawingContext, Brush foregroundBrush, GlyphRun glyphRun);
}
