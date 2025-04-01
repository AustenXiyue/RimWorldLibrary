namespace System.Configuration;

/// <summary>Specifies the options to apply to a property.</summary>
[Flags]
public enum ConfigurationPropertyOptions
{
	/// <summary>Indicates that no option applies to the property.</summary>
	None = 0,
	/// <summary>Indicates that the property is a default collection. </summary>
	IsDefaultCollection = 1,
	/// <summary>Indicates that the property is required. </summary>
	IsRequired = 2,
	/// <summary>Indicates that the property is a collection key.</summary>
	IsKey = 4,
	/// <summary>Indicates whether the type name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
	IsTypeStringTransformationRequired = 8,
	/// <summary>Indicates whether the assembly name for the configuration property requires transformation when it is serialized for an earlier version of the .NET Framework.</summary>
	IsAssemblyStringTransformationRequired = 0x10,
	/// <summary>Indicates whether the configuration property's parent configuration section should be queried at serialization time to determine whether the configuration property should be serialized into XML.</summary>
	IsVersionCheckRequired = 0x20
}
