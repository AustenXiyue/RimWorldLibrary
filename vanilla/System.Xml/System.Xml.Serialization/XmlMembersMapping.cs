using System.Text;
using Unity;

namespace System.Xml.Serialization;

/// <summary>Provides mappings between .NET Framework Web service methods and Web Services Description Language (WSDL) messages that are defined for SOAP Web services. </summary>
public class XmlMembersMapping : XmlMapping
{
	private XmlMemberMapping[] mappings;

	/// <summary>Gets the name of the .NET Framework type being mapped to the data type of an XML Schema element that represents a SOAP message.</summary>
	/// <returns>The name of the .NET Framework type.</returns>
	public string TypeName => base.Accessor.Mapping.TypeName;

	/// <summary>Gets the namespace of the .NET Framework type being mapped to the data type of an XML Schema element that represents a SOAP message.</summary>
	/// <returns>The .NET Framework namespace of the mapping.</returns>
	public string TypeNamespace => base.Accessor.Mapping.Namespace;

	/// <summary>Gets an item that contains internal type mapping information for a .NET Framework code entity that belongs to a Web service method being mapped to a SOAP message.</summary>
	/// <returns>The requested <see cref="T:System.Xml.Serialization.XmlMemberMapping" />.</returns>
	/// <param name="index">The index of the mapping to return.</param>
	public XmlMemberMapping this[int index] => mappings[index];

	/// <summary>Gets the number of .NET Framework code entities that belong to a Web service method to which a SOAP message is being mapped. </summary>
	/// <returns>The number of mappings in the collection.</returns>
	public int Count => mappings.Length;

	internal XmlMembersMapping(TypeScope scope, ElementAccessor accessor, XmlMappingAccess access)
		: base(scope, accessor, access)
	{
		MembersMapping membersMapping = (MembersMapping)accessor.Mapping;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(":");
		mappings = new XmlMemberMapping[membersMapping.Members.Length];
		for (int i = 0; i < mappings.Length; i++)
		{
			if (membersMapping.Members[i].TypeDesc.Type != null)
			{
				stringBuilder.Append(XmlMapping.GenerateKey(membersMapping.Members[i].TypeDesc.Type, null, null));
				stringBuilder.Append(":");
			}
			mappings[i] = new XmlMemberMapping(membersMapping.Members[i]);
		}
		SetKeyInternal(stringBuilder.ToString());
	}

	internal XmlMembersMapping()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
