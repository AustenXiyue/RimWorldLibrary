namespace System.Xml.Linq;

/// <summary>Represents an XML Document Type Definition (DTD). </summary>
/// <filterpriority>2</filterpriority>
public class XDocumentType : XNode
{
	private string name;

	private string publicId;

	private string systemId;

	private string internalSubset;

	private IDtdInfo dtdInfo;

	/// <summary>Gets or sets the internal subset for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the internal subset for this Document Type Definition (DTD).</returns>
	public string InternalSubset
	{
		get
		{
			return internalSubset;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			internalSubset = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Gets or sets the name for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name for this Document Type Definition (DTD).</returns>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			value = XmlConvert.VerifyName(value);
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Name);
			name = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Name);
			}
		}
	}

	/// <summary>Gets the node type for this node.</summary>
	/// <returns>The node type. For <see cref="T:System.Xml.Linq.XDocumentType" /> objects, this value is <see cref="F:System.Xml.XmlNodeType.DocumentType" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.DocumentType;

	/// <summary>Gets or sets the public identifier for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the public identifier for this Document Type Definition (DTD).</returns>
	public string PublicId
	{
		get
		{
			return publicId;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			publicId = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	/// <summary>Gets or sets the system identifier for this Document Type Definition (DTD).</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the system identifier for this Document Type Definition (DTD).</returns>
	public string SystemId
	{
		get
		{
			return systemId;
		}
		set
		{
			bool num = NotifyChanging(this, XObjectChangeEventArgs.Value);
			systemId = value;
			if (num)
			{
				NotifyChanged(this, XObjectChangeEventArgs.Value);
			}
		}
	}

	internal IDtdInfo DtdInfo => dtdInfo;

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Linq.XDocumentType" /> class. </summary>
	/// <param name="name">A <see cref="T:System.String" /> that contains the qualified name of the DTD, which is the same as the qualified name of the root element of the XML document.</param>
	/// <param name="publicId">A <see cref="T:System.String" /> that contains the public identifier of an external public DTD.</param>
	/// <param name="systemId">A <see cref="T:System.String" /> that contains the system identifier of an external private DTD.</param>
	/// <param name="internalSubset">A <see cref="T:System.String" /> that contains the internal subset for an internal DTD.</param>
	public XDocumentType(string name, string publicId, string systemId, string internalSubset)
	{
		this.name = XmlConvert.VerifyName(name);
		this.publicId = publicId;
		this.systemId = systemId;
		this.internalSubset = internalSubset;
	}

	/// <summary>Initializes an instance of the <see cref="T:System.Xml.Linq.XDocumentType" /> class from another <see cref="T:System.Xml.Linq.XDocumentType" /> object.</summary>
	/// <param name="other">An <see cref="T:System.Xml.Linq.XDocumentType" /> object to copy from.</param>
	public XDocumentType(XDocumentType other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		name = other.name;
		publicId = other.publicId;
		systemId = other.systemId;
		internalSubset = other.internalSubset;
		dtdInfo = other.dtdInfo;
	}

	internal XDocumentType(XmlReader r)
	{
		name = r.Name;
		publicId = r.GetAttribute("PUBLIC");
		systemId = r.GetAttribute("SYSTEM");
		internalSubset = r.Value;
		dtdInfo = r.DtdInfo;
		r.Read();
	}

	internal XDocumentType(string name, string publicId, string systemId, string internalSubset, IDtdInfo dtdInfo)
		: this(name, publicId, systemId, internalSubset)
	{
		this.dtdInfo = dtdInfo;
	}

	/// <summary>Write this <see cref="T:System.Xml.Linq.XDocumentType" /> to an <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">An <see cref="T:System.Xml.XmlWriter" /> into which this method will write.</param>
	/// <filterpriority>2</filterpriority>
	public override void WriteTo(XmlWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.WriteDocType(name, publicId, systemId, internalSubset);
	}

	internal override XNode CloneNode()
	{
		return new XDocumentType(this);
	}

	internal override bool DeepEquals(XNode node)
	{
		if (node is XDocumentType xDocumentType && name == xDocumentType.name && publicId == xDocumentType.publicId && systemId == xDocumentType.SystemId)
		{
			return internalSubset == xDocumentType.internalSubset;
		}
		return false;
	}

	internal override int GetDeepHashCode()
	{
		return name.GetHashCode() ^ ((publicId != null) ? publicId.GetHashCode() : 0) ^ ((systemId != null) ? systemId.GetHashCode() : 0) ^ ((internalSubset != null) ? internalSubset.GetHashCode() : 0);
	}
}
