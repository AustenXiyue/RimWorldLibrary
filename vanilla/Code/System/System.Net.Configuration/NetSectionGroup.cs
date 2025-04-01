using System.Configuration;

namespace System.Net.Configuration;

/// <summary>Gets the section group information for the networking namespaces. This class cannot be inherited.</summary>
public sealed class NetSectionGroup : ConfigurationSectionGroup
{
	/// <summary>Gets the configuration section containing the authentication modules registered for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.AuthenticationModulesSection" /> object.</returns>
	[ConfigurationProperty("authenticationModules")]
	public AuthenticationModulesSection AuthenticationModules => (AuthenticationModulesSection)base.Sections["authenticationModules"];

	/// <summary>Gets the configuration section containing the connection management settings for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.ConnectionManagementSection" /> object.</returns>
	[ConfigurationProperty("connectionManagement")]
	public ConnectionManagementSection ConnectionManagement => (ConnectionManagementSection)base.Sections["connectionManagement"];

	/// <summary>Gets the configuration section containing the default Web proxy server settings for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.DefaultProxySection" /> object.</returns>
	[ConfigurationProperty("defaultProxy")]
	public DefaultProxySection DefaultProxy => (DefaultProxySection)base.Sections["defaultProxy"];

	/// <summary>Gets the configuration section containing the SMTP client e-mail settings for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.MailSettingsSectionGroup" /> object.</returns>
	public MailSettingsSectionGroup MailSettings => (MailSettingsSectionGroup)base.SectionGroups["mailSettings"];

	/// <summary>Gets the configuration section containing the cache configuration settings for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.RequestCachingSection" /> object.</returns>
	[ConfigurationProperty("requestCaching")]
	public RequestCachingSection RequestCaching => (RequestCachingSection)base.Sections["requestCaching"];

	/// <summary>Gets the configuration section containing the network settings for the local computer.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.SettingsSection" /> object.</returns>
	[ConfigurationProperty("settings")]
	public SettingsSection Settings => (SettingsSection)base.Sections["settings"];

	/// <summary>Gets the configuration section containing the modules registered for use with the <see cref="T:System.Net.WebRequest" /> class.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.WebRequestModulesSection" /> object.</returns>
	[ConfigurationProperty("webRequestModules")]
	public WebRequestModulesSection WebRequestModules => (WebRequestModulesSection)base.Sections["webRequestModules"];

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Configuration.NetSectionGroup" /> class. </summary>
	[System.MonoTODO]
	public NetSectionGroup()
	{
	}

	/// <summary>Gets the System.Net configuration section group from the specified configuration file.</summary>
	/// <returns>A <see cref="T:System.Net.Configuration.NetSectionGroup" /> that represents the System.Net settings in <paramref name="config" />.</returns>
	/// <param name="config">A <see cref="T:System.Configuration.Configuration" /> that represents a configuration file.</param>
	[System.MonoTODO]
	public static NetSectionGroup GetSectionGroup(System.Configuration.Configuration config)
	{
		throw new NotImplementedException();
	}
}
