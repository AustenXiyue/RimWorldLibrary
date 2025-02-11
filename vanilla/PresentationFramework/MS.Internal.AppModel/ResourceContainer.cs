using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using MS.Internal.Resources;

namespace MS.Internal.AppModel;

internal class ResourceContainer : Package
{
	internal const string XamlExt = ".xaml";

	internal const string BamlExt = ".baml";

	private static Dictionary<string, ResourceManagerWrapper> _registeredResourceManagers = new Dictionary<string, ResourceManagerWrapper>();

	private static ResourceManagerWrapper _applicationResourceManagerWrapper = null;

	private static FileShare _fileShare = FileShare.Read;

	private static bool assemblyLoadhandlerAttached = false;

	internal static ResourceManagerWrapper ApplicationResourceManagerWrapper
	{
		get
		{
			if (_applicationResourceManagerWrapper == null)
			{
				Assembly resourceAssembly = Application.ResourceAssembly;
				if (resourceAssembly != null)
				{
					_applicationResourceManagerWrapper = new ResourceManagerWrapper(resourceAssembly);
				}
			}
			return _applicationResourceManagerWrapper;
		}
	}

	internal static FileShare FileShare => _fileShare;

	internal ResourceContainer()
		: base(FileAccess.Read)
	{
	}

	public override bool PartExists(Uri uri)
	{
		return true;
	}

	protected override PackagePart GetPartCore(Uri uri)
	{
		if (!assemblyLoadhandlerAttached)
		{
			AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoadEventHandler;
			assemblyLoadhandlerAttached = true;
		}
		bool isContentFile;
		string partName;
		ResourceManagerWrapper resourceManagerWrapper = GetResourceManagerWrapper(uri, out partName, out isContentFile);
		if (isContentFile)
		{
			return new ContentFilePart(this, uri);
		}
		partName = ResourceIDHelper.GetResourceIDFromRelativePath(partName);
		return new ResourcePart(this, uri, partName, resourceManagerWrapper);
	}

	private void OnAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
	{
		Assembly loadedAssembly = args.LoadedAssembly;
		if (!loadedAssembly.ReflectionOnly)
		{
			AssemblyName assemblyName = new AssemblyName(loadedAssembly.FullName);
			string text = assemblyName.Name.ToLowerInvariant();
			string text2 = string.Empty;
			string text3 = text;
			UpdateCachedRMW(text3, args.LoadedAssembly);
			string text4 = assemblyName.Version.ToString();
			if (!string.IsNullOrEmpty(text4))
			{
				text3 += text4;
				UpdateCachedRMW(text3, args.LoadedAssembly);
			}
			byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
			for (int i = 0; i < publicKeyToken.Length; i++)
			{
				text2 += publicKeyToken[i].ToString("x", NumberFormatInfo.InvariantInfo);
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text3 += text2;
				UpdateCachedRMW(text3, args.LoadedAssembly);
				text3 = text + text2;
				UpdateCachedRMW(text3, args.LoadedAssembly);
			}
		}
	}

	private void UpdateCachedRMW(string key, Assembly assembly)
	{
		if (_registeredResourceManagers.ContainsKey(key))
		{
			_registeredResourceManagers[key].Assembly = assembly;
		}
	}

	private ResourceManagerWrapper GetResourceManagerWrapper(Uri uri, out string partName, out bool isContentFile)
	{
		ResourceManagerWrapper value = ApplicationResourceManagerWrapper;
		isContentFile = false;
		BaseUriHelper.GetAssemblyNameAndPart(uri, out partName, out var assemblyName, out var assemblyVersion, out var assemblyKey);
		if (!string.IsNullOrEmpty(assemblyName))
		{
			string text = assemblyName + assemblyVersion + assemblyKey;
			_registeredResourceManagers.TryGetValue(text.ToLowerInvariant(), out value);
			if (value == null)
			{
				Assembly loadedAssembly = BaseUriHelper.GetLoadedAssembly(assemblyName, assemblyVersion, assemblyKey);
				value = ((!loadedAssembly.Equals(Application.ResourceAssembly)) ? new ResourceManagerWrapper(loadedAssembly) : ApplicationResourceManagerWrapper);
				_registeredResourceManagers[text.ToLowerInvariant()] = value;
			}
		}
		if (value == ApplicationResourceManagerWrapper)
		{
			if (value == null)
			{
				throw new IOException(SR.EntryAssemblyIsNull);
			}
			if (ContentFileHelper.IsContentFile(partName))
			{
				isContentFile = true;
				value = null;
			}
		}
		return value;
	}

	protected override PackagePart CreatePartCore(Uri uri, string contentType, CompressionOption compressionOption)
	{
		return null;
	}

	protected override void DeletePartCore(Uri uri)
	{
		throw new NotSupportedException();
	}

	protected override PackagePart[] GetPartsCore()
	{
		throw new NotSupportedException();
	}

	protected override void FlushCore()
	{
		throw new NotSupportedException();
	}
}
