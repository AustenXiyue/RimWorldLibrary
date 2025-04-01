using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the World Wide Web Consortium (W3C) all element (compositor).</summary>
public class XmlSchemaAll : XmlSchemaGroupBase
{
	private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

	/// <summary>Gets the collection of XmlSchemaElement elements contained within the all compositor.</summary>
	/// <returns>The collection of elements contained in XmlSchemaAll.</returns>
	[XmlElement("element", typeof(XmlSchemaElement))]
	public override XmlSchemaObjectCollection Items => items;

	internal override bool IsEmpty
	{
		get
		{
			if (!base.IsEmpty)
			{
				return items.Count == 0;
			}
			return true;
		}
	}

	internal override void SetItems(XmlSchemaObjectCollection newItems)
	{
		items = newItems;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaAll" /> class.</summary>
	public XmlSchemaAll()
	{
	}
}
