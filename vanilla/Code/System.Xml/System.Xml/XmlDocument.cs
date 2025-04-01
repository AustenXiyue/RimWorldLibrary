using System.Collections;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents an XML document.</summary>
public class XmlDocument : XmlNode
{
	private XmlImplementation implementation;

	private DomNameTable domNameTable;

	private XmlLinkedNode lastChild;

	private XmlNamedNodeMap entities;

	private Hashtable htElementIdMap;

	private Hashtable htElementIDAttrDecl;

	private SchemaInfo schemaInfo;

	private XmlSchemaSet schemas;

	private bool reportValidity;

	private bool actualLoadingStatus;

	private XmlNodeChangedEventHandler onNodeInsertingDelegate;

	private XmlNodeChangedEventHandler onNodeInsertedDelegate;

	private XmlNodeChangedEventHandler onNodeRemovingDelegate;

	private XmlNodeChangedEventHandler onNodeRemovedDelegate;

	private XmlNodeChangedEventHandler onNodeChangingDelegate;

	private XmlNodeChangedEventHandler onNodeChangedDelegate;

	internal bool fEntRefNodesPresent;

	internal bool fCDataNodesPresent;

	private bool preserveWhitespace;

	private bool isLoading;

	internal string strDocumentName;

	internal string strDocumentFragmentName;

	internal string strCommentName;

	internal string strTextName;

	internal string strCDataSectionName;

	internal string strEntityName;

	internal string strID;

	internal string strXmlns;

	internal string strXml;

	internal string strSpace;

	internal string strLang;

	internal string strEmpty;

	internal string strNonSignificantWhitespaceName;

	internal string strSignificantWhitespaceName;

	internal string strReservedXmlns;

	internal string strReservedXml;

	internal string baseURI;

	private XmlResolver resolver;

	internal bool bSetResolver;

	internal object objLock;

	private XmlAttribute namespaceXml;

	internal static EmptyEnumerator EmptyEnumerator = new EmptyEnumerator();

	internal static IXmlSchemaInfo NotKnownSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.NotKnown);

	internal static IXmlSchemaInfo ValidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Valid);

	internal static IXmlSchemaInfo InvalidSchemaInfo = new XmlSchemaInfo(XmlSchemaValidity.Invalid);

	internal SchemaInfo DtdSchemaInfo
	{
		get
		{
			return schemaInfo;
		}
		set
		{
			schemaInfo = value;
		}
	}

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>The node type. For XmlDocument nodes, this value is XmlNodeType.Document.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Document;

	/// <summary>Gets the parent node of this node (for nodes that can have parents).</summary>
	/// <returns>Always returns null.</returns>
	public override XmlNode ParentNode => null;

	/// <summary>Gets the node containing the DOCTYPE declaration.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> containing the DocumentType (DOCTYPE declaration).</returns>
	public virtual XmlDocumentType DocumentType => (XmlDocumentType)FindChild(XmlNodeType.DocumentType);

	internal virtual XmlDeclaration Declaration
	{
		get
		{
			if (HasChildNodes)
			{
				return FirstChild as XmlDeclaration;
			}
			return null;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlImplementation" /> object for the current document.</summary>
	/// <returns>The XmlImplementation object for the current document.</returns>
	public XmlImplementation Implementation => implementation;

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For XmlDocument nodes, the name is #document.</returns>
	public override string Name => strDocumentName;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For XmlDocument nodes, the local name is #document.</returns>
	public override string LocalName => strDocumentName;

	/// <summary>Gets the root <see cref="T:System.Xml.XmlElement" /> for the document.</summary>
	/// <returns>The XmlElement that represents the root of the XML document tree. If no root exists, null is returned.</returns>
	public XmlElement DocumentElement => (XmlElement)FindChild(XmlNodeType.Element);

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

	/// <summary>Gets the <see cref="T:System.Xml.XmlDocument" /> to which the current node belongs.</summary>
	/// <returns>For XmlDocument nodes (<see cref="P:System.Xml.XmlDocument.NodeType" /> equals XmlNodeType.Document), this property always returns null.</returns>
	public override XmlDocument OwnerDocument => null;

	/// <summary>Gets or sets the <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object associated with this <see cref="T:System.Xml.XmlDocument" />.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object containing the XML Schema Definition Language (XSD) schemas associated with this <see cref="T:System.Xml.XmlDocument" />; otherwise, an empty <see cref="T:System.Xml.Schema.XmlSchemaSet" /> object.</returns>
	public XmlSchemaSet Schemas
	{
		get
		{
			if (schemas == null)
			{
				schemas = new XmlSchemaSet(NameTable);
			}
			return schemas;
		}
		set
		{
			schemas = value;
		}
	}

	internal bool CanReportValidity => reportValidity;

	internal bool HasSetResolver => bSetResolver;

	/// <summary>Sets the <see cref="T:System.Xml.XmlResolver" /> to use for resolving external resources.</summary>
	/// <returns>The XmlResolver to use.In version 1.1 of the.NET Framework, the caller must be fully trusted in order to specify an XmlResolver.</returns>
	/// <exception cref="T:System.Xml.XmlException">This property is set to null and an external DTD or entity is encountered. </exception>
	public virtual XmlResolver XmlResolver
	{
		set
		{
			if (value != null)
			{
				try
				{
					new NamedPermissionSet("FullTrust").Demand();
				}
				catch (SecurityException inner)
				{
					throw new SecurityException(Res.GetString("XmlResolver can be set only by fully trusted code."), inner);
				}
			}
			resolver = value;
			if (!bSetResolver)
			{
				bSetResolver = true;
			}
			XmlDocumentType documentType = DocumentType;
			if (documentType != null)
			{
				documentType.DtdSchemaInfo = null;
			}
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.</summary>
	/// <returns>An XmlNameTable enabling you to get the atomized version of a string within the document.</returns>
	public XmlNameTable NameTable => implementation.NameTable;

	/// <summary>Gets or sets a value indicating whether to preserve white space in element content.</summary>
	/// <returns>true to preserve white space; otherwise false. The default is false.</returns>
	public bool PreserveWhitespace
	{
		get
		{
			return preserveWhitespace;
		}
		set
		{
			preserveWhitespace = value;
		}
	}

	/// <summary>Gets a value indicating whether the current node is read-only.</summary>
	/// <returns>true if the current node is read-only; otherwise false. XmlDocument nodes always return false.</returns>
	public override bool IsReadOnly => false;

	internal XmlNamedNodeMap Entities
	{
		get
		{
			if (entities == null)
			{
				entities = new XmlNamedNodeMap(this);
			}
			return entities;
		}
		set
		{
			entities = value;
		}
	}

	internal bool IsLoading
	{
		get
		{
			return isLoading;
		}
		set
		{
			isLoading = value;
		}
	}

	internal bool ActualLoadingStatus
	{
		get
		{
			return actualLoadingStatus;
		}
		set
		{
			actualLoadingStatus = value;
		}
	}

	internal Encoding TextEncoding
	{
		get
		{
			if (Declaration != null)
			{
				string encoding = Declaration.Encoding;
				if (encoding.Length > 0)
				{
					return System.Text.Encoding.GetEncoding(encoding);
				}
			}
			return null;
		}
	}

	/// <summary>Gets the concatenated values of the node and all its child nodes.</summary>
	/// <returns>The concatenated values of the node and all its child nodes.</returns>
	/// <exception cref="T:System.InvalidOperationException">Setting the value on the <see cref="P:System.Xml.XmlDocument.InnerText" /> property is not allowed.</exception>
	public override string InnerText
	{
		set
		{
			throw new InvalidOperationException(Res.GetString("The 'InnerText' of a 'Document' node is read-only and cannot be set."));
		}
	}

	/// <summary>Gets or sets the markup representing the children of the current node.</summary>
	/// <returns>The markup of the children of the current node.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML specified when setting this property is not well-formed. </exception>
	public override string InnerXml
	{
		get
		{
			return base.InnerXml;
		}
		set
		{
			LoadXml(value);
		}
	}

	internal string Version => Declaration?.Version;

	internal string Encoding => Declaration?.Encoding;

	internal string Standalone => Declaration?.Standalone;

	/// <summary>Returns the Post-Schema-Validation-Infoset (PSVI) of the node.</summary>
	/// <returns>The <see cref="T:System.Xml.Schema.IXmlSchemaInfo" /> object representing the PSVI of the node.</returns>
	public override IXmlSchemaInfo SchemaInfo
	{
		get
		{
			if (reportValidity)
			{
				XmlElement documentElement = DocumentElement;
				if (documentElement != null)
				{
					switch (documentElement.SchemaInfo.Validity)
					{
					case XmlSchemaValidity.Valid:
						return ValidSchemaInfo;
					case XmlSchemaValidity.Invalid:
						return InvalidSchemaInfo;
					}
				}
			}
			return NotKnownSchemaInfo;
		}
	}

	/// <summary>Gets the base URI of the current node.</summary>
	/// <returns>The location from which the node was loaded.</returns>
	public override string BaseURI => baseURI;

	internal override XPathNodeType XPNodeType => XPathNodeType.Root;

	internal bool HasEntityReferences => fEntRefNodesPresent;

	internal XmlAttribute NamespaceXml
	{
		get
		{
			if (namespaceXml == null)
			{
				namespaceXml = new XmlAttribute(AddAttrXmlName(strXmlns, strXml, strReservedXmlns, null), this);
				namespaceXml.Value = strReservedXml;
			}
			return namespaceXml;
		}
	}

	/// <summary>Occurs when a node belonging to this document is about to be inserted into another node.</summary>
	public event XmlNodeChangedEventHandler NodeInserting
	{
		add
		{
			onNodeInsertingDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeInsertingDelegate, value);
		}
		remove
		{
			onNodeInsertingDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeInsertingDelegate, value);
		}
	}

	/// <summary>Occurs when a node belonging to this document has been inserted into another node.</summary>
	public event XmlNodeChangedEventHandler NodeInserted
	{
		add
		{
			onNodeInsertedDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeInsertedDelegate, value);
		}
		remove
		{
			onNodeInsertedDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeInsertedDelegate, value);
		}
	}

	/// <summary>Occurs when a node belonging to this document is about to be removed from the document.</summary>
	public event XmlNodeChangedEventHandler NodeRemoving
	{
		add
		{
			onNodeRemovingDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeRemovingDelegate, value);
		}
		remove
		{
			onNodeRemovingDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeRemovingDelegate, value);
		}
	}

	/// <summary>Occurs when a node belonging to this document has been removed from its parent.</summary>
	public event XmlNodeChangedEventHandler NodeRemoved
	{
		add
		{
			onNodeRemovedDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeRemovedDelegate, value);
		}
		remove
		{
			onNodeRemovedDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeRemovedDelegate, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Xml.XmlNode.Value" /> of a node belonging to this document is about to be changed.</summary>
	public event XmlNodeChangedEventHandler NodeChanging
	{
		add
		{
			onNodeChangingDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeChangingDelegate, value);
		}
		remove
		{
			onNodeChangingDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeChangingDelegate, value);
		}
	}

	/// <summary>Occurs when the <see cref="P:System.Xml.XmlNode.Value" /> of a node belonging to this document has been changed.</summary>
	public event XmlNodeChangedEventHandler NodeChanged
	{
		add
		{
			onNodeChangedDelegate = (XmlNodeChangedEventHandler)Delegate.Combine(onNodeChangedDelegate, value);
		}
		remove
		{
			onNodeChangedDelegate = (XmlNodeChangedEventHandler)Delegate.Remove(onNodeChangedDelegate, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlDocument" /> class.</summary>
	public XmlDocument()
		: this(new XmlImplementation())
	{
	}

	/// <summary>Initializes a new instance of the XmlDocument class with the specified <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <param name="nt">The XmlNameTable to use. </param>
	public XmlDocument(XmlNameTable nt)
		: this(new XmlImplementation(nt))
	{
	}

	/// <summary>Initializes a new instance of the XmlDocument class with the specified <see cref="T:System.Xml.XmlImplementation" />.</summary>
	/// <param name="imp">The XmlImplementation to use. </param>
	protected internal XmlDocument(XmlImplementation imp)
	{
		implementation = imp;
		domNameTable = new DomNameTable(this);
		XmlNameTable nameTable = NameTable;
		nameTable.Add(string.Empty);
		strDocumentName = nameTable.Add("#document");
		strDocumentFragmentName = nameTable.Add("#document-fragment");
		strCommentName = nameTable.Add("#comment");
		strTextName = nameTable.Add("#text");
		strCDataSectionName = nameTable.Add("#cdata-section");
		strEntityName = nameTable.Add("#entity");
		strID = nameTable.Add("id");
		strNonSignificantWhitespaceName = nameTable.Add("#whitespace");
		strSignificantWhitespaceName = nameTable.Add("#significant-whitespace");
		strXmlns = nameTable.Add("xmlns");
		strXml = nameTable.Add("xml");
		strSpace = nameTable.Add("space");
		strLang = nameTable.Add("lang");
		strReservedXmlns = nameTable.Add("http://www.w3.org/2000/xmlns/");
		strReservedXml = nameTable.Add("http://www.w3.org/XML/1998/namespace");
		strEmpty = nameTable.Add(string.Empty);
		baseURI = string.Empty;
		objLock = new object();
	}

	internal static void CheckName(string name)
	{
		int num = ValidateNames.ParseNmtoken(name, 0);
		if (num < name.Length)
		{
			throw new XmlException("The '{0}' character, hexadecimal value {1}, cannot be included in a name.", XmlException.BuildCharExceptionArgs(name, num));
		}
	}

	internal XmlName AddXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
	{
		return domNameTable.AddName(prefix, localName, namespaceURI, schemaInfo);
	}

	internal XmlName GetXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
	{
		return domNameTable.GetName(prefix, localName, namespaceURI, schemaInfo);
	}

	internal XmlName AddAttrXmlName(string prefix, string localName, string namespaceURI, IXmlSchemaInfo schemaInfo)
	{
		XmlName xmlName = AddXmlName(prefix, localName, namespaceURI, schemaInfo);
		if (!IsLoading)
		{
			object prefix2 = xmlName.Prefix;
			object namespaceURI2 = xmlName.NamespaceURI;
			object localName2 = xmlName.LocalName;
			if ((prefix2 == strXmlns || (prefix2 == strEmpty && localName2 == strXmlns)) ^ (namespaceURI2 == strReservedXmlns))
			{
				throw new ArgumentException(Res.GetString("The namespace declaration attribute has an incorrect 'namespaceURI': '{0}'.", namespaceURI));
			}
		}
		return xmlName;
	}

	internal bool AddIdInfo(XmlName eleName, XmlName attrName)
	{
		if (htElementIDAttrDecl == null || htElementIDAttrDecl[eleName] == null)
		{
			if (htElementIDAttrDecl == null)
			{
				htElementIDAttrDecl = new Hashtable();
			}
			htElementIDAttrDecl.Add(eleName, attrName);
			return true;
		}
		return false;
	}

	private XmlName GetIDInfoByElement_(XmlName eleName)
	{
		XmlName xmlName = GetXmlName(eleName.Prefix, eleName.LocalName, string.Empty, null);
		if (xmlName != null)
		{
			return (XmlName)htElementIDAttrDecl[xmlName];
		}
		return null;
	}

	internal XmlName GetIDInfoByElement(XmlName eleName)
	{
		if (htElementIDAttrDecl == null)
		{
			return null;
		}
		return GetIDInfoByElement_(eleName);
	}

	private WeakReference GetElement(ArrayList elementList, XmlElement elem)
	{
		ArrayList arrayList = new ArrayList();
		foreach (WeakReference element in elementList)
		{
			if (!element.IsAlive)
			{
				arrayList.Add(element);
			}
			else if ((XmlElement)element.Target == elem)
			{
				return element;
			}
		}
		foreach (WeakReference item in arrayList)
		{
			elementList.Remove(item);
		}
		return null;
	}

	internal void AddElementWithId(string id, XmlElement elem)
	{
		if (htElementIdMap == null || !htElementIdMap.Contains(id))
		{
			if (htElementIdMap == null)
			{
				htElementIdMap = new Hashtable();
			}
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new WeakReference(elem));
			htElementIdMap.Add(id, arrayList);
		}
		else
		{
			ArrayList arrayList2 = (ArrayList)htElementIdMap[id];
			if (GetElement(arrayList2, elem) == null)
			{
				arrayList2.Add(new WeakReference(elem));
			}
		}
	}

	internal void RemoveElementWithId(string id, XmlElement elem)
	{
		if (htElementIdMap == null || !htElementIdMap.Contains(id))
		{
			return;
		}
		ArrayList arrayList = (ArrayList)htElementIdMap[id];
		WeakReference element = GetElement(arrayList, elem);
		if (element != null)
		{
			arrayList.Remove(element);
			if (arrayList.Count == 0)
			{
				htElementIdMap.Remove(id);
			}
		}
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned XmlDocument node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. </param>
	public override XmlNode CloneNode(bool deep)
	{
		XmlDocument xmlDocument = Implementation.CreateDocument();
		xmlDocument.SetBaseURI(baseURI);
		if (deep)
		{
			xmlDocument.ImportChildren(this, xmlDocument, deep);
		}
		return xmlDocument;
	}

	internal XmlResolver GetResolver()
	{
		return resolver;
	}

	internal override bool IsValidChildType(XmlNodeType type)
	{
		switch (type)
		{
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			return true;
		case XmlNodeType.DocumentType:
			if (DocumentType != null)
			{
				throw new InvalidOperationException(Res.GetString("This document already has a 'DocumentType' node."));
			}
			return true;
		case XmlNodeType.Element:
			if (DocumentElement != null)
			{
				throw new InvalidOperationException(Res.GetString("This document already has a 'DocumentElement' node."));
			}
			return true;
		case XmlNodeType.XmlDeclaration:
			if (Declaration != null)
			{
				throw new InvalidOperationException(Res.GetString("This document already has an 'XmlDeclaration' node."));
			}
			return true;
		default:
			return false;
		}
	}

	private bool HasNodeTypeInPrevSiblings(XmlNodeType nt, XmlNode refNode)
	{
		if (refNode == null)
		{
			return false;
		}
		XmlNode xmlNode = null;
		if (refNode.ParentNode != null)
		{
			xmlNode = refNode.ParentNode.FirstChild;
		}
		while (xmlNode != null)
		{
			if (xmlNode.NodeType == nt)
			{
				return true;
			}
			if (xmlNode == refNode)
			{
				break;
			}
			xmlNode = xmlNode.NextSibling;
		}
		return false;
	}

	private bool HasNodeTypeInNextSiblings(XmlNodeType nt, XmlNode refNode)
	{
		for (XmlNode xmlNode = refNode; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.NodeType == nt)
			{
				return true;
			}
		}
		return false;
	}

	internal override bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
	{
		if (refChild == null)
		{
			refChild = FirstChild;
		}
		if (refChild == null)
		{
			return true;
		}
		switch (newChild.NodeType)
		{
		case XmlNodeType.XmlDeclaration:
			return refChild == FirstChild;
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
			return refChild.NodeType != XmlNodeType.XmlDeclaration;
		case XmlNodeType.DocumentType:
			if (refChild.NodeType != XmlNodeType.XmlDeclaration)
			{
				return !HasNodeTypeInPrevSiblings(XmlNodeType.Element, refChild.PreviousSibling);
			}
			break;
		case XmlNodeType.Element:
			if (refChild.NodeType != XmlNodeType.XmlDeclaration)
			{
				return !HasNodeTypeInNextSiblings(XmlNodeType.DocumentType, refChild);
			}
			break;
		}
		return false;
	}

	internal override bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
	{
		if (refChild == null)
		{
			refChild = LastChild;
		}
		if (refChild == null)
		{
			return true;
		}
		switch (newChild.NodeType)
		{
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			return true;
		case XmlNodeType.DocumentType:
			return !HasNodeTypeInPrevSiblings(XmlNodeType.Element, refChild);
		case XmlNodeType.Element:
			return !HasNodeTypeInNextSiblings(XmlNodeType.DocumentType, refChild.NextSibling);
		default:
			return false;
		}
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlAttribute" /> with the specified <see cref="P:System.Xml.XmlDocument.Name" />.</summary>
	/// <returns>The new XmlAttribute.</returns>
	/// <param name="name">The qualified name of the attribute. If the name contains a colon, the <see cref="P:System.Xml.XmlNode.Prefix" /> property reflects the part of the name preceding the first colon and the <see cref="P:System.Xml.XmlDocument.LocalName" /> property reflects the part of the name following the first colon. The <see cref="P:System.Xml.XmlNode.NamespaceURI" /> remains empty unless the prefix is a recognized built-in prefix such as xmlns. In this case NamespaceURI has a value of http://www.w3.org/2000/xmlns/. </param>
	public XmlAttribute CreateAttribute(string name)
	{
		string prefix = string.Empty;
		string localName = string.Empty;
		string namespaceURI = string.Empty;
		XmlNode.SplitName(name, out prefix, out localName);
		SetDefaultNamespace(prefix, localName, ref namespaceURI);
		return CreateAttribute(prefix, localName, namespaceURI);
	}

	internal void SetDefaultNamespace(string prefix, string localName, ref string namespaceURI)
	{
		if (prefix == strXmlns || (prefix.Length == 0 && localName == strXmlns))
		{
			namespaceURI = strReservedXmlns;
		}
		else if (prefix == strXml)
		{
			namespaceURI = strReservedXml;
		}
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlCDataSection" /> containing the specified data.</summary>
	/// <returns>The new XmlCDataSection.</returns>
	/// <param name="data">The content of the new XmlCDataSection. </param>
	public virtual XmlCDataSection CreateCDataSection(string data)
	{
		fCDataNodesPresent = true;
		return new XmlCDataSection(data, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlComment" /> containing the specified data.</summary>
	/// <returns>The new XmlComment.</returns>
	/// <param name="data">The content of the new XmlComment. </param>
	public virtual XmlComment CreateComment(string data)
	{
		return new XmlComment(data, this);
	}

	/// <summary>Returns a new <see cref="T:System.Xml.XmlDocumentType" /> object.</summary>
	/// <returns>The new XmlDocumentType.</returns>
	/// <param name="name">Name of the document type. </param>
	/// <param name="publicId">The public identifier of the document type or null. You can specify a public URI and also a system identifier to identify the location of the external DTD subset.</param>
	/// <param name="systemId">The system identifier of the document type or null. Specifies the URL of the file location for the external DTD subset.</param>
	/// <param name="internalSubset">The DTD internal subset of the document type or null. </param>
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public virtual XmlDocumentType CreateDocumentType(string name, string publicId, string systemId, string internalSubset)
	{
		return new XmlDocumentType(name, publicId, systemId, internalSubset, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlDocumentFragment" />.</summary>
	/// <returns>The new XmlDocumentFragment.</returns>
	public virtual XmlDocumentFragment CreateDocumentFragment()
	{
		return new XmlDocumentFragment(this);
	}

	/// <summary>Creates an element with the specified name.</summary>
	/// <returns>The new XmlElement.</returns>
	/// <param name="name">The qualified name of the element. If the name contains a colon then the <see cref="P:System.Xml.XmlNode.Prefix" /> property reflects the part of the name preceding the colon and the <see cref="P:System.Xml.XmlDocument.LocalName" /> property reflects the part of the name after the colon. The qualified name cannot include a prefix of'xmlns'. </param>
	public XmlElement CreateElement(string name)
	{
		string prefix = string.Empty;
		string localName = string.Empty;
		XmlNode.SplitName(name, out prefix, out localName);
		return CreateElement(prefix, localName, string.Empty);
	}

	internal void AddDefaultAttributes(XmlElement elem)
	{
		SchemaInfo dtdSchemaInfo = DtdSchemaInfo;
		SchemaElementDecl schemaElementDecl = GetSchemaElementDecl(elem);
		if (schemaElementDecl == null || schemaElementDecl.AttDefs == null)
		{
			return;
		}
		IDictionaryEnumerator dictionaryEnumerator = schemaElementDecl.AttDefs.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			SchemaAttDef schemaAttDef = (SchemaAttDef)dictionaryEnumerator.Value;
			if (schemaAttDef.Presence == SchemaDeclBase.Use.Default || schemaAttDef.Presence == SchemaDeclBase.Use.Fixed)
			{
				string empty = string.Empty;
				string name = schemaAttDef.Name.Name;
				string attrNamespaceURI = string.Empty;
				if (dtdSchemaInfo.SchemaType == SchemaType.DTD)
				{
					empty = schemaAttDef.Name.Namespace;
				}
				else
				{
					empty = schemaAttDef.Prefix;
					attrNamespaceURI = schemaAttDef.Name.Namespace;
				}
				XmlAttribute attributeNode = PrepareDefaultAttribute(schemaAttDef, empty, name, attrNamespaceURI);
				elem.SetAttributeNode(attributeNode);
			}
		}
	}

	private SchemaElementDecl GetSchemaElementDecl(XmlElement elem)
	{
		SchemaInfo dtdSchemaInfo = DtdSchemaInfo;
		if (dtdSchemaInfo != null)
		{
			XmlQualifiedName key = new XmlQualifiedName(elem.LocalName, (dtdSchemaInfo.SchemaType == SchemaType.DTD) ? elem.Prefix : elem.NamespaceURI);
			if (dtdSchemaInfo.ElementDecls.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		return null;
	}

	private XmlAttribute PrepareDefaultAttribute(SchemaAttDef attdef, string attrPrefix, string attrLocalname, string attrNamespaceURI)
	{
		SetDefaultNamespace(attrPrefix, attrLocalname, ref attrNamespaceURI);
		XmlAttribute xmlAttribute = CreateDefaultAttribute(attrPrefix, attrLocalname, attrNamespaceURI);
		xmlAttribute.InnerXml = attdef.DefaultValueRaw;
		if (xmlAttribute is XmlUnspecifiedAttribute xmlUnspecifiedAttribute)
		{
			xmlUnspecifiedAttribute.SetSpecified(f: false);
		}
		return xmlAttribute;
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlEntityReference" /> with the specified name.</summary>
	/// <returns>The new XmlEntityReference.</returns>
	/// <param name="name">The name of the entity reference. </param>
	/// <exception cref="T:System.ArgumentException">The name is invalid (for example, names starting with'#' are invalid.) </exception>
	public virtual XmlEntityReference CreateEntityReference(string name)
	{
		return new XmlEntityReference(name, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlProcessingInstruction" /> with the specified name and data.</summary>
	/// <returns>The new XmlProcessingInstruction.</returns>
	/// <param name="target">The name of the processing instruction. </param>
	/// <param name="data">The data for the processing instruction. </param>
	public virtual XmlProcessingInstruction CreateProcessingInstruction(string target, string data)
	{
		return new XmlProcessingInstruction(target, data, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlDeclaration" /> node with the specified values.</summary>
	/// <returns>The new XmlDeclaration node.</returns>
	/// <param name="version">The version must be "1.0". </param>
	/// <param name="encoding">The value of the encoding attribute. This is the encoding that is used when you save the <see cref="T:System.Xml.XmlDocument" /> to a file or a stream; therefore, it must be set to a string supported by the <see cref="T:System.Text.Encoding" /> class, otherwise <see cref="M:System.Xml.XmlDocument.Save(System.String)" /> fails. If this is null or String.Empty, the Save method does not write an encoding attribute on the XML declaration and therefore the default encoding, UTF-8, is used.Note: If the XmlDocument is saved to either a <see cref="T:System.IO.TextWriter" /> or an <see cref="T:System.Xml.XmlTextWriter" />, this encoding value is discarded. Instead, the encoding of the TextWriter or the XmlTextWriter is used. This ensures that the XML written out can be read back using the correct encoding. </param>
	/// <param name="standalone">The value must be either "yes" or "no". If this is null or String.Empty, the Save method does not write a standalone attribute on the XML declaration. </param>
	/// <exception cref="T:System.ArgumentException">The values of <paramref name="version" /> or <paramref name="standalone" /> are something other than the ones specified above. </exception>
	public virtual XmlDeclaration CreateXmlDeclaration(string version, string encoding, string standalone)
	{
		return new XmlDeclaration(version, encoding, standalone, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlText" /> with the specified text.</summary>
	/// <returns>The new XmlText node.</returns>
	/// <param name="text">The text for the Text node. </param>
	public virtual XmlText CreateTextNode(string text)
	{
		return new XmlText(text, this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlSignificantWhitespace" /> node.</summary>
	/// <returns>A new XmlSignificantWhitespace node.</returns>
	/// <param name="text">The string must contain only the following characters &amp;#20; &amp;#10; &amp;#13; and &amp;#9; </param>
	public virtual XmlSignificantWhitespace CreateSignificantWhitespace(string text)
	{
		return new XmlSignificantWhitespace(text, this);
	}

	/// <summary>Creates a new <see cref="T:System.Xml.XPath.XPathNavigator" /> object for navigating this document.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object.</returns>
	public override XPathNavigator CreateNavigator()
	{
		return CreateNavigator(this);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XPath.XPathNavigator" /> object for navigating this document positioned on the <see cref="T:System.Xml.XmlNode" /> specified.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object.</returns>
	/// <param name="node">The <see cref="T:System.Xml.XmlNode" /> you want the navigator initially positioned on. </param>
	protected internal virtual XPathNavigator CreateNavigator(XmlNode node)
	{
		switch (node.NodeType)
		{
		case XmlNodeType.EntityReference:
		case XmlNodeType.Entity:
		case XmlNodeType.DocumentType:
		case XmlNodeType.Notation:
		case XmlNodeType.XmlDeclaration:
			return null;
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.SignificantWhitespace:
		{
			XmlNode xmlNode = node.ParentNode;
			if (xmlNode != null)
			{
				do
				{
					switch (xmlNode.NodeType)
					{
					case XmlNodeType.Attribute:
						return null;
					case XmlNodeType.EntityReference:
						goto IL_006a;
					}
					break;
					IL_006a:
					xmlNode = xmlNode.ParentNode;
				}
				while (xmlNode != null);
			}
			node = NormalizeText(node);
			break;
		}
		case XmlNodeType.Whitespace:
		{
			XmlNode xmlNode = node.ParentNode;
			if (xmlNode != null)
			{
				do
				{
					switch (xmlNode.NodeType)
					{
					case XmlNodeType.Attribute:
					case XmlNodeType.Document:
						return null;
					case XmlNodeType.EntityReference:
						goto IL_009f;
					}
					break;
					IL_009f:
					xmlNode = xmlNode.ParentNode;
				}
				while (xmlNode != null);
			}
			node = NormalizeText(node);
			break;
		}
		}
		return new DocumentXPathNavigator(this, node);
	}

	internal static bool IsTextNode(XmlNodeType nt)
	{
		if ((uint)(nt - 3) <= 1u || (uint)(nt - 13) <= 1u)
		{
			return true;
		}
		return false;
	}

	private XmlNode NormalizeText(XmlNode n)
	{
		XmlNode xmlNode = null;
		while (IsTextNode(n.NodeType))
		{
			xmlNode = n;
			n = n.PreviousSibling;
			if (n == null)
			{
				XmlNode xmlNode2 = xmlNode;
				while (xmlNode2.ParentNode != null && xmlNode2.ParentNode.NodeType == XmlNodeType.EntityReference)
				{
					if (xmlNode2.ParentNode.PreviousSibling != null)
					{
						n = xmlNode2.ParentNode.PreviousSibling;
						break;
					}
					xmlNode2 = xmlNode2.ParentNode;
					if (xmlNode2 == null)
					{
						break;
					}
				}
			}
			if (n == null)
			{
				break;
			}
			while (n.NodeType == XmlNodeType.EntityReference)
			{
				n = n.LastChild;
			}
		}
		return xmlNode;
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlWhitespace" /> node.</summary>
	/// <returns>A new XmlWhitespace node.</returns>
	/// <param name="text">The string must contain only the following characters &amp;#20; &amp;#10; &amp;#13; and &amp;#9; </param>
	public virtual XmlWhitespace CreateWhitespace(string text)
	{
		return new XmlWhitespace(text, this);
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlNodeList" /> containing a list of all descendant elements that match the specified <see cref="P:System.Xml.XmlDocument.Name" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a list of all matching nodes. If no nodes match <paramref name="name" />, the returned collection will be empty.</returns>
	/// <param name="name">The qualified name to match. It is matched against the Name property of the matching node. The special value "*" matches all tags. </param>
	public virtual XmlNodeList GetElementsByTagName(string name)
	{
		return new XmlElementList(this, name);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlAttribute" /> with the specified qualified name and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlAttribute.</returns>
	/// <param name="qualifiedName">The qualified name of the attribute. If the name contains a colon then the <see cref="P:System.Xml.XmlNode.Prefix" /> property will reflect the part of the name preceding the colon and the <see cref="P:System.Xml.XmlDocument.LocalName" /> property will reflect the part of the name after the colon. </param>
	/// <param name="namespaceURI">The namespaceURI of the attribute. If the qualified name includes a prefix of xmlns, then this parameter must be http://www.w3.org/2000/xmlns/. </param>
	public XmlAttribute CreateAttribute(string qualifiedName, string namespaceURI)
	{
		string prefix = string.Empty;
		string localName = string.Empty;
		XmlNode.SplitName(qualifiedName, out prefix, out localName);
		return CreateAttribute(prefix, localName, namespaceURI);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlElement" /> with the qualified name and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlElement.</returns>
	/// <param name="qualifiedName">The qualified name of the element. If the name contains a colon then the <see cref="P:System.Xml.XmlNode.Prefix" /> property will reflect the part of the name preceding the colon and the <see cref="P:System.Xml.XmlDocument.LocalName" /> property will reflect the part of the name after the colon. The qualified name cannot include a prefix of'xmlns'. </param>
	/// <param name="namespaceURI">The namespace URI of the element. </param>
	public XmlElement CreateElement(string qualifiedName, string namespaceURI)
	{
		string prefix = string.Empty;
		string localName = string.Empty;
		XmlNode.SplitName(qualifiedName, out prefix, out localName);
		return CreateElement(prefix, localName, namespaceURI);
	}

	/// <summary>Returns an <see cref="T:System.Xml.XmlNodeList" /> containing a list of all descendant elements that match the specified <see cref="P:System.Xml.XmlDocument.LocalName" /> and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNodeList" /> containing a list of all matching nodes. If no nodes match the specified <paramref name="localName" /> and <paramref name="namespaceURI" />, the returned collection will be empty.</returns>
	/// <param name="localName">The LocalName to match. The special value "*" matches all tags. </param>
	/// <param name="namespaceURI">NamespaceURI to match. </param>
	public virtual XmlNodeList GetElementsByTagName(string localName, string namespaceURI)
	{
		return new XmlElementList(this, localName, namespaceURI);
	}

	/// <summary>Gets the <see cref="T:System.Xml.XmlElement" /> with the specified ID.</summary>
	/// <returns>The XmlElement with the matching ID or null if no matching element is found.</returns>
	/// <param name="elementId">The attribute ID to match. </param>
	public virtual XmlElement GetElementById(string elementId)
	{
		if (htElementIdMap != null)
		{
			ArrayList arrayList = (ArrayList)htElementIdMap[elementId];
			if (arrayList != null)
			{
				foreach (WeakReference item in arrayList)
				{
					XmlElement xmlElement = (XmlElement)item.Target;
					if (xmlElement != null && xmlElement.IsConnected())
					{
						return xmlElement;
					}
				}
			}
		}
		return null;
	}

	/// <summary>Imports a node from another document to the current document.</summary>
	/// <returns>The imported <see cref="T:System.Xml.XmlNode" />.</returns>
	/// <param name="node">The node being imported. </param>
	/// <param name="deep">true to perform a deep clone; otherwise, false. </param>
	/// <exception cref="T:System.InvalidOperationException">Calling this method on a node type which cannot be imported. </exception>
	public virtual XmlNode ImportNode(XmlNode node, bool deep)
	{
		return ImportNodeInternal(node, deep);
	}

	private XmlNode ImportNodeInternal(XmlNode node, bool deep)
	{
		XmlNode xmlNode = null;
		if (node == null)
		{
			throw new InvalidOperationException(Res.GetString("Cannot import a null node."));
		}
		switch (node.NodeType)
		{
		case XmlNodeType.Element:
			xmlNode = CreateElement(node.Prefix, node.LocalName, node.NamespaceURI);
			ImportAttributes(node, xmlNode);
			if (deep)
			{
				ImportChildren(node, xmlNode, deep);
			}
			break;
		case XmlNodeType.Attribute:
			xmlNode = CreateAttribute(node.Prefix, node.LocalName, node.NamespaceURI);
			ImportChildren(node, xmlNode, deep: true);
			break;
		case XmlNodeType.Text:
			xmlNode = CreateTextNode(node.Value);
			break;
		case XmlNodeType.Comment:
			xmlNode = CreateComment(node.Value);
			break;
		case XmlNodeType.ProcessingInstruction:
			xmlNode = CreateProcessingInstruction(node.Name, node.Value);
			break;
		case XmlNodeType.XmlDeclaration:
		{
			XmlDeclaration xmlDeclaration = (XmlDeclaration)node;
			xmlNode = CreateXmlDeclaration(xmlDeclaration.Version, xmlDeclaration.Encoding, xmlDeclaration.Standalone);
			break;
		}
		case XmlNodeType.CDATA:
			xmlNode = CreateCDataSection(node.Value);
			break;
		case XmlNodeType.DocumentType:
		{
			XmlDocumentType xmlDocumentType = (XmlDocumentType)node;
			xmlNode = CreateDocumentType(xmlDocumentType.Name, xmlDocumentType.PublicId, xmlDocumentType.SystemId, xmlDocumentType.InternalSubset);
			break;
		}
		case XmlNodeType.DocumentFragment:
			xmlNode = CreateDocumentFragment();
			if (deep)
			{
				ImportChildren(node, xmlNode, deep);
			}
			break;
		case XmlNodeType.EntityReference:
			xmlNode = CreateEntityReference(node.Name);
			break;
		case XmlNodeType.Whitespace:
			xmlNode = CreateWhitespace(node.Value);
			break;
		case XmlNodeType.SignificantWhitespace:
			xmlNode = CreateSignificantWhitespace(node.Value);
			break;
		default:
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Res.GetString("Cannot import nodes of type '{0}'."), node.NodeType.ToString()));
		}
		return xmlNode;
	}

	private void ImportAttributes(XmlNode fromElem, XmlNode toElem)
	{
		int count = fromElem.Attributes.Count;
		for (int i = 0; i < count; i++)
		{
			if (fromElem.Attributes[i].Specified)
			{
				toElem.Attributes.SetNamedItem(ImportNodeInternal(fromElem.Attributes[i], deep: true));
			}
		}
	}

	private void ImportChildren(XmlNode fromNode, XmlNode toNode, bool deep)
	{
		for (XmlNode xmlNode = fromNode.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			toNode.AppendChild(ImportNodeInternal(xmlNode, deep));
		}
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlAttribute" /> with the specified <see cref="P:System.Xml.XmlNode.Prefix" />, <see cref="P:System.Xml.XmlDocument.LocalName" />, and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlAttribute.</returns>
	/// <param name="prefix">The prefix of the attribute (if any). String.Empty and null are equivalent. </param>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute (if any). String.Empty and null are equivalent. If <paramref name="prefix" /> is xmlns, then this parameter must be http://www.w3.org/2000/xmlns/; otherwise an exception is thrown. </param>
	public virtual XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
	{
		return new XmlAttribute(AddAttrXmlName(prefix, localName, namespaceURI, null), this);
	}

	/// <summary>Creates a default attribute with the specified prefix, local name and namespace URI.</summary>
	/// <returns>The new <see cref="T:System.Xml.XmlAttribute" />.</returns>
	/// <param name="prefix">The prefix of the attribute (if any). </param>
	/// <param name="localName">The local name of the attribute. </param>
	/// <param name="namespaceURI">The namespace URI of the attribute (if any). </param>
	protected internal virtual XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
	{
		return new XmlUnspecifiedAttribute(prefix, localName, namespaceURI, this);
	}

	/// <summary>Creates an element with the specified <see cref="P:System.Xml.XmlNode.Prefix" />, <see cref="P:System.Xml.XmlDocument.LocalName" />, and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new <see cref="T:System.Xml.XmlElement" />.</returns>
	/// <param name="prefix">The prefix of the new element (if any). String.Empty and null are equivalent. </param>
	/// <param name="localName">The local name of the new element. </param>
	/// <param name="namespaceURI">The namespace URI of the new element (if any). String.Empty and null are equivalent. </param>
	public virtual XmlElement CreateElement(string prefix, string localName, string namespaceURI)
	{
		XmlElement xmlElement = new XmlElement(AddXmlName(prefix, localName, namespaceURI, null), empty: true, this);
		if (!IsLoading)
		{
			AddDefaultAttributes(xmlElement);
		}
		return xmlElement;
	}

	/// <summary>Creates a <see cref="T:System.Xml.XmlNode" /> with the specified <see cref="T:System.Xml.XmlNodeType" />, <see cref="P:System.Xml.XmlNode.Prefix" />, <see cref="P:System.Xml.XmlDocument.Name" />, and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlNode.</returns>
	/// <param name="type">The XmlNodeType of the new node. </param>
	/// <param name="prefix">The prefix of the new node. </param>
	/// <param name="name">The local name of the new node. </param>
	/// <param name="namespaceURI">The namespace URI of the new node. </param>
	/// <exception cref="T:System.ArgumentException">The name was not provided and the XmlNodeType requires a name. </exception>
	public virtual XmlNode CreateNode(XmlNodeType type, string prefix, string name, string namespaceURI)
	{
		switch (type)
		{
		case XmlNodeType.Element:
			if (prefix != null)
			{
				return CreateElement(prefix, name, namespaceURI);
			}
			return CreateElement(name, namespaceURI);
		case XmlNodeType.Attribute:
			if (prefix != null)
			{
				return CreateAttribute(prefix, name, namespaceURI);
			}
			return CreateAttribute(name, namespaceURI);
		case XmlNodeType.Text:
			return CreateTextNode(string.Empty);
		case XmlNodeType.CDATA:
			return CreateCDataSection(string.Empty);
		case XmlNodeType.EntityReference:
			return CreateEntityReference(name);
		case XmlNodeType.ProcessingInstruction:
			return CreateProcessingInstruction(name, string.Empty);
		case XmlNodeType.XmlDeclaration:
			return CreateXmlDeclaration("1.0", null, null);
		case XmlNodeType.Comment:
			return CreateComment(string.Empty);
		case XmlNodeType.DocumentFragment:
			return CreateDocumentFragment();
		case XmlNodeType.DocumentType:
			return CreateDocumentType(name, string.Empty, string.Empty, string.Empty);
		case XmlNodeType.Document:
			return new XmlDocument();
		case XmlNodeType.SignificantWhitespace:
			return CreateSignificantWhitespace(string.Empty);
		case XmlNodeType.Whitespace:
			return CreateWhitespace(string.Empty);
		default:
			throw new ArgumentException(Res.GetString("Cannot create node of type {0}.", type));
		}
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlNode" /> with the specified node type, <see cref="P:System.Xml.XmlDocument.Name" />, and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlNode.</returns>
	/// <param name="nodeTypeString">String version of the <see cref="T:System.Xml.XmlNodeType" /> of the new node. This parameter must be one of the values listed in the table below. </param>
	/// <param name="name">The qualified name of the new node. If the name contains a colon, it is parsed into <see cref="P:System.Xml.XmlNode.Prefix" /> and <see cref="P:System.Xml.XmlDocument.LocalName" /> components. </param>
	/// <param name="namespaceURI">The namespace URI of the new node. </param>
	/// <exception cref="T:System.ArgumentException">The name was not provided and the XmlNodeType requires a name; or <paramref name="nodeTypeString" /> is not one of the strings listed below. </exception>
	public virtual XmlNode CreateNode(string nodeTypeString, string name, string namespaceURI)
	{
		return CreateNode(ConvertToNodeType(nodeTypeString), name, namespaceURI);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlNode" /> with the specified <see cref="T:System.Xml.XmlNodeType" />, <see cref="P:System.Xml.XmlDocument.Name" />, and <see cref="P:System.Xml.XmlNode.NamespaceURI" />.</summary>
	/// <returns>The new XmlNode.</returns>
	/// <param name="type">The XmlNodeType of the new node. </param>
	/// <param name="name">The qualified name of the new node. If the name contains a colon then it is parsed into <see cref="P:System.Xml.XmlNode.Prefix" /> and <see cref="P:System.Xml.XmlDocument.LocalName" /> components. </param>
	/// <param name="namespaceURI">The namespace URI of the new node. </param>
	/// <exception cref="T:System.ArgumentException">The name was not provided and the XmlNodeType requires a name. </exception>
	public virtual XmlNode CreateNode(XmlNodeType type, string name, string namespaceURI)
	{
		return CreateNode(type, null, name, namespaceURI);
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlNode" /> object based on the information in the <see cref="T:System.Xml.XmlReader" />. The reader must be positioned on a node or attribute.</summary>
	/// <returns>The new XmlNode or null if no more nodes exist.</returns>
	/// <param name="reader">The XML source </param>
	/// <exception cref="T:System.NullReferenceException">The reader is positioned on a node type that does not translate to a valid DOM node (for example, EndElement or EndEntity). </exception>
	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	public virtual XmlNode ReadNode(XmlReader reader)
	{
		XmlNode xmlNode = null;
		try
		{
			IsLoading = true;
			return new XmlLoader().ReadCurrentNode(this, reader);
		}
		finally
		{
			IsLoading = false;
		}
	}

	internal XmlNodeType ConvertToNodeType(string nodeTypeString)
	{
		return nodeTypeString switch
		{
			"element" => XmlNodeType.Element, 
			"attribute" => XmlNodeType.Attribute, 
			"text" => XmlNodeType.Text, 
			"cdatasection" => XmlNodeType.CDATA, 
			"entityreference" => XmlNodeType.EntityReference, 
			"entity" => XmlNodeType.Entity, 
			"processinginstruction" => XmlNodeType.ProcessingInstruction, 
			"comment" => XmlNodeType.Comment, 
			"document" => XmlNodeType.Document, 
			"documenttype" => XmlNodeType.DocumentType, 
			"documentfragment" => XmlNodeType.DocumentFragment, 
			"notation" => XmlNodeType.Notation, 
			"significantwhitespace" => XmlNodeType.SignificantWhitespace, 
			"whitespace" => XmlNodeType.Whitespace, 
			_ => throw new ArgumentException(Res.GetString("'{0}' does not represent any 'XmlNodeType'.", nodeTypeString)), 
		};
	}

	private XmlTextReader SetupReader(XmlTextReader tr)
	{
		tr.XmlValidatingReaderCompatibilityMode = true;
		tr.EntityHandling = EntityHandling.ExpandCharEntities;
		if (HasSetResolver)
		{
			tr.XmlResolver = GetResolver();
		}
		return tr;
	}

	/// <summary>Loads the XML document from the specified URL.</summary>
	/// <param name="filename">URL for the file containing the XML document to load. The URL can be either a local file or an HTTP URL (a Web address).</param>
	/// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, a <see cref="T:System.IO.FileNotFoundException" /> is raised. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="filename" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="filename" /> is null. </exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. </exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="filename" /> specified a file that is read-only.-or- This operation is not supported on the current platform.-or- <paramref name="filename" /> specified a directory.-or- The caller does not have the required permission. </exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="filename" /> was not found. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="filename" /> is in an invalid format. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	public virtual void Load(string filename)
	{
		XmlTextReader xmlTextReader = SetupReader(new XmlTextReader(filename, NameTable));
		try
		{
			Load(xmlTextReader);
		}
		finally
		{
			xmlTextReader.Close();
		}
	}

	/// <summary>Loads the XML document from the specified stream.</summary>
	/// <param name="inStream">The stream containing the XML document to load. </param>
	/// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, a <see cref="T:System.IO.FileNotFoundException" /> is raised. </exception>
	public virtual void Load(Stream inStream)
	{
		XmlTextReader xmlTextReader = SetupReader(new XmlTextReader(inStream, NameTable));
		try
		{
			Load(xmlTextReader);
		}
		finally
		{
			xmlTextReader.Impl.Close(closeInput: false);
		}
	}

	/// <summary>Loads the XML document from the specified <see cref="T:System.IO.TextReader" />.</summary>
	/// <param name="txtReader">The TextReader used to feed the XML data into the document. </param>
	/// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, the document remains empty. </exception>
	public virtual void Load(TextReader txtReader)
	{
		XmlTextReader xmlTextReader = SetupReader(new XmlTextReader(txtReader, NameTable));
		try
		{
			Load(xmlTextReader);
		}
		finally
		{
			xmlTextReader.Impl.Close(closeInput: false);
		}
	}

	/// <summary>Loads the XML document from the specified <see cref="T:System.Xml.XmlReader" />.</summary>
	/// <param name="reader">The XmlReader used to feed the XML data into the document. </param>
	/// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, the document remains empty. </exception>
	public virtual void Load(XmlReader reader)
	{
		try
		{
			IsLoading = true;
			actualLoadingStatus = true;
			RemoveAll();
			fEntRefNodesPresent = false;
			fCDataNodesPresent = false;
			reportValidity = true;
			new XmlLoader().Load(this, reader, preserveWhitespace);
		}
		finally
		{
			IsLoading = false;
			actualLoadingStatus = false;
			reportValidity = true;
		}
	}

	/// <summary>Loads the XML document from the specified string.</summary>
	/// <param name="xml">String containing the XML document to load. </param>
	/// <exception cref="T:System.Xml.XmlException">There is a load or parse error in the XML. In this case, the document remains empty. </exception>
	public virtual void LoadXml(string xml)
	{
		XmlTextReader xmlTextReader = SetupReader(new XmlTextReader(new StringReader(xml), NameTable));
		try
		{
			Load(xmlTextReader);
		}
		finally
		{
			xmlTextReader.Close();
		}
	}

	/// <summary>Saves the XML document to the specified file.</summary>
	/// <param name="filename">The location of the file where you want to save the document. </param>
	/// <exception cref="T:System.Xml.XmlException">The operation would not result in a well formed XML document (for example, no document element or duplicate XML declarations). </exception>
	public virtual void Save(string filename)
	{
		if (DocumentElement == null)
		{
			throw new XmlException("Invalid XML document. {0}", Res.GetString("The document does not have a root element."));
		}
		XmlDOMTextWriter xmlDOMTextWriter = new XmlDOMTextWriter(filename, TextEncoding);
		try
		{
			if (!preserveWhitespace)
			{
				xmlDOMTextWriter.Formatting = Formatting.Indented;
			}
			WriteTo(xmlDOMTextWriter);
			xmlDOMTextWriter.Flush();
		}
		finally
		{
			xmlDOMTextWriter.Close();
		}
	}

	/// <summary>Saves the XML document to the specified stream.</summary>
	/// <param name="outStream">The stream to which you want to save. </param>
	/// <exception cref="T:System.Xml.XmlException">The operation would not result in a well formed XML document (for example, no document element or duplicate XML declarations). </exception>
	public virtual void Save(Stream outStream)
	{
		XmlDOMTextWriter xmlDOMTextWriter = new XmlDOMTextWriter(outStream, TextEncoding);
		if (!preserveWhitespace)
		{
			xmlDOMTextWriter.Formatting = Formatting.Indented;
		}
		WriteTo(xmlDOMTextWriter);
		xmlDOMTextWriter.Flush();
	}

	/// <summary>Saves the XML document to the specified <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="writer">The TextWriter to which you want to save. </param>
	/// <exception cref="T:System.Xml.XmlException">The operation would not result in a well formed XML document (for example, no document element or duplicate XML declarations). </exception>
	public virtual void Save(TextWriter writer)
	{
		XmlDOMTextWriter xmlDOMTextWriter = new XmlDOMTextWriter(writer);
		if (!preserveWhitespace)
		{
			xmlDOMTextWriter.Formatting = Formatting.Indented;
		}
		Save(xmlDOMTextWriter);
	}

	/// <summary>Saves the XML document to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	/// <exception cref="T:System.Xml.XmlException">The operation would not result in a well formed XML document (for example, no document element or duplicate XML declarations). </exception>
	public virtual void Save(XmlWriter w)
	{
		XmlNode xmlNode = FirstChild;
		if (xmlNode == null)
		{
			return;
		}
		if (w.WriteState == WriteState.Start)
		{
			if (xmlNode is XmlDeclaration)
			{
				if (Standalone.Length == 0)
				{
					w.WriteStartDocument();
				}
				else if (Standalone == "yes")
				{
					w.WriteStartDocument(standalone: true);
				}
				else if (Standalone == "no")
				{
					w.WriteStartDocument(standalone: false);
				}
				xmlNode = xmlNode.NextSibling;
			}
			else
			{
				w.WriteStartDocument();
			}
		}
		while (xmlNode != null)
		{
			xmlNode.WriteTo(w);
			xmlNode = xmlNode.NextSibling;
		}
		w.Flush();
	}

	/// <summary>Saves the XmlDocument node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		WriteContentTo(w);
	}

	/// <summary>Saves all the children of the XmlDocument node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="xw">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter xw)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((XmlNode)enumerator.Current).WriteTo(xw);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	/// <summary>Validates the <see cref="T:System.Xml.XmlDocument" /> against the XML Schema Definition Language (XSD) schemas contained in the <see cref="P:System.Xml.XmlDocument.Schemas" /> property.</summary>
	/// <param name="validationEventHandler">The <see cref="T:System.Xml.Schema.ValidationEventHandler" /> object that receives information about schema validation warnings and errors.</param>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">A schema validation event occurred and no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> object was specified.</exception>
	public void Validate(ValidationEventHandler validationEventHandler)
	{
		Validate(validationEventHandler, this);
	}

	/// <summary>Validates the <see cref="T:System.Xml.XmlNode" /> object specified against the XML Schema Definition Language (XSD) schemas in the <see cref="P:System.Xml.XmlDocument.Schemas" /> property.</summary>
	/// <param name="validationEventHandler">The <see cref="T:System.Xml.Schema.ValidationEventHandler" /> object that receives information about schema validation warnings and errors.</param>
	/// <param name="nodeToValidate">The <see cref="T:System.Xml.XmlNode" /> object created from an <see cref="T:System.Xml.XmlDocument" /> to validate.</param>
	/// <exception cref="T:System.ArgumentException">The <see cref="T:System.Xml.XmlNode" /> object parameter was not created from an <see cref="T:System.Xml.XmlDocument" />.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Xml.XmlNode" /> object parameter is not an element, attribute, document fragment, or the root node.</exception>
	/// <exception cref="T:System.Xml.Schema.XmlSchemaValidationException">A schema validation event occurred and no <see cref="T:System.Xml.Schema.ValidationEventHandler" /> object was specified.</exception>
	public void Validate(ValidationEventHandler validationEventHandler, XmlNode nodeToValidate)
	{
		if (schemas == null || schemas.Count == 0)
		{
			throw new InvalidOperationException(Res.GetString("The XmlSchemaSet on the document is either null or has no schemas in it. Provide schema information before calling Validate."));
		}
		if (nodeToValidate.Document != this)
		{
			throw new ArgumentException(Res.GetString("Cannot validate '{0}' because its owner document is not the current document.", "nodeToValidate"));
		}
		if (nodeToValidate == this)
		{
			reportValidity = false;
		}
		new DocumentSchemaValidator(this, schemas, validationEventHandler).Validate(nodeToValidate);
		if (nodeToValidate == this)
		{
			reportValidity = true;
		}
	}

	internal override XmlNodeChangedEventArgs GetEventArgs(XmlNode node, XmlNode oldParent, XmlNode newParent, string oldValue, string newValue, XmlNodeChangedAction action)
	{
		reportValidity = false;
		switch (action)
		{
		case XmlNodeChangedAction.Insert:
			if (onNodeInsertingDelegate == null && onNodeInsertedDelegate == null)
			{
				return null;
			}
			break;
		case XmlNodeChangedAction.Remove:
			if (onNodeRemovingDelegate == null && onNodeRemovedDelegate == null)
			{
				return null;
			}
			break;
		case XmlNodeChangedAction.Change:
			if (onNodeChangingDelegate == null && onNodeChangedDelegate == null)
			{
				return null;
			}
			break;
		}
		return new XmlNodeChangedEventArgs(node, oldParent, newParent, oldValue, newValue, action);
	}

	internal XmlNodeChangedEventArgs GetInsertEventArgsForLoad(XmlNode node, XmlNode newParent)
	{
		if (onNodeInsertingDelegate == null && onNodeInsertedDelegate == null)
		{
			return null;
		}
		string value = node.Value;
		return new XmlNodeChangedEventArgs(node, null, newParent, value, value, XmlNodeChangedAction.Insert);
	}

	internal override void BeforeEvent(XmlNodeChangedEventArgs args)
	{
		if (args == null)
		{
			return;
		}
		switch (args.Action)
		{
		case XmlNodeChangedAction.Insert:
			if (onNodeInsertingDelegate != null)
			{
				onNodeInsertingDelegate(this, args);
			}
			break;
		case XmlNodeChangedAction.Remove:
			if (onNodeRemovingDelegate != null)
			{
				onNodeRemovingDelegate(this, args);
			}
			break;
		case XmlNodeChangedAction.Change:
			if (onNodeChangingDelegate != null)
			{
				onNodeChangingDelegate(this, args);
			}
			break;
		}
	}

	internal override void AfterEvent(XmlNodeChangedEventArgs args)
	{
		if (args == null)
		{
			return;
		}
		switch (args.Action)
		{
		case XmlNodeChangedAction.Insert:
			if (onNodeInsertedDelegate != null)
			{
				onNodeInsertedDelegate(this, args);
			}
			break;
		case XmlNodeChangedAction.Remove:
			if (onNodeRemovedDelegate != null)
			{
				onNodeRemovedDelegate(this, args);
			}
			break;
		case XmlNodeChangedAction.Change:
			if (onNodeChangedDelegate != null)
			{
				onNodeChangedDelegate(this, args);
			}
			break;
		}
	}

	internal XmlAttribute GetDefaultAttribute(XmlElement elem, string attrPrefix, string attrLocalname, string attrNamespaceURI)
	{
		SchemaInfo dtdSchemaInfo = DtdSchemaInfo;
		SchemaElementDecl schemaElementDecl = GetSchemaElementDecl(elem);
		if (schemaElementDecl != null && schemaElementDecl.AttDefs != null)
		{
			IDictionaryEnumerator dictionaryEnumerator = schemaElementDecl.AttDefs.GetEnumerator();
			while (dictionaryEnumerator.MoveNext())
			{
				SchemaAttDef schemaAttDef = (SchemaAttDef)dictionaryEnumerator.Value;
				if ((schemaAttDef.Presence == SchemaDeclBase.Use.Default || schemaAttDef.Presence == SchemaDeclBase.Use.Fixed) && schemaAttDef.Name.Name == attrLocalname && ((dtdSchemaInfo.SchemaType == SchemaType.DTD && schemaAttDef.Name.Namespace == attrPrefix) || (dtdSchemaInfo.SchemaType != SchemaType.DTD && schemaAttDef.Name.Namespace == attrNamespaceURI)))
				{
					return PrepareDefaultAttribute(schemaAttDef, attrPrefix, attrLocalname, attrNamespaceURI);
				}
			}
		}
		return null;
	}

	internal XmlEntity GetEntityNode(string name)
	{
		if (DocumentType != null)
		{
			XmlNamedNodeMap xmlNamedNodeMap = DocumentType.Entities;
			if (xmlNamedNodeMap != null)
			{
				return (XmlEntity)xmlNamedNodeMap.GetNamedItem(name);
			}
		}
		return null;
	}

	internal void SetBaseURI(string inBaseURI)
	{
		baseURI = inBaseURI;
	}

	internal override XmlNode AppendChildForLoad(XmlNode newChild, XmlDocument doc)
	{
		if (!IsValidChildType(newChild.NodeType))
		{
			throw new InvalidOperationException(Res.GetString("The specified node cannot be inserted as the valid child of this node, because the specified node is the wrong type."));
		}
		if (!CanInsertAfter(newChild, LastChild))
		{
			throw new InvalidOperationException(Res.GetString("Cannot insert the node in the specified location."));
		}
		XmlNodeChangedEventArgs insertEventArgsForLoad = GetInsertEventArgsForLoad(newChild, this);
		if (insertEventArgsForLoad != null)
		{
			BeforeEvent(insertEventArgsForLoad);
		}
		XmlLinkedNode xmlLinkedNode = (XmlLinkedNode)newChild;
		if (lastChild == null)
		{
			xmlLinkedNode.next = xmlLinkedNode;
		}
		else
		{
			xmlLinkedNode.next = lastChild.next;
			lastChild.next = xmlLinkedNode;
		}
		lastChild = xmlLinkedNode;
		xmlLinkedNode.SetParentForLoad(this);
		if (insertEventArgsForLoad != null)
		{
			AfterEvent(insertEventArgsForLoad);
		}
		return xmlLinkedNode;
	}
}
