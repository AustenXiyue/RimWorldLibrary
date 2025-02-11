using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal class FamilyCollection
{
	private static class LegacyArabicFonts
	{
		private static bool _usePrivateFontCollectionIsInitialized;

		private static object _staticLock;

		private static bool _usePrivateFontCollectionForLegacyArabicFonts;

		private static readonly string[] _legacyArabicFonts;

		private static FontCollection _legacyArabicFontCollection;

		internal static FontCollection LegacyArabicFontCollection
		{
			get
			{
				if (_legacyArabicFontCollection == null)
				{
					lock (_staticLock)
					{
						if (_legacyArabicFontCollection == null)
						{
							_legacyArabicFontCollection = DWriteFactory.GetFontCollectionFromFolder(new Uri(SxSFontsResourcePrefix));
						}
					}
				}
				return _legacyArabicFontCollection;
			}
		}

		internal static bool UsePrivateFontCollectionForLegacyArabicFonts
		{
			get
			{
				if (!_usePrivateFontCollectionIsInitialized)
				{
					lock (_staticLock)
					{
						if (!_usePrivateFontCollectionIsInitialized)
						{
							try
							{
								OperatingSystem oSVersion = Environment.OSVersion;
								_usePrivateFontCollectionForLegacyArabicFonts = oSVersion.Version.Major < 6 || (oSVersion.Version.Major == 6 && oSVersion.Version.Minor == 0);
							}
							catch (InvalidOperationException)
							{
								_usePrivateFontCollectionForLegacyArabicFonts = true;
							}
							_usePrivateFontCollectionIsInitialized = true;
						}
					}
				}
				return _usePrivateFontCollectionForLegacyArabicFonts;
			}
		}

		static LegacyArabicFonts()
		{
			_usePrivateFontCollectionIsInitialized = false;
			_staticLock = new object();
			_legacyArabicFonts = new string[4] { "Traditional Arabic", "Andalus", "Simplified Arabic", "Simplified Arabic Fixed" };
		}

		internal static bool IsLegacyArabicFont(string familyName)
		{
			for (int i = 0; i < _legacyArabicFonts.Length; i++)
			{
				if (string.Equals(familyName, _legacyArabicFonts[i], StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}

	private static class SystemCompositeFonts
	{
		internal const int NumOfSystemCompositeFonts = 4;

		private static object _systemCompositeFontsLock;

		private static readonly string[] _systemCompositeFontsNames;

		private static readonly string[] _systemCompositeFontsFileNames;

		private static CompositeFontFamily[] _systemCompositeFonts;

		static SystemCompositeFonts()
		{
			_systemCompositeFontsLock = new object();
			_systemCompositeFontsNames = new string[4] { "Global User Interface", "Global Monospace", "Global Sans Serif", "Global Serif" };
			_systemCompositeFontsFileNames = new string[4] { "GlobalUserInterface", "GlobalMonospace", "GlobalSansSerif", "GlobalSerif" };
			_systemCompositeFonts = new CompositeFontFamily[4];
		}

		internal static CompositeFontFamily GetFallbackFontForArabicLegacyFonts()
		{
			return GetCompositeFontFamilyAtIndex(1);
		}

		internal static CompositeFontFamily FindFamily(string familyName)
		{
			int indexOfFamily = GetIndexOfFamily(familyName);
			if (indexOfFamily >= 0)
			{
				return GetCompositeFontFamilyAtIndex(indexOfFamily);
			}
			return null;
		}

		internal static CompositeFontFamily GetCompositeFontFamilyAtIndex(int index)
		{
			if (_systemCompositeFonts[index] == null)
			{
				lock (_systemCompositeFontsLock)
				{
					if (_systemCompositeFonts[index] == null)
					{
						CompositeFontInfo fontInfo = CompositeFontParser.LoadXml(new FontSource(new Uri(Path.Combine(SxSFontsResourcePrefix, _systemCompositeFontsFileNames[index] + Util.CompositeFontExtension), UriKind.RelativeOrAbsolute), skipDemand: true, isComposite: true, isInternalCompositeFont: true).GetStream());
						_systemCompositeFonts[index] = new CompositeFontFamily(fontInfo);
					}
				}
			}
			return _systemCompositeFonts[index];
		}

		private static int GetIndexOfFamily(string familyName)
		{
			for (int i = 0; i < _systemCompositeFontsNames.Length; i++)
			{
				if (string.Equals(_systemCompositeFontsNames[i], familyName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
			return -1;
		}
	}

	private struct FamilyEnumerator : IEnumerator<MS.Internal.Text.TextInterface.FontFamily>, IEnumerator, IDisposable, IEnumerable<MS.Internal.Text.TextInterface.FontFamily>, IEnumerable
	{
		private uint _familyCount;

		private FontCollection _fontCollection;

		private bool _firstEnumeration;

		private uint _currentFamily;

		MS.Internal.Text.TextInterface.FontFamily IEnumerator<MS.Internal.Text.TextInterface.FontFamily>.Current
		{
			get
			{
				if (_currentFamily < 0 || _currentFamily >= _familyCount)
				{
					throw new InvalidOperationException();
				}
				return _fontCollection[_currentFamily];
			}
		}

		object IEnumerator.Current => ((IEnumerator<MS.Internal.Text.TextInterface.FontFamily>)this).Current;

		internal FamilyEnumerator(FontCollection fontCollection)
		{
			_fontCollection = fontCollection;
			_currentFamily = 0u;
			_firstEnumeration = true;
			_familyCount = fontCollection.FamilyCount;
		}

		public bool MoveNext()
		{
			if (_firstEnumeration)
			{
				_firstEnumeration = false;
			}
			else
			{
				_currentFamily++;
			}
			if (_currentFamily >= _familyCount)
			{
				_currentFamily = _familyCount;
				return false;
			}
			return true;
		}

		public void Reset()
		{
			_currentFamily = 0u;
			_firstEnumeration = true;
		}

		public void Dispose()
		{
		}

		IEnumerator<MS.Internal.Text.TextInterface.FontFamily> IEnumerable<MS.Internal.Text.TextInterface.FontFamily>.GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MS.Internal.Text.TextInterface.FontFamily>)this).GetEnumerator();
		}
	}

	private FontCollection _fontCollection;

	private Uri _folderUri;

	private List<CompositeFontFamily> _userCompositeFonts;

	private static object _staticLock = new object();

	internal static string SxSFontsResourcePrefix { get; } = "/" + Path.GetFileNameWithoutExtension("PresentationCore.dll") + ";component/fonts/";

	private bool UseSystemFonts => _fontCollection == DWriteFactory.SystemFontCollection;

	private IList<CompositeFontFamily> UserCompositeFonts
	{
		get
		{
			if (_userCompositeFonts == null)
			{
				_userCompositeFonts = GetCompositeFontList(new FontSourceCollection(_folderUri, isWindowsFonts: false, tryGetCompositeFontsOnly: true));
			}
			return _userCompositeFonts;
		}
	}

	internal uint FamilyCount => _fontCollection.FamilyCount + (UseSystemFonts ? 4 : checked((uint)UserCompositeFonts.Count));

	private static List<CompositeFontFamily> GetCompositeFontList(FontSourceCollection fontSourceCollection)
	{
		List<CompositeFontFamily> list = new List<CompositeFontFamily>();
		foreach (FontSource item2 in (IEnumerable<IFontSource>)fontSourceCollection)
		{
			if (item2.IsComposite)
			{
				CompositeFontFamily item = new CompositeFontFamily(CompositeFontParser.LoadXml(item2.GetStream()));
				list.Add(item);
			}
		}
		return list;
	}

	private FamilyCollection(Uri folderUri, FontCollection fontCollection)
	{
		_folderUri = folderUri;
		_fontCollection = fontCollection;
	}

	internal static FamilyCollection FromUri(Uri folderUri)
	{
		return new FamilyCollection(folderUri, DWriteFactory.GetFontCollectionFromFolder(folderUri));
	}

	internal static FamilyCollection FromWindowsFonts(Uri folderUri)
	{
		return new FamilyCollection(folderUri, DWriteFactory.SystemFontCollection);
	}

	internal IFontFamily LookupFamily(string familyName, ref System.Windows.FontStyle fontStyle, ref System.Windows.FontWeight fontWeight, ref System.Windows.FontStretch fontStretch)
	{
		if (familyName == null || familyName.Length == 0)
		{
			return null;
		}
		familyName = familyName.Trim();
		if (UseSystemFonts)
		{
			CompositeFontFamily compositeFontFamily = SystemCompositeFonts.FindFamily(familyName);
			if (compositeFontFamily != null)
			{
				return compositeFontFamily;
			}
		}
		MS.Internal.Text.TextInterface.FontFamily fontFamily = _fontCollection[familyName];
		if (fontFamily == null)
		{
			if (!UseSystemFonts)
			{
				CompositeFontFamily compositeFontFamily2 = LookUpUserCompositeFamily(familyName);
				if (compositeFontFamily2 != null)
				{
					return compositeFontFamily2;
				}
			}
			int num = -1;
			string text = familyName;
			do
			{
				num = familyName.LastIndexOf(' ');
				if (num < 0)
				{
					break;
				}
				familyName = familyName.Substring(0, num);
				fontFamily = _fontCollection[familyName];
			}
			while (fontFamily == null);
			if (fontFamily == null)
			{
				return null;
			}
			if (familyName.Length != text.Length)
			{
				int start = familyName.Length + 1;
				ReadOnlySpan<char> faceName = text.AsSpan(start);
				Font fontFromFamily = GetFontFromFamily(fontFamily, faceName);
				if (fontFromFamily != null)
				{
					fontStyle = new System.Windows.FontStyle((int)fontFromFamily.Style);
					fontWeight = new System.Windows.FontWeight((int)fontFromFamily.Weight);
					fontStretch = new System.Windows.FontStretch((int)fontFromFamily.Stretch);
				}
			}
		}
		if (UseSystemFonts && LegacyArabicFonts.UsePrivateFontCollectionForLegacyArabicFonts && LegacyArabicFonts.IsLegacyArabicFont(familyName))
		{
			fontFamily = LegacyArabicFonts.LegacyArabicFontCollection[familyName];
			if (fontFamily == null)
			{
				return SystemCompositeFonts.GetFallbackFontForArabicLegacyFonts();
			}
		}
		return new PhysicalFontFamily(fontFamily);
	}

	private CompositeFontFamily LookUpUserCompositeFamily(string familyName)
	{
		if (UserCompositeFonts != null)
		{
			foreach (CompositeFontFamily userCompositeFont in UserCompositeFonts)
			{
				foreach (KeyValuePair<XmlLanguage, string> familyName2 in userCompositeFont.FamilyNames)
				{
					if (string.Equals(familyName2.Value, familyName, StringComparison.OrdinalIgnoreCase))
					{
						return userCompositeFont;
					}
				}
			}
		}
		return null;
	}

	private static Font GetFontFromFamily(MS.Internal.Text.TextInterface.FontFamily fontFamily, ReadOnlySpan<char> faceName)
	{
		foreach (Font item in fontFamily)
		{
			foreach (KeyValuePair<CultureInfo, string> faceName2 in item.FaceNames)
			{
				if (MemoryExtensions.Equals(faceName, faceName2.Value, StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}
		}
		Dictionary<string, Font> dictionary = new Dictionary<string, Font>(StringComparer.OrdinalIgnoreCase);
		foreach (Font item2 in fontFamily)
		{
			foreach (KeyValuePair<CultureInfo, string> faceName3 in item2.FaceNames)
			{
				dictionary.TryAdd(faceName3.Value, item2);
			}
		}
		Font value = null;
		for (int num = faceName.LastIndexOf(' '); num > 0; num = faceName.LastIndexOf(' '))
		{
			faceName = faceName.Slice(0, num);
			if (dictionary.TryGetValue(faceName.ToString(), out value))
			{
				return value;
			}
		}
		return null;
	}

	private IEnumerable<MS.Internal.Text.TextInterface.FontFamily> GetPhysicalFontFamilies()
	{
		return new FamilyEnumerator(_fontCollection);
	}

	internal System.Windows.Media.FontFamily[] GetFontFamilies(Uri fontFamilyBaseUri, string fontFamilyLocationReference)
	{
		System.Windows.Media.FontFamily[] array = new System.Windows.Media.FontFamily[FamilyCount];
		int num = 0;
		foreach (MS.Internal.Text.TextInterface.FontFamily physicalFontFamily in GetPhysicalFontFamilies())
		{
			string familyName = Util.ConvertFontFamilyReferenceToFriendlyName(Util.ConvertFamilyNameAndLocationToFontFamilyReference(physicalFontFamily.OrdinalName, fontFamilyLocationReference));
			array[num++] = new System.Windows.Media.FontFamily(fontFamilyBaseUri, familyName);
		}
		if (UseSystemFonts)
		{
			for (int i = 0; i < 4; i++)
			{
				System.Windows.Media.FontFamily fontFamily = CreateFontFamily(SystemCompositeFonts.GetCompositeFontFamilyAtIndex(i), fontFamilyBaseUri, fontFamilyLocationReference);
				if (fontFamily != null)
				{
					array[num++] = fontFamily;
				}
			}
		}
		else
		{
			foreach (CompositeFontFamily userCompositeFont in UserCompositeFonts)
			{
				System.Windows.Media.FontFamily fontFamily = CreateFontFamily(userCompositeFont, fontFamilyBaseUri, fontFamilyLocationReference);
				if (fontFamily != null)
				{
					array[num++] = fontFamily;
				}
			}
		}
		return array;
	}

	private System.Windows.Media.FontFamily CreateFontFamily(CompositeFontFamily compositeFontFamily, Uri fontFamilyBaseUri, string fontFamilyLocationReference)
	{
		IEnumerator<string> enumerator = ((IFontFamily)compositeFontFamily).Names.Values.GetEnumerator();
		if (enumerator.MoveNext())
		{
			string familyName = Util.ConvertFontFamilyReferenceToFriendlyName(Util.ConvertFamilyNameAndLocationToFontFamilyReference(enumerator.Current, fontFamilyLocationReference));
			return new System.Windows.Media.FontFamily(fontFamilyBaseUri, familyName);
		}
		return null;
	}
}
