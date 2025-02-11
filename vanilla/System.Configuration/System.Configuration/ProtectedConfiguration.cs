namespace System.Configuration;

/// <summary>Provides access to the protected-configuration providers for the current application's configuration file. </summary>
public static class ProtectedConfiguration
{
	/// <summary>The name of the data protection provider.</summary>
	public const string DataProtectionProviderName = "DataProtectionConfigurationProvider";

	/// <summary>The name of the protected data section.</summary>
	public const string ProtectedDataSectionName = "configProtectedData";

	/// <summary>The name of the RSA provider.</summary>
	public const string RsaProviderName = "RsaProtectedConfigurationProvider";

	/// <summary>Gets the name of the default protected-configuration provider.</summary>
	/// <returns>The name of the default protected-configuration provider.</returns>
	public static string DefaultProvider => Section.DefaultProvider;

	/// <summary>Gets a collection of the installed protected-configuration providers.</summary>
	/// <returns>A <see cref="T:System.Configuration.ProtectedConfigurationProviderCollection" /> collection of installed <see cref="T:System.Configuration.ProtectedConfigurationProvider" /> objects.</returns>
	public static ProtectedConfigurationProviderCollection Providers => Section.GetAllProviders();

	internal static ProtectedConfigurationSection Section => (ProtectedConfigurationSection)ConfigurationManager.GetSection("configProtectedData");

	internal static ProtectedConfigurationProvider GetProvider(string name, bool throwOnError)
	{
		ProtectedConfigurationProvider protectedConfigurationProvider = Providers[name];
		if (protectedConfigurationProvider == null && throwOnError)
		{
			throw new Exception($"The protection provider '{name}' was not found.");
		}
		return protectedConfigurationProvider;
	}
}
