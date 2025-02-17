using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents the content of an XML comment.</summary>
public class XmlComment : XmlCharacterData
{
	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For comment nodes, the value is #comment.</returns>
	public override string Name => OwnerDocument.strCommentName;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For comment nodes, the value is #comment.</returns>
	public override string LocalName => OwnerDocument.strCommentName;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>For comment nodes, the value is XmlNodeType.Comment.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Comment;

	internal override XPathNodeType XPNodeType => XPathNodeType.Comment;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlComment" /> class.</summary>
	/// <param name="comment">The content of the comment element.</param>
	/// <param name="doc">The parent XML document.</param>
	protected internal XmlComment(string comment, XmlDocument doc)
		: base(comment, doc)
	{
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. Because comment nodes do not have children, the cloned node always includes the text content, regardless of the parameter setting. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateComment(Data);
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteComment(Data);
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />. Because comment nodes do not have children, this method has no effect.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}
}
