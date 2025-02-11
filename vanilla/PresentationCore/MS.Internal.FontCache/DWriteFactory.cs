using System;
using System.IO;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal static class DWriteFactory
{
	private static Factory _factory;

	private static FontCollection _systemFontCollection;

	private static object _systemFontCollectionLock;

	internal static Factory Instance => _factory;

	internal static FontCollection SystemFontCollection
	{
		get
		{
			if (_systemFontCollection == null)
			{
				lock (_systemFontCollectionLock)
				{
					if (_systemFontCollection == null)
					{
						_systemFontCollection = Instance.GetSystemFontCollection();
					}
				}
			}
			return _systemFontCollection;
		}
	}

	static DWriteFactory()
	{
		_systemFontCollection = null;
		_systemFontCollectionLock = new object();
		_factory = Factory.Create(FactoryType.Shared, new FontSourceCollectionFactory(), new FontSourceFactory());
		LocalizedErrorMsgs.EnumeratorNotStarted = SR.Enumerator_NotStarted;
		LocalizedErrorMsgs.EnumeratorReachedEnd = SR.Enumerator_ReachedEnd;
	}

	private static FontCollection GetFontCollectionFromFileOrFolder(Uri fontCollectionUri, bool isFolder)
	{
		if (Factory.IsLocalUri(fontCollectionUri))
		{
			string text = (isFolder ? fontCollectionUri.LocalPath : (Directory.GetParent(fontCollectionUri.LocalPath).FullName + Path.DirectorySeparatorChar));
			if (string.Equals((text.Length > 0 && text[text.Length - 1] != Path.DirectorySeparatorChar) ? (text + Path.DirectorySeparatorChar) : text, Util.WindowsFontsUriObject.LocalPath, StringComparison.OrdinalIgnoreCase))
			{
				return SystemFontCollection;
			}
			return Instance.GetFontCollection(new Uri(text));
		}
		return Instance.GetFontCollection(fontCollectionUri);
	}

	internal static FontCollection GetFontCollectionFromFolder(Uri fontCollectionUri)
	{
		return GetFontCollectionFromFileOrFolder(fontCollectionUri, isFolder: true);
	}

	internal static FontCollection GetFontCollectionFromFile(Uri fontCollectionUri)
	{
		return GetFontCollectionFromFileOrFolder(fontCollectionUri, isFolder: false);
	}
}
