using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32;

/// <summary>Represents the possible values for a top-level node on a foreign machine.</summary>
[Serializable]
[ComVisible(true)]
public enum RegistryHive
{
	/// <summary>Represents the HKEY_CLASSES_ROOT base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	ClassesRoot = int.MinValue,
	/// <summary>Represents the HKEY_CURRENT_CONFIG base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	CurrentConfig = -2147483643,
	/// <summary>Represents the HKEY_CURRENT_USER base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	CurrentUser = -2147483647,
	/// <summary>Represents the HKEY_DYN_DATA base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	DynData = -2147483642,
	/// <summary>Represents the HKEY_LOCAL_MACHINE base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	LocalMachine = -2147483646,
	/// <summary>Represents the HKEY_PERFORMANCE_DATA base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	PerformanceData = -2147483644,
	/// <summary>Represents the HKEY_USERS base key on another computer. This value can be passed to the <see cref="M:Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive,System.String)" /> method, to open this node remotely.</summary>
	Users = -2147483645
}
