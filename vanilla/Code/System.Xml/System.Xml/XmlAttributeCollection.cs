using System.Collections;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Xml;

/// <summary>Represents a collection of attributes that can be accessed by name or index.</summary>
public sealed class XmlAttributeCollection : XmlNamedNodeMap, ICollection, IEnumerable
{
	/// <summary>Gets the attribute with the specified index.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlAttribute" /> at the specified index.</returns>
	/// <param name="i">The index of the attribute. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">The index being passed in is out of range. </exception>
	[IndexerName("ItemOf")]
	public XmlAttribute this[int i]
	{
		get
		{
			try
			{
				return (XmlAttribute)nodes[i];
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new IndexOutOfRangeException(Res.GetString("The index being passed in is out of range."));
			}
		}
	}

	/// <summary>Gets the attribute with the specified name.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlAttribute" /> with the specified name. If the attribute does not exist, this property returns null.</returns>
	/// <param name="name">The qualified name of the attribute. </param>
	[IndexerName("ItemOf")]
	public XmlAttribute this[string name]
	{
		get
		{
			int hashCode = XmlName.GetHashCode(name);
			for (int i = 0; i < nodes.Count; i++)
			{
				XmlAttribute xmlAttribute = (XmlAttribute)nodes[i];
				if (hashCode == xmlAttribute.LocalNameHash && name == xmlAttribute.Name)
				{
					return xmlAttribute;
				}
			}
			return null;
		}
	}

	/// <summary>Gets the attribute with the specified local name and namespace Uniform Resource Identifier (URI).</summary>
	/// <returns>The <see cref="T:System.Xml.XmlAttribute" /> with the specified local name and namespace URI. If the attribute does not exist, this property returns null.</returns>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute. </param>
	[IndexerName("ItemOf")]
	public XmlAttribute this[string localName, string namespaceURI]
	{
		get
		{
			int hashCode = XmlName.GetHashCode(localName);
			for (int i = 0; i < nodes.Count; i++)
			{
				XmlAttribute xmlAttribute = (XmlAttribute)nodes[i];
				if (hashCode == xmlAttribute.LocalNameHash && localName == xmlAttribute.LocalName && namespaceURI == xmlAttribute.NamespaceURI)
				{
					return xmlAttribute;
				}
			}
			return null;
		}
	}

	/// <summary>For a description of this member, see <see cref="P:System.Xml.XmlAttributeCollection.System.Collections.ICollection.IsSynchronized" />.</summary>
	/// <returns>Returns true if the collection is synchronized.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.XmlAttributeCollection.System.Collections.ICollection.SyncRoot" />.</summary>
	/// <returns>Returns the <see cref="T:System.Object" /> that is the root of the collection.</returns>
	object ICollection.SyncRoot => this;

	/// <summary>For a description of this member, see <see cref="P:System.Xml.XmlAttributeCollection.System.Collections.ICollection.Count" />.</summary>
	/// <returns>Returns an int that contains the count of the attributes.</returns>
	int ICollection.Count => base.Count;

	internal XmlAttributeCollection(XmlNode parent)
		: base(parent)
	{
	}

	internal int FindNodeOffset(XmlAttribute node)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)nodes[i];
			if (xmlAttribute.LocalNameHash == node.LocalNameHash && xmlAttribute.Name == node.Name && xmlAttribute.NamespaceURI == node.NamespaceURI)
			{
				return i;
			}
		}
		return -1;
	}

	internal int FindNodeOffsetNS(XmlAttribute node)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			XmlAttribute xmlAttribute = (XmlAttribute)nodes[i];
			if (xmlAttribute.LocalNameHash == node.LocalNameHash && xmlAttribute.LocalName == node.LocalName && xmlAttribute.NamespaceURI == node.NamespaceURI)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Adds a <see cref="T:System.Xml.XmlNode" /> using its <see cref="P:System.Xml.XmlNode.Name" /> property </summary>
	/// <returns>If the <paramref name="node" /> replaces an existing node with the same name, the old node is returned; otherwise, the added node is returned.</returns>
	/// <param name="node">An attribute node to store in this collection. The node will later be accessible using the name of the node. If a node with that name is already present in the collection, it is replaced by the new one; otherwise, the node is appended to the end of the collection. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="node" /> was created from a different <see cref="T:System.Xml.XmlDocument" /> than the one that created this collection.This XmlAttributeCollection is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="node" /> is an <see cref="T:System.Xml.XmlAttribute" /> that is already an attribute of another <see cref="T:System.Xml.XmlElement" /> object. To re-use attributes in other elements, you must clone the XmlAttribute objects you want to re-use. </exception>
	public override XmlNode SetNamedItem(XmlNode node)
	{
		if (node != null && !(node is XmlAttribute))
		{
			throw new ArgumentException(Res.GetString("An 'Attributes' collection can only contain 'Attribute' objects."));
		}
		int num = FindNodeOffset(node.LocalName, node.NamespaceURI);
		if (num == -1)
		{
			return InternalAppendAttribute((XmlAttribute)node);
		}
		XmlNode result = base.RemoveNodeAt(num);
		InsertNodeAt(num, node);
		return result;
	}

	/// <summary>Inserts the specified attribute as the first node in the collection.</summary>
	/// <returns>The XmlAttribute added to the collection.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlAttribute" /> to insert. </param>
	public XmlAttribute Prepend(XmlAttribute node)
	{
		if (node.OwnerDocument != null && node.OwnerDocument != parent.OwnerDocument)
		{
			throw new ArgumentException(Res.GetString("The named node is from a different document context."));
		}
		if (node.OwnerElement != null)
		{
			Detach(node);
		}
		RemoveDuplicateAttribute(node);
		InsertNodeAt(0, node);
		return node;
	}

	/// <summary>Inserts the specified attribute as the last node in the collection.</summary>
	/// <returns>The XmlAttribute to append to the collection.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlAttribute" /> to insert. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="node" /> was created from a document different from the one that created this collection. </exception>
	public XmlAttribute Append(XmlAttribute node)
	{
		XmlDocument ownerDocument = node.OwnerDocument;
		if (ownerDocument == null || !ownerDocument.IsLoading)
		{
			if (ownerDocument != null && ownerDocument != parent.OwnerDocument)
			{
				throw new ArgumentException(Res.GetString("The named node is from a different document context."));
			}
			if (node.OwnerElement != null)
			{
				Detach(node);
			}
			AddNode(node);
		}
		else
		{
			base.AddNodeForLoad(node, ownerDocument);
			InsertParentIntoElementIdAttrMap(node);
		}
		return node;
	}

	/// <summary>Inserts the specified attribute immediately before the specified reference attribute.</summary>
	/// <returns>The XmlAttribute to insert into the collection.</returns>
	/// <param name="newNode">The <see cref="T:System.Xml.XmlAttribute" /> to insert. </param>
	/// <param name="refNode">The <see cref="T:System.Xml.XmlAttribute" /> that is the reference attribute. <paramref name="newNode" /> is placed before the <paramref name="refNode" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newNode" /> was created from a document different from the one that created this collection. Or the <paramref name="refNode" /> is not a member of this collection. </exception>
	public XmlAttribute InsertBefore(XmlAttribute newNode, XmlAttribute refNode)
	{
		if (newNode == refNode)
		{
			return newNode;
		}
		if (refNode == null)
		{
			return Append(newNode);
		}
		if (refNode.OwnerElement != parent)
		{
			throw new ArgumentException(Res.GetString("The reference node must be a child of the current node."));
		}
		if (newNode.OwnerDocument != null && newNode.OwnerDocument != parent.OwnerDocument)
		{
			throw new ArgumentException(Res.GetString("The named node is from a different document context."));
		}
		if (newNode.OwnerElement != null)
		{
			Detach(newNode);
		}
		int num = FindNodeOffset(refNode.LocalName, refNode.NamespaceURI);
		int num2 = RemoveDuplicateAttribute(newNode);
		if (num2 >= 0 && num2 < num)
		{
			num--;
		}
		InsertNodeAt(num, newNode);
		return newNode;
	}

	/// <summary>Inserts the specified attribute immediately after the specified reference attribute.</summary>
	/// <returns>The XmlAttribute to insert into the collection.</returns>
	/// <param name="newNode">The <see cref="T:System.Xml.XmlAttribute" /> to insert. </param>
	/// <param name="refNode">The <see cref="T:System.Xml.XmlAttribute" /> that is the reference attribute. <paramref name="newNode" /> is placed after the <paramref name="refNode" />. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="newNode" /> was created from a document different from the one that created this collection. Or the <paramref name="refNode" /> is not a member of this collection. </exception>
	public XmlAttribute InsertAfter(XmlAttribute newNode, XmlAttribute refNode)
	{
		if (newNode == refNode)
		{
			return newNode;
		}
		if (refNode == null)
		{
			return Prepend(newNode);
		}
		if (refNode.OwnerElement != parent)
		{
			throw new ArgumentException(Res.GetString("The reference node must be a child of the current node."));
		}
		if (newNode.OwnerDocument != null && newNode.OwnerDocument != parent.OwnerDocument)
		{
			throw new ArgumentException(Res.GetString("The named node is from a different document context."));
		}
		if (newNode.OwnerElement != null)
		{
			Detach(newNode);
		}
		int num = FindNodeOffset(refNode.LocalName, refNode.NamespaceURI);
		int num2 = RemoveDuplicateAttribute(newNode);
		if (num2 >= 0 && num2 < num)
		{
			num--;
		}
		InsertNodeAt(num + 1, newNode);
		return newNode;
	}

	/// <summary>Removes the specified attribute from the collection.</summary>
	/// <returns>The node removed or null if it is not found in the collection.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlAttribute" /> to remove. </param>
	public XmlAttribute Remove(XmlAttribute node)
	{
		int count = nodes.Count;
		for (int i = 0; i < count; i++)
		{
			if (nodes[i] == node)
			{
				RemoveNodeAt(i);
				return node;
			}
		}
		return null;
	}

	/// <summary>Removes the attribute corresponding to the specified index from the collection.</summary>
	/// <returns>Returns null if there is no attribute at the specified index.</returns>
	/// <param name="i">The index of the node to remove. The first node has index 0. </param>
	public XmlAttribute RemoveAt(int i)
	{
		if (i < 0 || i >= Count)
		{
			return null;
		}
		return (XmlAttribute)RemoveNodeAt(i);
	}

	/// <summary>Removes all attributes from the collection.</summary>
	public void RemoveAll()
	{
		int num = Count;
		while (num > 0)
		{
			num--;
			RemoveAt(num);
		}
	}

	/// <summary>For a description of this member, see <see cref="M:System.Xml.XmlAttributeCollection.CopyTo(System.Xml.XmlAttribute[],System.Int32)" />.</summary>
	/// <param name="array">The array that is the destination of the objects copied from this collection. </param>
	/// <param name="index">The index in the array where copying begins. </param>
	void ICollection.CopyTo(Array array, int index)
	{
		int num = 0;
		int count = Count;
		while (num < count)
		{
			array.SetValue(nodes[num], index);
			num++;
			index++;
		}
	}

	/// <summary>Copies all the <see cref="T:System.Xml.XmlAttribute" /> objects from this collection into the given array.</summary>
	/// <param name="array">The array that is the destination of the objects copied from this collection. </param>
	/// <param name="index">The index in the array where copying begins. </param>
	public void CopyTo(XmlAttribute[] array, int index)
	{
		int num = 0;
		int count = Count;
		while (num < count)
		{
			array[index] = (XmlAttribute)((XmlNode)nodes[num]).CloneNode(deep: true);
			num++;
			index++;
		}
	}

	internal override XmlNode AddNode(XmlNode node)
	{
		RemoveDuplicateAttribute((XmlAttribute)node);
		XmlNode result = base.AddNode(node);
		InsertParentIntoElementIdAttrMap((XmlAttribute)node);
		return result;
	}

	internal override XmlNode InsertNodeAt(int i, XmlNode node)
	{
		XmlNode result = base.InsertNodeAt(i, node);
		InsertParentIntoElementIdAttrMap((XmlAttribute)node);
		return result;
	}

	internal override XmlNode RemoveNodeAt(int i)
	{
		XmlNode xmlNode = base.RemoveNodeAt(i);
		RemoveParentFromElementIdAttrMap((XmlAttribute)xmlNode);
		XmlAttribute defaultAttribute = parent.OwnerDocument.GetDefaultAttribute((XmlElement)parent, xmlNode.Prefix, xmlNode.LocalName, xmlNode.NamespaceURI);
		if (defaultAttribute != null)
		{
			InsertNodeAt(i, defaultAttribute);
		}
		return xmlNode;
	}

	internal void Detach(XmlAttribute attr)
	{
		attr.OwnerElement.Attributes.Remove(attr);
	}

	internal void InsertParentIntoElementIdAttrMap(XmlAttribute attr)
	{
		if (parent is XmlElement xmlElement && parent.OwnerDocument != null)
		{
			XmlName iDInfoByElement = parent.OwnerDocument.GetIDInfoByElement(xmlElement.XmlName);
			if (iDInfoByElement != null && iDInfoByElement.Prefix == attr.XmlName.Prefix && iDInfoByElement.LocalName == attr.XmlName.LocalName)
			{
				parent.OwnerDocument.AddElementWithId(attr.Value, xmlElement);
			}
		}
	}

	internal void RemoveParentFromElementIdAttrMap(XmlAttribute attr)
	{
		if (parent is XmlElement xmlElement && parent.OwnerDocument != null)
		{
			XmlName iDInfoByElement = parent.OwnerDocument.GetIDInfoByElement(xmlElement.XmlName);
			if (iDInfoByElement != null && iDInfoByElement.Prefix == attr.XmlName.Prefix && iDInfoByElement.LocalName == attr.XmlName.LocalName)
			{
				parent.OwnerDocument.RemoveElementWithId(attr.Value, xmlElement);
			}
		}
	}

	internal int RemoveDuplicateAttribute(XmlAttribute attr)
	{
		int num = FindNodeOffset(attr.LocalName, attr.NamespaceURI);
		if (num != -1)
		{
			XmlAttribute attr2 = (XmlAttribute)nodes[num];
			base.RemoveNodeAt(num);
			RemoveParentFromElementIdAttrMap(attr2);
		}
		return num;
	}

	internal bool PrepareParentInElementIdAttrMap(string attrPrefix, string attrLocalName)
	{
		XmlElement xmlElement = parent as XmlElement;
		XmlName iDInfoByElement = parent.OwnerDocument.GetIDInfoByElement(xmlElement.XmlName);
		if (iDInfoByElement != null && iDInfoByElement.Prefix == attrPrefix && iDInfoByElement.LocalName == attrLocalName)
		{
			return true;
		}
		return false;
	}

	internal void ResetParentInElementIdAttrMap(string oldVal, string newVal)
	{
		XmlElement elem = parent as XmlElement;
		XmlDocument ownerDocument = parent.OwnerDocument;
		ownerDocument.RemoveElementWithId(oldVal, elem);
		ownerDocument.AddElementWithId(newVal, elem);
	}

	internal XmlAttribute InternalAppendAttribute(XmlAttribute node)
	{
		XmlNode xmlNode = base.AddNode(node);
		InsertParentIntoElementIdAttrMap(node);
		return (XmlAttribute)xmlNode;
	}

	internal XmlAttributeCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
