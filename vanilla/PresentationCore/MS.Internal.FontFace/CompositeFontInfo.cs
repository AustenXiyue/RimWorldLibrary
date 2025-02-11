using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.PresentationCore;

namespace MS.Internal.FontFace;

internal sealed class CompositeFontInfo
{
	private LanguageSpecificStringDictionary _familyNames;

	private double _baseline;

	private double _lineSpacing;

	private FamilyTypefaceCollection _familyTypefaces;

	private FontFamilyMapCollection _familyMaps;

	private ushort[] _defaultFamilyMapRanges;

	private Dictionary<XmlLanguage, ushort[]> _familyMapRangesByLanguage;

	private const int InitialCultureCount = 1;

	private const int InitialTargetFamilyCount = 1;

	private static readonly ushort[] EmptyFamilyMapRanges = new ushort[1];

	private const int InitialFamilyMapRangesCapacity = 7;

	internal const int FirstFamilyMapRange = 1;

	internal FamilyTypefaceCollection FamilyTypefaces => _familyTypefaces;

	internal double Baseline
	{
		get
		{
			return _baseline;
		}
		set
		{
			CompositeFontParser.VerifyNonNegativeMultiplierOfEm("Baseline", ref value);
			_baseline = value;
		}
	}

	internal double LineSpacing
	{
		get
		{
			return _lineSpacing;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("LineSpacing", ref value);
			_lineSpacing = value;
		}
	}

	internal LanguageSpecificStringDictionary FamilyNames => _familyNames;

	internal FontFamilyMapCollection FamilyMaps => _familyMaps;

	internal ICollection<XmlLanguage> FamilyMapLanguages
	{
		get
		{
			if (_familyMapRangesByLanguage != null)
			{
				return _familyMapRangesByLanguage.Keys;
			}
			return null;
		}
	}

	internal CompositeFontInfo()
	{
		_familyNames = new LanguageSpecificStringDictionary(new Dictionary<XmlLanguage, string>(1));
		_familyMaps = new FontFamilyMapCollection(this);
		_defaultFamilyMapRanges = EmptyFamilyMapRanges;
	}

	internal void PrepareToAddFamilyMap(FontFamilyMap familyMap)
	{
		if (familyMap == null)
		{
			throw new ArgumentNullException("familyMap");
		}
		if (string.IsNullOrEmpty(familyMap.Target))
		{
			throw new ArgumentException(SR.FamilyMap_TargetNotSet);
		}
		if (familyMap.Language != null)
		{
			if (_familyMapRangesByLanguage == null)
			{
				_familyMapRangesByLanguage = new Dictionary<XmlLanguage, ushort[]>(1);
				_familyMapRangesByLanguage.Add(familyMap.Language, EmptyFamilyMapRanges);
			}
			else if (!_familyMapRangesByLanguage.ContainsKey(familyMap.Language))
			{
				_familyMapRangesByLanguage.Add(familyMap.Language, EmptyFamilyMapRanges);
			}
		}
	}

	internal void InvalidateFamilyMapRanges()
	{
		_defaultFamilyMapRanges = EmptyFamilyMapRanges;
		if (_familyMapRangesByLanguage == null)
		{
			return;
		}
		Dictionary<XmlLanguage, ushort[]> dictionary = new Dictionary<XmlLanguage, ushort[]>(_familyMapRangesByLanguage.Count);
		foreach (XmlLanguage key in _familyMapRangesByLanguage.Keys)
		{
			dictionary.Add(key, EmptyFamilyMapRanges);
		}
		_familyMapRangesByLanguage = dictionary;
	}

	internal ushort[] GetFamilyMapsOfLanguage(XmlLanguage language)
	{
		ushort[] value = null;
		if (_familyMapRangesByLanguage != null && language != null)
		{
			foreach (XmlLanguage matchingLanguage in language.MatchingLanguages)
			{
				if (matchingLanguage.IetfLanguageTag.Length == 0)
				{
					break;
				}
				if (_familyMapRangesByLanguage.TryGetValue(matchingLanguage, out value))
				{
					if (!IsFamilyMapRangesValid(value))
					{
						value = CreateFamilyMapRanges(matchingLanguage);
						_familyMapRangesByLanguage[matchingLanguage] = value;
					}
					return value;
				}
			}
		}
		if (!IsFamilyMapRangesValid(_defaultFamilyMapRanges))
		{
			_defaultFamilyMapRanges = CreateFamilyMapRanges(null);
		}
		return _defaultFamilyMapRanges;
	}

	internal FontFamilyMap GetFamilyMapOfChar(ushort[] familyMapRanges, int ch)
	{
		for (int i = 1; i < familyMapRanges.Length; i += 2)
		{
			ushort num = familyMapRanges[i];
			int num2 = familyMapRanges[i + 1];
			for (int j = num; j < num2; j++)
			{
				FontFamilyMap fontFamilyMap = _familyMaps[j];
				Invariant.Assert(fontFamilyMap != null);
				if (fontFamilyMap.InRange(ch))
				{
					return fontFamilyMap;
				}
			}
		}
		return FontFamilyMap.Default;
	}

	private bool IsFamilyMapRangesValid(ushort[] familyMapRanges)
	{
		return familyMapRanges[0] == _familyMaps.Count;
	}

	private ushort[] CreateFamilyMapRanges(XmlLanguage language)
	{
		ushort[] array = new ushort[7]
		{
			(ushort)_familyMaps.Count,
			0,
			0,
			0,
			0,
			0,
			0
		};
		int num = 1;
		for (int i = 0; i < _familyMaps.Count; i++)
		{
			if (FontFamilyMap.MatchLanguage(_familyMaps[i].Language, language))
			{
				if (num + 2 > array.Length)
				{
					ushort[] array2 = new ushort[array.Length * 2 - 1];
					array.CopyTo(array2, 0);
					array = array2;
				}
				array[num++] = (ushort)i;
				for (i++; i < _familyMaps.Count && FontFamilyMap.MatchLanguage(_familyMaps[i].Language, language); i++)
				{
				}
				array[num++] = (ushort)i;
			}
		}
		if (num < array.Length)
		{
			ushort[] array3 = new ushort[num];
			Array.Copy(array, array3, num);
			array = array3;
		}
		return array;
	}

	internal FamilyTypefaceCollection GetFamilyTypefaceList()
	{
		if (_familyTypefaces == null)
		{
			_familyTypefaces = new FamilyTypefaceCollection();
		}
		return _familyTypefaces;
	}
}
