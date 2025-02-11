using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.FontCache;
using MS.Internal.FontFace;
using MS.Internal.Generic;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;
using MS.Utility;

namespace MS.Internal.Shaping;

internal class TypefaceMap
{
	internal class IntMap
	{
		private const byte NumberOfPlanes = 17;

		private Plane[] _planes;

		private static Plane EmptyPlane = new Plane();

		public ushort this[int i]
		{
			get
			{
				return _planes[i >> 16][(i >> 8) & 0xFF][i & 0xFF];
			}
			set
			{
				CreatePlane(i >> 16);
				_planes[i >> 16].CreatePage((i >> 8) & 0xFF, this);
				_planes[i >> 16][(i >> 8) & 0xFF][i & 0xFF] = value;
			}
		}

		public IntMap()
		{
			_planes = new Plane[17];
			for (int i = 0; i < 17; i++)
			{
				_planes[i] = EmptyPlane;
			}
		}

		private void CreatePlane(int i)
		{
			Invariant.Assert(i < 17);
			if (_planes[i] == EmptyPlane)
			{
				Plane plane = new Plane();
				_planes[i] = plane;
			}
		}
	}

	internal class Plane
	{
		private Page[] _data;

		private static Page EmptyPage = new Page();

		public Page this[int index]
		{
			get
			{
				return _data[index];
			}
			set
			{
				_data[index] = value;
			}
		}

		public Plane()
		{
			_data = new Page[256];
			for (int i = 0; i < 256; i++)
			{
				_data[i] = EmptyPage;
			}
		}

		internal void CreatePage(int i, IntMap intMap)
		{
			if (this[i] == EmptyPage)
			{
				Page value = new Page();
				this[i] = value;
			}
		}
	}

	internal class Page
	{
		private ushort[] _data;

		public ushort this[int index]
		{
			get
			{
				return _data[index];
			}
			set
			{
				_data[index] = value;
			}
		}

		public Page()
		{
			_data = new ushort[256];
		}
	}

	private System.Windows.Media.FontFamily[] _fontFamilies;

	private System.Windows.FontStyle _canonicalStyle;

	private System.Windows.FontWeight _canonicalWeight;

	private System.Windows.FontStretch _canonicalStretch;

	private bool _nullFont;

	private IList<ScaledShapeTypeface> _cachedScaledTypefaces = new List<ScaledShapeTypeface>(2);

	private IDictionary<CultureInfo, IntMap> _intMaps = new Dictionary<CultureInfo, IntMap>();

	private const int InitialScaledGlyphableTypefaceCount = 2;

	private const int MaxTypefaceMapDepths = 32;

	internal TypefaceMap(System.Windows.Media.FontFamily fontFamily, System.Windows.Media.FontFamily fallbackFontFamily, System.Windows.FontStyle canonicalStyle, System.Windows.FontWeight canonicalWeight, System.Windows.FontStretch canonicalStretch, bool nullFont)
	{
		Invariant.Assert(fontFamily != null);
		_fontFamilies = ((fallbackFontFamily != null) ? new System.Windows.Media.FontFamily[2] { fontFamily, fallbackFontFamily } : new System.Windows.Media.FontFamily[1] { fontFamily });
		_canonicalStyle = canonicalStyle;
		_canonicalWeight = canonicalWeight;
		_canonicalStretch = canonicalStretch;
		_nullFont = nullFont;
	}

	internal unsafe void GetShapeableText(CharacterBufferReference characterBufferReference, int stringLength, TextRunProperties textRunProperties, CultureInfo digitCulture, bool isRightToLeftParagraph, IList<TextShapeableSymbols> shapeableList, IShapeableTextCollector collector, TextFormattingMode textFormattingMode)
	{
		int num = 0;
		CharacterBufferRange characterBufferRange = new CharacterBufferRange(characterBufferReference, stringLength);
		CultureInfo cultureInfo = textRunProperties.CultureInfo;
		GCHandle gcHandle;
		nint num2 = characterBufferReference.CharacterBuffer.PinAndGetCharacterPointer(characterBufferReference.OffsetToFirstChar, out gcHandle);
		bool ignoreUserOverride;
		NumberSubstitutionMethod resolvedSubstitutionMethod = DigitState.GetResolvedSubstitutionMethod(textRunProperties, digitCulture, out ignoreUserOverride);
		IList<Span> collection = TextAnalyzer.Itemize((char*)((IntPtr)num2).ToPointer(), checked((uint)stringLength), cultureInfo, DWriteFactory.Instance, isRightToLeftParagraph, digitCulture, ignoreUserOverride, checked((uint)resolvedSubstitutionMethod), ClassificationUtility.Instance, UnsafeNativeMethods.CreateTextAnalysisSink, UnsafeNativeMethods.GetScriptAnalysisList, UnsafeNativeMethods.GetNumberSubstitutionList, UnsafeNativeMethods.CreateTextAnalysisSource);
		characterBufferReference.CharacterBuffer.UnpinCharacterPointer(gcHandle);
		SpanVector spanVector = new SpanVector(null, new FrugalStructList<Span>(collection));
		SpanVector<int> cachedScaledTypefaceIndexSpans = new SpanVector<int>(-1);
		foreach (Span item in spanVector)
		{
			MapItem(new CharacterBufferRange(characterBufferRange, num, item.length), cultureInfo, item, ref cachedScaledTypefaceIndexSpans, num);
			num += item.length;
		}
		int i = 0;
		SpanRider spanRider = new SpanRider(spanVector);
		SpanRider<int> spanRider2 = new SpanRider<int>(cachedScaledTypefaceIndexSpans);
		int val;
		for (; i < characterBufferRange.Length; i += val)
		{
			spanRider.At(i);
			spanRider2.At(i);
			int currentValue = spanRider2.CurrentValue;
			val = characterBufferRange.Length - i;
			val = Math.Min(val, spanRider.Length);
			val = Math.Min(val, spanRider2.Length);
			ScaledShapeTypeface scaledShapeTypeface = _cachedScaledTypefaces[currentValue];
			collector.Add(shapeableList, new CharacterBufferRange(characterBufferRange, i, val), textRunProperties, (ItemProps)spanRider.CurrentElement, scaledShapeTypeface.ShapeTypeface, scaledShapeTypeface.ScaleInEm, scaledShapeTypeface.NullShape, textFormattingMode);
		}
	}

	private void MapItem(CharacterBufferRange unicodeString, CultureInfo culture, Span itemSpan, ref SpanVector<int> cachedScaledTypefaceIndexSpans, int ichItem)
	{
		CultureInfo digitCulture = ((ItemProps)itemSpan.element).DigitCulture;
		if (!GetCachedScaledTypefaceMap(unicodeString, culture, digitCulture, ref cachedScaledTypefaceIndexSpans, ichItem))
		{
			SpanVector scaledTypefaceSpans = new SpanVector(null);
			PhysicalFontFamily firstValidFamily = null;
			int firstValidLength = 0;
			int nextValid;
			if (!_nullFont)
			{
				MapByFontFamilyList(unicodeString, culture, digitCulture, _fontFamilies, ref firstValidFamily, ref firstValidLength, null, 1.0, 0, scaledTypefaceSpans, 0, out nextValid);
			}
			else
			{
				MapUnresolvedCharacters(unicodeString, culture, digitCulture, firstValidFamily, ref firstValidLength, scaledTypefaceSpans, 0, out nextValid);
			}
			CacheScaledTypefaceMap(unicodeString, culture, digitCulture, scaledTypefaceSpans, ref cachedScaledTypefaceIndexSpans, ichItem);
		}
	}

	private bool GetCachedScaledTypefaceMap(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, ref SpanVector<int> cachedScaledTypefaceIndexSpans, int ichItem)
	{
		if (!_intMaps.TryGetValue(culture, out var value))
		{
			return false;
		}
		DigitMap digitMap = new DigitMap(digitCulture);
		int j;
		for (int i = 0; i < unicodeString.Length; i += j)
		{
			int i2 = digitMap[Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, i, unicodeString.Length - i), out var sizeofChar)];
			ushort num = value[i2];
			if (num == 0)
			{
				return false;
			}
			for (j = sizeofChar; i + j < unicodeString.Length; j += sizeofChar)
			{
				i2 = digitMap[Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, i + j, unicodeString.Length - i - j), out sizeofChar)];
				if (value[i2] != num && !Classification.IsCombining(i2) && !Classification.IsJoiner(i2))
				{
					break;
				}
			}
			cachedScaledTypefaceIndexSpans.Set(ichItem + i, j, num - 1);
		}
		return true;
	}

	private void CacheScaledTypefaceMap(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, SpanVector scaledTypefaceSpans, ref SpanVector<int> cachedScaledTypefaceIndexSpans, int ichItem)
	{
		if (!_intMaps.TryGetValue(culture, out var value))
		{
			value = new IntMap();
			_intMaps.Add(culture, value);
		}
		DigitMap digitMap = new DigitMap(digitCulture);
		SpanRider spanRider = new SpanRider(scaledTypefaceSpans);
		int num;
		for (int i = 0; i < unicodeString.Length; i += num)
		{
			spanRider.At(i);
			num = Math.Min(unicodeString.Length - i, spanRider.Length);
			int num2 = IndexOfScaledTypeface((ScaledShapeTypeface)spanRider.CurrentElement);
			cachedScaledTypefaceIndexSpans.Set(ichItem + i, num, num2);
			num2++;
			int sizeofChar;
			for (int j = 0; j < num; j += sizeofChar)
			{
				int num3 = digitMap[Classification.UnicodeScalar(new CharacterBufferRange(unicodeString, i + j, unicodeString.Length - i - j), out sizeofChar)];
				if (!Classification.IsCombining(num3) && !Classification.IsJoiner(num3))
				{
					if (value[num3] != 0 && value[num3] != num2)
					{
						Invariant.Assert(condition: false, string.Format(CultureInfo.InvariantCulture, "shapeable cache stores conflicting info, ch = {0}, map[ch] = {1}, index = {2}", num3, value[num3], num2));
					}
					value[num3] = (ushort)num2;
				}
			}
		}
	}

	private int IndexOfScaledTypeface(ScaledShapeTypeface scaledTypeface)
	{
		int i;
		for (i = 0; i < _cachedScaledTypefaces.Count && !scaledTypeface.Equals(_cachedScaledTypefaces[i]); i++)
		{
		}
		if (i == _cachedScaledTypefaces.Count)
		{
			i = _cachedScaledTypefaces.Count;
			_cachedScaledTypefaces.Add(scaledTypeface);
		}
		return i;
	}

	private int MapByFontFamily(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, IFontFamily fontFamily, CanonicalFontFamilyReference canonicalFamilyReference, System.Windows.FontStyle canonicalStyle, System.Windows.FontWeight canonicalWeight, System.Windows.FontStretch canonicalStretch, ref PhysicalFontFamily firstValidFamily, ref int firstValidLength, IDeviceFont deviceFont, double scaleInEm, int recursionDepth, SpanVector scaledTypefaceSpans, int firstCharIndex, out int nextValid)
	{
		if (recursionDepth >= 32)
		{
			nextValid = 0;
			return 0;
		}
		if (deviceFont == null)
		{
			deviceFont = fontFamily.GetDeviceFont(_canonicalStyle, _canonicalWeight, _canonicalStretch);
		}
		DigitMap digitMap = new DigitMap(digitCulture);
		int num = 0;
		int i = 0;
		nextValid = 0;
		bool flag = false;
		int num3;
		for (; i < unicodeString.Length; i += num3)
		{
			if (flag)
			{
				break;
			}
			int cchAdvance = unicodeString.Length - i;
			bool flag2 = false;
			if (deviceFont != null)
			{
				flag2 = deviceFont.ContainsCharacter(digitMap[unicodeString[i]]);
				int j;
				for (j = i + 1; j < unicodeString.Length && flag2 == deviceFont.ContainsCharacter(digitMap[unicodeString[j]]); j++)
				{
				}
				cchAdvance = j - i;
			}
			string targetFamilyName;
			double scaleInEm2;
			bool mapTargetFamilyNameAndScale = fontFamily.GetMapTargetFamilyNameAndScale(new CharacterBufferRange(unicodeString, i, cchAdvance), culture, digitCulture, scaleInEm, out cchAdvance, out targetFamilyName, out scaleInEm2);
			CharacterBufferRange unicodeString2 = new CharacterBufferRange(unicodeString, i, cchAdvance);
			int num2;
			int nextValid2;
			if (!mapTargetFamilyNameAndScale)
			{
				num2 = MapByFontFaceFamily(unicodeString2, culture, digitCulture, fontFamily, canonicalStyle, canonicalWeight, canonicalStretch, ref firstValidFamily, ref firstValidLength, flag2 ? deviceFont : null, nullFont: false, scaleInEm2, scaledTypefaceSpans, firstCharIndex + i, ignoreMissing: false, out nextValid2);
			}
			else if (!string.IsNullOrEmpty(targetFamilyName))
			{
				Uri baseUri = canonicalFamilyReference?.LocationUri;
				num2 = MapByFontFamilyName(unicodeString2, culture, digitCulture, targetFamilyName, baseUri, ref firstValidFamily, ref firstValidLength, flag2 ? deviceFont : null, scaleInEm2, recursionDepth + 1, scaledTypefaceSpans, firstCharIndex + i, out nextValid2);
			}
			else
			{
				num2 = 0;
				nextValid2 = cchAdvance;
			}
			num3 = cchAdvance;
			int num4 = 0;
			num3 = num2;
			num4 = nextValid2;
			if (num3 < cchAdvance)
			{
				flag = true;
			}
			num += num3;
			nextValid = i + num4;
		}
		return num;
	}

	private int MapUnresolvedCharacters(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, PhysicalFontFamily firstValidFamily, ref int firstValidLength, SpanVector scaledTypefaceSpans, int firstCharIndex, out int nextValid)
	{
		IFontFamily fontFamily = firstValidFamily;
		bool nullFont = false;
		if (firstValidLength <= 0)
		{
			fontFamily = System.Windows.Media.FontFamily.LookupFontFamily(System.Windows.Media.FontFamily.NullFontFamilyCanonicalName);
			Invariant.Assert(fontFamily != null);
			nullFont = true;
		}
		return MapByFontFaceFamily(unicodeString, culture, digitCulture, fontFamily, _canonicalStyle, _canonicalWeight, _canonicalStretch, ref firstValidFamily, ref firstValidLength, null, nullFont, 1.0, scaledTypefaceSpans, firstCharIndex, ignoreMissing: true, out nextValid);
	}

	private int MapByFontFamilyName(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, string familyName, Uri baseUri, ref PhysicalFontFamily firstValidFamily, ref int firstValidLength, IDeviceFont deviceFont, double scaleInEm, int fontMappingDepth, SpanVector scaledTypefaceSpans, int firstCharIndex, out int nextValid)
	{
		if (familyName == null)
		{
			return MapUnresolvedCharacters(unicodeString, culture, digitCulture, firstValidFamily, ref firstValidLength, scaledTypefaceSpans, firstCharIndex, out nextValid);
		}
		return MapByFontFamilyList(unicodeString, culture, digitCulture, new System.Windows.Media.FontFamily[1]
		{
			new System.Windows.Media.FontFamily(baseUri, familyName)
		}, ref firstValidFamily, ref firstValidLength, deviceFont, scaleInEm, fontMappingDepth, scaledTypefaceSpans, firstCharIndex, out nextValid);
	}

	private int MapByFontFamilyList(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, System.Windows.Media.FontFamily[] familyList, ref PhysicalFontFamily firstValidFamily, ref int firstValidLength, IDeviceFont deviceFont, double scaleInEm, int recursionDepth, SpanVector scaledTypefaceSpans, int firstCharIndex, out int nextValid)
	{
		int num = 0;
		int nextValid2 = 0;
		int i = 0;
		nextValid = 0;
		int num2;
		for (; i < unicodeString.Length; i += num2)
		{
			num2 = MapOnceByFontFamilyList(new CharacterBufferRange(unicodeString, i, unicodeString.Length - i), culture, digitCulture, familyList, ref firstValidFamily, ref firstValidLength, deviceFont, scaleInEm, recursionDepth, scaledTypefaceSpans, firstCharIndex + i, out nextValid2);
			if (num2 <= 0)
			{
				if (recursionDepth > 0)
				{
					break;
				}
				num2 = MapUnresolvedCharacters(new CharacterBufferRange(unicodeString, i, nextValid2), culture, digitCulture, firstValidFamily, ref firstValidLength, scaledTypefaceSpans, firstCharIndex + i, out nextValid2);
			}
		}
		num += i;
		nextValid = i + nextValid2;
		return num;
	}

	private int MapOnceByFontFamilyList(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, System.Windows.Media.FontFamily[] familyList, ref PhysicalFontFamily firstValidFamily, ref int firstValidLength, IDeviceFont deviceFont, double scaleInEm, int recursionDepth, SpanVector scaledTypefaceSpans, int firstCharIndex, out int nextValid)
	{
		Invariant.Assert(familyList != null);
		int num = 0;
		nextValid = 0;
		CharacterBufferRange unicodeString2 = unicodeString;
		System.Windows.FontStyle style = _canonicalStyle;
		System.Windows.FontWeight weight = _canonicalWeight;
		System.Windows.FontStretch stretch = _canonicalStretch;
		for (int i = 0; i < familyList.Length; i++)
		{
			FontFamilyIdentifier familyIdentifier = familyList[i].FamilyIdentifier;
			CanonicalFontFamilyReference canonicalFamilyReference = null;
			IFontFamily fontFamily;
			if (familyIdentifier.Count != 0)
			{
				canonicalFamilyReference = familyIdentifier[0];
				fontFamily = System.Windows.Media.FontFamily.LookupFontFamilyAndFace(canonicalFamilyReference, ref style, ref weight, ref stretch);
			}
			else
			{
				fontFamily = familyList[i].FirstFontFamily;
			}
			int num2 = 0;
			while (true)
			{
				if (fontFamily != null)
				{
					num = MapByFontFamily(unicodeString2, culture, digitCulture, fontFamily, canonicalFamilyReference, style, weight, stretch, ref firstValidFamily, ref firstValidLength, deviceFont, scaleInEm, recursionDepth, scaledTypefaceSpans, firstCharIndex, out nextValid);
					if (nextValid < unicodeString2.Length)
					{
						unicodeString2 = new CharacterBufferRange(unicodeString.CharacterBuffer, unicodeString.OffsetToFirstChar, nextValid);
					}
					if (num > 0)
					{
						i = familyList.Length;
						break;
					}
				}
				else
				{
					nextValid = unicodeString2.Length;
				}
				if (++num2 >= familyIdentifier.Count)
				{
					break;
				}
				canonicalFamilyReference = familyIdentifier[num2];
				fontFamily = System.Windows.Media.FontFamily.LookupFontFamilyAndFace(canonicalFamilyReference, ref style, ref weight, ref stretch);
			}
		}
		nextValid = unicodeString2.Length;
		return num;
	}

	private int MapByFontFaceFamily(CharacterBufferRange unicodeString, CultureInfo culture, CultureInfo digitCulture, IFontFamily fontFamily, System.Windows.FontStyle canonicalStyle, System.Windows.FontWeight canonicalWeight, System.Windows.FontStretch canonicalStretch, ref PhysicalFontFamily firstValidFamily, ref int firstValidLength, IDeviceFont deviceFont, bool nullFont, double scaleInEm, SpanVector scaledTypefaceSpans, int firstCharIndex, bool ignoreMissing, out int nextValid)
	{
		Invariant.Assert(fontFamily != null);
		PhysicalFontFamily physicalFontFamily = fontFamily as PhysicalFontFamily;
		Invariant.Assert(physicalFontFamily != null);
		int advance = unicodeString.Length;
		nextValid = 0;
		GlyphTypeface glyphTypeface = null;
		if (ignoreMissing)
		{
			glyphTypeface = physicalFontFamily.GetGlyphTypeface(canonicalStyle, canonicalWeight, canonicalStretch);
		}
		else if (nullFont)
		{
			glyphTypeface = physicalFontFamily.GetGlyphTypeface(canonicalStyle, canonicalWeight, canonicalStretch);
			advance = 0;
			nextValid = unicodeString.Length;
		}
		else
		{
			glyphTypeface = physicalFontFamily.MapGlyphTypeface(canonicalStyle, canonicalWeight, canonicalStretch, unicodeString, digitCulture, ref advance, ref nextValid);
		}
		Invariant.Assert(glyphTypeface != null);
		int length = unicodeString.Length;
		if (!ignoreMissing && advance > 0)
		{
			length = advance;
		}
		if (firstValidLength <= 0)
		{
			firstValidFamily = physicalFontFamily;
			firstValidLength = unicodeString.Length;
		}
		firstValidLength -= advance;
		scaledTypefaceSpans.SetValue(firstCharIndex, length, new ScaledShapeTypeface(glyphTypeface, deviceFont, scaleInEm, nullFont));
		return advance;
	}
}
