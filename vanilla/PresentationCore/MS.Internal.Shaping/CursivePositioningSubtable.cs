namespace MS.Internal.Shaping;

internal struct CursivePositioningSubtable
{
	private const ushort offsetFormat = 0;

	private const ushort offsetCoverage = 2;

	private const ushort offsetEntryExitCount = 4;

	private const ushort offsetEntryExitArray = 6;

	private const ushort sizeEntryExitRecord = 4;

	private const ushort offsetEntryAnchor = 0;

	private const ushort offsetExitAnchor = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private AnchorTable EntryAnchor(FontTable Table, int Index)
	{
		int uShort = Table.GetUShort(offset + 6 + 4 * Index);
		if (uShort == 0)
		{
			return new AnchorTable(Table, 0);
		}
		return new AnchorTable(Table, offset + uShort);
	}

	private AnchorTable ExitAnchor(FontTable Table, int Index)
	{
		int uShort = Table.GetUShort(offset + 6 + 4 * Index + 2);
		if (uShort == 0)
		{
			return new AnchorTable(Table, 0);
		}
		return new AnchorTable(Table, offset + uShort);
	}

	public unsafe bool Apply(IOpenTypeFont Font, FontTable Table, LayoutMetrics Metrics, GlyphInfoList GlyphInfo, ushort LookupFlags, int* Advances, LayoutOffset* Offsets, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
	{
		Invariant.Assert(FirstGlyph >= 0);
		Invariant.Assert(AfterLastGlyph <= GlyphInfo.Length);
		NextGlyph = FirstGlyph + 1;
		if (Format(Table) != 1)
		{
			return false;
		}
		bool flag = (LookupFlags & 1) != 0;
		ushort num = 64;
		int nextGlyphInLookup = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph, LookupFlags, 1);
		if (flag)
		{
			GlyphInfo.GlyphFlags[nextGlyphInLookup] &= (ushort)(~num);
		}
		if (nextGlyphInLookup >= AfterLastGlyph)
		{
			return false;
		}
		int nextGlyphInLookup2 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph - 1, LookupFlags, -1);
		if (nextGlyphInLookup2 < 0)
		{
			return false;
		}
		CoverageTable coverageTable = Coverage(Table);
		int glyphIndex = coverageTable.GetGlyphIndex(Table, GlyphInfo.Glyphs[nextGlyphInLookup]);
		if (glyphIndex == -1)
		{
			return false;
		}
		int glyphIndex2 = coverageTable.GetGlyphIndex(Table, GlyphInfo.Glyphs[nextGlyphInLookup2]);
		if (glyphIndex2 == -1)
		{
			return false;
		}
		AnchorTable staticAnchor = ExitAnchor(Table, glyphIndex2);
		if (staticAnchor.IsNull())
		{
			return false;
		}
		AnchorTable mobileAnchor = EntryAnchor(Table, glyphIndex);
		if (mobileAnchor.IsNull())
		{
			return false;
		}
		Positioning.AlignAnchors(Font, Table, Metrics, GlyphInfo, Advances, Offsets, nextGlyphInLookup2, nextGlyphInLookup, staticAnchor, mobileAnchor, UseAdvances: true);
		if (flag)
		{
			UshortList glyphFlags = GlyphInfo.GlyphFlags;
			int num2;
			for (num2 = nextGlyphInLookup; num2 > nextGlyphInLookup2; num2--)
			{
				glyphFlags[num2] |= num;
			}
			int dy = Offsets[nextGlyphInLookup].dy;
			num2 = nextGlyphInLookup;
			while ((glyphFlags[num2] & num) != 0)
			{
				Offsets[num2].dy -= dy;
				num2--;
			}
			Invariant.Assert(nextGlyphInLookup >= 0);
			Offsets[num2].dy -= dy;
		}
		return true;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return true;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public CursivePositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
