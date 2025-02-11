namespace MS.Internal.Shaping;

internal struct MarkToBasePositioningSubtable
{
	private struct BaseArray
	{
		private const int offsetAnchorArray = 2;

		private const int sizeAnchorOffset = 2;

		private int offset;

		public AnchorTable BaseAnchor(FontTable Table, ushort BaseIndex, ushort MarkClassCount, ushort MarkClass)
		{
			int uShort = Table.GetUShort(offset + 2 + (BaseIndex * MarkClassCount + MarkClass) * 2);
			if (uShort == 0)
			{
				return new AnchorTable(Table, 0);
			}
			return new AnchorTable(Table, offset + uShort);
		}

		public BaseArray(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetBaseCoverage = 4;

	private const int offsetClassCount = 6;

	private const int offsetMarkArray = 8;

	private const int offsetBaseArray = 10;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable MarkCoverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private CoverageTable BaseCoverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 4));
	}

	private ushort ClassCount(FontTable Table)
	{
		return Table.GetUShort(offset + 6);
	}

	private MarkArray Marks(FontTable Table)
	{
		return new MarkArray(offset + Table.GetUShort(offset + 8));
	}

	private BaseArray Bases(FontTable Table)
	{
		return new BaseArray(offset + Table.GetUShort(offset + 10));
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
		_ = GlyphInfo.Length;
		if ((GlyphInfo.GlyphFlags[FirstGlyph] & 7) != 3)
		{
			return false;
		}
		int glyphIndex = MarkCoverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[FirstGlyph]);
		if (glyphIndex == -1)
		{
			return false;
		}
		int nextGlyphInLookup = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph - 1, 8, -1);
		if (nextGlyphInLookup < 0)
		{
			return false;
		}
		int glyphIndex2 = BaseCoverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[nextGlyphInLookup]);
		if (glyphIndex2 == -1)
		{
			return false;
		}
		ushort num = ClassCount(Table);
		MarkArray markArray = Marks(Table);
		ushort num2 = markArray.Class(Table, (ushort)glyphIndex);
		if (num2 >= num)
		{
			return false;
		}
		AnchorTable mobileAnchor = markArray.MarkAnchor(Table, (ushort)glyphIndex);
		if (mobileAnchor.IsNull())
		{
			return false;
		}
		AnchorTable staticAnchor = Bases(Table).BaseAnchor(Table, (ushort)glyphIndex2, num, num2);
		if (staticAnchor.IsNull())
		{
			return false;
		}
		Positioning.AlignAnchors(Font, Table, Metrics, GlyphInfo, Advances, Offsets, nextGlyphInLookup, FirstGlyph, staticAnchor, mobileAnchor, UseAdvances: false);
		return true;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return false;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return MarkCoverage(table);
	}

	public MarkToBasePositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
