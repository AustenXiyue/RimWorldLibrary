using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the configuration system to set the <see cref="T:System.Configuration.ConfigurationSettings" /> class.</summary>
[ComVisible(false)]
public interface IInternalConfigSettingsFactory
{
	/// <summary>Indicates that initialization of the configuration system has completed. </summary>
	void CompleteInit();

	/// <summary>Provides hierarchical configuration settings and extensions specific to ASP.NET to the configuration system. </summary>
	/// <param name="internalConfigSystem">An <see cref="T:System.Configuration.Internal.IInternalConfigSystem" /> object used by the <see cref="T:System.Configuration.ConfigurationSettings" /> class.</param>
	/// <param name="initComplete">true if the initialization process of the configuration system is complete; otherwise, false.</param>
	void SetConfigurationSystem(IInternalConfigSystem internalConfigSystem, bool initComplete);
}
