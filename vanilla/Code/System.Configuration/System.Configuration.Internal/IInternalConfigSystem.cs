using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the .NET Framework to initialize application configuration properties.</summary>
[ComVisible(false)]
public interface IInternalConfigSystem
{
	/// <summary>Gets a value indicating whether the user configuration is supported. </summary>
	/// <returns>true if the user configuration is supported; otherwise, false.</returns>
	bool SupportsUserConfig { get; }

	/// <summary>Returns the configuration object based on the specified key. </summary>
	/// <returns>A configuration object.</returns>
	/// <param name="configKey">The configuration key value.</param>
	object GetSection(string configKey);

	/// <summary>Refreshes the configuration system based on the specified section name. </summary>
	/// <param name="sectionName">The name of the configuration section.</param>
	void RefreshConfig(string sectionName);
}
