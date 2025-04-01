namespace System.Xml;

/// <summary>Gets the node immediately preceding or following this node.</summary>
public abstract class XmlLinkedNode : XmlNode
{
	internal XmlLinkedNode next;

	/// <summary>Gets the node immediately preceding this node.</summary>
	/// <returns>The preceding <see cref="T:System.Xml.XmlNode" /> or null if one does not exist.</returns>
	public override XmlNode PreviousSibling
	{
		get
		{
			XmlNode xmlNode = ParentNode;
			if (xmlNode != null)
			{
				XmlNode xmlNode2 = xmlNode.FirstChild;
				while (xmlNode2 != null)
				{
					XmlNode nextSibling = xmlNode2.NextSibling;
					if (nextSibling == this)
					{
						break;
					}
					xmlNode2 = nextSibling;
				}
				return xmlNode2;
			}
			return null;
		}
	}

	/// <summary>Gets the node immediately following this node.</summary>
	/// <returns>The <see cref="T:System.Xml.XmlNode" /> immediately following this node or null if one does not exist.</returns>
	public override XmlNode NextSibling
	{
		get
		{
			XmlNode xmlNode = ParentNode;
			if (xmlNode != null && next != xmlNode.FirstChild)
			{
				return next;
			}
			return null;
		}
	}

	internal XmlLinkedNode()
	{
		next = null;
	}

	internal XmlLinkedNode(XmlDocument doc)
		: base(doc)
	{
		next = null;
	}
}
