using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace MS.Internal.PresentationFramework;

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

	internal const string IMAGE = "image";

	internal static string GetAssemblyPartialName(Assembly assembly)
	{
		string name = new AssemblyName(assembly.FullName).Name;
		if (name == null)
		{
			return string.Empty;
		}
		return name;
	}

	internal static string GetFullAssemblyNameFromPartialName(Assembly protoAssembly, string partialName)
	{
		return new AssemblyName(protoAssembly.FullName)
		{
			Name = partialName
		}.FullName;
	}

	internal static Point ClientToScreen(UIElement relativeTo, Point point)
	{
		PresentationSource presentationSource = PresentationSource.CriticalFromVisual(relativeTo);
		if (presentationSource == null)
		{
			return new Point(double.NaN, double.NaN);
		}
		relativeTo.TransformToAncestor(presentationSource.RootVisual).TryTransform(point, out var result);
		return PointUtil.ClientToScreen(PointUtil.RootToClient(result, presentationSource), presentationSource);
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

	internal static bool IsConnectedToPresentationSource(Visual visual)
	{
		return PresentationSource.CriticalFromVisual(visual) != null;
	}
}
