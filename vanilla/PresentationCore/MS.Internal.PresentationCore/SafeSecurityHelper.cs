using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Win32;
using MS.Internal.WindowsBase;
using MS.Win32;

namespace MS.Internal.PresentationCore;

internal static class SafeSecurityHelper
{
	internal enum KeyToRead
	{
		WebBrowserDisable = 1,
		MediaAudioDisable = 2,
		MediaVideoDisable = 4,
		MediaImageDisable = 8,
		MediaAudioOrVideoDisable = 6,
		ScriptInteropDisable = 16
	}

	private static Dictionary<object, AssemblyName> _assemblies;

	private static object syncObject = new object();

	private static bool _isGCCallbackPending;

	private static readonly WaitCallback _cleanupCollectedAssemblies = CleanupCollectedAssemblies;

	internal const string IMAGE = "image";

	internal static void TransformLocalRectToScreen(HandleRef hwnd, ref MS.Win32.NativeMethods.RECT rcWindowCoords)
	{
		int num = NativeMethodsSetLastError.MapWindowPoints(hwnd, new HandleRef(null, IntPtr.Zero), ref rcWindowCoords, 2);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == 0 && lastWin32Error != 0)
		{
			throw new Win32Exception(lastWin32Error);
		}
	}

	internal static string GetAssemblyPartialName(Assembly assembly)
	{
		string name = new AssemblyName(assembly.FullName).Name;
		if (name == null)
		{
			return string.Empty;
		}
		return name;
	}

	internal static Assembly GetLoadedAssembly(AssemblyName assemblyName)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Version version = assemblyName.Version;
		CultureInfo cultureInfo = assemblyName.CultureInfo;
		byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
		for (int num = assemblies.Length - 1; num >= 0; num--)
		{
			AssemblyName assemblyName2 = GetAssemblyName(assemblies[num]);
			Version version2 = assemblyName2.Version;
			CultureInfo cultureInfo2 = assemblyName2.CultureInfo;
			byte[] publicKeyToken2 = assemblyName2.GetPublicKeyToken();
			if (string.Compare(assemblyName2.Name, assemblyName.Name, ignoreCase: true, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS) == 0 && (version == null || version.Equals(version2)) && (cultureInfo == null || cultureInfo.Equals(cultureInfo2)) && (publicKeyToken == null || IsSameKeyToken(publicKeyToken, publicKeyToken2)))
			{
				return assemblies[num];
			}
		}
		return null;
	}

	private static AssemblyName GetAssemblyName(Assembly assembly)
	{
		object key = (assembly.IsDynamic ? ((ISerializable)new WeakRefKey(assembly)) : ((ISerializable)assembly));
		lock (syncObject)
		{
			AssemblyName value;
			if (_assemblies == null)
			{
				_assemblies = new Dictionary<object, AssemblyName>();
			}
			else if (_assemblies.TryGetValue(key, out value))
			{
				return value;
			}
			value = new AssemblyName(assembly.FullName);
			_assemblies.Add(key, value);
			if (assembly.IsDynamic && !_isGCCallbackPending)
			{
				GCNotificationToken.RegisterCallback(_cleanupCollectedAssemblies, null);
				_isGCCallbackPending = true;
			}
			return value;
		}
	}

	private static void CleanupCollectedAssemblies(object state)
	{
		bool flag = false;
		List<object> list = null;
		lock (syncObject)
		{
			foreach (object key in _assemblies.Keys)
			{
				if (!(key is WeakReference weakReference))
				{
					continue;
				}
				if (weakReference.IsAlive)
				{
					flag = true;
					continue;
				}
				if (list == null)
				{
					list = new List<object>();
				}
				list.Add(key);
			}
			if (list != null)
			{
				foreach (object item in list)
				{
					_assemblies.Remove(item);
				}
			}
			if (flag)
			{
				GCNotificationToken.RegisterCallback(_cleanupCollectedAssemblies, null);
			}
			else
			{
				_isGCCallbackPending = false;
			}
		}
	}

	internal static bool IsSameKeyToken(byte[] reqKeyToken, byte[] curKeyToken)
	{
		bool result = false;
		if (reqKeyToken == null && curKeyToken == null)
		{
			result = true;
		}
		else if (reqKeyToken != null && curKeyToken != null && reqKeyToken.Length == curKeyToken.Length)
		{
			result = true;
			for (int i = 0; i < reqKeyToken.Length; i++)
			{
				if (reqKeyToken[i] != curKeyToken[i])
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	internal static bool IsFeatureDisabled(KeyToRead key)
	{
		string text = null;
		bool flag = false;
		text = key switch
		{
			KeyToRead.WebBrowserDisable => "WebBrowserDisallow", 
			KeyToRead.MediaAudioDisable => "MediaAudioDisallow", 
			KeyToRead.MediaVideoDisable => "MediaVideoDisallow", 
			KeyToRead.MediaImageDisable => "MediaImageDisallow", 
			KeyToRead.MediaAudioOrVideoDisable => "MediaAudioDisallow", 
			KeyToRead.ScriptInteropDisable => "ScriptInteropDisallow", 
			_ => throw new ArgumentException(key.ToString()), 
		};
		object obj = null;
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NETFramework\\Windows Presentation Foundation\\Features");
		if (registryKey != null)
		{
			obj = registryKey.GetValue(text);
			if (obj is int && (int)obj == 1)
			{
				flag = true;
			}
			if (!flag && key == KeyToRead.MediaAudioOrVideoDisable)
			{
				text = "MediaVideoDisallow";
				obj = registryKey.GetValue(text);
				if (obj is int && (int)obj == 1)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	internal static CultureInfo GetCultureInfoByIetfLanguageTag(string languageTag)
	{
		return CultureInfo.GetCultureInfoByIetfLanguageTag(languageTag);
	}

	internal static bool IsConnectedToPresentationSource(Visual visual)
	{
		return PresentationSource.CriticalFromVisual(visual) != null;
	}
}
