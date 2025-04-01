using System.Xml.Schema;

namespace System.Xml;

/// <summary>Represents the document type declaration.</summary>
public class XmlDocumentType : XmlLinkedNode
{
	private string name;

	private string publicId;

	private string systemId;

	private string internalSubset;

	private bool namespaces;

	private XmlNamedNodeMap entities;

	private XmlNamedNodeMap notations;

	private SchemaInfo schemaInfo;

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For DocumentType nodes, this property returns the name of the document type.</returns>
	public override string Name => name;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For DocumentType nodes, this property returns the name of the document type.</returns>
	public override string LocalName => name;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>For DocumentType nodes, this value is XmlNodeType.DocumentType.</returns>
	public override XmlNodeType NodeType => XmlNodeType.DocumentType;

	/// <summary>Gets a value indicating whether the node is read-only.</summary>
	/// <returns>true if the node is read-only; otherwise false.Because DocumentType nodes are read-only, this property always returns true.</returns>
	public override bool IsReadOnly => true;

	/// <summary>Gets the collection of <see cref="T:System.Xml.XmlEntity" /> nodes declared in the document type declaration.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNamedNodeMap" /> containing the XmlEntity nodes. The returned XmlNamedNodeMap is read-only.</returns>
	public XmlNamedNodeMap Entities
	{
		get
		{
			if (entities == null)
			{
				entities = new XmlNamedNodeMap(this);
			}
			return entities;
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Xml.XmlNotation" /> nodes present in the document type declaration.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlNamedNodeMap" /> containing the XmlNotation nodes. The returned XmlNamedNodeMap is read-only.</returns>
	public XmlNamedNodeMap Notations
	{
		get
		{
			if (notations == null)
			{
				notations = new XmlNamedNodeMap(this);
			}
			return notations;
		}
	}

	/// <summary>Gets the value of the public identifier on the DOCTYPE declaration.</summary>
	/// <returns>The public identifier on the DOCTYPE. If there is no public identifier, null is returned.</returns>
	public string PublicId => publicId;

	/// <summary>Gets the value of the system identifier on the DOCTYPE declaration.</summary>
	/// <returns>The system identifier on the DOCTYPE. If there is no system identifier, null is returned.</returns>
	public string SystemId => systemId;

	/// <summary>Gets the value of the document type definition (DTD) internal subset on the DOCTYPE declaration.</summary>
	/// <returns>The DTD internal subset on the DOCTYPE. If there is no DTD internal subset, String.Empty is returned.</returns>
	public string InternalSubset => internalSubset;

	internal bool ParseWithNamespaces
	{
		get
		{
			return namespaces;
		}
		set
		{
			namespaces = value;
		}
	}

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlDocumentType" /> class.</summary>
	/// <param name="name">The qualified name; see the <see cref="P:System.Xml.XmlDocumentType.Name" /> property.</param>
	/// <param name="publicId">The public identifier; see the <see cref="P:System.Xml.XmlDocumentType.PublicId" /> property.</param>
	/// <param name="systemId">The system identifier; see the <see cref="P:System.Xml.XmlDocumentType.SystemId" /> property.</param>
	/// <param name="internalSubset">The DTD internal subset; see the <see cref="P:System.Xml.XmlDocumentType.InternalSubset" /> property.</param>
	/// <param name="doc">The parent document.</param>
	protected internal XmlDocumentType(string name, string publicId, string systemId, string internalSubset, XmlDocument doc)
		: base(doc)
	{
		this.name = name;
		this.publicId = publicId;
		this.systemId = systemId;
		namespaces = true;
		this.internalSubset = internalSubset;
		if (!doc.IsLoading)
		{
			doc.IsLoading = true;
			new XmlLoader().ParseDocumentType(this);
			doc.IsLoading = false;
		}
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. For document type nodes, the cloned node always includes the subtree, regardless of the parameter setting. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateDocumentType(name, publicId, systemId, internalSubset);
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteDocType(name, publicId, systemId, internalSubset);
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />. For XmlDocumentType nodes, this method has no effect.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}
}
