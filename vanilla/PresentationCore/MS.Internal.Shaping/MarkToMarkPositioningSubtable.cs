namespace MS.Internal.Shaping;

internal struct MarkToMarkPositioningSubtable
{
	private struct Mark2Array
	{
		private const int offsetCount = 0;

		private const int offsetAnchors = 2;

		private const int sizeAnchorOffset = 2;

		private int offset;

		public AnchorTable Anchor(FontTable Table, ushort Mark2Index, ushort Mark1ClassCount, ushort Mark1Class)
		{
			int uShort = Table.GetUShort(offset + 2 + (Mark2Index * Mark1ClassCount + Mark1Class) * 2);
			if (uShort == 0)
			{
				return new AnchorTable(Table, 0);
			}
			return new AnchorTable(Table, offset + uShort);
		}

		public Mark2Array(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetMark2Coverage = 4;

	private const int offsetClassCount = 6;

	private const int offsetMark1Array = 8;

	private const int offsetMark2Array = 10;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Mark1Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private CoverageTable Mark2Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 4));
	}

	private ushort Mark1ClassCount(FontTable Table)
	{
		return Table.GetUShort(offset + 6);
	}

	private MarkArray Mark1Array(FontTable Table)
	{
		return new MarkArray(offset + Table.GetUShort(offset + 8));
	}

	private Mark2Array Marks2(FontTable Table)
	{
		return new Mark2Array(offset + Table.GetUShort(offset + 10));
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
		int glyphIndex = Mark1Coverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[FirstGlyph]);
		if (glyphIndex == -1)
		{
			return false;
		}
		int nextGlyphInLookup = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph - 1, (ushort)(LookupFlags & 0xFF00), -1);
		if (nextGlyphInLookup < 0)
		{
			return false;
		}
		int glyphIndex2 = Mark2Coverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[nextGlyphInLookup]);
		if (glyphIndex2 == -1)
		{
			return false;
		}
		ushort num = Mark1ClassCount(Table);
		MarkArray markArray = Mark1Array(Table);
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
		AnchorTable staticAnchor = Marks2(Table).Anchor(Table, (ushort)glyphIndex2, num, num2);
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
		return Mark1Coverage(table);
	}

	public MarkToMarkPositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
