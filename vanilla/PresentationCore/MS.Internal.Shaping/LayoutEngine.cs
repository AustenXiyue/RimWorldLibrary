using System.IO;

namespace MS.Internal.Shaping;

internal static class LayoutEngine
{
	public const ushort LookupFlagRightToLeft = 1;

	public const ushort LookupFlagIgnoreBases = 2;

	public const ushort LookupFlagIgnoreLigatures = 4;

	public const ushort LookupFlagIgnoreMarks = 8;

	public const ushort LookupFlagMarkAttachmentTypeMask = 65280;

	public const ushort LookupFlagFindBase = 8;

	public const int LookForward = 1;

	public const int LookBackward = -1;

	public unsafe static void ApplyFeatures(IOpenTypeFont Font, OpenTypeLayoutWorkspace workspace, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, LangSysTable LangSys, FeatureList Features, LookupList Lookups, Feature[] FeatureSet, int featureCount, int featureSetOffset, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets)
	{
		UpdateGlyphFlags(Font, GlyphInfo, 0, GlyphInfo.Length, DoAll: false, GlyphFlags.Unassigned);
		if (workspace == null)
		{
			workspace = new OpenTypeLayoutWorkspace();
		}
		ushort num = Lookups.LookupCount(Table);
		CompileFeatureSet(FeatureSet, featureCount, featureSetOffset, CharCount, Table, LangSys, Features, num, workspace);
		OpenTypeLayoutCache.InitCache(Font, TableTag, GlyphInfo, workspace);
		for (ushort lookupIndex = 0; lookupIndex < num; lookupIndex++)
		{
			if (workspace.IsAggregatedFlagSet(lookupIndex))
			{
				int FirstChar = 0;
				int AfterLastChar = 0;
				int firstGlyph = 0;
				int afterLastGlyph = 0;
				OpenTypeLayoutCache.FindNextLookup(workspace, GlyphInfo, lookupIndex, out lookupIndex, out firstGlyph);
				if (lookupIndex >= num)
				{
					break;
				}
				if (workspace.IsAggregatedFlagSet(lookupIndex))
				{
					LookupTable lookup = Lookups.Lookup(Table, lookupIndex);
					uint Parameter = 0u;
					bool flag = IsLookupReversal(TableTag, lookup.LookupType());
					while (firstGlyph < GlyphInfo.Length)
					{
						if (!OpenTypeLayoutCache.FindNextGlyphInLookup(workspace, lookupIndex, flag, ref firstGlyph, ref afterLastGlyph))
						{
							firstGlyph = afterLastGlyph;
						}
						if (firstGlyph < afterLastGlyph)
						{
							int length = GlyphInfo.Length;
							int num2 = length - afterLastGlyph;
							if (ApplyLookup(Font, TableTag, Table, Metrics, lookup, CharCount, Charmap, GlyphInfo, Advances, Offsets, firstGlyph, afterLastGlyph, Parameter, 0, out var NextGlyph))
							{
								if (!flag)
								{
									OpenTypeLayoutCache.OnGlyphsChanged(workspace, GlyphInfo, length, firstGlyph, NextGlyph);
									afterLastGlyph = GlyphInfo.Length - num2;
									firstGlyph = NextGlyph;
								}
								else
								{
									OpenTypeLayoutCache.OnGlyphsChanged(workspace, GlyphInfo, length, NextGlyph, afterLastGlyph);
									afterLastGlyph = NextGlyph;
								}
							}
							else if (flag)
							{
								afterLastGlyph = NextGlyph;
							}
							else
							{
								firstGlyph = NextGlyph;
							}
						}
						else
						{
							GetNextEnabledGlyphRange(FeatureSet, featureCount, featureSetOffset, Table, workspace, LangSys, Features, lookupIndex, CharCount, Charmap, AfterLastChar, afterLastGlyph, GlyphInfo.Length, out FirstChar, out AfterLastChar, out firstGlyph, out afterLastGlyph, out Parameter);
						}
					}
				}
			}
		}
	}

	internal unsafe static bool ApplyLookup(IOpenTypeFont Font, OpenTypeTags TableTag, FontTable Table, LayoutMetrics Metrics, LookupTable Lookup, int CharCount, UshortList Charmap, GlyphInfoList GlyphInfo, int* Advances, LayoutOffset* Offsets, int FirstGlyph, int AfterLastGlyph, uint Parameter, int nestingLevel, out int NextGlyph)
	{
		ushort num = Lookup.LookupType();
		ushort lookupFlags = Lookup.LookupFlags();
		ushort num2 = Lookup.SubTableCount();
		bool flag = false;
		NextGlyph = FirstGlyph + 1;
		if (!IsLookupReversal(TableTag, num))
		{
			FirstGlyph = GetNextGlyphInLookup(Font, GlyphInfo, FirstGlyph, lookupFlags, 1);
		}
		else
		{
			AfterLastGlyph = GetNextGlyphInLookup(Font, GlyphInfo, AfterLastGlyph - 1, lookupFlags, -1) + 1;
		}
		if (FirstGlyph >= AfterLastGlyph)
		{
			return flag;
		}
		ushort num3 = num;
		ushort num4 = 0;
		while (!flag && num4 < num2)
		{
			num = num3;
			int offset = Lookup.SubtableOffset(Table, num4);
			switch (TableTag)
			{
			case OpenTypeTags.GSUB:
				if (num == 7)
				{
					ExtensionLookupTable extensionLookupTable2 = new ExtensionLookupTable(offset);
					num = extensionLookupTable2.LookupType(Table);
					offset = extensionLookupTable2.LookupSubtableOffset(Table);
				}
				switch (num)
				{
				case 1:
					flag = new SingleSubstitutionSubtable(offset).Apply(Table, GlyphInfo, FirstGlyph, out NextGlyph);
					break;
				case 2:
					flag = new MultipleSubstitutionSubtable(offset).Apply(Font, Table, CharCount, Charmap, GlyphInfo, lookupFlags, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 3:
					flag = new AlternateSubstitutionSubtable(offset).Apply(Table, GlyphInfo, Parameter, FirstGlyph, out NextGlyph);
					break;
				case 4:
					flag = new LigatureSubstitutionSubtable(offset).Apply(Font, Table, CharCount, Charmap, GlyphInfo, lookupFlags, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 5:
					flag = new ContextSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, lookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
					break;
				case 6:
					flag = new ChainingSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, lookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
					break;
				case 7:
					NextGlyph = FirstGlyph + 1;
					break;
				case 8:
					flag = new ReverseChainingSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, lookupFlags, FirstGlyph, AfterLastGlyph, Parameter, out NextGlyph);
					break;
				default:
					NextGlyph = FirstGlyph + 1;
					break;
				}
				if (flag)
				{
					if (!IsLookupReversal(TableTag, num))
					{
						UpdateGlyphFlags(Font, GlyphInfo, FirstGlyph, NextGlyph, DoAll: true, GlyphFlags.Substituted);
					}
					else
					{
						UpdateGlyphFlags(Font, GlyphInfo, NextGlyph, AfterLastGlyph, DoAll: true, GlyphFlags.Substituted);
					}
				}
				break;
			case OpenTypeTags.GPOS:
				if (num == 9)
				{
					ExtensionLookupTable extensionLookupTable = new ExtensionLookupTable(offset);
					num = extensionLookupTable.LookupType(Table);
					offset = extensionLookupTable.LookupSubtableOffset(Table);
				}
				switch (num)
				{
				case 1:
					flag = new SinglePositioningSubtable(offset).Apply(Table, Metrics, GlyphInfo, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 2:
					flag = new PairPositioningSubtable(offset).Apply(Font, Table, Metrics, GlyphInfo, lookupFlags, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 3:
					new CursivePositioningSubtable(offset).Apply(Font, Table, Metrics, GlyphInfo, lookupFlags, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 4:
					flag = new MarkToBasePositioningSubtable(offset).Apply(Font, Table, Metrics, GlyphInfo, lookupFlags, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 5:
					flag = new MarkToLigaturePositioningSubtable(offset).Apply(Font, Table, Metrics, GlyphInfo, lookupFlags, CharCount, Charmap, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 6:
					flag = new MarkToMarkPositioningSubtable(offset).Apply(Font, Table, Metrics, GlyphInfo, lookupFlags, Advances, Offsets, FirstGlyph, AfterLastGlyph, out NextGlyph);
					break;
				case 7:
					flag = new ContextSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, lookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
					break;
				case 8:
					flag = new ChainingSubtable(offset).Apply(Font, TableTag, Table, Metrics, CharCount, Charmap, GlyphInfo, Advances, Offsets, lookupFlags, FirstGlyph, AfterLastGlyph, Parameter, nestingLevel, out NextGlyph);
					break;
				case 9:
					NextGlyph = FirstGlyph + 1;
					break;
				default:
					NextGlyph = FirstGlyph + 1;
					break;
				}
				if (flag)
				{
					UpdateGlyphFlags(Font, GlyphInfo, FirstGlyph, NextGlyph, DoAll: false, GlyphFlags.Positioned);
				}
				break;
			}
			num4++;
		}
		return flag;
	}

	private static bool IsLookupReversal(OpenTypeTags TableTag, ushort LookupType)
	{
		if (TableTag == OpenTypeTags.GSUB)
		{
			return LookupType == 8;
		}
		return false;
	}

	private static void CompileFeatureSet(Feature[] FeatureSet, int featureCount, int featureSetOffset, int charCount, FontTable Table, LangSysTable LangSys, FeatureList Features, int lookupCount, OpenTypeLayoutWorkspace workspace)
	{
		workspace.InitLookupUsageFlags(lookupCount, featureCount);
		FeatureTable featureTable = LangSys.RequiredFeature(Table, Features);
		if (!featureTable.IsNull)
		{
			int num = featureTable.LookupCount(Table);
			for (ushort num2 = 0; num2 < num; num2++)
			{
				workspace.SetRequiredFeatureFlag(featureTable.LookupIndex(Table, num2));
			}
		}
		for (int i = 0; i < featureCount; i++)
		{
			Feature feature = FeatureSet[i];
			if (feature.Parameter == 0 || feature.StartIndex >= featureSetOffset + charCount || feature.StartIndex + feature.Length <= featureSetOffset)
			{
				continue;
			}
			FeatureTable featureTable2 = LangSys.FindFeature(Table, Features, feature.Tag);
			if (!featureTable2.IsNull)
			{
				int num3 = featureTable2.LookupCount(Table);
				for (ushort num4 = 0; num4 < num3; num4++)
				{
					workspace.SetFeatureFlag(featureTable2.LookupIndex(Table, num4), i);
				}
			}
		}
	}

	private static void GetNextEnabledGlyphRange(Feature[] FeatureSet, int featureCount, int featureSetOffset, FontTable Table, OpenTypeLayoutWorkspace workspace, LangSysTable LangSys, FeatureList Features, ushort lookupIndex, int CharCount, UshortList Charmap, int StartChar, int StartGlyph, int GlyphRunLength, out int FirstChar, out int AfterLastChar, out int FirstGlyph, out int AfterLastGlyph, out uint Parameter)
	{
		FirstChar = int.MaxValue;
		AfterLastChar = int.MaxValue;
		FirstGlyph = StartGlyph;
		AfterLastGlyph = GlyphRunLength;
		Parameter = 0u;
		if (workspace.IsRequiredFeatureFlagSet(lookupIndex))
		{
			FirstChar = StartChar;
			AfterLastChar = CharCount;
			FirstGlyph = StartGlyph;
			AfterLastGlyph = GlyphRunLength;
			return;
		}
		for (int i = 0; i < featureCount; i++)
		{
			if (workspace.IsFeatureFlagSet(lookupIndex, i))
			{
				Feature feature = FeatureSet[i];
				int num = feature.StartIndex - featureSetOffset;
				if (num < 0)
				{
					num = 0;
				}
				int num2 = feature.StartIndex + feature.Length - featureSetOffset;
				if (num2 > CharCount)
				{
					num2 = CharCount;
				}
				if (num2 > StartChar && (num < FirstChar || (num == FirstChar && num2 >= AfterLastChar)))
				{
					FirstChar = num;
					AfterLastChar = num2;
					Parameter = feature.Parameter;
				}
			}
		}
		if (FirstChar == int.MaxValue)
		{
			FirstGlyph = GlyphRunLength;
			AfterLastGlyph = GlyphRunLength;
			return;
		}
		if (StartGlyph > Charmap[FirstChar])
		{
			FirstGlyph = StartGlyph;
		}
		else
		{
			FirstGlyph = Charmap[FirstChar];
		}
		if (AfterLastChar < CharCount)
		{
			AfterLastGlyph = Charmap[AfterLastChar];
		}
		else
		{
			AfterLastGlyph = GlyphRunLength;
		}
	}

	private static void UpdateGlyphFlags(IOpenTypeFont Font, GlyphInfoList GlyphInfo, int FirstGlyph, int AfterLastGlyph, bool DoAll, GlyphFlags FlagToSet)
	{
		ushort num = 7;
		FontTable fontTable = Font.GetFontTable(OpenTypeTags.GDEF);
		if (!fontTable.IsPresent)
		{
			for (int i = FirstGlyph; i < AfterLastGlyph; i++)
			{
				_ = GlyphInfo.GlyphFlags[i];
			}
			return;
		}
		ClassDefTable glyphClassDef = new GDEFHeader(0).GetGlyphClassDef(fontTable);
		for (int j = FirstGlyph; j < AfterLastGlyph; j++)
		{
			ushort num2 = (ushort)((uint)GlyphInfo.GlyphFlags[j] | (uint)FlagToSet);
			if ((num2 & num) == 7 || FlagToSet != 0)
			{
				ushort glyph = GlyphInfo.Glyphs[j];
				num2 &= (ushort)(~num);
				int @class = glyphClassDef.GetClass(fontTable, glyph);
				GlyphInfo.GlyphFlags[j] = (ushort)(num2 | ((@class != -1) ? ((ushort)@class) : 0));
			}
		}
	}

	internal static int GetNextGlyphInLookup(IOpenTypeFont Font, GlyphInfoList GlyphInfo, int FirstGlyph, ushort LookupFlags, int Direction)
	{
		FontTable fontTable = null;
		ClassDefTable classDefTable = ClassDefTable.InvalidClassDef;
		if (LookupFlags == 0)
		{
			return FirstGlyph;
		}
		if ((LookupFlags & 0xFF00) != 0)
		{
			fontTable = Font.GetFontTable(OpenTypeTags.GDEF);
			if (fontTable.IsPresent)
			{
				classDefTable = new GDEFHeader(0).GetMarkAttachClassDef(fontTable);
			}
		}
		UshortList glyphFlags = GlyphInfo.GlyphFlags;
		ushort num = (ushort)((LookupFlags & 0xFF00) >> 8);
		int length = GlyphInfo.Length;
		int i;
		for (i = FirstGlyph; i < length && i >= 0; i += Direction)
		{
			if (((LookupFlags & 2) == 0 || (glyphFlags[i] & 7) != 1) && ((LookupFlags & 8) == 0 || (glyphFlags[i] & 7) != 3) && ((LookupFlags & 4) == 0 || (glyphFlags[i] & 7) != 2) && (num == 0 || (glyphFlags[i] & 7) != 3 || classDefTable.IsInvalid || num == classDefTable.GetClass(fontTable, GlyphInfo.Glyphs[i])))
			{
				return i;
			}
		}
		return i;
	}

	internal static void GetComplexLanguageList(OpenTypeTags tableTag, FontTable table, uint[] featureTagsList, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId, out WritingSystem[] complexLanguages, out int complexLanguageCount)
	{
		ScriptList scriptList = new ScriptList(0);
		FeatureList featureList = new FeatureList(0);
		LookupList lookupList = new LookupList(0);
		switch (tableTag)
		{
		case OpenTypeTags.GSUB:
		{
			GSUBHeader gSUBHeader = new GSUBHeader(0);
			scriptList = gSUBHeader.GetScriptList(table);
			featureList = gSUBHeader.GetFeatureList(table);
			lookupList = gSUBHeader.GetLookupList(table);
			break;
		}
		case OpenTypeTags.GPOS:
		{
			GPOSHeader gPOSHeader = new GPOSHeader(0);
			scriptList = gPOSHeader.GetScriptList(table);
			featureList = gPOSHeader.GetFeatureList(table);
			lookupList = gPOSHeader.GetLookupList(table);
			break;
		}
		}
		int scriptCount = scriptList.GetScriptCount(table);
		int num = featureList.FeatureCount(table);
		int num2 = lookupList.LookupCount(table);
		uint[] array = new uint[num2 + 31 >> 5];
		for (int i = 0; i < num2 + 31 >> 5; i++)
		{
			array[i] = 0u;
		}
		for (ushort num3 = 0; num3 < num; num3++)
		{
			uint num4 = featureList.FeatureTag(table, num3);
			bool flag = false;
			for (int j = 0; j < featureTagsList.Length; j++)
			{
				if (featureTagsList[j] == num4)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				FeatureTable featureTable = featureList.FeatureTable(table, num3);
				ushort num5 = featureTable.LookupCount(table);
				for (ushort num6 = 0; num6 < num5; num6++)
				{
					ushort num7 = featureTable.LookupIndex(table, num6);
					if (num7 >= num2)
					{
						throw new FileFormatException();
					}
					array[num7 >> 5] |= (uint)(1 << num7 % 32);
				}
			}
		}
		for (ushort num8 = 0; num8 < num2; num8++)
		{
			if ((array[num8 >> 5] & (1 << num8 % 32)) != 0L)
			{
				LookupTable lookupTable = lookupList.Lookup(table, num8);
				ushort num9 = lookupTable.LookupType();
				ushort num10 = lookupTable.SubTableCount();
				bool flag2 = false;
				ushort num11 = num9;
				ushort num12 = 0;
				while (!flag2 && num12 < num10)
				{
					num9 = num11;
					int offset = lookupTable.SubtableOffset(table, num12);
					switch (tableTag)
					{
					case OpenTypeTags.GSUB:
						if (num9 == 7)
						{
							ExtensionLookupTable extensionLookupTable2 = new ExtensionLookupTable(offset);
							num9 = extensionLookupTable2.LookupType(table);
							offset = extensionLookupTable2.LookupSubtableOffset(table);
						}
						switch (num9)
						{
						case 1:
							flag2 = new SingleSubstitutionSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 2:
							flag2 = new MultipleSubstitutionSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 3:
							flag2 = new AlternateSubstitutionSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 4:
							flag2 = new LigatureSubstitutionSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 5:
							flag2 = new ContextSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 6:
							flag2 = new ChainingSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 8:
							flag2 = new ReverseChainingSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						default:
							flag2 = true;
							break;
						case 7:
							break;
						}
						break;
					case OpenTypeTags.GPOS:
						if (num9 == 9)
						{
							ExtensionLookupTable extensionLookupTable = new ExtensionLookupTable(offset);
							num9 = extensionLookupTable.LookupType(table);
							offset = extensionLookupTable.LookupSubtableOffset(table);
						}
						switch (num9)
						{
						case 1:
							flag2 = new SinglePositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 2:
							flag2 = new PairPositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 3:
							flag2 = new CursivePositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 4:
							flag2 = new MarkToBasePositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 5:
							flag2 = new MarkToLigaturePositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 6:
							flag2 = new MarkToMarkPositioningSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 7:
							flag2 = new ContextSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						case 8:
							flag2 = new ChainingSubtable(offset).IsLookupCovered(table, glyphBits, minGlyphId, maxGlyphId);
							break;
						default:
							flag2 = true;
							break;
						case 9:
							break;
						}
						break;
					}
					num12++;
				}
				if (!flag2)
				{
					array[num8 >> 5] &= (uint)(~(1 << num8 % 32));
				}
			}
		}
		bool flag3 = false;
		for (int k = 0; k < num2 + 31 >> 5; k++)
		{
			if (array[k] != 0)
			{
				flag3 = true;
				break;
			}
		}
		if (!flag3)
		{
			complexLanguages = null;
			complexLanguageCount = 0;
			return;
		}
		complexLanguages = new WritingSystem[10];
		complexLanguageCount = 0;
		for (ushort num13 = 0; num13 < scriptCount; num13++)
		{
			ScriptTable scriptTable = scriptList.GetScriptTable(table, num13);
			uint scriptTag = scriptList.GetScriptTag(table, num13);
			ushort langSysCount = scriptTable.GetLangSysCount(table);
			if (scriptTable.IsDefaultLangSysExists(table))
			{
				AppendLangSys(scriptTag, 1684434036u, scriptTable.GetDefaultLangSysTable(table), featureList, table, featureTagsList, array, ref complexLanguages, ref complexLanguageCount);
			}
			for (ushort num14 = 0; num14 < langSysCount; num14++)
			{
				uint langSysTag = scriptTable.GetLangSysTag(table, num14);
				AppendLangSys(scriptTag, langSysTag, scriptTable.GetLangSysTable(table, num14), featureList, table, featureTagsList, array, ref complexLanguages, ref complexLanguageCount);
			}
		}
	}

	private static void AppendLangSys(uint scriptTag, uint langSysTag, LangSysTable langSysTable, FeatureList featureList, FontTable table, uint[] featureTagsList, uint[] lookupBits, ref WritingSystem[] complexLanguages, ref int complexLanguageCount)
	{
		ushort num = langSysTable.FeatureCount(table);
		bool flag = false;
		ushort num2 = 0;
		while (!flag && num2 < num)
		{
			ushort featureIndex = langSysTable.GetFeatureIndex(table, num2);
			uint num3 = featureList.FeatureTag(table, featureIndex);
			bool flag2 = false;
			int num4 = 0;
			while (!flag && num4 < featureTagsList.Length)
			{
				if (featureTagsList[num4] == num3)
				{
					flag2 = true;
					break;
				}
				num4++;
			}
			if (flag2)
			{
				FeatureTable featureTable = featureList.FeatureTable(table, featureIndex);
				ushort num5 = featureTable.LookupCount(table);
				for (ushort num6 = 0; num6 < num5; num6++)
				{
					ushort num7 = featureTable.LookupIndex(table, num6);
					if ((lookupBits[num7 >> 5] & (1 << num7 % 32)) != 0L)
					{
						flag = true;
						break;
					}
				}
			}
			num2++;
		}
		if (!flag)
		{
			return;
		}
		if (complexLanguages.Length == complexLanguageCount)
		{
			WritingSystem[] array = new WritingSystem[complexLanguages.Length * 3 / 2];
			for (int i = 0; i < complexLanguages.Length; i++)
			{
				array[i] = complexLanguages[i];
			}
			complexLanguages = array;
		}
		complexLanguages[complexLanguageCount].scriptTag = scriptTag;
		complexLanguages[complexLanguageCount].langSysTag = langSysTag;
		complexLanguageCount++;
	}
}
