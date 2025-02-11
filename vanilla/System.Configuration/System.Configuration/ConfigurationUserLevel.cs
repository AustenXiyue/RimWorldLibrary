namespace System.Configuration;

/// <summary>Used to specify which configuration file is to be represented by the Configuration object.</summary>
public enum ConfigurationUserLevel
{
	/// <summary>Get the <see cref="T:System.Configuration.Configuration" /> that applies to all users.</summary>
	None = 0,
	/// <summary>Get the roaming <see cref="T:System.Configuration.Configuration" /> that applies to the current user.</summary>
	PerUserRoaming = 10,
	/// <summary>Get the local <see cref="T:System.Configuration.Configuration" /> that applies to the current user.</summary>
	PerUserRoamingAndLocal = 20
}
