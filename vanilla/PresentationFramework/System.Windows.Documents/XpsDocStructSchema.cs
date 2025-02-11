using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using System.Xml.Schema;
using MS.Internal;

namespace System.Windows.Documents;

internal sealed class XpsDocStructSchema : XpsSchema
{
	private static ContentType _documentStructureContentType = new ContentType("application/vnd.ms-package.xps-documentstructure+xml");

	private static ContentType _storyFragmentsContentType = new ContentType("application/vnd.ms-package.xps-storyfragments+xml");

	private const string _xpsDocStructureSchemaNamespace = "http://schemas.microsoft.com/xps/2005/06/documentstructure";

	private static XmlReaderSettings _xmlReaderSettings;

	public override string RootNamespaceUri => "http://schemas.microsoft.com/xps/2005/06/documentstructure";

	private static byte[] SchemaBytes => (byte[])new ResourceManager("Schemas_DocStructure", Assembly.GetAssembly(typeof(XpsDocStructSchema))).GetObject("DocStructure.xsd");

	public XpsDocStructSchema()
	{
		XpsSchema.RegisterSchema(this, new ContentType[2] { _documentStructureContentType, _storyFragmentsContentType });
	}

	public override XmlReaderSettings GetXmlReaderSettings()
	{
		if (_xmlReaderSettings == null)
		{
			_xmlReaderSettings = new XmlReaderSettings();
			_xmlReaderSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints;
			MemoryStream input = new MemoryStream(SchemaBytes);
			XmlResolver xmlResolver = new XmlUrlResolver();
			_xmlReaderSettings.ValidationType = ValidationType.Schema;
			_xmlReaderSettings.Schemas.XmlResolver = xmlResolver;
			_xmlReaderSettings.Schemas.Add("http://schemas.microsoft.com/xps/2005/06/documentstructure", new XmlTextReader(input));
		}
		return _xmlReaderSettings;
	}

	public override bool IsValidRootNamespaceUri(string namespaceUri)
	{
		return namespaceUri.Equals("http://schemas.microsoft.com/xps/2005/06/documentstructure", StringComparison.Ordinal);
	}
}
