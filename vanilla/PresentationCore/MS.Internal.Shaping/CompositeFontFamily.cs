using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.FontFace;
using MS.Internal.TextFormatting;

namespace MS.Internal.Shaping;

internal sealed class CompositeFontFamily : IFontFamily
{
	private readonly CompositeFontInfo _fontInfo;

	private IFontFamily _firstFontFamily;

	IDictionary<XmlLanguage, string> IFontFamily.Names => _fontInfo.FamilyNames;

	double IFontFamily.BaselineDesign
	{
		get
		{
			if (_fontInfo.Baseline == 0.0)
			{
				_fontInfo.Baseline = GetFirstFontFamily().BaselineDesign;
			}
			return _fontInfo.Baseline;
		}
	}

	double IFontFamily.LineSpacingDesign
	{
		get
		{
			if (_fontInfo.LineSpacing == 0.0)
			{
				_fontInfo.LineSpacing = GetFirstFontFamily().LineSpacingDesign;
			}
			return _fontInfo.LineSpacing;
		}
	}

	internal LanguageSpecificStringDictionary FamilyNames => _fontInfo.FamilyNames;

	internal FamilyTypefaceCollection FamilyTypefaces => _fontInfo.GetFamilyTypefaceList();

	internal FontFamilyMapCollection FamilyMaps => _fontInfo.FamilyMaps;

	internal CompositeFontFamily()
		: this(new CompositeFontInfo())
	{
	}

	internal CompositeFontFamily(CompositeFontInfo fontInfo)
	{
		_fontInfo = fontInfo;
	}

	internal CompositeFontFamily(string friendlyName)
		: this(friendlyName, null)
	{
	}

	internal CompositeFontFamily(string friendlyName, IFontFamily firstFontFamily)
		: this()
	{
		FamilyMaps.Add(new FontFamilyMap(0, 1114111, null, friendlyName, 1.0));
		_firstFontFamily = firstFontFamily;
	}

	public double Baseline(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		if (textFormattingMode == TextFormattingMode.Ideal)
		{
			return ((IFontFamily)this).BaselineDesign * emSize;
		}
		if (_fontInfo.Baseline != 0.0)
		{
			return Math.Round(_fontInfo.Baseline * emSize);
		}
		return GetFirstFontFamily().Baseline(emSize, toReal, pixelsPerDip, textFormattingMode);
	}

	public void SetBaseline(double value)
	{
		_fontInfo.Baseline = value;
	}

	public double LineSpacing(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		if (textFormattingMode == TextFormattingMode.Ideal)
		{
			return ((IFontFamily)this).LineSpacingDesign * emSize;
		}
		if (_fontInfo.LineSpacing != 0.0)
		{
			return Math.Round(_fontInfo.LineSpacing * emSize);
		}
		return GetFirstFontFamily().LineSpacing(emSize, toReal, pixelsPerDip, textFormattingMode);
	}

	public void SetLineSpacing(double value)
	{
		_fontInfo.LineSpacing = value;
	}

	ITypefaceMetrics IFontFamily.GetTypefaceMetrics(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		if (_fontInfo.FamilyTypefaces == null && _fontInfo.FamilyMaps.Count == 1 && _fontInfo.FamilyMaps[0].IsSimpleFamilyMap)
		{
			return GetFirstFontFamily().GetTypefaceMetrics(style, weight, stretch);
		}
		return FindTypefaceMetrics(style, weight, stretch);
	}

	IDeviceFont IFontFamily.GetDeviceFont(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		FamilyTypeface familyTypeface = FindExactFamilyTypeface(style, weight, stretch);
		if (familyTypeface != null && familyTypeface.DeviceFontName != null)
		{
			return familyTypeface;
		}
		return null;
	}

	bool IFontFamily.GetMapTargetFamilyNameAndScale(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, double defaultSizeInEm, out int cchAdvance, out string targetFamilyName, out double scaleInEm)
	{
		Invariant.Assert(unicodeString.CharacterBuffer != null && unicodeString.Length > 0);
		Invariant.Assert(culture != null);
		FontFamilyMap targetFamilyMap = GetTargetFamilyMap(unicodeString, culture, digitCulture, out cchAdvance);
		targetFamilyName = targetFamilyMap.Target;
		scaleInEm = targetFamilyMap.Scale;
		return true;
	}

	ICollection<Typeface> IFontFamily.GetTypefaces(FontFamilyIdentifier familyIdentifier)
	{
		return new TypefaceCollection(new FontFamily(familyIdentifier), FamilyTypefaces);
	}

	private FontFamilyMap GetTargetFamilyMap(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, out int cchAdvance)
	{
		DigitMap digitMap = new DigitMap(digitCulture);
		ushort[] familyMapsOfLanguage = _fontInfo.GetFamilyMapsOfLanguage(XmlLanguage.GetLanguage(culture.IetfLanguageTag));
		int sizeofChar = 0;
		int num = 0;
		cchAdvance = Classification.AdvanceWhile(unicodeString, ItemClass.JoinerClass);
		if (cchAdvance >= unicodeString.Length)
		{
			return _fontInfo.GetFamilyMapOfChar(familyMapsOfLanguage, Classification.UnicodeScalar(unicodeString, out sizeofChar));
		}
		num = Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, cchAdvance, unicodeString.Length - cchAdvance), out sizeofChar);
		bool flag = !Classification.IsCombining(num);
		num = digitMap[num];
		FontFamilyMap familyMapOfChar = _fontInfo.GetFamilyMapOfChar(familyMapsOfLanguage, num);
		Invariant.Assert(familyMapOfChar != null);
		cchAdvance += sizeofChar;
		for (; cchAdvance < unicodeString.Length; cchAdvance += sizeofChar)
		{
			num = Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, cchAdvance, unicodeString.Length - cchAdvance), out sizeofChar);
			if (!Classification.IsJoiner(num))
			{
				if (!Classification.IsCombining(num))
				{
					flag = true;
				}
				else if (flag)
				{
					continue;
				}
				num = digitMap[num];
				if (_fontInfo.GetFamilyMapOfChar(familyMapsOfLanguage, num) != familyMapOfChar)
				{
					break;
				}
			}
		}
		return familyMapOfChar;
	}

	private IFontFamily GetFirstFontFamily()
	{
		if (_firstFontFamily == null)
		{
			if (_fontInfo.FamilyMaps.Count != 0)
			{
				_firstFontFamily = FontFamily.FindFontFamilyFromFriendlyNameList(_fontInfo.FamilyMaps[0].Target);
			}
			else
			{
				_firstFontFamily = FontFamily.LookupFontFamily(FontFamily.NullFontFamilyCanonicalName);
			}
			Invariant.Assert(_firstFontFamily != null);
		}
		return _firstFontFamily;
	}

	private ITypefaceMetrics FindTypefaceMetrics(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		FamilyTypeface familyTypeface = FindNearestFamilyTypeface(style, weight, stretch);
		if (familyTypeface == null)
		{
			return new CompositeTypefaceMetrics();
		}
		return familyTypeface;
	}

	private FamilyTypeface FindNearestFamilyTypeface(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		if (_fontInfo.FamilyTypefaces == null || _fontInfo.FamilyTypefaces.Count == 0)
		{
			return null;
		}
		FamilyTypeface familyTypeface = _fontInfo.FamilyTypefaces[0];
		MatchingStyle best = new MatchingStyle(familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch);
		MatchingStyle target = new MatchingStyle(style, weight, stretch);
		for (int i = 1; i < _fontInfo.FamilyTypefaces.Count; i++)
		{
			FamilyTypeface familyTypeface2 = _fontInfo.FamilyTypefaces[i];
			MatchingStyle matching = new MatchingStyle(familyTypeface2.Style, familyTypeface2.Weight, familyTypeface2.Stretch);
			if (MatchingStyle.IsBetterMatch(target, best, ref matching))
			{
				familyTypeface = familyTypeface2;
				best = matching;
			}
		}
		return familyTypeface;
	}

	private FamilyTypeface FindExactFamilyTypeface(FontStyle style, FontWeight weight, FontStretch stretch)
	{
		if (_fontInfo.FamilyTypefaces == null || _fontInfo.FamilyTypefaces.Count == 0)
		{
			return null;
		}
		MatchingStyle matchingStyle = new MatchingStyle(style, weight, stretch);
		foreach (FamilyTypeface familyTypeface in _fontInfo.FamilyTypefaces)
		{
			if (new MatchingStyle(familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch) == matchingStyle)
			{
				return familyTypeface;
			}
		}
		return null;
	}
}
