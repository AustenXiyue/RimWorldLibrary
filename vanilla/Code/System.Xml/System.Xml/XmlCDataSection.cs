using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents a CDATA section.</summary>
public class XmlCDataSection : XmlCharacterData
{
	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For CDATA nodes, the name is #cdata-section.</returns>
	public override string Name => OwnerDocument.strCDataSectionName;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For CDATA nodes, the local name is #cdata-section.</returns>
	public override string LocalName => OwnerDocument.strCDataSectionName;

	/// <summary>Gets the type of the current node.</summary>
	/// <returns>The node type. For CDATA nodes, the value is XmlNodeType.CDATA.</returns>
	public override XmlNodeType NodeType => XmlNodeType.CDATA;

	public override XmlNode ParentNode
	{
		get
		{
			switch (parentNode.NodeType)
			{
			case XmlNodeType.Document:
				return null;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
			{
				XmlNode xmlNode = parentNode.parentNode;
				while (xmlNode.IsText)
				{
					xmlNode = xmlNode.parentNode;
				}
				return xmlNode;
			}
			default:
				return parentNode;
			}
		}
	}

	internal override XPathNodeType XPNodeType => XPathNodeType.Text;

	internal override bool IsText => true;

	public override XmlNode PreviousText
	{
		get
		{
			if (parentNode.IsText)
			{
				return parentNode;
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlCDataSection" /> class.</summary>
	/// <param name="data">
	///   <see cref="T:System.String" /> that contains character data.</param>
	/// <param name="doc">
	///   <see cref="T:System.Xml.XmlDocument" /> object.</param>
	protected internal XmlCDataSection(string data, XmlDocument doc)
		: base(data, doc)
	{
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. Because CDATA nodes do not have children, regardless of the parameter setting, the cloned node will always include the data content. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateCDataSection(Data);
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteCData(Data);
	}

	/// <summary>Saves the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The XmlWriter to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}
}
