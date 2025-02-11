namespace System.Xml.Serialization;

/// <summary>Specifies that the target property, parameter, return value, or class member contains prefixes associated with namespaces that are used within an XML document.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
public class XmlNamespaceDeclarationsAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.XmlNamespaceDeclarationsAttribute" /> class.</summary>
	public XmlNamespaceDeclarationsAttribute()
	{
	}
}
