using System.Collections;
using Unity;

namespace System.Xml;

/// <summary>Represents a collection of nodes that can be accessed by name or index.</summary>
public class XmlNamedNodeMap : IEnumerable
{
	internal struct SmallXmlNodeList
	{
		private class SingleObjectEnumerator : IEnumerator
		{
			private object loneValue;

			private int position = -1;

			public object Current
			{
				get
				{
					if (position != 0)
					{
						throw new InvalidOperationException();
					}
					return loneValue;
				}
			}

			public SingleObjectEnumerator(object value)
			{
				loneValue = value;
			}

			public bool MoveNext()
			{
				if (position < 0)
				{
					position = 0;
					return true;
				}
				position = 1;
				return false;
			}

			public void Reset()
			{
				position = -1;
			}
		}

		private object field;

		public int Count
		{
			get
			{
				if (field == null)
				{
					return 0;
				}
				if (field is ArrayList arrayList)
				{
					return arrayList.Count;
				}
				return 1;
			}
		}

		public object this[int index]
		{
			get
			{
				if (field == null)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (field is ArrayList arrayList)
				{
					return arrayList[index];
				}
				if (index != 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return field;
			}
		}

		public void Add(object value)
		{
			if (field == null)
			{
				if (value == null)
				{
					ArrayList arrayList = new ArrayList();
					arrayList.Add(null);
					field = arrayList;
				}
				else
				{
					field = value;
				}
			}
			else if (field is ArrayList arrayList2)
			{
				arrayList2.Add(value);
			}
			else
			{
				ArrayList arrayList3 = new ArrayList();
				arrayList3.Add(field);
				arrayList3.Add(value);
				field = arrayList3;
			}
		}

		public void RemoveAt(int index)
		{
			if (field == null)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (field is ArrayList arrayList)
			{
				arrayList.RemoveAt(index);
				return;
			}
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			field = null;
		}

		public void Insert(int index, object value)
		{
			if (field == null)
			{
				if (index != 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				Add(value);
				return;
			}
			if (field is ArrayList arrayList)
			{
				arrayList.Insert(index, value);
				return;
			}
			switch (index)
			{
			case 0:
			{
				ArrayList arrayList2 = new ArrayList();
				arrayList2.Add(value);
				arrayList2.Add(field);
				field = arrayList2;
				break;
			}
			case 1:
			{
				ArrayList arrayList2 = new ArrayList();
				arrayList2.Add(field);
				arrayList2.Add(value);
				field = arrayList2;
				break;
			}
			default:
				throw new ArgumentOutOfRangeException("index");
			}
		}

		public IEnumerator GetEnumerator()
		{
			if (field == null)
			{
				return XmlDocument.EmptyEnumerator;
			}
			if (field is ArrayList arrayList)
			{
				return arrayList.GetEnumerator();
			}
			return new SingleObjectEnumerator(field);
		}
	}

	internal XmlNode parent;

	internal SmallXmlNodeList nodes;

	/// <summary>Gets the number of nodes in the XmlNamedNodeMap.</summary>
	/// <returns>The number of nodes.</returns>
	public virtual int Count => nodes.Count;

	internal XmlNamedNodeMap(XmlNode parent)
	{
		this.parent = parent;
	}

	/// <summary>Retrieves an <see cref="T:System.Xml.XmlNode" /> specified by name.</summary>
	/// <returns>An XmlNode with the specified name or null if a matching node is not found.</returns>
	/// <param name="name">The qualified name of the node to retrieve. It is matched against the <see cref="P:System.Xml.XmlNode.Name" /> property of the matching node.</param>
	public virtual XmlNode GetNamedItem(string name)
	{
		int num = FindNodeOffset(name);
		if (num >= 0)
		{
			return (XmlNode)nodes[num];
		}
		return null;
	}

	/// <summary>Adds an <see cref="T:System.Xml.XmlNode" /> using its <see cref="P:System.Xml.XmlNode.Name" /> property.</summary>
	/// <returns>If the <paramref name="node" /> replaces an existing node with the same name, the old node is returned; otherwise, null is returned.</returns>
	/// <param name="node">An XmlNode to store in the XmlNamedNodeMap. If a node with that name is already present in the map, it is replaced by the new one.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="node" /> was created from a different <see cref="T:System.Xml.XmlDocument" /> than the one that created the XmlNamedNodeMap; or the XmlNamedNodeMap is read-only.</exception>
	public virtual XmlNode SetNamedItem(XmlNode node)
	{
		if (node == null)
		{
			return null;
		}
		int num = FindNodeOffset(node.LocalName, node.NamespaceURI);
		if (num == -1)
		{
			AddNode(node);
			return null;
		}
		return ReplaceNodeAt(num, node);
	}

	/// <summary>Removes the node from the XmlNamedNodeMap.</summary>
	/// <returns>The XmlNode removed from this XmlNamedNodeMap or null if a matching node was not found.</returns>
	/// <param name="name">The qualified name of the node to remove. The name is matched against the <see cref="P:System.Xml.XmlNode.Name" /> property of the matching node.</param>
	public virtual XmlNode RemoveNamedItem(string name)
	{
		int num = FindNodeOffset(name);
		if (num >= 0)
		{
			return RemoveNodeAt(num);
		}
		return null;
	}

	/// <summary>Retrieves the node at the specified index in the XmlNamedNodeMap.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> at the specified index. If <paramref name="index" /> is less than 0 or greater than or equal to the <see cref="P:System.Xml.XmlNamedNodeMap.Count" /> property, null is returned.</returns>
	/// <param name="index">The index position of the node to retrieve from the XmlNamedNodeMap. The index is zero-based; therefore, the index of the first node is 0 and the index of the last node is <see cref="P:System.Xml.XmlNamedNodeMap.Count" /> -1.</param>
	public virtual XmlNode Item(int index)
	{
		if (index < 0 || index >= nodes.Count)
		{
			return null;
		}
		try
		{
			return (XmlNode)nodes[index];
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new IndexOutOfRangeException(Res.GetString("The index being passed in is out of range."));
		}
	}

	/// <summary>Retrieves a node with the matching <see cref="P:System.Xml.XmlNode.LocalName" /> and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNode" /> with the matching local name and namespace URI or null if a matching node was not found.</returns>
	/// <param name="localName">The local name of the node to retrieve.</param>
	/// <param name="namespaceURI">The namespace Uniform Resource Identifier (URI) of the node to retrieve.</param>
	public virtual XmlNode GetNamedItem(string localName, string namespaceURI)
	{
		int num = FindNodeOffset(localName, namespaceURI);
		if (num >= 0)
		{
			return (XmlNode)nodes[num];
		}
		return null;
	}

	/// <summary>Removes a node with the matching <see cref="P:System.Xml.XmlNode.LocalName" /> and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> removed or null if a matching node was not found.</returns>
	/// <param name="localName">The local name of the node to remove.</param>
	/// <param name="namespaceURI">The namespace URI of the node to remove.</param>
	public virtual XmlNode RemoveNamedItem(string localName, string namespaceURI)
	{
		int num = FindNodeOffset(localName, namespaceURI);
		if (num >= 0)
		{
			return RemoveNodeAt(num);
		}
		return null;
	}

	/// <summary>Provides support for the "foreach" style iteration over the collection of nodes in the XmlNamedNodeMap.</summary>
	/// <returns>An enumerator object.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		return nodes.GetEnumerator();
	}

	internal int FindNodeOffset(string name)
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			XmlNode xmlNode = (XmlNode)nodes[i];
			if (name == xmlNode.Name)
			{
				return i;
			}
		}
		return -1;
	}

	internal int FindNodeOffset(string localName, string namespaceURI)
	{
		int count = Count;
		for (int i = 0; i < count; i++)
		{
			XmlNode xmlNode = (XmlNode)nodes[i];
			if (xmlNode.LocalName == localName && xmlNode.NamespaceURI == namespaceURI)
			{
				return i;
			}
		}
		return -1;
	}

	internal virtual XmlNode AddNode(XmlNode node)
	{
		XmlNode oldParent = ((node.NodeType != XmlNodeType.Attribute) ? node.ParentNode : ((XmlAttribute)node).OwnerElement);
		string value = node.Value;
		XmlNodeChangedEventArgs eventArgs = parent.GetEventArgs(node, oldParent, parent, value, value, XmlNodeChangedAction.Insert);
		if (eventArgs != null)
		{
			parent.BeforeEvent(eventArgs);
		}
		nodes.Add(node);
		node.SetParent(parent);
		if (eventArgs != null)
		{
			parent.AfterEvent(eventArgs);
		}
		return node;
	}

	internal virtual XmlNode AddNodeForLoad(XmlNode node, XmlDocument doc)
	{
		XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(node, parent);
		if (insertEventArgsForLoad != null)
		{
			doc.BeforeEvent(insertEventArgsForLoad);
		}
		nodes.Add(node);
		node.SetParent(parent);
		if (insertEventArgsForLoad != null)
		{
			doc.AfterEvent(insertEventArgsForLoad);
		}
		return node;
	}

	internal virtual XmlNode RemoveNodeAt(int i)
	{
		XmlNode xmlNode = (XmlNode)nodes[i];
		string value = xmlNode.Value;
		XmlNodeChangedEventArgs eventArgs = parent.GetEventArgs(xmlNode, parent, null, value, value, XmlNodeChangedAction.Remove);
		if (eventArgs != null)
		{
			parent.BeforeEvent(eventArgs);
		}
		nodes.RemoveAt(i);
		xmlNode.SetParent(null);
		if (eventArgs != null)
		{
			parent.AfterEvent(eventArgs);
		}
		return xmlNode;
	}

	internal XmlNode ReplaceNodeAt(int i, XmlNode node)
	{
		XmlNode result = RemoveNodeAt(i);
		InsertNodeAt(i, node);
		return result;
	}

	internal virtual XmlNode InsertNodeAt(int i, XmlNode node)
	{
		XmlNode oldParent = ((node.NodeType != XmlNodeType.Attribute) ? node.ParentNode : ((XmlAttribute)node).OwnerElement);
		string value = node.Value;
		XmlNodeChangedEventArgs eventArgs = parent.GetEventArgs(node, oldParent, parent, value, value, XmlNodeChangedAction.Insert);
		if (eventArgs != null)
		{
			parent.BeforeEvent(eventArgs);
		}
		nodes.Insert(i, node);
		node.SetParent(parent);
		if (eventArgs != null)
		{
			parent.AfterEvent(eventArgs);
		}
		return node;
	}

	internal XmlNamedNodeMap()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
