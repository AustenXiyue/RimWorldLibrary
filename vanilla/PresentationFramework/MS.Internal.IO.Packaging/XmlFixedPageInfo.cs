using System;
using System.Globalization;
using System.Windows;
using System.Xml;

namespace MS.Internal.IO.Packaging;

internal class XmlFixedPageInfo : FixedPageInfo
{
	private const string _fixedPageName = "FixedPage";

	private const string _glyphRunName = "Glyphs";

	private XmlNode _pageNode;

	private XmlNodeList _nodeList;

	private XmlGlyphRunInfo[] _glyphRunList;

	internal override int GlyphRunCount => GlyphRunList.Length;

	private XmlGlyphRunInfo[] GlyphRunList
	{
		get
		{
			if (_glyphRunList == null)
			{
				_glyphRunList = new XmlGlyphRunInfo[NodeList.Count];
			}
			return _glyphRunList;
		}
	}

	private XmlNodeList NodeList
	{
		get
		{
			if (_nodeList == null)
			{
				string xpath = string.Format(CultureInfo.InvariantCulture, ".//*[namespace-uri()='{0}' and local-name()='{1}']", ElementTableKey.FixedMarkupNamespace, "Glyphs");
				_nodeList = _pageNode.SelectNodes(xpath);
			}
			return _nodeList;
		}
	}

	internal XmlFixedPageInfo(XmlNode fixedPageNode)
	{
		_pageNode = fixedPageNode;
		if (_pageNode.LocalName != "FixedPage" || _pageNode.NamespaceURI != ElementTableKey.FixedMarkupNamespace)
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedXmlNodeInXmlFixedPageInfoConstructor, _pageNode.NamespaceURI, _pageNode.LocalName, ElementTableKey.FixedMarkupNamespace, "FixedPage"));
		}
	}

	internal override GlyphRunInfo GlyphRunAtPosition(int position)
	{
		if (position < 0 || position >= GlyphRunList.Length)
		{
			return null;
		}
		if (GlyphRunList[position] == null)
		{
			GlyphRunList[position] = new XmlGlyphRunInfo(NodeList[position]);
		}
		return GlyphRunList[position];
	}
}
