using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Xml.Linq;

/// <summary>Represents a node that can contain other nodes.</summary>
/// <filterpriority>2</filterpriority>
public abstract class XContainer : XNode
{
	internal object content;

	/// <summary>Get the first child node of this node.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNode" /> containing the first child node of the <see cref="T:System.Xml.Linq.XContainer" />.</returns>
	/// <filterpriority>2</filterpriority>
	public XNode FirstNode => LastNode?.next;

	/// <summary>Get the last child node of this node.</summary>
	/// <returns>An <see cref="T:System.Xml.Linq.XNode" /> containing the last child node of the <see cref="T:System.Xml.Linq.XContainer" />.</returns>
	/// <filterpriority>2</filterpriority>
	public XNode LastNode
	{
		get
		{
			if (content == null)
			{
				return null;
			}
			if (content is XNode result)
			{
				return result;
			}
			if (content is string text)
			{
				if (text.Length == 0)
				{
					return null;
				}
				XText xText = new XText(text);
				xText.parent = this;
				xText.next = xText;
				Interlocked.CompareExchange<object>(ref content, (object)xText, (object)text);
			}
			return (XNode)content;
		}
	}

	internal XContainer()
	{
	}

	internal XContainer(XContainer other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other.content is string)
		{
			content = other.content;
			return;
		}
		XNode xNode = (XNode)other.content;
		if (xNode != null)
		{
			do
			{
				xNode = xNode.next;
				AppendNodeSkipNotify(xNode.CloneNode());
			}
			while (xNode != other.content);
		}
	}

	/// <summary>Adds the specified content as children of this <see cref="T:System.Xml.Linq.XContainer" />.</summary>
	/// <param name="content">A content object containing simple content or a collection of content objects to be added.</param>
	public void Add(object content)
	{
		if (SkipNotify())
		{
			AddContentSkipNotify(content);
		}
		else
		{
			if (content == null)
			{
				return;
			}
			if (content is XNode n)
			{
				AddNode(n);
				return;
			}
			if (content is string s)
			{
				AddString(s);
				return;
			}
			if (content is XAttribute a)
			{
				AddAttribute(a);
				return;
			}
			if (content is XStreamingElement other)
			{
				AddNode(new XElement(other));
				return;
			}
			if (content is object[] array)
			{
				object[] array2 = array;
				foreach (object obj in array2)
				{
					Add(obj);
				}
				return;
			}
			if (content is IEnumerable enumerable)
			{
				{
					foreach (object item in enumerable)
					{
						Add(item);
					}
					return;
				}
			}
			AddString(GetStringValue(content));
		}
	}

	/// <summary>Adds the specified content as children of this <see cref="T:System.Xml.Linq.XContainer" />.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	public void Add(params object[] content)
	{
		Add((object)content);
	}

	/// <summary>Adds the specified content as the first children of this document or element.</summary>
	/// <param name="content">A content object containing simple content or a collection of content objects to be added.</param>
	public void AddFirst(object content)
	{
		new Inserter(this, null).Add(content);
	}

	/// <summary>Adds the specified content as the first children of this document or element.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	/// <exception cref="T:System.InvalidOperationException">The parent is null.</exception>
	public void AddFirst(params object[] content)
	{
		AddFirst((object)content);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlWriter" /> that can be used to add nodes to the <see cref="T:System.Xml.Linq.XContainer" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlWriter" /> that is ready to have content written to it.</returns>
	/// <filterpriority>2</filterpriority>
	public XmlWriter CreateWriter()
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.ConformanceLevel = ((!(this is XDocument)) ? ConformanceLevel.Fragment : ConformanceLevel.Document);
		return XmlWriter.Create(new XNodeBuilder(this), xmlWriterSettings);
	}

	/// <summary>Returns a collection of the descendant nodes for this document or element, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> containing the descendant nodes of the <see cref="T:System.Xml.Linq.XContainer" />, in document order.</returns>
	public IEnumerable<XNode> DescendantNodes()
	{
		return GetDescendantNodes(self: false);
	}

	/// <summary>Returns a collection of the descendant elements for this document or element, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> containing the descendant elements of the <see cref="T:System.Xml.Linq.XContainer" />.</returns>
	public IEnumerable<XElement> Descendants()
	{
		return GetDescendants(null, self: false);
	}

	/// <summary>Returns a filtered collection of the descendant elements for this document or element, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> containing the descendant elements of the <see cref="T:System.Xml.Linq.XContainer" /> that match the specified <see cref="T:System.Xml.Linq.XName" />.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> Descendants(XName name)
	{
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetDescendants(name, self: false);
	}

	/// <summary>Gets the first (in document order) child element with the specified <see cref="T:System.Xml.Linq.XName" />.</summary>
	/// <returns>A <see cref="T:System.Xml.Linq.XElement" /> that matches the specified <see cref="T:System.Xml.Linq.XName" />, or null.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public XElement Element(XName name)
	{
		XNode xNode = content as XNode;
		if (xNode != null)
		{
			do
			{
				xNode = xNode.next;
				if (xNode is XElement xElement && xElement.name == name)
				{
					return xElement;
				}
			}
			while (xNode != content);
		}
		return null;
	}

	/// <summary>Returns a collection of the child elements of this element or document, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> containing the child elements of this <see cref="T:System.Xml.Linq.XContainer" />, in document order.</returns>
	public IEnumerable<XElement> Elements()
	{
		return GetElements(null);
	}

	/// <summary>Returns a filtered collection of the child elements of this element or document, in document order. Only elements that have a matching <see cref="T:System.Xml.Linq.XName" /> are included in the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XElement" /> containing the children of the <see cref="T:System.Xml.Linq.XContainer" /> that have a matching <see cref="T:System.Xml.Linq.XName" />, in document order.</returns>
	/// <param name="name">The <see cref="T:System.Xml.Linq.XName" /> to match.</param>
	public IEnumerable<XElement> Elements(XName name)
	{
		if (!(name != null))
		{
			return XElement.EmptySequence;
		}
		return GetElements(name);
	}

	/// <summary>Returns a collection of the child nodes of this element or document, in document order.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Xml.Linq.XNode" /> containing the contents of this <see cref="T:System.Xml.Linq.XContainer" />, in document order.</returns>
	public IEnumerable<XNode> Nodes()
	{
		XNode n = LastNode;
		if (n != null)
		{
			do
			{
				n = n.next;
				yield return n;
			}
			while (n.parent == this && n != content);
		}
	}

	/// <summary>Removes the child nodes from this document or element.</summary>
	public void RemoveNodes()
	{
		if (SkipNotify())
		{
			RemoveNodesSkipNotify();
			return;
		}
		while (content != null)
		{
			if (content is string text)
			{
				if (text.Length > 0)
				{
					ConvertTextToNode();
				}
				else if (this is XElement)
				{
					NotifyChanging(this, XObjectChangeEventArgs.Value);
					if (text != content)
					{
						throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
					}
					content = null;
					NotifyChanged(this, XObjectChangeEventArgs.Value);
				}
				else
				{
					content = null;
				}
			}
			if (content is XNode { next: var xNode2 } xNode)
			{
				NotifyChanging(xNode2, XObjectChangeEventArgs.Remove);
				if (xNode != content || xNode2 != xNode.next)
				{
					throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
				}
				if (xNode2 != xNode)
				{
					xNode.next = xNode2.next;
				}
				else
				{
					content = null;
				}
				xNode2.parent = null;
				xNode2.next = null;
				NotifyChanged(xNode2, XObjectChangeEventArgs.Remove);
			}
		}
	}

	/// <summary>Replaces the children nodes of this document or element with the specified content.</summary>
	/// <param name="content">A content object containing simple content or a collection of content objects that replace the children nodes.</param>
	public void ReplaceNodes(object content)
	{
		content = GetContentSnapshot(content);
		RemoveNodes();
		Add(content);
	}

	/// <summary>Replaces the children nodes of this document or element with the specified content.</summary>
	/// <param name="content">A parameter list of content objects.</param>
	public void ReplaceNodes(params object[] content)
	{
		ReplaceNodes((object)content);
	}

	internal virtual void AddAttribute(XAttribute a)
	{
	}

	internal virtual void AddAttributeSkipNotify(XAttribute a)
	{
	}

	internal void AddContentSkipNotify(object content)
	{
		if (content == null)
		{
			return;
		}
		if (content is XNode n)
		{
			AddNodeSkipNotify(n);
			return;
		}
		if (content is string s)
		{
			AddStringSkipNotify(s);
			return;
		}
		if (content is XAttribute a)
		{
			AddAttributeSkipNotify(a);
			return;
		}
		if (content is XStreamingElement other)
		{
			AddNodeSkipNotify(new XElement(other));
			return;
		}
		if (content is object[] array)
		{
			object[] array2 = array;
			foreach (object obj in array2)
			{
				AddContentSkipNotify(obj);
			}
			return;
		}
		if (content is IEnumerable enumerable)
		{
			{
				foreach (object item in enumerable)
				{
					AddContentSkipNotify(item);
				}
				return;
			}
		}
		AddStringSkipNotify(GetStringValue(content));
	}

	internal void AddNode(XNode n)
	{
		ValidateNode(n, this);
		if (n.parent != null)
		{
			n = n.CloneNode();
		}
		else
		{
			XNode xNode = this;
			while (xNode.parent != null)
			{
				xNode = xNode.parent;
			}
			if (n == xNode)
			{
				n = n.CloneNode();
			}
		}
		ConvertTextToNode();
		AppendNode(n);
	}

	internal void AddNodeSkipNotify(XNode n)
	{
		ValidateNode(n, this);
		if (n.parent != null)
		{
			n = n.CloneNode();
		}
		else
		{
			XNode xNode = this;
			while (xNode.parent != null)
			{
				xNode = xNode.parent;
			}
			if (n == xNode)
			{
				n = n.CloneNode();
			}
		}
		ConvertTextToNode();
		AppendNodeSkipNotify(n);
	}

	internal void AddString(string s)
	{
		ValidateString(s);
		if (content == null)
		{
			if (s.Length > 0)
			{
				AppendNode(new XText(s));
			}
			else if (this is XElement)
			{
				NotifyChanging(this, XObjectChangeEventArgs.Value);
				if (content != null)
				{
					throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
				}
				content = s;
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
			else
			{
				content = s;
			}
		}
		else if (s.Length > 0)
		{
			ConvertTextToNode();
			if (content is XText xText && !(xText is XCData))
			{
				xText.Value += s;
			}
			else
			{
				AppendNode(new XText(s));
			}
		}
	}

	internal void AddStringSkipNotify(string s)
	{
		ValidateString(s);
		if (content == null)
		{
			content = s;
		}
		else if (s.Length > 0)
		{
			if (content is string)
			{
				content = (string)content + s;
			}
			else if (content is XText xText && !(xText is XCData))
			{
				xText.text += s;
			}
			else
			{
				AppendNodeSkipNotify(new XText(s));
			}
		}
	}

	internal void AppendNode(XNode n)
	{
		bool num = NotifyChanging(n, XObjectChangeEventArgs.Add);
		if (n.parent != null)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
		}
		AppendNodeSkipNotify(n);
		if (num)
		{
			NotifyChanged(n, XObjectChangeEventArgs.Add);
		}
	}

	internal void AppendNodeSkipNotify(XNode n)
	{
		n.parent = this;
		if (content == null || content is string)
		{
			n.next = n;
		}
		else
		{
			XNode xNode = (XNode)content;
			n.next = xNode.next;
			xNode.next = n;
		}
		content = n;
	}

	internal override void AppendText(StringBuilder sb)
	{
		if (content is string value)
		{
			sb.Append(value);
			return;
		}
		XNode xNode = (XNode)content;
		if (xNode != null)
		{
			do
			{
				xNode = xNode.next;
				xNode.AppendText(sb);
			}
			while (xNode != content);
		}
	}

	private string GetTextOnly()
	{
		if (content == null)
		{
			return null;
		}
		string text = content as string;
		if (text == null)
		{
			XNode xNode = (XNode)content;
			do
			{
				xNode = xNode.next;
				if (xNode.NodeType != XmlNodeType.Text)
				{
					return null;
				}
				text += ((XText)xNode).Value;
			}
			while (xNode != content);
		}
		return text;
	}

	private string CollectText(ref XNode n)
	{
		string text = "";
		while (n != null && n.NodeType == XmlNodeType.Text)
		{
			text += ((XText)n).Value;
			n = ((n != content) ? n.next : null);
		}
		return text;
	}

	internal bool ContentsEqual(XContainer e)
	{
		if (content == e.content)
		{
			return true;
		}
		string textOnly = GetTextOnly();
		if (textOnly != null)
		{
			return textOnly == e.GetTextOnly();
		}
		XNode xNode = content as XNode;
		XNode xNode2 = e.content as XNode;
		if (xNode != null && xNode2 != null)
		{
			xNode = xNode.next;
			xNode2 = xNode2.next;
			while (!(CollectText(ref xNode) != e.CollectText(ref xNode2)))
			{
				if (xNode == null && xNode2 == null)
				{
					return true;
				}
				if (xNode == null || xNode2 == null || !xNode.DeepEquals(xNode2))
				{
					break;
				}
				xNode = ((xNode != content) ? xNode.next : null);
				xNode2 = ((xNode2 != e.content) ? xNode2.next : null);
			}
		}
		return false;
	}

	internal int ContentsHashCode()
	{
		string textOnly = GetTextOnly();
		if (textOnly != null)
		{
			return textOnly.GetHashCode();
		}
		int num = 0;
		XNode n = content as XNode;
		if (n != null)
		{
			do
			{
				n = n.next;
				string text = CollectText(ref n);
				if (text.Length > 0)
				{
					num ^= text.GetHashCode();
				}
				if (n == null)
				{
					break;
				}
				num ^= n.GetDeepHashCode();
			}
			while (n != content);
		}
		return num;
	}

	internal void ConvertTextToNode()
	{
		if (content is string { Length: >0 } text)
		{
			XText xText = new XText(text);
			xText.parent = this;
			xText.next = xText;
			content = xText;
		}
	}

	internal static string GetDateTimeString(DateTime value)
	{
		return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
	}

	internal IEnumerable<XNode> GetDescendantNodes(bool self)
	{
		if (self)
		{
			yield return this;
		}
		XNode n = this;
		while (true)
		{
			XNode firstNode;
			if (n is XContainer xContainer && (firstNode = xContainer.FirstNode) != null)
			{
				n = firstNode;
			}
			else
			{
				while (n != null && n != this && n == n.parent.content)
				{
					n = n.parent;
				}
				if (n == null || n == this)
				{
					break;
				}
				n = n.next;
			}
			yield return n;
		}
	}

	internal IEnumerable<XElement> GetDescendants(XName name, bool self)
	{
		if (self)
		{
			XElement xElement = (XElement)this;
			if (name == null || xElement.name == name)
			{
				yield return xElement;
			}
		}
		XNode n = this;
		XContainer xContainer = this;
		while (true)
		{
			if (xContainer != null && xContainer.content is XNode)
			{
				n = ((XNode)xContainer.content).next;
			}
			else
			{
				while (n != this && n == n.parent.content)
				{
					n = n.parent;
				}
				if (n == this)
				{
					break;
				}
				n = n.next;
			}
			XElement e = n as XElement;
			if (e != null && (name == null || e.name == name))
			{
				yield return e;
			}
			xContainer = e;
		}
	}

	private IEnumerable<XElement> GetElements(XName name)
	{
		XNode n = content as XNode;
		if (n == null)
		{
			yield break;
		}
		do
		{
			n = n.next;
			if (n is XElement xElement && (name == null || xElement.name == name))
			{
				yield return xElement;
			}
		}
		while (n.parent == this && n != content);
	}

	internal static string GetStringValue(object value)
	{
		string text;
		if (value is string)
		{
			text = (string)value;
		}
		else if (value is double)
		{
			text = XmlConvert.ToString((double)value);
		}
		else if (value is float)
		{
			text = XmlConvert.ToString((float)value);
		}
		else if (value is decimal)
		{
			text = XmlConvert.ToString((decimal)value);
		}
		else if (value is bool)
		{
			text = XmlConvert.ToString((bool)value);
		}
		else if (value is DateTime)
		{
			text = GetDateTimeString((DateTime)value);
		}
		else if (value is DateTimeOffset)
		{
			text = XmlConvert.ToString((DateTimeOffset)value);
		}
		else if (value is TimeSpan)
		{
			text = XmlConvert.ToString((TimeSpan)value);
		}
		else
		{
			if (value is XObject)
			{
				throw new ArgumentException(Res.GetString("Argument_XObjectValue"));
			}
			text = value.ToString();
		}
		if (text == null)
		{
			throw new ArgumentException(Res.GetString("Argument_ConvertToString"));
		}
		return text;
	}

	internal void ReadContentFrom(XmlReader r)
	{
		if (r.ReadState != ReadState.Interactive)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedInteractive"));
		}
		XContainer xContainer = this;
		NamespaceCache namespaceCache = default(NamespaceCache);
		NamespaceCache namespaceCache2 = default(NamespaceCache);
		do
		{
			switch (r.NodeType)
			{
			case XmlNodeType.Element:
			{
				XElement xElement = new XElement(namespaceCache.Get(r.NamespaceURI).GetName(r.LocalName));
				if (r.MoveToFirstAttribute())
				{
					do
					{
						xElement.AppendAttributeSkipNotify(new XAttribute(namespaceCache2.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value));
					}
					while (r.MoveToNextAttribute());
					r.MoveToElement();
				}
				xContainer.AddNodeSkipNotify(xElement);
				if (!r.IsEmptyElement)
				{
					xContainer = xElement;
				}
				break;
			}
			case XmlNodeType.EndElement:
				if (xContainer.content == null)
				{
					xContainer.content = string.Empty;
				}
				if (xContainer == this)
				{
					return;
				}
				xContainer = xContainer.parent;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				xContainer.AddStringSkipNotify(r.Value);
				break;
			case XmlNodeType.CDATA:
				xContainer.AddNodeSkipNotify(new XCData(r.Value));
				break;
			case XmlNodeType.Comment:
				xContainer.AddNodeSkipNotify(new XComment(r.Value));
				break;
			case XmlNodeType.ProcessingInstruction:
				xContainer.AddNodeSkipNotify(new XProcessingInstruction(r.Name, r.Value));
				break;
			case XmlNodeType.DocumentType:
				xContainer.AddNodeSkipNotify(new XDocumentType(r.LocalName, r.GetAttribute("PUBLIC"), r.GetAttribute("SYSTEM"), r.Value, r.DtdInfo));
				break;
			case XmlNodeType.EntityReference:
				if (!r.CanResolveEntity)
				{
					throw new InvalidOperationException(Res.GetString("InvalidOperation_UnresolvedEntityReference"));
				}
				r.ResolveEntity();
				break;
			default:
				throw new InvalidOperationException(Res.GetString("InvalidOperation_UnexpectedNodeType", r.NodeType));
			case XmlNodeType.EndEntity:
				break;
			}
		}
		while (r.Read());
	}

	internal void ReadContentFrom(XmlReader r, LoadOptions o)
	{
		if ((o & (LoadOptions.SetBaseUri | LoadOptions.SetLineInfo)) == 0)
		{
			ReadContentFrom(r);
			return;
		}
		if (r.ReadState != ReadState.Interactive)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExpectedInteractive"));
		}
		XContainer xContainer = this;
		XNode xNode = null;
		NamespaceCache namespaceCache = default(NamespaceCache);
		NamespaceCache namespaceCache2 = default(NamespaceCache);
		string text = (((o & LoadOptions.SetBaseUri) != 0) ? r.BaseURI : null);
		IXmlLineInfo xmlLineInfo = (((o & LoadOptions.SetLineInfo) != 0) ? (r as IXmlLineInfo) : null);
		do
		{
			string baseURI = r.BaseURI;
			switch (r.NodeType)
			{
			case XmlNodeType.Element:
			{
				XElement xElement2 = new XElement(namespaceCache.Get(r.NamespaceURI).GetName(r.LocalName));
				if (text != null && text != baseURI)
				{
					xElement2.SetBaseUri(baseURI);
				}
				if (xmlLineInfo != null && xmlLineInfo.HasLineInfo())
				{
					xElement2.SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}
				if (r.MoveToFirstAttribute())
				{
					do
					{
						XAttribute xAttribute = new XAttribute(namespaceCache2.Get((r.Prefix.Length == 0) ? string.Empty : r.NamespaceURI).GetName(r.LocalName), r.Value);
						if (xmlLineInfo != null && xmlLineInfo.HasLineInfo())
						{
							xAttribute.SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						xElement2.AppendAttributeSkipNotify(xAttribute);
					}
					while (r.MoveToNextAttribute());
					r.MoveToElement();
				}
				xContainer.AddNodeSkipNotify(xElement2);
				if (!r.IsEmptyElement)
				{
					xContainer = xElement2;
					if (text != null)
					{
						text = baseURI;
					}
				}
				break;
			}
			case XmlNodeType.EndElement:
				if (xContainer.content == null)
				{
					xContainer.content = string.Empty;
				}
				if (xContainer is XElement xElement && xmlLineInfo != null && xmlLineInfo.HasLineInfo())
				{
					xElement.SetEndElementLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}
				if (xContainer == this)
				{
					return;
				}
				if (text != null && xContainer.HasBaseUri)
				{
					text = xContainer.parent.BaseUri;
				}
				xContainer = xContainer.parent;
				break;
			case XmlNodeType.Text:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				if ((text != null && text != baseURI) || (xmlLineInfo != null && xmlLineInfo.HasLineInfo()))
				{
					xNode = new XText(r.Value);
				}
				else
				{
					xContainer.AddStringSkipNotify(r.Value);
				}
				break;
			case XmlNodeType.CDATA:
				xNode = new XCData(r.Value);
				break;
			case XmlNodeType.Comment:
				xNode = new XComment(r.Value);
				break;
			case XmlNodeType.ProcessingInstruction:
				xNode = new XProcessingInstruction(r.Name, r.Value);
				break;
			case XmlNodeType.DocumentType:
				xNode = new XDocumentType(r.LocalName, r.GetAttribute("PUBLIC"), r.GetAttribute("SYSTEM"), r.Value, r.DtdInfo);
				break;
			case XmlNodeType.EntityReference:
				if (!r.CanResolveEntity)
				{
					throw new InvalidOperationException(Res.GetString("InvalidOperation_UnresolvedEntityReference"));
				}
				r.ResolveEntity();
				break;
			default:
				throw new InvalidOperationException(Res.GetString("InvalidOperation_UnexpectedNodeType", r.NodeType));
			case XmlNodeType.EndEntity:
				break;
			}
			if (xNode != null)
			{
				if (text != null && text != baseURI)
				{
					xNode.SetBaseUri(baseURI);
				}
				if (xmlLineInfo != null && xmlLineInfo.HasLineInfo())
				{
					xNode.SetLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
				}
				xContainer.AddNodeSkipNotify(xNode);
				xNode = null;
			}
		}
		while (r.Read());
	}

	internal void RemoveNode(XNode n)
	{
		bool flag = NotifyChanging(n, XObjectChangeEventArgs.Remove);
		if (n.parent != this)
		{
			throw new InvalidOperationException(Res.GetString("InvalidOperation_ExternalCode"));
		}
		XNode xNode = (XNode)content;
		while (xNode.next != n)
		{
			xNode = xNode.next;
		}
		if (xNode == n)
		{
			content = null;
		}
		else
		{
			if (content == n)
			{
				content = xNode;
			}
			xNode.next = n.next;
		}
		n.parent = null;
		n.next = null;
		if (flag)
		{
			NotifyChanged(n, XObjectChangeEventArgs.Remove);
		}
	}

	private void RemoveNodesSkipNotify()
	{
		XNode xNode = content as XNode;
		if (xNode != null)
		{
			do
			{
				XNode xNode2 = xNode.next;
				xNode.parent = null;
				xNode.next = null;
				xNode = xNode2;
			}
			while (xNode != content);
		}
		content = null;
	}

	internal virtual void ValidateNode(XNode node, XNode previous)
	{
	}

	internal virtual void ValidateString(string s)
	{
	}

	internal void WriteContentTo(XmlWriter writer)
	{
		if (content == null)
		{
			return;
		}
		if (content is string)
		{
			if (this is XDocument)
			{
				writer.WriteWhitespace((string)content);
			}
			else
			{
				writer.WriteString((string)content);
			}
			return;
		}
		XNode xNode = (XNode)content;
		do
		{
			xNode = xNode.next;
			xNode.WriteTo(writer);
		}
		while (xNode != content);
	}

	private static void AddContentToList(List<object> list, object content)
	{
		IEnumerable enumerable = ((content is string) ? null : (content as IEnumerable));
		if (enumerable == null)
		{
			list.Add(content);
			return;
		}
		foreach (object item in enumerable)
		{
			if (item != null)
			{
				AddContentToList(list, item);
			}
		}
	}

	internal static object GetContentSnapshot(object content)
	{
		if (content is string || !(content is IEnumerable))
		{
			return content;
		}
		List<object> list = new List<object>();
		AddContentToList(list, content);
		return list;
	}
}
