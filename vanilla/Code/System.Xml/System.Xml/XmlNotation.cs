using Unity;

namespace System.Xml;

/// <summary>Represents a notation declaration, such as &lt;!NOTATION... &gt;.</summary>
public class XmlNotation : XmlNode
{
	private string publicId;

	private string systemId;

	private string name;

	/// <summary>Gets the name of the current node.</summary>
	/// <returns>The name of the notation.</returns>
	public override string Name => name;

	/// <summary>Gets the name of the current node without the namespace prefix.</summary>
	/// <returns>For XmlNotation nodes, this property returns the name of the notation.</returns>
	public override string LocalName => name;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>The node type. For XmlNotation nodes, the value is XmlNodeType.Notation.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Notation;

	/// <summary>Gets a value indicating whether the node is read-only.</summary>
	/// <returns>true if the node is read-only; otherwise false.Because XmlNotation nodes are read-only, this property always returns true.</returns>
	public override bool IsReadOnly => true;

	/// <summary>Gets the value of the public identifier on the notation declaration.</summary>
	/// <returns>The public identifier on the notation. If there is no public identifier, null is returned.</returns>
	public string PublicId => publicId;

	/// <summary>Gets the value of the system identifier on the notation declaration.</summary>
	/// <returns>The system identifier on the notation. If there is no system identifier, null is returned.</returns>
	public string SystemId => systemId;

	/// <summary>Gets the markup representing this node and all its children.</summary>
	/// <returns>For XmlNotation nodes, String.Empty is returned.</returns>
	public override string OuterXml => string.Empty;

	/// <summary>Gets the markup representing the children of this node.</summary>
	/// <returns>For XmlNotation nodes, String.Empty is returned.</returns>
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

	internal XmlNotation(string name, string publicId, string systemId, XmlDocument doc)
		: base(doc)
	{
		this.name = doc.NameTable.Add(name);
		this.publicId = publicId;
		this.systemId = systemId;
	}

	/// <summary>Creates a duplicate of this node. Notation nodes cannot be cloned. Calling this method on an <see cref="T:System.Xml.XmlNotation" /> object throws an exception.</summary>
	/// <returns>Returns a <see cref="T:System.Xml.XmlNode" /> copy of the node from which the method is called.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
	/// <exception cref="T:System.InvalidOperationException">Notation nodes cannot be cloned. Calling this method on an <see cref="T:System.Xml.XmlNotation" /> object throws an exception.</exception>
	public override XmlNode CloneNode(bool deep)
	{
		throw new InvalidOperationException(Res.GetString("'Entity' and 'Notation' nodes cannot be cloned."));
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />. This method has no effect on XmlNotation nodes.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
	}

	/// <summary>Saves the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />. This method has no effect on XmlNotation nodes.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}

	internal XmlNotation()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
