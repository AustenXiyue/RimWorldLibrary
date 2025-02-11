namespace MS.Internal.Shaping;

internal struct ChainingSubtable
{
	private const int offsetFormat = 0;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public unsafe bool Apply(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		NextGlyph = FirstGlyph + 1;
		return Format(Table) switch
		{
			1 => new GlyphChainingSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph), 
			2 => new ClassChainingSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph), 
			3 => new CoverageChainingSubtable(Table, offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, LookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph), 
			_ => false, 
		};
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Format(table) switch
		{
			1 => new GlyphChainingSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			2 => new ClassChainingSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			3 => new CoverageChainingSubtable(table, offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId), 
			_ => true, 
		};
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Format(table) switch
		{
			1 => new GlyphChainingSubtable(offset).GetPrimaryCoverage(table), 
			2 => new ClassChainingSubtable(offset).GetPrimaryCoverage(table), 
			3 => new CoverageChainingSubtable(table, offset).GetPrimaryCoverage(table), 
			_ => CoverageTable.InvalidCoverage, 
		};
	}

	public ChainingSubtable(int Offset)
	{
		offset = Offset;
	}
}
