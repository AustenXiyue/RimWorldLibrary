using System.Threading;
using Microsoft.Win32;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class TextServicesLoader
{
	private enum EnableState
	{
		Error,
		None,
		Enabled,
		Disabled
	}

	private delegate EnableState IterateHandler(RegistryKey key, string subKeyName, bool localMachine);

	private enum InstallState
	{
		Unknown,
		Installed,
		NotInstalled
	}

	private const int CLSIDLength = 38;

	private const int LANGIDLength = 10;

	private static InstallState s_servicesInstalled = InstallState.Unknown;

	private static object s_servicesInstalledLock = new object();

	internal static bool ServicesInstalled
	{
		get
		{
			lock (s_servicesInstalledLock)
			{
				if (s_servicesInstalled == InstallState.Unknown)
				{
					s_servicesInstalled = (TIPsWantToRun() ? InstallState.Installed : InstallState.NotInstalled);
				}
			}
			return s_servicesInstalled == InstallState.Installed;
		}
	}

	private TextServicesLoader()
	{
	}

	internal static UnsafeNativeMethods.ITfThreadMgr Load()
	{
		Invariant.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA, "Load called on MTA thread!");
		if (ServicesInstalled && UnsafeNativeMethods.TF_CreateThreadMgr(out var threadManager) == 0)
		{
			return threadManager;
		}
		return null;
	}

	private static bool TIPsWantToRun()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\CTF", writable: false);
		if (registryKey != null)
		{
			object value = registryKey.GetValue("Disable Thread Input Manager");
			if (value is int && (int)value != 0)
			{
				return false;
			}
		}
		return IterateSubKeys(Registry.LocalMachine, "SOFTWARE\\Microsoft\\CTF\\TIP", SingleTIPWantsToRun, localMachine: true) == EnableState.Enabled;
	}

	private static EnableState SingleTIPWantsToRun(RegistryKey keyLocalMachine, string subKeyName, bool localMachine)
	{
		if (subKeyName.Length != 38)
		{
			return EnableState.Disabled;
		}
		EnableState enableState = IterateSubKeys(Registry.CurrentUser, "SOFTWARE\\Microsoft\\CTF\\TIP\\" + subKeyName + "\\LanguageProfile", IsLangidEnabled, localMachine: false);
		if (enableState == EnableState.None || enableState == EnableState.Error)
		{
			enableState = IterateSubKeys(keyLocalMachine, subKeyName + "\\LanguageProfile", IsLangidEnabled, localMachine: true);
			if (enableState == EnableState.None)
			{
				enableState = EnableState.Enabled;
			}
		}
		return enableState;
	}

	private static EnableState IsLangidEnabled(RegistryKey key, string subKeyName, bool localMachine)
	{
		if (subKeyName.Length != 10)
		{
			return EnableState.Error;
		}
		return IterateSubKeys(key, subKeyName, IsAssemblyEnabled, localMachine);
	}

	private static EnableState IsAssemblyEnabled(RegistryKey key, string subKeyName, bool localMachine)
	{
		if (subKeyName.Length != 38)
		{
			return EnableState.Error;
		}
		RegistryKey registryKey = key.OpenSubKey(subKeyName);
		if (registryKey == null)
		{
			return EnableState.Error;
		}
		object value = registryKey.GetValue("Enable");
		if (value is int)
		{
			if ((int)value != 0)
			{
				return EnableState.Enabled;
			}
			return EnableState.Disabled;
		}
		return EnableState.None;
	}

	private static EnableState IterateSubKeys(RegistryKey keyBase, string subKey, IterateHandler handler, bool localMachine)
	{
		RegistryKey registryKey = keyBase.OpenSubKey(subKey, writable: false);
		if (registryKey == null)
		{
			return EnableState.Error;
		}
		string[] subKeyNames = registryKey.GetSubKeyNames();
		EnableState enableState = EnableState.Error;
		string[] array = subKeyNames;
		foreach (string subKeyName in array)
		{
			switch (handler(registryKey, subKeyName, localMachine))
			{
			case EnableState.None:
				if (localMachine)
				{
					return EnableState.None;
				}
				if (enableState == EnableState.Error)
				{
					enableState = EnableState.None;
				}
				break;
			case EnableState.Disabled:
				enableState = EnableState.Disabled;
				break;
			case EnableState.Enabled:
				return EnableState.Enabled;
			}
		}
		return enableState;
	}
}
