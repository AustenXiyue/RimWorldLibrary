namespace MS.Internal.Shaping;

internal interface IOpenTypeFont
{
	FontTable GetFontTable(OpenTypeTags TableTag);

	LayoutOffset GetGlyphPointCoord(ushort Glyph, ushort PointIndex);

	byte[] GetTableCache(OpenTypeTags tableTag);

	byte[] AllocateTableCache(OpenTypeTags tableTag, int size);
}
