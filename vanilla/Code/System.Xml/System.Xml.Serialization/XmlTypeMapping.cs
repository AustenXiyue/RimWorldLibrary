using Unity;

namespace System.Xml.Serialization;

/// <summary>Contains a mapping of one type to another.</summary>
public class XmlTypeMapping : XmlMapping
{
	internal TypeMapping Mapping => base.Accessor.Mapping;

	/// <summary>Gets the type name of the mapped object.</summary>
	/// <returns>The type name of the mapped object.</returns>
	public string TypeName => Mapping.TypeDesc.Name;

	/// <summary>The fully qualified type name that includes the namespace (or namespaces) and type.</summary>
	/// <returns>The fully qualified type name.</returns>
	public string TypeFullName => Mapping.TypeDesc.FullName;

	/// <summary>Gets the XML element name of the mapped object.</summary>
	/// <returns>The XML element name of the mapped object. The default is the class name of the object.</returns>
	public string XsdTypeName => Mapping.TypeName;

	/// <summary>Gets the XML namespace of the mapped object.</summary>
	/// <returns>The XML namespace of the mapped object. The default is an empty string ("").</returns>
	public string XsdTypeNamespace => Mapping.Namespace;

	internal XmlTypeMapping(TypeScope scope, ElementAccessor accessor)
		: base(scope, accessor)
	{
	}

	internal XmlTypeMapping()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
