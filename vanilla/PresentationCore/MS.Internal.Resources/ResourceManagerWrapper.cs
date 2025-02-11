using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using MS.Internal.PresentationCore;

namespace MS.Internal.Resources;

internal class ResourceManagerWrapper
{
	private ResourceManager _resourceManager;

	private ResourceSet _resourceSet;

	private Assembly _assembly;

	private ArrayList _resourceList;

	private const string LocalizableResourceNameSuffix = ".g";

	private const string UnLocalizableResourceNameSuffix = ".unlocalizable.g";

	internal Assembly Assembly
	{
		get
		{
			return _assembly;
		}
		set
		{
			_assembly = value;
			_resourceManager = null;
			_resourceSet = null;
			_resourceList = null;
		}
	}

	internal IList ResourceList
	{
		get
		{
			if (_resourceList == null)
			{
				_resourceList = new ArrayList();
				if (ResourceManager != null)
				{
					CultureInfo neutralResourcesLanguage = GetNeutralResourcesLanguage();
					ResourceSet resourceSet = ResourceManager.GetResourceSet(neutralResourcesLanguage, createIfNotExists: true, tryParents: false);
					if (resourceSet != null)
					{
						AddResourceNameToList(resourceSet, ref _resourceList);
						resourceSet.Close();
					}
				}
				if (ResourceSet != null)
				{
					AddResourceNameToList(ResourceSet, ref _resourceList);
				}
			}
			return _resourceList;
		}
	}

	private ResourceSet ResourceSet
	{
		get
		{
			if (_resourceSet == null)
			{
				ResourceManager resourceManager = new ResourceManager(SafeSecurityHelper.GetAssemblyPartialName(_assembly) + ".unlocalizable.g", _assembly);
				_resourceSet = resourceManager.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false);
			}
			return _resourceSet;
		}
	}

	private ResourceManager ResourceManager
	{
		get
		{
			if (_resourceManager == null)
			{
				string baseName = SafeSecurityHelper.GetAssemblyPartialName(_assembly) + ".g";
				_resourceManager = new ResourceManager(baseName, _assembly);
			}
			return _resourceManager;
		}
	}

	internal ResourceManagerWrapper(Assembly assembly)
	{
		_assembly = assembly;
	}

	internal Stream GetStream(string name)
	{
		Stream stream = null;
		try
		{
			stream = ResourceManager.GetStream(name, CultureInfo.CurrentUICulture);
		}
		catch (SystemException ex)
		{
			if (!(ex is MissingManifestResourceException) && !(ex is MissingSatelliteAssemblyException))
			{
				throw;
			}
		}
		if (stream == null && ResourceSet != null)
		{
			try
			{
				stream = ResourceSet.GetObject(name) as Stream;
			}
			catch (SystemException ex2)
			{
				if (!(ex2 is MissingManifestResourceException))
				{
					throw;
				}
			}
		}
		return stream;
	}

	private CultureInfo GetNeutralResourcesLanguage()
	{
		CultureInfo result = CultureInfo.InvariantCulture;
		if (Attribute.GetCustomAttribute(_assembly, typeof(NeutralResourcesLanguageAttribute)) is NeutralResourcesLanguageAttribute neutralResourcesLanguageAttribute)
		{
			result = new CultureInfo(neutralResourcesLanguageAttribute.CultureName);
		}
		return result;
	}

	private void AddResourceNameToList(ResourceSet rs, ref ArrayList resourceList)
	{
		IDictionaryEnumerator enumerator = rs.GetEnumerator();
		if (enumerator != null)
		{
			while (enumerator.MoveNext())
			{
				string value = enumerator.Key as string;
				resourceList.Add(value);
			}
		}
	}
}
