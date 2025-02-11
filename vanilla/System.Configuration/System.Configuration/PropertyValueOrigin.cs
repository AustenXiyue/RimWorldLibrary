namespace System.Configuration;

/// <summary>Specifies the level in the configuration hierarchy where a configuration property value originated.</summary>
public enum PropertyValueOrigin
{
	/// <summary>The configuration property value originates from the <see cref="P:System.Configuration.ConfigurationProperty.DefaultValue" /> property.</summary>
	Default,
	/// <summary>The configuration property value is inherited from a parent level in the configuration.</summary>
	Inherited,
	/// <summary>The configuration property value is defined at the current level of the hierarchy.</summary>
	SetHere
}
