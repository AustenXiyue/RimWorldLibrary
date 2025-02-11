using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace System.Security.Cryptography.Xml;

/// <summary>Represents the XPath transform for a digital signature as defined by the W3C.</summary>
public class XmlDsigXPathTransform : Transform
{
	private Type[] _inputTypes = new Type[3]
	{
		typeof(Stream),
		typeof(XmlNodeList),
		typeof(XmlDocument)
	};

	private Type[] _outputTypes = new Type[1] { typeof(XmlNodeList) };

	private string _xpathexpr;

	private XmlDocument _document;

	private XmlNamespaceManager _nsm;

	/// <summary>Gets an array of types that are valid inputs to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigXPathTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</summary>
	/// <returns>An array of valid input types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object; you can pass only objects of one of these types to the <see cref="M:System.Security.Cryptography.Xml.XmlDsigXPathTransform.LoadInput(System.Object)" /> method of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</returns>
	public override Type[] InputTypes => _inputTypes;

	/// <summary>Gets an array of types that are possible outputs from the <see cref="M:System.Security.Cryptography.Xml.XmlDsigXPathTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</summary>
	/// <returns>An array of valid output types for the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object; the <see cref="M:System.Security.Cryptography.Xml.XmlDsigXPathTransform.GetOutput" /> methods of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object return only objects of one of these types.</returns>
	public override Type[] OutputTypes => _outputTypes;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> class.</summary>
	public XmlDsigXPathTransform()
	{
		base.Algorithm = "http://www.w3.org/TR/1999/REC-xpath-19991116";
	}

	/// <summary>Parses the specified <see cref="T:System.Xml.XmlNodeList" /> object as transform-specific content of a &lt;Transform&gt; element and configures the internal state of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object to match the &lt;Transform&gt; element.</summary>
	/// <param name="nodeList">An <see cref="T:System.Xml.XmlNodeList" /> object to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object. </param>
	/// <exception cref="T:System.Security.Cryptography.CryptographicException">The <paramref name="nodeList" /> parameter is null.-or- The <paramref name="nodeList" /> parameter does not contain an <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> element. </exception>
	public override void LoadInnerXml(XmlNodeList nodeList)
	{
		if (nodeList == null)
		{
			throw new CryptographicException("Unknown transform has been encountered.");
		}
		foreach (XmlNode node in nodeList)
		{
			string text = null;
			string text2 = null;
			if (!(node is XmlElement { LocalName: "XPath" } xmlElement))
			{
				continue;
			}
			_xpathexpr = xmlElement.InnerXml.Trim(null);
			XmlNameTable nameTable = new XmlNodeReader(xmlElement).NameTable;
			_nsm = new XmlNamespaceManager(nameTable);
			foreach (XmlAttribute attribute in xmlElement.Attributes)
			{
				if (attribute.Prefix == "xmlns")
				{
					text = attribute.LocalName;
					text2 = attribute.Value;
					if (text == null)
					{
						text = xmlElement.Prefix;
						text2 = xmlElement.NamespaceURI;
					}
					_nsm.AddNamespace(text, text2);
				}
			}
			break;
		}
		if (_xpathexpr == null)
		{
			throw new CryptographicException("Unknown transform has been encountered.");
		}
	}

	/// <summary>Returns an XML representation of the parameters of a <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object that are suitable to be included as subelements of an XMLDSIG &lt;Transform&gt; element.</summary>
	/// <returns>A list of the XML nodes that represent the transform-specific content needed to describe the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object in an XMLDSIG &lt;Transform&gt; element.</returns>
	protected override XmlNodeList GetInnerXml()
	{
		XmlDocument xmlDocument = new XmlDocument();
		XmlElement xmlElement = xmlDocument.CreateElement(null, "XPath", "http://www.w3.org/2000/09/xmldsig#");
		if (_nsm != null)
		{
			foreach (string item in _nsm)
			{
				switch (item)
				{
				case "xml":
				case "xmlns":
				case null:
					continue;
				}
				if (item.Length > 0)
				{
					xmlElement.SetAttribute("xmlns:" + item, _nsm.LookupNamespace(item));
				}
			}
		}
		xmlElement.InnerXml = _xpathexpr;
		xmlDocument.AppendChild(xmlElement);
		return xmlDocument.ChildNodes;
	}

	/// <summary>Loads the specified input into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</summary>
	/// <param name="obj">The input to load into the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object. </param>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override void LoadInput(object obj)
	{
		if (obj is Stream)
		{
			LoadStreamInput((Stream)obj);
		}
		else if (obj is XmlNodeList)
		{
			LoadXmlNodeListInput((XmlNodeList)obj);
		}
		else if (obj is XmlDocument)
		{
			LoadXmlDocumentInput((XmlDocument)obj);
		}
	}

	private void LoadStreamInput(Stream stream)
	{
		XmlResolver xmlResolver = (base.ResolverSet ? _xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI));
		XmlReader reader = Utils.PreProcessStreamInput(stream, xmlResolver, base.BaseURI);
		_document = new XmlDocument();
		_document.PreserveWhitespace = true;
		_document.Load(reader);
	}

	private void LoadXmlNodeListInput(XmlNodeList nodeList)
	{
		XmlResolver resolver = (base.ResolverSet ? _xmlResolver : new XmlSecureResolver(new XmlUrlResolver(), base.BaseURI));
		using MemoryStream stream = new MemoryStream(new CanonicalXml(nodeList, resolver, includeComments: true).GetBytes());
		LoadStreamInput(stream);
	}

	private void LoadXmlDocumentInput(XmlDocument doc)
	{
		_document = doc;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public override object GetOutput()
	{
		CanonicalXmlNodeList canonicalXmlNodeList = new CanonicalXmlNodeList();
		if (!string.IsNullOrEmpty(_xpathexpr))
		{
			XPathNavigator xPathNavigator = _document.CreateNavigator();
			XPathNodeIterator xPathNodeIterator = xPathNavigator.Select("//. | //@*");
			XPathExpression xPathExpression = xPathNavigator.Compile("boolean(" + _xpathexpr + ")");
			xPathExpression.SetContext(_nsm);
			while (xPathNodeIterator.MoveNext())
			{
				XmlNode node = ((IHasXmlNode)xPathNodeIterator.Current).GetNode();
				if ((bool)xPathNodeIterator.Current.Evaluate(xPathExpression))
				{
					canonicalXmlNodeList.Add(node);
				}
			}
			xPathNodeIterator = xPathNavigator.Select("//namespace::*");
			while (xPathNodeIterator.MoveNext())
			{
				XmlNode node2 = ((IHasXmlNode)xPathNodeIterator.Current).GetNode();
				canonicalXmlNodeList.Add(node2);
			}
		}
		return canonicalXmlNodeList;
	}

	/// <summary>Returns the output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object of type <see cref="T:System.Xml.XmlNodeList" />.</summary>
	/// <returns>The output of the current <see cref="T:System.Security.Cryptography.Xml.XmlDsigXPathTransform" /> object of type <see cref="T:System.Xml.XmlNodeList" />.</returns>
	/// <param name="type">The type of the output to return. <see cref="T:System.Xml.XmlNodeList" /> is the only valid type for this parameter. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="type" /> parameter is not an <see cref="T:System.Xml.XmlNodeList" /> object.</exception>
	public override object GetOutput(Type type)
	{
		if (type != typeof(XmlNodeList) && !type.IsSubclassOf(typeof(XmlNodeList)))
		{
			throw new ArgumentException("The input type was invalid for this transform.", "type");
		}
		return (XmlNodeList)GetOutput();
	}
}
