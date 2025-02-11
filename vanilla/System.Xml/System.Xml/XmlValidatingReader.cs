using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.Schema;

namespace System.Xml;

/// <summary>Represents a reader that provides document type definition (DTD), XML-Data Reduced (XDR) schema, and XML Schema definition language (XSD) validation.</summary>
[Obsolete("Use XmlReader created by XmlReader.Create() method using appropriate XmlReaderSettings instead. http://go.microsoft.com/fwlink/?linkid=14202")]
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
public class XmlValidatingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
{
	private XmlValidatingReaderImpl impl;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlNodeType" /> values representing the type of the current node.</returns>
	public override XmlNodeType NodeType => impl.NodeType;

	/// <summary>Gets the qualified name of the current node.</summary>
	/// <returns>The qualified name of the current node. For example, Name is bk:book for the element &lt;bk:book&gt;.The name returned is dependent on the <see cref="P:System.Xml.XmlValidatingReader.NodeType" /> of the node. The following node types return the listed values. All other node types return an empty string.Node Type Name AttributeThe name of the attribute. DocumentTypeThe document type name. ElementThe tag name. EntityReferenceThe name of the entity referenced. ProcessingInstructionThe target of the processing instruction. XmlDeclarationThe literal string xml. </returns>
	public override string Name => impl.Name;

	/// <summary>Gets the local name of the current node.</summary>
	/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
	public override string LocalName => impl.LocalName;

	/// <summary>Gets the namespace Uniform Resource Identifier (URI) (as defined in the World Wide Web Consortium (W3C) Namespace specification) of the node on which the reader is positioned.</summary>
	/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
	public override string NamespaceURI => impl.NamespaceURI;

	/// <summary>Gets the namespace prefix associated with the current node.</summary>
	/// <returns>The namespace prefix associated with the current node.</returns>
	public override string Prefix => impl.Prefix;

	/// <summary>Gets a value indicating whether the current node can have a <see cref="P:System.Xml.XmlValidatingReader.Value" /> other than String.Empty.</summary>
	/// <returns>true if the node on which the reader is currently positioned can have a Value; otherwise, false.</returns>
	public override bool HasValue => impl.HasValue;

	/// <summary>Gets the text value of the current node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlValidatingReader.NodeType" /> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node Type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space between markup in a mixed content model. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
	public override string Value => impl.Value;

	/// <summary>Gets the depth of the current node in the XML document.</summary>
	/// <returns>The depth of the current node in the XML document.</returns>
	public override int Depth => impl.Depth;

	/// <summary>Gets the base URI of the current node.</summary>
	/// <returns>The base URI of the current node.</returns>
	public override string BaseURI => impl.BaseURI;

	/// <summary>Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).</summary>
	/// <returns>true if the current node is an element (<see cref="P:System.Xml.XmlValidatingReader.NodeType" /> equals XmlNodeType.Element) that ends with /&gt;; otherwise, false.</returns>
	public override bool IsEmptyElement => impl.IsEmptyElement;

	/// <summary>Gets a value indicating whether the current node is an attribute that was generated from the default value defined in the document type definition (DTD) or schema.</summary>
	/// <returns>true if the current node is an attribute whose value was generated from the default value defined in the DTD or schema; false if the attribute value was explicitly set.</returns>
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
	/// <returns>The number of attributes on the current node. This number includes default attributes.</returns>
	public override int AttributeCount => impl.AttributeCount;

	/// <summary>Gets a value indicating whether the reader is positioned at the end of the stream.</summary>
	/// <returns>true if the reader is positioned at the end of the stream; otherwise, false.</returns>
	public override bool EOF => impl.EOF;

	/// <summary>Gets the state of the reader.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ReadState" /> values.</returns>
	public override ReadState ReadState => impl.ReadState;

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.</summary>
	/// <returns>XmlNameTable that enables you to get the atomized version of a string within the node.</returns>
	public override XmlNameTable NameTable => impl.NameTable;

	/// <summary>Gets a value indicating whether this reader can parse and resolve entities.</summary>
	/// <returns>true if the reader can parse and resolve entities; otherwise, false. XmlValidatingReader always returns true.</returns>
	public override bool CanResolveEntity => true;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlValidatingReader" /> implements the binary content read methods.</summary>
	/// <returns>true if the binary content read methods are implemented; otherwise false. The <see cref="T:System.Xml.XmlValidatingReader" /> class returns true.</returns>
	public override bool CanReadBinaryContent => true;

	/// <summary>Gets the current line number.</summary>
	/// <returns>The current line number. The starting value for this property is 1.</returns>
	public int LineNumber => impl.LineNumber;

	/// <summary>Gets the current line position.</summary>
	/// <returns>The current line position. The starting value for this property is 1.</returns>
	public int LinePosition => impl.LinePosition;

	/// <summary>Gets a schema type object.</summary>
	/// <returns>
	///   <see cref="T:System.Xml.Schema.XmlSchemaDatatype" />, <see cref="T:System.Xml.Schema.XmlSchemaSimpleType" />, or <see cref="T:System.Xml.Schema.XmlSchemaComplexType" /> depending whether the node value is a built in XML Schema definition language (XSD) type or a user defined simpleType or complexType; null if the current node has no schema type.</returns>
	public object SchemaType => impl.SchemaType;

	/// <summary>Gets the <see cref="T:System.Xml.XmlReader" /> used to construct this XmlValidatingReader.</summary>
	/// <returns>The XmlReader specified in the constructor.</returns>
	public XmlReader Reader => impl.Reader;

	/// <summary>Gets or sets a value indicating the type of validation to perform.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ValidationType" /> values. If this property is not set, it defaults to ValidationType.Auto.</returns>
	/// <exception cref="T:System.InvalidOperationException">Setting the property after a Read has been called. </exception>
	public ValidationType ValidationType
	{
		get
		{
			return impl.ValidationType;
		}
		set
		{
			impl.ValidationType = value;
		}
	}

	/// <summary>Gets a <see cref="T:System.Xml.Schema.XmlSchemaCollection" /> to use for validation.</summary>
	/// <returns>The XmlSchemaCollection to use for validation.</returns>
	public XmlSchemaCollection Schemas => impl.Schemas;

	/// <summary>Gets or sets a value that specifies how the reader handles entities.</summary>
	/// <returns>One of the <see cref="T:System.Xml.EntityHandling" /> values. If no EntityHandling is specified, it defaults to EntityHandling.ExpandEntities.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Invalid value was specified. </exception>
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

	/// <summary>Sets the <see cref="T:System.Xml.XmlResolver" /> used for resolving external document type definition (DTD) and schema location references. The XmlResolver is also used to handle any import or include elements found in XML Schema definition language (XSD) schemas.</summary>
	/// <returns>The XmlResolver to use. If set to null, external resources are not resolved.In version 1.1 of the .NET Framework, the caller must be fully trusted to specify an XmlResolver.</returns>
	public XmlResolver XmlResolver
	{
		set
		{
			impl.XmlResolver = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to do namespace support.</summary>
	/// <returns>true to do namespace support; otherwise, false. The default is true.</returns>
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

	/// <summary>Gets the encoding attribute for the document.</summary>
	/// <returns>The encoding value. If no encoding attribute exists, and there is not byte-order mark, this defaults to UTF-8.</returns>
	public Encoding Encoding => impl.Encoding;

	internal XmlValidatingReaderImpl Impl => impl;

	internal override IDtdInfo DtdInfo => impl.DtdInfo;

	/// <summary>Sets an event handler for receiving information about document type definition (DTD), XML-Data Reduced (XDR) schema, and XML Schema definition language (XSD) schema validation errors.</summary>
	public event ValidationEventHandler ValidationEventHandler
	{
		add
		{
			impl.ValidationEventHandler += value;
		}
		remove
		{
			impl.ValidationEventHandler -= value;
		}
	}

	/// <summary>Initializes a new instance of the XmlValidatingReader class that validates the content returned from the given <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The XmlReader to read from while validating. The current implementation supports only <see cref="T:System.Xml.XmlTextReader" />. </param>
	/// <exception cref="T:System.ArgumentException">The reader specified is not an XmlTextReader. </exception>
	public XmlValidatingReader(XmlReader reader)
	{
		impl = new XmlValidatingReaderImpl(reader);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the XmlValidatingReader class with the specified values.</summary>
	/// <param name="xmlFragment">The string containing the XML fragment to parse. </param>
	/// <param name="fragType">The <see cref="T:System.Xml.XmlNodeType" /> of the XML fragment. This also determines what the fragment string can contain (see table below). </param>
	/// <param name="context">The <see cref="T:System.Xml.XmlParserContext" /> in which the XML fragment is to be parsed. This includes the <see cref="T:System.Xml.NameTable" /> to use, encoding, namespace scope, current xml:lang, and xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="fragType" /> is not one of the node types listed in the table below. </exception>
	public XmlValidatingReader(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
	{
		if (xmlFragment == null)
		{
			throw new ArgumentNullException("xmlFragment");
		}
		impl = new XmlValidatingReaderImpl(xmlFragment, fragType, context);
		impl.OuterReader = this;
	}

	/// <summary>Initializes a new instance of the XmlValidatingReader class with the specified values.</summary>
	/// <param name="xmlFragment">The stream containing the XML fragment to parse. </param>
	/// <param name="fragType">The <see cref="T:System.Xml.XmlNodeType" /> of the XML fragment. This determines what the fragment can contain (see table below). </param>
	/// <param name="context">The <see cref="T:System.Xml.XmlParserContext" /> in which the XML fragment is to be parsed. This includes the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, current xml:lang, and xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="fragType" /> is not one of the node types listed in the table below. </exception>
	public XmlValidatingReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
	{
		if (xmlFragment == null)
		{
			throw new ArgumentNullException("xmlFragment");
		}
		impl = new XmlValidatingReaderImpl(xmlFragment, fragType, context);
		impl.OuterReader = this;
	}

	/// <summary>Gets the value of the attribute with the specified name.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override string GetAttribute(string name)
	{
		return impl.GetAttribute(name);
	}

	/// <summary>Gets the value of the attribute with the specified local name and namespace Uniform Resource Identifier (URI).</summary>
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
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="i" /> parameter is less than 0 or greater than or equal to <see cref="P:System.Xml.XmlValidatingReader.AttributeCount" />. </exception>
	public override string GetAttribute(int i)
	{
		return impl.GetAttribute(i);
	}

	/// <summary>Moves to the attribute with the specified name.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the position of the reader does not change.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override bool MoveToAttribute(string name)
	{
		return impl.MoveToAttribute(name);
	}

	/// <summary>Moves to the attribute with the specified local name and namespace Uniform Resource Identifier (URI).</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the position of the reader does not change.</returns>
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
	public override bool Read()
	{
		return impl.Read();
	}

	/// <summary>Changes the <see cref="P:System.Xml.XmlReader.ReadState" /> to Closed.</summary>
	public override void Close()
	{
		impl.Close();
	}

	/// <summary>Resolves a namespace prefix in the current element's scope.</summary>
	/// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
	/// <param name="prefix">The prefix whose namespace Uniform Resource Identifier (URI) you want to resolve. To match the default namespace, pass an empty string. </param>
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
	/// <exception cref="T:System.InvalidOperationException">The reader is not positioned on an EntityReference node. </exception>
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
	///   <see cref="M:System.Xml.XmlValidatingReader.ReadContentAsBase64(System.Byte[],System.Int32,System.Int32)" />  is not supported on the current node.</exception>
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
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlValidatingReader" /> implementation does not support this method.</exception>
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
	///   <see cref="M:System.Xml.XmlValidatingReader.ReadContentAsBinHex(System.Byte[],System.Int32,System.Int32)" />  is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlValidatingReader" /> implementation does not support this method.</exception>
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
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlValidatingReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.Xml.XmlException">The element contains mixed-content.</exception>
	/// <exception cref="T:System.FormatException">The content cannot be converted to the requested type.</exception>
	public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
	{
		return impl.ReadElementContentAsBinHex(buffer, index, count);
	}

	/// <summary>Reads the contents of an element or text node as a string.</summary>
	/// <returns>The contents of the element or text node. This can be an empty string if the reader is positioned on something other than an element or text node, or if there is no more text content to return in the current context.NoteThe text node can be either an element or an attribute text node.</returns>
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
	/// <returns>An T:System.Collections.IDictionary object that identifies the namespaces in scope.</returns>
	/// <param name="scope">An <see cref="T:System.Xml.XmlNamespaceScope" /> object that identifies the scope of the reader.</param>
	IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
	{
		return impl.GetNamespacesInScope(scope);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupNamespace(System.String)" />.</summary>
	/// <returns>A string value that contains the namespace Uri that is associated with the prefix.</returns>
	/// <param name="prefix">The namespace prefix.</param>
	string IXmlNamespaceResolver.LookupNamespace(string prefix)
	{
		return impl.LookupNamespace(prefix);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupPrefix(System.String)" />.</summary>
	/// <returns>A string value that contains the namespace prefix that is associated with the <paramref name="namespaceName" />.</returns>
	/// <param name="namespaceName">The namespace that is associated with the prefix.</param>
	string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
	{
		return impl.LookupPrefix(namespaceName);
	}

	/// <summary>Gets the common language runtime type for the specified XML Schema definition language (XSD) type.</summary>
	/// <returns>The common language runtime type for the specified XML Schema type.</returns>
	public object ReadTypedValue()
	{
		return impl.ReadTypedValue();
	}
}
