using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the .NET Framework to initialize configuration properties.</summary>
[ComVisible(false)]
public interface IConfigurationManagerInternal
{
	/// <summary>Gets the configuration file name related to the application path.</summary>
	/// <returns>A string value representing a configuration file name.</returns>
	string ApplicationConfigUri { get; }

	/// <summary>Gets the local configuration directory of the application based on the entry assembly.</summary>
	/// <returns>A string representing the local configuration directory.</returns>
	string ExeLocalConfigDirectory { get; }

	/// <summary>Gets the local configuration path of the application based on the entry assembly.</summary>
	/// <returns>A string value representing the local configuration path of the application.</returns>
	string ExeLocalConfigPath { get; }

	/// <summary>Gets the product name of the application based on the entry assembly.</summary>
	/// <returns>A string value representing the product name of the application.</returns>
	string ExeProductName { get; }

	/// <summary>Gets the product version of the application based on the entry assembly.</summary>
	/// <returns>A string value representing the product version of the application.</returns>
	string ExeProductVersion { get; }

	/// <summary>Gets the roaming configuration directory of the application based on the entry assembly.</summary>
	/// <returns>A string value representing the roaming configuration directory of the application.</returns>
	string ExeRoamingConfigDirectory { get; }

	/// <summary>Gets the roaming user's configuration path based on the application's entry assembly.</summary>
	/// <returns>A string value representing the roaming user's configuration path.</returns>
	string ExeRoamingConfigPath { get; }

	/// <summary>Gets the configuration path for the Machine.config file.</summary>
	/// <returns>A string value representing the path of the Machine.config file.</returns>
	string MachineConfigPath { get; }

	/// <summary>Gets a value representing the configuration system's status.</summary>
	/// <returns>true if the configuration system is in the process of being initialized; otherwise, false.</returns>
	bool SetConfigurationSystemInProgress { get; }

	/// <summary>Gets a value that specifies whether user configuration settings are supported.</summary>
	/// <returns>true if the configuration system supports user configuration settings; otherwise, false.</returns>
	bool SupportsUserConfig { get; }

	/// <summary>Gets the name of the file used to store user configuration settings.</summary>
	/// <returns>A string specifying the name of the file used to store user configuration.</returns>
	string UserConfigFilename { get; }
}
