using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents a single node in the XML document. </summary>
[DebuggerDisplay("{debuggerDisplayProxy}")]
public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable
{
	internal XmlNode parentNode;

	/// <summary>Gets the qualified name of the node, when overridden in a derived class.</summary>
	/// <returns>The qualified name of the node. The name returned is dependent on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node: Type Name Attribute The qualified name of the attribute. CDATA #cdata-section Comment #comment Document #document DocumentFragment #document-fragment DocumentType The document type name. Element The qualified name of the element. Entity The name of the entity. EntityReference The name of the entity referenced. Notation The notation name. ProcessingInstruction The target of the processing instruction. Text #text Whitespace #whitespace SignificantWhitespace #significant-whitespace XmlDeclaration #xml-declaration </returns>
	public abstract string Name { get; }

	/// <summary>Gets or sets the value of the node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node: Type Value Attribute The value of the attribute. CDATASection The content of the CDATA Section. Comment The content of the comment. Document null. DocumentFragment null. DocumentType null. Element null. You can use the <see cref="P:System.Xml.XmlElement.InnerText" /> or <see cref="P:System.Xml.XmlElement.InnerXml" /> properties to access the value of the element node. Entity null. EntityReference null. Notation null. ProcessingInstruction The entire content excluding the target. Text The content of the text node. SignificantWhitespace The white space characters. White space can consist of one or more space characters, carriage returns, line feeds, or tabs. Whitespace The white space characters. White space can consist of one or more space characters, carriage returns, line feeds, or tabs. XmlDeclaration The content of the declaration (that is, everything between &lt;?xml and ?&gt;). </returns>
	/// <exception cref="T:System.ArgumentException">Setting the value of a node that is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">Setting the value of a node that is not supposed to have a value (for example, an Element node). </exception>
	public virtual string Value
	{
		get
		{
			return null;
		}
		set
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Res.GetString("Cannot set a value on node type '{0}'."), NodeType.ToString()));
		}
	}

	/// <summary>Gets the type of the current node, when overridden in a derived class.</summary>
	/// <returns>One of the <see cref="T:System.Xml.XmlNodeType" /> values.</returns>
	public abstract XmlNodeType NodeType { get; }

	/// <summary>Gets the parent of this node (for nodes that can have parents).</summary>
	/// <returns>The XmlNode that is the parent of the current node. If a node has just been created and not yet added to the tree, or if it has been removed from the tree, the parent is null. For all other nodes, the value returned depends on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node. The following table describes the possible return values for the ParentNode property.NodeType Return Value of ParentNode Attribute, Document, DocumentFragment, Entity, Notation Returns null; these nodes do not have parents. CDATA Returns the element or entity reference containing the CDATA section. Comment Returns the element, entity reference, document type, or document containing the comment. DocumentType Returns the document node. Element Returns the parent node of the element. If the element is the root node in the tree, the parent is the document node. EntityReference Returns the element, attribute, or entity reference containing the entity reference. ProcessingInstruction Returns the document, element, document type, or entity reference containing the processing instruction. Text Returns the parent element, attribute, or entity reference containing the text node. </returns>
	public virtual XmlNode ParentNode
	{
		get
		{
			if (parentNode.NodeType != XmlNodeType.Document)
			{
				return parentNode;
			}
			if (parentNode.FirstChild is XmlLinkedNode xmlLinkedNode)
			{
				XmlLinkedNode xmlLinkedNode2 = xmlLinkedNode;
				do
				{
					if (xmlLinkedNode2 == this)
					{
						return parentNode;
					}
					xmlLinkedNode2 = xmlLinkedNode2.next;
				}
				while (xmlLinkedNode2 != null && xmlLinkedNode2 != xmlLinkedNode);
			}
			return null;
		}
	}

	/// <summary>Gets all the child nodes of the node.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> that contains all the child nodes of the node.If there are no child nodes, this property returns an empty <see cref="T:System.Xml.XmlNodeList" />.</returns>
	public virtual XmlNodeList ChildNodes => new XmlChildNodes(this);

	/// <summary>Gets the node immediately preceding this node.</summary>
	/// <returns>The preceding XmlNode. If there is no preceding node, null is returned.</returns>
	public virtual XmlNode PreviousSibling => null;

	/// <summary>Gets the node immediately following this node.</summary>
	/// <returns>The next XmlNode. If there is no next node, null is returned.</returns>
	public virtual XmlNode NextSibling => null;

	/// <summary>Gets an <see cref="T:System.Xml.XmlAttributeCollection" /> containing the attributes of this node.</summary>
	/// <returns>An XmlAttributeCollection containing the attributes of the node.If the node is of type XmlNodeType.Element, the attributes of the node are returned. Otherwise, this property returns null.</returns>
	public virtual XmlAttributeCollection Attributes => null;

	/// <summary>Gets the <see cref="T:System.Xml.XmlDocument" /> to which this node belongs.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlDocument" /> to which this node belongs.If the node is an <see cref="T:System.Xml.XmlDocument" /> (NodeType equals XmlNodeType.Document), this property returns null.</returns>
	public virtual XmlDocument OwnerDocument
	{
		get
		{
			if (parentNode.NodeType == XmlNodeType.Document)
			{
				return (XmlDocument)parentNode;
			}
			return parentNode.OwnerDocument;
		}
	}

	/// <summary>Gets the first child of the node.</summary>
	/// <returns>The first child of the node. If there is no such node, null is returned.</returns>
	public virtual XmlNode FirstChild => LastNode?.next;

	/// <summary>Gets the last child of the node.</summary>
	/// <returns>The last child of the node. If there is no such node, null is returned.</returns>
	public virtual XmlNode LastChild => LastNode;

	internal virtual bool IsContainer => false;

	internal virtual XmlLinkedNode LastNode
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	/// <summary>Gets a value indicating whether this node has any child nodes.</summary>
	/// <returns>true if the node has child nodes; otherwise, false.</returns>
	public virtual bool HasChildNodes => LastNode != null;

	/// <summary>Gets the namespace URI of this node.</summary>
	/// <returns>The namespace URI of this node. If there is no namespace URI, this property returns String.Empty.</returns>
	public virtual string NamespaceURI => string.Empty;

	/// <summary>Gets or sets the namespace prefix of this node.</summary>
	/// <returns>The namespace prefix of this node. For example, Prefix is bk for the element &lt;bk:book&gt;. If there is no prefix, this property returns String.Empty.</returns>
	/// <exception cref="T:System.ArgumentException">This node is read-only. </exception>
	/// <exception cref="T:System.Xml.XmlException">The specified prefix contains an invalid character.The specified prefix is malformed.The specified prefix is "xml" and the namespaceURI of this node is different from "http://www.w3.org/XML/1998/namespace".This node is an attribute and the specified prefix is "xmlns" and the namespaceURI of this node is different from "http://www.w3.org/2000/xmlns/ ".This node is an attribute and the qualifiedName of this node is "xmlns". </exception>
	public virtual string Prefix
	{
		get
		{
			return string.Empty;
		}
		set
		{
		}
	}

	/// <summary>Gets the local name of the node, when overridden in a derived class.</summary>
	/// <returns>The name of the node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.The name returned is dependent on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node: Type Name Attribute The local name of the attribute. CDATA #cdata-section Comment #comment Document #document DocumentFragment #document-fragment DocumentType The document type name. Element The local name of the element. Entity The name of the entity. EntityReference The name of the entity referenced. Notation The notation name. ProcessingInstruction The target of the processing instruction. Text #text Whitespace #whitespace SignificantWhitespace #significant-whitespace XmlDeclaration #xml-declaration </returns>
	public abstract string LocalName { get; }

	/// <summary>Gets a value indicating whether the node is read-only.</summary>
	/// <returns>true if the node is read-only; otherwise false.</returns>
	public virtual bool IsReadOnly
	{
		get
		{
			_ = OwnerDocument;
			return HasReadOnlyParent(this);
		}
	}

	/// <summary>Gets or sets the concatenated values of the node and all its child nodes.</summary>
	/// <returns>The concatenated values of the node and all its child nodes.</returns>
	public virtual string InnerText
	{
		get
		{
			XmlNode firstChild = FirstChild;
			if (firstChild == null)
			{
				return string.Empty;
			}
			if (firstChild.NextSibling == null)
			{
				XmlNodeType nodeType = firstChild.NodeType;
				if ((uint)(nodeType - 3) <= 1u || (uint)(nodeType - 13) <= 1u)
				{
					return firstChild.Value;
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			AppendChildText(stringBuilder);
			return stringBuilder.ToString();
		}
		set
		{
			XmlNode firstChild = FirstChild;
			if (firstChild != null && firstChild.NextSibling == null && firstChild.NodeType == XmlNodeType.Text)
			{
				firstChild.Value = value;
				return;
			}
			RemoveAll();
			AppendChild(OwnerDocument.CreateTextNode(value));
		}
	}

	/// <summary>Gets the markup containing this node and all its child nodes.</summary>
	/// <returns>The markup containing this node and all its child nodes.NoteOuterXml does not return default attributes.</returns>
	public virtual string OuterXml
	{
		get
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlDOMTextWriter xmlDOMTextWriter = new XmlDOMTextWriter(stringWriter);
			try
			{
				WriteTo(xmlDOMTextWriter);
			}
			finally
			{
				xmlDOMTextWriter.Close();
			}
			return stringWriter.ToString();
		}
	}

	/// <summary>Gets or sets the markup representing only the child nodes of this node.</summary>
	/// <returns>The markup of the child nodes of this node.NoteInnerXml does not return default attributes.</returns>
	/// <exception cref="T:System.InvalidOperationException">Setting this property on a node that cannot have child nodes. </exception>
	/// <exception cref="T:System.Xml.XmlException">The XML specified when setting this property is not well-formed. </exception>
	public virtual string InnerXml
	{
		get
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			XmlDOMTextWriter xmlDOMTextWriter = new XmlDOMTextWriter(stringWriter);
			try
			{
				WriteContentTo(xmlDOMTextWriter);
			}
			finally
			{
				xmlDOMTextWriter.Close();
			}
			return stringWriter.ToString();
		}
		set
		{
			throw new InvalidOperationException(Res.GetString("Cannot set the 'InnerXml' for the current node because it is either read-only or cannot have children."));
		}
	}

	/// <summary>Gets the post schema validation infoset that has been assigned to this node as a result of schema validation.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object containing the post schema validation infoset of this node.</returns>
	public virtual IXmlSchemaInfo SchemaInfo => XmlDocument.NotKnownSchemaInfo;

	/// <summary>Gets the base URI of the current node.</summary>
	/// <returns>The location from which the node was loaded or String.Empty if the node has no base URI.</returns>
	public virtual string BaseURI
	{
		get
		{
			for (XmlNode xmlNode = ParentNode; xmlNode != null; xmlNode = xmlNode.ParentNode)
			{
				switch (xmlNode.NodeType)
				{
				case XmlNodeType.EntityReference:
					return ((XmlEntityReference)xmlNode).ChildBaseURI;
				case XmlNodeType.Attribute:
				case XmlNodeType.Entity:
				case XmlNodeType.Document:
					return xmlNode.BaseURI;
				}
			}
			return string.Empty;
		}
	}

	internal XmlDocument Document
	{
		get
		{
			if (NodeType == XmlNodeType.Document)
			{
				return (XmlDocument)this;
			}
			return OwnerDocument;
		}
	}

	/// <summary>Gets the first child element with the specified <see cref="P:System.Xml.XmlNode.Name" />.</summary>
	/// <returns>The first <see cref="T:System.Xml.XmlElement" /> that matches the specified name.</returns>
	/// <param name="name">The qualified name of the element to retrieve. </param>
	public virtual XmlElement this[string name]
	{
		get
		{
			for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name == name)
				{
					return (XmlElement)xmlNode;
				}
			}
			return null;
		}
	}

	/// <summary>Gets the first child element with the specified <see cref="P:System.Xml.XmlNode.LocalName" /> and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The first <see cref="T:System.Xml.XmlElement" /> with the matching <paramref name="localname" /> and <paramref name="ns" />.</returns>
	/// <param name="localname">The local name of the element. </param>
	/// <param name="ns">The namespace URI of the element. </param>
	public virtual XmlElement this[string localname, string ns]
	{
		get
		{
			for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.LocalName == localname && xmlNode.NamespaceURI == ns)
				{
					return (XmlElement)xmlNode;
				}
			}
			return null;
		}
	}

	internal virtual XmlSpace XmlSpace
	{
		get
		{
			XmlNode xmlNode = this;
			XmlElement xmlElement = null;
			do
			{
				if (xmlNode is XmlElement xmlElement2 && xmlElement2.HasAttribute("xml:space"))
				{
					string text = XmlConvert.TrimString(xmlElement2.GetAttribute("xml:space"));
					if (text == "default")
					{
						return XmlSpace.Default;
					}
					if (text == "preserve")
					{
						return XmlSpace.Preserve;
					}
				}
				xmlNode = xmlNode.ParentNode;
			}
			while (xmlNode != null);
			return XmlSpace.None;
		}
	}

	internal virtual string XmlLang
	{
		get
		{
			XmlNode xmlNode = this;
			XmlElement xmlElement = null;
			do
			{
				if (xmlNode is XmlElement xmlElement2 && xmlElement2.HasAttribute("xml:lang"))
				{
					return xmlElement2.GetAttribute("xml:lang");
				}
				xmlNode = xmlNode.ParentNode;
			}
			while (xmlNode != null);
			return string.Empty;
		}
	}

	internal virtual XPathNodeType XPNodeType => (XPathNodeType)(-1);

	internal virtual string XPLocalName => string.Empty;

	internal virtual bool IsText => false;

	public virtual XmlNode PreviousText => null;

	private object debuggerDisplayProxy => new DebuggerDisplayXmlNodeProxy(this);

	internal XmlNode()
	{
	}

	internal XmlNode(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentException(Res.GetString("Cannot create a node without an owner document."));
		}
		parentNode = doc;
	}

	/// <summary>Creates an <see cref="T:System.Xml.XPath.XPathNavigator" /> for navigating this object.</summary>
	/// <returns>An XPathNavigator object used to navigate the node. The XPathNavigator is positioned on the node from which the method was called. It is not positioned on the root of the document.</returns>
	public virtual XPathNavigator CreateNavigator()
	{
		if (this is XmlDocument xmlDocument)
		{
			return xmlDocument.CreateNavigator(this);
		}
		return OwnerDocument.CreateNavigator(this);
	}

	/// <summary>Selects the first XmlNode that matches the XPath expression.</summary>
	/// <returns>The first XmlNode that matches the XPath query or null if no matching node is found. The XmlNode should not be expected to be connected "live" to the XML document. That is, changes that appear in the XML document may not appear in the XmlNode, and vice versa.</returns>
	/// <param name="xpath">The XPath expression. </param>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression contains a prefix. </exception>
	public XmlNode SelectSingleNode(string xpath)
	{
		return SelectNodes(xpath)?[0];
	}

	/// <summary>Selects the first XmlNode that matches the XPath expression. Any prefixes found in the XPath expression are resolved using the supplied <see cref="T:System.Xml.XmlNamespaceManager" />.</summary>
	/// <returns>The first XmlNode that matches the XPath query or null if no matching node is found. The XmlNode should not be expected to be connected "live" to the XML document. That is, changes that appear in the XML document may not appear in the XmlNode, and vice versa.</returns>
	/// <param name="xpath">The XPath expression. </param>
	/// <param name="nsmgr">An <see cref="T:System.Xml.XmlNamespaceManager" /> to use for resolving namespaces for prefixes in the XPath expression. </param>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression contains a prefix which is not defined in the XmlNamespaceManager. </exception>
	public XmlNode SelectSingleNode(string xpath, XmlNamespaceManager nsmgr)
	{
		XPathNavigator xPathNavigator = CreateNavigator();
		if (xPathNavigator == null)
		{
			return null;
		}
		XPathExpression xPathExpression = xPathNavigator.Compile(xpath);
		xPathExpression.SetContext(nsmgr);
		return new XPathNodeList(xPathNavigator.Select(xPathExpression))[0];
	}

	/// <summary>Selects a list of nodes matching the XPath expression.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a collection of nodes matching the XPath query.</returns>
	/// <param name="xpath">The XPath expression. </param>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression contains a prefix. </exception>
	public XmlNodeList SelectNodes(string xpath)
	{
		XPathNavigator xPathNavigator = CreateNavigator();
		if (xPathNavigator == null)
		{
			return null;
		}
		return new XPathNodeList(xPathNavigator.Select(xpath));
	}

	/// <summary>Selects a list of nodes matching the XPath expression. Any prefixes found in the XPath expression are resolved using the supplied <see cref="T:System.Xml.XmlNamespaceManager" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a collection of nodes matching the XPath query.</returns>
	/// <param name="xpath">The XPath expression. </param>
	/// <param name="nsmgr">An <see cref="T:System.Xml.XmlNamespaceManager" /> to use for resolving namespaces for prefixes in the XPath expression. </param>
	/// <exception cref="T:System.Xml.XPath.XPathException">The XPath expression contains a prefix which is not defined in the XmlNamespaceManager. </exception>
	public XmlNodeList SelectNodes(string xpath, XmlNamespaceManager nsmgr)
	{
		XPathNavigator xPathNavigator = CreateNavigator();
		if (xPathNavigator == null)
		{
			return null;
		}
		XPathExpression xPathExpression = xPathNavigator.Compile(xpath);
		xPathExpression.SetContext(nsmgr);
		return new XPathNodeList(xPathNavigator.Select(xPathExpression));
	}

	internal bool AncestorNode(XmlNode node)
	{
		XmlNode xmlNode = ParentNode;
		while (xmlNode != null && xmlNode != this)
		{
			if (xmlNode == node)
			{
				return true;
			}
			xmlNode = xmlNode.ParentNode;
		}
		return false;
	}

	internal bool IsConnected()
	{
		XmlNode xmlNode = ParentNode;
		while (xmlNode != null && xmlNode.NodeType != XmlNodeType.Document)
		{
			xmlNode = xmlNode.ParentNode;
		}
		return xmlNode != null;
	}

	/// <summary>Inserts the specified node immediately before the specified reference node.</summary>
	/// <returns>The node being inserted.</returns>
	/// <param name="newChild">The XmlNode to insert. </param>
	/// <param name="refChild">The XmlNode that is the reference node. The <paramref name="newChild" /> is placed before this node. </param>
	/// <exception cref="T:System.InvalidOperationException">The current node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.The <paramref name="refChild" /> is not a child of this node.This node is read-only. </exception>
	public virtual XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
	{
		if (this == newChild || AncestorNode(newChild))
		{
			throw new ArgumentException(Res.GetString("Cannot insert a node or any ancestor of that node as a child of itself."));
		}
		if (refChild == null)
		{
			return AppendChild(newChild);
		}
		if (!IsContainer)
		{
			throw new InvalidOperationException(Res.GetString("The current node cannot contain other nodes."));
		}
		if (refChild.ParentNode != this)
		{
			throw new ArgumentException(Res.GetString("The reference node is not a child of this node."));
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		XmlDocument ownerDocument = newChild.OwnerDocument;
		XmlDocument ownerDocument2 = OwnerDocument;
		if (ownerDocument != null && ownerDocument != ownerDocument2 && ownerDocument != this)
		{
			throw new ArgumentException(Res.GetString("The node to be inserted is from a different document context."));
		}
		if (!CanInsertBefore(newChild, refChild))
		{
			throw new InvalidOperationException(Res.GetString("Cannot insert the node in the specified location."));
		}
		if (newChild.ParentNode != null)
		{
			newChild.ParentNode.RemoveChild(newChild);
		}
		if (newChild.NodeType == XmlNodeType.DocumentFragment)
		{
			XmlNode firstChild;
			XmlNode result = (firstChild = newChild.FirstChild);
			if (firstChild != null)
			{
				newChild.RemoveChild(firstChild);
				InsertBefore(firstChild, refChild);
				InsertAfter(newChild, firstChild);
			}
			return result;
		}
		if (!(newChild is XmlLinkedNode) || !IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type."));
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		XmlLinkedNode xmlLinkedNode2 = (XmlLinkedNode)refChild;
		string value = newChild.Value;
		XmlNodeChangedEventArgs eventArgs = GetEventArgs(newChild, newChild.ParentNode, this, value, value, XmlNodeChangedAction.Insert);
		if (eventArgs != null)
		{
			BeforeEvent(eventArgs);
		}
		if (xmlLinkedNode2 == FirstChild)
		{
			xmlLinkedNode.next = xmlLinkedNode2;
			LastNode.next = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
			if (xmlLinkedNode.IsText && xmlLinkedNode2.IsText)
			{
				NestTextNodes(xmlLinkedNode, xmlLinkedNode2);
			}
		}
		else
		{
			XmlLinkedNode xmlLinkedNode3 = (XmlLinkedNode)xmlLinkedNode2.PreviousSibling;
			xmlLinkedNode.next = xmlLinkedNode2;
			xmlLinkedNode3.next = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
			if (xmlLinkedNode3.IsText)
			{
				if (xmlLinkedNode.IsText)
				{
					NestTextNodes(xmlLinkedNode3, xmlLinkedNode);
					if (xmlLinkedNode2.IsText)
					{
						NestTextNodes(xmlLinkedNode, xmlLinkedNode2);
					}
				}
				else if (xmlLinkedNode2.IsText)
				{
					UnnestTextNodes(xmlLinkedNode3, xmlLinkedNode2);
				}
			}
			else if (xmlLinkedNode.IsText && xmlLinkedNode2.IsText)
			{
				NestTextNodes(xmlLinkedNode, xmlLinkedNode2);
			}
		}
		if (eventArgs != null)
		{
			AfterEvent(eventArgs);
		}
		return xmlLinkedNode;
	}

	/// <summary>Inserts the specified node immediately after the specified reference node.</summary>
	/// <returns>The node being inserted.</returns>
	/// <param name="newChild">The XmlNode to insert. </param>
	/// <param name="refChild">The XmlNode that is the reference node. The <paramref name="newNode" /> is placed after the <paramref name="refNode" />. </param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.The <paramref name="refChild" /> is not a child of this node.This node is read-only. </exception>
	public virtual XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
	{
		if (this == newChild || AncestorNode(newChild))
		{
			throw new ArgumentException(Res.GetString("Cannot insert a node or any ancestor of that node as a child of itself."));
		}
		if (refChild == null)
		{
			return PrependChild(newChild);
		}
		if (!IsContainer)
		{
			throw new InvalidOperationException(Res.GetString("The current node cannot contain other nodes."));
		}
		if (refChild.ParentNode != this)
		{
			throw new ArgumentException(Res.GetString("The reference node is not a child of this node."));
		}
		if (newChild == refChild)
		{
			return newChild;
		}
		XmlDocument ownerDocument = newChild.OwnerDocument;
		XmlDocument ownerDocument2 = OwnerDocument;
		if (ownerDocument != null && ownerDocument != ownerDocument2 && ownerDocument != this)
		{
			throw new ArgumentException(Res.GetString("The node to be inserted is from a different document context."));
		}
		if (!CanInsertAfter(newChild, refChild))
		{
			throw new InvalidOperationException(Res.GetString("Cannot insert the node in the specified location."));
		}
		if (newChild.ParentNode != null)
		{
			newChild.ParentNode.RemoveChild(newChild);
		}
		if (newChild.NodeType == XmlNodeType.DocumentFragment)
		{
			XmlNode refChild2 = refChild;
			XmlNode firstChild = newChild.FirstChild;
			XmlNode xmlNode = firstChild;
			while (xmlNode != null)
			{
				XmlNode nextSibling = xmlNode.NextSibling;
				newChild.RemoveChild(xmlNode);
				InsertAfter(xmlNode, refChild2);
				refChild2 = xmlNode;
				xmlNode = nextSibling;
			}
			return firstChild;
		}
		if (!(newChild is XmlLinkedNode) || !IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type."));
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		XmlLinkedNode xmlLinkedNode2 = (XmlLinkedNode)refChild;
		string value = newChild.Value;
		XmlNodeChangedEventArgs eventArgs = GetEventArgs(newChild, newChild.ParentNode, this, value, value, XmlNodeChangedAction.Insert);
		if (eventArgs != null)
		{
			BeforeEvent(eventArgs);
		}
		if (xmlLinkedNode2 == LastNode)
		{
			xmlLinkedNode.next = xmlLinkedNode2.next;
			xmlLinkedNode2.next = xmlLinkedNode;
			LastNode = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
			if (xmlLinkedNode2.IsText && xmlLinkedNode.IsText)
			{
				NestTextNodes(xmlLinkedNode2, xmlLinkedNode);
			}
		}
		else
		{
			XmlLinkedNode xmlLinkedNode3 = (xmlLinkedNode.next = xmlLinkedNode2.next);
			xmlLinkedNode2.next = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
			if (xmlLinkedNode2.IsText)
			{
				if (xmlLinkedNode.IsText)
				{
					NestTextNodes(xmlLinkedNode2, xmlLinkedNode);
					if (xmlLinkedNode3.IsText)
					{
						NestTextNodes(xmlLinkedNode, xmlLinkedNode3);
					}
				}
				else if (xmlLinkedNode3.IsText)
				{
					UnnestTextNodes(xmlLinkedNode2, xmlLinkedNode3);
				}
			}
			else if (xmlLinkedNode.IsText && xmlLinkedNode3.IsText)
			{
				NestTextNodes(xmlLinkedNode, xmlLinkedNode3);
			}
		}
		if (eventArgs != null)
		{
			AfterEvent(eventArgs);
		}
		return xmlLinkedNode;
	}

	/// <summary>Replaces the child node <paramref name="oldChild" /> with <paramref name="newChild" /> node.</summary>
	/// <returns>The node replaced.</returns>
	/// <param name="newChild">The new node to put in the child list. </param>
	/// <param name="oldChild">The node being replaced in the list. </param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only.The <paramref name="oldChild" /> is not a child of this node. </exception>
	public virtual XmlNode ReplaceChild(XmlNode newChild, XmlNode oldChild)
	{
		XmlNode nextSibling = oldChild.NextSibling;
		RemoveChild(oldChild);
		InsertBefore(newChild, nextSibling);
		return oldChild;
	}

	/// <summary>Removes specified child node.</summary>
	/// <returns>The node removed.</returns>
	/// <param name="oldChild">The node being removed. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="oldChild" /> is not a child of this node. Or this node is read-only. </exception>
	public virtual XmlNode RemoveChild(XmlNode oldChild)
	{
		if (!IsContainer)
		{
			throw new InvalidOperationException(Res.GetString("The current node cannot contain other nodes, so the node to be removed is not its child."));
		}
		if (oldChild.ParentNode != this)
		{
			throw new ArgumentException(Res.GetString("The node to be removed is not a child of this node."));
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)oldChild;
		string value = xmlLinkedNode.Value;
		XmlNodeChangedEventArgs eventArgs = GetEventArgs(xmlLinkedNode, this, null, value, value, XmlNodeChangedAction.Remove);
		if (eventArgs != null)
		{
			BeforeEvent(eventArgs);
		}
		XmlLinkedNode lastNode = LastNode;
		if (xmlLinkedNode == FirstChild)
		{
			if (xmlLinkedNode == lastNode)
			{
				LastNode = null;
				xmlLinkedNode.next = null;
				xmlLinkedNode.SetParent(null);
			}
			else
			{
				XmlLinkedNode next = xmlLinkedNode.next;
				if (next.IsText && xmlLinkedNode.IsText)
				{
					UnnestTextNodes(xmlLinkedNode, next);
				}
				lastNode.next = next;
				xmlLinkedNode.next = null;
				xmlLinkedNode.SetParent(null);
			}
		}
		else if (xmlLinkedNode == lastNode)
		{
			XmlLinkedNode xmlLinkedNode2 = (XmlLinkedNode)xmlLinkedNode.PreviousSibling;
			xmlLinkedNode2.next = xmlLinkedNode.next;
			LastNode = xmlLinkedNode2;
			xmlLinkedNode.next = null;
			xmlLinkedNode.SetParent(null);
		}
		else
		{
			XmlLinkedNode xmlLinkedNode3 = (XmlLinkedNode)xmlLinkedNode.PreviousSibling;
			XmlLinkedNode next2 = xmlLinkedNode.next;
			if (next2.IsText)
			{
				if (xmlLinkedNode3.IsText)
				{
					NestTextNodes(xmlLinkedNode3, next2);
				}
				else if (xmlLinkedNode.IsText)
				{
					UnnestTextNodes(xmlLinkedNode, next2);
				}
			}
			xmlLinkedNode3.next = next2;
			xmlLinkedNode.next = null;
			xmlLinkedNode.SetParent(null);
		}
		if (eventArgs != null)
		{
			AfterEvent(eventArgs);
		}
		return oldChild;
	}

	/// <summary>Adds the specified node to the beginning of the list of child nodes for this node.</summary>
	/// <returns>The node added.</returns>
	/// <param name="newChild">The node to add. All the contents of the node to be added are moved into the specified location.</param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only. </exception>
	public virtual XmlNode PrependChild(XmlNode newChild)
	{
		return InsertBefore(newChild, FirstChild);
	}

	/// <summary>Adds the specified node to the end of the list of child nodes, of this node.</summary>
	/// <returns>The node added.</returns>
	/// <param name="newChild">The node to add. All the contents of the node to be added are moved into the specified location. </param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only. </exception>
	public virtual XmlNode AppendChild(XmlNode newChild)
	{
		XmlDocument xmlDocument = OwnerDocument;
		if (xmlDocument == null)
		{
			xmlDocument = this as XmlDocument;
		}
		if (!IsContainer)
		{
			throw new InvalidOperationException(Res.GetString("The current node cannot contain other nodes."));
		}
		if (this == newChild || AncestorNode(newChild))
		{
			throw new ArgumentException(Res.GetString("Cannot insert a node or any ancestor of that node as a child of itself."));
		}
		if (newChild.ParentNode != null)
		{
			newChild.ParentNode.RemoveChild(newChild);
		}
		XmlDocument ownerDocument = newChild.OwnerDocument;
		if (ownerDocument != null && ownerDocument != xmlDocument && ownerDocument != this)
		{
			throw new ArgumentException(Res.GetString("The node to be inserted is from a different document context."));
		}
		if (newChild.NodeType == XmlNodeType.DocumentFragment)
		{
			XmlNode firstChild = newChild.FirstChild;
			XmlNode xmlNode = firstChild;
			while (xmlNode != null)
			{
				XmlNode nextSibling = xmlNode.NextSibling;
				newChild.RemoveChild(xmlNode);
				AppendChild(xmlNode);
				xmlNode = nextSibling;
			}
			return firstChild;
		}
		if (!(newChild is XmlLinkedNode) || !IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type."));
		}
		if (!CanInsertAfter(newChild, LastChild))
		{
			throw new InvalidOperationException(Res.GetString("Cannot insert the node in the specified location."));
		}
		string value = newChild.Value;
		XmlNodeChangedEventArgs eventArgs = GetEventArgs(newChild, newChild.ParentNode, this, value, value, XmlNodeChangedAction.Insert);
		if (eventArgs != null)
		{
			BeforeEvent(eventArgs);
		}
		XmlLinkedNode lastNode = LastNode;
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		if (lastNode == null)
		{
			xmlLinkedNode.next = xmlLinkedNode;
			LastNode = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
		}
		else
		{
			xmlLinkedNode.next = lastNode.next;
			lastNode.next = xmlLinkedNode;
			LastNode = xmlLinkedNode;
			xmlLinkedNode.SetParent(this);
			if (lastNode.IsText && xmlLinkedNode.IsText)
			{
				NestTextNodes(lastNode, xmlLinkedNode);
			}
		}
		if (eventArgs != null)
		{
			AfterEvent(eventArgs);
		}
		return xmlLinkedNode;
	}

	internal virtual XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
	{
		XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(newChild, this);
		if (insertEventArgsForLoad != null)
		{
			doc.BeforeEvent(insertEventArgsForLoad);
		}
		XmlLinkedNode lastNode = LastNode;
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		if (lastNode == null)
		{
			xmlLinkedNode.next = xmlLinkedNode;
			LastNode = xmlLinkedNode;
			xmlLinkedNode.SetParentForLoad(this);
		}
		else
		{
			xmlLinkedNode.next = lastNode.next;
			lastNode.next = xmlLinkedNode;
			LastNode = xmlLinkedNode;
			if (lastNode.IsText && xmlLinkedNode.IsText)
			{
				NestTextNodes(lastNode, xmlLinkedNode);
			}
			else
			{
				xmlLinkedNode.SetParentForLoad(this);
			}
		}
		if (insertEventArgsForLoad != null)
		{
			doc.AfterEvent(insertEventArgsForLoad);
		}
		return xmlLinkedNode;
	}

	internal virtual bool IsValidChildType(XmlNodeType type)
	{
		return false;
	}

	internal virtual bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
	{
		return true;
	}

	internal virtual bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
	{
		return true;
	}

	/// <summary>Creates a duplicate of the node, when overridden in a derived class.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. </param>
	/// <exception cref="T:System.InvalidOperationException">Calling this method on a node type that cannot be cloned. </exception>
	public abstract XmlNode CloneNode(bool deep);

	internal virtual void CopyChildren(XmlDocument doc, XmlNode container, bool deep)
	{
		for (XmlNode xmlNode = container.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			AppendChildForLoad(xmlNode.CloneNode(deep), doc);
		}
	}

	/// <summary>Puts all XmlText nodes in the full depth of the sub-tree underneath this XmlNode into a "normal" form where only markup (that is, tags, comments, processing instructions, CDATA sections, and entity references) separates XmlText nodes, that is, there are no adjacent XmlText nodes.</summary>
	public virtual void Normalize()
	{
		XmlNode xmlNode = null;
		StringBuilder stringBuilder = new StringBuilder();
		XmlNode nextSibling;
		for (XmlNode xmlNode2 = FirstChild; xmlNode2 != null; xmlNode2 = nextSibling)
		{
			nextSibling = xmlNode2.NextSibling;
			switch (xmlNode2.NodeType)
			{
			case XmlNodeType.Text:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				stringBuilder.Append(xmlNode2.Value);
				if (NormalizeWinner(xmlNode, xmlNode2) == xmlNode)
				{
					RemoveChild(xmlNode2);
					continue;
				}
				if (xmlNode != null)
				{
					RemoveChild(xmlNode);
				}
				xmlNode = xmlNode2;
				continue;
			case XmlNodeType.Element:
				xmlNode2.Normalize();
				break;
			}
			if (xmlNode != null)
			{
				xmlNode.Value = stringBuilder.ToString();
				xmlNode = null;
			}
			stringBuilder.Remove(0, stringBuilder.Length);
		}
		if (xmlNode != null && stringBuilder.Length > 0)
		{
			xmlNode.Value = stringBuilder.ToString();
		}
	}

	private XmlNode NormalizeWinner(XmlNode firstNode, XmlNode secondNode)
	{
		if (firstNode == null)
		{
			return secondNode;
		}
		if (firstNode.NodeType == XmlNodeType.Text)
		{
			return firstNode;
		}
		if (secondNode.NodeType == XmlNodeType.Text)
		{
			return secondNode;
		}
		if (firstNode.NodeType == XmlNodeType.SignificantWhitespace)
		{
			return firstNode;
		}
		if (secondNode.NodeType == XmlNodeType.SignificantWhitespace)
		{
			return secondNode;
		}
		if (firstNode.NodeType == XmlNodeType.Whitespace)
		{
			return firstNode;
		}
		if (secondNode.NodeType == XmlNodeType.Whitespace)
		{
			return secondNode;
		}
		return null;
	}

	/// <summary>Tests if the DOM implementation implements a specific feature.</summary>
	/// <returns>true if the feature is implemented in the specified version; otherwise, false. The following table describes the combinations that return true.Feature Version XML 1.0 XML 2.0 </returns>
	/// <param name="feature">The package name of the feature to test. This name is not case-sensitive. </param>
	/// <param name="version">The version number of the package name to test. If the version is not specified (null), supporting any version of the feature causes the method to return true. </param>
	public virtual bool Supports(string feature, string version)
	{
		if (string.Compare("XML", feature, StringComparison.OrdinalIgnoreCase) == 0)
		{
			switch (version)
			{
			case null:
			case "1.0":
			case "2.0":
				return true;
			}
		}
		return false;
	}

	internal static bool HasReadOnlyParent(XmlNode n)
	{
		while (n != null)
		{
			switch (n.NodeType)
			{
			case XmlNodeType.EntityReference:
			case XmlNodeType.Entity:
				return true;
			case XmlNodeType.Attribute:
				n = ((XmlAttribute)n).OwnerElement;
				break;
			default:
				n = n.ParentNode;
				break;
			}
		}
		return false;
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	public virtual XmlNode Clone()
	{
		return CloneNode(deep: true);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.XmlNode.Clone" />.</summary>
	/// <returns>A copy of the node from which it is called.</returns>
	object ICloneable.Clone()
	{
		return CloneNode(deep: true);
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.XmlNode.GetEnumerator" />.</summary>
	/// <returns>Returns an enumerator for the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new XmlChildEnumerator(this);
	}

	/// <summary>Get an enumerator that iterates through the child nodes in the current node.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the child nodes in the current node.</returns>
	public IEnumerator GetEnumerator()
	{
		return new XmlChildEnumerator(this);
	}

	private void AppendChildText(StringBuilder builder)
	{
		for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.FirstChild == null)
			{
				if (xmlNode.NodeType == XmlNodeType.Text || xmlNode.NodeType == XmlNodeType.CDATA || xmlNode.NodeType == XmlNodeType.Whitespace || xmlNode.NodeType == XmlNodeType.SignificantWhitespace)
				{
					builder.Append(xmlNode.InnerText);
				}
			}
			else
			{
				xmlNode.AppendChildText(builder);
			}
		}
	}

	/// <summary>Saves the current node to the specified <see cref="T:System.Xml.XmlWriter" />, when overridden in a derived class.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public abstract void WriteTo(XmlWriter w);

	/// <summary>Saves all the child nodes of the node to the specified <see cref="T:System.Xml.XmlWriter" />, when overridden in a derived class.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public abstract void WriteContentTo(XmlWriter w);

	/// <summary>Removes all the child nodes and/or attributes of the current node.</summary>
	public virtual void RemoveAll()
	{
		XmlNode xmlNode = FirstChild;
		while (xmlNode != null)
		{
			XmlNode nextSibling = xmlNode.NextSibling;
			RemoveChild(xmlNode);
			xmlNode = nextSibling;
		}
	}

	/// <summary>Looks up the closest xmlns declaration for the given prefix that is in scope for the current node and returns the namespace URI in the declaration.</summary>
	/// <returns>The namespace URI of the specified prefix.</returns>
	/// <param name="prefix">The prefix whose namespace URI you want to find. </param>
	public virtual string GetNamespaceOfPrefix(string prefix)
	{
		string namespaceOfPrefixStrict = GetNamespaceOfPrefixStrict(prefix);
		if (namespaceOfPrefixStrict == null)
		{
			return string.Empty;
		}
		return namespaceOfPrefixStrict;
	}

	internal string GetNamespaceOfPrefixStrict(string prefix)
	{
		XmlDocument document = Document;
		if (document != null)
		{
			prefix = document.NameTable.Get(prefix);
			if (prefix == null)
			{
				return null;
			}
			XmlNode xmlNode = this;
			while (xmlNode != null)
			{
				if (xmlNode.NodeType == XmlNodeType.Element)
				{
					XmlElement xmlElement = (XmlElement)xmlNode;
					if (xmlElement.HasAttributes)
					{
						XmlAttributeCollection attributes = xmlElement.Attributes;
						if (prefix.Length == 0)
						{
							for (int i = 0; i < attributes.Count; i++)
							{
								XmlAttribute xmlAttribute = attributes[i];
								if (xmlAttribute.Prefix.Length == 0 && Ref.Equal(xmlAttribute.LocalName, document.strXmlns))
								{
									return xmlAttribute.Value;
								}
							}
						}
						else
						{
							for (int j = 0; j < attributes.Count; j++)
							{
								XmlAttribute xmlAttribute2 = attributes[j];
								if (Ref.Equal(xmlAttribute2.Prefix, document.strXmlns))
								{
									if (Ref.Equal(xmlAttribute2.LocalName, prefix))
									{
										return xmlAttribute2.Value;
									}
								}
								else if (Ref.Equal(xmlAttribute2.Prefix, prefix))
								{
									return xmlAttribute2.NamespaceURI;
								}
							}
						}
					}
					if (Ref.Equal(xmlNode.Prefix, prefix))
					{
						return xmlNode.NamespaceURI;
					}
					xmlNode = xmlNode.ParentNode;
				}
				else
				{
					xmlNode = ((xmlNode.NodeType != XmlNodeType.Attribute) ? xmlNode.ParentNode : ((XmlAttribute)xmlNode).OwnerElement);
				}
			}
			if (Ref.Equal(document.strXml, prefix))
			{
				return document.strReservedXml;
			}
			if (Ref.Equal(document.strXmlns, prefix))
			{
				return document.strReservedXmlns;
			}
		}
		return null;
	}

	/// <summary>Looks up the closest xmlns declaration for the given namespace URI that is in scope for the current node and returns the prefix defined in that declaration.</summary>
	/// <returns>The prefix for the specified namespace URI.</returns>
	/// <param name="namespaceURI">The namespace URI whose prefix you want to find. </param>
	public virtual string GetPrefixOfNamespace(string namespaceURI)
	{
		string prefixOfNamespaceStrict = GetPrefixOfNamespaceStrict(namespaceURI);
		if (prefixOfNamespaceStrict == null)
		{
			return string.Empty;
		}
		return prefixOfNamespaceStrict;
	}

	internal string GetPrefixOfNamespaceStrict(string namespaceURI)
	{
		XmlDocument document = Document;
		if (document != null)
		{
			namespaceURI = document.NameTable.Add(namespaceURI);
			XmlNode xmlNode = this;
			while (xmlNode != null)
			{
				if (xmlNode.NodeType == XmlNodeType.Element)
				{
					XmlElement xmlElement = (XmlElement)xmlNode;
					if (xmlElement.HasAttributes)
					{
						XmlAttributeCollection attributes = xmlElement.Attributes;
						for (int i = 0; i < attributes.Count; i++)
						{
							XmlAttribute xmlAttribute = attributes[i];
							if (xmlAttribute.Prefix.Length == 0)
							{
								if (Ref.Equal(xmlAttribute.LocalName, document.strXmlns) && xmlAttribute.Value == namespaceURI)
								{
									return string.Empty;
								}
							}
							else if (Ref.Equal(xmlAttribute.Prefix, document.strXmlns))
							{
								if (xmlAttribute.Value == namespaceURI)
								{
									return xmlAttribute.LocalName;
								}
							}
							else if (Ref.Equal(xmlAttribute.NamespaceURI, namespaceURI))
							{
								return xmlAttribute.Prefix;
							}
						}
					}
					if (Ref.Equal(xmlNode.NamespaceURI, namespaceURI))
					{
						return xmlNode.Prefix;
					}
					xmlNode = xmlNode.ParentNode;
				}
				else
				{
					xmlNode = ((xmlNode.NodeType != XmlNodeType.Attribute) ? xmlNode.ParentNode : ((XmlAttribute)xmlNode).OwnerElement);
				}
			}
			if (Ref.Equal(document.strReservedXml, namespaceURI))
			{
				return document.strXml;
			}
			if (Ref.Equal(document.strReservedXmlns, namespaceURI))
			{
				return document.strXmlns;
			}
		}
		return null;
	}

	internal virtual void SetParent(XmlNode node)
	{
		if (node == null)
		{
			parentNode = OwnerDocument;
		}
		else
		{
			parentNode = node;
		}
	}

	internal virtual void SetParentForLoad(XmlNode node)
	{
		parentNode = node;
	}

	internal static void SplitName(string name, out string prefix, out string localName)
	{
		int num = name.IndexOf(':');
		if (-1 == num || num == 0 || name.Length - 1 == num)
		{
			prefix = string.Empty;
			localName = name;
		}
		else
		{
			prefix = name.Substring(0, num);
			localName = name.Substring(num + 1);
		}
	}

	internal virtual XmlNode FindChild(XmlNodeType type)
	{
		for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.NodeType == type)
			{
				return xmlNode;
			}
		}
		return null;
	}

	internal virtual XmlNodeChangedEventArgs GetEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action)
	{
		XmlDocument ownerDocument = OwnerDocument;
		if (ownerDocument != null)
		{
			if (!ownerDocument.IsLoading && ((newParent != null && newParent.IsReadOnly) || (oldParent != null && oldParent.IsReadOnly)))
			{
				throw new InvalidOperationException(Res.GetString("This node is read-only. It cannot be modified."));
			}
			return ownerDocument.GetEventArgs(node, oldParent, newParent, oldValue, newValue, action);
		}
		return null;
	}

	internal virtual void BeforeEvent(XmlNodeChangedEventArgs args)
	{
		if (args != null)
		{
			OwnerDocument.BeforeEvent(args);
		}
	}

	internal virtual void AfterEvent(XmlNodeChangedEventArgs args)
	{
		if (args != null)
		{
			OwnerDocument.AfterEvent(args);
		}
	}

	internal virtual string GetXPAttribute(string localName, string namespaceURI)
	{
		return string.Empty;
	}

	internal static void NestTextNodes(XmlNode prevNode, XmlNode nextNode)
	{
		nextNode.parentNode = prevNode;
	}

	internal static void UnnestTextNodes(XmlNode prevNode, XmlNode nextNode)
	{
		nextNode.parentNode = prevNode.ParentNode;
	}
}
