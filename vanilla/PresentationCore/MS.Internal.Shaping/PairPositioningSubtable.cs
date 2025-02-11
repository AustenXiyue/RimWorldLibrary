namespace MS.Internal.Shaping;

internal struct PairPositioningSubtable
{
	private struct PairSetTable
	{
		private const int offsetPairValueCount = 0;

		private const int offsetPairValueArray = 2;

		private const int offsetPairValueSecondGlyph = 0;

		private const int offsetPairValueValue1 = 2;

		private int offset;

		private ushort pairValueRecordSize;

		private ushort secondValueRecordOffset;

		public ushort PairValueCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public ushort PairValueGlyph(FontTable Table, ushort Index)
		{
			return Table.GetUShort(offset + 2 + Index * pairValueRecordSize);
		}

		public ValueRecordTable FirstValueRecord(FontTable Table, ushort Index, ushort Format)
		{
			return new ValueRecordTable(offset + 2 + Index * pairValueRecordSize + 2, offset, Format);
		}

		public ValueRecordTable SecondValueRecord(FontTable Table, ushort Index, ushort Format)
		{
			return new ValueRecordTable(offset + 2 + Index * pairValueRecordSize + secondValueRecordOffset, offset, Format);
		}

		public int FindPairValue(FontTable Table, ushort Glyph)
		{
			ushort num = PairValueCount(Table);
			for (ushort num2 = 0; num2 < num; num2++)
			{
				if (PairValueGlyph(Table, num2) == Glyph)
				{
					return num2;
				}
			}
			return -1;
		}

		public PairSetTable(int Offset, ushort firstValueRecordSize, ushort secondValueRecordSize)
		{
			secondValueRecordOffset = (ushort)(2 + ValueRecordTable.Size(firstValueRecordSize));
			pairValueRecordSize = (ushort)(secondValueRecordOffset + ValueRecordTable.Size(secondValueRecordSize));
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetValueFormat1 = 4;

	private const int offsetValueFormat2 = 6;

	private const int offsetFormat1PairSetCount = 8;

	private const int offsetFormat1PairSetArray = 10;

	private const int sizeFormat1PairSetOffset = 2;

	private const int offsetFormat2ClassDef1 = 8;

	private const int offsetFormat2ClassDef2 = 10;

	private const int offsetFormat2Class1Count = 12;

	private const int offsetFormat2Class2Count = 14;

	private const int offsetFormat2ValueRecordArray = 16;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetOffset(offset + 2));
	}

	private ushort FirstValueFormat(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private ushort SecondValueFormat(FontTable Table)
	{
		return Table.GetUShort(offset + 6);
	}

	private PairSetTable Format1PairSet(FontTable Table, ushort Index)
	{
		Invariant.Assert(Format(Table) == 1);
		return new PairSetTable(offset + Table.GetUShort(offset + 10 + Index * 2), FirstValueFormat(Table), SecondValueFormat(Table));
	}

	private ClassDefTable Format2Class1Table(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 2);
		return new ClassDefTable(offset + Table.GetUShort(offset + 8));
	}

	private ClassDefTable Format2Class2Table(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 2);
		return new ClassDefTable(offset + Table.GetUShort(offset + 10));
	}

	private ushort Format2Class1Count(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 2);
		return Table.GetUShort(offset + 12);
	}

	private ushort Format2Class2Count(FontTable Table)
	{
		Invariant.Assert(Format(Table) == 2);
		return Table.GetUShort(offset + 14);
	}

	private ValueRecordTable Format2FirstValueRecord(FontTable Table, ushort Class2Count, ushort Class1Index, ushort Class2Index)
	{
		Invariant.Assert(Format(Table) == 2);
		ushort format = FirstValueFormat(Table);
		ushort format2 = SecondValueFormat(Table);
		int num = ValueRecordTable.Size(format) + ValueRecordTable.Size(format2);
		return new ValueRecordTable(offset + 16 + (Class1Index * Class2Count + Class2Index) * num, offset, format);
	}

	private ValueRecordTable Format2SecondValueRecord(FontTable Table, ushort Class2Count, ushort Class1Index, ushort Class2Index)
	{
		Invariant.Assert(Format(Table) == 2);
		ushort format = FirstValueFormat(Table);
		ushort format2 = SecondValueFormat(Table);
		int num = ValueRecordTable.Size(format);
		int num2 = num + ValueRecordTable.Size(format2);
		return new ValueRecordTable(offset + 16 + (Class1Index * Class2Count + Class2Index) * num2 + num, offset, format2);
	}

	public unsafe bool Apply(IOpenTypeFont Font, FontTable Table, LayoutMetrics Metrics, GlyphInfoList GlyphInfo, ushort LookupFlags, int* Advances, LayoutOffset* Offsets, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
	{
		Invariant.Assert(FirstGlyph >= 0);
		Invariant.Assert(AfterLastGlyph <= GlyphInfo.Length);
		NextGlyph = FirstGlyph + 1;
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		int nextGlyphInLookup = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph + 1, LookupFlags, 1);
		if (nextGlyphInLookup >= AfterLastGlyph)
		{
			return false;
		}
		ushort glyph2 = GlyphInfo.Glyphs[nextGlyphInLookup];
		ValueRecordTable valueRecordTable;
		ValueRecordTable valueRecordTable2;
		switch (Format(Table))
		{
		case 1:
		{
			int glyphIndex = Coverage(Table).GetGlyphIndex(Table, glyph);
			if (glyphIndex == -1)
			{
				return false;
			}
			PairSetTable pairSetTable = Format1PairSet(Table, (ushort)glyphIndex);
			int num = pairSetTable.FindPairValue(Table, glyph2);
			if (num == -1)
			{
				return false;
			}
			valueRecordTable = pairSetTable.FirstValueRecord(Table, (ushort)num, FirstValueFormat(Table));
			valueRecordTable2 = pairSetTable.SecondValueRecord(Table, (ushort)num, SecondValueFormat(Table));
			break;
		}
		case 2:
		{
			if (Coverage(Table).GetGlyphIndex(Table, glyph) == -1)
			{
				return false;
			}
			ushort @class = Format2Class1Table(Table).GetClass(Table, glyph);
			if (@class >= Format2Class1Count(Table))
			{
				return false;
			}
			ushort class2 = Format2Class2Table(Table).GetClass(Table, glyph2);
			if (class2 >= Format2Class2Count(Table))
			{
				return false;
			}
			ushort class2Count = Format2Class2Count(Table);
			valueRecordTable = Format2FirstValueRecord(Table, class2Count, @class, class2);
			valueRecordTable2 = Format2SecondValueRecord(Table, class2Count, @class, class2);
			break;
		}
		default:
			return false;
		}
		valueRecordTable.AdjustPos(Table, Metrics, ref Offsets[FirstGlyph], ref Advances[FirstGlyph]);
		valueRecordTable2.AdjustPos(Table, Metrics, ref Offsets[nextGlyphInLookup], ref Advances[nextGlyphInLookup]);
		return true;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		return Coverage(table).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId);
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public PairPositioningSubtable(int Offset)
	{
		offset = Offset;
	}
}
