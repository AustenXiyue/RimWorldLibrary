using System.Xml;

namespace MS.Internal.IO.Packaging;

internal class FixedPageContentExtractor
{
	private XmlFixedPageInfo _fixedPageInfo;

	private int _nextGlyphRun;

	internal bool AtEndOfPage => _nextGlyphRun >= _fixedPageInfo.GlyphRunCount;

	internal FixedPageContentExtractor(XmlNode fixedPage)
	{
		_fixedPageInfo = new XmlFixedPageInfo(fixedPage);
		_nextGlyphRun = 0;
	}

	internal string NextGlyphContent(out bool inline, out uint lcid)
	{
		inline = false;
		lcid = 0u;
		if (_nextGlyphRun >= _fixedPageInfo.GlyphRunCount)
		{
			return null;
		}
		GlyphRunInfo glyphRunInfo = _fixedPageInfo.GlyphRunAtPosition(_nextGlyphRun);
		lcid = glyphRunInfo.LanguageID;
		_nextGlyphRun++;
		return glyphRunInfo.UnicodeString;
	}
}
