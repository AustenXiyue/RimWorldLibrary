namespace System.Xml.Serialization;

/// <summary>Specifies that the member (a field that returns an array of <see cref="T:System.Xml.XmlAttribute" /> objects) can contain any XML attributes.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
public class XmlAnyAttributeAttribute : Attribute
{
	/// <summary>Constructs a new instance of the <see cref="T:System.Xml.Serialization.XmlAnyAttributeAttribute" /> class.</summary>
	public XmlAnyAttributeAttribute()
	{
	}
}
