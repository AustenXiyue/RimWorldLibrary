namespace MS.Internal.Shaping;

internal struct ContextualLookupRecords
{
	private const int offsetSequenceIndex = 0;

	private const int offsetLookupIndex = 2;

	private const int sizeLookupRecord = 4;

	private const int MaximumContextualLookupNestingLevel = 16;

	private int offset;

	private ushort recordCount;

	private ushort SequenceIndex(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + Index * 4);
	}

	private ushort LookupIndex(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + Index * 4 + 2);
	}

	public unsafe void ApplyContextualLookups(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int nextGlyph)
	{
		if (nestingLevel >= 16)
		{
			nextGlyph = AfterLastGlyph;
			return;
		}
		LookupList lookupList = ((TableTag != OpenTypeTags.GSUB) ? new GPOSHeader(0).GetLookupList(Table) : new GSUBHeader(0).GetLookupList(Table));
		int num = -1;
		int num2 = -1;
		while (true)
		{
			ushort num3 = ushort.MaxValue;
			ushort num4 = ushort.MaxValue;
			for (ushort num5 = 0; num5 < recordCount; num5++)
			{
				ushort num6 = LookupIndex(Table, num5);
				ushort num7 = SequenceIndex(Table, num5);
				if (num6 >= num && (num6 != num || num7 > num2) && (num6 < num3 || (num6 == num3 && num7 < num4)))
				{
					num3 = num6;
					num4 = num7;
				}
			}
			if (num3 == ushort.MaxValue)
			{
				break;
			}
			num = num3;
			num2 = num4;
			int num8 = FirstGlyph;
			for (int i = 0; i < num4; i++)
			{
				if (num8 >= AfterLastGlyph)
				{
					break;
				}
				num8 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num8 + 1, LookupFlags, 1);
			}
			if (num8 < AfterLastGlyph)
			{
				int length = GlyphInfo.Length;
				LayoutEngine.ApplyLookup(Font, TableTag, Table, Metrics, lookupList.Lookup(Table, num3), CharCount, Charmap, GlyphInfo, Advances, Offsets, num8, AfterLastGlyph, Parameter, nestingLevel + 1, out var _);
				AfterLastGlyph += GlyphInfo.Length - length;
			}
		}
		nextGlyph = AfterLastGlyph;
	}

	public ContextualLookupRecords(int Offset, ushort RecordCount)
	{
		offset = Offset;
		recordCount = RecordCount;
	}
}
