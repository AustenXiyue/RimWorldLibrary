using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using System.Xml.Schema;
using MS.Internal;

namespace System.Windows.Documents;

internal class XpsS0Schema : XpsSchema
{
	protected static ContentType _fontContentType = new ContentType("application/vnd.ms-opentype");

	protected static ContentType _colorContextContentType = new ContentType("application/vnd.ms-color.iccprofile");

	protected static ContentType _obfuscatedContentType = new ContentType("application/vnd.ms-package.obfuscated-opentype");

	protected static ContentType _jpgContentType = new ContentType("image/jpeg");

	protected static ContentType _pngContentType = new ContentType("image/png");

	protected static ContentType _tifContentType = new ContentType("image/tiff");

	protected static ContentType _wmpContentType = new ContentType("image/vnd.ms-photo");

	protected static ContentType _fixedDocumentSequenceContentType = new ContentType("application/vnd.ms-package.xps-fixeddocumentsequence+xml");

	protected static ContentType _fixedDocumentContentType = new ContentType("application/vnd.ms-package.xps-fixeddocument+xml");

	protected static ContentType _fixedPageContentType = new ContentType("application/vnd.ms-package.xps-fixedpage+xml");

	protected static ContentType _resourceDictionaryContentType = new ContentType("application/vnd.ms-package.xps-resourcedictionary+xml");

	protected static ContentType _printTicketContentType = new ContentType("application/vnd.ms-printing.printticket+xml");

	protected static ContentType _discardControlContentType = new ContentType("application/vnd.ms-package.xps-discard-control+xml");

	private const string _xpsS0SchemaNamespace = "http://schemas.microsoft.com/xps/2005/06";

	private const string _contextColor = "ContextColor ";

	private const string _colorConvertedBitmap = "{ColorConvertedBitmap ";

	private static XmlReaderSettings _xmlReaderSettings;

	public override string RootNamespaceUri => "http://schemas.microsoft.com/xps/2005/06";

	private static byte[] S0SchemaBytes => (byte[])new ResourceManager("Schemas_S0", Assembly.GetAssembly(typeof(XpsS0Schema))).GetObject("s0schema.xsd");

	private static byte[] DictionarySchemaBytes => (byte[])new ResourceManager("Schemas_S0", Assembly.GetAssembly(typeof(XpsS0Schema))).GetObject("rdkey.xsd");

	protected XpsS0Schema()
	{
	}

	public override XmlReaderSettings GetXmlReaderSettings()
	{
		if (_xmlReaderSettings == null)
		{
			_xmlReaderSettings = new XmlReaderSettings();
			_xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints;
			MemoryStream input = new MemoryStream(S0SchemaBytes);
			MemoryStream input2 = new MemoryStream(DictionarySchemaBytes);
			XmlResolver xmlResolver = new XmlUrlResolver();
			_xmlReaderSettings.ValidationType = ValidationType.Schema;
			_xmlReaderSettings.Schemas.XmlResolver = xmlResolver;
			_xmlReaderSettings.Schemas.Add("http://schemas.microsoft.com/xps/2005/06", new XmlTextReader(input));
			_xmlReaderSettings.Schemas.Add(null, new XmlTextReader(input2));
		}
		return _xmlReaderSettings;
	}

	public override bool HasRequiredResources(ContentType mimeType)
	{
		if (_fixedPageContentType.AreTypeAndSubTypeEqual(mimeType))
		{
			return true;
		}
		return false;
	}

	public override bool HasUriAttributes(ContentType mimeType)
	{
		return true;
	}

	public override bool AllowsMultipleReferencesToSameUri(ContentType mimeType)
	{
		if (_fixedDocumentSequenceContentType.AreTypeAndSubTypeEqual(mimeType) || _fixedDocumentContentType.AreTypeAndSubTypeEqual(mimeType))
		{
			return false;
		}
		return true;
	}

	public override bool IsValidRootNamespaceUri(string namespaceUri)
	{
		return namespaceUri.Equals("http://schemas.microsoft.com/xps/2005/06", StringComparison.Ordinal);
	}

	public override string[] ExtractUriFromAttr(string attrName, string attrValue)
	{
		if (attrName.Equals("Source", StringComparison.Ordinal) || attrName.Equals("FontUri", StringComparison.Ordinal))
		{
			return new string[1] { attrValue };
		}
		if (attrName.Equals("ImageSource", StringComparison.Ordinal))
		{
			if (attrValue.StartsWith("{ColorConvertedBitmap ", StringComparison.Ordinal))
			{
				attrValue = attrValue.Substring("{ColorConvertedBitmap ".Length);
				return attrValue.Split(' ', '}');
			}
			return new string[1] { attrValue };
		}
		if (attrName.Equals("Color", StringComparison.Ordinal) || attrName.Equals("Fill", StringComparison.Ordinal) || attrName.Equals("Stroke", StringComparison.Ordinal))
		{
			ReadOnlySpan<char> span = attrValue.AsSpan().Trim();
			if (span.StartsWith("ContextColor ", StringComparison.Ordinal))
			{
				span = span.Slice("ContextColor ".Length).Trim();
				int num = span.IndexOf(' ');
				if (num >= 0)
				{
					return new string[1] { span.Slice(0, num).ToString() };
				}
			}
		}
		return null;
	}
}
