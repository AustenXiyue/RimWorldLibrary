using System.Collections.Generic;
using System.IO;
using MS.Internal.Xml.Cache;

namespace System.Xml.XPath;

/// <summary>Provides a fast, read-only, in-memory representation of an XML document by using the XPath data model.</summary>
public class XPathDocument : IXPathNavigable
{
	internal enum LoadFlags
	{
		None,
		AtomizeNames,
		Fragment
	}

	private XPathNode[] pageText;

	private XPathNode[] pageRoot;

	private XPathNode[] pageXmlNmsp;

	private int idxText;

	private int idxRoot;

	private int idxXmlNmsp;

	private XmlNameTable nameTable;

	private bool hasLineInfo;

	private Dictionary<XPathNodeRef, XPathNodeRef> mapNmsp;

	private Dictionary<string, XPathNodeRef> idValueMap;

	internal XmlNameTable NameTable => nameTable;

	internal bool HasLineInfo => hasLineInfo;

	internal XPathDocument()
	{
		nameTable = new NameTable();
	}

	internal XPathDocument(XmlNameTable nameTable)
	{
		if (nameTable == null)
		{
			throw new ArgumentNullException("nameTable");
		}
		this.nameTable = nameTable;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data that is contained in the specified <see cref="T:System.Xml.XmlReader" /> object.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> object that contains the XML data. </param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object passed as a parameter is null.</exception>
	public XPathDocument(XmlReader reader)
		: this(reader, XmlSpace.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data that is contained in the specified <see cref="T:System.Xml.XmlReader" /> object with the specified white space handling.</summary>
	/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> object that contains the XML data.</param>
	/// <param name="space">An <see cref="T:System.Xml.XmlSpace" /> object.</param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.Xml.XmlReader" /> object parameter or <see cref="T:System.Xml.XmlSpace" /> object parameter is null.</exception>
	public XPathDocument(XmlReader reader, XmlSpace space)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		LoadFromReader(reader, space);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data that is contained in the specified <see cref="T:System.IO.TextReader" /> object.</summary>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> object that contains the XML data.</param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.IO.TextReader" /> object passed as a parameter is null.</exception>
	public XPathDocument(TextReader textReader)
	{
		XmlTextReaderImpl xmlTextReaderImpl = SetupReader(new XmlTextReaderImpl(string.Empty, textReader));
		try
		{
			LoadFromReader(xmlTextReaderImpl, XmlSpace.Default);
		}
		finally
		{
			xmlTextReaderImpl.Close();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data in the specified <see cref="T:System.IO.Stream" /> object.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> object that contains the XML data.</param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="T:System.IO.Stream" /> object passed as a parameter is null.</exception>
	public XPathDocument(Stream stream)
	{
		XmlTextReaderImpl xmlTextReaderImpl = SetupReader(new XmlTextReaderImpl(string.Empty, stream));
		try
		{
			LoadFromReader(xmlTextReaderImpl, XmlSpace.Default);
		}
		finally
		{
			xmlTextReaderImpl.Close();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data in the specified file.</summary>
	/// <param name="uri">The path of the file that contains the XML data.</param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The file path parameter is null.</exception>
	public XPathDocument(string uri)
		: this(uri, XmlSpace.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathDocument" /> class from the XML data in the file specified with the white space handling specified.</summary>
	/// <param name="uri">The path of the file that contains the XML data.</param>
	/// <param name="space">An <see cref="T:System.Xml.XmlSpace" /> object.</param>
	/// <exception cref="T:System.Xml.XmlException">An error was encountered in the XML data. The <see cref="T:System.Xml.XPath.XPathDocument" /> remains empty. </exception>
	/// <exception cref="T:System.ArgumentNullException">The file path parameter or <see cref="T:System.Xml.XmlSpace" /> object parameter is null.</exception>
	public XPathDocument(string uri, XmlSpace space)
	{
		XmlTextReaderImpl xmlTextReaderImpl = SetupReader(new XmlTextReaderImpl(uri));
		try
		{
			LoadFromReader(xmlTextReaderImpl, space);
		}
		finally
		{
			xmlTextReaderImpl.Close();
		}
	}

	internal XmlRawWriter LoadFromWriter(LoadFlags flags, string baseUri)
	{
		return new XPathDocumentBuilder(this, null, baseUri, flags);
	}

	internal void LoadFromReader(XmlReader reader, XmlSpace space)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		IXmlLineInfo xmlLineInfo = reader as IXmlLineInfo;
		if (xmlLineInfo == null || !xmlLineInfo.HasLineInfo())
		{
			xmlLineInfo = null;
		}
		hasLineInfo = xmlLineInfo != null;
		nameTable = reader.NameTable;
		XPathDocumentBuilder xPathDocumentBuilder = new XPathDocumentBuilder(this, xmlLineInfo, reader.BaseURI, LoadFlags.None);
		try
		{
			bool flag = reader.ReadState == ReadState.Initial;
			int depth = reader.Depth;
			string text = nameTable.Get("http://www.w3.org/2000/xmlns/");
			if (flag && !reader.Read())
			{
				return;
			}
			while (flag || reader.Depth >= depth)
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
				{
					bool isEmptyElement = reader.IsEmptyElement;
					xPathDocumentBuilder.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.BaseURI);
					while (reader.MoveToNextAttribute())
					{
						string namespaceURI = reader.NamespaceURI;
						if ((object)namespaceURI == text)
						{
							if (reader.Prefix.Length == 0)
							{
								xPathDocumentBuilder.WriteNamespaceDeclaration(string.Empty, reader.Value);
							}
							else
							{
								xPathDocumentBuilder.WriteNamespaceDeclaration(reader.LocalName, reader.Value);
							}
						}
						else
						{
							xPathDocumentBuilder.WriteStartAttribute(reader.Prefix, reader.LocalName, namespaceURI);
							xPathDocumentBuilder.WriteString(reader.Value, TextBlockType.Text);
							xPathDocumentBuilder.WriteEndAttribute();
						}
					}
					if (isEmptyElement)
					{
						xPathDocumentBuilder.WriteEndElement(allowShortcutTag: true);
					}
					break;
				}
				case XmlNodeType.EndElement:
					xPathDocumentBuilder.WriteEndElement(allowShortcutTag: false);
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
					xPathDocumentBuilder.WriteString(reader.Value, TextBlockType.Text);
					break;
				case XmlNodeType.SignificantWhitespace:
					if (reader.XmlSpace == XmlSpace.Preserve)
					{
						xPathDocumentBuilder.WriteString(reader.Value, TextBlockType.SignificantWhitespace);
						break;
					}
					goto case XmlNodeType.Whitespace;
				case XmlNodeType.Whitespace:
					if (space == XmlSpace.Preserve && (!flag || reader.Depth != 0))
					{
						xPathDocumentBuilder.WriteString(reader.Value, TextBlockType.Whitespace);
					}
					break;
				case XmlNodeType.Comment:
					xPathDocumentBuilder.WriteComment(reader.Value);
					break;
				case XmlNodeType.ProcessingInstruction:
					xPathDocumentBuilder.WriteProcessingInstruction(reader.LocalName, reader.Value, reader.BaseURI);
					break;
				case XmlNodeType.EntityReference:
					reader.ResolveEntity();
					break;
				case XmlNodeType.DocumentType:
				{
					IDtdInfo dtdInfo = reader.DtdInfo;
					if (dtdInfo != null)
					{
						xPathDocumentBuilder.CreateIdTables(dtdInfo);
					}
					break;
				}
				}
				if (!reader.Read())
				{
					break;
				}
			}
		}
		finally
		{
			xPathDocumentBuilder.Close();
		}
	}

	/// <summary>Initializes a read-only <see cref="T:System.Xml.XPath.XPathNavigator" /> object for navigating through nodes in this <see cref="T:System.Xml.XPath.XPathDocument" />.</summary>
	/// <returns>A read-only <see cref="T:System.Xml.XPath.XPathNavigator" /> object.</returns>
	public XPathNavigator CreateNavigator()
	{
		return new XPathDocumentNavigator(pageRoot, idxRoot, null, 0);
	}

	internal int GetCollapsedTextNode(out XPathNode[] pageText)
	{
		pageText = this.pageText;
		return idxText;
	}

	internal void SetCollapsedTextNode(XPathNode[] pageText, int idxText)
	{
		this.pageText = pageText;
		this.idxText = idxText;
	}

	internal int GetRootNode(out XPathNode[] pageRoot)
	{
		pageRoot = this.pageRoot;
		return idxRoot;
	}

	internal void SetRootNode(XPathNode[] pageRoot, int idxRoot)
	{
		this.pageRoot = pageRoot;
		this.idxRoot = idxRoot;
	}

	internal int GetXmlNamespaceNode(out XPathNode[] pageXmlNmsp)
	{
		pageXmlNmsp = this.pageXmlNmsp;
		return idxXmlNmsp;
	}

	internal void SetXmlNamespaceNode(XPathNode[] pageXmlNmsp, int idxXmlNmsp)
	{
		this.pageXmlNmsp = pageXmlNmsp;
		this.idxXmlNmsp = idxXmlNmsp;
	}

	internal void AddNamespace(XPathNode[] pageElem, int idxElem, XPathNode[] pageNmsp, int idxNmsp)
	{
		if (mapNmsp == null)
		{
			mapNmsp = new Dictionary<XPathNodeRef, XPathNodeRef>();
		}
		mapNmsp.Add(new XPathNodeRef(pageElem, idxElem), new XPathNodeRef(pageNmsp, idxNmsp));
	}

	internal int LookupNamespaces(XPathNode[] pageElem, int idxElem, out XPathNode[] pageNmsp)
	{
		XPathNodeRef key = new XPathNodeRef(pageElem, idxElem);
		if (mapNmsp == null || !mapNmsp.ContainsKey(key))
		{
			pageNmsp = null;
			return 0;
		}
		key = mapNmsp[key];
		pageNmsp = key.Page;
		return key.Index;
	}

	internal void AddIdElement(string id, XPathNode[] pageElem, int idxElem)
	{
		if (idValueMap == null)
		{
			idValueMap = new Dictionary<string, XPathNodeRef>();
		}
		if (!idValueMap.ContainsKey(id))
		{
			idValueMap.Add(id, new XPathNodeRef(pageElem, idxElem));
		}
	}

	internal int LookupIdElement(string id, out XPathNode[] pageElem)
	{
		if (idValueMap == null || !idValueMap.ContainsKey(id))
		{
			pageElem = null;
			return 0;
		}
		XPathNodeRef xPathNodeRef = idValueMap[id];
		pageElem = xPathNodeRef.Page;
		return xPathNodeRef.Index;
	}

	private XmlTextReaderImpl SetupReader(XmlTextReaderImpl reader)
	{
		reader.EntityHandling = EntityHandling.ExpandEntities;
		reader.XmlValidatingReaderCompatibilityMode = true;
		return reader;
	}
}
