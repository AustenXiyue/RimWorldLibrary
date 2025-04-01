namespace System.Management.Instrumentation;

/// <summary>Defines values that specify the hosting model for the provider.</summary>
public enum ManagementHostingModel
{
	/// <summary>Activates the provider as a decoupled provider.</summary>
	Decoupled = 0,
	/// <summary>Activates the provider in the provider host process that is running under the LocalService account.</summary>
	LocalService = 2,
	/// <summary>Activates the provider in the provider host process that is running under the LocalSystem account.</summary>
	LocalSystem = 3,
	/// <summary>Activates the provider in the provider host process that is running under the NetworkService account.</summary>
	NetworkService = 1
}
