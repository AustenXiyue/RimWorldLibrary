using System.IO;

namespace MS.Internal.Shaping;

internal static class OpenTypeLayout
{
	internal static TagInfoFlags FindScript(IOpenTypeFont Font, uint ScriptTag)
	{
		TagInfoFlags tagInfoFlags = TagInfoFlags.None;
		try
		{
			FontTable fontTable = Font.GetFontTable(OpenTypeTags.GSUB);
			if (fontTable.IsPresent && !new GSUBHeader(0).GetScriptList(fontTable).FindScript(fontTable, ScriptTag).IsNull)
			{
				tagInfoFlags |= TagInfoFlags.Substitution;
			}
		}
		catch (FileFormatException)
		{
			return TagInfoFlags.None;
		}
		try
		{
			FontTable fontTable2 = Font.GetFontTable(OpenTypeTags.GPOS);
			if (fontTable2.IsPresent && !new GPOSHeader(0).GetScriptList(fontTable2).FindScript(fontTable2, ScriptTag).IsNull)
			{
				tagInfoFlags |= TagInfoFlags.Positioning;
			}
		}
		catch (FileFormatException)
		{
			return TagInfoFlags.None;
		}
		return tagInfoFlags;
	}

	internal static TagInfoFlags FindLangSys(IOpenTypeFont Font, uint ScriptTag, uint LangSysTag)
	{
		TagInfoFlags tagInfoFlags = TagInfoFlags.None;
		try
		{
			FontTable fontTable = Font.GetFontTable(OpenTypeTags.GSUB);
			if (fontTable.IsPresent)
			{
				ScriptTable scriptTable = new GSUBHeader(0).GetScriptList(fontTable).FindScript(fontTable, ScriptTag);
				if (!scriptTable.IsNull && !scriptTable.FindLangSys(fontTable, LangSysTag).IsNull)
				{
					tagInfoFlags |= TagInfoFlags.Substitution;
				}
			}
		}
		catch (FileFormatException)
		{
			return TagInfoFlags.None;
		}
		try
		{
			FontTable fontTable2 = Font.GetFontTable(OpenTypeTags.GPOS);
			if (fontTable2.IsPresent)
			{
				ScriptTable scriptTable2 = new GPOSHeader(0).GetScriptList(fontTable2).FindScript(fontTable2, ScriptTag);
				if (!scriptTable2.IsNull && !scriptTable2.FindLangSys(fontTable2, LangSysTag).IsNull)
				{
					tagInfoFlags |= TagInfoFlags.Positioning;
				}
			}
		}
		catch (FileFormatException)
		{
			return TagInfoFlags.None;
		}
		return tagInfoFlags;
	}

	internal unsafe static OpenTypeLayoutResult SubstituteGlyphs(IOpenTypeFont Font, OpenTypeLayoutWorkspace workspace, uint ScriptTag, uint LangSysTag, Feature[] FeatureSet, int featureCount, int featureSetOffset, int CharCount, UshortList Charmap, GlyphInfoList Glyphs)
	{
		try
		{
			FontTable fontTable = Font.GetFontTable(OpenTypeTags.GSUB);
			if (!fontTable.IsPresent)
			{
				return OpenTypeLayoutResult.ScriptNotFound;
			}
			GSUBHeader gSUBHeader = new GSUBHeader(0);
			ScriptTable scriptTable = gSUBHeader.GetScriptList(fontTable).FindScript(fontTable, ScriptTag);
			if (scriptTable.IsNull)
			{
				return OpenTypeLayoutResult.ScriptNotFound;
			}
			LangSysTable langSys = scriptTable.FindLangSys(fontTable, LangSysTag);
			if (langSys.IsNull)
			{
				return OpenTypeLayoutResult.LangSysNotFound;
			}
			FeatureList featureList = gSUBHeader.GetFeatureList(fontTable);
			LookupList lookupList = gSUBHeader.GetLookupList(fontTable);
			LayoutEngine.ApplyFeatures(Font, workspace, OpenTypeTags.GSUB, fontTable, default(LayoutMetrics), langSys, featureList, lookupList, FeatureSet, featureCount, featureSetOffset, CharCount, Charmap, Glyphs, null, null);
		}
		catch (FileFormatException)
		{
			return OpenTypeLayoutResult.BadFontTable;
		}
		return OpenTypeLayoutResult.Success;
	}

	internal unsafe static OpenTypeLayoutResult PositionGlyphs(IOpenTypeFont Font, OpenTypeLayoutWorkspace workspace, uint ScriptTag, uint LangSysTag, LayoutMetrics Metrics, Feature[] FeatureSet, int featureCount, int featureSetOffset, int CharCount, UshortList Charmap, GlyphInfoList Glyphs, int* Advances, LayoutOffset* Offsets)
	{
		try
		{
			FontTable fontTable = Font.GetFontTable(OpenTypeTags.GPOS);
			if (!fontTable.IsPresent)
			{
				return OpenTypeLayoutResult.ScriptNotFound;
			}
			GPOSHeader gPOSHeader = new GPOSHeader(0);
			ScriptTable scriptTable = gPOSHeader.GetScriptList(fontTable).FindScript(fontTable, ScriptTag);
			if (scriptTable.IsNull)
			{
				return OpenTypeLayoutResult.ScriptNotFound;
			}
			LangSysTable langSys = scriptTable.FindLangSys(fontTable, LangSysTag);
			if (langSys.IsNull)
			{
				return OpenTypeLayoutResult.LangSysNotFound;
			}
			FeatureList featureList = gPOSHeader.GetFeatureList(fontTable);
			LookupList lookupList = gPOSHeader.GetLookupList(fontTable);
			LayoutEngine.ApplyFeatures(Font, workspace, OpenTypeTags.GPOS, fontTable, Metrics, langSys, featureList, lookupList, FeatureSet, featureCount, featureSetOffset, CharCount, Charmap, Glyphs, Advances, Offsets);
		}
		catch (FileFormatException)
		{
			return OpenTypeLayoutResult.BadFontTable;
		}
		return OpenTypeLayoutResult.Success;
	}

	internal static OpenTypeLayoutResult CreateLayoutCache(IOpenTypeFont font, int maxCacheSize)
	{
		OpenTypeLayoutCache.CreateCache(font, maxCacheSize);
		return OpenTypeLayoutResult.Success;
	}

	internal static OpenTypeLayoutResult GetComplexLanguageList(IOpenTypeFont Font, uint[] featureList, uint[] glyphBits, ushort minGlyphId, ushort maxGlyphId, out WritingSystem[] complexLanguages)
	{
		try
		{
			WritingSystem[] complexLanguages2 = null;
			WritingSystem[] complexLanguages3 = null;
			int complexLanguageCount = 0;
			int complexLanguageCount2 = 0;
			FontTable fontTable = Font.GetFontTable(OpenTypeTags.GSUB);
			FontTable fontTable2 = Font.GetFontTable(OpenTypeTags.GPOS);
			if (fontTable.IsPresent)
			{
				LayoutEngine.GetComplexLanguageList(OpenTypeTags.GSUB, fontTable, featureList, glyphBits, minGlyphId, maxGlyphId, out complexLanguages2, out complexLanguageCount);
			}
			if (fontTable2.IsPresent)
			{
				LayoutEngine.GetComplexLanguageList(OpenTypeTags.GPOS, fontTable2, featureList, glyphBits, minGlyphId, maxGlyphId, out complexLanguages3, out complexLanguageCount2);
			}
			if (complexLanguages2 == null && complexLanguages3 == null)
			{
				complexLanguages = null;
				return OpenTypeLayoutResult.Success;
			}
			int num = 0;
			for (int i = 0; i < complexLanguageCount2; i++)
			{
				bool flag = false;
				for (int j = 0; j < complexLanguageCount; j++)
				{
					if (complexLanguages2[j].scriptTag == complexLanguages3[i].scriptTag && complexLanguages2[j].langSysTag == complexLanguages3[i].langSysTag)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (num < i)
					{
						complexLanguages3[num] = complexLanguages3[i];
					}
					num++;
				}
			}
			complexLanguages = new WritingSystem[complexLanguageCount + num];
			for (int i = 0; i < complexLanguageCount; i++)
			{
				complexLanguages[i] = complexLanguages2[i];
			}
			for (int i = 0; i < num; i++)
			{
				complexLanguages[complexLanguageCount + i] = complexLanguages3[i];
			}
			return OpenTypeLayoutResult.Success;
		}
		catch (FileFormatException)
		{
			complexLanguages = null;
			return OpenTypeLayoutResult.BadFontTable;
		}
	}
}
