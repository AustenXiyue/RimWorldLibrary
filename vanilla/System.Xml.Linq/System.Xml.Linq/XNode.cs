using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace System.Xml.Linq;

/// <summary>Represents the abstract concept of a node (element, comment, document type, processing instruction, or text node) in the XML tree.  </summary>
/// <filterpriority>2</filterpriority>
public abstract class XNode : XObject
{
	private static XNodeDocumentOrderComparer documentOrderComparer;

	private static XNodeEqualityComparer equalityComparer;

	internal XNode next;

	/// <summary>Gets the next sibling node of this node.</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XNode" /> that contains the next sibling node.</returns>
	/// <filterpriority>2</filterpriority>
	public XNode NextNode
	{
		get
		{
			if (parent != null && this != parent.content)
			{
				return next;
			}
			return null;
		}
	}

	/// <summary>Gets the previous sibling node of this node.</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XNode" /> that contains the previous sibling node.</returns>
	/// <filterpriority>2</filterpriority>
	public XNode PreviousNode
	{
		get
		{
			if (parent == null)
			{
				return null;
			}
			XNode xNode = ((XNode)parent.content).next;
			XNode result = null;
			while (xNode != this)
			{
				result = xNode;
				xNode = xNode.next;
			}
			return result;
		}
	}

	/// <summary>Gets a comparer that can compare the relative position of two nodes.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNodeDocumentOrderComparer" /> that can compare the relative position of two nodes.</returns>
	public static XNodeDocumentOrderComparer DocumentOrderComparer
	{
		get
		{
			if (documentOrderComparer == null)
			{
				documentOrderComparer = new XNodeDocumentOrderComparer();
			}
			return documentOrderComparer;
		}
	}

	/// <summary>Gets a comparer that can compare two nodes for value equality.</summary>
	/// <returns>A <see cref="T:System.Xml.Linq.XNodeEqualityComparer" /> that can compare two nodes for value equality.</returns>
	public static XNodeEqualityComparer EqualityComparer
	{
		get
		{
			if (equalityComparer == null)
			{
				equalityComparer = new XNodeEqualityComparer();
			}
			return equalityComparer;
		}
	}

	internal XNode()
	{
	}

	/// <summary>Adds the specified content immediately after this node.</summary>
	/// <param name="content">A content object that contains simple content or a collection of content objects to be added after this node.</param>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void AddAfterSelf(object content)
	{
		if (parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingParent"));
		}
		new Inserter(parent, this).Add(content);
	}

	/// <summary>Adds the specified content immediately after this node.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void AddAfterSelf(params object[] content)
	{
		AddAfterSelf((object)content);
	}

	/// <summary>Adds the specified content immediately before this node.</summary>
	/// <param name="content">A content object that contains simple content or a collection of content objects to be added before this node.</param>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void AddBeforeSelf(object content)
	{
		if (parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingParent"));
		}
		XNode xNode = (XNode)parent.content;
		while (xNode.next != this)
		{
			xNode = xNode.next;
		}
		if (xNode == parent.content)
		{
			xNode = null;
		}
		new Inserter(parent, xNode).Add(content);
	}

	/// <summary>Adds the specified content immediately before this node.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void AddBeforeSelf(params object[] content)
	{
		AddBeforeSelf((object)content);
	}

	/// <summary>Returns a collection of the ancestor elements of this node.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the ancestor elements of this node.</returns>
	public IEnumerable<XElement> Ancestors()
	{
		return GetAncestors(null, self: false);
	}

	/// <summary>Returns a filtered collection of the ancestor elements of this node. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the ancestor elements of this node. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.The nodes in the returned collection are in reverse document order.This method uses deferred execution.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> Ancestors(XName name)
	{
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetAncestors(name, self: false);
	}

	/// <summary>Compares two nodes to determine their relative XML document order.</summary>
	/// <returns>An int containing 0 if the nodes are equal; -1 if <paramref name="n1" /> is before <paramref name="n2" />; 1 if <paramref name="n1" /> is after <paramref name="n2" />.</returns>
	/// <param name="n1">First <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="n2">Second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <exception cref="T:System.InvalidOperationException">The two nodes do not share a common ancestor.</exception>
	public static int CompareDocumentOrder(XNode n1, XNode n2)
	{
		if (n1 == n2)
		{
			return 0;
		}
		if (n1 == null)
		{
			return -1;
		}
		if (n2 == null)
		{
			return 1;
		}
		if (n1.parent != n2.parent)
		{
			int num = 0;
			XNode xNode = n1;
			while (xNode.parent != null)
			{
				xNode = xNode.parent;
				num++;
			}
			XNode xNode2 = n2;
			while (xNode2.parent != null)
			{
				xNode2 = xNode2.parent;
				num--;
			}
			if (xNode != xNode2)
			{
				throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingAncestor"));
			}
			if (num < 0)
			{
				do
				{
					n2 = n2.parent;
					num++;
				}
				while (num != 0);
				if (n1 == n2)
				{
					return -1;
				}
			}
			else if (num > 0)
			{
				do
				{
					n1 = n1.parent;
					num--;
				}
				while (num != 0);
				if (n1 == n2)
				{
					return 1;
				}
			}
			while (n1.parent != n2.parent)
			{
				n1 = n1.parent;
				n2 = n2.parent;
			}
		}
		else if (n1.parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingAncestor"));
		}
		XNode xNode3 = (XNode)n1.parent.content;
		do
		{
			xNode3 = xNode3.next;
			if (xNode3 == n1)
			{
				return -1;
			}
		}
		while (xNode3 != n2);
		return 1;
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlReader" /> for this node.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> that can be used to read this node and its descendants.</returns>
	/// <filterpriority>2</filterpriority>
	public XmlReader CreateReader()
	{
		return new XNodeReader(this, null);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlReader" /> with the options specified by the <paramref name="readerOptions" /> parameter.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlReader" /> object.</returns>
	/// <param name="readerOptions">A <see cref="T:System.Xml.Linq.ReaderOptions" /> object that specifies whether to omit duplicate namespaces.</param>
	public XmlReader CreateReader(ReaderOptions readerOptions)
	{
		return new XNodeReader(this, null, readerOptions);
	}

	/// <summary>Returns a collection of the sibling nodes after this node, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> of the sibling nodes after this node, in document order.</returns>
	public IEnumerable<XNode> NodesAfterSelf()
	{
		XNode n = this;
		while (n.parent != null && n != n.parent.content)
		{
			n = n.next;
			yield return n;
		}
	}

	/// <summary>Returns a collection of the sibling nodes before this node, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> of the sibling nodes before this node, in document order.</returns>
	public IEnumerable<XNode> NodesBeforeSelf()
	{
		if (parent == null)
		{
			yield break;
		}
		XNode n = (XNode)parent.content;
		do
		{
			n = n.next;
			if (n != this)
			{
				yield return n;
				continue;
			}
			break;
		}
		while (parent != null && parent == n.parent);
	}

	/// <summary>Returns a collection of the sibling elements after this node, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the sibling elements after this node, in document order.</returns>
	public IEnumerable<XElement> ElementsAfterSelf()
	{
		return GetElementsAfterSelf(null);
	}

	/// <summary>Returns a filtered collection of the sibling elements after this node, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the sibling elements after this node, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> ElementsAfterSelf(XName name)
	{
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetElementsAfterSelf(name);
	}

	/// <summary>Returns a collection of the sibling elements before this node, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the sibling elements before this node, in document order.</returns>
	public IEnumerable<XElement> ElementsBeforeSelf()
	{
		return GetElementsBeforeSelf(null);
	}

	/// <summary>Returns a filtered collection of the sibling elements before this node, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> of the sibling elements before this node, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> ElementsBeforeSelf(XName name)
	{
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetElementsBeforeSelf(name);
	}

	/// <summary>Determines if the current node appears after a specified node in terms of document order.</summary>
	/// <returns>true if this node appears after the specified node; otherwise false.</returns>
	/// <param name="node">The <see cref="T:System.Xml.Linq.XNode" /> to compare for document order.</param>
	public bool IsAfter(XNode node)
	{
		return CompareDocumentOrder(this, node) > 0;
	}

	/// <summary>Determines if the current node appears before a specified node in terms of document order.</summary>
	/// <returns>true if this node appears before the specified node; otherwise false.</returns>
	/// <param name="node">The <see cref="T:System.Xml.Linq.XNode" /> to compare for document order.</param>
	public bool IsBefore(XNode node)
	{
		return CompareDocumentOrder(this, node) < 0;
	}

	/// <summary>Creates an <see cref="T:System.Xml.Linq.XNode" /> from an <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNode" /> that contains the node and its descendant nodes that were read from the reader. The runtime type of the node is determined by the node type (<see cref="P:System.Xml.Linq.XObject.NodeType" />) of the first node encountered in the reader.</returns>
	/// <param name="reader">An <see cref="T:System.Xml.XmlReader" /> positioned at the node to read into this <see cref="T:System.Xml.Linq.XNode" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlReader" /> is not positioned on a recognized node type.</exception>
	/// <exception cref="T:System.Xml.XmlException">The underlying <see cref="T:System.Xml.XmlReader" /> throws an exception.</exception>
	/// <filterpriority>2</filterpriority>
	public static XNode ReadFrom(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.ReadState != ReadState.Interactive)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedInteractive"));
		}
		switch (reader.NodeType)
		{
		case XmlNodeType.Text:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			return new XText(reader);
		case XmlNodeType.CDATA:
			return new XCData(reader);
		case XmlNodeType.Comment:
			return new XComment(reader);
		case XmlNodeType.DocumentType:
			return new XDocumentType(reader);
		case XmlNodeType.Element:
			return new XElement(reader);
		case XmlNodeType.ProcessingInstruction:
			return new XProcessingInstruction(reader);
		default:
			throw new InvalidOperationException(Res.GetString("InvalidOperation_UnexpectedNodeType", reader.NodeType));
		}
	}

	/// <summary>Removes this node from its parent.</summary>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void Remove()
	{
		if (parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingParent"));
		}
		parent.RemoveNode(this);
	}

	/// <summary>Replaces this node with the specified content.</summary>
	/// <param name="content">Content that replaces this node.</param>
	public void ReplaceWith(object content)
	{
		if (parent == null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_MissingParent"));
		}
		XContainer xContainer = parent;
		XNode xNode = (XNode)parent.content;
		while (xNode.next != this)
		{
			xNode = xNode.next;
		}
		if (xNode == parent.content)
		{
			xNode = null;
		}
		parent.RemoveNode(this);
		if (xNode != null && xNode.parent != xContainer)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
		}
		new Inserter(xContainer, xNode).Add(content);
	}

	/// <summary>Replaces this node with the specified content.</summary>
	/// <param name="content">A parameter list of the new content.</param>
	public void ReplaceWith(params object[] content)
	{
		ReplaceWith((object)content);
	}

	/// <summary>Returns the indented XML for this node.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the indented XML.</returns>
	public override string ToString()
	{
		return GetXmlString(GetSaveOptionsFromAnnotations());
	}

	/// <summary>Returns the XML for this node, optionally disabling formatting.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the XML.</returns>
	/// <param name="options">A <see cref="T:System.Xml.Linq.SaveOptions" /> that specifies formatting behavior.</param>
	public string ToString(SaveOptions options)
	{
		return GetXmlString(options);
	}

	/// <summary>Compares the values of two nodes, including the values of all descendant nodes.</summary>
	/// <returns>true if the nodes are equal; otherwise false.</returns>
	/// <param name="n1">The first <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	/// <param name="n2">The second <see cref="T:System.Xml.Linq.XNode" /> to compare.</param>
	public static bool DeepEquals(XNode n1, XNode n2)
	{
		if (n1 == n2)
		{
			return true;
		}
		if (n1 == null || n2 == null)
		{
			return false;
		}
		return n1.DeepEquals(n2);
	}

	/// <summary>Writes this node to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public abstract void WriteTo(XmlWriter writer);

	internal virtual void AppendText(StringBuilder sb)
	{
	}

	internal abstract XNode CloneNode();

	internal abstract bool DeepEquals(XNode node);

	internal IEnumerable<XElement> GetAncestors(XName name, bool self)
	{
		for (XElement e = (self ? this : parent) as XElement; e != null; e = e.parent as XElement)
		{
			if (name == null || e.name == name)
			{
				yield return e;
			}
		}
	}

	private IEnumerable<XElement> GetElementsAfterSelf(XName name)
	{
		XNode n = this;
		while (n.parent != null && n != n.parent.content)
		{
			n = n.next;
			if (n is XElement xElement && (name == null || xElement.name == name))
			{
				yield return xElement;
			}
		}
	}

	private IEnumerable<XElement> GetElementsBeforeSelf(XName name)
	{
		if (parent == null)
		{
			yield break;
		}
		XNode n = (XNode)parent.content;
		do
		{
			n = n.next;
			if (n != this)
			{
				if (n is XElement xElement && (name == null || xElement.name == name))
				{
					yield return xElement;
				}
				continue;
			}
			break;
		}
		while (parent != null && parent == n.parent);
	}

	internal abstract int GetDeepHashCode();

	internal static XmlReaderSettings GetXmlReaderSettings(LoadOptions o)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		if ((o & LoadOptions.PreserveWhitespace) == 0)
		{
			xmlReaderSettings.IgnoreWhitespace = true;
		}
		xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
		xmlReaderSettings.MaxCharactersFromEntities = 10000000L;
		xmlReaderSettings.XmlResolver = null;
		return xmlReaderSettings;
	}

	internal static XmlWriterSettings GetXmlWriterSettings(SaveOptions o)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		if ((o & SaveOptions.DisableFormatting) == 0)
		{
			xmlWriterSettings.Indent = true;
		}
		if ((o & SaveOptions.OmitDuplicateNamespaces) != 0)
		{
			xmlWriterSettings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
		}
		return xmlWriterSettings;
	}

	private string GetXmlString(SaveOptions o)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		if ((o & SaveOptions.DisableFormatting) == 0)
		{
			xmlWriterSettings.Indent = true;
		}
		if ((o & SaveOptions.OmitDuplicateNamespaces) != 0)
		{
			xmlWriterSettings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
		}
		if (this is XText)
		{
			xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
		}
		using (XmlWriter writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
		{
			if (this is XDocument xDocument)
			{
				xDocument.WriteContentTo(writer);
			}
			else
			{
				WriteTo(writer);
			}
		}
		return stringWriter.ToString();
	}
}
