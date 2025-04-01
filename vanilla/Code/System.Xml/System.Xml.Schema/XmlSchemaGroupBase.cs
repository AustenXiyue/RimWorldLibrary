using System.Xml.Serialization;

namespace System.Xml.Schema;

/// <summary>An abstract class for <see cref="T:System.Xml.Schema.XmlSchemaAll" />, <see cref="T:System.Xml.Schema.XmlSchemaChoice" />, or <see cref="T:System.Xml.Schema.XmlSchemaSequence" />.</summary>
public abstract class XmlSchemaGroupBase : XmlSchemaParticle
{
	/// <summary>This collection is used to add new elements to the compositor.</summary>
	/// <returns>An XmlSchemaObjectCollection.</returns>
	[XmlIgnore]
	public abstract XmlSchemaObjectCollection Items { get; }

	internal abstract void SetItems(XmlSchemaObjectCollection newItems);

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Schema.XmlSchemaGroupBase" /> class.</summary>
	protected XmlSchemaGroupBase()
	{
	}
}
