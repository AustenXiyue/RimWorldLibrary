using System.Collections.Specialized;

namespace System.Configuration;

/// <summary>Provides runtime versions 1.0 and 1.1 support for reading configuration sections and common configuration settings.</summary>
/// <filterpriority>2</filterpriority>
public sealed class ConfigurationSettings
{
	private static IConfigurationSystem config = DefaultConfig.GetInstance();

	private static object lockobj = new object();

	/// <summary>Gets a read-only <see cref="T:System.Collections.Specialized.NameValueCollection" /> of the application settings section of the configuration file.</summary>
	/// <returns>A read-only <see cref="T:System.Collections.Specialized.NameValueCollection" /> of the application settings section from the configuration file.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("This property is obsolete.  Please use System.Configuration.ConfigurationManager.AppSettings")]
	public static NameValueCollection AppSettings
	{
		get
		{
			object obj = ConfigurationManager.GetSection("appSettings");
			if (obj == null)
			{
				obj = new NameValueCollection();
			}
			return (NameValueCollection)obj;
		}
	}

	private ConfigurationSettings()
	{
	}

	/// <summary>Returns the <see cref="T:System.Configuration.ConfigurationSection" /> object for the passed configuration section name and path.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSection" /> object for the passed configuration section name and path.NoteThe <see cref="T:System.Configuration.ConfigurationSettings" /> class provides backward compatibility only. You should use the <see cref="T:System.Configuration.ConfigurationManager" /> class or <see cref="T:System.Web.Configuration.WebConfigurationManager" /> class instead.</returns>
	/// <param name="sectionName">A configuration name and path, such as "system.net/settings".</param>
	/// <exception cref="T:System.Configuration.ConfigurationException">Unable to retrieve the requested section.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete("This method is obsolete, it has been replaced by System.Configuration!System.Configuration.ConfigurationManager.GetSection")]
	public static object GetConfig(string sectionName)
	{
		return ConfigurationManager.GetSection(sectionName);
	}

	internal static IConfigurationSystem ChangeConfigurationSystem(IConfigurationSystem newSystem)
	{
		if (newSystem == null)
		{
			throw new ArgumentNullException("newSystem");
		}
		lock (lockobj)
		{
			IConfigurationSystem result = config;
			config = newSystem;
			return result;
		}
	}
}
