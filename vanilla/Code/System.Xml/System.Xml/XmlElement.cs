using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents an element.</summary>
public class XmlElement : XmlLinkedNode
{
	private XmlName name;

	private XmlAttributeCollection attributes;

	private XmlLinkedNode lastChild;

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

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>The qualified name of the node. For XmlElement nodes, this is the tag name of the element.</returns>
	public override string Name => name.Name;

	/// <summary>Gets the local name of the current node.</summary>
	/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.</returns>
	public override string LocalName => name.LocalName;

	/// <summary>Gets the namespace URI of this node.</summary>
	/// <returns>The namespace URI of this node. If there is no namespace URI, this property returns String.Empty.</returns>
	public override string NamespaceURI => name.NamespaceURI;

	/// <summary>Gets or sets the namespace prefix of this node.</summary>
	/// <returns>The namespace prefix of this node. If there is no prefix, this property returns String.Empty.</returns>
	/// <exception cref="T:System.ArgumentException">This node is read-only </exception>
	/// <exception cref="T:System.Xml.XmlException">The specified prefix contains an invalid character.The specified prefix is malformed.The namespaceURI of this node is null.The specified prefix is "xml" and the namespaceURI of this node is different from http://www.w3.org/XML/1998/namespace. </exception>
	public override string Prefix
	{
		get
		{
			return name.Prefix;
		}
		set
		{
			name = name.OwnerDocument.AddXmlName(value, LocalName, NamespaceURI, SchemaInfo);
		}
	}

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>The node type. For XmlElement nodes, this value is XmlNodeType.Element.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Element;

	public override XmlNode ParentNode => parentNode;

	/// <summary>Gets the <see cref="T:System.Xml.XmlDocument" /> to which this node belongs.</summary>
	/// <returns>The XmlDocument to which this element belongs.</returns>
	public override XmlDocument OwnerDocument => name.OwnerDocument;

	internal override bool IsContainer => true;

	/// <summary>Gets or sets the tag format of the element.</summary>
	/// <returns>Returns true if the element is to be serialized in the short tag format "&lt;item/&gt;"; false for the long format "&lt;item&gt;&lt;/item&gt;".When setting this property, if set to true, the children of the element are removed and the element is serialized in the short tag format. If set to false, the value of the property is changed (regardless of whether or not the element has content); if the element is empty, it is serialized in the long format.This property is a Microsoft extension to the Document Object Model (DOM).</returns>
	public bool IsEmpty
	{
		get
		{
			return lastChild == this;
		}
		set
		{
			if (value)
			{
				if (lastChild != this)
				{
					RemoveAllChildren();
					lastChild = this;
				}
			}
			else if (lastChild == this)
			{
				lastChild = null;
			}
		}
	}

	internal override XmlLinkedNode LastNode
	{
		get
		{
			if (lastChild != this)
			{
				return lastChild;
			}
			return null;
		}
		set
		{
			lastChild = value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Xml.XmlAttributeCollection" /> containing the list of attributes for this node.</summary>
	/// <returns>
	///   <see cref="T:System.Xml.XmlAttributeCollection" /> containing the list of attributes for this node.</returns>
	public override XmlAttributeCollection Attributes
	{
		get
		{
			if (attributes == null)
			{
				lock (OwnerDocument.objLock)
				{
					if (attributes == null)
					{
						attributes = new XmlAttributeCollection(this);
					}
				}
			}
			return attributes;
		}
	}

	/// <summary>Gets a boolean value indicating whether the current node has any attributes.</summary>
	/// <returns>true if the current node has attributes; otherwise, false.</returns>
	public virtual bool HasAttributes
	{
		get
		{
			if (attributes == null)
			{
				return false;
			}
			return attributes.Count > 0;
		}
	}

	/// <summary>Gets the post schema validation infoset that has been assigned to this node as a result of schema validation.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object containing the post schema validation infoset of this node.</returns>
	public override IXmlSchemaInfo SchemaInfo => name;

	/// <summary>Gets or sets the markup representing just the children of this node.</summary>
	/// <returns>The markup of the children of this node.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML specified when setting this property is not well-formed. </exception>
	public override string InnerXml
	{
		get
		{
			return base.InnerXml;
		}
		set
		{
			RemoveAllChildren();
			new XmlLoader().LoadInnerXmlElement(this, value);
		}
	}

	/// <summary>Gets or sets the concatenated values of the node and all its children.</summary>
	/// <returns>The concatenated values of the node and all its children.</returns>
	public override string InnerText
	{
		get
		{
			return base.InnerText;
		}
		set
		{
			XmlLinkedNode lastNode = LastNode;
			if (lastNode != null && lastNode.NodeType == XmlNodeType.Text && lastNode.next == lastNode)
			{
				lastNode.Value = value;
				return;
			}
			RemoveAllChildren();
			AppendChild(OwnerDocument.CreateTextNode(value));
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlNode" /> immediately following this element.</summary>
	/// <returns>The XmlNode immediately following this element.</returns>
	public override XmlNode NextSibling
	{
		get
		{
			if (parentNode != null && parentNode.LastNode != this)
			{
				return next;
			}
			return null;
		}
	}

	internal override XPathNodeType XPNodeType => XPathNodeType.Element;

	internal override string XPLocalName => LocalName;

	internal XmlElement(XmlName name, bool empty, XmlDocument doc)
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
			throw new ArgumentException(Res.GetString("The local name for elements or attributes cannot be null or an empty string."));
		}
		this.name = name;
		if (empty)
		{
			lastChild = this;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlElement" /> class.</summary>
	/// <param name="prefix">The namespace prefix; see the <see cref="P:System.Xml.XmlElement.Prefix" /> property.</param>
	/// <param name="localName">The local name; see the <see cref="P:System.Xml.XmlElement.LocalName" /> property.</param>
	/// <param name="namespaceURI">The namespace URI; see the <see cref="P:System.Xml.XmlElement.NamespaceURI" /> property.</param>
	/// <param name="doc">The parent XML document.</param>
	protected internal XmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc)
		: this(doc.AddXmlName(prefix, localName, namespaceURI, null), empty: true, doc)
	{
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself (and its attributes if the node is an XmlElement). </param>
	public override XmlNode CloneNode(bool deep)
	{
		XmlDocument ownerDocument = OwnerDocument;
		bool isLoading = ownerDocument.IsLoading;
		ownerDocument.IsLoading = true;
		XmlElement xmlElement = ownerDocument.CreateElement(Prefix, LocalName, NamespaceURI);
		ownerDocument.IsLoading = isLoading;
		if (xmlElement.IsEmpty != IsEmpty)
		{
			xmlElement.IsEmpty = IsEmpty;
		}
		if (HasAttributes)
		{
			foreach (XmlAttribute attribute in Attributes)
			{
				XmlAttribute xmlAttribute2 = (XmlAttribute)attribute.CloneNode(deep: true);
				if (attribute is XmlUnspecifiedAttribute && !attribute.Specified)
				{
					((XmlUnspecifiedAttribute)xmlAttribute2).SetSpecified(f: false);
				}
				xmlElement.Attributes.InternalAppendAttribute(xmlAttribute2);
			}
		}
		if (deep)
		{
			xmlElement.CopyChildren(ownerDocument, this, deep);
		}
		return xmlElement;
	}

	internal override XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
	{
		XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(newChild, this);
		if (insertEventArgsForLoad != null)
		{
			doc.BeforeEvent(insertEventArgsForLoad);
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		if (lastChild == null || lastChild == this)
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
		switch (type)
		{
		case XmlNodeType.Element:
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.EntityReference:
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			return true;
		default:
			return false;
		}
	}

	/// <summary>Returns the value for the attribute with the specified name.</summary>
	/// <returns>The value of the specified attribute. An empty string is returned if a matching attribute is not found or if the attribute does not have a specified or default value.</returns>
	/// <param name="name">The name of the attribute to retrieve. This is a qualified name. It is matched against the Name property of the matching node. </param>
	public virtual string GetAttribute(string name)
	{
		XmlAttribute attributeNode = GetAttributeNode(name);
		if (attributeNode != null)
		{
			return attributeNode.Value;
		}
		return string.Empty;
	}

	/// <summary>Sets the value of the attribute with the specified name.</summary>
	/// <param name="name">The name of the attribute to create or alter. This is a qualified name. If the name contains a colon it is parsed into prefix and local name components. </param>
	/// <param name="value">The value to set for the attribute. </param>
	/// <exception cref="T:System.Xml.XmlException">The specified name contains an invalid character. </exception>
	/// <exception cref="T:System.ArgumentException">The node is read-only. </exception>
	public virtual void SetAttribute(string name, string value)
	{
		XmlAttribute attributeNode = GetAttributeNode(name);
		if (attributeNode == null)
		{
			attributeNode = OwnerDocument.CreateAttribute(name);
			attributeNode.Value = value;
			Attributes.InternalAppendAttribute(attributeNode);
		}
		else
		{
			attributeNode.Value = value;
		}
	}

	/// <summary>Removes an attribute by name.</summary>
	/// <param name="name">The name of the attribute to remove.This is a qualified name. It is matched against the Name property of the matching node. </param>
	/// <exception cref="T:System.ArgumentException">The node is read-only. </exception>
	public virtual void RemoveAttribute(string name)
	{
		if (HasAttributes)
		{
			Attributes.RemoveNamedItem(name);
		}
	}

	/// <summary>Returns the XmlAttribute with the specified name.</summary>
	/// <returns>The specified XmlAttribute or null if a matching attribute was not found.</returns>
	/// <param name="name">The name of the attribute to retrieve. This is a qualified name. It is matched against the Name property of the matching node. </param>
	public virtual XmlAttribute GetAttributeNode(string name)
	{
		if (HasAttributes)
		{
			return Attributes[name];
		}
		return null;
	}

	/// <summary>Adds the specified <see cref="T:System.Xml.XmlAttribute" />.</summary>
	/// <returns>If the attribute replaces an existing attribute with the same name, the old XmlAttribute is returned; otherwise, null is returned.</returns>
	/// <param name="newAttr">The XmlAttribute node to add to the attribute collection for this element. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newAttr" /> was created from a different document than the one that created this node. Or this node is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="newAttr" /> is already an attribute of another XmlElement object. You must explicitly clone XmlAttribute nodes to re-use them in other XmlElement objects. </exception>
	public virtual XmlAttribute SetAttributeNode(XmlAttribute newAttr)
	{
		if (newAttr.OwnerElement != null)
		{
			throw new InvalidOperationException(Res.GetString("The 'Attribute' node cannot be inserted because it is already an attribute of another element."));
		}
		return (XmlAttribute)Attributes.SetNamedItem(newAttr);
	}

	/// <summary>Removes the specified <see cref="T:System.Xml.XmlAttribute" />.</summary>
	/// <returns>The removed XmlAttribute or null if <paramref name="oldAttr" /> is not an attribute node of the XmlElement.</returns>
	/// <param name="oldAttr">The XmlAttribute node to remove. If the removed attribute has a default value, it is immediately replaced. </param>
	/// <exception cref="T:System.ArgumentException">This node is read-only. </exception>
	public virtual XmlAttribute RemoveAttributeNode(XmlAttribute oldAttr)
	{
		if (HasAttributes)
		{
			return Attributes.Remove(oldAttr);
		}
		return null;
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlNodeList" /> containing a list of all descendant elements that match the specified <see cref="P:System.Xml.XmlElement.Name" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a list of all matching nodes.</returns>
	/// <param name="name">The name tag to match. This is a qualified name. It is matched against the Name property of the matching node. The asterisk (*) is a special value that matches all tags. </param>
	public virtual XmlNodeList GetElementsByTagName(string name)
	{
		return new XmlElementList(this, name);
	}

	/// <summary>Returns the value for the attribute with the specified local name and namespace URI.</summary>
	/// <returns>The value of the specified attribute. An empty string is returned if a matching attribute is not found or if the attribute does not have a specified or default value.</returns>
	/// <param name="localName">The local name of the attribute to retrieve. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute to retrieve. </param>
	public virtual string GetAttribute(string localName, string namespaceURI)
	{
		XmlAttribute attributeNode = GetAttributeNode(localName, namespaceURI);
		if (attributeNode != null)
		{
			return attributeNode.Value;
		}
		return string.Empty;
	}

	/// <summary>Sets the value of the attribute with the specified local name and namespace URI.</summary>
	/// <returns>The attribute value.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	/// <param name="value">The value to set for the attribute. </param>
	public virtual string SetAttribute(string localName, string namespaceURI, string value)
	{
		XmlAttribute attributeNode = GetAttributeNode(localName, namespaceURI);
		if (attributeNode == null)
		{
			attributeNode = OwnerDocument.CreateAttribute(string.Empty, localName, namespaceURI);
			attributeNode.Value = value;
			Attributes.InternalAppendAttribute(attributeNode);
		}
		else
		{
			attributeNode.Value = value;
		}
		return value;
	}

	/// <summary>Removes an attribute with the specified local name and namespace URI. (If the removed attribute has a default value, it is immediately replaced).</summary>
	/// <param name="localName">The local name of the attribute to remove. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute to remove. </param>
	/// <exception cref="T:System.ArgumentException">The node is read-only. </exception>
	public virtual void RemoveAttribute(string localName, string namespaceURI)
	{
		RemoveAttributeNode(localName, namespaceURI);
	}

	/// <summary>Returns the <see cref="T:System.Xml.XmlAttribute" /> with the specified local name and namespace URI.</summary>
	/// <returns>The specified XmlAttribute or null if a matching attribute was not found.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public virtual XmlAttribute GetAttributeNode(string localName, string namespaceURI)
	{
		if (HasAttributes)
		{
			return Attributes[localName, namespaceURI];
		}
		return null;
	}

	/// <summary>Adds the specified <see cref="T:System.Xml.XmlAttribute" />.</summary>
	/// <returns>The XmlAttribute to add.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	public virtual XmlAttribute SetAttributeNode(string localName, string namespaceURI)
	{
		XmlAttribute xmlAttribute = GetAttributeNode(localName, namespaceURI);
		if (xmlAttribute == null)
		{
			xmlAttribute = OwnerDocument.CreateAttribute(string.Empty, localName, namespaceURI);
			Attributes.InternalAppendAttribute(xmlAttribute);
		}
		return xmlAttribute;
	}

	/// <summary>Removes the <see cref="T:System.Xml.XmlAttribute" /> specified by the local name and namespace URI. (If the removed attribute has a default value, it is immediately replaced).</summary>
	/// <returns>The removed XmlAttribute or null if the XmlElement does not have a matching attribute node.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	/// <exception cref="T:System.ArgumentException">This node is read-only. </exception>
	public virtual XmlAttribute RemoveAttributeNode(string localName, string namespaceURI)
	{
		if (HasAttributes)
		{
			XmlAttribute attributeNode = GetAttributeNode(localName, namespaceURI);
			Attributes.Remove(attributeNode);
			return attributeNode;
		}
		return null;
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlNodeList" /> containing a list of all descendant elements that match the specified <see cref="P:System.Xml.XmlElement.LocalName" /> and <see cref="P:System.Xml.XmlElement.NamespaceURI" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a list of all matching nodes.</returns>
	/// <param name="localName">The local name to match. The asterisk (*) is a special value that matches all tags. </param>
	/// <param name="namespaceURI">The namespace URI to match. </param>
	public virtual XmlNodeList GetElementsByTagName(string localName, string namespaceURI)
	{
		return new XmlElementList(this, localName, namespaceURI);
	}

	/// <summary>Determines whether the current node has an attribute with the specified name.</summary>
	/// <returns>true if the current node has the specified attribute; otherwise, false.</returns>
	/// <param name="name">The name of the attribute to find. This is a qualified name. It is matched against the Name property of the matching node. </param>
	public virtual bool HasAttribute(string name)
	{
		return GetAttributeNode(name) != null;
	}

	/// <summary>Determines whether the current node has an attribute with the specified local name and namespace URI.</summary>
	/// <returns>true if the current node has the specified attribute; otherwise, false.</returns>
	/// <param name="localName">The local name of the attribute to find. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute to find. </param>
	public virtual bool HasAttribute(string localName, string namespaceURI)
	{
		return GetAttributeNode(localName, namespaceURI) != null;
	}

	/// <summary>Saves the current node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		if (GetType() == typeof(XmlElement))
		{
			WriteElementTo(w, this);
			return;
		}
		WriteStartElement(w);
		if (IsEmpty)
		{
			w.WriteEndElement();
			return;
		}
		WriteContentTo(w);
		w.WriteFullEndElement();
	}

	private static void WriteElementTo(XmlWriter writer, XmlElement e)
	{
		XmlNode xmlNode = e;
		XmlNode xmlNode2 = e;
		while (true)
		{
			e = xmlNode2 as XmlElement;
			if (e != null && e.GetType() == typeof(XmlElement))
			{
				e.WriteStartElement(writer);
				if (e.IsEmpty)
				{
					writer.WriteEndElement();
				}
				else
				{
					if (e.lastChild != null)
					{
						xmlNode2 = e.FirstChild;
						continue;
					}
					writer.WriteFullEndElement();
				}
			}
			else
			{
				xmlNode2.WriteTo(writer);
			}
			while (xmlNode2 != xmlNode && xmlNode2 == xmlNode2.ParentNode.LastChild)
			{
				xmlNode2 = xmlNode2.ParentNode;
				writer.WriteFullEndElement();
			}
			if (xmlNode2 != xmlNode)
			{
				xmlNode2 = xmlNode2.NextSibling;
				continue;
			}
			break;
		}
	}

	private void WriteStartElement(XmlWriter w)
	{
		w.WriteStartElement(Prefix, LocalName, NamespaceURI);
		if (HasAttributes)
		{
			XmlAttributeCollection xmlAttributeCollection = Attributes;
			for (int i = 0; i < xmlAttributeCollection.Count; i++)
			{
				xmlAttributeCollection[i].WriteTo(w);
			}
		}
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
		for (XmlNode xmlNode = FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			xmlNode.WriteTo(w);
		}
	}

	/// <summary>Removes the attribute node with the specified index from the element. (If the removed attribute has a default value, it is immediately replaced).</summary>
	/// <returns>The attribute node removed or null if there is no node at the given index.</returns>
	/// <param name="i">The index of the node to remove. The first node has index 0. </param>
	public virtual XmlNode RemoveAttributeAt(int i)
	{
		if (HasAttributes)
		{
			return attributes.RemoveAt(i);
		}
		return null;
	}

	/// <summary>Removes all specified attributes from the element. Default attributes are not removed.</summary>
	public virtual void RemoveAllAttributes()
	{
		if (HasAttributes)
		{
			attributes.RemoveAll();
		}
	}

	/// <summary>Removes all specified attributes and children of the current node. Default attributes are not removed.</summary>
	public override void RemoveAll()
	{
		base.RemoveAll();
		RemoveAllAttributes();
	}

	internal void RemoveAllChildren()
	{
		base.RemoveAll();
	}

	internal override void SetParent(XmlNode node)
	{
		parentNode = node;
	}

	internal override string GetXPAttribute(string localName, string ns)
	{
		if (ns == OwnerDocument.strReservedXmlns)
		{
			return null;
		}
		XmlAttribute attributeNode = GetAttributeNode(localName, ns);
		if (attributeNode != null)
		{
			return attributeNode.Value;
		}
		return string.Empty;
	}
}
