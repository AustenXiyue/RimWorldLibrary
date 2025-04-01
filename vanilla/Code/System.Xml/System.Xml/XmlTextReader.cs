using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Permissions;
using System.Text;

namespace System.Xml;

/// <summary>Represents a reader that provides fast, non-cached, forward-only access to XML data.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class XmlTextReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
{
	private XmlTextReaderImpl impl;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlNodeType" /> values representing the type of the current node.</returns>
	public override XmlNodeType NodeType => impl.NodeType;

	/// <summary>Gets the qualified name of the current node.</summary>
	/// <returns>The qualified name of the current node. For example, Name is bk:book for the element &lt;bk:book&gt;.The name returned is dependent on the <see cref="P:System.Xml.XmlTextReader.NodeType" /> of the node. The following node types return the listed values. All other node types return an empty string.Node Type Name AttributeThe name of the attribute. DocumentTypeThe document type name. ElementThe tag name. EntityReferenceThe name of the entity referenced. ProcessingInstructionThe target of the processing instruction. XmlDeclarationThe literal string xml. </returns>
	public override string Name => impl.Name;

	/// <summary>Gets the local name of the current node.</summary>
	/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
	public override string LocalName => impl.LocalName;

	/// <summary>Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.</summary>
	/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
	public override string NamespaceURI => impl.NamespaceURI;

	/// <summary>Gets the namespace prefix associated with the current node.</summary>
	/// <returns>The namespace prefix associated with the current node.</returns>
	public override string Prefix => impl.Prefix;

	/// <summary>Gets a value indicating whether the current node can have a <see cref="P:System.Xml.XmlTextReader.Value" /> other than String.Empty.</summary>
	/// <returns>true if the node on which the reader is currently positioned can have a Value; otherwise, false.</returns>
	public override bool HasValue => impl.HasValue;

	/// <summary>Gets the text value of the current node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlTextReader.NodeType" /> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node Type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space within an xml:space= 'preserve' scope. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
	public override string Value => impl.Value;

	/// <summary>Gets the depth of the current node in the XML document.</summary>
	/// <returns>The depth of the current node in the XML document.</returns>
	public override int Depth => impl.Depth;

	/// <summary>Gets the base URI of the current node.</summary>
	/// <returns>The base URI of the current node.</returns>
	public override string BaseURI => impl.BaseURI;

	/// <summary>Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).</summary>
	/// <returns>true if the current node is an element (<see cref="P:System.Xml.XmlTextReader.NodeType" /> equals XmlNodeType.Element) that ends with /&gt;; otherwise, false.</returns>
	public override bool IsEmptyElement => impl.IsEmptyElement;

	/// <summary>Gets a value indicating whether the current node is an attribute that was generated from the default value defined in the DTD or schema.</summary>
	/// <returns>This property always returns false. (<see cref="T:System.Xml.XmlTextReader" /> does not expand default attributes.) </returns>
	public override bool IsDefault => impl.IsDefault;

	/// <summary>Gets the quotation mark character used to enclose the value of an attribute node.</summary>
	/// <returns>The quotation mark character (" or ') used to enclose the value of an attribute node.</returns>
	public override char QuoteChar => impl.QuoteChar;

	/// <summary>Gets the current xml:space scope.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlSpace" /> values. If no xml:space scope exists, this property defaults to XmlSpace.None.</returns>
	public override XmlSpace XmlSpace => impl.XmlSpace;

	/// <summary>Gets the current xml:lang scope.</summary>
	/// <returns>The current xml:lang scope.</returns>
	public override string XmlLang => impl.XmlLang;

	/// <summary>Gets the number of attributes on the current node.</summary>
	/// <returns>The number of attributes on the current node.</returns>
	public override int AttributeCount => impl.AttributeCount;

	/// <summary>Gets a value indicating whether the reader is positioned at the end of the stream.</summary>
	/// <returns>true if the reader is positioned at the end of the stream; otherwise, false.</returns>
	public override bool EOF => impl.EOF;

	/// <summary>Gets the state of the reader.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ReadState" /> values.</returns>
	public override ReadState ReadState => impl.ReadState;

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.</summary>
	/// <returns>The XmlNameTable enabling you to get the atomized version of a string within the node.</returns>
	public override XmlNameTable NameTable => impl.NameTable;

	/// <summary>Gets a value indicating whether this reader can parse and resolve entities.</summary>
	/// <returns>true if the reader can parse and resolve entities; otherwise, false. The XmlTextReader class always returns true.</returns>
	public override bool CanResolveEntity => true;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlTextReader" /> implements the binary content read methods.</summary>
	/// <returns>true if the binary content read methods are implemented; otherwise false. The <see cref="T:System.Xml.XmlTextReader" /> class always returns true.</returns>
	public override bool CanReadBinaryContent => true;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlTextReader" /> implements the <see cref="M:System.Xml.XmlReader.ReadValueChunk(System.Char[],System.Int32,System.Int32)" /> method.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XmlTextReader" /> implements the <see cref="M:System.Xml.XmlReader.ReadValueChunk(System.Char[],System.Int32,System.Int32)" /> method; otherwise false. The <see cref="T:System.Xml.XmlTextReader" /> class always returns false.</returns>
	public override bool CanReadValueChunk => false;

	/// <summary>Gets the current line number.</summary>
	/// <returns>The current line number.</returns>
	public int LineNumber => impl.LineNumber;

	/// <summary>Gets the current line position.</summary>
	/// <returns>The current line position.</returns>
	public int LinePosition => impl.LinePosition;

	/// <summary>Gets or sets a value indicating whether to do namespace support.</summary>
	/// <returns>true to do namespace support; otherwise, false. The default is true.</returns>
	/// <exception cref="T:System.InvalidOperationException">Setting this property after a read operation has occurred (<see cref="P:System.Xml.XmlTextReader.ReadState" /> is not ReadState.Initial). </exception>
	public bool Namespaces
	{
		get
		{
			return impl.Namespaces;
		}
		set
		{
			impl.Namespaces = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to normalize white space and attribute values.</summary>
	/// <returns>true to normalize; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">Setting this property when the reader is closed (<see cref="P:System.Xml.XmlTextReader.ReadState" /> is ReadState.Closed). </exception>
	public bool Normalization
	{
		get
		{
			return impl.Normalization;
		}
		set
		{
			impl.Normalization = value;
		}
	}

	/// <summary>Gets the encoding of the document.</summary>
	/// <returns>The encoding value. If no encoding attribute exists, and there is no byte-order mark, this defaults to UTF-8.</returns>
	public Encoding Encoding => impl.Encoding;

	/// <summary>Gets or sets a value that specifies how white space is handled.</summary>
	/// <returns>One of the <see cref="T:System.Xml.WhitespaceHandling" /> values. The default is WhitespaceHandling.All (returns Whitespace and SignificantWhitespace nodes).</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Invalid value specified. </exception>
	/// <exception cref="T:System.InvalidOperationException">Setting this property when the reader is closed (<see cref="P:System.Xml.XmlTextReader.ReadState" /> is ReadState.Closed). </exception>
	public WhitespaceHandling WhitespaceHandling
	{
		get
		{
			return impl.WhitespaceHandling;
		}
		set
		{
			impl.WhitespaceHandling = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to allow DTD processing. This property is obsolete. Use <see cref="P:System.Xml.XmlTextReader.DtdProcessing" /> instead.</summary>
	/// <returns>true to disallow DTD processing; otherwise false. The default is false.</returns>
	[Obsolete("Use DtdProcessing property instead.")]
	public bool ProhibitDtd
	{
		get
		{
			return impl.DtdProcessing == DtdProcessing.Prohibit;
		}
		set
		{
			impl.DtdProcessing = ((!value) ? DtdProcessing.Parse : DtdProcessing.Prohibit);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.DtdProcessing" /> enumeration.</summary>
	/// <returns>The <see cref="T:System.Xml.DtdProcessing" /> enumeration.</returns>
	public DtdProcessing DtdProcessing
	{
		get
		{
			return impl.DtdProcessing;
		}
		set
		{
			impl.DtdProcessing = value;
		}
	}

	/// <summary>Gets or sets a value that specifies how the reader handles entities.</summary>
	/// <returns>One of the <see cref="T:System.Xml.EntityHandling" /> values. If no EntityHandling is specified, it defaults to EntityHandling.ExpandCharEntities.</returns>
	public EntityHandling EntityHandling
	{
		get
		{
			return impl.EntityHandling;
		}
		set
		{
			impl.EntityHandling = value;
		}
	}

	/// <summary>Sets the <see cref="T:System.Xml.XmlResolver" /> used for resolving DTD references.</summary>
	/// <returns>The XmlResolver to use. If set to null, external resources are not resolved.In version 1.1 of the .NET Framework, the caller must be fully trusted in order to specify an XmlResolver.</returns>
	public XmlResolver XmlResolver
	{
		set
		{
			impl.XmlResolver = value;
		}
	}

	internal XmlTextReaderImpl Impl => impl;

	internal override XmlNamespaceManager NamespaceManager => impl.NamespaceManager;

	internal bool XmlValidatingReaderCompatibilityMode
	{
		set
		{
			impl.XmlValidatingReaderCompatibilityMode = value;
		}
	}

	internal override IDtdInfo DtdInfo => impl.DtdInfo;

	/// <summary>Initializes a new instance of the XmlTextReader.</summary>
	protected XmlTextReader()
	{
		impl = new XmlTextReaderImpl();
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="nt">The XmlNameTable to use. </param>
	protected XmlTextReader(XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(nt);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified stream.</summary>
	/// <param name="input">The stream containing the XML data to read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	public XmlTextReader(Stream input)
	{
		impl = new XmlTextReaderImpl(input);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified URL and stream.</summary>
	/// <param name="url">The URL to use for resolving external resources. The <see cref="P:System.Xml.XmlTextReader.BaseURI" /> is set to this value. </param>
	/// <param name="input">The stream containing the XML data to read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is null. </exception>
	public XmlTextReader(string url, Stream input)
	{
		impl = new XmlTextReaderImpl(url, input);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified stream and <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="input">The stream containing the XML data to read. </param>
	/// <param name="nt">The XmlNameTable to use. </param>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="input" /> or <paramref name="nt" /> value is null. </exception>
	public XmlTextReader(Stream input, XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(input, nt);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified URL, stream and <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="url">The URL to use for resolving external resources. The <see cref="P:System.Xml.XmlTextReader.BaseURI" /> is set to this value. If <paramref name="url" /> is null, BaseURI is set to String.Empty. </param>
	/// <param name="input">The stream containing the XML data to read. </param>
	/// <param name="nt">The XmlNameTable to use. </param>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="input" /> or <paramref name="nt" /> value is null. </exception>
	public XmlTextReader(string url, Stream input, XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(url, input, nt);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified <see cref="T:System.IO.TextReader" />.</summary>
	/// <param name="input">The TextReader containing the XML data to read. </param>
	public XmlTextReader(TextReader input)
	{
		impl = new XmlTextReaderImpl(input);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified URL and <see cref="T:System.IO.TextReader" />.</summary>
	/// <param name="url">The URL to use for resolving external resources. The <see cref="P:System.Xml.XmlTextReader.BaseURI" /> is set to this value. </param>
	/// <param name="input">The TextReader containing the XML data to read. </param>
	public XmlTextReader(string url, TextReader input)
	{
		impl = new XmlTextReaderImpl(url, input);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified <see cref="T:System.IO.TextReader" /> and <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="input">The TextReader containing the XML data to read. </param>
	/// <param name="nt">The XmlNameTable to use. </param>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="nt" /> value is null. </exception>
	public XmlTextReader(TextReader input, XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(input, nt);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified URL, <see cref="T:System.IO.TextReader" /> and <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="url">The URL to use for resolving external resources. The <see cref="P:System.Xml.XmlTextReader.BaseURI" /> is set to this value. If <paramref name="url" /> is null, BaseURI is set to String.Empty. </param>
	/// <param name="input">The TextReader containing the XML data to read. </param>
	/// <param name="nt">The XmlNameTable to use. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="nt" /> value is null. </exception>
	public XmlTextReader(string url, TextReader input, XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(url, input, nt);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified stream, <see cref="T:System.Xml.XmlNodeType" />, and <see cref="T:System.Xml.XmlParserContext" />.</summary>
	/// <param name="xmlFragment">The stream containing the XML fragment to parse. </param>
	/// <param name="fragType">The <see cref="T:System.Xml.XmlNodeType" /> of the XML fragment. This also determines what the fragment can contain. (See table below.) </param>
	/// <param name="context">The <see cref="T:System.Xml.XmlParserContext" /> in which the <paramref name="xmlFragment" /> is to be parsed. This includes the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, the current xml:lang, and the xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="fragType" /> is not an Element, Attribute, or Document XmlNodeType. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlFragment" /> is null. </exception>
	public XmlTextReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
	{
		impl = new XmlTextReaderImpl(xmlFragment, fragType, context);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified string, <see cref="T:System.Xml.XmlNodeType" />, and <see cref="T:System.Xml.XmlParserContext" />.</summary>
	/// <param name="xmlFragment">The string containing the XML fragment to parse. </param>
	/// <param name="fragType">The <see cref="T:System.Xml.XmlNodeType" /> of the XML fragment. This also determines what the fragment string can contain. (See table below.) </param>
	/// <param name="context">The <see cref="T:System.Xml.XmlParserContext" /> in which the <paramref name="xmlFragment" /> is to be parsed. This includes the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, the current xml:lang, and the xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="fragType" /> is not an Element, Attribute, or DocumentXmlNodeType. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xmlFragment" /> is null. </exception>
	public XmlTextReader(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
	{
		impl = new XmlTextReaderImpl(xmlFragment, fragType, context);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified file.</summary>
	/// <param name="url">The URL for the file containing the XML data. The <see cref="P:System.Xml.XmlTextReader.BaseURI" /> is set to this value. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be found.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">Part of the filename or directory cannot be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="url" /> is an empty string.</exception>
	/// <exception cref="T:System.Net.WebException">The remote filename cannot be resolved.-or-An error occurred while processing the request.</exception>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="url" /> is not a valid URI.</exception>
	public XmlTextReader(string url)
	{
		impl = new XmlTextReaderImpl(url, new NameTable());
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlTextReader" /> class with the specified file and <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="url">The URL for the file containing the XML data to read. </param>
	/// <param name="nt">The XmlNameTable to use. </param>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="nt" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be found.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">Part of the filename or directory cannot be found.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="url" /> is an empty string.</exception>
	/// <exception cref="T:System.Net.WebException">The remote filename cannot be resolved.-or-An error occurred while processing the request.</exception>
	/// <exception cref="T:System.UriFormatException">
	///   <paramref name="url" /> is not a valid URI.</exception>
	public XmlTextReader(string url, XmlNameTable nt)
	{
		impl = new XmlTextReaderImpl(url, nt);
		impl.OuterReader = this;
	}

	/// <summary>Gets the value of the attribute with the specified name.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override string GetAttribute(string name)
	{
		return impl.GetAttribute(name);
	}

	/// <summary>Gets the value of the attribute with the specified local name and namespace URI.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned. This method does not move the reader.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public override string GetAttribute(string localName, string namespaceURI)
	{
		return impl.GetAttribute(localName, namespaceURI);
	}

	/// <summary>Gets the value of the attribute with the specified index.</summary>
	/// <returns>The value of the specified attribute.</returns>
	/// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.) </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="i" /> parameter is less than 0 or greater than or equal to <see cref="P:System.Xml.XmlTextReader.AttributeCount" />. </exception>
	public override string GetAttribute(int i)
	{
		return impl.GetAttribute(i);
	}

	/// <summary>Moves to the attribute with the specified name.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override bool MoveToAttribute(string name)
	{
		return impl.MoveToAttribute(name);
	}

	/// <summary>Moves to the attribute with the specified local name and namespace URI.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
		return impl.MoveToAttribute(localName, namespaceURI);
	}

	/// <summary>Moves to the attribute with the specified index.</summary>
	/// <param name="i">The index of the attribute. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="i" /> parameter is less than 0 or greater than or equal to <see cref="P:System.Xml.XmlReader.AttributeCount" />. </exception>
	public override void MoveToAttribute(int i)
	{
		impl.MoveToAttribute(i);
	}

	/// <summary>Moves to the first attribute.</summary>
	/// <returns>true if an attribute exists (the reader moves to the first attribute); otherwise, false (the position of the reader does not change).</returns>
	public override bool MoveToFirstAttribute()
	{
		return impl.MoveToFirstAttribute();
	}

	/// <summary>Moves to the next attribute.</summary>
	/// <returns>true if there is a next attribute; false if there are no more attributes.</returns>
	public override bool MoveToNextAttribute()
	{
		return impl.MoveToNextAttribute();
	}

	/// <summary>Moves to the element that contains the current attribute node.</summary>
	/// <returns>true if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); false if the reader is not positioned on an attribute (the position of the reader does not change).</returns>
	public override bool MoveToElement()
	{
		return impl.MoveToElement();
	}

	/// <summary>Parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.</summary>
	/// <returns>true if there are nodes to return.false if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read.An empty attribute, such as, misc="", returns true with a single node with a value of String.Empty.</returns>
	public override bool ReadAttributeValue()
	{
		return impl.ReadAttributeValue();
	}

	/// <summary>Reads the next node from the stream.</summary>
	/// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
	/// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML. </exception>
	public override bool Read()
	{
		return impl.Read();
	}

	/// <summary>Changes the <see cref="P:System.Xml.XmlReader.ReadState" /> to Closed.</summary>
	public override void Close()
	{
		impl.Close();
	}

	/// <summary>Skips the children of the current node.</summary>
	public override void Skip()
	{
		impl.Skip();
	}

	/// <summary>Resolves a namespace prefix in the current element's scope.</summary>
	/// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string. This string does not have to be atomized. </param>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Xml.XmlTextReader.Namespaces" /> property is set to true and the <paramref name="prefix" /> value is null. </exception>
	public override string LookupNamespace(string prefix)
	{
		string text = impl.LookupNamespace(prefix);
		if (text != null && text.Length == 0)
		{
			text = null;
		}
		return text;
	}

	/// <summary>Resolves the entity reference for EntityReference nodes.</summary>
	public override void ResolveEntity()
	{
		impl.ResolveEntity();
	}

	/// <summary>Reads the content and returns the Base64 decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlTextReader.ReadContentAsBase64(System.Byte[],System.Int32,System.Int32)" />  is not supported in the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	public override int ReadContentAsBase64(byte[] buffer, int index, int count)
	{
		return impl.ReadContentAsBase64(buffer, index, count);
	}

	/// <summary>Reads the element and decodes the Base64 content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node is not an element node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlTextReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.Xml.XmlException">The element contains mixed-content.</exception>
	/// <exception cref="T:System.FormatException">The content cannot be converted to the requested type.</exception>
	public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
	{
		return impl.ReadElementContentAsBase64(buffer, index, count);
	}

	/// <summary>Reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlTextReader.ReadContentAsBinHex(System.Byte[],System.Int32,System.Int32)" />  is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlTextReader" /> implementation does not support this method.</exception>
	public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
	{
		return impl.ReadContentAsBinHex(buffer, index, count);
	}

	/// <summary>Reads the element and decodes the BinHex content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node is not an element node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.Xml.XmlException">The element contains mixed-content.</exception>
	/// <exception cref="T:System.FormatException">The content cannot be converted to the requested type.</exception>
	public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
	{
		return impl.ReadElementContentAsBinHex(buffer, index, count);
	}

	/// <summary>Reads the contents of an element or a text node as a string.</summary>
	/// <returns>The contents of the element or text node. This can be an empty string if the reader is positioned on something other than an element or text node, or if there is no more text content to return in the current context.Note: The text node can be either an element or an attribute text node.</returns>
	/// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML. </exception>
	/// <exception cref="T:System.InvalidOperationException">An invalid operation was attempted. </exception>
	public override string ReadString()
	{
		impl.MoveOffEntityReference();
		return base.ReadString();
	}

	/// <summary>Gets a value indicating whether the class can return line information.</summary>
	/// <returns>true if the class can return line information; otherwise, false.</returns>
	public bool HasLineInfo()
	{
		return true;
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.GetNamespacesInScope(System.Xml.XmlNamespaceScope)" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> that contains the current in-scope namespaces.</returns>
	/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope" /> value that specifies the type of namespace nodes to return.</param>
	IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
	{
		return impl.GetNamespacesInScope(scope);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupNamespace(System.String)" />.</summary>
	/// <returns>The namespace URI that is mapped to the prefix; null if the prefix is not mapped to a namespace URI.</returns>
	/// <param name="prefix">The prefix whose namespace URI you wish to find.</param>
	string IXmlNamespaceResolver.LookupNamespace(string prefix)
	{
		return impl.LookupNamespace(prefix);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupPrefix(System.String)" />.</summary>
	/// <returns>The prefix that is mapped to the namespace URI; null if the namespace URI is not mapped to a prefix.</returns>
	/// <param name="namespaceName">The namespace URI whose prefix you wish to find.</param>
	string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
	{
		return impl.LookupPrefix(namespaceName);
	}

	/// <summary>Gets a collection that contains all namespaces currently in-scope.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionary" /> object that contains all the current in-scope namespaces. If the reader is not positioned on an element, an empty dictionary (no namespaces) is returned.</returns>
	/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope" /> value that specifies the type of namespace nodes to return.</param>
	public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
	{
		return impl.GetNamespacesInScope(scope);
	}

	/// <summary>Resets the state of the reader to ReadState.Initial.</summary>
	/// <exception cref="T:System.InvalidOperationException">Calling ResetState if the reader was constructed using an <see cref="T:System.Xml.XmlParserContext" />. </exception>
	/// <exception cref="T:System.Xml.XmlException">Documents in a single stream do not share the same encoding.</exception>
	public void ResetState()
	{
		impl.ResetState();
	}

	/// <summary>Gets the remainder of the buffered XML.</summary>
	/// <returns>A <see cref="T:System.IO.TextReader" /> containing the remainder of the buffered XML.</returns>
	public TextReader GetRemainder()
	{
		return impl.GetRemainder();
	}

	/// <summary>Reads the text contents of an element into a character buffer. This method is designed to read large streams of embedded text by calling it successively.</summary>
	/// <returns>The number of characters read. This can be 0 if the reader is not positioned on an element or if there is no more text content to return in the current context.</returns>
	/// <param name="buffer">The array of characters that serves as the buffer to which the text contents are written. </param>
	/// <param name="index">The position within <paramref name="buffer" /> where the method can begin writing text contents. </param>
	/// <param name="count">The number of characters to write into <paramref name="buffer" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="count" /> is greater than the space specified in the <paramref name="buffer" /> (buffer size - <paramref name="index" />). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" />&lt; 0 or <paramref name="count" />&lt; 0. </exception>
	public int ReadChars(char[] buffer, int index, int count)
	{
		return impl.ReadChars(buffer, index, count);
	}

	/// <summary>Decodes Base64 and returns the decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="array">The array of characters that serves as the buffer to which the text contents are written. </param>
	/// <param name="offset">The zero-based index into the array specifying where the method can begin to write to the buffer. </param>
	/// <param name="len">The number of bytes to write into the buffer. </param>
	/// <exception cref="T:System.Xml.XmlException">The Base64 sequence is not valid. </exception>
	/// <exception cref="T:System.ArgumentNullException">The value of <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> &lt; 0, or <paramref name="len" /> &lt; 0, or <paramref name="len" /> &gt; <paramref name="array" />.Length- <paramref name="offset" />. </exception>
	public int ReadBase64(byte[] array, int offset, int len)
	{
		return impl.ReadBase64(array, offset, len);
	}

	/// <summary>Decodes BinHex and returns the decoded binary bytes.</summary>
	/// <returns>The number of bytes written to your buffer.</returns>
	/// <param name="array">The byte array that serves as the buffer to which the decoded binary bytes are written. </param>
	/// <param name="offset">The zero-based index into the array specifying where the method can begin to write to the buffer. </param>
	/// <param name="len">The number of bytes to write into the buffer. </param>
	/// <exception cref="T:System.Xml.XmlException">The BinHex sequence is not valid. </exception>
	/// <exception cref="T:System.ArgumentNullException">The value of <paramref name="array" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="offset" /> &lt; 0, or <paramref name="len" /> &lt; 0, or <paramref name="len" /> &gt; <paramref name="array" />.Length- <paramref name="offset" />. </exception>
	public int ReadBinHex(byte[] array, int offset, int len)
	{
		return impl.ReadBinHex(array, offset, len);
	}
}
