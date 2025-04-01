using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using MS.Internal.Xml.Linq.ComponentModel;

namespace System.Xml.Linq;

/// <summary>Represents an XML element.</summary>
[XmlSchemaProvider(null, IsAny = true)]
[TypeDescriptionProvider(typeof(XTypeDescriptionProvider<XElement>))]
public class XElement : XContainer, IXmlSerializable
{
	private static IEnumerable<XElement> emptySequence;

	internal XName name;

	internal XAttribute lastAttr;

	/// <summary>Gets an empty collection of elements.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contains an empty collection.</returns>
	public static IEnumerable<XElement> EmptySequence
	{
		get
		{
			if (emptySequence == null)
			{
				emptySequence = new XElement[0];
			}
			return emptySequence;
		}
	}

	/// <summary>Gets the first attribute of this element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XAttribute" /> that contains the first attribute of this element.</returns>
	/// <filterpriority>2</filterpriority>
	public XAttribute FirstAttribute
	{
		get
		{
			if (lastAttr == null)
			{
				return null;
			}
			return lastAttr.next;
		}
	}

	/// <summary>Gets a value indicating whether this element as at least one attribute.</summary>
	/// <returns>true if this element has at least one attribute; otherwise false.</returns>
	public bool HasAttributes => lastAttr != null;

	/// <summary>Gets a value indicating whether this element has at least one child element.</summary>
	/// <returns>true if this element has at least one child element; otherwise false.</returns>
	public bool HasElements
	{
		get
		{
			XNode xNode = content as XNode;
			if (xNode != null)
			{
				do
				{
					if (xNode is XElement)
					{
						return true;
					}
					xNode = xNode.next;
				}
				while (xNode != content);
			}
			return false;
		}
	}

	/// <summary>Gets a value indicating whether this element contains no content.</summary>
	/// <returns>true if this element contains no content; otherwise false.</returns>
	public bool IsEmpty => content == null;

	/// <summary>Gets the last attribute of this element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XAttribute" /> that contains the last attribute of this element.</returns>
	/// <filterpriority>2</filterpriority>
	public XAttribute LastAttribute => lastAttr;

	/// <summary>Gets or sets the name of this element.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XName" /> that contains the name of this element.</returns>
	public XName Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Name);
			name = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Name);
			}
		}
	}

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XElement" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.Element" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Element;

	/// <summary>Gets or sets the concatenated text contents of this element.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains all of the text content of this element. If there are multiple text nodes, they will be concatenated.</returns>
	public string Value
	{
		get
		{
			if (content == null)
			{
				return string.Empty;
			}
			if (content is string result)
			{
				return result;
			}
			StringBuilder stringBuilder = new StringBuilder();
			AppendText(stringBuilder);
			return stringBuilder.ToString();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			RemoveNodes();
			Add(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class with the specified name. </summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the name of the element.</param>
	public XElement(XName name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		this.name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class with the specified name and content.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the element name.</param>
	/// <param name="content">The contents of the element.</param>
	public XElement(XName name, object content)
		: this(name)
	{
		AddContentSkipNotify(content);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class with the specified name and content.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the element name.</param>
	/// <param name="content">The initial content of the element.</param>
	public XElement(XName name, params object[] content)
		: this(name, (object)content)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class from another <see cref="T:System.Xml.Linq.XElement" /> object.</summary>
	/// <param name="other">An <see cref="T:System.Xml.Linq.XElement" /> object to copy from.</param>
	public XElement(XElement other)
		: base(other)
	{
		name = other.name;
		XAttribute xAttribute = other.lastAttr;
		if (xAttribute != null)
		{
			do
			{
				xAttribute = xAttribute.next;
				AppendAttributeSkipNotify(new XAttribute(xAttribute));
			}
			while (xAttribute != other.lastAttr);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XElement" /> class from an <see cref="T:System.Xml.Linq.XStreamingElement" /> object.</summary>
	/// <param name="other">An <see cref="T:System.Xml.Linq.XStreamingElement" /> that contains unevaluated queries that will be iterated for the contents of this <see cref="T:System.Xml.Linq.XElement" />.</param>
	public XElement(XStreamingElement other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		name = other.name;
		AddContentSkipNotify(other.content);
	}

	internal XElement()
		: this("default")
	{
	}

	internal XElement(XmlReader r)
		: this(r, LoadOptions.None)
	{
	}

	internal XElement(XmlReader r, LoadOptions o)
	{
		ReadElementFrom(r, o);
	}

	private static object ConvertForAssignment(object value)
	{
		if (!(value is XmlNode node))
		{
			return value;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.ImportNode(node, deep: true));
		return Parse(xmlDocument.InnerXml);
	}

	/// <summary>Returns a collection of elements that contain this element, and the ancestors of this element. </summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of elements that contain this element, and the ancestors of this element. </returns>
	public IEnumerable<XElement> AncestorsAndSelf()
	{
		return GetAncestors(null, self: true);
	}

	/// <summary>Returns a filtered collection of elements that contain this element, and the ancestors of this element. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contain this element, and the ancestors of this element. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> AncestorsAndSelf(XName name)
	{
		if (!(name != null))
		{
			return EmptySequence;
		}
		return GetAncestors(name, self: true);
	}

	/// <summary>Returns the <see cref="T:System.Xml.Linq.XAttribute" /> of this <see cref="T:System.Xml.Linq.XElement" /> that has the specified <see cref="T:System.Xml.Linq.XName" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XAttribute" /> that has the specified <see cref="T:System.Xml.Linq.XName" />; null if there is no attribute with the specified name.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> of the <see cref="T:System.Xml.Linq.XAttribute" /> to get.</param>
	public XAttribute Attribute(XName name)
	{
		XAttribute xAttribute = lastAttr;
		if (xAttribute != null)
		{
			do
			{
				xAttribute = xAttribute.next;
				if (xAttribute.name == name)
				{
					return xAttribute;
				}
			}
			while (xAttribute != lastAttr);
		}
		return null;
	}

	/// <summary>Returns a collection of attributes of this element.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> of attributes of this element.</returns>
	public IEnumerable<XAttribute> Attributes()
	{
		return GetAttributes(null);
	}

	/// <summary>Returns a filtered collection of attributes of this element. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XAttribute" /> that contains the attributes of this element. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XAttribute> Attributes(XName name)
	{
		if (!(name != null))
		{
			return XAttribute.EmptySequence;
		}
		return GetAttributes(name);
	}

	/// <summary>Returns a collection of nodes that contain this element, and all descendant nodes of this element, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> that contain this element, and all descendant nodes of this element, in document order.</returns>
	public IEnumerable<XNode> DescendantNodesAndSelf()
	{
		return GetDescendantNodes(self: true);
	}

	/// <summary>Returns a collection of elements that contain this element, and all descendant elements of this element, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of elements that contain this element, and all descendant elements of this element, in document order.</returns>
	public IEnumerable<XElement> DescendantsAndSelf()
	{
		return GetDescendants(null, self: true);
	}

	/// <summary>Returns a filtered collection of elements that contain this element, and all descendant elements of this element, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> that contain this element, and all descendant elements of this element, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> DescendantsAndSelf(XName name)
	{
		if (!(name != null))
		{
			return EmptySequence;
		}
		return GetDescendants(name, self: true);
	}

	/// <summary>Gets the default <see cref="T:System.Xml.Linq.XNamespace" /> of this <see cref="T:System.Xml.Linq.XElement" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNamespace" /> that contains the default namespace of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <filterpriority>2</filterpriority>
	public XNamespace GetDefaultNamespace()
	{
		string namespaceOfPrefixInScope = GetNamespaceOfPrefixInScope("xmlns", null);
		if (namespaceOfPrefixInScope == null)
		{
			return XNamespace.None;
		}
		return XNamespace.Get(namespaceOfPrefixInScope);
	}

	/// <summary>Gets the namespace associated with a particular prefix for this <see cref="T:System.Xml.Linq.XElement" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNamespace" /> for the namespace associated with the prefix for this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="prefix">A string that contains the namespace prefix to look up.</param>
	/// <filterpriority>2</filterpriority>
	public XNamespace GetNamespaceOfPrefix(string prefix)
	{
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		if (prefix.Length == 0)
		{
			throw new ArgumentException(Res.GetString("Argument_InvalidPrefix", prefix));
		}
		if (prefix == "xmlns")
		{
			return XNamespace.Xmlns;
		}
		string namespaceOfPrefixInScope = GetNamespaceOfPrefixInScope(prefix, null);
		if (namespaceOfPrefixInScope != null)
		{
			return XNamespace.Get(namespaceOfPrefixInScope);
		}
		if (prefix == "xml")
		{
			return XNamespace.Xml;
		}
		return null;
	}

	/// <summary>Gets the prefix associated with a namespace for this <see cref="T:System.Xml.Linq.XElement" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the namespace prefix.</returns>
	/// <param name="ns">An <see cref="T:System.Xml.Linq.XNamespace" /> to look up.</param>
	/// <filterpriority>2</filterpriority>
	public string GetPrefixOfNamespace(XNamespace ns)
	{
		if (ns == null)
		{
			throw new ArgumentNullException("ns");
		}
		string namespaceName = ns.NamespaceName;
		bool flag = false;
		XElement xElement = this;
		do
		{
			XAttribute xAttribute = xElement.lastAttr;
			if (xAttribute != null)
			{
				bool flag2 = false;
				do
				{
					xAttribute = xAttribute.next;
					if (xAttribute.IsNamespaceDeclaration)
					{
						if (xAttribute.Value == namespaceName && xAttribute.Name.NamespaceName.Length != 0 && (!flag || GetNamespaceOfPrefixInScope(xAttribute.Name.LocalName, xElement) == null))
						{
							return xAttribute.Name.LocalName;
						}
						flag2 = true;
					}
				}
				while (xAttribute != xElement.lastAttr);
				flag = flag || flag2;
			}
			xElement = xElement.parent as XElement;
		}
		while (xElement != null);
		if ((object)namespaceName == "http://www.w3.org/XML/1998/namespace")
		{
			if (!flag || GetNamespaceOfPrefixInScope("xml", null) == null)
			{
				return "xml";
			}
		}
		else if ((object)namespaceName == "http://www.w3.org/2000/xmlns/")
		{
			return "xmlns";
		}
		return null;
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from a file.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the contents of the specified file.</returns>
	/// <param name="uri">A URI string referencing the file to load into a new <see cref="T:System.Xml.Linq.XElement" />.</param>
	public static XElement Load(string uri)
	{
		return Load(uri, LoadOptions.None);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from a file, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the contents of the specified file.</returns>
	/// <param name="uri">A URI string referencing the file to load into an <see cref="T:System.Xml.Linq.XElement" />.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XElement Load(string uri, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(uri, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XElement" /> instance by using the specified stream.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> object used to read the data that is contained in the stream.</returns>
	/// <param name="stream">The stream that contains the XML data.</param>
	public static XElement Load(Stream stream)
	{
		return Load(stream, LoadOptions.None);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XElement" /> instance by using the specified stream, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> object used to read the data that the stream contains.</returns>
	/// <param name="stream">The stream containing the XML data.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> object that specifies whether to load base URI and line information.</param>
	public static XElement Load(Stream stream, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from a <see cref="T:System.IO.TextReader" />. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the XML that was read from the specified <see cref="T:System.IO.TextReader" />.</returns>
	/// <param name="textReader">A <see cref="T:System.IO.TextReader" /> that will be read for the <see cref="T:System.Xml.Linq.XElement" /> content.</param>
	public static XElement Load(TextReader textReader)
	{
		return Load(textReader, LoadOptions.None);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from a <see cref="T:System.IO.TextReader" />, optionally preserving white space and retaining line information. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the XML that was read from the specified <see cref="T:System.IO.TextReader" />.</returns>
	/// <param name="textReader">A <see cref="T:System.IO.TextReader" /> that will be read for the <see cref="T:System.Xml.Linq.XElement" /> content.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XElement Load(TextReader textReader, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(textReader, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from an <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the XML that was read from the specified <see cref="T:System.Xml.XmlReader" />.</returns>
	/// <param name="reader">A <see cref="T:System.Xml.XmlReader" /> that will be read for the content of the <see cref="T:System.Xml.Linq.XElement" />.</param>
	public static XElement Load(XmlReader reader)
	{
		return Load(reader, LoadOptions.None);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from an <see cref="T:System.Xml.XmlReader" />, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> that contains the XML that was read from the specified <see cref="T:System.Xml.XmlReader" />.</returns>
	/// <param name="reader">A <see cref="T:System.Xml.XmlReader" /> that will be read for the content of the <see cref="T:System.Xml.Linq.XElement" />.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XElement Load(XmlReader reader, LoadOptions options)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.MoveToContent() != XmlNodeType.Element)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedNodeType", XmlNodeType.Element, reader.NodeType));
		}
		XElement result = new XElement(reader, options);
		reader.MoveToContent();
		if (!reader.EOF)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedEndOfFile"));
		}
		return result;
	}

	/// <summary>Load an <see cref="T:System.Xml.Linq.XElement" /> from a string that contains XML.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> populated from the string that contains XML.</returns>
	/// <param name="text">A <see cref="T:System.String" /> that contains XML.</param>
	public static XElement Parse(string text)
	{
		return Parse(text, LoadOptions.None);
	}

	/// <summary>Load an <see cref="T:System.Xml.Linq.XElement" /> from a string that contains XML, optionally preserving white space and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XElement" /> populated from the string that contains XML.</returns>
	/// <param name="text">A <see cref="T:System.String" /> that contains XML.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XElement Parse(string text, LoadOptions options)
	{
		using StringReader input = new StringReader(text);
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Removes nodes and attributes from this <see cref="T:System.Xml.Linq.XElement" />.</summary>
	public void RemoveAll()
	{
		RemoveAttributes();
		RemoveNodes();
	}

	/// <summary>Removes the attributes of this <see cref="T:System.Xml.Linq.XElement" />.</summary>
	public void RemoveAttributes()
	{
		if (SkipNotify())
		{
			RemoveAttributesSkipNotify();
			return;
		}
		while (lastAttr != null)
		{
			XAttribute xAttribute = lastAttr.next;
			NotifyChanging(xAttribute, XObjectChangeEventArgs.Remove);
			if (lastAttr == null || xAttribute != lastAttr.next)
			{
				throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
			}
			if (xAttribute != lastAttr)
			{
				lastAttr.next = xAttribute.next;
			}
			else
			{
				lastAttr = null;
			}
			xAttribute.parent = null;
			xAttribute.next = null;
			NotifyChanged(xAttribute, XObjectChangeEventArgs.Remove);
		}
	}

	/// <summary>Replaces the child nodes and the attributes of this element with the specified content.</summary>
	/// <param name="content">The content that will replace the child nodes and attributes of this element.</param>
	public void ReplaceAll(object content)
	{
		content = XContainer.GetContentSnapshot(content);
		RemoveAll();
		Add(content);
	}

	/// <summary>Replaces the child nodes and the attributes of this element with the specified content.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	public void ReplaceAll(params object[] content)
	{
		ReplaceAll((object)content);
	}

	/// <summary>Replaces the attributes of this element with the specified content.</summary>
	/// <param name="content">The content that will replace the attributes of this element.</param>
	public void ReplaceAttributes(object content)
	{
		content = XContainer.GetContentSnapshot(content);
		RemoveAttributes();
		Add(content);
	}

	/// <summary>Replaces the attributes of this element with the specified content.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	public void ReplaceAttributes(params object[] content)
	{
		ReplaceAttributes((object)content);
	}

	/// <summary>Serialize this element to a file.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the name of the file.</param>
	public void Save(string fileName)
	{
		Save(fileName, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Serialize this element to a file, optionally disabling formatting.</summary>
	/// <param name="fileName">A <see cref="T:System.String" /> that contains the name of the file.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(string fileName, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XElement" /> to the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XElement" /> to.</param>
	public void Save(Stream stream)
	{
		Save(stream, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XElement" /> to the specified <see cref="T:System.IO.Stream" />, optionally specifying formatting behavior.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XElement" /> to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> object that specifies formatting behavior.</param>
	public void Save(Stream stream, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this element to a <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="textWriter">A <see cref="T:System.IO.TextWriter" /> that the <see cref="T:System.Xml.Linq.XElement" /> will be written to.</param>
	public void Save(TextWriter textWriter)
	{
		Save(textWriter, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Serialize this element to a <see cref="T:System.IO.TextWriter" />, optionally disabling formatting.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> to output the XML to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(TextWriter textWriter, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this element to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">A <see cref="T:System.Xml.XmlWriter" /> that the <see cref="T:System.Xml.Linq.XElement" /> will be written to.</param>
	public void Save(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteStartDocument();
		WriteTo(writer);
		writer.WriteEndDocument();
	}

	/// <summary>Sets the value of an attribute, adds an attribute, or removes an attribute. </summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the name of the attribute to change.</param>
	/// <param name="value">The value to assign to the attribute. The attribute is removed if the value is null. Otherwise, the value is converted to its string representation and assigned to the <see cref="P:System.Xml.Linq.XAttribute.Value" /> property of the attribute.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> is an instance of <see cref="T:System.Xml.Linq.XObject" />.</exception>
	public void SetAttributeValue(XName name, object value)
	{
		XAttribute xAttribute = Attribute(name);
		if (value == null)
		{
			if (xAttribute != null)
			{
				RemoveAttribute(xAttribute);
			}
		}
		else if (xAttribute != null)
		{
			xAttribute.Value = XContainer.GetStringValue(value);
		}
		else
		{
			AppendAttribute(new XAttribute(name, value));
		}
	}

	/// <summary>Sets the value of a child element, adds a child element, or removes a child element.</summary>
	/// <param name="name">An <see cref="T:System.Xml.Linq.XName" /> that contains the name of the child element to change.</param>
	/// <param name="value">The value to assign to the child element. The child element is removed if the value is null. Otherwise, the value is converted to its string representation and assigned to the <see cref="P:System.Xml.Linq.XElement.Value" /> property of the child element.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> is an instance of <see cref="T:System.Xml.Linq.XObject" />.</exception>
	public void SetElementValue(XName name, object value)
	{
		XElement xElement = Element(name);
		if (value == null)
		{
			if (xElement != null)
			{
				RemoveNode(xElement);
			}
		}
		else if (xElement != null)
		{
			xElement.Value = XContainer.GetStringValue(value);
		}
		else
		{
			AddNode(new XElement(name, XContainer.GetStringValue(value)));
		}
	}

	/// <summary>Sets the value of this element.</summary>
	/// <param name="value">The value to assign to this element. The value is converted to its string representation and assigned to the <see cref="P:System.Xml.Linq.XElement.Value" /> property.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> is an <see cref="T:System.Xml.Linq.XObject" />.</exception>
	public void SetValue(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		Value = XContainer.GetStringValue(value);
	}

	/// <summary>Write this element to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		new ElementWriter(writer).WriteElement(this);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.String" />.</param>
	[CLSCompliant(false)]
	public static explicit operator string(XElement element)
	{
		return element?.Value;
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Boolean" />.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Boolean" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Boolean" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator bool(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToBoolean(element.Value.ToLower(CultureInfo.InvariantCulture));
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Boolean" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator bool?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToBoolean(element.Value.ToLower(CultureInfo.InvariantCulture));
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to an <see cref="T:System.Int32" />.</summary>
	/// <returns>A <see cref="T:System.Int32" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Int32" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Int32" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator int(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToInt32(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Int32" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator int?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToInt32(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.UInt32" />.</summary>
	/// <returns>A <see cref="T:System.UInt32" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.UInt32" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.UInt32" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator uint(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToUInt32(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.UInt32" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator uint?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToUInt32(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to an <see cref="T:System.Int64" />.</summary>
	/// <returns>A <see cref="T:System.Int64" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Int64" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Int64" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator long(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToInt64(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Int64" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator long?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToInt64(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.UInt64" />.</summary>
	/// <returns>A <see cref="T:System.UInt64" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.UInt64" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.UInt64" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator ulong(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToUInt64(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.UInt64" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator ulong?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToUInt64(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Single" />.</summary>
	/// <returns>A <see cref="T:System.Single" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Single" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Single" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator float(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToSingle(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Single" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator float?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToSingle(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Double" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Double" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator double(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToDouble(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Double" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator double?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToDouble(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Decimal" />.</summary>
	/// <returns>A <see cref="T:System.Decimal" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Decimal" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Decimal" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator decimal(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToDecimal(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Decimal" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator decimal?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToDecimal(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.DateTime" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.DateTime" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTime(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return DateTime.Parse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.DateTime" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTime?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return DateTime.Parse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XAttribute" /> to a <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>A <see cref="T:System.DateTimeOffset" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.DateTimeOffset" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.DateTimeOffset" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTimeOffset(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToDateTimeOffset(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to an <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.DateTimeOffset" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator DateTimeOffset?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToDateTimeOffset(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.TimeSpan" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.TimeSpan" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator TimeSpan(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToTimeSpan(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.TimeSpan" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator TimeSpan?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToTimeSpan(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Guid" />.</summary>
	/// <returns>A <see cref="T:System.Guid" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Guid" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Guid" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="element" /> parameter is null.</exception>
	[CLSCompliant(false)]
	public static explicit operator Guid(XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return XmlConvert.ToGuid(element.Value);
	}

	/// <summary>Cast the value of this <see cref="T:System.Xml.Linq.XElement" /> to a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" />.</summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> that contains the content of this <see cref="T:System.Xml.Linq.XElement" />.</returns>
	/// <param name="element">The <see cref="T:System.Xml.Linq.XElement" /> to cast to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" />.</param>
	/// <exception cref="T:System.FormatException">The element does not contain a valid <see cref="T:System.Guid" /> value.</exception>
	[CLSCompliant(false)]
	public static explicit operator Guid?(XElement element)
	{
		if (element == null)
		{
			return null;
		}
		return XmlConvert.ToGuid(element.Value);
	}

	/// <summary>Gets an XML schema definition that describes the XML representation of this object.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.</returns>
	XmlSchema IXmlSerializable.GetSchema()
	{
		return null;
	}

	/// <summary>Generates an object from its XML representation.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> from which the object is deserialized.</param>
	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (parent != null || annotations != null || content != null || lastAttr != null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_DeserializeInstance"));
		}
		if (reader.MoveToContent() != XmlNodeType.Element)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedNodeType", XmlNodeType.Element, reader.NodeType));
		}
		ReadElementFrom(reader, LoadOptions.None);
	}

	/// <summary>Converts an object into its XML representation.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> to which this object is serialized.</param>
	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		WriteTo(writer);
	}

	internal override void AddAttribute(XAttribute a)
	{
		if (Attribute(a.Name) != null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_DuplicateAttribute"));
		}
		if (a.parent != null)
		{
			a = new XAttribute(a);
		}
		AppendAttribute(a);
	}

	internal override void AddAttributeSkipNotify(XAttribute a)
	{
		if (Attribute(a.Name) != null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_DuplicateAttribute"));
		}
		if (a.parent != null)
		{
			a = new XAttribute(a);
		}
		AppendAttributeSkipNotify(a);
	}

	internal void AppendAttribute(XAttribute a)
	{
		bool num = NotifyChanging(a, XObjectChangeEventArgs.Add);
		if (a.parent != null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
		}
		AppendAttributeSkipNotify(a);
		if (num)
		{
			NotifyChanged(a, XObjectChangeEventArgs.Add);
		}
	}

	internal void AppendAttributeSkipNotify(XAttribute a)
	{
		a.parent = this;
		if (lastAttr == null)
		{
			a.next = a;
		}
		else
		{
			a.next = lastAttr.next;
			lastAttr.next = a;
		}
		lastAttr = a;
	}

	private bool AttributesEqual(XElement e)
	{
		XAttribute xAttribute = lastAttr;
		XAttribute xAttribute2 = e.lastAttr;
		if (xAttribute != null && xAttribute2 != null)
		{
			do
			{
				xAttribute = xAttribute.next;
				xAttribute2 = xAttribute2.next;
				if (xAttribute.name != xAttribute2.name || xAttribute.value != xAttribute2.value)
				{
					return false;
				}
			}
			while (xAttribute != lastAttr);
			return xAttribute2 == e.lastAttr;
		}
		if (xAttribute == null)
		{
			return xAttribute2 == null;
		}
		return false;
	}

	internal override XNode CloneNode()
	{
		return new XElement(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XElement xElement && name == xElement.name && ContentsEqual(xElement))
		{
			return AttributesEqual(xElement);
		}
		return false;
	}

	private IEnumerable<XAttribute> GetAttributes(XName name)
	{
		XAttribute a = lastAttr;
		if (a == null)
		{
			yield break;
		}
		do
		{
			a = a.next;
			if (name == null || a.name == name)
			{
				yield return a;
			}
		}
		while (a.parent == this && a != lastAttr);
	}

	private string GetNamespaceOfPrefixInScope(string prefix, XElement outOfScope)
	{
		for (XElement xElement = this; xElement != outOfScope; xElement = xElement.parent as XElement)
		{
			XAttribute xAttribute = xElement.lastAttr;
			if (xAttribute != null)
			{
				do
				{
					xAttribute = xAttribute.next;
					if (xAttribute.IsNamespaceDeclaration && xAttribute.Name.LocalName == prefix)
					{
						return xAttribute.Value;
					}
				}
				while (xAttribute != xElement.lastAttr);
			}
		}
		return null;
	}

	internal override int GetDeepHashCode()
	{
		int hashCode = name.GetHashCode();
		hashCode ^= ContentsHashCode();
		XAttribute xAttribute = lastAttr;
		if (xAttribute != null)
		{
			do
			{
				xAttribute = xAttribute.next;
				hashCode ^= xAttribute.GetDeepHashCode();
			}
			while (xAttribute != lastAttr);
		}
		return hashCode;
	}

	private void ReadElementFrom(XmlReader r, LoadOptions o)
	{
		if (r.ReadState != ReadState.Interactive)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedInteractive"));
		}
		name = XNamespace.Get(r.NamespaceURI).GetName(r.LocalName);
		if ((o & LoadOptions.SetBaseUri) != 0)
		{
			string baseURI = r.BaseURI;
			if (baseURI != null && baseURI.Length != 0)
			{
				SetBaseUri(baseURI);
			}
		}
		IXmlLineInfo xmlLineInfo = null;
		if ((o & LoadOptions.SetLineInfo) != 0)
		{
			xmlLineInfo = r as IXmlLineInfo;
			if (xmlLineInfo != null && xmlLineInfo.HasLineInfo())
			{
				SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
		}
		if (r.MoveToFirstAttribute())
		{
			do
			{
				XAttribute xAttribute = new XAttribute(XNamespace.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value);
				if (xmlLineInfo != null && xmlLineInfo.HasLineInfo())
				{
					xAttribute.SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}
				AppendAttributeSkipNotify(xAttribute);
			}
			while (r.MoveToNextAttribute());
			r.MoveToElement();
		}
		if (!r.IsEmptyElement)
		{
			r.Read();
			ReadContentFrom(r, o);
		}
		r.Read();
	}

	internal void RemoveAttribute(XAttribute a)
	{
		bool flag = NotifyChanging(a, XObjectChangeEventArgs.Remove);
		if (a.parent != this)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
		}
		XAttribute xAttribute = lastAttr;
		XAttribute xAttribute2;
		while ((xAttribute2 = xAttribute.next) != a)
		{
			xAttribute = xAttribute2;
		}
		if (xAttribute == a)
		{
			lastAttr = null;
		}
		else
		{
			if (lastAttr == a)
			{
				lastAttr = xAttribute;
			}
			xAttribute.next = a.next;
		}
		a.parent = null;
		a.next = null;
		if (flag)
		{
			NotifyChanged(a, XObjectChangeEventArgs.Remove);
		}
	}

	private void RemoveAttributesSkipNotify()
	{
		if (lastAttr != null)
		{
			XAttribute xAttribute = lastAttr;
			do
			{
				XAttribute xAttribute2 = xAttribute.next;
				xAttribute.parent = null;
				xAttribute.next = null;
				xAttribute = xAttribute2;
			}
			while (xAttribute != lastAttr);
			lastAttr = null;
		}
	}

	internal void SetEndElementLineInfo(int lineNumber, int linePosition)
	{
		AddAnnotation(new LineInfoEndElementAnnotation(lineNumber, linePosition));
	}

	internal override void ValidateNode(XNode node, XNode previous)
	{
		if (node is XDocument)
		{
			throw new ArgumentException(Res.GetString("Argument_AddNode", XmlNodeType.Document));
		}
		if (node is XDocumentType)
		{
			throw new ArgumentException(Res.GetString("Argument_AddNode", XmlNodeType.DocumentType));
		}
	}
}
