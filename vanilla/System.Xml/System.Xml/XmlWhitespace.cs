using System.Xml.XPath;

namespace System.Xml;

/// <summary>Represents white space in element content.</summary>
public class XmlWhitespace : XmlCharacterData
{
	/// <summary>Gets the qualified name of the node.</summary>
	/// <returns>For XmlWhitespace nodes, this property returns #whitespace.</returns>
	public override string Name => OwnerDocument.strNonSignificantWhitespaceName;

	/// <summary>Gets the local name of the node.</summary>
	/// <returns>For XmlWhitespace nodes, this property returns #whitespace.</returns>
	public override string LocalName => OwnerDocument.strNonSignificantWhitespaceName;

	/// <summary>Gets the type of the node.</summary>
	/// <returns>For XmlWhitespace nodes, the value is <see cref="F:System.Xml.XmlNodeType.Whitespace" />.</returns>
	public override XmlNodeType NodeType => XmlNodeType.Whitespace;

	/// <summary>Gets the parent of the current node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> parent node of the current node.</returns>
	public override XmlNode ParentNode
	{
		get
		{
			switch (parentNode.NodeType)
			{
			case XmlNodeType.Document:
				return base.ParentNode;
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

	/// <summary>Gets or sets the value of the node.</summary>
	/// <returns>The white space characters found in the node.</returns>
	/// <exception cref="T:System.ArgumentException">Setting <see cref="P:System.Xml.XmlWhitespace.Value" /> to invalid white space characters. </exception>
	public override string Value
	{
		get
		{
			return Data;
		}
		set
		{
			if (CheckOnData(value))
			{
				Data = value;
				return;
			}
			throw new ArgumentException(Res.GetString("The string for white space contains an invalid character."));
		}
	}

	internal override XPathNodeType XPNodeType
	{
		get
		{
			XPathNodeType xnt = XPathNodeType.Whitespace;
			DecideXPNodeTypeForTextNodes(this, ref xnt);
			return xnt;
		}
	}

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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlWhitespace" /> class.</summary>
	/// <param name="strData">The white space characters of the node.</param>
	/// <param name="doc">The <see cref="T:System.Xml.XmlDocument" /> object.</param>
	protected internal XmlWhitespace(string strData, XmlDocument doc)
		: base(strData, doc)
	{
		if (!doc.IsLoading && !CheckOnData(strData))
		{
			throw new ArgumentException(Res.GetString("The string for white space contains an invalid character."));
		}
	}

	/// <summary>Creates a duplicate of this node.</summary>
	/// <returns>The cloned node.</returns>
	/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself. For white space nodes, the cloned node always includes the data value, regardless of the parameter setting. </param>
	public override XmlNode CloneNode(bool deep)
	{
		return OwnerDocument.CreateWhitespace(Data);
	}

	/// <summary>Saves the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The <see cref="T:System.Xml.XmlWriter" /> to which you want to save.</param>
	public override void WriteTo(XmlWriter w)
	{
		w.WriteWhitespace(Data);
	}

	/// <summary>Saves all the children of the node to the specified <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="w">The <see cref="T:System.Xml.XmlWriter" /> to which you want to save. </param>
	public override void WriteContentTo(XmlWriter w)
	{
	}
}
