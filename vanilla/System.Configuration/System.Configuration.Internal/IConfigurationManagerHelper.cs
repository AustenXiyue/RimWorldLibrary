using System.Runtime.InteropServices;

namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the .NET Framework to support configuration management.</summary>
[ComVisible(false)]
public interface IConfigurationManagerHelper
{
	/// <summary>Ensures that the networking configuration is loaded.</summary>
	void EnsureNetConfigLoaded();
}
