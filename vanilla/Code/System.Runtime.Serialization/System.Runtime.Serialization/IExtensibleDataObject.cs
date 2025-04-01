namespace System.Runtime.Serialization;

/// <summary>Provides a data structure to store extra data encountered by the <see cref="T:System.Runtime.Serialization.XmlObjectSerializer" /> during deserialization of a type marked with the <see cref="T:System.Runtime.Serialization.DataContractAttribute" /> attribute. </summary>
public interface IExtensibleDataObject
{
	/// <summary>Gets or sets the structure that contains extra data.</summary>
	/// <returns>An <see cref="T:System.Runtime.Serialization.ExtensionDataObject" /> that contains data that is not recognized as belonging to the data contract.</returns>
	ExtensionDataObject ExtensionData { get; set; }
}
