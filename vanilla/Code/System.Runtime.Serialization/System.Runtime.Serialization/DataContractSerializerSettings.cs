using System.Collections.Generic;
using System.Xml;

namespace System.Runtime.Serialization;

/// <summary>Specifies data contract serializer settings.</summary>
public class DataContractSerializerSettings
{
	private int maxItemsInObjectGraph = int.MaxValue;

	/// <summary>Gets or sets the root name of the selected object.</summary>
	/// <returns>The root name of the selected object.</returns>
	public XmlDictionaryString RootName { get; set; }

	/// <summary>Gets or sets the root namespace for the specified object.</summary>
	/// <returns>The root namespace for the specified object.</returns>
	public XmlDictionaryString RootNamespace { get; set; }

	/// <summary>Gets or sets a collection of types that may be present in the object graph serialized using this instance of the DataContractSerializerSettings.</summary>
	/// <returns>A collection of types that may be present in the object graph serialized using this instance of the DataContractSerializerSettings.</returns>
	public IEnumerable<Type> KnownTypes { get; set; }

	/// <summary>Gets or sets the maximum number of items in an object graph to serialize or deserialize.</summary>
	/// <returns>The maximum number of items in an object graph to serialize or deserialize.</returns>
	public int MaxItemsInObjectGraph
	{
		get
		{
			return maxItemsInObjectGraph;
		}
		set
		{
			maxItemsInObjectGraph = value;
		}
	}

	/// <summary>Gets or sets a value that specifies whether to ignore data supplied by an extension of the class when the class is being serialized or deserialized.</summary>
	/// <returns>True to ignore data supplied by an extension of the class when the class is being serialized or deserialized; otherwise, false.</returns>
	public bool IgnoreExtensionDataObject { get; set; }

	/// <summary>Gets or sets a value that specifies whether to use non-standard XML constructs to preserve object reference data.</summary>
	/// <returns>True to use non-standard XML constructs to preserve object reference data; otherwise, false.</returns>
	public bool PreserveObjectReferences { get; set; }

	/// <summary>Gets or sets a serialization surrogate.</summary>
	/// <returns>The serialization surrogate.</returns>
	public IDataContractSurrogate DataContractSurrogate { get; set; }

	/// <summary>Gets or sets the component used to dynamically map xsi:type declarations to known contract types.</summary>
	/// <returns>The component used to dynamically map xsi:type declarations to known contract types.</returns>
	public DataContractResolver DataContractResolver { get; set; }

	/// <summary>Gets or sets a value that specifies whether to serialize read only types.</summary>
	/// <returns>True to serialize read only types; otherwise, false.</returns>
	public bool SerializeReadOnlyTypes { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.DataContractSerializerSettings" /> class.</summary>
	public DataContractSerializerSettings()
	{
	}
}
