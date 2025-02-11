using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>Represents the sequence element (compositor) from the XML Schema as specified by the World Wide Web Consortium (W3C). The sequence requires the elements in the group to appear in the specified sequence within the containing element.</summary>
public class XmlSchemaSequence : XmlSchemaGroupBase
{
	private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();

	/// <summary>The elements contained within the compositor. Collection of <see cref="T:System.Xml.Schema.XmlSchemaElement" />, <see cref="T:System.Xml.Schema.XmlSchemaGroupRef" />, <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, <see cref="T:System.Xml.Schema.XmlSchemaSequence" />, or <see cref="T:System.Xml.Schema.XmlSchemaAny" />.</summary>
	/// <returns>The elements contained within the compositor.</returns>
	[XmlElement("element", typeof(XmlSchemaElement))]
	[XmlElement("group", typeof(XmlSchemaGroupRef))]
	[XmlElement("choice", typeof(XmlSchemaChoice))]
	[XmlElement("any", typeof(XmlSchemaAny))]
	[XmlElement("sequence", typeof(XmlSchemaSequence))]
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

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaSequence" /> class.</summary>
	public XmlSchemaSequence()
	{
	}
}
