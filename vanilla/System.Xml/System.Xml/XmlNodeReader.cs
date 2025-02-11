using System.Collections.Generic;
using System.Xml.Schema;

namespace System.Xml;

/// <summary>Represents a reader that provides fast, non-cached forward only access to XML data in an <see cref="T:System.Xml.XmlNode" />.</summary>
public class XmlNodeReader : XmlReader, IXmlNamespaceResolver
{
	private XmlNodeReaderNavigator readerNav;

	private XmlNodeType nodeType;

	private int curDepth;

	private ReadState readState;

	private bool fEOF;

	private bool bResolveEntity;

	private bool bStartFromDocument;

	private bool bInReadBinary;

	private ReadContentAsBinaryHelper readBinaryHelper;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlNodeType" /> values representing the type of the current node.</returns>
	public override XmlNodeType NodeType
	{
		get
		{
			if (!IsInReadingStates())
			{
				return XmlNodeType.None;
			}
			return nodeType;
		}
	}

	/// <summary>Gets the qualified name of the current node.</summary>
	/// <returns>The qualified name of the current node. For example, Name is bk:book for the element &lt;bk:book&gt;.The name returned is dependent on the <see cref="P:System.Xml.XmlNodeReader.NodeType" /> of the node. The following node types return the listed values. All other node types return an empty string.Node Type Name AttributeThe name of the attribute. DocumentTypeThe document type name. ElementThe tag name. EntityReferenceThe name of the entity referenced. ProcessingInstructionThe target of the processing instruction. XmlDeclarationThe literal string xml. </returns>
	public override string Name
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.Name;
		}
	}

	/// <summary>Gets the local name of the current node.</summary>
	/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
	public override string LocalName
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.LocalName;
		}
	}

	/// <summary>Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.</summary>
	/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
	public override string NamespaceURI
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.NamespaceURI;
		}
	}

	/// <summary>Gets the namespace prefix associated with the current node.</summary>
	/// <returns>The namespace prefix associated with the current node.</returns>
	public override string Prefix
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.Prefix;
		}
	}

	/// <summary>Gets a value indicating whether the current node can have a <see cref="P:System.Xml.XmlNodeReader.Value" />.</summary>
	/// <returns>true if the node on which the reader is currently positioned can have a Value; otherwise, false.</returns>
	public override bool HasValue
	{
		get
		{
			if (!IsInReadingStates())
			{
				return false;
			}
			return readerNav.HasValue;
		}
	}

	/// <summary>Gets the text value of the current node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlNodeReader.NodeType" /> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node Type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space between markup in a mixed content model. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
	public override string Value
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.Value;
		}
	}

	/// <summary>Gets the depth of the current node in the XML document.</summary>
	/// <returns>The depth of the current node in the XML document.</returns>
	public override int Depth => curDepth;

	/// <summary>Gets the base URI of the current node.</summary>
	/// <returns>The base URI of the current node.</returns>
	public override string BaseURI => readerNav.BaseURI;

	/// <summary>Gets a value indicating whether this reader can parse and resolve entities.</summary>
	/// <returns>true if the reader can parse and resolve entities; otherwise, false. XmlNodeReader always returns true.</returns>
	public override bool CanResolveEntity => true;

	/// <summary>Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).</summary>
	/// <returns>true if the current node is an element (<see cref="P:System.Xml.XmlNodeReader.NodeType" /> equals XmlNodeType.Element) and it ends with /&gt;; otherwise, false.</returns>
	public override bool IsEmptyElement
	{
		get
		{
			if (!IsInReadingStates())
			{
				return false;
			}
			return readerNav.IsEmptyElement;
		}
	}

	/// <summary>Gets a value indicating whether the current node is an attribute that was generated from the default value defined in the document type definition (DTD) or schema.</summary>
	/// <returns>true if the current node is an attribute whose value was generated from the default value defined in the DTD or schema; false if the attribute value was explicitly set.</returns>
	public override bool IsDefault
	{
		get
		{
			if (!IsInReadingStates())
			{
				return false;
			}
			return readerNav.IsDefault;
		}
	}

	/// <summary>Gets the current xml:space scope.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlSpace" /> values. If no xml:space scope exists, this property defaults to XmlSpace.None.</returns>
	public override XmlSpace XmlSpace
	{
		get
		{
			if (!IsInReadingStates())
			{
				return XmlSpace.None;
			}
			return readerNav.XmlSpace;
		}
	}

	/// <summary>Gets the current xml:lang scope.</summary>
	/// <returns>The current xml:lang scope.</returns>
	public override string XmlLang
	{
		get
		{
			if (!IsInReadingStates())
			{
				return string.Empty;
			}
			return readerNav.XmlLang;
		}
	}

	/// <summary>Gets the schema information that has been assigned to the current node.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object containing the schema information for the current node.</returns>
	public override IXmlSchemaInfo SchemaInfo
	{
		get
		{
			if (!IsInReadingStates())
			{
				return null;
			}
			return readerNav.SchemaInfo;
		}
	}

	/// <summary>Gets the number of attributes on the current node.</summary>
	/// <returns>The number of attributes on the current node. This number includes default attributes.</returns>
	public override int AttributeCount
	{
		get
		{
			if (!IsInReadingStates() || nodeType == XmlNodeType.EndElement)
			{
				return 0;
			}
			return readerNav.AttributeCount;
		}
	}

	/// <summary>Gets a value indicating whether the reader is positioned at the end of the stream.</summary>
	/// <returns>true if the reader is positioned at the end of the stream; otherwise, false.</returns>
	public override bool EOF
	{
		get
		{
			if (readState != ReadState.Closed)
			{
				return fEOF;
			}
			return false;
		}
	}

	/// <summary>Gets the state of the reader.</summary>
	/// <returns>One of the <see cref="T:System.Xml.ReadState" /> values.</returns>
	public override ReadState ReadState => readState;

	/// <summary>Gets a value indicating whether the current node has any attributes.</summary>
	/// <returns>true if the current node has attributes; otherwise, false.</returns>
	public override bool HasAttributes => AttributeCount > 0;

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.</summary>
	/// <returns>The XmlNameTable enabling you to get the atomized version of a string within the node.</returns>
	public override XmlNameTable NameTable => readerNav.NameTable;

	/// <summary>Gets a value indicating whether the <see cref="T:System.Xml.XmlNodeReader" /> implements the binary content read methods.</summary>
	/// <returns>true if the binary content read methods are implemented; otherwise false. The <see cref="T:System.Xml.XmlNodeReader" /> class always returns true.</returns>
	public override bool CanReadBinaryContent => true;

	internal override IDtdInfo DtdInfo => readerNav.Document.DtdSchemaInfo;

	/// <summary>Creates an instance of the XmlNodeReader class using the specified <see cref="T:System.Xml.XmlNode" />.</summary>
	/// <param name="node">The XmlNode you want to read. </param>
	public XmlNodeReader(XmlNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		readerNav = new XmlNodeReaderNavigator(node);
		curDepth = 0;
		readState = ReadState.Initial;
		fEOF = false;
		nodeType = XmlNodeType.None;
		bResolveEntity = false;
		bStartFromDocument = false;
	}

	internal bool IsInReadingStates()
	{
		return readState == ReadState.Interactive;
	}

	/// <summary>Gets the value of the attribute with the specified name.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override string GetAttribute(string name)
	{
		if (!IsInReadingStates())
		{
			return null;
		}
		return readerNav.GetAttribute(name);
	}

	/// <summary>Gets the value of the attribute with the specified local name and namespace URI.</summary>
	/// <returns>The value of the specified attribute. If the attribute is not found, null is returned.</returns>
	/// <param name="name">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public override string GetAttribute(string name, string namespaceURI)
	{
		if (!IsInReadingStates())
		{
			return null;
		}
		string ns = ((namespaceURI == null) ? string.Empty : namespaceURI);
		return readerNav.GetAttribute(name, ns);
	}

	/// <summary>Gets the value of the attribute with the specified index.</summary>
	/// <returns>The value of the specified attribute.</returns>
	/// <param name="attributeIndex">The index of the attribute. The index is zero-based. (The first attribute has index 0.) </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="i" /> parameter is less than 0 or greater than or equal to <see cref="P:System.Xml.XmlNodeReader.AttributeCount" />. </exception>
	public override string GetAttribute(int attributeIndex)
	{
		if (!IsInReadingStates())
		{
			throw new ArgumentOutOfRangeException("attributeIndex");
		}
		return readerNav.GetAttribute(attributeIndex);
	}

	/// <summary>Moves to the attribute with the specified name.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	public override bool MoveToAttribute(string name)
	{
		if (!IsInReadingStates())
		{
			return false;
		}
		readerNav.ResetMove(ref curDepth, ref nodeType);
		if (readerNav.MoveToAttribute(name))
		{
			curDepth++;
			nodeType = readerNav.NodeType;
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
			return true;
		}
		readerNav.RollBackMove(ref curDepth);
		return false;
	}

	/// <summary>Moves to the attribute with the specified local name and namespace URI.</summary>
	/// <returns>true if the attribute is found; otherwise, false. If false, the reader's position does not change.</returns>
	/// <param name="name">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public override bool MoveToAttribute(string name, string namespaceURI)
	{
		if (!IsInReadingStates())
		{
			return false;
		}
		readerNav.ResetMove(ref curDepth, ref nodeType);
		string namespaceURI2 = ((namespaceURI == null) ? string.Empty : namespaceURI);
		if (readerNav.MoveToAttribute(name, namespaceURI2))
		{
			curDepth++;
			nodeType = readerNav.NodeType;
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
			return true;
		}
		readerNav.RollBackMove(ref curDepth);
		return false;
	}

	/// <summary>Moves to the attribute with the specified index.</summary>
	/// <param name="attributeIndex">The index of the attribute. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="i" /> parameter is less than 0 or greater than or equal to <see cref="P:System.Xml.XmlReader.AttributeCount" />. </exception>
	public override void MoveToAttribute(int attributeIndex)
	{
		if (!IsInReadingStates())
		{
			throw new ArgumentOutOfRangeException("attributeIndex");
		}
		readerNav.ResetMove(ref curDepth, ref nodeType);
		try
		{
			if (AttributeCount <= 0)
			{
				throw new ArgumentOutOfRangeException("attributeIndex");
			}
			readerNav.MoveToAttribute(attributeIndex);
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
		}
		catch
		{
			readerNav.RollBackMove(ref curDepth);
			throw;
		}
		curDepth++;
		nodeType = readerNav.NodeType;
	}

	/// <summary>Moves to the first attribute.</summary>
	/// <returns>true if an attribute exists (the reader moves to the first attribute); otherwise, false (the position of the reader does not change).</returns>
	public override bool MoveToFirstAttribute()
	{
		if (!IsInReadingStates())
		{
			return false;
		}
		readerNav.ResetMove(ref curDepth, ref nodeType);
		if (AttributeCount > 0)
		{
			readerNav.MoveToAttribute(0);
			curDepth++;
			nodeType = readerNav.NodeType;
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
			return true;
		}
		readerNav.RollBackMove(ref curDepth);
		return false;
	}

	/// <summary>Moves to the next attribute.</summary>
	/// <returns>true if there is a next attribute; false if there are no more attributes.</returns>
	public override bool MoveToNextAttribute()
	{
		if (!IsInReadingStates() || nodeType == XmlNodeType.EndElement)
		{
			return false;
		}
		readerNav.LogMove(curDepth);
		readerNav.ResetToAttribute(ref curDepth);
		if (readerNav.MoveToNextAttribute(ref curDepth))
		{
			nodeType = readerNav.NodeType;
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
			return true;
		}
		readerNav.RollBackMove(ref curDepth);
		return false;
	}

	/// <summary>Moves to the element that contains the current attribute node.</summary>
	/// <returns>true if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); false if the reader is not positioned on an attribute (the position of the reader does not change).</returns>
	public override bool MoveToElement()
	{
		if (!IsInReadingStates())
		{
			return false;
		}
		readerNav.LogMove(curDepth);
		readerNav.ResetToAttribute(ref curDepth);
		if (readerNav.MoveToElement())
		{
			curDepth--;
			nodeType = readerNav.NodeType;
			if (bInReadBinary)
			{
				FinishReadBinary();
			}
			return true;
		}
		readerNav.RollBackMove(ref curDepth);
		return false;
	}

	/// <summary>Reads the next node from the stream.</summary>
	/// <returns>true if the next node was read successfully; false if there are no more nodes to read.</returns>
	public override bool Read()
	{
		return Read(fSkipChildren: false);
	}

	private bool Read(bool fSkipChildren)
	{
		if (fEOF)
		{
			return false;
		}
		if (readState == ReadState.Initial)
		{
			if (readerNav.NodeType == XmlNodeType.Document || readerNav.NodeType == XmlNodeType.DocumentFragment)
			{
				bStartFromDocument = true;
				if (!ReadNextNode(fSkipChildren))
				{
					readState = ReadState.Error;
					return false;
				}
			}
			ReSetReadingMarks();
			readState = ReadState.Interactive;
			nodeType = readerNav.NodeType;
			curDepth = 0;
			return true;
		}
		if (bInReadBinary)
		{
			FinishReadBinary();
		}
		if (readerNav.CreatedOnAttribute)
		{
			return false;
		}
		ReSetReadingMarks();
		if (ReadNextNode(fSkipChildren))
		{
			return true;
		}
		if (readState == ReadState.Initial || readState == ReadState.Interactive)
		{
			readState = ReadState.Error;
		}
		if (readState == ReadState.EndOfFile)
		{
			nodeType = XmlNodeType.None;
		}
		return false;
	}

	private bool ReadNextNode(bool fSkipChildren)
	{
		if (readState != ReadState.Interactive && readState != 0)
		{
			nodeType = XmlNodeType.None;
			return false;
		}
		bool num = !fSkipChildren;
		XmlNodeType xmlNodeType = readerNav.NodeType;
		if (num && nodeType != XmlNodeType.EndElement && nodeType != XmlNodeType.EndEntity && (xmlNodeType == XmlNodeType.Element || (xmlNodeType == XmlNodeType.EntityReference && bResolveEntity) || ((readerNav.NodeType == XmlNodeType.Document || readerNav.NodeType == XmlNodeType.DocumentFragment) && readState == ReadState.Initial)))
		{
			if (readerNav.MoveToFirstChild())
			{
				nodeType = readerNav.NodeType;
				curDepth++;
				if (bResolveEntity)
				{
					bResolveEntity = false;
				}
				return true;
			}
			if (readerNav.NodeType == XmlNodeType.Element && !readerNav.IsEmptyElement)
			{
				nodeType = XmlNodeType.EndElement;
				return true;
			}
			if (readerNav.NodeType == XmlNodeType.EntityReference && bResolveEntity)
			{
				bResolveEntity = false;
				nodeType = XmlNodeType.EndEntity;
				return true;
			}
			return ReadForward(fSkipChildren);
		}
		if (readerNav.NodeType == XmlNodeType.EntityReference && bResolveEntity)
		{
			if (readerNav.MoveToFirstChild())
			{
				nodeType = readerNav.NodeType;
				curDepth++;
			}
			else
			{
				nodeType = XmlNodeType.EndEntity;
			}
			bResolveEntity = false;
			return true;
		}
		return ReadForward(fSkipChildren);
	}

	private void SetEndOfFile()
	{
		fEOF = true;
		readState = ReadState.EndOfFile;
		nodeType = XmlNodeType.None;
	}

	private bool ReadAtZeroLevel(bool fSkipChildren)
	{
		if (!fSkipChildren && nodeType != XmlNodeType.EndElement && readerNav.NodeType == XmlNodeType.Element && !readerNav.IsEmptyElement)
		{
			nodeType = XmlNodeType.EndElement;
			return true;
		}
		SetEndOfFile();
		return false;
	}

	private bool ReadForward(bool fSkipChildren)
	{
		if (readState == ReadState.Error)
		{
			return false;
		}
		if (!bStartFromDocument && curDepth == 0)
		{
			return ReadAtZeroLevel(fSkipChildren);
		}
		if (readerNav.MoveToNext())
		{
			nodeType = readerNav.NodeType;
			return true;
		}
		if (curDepth == 0)
		{
			return ReadAtZeroLevel(fSkipChildren);
		}
		if (readerNav.MoveToParent())
		{
			if (readerNav.NodeType == XmlNodeType.Element)
			{
				curDepth--;
				nodeType = XmlNodeType.EndElement;
				return true;
			}
			if (readerNav.NodeType == XmlNodeType.EntityReference)
			{
				curDepth--;
				nodeType = XmlNodeType.EndEntity;
				return true;
			}
			return true;
		}
		return false;
	}

	private void ReSetReadingMarks()
	{
		readerNav.ResetMove(ref curDepth, ref nodeType);
	}

	/// <summary>Changes the <see cref="P:System.Xml.XmlNodeReader.ReadState" /> to Closed.</summary>
	public override void Close()
	{
		readState = ReadState.Closed;
	}

	/// <summary>Skips the children of the current node.</summary>
	public override void Skip()
	{
		Read(fSkipChildren: true);
	}

	/// <summary>Reads the contents of an element or text node as a string.</summary>
	/// <returns>The contents of the element or text-like node (This can include CDATA, Text nodes, and so on). This can be an empty string if the reader is positioned on something other than an element or text node, or if there is no more text content to return in the current context.Note: The text node can be either an element or an attribute text node.</returns>
	public override string ReadString()
	{
		if (NodeType == XmlNodeType.EntityReference && bResolveEntity && !Read())
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current state of the object."));
		}
		return base.ReadString();
	}

	/// <summary>Resolves a namespace prefix in the current element's scope.</summary>
	/// <returns>The namespace URI to which the prefix maps or null if no matching prefix is found.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string. This string does not have to be atomized. </param>
	public override string LookupNamespace(string prefix)
	{
		if (!IsInReadingStates())
		{
			return null;
		}
		string text = readerNav.LookupNamespace(prefix);
		if (text != null && text.Length == 0)
		{
			return null;
		}
		return text;
	}

	/// <summary>Resolves the entity reference for EntityReference nodes.</summary>
	/// <exception cref="T:System.InvalidOperationException">The reader is not positioned on an EntityReference node. </exception>
	public override void ResolveEntity()
	{
		if (!IsInReadingStates() || nodeType != XmlNodeType.EntityReference)
		{
			throw new InvalidOperationException(Res.GetString("The node is not an expandable 'EntityReference' node."));
		}
		bResolveEntity = true;
	}

	/// <summary>Parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.</summary>
	/// <returns>true if there are nodes to return.false if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read.An empty attribute, such as, misc="", returns true with a single node with a value of String.Empty.</returns>
	public override bool ReadAttributeValue()
	{
		if (!IsInReadingStates())
		{
			return false;
		}
		if (readerNav.ReadAttributeValue(ref curDepth, ref bResolveEntity, ref nodeType))
		{
			bInReadBinary = false;
			return true;
		}
		return false;
	}

	/// <summary>Reads the content and returns the Base64 decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlNodeReader.ReadContentAsBase64(System.Byte[],System.Int32,System.Int32)" /> is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	public override int ReadContentAsBase64(byte[] buffer, int index, int count)
	{
		if (readState != ReadState.Interactive)
		{
			return 0;
		}
		if (!bInReadBinary)
		{
			readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
		}
		bInReadBinary = false;
		int result = readBinaryHelper.ReadContentAsBase64(buffer, index, count);
		bInReadBinary = true;
		return result;
	}

	/// <summary>Reads the content and returns the BinHex decoded binary bytes.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Xml.XmlNodeReader.ReadContentAsBinHex(System.Byte[],System.Int32,System.Int32)" />  is not supported on the current node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
	{
		if (readState != ReadState.Interactive)
		{
			return 0;
		}
		if (!bInReadBinary)
		{
			readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
		}
		bInReadBinary = false;
		int result = readBinaryHelper.ReadContentAsBinHex(buffer, index, count);
		bInReadBinary = true;
		return result;
	}

	/// <summary>Reads the element and decodes the Base64 content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node is not an element node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.Xml.XmlException">The element contains mixed content.</exception>
	/// <exception cref="T:System.FormatException">The content cannot be converted to the requested type.</exception>
	public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
	{
		if (readState != ReadState.Interactive)
		{
			return 0;
		}
		if (!bInReadBinary)
		{
			readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
		}
		bInReadBinary = false;
		int result = readBinaryHelper.ReadElementContentAsBase64(buffer, index, count);
		bInReadBinary = true;
		return result;
	}

	/// <summary>Reads the element and decodes the BinHex content.</summary>
	/// <returns>The number of bytes written to the buffer.</returns>
	/// <param name="buffer">The buffer into which to copy the resulting text. This value cannot be null.</param>
	/// <param name="index">The offset into the buffer where to start copying the result.</param>
	/// <param name="count">The maximum number of bytes to copy into the buffer. The actual number of bytes copied is returned from this method.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer" /> value is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The current node is not an element node.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The index into the buffer or index + count is larger than the allocated buffer size.</exception>
	/// <exception cref="T:System.Xml.XmlException">The element contains mixed content.</exception>
	/// <exception cref="T:System.FormatException">The content cannot be converted to the requested type.</exception>
	public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
	{
		if (readState != ReadState.Interactive)
		{
			return 0;
		}
		if (!bInReadBinary)
		{
			readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
		}
		bInReadBinary = false;
		int result = readBinaryHelper.ReadElementContentAsBinHex(buffer, index, count);
		bInReadBinary = true;
		return result;
	}

	private void FinishReadBinary()
	{
		bInReadBinary = false;
		readBinaryHelper.Finish();
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.GetNamespacesInScope(System.Xml.XmlNamespaceScope)" />.</summary>
	/// <returns>
	///   <see cref="T:System.Collections.IDictionary" /> object that contains the namespaces that are in scope.</returns>
	/// <param name="scope">
	///   <see cref="T:System.Xml.XmlNamespaceScope" /> object.</param>
	IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
	{
		return readerNav.GetNamespacesInScope(scope);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupPrefix(System.String)" />.</summary>
	/// <returns>
	///   <see cref="T:System.String" /> object that contains the namespace prefix.</returns>
	/// <param name="namespaceName">
	///   <see cref="T:System.String" /> object that identifies the namespace.</param>
	string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
	{
		return readerNav.LookupPrefix(namespaceName);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.IXmlNamespaceResolver.LookupNamespace(System.String)" />.</summary>
	/// <returns>
	///   <see cref="T:System.String" /> that contains the namespace name.</returns>
	/// <param name="prefix">
	///   <see cref="T:System.String" /> that contains the namespace prefix.</param>
	string IXmlNamespaceResolver.LookupNamespace(string prefix)
	{
		if (!IsInReadingStates())
		{
			return readerNav.DefaultLookupNamespace(prefix);
		}
		string text = readerNav.LookupNamespace(prefix);
		if (text != null)
		{
			text = readerNav.NameTable.Add(text);
		}
		return text;
	}
}
