namespace MS.Internal;

internal abstract class FixedPageInfo
{
	internal abstract int GlyphRunCount { get; }

	internal abstract GlyphRunInfo GlyphRunAtPosition(int position);
}
