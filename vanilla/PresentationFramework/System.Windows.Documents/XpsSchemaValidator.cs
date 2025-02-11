using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows.Markup;
using System.Xml;
using MS.Internal;

namespace System.Windows.Documents;

internal class XpsSchemaValidator
{
	private class XmlEncodingEnforcingTextReader : XmlTextReader
	{
		private bool _encodingChecked;

		public XmlEncodingEnforcingTextReader(Stream objectStream)
			: base(objectStream)
		{
		}

		public override bool Read()
		{
			bool num = base.Read();
			if (num && !_encodingChecked)
			{
				if (base.NodeType == XmlNodeType.XmlDeclaration)
				{
					string text = base["encoding"];
					if (text != null && !text.Equals(System.Text.Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase) && !text.Equals(System.Text.Encoding.UTF8.WebName, StringComparison.OrdinalIgnoreCase))
					{
						throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedEncoding);
					}
				}
				if (!(base.Encoding is UTF8Encoding) && !(base.Encoding is UnicodeEncoding))
				{
					throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedEncoding);
				}
				_encodingChecked = true;
			}
			return num;
		}
	}

	private class RootXMLNSAndUriValidatingXmlReader : XmlWrappingReader
	{
		private XpsValidatingLoader _loader;

		private XpsSchema _schema;

		private Uri _packageUri;

		private Uri _baseUri;

		private string _lastAttr;

		private int _node;

		private bool _rootXMLNSChecked;

		public override string Value
		{
			get
			{
				CheckUri(base.Reader.Value);
				return base.Reader.Value;
			}
		}

		public RootXMLNSAndUriValidatingXmlReader(XpsValidatingLoader loader, XpsSchema schema, XmlReader xmlReader, Uri packageUri, Uri baseUri)
			: base(xmlReader)
		{
			_loader = loader;
			_schema = schema;
			_packageUri = packageUri;
			_baseUri = baseUri;
		}

		public RootXMLNSAndUriValidatingXmlReader(XpsValidatingLoader loader, XpsSchema schema, XmlReader xmlReader)
			: base(xmlReader)
		{
			_loader = loader;
			_schema = schema;
		}

		private void CheckUri(string attr)
		{
			CheckUri(base.Reader.LocalName, attr);
		}

		private void CheckUri(string localName, string attr)
		{
			if ((object)attr == _lastAttr)
			{
				return;
			}
			_lastAttr = attr;
			string[] array = _schema.ExtractUriFromAttr(localName, attr);
			if (array == null)
			{
				return;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Length > 0)
				{
					Uri partUri = PackUriHelper.ResolvePartUri(_baseUri, new Uri(text, UriKind.Relative));
					Uri uri = PackUriHelper.Create(_packageUri, partUri);
					_loader.UriHitHandler(_node, uri);
				}
			}
		}

		public override string GetAttribute(string name)
		{
			string attribute = base.Reader.GetAttribute(name);
			CheckUri(name, attribute);
			return attribute;
		}

		public override string GetAttribute(string name, string namespaceURI)
		{
			string attribute = base.Reader.GetAttribute(name, namespaceURI);
			CheckUri(attribute);
			return attribute;
		}

		public override string GetAttribute(int i)
		{
			string attribute = base.Reader.GetAttribute(i);
			CheckUri(attribute);
			return attribute;
		}

		public override bool Read()
		{
			_node++;
			bool result = base.Reader.Read();
			if (base.Reader.NodeType == XmlNodeType.Element && !_rootXMLNSChecked)
			{
				if (!_schema.IsValidRootNamespaceUri(base.Reader.NamespaceURI))
				{
					throw new FileFormatException(SR.XpsValidatingLoaderUnsupportedRootNamespaceUri);
				}
				_rootXMLNSChecked = true;
			}
			return result;
		}
	}

	private XmlReader _compatReader;

	private static string[] _predefinedNamespaces = new string[1] { "http://schemas.microsoft.com/xps/2005/06/resourcedictionary-key" };

	public XmlReader XmlReader => _compatReader;

	public XpsSchemaValidator(XpsValidatingLoader loader, XpsSchema schema, ContentType mimeType, Stream objectStream, Uri packageUri, Uri baseUri)
	{
		XmlReader baseReader = new XmlEncodingEnforcingTextReader(objectStream)
		{
			ProhibitDtd = true,
			Normalization = true
		};
		string[] array = _predefinedNamespaces;
		if (!string.IsNullOrEmpty(schema.RootNamespaceUri))
		{
			array = new string[_predefinedNamespaces.Length + 1];
			array[0] = schema.RootNamespaceUri;
			_predefinedNamespaces.CopyTo(array, 1);
		}
		baseReader = new XmlCompatibilityReader(baseReader, array);
		baseReader = XmlReader.Create(baseReader, schema.GetXmlReaderSettings());
		_compatReader = ((!schema.HasUriAttributes(mimeType) || !(packageUri != null) || !(baseUri != null)) ? new RootXMLNSAndUriValidatingXmlReader(loader, schema, baseReader) : new RootXMLNSAndUriValidatingXmlReader(loader, schema, baseReader, packageUri, baseUri));
	}
}
