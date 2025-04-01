using System.Collections;

namespace System.Xml;

/// <summary>Represents an entity reference node.</summary>
public class XmlEntityReference : XmlLinkedNode
{
	private string name;

	private XmlLinkedNode lastChild;

	/// <summary>Gets the name of the node.</summary>
	/// <returns>The name of the entity referenced.</returns>
	public override string Name => name;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For XmlEntityReference nodes, this property returns the name of the entity referenced.</returns>
	public override string LocalName => name;

	/// <summary>Gets or sets the value of the node.</summary>
	/// <returns>The value of the node. For XmlEntityReference nodes, this property returns null.</returns>
	/// <exception cref="T:System.ArgumentException">Node is read-only. </exception>
	/// <exception cref="T:System.InvalidOperationException">Setting the property. </exception>
	public override string Value
	{
		get
		{
			return null;
		}
		set
		{
			throw new InvalidOperationException(Res.GetString("'EntityReference' nodes have no support for setting value."));
		}
	}

	/// <summary>Gets the type of the node.</summary>
	/// <returns>The node type. For XmlEntityReference nodes, the value is XmlNodeType.EntityReference.</returns>
	public override XmlNodeType NodeType => XmlNodeType.EntityReference;

	/// <summary>Gets a value indicating whether the node is read-only.</summary>
	/// <returns>true if the node is read-only; otherwise false.Because XmlEntityReference nodes are read-only, this property always returns true.</returns>
	public override bool IsReadOnly => true;

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

	/// <summary>Gets the base Uniform Resource Identifier (URI) of the current node.</summary>
	/// <returns>The location from which the node was loaded.</returns>
	public override string BaseURI => OwnerDocument.BaseURI;

	internal string ChildBaseURI
	{
		get
		{
			XmlEntity entityNode = OwnerDocument.GetEntityNode(name);
			if (entityNode != null)
			{
				if (entityNode.SystemId != null && entityNode.SystemId.Length > 0)
				{
					return ConstructBaseURI(entityNode.BaseURI, entityNode.SystemId);
				}
				return entityNode.BaseURI;
			}
			return string.Empty;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlEntityReference" /> class.</summary>
	/// <param name="name">The name of the entity reference; see the <see cref="P:System.Xml.XmlEntityReference.Name" /> property.</param>
	/// <param name="doc">The parent XML document.</param>
	protected internal XmlEntityReference(string name, XmlDocument doc)
		: base(doc)
	{
		if (!doc.IsLoading && name.Length > 0 && name[0] == '#')
		{
			throw new ArgumentException(Res.GetString("Cannot create an 'EntityReference' node with a name starting with '#'."));
		}
		this.name = doc.NameTable.Add(name);
		doc.fEntRefNodesPresent = true;
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. For XmlEntityReference nodes, this method always returns an entity reference node with no children. The replacement text is set when the node is inserted into a parent. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateEntityReference(name);
	}

	internal override void SetParent(XmlNode node)
	{
		base.SetParent(node);
		if (LastNode == null && node != null && node != OwnerDocument)
		{
			new XmlLoader().ExpandEntityReference(this);
		}
	}

	internal override void SetParentForLoad(XmlNode node)
	{
		SetParent(node);
	}

	internal override bool IsValidChildType(XmlNodeType type)
	{
		switch (type)
		{
		case XmlNodeType.Element:
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
		case XmlNodeType.EntityReference:
		case XmlNodeType.ProcessingInstruction:
		case XmlNodeType.Comment:
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			return true;
		default:
			return false;
		}
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteEntityRef(name);
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((XmlNode)enumerator.Current).WriteTo(w);
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

	private string ConstructBaseURI(string baseURI, string systemId)
	{
		if (baseURI == null)
		{
			return systemId;
		}
		int num = baseURI.LastIndexOf('/') + 1;
		string text = baseURI;
		if (num > 0 && num < baseURI.Length)
		{
			text = baseURI.Substring(0, num);
		}
		else if (num == 0)
		{
			text += "\\";
		}
		return text + systemId.Replace('\\', '/');
	}
}
