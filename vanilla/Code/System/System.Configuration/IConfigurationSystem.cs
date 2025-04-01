using System.Runtime.InteropServices;

namespace System.Configuration;

/// <summary>Provides standard configuration methods.</summary>
/// <filterpriority>2</filterpriority>
[ComVisible(false)]
public interface IConfigurationSystem
{
	/// <summary>Gets the specified configuration.</summary>
	/// <returns>The object representing the configuration.</returns>
	/// <param name="configKey">The configuration key.</param>
	/// <filterpriority>2</filterpriority>
	object GetConfig(string configKey);

	/// <summary>Used for initialization.</summary>
	/// <filterpriority>2</filterpriority>
	void Init();
}
