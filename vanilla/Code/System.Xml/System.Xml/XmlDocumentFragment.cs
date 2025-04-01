using System.Collections;
using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents a lightweight object that is useful for tree insert operations.</summary>
public class XmlDocumentFragment : XmlNode
{
	private XmlLinkedNode lastChild;

	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For XmlDocumentFragment, the name is #document-fragment.</returns>
	public override string Name => OwnerDocument.strDocumentFragmentName;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For XmlDocumentFragment nodes, the local name is #document-fragment.</returns>
	public override string LocalName => OwnerDocument.strDocumentFragmentName;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>For XmlDocumentFragment nodes, this value is XmlNodeType.DocumentFragment.</returns>
	public override XmlNodeType NodeType => XmlNodeType.DocumentFragment;

	/// <summary>Gets the parent of this node (for nodes that can have parents).</summary>
	/// <returns>The parent of this node.For XmlDocumentFragment nodes, this property is always null.</returns>
	public override XmlNode ParentNode => null;

	/// <summary>Gets the <see cref="T:System.Xml.XmlDocument" /> to which this node belongs.</summary>
	/// <returns>The XmlDocument to which this node belongs.</returns>
	public override XmlDocument OwnerDocument => (XmlDocument)parentNode;

	/// <summary>Gets or sets the markup representing the children of this node.</summary>
	/// <returns>The markup of the children of this node.</returns>
	/// <exception cref="T:System.Xml.XmlException">The XML specified when setting this property is not well-formed. </exception>
	public override string InnerXml
	{
		get
		{
			return base.InnerXml;
		}
		set
		{
			RemoveAll();
			new XmlLoader().ParsePartialContent(this, value, XmlNodeType.Element);
		}
	}

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

	internal override XPathNodeType XPNodeType => XPathNodeType.Root;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlDocumentFragment" /> class.</summary>
	/// <param name="ownerDocument">The XML document that is the source of the fragment.</param>
	protected internal XmlDocumentFragment(XmlDocument ownerDocument)
	{
		if (ownerDocument == null)
		{
			throw new ArgumentException(Res.GetString("Cannot create a node without an owner document."));
		}
		parentNode = ownerDocument;
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. </param>
	public override XmlNode CloneNode(bool deep)
	{
		XmlDocument ownerDocument = OwnerDocument;
		XmlDocumentFragment xmlDocumentFragment = ownerDocument.CreateDocumentFragment();
		if (deep)
		{
			xmlDocumentFragment.CopyChildren(ownerDocument, this, deep);
		}
		return xmlDocumentFragment;
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
		case XmlNodeType.XmlDeclaration:
		{
			XmlNode firstChild = FirstChild;
			if (firstChild == null || firstChild.NodeType != XmlNodeType.XmlDeclaration)
			{
				return true;
			}
			return false;
		}
		default:
			return false;
		}
	}

	internal override bool CanInsertAfter(XmlNode newChild, XmlNode refChild)
	{
		if (newChild.NodeType == XmlNodeType.XmlDeclaration)
		{
			if (refChild == null)
			{
				return LastNode == null;
			}
			return false;
		}
		return true;
	}

	internal override bool CanInsertBefore(XmlNode newChild, XmlNode refChild)
	{
		if (newChild.NodeType == XmlNodeType.XmlDeclaration)
		{
			if (refChild != null)
			{
				return refChild == FirstChild;
			}
			return true;
		}
		return true;
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		WriteContentTo(w);
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
}
