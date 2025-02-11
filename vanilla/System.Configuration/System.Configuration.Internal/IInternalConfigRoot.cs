using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines interfaces used by internal .NET structures to support a configuration root object.</summary>
[ComVisible(false)]
public interface IInternalConfigRoot
{
	/// <summary>Returns a value indicating whether the configuration is a design-time configuration.</summary>
	/// <returns>true if the configuration is a design-time configuration; false if the configuration is not a design-time configuration.</returns>
	bool IsDesignTime { get; }

	/// <summary>Represents the method that handles the <see cref="E:System.Configuration.Internal.IInternalConfigRoot.ConfigChanged" /> event of an <see cref="T:System.Configuration.Internal.IInternalConfigRoot" /> object.</summary>
	event InternalConfigEventHandler ConfigChanged;

	/// <summary>Represents the method that handles the <see cref="E:System.Configuration.Internal.IInternalConfigRoot.ConfigRemoved" /> event of a <see cref="T:System.Configuration.Internal.IInternalConfigRoot" /> object.</summary>
	event InternalConfigEventHandler ConfigRemoved;

	/// <summary>Returns an <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object representing a configuration specified by a configuration path.</summary>
	/// <returns>An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object representing a configuration specified by <paramref name="configPath" />.</returns>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	IInternalConfigRecord GetConfigRecord(string configPath);

	/// <summary>Returns an <see cref="T:System.Object" /> representing the data in a section of a configuration file.</summary>
	/// <returns>An <see cref="T:System.Object" /> representing the data in a section of a configuration file.</returns>
	/// <param name="section">A string representing a section of a configuration file.</param>
	/// <param name="configPath">A string representing the path to a configuration file.</param>
	object GetSection(string section, string configPath);

	/// <summary>Returns a value representing the file path of the nearest configuration ancestor that has configuration data.</summary>
	/// <returns>Returns a string representing the file path of the nearest configuration ancestor that has configuration data.</returns>
	/// <param name="configPath">The path of configuration file.</param>
	string GetUniqueConfigPath(string configPath);

	/// <summary>Returns an <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object representing a unique configuration record for given configuration path.</summary>
	/// <returns>An <see cref="T:System.Configuration.Internal.IInternalConfigRecord" /> object representing a unique configuration record for a given configuration path.</returns>
	/// <param name="configPath">The path of the configuration file.</param>
	IInternalConfigRecord GetUniqueConfigRecord(string configPath);

	/// <summary>Initializes a configuration object.</summary>
	/// <param name="host">An <see cref="T:System.Configuration.Internal.IInternalConfigHost" /> object.</param>
	/// <param name="isDesignTime">true if design time; false if run time.</param>
	void Init(IInternalConfigHost host, bool isDesignTime);

	/// <summary>Finds and removes a configuration record and all its children for a given configuration path.</summary>
	/// <param name="configPath">The path of the configuration file.</param>
	void RemoveConfig(string configPath);
}
