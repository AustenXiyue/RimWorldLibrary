using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace MS.Internal;

internal class AssemblyFilter
{
	private static MS.Internal.SecurityCriticalDataForSet<List<string>> _assemblyList;

	private static MS.Internal.SecurityCriticalDataForSet<bool> _disallowedListExtracted;

	private static readonly object _lock;

	private const string FILEVERSION_STRING = ", FileVersion=";

	private const string KILL_BIT_REGISTRY_HIVE = "HKEY_LOCAL_MACHINE\\";

	private const string KILL_BIT_REGISTRY_LOCATION = "Software\\Microsoft\\.NetFramework\\policy\\APTCA";

	private const string SUBKEY_VALUE = "APTCA_FLAG";

	static AssemblyFilter()
	{
		_lock = new object();
		_disallowedListExtracted = new MS.Internal.SecurityCriticalDataForSet<bool>(value: false);
		_assemblyList = new MS.Internal.SecurityCriticalDataForSet<List<string>>(new List<string>());
	}

	internal void FilterCallback(object sender, AssemblyLoadEventArgs args)
	{
	}

	private string AssemblyNameWithFileVersion(Assembly a)
	{
		StringBuilder stringBuilder = new StringBuilder(a.FullName);
		FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(a.Location);
		if (versionInfo != null && versionInfo.ProductVersion != null)
		{
			stringBuilder.Append(", FileVersion=" + versionInfo.ProductVersion);
		}
		return stringBuilder.ToString().ToLower(CultureInfo.InvariantCulture).Trim();
	}

	private bool AssemblyOnDisallowedList(string assemblyToCheck)
	{
		bool result = false;
		if (!_disallowedListExtracted.Value)
		{
			ExtractDisallowedRegistryList();
			_disallowedListExtracted.Value = true;
		}
		if (_assemblyList.Value.Contains(assemblyToCheck))
		{
			result = true;
		}
		return result;
	}

	private void ExtractDisallowedRegistryList()
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NetFramework\\policy\\APTCA");
		if (registryKey == null)
		{
			return;
		}
		string[] subKeyNames = registryKey.GetSubKeyNames();
		foreach (string text in subKeyNames)
		{
			registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NetFramework\\policy\\APTCA\\" + text);
			object value = registryKey.GetValue("APTCA_FLAG");
			if (value != null && (int)value == 1 && !_assemblyList.Value.Contains(text))
			{
				_assemblyList.Value.Add(text.ToLower(CultureInfo.InvariantCulture).Trim());
			}
		}
	}
}
