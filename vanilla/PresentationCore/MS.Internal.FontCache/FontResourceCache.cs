using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Navigation;
using MS.Internal.Resources;

namespace MS.Internal.FontCache;

internal static class FontResourceCache
{
	private const string FakeFileName = "X";

	private static Dictionary<Assembly, Dictionary<string, List<string>>> _assemblyCaches = new Dictionary<Assembly, Dictionary<string, List<string>>>(1);

	private static void ConstructFontResourceCache(Assembly entryAssembly, Dictionary<string, List<string>> folderResourceMap)
	{
		HashSet<string> contentFiles = ContentFileHelper.GetContentFiles(entryAssembly);
		if (contentFiles != null)
		{
			foreach (string item in contentFiles)
			{
				AddResourceToFolderMap(folderResourceMap, item);
			}
		}
		IList resourceList = new ResourceManagerWrapper(entryAssembly).ResourceList;
		if (resourceList == null)
		{
			return;
		}
		foreach (string item2 in resourceList)
		{
			AddResourceToFolderMap(folderResourceMap, item2);
		}
	}

	internal static List<string> LookupFolder(Uri uri)
	{
		bool flag = IsFolderUri(uri);
		if (flag)
		{
			uri = new Uri(uri, "X");
		}
		BaseUriHelper.GetAssemblyAndPartNameFromPackAppUri(uri, out var assembly, out var partName);
		if (assembly == null)
		{
			return null;
		}
		if (flag)
		{
			partName = partName.Substring(0, partName.Length - "X".Length);
		}
		Dictionary<string, List<string>> value;
		lock (_assemblyCaches)
		{
			if (!_assemblyCaches.TryGetValue(assembly, out value))
			{
				value = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
				ConstructFontResourceCache(assembly, value);
				_assemblyCaches.Add(assembly, value);
			}
		}
		value.TryGetValue(partName, out var value2);
		return value2;
	}

	private static bool IsFolderUri(Uri uri)
	{
		string components = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
		if (components.Length != 0)
		{
			return components[components.Length - 1] == '/';
		}
		return true;
	}

	private static void AddResourceToFolderMap(Dictionary<string, List<string>> folderResourceMap, string resourceFullName)
	{
		int num = resourceFullName.LastIndexOf('/');
		string key;
		string text;
		if (num == -1)
		{
			key = string.Empty;
			text = resourceFullName;
		}
		else
		{
			key = resourceFullName.Substring(0, num + 1);
			text = resourceFullName.Substring(num + 1);
		}
		if (Util.IsSupportedFontExtension(Path.GetExtension(text), out var _))
		{
			if (!folderResourceMap.ContainsKey(key))
			{
				folderResourceMap[key] = new List<string>(1);
			}
			folderResourceMap[key].Add(text);
			folderResourceMap[resourceFullName] = new List<string>(1);
			folderResourceMap[resourceFullName].Add(string.Empty);
		}
	}
}
