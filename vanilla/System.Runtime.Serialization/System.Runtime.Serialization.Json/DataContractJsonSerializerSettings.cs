using System.Collections.Generic;

namespace System.Runtime.Serialization.Json;

/// <summary>Specifies <see cref="T:System.Runtime.Serialization.Json.DataContractJsonSerializer" /> settings.</summary>
public class DataContractJsonSerializerSettings
{
	private int maxItemsInObjectGraph = int.MaxValue;

	/// <summary>Gets or sets the root name of the selected object.</summary>
	/// <returns>The root name of the selected object.</returns>
	public string RootName { get; set; }

	/// <summary>Gets or sets a collection of types that may be present in the object graph serialized using this instance the DataContractJsonSerializerSettings.</summary>
	/// <returns>A collection of types that may be present in the object graph serialized using this instance the DataContractJsonSerializerSettings.</returns>
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

	/// <summary>Gets or sets a surrogate type that is currently active for given IDataContractSurrogate instance.</summary>
	/// <returns>The surrogate type that is currently active for given IDataContractSurrogate instance.</returns>
	public IDataContractSurrogate DataContractSurrogate { get; set; }

	/// <summary>Gets or sets the data contract JSON serializer settings to emit type information.</summary>
	/// <returns>The data contract JSON serializer settings to emit type information.</returns>
	public EmitTypeInformation EmitTypeInformation { get; set; }

	/// <summary>Gets or sets a DateTimeFormat that defines the culturally appropriate format of displaying dates and times.</summary>
	/// <returns>The DateTimeFormat that defines the culturally appropriate format of displaying dates and times.</returns>
	public DateTimeFormat DateTimeFormat { get; set; }

	/// <summary>Gets or sets a value that specifies whether to serialize read only types.</summary>
	/// <returns>True to serialize read only types; otherwise false.</returns>
	public bool SerializeReadOnlyTypes { get; set; }

	/// <summary>Gets or sets a value that specifies whether to use a simple dictionary format.</summary>
	/// <returns>True to use a simple dictionary format; otherwise, false.</returns>
	public bool UseSimpleDictionaryFormat { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.Json.DataContractJsonSerializerSettings" /> class.</summary>
	public DataContractJsonSerializerSettings()
	{
	}
}
