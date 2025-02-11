using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

internal class FontSourceCollection : IFontSourceCollection, IEnumerable<IFontSource>, IEnumerable
{
	private Uri _uri;

	private bool _isWindowsFonts;

	private bool _isFileSystemFolder;

	private volatile IList<IFontSource> _fontSources;

	private bool _tryGetCompositeFontsOnly;

	private const string InstalledWindowsFontsRegistryKey = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts";

	private const string InstalledWindowsFontsRegistryKeyFullPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts";

	public FontSourceCollection(Uri folderUri, bool isWindowsFonts)
	{
		Initialize(folderUri, isWindowsFonts, tryGetCompositeFontsOnly: false);
	}

	public FontSourceCollection(Uri folderUri, bool isWindowsFonts, bool tryGetCompositeFontsOnly)
	{
		Initialize(folderUri, isWindowsFonts, tryGetCompositeFontsOnly);
	}

	private void Initialize(Uri folderUri, bool isWindowsFonts, bool tryGetCompositeFontsOnly)
	{
		_uri = folderUri;
		_isWindowsFonts = isWindowsFonts;
		_tryGetCompositeFontsOnly = tryGetCompositeFontsOnly;
		bool isComposite = false;
		if (Util.IsSupportedFontExtension(Util.GetUriExtension(_uri), out isComposite) || !Util.IsEnumerableFontUriScheme(_uri))
		{
			_fontSources = new List<IFontSource>(1);
			_fontSources.Add(new FontSource(_uri, skipDemand: false, isComposite));
		}
		else
		{
			InitializeDirectoryProperties();
		}
	}

	private void InitializeDirectoryProperties()
	{
		_isFileSystemFolder = false;
		if (!_uri.IsFile)
		{
			return;
		}
		if (_isWindowsFonts)
		{
			if ((object)_uri == Util.WindowsFontsUriObject)
			{
				_isFileSystemFolder = true;
			}
			else
			{
				_isFileSystemFolder = false;
			}
		}
		else
		{
			string localPath = _uri.LocalPath;
			_isFileSystemFolder = localPath[localPath.Length - 1] == Path.DirectorySeparatorChar;
		}
	}

	private void SetFontSources()
	{
		if (_fontSources != null)
		{
			return;
		}
		lock (this)
		{
			List<IFontSource> list;
			if (_uri.IsFile)
			{
				bool flag = false;
				ICollection<string> collection;
				if (_isFileSystemFolder)
				{
					if (_isWindowsFonts)
					{
						if (_tryGetCompositeFontsOnly)
						{
							collection = Directory.GetFiles(_uri.LocalPath, "*" + Util.CompositeFontExtension);
							flag = true;
						}
						else
						{
							HashSet<string> hashSet = new HashSet<string>(512, StringComparer.OrdinalIgnoreCase);
							using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts"))
							{
								Invariant.Assert(registryKey != null);
								string[] valueNames = registryKey.GetValueNames();
								foreach (string name in valueNames)
								{
									string text = registryKey.GetValue(name) as string;
									if (text != null)
									{
										if (Path.GetFileName(text) == text)
										{
											text = Path.Combine(Util.WindowsFontsLocalPath, text);
										}
										hashSet.Add(text);
									}
								}
							}
							hashSet.UnionWith(Directory.EnumerateFiles(_uri.LocalPath));
							collection = hashSet;
						}
					}
					else if (_tryGetCompositeFontsOnly)
					{
						collection = Directory.GetFiles(_uri.LocalPath, "*" + Util.CompositeFontExtension);
						flag = true;
					}
					else
					{
						collection = Directory.GetFiles(_uri.LocalPath);
					}
				}
				else
				{
					collection = new string[1] { _uri.LocalPath };
				}
				list = new List<IFontSource>(collection.Count);
				if (flag)
				{
					foreach (string item in collection)
					{
						list.Add(new FontSource(new Uri(item, UriKind.Absolute), _isWindowsFonts, isComposite: true));
					}
				}
				else
				{
					foreach (string item2 in collection)
					{
						if (Util.IsSupportedFontExtension(Path.GetExtension(item2), out var isComposite))
						{
							list.Add(new FontSource(new Uri(item2, UriKind.Absolute), _isWindowsFonts, isComposite));
						}
					}
				}
			}
			else
			{
				List<string> list2 = FontResourceCache.LookupFolder(_uri);
				if (list2 == null)
				{
					list = new List<IFontSource>(0);
				}
				else
				{
					bool flag2 = false;
					list = new List<IFontSource>(list2.Count);
					foreach (string item3 in list2)
					{
						if (string.IsNullOrEmpty(item3))
						{
							flag2 = Util.IsCompositeFont(Path.GetExtension(_uri.AbsoluteUri));
							list.Add(new FontSource(_uri, _isWindowsFonts, flag2));
						}
						else
						{
							flag2 = Util.IsCompositeFont(Path.GetExtension(item3));
							list.Add(new FontSource(new Uri(_uri, item3), _isWindowsFonts, flag2));
						}
					}
				}
			}
			_fontSources = list;
		}
	}

	IEnumerator<IFontSource> IEnumerable<IFontSource>.GetEnumerator()
	{
		SetFontSources();
		return _fontSources.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		SetFontSources();
		return _fontSources.GetEnumerator();
	}
}
