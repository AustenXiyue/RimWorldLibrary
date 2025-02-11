using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;
using MS.Internal.Shaping;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal sealed class FontFaceLayoutInfo
{
	private static class Os2EmbeddingFlags
	{
		public const ushort RestrictedLicense = 2;

		public const ushort PreviewAndPrint = 4;

		public const ushort Editable = 8;

		public const ushort InstallableMask = 14;

		public const ushort NoSubsetting = 256;

		public const ushort BitmapOnly = 512;
	}

	internal sealed class IntMap : IDictionary<int, ushort>, ICollection<KeyValuePair<int, ushort>>, IEnumerable<KeyValuePair<int, ushort>>, IEnumerable
	{
		private Font _font;

		private Dictionary<int, ushort> _cmap;

		private Dictionary<int, ushort> CMap
		{
			get
			{
				if (_cmap == null)
				{
					lock (this)
					{
						if (_cmap == null)
						{
							_cmap = new Dictionary<int, ushort>();
							for (int i = 0; i <= 1114111; i++)
							{
								if (TryGetValue(i, out var value))
								{
									_cmap.Add(i, value);
								}
							}
						}
					}
				}
				return _cmap;
			}
		}

		public ICollection<int> Keys => CMap.Keys;

		public ICollection<ushort> Values => CMap.Values;

		ushort IDictionary<int, ushort>.this[int i]
		{
			get
			{
				if (!TryGetValue(i, out var value))
				{
					throw new KeyNotFoundException();
				}
				return value;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count => CMap.Count;

		public bool IsReadOnly => true;

		internal IntMap(Font font)
		{
			_font = font;
			_cmap = null;
		}

		public void Add(int key, ushort value)
		{
			throw new NotSupportedException();
		}

		public bool ContainsKey(int key)
		{
			return _font.HasCharacter(checked((uint)key));
		}

		public bool Remove(int key)
		{
			throw new NotSupportedException();
		}

		public unsafe bool TryGetValue(int key, out ushort value)
		{
			uint num = checked((uint)key);
			uint* pCodePoints = &num;
			MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
			ushort num2 = default(ushort);
			try
			{
				fontFace.GetArrayOfGlyphIndices(pCodePoints, 1u, &num2);
			}
			finally
			{
				fontFace.Release();
			}
			value = num2;
			return value != 0;
		}

		internal unsafe void TryGetValues(uint* pKeys, uint characterCount, ushort* pIndices)
		{
			MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
			try
			{
				fontFace.GetArrayOfGlyphIndices(pKeys, characterCount, pIndices);
			}
			finally
			{
				fontFace.Release();
			}
		}

		public void Add(KeyValuePair<int, ushort> item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(KeyValuePair<int, ushort> item)
		{
			return ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<int, ushort>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (array.Rank != 1)
			{
				throw new ArgumentException(SR.Collection_BadRank);
			}
			if (arrayIndex < 0 || arrayIndex >= array.Length || arrayIndex + Count > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			using IEnumerator<KeyValuePair<int, ushort>> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, ushort> current = enumerator.Current;
				array[arrayIndex++] = current;
			}
		}

		public bool Remove(KeyValuePair<int, ushort> item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<KeyValuePair<int, ushort>> GetEnumerator()
		{
			return CMap.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<int, ushort>>)this).GetEnumerator();
		}
	}

	private FontTechnology _fontTechnology;

	private TypographyAvailabilities _typographyAvailabilities;

	private FontEmbeddingRight _embeddingRights;

	private bool _embeddingRightsInitialized;

	private bool _gsubInitialized;

	private bool _gposInitialized;

	private bool _gdefInitialized;

	private bool _fontTechnologyInitialized;

	private bool _typographyAvailabilitiesInitialized;

	private byte[] _gsubCache;

	private byte[] _gposCache;

	private byte[] _gsub;

	private byte[] _gpos;

	private byte[] _gdef;

	private Font _font;

	private ushort _blankGlyphIndex;

	private IntMap _cmap;

	private static readonly uint[] LoclFeature = new uint[1] { 1819239276u };

	private static readonly uint[] RequiredTypographyFeatures = new uint[8] { 1667460464u, 1919707495u, 1818847073u, 1668049255u, 1667329140u, 1801810542u, 1835102827u, 1835756907u };

	private static readonly uint[] RequiredFeatures = new uint[9] { 1819239276u, 1667460464u, 1919707495u, 1818847073u, 1668049255u, 1667329140u, 1801810542u, 1835102827u, 1835756907u };

	private static readonly UnicodeRange[] fastTextRanges = new UnicodeRange[8]
	{
		new UnicodeRange(32, 126),
		new UnicodeRange(161, 255),
		new UnicodeRange(256, 383),
		new UnicodeRange(384, 591),
		new UnicodeRange(7680, 7935),
		new UnicodeRange(12352, 12440),
		new UnicodeRange(12443, 12447),
		new UnicodeRange(12448, 12543)
	};

	internal IntMap CharacterMap => _cmap;

	internal ushort BlankGlyph => _blankGlyphIndex;

	internal ushort DesignEmHeight => _font.Metrics.DesignUnitsPerEm;

	internal FontEmbeddingRight EmbeddingRights
	{
		get
		{
			if (!_embeddingRightsInitialized)
			{
				FontEmbeddingRight embeddingRights = FontEmbeddingRight.RestrictedLicense;
				MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
				bool flag;
				ushort fsType;
				try
				{
					flag = fontFace.ReadFontEmbeddingRights(out fsType);
				}
				finally
				{
					fontFace.Release();
				}
				if (flag)
				{
					if ((fsType & 0xE) == 0)
					{
						switch (fsType & 0x300)
						{
						case 0:
							embeddingRights = FontEmbeddingRight.Installable;
							break;
						case 256:
							embeddingRights = FontEmbeddingRight.InstallableButNoSubsetting;
							break;
						case 512:
							embeddingRights = FontEmbeddingRight.InstallableButWithBitmapsOnly;
							break;
						case 768:
							embeddingRights = FontEmbeddingRight.InstallableButNoSubsettingAndWithBitmapsOnly;
							break;
						}
					}
					else if ((fsType & 8) != 0)
					{
						switch (fsType & 0x300)
						{
						case 0:
							embeddingRights = FontEmbeddingRight.Editable;
							break;
						case 256:
							embeddingRights = FontEmbeddingRight.EditableButNoSubsetting;
							break;
						case 512:
							embeddingRights = FontEmbeddingRight.EditableButWithBitmapsOnly;
							break;
						case 768:
							embeddingRights = FontEmbeddingRight.EditableButNoSubsettingAndWithBitmapsOnly;
							break;
						}
					}
					else if ((fsType & 4) != 0)
					{
						switch (fsType & 0x300)
						{
						case 0:
							embeddingRights = FontEmbeddingRight.PreviewAndPrint;
							break;
						case 256:
							embeddingRights = FontEmbeddingRight.PreviewAndPrintButNoSubsetting;
							break;
						case 512:
							embeddingRights = FontEmbeddingRight.PreviewAndPrintButWithBitmapsOnly;
							break;
						case 768:
							embeddingRights = FontEmbeddingRight.PreviewAndPrintButNoSubsettingAndWithBitmapsOnly;
							break;
						}
					}
				}
				_embeddingRights = embeddingRights;
				_embeddingRightsInitialized = true;
			}
			return _embeddingRights;
		}
	}

	internal FontTechnology FontTechnology
	{
		get
		{
			if (!_fontTechnologyInitialized)
			{
				ComputeFontTechnology();
				_fontTechnologyInitialized = true;
			}
			return _fontTechnology;
		}
	}

	internal TypographyAvailabilities TypographyAvailabilities
	{
		get
		{
			if (!_typographyAvailabilitiesInitialized)
			{
				ComputeTypographyAvailabilities();
				_typographyAvailabilitiesInitialized = true;
			}
			return _typographyAvailabilities;
		}
	}

	internal ushort GlyphCount
	{
		get
		{
			MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
			try
			{
				return fontFace.GlyphCount;
			}
			finally
			{
				fontFace.Release();
			}
		}
	}

	internal FontFaceLayoutInfo(Font font)
	{
		_fontTechnologyInitialized = false;
		_typographyAvailabilitiesInitialized = false;
		_gsubInitialized = false;
		_gposInitialized = false;
		_gdefInitialized = false;
		_embeddingRightsInitialized = false;
		_gsubCache = null;
		_gposCache = null;
		_gsub = null;
		_gpos = null;
		_gdef = null;
		_font = font;
		_cmap = new IntMap(_font);
		_cmap.TryGetValue(32, out _blankGlyphIndex);
	}

	private byte[] GetFontTable(OpenTypeTableTag openTypeTableTag)
	{
		MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
		byte[] tableData;
		try
		{
			if (!fontFace.TryGetFontTable(openTypeTableTag, out tableData))
			{
				tableData = null;
				return tableData;
			}
		}
		finally
		{
			fontFace.Release();
		}
		return tableData;
	}

	internal byte[] Gsub()
	{
		if (!_gsubInitialized)
		{
			_gsub = GetFontTable(OpenTypeTableTag.TTO_GSUB);
			_gsubInitialized = true;
		}
		return _gsub;
	}

	internal byte[] Gpos()
	{
		if (!_gposInitialized)
		{
			_gpos = GetFontTable(OpenTypeTableTag.TTO_GPOS);
			_gposInitialized = true;
		}
		return _gpos;
	}

	internal byte[] Gdef()
	{
		if (!_gdefInitialized)
		{
			_gdef = GetFontTable(OpenTypeTableTag.TTO_GDEF);
			_gdefInitialized = true;
		}
		return _gdef;
	}

	internal byte[] GetTableCache(OpenTypeTags tableTag)
	{
		switch (tableTag)
		{
		case OpenTypeTags.GSUB:
			if (Gsub() != null)
			{
				return _gsubCache;
			}
			break;
		case OpenTypeTags.GPOS:
			if (Gpos() != null)
			{
				return _gposCache;
			}
			break;
		default:
			throw new NotSupportedException();
		}
		return null;
	}

	internal byte[] AllocateTableCache(OpenTypeTags tableTag, int size)
	{
		switch (tableTag)
		{
		case OpenTypeTags.GSUB:
			_gsubCache = new byte[size];
			return _gsubCache;
		case OpenTypeTags.GPOS:
			_gposCache = new byte[size];
			return _gposCache;
		default:
			throw new NotSupportedException();
		}
	}

	private void ComputeFontTechnology()
	{
		MS.Internal.Text.TextInterface.FontFace fontFace = _font.GetFontFace();
		try
		{
			if (fontFace.Type == FontFaceType.TrueTypeCollection)
			{
				_fontTechnology = FontTechnology.TrueTypeCollection;
			}
			else if (fontFace.Type == FontFaceType.CFF)
			{
				_fontTechnology = FontTechnology.PostscriptOpenType;
			}
			else
			{
				_fontTechnology = FontTechnology.TrueType;
			}
		}
		finally
		{
			fontFace.Release();
		}
	}

	private unsafe void ComputeTypographyAvailabilities()
	{
		int num = GlyphCount + 31 >> 5;
		uint[] uInts = BufferCache.GetUInts(num);
		Array.Clear(uInts, 0, num);
		ushort num2 = ushort.MaxValue;
		ushort num3 = 0;
		TypographyAvailabilities typographyAvailabilities = TypographyAvailabilities.None;
		GsubGposTables font = new GsubGposTables(this);
		for (int i = 0; i < fastTextRanges.Length; i++)
		{
			uint[] fullRange = fastTextRanges[i].GetFullRange();
			ushort[] uShorts = BufferCache.GetUShorts(fullRange.Length);
			fixed (uint* pKeys = &fullRange[0])
			{
				fixed (ushort* pIndices = &uShorts[0])
				{
					CharacterMap.TryGetValues(pKeys, checked((uint)fullRange.Length), pIndices);
				}
			}
			for (int j = 0; j < fullRange.Length; j++)
			{
				ushort num4 = uShorts[j];
				if (num4 != 0)
				{
					uInts[num4 >> 5] |= (uint)(1 << num4 % 32);
					if (num4 > num3)
					{
						num3 = num4;
					}
					if (num4 < num2)
					{
						num2 = num4;
					}
				}
			}
			BufferCache.ReleaseUShorts(uShorts);
		}
		if (OpenTypeLayout.GetComplexLanguageList(font, LoclFeature, uInts, num2, num3, out var complexLanguages) != 0)
		{
			_typographyAvailabilities = TypographyAvailabilities.None;
			return;
		}
		if (complexLanguages != null)
		{
			TypographyAvailabilities typographyAvailabilities2 = TypographyAvailabilities.FastTextMajorLanguageLocalizedFormAvailable | TypographyAvailabilities.FastTextExtraLanguageLocalizedFormAvailable;
			for (int k = 0; k < complexLanguages.Length; k++)
			{
				if (typographyAvailabilities == typographyAvailabilities2)
				{
					break;
				}
				typographyAvailabilities = ((!MajorLanguages.Contains((ScriptTags)complexLanguages[k].scriptTag, (LanguageTags)complexLanguages[k].langSysTag)) ? (typographyAvailabilities | TypographyAvailabilities.FastTextExtraLanguageLocalizedFormAvailable) : (typographyAvailabilities | TypographyAvailabilities.FastTextMajorLanguageLocalizedFormAvailable));
			}
		}
		if (OpenTypeLayout.GetComplexLanguageList(font, RequiredTypographyFeatures, uInts, num2, num3, out complexLanguages) != 0)
		{
			_typographyAvailabilities = TypographyAvailabilities.None;
			return;
		}
		if (complexLanguages != null)
		{
			typographyAvailabilities |= TypographyAvailabilities.FastTextTypographyAvailable;
		}
		for (int l = 0; l < num; l++)
		{
			uInts[l] = uint.MaxValue;
		}
		if (OpenTypeLayout.GetComplexLanguageList(font, RequiredFeatures, uInts, num2, num3, out complexLanguages) != 0)
		{
			_typographyAvailabilities = TypographyAvailabilities.None;
			return;
		}
		if (complexLanguages != null)
		{
			for (int m = 0; m < complexLanguages.Length; m++)
			{
				typographyAvailabilities = ((complexLanguages[m].scriptTag != 1751215721) ? (typographyAvailabilities | TypographyAvailabilities.Available) : (typographyAvailabilities | TypographyAvailabilities.IdeoTypographyAvailable));
			}
		}
		if (typographyAvailabilities != 0)
		{
			typographyAvailabilities |= TypographyAvailabilities.Available;
		}
		_typographyAvailabilities = typographyAvailabilities;
		BufferCache.ReleaseUInts(uInts);
	}
}
