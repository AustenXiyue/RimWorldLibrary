using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;

namespace MS.Internal.FontFace;

internal sealed class PhysicalFontFamily : IFontFamily
{
	private struct MatchingFace
	{
		private Font _face;

		private MatchingStyle _style;

		internal Font FontFace => _face;

		internal MatchingStyle MatchingStyle => _style;

		internal MatchingFace(Font face)
		{
			_face = face;
			_style = new MatchingStyle(new System.Windows.FontStyle((int)face.Style), new System.Windows.FontWeight((int)face.Weight), new System.Windows.FontStretch((int)face.Stretch));
		}
	}

	private class MatchingFaceComparer : IComparer<MatchingFace>
	{
		private MatchingStyle _targetStyle;

		internal MatchingFaceComparer(MatchingStyle targetStyle)
		{
			_targetStyle = targetStyle;
		}

		int IComparer<MatchingFace>.Compare(MatchingFace a, MatchingFace b)
		{
			if (!a.MatchingStyle.IsBetterMatch(_targetStyle, b.MatchingStyle))
			{
				return 1;
			}
			return -1;
		}
	}

	private MS.Internal.Text.TextInterface.FontFamily _family;

	private IDictionary<XmlLanguage, string> _familyNames;

	IDictionary<XmlLanguage, string> IFontFamily.Names
	{
		get
		{
			if (_familyNames == null)
			{
				_familyNames = ConvertDictionary(_family.FamilyNames);
			}
			return _familyNames;
		}
	}

	double IFontFamily.BaselineDesign => ((IFontFamily)this).Baseline(1.0, 1.0, 1.0, TextFormattingMode.Ideal);

	double IFontFamily.LineSpacingDesign => ((IFontFamily)this).LineSpacing(1.0, 1.0, 1.0, TextFormattingMode.Ideal);

	private static IDictionary<XmlLanguage, string> ConvertDictionary(IDictionary<CultureInfo, string> dictionary)
	{
		Dictionary<XmlLanguage, string> dictionary2 = new Dictionary<XmlLanguage, string>();
		foreach (KeyValuePair<CultureInfo, string> item in dictionary)
		{
			XmlLanguage language = XmlLanguage.GetLanguage(item.Key.Name);
			if (!dictionary2.ContainsKey(language))
			{
				dictionary2.Add(language, item.Value);
			}
		}
		return dictionary2;
	}

	internal PhysicalFontFamily(MS.Internal.Text.TextInterface.FontFamily family)
	{
		Invariant.Assert(family != null);
		_family = family;
	}

	ITypefaceMetrics IFontFamily.GetTypefaceMetrics(System.Windows.FontStyle style, System.Windows.FontWeight weight, System.Windows.FontStretch stretch)
	{
		return GetGlyphTypeface(style, weight, stretch);
	}

	IDeviceFont IFontFamily.GetDeviceFont(System.Windows.FontStyle style, System.Windows.FontWeight weight, System.Windows.FontStretch stretch)
	{
		return null;
	}

	internal GlyphTypeface GetGlyphTypeface(System.Windows.FontStyle style, System.Windows.FontWeight weight, System.Windows.FontStretch stretch)
	{
		return new GlyphTypeface(_family.GetFirstMatchingFont((MS.Internal.Text.TextInterface.FontWeight)weight.ToOpenTypeWeight(), (MS.Internal.Text.TextInterface.FontStretch)stretch.ToOpenTypeStretch(), (MS.Internal.Text.TextInterface.FontStyle)style.GetStyleForInternalConstruction()));
	}

	internal GlyphTypeface MapGlyphTypeface(System.Windows.FontStyle style, System.Windows.FontWeight weight, System.Windows.FontStretch stretch, CharacterBufferRange charString, CultureInfo digitCulture, ref int advance, ref int nextValid)
	{
		int num = charString.Length;
		LegacyPriorityQueue<MatchingFace> legacyPriorityQueue = new LegacyPriorityQueue<MatchingFace>(comparer: new MatchingFaceComparer(new MatchingStyle(style, weight, stretch)), capacity: checked((int)_family.Count));
		foreach (Font item in _family)
		{
			legacyPriorityQueue.Push(new MatchingFace(item));
		}
		Font font = null;
		while (legacyPriorityQueue.Count != 0)
		{
			int nextValid2 = 0;
			Font fontFace = legacyPriorityQueue.Top.FontFace;
			int num2 = MapCharacters(fontFace, charString, digitCulture, ref nextValid2);
			if (num2 > 0)
			{
				if (num > 0 && num < num2)
				{
					advance = num;
					nextValid = 0;
				}
				else
				{
					advance = num2;
					nextValid = nextValid2;
				}
				return new GlyphTypeface(fontFace);
			}
			if (nextValid2 < num)
			{
				num = nextValid2;
			}
			if (font == null)
			{
				font = fontFace;
			}
			legacyPriorityQueue.Pop();
		}
		advance = 0;
		nextValid = num;
		return new GlyphTypeface(font);
	}

	private int MapCharacters(Font font, CharacterBufferRange unicodeString, CultureInfo digitCulture, ref int nextValid)
	{
		DigitMap digitMap = new DigitMap(digitCulture);
		int sizeofChar = 0;
		int i = Classification.AdvanceWhile(unicodeString, ItemClass.JoinerClass);
		if (i >= unicodeString.Length)
		{
			return i;
		}
		int num = -1;
		for (; i < unicodeString.Length; i += sizeofChar)
		{
			int num2 = Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, i, unicodeString.Length - i), out sizeofChar);
			if (Classification.IsJoiner(num2))
			{
				continue;
			}
			if (!Classification.IsCombining(num2))
			{
				num = num2;
			}
			else if (num != -1 && Classification.GetScript(num) == Classification.GetScript(num2))
			{
				continue;
			}
			int num3 = digitMap[num2];
			checked
			{
				if (!font.HasCharacter((uint)num3))
				{
					if (num3 == num2)
					{
						break;
					}
					num3 = DigitMap.GetFallbackCharacter(num3);
					if (num3 == 0 || !font.HasCharacter((uint)num3))
					{
						break;
					}
				}
			}
		}
		if (i < unicodeString.Length)
		{
			for (nextValid = i + sizeofChar; nextValid < unicodeString.Length; nextValid += sizeofChar)
			{
				int num4 = Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, nextValid, unicodeString.Length - nextValid), out sizeofChar);
				int num5 = digitMap[num4];
				checked
				{
					if (!Classification.IsJoiner(num5) && (num == -1 || !Classification.IsCombining(num5) || Classification.GetScript(num5) != Classification.GetScript(num)))
					{
						if (font.HasCharacter((uint)num5))
						{
							break;
						}
						if (num5 != num4)
						{
							num5 = DigitMap.GetFallbackCharacter(num5);
							if (num5 != 0 && font.HasCharacter((uint)num5))
							{
								break;
							}
						}
					}
				}
			}
		}
		return i;
	}

	double IFontFamily.Baseline(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		if (textFormattingMode == TextFormattingMode.Ideal)
		{
			return emSize * _family.Metrics.Baseline;
		}
		double num = emSize * toReal;
		return TextFormatterImp.RoundDipForDisplayMode(_family.DisplayMetrics((float)num, (float)pixelsPerDip).Baseline * num, pixelsPerDip) / toReal;
	}

	double IFontFamily.LineSpacing(double emSize, double toReal, double pixelsPerDip, TextFormattingMode textFormattingMode)
	{
		if (textFormattingMode == TextFormattingMode.Ideal)
		{
			return emSize * _family.Metrics.LineSpacing;
		}
		double num = emSize * toReal;
		return TextFormatterImp.RoundDipForDisplayMode(_family.DisplayMetrics((float)num, (float)pixelsPerDip).LineSpacing * num, pixelsPerDip) / toReal;
	}

	ICollection<Typeface> IFontFamily.GetTypefaces(FontFamilyIdentifier familyIdentifier)
	{
		return new TypefaceCollection(new System.Windows.Media.FontFamily(familyIdentifier), _family);
	}

	bool IFontFamily.GetMapTargetFamilyNameAndScale(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, double defaultSizeInEm, out int cchAdvance, out string targetFamilyName, out double scaleInEm)
	{
		cchAdvance = unicodeString.Length;
		targetFamilyName = null;
		scaleInEm = defaultSizeInEm;
		return false;
	}
}
