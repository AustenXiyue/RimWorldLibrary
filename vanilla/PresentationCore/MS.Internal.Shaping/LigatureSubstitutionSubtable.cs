using System.IO;

namespace MS.Internal.Shaping;

internal struct LigatureSubstitutionSubtable
{
	private struct LigatureSetTable
	{
		private const int offsetLigatureCount = 0;

		private const int offsetLigatureArray = 2;

		private const int sizeLigatureOffset = 2;

		private int offset;

		public ushort LigatureCount(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public LigatureTable Ligature(FontTable Table, ushort Index)
		{
			return new LigatureTable(offset + Table.GetUShort(offset + 2 + Index * 2));
		}

		public LigatureSetTable(int Offset)
		{
			offset = Offset;
		}
	}

	private struct LigatureTable
	{
		private const int offsetLigatureGlyph = 0;

		private const int offsetComponentCount = 2;

		private const int offsetComponentArray = 4;

		private const int sizeComponent = 2;

		private int offset;

		public ushort LigatureGlyph(FontTable Table)
		{
			return Table.GetUShort(offset);
		}

		public ushort ComponentCount(FontTable Table)
		{
			return Table.GetUShort(offset + 2);
		}

		public ushort Component(FontTable Table, ushort Index)
		{
			return Table.GetUShort(offset + 4 + (Index - 1) * 2);
		}

		public LigatureTable(int Offset)
		{
			offset = Offset;
		}
	}

	private const int offsetFormat = 0;

	private const int offsetCoverage = 2;

	private const int offsetLigatureSetCount = 4;

	private const int offsetLigatureSetArray = 6;

	private const int sizeLigatureSet = 2;

	private int offset;

	private ushort Format(FontTable Table)
	{
		return Table.GetUShort(offset);
	}

	private CoverageTable Coverage(FontTable Table)
	{
		return new CoverageTable(offset + Table.GetUShort(offset + 2));
	}

	private ushort LigatureSetCount(FontTable Table)
	{
		return Table.GetUShort(offset + 4);
	}

	private LigatureSetTable LigatureSet(FontTable Table, ushort Index)
	{
		return new LigatureSetTable(offset + Table.GetUShort(offset + 6 + Index * 2));
	}

	public bool Apply(IOpenTypeFont Font, FontTable Table, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, ushort LookupFlags, int FirstGlyph, int AfterLastGlyph, out int NextGlyph)
	{
		Invariant.Assert(FirstGlyph >= 0);
		Invariant.Assert(AfterLastGlyph <= GlyphInfo.Length);
		NextGlyph = FirstGlyph + 1;
		if (Format(Table) != 1)
		{
			return false;
		}
		_ = GlyphInfo.Length;
		ushort glyph = GlyphInfo.Glyphs[FirstGlyph];
		int glyphIndex = Coverage(Table).GetGlyphIndex(Table, glyph);
		if (glyphIndex == -1)
		{
			return false;
		}
		ushort value = 0;
		bool flag = false;
		ushort num = 0;
		LigatureSetTable ligatureSetTable = LigatureSet(Table, (ushort)glyphIndex);
		ushort num2 = ligatureSetTable.LigatureCount(Table);
		for (ushort num3 = 0; num3 < num2; num3++)
		{
			LigatureTable ligatureTable = ligatureSetTable.Ligature(Table, num3);
			num = ligatureTable.ComponentCount(Table);
			if (num == 0)
			{
				throw new FileFormatException();
			}
			int num4 = FirstGlyph;
			ushort num5 = 1;
			for (num5 = 1; num5 < num; num5++)
			{
				num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1);
				if (num4 >= AfterLastGlyph || GlyphInfo.Glyphs[num4] != ligatureTable.Component(Table, num5))
				{
					break;
				}
			}
			if (num5 == num)
			{
				flag = true;
				value = ligatureTable.LigatureGlyph(Table);
				break;
			}
		}
		if (flag)
		{
			int num6 = 0;
			int num7 = int.MaxValue;
			int num4 = FirstGlyph;
			for (ushort num8 = 0; num8 < num; num8++)
			{
				Invariant.Assert(num4 < AfterLastGlyph);
				int num9 = GlyphInfo.FirstChars[num4];
				int num10 = GlyphInfo.LigatureCounts[num4];
				num6 += num10;
				if (num9 < num7)
				{
					num7 = num9;
				}
				num4 = LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1);
			}
			num4 = FirstGlyph;
			int num11 = FirstGlyph;
			ushort num12 = 0;
			for (ushort num13 = 1; num13 <= num; num13++)
			{
				num11 = num4;
				num4 = ((num13 >= num) ? GlyphInfo.Length : LayoutEngine.GetNextGlyphInLookup(Font, GlyphInfo, num4 + 1, LookupFlags, 1));
				for (int i = 0; i < CharCount; i++)
				{
					if (Charmap[i] == num11)
					{
						Charmap[i] = (ushort)FirstGlyph;
					}
				}
				if (num12 > 0)
				{
					for (int j = num11 + 1; j < num4; j++)
					{
						GlyphInfo.Glyphs[j - num12] = GlyphInfo.Glyphs[j];
						GlyphInfo.GlyphFlags[j - num12] = GlyphInfo.GlyphFlags[j];
						GlyphInfo.FirstChars[j - num12] = GlyphInfo.FirstChars[j];
						GlyphInfo.LigatureCounts[j - num12] = GlyphInfo.LigatureCounts[j];
					}
					if (num4 - num11 > 1)
					{
						for (int k = 0; k < CharCount; k++)
						{
							ushort num14 = Charmap[k];
							if (num14 > num11 && num14 < num4)
							{
								Charmap[k] -= num12;
							}
						}
					}
				}
				num12++;
			}
			GlyphInfo.Glyphs[FirstGlyph] = value;
			GlyphInfo.GlyphFlags[FirstGlyph] = 23;
			GlyphInfo.FirstChars[FirstGlyph] = (ushort)num7;
			GlyphInfo.LigatureCounts[FirstGlyph] = (ushort)num6;
			if (num > 1)
			{
				GlyphInfo.Remove(GlyphInfo.Length - num + 1, num - 1);
			}
			NextGlyph = num11 - (num - 1) + 1;
		}
		return flag;
	}

	public bool IsLookupCovered(FontTable table, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId)
	{
		if (!Coverage(table).IsAnyGlyphCovered(table, glyphBits, minGlyphId, maxGlyphId))
		{
			return false;
		}
		ushort num = LigatureSetCount(table);
		for (ushort num2 = 0; num2 < num; num2++)
		{
			LigatureSetTable ligatureSetTable = LigatureSet(table, num2);
			ushort num3 = ligatureSetTable.LigatureCount(table);
			for (ushort num4 = 0; num4 < num3; num4++)
			{
				LigatureTable ligatureTable = ligatureSetTable.Ligature(table, num4);
				ushort num5 = ligatureTable.ComponentCount(table);
				bool flag = true;
				for (ushort num6 = 1; num6 < num5; num6++)
				{
					ushort num7 = ligatureTable.Component(table, num6);
					if (num7 > maxGlyphId || num7 < minGlyphId || (glyphBits[num7 >> 5] & (1 << num7 % 32)) == 0L)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public CoverageTable GetPrimaryCoverage(FontTable table)
	{
		return Coverage(table);
	}

	public LigatureSubstitutionSubtable(int Offset)
	{
		offset = Offset;
	}
}
