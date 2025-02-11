using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using MS.Internal.PresentationCore;

namespace MS.Internal.FontFace;

internal class CompositeFontParser
{
	private const NumberStyles UnsignedDecimalPointStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint;

	private const NumberStyles SignedDecimalPointStyle = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;

	private CompositeFontInfo _compositeFontInfo;

	private XmlReader _reader;

	private Hashtable _namespaceMap;

	private TypeConverter _doubleTypeConverter;

	private TypeConverter _xmlLanguageTypeConverter;

	private const string SystemClrNamespace = "System";

	private const string CompositeFontNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/composite-font";

	private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

	private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

	private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

	private const string FontFamilyCollectionElement = "FontFamilyCollection";

	private const string FontFamilyElement = "FontFamily";

	private const string BaselineAttribute = "Baseline";

	private const string LineSpacingAttribute = "LineSpacing";

	private const string FamilyNamesPropertyElement = "FontFamily.FamilyNames";

	private const string StringElement = "String";

	private const string FamilyTypefacesPropertyElement = "FontFamily.FamilyTypefaces";

	private const string FamilyTypefaceElement = "FamilyTypeface";

	private const string FamilyMapsPropertyElement = "FontFamily.FamilyMaps";

	private const string FamilyMapElement = "FontFamilyMap";

	private const string KeyAttribute = "Key";

	private const string LanguageAttribute = "Language";

	private const string NameAttribute = "Name";

	private const string StyleAttribute = "Style";

	private const string WeightAttribute = "Weight";

	private const string StretchAttribute = "Stretch";

	private const string UnderlinePositionAttribute = "UnderlinePosition";

	private const string UnderlineThicknessAttribute = "UnderlineThickness";

	private const string StrikethroughPositionAttribute = "StrikethroughPosition";

	private const string StrikethroughThicknessAttribute = "StrikethroughThickness";

	private const string CapsHeightAttribute = "CapsHeight";

	private const string XHeightAttribute = "XHeight";

	private const string UnicodeAttribute = "Unicode";

	private const string TargetAttribute = "Target";

	private const string ScaleAttribute = "Scale";

	private const string DeviceFontNameAttribute = "DeviceFontName";

	private const string DeviceFontCharacterMetricsPropertyElement = "FamilyTypeface.DeviceFontCharacterMetrics";

	private const string CharacterMetricsElement = "CharacterMetrics";

	private const string MetricsAttribute = "Metrics";

	private const string OsAttribute = "OS";

	internal static void VerifyMultiplierOfEm(string propertyName, ref double value)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException(SR.Format(SR.PropertyValueCannotBeNaN, propertyName));
		}
		if (value > 100.0)
		{
			value = 100.0;
		}
		else if (value < -100.0)
		{
			value = -100.0;
		}
	}

	internal static void VerifyPositiveMultiplierOfEm(string propertyName, ref double value)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException(SR.Format(SR.PropertyValueCannotBeNaN, propertyName));
		}
		if (value > 100.0)
		{
			value = 100.0;
		}
		else if (value <= 0.0)
		{
			throw new ArgumentException(SR.Format(SR.PropertyMustBeGreaterThanZero, propertyName));
		}
	}

	internal static void VerifyNonNegativeMultiplierOfEm(string propertyName, ref double value)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException(SR.Format(SR.PropertyValueCannotBeNaN, propertyName));
		}
		if (value > 100.0)
		{
			value = 100.0;
		}
		else if (value < 0.0)
		{
			throw new ArgumentException(SR.Format(SR.PropertyCannotBeNegative, propertyName));
		}
	}

	private double GetAttributeAsDouble()
	{
		object obj = null;
		try
		{
			obj = _doubleTypeConverter.ConvertFromString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, GetAttributeValue());
		}
		catch (NotSupportedException)
		{
			FailAttributeValue();
		}
		if (obj == null)
		{
			FailAttributeValue();
		}
		return (double)obj;
	}

	private XmlLanguage GetAttributeAsXmlLanguage()
	{
		object obj = null;
		try
		{
			obj = _xmlLanguageTypeConverter.ConvertFromString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, GetAttributeValue());
		}
		catch (NotSupportedException)
		{
			FailAttributeValue();
		}
		if (obj == null)
		{
			FailAttributeValue();
		}
		return (XmlLanguage)obj;
	}

	private string GetAttributeValue()
	{
		string text = _reader.Value;
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		if (text[0] == '{')
		{
			if (text.Length > 1 && text[1] == '}')
			{
				text = text.Substring(2);
			}
			else
			{
				FailAttributeValue();
			}
		}
		return text;
	}

	internal static CompositeFontInfo LoadXml(Stream fileStream)
	{
		return new CompositeFontParser(fileStream)._compositeFontInfo;
	}

	private CompositeFontParser(Stream fileStream)
	{
		_compositeFontInfo = new CompositeFontInfo();
		_namespaceMap = new Hashtable();
		_doubleTypeConverter = TypeDescriptor.GetConverter(typeof(double));
		_xmlLanguageTypeConverter = new XmlLanguageConverter();
		_reader = CreateXmlReader(fileStream);
		try
		{
			if (IsStartElement("FontFamily", "http://schemas.microsoft.com/winfx/2006/xaml/composite-font"))
			{
				ParseFontFamilyElement();
			}
			else if (IsStartElement("FontFamilyCollection", "http://schemas.microsoft.com/winfx/2006/xaml/composite-font"))
			{
				ParseFontFamilyCollectionElement();
			}
			else
			{
				FailUnknownElement();
			}
		}
		catch (XmlException x)
		{
			FailNotWellFormed(x);
		}
		catch (Exception ex) when (string.Equals(ex.GetType().FullName, "System.Security.XmlSyntaxException", StringComparison.OrdinalIgnoreCase))
		{
			FailNotWellFormed(ex);
		}
		catch (FormatException ex2)
		{
			if (_reader.NodeType == XmlNodeType.Attribute)
			{
				FailAttributeValue(ex2);
			}
			else
			{
				Fail(ex2.Message, ex2);
			}
		}
		catch (ArgumentException ex3)
		{
			if (_reader.NodeType == XmlNodeType.Attribute)
			{
				FailAttributeValue(ex3);
			}
			else
			{
				Fail(ex3.Message, ex3);
			}
		}
		finally
		{
			_reader.Close();
			_reader = null;
		}
	}

	private XmlReader CreateXmlReader(Stream fileStream)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.CloseInput = true;
		xmlReaderSettings.IgnoreComments = true;
		xmlReaderSettings.IgnoreWhitespace = false;
		xmlReaderSettings.ProhibitDtd = true;
		return new XmlCompatibilityReader(XmlReader.Create(fileStream, xmlReaderSettings), IsXmlNamespaceSupported);
	}

	private bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
	{
		newXmlNamespace = null;
		if (!(xmlNamespace == "http://schemas.microsoft.com/winfx/2006/xaml/composite-font") && !(xmlNamespace == "http://schemas.microsoft.com/winfx/2006/xaml"))
		{
			return IsMappedNamespace(xmlNamespace);
		}
		return true;
	}

	private bool IsStartElement(string localName, string namespaceURI)
	{
		MoveToContent();
		return _reader.IsStartElement(localName, namespaceURI);
	}

	private XmlNodeType MoveToContent()
	{
		bool flag = false;
		do
		{
			XmlNodeType nodeType = _reader.NodeType;
			if (nodeType == XmlNodeType.Element || (uint)(nodeType - 4) <= 1u || (uint)(nodeType - 15) <= 1u)
			{
				flag = true;
			}
		}
		while (!flag && _reader.Read());
		return _reader.NodeType;
	}

	private bool IsMappedNamespace(string xmlNamespace)
	{
		return _namespaceMap.ContainsKey(xmlNamespace);
	}

	private bool IsSystemNamespace(string xmlNamespace)
	{
		return xmlNamespace == "clr-namespace:System;assembly=mscorlib";
	}

	private void ParseFontFamilyCollectionElement()
	{
		bool flag = false;
		while (_reader.Read())
		{
			if (Enum.TryParse<OperatingSystemVersion>(_reader.GetAttribute("OS"), out var result) && OSVersionHelper.IsOsVersionOrGreater(result))
			{
				flag = true;
				ParseFontFamilyElement();
				return;
			}
		}
		if (!flag)
		{
			Fail($"No FontFamily element found in FontFamilyCollection that matches current OS or greater: {OSVersionHelper.GetOsVersion().ToString()}");
		}
	}

	private void ParseFontFamilyElement()
	{
		if (_reader.MoveToFirstAttribute())
		{
			do
			{
				if (IsCompositeFontAttribute())
				{
					string localName = _reader.LocalName;
					if (localName == "Baseline")
					{
						_compositeFontInfo.Baseline = GetAttributeAsDouble();
					}
					else if (localName == "LineSpacing")
					{
						_compositeFontInfo.LineSpacing = GetAttributeAsDouble();
					}
					else if (localName != "OS")
					{
						FailUnknownAttribute();
					}
				}
				else if (!IsIgnorableAttribute())
				{
					FailUnknownAttribute();
				}
			}
			while (_reader.MoveToNextAttribute());
			_reader.MoveToElement();
		}
		if (_reader.IsEmptyElement)
		{
			VerifyCompositeFontInfo();
			_reader.Read();
			return;
		}
		_reader.Read();
		while (MoveToContent() != XmlNodeType.EndElement)
		{
			if (_reader.NodeType == XmlNodeType.Element && _reader.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml/composite-font")
			{
				bool isEmptyElement = _reader.IsEmptyElement;
				switch (_reader.LocalName)
				{
				case "FontFamily.FamilyNames":
					VerifyNoAttributes();
					_reader.Read();
					if (isEmptyElement)
					{
						break;
					}
					while (MoveToContent() == XmlNodeType.Element)
					{
						if (_reader.LocalName == "String" && IsSystemNamespace(_reader.NamespaceURI))
						{
							ParseFamilyNameElement();
						}
						else
						{
							FailUnknownElement();
						}
					}
					_reader.ReadEndElement();
					break;
				case "FontFamily.FamilyTypefaces":
					VerifyNoAttributes();
					_reader.Read();
					if (!isEmptyElement)
					{
						while (IsStartElement("FamilyTypeface", "http://schemas.microsoft.com/winfx/2006/xaml/composite-font"))
						{
							ParseFamilyTypefaceElement();
						}
						_reader.ReadEndElement();
					}
					break;
				case "FontFamily.FamilyMaps":
					VerifyNoAttributes();
					_reader.Read();
					if (!isEmptyElement)
					{
						while (IsStartElement("FontFamilyMap", "http://schemas.microsoft.com/winfx/2006/xaml/composite-font"))
						{
							ParseFamilyMapElement();
						}
						_reader.ReadEndElement();
					}
					break;
				default:
					FailUnknownElement();
					break;
				}
			}
			else
			{
				_reader.Skip();
			}
		}
		VerifyCompositeFontInfo();
		_reader.ReadEndElement();
	}

	private void VerifyNoAttributes()
	{
		if (!_reader.MoveToFirstAttribute())
		{
			return;
		}
		do
		{
			if (!IsIgnorableAttribute())
			{
				FailUnknownAttribute();
			}
		}
		while (_reader.MoveToNextAttribute());
		_reader.MoveToElement();
	}

	private void ParseFamilyNameElement()
	{
		XmlLanguage xmlLanguage = null;
		if (_reader.MoveToFirstAttribute())
		{
			do
			{
				if (_reader.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml" && _reader.LocalName == "Key")
				{
					xmlLanguage = GetAttributeAsXmlLanguage();
				}
				else if (!IsIgnorableAttribute())
				{
					FailUnknownAttribute();
				}
			}
			while (_reader.MoveToNextAttribute());
			_reader.MoveToElement();
		}
		if (xmlLanguage == null)
		{
			FailMissingAttribute("Language");
		}
		string value = _reader.ReadElementString();
		if (string.IsNullOrEmpty(value))
		{
			FailMissingAttribute("Name");
		}
		_compositeFontInfo.FamilyNames.Add(xmlLanguage, value);
	}

	private void ParseFamilyTypefaceElement()
	{
		FamilyTypeface familyTypeface = new FamilyTypeface();
		ParseFamilyTypefaceAttributes(familyTypeface);
		if (_reader.IsEmptyElement)
		{
			_reader.Read();
		}
		else
		{
			_reader.Read();
			while (MoveToContent() != XmlNodeType.EndElement)
			{
				if (_reader.NodeType == XmlNodeType.Element && _reader.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml/composite-font")
				{
					if (_reader.LocalName == "FamilyTypeface.DeviceFontCharacterMetrics")
					{
						VerifyNoAttributes();
						if (_reader.IsEmptyElement)
						{
							_reader.Read();
							continue;
						}
						_reader.Read();
						while (MoveToContent() == XmlNodeType.Element)
						{
							if (_reader.LocalName == "CharacterMetrics")
							{
								ParseCharacterMetricsElement(familyTypeface);
							}
							else
							{
								FailUnknownElement();
							}
						}
						_reader.ReadEndElement();
					}
					else
					{
						FailUnknownElement();
					}
				}
				else
				{
					_reader.Skip();
				}
			}
			_reader.ReadEndElement();
		}
		_compositeFontInfo.GetFamilyTypefaceList().Add(familyTypeface);
	}

	private void ParseFamilyTypefaceAttributes(FamilyTypeface face)
	{
		if (!_reader.MoveToFirstAttribute())
		{
			return;
		}
		do
		{
			if (IsCompositeFontAttribute())
			{
				switch (_reader.LocalName)
				{
				case "Style":
				{
					FontStyle fontStyle = default(FontStyle);
					if (!FontStyles.FontStyleStringToKnownStyle(GetAttributeValue(), CultureInfo.InvariantCulture, ref fontStyle))
					{
						FailAttributeValue();
					}
					face.Style = fontStyle;
					break;
				}
				case "Weight":
				{
					FontWeight fontWeight = default(FontWeight);
					if (!FontWeights.FontWeightStringToKnownWeight(GetAttributeValue(), CultureInfo.InvariantCulture, ref fontWeight))
					{
						FailAttributeValue();
					}
					face.Weight = fontWeight;
					break;
				}
				case "Stretch":
				{
					FontStretch fontStretch = default(FontStretch);
					if (!FontStretches.FontStretchStringToKnownStretch(GetAttributeValue(), CultureInfo.InvariantCulture, ref fontStretch))
					{
						FailAttributeValue();
					}
					face.Stretch = fontStretch;
					break;
				}
				case "UnderlinePosition":
					face.UnderlinePosition = GetAttributeAsDouble();
					break;
				case "UnderlineThickness":
					face.UnderlineThickness = GetAttributeAsDouble();
					break;
				case "StrikethroughPosition":
					face.StrikethroughPosition = GetAttributeAsDouble();
					break;
				case "StrikethroughThickness":
					face.StrikethroughThickness = GetAttributeAsDouble();
					break;
				case "CapsHeight":
					face.CapsHeight = GetAttributeAsDouble();
					break;
				case "XHeight":
					face.XHeight = GetAttributeAsDouble();
					break;
				case "DeviceFontName":
					face.DeviceFontName = GetAttributeValue();
					break;
				default:
					FailUnknownAttribute();
					break;
				}
			}
			else if (!IsIgnorableAttribute())
			{
				FailUnknownAttribute();
			}
		}
		while (_reader.MoveToNextAttribute());
		_reader.MoveToElement();
	}

	private void ParseCharacterMetricsElement(FamilyTypeface face)
	{
		string text = null;
		string text2 = null;
		if (_reader.MoveToFirstAttribute())
		{
			do
			{
				if (_reader.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml" && _reader.LocalName == "Key")
				{
					text = GetAttributeValue();
				}
				else if (IsCompositeFontAttribute() && _reader.LocalName == "Metrics")
				{
					text2 = GetAttributeValue();
				}
				else if (!IsIgnorableAttribute())
				{
					FailUnknownAttribute();
				}
			}
			while (_reader.MoveToNextAttribute());
			_reader.MoveToElement();
		}
		if (text == null)
		{
			FailMissingAttribute("Key");
		}
		if (text2 == null)
		{
			FailMissingAttribute("Metrics");
		}
		face.DeviceFontCharacterMetrics.Add(CharacterMetricsDictionary.ConvertKey(text), new CharacterMetrics(text2));
		ParseEmptyElement();
	}

	private void ParseFamilyMapElement()
	{
		FontFamilyMap fontFamilyMap = new FontFamilyMap();
		if (_reader.MoveToFirstAttribute())
		{
			do
			{
				if (IsCompositeFontAttribute())
				{
					switch (_reader.LocalName)
					{
					case "Unicode":
						fontFamilyMap.Unicode = GetAttributeValue();
						break;
					case "Target":
						fontFamilyMap.Target = GetAttributeValue();
						break;
					case "Scale":
						fontFamilyMap.Scale = GetAttributeAsDouble();
						break;
					case "Language":
						fontFamilyMap.Language = GetAttributeAsXmlLanguage();
						break;
					default:
						FailUnknownAttribute();
						break;
					}
				}
				else if (!IsIgnorableAttribute())
				{
					FailUnknownAttribute();
				}
			}
			while (_reader.MoveToNextAttribute());
			_reader.MoveToElement();
		}
		_compositeFontInfo.FamilyMaps.Add(fontFamilyMap);
		ParseEmptyElement();
	}

	private void ParseEmptyElement()
	{
		if (_reader.IsEmptyElement)
		{
			_reader.Read();
			return;
		}
		_reader.Read();
		while (MoveToContent() != XmlNodeType.EndElement)
		{
			if (_reader.NodeType == XmlNodeType.Element && _reader.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml/composite-font")
			{
				FailUnknownElement();
			}
			else
			{
				_reader.Skip();
			}
		}
		_reader.ReadEndElement();
	}

	private bool IsCompositeFontAttribute()
	{
		string namespaceURI = _reader.NamespaceURI;
		if (!string.IsNullOrEmpty(namespaceURI))
		{
			return namespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml/composite-font";
		}
		return true;
	}

	private bool IsIgnorableAttribute()
	{
		string namespaceURI = _reader.NamespaceURI;
		if (!(namespaceURI == "http://www.w3.org/XML/1998/namespace"))
		{
			return namespaceURI == "http://www.w3.org/2000/xmlns/";
		}
		return true;
	}

	private void VerifyCompositeFontInfo()
	{
		if (_compositeFontInfo.FamilyMaps.Count == 0)
		{
			Fail(SR.Format(SR.CompositeFontMissingElement, "FontFamilyMap"));
		}
		if (_compositeFontInfo.FamilyNames.Count == 0)
		{
			Fail(SR.Format(SR.CompositeFontMissingElement, "String"));
		}
	}

	private void FailNotWellFormed(Exception x)
	{
		throw new FileFormatException(new Uri(_reader.BaseURI, UriKind.RelativeOrAbsolute), x);
	}

	private void FailAttributeValue()
	{
		Fail(SR.Format(SR.CompositeFontAttributeValue1, _reader.LocalName));
	}

	private void FailAttributeValue(Exception x)
	{
		Fail(SR.Format(SR.CompositeFontAttributeValue2, _reader.LocalName, x.Message), x);
	}

	private void FailUnknownElement()
	{
		Fail(SR.Format(SR.CompositeFontUnknownElement, _reader.LocalName, _reader.NamespaceURI));
	}

	private void FailUnknownAttribute()
	{
		Fail(SR.Format(SR.CompositeFontUnknownAttribute, _reader.LocalName, _reader.NamespaceURI));
	}

	private void FailMissingAttribute(string name)
	{
		Fail(SR.Format(SR.CompositeFontMissingAttribute, name));
	}

	private void Fail(string message)
	{
		Fail(message, null);
	}

	private void Fail(string message, Exception innerException)
	{
		throw new FileFormatException(new Uri(_reader.BaseURI, UriKind.RelativeOrAbsolute), message, innerException);
	}
}
