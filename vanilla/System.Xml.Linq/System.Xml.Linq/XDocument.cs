using System.IO;
using System.Text;

namespace System.Xml.Linq;

/// <summary>Represents an XML document. </summary>
public class XDocument : XContainer
{
	private XDeclaration declaration;

	/// <summary>Gets or sets the XML declaration for this document.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDeclaration" /> that contains the XML declaration for this document.</returns>
	public XDeclaration Declaration
	{
		get
		{
			return declaration;
		}
		set
		{
			declaration = value;
		}
	}

	/// <summary>Gets the Document Type Definition (DTD) for this document.</summary>
	/// <returns>A <see cref="T:System.Xml.Linq.XDocumentType" /> that contains the DTD for this document.</returns>
	public XDocumentType DocumentType => GetFirstNode<XDocumentType>();

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XDocument" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.Document" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Document;

	/// <summary>Gets the root element of the XML Tree for this document.</summary>
	/// <returns>The root <see cref="T:System.Xml.Linq.XElement" /> of the XML tree.</returns>
	public XElement Root => GetFirstNode<XElement>();

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDocument" /> class. </summary>
	public XDocument()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDocument" /> class with the specified content.</summary>
	/// <param name="content">A parameter list of content objects to add to this document.</param>
	public XDocument(params object[] content)
		: this()
	{
		AddContentSkipNotify(content);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDocument" /> class with the specified <see cref="T:System.Xml.Linq.XDeclaration" /> and content.</summary>
	/// <param name="declaration">An <see cref="T:System.Xml.Linq.XDeclaration" /> for the document.</param>
	/// <param name="content">The content of the document.</param>
	public XDocument(XDeclaration declaration, params object[] content)
		: this(content)
	{
		this.declaration = declaration;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Linq.XDocument" /> class from an existing <see cref="T:System.Xml.Linq.XDocument" /> object.</summary>
	/// <param name="other">The <see cref="T:System.Xml.Linq.XDocument" /> object that will be copied.</param>
	public XDocument(XDocument other)
		: base(other)
	{
		if (other.declaration != null)
		{
			declaration = new XDeclaration(other.declaration);
		}
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a file. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the contents of the specified file.</returns>
	/// <param name="uri">A URI string that references the file to load into a new <see cref="T:System.Xml.Linq.XDocument" />.</param>
	public static XDocument Load(string uri)
	{
		return Load(uri, LoadOptions.None);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a file, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the contents of the specified file.</returns>
	/// <param name="uri">A URI string that references the file to load into a new <see cref="T:System.Xml.Linq.XDocument" />.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XDocument Load(string uri, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(uri, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> instance by using the specified stream.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> object that reads the data that is contained in the stream. </returns>
	/// <param name="stream">The stream that contains the XML data.</param>
	public static XDocument Load(Stream stream)
	{
		return Load(stream, LoadOptions.None);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> instance by using the specified stream, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> object that reads the data that is contained in the stream.</returns>
	/// <param name="stream">The stream containing the XML data.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies whether to load base URI and line information.</param>
	public static XDocument Load(Stream stream, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(stream, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a <see cref="T:System.IO.TextReader" />. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the contents of the specified <see cref="T:System.IO.TextReader" />.</returns>
	/// <param name="textReader">A <see cref="T:System.IO.TextReader" /> that contains the content for the <see cref="T:System.Xml.Linq.XDocument" />.</param>
	public static XDocument Load(TextReader textReader)
	{
		return Load(textReader, LoadOptions.None);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a <see cref="T:System.IO.TextReader" />, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the XML that was read from the specified <see cref="T:System.IO.TextReader" />.</returns>
	/// <param name="textReader">A <see cref="T:System.IO.TextReader" /> that contains the content for the <see cref="T:System.Xml.Linq.XDocument" />.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XDocument Load(TextReader textReader, LoadOptions options)
	{
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(textReader, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from an <see cref="T:System.Xml.XmlReader" />. </summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the contents of the specified <see cref="T:System.Xml.XmlReader" />.</returns>
	/// <param name="reader">A <see cref="T:System.Xml.XmlReader" /> that contains the content for the <see cref="T:System.Xml.Linq.XDocument" />.</param>
	public static XDocument Load(XmlReader reader)
	{
		return Load(reader, LoadOptions.None);
	}

	/// <summary>Loads an <see cref="T:System.Xml.Linq.XElement" /> from an <see cref="T:System.Xml.XmlReader" />, optionally setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> that contains the XML that was read from the specified <see cref="T:System.Xml.XmlReader" />.</returns>
	/// <param name="reader">A <see cref="T:System.Xml.XmlReader" /> that will be read for the content of the <see cref="T:System.Xml.Linq.XDocument" />.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies whether to load base URI and line information.</param>
	public static XDocument Load(XmlReader reader, LoadOptions options)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.ReadState == ReadState.Initial)
		{
			reader.Read();
		}
		XDocument xDocument = new XDocument();
		if ((options & LoadOptions.SetBaseUri) != 0)
		{
			string baseURI = reader.BaseURI;
			if (baseURI != null && baseURI.Length != 0)
			{
				xDocument.SetBaseUri(baseURI);
			}
		}
		if ((options & LoadOptions.SetLineInfo) != 0 && reader is IXmlLineInfo xmlLineInfo && xmlLineInfo.HasLineInfo())
		{
			xDocument.SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}
		if (reader.NodeType == XmlNodeType.XmlDeclaration)
		{
			xDocument.Declaration = new XDeclaration(reader);
		}
		xDocument.ReadContentFrom(reader, options);
		if (!reader.EOF)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedEndOfFile"));
		}
		if (xDocument.Root == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingRoot"));
		}
		return xDocument;
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a string.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> populated from the string that contains XML.</returns>
	/// <param name="text">A string that contains XML.</param>
	public static XDocument Parse(string text)
	{
		return Parse(text, LoadOptions.None);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.Linq.XDocument" /> from a string, optionally preserving white space, setting the base URI, and retaining line information.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XDocument" /> populated from the string that contains XML.</returns>
	/// <param name="text">A string that contains XML.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.LoadOptions" /> that specifies white space behavior, and whether to load base URI and line information.</param>
	public static XDocument Parse(string text, LoadOptions options)
	{
		using StringReader input = new StringReader(text);
		XmlReaderSettings xmlReaderSettings = XNode.GetXmlReaderSettings(options);
		using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
		return Load(reader, options);
	}

	/// <summary>Serialize this <see cref="T:System.Xml.Linq.XDocument" /> to a file, overwriting an existing file, if it exists.</summary>
	/// <param name="fileName">A string that contains the name of the file.</param>
	public void Save(string fileName)
	{
		Save(fileName, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Serialize this <see cref="T:System.Xml.Linq.XDocument" /> to a file, optionally disabling formatting.</summary>
	/// <param name="fileName">A string that contains the name of the file.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(string fileName, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		if (declaration != null && !string.IsNullOrEmpty(declaration.Encoding))
		{
			try
			{
				xmlWriterSettings.Encoding = Encoding.GetEncoding(declaration.Encoding);
			}
			catch (ArgumentException)
			{
			}
		}
		using XmlWriter writer = XmlWriter.Create(fileName, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XDocument" /> to the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XDocument" /> to.</param>
	public void Save(Stream stream)
	{
		Save(stream, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Outputs this <see cref="T:System.Xml.Linq.XDocument" /> to the specified <see cref="T:System.IO.Stream" />, optionally specifying formatting behavior.</summary>
	/// <param name="stream">The stream to output this <see cref="T:System.Xml.Linq.XDocument" /> to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(Stream stream, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		if (declaration != null && !string.IsNullOrEmpty(declaration.Encoding))
		{
			try
			{
				xmlWriterSettings.Encoding = Encoding.GetEncoding(declaration.Encoding);
			}
			catch (ArgumentException)
			{
			}
		}
		using XmlWriter writer = XmlWriter.Create(stream, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this <see cref="T:System.Xml.Linq.XDocument" /> to a <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="textWriter">A <see cref="T:System.IO.TextWriter" /> that the <see cref="T:System.Xml.Linq.XDocument" /> will be written to.</param>
	public void Save(TextWriter textWriter)
	{
		Save(textWriter, GetSaveOptionsFromAnnotations());
	}

	/// <summary>Serialize this <see cref="T:System.Xml.Linq.XDocument" /> to a <see cref="T:System.IO.TextWriter" />, optionally disabling formatting.</summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> to output the XML to.</param>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public void Save(TextWriter textWriter, SaveOptions options)
	{
		XmlWriterSettings xmlWriterSettings = XNode.GetXmlWriterSettings(options);
		using XmlWriter writer = XmlWriter.Create(textWriter, xmlWriterSettings);
		Save(writer);
	}

	/// <summary>Serialize this <see cref="T:System.Xml.Linq.XDocument" /> to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">A <see cref="T:System.Xml.XmlWriter" /> that the <see cref="T:System.Xml.Linq.XDocument" /> will be written to.</param>
	public void Save(XmlWriter writer)
	{
		WriteTo(writer);
	}

	/// <summary>Write this document to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (declaration != null && declaration.Standalone == "yes")
		{
			writer.WriteStartDocument(standalone: true);
		}
		else if (declaration != null && declaration.Standalone == "no")
		{
			writer.WriteStartDocument(standalone: false);
		}
		else
		{
			writer.WriteStartDocument();
		}
		WriteContentTo(writer);
		writer.WriteEndDocument();
	}

	internal override void AddAttribute(XAttribute a)
	{
		throw new ArgumentException(Res.GetString("Argument_AddAttribute"));
	}

	internal override void AddAttributeSkipNotify(XAttribute a)
	{
		throw new ArgumentException(Res.GetString("Argument_AddAttribute"));
	}

	internal override XNode CloneNode()
	{
		return new XDocument(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XDocument e)
		{
			return ContentsEqual(e);
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return ContentsHashCode();
	}

	private T GetFirstNode<T>() where T : XNode
	{
		XNode xNode = content as XNode;
		if (xNode != null)
		{
			do
			{
				xNode = xNode.next;
				if (xNode is T result)
				{
					return result;
				}
			}
			while (xNode != content);
		}
		return null;
	}

	internal static bool IsWhitespace(string s)
	{
		foreach (char c in s)
		{
			if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
			{
				return false;
			}
		}
		return true;
	}

	internal override void ValidateNode(XNode node, XNode previous)
	{
		switch (node.NodeType)
		{
		case XmlNodeType.Text:
			ValidateString(((XText)node).Value);
			break;
		case XmlNodeType.Element:
			ValidateDocument(previous, XmlNodeType.DocumentType, XmlNodeType.None);
			break;
		case XmlNodeType.DocumentType:
			ValidateDocument(previous, XmlNodeType.None, XmlNodeType.Element);
			break;
		case XmlNodeType.CDATA:
			throw new ArgumentException(Res.GetString("Argument_AddNode", XmlNodeType.CDATA));
		case XmlNodeType.Document:
			throw new ArgumentException(Res.GetString("Argument_AddNode", XmlNodeType.Document));
		}
	}

	private void ValidateDocument(XNode previous, XmlNodeType allowBefore, XmlNodeType allowAfter)
	{
		XNode xNode = content as XNode;
		if (xNode == null)
		{
			return;
		}
		if (previous == null)
		{
			allowBefore = allowAfter;
		}
		do
		{
			xNode = xNode.next;
			XmlNodeType nodeType = xNode.NodeType;
			if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.DocumentType)
			{
				if (nodeType != allowBefore)
				{
					throw new InvalidOperationException(Res.GetString("InvalidOperation_DocumentStructure"));
				}
				allowBefore = XmlNodeType.None;
			}
			if (xNode == previous)
			{
				allowBefore = allowAfter;
			}
		}
		while (xNode != content);
	}

	internal override void ValidateString(string s)
	{
		if (!IsWhitespace(s))
		{
			throw new ArgumentException(Res.GetString("Argument_AddNonWhitespace"));
		}
	}
}
