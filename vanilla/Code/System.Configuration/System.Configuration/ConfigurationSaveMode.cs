namespace System.Configuration;

/// <summary>Determines which properties are written out to a configuration file.</summary>
public enum ConfigurationSaveMode
{
	/// <summary>Causes only properties that differ from inherited values to be written to the configuration file.</summary>
	Minimal = 1,
	/// <summary>Causes all properties to be written to the configuration file. This is useful mostly for creating information configuration files or moving configuration values from one machine to another.</summary>
	Full = 2,
	/// <summary>Causes only modified properties to be written to the configuration file, even when the value is the same as the inherited value.</summary>
	Modified = 0
}
