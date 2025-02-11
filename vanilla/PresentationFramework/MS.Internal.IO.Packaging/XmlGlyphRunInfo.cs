using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace MS.Internal.IO.Packaging;

internal class XmlGlyphRunInfo : GlyphRunInfo
{
	private const string _glyphRunName = "Glyphs";

	private const string _xmlLangAttribute = "xml:lang";

	private const string _unicodeStringAttribute = "UnicodeString";

	private const string _undeterminedLanguageStringUpper = "UND";

	private XmlElement _glyphsNode;

	private string _unicodeString;

	private uint? _languageID;

	internal override Point StartPosition
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override Point EndPosition
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override double WidthEmFontSize
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override double HeightEmFontSize
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override bool GlyphsHaveSidewaysOrientation
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override int BidiLevel
	{
		get
		{
			throw new NotSupportedException(SR.XmlGlyphRunInfoIsNonGraphic);
		}
	}

	internal override uint LanguageID
	{
		get
		{
			checked
			{
				if (!_languageID.HasValue)
				{
					XmlElement xmlElement = _glyphsNode;
					while (xmlElement != null && !_languageID.HasValue)
					{
						string attribute = xmlElement.GetAttribute("xml:lang");
						if (attribute != null && attribute.Length > 0)
						{
							if (string.CompareOrdinal(attribute.ToUpperInvariant(), "UND") == 0)
							{
								_languageID = 0u;
							}
							else
							{
								CultureInfo compatibleCulture = XmlLanguage.GetLanguage(attribute).GetCompatibleCulture();
								_languageID = (uint)compatibleCulture.LCID;
							}
						}
						xmlElement = xmlElement.ParentNode as XmlElement;
					}
					if (!_languageID.HasValue)
					{
						_languageID = (uint)CultureInfo.InvariantCulture.LCID;
					}
				}
				return _languageID.Value;
			}
		}
	}

	internal override string UnicodeString
	{
		get
		{
			if (_unicodeString == null)
			{
				_unicodeString = _glyphsNode.GetAttribute("UnicodeString");
			}
			return _unicodeString;
		}
	}

	internal XmlGlyphRunInfo(XmlNode glyphsNode)
	{
		_glyphsNode = glyphsNode as XmlElement;
	}
}
