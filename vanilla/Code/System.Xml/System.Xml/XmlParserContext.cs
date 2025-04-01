using System.Text;

namespace System.Xml;

/// <summary>Provides all the context information required by the <see cref="T:System.Xml.XmlReader" /> to parse an XML fragment.</summary>
public class XmlParserContext
{
	private XmlNameTable _nt;

	private XmlNamespaceManager _nsMgr;

	private string _docTypeName = string.Empty;

	private string _pubId = string.Empty;

	private string _sysId = string.Empty;

	private string _internalSubset = string.Empty;

	private string _xmlLang = string.Empty;

	private XmlSpace _xmlSpace;

	private string _baseURI = string.Empty;

	private Encoding _encoding;

	/// <summary>Gets the <see cref="T:System.Xml.XmlNameTable" /> used to atomize strings. For more information on atomized strings, see <see cref="T:System.Xml.XmlNameTable" />.</summary>
	/// <returns>The XmlNameTable.</returns>
	public XmlNameTable NameTable
	{
		get
		{
			return _nt;
		}
		set
		{
			_nt = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Xml.XmlNamespaceManager" />.</summary>
	/// <returns>The XmlNamespaceManager.</returns>
	public XmlNamespaceManager NamespaceManager
	{
		get
		{
			return _nsMgr;
		}
		set
		{
			_nsMgr = value;
		}
	}

	/// <summary>Gets or sets the name of the document type declaration.</summary>
	/// <returns>The name of the document type declaration.</returns>
	public string DocTypeName
	{
		get
		{
			return _docTypeName;
		}
		set
		{
			_docTypeName = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the public identifier.</summary>
	/// <returns>The public identifier.</returns>
	public string PublicId
	{
		get
		{
			return _pubId;
		}
		set
		{
			_pubId = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the system identifier.</summary>
	/// <returns>The system identifier.</returns>
	public string SystemId
	{
		get
		{
			return _sysId;
		}
		set
		{
			_sysId = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the base URI.</summary>
	/// <returns>The base URI to use to resolve the DTD file.</returns>
	public string BaseURI
	{
		get
		{
			return _baseURI;
		}
		set
		{
			_baseURI = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the internal DTD subset.</summary>
	/// <returns>The internal DTD subset. For example, this property returns everything between the square brackets &lt;!DOCTYPE doc [...]&gt;.</returns>
	public string InternalSubset
	{
		get
		{
			return _internalSubset;
		}
		set
		{
			_internalSubset = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the current xml:lang scope.</summary>
	/// <returns>The current xml:lang scope. If there is no xml:lang in scope, String.Empty is returned.</returns>
	public string XmlLang
	{
		get
		{
			return _xmlLang;
		}
		set
		{
			_xmlLang = ((value == null) ? string.Empty : value);
		}
	}

	/// <summary>Gets or sets the current xml:space scope.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlSpace" /> value indicating the xml:space scope.</returns>
	public XmlSpace XmlSpace
	{
		get
		{
			return _xmlSpace;
		}
		set
		{
			_xmlSpace = value;
		}
	}

	/// <summary>Gets or sets the encoding type.</summary>
	/// <returns>An <see cref="T:System.Text.Encoding" /> object indicating the encoding type.</returns>
	public Encoding Encoding
	{
		get
		{
			return _encoding;
		}
		set
		{
			_encoding = value;
		}
	}

	internal bool HasDtdInfo
	{
		get
		{
			if (!(_internalSubset != string.Empty) && !(_pubId != string.Empty))
			{
				return _sysId != string.Empty;
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the XmlParserContext class with the specified <see cref="T:System.Xml.XmlNameTable" />, <see cref="T:System.Xml.XmlNamespaceManager" />, xml:lang, and xml:space values.</summary>
	/// <param name="nt">The <see cref="T:System.Xml.XmlNameTable" /> to use to atomize strings. If this is null, the name table used to construct the <paramref name="nsMgr" /> is used instead. For more information about atomized strings, see <see cref="T:System.Xml.XmlNameTable" />. </param>
	/// <param name="nsMgr">The <see cref="T:System.Xml.XmlNamespaceManager" /> to use for looking up namespace information, or null. </param>
	/// <param name="xmlLang">The xml:lang scope. </param>
	/// <param name="xmlSpace">An <see cref="T:System.Xml.XmlSpace" /> value indicating the xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="nt" /> is not the same XmlNameTable used to construct <paramref name="nsMgr" />. </exception>
	public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string xmlLang, XmlSpace xmlSpace)
		: this(nt, nsMgr, null, null, null, null, string.Empty, xmlLang, xmlSpace)
	{
	}

	/// <summary>Initializes a new instance of the XmlParserContext class with the specified <see cref="T:System.Xml.XmlNameTable" />, <see cref="T:System.Xml.XmlNamespaceManager" />, xml:lang, xml:space, and encoding.</summary>
	/// <param name="nt">The <see cref="T:System.Xml.XmlNameTable" /> to use to atomize strings. If this is null, the name table used to construct the <paramref name="nsMgr" /> is used instead. For more information on atomized strings, see <see cref="T:System.Xml.XmlNameTable" />. </param>
	/// <param name="nsMgr">The <see cref="T:System.Xml.XmlNamespaceManager" /> to use for looking up namespace information, or null. </param>
	/// <param name="xmlLang">The xml:lang scope. </param>
	/// <param name="xmlSpace">An <see cref="T:System.Xml.XmlSpace" /> value indicating the xml:space scope. </param>
	/// <param name="enc">An <see cref="T:System.Text.Encoding" /> object indicating the encoding setting. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="nt" /> is not the same XmlNameTable used to construct <paramref name="nsMgr" />. </exception>
	public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string xmlLang, XmlSpace xmlSpace, Encoding enc)
		: this(nt, nsMgr, null, null, null, null, string.Empty, xmlLang, xmlSpace, enc)
	{
	}

	/// <summary>Initializes a new instance of the XmlParserContext class with the specified <see cref="T:System.Xml.XmlNameTable" />, <see cref="T:System.Xml.XmlNamespaceManager" />, base URI, xml:lang, xml:space, and document type values.</summary>
	/// <param name="nt">The <see cref="T:System.Xml.XmlNameTable" /> to use to atomize strings. If this is null, the name table used to construct the <paramref name="nsMgr" /> is used instead. For more information about atomized strings, see <see cref="T:System.Xml.XmlNameTable" />. </param>
	/// <param name="nsMgr">The <see cref="T:System.Xml.XmlNamespaceManager" /> to use for looking up namespace information, or null. </param>
	/// <param name="docTypeName">The name of the document type declaration. </param>
	/// <param name="pubId">The public identifier. </param>
	/// <param name="sysId">The system identifier. </param>
	/// <param name="internalSubset">The internal DTD subset. The DTD subset is used for entity resolution, not for document validation.</param>
	/// <param name="baseURI">The base URI for the XML fragment (the location from which the fragment was loaded). </param>
	/// <param name="xmlLang">The xml:lang scope. </param>
	/// <param name="xmlSpace">An <see cref="T:System.Xml.XmlSpace" /> value indicating the xml:space scope. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="nt" /> is not the same XmlNameTable used to construct <paramref name="nsMgr" />. </exception>
	public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string docTypeName, string pubId, string sysId, string internalSubset, string baseURI, string xmlLang, XmlSpace xmlSpace)
		: this(nt, nsMgr, docTypeName, pubId, sysId, internalSubset, baseURI, xmlLang, xmlSpace, null)
	{
	}

	/// <summary>Initializes a new instance of the XmlParserContext class with the specified <see cref="T:System.Xml.XmlNameTable" />, <see cref="T:System.Xml.XmlNamespaceManager" />, base URI, xml:lang, xml:space, encoding, and document type values.</summary>
	/// <param name="nt">The <see cref="T:System.Xml.XmlNameTable" /> to use to atomize strings. If this is null, the name table used to construct the <paramref name="nsMgr" /> is used instead. For more information about atomized strings, see <see cref="T:System.Xml.XmlNameTable" />. </param>
	/// <param name="nsMgr">The <see cref="T:System.Xml.XmlNamespaceManager" /> to use for looking up namespace information, or null. </param>
	/// <param name="docTypeName">The name of the document type declaration. </param>
	/// <param name="pubId">The public identifier. </param>
	/// <param name="sysId">The system identifier. </param>
	/// <param name="internalSubset">The internal DTD subset. The DTD is used for entity resolution, not for document validation.</param>
	/// <param name="baseURI">The base URI for the XML fragment (the location from which the fragment was loaded). </param>
	/// <param name="xmlLang">The xml:lang scope. </param>
	/// <param name="xmlSpace">An <see cref="T:System.Xml.XmlSpace" /> value indicating the xml:space scope. </param>
	/// <param name="enc">An <see cref="T:System.Text.Encoding" /> object indicating the encoding setting. </param>
	/// <exception cref="T:System.Xml.XmlException">
	///   <paramref name="nt" /> is not the same XmlNameTable used to construct <paramref name="nsMgr" />. </exception>
	public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string docTypeName, string pubId, string sysId, string internalSubset, string baseURI, string xmlLang, XmlSpace xmlSpace, Encoding enc)
	{
		if (nsMgr != null)
		{
			if (nt == null)
			{
				_nt = nsMgr.NameTable;
			}
			else
			{
				if (nt != nsMgr.NameTable)
				{
					throw new XmlException("Not the same name table.", string.Empty);
				}
				_nt = nt;
			}
		}
		else
		{
			_nt = nt;
		}
		_nsMgr = nsMgr;
		_docTypeName = ((docTypeName == null) ? string.Empty : docTypeName);
		_pubId = ((pubId == null) ? string.Empty : pubId);
		_sysId = ((sysId == null) ? string.Empty : sysId);
		_internalSubset = ((internalSubset == null) ? string.Empty : internalSubset);
		_baseURI = ((baseURI == null) ? string.Empty : baseURI);
		_xmlLang = ((xmlLang == null) ? string.Empty : xmlLang);
		_xmlSpace = xmlSpace;
		_encoding = enc;
	}
}
