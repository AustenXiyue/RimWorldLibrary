using System.Configuration;

namespace System.Runtime.Serialization.Configuration;

/// <summary>Handles the XML elements used to configure serialization by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />.</summary>
public sealed class SerializationSectionGroup : ConfigurationSectionGroup
{
	/// <summary>Gets the <see cref="T:System.Runtime.Serialization.Configuration.DataContractSerializerSection" /> used to set up the known types collection. </summary>
	/// <returns>The <see cref="T:System.Runtime.Serialization.Configuration.DataContractSerializerSection" /> used for the serialization configuration section.</returns>
	public DataContractSerializerSection DataContractSerializer => (DataContractSerializerSection)base.Sections["dataContractSerializer"];

	/// <summary>Gets the <see cref="T:System.Runtime.Serialization.Configuration.NetDataContractSerializerSection" /> used to set up the known types collection. </summary>
	/// <returns>The <see cref="T:System.Runtime.Serialization.Configuration.NetDataContractSerializerSection" /> object.</returns>
	public NetDataContractSerializerSection NetDataContractSerializer => (NetDataContractSerializerSection)base.Sections["netDataContractSerializer"];

	/// <summary>Gets the serialization configuration section for the specified configuration.</summary>
	/// <returns>A <see cref="T:System.Runtime.Serialization.Configuration.SerializationSectionGroup" /> that represents the configuration section.</returns>
	/// <param name="config">A <see cref="T:System.Configuration.Configuration" /> that represents the configuration to retrieve.</param>
	public static SerializationSectionGroup GetSectionGroup(System.Configuration.Configuration config)
	{
		if (config == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
		}
		return (SerializationSectionGroup)config.SectionGroups["system.runtime.serialization"];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Configuration.SerializationSectionGroup" /> class.  </summary>
	public SerializationSectionGroup()
	{
	}
}
