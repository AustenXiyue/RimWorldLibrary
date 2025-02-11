using Unity;

namespace System.Xml;

/// <summary>Represents an entity declaration, such as &lt;!ENTITY... &gt;.</summary>
public class XmlEntity : XmlNode
{
	private string publicId;

	private string systemId;

	private string notationName;

	private string name;

	private string unparsedReplacementStr;

	private string baseURI;

	private XmlLinkedNode lastChild;

	private bool childrenFoliating;

	/// <summary>Gets a value indicating whether the node is read-only.</summary>
	/// <returns>true if the node is read-only; otherwise false.Because XmlEntity nodes are read-only, this property always returns true.</returns>
	public override bool IsReadOnly => true;

	/// <summary>Gets the name of the node.</summary>
	/// <returns>The name of the entity.</returns>
	public override string Name => name;

	/// <summary>Gets the name of the node without the namespace prefix.</summary>
	/// <returns>For XmlEntity nodes, this property returns the name of the entity.</returns>
	public override string LocalName => name;

	/// <summary>Gets the concatenated values of the entity node and all its children.</summary>
	/// <returns>The concatenated values of the node and all its children.</returns>
	/// <exception cref="T:System.InvalidOperationException">Attempting to set the property. </exception>
	public override string InnerText
	{
		get
		{
			return base.InnerText;
		}
		set
		{
			throw new InvalidOperationException(Res.GetString("The 'InnerText' of an 'Entity' node is read-only and cannot be set."));
		}
	}

	internal override bool IsContainer => true;

	internal override XmlLinkedNode LastNode
	{
		get
		{
			if (lastChild == null && !childrenFoliating)
			{
				childrenFoliating = true;
				new XmlLoader().ExpandEntity(this);
			}
			return lastChild;
		}
		set
		{
			lastChild = value;
		}
	}

	/// <summary>Gets the type of the node.</summary>
	/// <returns>The node type. For XmlEntity nodes, the value is XmlNodeType.Entity.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Entity;

	/// <summary>Gets the value of the public identifier on the entity declaration.</summary>
	/// <returns>The public identifier on the entity. If there is no public identifier, null is returned.</returns>
	public string PublicId => publicId;

	/// <summary>Gets the value of the system identifier on the entity declaration.</summary>
	/// <returns>The system identifier on the entity. If there is no system identifier, null is returned.</returns>
	public string SystemId => systemId;

	/// <summary>Gets the name of the optional NDATA attribute on the entity declaration.</summary>
	/// <returns>The name of the NDATA attribute. If there is no NDATA, null is returned.</returns>
	public string NotationName => notationName;

	/// <summary>Gets the markup representing this node and all its children.</summary>
	/// <returns>For XmlEntity nodes, String.Empty is returned.</returns>
	public override string OuterXml => string.Empty;

	/// <summary>Gets the markup representing the children of this node.</summary>
	/// <returns>For XmlEntity nodes, String.Empty is returned.</returns>
	/// <exception cref="T:System.InvalidOperationException">Attempting to set the property. </exception>
	public override string InnerXml
	{
		get
		{
			return string.Empty;
		}
		set
		{
			throw new InvalidOperationException(Res.GetString("Cannot set the 'InnerXml' for the current node because it is either read-only or cannot have children."));
		}
	}

	/// <summary>Gets the base Uniform Resource Identifier (URI) of the current node.</summary>
	/// <returns>The location from which the node was loaded.</returns>
	public override string BaseURI => baseURI;

	internal XmlEntity(string name, string strdata, string publicId, string systemId, string notationName, XmlDocument doc)
		: base(doc)
	{
		this.name = doc.NameTable.Add(name);
		this.publicId = publicId;
		this.systemId = systemId;
		this.notationName = notationName;
		unparsedReplacementStr = strdata;
		childrenFoliating = false;
	}

	/// <summary>Creates a duplicate of this node. Entity nodes cannot be cloned. Calling this method on an <see cref="T:System.Xml.XmlEntity" /> object throws an exception.</summary>
	/// <returns>Returns a copy of the <see cref="T:System.Xml.XmlNode" /> from which the method is called.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
	/// <exception cref="T:System.InvalidOperationException">Entity nodes cannot be cloned. Calling this method on an <see cref="T:System.Xml.XmlEntity" /> object throws an exception.</exception>
	public override XmlNode CloneNode(bool deep)
	{
		throw new InvalidOperationException(Res.GetString("'Entity' and 'Notation' nodes cannot be cloned."));
	}

	internal override bool IsValidChildType(XmlNodeType type)
	{
		if (type != XmlNodeType.Text && type != XmlNodeType.Element && type != XmlNodeType.ProcessingInstruction && type != XmlNodeType.Comment && type != XmlNodeType.CDATA && type != XmlNodeType.Whitespace && type != XmlNodeType.SignificantWhitespace)
		{
			return type == XmlNodeType.EntityReference;
		}
		return true;
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />. For XmlEntity nodes, this method has no effect.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />. For XmlEntity nodes, this method has no effect.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}

	internal void SetBaseURI(string inBaseURI)
	{
		baseURI = inBaseURI;
	}

	internal XmlEntity()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
