using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace System.Xml;

/// <summary>Represents a reader that provides fast, noncached, forward-only access to XML data.</summary>
[DebuggerDisplay("{debuggerDisplayProxy}")]
[DebuggerDisplay("{debuggerDisplayProxy}")]
public abstract class XmlReader : IDisposable
{
	[DebuggerDisplay("{ToString()}")]
	private struct XmlReaderDebuggerDisplayProxy
	{
		private XmlReader reader;

		internal XmlReaderDebuggerDisplayProxy(XmlReader reader)
		{
			this.reader = reader;
		}

		public override string ToString()
		{
			XmlNodeType nodeType = reader.NodeType;
			string text = nodeType.ToString();
			switch (nodeType)
			{
			case XmlNodeType.Element:
			case XmlNodeType.EntityReference:
			case XmlNodeType.EndElement:
			case XmlNodeType.EndEntity:
				text = text + ", Name=\"" + reader.Name + "\"";
				break;
			case XmlNodeType.Attribute:
			case XmlNodeType.ProcessingInstruction:
				text = text + ", Name=\"" + reader.Name + "\", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(reader.Value) + "\"";
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Comment:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.XmlDeclaration:
				text = text + ", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(reader.Value) + "\"";
				break;
			case XmlNodeType.DocumentType:
				text = text + ", Name=\"" + reader.Name + "'";
				text = text + ", SYSTEM=\"" + reader.GetAttribute("SYSTEM") + "\"";
				text = text + ", PUBLIC=\"" + reader.GetAttribute("PUBLIC") + "\"";
				text = text + ", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(reader.Value) + "\"";
				break;
			}
			return text;
		}
	}

	private static uint IsTextualNodeBitmap = 24600u;

	private static uint CanReadContentAsBitmap = 123324u;

	private static uint HasValueBitmap = 157084u;

	internal const int DefaultBufferSize = 4096;

	internal const int BiggerBufferSize = 8192;

	internal const int MaxStreamLengthForDefaultBufferSize = 65536;

	internal const int AsyncBufferSize = 65536;

	/// <summary>Gets the <see cref="T:System.Xml.XmlReaderSettings" /> object used to create this <see cref="T:System.Xml.XmlReader" /> instance.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlReaderSettings" /> object used to create this reader instance. If this reader was not created using the <see cref="Overload:System.Xml.XmlReader.Create" /> method, this property returns null.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual XmlReaderSettings Settings => null;

	/// <summary>When overridden in a derived class, gets the type of the current node.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlNodeType" /> values representing the type of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract XmlNodeType NodeType { get; }

	/// <summary>When overridden in a derived class, gets the qualified name of the current node.</summary>
	/// <returns>The qualified name of the current node. For example, Name is bk:book for the element &lt;bk:book&gt;.The name returned is dependent on the <see cref="P:System.Xml.XmlReader.NodeType" /> of the node. The following node types return the listed values. All other node types return an empty string.Node type Name AttributeThe name of the attribute. DocumentTypeThe document type name. ElementThe tag name. EntityReferenceThe name of the entity referenced. ProcessingInstructionThe target of the processing instruction. XmlDeclarationThe literal string xml. </returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string Name
	{
		get
		{
			if (Prefix.Length == 0)
			{
				return LocalName;
			}
			return NameTable.Add(Prefix + ":" + LocalName);
		}
	}

	/// <summary>When overridden in a derived class, gets the local name of the current node.</summary>
	/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string LocalName { get; }

	/// <summary>When overridden in a derived class, gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.</summary>
	/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string NamespaceURI { get; }

	/// <summary>When overridden in a derived class, gets the namespace prefix associated with the current node.</summary>
	/// <returns>The namespace prefix associated with the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string Prefix { get; }

	/// <summary>When overridden in a derived class, gets a value indicating whether the current node can have a <see cref="P:System.Xml.XmlReader.Value" />.</summary>
	/// <returns>true if the node on which the reader is currently positioned can have a Value; otherwise, false. If false, the node has a value of String.Empty.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool HasValue => HasValueInternal(NodeType);

	/// <summary>When overridden in a derived class, gets the text value of the current node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlReader.NodeType" /> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space between markup in a mixed content model. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string Value { get; }

	/// <summary>When overridden in a derived class, gets the depth of the current node in the XML document.</summary>
	/// <returns>The depth of the current node in the XML document.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract int Depth { get; }

	/// <summary>When overridden in a derived class, gets the base URI of the current node.</summary>
	/// <returns>The base URI of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string BaseURI { get; }

	/// <summary>When overridden in a derived class, gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).</summary>
	/// <returns>true if the current node is an element (<see cref="P:System.Xml.XmlReader.NodeType" /> equals XmlNodeType.Element) that ends with /&gt;; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool IsEmptyElement { get; }

	/// <summary>When overridden in a derived class, gets a value indicating whether the current node is an attribute that was generated from the default value defined in the DTD or schema.</summary>
	/// <returns>true if the current node is an attribute whose value was generated from the default value defined in the DTD or schema; false if the attribute value was explicitly set.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool IsDefault => false;

	/// <summary>When overridden in a derived class, gets the quotation mark character used to enclose the value of an attribute node.</summary>
	/// <returns>The quotation mark character (" or ') used to enclose the value of an attribute node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual char QuoteChar => '"';

	/// <summary>When overridden in a derived class, gets the current xml:space scope.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlSpace" /> values. If no xml:space scope exists, this property defaults to XmlSpace.None.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual XmlSpace XmlSpace => XmlSpace.None;

	/// <summary>When overridden in a derived class, gets the current xml:lang scope.</summary>
	/// <returns>The current xml:lang scope.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string XmlLang => string.Empty;

	/// <summary>Gets the schema information that has been assigned to the current node as a result of schema validation.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object containing the schema information for the current node. Schema information can be set on elements, attributes, or on text nodes with a non-null <see cref="P:System.Xml.XmlReader.ValueType" /> (typed values).If the current node is not one of the above node types, or if the XmlReader instance does not report schema information, this property returns null.If this property is called from an <see cref="T:System.Xml.XmlTextReader" /> or an <see cref="T:System.Xml.XmlValidatingReader" /> object, this property always returns null. These XmlReader implementations do not expose schema information through the SchemaInfo property.NoteIf you have to get the post-schema-validation information set (PSVI) for an element, position the reader on the end tag of the element, rather than on the start tag. You get the PSVI through the SchemaInfo property of a reader. The validating reader that is created through <see cref="Overload:System.Xml.XmlReader.Create" /> with the <see cref="P:System.Xml.XmlReaderSettings.ValidationType" /> property set to <see cref="F:System.Xml.ValidationType.Schema" /> has complete PSVI for an element only when the reader is positioned on the end tag of an element.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual IXmlSchemaInfo SchemaInfo => this as IXmlSchemaInfo;

	/// <summary>Gets The Common Language Runtime (CLR) type for the current node.</summary>
	/// <returns>The CLR type that corresponds to the typed value of the node. The default is System.String.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual Type ValueType => typeof(string);

	/// <summary>When overridden in a derived class, gets the number of attributes on the current node.</summary>
	/// <returns>The number of attributes on the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract int AttributeCount { get; }

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified index.</summary>
	/// <returns>The value of the specified attribute.</returns>
	/// <param name="i">The index of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string this[int i] => GetAttribute(i);

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The qualified name of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string this[string name] => GetAttribute(name);

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The local name of the attribute.</param>
	/// <param name="namespaceURI">The namespace URI of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string this[string name, string namespaceURI] => GetAttribute(name, namespaceURI);

	/// <summary>When overridden in a derived class, gets a value indicating whether the reader is positioned at the end of the stream.</summary>
	/// <returns>true if the reader is positioned at the end of the stream; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool EOF { get; }

	/// <summary>When overridden in a derived class, gets the state of the reader.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ReadState" /> values.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract ReadState ReadState { get; }

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.</summary>
	/// <returns>The XmlNameTable enabling you to get the atomized version of a string within the node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract XmlNameTable NameTable { get; }

	/// <summary>Gets a value indicating whether this reader can parse and resolve entities.</summary>
	/// <returns>true if the reader can parse and resolve entities; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool CanResolveEntity => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlReader" /> implements the binary content read methods.</summary>
	/// <returns>true if the binary content read methods are implemented; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool CanReadBinaryContent => false;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlReader" /> implements the <see cref="M:System.Xml.XmlReader.ReadValueChunk(System.Char[],System.Int32,System.Int32)" /> method.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XmlReader" /> implements the <see cref="M:System.Xml.XmlReader.ReadValueChunk(System.Char[],System.Int32,System.Int32)" /> method; otherwise false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool CanReadValueChunk => false;

	/// <summary>Gets a value indicating whether the current node has any attributes.</summary>
	/// <returns>true if the current node has attributes; otherwise, false.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool HasAttributes => AttributeCount > 0;

	internal virtual XmlNamespaceManager NamespaceManager => null;

	internal bool IsDefaultInternal
	{
		get
		{
			if (IsDefault)
			{
				return true;
			}
			IXmlSchemaInfo schemaInfo = SchemaInfo;
			if (schemaInfo != null && schemaInfo.IsDefault)
			{
				return true;
			}
			return false;
		}
	}

	internal virtual IDtdInfo DtdInfo => null;

	private object debuggerDisplayProxy => new XmlReaderDebuggerDisplayProxy(this);

	/// <summary>Reads the text content at the current position as an <see cref="T:System.Object" />.</summary>
	/// <returns>The text content as the most appropriate common language runtime (CLR) object.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadContentAsObject()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsObject");
		}
		return InternalReadContentAsString();
	}

	/// <summary>Reads the text content at the current position as a Boolean.</summary>
	/// <returns>The text content as a <see cref="T:System.Boolean" /> object.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool ReadContentAsBoolean()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsBoolean");
		}
		try
		{
			return XmlConvert.ToBoolean(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Boolean", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The text content as a <see cref="T:System.DateTime" /> object.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual DateTime ReadContentAsDateTime()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsDateTime");
		}
		try
		{
			return XmlConvert.ToDateTime(InternalReadContentAsString(), XmlDateTimeSerializationMode.RoundtripKind);
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "DateTime", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a <see cref="T:System.DateTimeOffset" /> object.</summary>
	/// <returns>The text content as a <see cref="T:System.DateTimeOffset" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual DateTimeOffset ReadContentAsDateTimeOffset()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsDateTimeOffset");
		}
		try
		{
			return XmlConvert.ToDateTimeOffset(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "DateTimeOffset", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a double-precision floating-point number.</summary>
	/// <returns>The text content as a double-precision floating-point number.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual double ReadContentAsDouble()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsDouble");
		}
		try
		{
			return XmlConvert.ToDouble(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Double", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a single-precision floating point number.</summary>
	/// <returns>The text content at the current position as a single-precision floating point number.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual float ReadContentAsFloat()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsFloat");
		}
		try
		{
			return XmlConvert.ToSingle(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Float", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The text content at the current position as a <see cref="T:System.Decimal" /> object.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual decimal ReadContentAsDecimal()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsDecimal");
		}
		try
		{
			return XmlConvert.ToDecimal(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Decimal", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a 32-bit signed integer.</summary>
	/// <returns>The text content as a 32-bit signed integer.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadContentAsInt()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsInt");
		}
		try
		{
			return XmlConvert.ToInt32(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Int", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a 64-bit signed integer.</summary>
	/// <returns>The text content as a 64-bit signed integer.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual long ReadContentAsLong()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsLong");
		}
		try
		{
			return XmlConvert.ToInt64(InternalReadContentAsString());
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", "Long", innerException, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the text content at the current position as a <see cref="T:System.String" /> object.</summary>
	/// <returns>The text content as a <see cref="T:System.String" /> object.</returns>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.FormatException">The string format is not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string ReadContentAsString()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsString");
		}
		return InternalReadContentAsString();
	}

	/// <summary>Reads the content as an object of the type specified.</summary>
	/// <returns>The concatenated text content or attribute value converted to the requested type.</returns>
	/// <param name="returnType">The type of the value to be returned.Note   With the release of the .NET Framework 3.5, the value of the <paramref name="returnType" /> parameter can now be the <see cref="T:System.DateTimeOffset" /> type.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion. For example, this can be used when converting an <see cref="T:System.Xml.XmlQualifiedName" /> object to an xs:string.This value can be null.</param>
	/// <exception cref="T:System.FormatException">The content is not in the correct format for the target type.</exception>
	/// <exception cref="T:System.InvalidCastException">The attempted cast is not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="returnType" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node is not a supported node type. See the table below for details.</exception>
	/// <exception cref="T:System.OverflowException">Read Decimal.MaxValue.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAs");
		}
		string text = InternalReadContentAsString();
		if (returnType == typeof(string))
		{
			return text;
		}
		try
		{
			return XmlUntypedConverter.Untyped.ChangeType(text, returnType, (namespaceResolver == null) ? (this as IXmlNamespaceResolver) : namespaceResolver);
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", returnType.ToString(), innerException, this as IXmlLineInfo);
		}
		catch (InvalidCastException innerException2)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", returnType.ToString(), innerException2, this as IXmlLineInfo);
		}
	}

	/// <summary>Reads the current element and returns the contents as an <see cref="T:System.Object" />.</summary>
	/// <returns>A boxed common language runtime (CLR) object of the most appropriate type. The <see cref="P:System.Xml.XmlReader.ValueType" /> property determines the appropriate CLR type. If the content is typed as a list type, this method returns an array of boxed objects of the appropriate type.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadElementContentAsObject()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsObject"))
		{
			object result = ReadContentAsObject();
			FinishReadElementContentAsXxx();
			return result;
		}
		return string.Empty;
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as an <see cref="T:System.Object" />.</summary>
	/// <returns>A boxed common language runtime (CLR) object of the most appropriate type. The <see cref="P:System.Xml.XmlReader.ValueType" /> property determines the appropriate CLR type. If the content is typed as a list type, this method returns an array of boxed objects of the appropriate type.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadElementContentAsObject(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsObject();
	}

	/// <summary>Reads the current element and returns the contents as a <see cref="T:System.Boolean" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.Boolean" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.Boolean" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool ReadElementContentAsBoolean()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsBoolean"))
		{
			bool result = ReadContentAsBoolean();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToBoolean(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a <see cref="T:System.Boolean" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.Boolean" /> object.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool ReadElementContentAsBoolean(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsBoolean();
	}

	/// <summary>Reads the current element and returns the contents as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.DateTime" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.DateTime" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual DateTime ReadElementContentAsDateTime()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsDateTime"))
		{
			DateTime result = ReadContentAsDateTime();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToDateTime(string.Empty, XmlDateTimeSerializationMode.RoundtripKind);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a <see cref="T:System.DateTime" /> object.</summary>
	/// <returns>The element contents as a <see cref="T:System.DateTime" /> object.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsDateTime();
	}

	/// <summary>Reads the current element and returns the contents as a double-precision floating-point number.</summary>
	/// <returns>The element content as a double-precision floating-point number.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a double-precision floating-point number.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual double ReadElementContentAsDouble()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsDouble"))
		{
			double result = ReadContentAsDouble();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToDouble(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a double-precision floating-point number.</summary>
	/// <returns>The element content as a double-precision floating-point number.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual double ReadElementContentAsDouble(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsDouble();
	}

	/// <summary>Reads the current element and returns the contents as single-precision floating-point number.</summary>
	/// <returns>The element content as a single-precision floating point number.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a single-precision floating-point number.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual float ReadElementContentAsFloat()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsFloat"))
		{
			float result = ReadContentAsFloat();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToSingle(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a single-precision floating-point number.</summary>
	/// <returns>The element content as a single-precision floating point number.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a single-precision floating-point number.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual float ReadElementContentAsFloat(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsFloat();
	}

	/// <summary>Reads the current element and returns the contents as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.Decimal" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.Decimal" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual decimal ReadElementContentAsDecimal()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsDecimal"))
		{
			decimal result = ReadContentAsDecimal();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToDecimal(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a <see cref="T:System.Decimal" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.Decimal" /> object.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.Decimal" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual decimal ReadElementContentAsDecimal(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsDecimal();
	}

	/// <summary>Reads the current element and returns the contents as a 32-bit signed integer.</summary>
	/// <returns>The element content as a 32-bit signed integer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a 32-bit signed integer.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadElementContentAsInt()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsInt"))
		{
			int result = ReadContentAsInt();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToInt32(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a 32-bit signed integer.</summary>
	/// <returns>The element content as a 32-bit signed integer.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a 32-bit signed integer.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadElementContentAsInt(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsInt();
	}

	/// <summary>Reads the current element and returns the contents as a 64-bit signed integer.</summary>
	/// <returns>The element content as a 64-bit signed integer.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a 64-bit signed integer.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual long ReadElementContentAsLong()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsLong"))
		{
			long result = ReadContentAsLong();
			FinishReadElementContentAsXxx();
			return result;
		}
		return XmlConvert.ToInt64(string.Empty);
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a 64-bit signed integer.</summary>
	/// <returns>The element content as a 64-bit signed integer.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a 64-bit signed integer.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual long ReadElementContentAsLong(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsLong();
	}

	/// <summary>Reads the current element and returns the contents as a <see cref="T:System.String" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.String" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.String" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string ReadElementContentAsString()
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAsString"))
		{
			string result = ReadContentAsString();
			FinishReadElementContentAsXxx();
			return result;
		}
		return string.Empty;
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the current element and returns the contents as a <see cref="T:System.String" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.String" /> object.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to a <see cref="T:System.String" /> object.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string ReadElementContentAsString(string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAsString();
	}

	/// <summary>Reads the element content as the requested type.</summary>
	/// <returns>The element content converted to the requested typed object.</returns>
	/// <param name="returnType">The type of the value to be returned.Note   With the release of the .NET Framework 3.5, the value of the <paramref name="returnType" /> parameter can now be the <see cref="T:System.DateTimeOffset" /> type.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.OverflowException">Read Decimal.MaxValue.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
	{
		if (SetupReadElementContentAsXxx("ReadElementContentAs"))
		{
			object result = ReadContentAs(returnType, namespaceResolver);
			FinishReadElementContentAsXxx();
			return result;
		}
		if (!(returnType == typeof(string)))
		{
			return XmlUntypedConverter.Untyped.ChangeType(string.Empty, returnType, namespaceResolver);
		}
		return string.Empty;
	}

	/// <summary>Checks that the specified local name and namespace URI matches that of the current element, then reads the element content as the requested type.</summary>
	/// <returns>The element content converted to the requested typed object.</returns>
	/// <param name="returnType">The type of the value to be returned.Note   With the release of the .NET Framework 3.5, the value of the <paramref name="returnType" /> parameter can now be the <see cref="T:System.DateTimeOffset" /> type.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion.</param>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on an element.</exception>
	/// <exception cref="T:System.Xml.XmlException">The current element contains child elements.-or-The element content cannot be converted to the requested type.</exception>
	/// <exception cref="T:System.ArgumentNullException">The method is called with null arguments.</exception>
	/// <exception cref="T:System.ArgumentException">The specified local name and namespace URI do not match that of the current element being read.</exception>
	/// <exception cref="T:System.OverflowException">Read Decimal.MaxValue.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
	{
		CheckElement(localName, namespaceURI);
		return ReadElementContentAs(returnType, namespaceResolver);
	}

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned.</returns>
	/// <param name="name">The qualified name of the attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string GetAttribute(string name);

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned. This method does not move the reader.</returns>
	/// <param name="name">The local name of the attribute.</param>
	/// <param name="namespaceURI">The namespace URI of the attribute.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="name" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string GetAttribute(string name, string namespaceURI);

	/// <summary>When overridden in a derived class, gets the value of the attribute with the specified index.</summary>
	/// <returns>The value of the specified attribute. This method does not move the reader.</returns>
	/// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.)</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="i" /> is out of range. It must be non-negative and less than the size of the attribute collection.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string GetAttribute(int i);

	/// <summary>When overridden in a derived class, moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="name">The qualified name of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentException">The parameter is an empty string.</exception>
	public abstract bool MoveToAttribute(string name);

	/// <summary>When overridden in a derived class, moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="name">The local name of the attribute.</param>
	/// <param name="ns">The namespace URI of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentNullException">Both parameter values are null.</exception>
	public abstract bool MoveToAttribute(string name, string ns);

	/// <summary>When overridden in a derived class, moves to the attribute with the specified index.</summary>
	/// <param name="i">The index of the attribute.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The parameter has a negative value.</exception>
	public virtual void MoveToAttribute(int i)
	{
		if (i < 0 || i >= AttributeCount)
		{
			throw new ArgumentOutOfRangeException("i");
		}
		MoveToElement();
		MoveToFirstAttribute();
		for (int j = 0; j < i; j++)
		{
			MoveToNextAttribute();
		}
	}

	/// <summary>When overridden in a derived class, moves to the first attribute.</summary>
	/// <returns>true if an attribute exists (the reader moves to the first attribute); otherwise, false (the position of the reader does not change).</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool MoveToFirstAttribute();

	/// <summary>When overridden in a derived class, moves to the next attribute.</summary>
	/// <returns>true if there is a next attribute; false if there are no more attributes.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool MoveToNextAttribute();

	/// <summary>When overridden in a derived class, moves to the element that contains the current attribute node.</summary>
	/// <returns>true if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); false if the reader is not positioned on an attribute (the position of the reader does not change).</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool MoveToElement();

	/// <summary>When overridden in a derived class, parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.</summary>
	/// <returns>true if there are nodes to return.false if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read.An empty attribute, such as, misc="", returns true with a single node with a value of String.Empty.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool ReadAttributeValue();

	/// <summary>When overridden in a derived class, reads the next node from the stream.</summary>
	/// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
	/// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract bool Read();

	/// <summary>When overridden in a derived class, changes the <see cref="P:System.Xml.XmlReader.ReadState" /> to Closed.</summary>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void Close()
	{
	}

	/// <summary>Skips the children of the current node.</summary>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void Skip()
	{
		if (ReadState == ReadState.Interactive)
		{
			SkipSubtree();
		}
	}

	/// <summary>When overridden in a derived class, resolves a namespace prefix in the current element's scope.</summary>
	/// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string. </param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract string LookupNamespace(string prefix);

	/// <summary>When overridden in a derived class, resolves the entity reference for EntityReference nodes.</summary>
	/// <exception cref="T:System.InvalidOperationException">The reader is not positioned on an EntityReference node; this implementation of the reader cannot resolve entities (<see cref="P:System.Xml.XmlReader.CanResolveEntity" /> returns false).</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public abstract void ResolveEntity();

	/// <summary>Reads the content and returns the Base64 decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlReader.ReadContentAsBase64(System.Byte[],System.Int32,System.Int32)" /> is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadContentAsBase64(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadContentAsBase64"));
	}

	/// <summary>Reads the element and decodes the Base64 content.</summary>
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
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadElementContentAsBase64(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadElementContentAsBase64"));
	}

	/// <summary>Reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlReader.ReadContentAsBinHex(System.Byte[],System.Int32,System.Int32)" /> is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadContentAsBinHex(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadContentAsBinHex"));
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
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadElementContentAsBinHex"));
	}

	/// <summary>Reads large streams of text embedded in an XML document.</summary>
	/// <returns>The number of characters read into the buffer. The value zero is returned when there is no more text content.</returns>
	/// <param name="buffer">The array of characters that serves as the buffer to which the text contents are written. This value cannot be null.</param>
	/// <param name="index">The offset within the buffer where the <see cref="T:System.Xml.XmlReader" /> can start to copy the results.</param>
	/// <param name="count">The maximum number of characters to copy into the buffer. The actual number of characters copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">The current node does not have a value (<see cref="P:System.Xml.XmlReader.HasValue" /> is false).</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer, or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Xml.XmlReader" /> implementation does not support this method.</exception>
	/// <exception cref="T:System.Xml.XmlException">The XML data is not well-formed.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual int ReadValueChunk(char[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("ReadValueChunk method is not supported on this XmlReader. Use CanReadValueChunk property to find out if an XmlReader implements it."));
	}

	/// <summary>When overridden in a derived class, reads the contents of an element or text node as a string.</summary>
	/// <returns>The contents of the element or an empty string.</returns>
	/// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual string ReadString()
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		MoveToElement();
		if (NodeType == XmlNodeType.Element)
		{
			if (IsEmptyElement)
			{
				return string.Empty;
			}
			if (!Read())
			{
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current state of the object."));
			}
			if (NodeType == XmlNodeType.EndElement)
			{
				return string.Empty;
			}
		}
		string text = string.Empty;
		while (IsTextualNode(NodeType))
		{
			text += Value;
			if (!Read())
			{
				break;
			}
		}
		return text;
	}

	/// <summary>Checks whether the current node is a content (non-white space text, CDATA, Element, EndElement, EntityReference, or EndEntity) node. If the node is not a content node, the reader skips ahead to the next content node or end of file. It skips over nodes of the following type: ProcessingInstruction, DocumentType, Comment, Whitespace, or SignificantWhitespace.</summary>
	/// <returns>The <see cref="P:System.Xml.XmlReader.NodeType" /> of the current node found by the method or XmlNodeType.None if the reader has reached the end of the input stream.</returns>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual XmlNodeType MoveToContent()
	{
		do
		{
			switch (NodeType)
			{
			case XmlNodeType.Attribute:
				MoveToElement();
				break;
			case XmlNodeType.Element:
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.EntityReference:
			case XmlNodeType.EndElement:
			case XmlNodeType.EndEntity:
				break;
			default:
				continue;
			}
			return NodeType;
		}
		while (Read());
		return NodeType;
	}

	/// <summary>Checks that the current node is an element and advances the reader to the next node.</summary>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML was encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void ReadStartElement()
	{
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		Read();
	}

	/// <summary>Checks that the current content node is an element with the given <see cref="P:System.Xml.XmlReader.Name" /> and advances the reader to the next node.</summary>
	/// <param name="name">The qualified name of the element.</param>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML was encountered in the input stream. -or- The <see cref="P:System.Xml.XmlReader.Name" /> of the element does not match the given <paramref name="name" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void ReadStartElement(string name)
	{
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (Name == name)
		{
			Read();
			return;
		}
		throw new XmlException("Element '{0}' was not found.", name, this as IXmlLineInfo);
	}

	/// <summary>Checks that the current content node is an element with the given <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> and advances the reader to the next node.</summary>
	/// <param name="localname">The local name of the element.</param>
	/// <param name="ns">The namespace URI of the element.</param>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML was encountered in the input stream.-or-The <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> properties of the element found do not match the given arguments.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void ReadStartElement(string localname, string ns)
	{
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (LocalName == localname && NamespaceURI == ns)
		{
			Read();
			return;
		}
		throw new XmlException("Element '{0}' with namespace name '{1}' was not found.", new string[2] { localname, ns }, this as IXmlLineInfo);
	}

	/// <summary>Reads a text-only element.</summary>
	/// <returns>The text contained in the element that was read. An empty string if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
	/// <exception cref="T:System.Xml.XmlException">The next content node is not a start tag; or the element found does not contain a simple text value.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual string ReadElementString()
	{
		string result = string.Empty;
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (!IsEmptyElement)
		{
			Read();
			result = ReadString();
			if (NodeType != XmlNodeType.EndElement)
			{
				throw new XmlException("Unexpected node type {0}. {1} method can only be called on elements with simple or empty content.", new string[2]
				{
					NodeType.ToString(),
					"ReadElementString"
				}, this as IXmlLineInfo);
			}
			Read();
		}
		else
		{
			Read();
		}
		return result;
	}

	/// <summary>Checks that the <see cref="P:System.Xml.XmlReader.Name" /> property of the element found matches the given string before reading a text-only element.</summary>
	/// <returns>The text contained in the element that was read. An empty string if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
	/// <param name="name">The name to check.</param>
	/// <exception cref="T:System.Xml.XmlException">If the next content node is not a start tag; if the element Name does not match the given argument; or if the element found does not contain a simple text value.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual string ReadElementString(string name)
	{
		string result = string.Empty;
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (Name != name)
		{
			throw new XmlException("Element '{0}' was not found.", name, this as IXmlLineInfo);
		}
		if (!IsEmptyElement)
		{
			result = ReadString();
			if (NodeType != XmlNodeType.EndElement)
			{
				throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
			}
			Read();
		}
		else
		{
			Read();
		}
		return result;
	}

	/// <summary>Checks that the <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> properties of the element found matches the given strings before reading a text-only element.</summary>
	/// <returns>The text contained in the element that was read. An empty string if the element is empty (&lt;item&gt;&lt;/item&gt; or &lt;item/&gt;).</returns>
	/// <param name="localname">The local name to check.</param>
	/// <param name="ns">The namespace URI to check.</param>
	/// <exception cref="T:System.Xml.XmlException">If the next content node is not a start tag; if the element LocalName or NamespaceURI do not match the given arguments; or if the element found does not contain a simple text value.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual string ReadElementString(string localname, string ns)
	{
		string result = string.Empty;
		if (MoveToContent() != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (LocalName != localname || NamespaceURI != ns)
		{
			throw new XmlException("Element '{0}' with namespace name '{1}' was not found.", new string[2] { localname, ns }, this as IXmlLineInfo);
		}
		if (!IsEmptyElement)
		{
			result = ReadString();
			if (NodeType != XmlNodeType.EndElement)
			{
				throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
			}
			Read();
		}
		else
		{
			Read();
		}
		return result;
	}

	/// <summary>Checks that the current content node is an end tag and advances the reader to the next node.</summary>
	/// <exception cref="T:System.Xml.XmlException">The current node is not an end tag or if incorrect XML is encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual void ReadEndElement()
	{
		if (MoveToContent() != XmlNodeType.EndElement)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		Read();
	}

	/// <summary>Calls <see cref="M:System.Xml.XmlReader.MoveToContent" /> and tests if the current content node is a start tag or empty element tag.</summary>
	/// <returns>true if <see cref="M:System.Xml.XmlReader.MoveToContent" /> finds a start tag or empty element tag; false if a node type other than XmlNodeType.Element was found.</returns>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML is encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool IsStartElement()
	{
		return MoveToContent() == XmlNodeType.Element;
	}

	/// <summary>Calls <see cref="M:System.Xml.XmlReader.MoveToContent" /> and tests if the current content node is a start tag or empty element tag and if the <see cref="P:System.Xml.XmlReader.Name" /> property of the element found matches the given argument.</summary>
	/// <returns>true if the resulting node is an element and the Name property matches the specified string. false if a node type other than XmlNodeType.Element was found or if the element Name property does not match the specified string.</returns>
	/// <param name="name">The string matched against the Name property of the element found.</param>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML is encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool IsStartElement(string name)
	{
		if (MoveToContent() == XmlNodeType.Element)
		{
			return Name == name;
		}
		return false;
	}

	/// <summary>Calls <see cref="M:System.Xml.XmlReader.MoveToContent" /> and tests if the current content node is a start tag or empty element tag and if the <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" /> properties of the element found match the given strings.</summary>
	/// <returns>true if the resulting node is an element. false if a node type other than XmlNodeType.Element was found or if the LocalName and NamespaceURI properties of the element do not match the specified strings.</returns>
	/// <param name="localname">The string to match against the LocalName property of the element found.</param>
	/// <param name="ns">The string to match against the NamespaceURI property of the element found.</param>
	/// <exception cref="T:System.Xml.XmlException">Incorrect XML is encountered in the input stream.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual bool IsStartElement(string localname, string ns)
	{
		if (MoveToContent() == XmlNodeType.Element)
		{
			if (LocalName == localname)
			{
				return NamespaceURI == ns;
			}
			return false;
		}
		return false;
	}

	/// <summary>Reads until an element with the specified qualified name is found.</summary>
	/// <returns>true if a matching element is found; otherwise false and the <see cref="T:System.Xml.XmlReader" /> is in an end of file state.</returns>
	/// <param name="name">The qualified name of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentException">The parameter is an empty string.</exception>
	public virtual bool ReadToFollowing(string name)
	{
		if (name == null || name.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
		}
		name = NameTable.Add(name);
		while (Read())
		{
			if (NodeType == XmlNodeType.Element && Ref.Equal(name, Name))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Reads until an element with the specified local name and namespace URI is found.</summary>
	/// <returns>true if a matching element is found; otherwise false and the <see cref="T:System.Xml.XmlReader" /> is in an end of file state.</returns>
	/// <param name="localName">The local name of the element.</param>
	/// <param name="namespaceURI">The namespace URI of the element.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentNullException">Both parameter values are null.</exception>
	public virtual bool ReadToFollowing(string localName, string namespaceURI)
	{
		if (localName == null || localName.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
		}
		if (namespaceURI == null)
		{
			throw new ArgumentNullException("namespaceURI");
		}
		localName = NameTable.Add(localName);
		namespaceURI = NameTable.Add(namespaceURI);
		while (Read())
		{
			if (NodeType == XmlNodeType.Element && Ref.Equal(localName, LocalName) && Ref.Equal(namespaceURI, NamespaceURI))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Advances the <see cref="T:System.Xml.XmlReader" /> to the next descendant element with the specified qualified name.</summary>
	/// <returns>true if a matching descendant element is found; otherwise false. If a matching child element is not found, the <see cref="T:System.Xml.XmlReader" /> is positioned on the end tag (<see cref="P:System.Xml.XmlReader.NodeType" /> is XmlNodeType.EndElement) of the element.If the <see cref="T:System.Xml.XmlReader" /> is not positioned on an element when <see cref="M:System.Xml.XmlReader.ReadToDescendant(System.String)" /> was called, this method returns false and the position of the <see cref="T:System.Xml.XmlReader" /> is not changed.</returns>
	/// <param name="name">The qualified name of the element you wish to move to.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentException">The parameter is an empty string.</exception>
	public virtual bool ReadToDescendant(string name)
	{
		if (name == null || name.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
		}
		int num = Depth;
		if (NodeType != XmlNodeType.Element)
		{
			if (ReadState != 0)
			{
				return false;
			}
			num--;
		}
		else if (IsEmptyElement)
		{
			return false;
		}
		name = NameTable.Add(name);
		while (Read() && Depth > num)
		{
			if (NodeType == XmlNodeType.Element && Ref.Equal(name, Name))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Advances the <see cref="T:System.Xml.XmlReader" /> to the next descendant element with the specified local name and namespace URI.</summary>
	/// <returns>true if a matching descendant element is found; otherwise false. If a matching child element is not found, the <see cref="T:System.Xml.XmlReader" /> is positioned on the end tag (<see cref="P:System.Xml.XmlReader.NodeType" /> is XmlNodeType.EndElement) of the element.If the <see cref="T:System.Xml.XmlReader" /> is not positioned on an element when <see cref="M:System.Xml.XmlReader.ReadToDescendant(System.String,System.String)" /> was called, this method returns false and the position of the <see cref="T:System.Xml.XmlReader" /> is not changed.</returns>
	/// <param name="localName">The local name of the element you wish to move to.</param>
	/// <param name="namespaceURI">The namespace URI of the element you wish to move to.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentNullException">Both parameter values are null.</exception>
	public virtual bool ReadToDescendant(string localName, string namespaceURI)
	{
		if (localName == null || localName.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
		}
		if (namespaceURI == null)
		{
			throw new ArgumentNullException("namespaceURI");
		}
		int num = Depth;
		if (NodeType != XmlNodeType.Element)
		{
			if (ReadState != 0)
			{
				return false;
			}
			num--;
		}
		else if (IsEmptyElement)
		{
			return false;
		}
		localName = NameTable.Add(localName);
		namespaceURI = NameTable.Add(namespaceURI);
		while (Read() && Depth > num)
		{
			if (NodeType == XmlNodeType.Element && Ref.Equal(localName, LocalName) && Ref.Equal(namespaceURI, NamespaceURI))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Advances the XmlReader to the next sibling element with the specified qualified name.</summary>
	/// <returns>true if a matching sibling element is found; otherwise false. If a matching sibling element is not found, the XmlReader is positioned on the end tag (<see cref="P:System.Xml.XmlReader.NodeType" /> is XmlNodeType.EndElement) of the parent element.</returns>
	/// <param name="name">The qualified name of the sibling element you wish to move to.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentException">The parameter is an empty string.</exception>
	public virtual bool ReadToNextSibling(string name)
	{
		if (name == null || name.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
		}
		name = NameTable.Add(name);
		while (SkipSubtree())
		{
			XmlNodeType nodeType = NodeType;
			if (nodeType == XmlNodeType.Element && Ref.Equal(name, Name))
			{
				return true;
			}
			if (nodeType == XmlNodeType.EndElement || EOF)
			{
				break;
			}
		}
		return false;
	}

	/// <summary>Advances the XmlReader to the next sibling element with the specified local name and namespace URI.</summary>
	/// <returns>true if a matching sibling element is found; otherwise, false. If a matching sibling element is not found, the XmlReader is positioned on the end tag (<see cref="P:System.Xml.XmlReader.NodeType" /> is XmlNodeType.EndElement) of the parent element.</returns>
	/// <param name="localName">The local name of the sibling element you wish to move to.</param>
	/// <param name="namespaceURI">The namespace URI of the sibling element you wish to move to.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.ArgumentNullException">Both parameter values are null.</exception>
	public virtual bool ReadToNextSibling(string localName, string namespaceURI)
	{
		if (localName == null || localName.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
		}
		if (namespaceURI == null)
		{
			throw new ArgumentNullException("namespaceURI");
		}
		localName = NameTable.Add(localName);
		namespaceURI = NameTable.Add(namespaceURI);
		while (SkipSubtree())
		{
			XmlNodeType nodeType = NodeType;
			if (nodeType == XmlNodeType.Element && Ref.Equal(localName, LocalName) && Ref.Equal(namespaceURI, NamespaceURI))
			{
				return true;
			}
			if (nodeType == XmlNodeType.EndElement || EOF)
			{
				break;
			}
		}
		return false;
	}

	/// <summary>Returns a value indicating whether the string argument is a valid XML name.</summary>
	/// <returns>true if the name is valid; otherwise, false.</returns>
	/// <param name="str">The name to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="str" /> value is null.</exception>
	public static bool IsName(string str)
	{
		if (str == null)
		{
			throw new NullReferenceException();
		}
		return ValidateNames.IsNameNoNamespaces(str);
	}

	/// <summary>Returns a value indicating whether or not the string argument is a valid XML name token.</summary>
	/// <returns>true if it is a valid name token; otherwise false.</returns>
	/// <param name="str">The name token to validate.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="str" /> value is null.</exception>
	public static bool IsNameToken(string str)
	{
		if (str == null)
		{
			throw new NullReferenceException();
		}
		return ValidateNames.IsNmtokenNoNamespaces(str);
	}

	/// <summary>When overridden in a derived class, reads all the content, including markup, as a string.</summary>
	/// <returns>All the XML content, including markup, in the current node. If the current node has no children, an empty string is returned.If the current node is neither an element nor attribute, an empty string is returned.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML was not well-formed, or an error occurred while parsing the XML.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string ReadInnerXml()
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		if (NodeType != XmlNodeType.Attribute && NodeType != XmlNodeType.Element)
		{
			Read();
			return string.Empty;
		}
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriter xmlWriter = CreateWriterForInnerOuterXml(stringWriter);
		try
		{
			if (NodeType == XmlNodeType.Attribute)
			{
				((XmlTextWriter)xmlWriter).QuoteChar = QuoteChar;
				WriteAttributeValue(xmlWriter);
			}
			if (NodeType == XmlNodeType.Element)
			{
				WriteNode(xmlWriter, defattr: false);
			}
		}
		finally
		{
			xmlWriter.Close();
		}
		return stringWriter.ToString();
	}

	private void WriteNode(XmlWriter xtw, bool defattr)
	{
		int num = ((NodeType == XmlNodeType.None) ? (-1) : Depth);
		while (Read() && num < Depth)
		{
			switch (NodeType)
			{
			case XmlNodeType.Element:
				xtw.WriteStartElement(Prefix, LocalName, NamespaceURI);
				((XmlTextWriter)xtw).QuoteChar = QuoteChar;
				xtw.WriteAttributes(this, defattr);
				if (IsEmptyElement)
				{
					xtw.WriteEndElement();
				}
				break;
			case XmlNodeType.Text:
				xtw.WriteString(Value);
				break;
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				xtw.WriteWhitespace(Value);
				break;
			case XmlNodeType.CDATA:
				xtw.WriteCData(Value);
				break;
			case XmlNodeType.EntityReference:
				xtw.WriteEntityRef(Name);
				break;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.XmlDeclaration:
				xtw.WriteProcessingInstruction(Name, Value);
				break;
			case XmlNodeType.DocumentType:
				xtw.WriteDocType(Name, GetAttribute("PUBLIC"), GetAttribute("SYSTEM"), Value);
				break;
			case XmlNodeType.Comment:
				xtw.WriteComment(Value);
				break;
			case XmlNodeType.EndElement:
				xtw.WriteFullEndElement();
				break;
			}
		}
		if (num == Depth && NodeType == XmlNodeType.EndElement)
		{
			Read();
		}
	}

	private void WriteAttributeValue(XmlWriter xtw)
	{
		string name = Name;
		while (ReadAttributeValue())
		{
			if (NodeType == XmlNodeType.EntityReference)
			{
				xtw.WriteEntityRef(Name);
			}
			else
			{
				xtw.WriteString(Value);
			}
		}
		MoveToAttribute(name);
	}

	/// <summary>When overridden in a derived class, reads the content, including markup, representing this node and all its children.</summary>
	/// <returns>If the reader is positioned on an element or an attribute node, this method returns all the XML content, including markup, of the current node and all its children; otherwise, it returns an empty string.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML was not well-formed, or an error occurred while parsing the XML.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual string ReadOuterXml()
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		if (NodeType != XmlNodeType.Attribute && NodeType != XmlNodeType.Element)
		{
			Read();
			return string.Empty;
		}
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriter xmlWriter = CreateWriterForInnerOuterXml(stringWriter);
		try
		{
			if (NodeType == XmlNodeType.Attribute)
			{
				xmlWriter.WriteStartAttribute(Prefix, LocalName, NamespaceURI);
				WriteAttributeValue(xmlWriter);
				xmlWriter.WriteEndAttribute();
			}
			else
			{
				xmlWriter.WriteNode(this, defattr: false);
			}
		}
		finally
		{
			xmlWriter.Close();
		}
		return stringWriter.ToString();
	}

	private XmlWriter CreateWriterForInnerOuterXml(StringWriter sw)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
		SetNamespacesFlag(xmlTextWriter);
		return xmlTextWriter;
	}

	private void SetNamespacesFlag(XmlTextWriter xtw)
	{
		if (this is XmlTextReader xmlTextReader)
		{
			xtw.Namespaces = xmlTextReader.Namespaces;
		}
		else if (this is XmlValidatingReader xmlValidatingReader)
		{
			xtw.Namespaces = xmlValidatingReader.Namespaces;
		}
	}

	/// <summary>Returns a new XmlReader instance that can be used to read the current node, and all its descendants.</summary>
	/// <returns>A new XmlReader instance set to ReadState.Initial. A call to the <see cref="M:System.Xml.XmlReader.Read" /> method positions the new XmlReader on the node that was current before the call to ReadSubtree method.</returns>
	/// <exception cref="T:System.InvalidOperationException">The XmlReader is not positioned on an element when this method is called.</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public virtual XmlReader ReadSubtree()
	{
		if (NodeType != XmlNodeType.Element)
		{
			throw new InvalidOperationException(Res.GetString("ReadSubtree() can be called only if the reader is on an element node."));
		}
		return new XmlSubtreeReader(this);
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Xml.XmlReader" /> class.</summary>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Xml.XmlReader" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && ReadState != ReadState.Closed)
		{
			Close();
		}
	}

	internal static bool IsTextualNode(XmlNodeType nodeType)
	{
		return (IsTextualNodeBitmap & (1 << (int)nodeType)) != 0;
	}

	internal static bool CanReadContentAs(XmlNodeType nodeType)
	{
		return (CanReadContentAsBitmap & (1 << (int)nodeType)) != 0;
	}

	internal static bool HasValueInternal(XmlNodeType nodeType)
	{
		return (HasValueBitmap & (1 << (int)nodeType)) != 0;
	}

	private bool SkipSubtree()
	{
		MoveToElement();
		if (NodeType == XmlNodeType.Element && !IsEmptyElement)
		{
			int depth = Depth;
			while (Read() && depth < Depth)
			{
			}
			if (NodeType == XmlNodeType.EndElement)
			{
				return Read();
			}
			return false;
		}
		return Read();
	}

	internal void CheckElement(string localName, string namespaceURI)
	{
		if (localName == null || localName.Length == 0)
		{
			throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
		}
		if (namespaceURI == null)
		{
			throw new ArgumentNullException("namespaceURI");
		}
		if (NodeType != XmlNodeType.Element)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString(), this as IXmlLineInfo);
		}
		if (LocalName != localName || NamespaceURI != namespaceURI)
		{
			throw new XmlException("Element '{0}' with namespace name '{1}' was not found.", new string[2] { localName, namespaceURI }, this as IXmlLineInfo);
		}
	}

	internal Exception CreateReadContentAsException(string methodName)
	{
		return CreateReadContentAsException(methodName, NodeType, this as IXmlLineInfo);
	}

	internal Exception CreateReadElementContentAsException(string methodName)
	{
		return CreateReadElementContentAsException(methodName, NodeType, this as IXmlLineInfo);
	}

	internal bool CanReadContentAs()
	{
		return CanReadContentAs(NodeType);
	}

	internal static Exception CreateReadContentAsException(string methodName, XmlNodeType nodeType, IXmlLineInfo lineInfo)
	{
		return new InvalidOperationException(AddLineInfo(Res.GetString("The {0} method is not supported on node type {1}. If you want to read typed content of an element, use the ReadElementContentAs method.", methodName, nodeType.ToString()), lineInfo));
	}

	internal static Exception CreateReadElementContentAsException(string methodName, XmlNodeType nodeType, IXmlLineInfo lineInfo)
	{
		return new InvalidOperationException(AddLineInfo(Res.GetString("The {0} method is not supported on node type {1}.", methodName, nodeType.ToString()), lineInfo));
	}

	private static string AddLineInfo(string message, IXmlLineInfo lineInfo)
	{
		if (lineInfo != null)
		{
			message = message + " " + Res.GetString("Line {0}, position {1}.", lineInfo.LineNumber.ToString(CultureInfo.InvariantCulture), lineInfo.LinePosition.ToString(CultureInfo.InvariantCulture));
		}
		return message;
	}

	internal string InternalReadContentAsString()
	{
		string text = string.Empty;
		StringBuilder stringBuilder = null;
		bool num;
		do
		{
			switch (NodeType)
			{
			case XmlNodeType.Attribute:
				return Value;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				if (text.Length == 0)
				{
					text = Value;
				}
				else
				{
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder();
						stringBuilder.Append(text);
					}
					stringBuilder.Append(Value);
				}
				goto case XmlNodeType.ProcessingInstruction;
			case XmlNodeType.EntityReference:
				if (!CanResolveEntity)
				{
					break;
				}
				ResolveEntity();
				goto case XmlNodeType.ProcessingInstruction;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			case XmlNodeType.EndEntity:
				num = ((AttributeCount != 0) ? ReadAttributeValue() : Read());
				continue;
			}
			break;
		}
		while (num);
		if (stringBuilder != null)
		{
			return stringBuilder.ToString();
		}
		return text;
	}

	private bool SetupReadElementContentAsXxx(string methodName)
	{
		if (NodeType != XmlNodeType.Element)
		{
			throw CreateReadElementContentAsException(methodName);
		}
		bool isEmptyElement = IsEmptyElement;
		Read();
		if (isEmptyElement)
		{
			return false;
		}
		switch (NodeType)
		{
		case XmlNodeType.EndElement:
			Read();
			return false;
		case XmlNodeType.Element:
			throw new XmlException("ReadElementContentAs() methods cannot be called on an element that has child elements.", string.Empty, this as IXmlLineInfo);
		default:
			return true;
		}
	}

	private void FinishReadElementContentAsXxx()
	{
		if (NodeType != XmlNodeType.EndElement)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString());
		}
		Read();
	}

	internal static Encoding GetEncoding(XmlReader reader)
	{
		return GetXmlTextReaderImpl(reader)?.Encoding;
	}

	internal static ConformanceLevel GetV1ConformanceLevel(XmlReader reader)
	{
		return GetXmlTextReaderImpl(reader)?.V1ComformanceLevel ?? ConformanceLevel.Document;
	}

	private static XmlTextReaderImpl GetXmlTextReaderImpl(XmlReader reader)
	{
		if (reader is XmlTextReaderImpl result)
		{
			return result;
		}
		if (reader is XmlTextReader xmlTextReader)
		{
			return xmlTextReader.Impl;
		}
		if (reader is XmlValidatingReaderImpl xmlValidatingReaderImpl)
		{
			return xmlValidatingReaderImpl.ReaderImpl;
		}
		if (reader is XmlValidatingReader xmlValidatingReader)
		{
			return xmlValidatingReader.Impl.ReaderImpl;
		}
		return null;
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance with specified URI.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read the XML data.</returns>
	/// <param name="inputUri">The URI for the file containing the XML data. The <see cref="T:System.Xml.XmlUrlResolver" /> class is used to convert the path to a canonical data representation.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="inputUri" /> value is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Xml.XmlReader" /> does not have sufficient permissions to access the location of the XML data.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file identified by the URI does not exist.</exception>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI format is not correct.</exception>
	public static XmlReader Create(string inputUri)
	{
		return Create(inputUri, null, null);
	}

	/// <summary>Creates a new instance with the specified URI and <see cref="T:System.Xml.XmlReaderSettings" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="inputUri">The URI for the file containing the XML data. The <see cref="T:System.Xml.XmlResolver" /> object on the <see cref="T:System.Xml.XmlReaderSettings" /> object is used to convert the path to a canonical data representation. If <see cref="P:System.Xml.XmlReaderSettings.XmlResolver" /> is null, a new <see cref="T:System.Xml.XmlUrlResolver" /> object is used.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="inputUri" /> value is null.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by the URI cannot be found.</exception>
	/// <exception cref="T:System.UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI format is not correct.</exception>
	public static XmlReader Create(string inputUri, XmlReaderSettings settings)
	{
		return Create(inputUri, settings, null);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified URI, <see cref="T:System.Xml.XmlReaderSettings" />, and <see cref="T:System.Xml.XmlParserContext" /> objects.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="inputUri">The URI for the file containing the XML data. The <see cref="T:System.Xml.XmlResolver" /> object on the <see cref="T:System.Xml.XmlReaderSettings" /> object is used to convert the path to a canonical data representation. If <see cref="P:System.Xml.XmlReaderSettings.XmlResolver" /> is null, a new <see cref="T:System.Xml.XmlUrlResolver" /> object is used.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <param name="inputContext">The <see cref="T:System.Xml.XmlParserContext" /> object that provides the context information required to parse the XML fragment. The context information can include the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, the current xml:lang and xml:space scope, base URI, and document type definition. This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The inputUri value is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Xml.XmlReader" /> does not have sufficient permissions to access the location of the XML data.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Xml.XmlReaderSettings.NameTable" />  and <see cref="P:System.Xml.XmlParserContext.NameTable" /> properties both contain values. (Only one of these NameTable properties can be set and used).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by the URI cannot be found.</exception>
	/// <exception cref="T:System.UriFormatException">The URI format is not correct.</exception>
	public static XmlReader Create(string inputUri, XmlReaderSettings settings, XmlParserContext inputContext)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(inputUri, inputContext);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified stream.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object used to read the data contained in the stream.</returns>
	/// <param name="input">The stream containing the XML data.The <see cref="T:System.Xml.XmlReader" /> scans the first bytes of the stream looking for a byte order mark or other sign of encoding. When encoding is determined, the encoding is used to continue reading the stream, and processing continues parsing the input as a stream of (Unicode) characters.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The <see cref="T:System.Xml.XmlReader" /> does not have sufficient permissions to access the location of the XML data.</exception>
	public static XmlReader Create(Stream input)
	{
		return Create(input, null, string.Empty);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance with the specified stream and <see cref="T:System.Xml.XmlReaderSettings" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read the XML data.</returns>
	/// <param name="input">The stream containing the XML data.The <see cref="T:System.Xml.XmlReader" /> scans the first bytes of the stream looking for a byte order mark or other sign of encoding. When encoding is determined, the encoding is used to continue reading the stream, and processing continues parsing the input as a stream of (Unicode) characters.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(Stream input, XmlReaderSettings settings)
	{
		return Create(input, settings, string.Empty);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified stream, base URI, and <see cref="T:System.Xml.XmlReaderSettings" /> object.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="input">The stream containing the XML data. The <see cref="T:System.Xml.XmlReader" /> scans the first bytes of the stream looking for a byte order mark or other sign of encoding. When encoding is determined, the encoding is used to continue reading the stream, and processing continues parsing the input as a stream of (Unicode) characters.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <param name="baseUri">The base URI for the entity or document being read. This value can be null.Security Note   The base URI is used to resolve the relative URI of the XML document. Do not use a base URI from an untrusted source.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(input, null, baseUri, null);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified stream, <see cref="T:System.Xml.XmlReaderSettings" />, and <see cref="T:System.Xml.XmlParserContext" /> objects.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="input">The stream containing the XML data. The <see cref="T:System.Xml.XmlReader" /> scans the first bytes of the stream looking for a byte order mark or other sign of encoding. When encoding is determined, the encoding is used to continue reading the stream, and processing continues parsing the input as a stream of (Unicode) characters.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <param name="inputContext">The <see cref="T:System.Xml.XmlParserContext" /> object that provides the context information required to parse the XML fragment. The context information can include the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, the current xml:lang and xml:space scope, base URI, and document type definition. This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(Stream input, XmlReaderSettings settings, XmlParserContext inputContext)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(input, null, string.Empty, inputContext);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance with the specified <see cref="T:System.IO.TextReader" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read the XML data.</returns>
	/// <param name="input">The <see cref="T:System.IO.TextReader" /> from which to read the XML data. Because a <see cref="T:System.IO.TextReader" /> returns a stream of Unicode characters, the encoding specified in the XML declaration is not used by the <see cref="T:System.Xml.XmlReader" /> to decode the data stream.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(TextReader input)
	{
		return Create(input, null, string.Empty);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified <see cref="T:System.IO.TextReader" /> and <see cref="T:System.Xml.XmlReaderSettings" /> objects.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="input">The <see cref="T:System.IO.TextReader" /> from which to read the XML data. Because a <see cref="T:System.IO.TextReader" /> returns a stream of Unicode characters, the encoding specified in the XML declaration is not used by the <see cref="T:System.Xml.XmlReader" /> to decode the data stream</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" />. This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(TextReader input, XmlReaderSettings settings)
	{
		return Create(input, settings, string.Empty);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified <see cref="T:System.IO.TextReader" />, <see cref="T:System.Xml.XmlReaderSettings" />, and base URI.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="input">The <see cref="T:System.IO.TextReader" /> from which to read the XML data. Because a <see cref="T:System.IO.TextReader" /> returns a stream of Unicode characters, the encoding specified in the XML declaration is not used by the <see cref="T:System.Xml.XmlReader" /> to decode the data stream.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <param name="baseUri">The base URI for the entity or document being read. This value can be null.Security Note   The base URI is used to resolve the relative URI of the XML document. Do not use a base URI from an untrusted source.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	public static XmlReader Create(TextReader input, XmlReaderSettings settings, string baseUri)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(input, baseUri, null);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance using the specified <see cref="T:System.IO.TextReader" />, <see cref="T:System.Xml.XmlReaderSettings" />, and <see cref="T:System.Xml.XmlParserContext" /> objects.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object to read XML data.</returns>
	/// <param name="input">The <see cref="T:System.IO.TextReader" /> from which to read the XML data. Because a <see cref="T:System.IO.TextReader" /> returns a stream of Unicode characters, the encoding specified in the XML declaration is not used by the <see cref="T:System.Xml.XmlReader" /> to decode the data stream.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance. This value can be null.</param>
	/// <param name="inputContext">The <see cref="T:System.Xml.XmlParserContext" /> object that provides the context information required to parse the XML fragment. The context information can include the <see cref="T:System.Xml.XmlNameTable" /> to use, encoding, namespace scope, the current xml:lang and xml:space scope, base URI, and document type definition.This value can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> value is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Xml.XmlReaderSettings.NameTable" />  and <see cref="P:System.Xml.XmlParserContext.NameTable" /> properties both contain values. (Only one of these NameTable properties can be set and used).</exception>
	public static XmlReader Create(TextReader input, XmlReaderSettings settings, XmlParserContext inputContext)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(input, string.Empty, inputContext);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XmlReader" /> instance with the specified <see cref="T:System.Xml.XmlReader" /> and <see cref="T:System.Xml.XmlReaderSettings" /> objects.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object that is wrapped around the specified <see cref="T:System.Xml.XmlReader" /> object.</returns>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> object that you wish to use as the underlying reader.</param>
	/// <param name="settings">The <see cref="T:System.Xml.XmlReaderSettings" /> object used to configure the new <see cref="T:System.Xml.XmlReader" /> instance.The conformance level of the <see cref="T:System.Xml.XmlReaderSettings" /> object must either match the conformance level of the underlying reader, or it must be set to <see cref="F:System.Xml.ConformanceLevel.Auto" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="reader" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">If the <see cref="T:System.Xml.XmlReaderSettings" /> object specifies a conformance level that is not consistent with conformance level of the underlying reader.-or-The underlying <see cref="T:System.Xml.XmlReader" /> is in an <see cref="F:System.Xml.ReadState.Error" /> or <see cref="F:System.Xml.ReadState.Closed" /> state.</exception>
	public static XmlReader Create(XmlReader reader, XmlReaderSettings settings)
	{
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		return settings.CreateReader(reader);
	}

	internal static XmlReader CreateSqlReader(Stream input, XmlReaderSettings settings, XmlParserContext inputContext)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (settings == null)
		{
			settings = new XmlReaderSettings();
		}
		byte[] array = new byte[CalcBufferSize(input)];
		int num = 0;
		int num2;
		do
		{
			num2 = input.Read(array, num, array.Length - num);
			num += num2;
		}
		while (num2 > 0 && num < 2);
		XmlReader xmlReader;
		if (num >= 2 && array[0] == 223 && array[1] == byte.MaxValue)
		{
			if (inputContext != null)
			{
				throw new ArgumentException(Res.GetString("BinaryXml Parser does not support initialization with XmlParserContext."), "inputContext");
			}
			xmlReader = new XmlSqlBinaryReader(input, array, num, string.Empty, settings.CloseInput, settings);
		}
		else
		{
			xmlReader = new XmlTextReaderImpl(input, array, num, settings, null, string.Empty, inputContext, settings.CloseInput);
		}
		if (settings.ValidationType != 0)
		{
			xmlReader = settings.AddValidation(xmlReader);
		}
		if (settings.Async)
		{
			xmlReader = XmlAsyncCheckReader.CreateAsyncCheckWrapper(xmlReader);
		}
		return xmlReader;
	}

	internal static int CalcBufferSize(Stream input)
	{
		int num = 4096;
		if (input.CanSeek)
		{
			long length = input.Length;
			if (length < num)
			{
				num = checked((int)length);
			}
			else if (length > 65536)
			{
				num = 8192;
			}
		}
		return num;
	}

	/// <summary>Asynchronously gets the value of the current node.</summary>
	/// <returns>The value of the current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<string> GetValueAsync()
	{
		throw new NotImplementedException();
	}

	/// <summary>Asynchronously reads the text content at the current position as an <see cref="T:System.Object" />.</summary>
	/// <returns>The text content as the most appropriate common language runtime (CLR) object.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<object> ReadContentAsObjectAsync()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsObject");
		}
		return await InternalReadContentAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>Asynchronously reads the text content at the current position as a <see cref="T:System.String" /> object.</summary>
	/// <returns>The text content as a <see cref="T:System.String" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<string> ReadContentAsStringAsync()
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAsString");
		}
		return InternalReadContentAsStringAsync();
	}

	/// <summary>Asynchronously reads the content as an object of the type specified.</summary>
	/// <returns>The concatenated text content or attribute value converted to the requested type.</returns>
	/// <param name="returnType">The type of the value to be returned.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<object> ReadContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver)
	{
		if (!CanReadContentAs())
		{
			throw CreateReadContentAsException("ReadContentAs");
		}
		string text = await InternalReadContentAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (returnType == typeof(string))
		{
			return text;
		}
		try
		{
			return XmlUntypedConverter.Untyped.ChangeType(text, returnType, (namespaceResolver == null) ? (this as IXmlNamespaceResolver) : namespaceResolver);
		}
		catch (FormatException innerException)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", returnType.ToString(), innerException, this as IXmlLineInfo);
		}
		catch (InvalidCastException innerException2)
		{
			throw new XmlException("Content cannot be converted to the type {0}.", returnType.ToString(), innerException2, this as IXmlLineInfo);
		}
	}

	/// <summary>Asynchronously reads the current element and returns the contents as an <see cref="T:System.Object" />.</summary>
	/// <returns>A boxed common language runtime (CLR) object of the most appropriate type. The <see cref="P:System.Xml.XmlReader.ValueType" /> property determines the appropriate CLR type. If the content is typed as a list type, this method returns an array of boxed objects of the appropriate type.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<object> ReadElementContentAsObjectAsync()
	{
		if (await SetupReadElementContentAsXxxAsync("ReadElementContentAsObject").ConfigureAwait(continueOnCapturedContext: false))
		{
			object value = await ReadContentAsObjectAsync().ConfigureAwait(continueOnCapturedContext: false);
			await FinishReadElementContentAsXxxAsync().ConfigureAwait(continueOnCapturedContext: false);
			return value;
		}
		return string.Empty;
	}

	/// <summary>Asynchronously reads the current element and returns the contents as a <see cref="T:System.String" /> object.</summary>
	/// <returns>The element content as a <see cref="T:System.String" /> object.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<string> ReadElementContentAsStringAsync()
	{
		if (await SetupReadElementContentAsXxxAsync("ReadElementContentAsString").ConfigureAwait(continueOnCapturedContext: false))
		{
			string value = await ReadContentAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
			await FinishReadElementContentAsXxxAsync().ConfigureAwait(continueOnCapturedContext: false);
			return value;
		}
		return string.Empty;
	}

	/// <summary>Asynchronously reads the element content as the requested type.</summary>
	/// <returns>The element content converted to the requested typed object.</returns>
	/// <param name="returnType">The type of the value to be returned.</param>
	/// <param name="namespaceResolver">An <see cref="T:System.Xml.IXmlNamespaceResolver" /> object that is used to resolve any namespace prefixes related to type conversion.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<object> ReadElementContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver)
	{
		if (await SetupReadElementContentAsXxxAsync("ReadElementContentAs").ConfigureAwait(continueOnCapturedContext: false))
		{
			object value = await ReadContentAsAsync(returnType, namespaceResolver).ConfigureAwait(continueOnCapturedContext: false);
			await FinishReadElementContentAsXxxAsync().ConfigureAwait(continueOnCapturedContext: false);
			return value;
		}
		return (returnType == typeof(string)) ? string.Empty : XmlUntypedConverter.Untyped.ChangeType(string.Empty, returnType, namespaceResolver);
	}

	/// <summary>Asynchronously reads the next node from the stream.</summary>
	/// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<bool> ReadAsync()
	{
		throw new NotImplementedException();
	}

	/// <summary>Asynchronously skips the children of the current node.</summary>
	/// <returns>The current node.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task SkipAsync()
	{
		if (ReadState != ReadState.Interactive)
		{
			return AsyncHelper.DoneTask;
		}
		return SkipSubtreeAsync();
	}

	/// <summary>Asynchronously reads the content and returns the Base64 decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<int> ReadContentAsBase64Async(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadContentAsBase64"));
	}

	/// <summary>Asynchronously reads the element and decodes the Base64 content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<int> ReadElementContentAsBase64Async(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadElementContentAsBase64"));
	}

	/// <summary>Asynchronously reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<int> ReadContentAsBinHexAsync(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadContentAsBinHex"));
	}

	/// <summary>Asynchronously reads the element and decodes the BinHex content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<int> ReadElementContentAsBinHexAsync(byte[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("{0} method is not supported on this XmlReader. Use CanReadBinaryContent property to find out if a reader implements it.", "ReadElementContentAsBinHex"));
	}

	/// <summary>Asynchronously reads large streams of text embedded in an XML document.</summary>
	/// <returns>The number of characters read into the buffer. The value zero is returned when there is no more text content.</returns>
	/// <param name="buffer">The array of characters that serves as the buffer to which the text contents are written. This value cannot be null.</param>
	/// <param name="index">The offset within the buffer where the <see cref="T:System.Xml.XmlReader" /> can start to copy the results.</param>
	/// <param name="count">The maximum number of characters to copy into the buffer. The actual number of characters copied is returned from this method.</param>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual Task<int> ReadValueChunkAsync(char[] buffer, int index, int count)
	{
		throw new NotSupportedException(Res.GetString("ReadValueChunk method is not supported on this XmlReader. Use CanReadValueChunk property to find out if an XmlReader implements it."));
	}

	/// <summary>Asynchronously checks whether the current node is a content node. If the node is not a content node, the reader skips ahead to the next content node or end of file.</summary>
	/// <returns>The <see cref="P:System.Xml.XmlReader.NodeType" /> of the current node found by the method or XmlNodeType.None if the reader has reached the end of the input stream.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<XmlNodeType> MoveToContentAsync()
	{
		do
		{
			switch (NodeType)
			{
			case XmlNodeType.Attribute:
				MoveToElement();
				break;
			case XmlNodeType.Element:
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.EntityReference:
			case XmlNodeType.EndElement:
			case XmlNodeType.EndEntity:
				break;
			default:
				continue;
			}
			return NodeType;
		}
		while (await ReadAsync().ConfigureAwait(continueOnCapturedContext: false));
		return NodeType;
	}

	/// <summary>Asynchronously reads all the content, including markup, as a string.</summary>
	/// <returns>All the XML content, including markup, in the current node. If the current node has no children, an empty string is returned.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<string> ReadInnerXmlAsync()
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		if (NodeType != XmlNodeType.Attribute && NodeType != XmlNodeType.Element)
		{
			await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
			return string.Empty;
		}
		StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriter xtw = CreateWriterForInnerOuterXml(sw);
		try
		{
			if (NodeType == XmlNodeType.Attribute)
			{
				((XmlTextWriter)xtw).QuoteChar = QuoteChar;
				WriteAttributeValue(xtw);
			}
			if (NodeType == XmlNodeType.Element)
			{
				await WriteNodeAsync(xtw, defattr: false).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		finally
		{
			xtw.Close();
		}
		return sw.ToString();
	}

	private async Task WriteNodeAsync(XmlWriter xtw, bool defattr)
	{
		int d = ((NodeType == XmlNodeType.None) ? (-1) : Depth);
		while (await ReadAsync().ConfigureAwait(continueOnCapturedContext: false) && d < Depth)
		{
			switch (NodeType)
			{
			case XmlNodeType.Element:
				xtw.WriteStartElement(Prefix, LocalName, NamespaceURI);
				((XmlTextWriter)xtw).QuoteChar = QuoteChar;
				xtw.WriteAttributes(this, defattr);
				if (IsEmptyElement)
				{
					xtw.WriteEndElement();
				}
				break;
			case XmlNodeType.Text:
			{
				XmlWriter xmlWriter = xtw;
				xmlWriter.WriteString(await GetValueAsync().ConfigureAwait(continueOnCapturedContext: false));
				break;
			}
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
			{
				XmlWriter xmlWriter = xtw;
				xmlWriter.WriteWhitespace(await GetValueAsync().ConfigureAwait(continueOnCapturedContext: false));
				break;
			}
			case XmlNodeType.CDATA:
				xtw.WriteCData(Value);
				break;
			case XmlNodeType.EntityReference:
				xtw.WriteEntityRef(Name);
				break;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.XmlDeclaration:
				xtw.WriteProcessingInstruction(Name, Value);
				break;
			case XmlNodeType.DocumentType:
				xtw.WriteDocType(Name, GetAttribute("PUBLIC"), GetAttribute("SYSTEM"), Value);
				break;
			case XmlNodeType.Comment:
				xtw.WriteComment(Value);
				break;
			case XmlNodeType.EndElement:
				xtw.WriteFullEndElement();
				break;
			}
		}
		if (d == Depth && NodeType == XmlNodeType.EndElement)
		{
			await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	/// <summary>Asynchronously reads the content, including markup, representing this node and all its children.</summary>
	/// <returns>If the reader is positioned on an element or an attribute node, this method returns all the XML content, including markup, of the current node and all its children; otherwise, it returns an empty string.</returns>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> method was called before a previous asynchronous operation finished. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “An asynchronous operation is already in progress.”</exception>
	/// <exception cref="T:System.InvalidOperationException">An <see cref="T:System.Xml.XmlReader" /> asynchronous method was called without setting the <see cref="P:System.Xml.XmlReaderSettings.Async" /> flag to true. In this case, <see cref="T:System.InvalidOperationException" /> is thrown with the message “Set XmlReaderSettings.Async to true if you want to use Async Methods.”</exception>
	public virtual async Task<string> ReadOuterXmlAsync()
	{
		if (ReadState != ReadState.Interactive)
		{
			return string.Empty;
		}
		if (NodeType != XmlNodeType.Attribute && NodeType != XmlNodeType.Element)
		{
			await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
			return string.Empty;
		}
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriter xmlWriter = CreateWriterForInnerOuterXml(stringWriter);
		try
		{
			if (NodeType == XmlNodeType.Attribute)
			{
				xmlWriter.WriteStartAttribute(Prefix, LocalName, NamespaceURI);
				WriteAttributeValue(xmlWriter);
				xmlWriter.WriteEndAttribute();
			}
			else
			{
				xmlWriter.WriteNode(this, defattr: false);
			}
		}
		finally
		{
			xmlWriter.Close();
		}
		return stringWriter.ToString();
	}

	private async Task<bool> SkipSubtreeAsync()
	{
		MoveToElement();
		if (NodeType == XmlNodeType.Element && !IsEmptyElement)
		{
			int depth = Depth;
			while (await ReadAsync().ConfigureAwait(continueOnCapturedContext: false) && depth < Depth)
			{
			}
			if (NodeType == XmlNodeType.EndElement)
			{
				return await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
			}
			return false;
		}
		return await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	internal async Task<string> InternalReadContentAsStringAsync()
	{
		string value = string.Empty;
		StringBuilder sb = null;
		bool flag;
		do
		{
			switch (NodeType)
			{
			case XmlNodeType.Attribute:
				return Value;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				if (value.Length == 0)
				{
					value = await GetValueAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					if (sb == null)
					{
						sb = new StringBuilder();
						sb.Append(value);
					}
					StringBuilder stringBuilder = sb;
					stringBuilder.Append(await GetValueAsync().ConfigureAwait(continueOnCapturedContext: false));
				}
				goto case XmlNodeType.ProcessingInstruction;
			case XmlNodeType.EntityReference:
				if (!CanResolveEntity)
				{
					break;
				}
				ResolveEntity();
				goto case XmlNodeType.ProcessingInstruction;
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			case XmlNodeType.EndEntity:
				flag = ((AttributeCount == 0) ? (await ReadAsync().ConfigureAwait(continueOnCapturedContext: false)) : ReadAttributeValue());
				continue;
			}
			break;
		}
		while (flag);
		return (sb == null) ? value : sb.ToString();
	}

	private async Task<bool> SetupReadElementContentAsXxxAsync(string methodName)
	{
		if (NodeType != XmlNodeType.Element)
		{
			throw CreateReadElementContentAsException(methodName);
		}
		bool isEmptyElement = IsEmptyElement;
		await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (isEmptyElement)
		{
			return false;
		}
		switch (NodeType)
		{
		case XmlNodeType.EndElement:
			await ReadAsync().ConfigureAwait(continueOnCapturedContext: false);
			return false;
		case XmlNodeType.Element:
			throw new XmlException("ReadElementContentAs() methods cannot be called on an element that has child elements.", string.Empty, this as IXmlLineInfo);
		default:
			return true;
		}
	}

	private Task FinishReadElementContentAsXxxAsync()
	{
		if (NodeType != XmlNodeType.EndElement)
		{
			throw new XmlException("'{0}' is an invalid XmlNodeType.", NodeType.ToString());
		}
		return ReadAsync();
	}

	/// <summary>Initializes a new instance of the XmlReader class.</summary>
	protected XmlReader()
	{
	}
}
