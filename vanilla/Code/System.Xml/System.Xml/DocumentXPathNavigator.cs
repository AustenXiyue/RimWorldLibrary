using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml;

internal sealed class DocumentXPathNavigator : XPathNavigator, IHasXmlNode
{
	private XmlDocument document;

	private XmlNode source;

	private int attributeIndex;

	private XmlElement namespaceParent;

	public override XmlNameTable NameTable => document.NameTable;

	public override XPathNodeType NodeType
	{
		get
		{
			CalibrateText();
			return source.XPNodeType;
		}
	}

	public override string LocalName => source.XPLocalName;

	public override string NamespaceURI
	{
		get
		{
			if (source is XmlAttribute { IsNamespace: not false })
			{
				return string.Empty;
			}
			return source.NamespaceURI;
		}
	}

	public override string Name
	{
		get
		{
			switch (source.NodeType)
			{
			case XmlNodeType.Element:
			case XmlNodeType.ProcessingInstruction:
				return source.Name;
			case XmlNodeType.Attribute:
				if (((XmlAttribute)source).IsNamespace)
				{
					string localName = source.LocalName;
					if (Ref.Equal(localName, document.strXmlns))
					{
						return string.Empty;
					}
					return localName;
				}
				return source.Name;
			default:
				return string.Empty;
			}
		}
	}

	public override string Prefix
	{
		get
		{
			if (source is XmlAttribute { IsNamespace: not false })
			{
				return string.Empty;
			}
			return source.Prefix;
		}
	}

	public override string Value
	{
		get
		{
			switch (source.NodeType)
			{
			case XmlNodeType.Element:
			case XmlNodeType.DocumentFragment:
				return source.InnerText;
			case XmlNodeType.Document:
				return ValueDocument;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				return ValueText;
			default:
				return source.Value;
			}
		}
	}

	private string ValueDocument
	{
		get
		{
			XmlElement documentElement = document.DocumentElement;
			if (documentElement != null)
			{
				return documentElement.InnerText;
			}
			return string.Empty;
		}
	}

	private string ValueText
	{
		get
		{
			CalibrateText();
			string text = source.Value;
			XmlNode xmlNode = NextSibling(source);
			if (xmlNode != null && xmlNode.IsText)
			{
				StringBuilder stringBuilder = new StringBuilder(text);
				do
				{
					stringBuilder.Append(xmlNode.Value);
					xmlNode = NextSibling(xmlNode);
				}
				while (xmlNode != null && xmlNode.IsText);
				text = stringBuilder.ToString();
			}
			return text;
		}
	}

	public override string BaseURI => source.BaseURI;

	public override bool IsEmptyElement
	{
		get
		{
			if (source is XmlElement xmlElement)
			{
				return xmlElement.IsEmpty;
			}
			return false;
		}
	}

	public override string XmlLang => source.XmlLang;

	public override object UnderlyingObject
	{
		get
		{
			CalibrateText();
			return source;
		}
	}

	public override bool HasAttributes
	{
		get
		{
			if (source is XmlElement { HasAttributes: not false, Attributes: var attributes })
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					if (!attributes[i].IsNamespace)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public override bool HasChildren
	{
		get
		{
			switch (source.NodeType)
			{
			case XmlNodeType.Element:
			{
				XmlNode xmlNode = FirstChild(source);
				if (xmlNode == null)
				{
					return false;
				}
				return true;
			}
			case XmlNodeType.Document:
			case XmlNodeType.DocumentFragment:
			{
				XmlNode xmlNode = FirstChild(source);
				if (xmlNode == null)
				{
					return false;
				}
				while (!IsValidChild(source, xmlNode))
				{
					xmlNode = NextSibling(xmlNode);
					if (xmlNode == null)
					{
						return false;
					}
				}
				return true;
			}
			default:
				return false;
			}
		}
	}

	public override IXmlSchemaInfo SchemaInfo => source.SchemaInfo;

	public override bool CanEdit => true;

	public DocumentXPathNavigator(XmlDocument document, XmlNode node)
	{
		this.document = document;
		ResetPosition(node);
	}

	public DocumentXPathNavigator(DocumentXPathNavigator other)
	{
		document = other.document;
		source = other.source;
		attributeIndex = other.attributeIndex;
		namespaceParent = other.namespaceParent;
	}

	public override XPathNavigator Clone()
	{
		return new DocumentXPathNavigator(this);
	}

	public override void SetValue(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		XmlNode xmlNode = source;
		switch (xmlNode.NodeType)
		{
		case XmlNodeType.Attribute:
			if (!((XmlAttribute)xmlNode).IsNamespace)
			{
				xmlNode.InnerText = value;
				return;
			}
			break;
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
		{
			CalibrateText();
			xmlNode = source;
			XmlNode xmlNode2 = TextEnd(xmlNode);
			if (xmlNode != xmlNode2)
			{
				if (xmlNode.IsReadOnly)
				{
					throw new InvalidOperationException(Res.GetString("This node is read-only. It cannot be modified."));
				}
				DeleteToFollowingSibling(xmlNode.NextSibling, xmlNode2);
			}
			goto case XmlNodeType.Element;
		}
		case XmlNodeType.Element:
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
			xmlNode.InnerText = value;
			return;
		}
		throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
	}

	public override string GetAttribute(string localName, string namespaceURI)
	{
		return source.GetXPAttribute(localName, namespaceURI);
	}

	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
		if (source is XmlElement { HasAttributes: not false, Attributes: var attributes })
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				XmlAttribute xmlAttribute = attributes[i];
				if (xmlAttribute.LocalName == localName && xmlAttribute.NamespaceURI == namespaceURI)
				{
					if (!xmlAttribute.IsNamespace)
					{
						source = xmlAttribute;
						attributeIndex = i;
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}

	public override bool MoveToFirstAttribute()
	{
		if (source is XmlElement { HasAttributes: not false, Attributes: var attributes })
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				XmlAttribute xmlAttribute = attributes[i];
				if (!xmlAttribute.IsNamespace)
				{
					source = xmlAttribute;
					attributeIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public override bool MoveToNextAttribute()
	{
		if (!(source is XmlAttribute { IsNamespace: false } xmlAttribute))
		{
			return false;
		}
		if (!CheckAttributePosition(xmlAttribute, out var attributes, attributeIndex) && !ResetAttributePosition(xmlAttribute, attributes, out attributeIndex))
		{
			return false;
		}
		for (int i = attributeIndex + 1; i < attributes.Count; i++)
		{
			XmlAttribute xmlAttribute2 = attributes[i];
			if (!xmlAttribute2.IsNamespace)
			{
				source = xmlAttribute2;
				attributeIndex = i;
				return true;
			}
		}
		return false;
	}

	public override string GetNamespace(string name)
	{
		XmlNode xmlNode = source;
		while (xmlNode != null && xmlNode.NodeType != XmlNodeType.Element)
		{
			xmlNode = ((!(xmlNode is XmlAttribute xmlAttribute)) ? xmlNode.ParentNode : xmlAttribute.OwnerElement);
		}
		XmlElement xmlElement = xmlNode as XmlElement;
		if (xmlElement != null)
		{
			string localName = ((name == null || name.Length == 0) ? document.strXmlns : name);
			string strReservedXmlns = document.strReservedXmlns;
			do
			{
				XmlAttribute attributeNode = xmlElement.GetAttributeNode(localName, strReservedXmlns);
				if (attributeNode != null)
				{
					return attributeNode.Value;
				}
				xmlElement = xmlElement.ParentNode as XmlElement;
			}
			while (xmlElement != null);
		}
		if (name == document.strXml)
		{
			return document.strReservedXml;
		}
		if (name == document.strXmlns)
		{
			return document.strReservedXmlns;
		}
		return string.Empty;
	}

	public override bool MoveToNamespace(string name)
	{
		if (name == document.strXmlns)
		{
			return false;
		}
		XmlElement xmlElement = source as XmlElement;
		if (xmlElement != null)
		{
			string localName = ((name == null || name.Length == 0) ? document.strXmlns : name);
			string strReservedXmlns = document.strReservedXmlns;
			do
			{
				XmlAttribute attributeNode = xmlElement.GetAttributeNode(localName, strReservedXmlns);
				if (attributeNode != null)
				{
					namespaceParent = (XmlElement)source;
					source = attributeNode;
					return true;
				}
				xmlElement = xmlElement.ParentNode as XmlElement;
			}
			while (xmlElement != null);
			if (name == document.strXml)
			{
				namespaceParent = (XmlElement)source;
				source = document.NamespaceXml;
				return true;
			}
		}
		return false;
	}

	public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
	{
		if (!(source is XmlElement xmlElement))
		{
			return false;
		}
		int index = int.MaxValue;
		switch (scope)
		{
		case XPathNamespaceScope.Local:
		{
			if (!xmlElement.HasAttributes)
			{
				return false;
			}
			XmlAttributeCollection attributes = xmlElement.Attributes;
			if (!MoveToFirstNamespaceLocal(attributes, ref index))
			{
				return false;
			}
			source = attributes[index];
			attributeIndex = index;
			namespaceParent = xmlElement;
			break;
		}
		case XPathNamespaceScope.ExcludeXml:
		{
			XmlAttributeCollection attributes = xmlElement.Attributes;
			if (!MoveToFirstNamespaceGlobal(ref attributes, ref index))
			{
				return false;
			}
			XmlAttribute xmlAttribute = attributes[index];
			while (Ref.Equal(xmlAttribute.LocalName, document.strXml))
			{
				if (!MoveToNextNamespaceGlobal(ref attributes, ref index))
				{
					return false;
				}
				xmlAttribute = attributes[index];
			}
			source = xmlAttribute;
			attributeIndex = index;
			namespaceParent = xmlElement;
			break;
		}
		case XPathNamespaceScope.All:
		{
			XmlAttributeCollection attributes = xmlElement.Attributes;
			if (!MoveToFirstNamespaceGlobal(ref attributes, ref index))
			{
				source = document.NamespaceXml;
			}
			else
			{
				source = attributes[index];
				attributeIndex = index;
			}
			namespaceParent = xmlElement;
			break;
		}
		default:
			return false;
		}
		return true;
	}

	private static bool MoveToFirstNamespaceLocal(XmlAttributeCollection attributes, ref int index)
	{
		for (int num = attributes.Count - 1; num >= 0; num--)
		{
			if (attributes[num].IsNamespace)
			{
				index = num;
				return true;
			}
		}
		return false;
	}

	private static bool MoveToFirstNamespaceGlobal(ref XmlAttributeCollection attributes, ref int index)
	{
		if (MoveToFirstNamespaceLocal(attributes, ref index))
		{
			return true;
		}
		for (XmlElement xmlElement = attributes.parent.ParentNode as XmlElement; xmlElement != null; xmlElement = xmlElement.ParentNode as XmlElement)
		{
			if (xmlElement.HasAttributes)
			{
				attributes = xmlElement.Attributes;
				if (MoveToFirstNamespaceLocal(attributes, ref index))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool MoveToNextNamespace(XPathNamespaceScope scope)
	{
		if (!(source is XmlAttribute { IsNamespace: not false } xmlAttribute))
		{
			return false;
		}
		int index = attributeIndex;
		if (!CheckAttributePosition(xmlAttribute, out var attributes, index) && !ResetAttributePosition(xmlAttribute, attributes, out index))
		{
			return false;
		}
		switch (scope)
		{
		case XPathNamespaceScope.Local:
			if (xmlAttribute.OwnerElement != namespaceParent)
			{
				return false;
			}
			if (!MoveToNextNamespaceLocal(attributes, ref index))
			{
				return false;
			}
			source = attributes[index];
			attributeIndex = index;
			break;
		case XPathNamespaceScope.ExcludeXml:
		{
			XmlAttribute xmlAttribute2;
			string localName;
			do
			{
				if (!MoveToNextNamespaceGlobal(ref attributes, ref index))
				{
					return false;
				}
				xmlAttribute2 = attributes[index];
				localName = xmlAttribute2.LocalName;
			}
			while (PathHasDuplicateNamespace(xmlAttribute2.OwnerElement, namespaceParent, localName) || Ref.Equal(localName, document.strXml));
			source = xmlAttribute2;
			attributeIndex = index;
			break;
		}
		case XPathNamespaceScope.All:
		{
			XmlAttribute xmlAttribute2;
			do
			{
				if (!MoveToNextNamespaceGlobal(ref attributes, ref index))
				{
					if (PathHasDuplicateNamespace(null, namespaceParent, document.strXml))
					{
						return false;
					}
					source = document.NamespaceXml;
					return true;
				}
				xmlAttribute2 = attributes[index];
			}
			while (PathHasDuplicateNamespace(xmlAttribute2.OwnerElement, namespaceParent, xmlAttribute2.LocalName));
			source = xmlAttribute2;
			attributeIndex = index;
			break;
		}
		default:
			return false;
		}
		return true;
	}

	private static bool MoveToNextNamespaceLocal(XmlAttributeCollection attributes, ref int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			if (attributes[num].IsNamespace)
			{
				index = num;
				return true;
			}
		}
		return false;
	}

	private static bool MoveToNextNamespaceGlobal(ref XmlAttributeCollection attributes, ref int index)
	{
		if (MoveToNextNamespaceLocal(attributes, ref index))
		{
			return true;
		}
		for (XmlElement xmlElement = attributes.parent.ParentNode as XmlElement; xmlElement != null; xmlElement = xmlElement.ParentNode as XmlElement)
		{
			if (xmlElement.HasAttributes)
			{
				attributes = xmlElement.Attributes;
				if (MoveToFirstNamespaceLocal(attributes, ref index))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool PathHasDuplicateNamespace(XmlElement top, XmlElement bottom, string localName)
	{
		string strReservedXmlns = document.strReservedXmlns;
		while (bottom != null && bottom != top)
		{
			if (bottom.GetAttributeNode(localName, strReservedXmlns) != null)
			{
				return true;
			}
			bottom = bottom.ParentNode as XmlElement;
		}
		return false;
	}

	public override string LookupNamespace(string prefix)
	{
		string text = base.LookupNamespace(prefix);
		if (text != null)
		{
			text = NameTable.Add(text);
		}
		return text;
	}

	public override bool MoveToNext()
	{
		XmlNode xmlNode = NextSibling(source);
		if (xmlNode == null)
		{
			return false;
		}
		if (xmlNode.IsText && source.IsText)
		{
			xmlNode = NextSibling(TextEnd(xmlNode));
			if (xmlNode == null)
			{
				return false;
			}
		}
		XmlNode parent = ParentNode(xmlNode);
		while (!IsValidChild(parent, xmlNode))
		{
			xmlNode = NextSibling(xmlNode);
			if (xmlNode == null)
			{
				return false;
			}
		}
		source = xmlNode;
		return true;
	}

	public override bool MoveToPrevious()
	{
		XmlNode xmlNode = PreviousSibling(source);
		if (xmlNode == null)
		{
			return false;
		}
		if (xmlNode.IsText)
		{
			if (source.IsText)
			{
				xmlNode = PreviousSibling(TextStart(xmlNode));
				if (xmlNode == null)
				{
					return false;
				}
			}
			else
			{
				xmlNode = TextStart(xmlNode);
			}
		}
		XmlNode parent = ParentNode(xmlNode);
		while (!IsValidChild(parent, xmlNode))
		{
			xmlNode = PreviousSibling(xmlNode);
			if (xmlNode == null)
			{
				return false;
			}
		}
		source = xmlNode;
		return true;
	}

	public override bool MoveToFirst()
	{
		if (source.NodeType == XmlNodeType.Attribute)
		{
			return false;
		}
		XmlNode xmlNode = ParentNode(source);
		if (xmlNode == null)
		{
			return false;
		}
		XmlNode xmlNode2 = FirstChild(xmlNode);
		while (!IsValidChild(xmlNode, xmlNode2))
		{
			xmlNode2 = NextSibling(xmlNode2);
			if (xmlNode2 == null)
			{
				return false;
			}
		}
		source = xmlNode2;
		return true;
	}

	public override bool MoveToFirstChild()
	{
		XmlNode xmlNode;
		switch (source.NodeType)
		{
		case XmlNodeType.Element:
			xmlNode = FirstChild(source);
			if (xmlNode == null)
			{
				return false;
			}
			break;
		case XmlNodeType.Document:
		case XmlNodeType.DocumentFragment:
			xmlNode = FirstChild(source);
			if (xmlNode == null)
			{
				return false;
			}
			while (!IsValidChild(source, xmlNode))
			{
				xmlNode = NextSibling(xmlNode);
				if (xmlNode == null)
				{
					return false;
				}
			}
			break;
		default:
			return false;
		}
		source = xmlNode;
		return true;
	}

	public override bool MoveToParent()
	{
		XmlNode xmlNode = ParentNode(source);
		if (xmlNode != null)
		{
			source = xmlNode;
			return true;
		}
		if (source is XmlAttribute xmlAttribute)
		{
			xmlNode = (xmlAttribute.IsNamespace ? namespaceParent : xmlAttribute.OwnerElement);
			if (xmlNode != null)
			{
				source = xmlNode;
				namespaceParent = null;
				return true;
			}
		}
		return false;
	}

	public override void MoveToRoot()
	{
		while (true)
		{
			XmlNode xmlNode = source.ParentNode;
			if (xmlNode == null)
			{
				if (!(source is XmlAttribute xmlAttribute))
				{
					break;
				}
				xmlNode = (xmlAttribute.IsNamespace ? namespaceParent : xmlAttribute.OwnerElement);
				if (xmlNode == null)
				{
					break;
				}
			}
			source = xmlNode;
		}
		namespaceParent = null;
	}

	public override bool MoveTo(XPathNavigator other)
	{
		if (other is DocumentXPathNavigator documentXPathNavigator && document == documentXPathNavigator.document)
		{
			source = documentXPathNavigator.source;
			attributeIndex = documentXPathNavigator.attributeIndex;
			namespaceParent = documentXPathNavigator.namespaceParent;
			return true;
		}
		return false;
	}

	public override bool MoveToId(string id)
	{
		XmlElement elementById = document.GetElementById(id);
		if (elementById != null)
		{
			source = elementById;
			namespaceParent = null;
			return true;
		}
		return false;
	}

	public override bool MoveToChild(string localName, string namespaceUri)
	{
		if (source.NodeType == XmlNodeType.Attribute)
		{
			return false;
		}
		XmlNode xmlNode = FirstChild(source);
		if (xmlNode != null)
		{
			do
			{
				if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.LocalName == localName && xmlNode.NamespaceURI == namespaceUri)
				{
					source = xmlNode;
					return true;
				}
				xmlNode = NextSibling(xmlNode);
			}
			while (xmlNode != null);
		}
		return false;
	}

	public override bool MoveToChild(XPathNodeType type)
	{
		if (source.NodeType == XmlNodeType.Attribute)
		{
			return false;
		}
		XmlNode xmlNode = FirstChild(source);
		if (xmlNode != null)
		{
			int contentKindMask = XPathNavigator.GetContentKindMask(type);
			if (contentKindMask == 0)
			{
				return false;
			}
			do
			{
				if (((1 << (int)xmlNode.XPNodeType) & contentKindMask) != 0)
				{
					source = xmlNode;
					return true;
				}
				xmlNode = NextSibling(xmlNode);
			}
			while (xmlNode != null);
		}
		return false;
	}

	public override bool MoveToFollowing(string localName, string namespaceUri, XPathNavigator end)
	{
		XmlNode xmlNode = null;
		DocumentXPathNavigator documentXPathNavigator = end as DocumentXPathNavigator;
		if (documentXPathNavigator != null)
		{
			if (document != documentXPathNavigator.document)
			{
				return false;
			}
			XmlNodeType nodeType = documentXPathNavigator.source.NodeType;
			if (nodeType == XmlNodeType.Attribute)
			{
				documentXPathNavigator = (DocumentXPathNavigator)documentXPathNavigator.Clone();
				if (!documentXPathNavigator.MoveToNonDescendant())
				{
					return false;
				}
			}
			xmlNode = documentXPathNavigator.source;
		}
		XmlNode xmlNode2 = source;
		if (xmlNode2.NodeType == XmlNodeType.Attribute)
		{
			xmlNode2 = ((XmlAttribute)xmlNode2).OwnerElement;
			if (xmlNode2 == null)
			{
				return false;
			}
		}
		do
		{
			XmlNode firstChild = xmlNode2.FirstChild;
			if (firstChild != null)
			{
				xmlNode2 = firstChild;
			}
			else
			{
				XmlNode nextSibling;
				while (true)
				{
					nextSibling = xmlNode2.NextSibling;
					if (nextSibling != null)
					{
						break;
					}
					XmlNode parentNode = xmlNode2.ParentNode;
					if (parentNode != null)
					{
						xmlNode2 = parentNode;
						continue;
					}
					return false;
				}
				xmlNode2 = nextSibling;
			}
			if (xmlNode2 == xmlNode)
			{
				return false;
			}
		}
		while (xmlNode2.NodeType != XmlNodeType.Element || xmlNode2.LocalName != localName || xmlNode2.NamespaceURI != namespaceUri);
		source = xmlNode2;
		return true;
	}

	public override bool MoveToFollowing(XPathNodeType type, XPathNavigator end)
	{
		XmlNode xmlNode = null;
		DocumentXPathNavigator documentXPathNavigator = end as DocumentXPathNavigator;
		if (documentXPathNavigator != null)
		{
			if (document != documentXPathNavigator.document)
			{
				return false;
			}
			XmlNodeType nodeType = documentXPathNavigator.source.NodeType;
			if (nodeType == XmlNodeType.Attribute)
			{
				documentXPathNavigator = (DocumentXPathNavigator)documentXPathNavigator.Clone();
				if (!documentXPathNavigator.MoveToNonDescendant())
				{
					return false;
				}
			}
			xmlNode = documentXPathNavigator.source;
		}
		int contentKindMask = XPathNavigator.GetContentKindMask(type);
		if (contentKindMask == 0)
		{
			return false;
		}
		XmlNode xmlNode2 = source;
		switch (xmlNode2.NodeType)
		{
		case XmlNodeType.Attribute:
			xmlNode2 = ((XmlAttribute)xmlNode2).OwnerElement;
			if (xmlNode2 == null)
			{
				return false;
			}
			break;
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			xmlNode2 = TextEnd(xmlNode2);
			break;
		}
		do
		{
			XmlNode firstChild = xmlNode2.FirstChild;
			if (firstChild != null)
			{
				xmlNode2 = firstChild;
			}
			else
			{
				XmlNode nextSibling;
				while (true)
				{
					nextSibling = xmlNode2.NextSibling;
					if (nextSibling != null)
					{
						break;
					}
					XmlNode parentNode = xmlNode2.ParentNode;
					if (parentNode != null)
					{
						xmlNode2 = parentNode;
						continue;
					}
					return false;
				}
				xmlNode2 = nextSibling;
			}
			if (xmlNode2 == xmlNode)
			{
				return false;
			}
		}
		while (((1 << (int)xmlNode2.XPNodeType) & contentKindMask) == 0);
		source = xmlNode2;
		return true;
	}

	public override bool MoveToNext(string localName, string namespaceUri)
	{
		XmlNode xmlNode = NextSibling(source);
		if (xmlNode == null)
		{
			return false;
		}
		do
		{
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.LocalName == localName && xmlNode.NamespaceURI == namespaceUri)
			{
				source = xmlNode;
				return true;
			}
			xmlNode = NextSibling(xmlNode);
		}
		while (xmlNode != null);
		return false;
	}

	public override bool MoveToNext(XPathNodeType type)
	{
		XmlNode xmlNode = NextSibling(source);
		if (xmlNode == null)
		{
			return false;
		}
		if (xmlNode.IsText && source.IsText)
		{
			xmlNode = NextSibling(TextEnd(xmlNode));
			if (xmlNode == null)
			{
				return false;
			}
		}
		int contentKindMask = XPathNavigator.GetContentKindMask(type);
		if (contentKindMask == 0)
		{
			return false;
		}
		do
		{
			if (((1 << (int)xmlNode.XPNodeType) & contentKindMask) != 0)
			{
				source = xmlNode;
				return true;
			}
			xmlNode = NextSibling(xmlNode);
		}
		while (xmlNode != null);
		return false;
	}

	public override bool IsSamePosition(XPathNavigator other)
	{
		if (other is DocumentXPathNavigator documentXPathNavigator)
		{
			CalibrateText();
			documentXPathNavigator.CalibrateText();
			if (source == documentXPathNavigator.source)
			{
				return namespaceParent == documentXPathNavigator.namespaceParent;
			}
			return false;
		}
		return false;
	}

	public override bool IsDescendant(XPathNavigator other)
	{
		if (other is DocumentXPathNavigator documentXPathNavigator)
		{
			return IsDescendant(source, documentXPathNavigator.source);
		}
		return false;
	}

	public override bool CheckValidity(XmlSchemaSet schemas, ValidationEventHandler validationEventHandler)
	{
		XmlDocument xmlDocument;
		if (source.NodeType == XmlNodeType.Document)
		{
			xmlDocument = (XmlDocument)source;
		}
		else
		{
			xmlDocument = source.OwnerDocument;
			if (schemas != null)
			{
				throw new ArgumentException(Res.GetString("An XmlSchemaSet is only allowed as a parameter on the Root node.", null));
			}
		}
		if (schemas == null && xmlDocument != null)
		{
			schemas = xmlDocument.Schemas;
		}
		if (schemas == null || schemas.Count == 0)
		{
			throw new InvalidOperationException(Res.GetString("The XmlSchemaSet on the document is either null or has no schemas in it. Provide schema information before calling Validate."));
		}
		return new DocumentSchemaValidator(xmlDocument, schemas, validationEventHandler)
		{
			PsviAugmentation = false
		}.Validate(source);
	}

	private static XmlNode OwnerNode(XmlNode node)
	{
		XmlNode parentNode = node.ParentNode;
		if (parentNode != null)
		{
			return parentNode;
		}
		if (node is XmlAttribute xmlAttribute)
		{
			return xmlAttribute.OwnerElement;
		}
		return null;
	}

	private static int GetDepth(XmlNode node)
	{
		int num = 0;
		for (XmlNode xmlNode = OwnerNode(node); xmlNode != null; xmlNode = OwnerNode(xmlNode))
		{
			num++;
		}
		return num;
	}

	private XmlNodeOrder Compare(XmlNode node1, XmlNode node2)
	{
		if (node1.XPNodeType == XPathNodeType.Attribute)
		{
			if (node2.XPNodeType == XPathNodeType.Attribute)
			{
				XmlElement ownerElement = ((XmlAttribute)node1).OwnerElement;
				if (ownerElement.HasAttributes)
				{
					XmlAttributeCollection attributes = ownerElement.Attributes;
					for (int i = 0; i < attributes.Count; i++)
					{
						XmlAttribute xmlAttribute = attributes[i];
						if (xmlAttribute == node1)
						{
							return XmlNodeOrder.Before;
						}
						if (xmlAttribute == node2)
						{
							return XmlNodeOrder.After;
						}
					}
				}
				return XmlNodeOrder.Unknown;
			}
			return XmlNodeOrder.Before;
		}
		if (node2.XPNodeType == XPathNodeType.Attribute)
		{
			return XmlNodeOrder.After;
		}
		XmlNode nextSibling = node1.NextSibling;
		while (nextSibling != null && nextSibling != node2)
		{
			nextSibling = nextSibling.NextSibling;
		}
		if (nextSibling == null)
		{
			return XmlNodeOrder.After;
		}
		return XmlNodeOrder.Before;
	}

	public override XmlNodeOrder ComparePosition(XPathNavigator other)
	{
		if (!(other is DocumentXPathNavigator documentXPathNavigator))
		{
			return XmlNodeOrder.Unknown;
		}
		CalibrateText();
		documentXPathNavigator.CalibrateText();
		if (source == documentXPathNavigator.source && namespaceParent == documentXPathNavigator.namespaceParent)
		{
			return XmlNodeOrder.Same;
		}
		if (namespaceParent != null || documentXPathNavigator.namespaceParent != null)
		{
			return base.ComparePosition(other);
		}
		XmlNode xmlNode = source;
		XmlNode xmlNode2 = documentXPathNavigator.source;
		XmlNode xmlNode3 = OwnerNode(xmlNode);
		XmlNode xmlNode4 = OwnerNode(xmlNode2);
		if (xmlNode3 == xmlNode4)
		{
			if (xmlNode3 == null)
			{
				return XmlNodeOrder.Unknown;
			}
			return Compare(xmlNode, xmlNode2);
		}
		int num = GetDepth(xmlNode);
		int num2 = GetDepth(xmlNode2);
		if (num2 > num)
		{
			while (xmlNode2 != null && num2 > num)
			{
				xmlNode2 = OwnerNode(xmlNode2);
				num2--;
			}
			if (xmlNode == xmlNode2)
			{
				return XmlNodeOrder.Before;
			}
			xmlNode4 = OwnerNode(xmlNode2);
		}
		else if (num > num2)
		{
			while (xmlNode != null && num > num2)
			{
				xmlNode = OwnerNode(xmlNode);
				num--;
			}
			if (xmlNode == xmlNode2)
			{
				return XmlNodeOrder.After;
			}
			xmlNode3 = OwnerNode(xmlNode);
		}
		while (xmlNode3 != null && xmlNode4 != null)
		{
			if (xmlNode3 == xmlNode4)
			{
				return Compare(xmlNode, xmlNode2);
			}
			xmlNode = xmlNode3;
			xmlNode2 = xmlNode4;
			xmlNode3 = OwnerNode(xmlNode);
			xmlNode4 = OwnerNode(xmlNode2);
		}
		return XmlNodeOrder.Unknown;
	}

	XmlNode IHasXmlNode.GetNode()
	{
		return source;
	}

	public override XPathNodeIterator SelectDescendants(string localName, string namespaceURI, bool matchSelf)
	{
		string text = document.NameTable.Get(namespaceURI);
		if (text == null || source.NodeType == XmlNodeType.Attribute)
		{
			return new DocumentXPathNodeIterator_Empty(this);
		}
		string text2 = document.NameTable.Get(localName);
		if (text2 == null)
		{
			return new DocumentXPathNodeIterator_Empty(this);
		}
		if (text2.Length == 0)
		{
			if (matchSelf)
			{
				return new DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName(this, text);
			}
			return new DocumentXPathNodeIterator_ElemChildren_NoLocalName(this, text);
		}
		if (matchSelf)
		{
			return new DocumentXPathNodeIterator_ElemChildren_AndSelf(this, text2, text);
		}
		return new DocumentXPathNodeIterator_ElemChildren(this, text2, text);
	}

	public override XPathNodeIterator SelectDescendants(XPathNodeType nt, bool includeSelf)
	{
		if (nt == XPathNodeType.Element)
		{
			XmlNodeType nodeType = source.NodeType;
			if (nodeType != XmlNodeType.Document && nodeType != XmlNodeType.Element)
			{
				return new DocumentXPathNodeIterator_Empty(this);
			}
			if (includeSelf)
			{
				return new DocumentXPathNodeIterator_AllElemChildren_AndSelf(this);
			}
			return new DocumentXPathNodeIterator_AllElemChildren(this);
		}
		return base.SelectDescendants(nt, includeSelf);
	}

	public override XmlWriter PrependChild()
	{
		XmlNodeType nodeType = source.NodeType;
		if (nodeType != XmlNodeType.Element && nodeType != XmlNodeType.Document && nodeType != XmlNodeType.DocumentFragment)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.PrependChild, source, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(source, document);
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override XmlWriter AppendChild()
	{
		XmlNodeType nodeType = source.NodeType;
		if (nodeType != XmlNodeType.Element && nodeType != XmlNodeType.Document && nodeType != XmlNodeType.DocumentFragment)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.AppendChild, source, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(source, document);
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override XmlWriter InsertAfter()
	{
		XmlNode xmlNode = source;
		switch (xmlNode.NodeType)
		{
		case XmlNodeType.Attribute:
		case XmlNodeType.Document:
		case XmlNodeType.DocumentFragment:
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			xmlNode = TextEnd(xmlNode);
			break;
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.InsertSiblingAfter, xmlNode, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(xmlNode.ParentNode, document);
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override XmlWriter InsertBefore()
	{
		switch (source.NodeType)
		{
		case XmlNodeType.Attribute:
		case XmlNodeType.Document:
		case XmlNodeType.DocumentFragment:
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			CalibrateText();
			break;
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.InsertSiblingBefore, source, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(source.ParentNode, document);
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override XmlWriter CreateAttributes()
	{
		if (source.NodeType != XmlNodeType.Element)
		{
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.AppendAttribute, source, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(source, document);
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override XmlWriter ReplaceRange(XPathNavigator lastSiblingToReplace)
	{
		if (!(lastSiblingToReplace is DocumentXPathNavigator documentXPathNavigator))
		{
			if (lastSiblingToReplace == null)
			{
				throw new ArgumentNullException("lastSiblingToReplace");
			}
			throw new NotSupportedException();
		}
		CalibrateText();
		documentXPathNavigator.CalibrateText();
		XmlNode xmlNode = source;
		XmlNode xmlNode2 = documentXPathNavigator.source;
		if (xmlNode == xmlNode2)
		{
			switch (xmlNode.NodeType)
			{
			case XmlNodeType.Attribute:
			case XmlNodeType.Document:
			case XmlNodeType.DocumentFragment:
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				xmlNode2 = documentXPathNavigator.TextEnd(xmlNode2);
				break;
			}
		}
		else
		{
			if (xmlNode2.IsText)
			{
				xmlNode2 = documentXPathNavigator.TextEnd(xmlNode2);
			}
			if (!IsFollowingSibling(xmlNode, xmlNode2))
			{
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
			}
		}
		DocumentXmlWriter documentXmlWriter = new DocumentXmlWriter(DocumentXmlWriterType.ReplaceToFollowingSibling, xmlNode, document);
		documentXmlWriter.NamespaceManager = GetNamespaceManager(xmlNode.ParentNode, document);
		documentXmlWriter.Navigator = this;
		documentXmlWriter.EndNode = xmlNode2;
		return new XmlWellFormedWriter(documentXmlWriter, documentXmlWriter.Settings);
	}

	public override void DeleteRange(XPathNavigator lastSiblingToDelete)
	{
		if (!(lastSiblingToDelete is DocumentXPathNavigator documentXPathNavigator))
		{
			if (lastSiblingToDelete == null)
			{
				throw new ArgumentNullException("lastSiblingToDelete");
			}
			throw new NotSupportedException();
		}
		CalibrateText();
		documentXPathNavigator.CalibrateText();
		XmlNode xmlNode = source;
		XmlNode xmlNode2 = documentXPathNavigator.source;
		if (xmlNode == xmlNode2)
		{
			switch (xmlNode.NodeType)
			{
			case XmlNodeType.Attribute:
			{
				XmlAttribute xmlAttribute = (XmlAttribute)xmlNode;
				if (!xmlAttribute.IsNamespace)
				{
					XmlNode xmlNode3 = OwnerNode(xmlAttribute);
					DeleteAttribute(xmlAttribute, attributeIndex);
					if (xmlNode3 != null)
					{
						ResetPosition(xmlNode3);
					}
					break;
				}
				goto default;
			}
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				xmlNode2 = documentXPathNavigator.TextEnd(xmlNode2);
				goto case XmlNodeType.Element;
			case XmlNodeType.Element:
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			{
				XmlNode xmlNode3 = OwnerNode(xmlNode);
				DeleteToFollowingSibling(xmlNode, xmlNode2);
				if (xmlNode3 != null)
				{
					ResetPosition(xmlNode3);
				}
				break;
			}
			default:
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
			}
		}
		else
		{
			if (xmlNode2.IsText)
			{
				xmlNode2 = documentXPathNavigator.TextEnd(xmlNode2);
			}
			if (!IsFollowingSibling(xmlNode, xmlNode2))
			{
				throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
			}
			XmlNode xmlNode4 = OwnerNode(xmlNode);
			DeleteToFollowingSibling(xmlNode, xmlNode2);
			if (xmlNode4 != null)
			{
				ResetPosition(xmlNode4);
			}
		}
	}

	public override void DeleteSelf()
	{
		XmlNode xmlNode = source;
		XmlNode end = xmlNode;
		switch (xmlNode.NodeType)
		{
		case XmlNodeType.Attribute:
		{
			XmlAttribute xmlAttribute = (XmlAttribute)xmlNode;
			if (!xmlAttribute.IsNamespace)
			{
				XmlNode xmlNode2 = OwnerNode(xmlAttribute);
				DeleteAttribute(xmlAttribute, attributeIndex);
				if (xmlNode2 != null)
				{
					ResetPosition(xmlNode2);
				}
				break;
			}
			goto default;
		}
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			CalibrateText();
			xmlNode = source;
			end = TextEnd(xmlNode);
			goto case XmlNodeType.Element;
		case XmlNodeType.Element:
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
		{
			XmlNode xmlNode2 = OwnerNode(xmlNode);
			DeleteToFollowingSibling(xmlNode, end);
			if (xmlNode2 != null)
			{
				ResetPosition(xmlNode2);
			}
			break;
		}
		default:
			throw new InvalidOperationException(Res.GetString("Operation is not valid due to the current position of the navigator."));
		}
	}

	private static void DeleteAttribute(XmlAttribute attribute, int index)
	{
		if (!CheckAttributePosition(attribute, out var attributes, index) && !ResetAttributePosition(attribute, attributes, out index))
		{
			throw new InvalidOperationException(Res.GetString("The current position of the navigator is missing a valid parent."));
		}
		if (attribute.IsReadOnly)
		{
			throw new InvalidOperationException(Res.GetString("This node is read-only. It cannot be modified."));
		}
		attributes.RemoveAt(index);
	}

	internal static void DeleteToFollowingSibling(XmlNode node, XmlNode end)
	{
		XmlNode parentNode = node.ParentNode;
		if (parentNode == null)
		{
			throw new InvalidOperationException(Res.GetString("The current position of the navigator is missing a valid parent."));
		}
		if (node.IsReadOnly || end.IsReadOnly)
		{
			throw new InvalidOperationException(Res.GetString("This node is read-only. It cannot be modified."));
		}
		while (node != end)
		{
			XmlNode oldChild = node;
			node = node.NextSibling;
			parentNode.RemoveChild(oldChild);
		}
		parentNode.RemoveChild(node);
	}

	private static XmlNamespaceManager GetNamespaceManager(XmlNode node, XmlDocument document)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
		List<XmlElement> list = new List<XmlElement>();
		while (node != null)
		{
			if (node is XmlElement { HasAttributes: not false } xmlElement)
			{
				list.Add(xmlElement);
			}
			node = node.ParentNode;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			xmlNamespaceManager.PushScope();
			XmlAttributeCollection attributes = list[num].Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				XmlAttribute xmlAttribute = attributes[i];
				if (xmlAttribute.IsNamespace)
				{
					string prefix = ((xmlAttribute.Prefix.Length == 0) ? string.Empty : xmlAttribute.LocalName);
					xmlNamespaceManager.AddNamespace(prefix, xmlAttribute.Value);
				}
			}
		}
		return xmlNamespaceManager;
	}

	internal void ResetPosition(XmlNode node)
	{
		source = node;
		if (node is XmlAttribute { OwnerElement: { } ownerElement } xmlAttribute)
		{
			ResetAttributePosition(xmlAttribute, ownerElement.Attributes, out attributeIndex);
			if (xmlAttribute.IsNamespace)
			{
				namespaceParent = ownerElement;
			}
		}
	}

	private static bool ResetAttributePosition(XmlAttribute attribute, XmlAttributeCollection attributes, out int index)
	{
		if (attributes != null)
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attribute == attributes[i])
				{
					index = i;
					return true;
				}
			}
		}
		index = 0;
		return false;
	}

	private static bool CheckAttributePosition(XmlAttribute attribute, out XmlAttributeCollection attributes, int index)
	{
		XmlElement ownerElement = attribute.OwnerElement;
		if (ownerElement != null)
		{
			attributes = ownerElement.Attributes;
			if (index >= 0 && index < attributes.Count && attribute == attributes[index])
			{
				return true;
			}
		}
		else
		{
			attributes = null;
		}
		return false;
	}

	private void CalibrateText()
	{
		for (XmlNode xmlNode = PreviousText(source); xmlNode != null; xmlNode = PreviousText(xmlNode))
		{
			ResetPosition(xmlNode);
		}
	}

	private XmlNode ParentNode(XmlNode node)
	{
		XmlNode parentNode = node.ParentNode;
		if (!document.HasEntityReferences)
		{
			return parentNode;
		}
		return ParentNodeTail(parentNode);
	}

	private XmlNode ParentNodeTail(XmlNode parent)
	{
		while (parent != null && parent.NodeType == XmlNodeType.EntityReference)
		{
			parent = parent.ParentNode;
		}
		return parent;
	}

	private XmlNode FirstChild(XmlNode node)
	{
		XmlNode firstChild = node.FirstChild;
		if (!document.HasEntityReferences)
		{
			return firstChild;
		}
		return FirstChildTail(firstChild);
	}

	private XmlNode FirstChildTail(XmlNode child)
	{
		while (child != null && child.NodeType == XmlNodeType.EntityReference)
		{
			child = child.FirstChild;
		}
		return child;
	}

	private XmlNode NextSibling(XmlNode node)
	{
		XmlNode nextSibling = node.NextSibling;
		if (!document.HasEntityReferences)
		{
			return nextSibling;
		}
		return NextSiblingTail(node, nextSibling);
	}

	private XmlNode NextSiblingTail(XmlNode node, XmlNode sibling)
	{
		while (sibling == null)
		{
			node = node.ParentNode;
			if (node == null || node.NodeType != XmlNodeType.EntityReference)
			{
				return null;
			}
			sibling = node.NextSibling;
		}
		while (sibling != null && sibling.NodeType == XmlNodeType.EntityReference)
		{
			sibling = sibling.FirstChild;
		}
		return sibling;
	}

	private XmlNode PreviousSibling(XmlNode node)
	{
		XmlNode previousSibling = node.PreviousSibling;
		if (!document.HasEntityReferences)
		{
			return previousSibling;
		}
		return PreviousSiblingTail(node, previousSibling);
	}

	private XmlNode PreviousSiblingTail(XmlNode node, XmlNode sibling)
	{
		while (sibling == null)
		{
			node = node.ParentNode;
			if (node == null || node.NodeType != XmlNodeType.EntityReference)
			{
				return null;
			}
			sibling = node.PreviousSibling;
		}
		while (sibling != null && sibling.NodeType == XmlNodeType.EntityReference)
		{
			sibling = sibling.LastChild;
		}
		return sibling;
	}

	private XmlNode PreviousText(XmlNode node)
	{
		XmlNode previousText = node.PreviousText;
		if (!document.HasEntityReferences)
		{
			return previousText;
		}
		return PreviousTextTail(node, previousText);
	}

	private XmlNode PreviousTextTail(XmlNode node, XmlNode text)
	{
		if (text != null)
		{
			return text;
		}
		if (!node.IsText)
		{
			return null;
		}
		XmlNode xmlNode;
		for (xmlNode = node.PreviousSibling; xmlNode == null; xmlNode = node.PreviousSibling)
		{
			node = node.ParentNode;
			if (node == null || node.NodeType != XmlNodeType.EntityReference)
			{
				return null;
			}
		}
		while (xmlNode != null)
		{
			switch (xmlNode.NodeType)
			{
			case XmlNodeType.EntityReference:
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				return xmlNode;
			default:
				return null;
			}
			xmlNode = xmlNode.LastChild;
		}
		return null;
	}

	internal static bool IsFollowingSibling(XmlNode left, XmlNode right)
	{
		while (true)
		{
			left = left.NextSibling;
			if (left == null)
			{
				break;
			}
			if (left == right)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsDescendant(XmlNode top, XmlNode bottom)
	{
		while (true)
		{
			XmlNode xmlNode = bottom.ParentNode;
			if (xmlNode == null)
			{
				if (!(bottom is XmlAttribute xmlAttribute))
				{
					break;
				}
				xmlNode = xmlAttribute.OwnerElement;
				if (xmlNode == null)
				{
					break;
				}
			}
			bottom = xmlNode;
			if (top == bottom)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidChild(XmlNode parent, XmlNode child)
	{
		switch (parent.NodeType)
		{
		case XmlNodeType.Element:
			return true;
		case XmlNodeType.DocumentFragment:
			switch (child.NodeType)
			{
			case XmlNodeType.Element:
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.Comment:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				return true;
			}
			break;
		case XmlNodeType.Document:
		{
			XmlNodeType nodeType = child.NodeType;
			if (nodeType == XmlNodeType.Element || (uint)(nodeType - 7) <= 1u)
			{
				return true;
			}
			break;
		}
		}
		return false;
	}

	private XmlNode TextStart(XmlNode node)
	{
		XmlNode result;
		do
		{
			result = node;
			node = PreviousSibling(node);
		}
		while (node != null && node.IsText);
		return result;
	}

	private XmlNode TextEnd(XmlNode node)
	{
		XmlNode result;
		do
		{
			result = node;
			node = NextSibling(node);
		}
		while (node != null && node.IsText);
		return result;
	}
}
