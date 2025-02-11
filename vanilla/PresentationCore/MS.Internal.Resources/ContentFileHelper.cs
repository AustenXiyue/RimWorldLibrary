using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Navigation;
using System.Windows.Resources;

namespace MS.Internal.Resources;

internal static class ContentFileHelper
{
	private static HashSet<string> _contentFiles;

	internal static bool IsContentFile(string partName)
	{
		if (_contentFiles == null)
		{
			_contentFiles = GetContentFiles(BaseUriHelper.ResourceAssembly);
		}
		if (_contentFiles != null && _contentFiles.Count > 0 && _contentFiles.Contains(partName))
		{
			return true;
		}
		return false;
	}

	internal static HashSet<string> GetContentFiles(Assembly asm)
	{
		HashSet<string> hashSet = null;
		if (asm == null)
		{
			asm = BaseUriHelper.ResourceAssembly;
			if (asm == null)
			{
				return new HashSet<string>();
			}
		}
		Attribute[] customAttributes = Attribute.GetCustomAttributes(asm, typeof(AssemblyAssociatedContentFileAttribute));
		if (customAttributes != null && customAttributes.Length != 0)
		{
			hashSet = new HashSet<string>(customAttributes.Length, StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				AssemblyAssociatedContentFileAttribute assemblyAssociatedContentFileAttribute = (AssemblyAssociatedContentFileAttribute)customAttributes[i];
				hashSet.Add(assemblyAssociatedContentFileAttribute.RelativeContentFilePath);
			}
		}
		return hashSet;
	}
}
