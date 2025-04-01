using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32;

/// <summary>Provides <see cref="T:Microsoft.Win32.RegistryKey" /> objects that represent the root keys in the Windows registry, and static methods to access key/value pairs.</summary>
[ComVisible(true)]
public static class Registry
{
	/// <summary>Defines the types (or classes) of documents and the properties associated with those types. This field reads the Windows registry base key HKEY_CLASSES_ROOT.</summary>
	public static readonly RegistryKey ClassesRoot = new RegistryKey(RegistryHive.ClassesRoot);

	/// <summary>Contains configuration information pertaining to the hardware that is not specific to the user. This field reads the Windows registry base key HKEY_CURRENT_CONFIG.</summary>
	public static readonly RegistryKey CurrentConfig = new RegistryKey(RegistryHive.CurrentConfig);

	/// <summary>Contains information about the current user preferences. This field reads the Windows registry base key HKEY_CURRENT_USER </summary>
	public static readonly RegistryKey CurrentUser = new RegistryKey(RegistryHive.CurrentUser);

	/// <summary>Contains dynamic registry data. This field reads the Windows registry base key HKEY_DYN_DATA.</summary>
	/// <exception cref="T:System.ObjectDisposedException">The operating system does not support dynamic data; that is, it is not Windows 98, Windows 98 Second Edition, or Windows Millennium Edition (Windows Me).</exception>
	[Obsolete("Use PerformanceData instead")]
	public static readonly RegistryKey DynData = new RegistryKey(RegistryHive.DynData);

	/// <summary>Contains the configuration data for the local machine. This field reads the Windows registry base key HKEY_LOCAL_MACHINE.</summary>
	public static readonly RegistryKey LocalMachine = new RegistryKey(RegistryHive.LocalMachine);

	/// <summary>Contains performance information for software components. This field reads the Windows registry base key HKEY_PERFORMANCE_DATA.</summary>
	public static readonly RegistryKey PerformanceData = new RegistryKey(RegistryHive.PerformanceData);

	/// <summary>Contains information about the default user configuration. This field reads the Windows registry base key HKEY_USERS.</summary>
	public static readonly RegistryKey Users = new RegistryKey(RegistryHive.Users);

	private static RegistryKey ToKey(string keyName, bool setting)
	{
		if (keyName == null)
		{
			throw new ArgumentException("Not a valid registry key name", "keyName");
		}
		RegistryKey registryKey = null;
		string[] array = keyName.Split('\\');
		registryKey = array[0] switch
		{
			"HKEY_CLASSES_ROOT" => ClassesRoot, 
			"HKEY_CURRENT_CONFIG" => CurrentConfig, 
			"HKEY_CURRENT_USER" => CurrentUser, 
			"HKEY_DYN_DATA" => DynData, 
			"HKEY_LOCAL_MACHINE" => LocalMachine, 
			"HKEY_PERFORMANCE_DATA" => PerformanceData, 
			"HKEY_USERS" => Users, 
			_ => throw new ArgumentException("Keyname does not start with a valid registry root", "keyName"), 
		};
		for (int i = 1; i < array.Length; i++)
		{
			RegistryKey registryKey2 = registryKey.OpenSubKey(array[i], setting);
			if (registryKey2 == null)
			{
				if (!setting)
				{
					return null;
				}
				registryKey2 = registryKey.CreateSubKey(array[i]);
			}
			registryKey = registryKey2;
		}
		return registryKey;
	}

	/// <summary>Sets the specified name/value pair on the specified registry key. If the specified key does not exist, it is created.</summary>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="value">The value to be stored.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root. -or-<paramref name="keyName" /> is longer than the maximum length allowed (255 characters).</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and thus cannot be written to; for example, it is a root-level node. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static void SetValue(string keyName, string valueName, object value)
	{
		RegistryKey registryKey = ToKey(keyName, setting: true);
		if (valueName.Length > 255)
		{
			throw new ArgumentException("valueName is larger than 255 characters", "valueName");
		}
		if (registryKey == null)
		{
			throw new ArgumentException("cant locate that keyName", "keyName");
		}
		registryKey.SetValue(valueName, value);
	}

	/// <summary>Sets the name/value pair on the specified registry key, using the specified registry data type. If the specified key does not exist, it is created.</summary>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="value">The value to be stored.</param>
	/// <param name="valueKind">The registry data type to use when storing the data.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root.-or-<paramref name="keyName" /> is longer than the maximum length allowed (255 characters).-or- The type of <paramref name="value" /> did not match the registry data type specified by <paramref name="valueKind" />, therefore the data could not be converted properly. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <see cref="T:Microsoft.Win32.RegistryKey" /> is read-only, and thus cannot be written to; for example, it is a root-level node, or the key has not been opened with write access. </exception>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
	{
		RegistryKey registryKey = ToKey(keyName, setting: true);
		if (valueName.Length > 255)
		{
			throw new ArgumentException("valueName is larger than 255 characters", "valueName");
		}
		if (registryKey == null)
		{
			throw new ArgumentException("cant locate that keyName", "keyName");
		}
		registryKey.SetValue(valueName, value, valueKind);
	}

	/// <summary>Retrieves the value associated with the specified name, in the specified registry key. If the name is not found in the specified key, returns a default value that you provide, or null if the specified key does not exist. </summary>
	/// <returns>null if the subkey specified by <paramref name="keyName" /> does not exist; otherwise, the value associated with <paramref name="valueName" />, or <paramref name="defaultValue" /> if <paramref name="valueName" /> is not found.</returns>
	/// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
	/// <param name="valueName">The name of the name/value pair.</param>
	/// <param name="defaultValue">The value to return if <paramref name="valueName" /> does not exist.</param>
	/// <exception cref="T:System.Security.SecurityException">The user does not have the permissions required to read from the registry key. </exception>
	/// <exception cref="T:System.IO.IOException">The <see cref="T:Microsoft.Win32.RegistryKey" /> that contains the specified value has been marked for deletion. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="keyName" /> does not begin with a valid registry root. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.RegistryPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="\" />
	/// </PermissionSet>
	public static object GetValue(string keyName, string valueName, object defaultValue)
	{
		RegistryKey registryKey = ToKey(keyName, setting: false);
		if (registryKey == null)
		{
			return defaultValue;
		}
		return registryKey.GetValue(valueName, defaultValue);
	}
}
