namespace MS.Internal.Shaping;

internal struct CoverageTable
{
	private const int offsetFormat = 0;

	private const int offsetFormat1GlyphCount = 2;

	private const int offsetFormat1GlyphArray = 4;

	private const int sizeFormat1GlyphId = 2;

	private const int offsetFormat2RangeCount = 2;

	private const int offsetFormat2RangeRecordArray = 4;

	private const int sizeFormat2RangeRecord = 6;

	private const int offsetFormat2RangeRecordStart = 0;

	private const int offsetFormat2RangeRecordEnd = 2;

	private const int offsetFormat2RangeRecordStartIndex = 4;

	private int offset;

	public static CoverageTable InvalidCoverage => new CoverageTable(-1);

	public bool IsInvalid => offset == -1;

	public ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	public ushort Format1GlyphCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	public ushort Format1Glyph(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 2);
	}

	public ushort Format2RangeCount(FontTable Table)
	{
		return Table.GetUShort(offset + 2);
	}

	public ushort Format2RangeStartGlyph(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6);
	}

	public ushort Format2RangeEndGlyph(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6 + 2);
	}

	public ushort Format2RangeStartCoverageIndex(FontTable Table, ushort Index)
	{
		return Table.GetUShort(offset + 4 + Index * 6 + 4);
	}

	public int GetGlyphIndex(FontTable Table, ushort glyph)
	{
		switch (Format(Table))
		{
		case 1:
		{
			ushort num4 = 0;
			ushort num5 = Format1GlyphCount(Table);
			while (num4 < num5)
			{
				ushort num6 = (ushort)(num4 + num5 >> 1);
				ushort num7 = Format1Glyph(Table, num6);
				if (glyph < num7)
				{
					num5 = num6;
					continue;
				}
				if (glyph > num7)
				{
					num4 = (ushort)(num6 + 1);
					continue;
				}
				return num6;
			}
			return -1;
		}
		case 2:
		{
			ushort num = 0;
			ushort num2 = Format2RangeCount(Table);
			while (num < num2)
			{
				ushort num3 = (ushort)(num + num2 >> 1);
				if (glyph < Format2RangeStartGlyph(Table, num3))
				{
					num2 = num3;
					continue;
				}
				if (glyph > Format2RangeEndGlyph(Table, num3))
				{
					num = (ushort)(num3 + 1);
					continue;
				}
				return glyph - Format2RangeStartGlyph(Table, num3) + Format2RangeStartCoverageIndex(Table, num3);
			}
			return -1;
		}
		default:
			return -1;
		}
	}

	public bool IsAnyGlyphCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		switch (Format(table))
		{
		case 1:
		{
			ushort num8 = Format1GlyphCount(table);
			if (num8 == 0)
			{
				return false;
			}
			ushort num9 = Format1Glyph(table, 0);
			ushort num10 = Format1Glyph(table, (ushort)(num8 - 1));
			if (maxGlyphId < num9 || minGlyphId > num10)
			{
				return false;
			}
			for (ushort num11 = 0; num11 < num8; num11++)
			{
				ushort num12 = Format1Glyph(table, num11);
				if (num12 <= maxGlyphId && num12 >= minGlyphId && (glyphBits[num12 >> 5] & (1 << num12 % 32)) != 0L)
				{
					return true;
				}
			}
			return false;
		}
		case 2:
		{
			ushort num = Format2RangeCount(table);
			if (num == 0)
			{
				return false;
			}
			ushort num2 = Format2RangeStartGlyph(table, 0);
			ushort num3 = Format2RangeEndGlyph(table, (ushort)(num - 1));
			if (maxGlyphId < num2 || minGlyphId > num3)
			{
				return false;
			}
			for (ushort num4 = 0; num4 < num; num4++)
			{
				ushort num5 = Format2RangeStartGlyph(table, num4);
				ushort num6 = Format2RangeEndGlyph(table, num4);
				for (ushort num7 = num5; num7 <= num6; num7++)
				{
					if (num7 <= maxGlyphId && num7 >= minGlyphId && (glyphBits[num7 >> 5] & (1 << num7 % 32)) != 0L)
					{
						return true;
					}
				}
			}
			return false;
		}
		default:
			return true;
		}
	}

	public CoverageTable(int Offset)
	{
		offset = Offset;
	}
}
