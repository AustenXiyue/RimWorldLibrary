using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents an attribute. Valid and default values for the attribute are defined in a document type definition (DTD) or schema.</summary>
public class XmlAttribute : XmlNode
{
	private XmlName name;

	private XmlLinkedNode lastChild;

	internal int LocalNameHash => name.HashCode;

	internal XmlName XmlName
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	/// <summary>Gets the parent of this node. For XmlAttribute nodes, this property always returns null.</summary>
	/// <returns>For XmlAttribute nodes, this property always returns null.</returns>
	public override XmlNode ParentNode => null;

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>The qualified name of the attribute node.</returns>
	public override string Name => name.Name;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>The name of the attribute node with the prefix removed. In the following example &lt;book bk:genre= 'novel'&gt;, the LocalName of the attribute is genre.</returns>
	public override string LocalName => name.LocalName;

	/// <summary>Gets the namespace URI of this node.</summary>
	/// <returns>The namespace URI of this node. If the attribute is not explicitly given a namespace, this property returns String.Empty.</returns>
	public override string NamespaceURI => name.NamespaceURI;

	/// <summary>Gets or sets the namespace prefix of this node.</summary>
	/// <returns>The namespace prefix of this node. If there is no prefix, this property returns String.Empty.</returns>
	/// <exception cref="T:System.ArgumentException">This node is read-only.</exception>
	/// <exception cref="T:System.Xml.XmlException">The specified prefix contains an invalid character.The specified prefix is malformed.The namespaceURI of this node is null.The specified prefix is "xml", and the namespaceURI of this node is different from "http://www.w3.org/XML/1998/namespace".This node is an attribute, the specified prefix is "xmlns", and the namespaceURI of this node is different from "http://www.w3.org/2000/xmlns/".This node is an attribute, and the qualifiedName of this node is "xmlns" [Namespaces].</exception>
	public override string Prefix
	{
		get
		{
			return name.Prefix;
		}
		set
		{
			name = name.OwnerDocument.AddAttrXmlName(value, LocalName, NamespaceURI, SchemaInfo);
		}
	}

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>The node type for XmlAttribute nodes is XmlNodeType.Attribute.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Attribute;

	/// <summary>Gets the <see cref="T:System.Xml.XmlDocument" /> to which this node belongs.</summary>
	/// <returns>An XML document to which this node belongs.</returns>
	public override XmlDocument OwnerDocument => name.OwnerDocument;

	/// <summary>Gets or sets the value of the node.</summary>
	/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node. For XmlAttribute nodes, this property is the value of attribute.</returns>
	/// <exception cref="T:System.ArgumentException">The node is read-only and a set operation is called.</exception>
	public override string Value
	{
		get
		{
			return InnerText;
		}
		set
		{
			InnerText = value;
		}
	}

	/// <summary>Gets the post-schema-validation-infoset that has been assigned to this node as a result of schema validation.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> containing the post-schema-validation-infoset of this node.</returns>
	public override IXmlSchemaInfo SchemaInfo => name;

	/// <summary>Sets the concatenated values of the node and all its children.</summary>
	/// <returns>The concatenated values of the node and all its children. For attribute nodes, this property has the same functionality as the <see cref="P:System.Xml.XmlAttribute.Value" /> property.</returns>
	public override string InnerText
	{
		set
		{
			if (PrepareOwnerElementInElementIdAttrMap())
			{
				string innerText = base.InnerText;
				base.InnerText = value;
				ResetOwnerElementInElementIdAttrMap(innerText);
			}
			else
			{
				base.InnerText = value;
			}
		}
	}

	internal override bool IsContainer => true;

	internal override XmlLinkedNode LastNode
	{
		get
		{
			return lastChild;
		}
		set
		{
			lastChild = value;
		}
	}

	/// <summary>Gets a value indicating whether the attribute value was explicitly set.</summary>
	/// <returns>true if this attribute was explicitly given a value in the original instance document; otherwise, false. A value of false indicates that the value of the attribute came from the DTD.</returns>
	public virtual bool Specified => true;

	/// <summary>Gets the <see cref="T:System.Xml.XmlElement" /> to which the attribute belongs.</summary>
	/// <returns>The XmlElement that the attribute belongs to or null if this attribute is not part of an XmlElement.</returns>
	public virtual XmlElement OwnerElement => parentNode as XmlElement;

	/// <summary>Sets the value of the attribute.</summary>
	/// <returns>The attribute value.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML specified when setting this property is not well-formed.</exception>
	public override string InnerXml
	{
		set
		{
			RemoveAll();
			new XmlLoader().LoadInnerXmlAttribute(this, value);
		}
	}

	/// <summary>Gets the base Uniform Resource Identifier (URI) of the node.</summary>
	/// <returns>The location from which the node was loaded or String.Empty if the node has no base URI. Attribute nodes have the same base URI as their owner element. If an attribute node does not have an owner element, BaseURI returns String.Empty.</returns>
	public override string BaseURI
	{
		get
		{
			if (OwnerElement != null)
			{
				return OwnerElement.BaseURI;
			}
			return string.Empty;
		}
	}

	internal override XmlSpace XmlSpace
	{
		get
		{
			if (OwnerElement != null)
			{
				return OwnerElement.XmlSpace;
			}
			return XmlSpace.None;
		}
	}

	internal override string XmlLang
	{
		get
		{
			if (OwnerElement != null)
			{
				return OwnerElement.XmlLang;
			}
			return string.Empty;
		}
	}

	internal override XPathNodeType XPNodeType
	{
		get
		{
			if (IsNamespace)
			{
				return XPathNodeType.Namespace;
			}
			return XPathNodeType.Attribute;
		}
	}

	internal override string XPLocalName
	{
		get
		{
			if (name.Prefix.Length == 0 && name.LocalName == "xmlns")
			{
				return string.Empty;
			}
			return name.LocalName;
		}
	}

	internal bool IsNamespace => Ref.Equal(name.NamespaceURI, name.OwnerDocument.strReservedXmlns);

	internal XmlAttribute(XmlName name, XmlDocument doc)
		: base(doc)
	{
		parentNode = null;
		if (!doc.IsLoading)
		{
			XmlDocument.CheckName(name.Prefix);
			XmlDocument.CheckName(name.LocalName);
		}
		if (name.LocalName.Length == 0)
		{
			throw new ArgumentException(Res.GetString("The attribute local name cannot be empty."));
		}
		this.name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlAttribute" /> class.</summary>
	/// <param name="prefix">The namespace prefix.</param>
	/// <param name="localName">The local name of the attribute.</param>
	/// <param name="namespaceURI">The namespace uniform resource identifier (URI).</param>
	/// <param name="doc">The parent XML document.</param>
	protected internal XmlAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc)
		: this(doc.AddAttrXmlName(prefix, localName, namespaceURI, null), doc)
	{
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The duplicate node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself </param>
	public override XmlNode CloneNode(bool deep)
	{
		XmlDocument ownerDocument = OwnerDocument;
		XmlAttribute xmlAttribute = ownerDocument.CreateAttribute(Prefix, LocalName, NamespaceURI);
		xmlAttribute.CopyChildren(ownerDocument, this, deep: true);
		return xmlAttribute;
	}

	internal bool PrepareOwnerElementInElementIdAttrMap()
	{
		if (OwnerDocument.DtdSchemaInfo != null)
		{
			XmlElement ownerElement = OwnerElement;
			if (ownerElement != null)
			{
				return ownerElement.Attributes.PrepareParentInElementIdAttrMap(Prefix, LocalName);
			}
		}
		return false;
	}

	internal void ResetOwnerElementInElementIdAttrMap(string oldInnerText)
	{
		OwnerElement?.Attributes.ResetParentInElementIdAttrMap(oldInnerText, InnerText);
	}

	internal override XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
	{
		XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(newChild, this);
		if (insertEventArgsForLoad != null)
		{
			doc.BeforeEvent(insertEventArgsForLoad);
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		if (lastChild == null)
		{
			xmlLinkedNode.next = xmlLinkedNode;
			lastChild = xmlLinkedNode;
			xmlLinkedNode.SetParentForLoad(this);
		}
		else
		{
			XmlLinkedNode xmlLinkedNode2 = lastChild;
			xmlLinkedNode.next = xmlLinkedNode2.next;
			xmlLinkedNode2.next = xmlLinkedNode;
			lastChild = xmlLinkedNode;
			if (xmlLinkedNode2.IsText && xmlLinkedNode.IsText)
			{
				XmlNode.NestTextNodes(xmlLinkedNode2, xmlLinkedNode);
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

	internal override bool IsValidChildType(XmlNodeType type)
	{
		if (type != XmlNodeType.Text)
		{
			return type == XmlNodeType.EntityReference;
		}
		return true;
	}

	/// <summary>Inserts the specified node immediately before the specified reference node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> inserted.</returns>
	/// <param name="newChild">The <see cref="T:System.Xml.XmlNode" /> to insert.</param>
	/// <param name="refChild">The <see cref="T:System.Xml.XmlNode" /> that is the reference node. The <paramref name="newChild" /> is placed before this node.</param>
	/// <exception cref="T:System.InvalidOperationException">The current node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.The <paramref name="refChild" /> is not a child of this node.This node is read-only.</exception>
	public override XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.InsertBefore(newChild, refChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.InsertBefore(newChild, refChild);
		}
		return result;
	}

	/// <summary>Inserts the specified node immediately after the specified reference node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> inserted.</returns>
	/// <param name="newChild">The <see cref="T:System.Xml.XmlNode" /> to insert.</param>
	/// <param name="refChild">The <see cref="T:System.Xml.XmlNode" /> that is the reference node. The <paramref name="newChild" /> is placed after the <paramref name="refChild" />.</param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.The <paramref name="refChild" /> is not a child of this node.This node is read-only.</exception>
	public override XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.InsertAfter(newChild, refChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.InsertAfter(newChild, refChild);
		}
		return result;
	}

	/// <summary>Replaces the child node specified with the new child node specified.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> replaced.</returns>
	/// <param name="newChild">The new child <see cref="T:System.Xml.XmlNode" />.</param>
	/// <param name="oldChild">The <see cref="T:System.Xml.XmlNode" /> to replace.</param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only.The <paramref name="oldChild" /> is not a child of this node.</exception>
	public override XmlNode ReplaceChild(XmlNode newChild, XmlNode oldChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.ReplaceChild(newChild, oldChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.ReplaceChild(newChild, oldChild);
		}
		return result;
	}

	/// <summary>Removes the specified child node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> removed.</returns>
	/// <param name="oldChild">The <see cref="T:System.Xml.XmlNode" /> to remove.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="oldChild" /> is not a child of this node. Or this node is read-only.</exception>
	public override XmlNode RemoveChild(XmlNode oldChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.RemoveChild(oldChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.RemoveChild(oldChild);
		}
		return result;
	}

	/// <summary>Adds the specified node to the beginning of the list of child nodes for this node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> added.</returns>
	/// <param name="newChild">The <see cref="T:System.Xml.XmlNode" /> to add. If it is an <see cref="T:System.Xml.XmlDocumentFragment" />, the entire contents of the document fragment are moved into the child list of this node.</param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only.</exception>
	public override XmlNode PrependChild(XmlNode newChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.PrependChild(newChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.PrependChild(newChild);
		}
		return result;
	}

	/// <summary>Adds the specified node to the end of the list of child nodes, of this node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> added.</returns>
	/// <param name="newChild">The <see cref="T:System.Xml.XmlNode" /> to add.</param>
	/// <exception cref="T:System.InvalidOperationException">This node is of a type that does not allow child nodes of the type of the <paramref name="newChild" /> node.The <paramref name="newChild" /> is an ancestor of this node.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newChild" /> was created from a different document than the one that created this node.This node is read-only.</exception>
	public override XmlNode AppendChild(XmlNode newChild)
	{
		XmlNode result;
		if (PrepareOwnerElementInElementIdAttrMap())
		{
			string innerText = InnerText;
			result = base.AppendChild(newChild);
			ResetOwnerElementInElementIdAttrMap(innerText);
		}
		else
		{
			result = base.AppendChild(newChild);
		}
		return result;
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save.</param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteStartAttribute(Prefix, LocalName, NamespaceURI);
		WriteContentTo(w);
		w.WriteEndAttribute();
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save.</param>
	public override void WriteContentTo(XmlWriter w)
	{
		for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			xmlNode.WriteTo(w);
		}
	}

	internal override void SetParent(XmlNode node)
	{
		parentNode = node;
	}
}
