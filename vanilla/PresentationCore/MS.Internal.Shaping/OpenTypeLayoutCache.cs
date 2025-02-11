using System;
using System.Collections;
using System.IO;

namespace MS.Internal.Shaping;

internal static class OpenTypeLayoutCache
{
	private class GlyphLookupRecord : IComparable<GlyphLookupRecord>
	{
		private ushort _glyph;

		private ushort _lookup;

		public ushort Glyph => _glyph;

		public ushort Lookup => _lookup;

		public GlyphLookupRecord(ushort glyph, ushort lookup)
		{
			_glyph = glyph;
			_lookup = lookup;
		}

		public int CompareTo(GlyphLookupRecord value)
		{
			if (_glyph < value._glyph)
			{
				return -1;
			}
			if (_glyph > value._glyph)
			{
				return 1;
			}
			if (_lookup < value._lookup)
			{
				return -1;
			}
			if (_lookup > value._lookup)
			{
				return 1;
			}
			return 0;
		}

		public bool Equals(GlyphLookupRecord value)
		{
			if (_glyph == value._glyph)
			{
				return _lookup == value._lookup;
			}
			return false;
		}

		public static bool operator ==(GlyphLookupRecord value1, GlyphLookupRecord value2)
		{
			return value1.Equals(value2);
		}

		public static bool operator !=(GlyphLookupRecord value1, GlyphLookupRecord value2)
		{
			return !value1.Equals(value2);
		}

		public override bool Equals(object value)
		{
			return Equals((GlyphLookupRecord)value);
		}

		public override int GetHashCode()
		{
			return _glyph << 16 + _lookup;
		}
	}

	public static void InitCache(IOpenTypeFont font, OpenTypeTags tableTag, GlyphInfoList glyphInfo, OpenTypeLayoutWorkspace workspace)
	{
		byte[] tableCache = font.GetTableCache(tableTag);
		if (tableCache == null)
		{
			workspace.TableCacheData = null;
			return;
		}
		workspace.TableCacheData = tableCache;
		workspace.AllocateCachePointers(glyphInfo.Length);
		RenewPointers(glyphInfo, workspace, 0, glyphInfo.Length);
	}

	public static void OnGlyphsChanged(OpenTypeLayoutWorkspace workspace, GlyphInfoList glyphInfo, int oldLength, int firstGlyphChanged, int afterLastGlyphChanged)
	{
		if (workspace.TableCacheData != null)
		{
			workspace.UpdateCachePointers(oldLength, glyphInfo.Length, firstGlyphChanged, afterLastGlyphChanged);
			RenewPointers(glyphInfo, workspace, firstGlyphChanged, afterLastGlyphChanged);
		}
	}

	private unsafe static ushort GetCacheLookupCount(OpenTypeLayoutWorkspace workspace)
	{
		if (workspace.TableCacheData == null)
		{
			return 0;
		}
		fixed (byte* ptr = &workspace.TableCacheData[0])
		{
			ushort* ptr2 = (ushort*)ptr;
			return ptr2[2];
		}
	}

	public static void FindNextLookup(OpenTypeLayoutWorkspace workspace, GlyphInfoList glyphInfo, ushort firstLookupIndex, out ushort lookupIndex, out int firstGlyph)
	{
		if (firstLookupIndex >= GetCacheLookupCount(workspace))
		{
			lookupIndex = firstLookupIndex;
			firstGlyph = 0;
			return;
		}
		ushort[] cachePointers = workspace.CachePointers;
		int length = glyphInfo.Length;
		lookupIndex = ushort.MaxValue;
		firstGlyph = 0;
		for (int i = 0; i < length; i++)
		{
			while (cachePointers[i] < firstLookupIndex)
			{
				cachePointers[i]++;
			}
			if (cachePointers[i] < lookupIndex)
			{
				lookupIndex = cachePointers[i];
				firstGlyph = i;
			}
		}
		if (lookupIndex == ushort.MaxValue)
		{
			lookupIndex = GetCacheLookupCount(workspace);
			firstGlyph = 0;
		}
	}

	public static bool FindNextGlyphInLookup(OpenTypeLayoutWorkspace workspace, ushort lookupIndex, bool isLookupReversal, ref int firstGlyph, ref int afterLastGlyph)
	{
		if (lookupIndex >= GetCacheLookupCount(workspace))
		{
			return true;
		}
		ushort[] cachePointers = workspace.CachePointers;
		if (!isLookupReversal)
		{
			for (int i = firstGlyph; i < afterLastGlyph; i++)
			{
				if (cachePointers[i] == lookupIndex)
				{
					firstGlyph = i;
					return true;
				}
			}
			return false;
		}
		for (int num = afterLastGlyph - 1; num >= firstGlyph; num--)
		{
			if (cachePointers[num] == lookupIndex)
			{
				afterLastGlyph = num + 1;
				return true;
			}
		}
		return false;
	}

	private unsafe static void RenewPointers(GlyphInfoList glyphInfo, OpenTypeLayoutWorkspace workspace, int firstGlyph, int afterLastGlyph)
	{
		fixed (byte* ptr = &workspace.TableCacheData[0])
		{
			if (ptr == null)
			{
				return;
			}
			ushort[] cachePointers = workspace.CachePointers;
			for (int i = firstGlyph; i < afterLastGlyph; i++)
			{
				ushort num = glyphInfo.Glyphs[i];
				int num2 = 2;
				ushort num3 = *(ushort*)(ptr + (nint)3 * (nint)2);
				ushort* ptr2 = (ushort*)(ptr + (nint)4 * (nint)2);
				int num4 = 0;
				int num5 = num3;
				while (num4 < num5)
				{
					int num6 = num4 + num5 >> 1;
					ushort num7 = ptr2[num6 * 2];
					if (num < num7)
					{
						num5 = num6;
						continue;
					}
					if (num > num7)
					{
						num4 = num6 + 1;
						continue;
					}
					num2 = ptr2[num6 * 2 + 1];
					break;
				}
				cachePointers[i] = *(ushort*)(ptr + num2);
			}
		}
	}

	internal static void CreateCache(IOpenTypeFont font, int maxCacheSize)
	{
		if (maxCacheSize > 65535)
		{
			maxCacheSize = 65535;
		}
		int num = 0;
		CreateTableCache(font, OpenTypeTags.GSUB, maxCacheSize - num, out var tableCacheSize);
		num += tableCacheSize;
		CreateTableCache(font, OpenTypeTags.GPOS, maxCacheSize - num, out tableCacheSize);
		num += tableCacheSize;
	}

	private static void CreateTableCache(IOpenTypeFont font, OpenTypeTags tableTag, int maxCacheSize, out int tableCacheSize)
	{
		tableCacheSize = 0;
		int cacheSize = 0;
		int recordCount = 0;
		int glyphCount = 0;
		int lastLookupAdded = -1;
		GlyphLookupRecord[] records = null;
		try
		{
			ComputeTableCache(font, tableTag, maxCacheSize, ref cacheSize, ref records, ref recordCount, ref glyphCount, ref lastLookupAdded);
		}
		catch (FileFormatException)
		{
			cacheSize = 0;
		}
		if (cacheSize > 0)
		{
			tableCacheSize = FillTableCache(font, tableTag, cacheSize, records, recordCount, glyphCount, lastLookupAdded);
		}
	}

	private static void ComputeTableCache(IOpenTypeFont font, OpenTypeTags tableTag, int maxCacheSize, ref int cacheSize, ref GlyphLookupRecord[] records, ref int recordCount, ref int glyphCount, ref int lastLookupAdded)
	{
		FontTable fontTable = font.GetFontTable(tableTag);
		if (!fontTable.IsPresent)
		{
			return;
		}
		FeatureList featureList;
		LookupList lookupList;
		switch (tableTag)
		{
		case OpenTypeTags.GSUB:
		{
			GSUBHeader gSUBHeader = default(GSUBHeader);
			featureList = gSUBHeader.GetFeatureList(fontTable);
			lookupList = gSUBHeader.GetLookupList(fontTable);
			break;
		}
		case OpenTypeTags.GPOS:
		{
			GPOSHeader gPOSHeader = default(GPOSHeader);
			featureList = gPOSHeader.GetFeatureList(fontTable);
			lookupList = gPOSHeader.GetLookupList(fontTable);
			break;
		}
		default:
			featureList = new FeatureList(0);
			lookupList = new LookupList(0);
			break;
		}
		int num = maxCacheSize / 4;
		records = new GlyphLookupRecord[num];
		int num2 = lookupList.LookupCount(fontTable);
		int num3 = 0;
		BitArray bitArray = new BitArray(num2);
		for (ushort num4 = 0; num4 < featureList.FeatureCount(fontTable); num4++)
		{
			FeatureTable featureTable = featureList.FeatureTable(fontTable, num4);
			for (ushort num5 = 0; num5 < featureTable.LookupCount(fontTable); num5++)
			{
				ushort num6 = featureTable.LookupIndex(fontTable, num5);
				if (num6 < num2)
				{
					bitArray[num6] = true;
				}
			}
		}
		for (ushort num7 = 0; num7 < num2; num7++)
		{
			if (bitArray[num7])
			{
				int maxLookupGlyph = -1;
				bool flag = false;
				LookupTable lookupTable = lookupList.Lookup(fontTable, num7);
				ushort lookupType = lookupTable.LookupType();
				ushort num8 = lookupTable.SubTableCount();
				for (ushort num9 = 0; num9 < num8; num9++)
				{
					int subtableOffset = lookupTable.SubtableOffset(fontTable, num9);
					CoverageTable subtablePrincipalCoverage = GetSubtablePrincipalCoverage(fontTable, tableTag, lookupType, subtableOffset);
					if (!subtablePrincipalCoverage.IsInvalid)
					{
						flag = !AppendCoverageGlyphRecords(fontTable, num7, subtablePrincipalCoverage, records, ref recordCount, ref maxLookupGlyph);
						if (flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
				lastLookupAdded = num7;
				num3 = recordCount;
			}
		}
		recordCount = num3;
		if (lastLookupAdded == -1)
		{
			return;
		}
		Array.Sort(records, 0, recordCount);
		cacheSize = -1;
		glyphCount = -1;
		while (recordCount > 0)
		{
			CalculateCacheSize(records, recordCount, out cacheSize, out glyphCount);
			if (cacheSize <= maxCacheSize)
			{
				break;
			}
			int num10 = -1;
			for (int i = 0; i < recordCount; i++)
			{
				int lookup = records[i].Lookup;
				if (num10 < lookup)
				{
					num10 = lookup;
				}
			}
			int num11 = 0;
			for (int j = 0; j < recordCount; j++)
			{
				if (records[j].Lookup != num10 && num11 != j)
				{
					records[num11] = records[j];
					num11++;
				}
			}
			recordCount = num11;
			lastLookupAdded = num10 - 1;
		}
		_ = recordCount;
	}

	private unsafe static int FillTableCache(IOpenTypeFont font, OpenTypeTags tableTag, int cacheSize, GlyphLookupRecord[] records, int recordCount, int glyphCount, int lastLookupAdded)
	{
		byte[] array = font.AllocateTableCache(tableTag, cacheSize);
		if (array == null)
		{
			return 0;
		}
		fixed (byte* ptr = &array[0])
		{
			ushort* ptr2 = (ushort*)ptr;
			*ptr2 = (ushort)cacheSize;
			ptr2[1] = ushort.MaxValue;
			ptr2[2] = (ushort)(lastLookupAdded + 1);
			ptr2[3] = (ushort)glyphCount;
			ushort* ptr3 = ptr2 + 4;
			ushort* ptr4 = ptr3 + glyphCount * 2;
			ushort* ptr5 = null;
			int glyphListIndex = -1;
			int num = 0;
			int num2 = 0;
			int num3 = 1;
			ushort glyph = records[0].Glyph;
			for (int i = 1; i < recordCount; i++)
			{
				if (records[i].Glyph == glyph)
				{
					continue;
				}
				if (num != num3 || !CompareGlyphRecordLists(records, recordCount, glyphListIndex, num2))
				{
					ptr5 = ptr4;
					for (int j = num2; j < i; j++)
					{
						*ptr4 = records[j].Lookup;
						ptr4++;
					}
					*ptr4 = ushort.MaxValue;
					ptr4++;
				}
				*ptr3 = glyph;
				ptr3++;
				*ptr3 = (ushort)((ptr5 - ptr2) * 2);
				ptr3++;
				glyphListIndex = num2;
				num = num3;
				glyph = records[i].Glyph;
				num2 = i;
				num3 = 1;
			}
			if (num != num3 || !CompareGlyphRecordLists(records, recordCount, glyphListIndex, num2))
			{
				ptr5 = ptr4;
				for (int k = num2; k < recordCount; k++)
				{
					*ptr4 = records[k].Lookup;
					ptr4++;
				}
				*ptr4 = ushort.MaxValue;
				ptr4++;
			}
			*ptr3 = glyph;
			ptr3++;
			*ptr3 = (ushort)((ptr5 - ptr2) * 2);
			ptr3++;
		}
		return cacheSize;
	}

	private static void CalculateCacheSize(GlyphLookupRecord[] records, int recordCount, out int cacheSize, out int glyphCount)
	{
		glyphCount = 1;
		int num = 0;
		int num2 = 0;
		int glyphListIndex = -1;
		int num3 = 0;
		int num4 = 0;
		int num5 = 1;
		ushort glyph = records[0].Glyph;
		for (int i = 1; i < recordCount; i++)
		{
			if (records[i].Glyph != glyph)
			{
				glyphCount++;
				if (num3 != num5 || !CompareGlyphRecordLists(records, recordCount, glyphListIndex, num4))
				{
					num++;
					num2 += num5;
				}
				glyphListIndex = num4;
				num3 = num5;
				glyph = records[i].Glyph;
				num4 = i;
				num5 = 1;
			}
			else
			{
				num5++;
			}
		}
		if (num3 != num5 || !CompareGlyphRecordLists(records, recordCount, glyphListIndex, num4))
		{
			num++;
			num2 += num5;
		}
		cacheSize = 2 * (4 + glyphCount * 2 + num2 + num);
	}

	private static bool CompareGlyphRecordLists(GlyphLookupRecord[] records, int recordCount, int glyphListIndex1, int glyphListIndex2)
	{
		ushort glyph = records[glyphListIndex1].Glyph;
		ushort glyph2 = records[glyphListIndex2].Glyph;
		while (true)
		{
			ushort num;
			ushort num2;
			if (glyphListIndex1 != recordCount)
			{
				num = records[glyphListIndex1].Glyph;
				num2 = records[glyphListIndex1].Lookup;
			}
			else
			{
				num = ushort.MaxValue;
				num2 = ushort.MaxValue;
			}
			ushort num3;
			ushort num4;
			if (glyphListIndex2 != recordCount)
			{
				num3 = records[glyphListIndex2].Glyph;
				num4 = records[glyphListIndex2].Lookup;
			}
			else
			{
				num3 = ushort.MaxValue;
				num4 = ushort.MaxValue;
			}
			if (num != glyph && num3 != glyph2)
			{
				return true;
			}
			if (num != glyph || num3 != glyph2)
			{
				return false;
			}
			if (num2 != num4)
			{
				break;
			}
			glyphListIndex1++;
			glyphListIndex2++;
		}
		return false;
	}

	private static CoverageTable GetSubtablePrincipalCoverage(FontTable table, OpenTypeTags tableTag, ushort lookupType, int subtableOffset)
	{
		_ = CoverageTable.InvalidCoverage;
		switch (tableTag)
		{
		case OpenTypeTags.GSUB:
			if (lookupType == 7)
			{
				ExtensionLookupTable extensionLookupTable2 = new ExtensionLookupTable(subtableOffset);
				lookupType = extensionLookupTable2.LookupType(table);
				subtableOffset = extensionLookupTable2.LookupSubtableOffset(table);
			}
			switch (lookupType)
			{
			case 1:
				return new SingleSubstitutionSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 2:
				return new MultipleSubstitutionSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 3:
				return new AlternateSubstitutionSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 4:
				return new LigatureSubstitutionSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 5:
				return new ContextSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 6:
				return new ChainingSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 8:
				return new ReverseChainingSubtable(subtableOffset).GetPrimaryCoverage(table);
			}
			break;
		case OpenTypeTags.GPOS:
			if (lookupType == 9)
			{
				ExtensionLookupTable extensionLookupTable = new ExtensionLookupTable(subtableOffset);
				lookupType = extensionLookupTable.LookupType(table);
				subtableOffset = extensionLookupTable.LookupSubtableOffset(table);
			}
			switch (lookupType)
			{
			case 1:
				return new SinglePositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 2:
				return new PairPositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 3:
				return new CursivePositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 4:
				return new MarkToBasePositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 5:
				return new MarkToLigaturePositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 6:
				return new MarkToMarkPositioningSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 7:
				return new ContextSubtable(subtableOffset).GetPrimaryCoverage(table);
			case 8:
				return new ChainingSubtable(subtableOffset).GetPrimaryCoverage(table);
			}
			break;
		}
		return CoverageTable.InvalidCoverage;
	}

	private static bool AppendCoverageGlyphRecords(FontTable table, ushort lookupIndex, CoverageTable coverage, GlyphLookupRecord[] records, ref int recordCount, ref int maxLookupGlyph)
	{
		switch (coverage.Format(table))
		{
		case 1:
		{
			ushort num5 = coverage.Format1GlyphCount(table);
			for (ushort num6 = 0; num6 < num5; num6++)
			{
				if (!AppendGlyphRecord(coverage.Format1Glyph(table, num6), lookupIndex, records, ref recordCount, ref maxLookupGlyph))
				{
					return false;
				}
			}
			break;
		}
		case 2:
		{
			ushort num = coverage.Format2RangeCount(table);
			for (ushort num2 = 0; num2 < num; num2++)
			{
				ushort num3 = coverage.Format2RangeStartGlyph(table, num2);
				ushort num4 = coverage.Format2RangeEndGlyph(table, num2);
				for (int i = num3; i <= num4; i++)
				{
					if (!AppendGlyphRecord((ushort)i, lookupIndex, records, ref recordCount, ref maxLookupGlyph))
					{
						return false;
					}
				}
			}
			break;
		}
		}
		return true;
	}

	private static bool AppendGlyphRecord(ushort glyph, ushort lookupIndex, GlyphLookupRecord[] records, ref int recordCount, ref int maxLookupGlyph)
	{
		if (glyph == maxLookupGlyph)
		{
			return true;
		}
		if (glyph > maxLookupGlyph)
		{
			maxLookupGlyph = glyph;
		}
		else
		{
			int num = recordCount - 1;
			while (num >= 0 && records[num].Lookup == lookupIndex)
			{
				if (records[num].Glyph == glyph)
				{
					return true;
				}
				num--;
			}
		}
		if (recordCount == records.Length)
		{
			return false;
		}
		records[recordCount] = new GlyphLookupRecord(glyph, lookupIndex);
		recordCount++;
		return true;
	}
}
