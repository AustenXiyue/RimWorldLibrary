using System.Collections.Specialized;
using System.Configuration.Internal;
using System.IO;
using System.Reflection;
using System.Text;
using Unity;

namespace System.Configuration;

/// <summary>Provides access to configuration files for client applications. This class cannot be inherited.</summary>
public static class ConfigurationManager
{
	private static InternalConfigurationFactory configFactory = new InternalConfigurationFactory();

	private static IInternalConfigSystem configSystem = new ClientConfigurationSystem();

	private static object lockobj = new object();

	internal static IInternalConfigConfigurationFactory ConfigurationFactory => configFactory;

	internal static IInternalConfigSystem ConfigurationSystem => configSystem;

	/// <summary>Gets the <see cref="T:System.Configuration.AppSettingsSection" /> data for the current application's default configuration.</summary>
	/// <returns>Returns a <see cref="T:System.Collections.Specialized.NameValueCollection" /> object that contains the contents of the <see cref="T:System.Configuration.AppSettingsSection" /> object for the current application's default configuration. </returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">Could not retrieve a <see cref="T:System.Collections.Specialized.NameValueCollection" /> object with the application settings data.</exception>
	public static NameValueCollection AppSettings => (NameValueCollection)GetSection("appSettings");

	/// <summary>Gets the <see cref="T:System.Configuration.ConnectionStringsSection" /> data for the current application's default configuration.</summary>
	/// <returns>Returns a <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> object that contains the contents of the <see cref="T:System.Configuration.ConnectionStringsSection" /> object for the current application's default configuration. </returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">Could not retrieve a <see cref="T:System.Configuration.ConnectionStringSettingsCollection" /> object.</exception>
	public static ConnectionStringSettingsCollection ConnectionStrings => ((ConnectionStringsSection)GetSection("connectionStrings")).ConnectionStrings;

	[System.MonoTODO("Evidence and version still needs work")]
	private static string GetAssemblyInfo(Assembly a)
	{
		object[] customAttributes = a.GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false);
		string arg = ((customAttributes == null || customAttributes.Length == 0) ? AppDomain.CurrentDomain.FriendlyName : ((AssemblyProductAttribute)customAttributes[0]).Product);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("evidencehere");
		string arg2 = stringBuilder.ToString();
		customAttributes = a.GetCustomAttributes(typeof(AssemblyVersionAttribute), inherit: false);
		return Path.Combine(path2: (customAttributes == null || customAttributes.Length == 0) ? "1.0.0.0" : ((AssemblyVersionAttribute)customAttributes[0]).Version, path1: $"{arg}_{arg2}");
	}

	internal static Configuration OpenExeConfigurationInternal(ConfigurationUserLevel userLevel, Assembly calling_assembly, string exePath)
	{
		ExeConfigurationFileMap exeConfigurationFileMap = new ExeConfigurationFileMap();
		if (userLevel != 0)
		{
			if (userLevel != ConfigurationUserLevel.PerUserRoaming)
			{
				if (userLevel != ConfigurationUserLevel.PerUserRoamingAndLocal)
				{
					goto IL_00ea;
				}
				exeConfigurationFileMap.LocalUserConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GetAssemblyInfo(calling_assembly));
				exeConfigurationFileMap.LocalUserConfigFilename = Path.Combine(exeConfigurationFileMap.LocalUserConfigFilename, "user.config");
			}
			exeConfigurationFileMap.RoamingUserConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetAssemblyInfo(calling_assembly));
			exeConfigurationFileMap.RoamingUserConfigFilename = Path.Combine(exeConfigurationFileMap.RoamingUserConfigFilename, "user.config");
		}
		if (exePath == null || exePath.Length == 0)
		{
			exeConfigurationFileMap.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
		}
		else
		{
			if (!Path.IsPathRooted(exePath))
			{
				exePath = Path.GetFullPath(exePath);
			}
			if (!File.Exists(exePath))
			{
				Exception inner = new ArgumentException("The specified path does not exist.", "exePath");
				throw new ConfigurationErrorsException("Error Initializing the configuration system:", inner);
			}
			exeConfigurationFileMap.ExeConfigFilename = exePath + ".config";
		}
		goto IL_00ea;
		IL_00ea:
		return ConfigurationFactory.Create(typeof(ExeConfigurationHost), exeConfigurationFileMap, userLevel);
	}

	/// <summary>Opens the configuration file for the current application as a <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.Configuration" /> object.</returns>
	/// <param name="userLevel">The <see cref="T:System.Configuration.ConfigurationUserLevel" /> for which you are opening the configuration.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
	{
		return OpenExeConfigurationInternal(userLevel, Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), null);
	}

	/// <summary>Opens the specified client configuration file as a <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.Configuration" /> object.</returns>
	/// <param name="exePath">The path of the executable (exe) file.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static Configuration OpenExeConfiguration(string exePath)
	{
		return OpenExeConfigurationInternal(ConfigurationUserLevel.None, Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), exePath);
	}

	/// <summary>Opens the specified client configuration file as a <see cref="T:System.Configuration.Configuration" /> object that uses the specified file mapping and user level.</summary>
	/// <returns>The configuration object.</returns>
	/// <param name="fileMap">An <see cref="T:System.Configuration.ExeConfigurationFileMap" /> object that references configuration file to use instead of the application default configuration file.</param>
	/// <param name="userLevel">The <see cref="T:System.Configuration.ConfigurationUserLevel" /> object for which you are opening the configuration.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	[System.MonoLimitation("ConfigurationUserLevel parameter is not supported.")]
	public static Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
	{
		return ConfigurationFactory.Create(typeof(ExeConfigurationHost), fileMap, userLevel);
	}

	/// <summary>Opens the machine configuration file on the current computer as a <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.Configuration" /> object.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static Configuration OpenMachineConfiguration()
	{
		ConfigurationFileMap configurationFileMap = new ConfigurationFileMap();
		return ConfigurationFactory.Create(typeof(MachineConfigurationHost), configurationFileMap);
	}

	/// <summary>Opens the machine configuration file as a <see cref="T:System.Configuration.Configuration" /> object that uses the specified file mapping.</summary>
	/// <returns>A <see cref="T:System.Configuration.Configuration" /> object.</returns>
	/// <param name="fileMap">An <see cref="T:System.Configuration.ExeConfigurationFileMap" /> object that references configuration file to use instead of the application default configuration file.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap)
	{
		return ConfigurationFactory.Create(typeof(MachineConfigurationHost), fileMap);
	}

	/// <summary>Retrieves a specified configuration section for the current application's default configuration.</summary>
	/// <returns>The specified <see cref="T:System.Configuration.ConfigurationSection" /> object, or null if the section does not exist.</returns>
	/// <param name="sectionName">The configuration section path and name.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static object GetSection(string sectionName)
	{
		object section = ConfigurationSystem.GetSection(sectionName);
		if (section is ConfigurationSection)
		{
			return ((ConfigurationSection)section).GetRuntimeObject();
		}
		return section;
	}

	/// <summary>Refreshes the named section so the next time that it is retrieved it will be re-read from disk.</summary>
	/// <param name="sectionName">The configuration section name or the configuration path and section name of the section to refresh.</param>
	public static void RefreshSection(string sectionName)
	{
		ConfigurationSystem.RefreshConfig(sectionName);
	}

	internal static IInternalConfigSystem ChangeConfigurationSystem(IInternalConfigSystem newSystem)
	{
		if (newSystem == null)
		{
			throw new ArgumentNullException("newSystem");
		}
		lock (lockobj)
		{
			IInternalConfigSystem result = configSystem;
			configSystem = newSystem;
			return result;
		}
	}

	/// <summary>Opens the specified client configuration file as a <see cref="T:System.Configuration.Configuration" /> object that uses the specified file mapping, user level, and preload option.</summary>
	/// <returns>The configuration object.</returns>
	/// <param name="fileMap">An <see cref="T:System.Configuration.ExeConfigurationFileMap" /> object that references the configuration file to use instead of the default application configuration file.</param>
	/// <param name="userLevel">The <see cref="T:System.Configuration.ConfigurationUserLevel" /> object for which you are opening the configuration.</param>
	/// <param name="preLoad">true to preload all section groups and sections; otherwise, false.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">A configuration file could not be loaded.</exception>
	public static Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel, bool preLoad)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
