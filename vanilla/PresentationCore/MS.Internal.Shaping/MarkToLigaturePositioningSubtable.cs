namespace MS.Internal.Shaping;

internal struct MarkToLigaturePositioningSubtable
{
	private const int offsetFormat = 0;

	private const int offsetMarkCoverage = 2;

	private const int offsetLigatureCoverage = 4;

	private const int offsetClassCount = 6;

	private const int offsetMarkArray = 8;

	private const int offsetLigatureArray = 10;

	private const int offsetLigatureAttachArray = 2;

	private const int sizeOffset = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable MarkCoverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private CoverageTable LigatureCoverage(FontTable Table)
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

	private LigatureAttachTable Ligatures(FontTable Table, int Index, ushort ClassCount)
	{
		int num = offset + Table.GetUShort(offset + 10);
		return new LigatureAttachTable(num + Table.GetUShort(num + 2 + Index * 2), ClassCount);
	}

	private void FindBaseLigature(int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int markGlyph, out ushort component, out int ligatureGlyph)
	{
		int num = 0;
		ligatureGlyph = -1;
		component = 0;
		bool flag = false;
		int num2 = GlyphInfo.FirstChars[markGlyph];
		while (num2 >= 0 && !flag)
		{
			ushort num3 = Charmap[num2];
			if ((GlyphInfo.GlyphFlags[num3] & 7) != 3)
			{
				num = num2;
				ligatureGlyph = num3;
				flag = true;
			}
			num2--;
		}
		if (!flag)
		{
			return;
		}
		ushort num4 = 0;
		ushort num5 = GlyphInfo.FirstChars[ligatureGlyph];
		while (num5 < CharCount && num5 != num)
		{
			if (Charmap[num5] == ligatureGlyph)
			{
				num4++;
			}
			num5++;
		}
		component = num4;
	}

	public unsafe bool Apply(IOpenTypeFont Font, FontTable Table, LayoutMetrics Metrics, GlyphInfoList GlyphInfo, ushort LookupFlags, int CharCount, UshortList Charmap, int* Advances, LayoutOffset* Offsets, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
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
		FindBaseLigature(CharCount, Charmap, GlyphInfo, FirstGlyph, out var component, out var ligatureGlyph);
		if (ligatureGlyph < 0)
		{
			return false;
		}
		int glyphIndex2 = LigatureCoverage(Table).GetGlyphIndex(Table, GlyphInfo.Glyphs[ligatureGlyph]);
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
		AnchorTable staticAnchor = Ligatures(Table, glyphIndex2, num).LigatureAnchor(Table, component, num2);
		if (staticAnchor.IsNull())
		{
			return false;
		}
		AnchorTable mobileAnchor = markArray.MarkAnchor(Table, (ushort)glyphIndex);
		if (mobileAnchor.IsNull())
		{
			return false;
		}
		Positioning.AlignAnchors(Font, Table, Metrics, GlyphInfo, Advances, Offsets, ligatureGlyph, FirstGlyph, staticAnchor, mobileAnchor, UseAdvances: false);
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

	public MarkToLigaturePositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
