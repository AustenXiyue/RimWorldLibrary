namespace System.Xml.Serialization;

/// <summary>Instructs the <see cref="M:System.Xml.Serialization.XmlSerializer.Serialize(System.IO.TextWriter,System.Object)" /> method of the <see cref="T:System.Xml.Serialization.XmlSerializer" /> not to serialize the public field or public read/write property value.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class XmlIgnoreAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlIgnoreAttribute" /> class.</summary>
	public XmlIgnoreAttribute()
	{
	}
}
