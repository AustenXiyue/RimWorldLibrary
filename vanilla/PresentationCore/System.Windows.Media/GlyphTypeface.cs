using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows.Media.Composition;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;
using MS.Internal.TextFormatting;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Specifies a physical font face that corresponds to a font file on the disk.</summary>
public class GlyphTypeface : ITypefaceMetrics, ISupportInitialize
{
	private delegate double GlyphAccessor(ushort glyphIndex, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways);

	private class GlyphIndexer : IDictionary<ushort, double>, ICollection<KeyValuePair<ushort, double>>, IEnumerable<KeyValuePair<ushort, double>>, IEnumerable
	{
		private class ValueCollection : ICollection<double>, IEnumerable<double>, IEnumerable
		{
			private GlyphIndexer _glyphIndexer;

			public int Count => _glyphIndexer._numberOfGlyphs;

			public bool IsReadOnly => true;

			public ValueCollection(GlyphIndexer glyphIndexer)
			{
				_glyphIndexer = glyphIndexer;
			}

			public void Add(double item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(double item)
			{
				using (IEnumerator<double> enumerator = GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == item)
						{
							return true;
						}
					}
				}
				return false;
			}

			public void CopyTo(double[] array, int arrayIndex)
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
				for (ushort num = 0; num < Count; num++)
				{
					array[arrayIndex + num] = _glyphIndexer[num];
				}
			}

			public bool Remove(double item)
			{
				throw new NotSupportedException();
			}

			public IEnumerator<double> GetEnumerator()
			{
				ushort i = 0;
				while (i < Count)
				{
					yield return _glyphIndexer[i];
					ushort num = (ushort)(i + 1);
					i = num;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<double>)this).GetEnumerator();
			}
		}

		private GlyphAccessor _accessor;

		private ushort _numberOfGlyphs;

		public ICollection<ushort> Keys => new SequentialUshortCollection(_numberOfGlyphs);

		public ICollection<double> Values => new ValueCollection(this);

		public double this[ushort key]
		{
			get
			{
				return _accessor(key, 1f, TextFormattingMode.Ideal, isSideways: false);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count => _numberOfGlyphs;

		public bool IsReadOnly => true;

		internal GlyphIndexer(GlyphAccessor accessor, ushort numberOfGlyphs)
		{
			_accessor = accessor;
			_numberOfGlyphs = numberOfGlyphs;
		}

		public void Add(ushort key, double value)
		{
			throw new NotSupportedException();
		}

		public bool ContainsKey(ushort key)
		{
			return key < _numberOfGlyphs;
		}

		public bool Remove(ushort key)
		{
			throw new NotSupportedException();
		}

		public bool TryGetValue(ushort key, out double value)
		{
			if (ContainsKey(key))
			{
				value = this[key];
				return true;
			}
			value = 0.0;
			return false;
		}

		public void Add(KeyValuePair<ushort, double> item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(KeyValuePair<ushort, double> item)
		{
			return ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<ushort, double>[] array, int arrayIndex)
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
			for (ushort num = 0; num < Count; num++)
			{
				array[arrayIndex + num] = new KeyValuePair<ushort, double>(num, this[num]);
			}
		}

		public bool Remove(KeyValuePair<ushort, double> item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<KeyValuePair<ushort, double>> GetEnumerator()
		{
			ushort i = 0;
			while (i < Count)
			{
				yield return new KeyValuePair<ushort, double>(i, this[i]);
				ushort num = (ushort)(i + 1);
				i = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<ushort, double>>)this).GetEnumerator();
		}
	}

	private enum InitializationState
	{
		Uninitialized,
		IsInitializing,
		IsInitialized
	}

	private FontFaceLayoutInfo _fontFace;

	private StyleSimulations _styleSimulations;

	private Font _font;

	private FontSource _fontSource;

	private SecurityCriticalDataClass<Uri> _originalUri;

	private const double CFFConversionFactor = 1.52587890625E-05;

	private InitializationState _initializationState;

	/// <summary>Gets or sets the URI for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>The URI for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</returns>
	public Uri FontUri
	{
		get
		{
			CheckInitialized();
			return _originalUri.Value;
		}
		set
		{
			CheckInitializing();
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!value.IsAbsoluteUri)
			{
				throw new ArgumentException(SR.UriNotAbsolute, "value");
			}
			_originalUri = new SecurityCriticalDataClass<Uri>(value);
		}
	}

	/// <summary>Gets the family name for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent family name information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string containing the family name.</returns>
	public IDictionary<CultureInfo, string> FamilyNames
	{
		get
		{
			CheckInitialized();
			if (_font.GetInformationalStrings(InformationalStringID.PreferredFamilyNames, out var informationalStrings) || _font.GetInformationalStrings(InformationalStringID.WIN32FamilyNames, out informationalStrings))
			{
				return informationalStrings;
			}
			return null;
		}
	}

	/// <summary>Gets the face name for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent face name information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string containing the face name.</returns>
	public IDictionary<CultureInfo, string> FaceNames
	{
		get
		{
			CheckInitialized();
			if (_font.GetInformationalStrings(InformationalStringID.PreferredSubFamilyNames, out var informationalStrings) || _font.GetInformationalStrings(InformationalStringID.Win32SubFamilyNames, out informationalStrings))
			{
				return informationalStrings;
			}
			return null;
		}
	}

	/// <summary>Gets the Win32 family name for the font represented by the  <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent Win32 family name information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that represents the Win32 family name.</returns>
	public IDictionary<CultureInfo, string> Win32FamilyNames
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.WIN32FamilyNames);
		}
	}

	IDictionary<XmlLanguage, string> ITypefaceMetrics.AdjustedFaceNames
	{
		get
		{
			CheckInitialized();
			LocalizedStrings faceNames = _font.FaceNames;
			IDictionary<XmlLanguage, string> dictionary = new Dictionary<XmlLanguage, string>(((ICollection<KeyValuePair<CultureInfo, string>>)faceNames).Count);
			foreach (KeyValuePair<CultureInfo, string> item in (IEnumerable<KeyValuePair<CultureInfo, string>>)faceNames)
			{
				dictionary[XmlLanguage.GetLanguage(item.Key.IetfLanguageTag)] = item.Value;
			}
			return dictionary;
		}
	}

	/// <summary>Gets the Win32 face name for the font represented by the  <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent Win32 face name information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that represents the Win32 face name.</returns>
	public IDictionary<CultureInfo, string> Win32FaceNames
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.Win32SubFamilyNames);
		}
	}

	/// <summary>Gets the version string information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object interpreted from the font's 'NAME' table.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent version string information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that represents the version.</returns>
	public IDictionary<CultureInfo, string> VersionStrings
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.VersionStrings);
		}
	}

	/// <summary>Gets the copyright information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent copyright information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a copyright information string.</returns>
	public IDictionary<CultureInfo, string> Copyrights
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.CopyrightNotice);
		}
	}

	/// <summary>Gets the font manufacturer information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs for the font manufacturer information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that describes the font manufacturer information.</returns>
	public IDictionary<CultureInfo, string> ManufacturerNames
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.Manufacturer);
		}
	}

	/// <summary>Gets the trademark notice information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent trademark notice information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that the trademark notice information.</returns>
	public IDictionary<CultureInfo, string> Trademarks
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.Trademark);
		}
	}

	/// <summary>Gets the designer information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent designer information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a designer information string.</returns>
	public IDictionary<CultureInfo, string> DesignerNames
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.Designer);
		}
	}

	/// <summary>Gets the description information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a description information string.</returns>
	public IDictionary<CultureInfo, string> Descriptions
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.Description);
		}
	}

	/// <summary>Gets the vendor URL information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent vendor URL information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that references a vendor URL.</returns>
	public IDictionary<CultureInfo, string> VendorUrls
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.FontVendorURL);
		}
	}

	/// <summary>Gets the designer URL information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent designer information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that references a designer URL.</returns>
	public IDictionary<CultureInfo, string> DesignerUrls
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.DesignerURL);
		}
	}

	/// <summary>Gets the font license description information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs font license information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that describes the font license information.</returns>
	public IDictionary<CultureInfo, string> LicenseDescriptions
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.LicenseDescription);
		}
	}

	/// <summary>Gets the sample text information for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs sample text information. The key is a <see cref="T:System.Globalization.CultureInfo" /> object that identifies the culture. The value is a string that describes the sample text information.</returns>
	public IDictionary<CultureInfo, string> SampleTexts
	{
		get
		{
			CheckInitialized();
			return GetFontInfo(InformationalStringID.SampleText);
		}
	}

	/// <summary>Gets the style for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.FontStyle" /> value that represents the style value.</returns>
	public FontStyle Style
	{
		get
		{
			CheckInitialized();
			return new FontStyle((int)_font.Style);
		}
	}

	/// <summary>Gets the designed weight of the font represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.FontWeight" /> that represents the font weight.</returns>
	public FontWeight Weight
	{
		get
		{
			CheckInitialized();
			return new FontWeight((int)_font.Weight);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.FontStretch" /> value for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.FontStretch" /> value that represents the font stretch.</returns>
	public FontStretch Stretch
	{
		get
		{
			CheckInitialized();
			return new FontStretch((int)_font.Stretch);
		}
	}

	/// <summary>Gets the font face version interpreted from the font's 'NAME' table.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the version.</returns>
	public double Version
	{
		get
		{
			CheckInitialized();
			return _font.Version;
		}
	}

	/// <summary>Gets the height of the character cell relative to the em size.</summary>
	/// <returns>An <see cref="T:System.Double" /> value that represents the height of the character cell.</returns>
	public double Height
	{
		get
		{
			CheckInitialized();
			return (double)(_font.Metrics.Ascent + _font.Metrics.Descent) / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets the baseline value for the <see cref="T:System.Windows.Media.GlyphTypeface" />.</summary>
	/// <returns>A value of type <see cref="T:System.Double" /> that represents the baseline.</returns>
	public double Baseline
	{
		get
		{
			CheckInitialized();
			return (double)(int)_font.Metrics.Ascent / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets the distance from the baseline to the top of an English capital, relative to em size, for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the distance from the baseline to the top of an English capital letter, expressed as a fraction of the font em size.</returns>
	public double CapsHeight
	{
		get
		{
			CheckInitialized();
			return (double)(int)_font.Metrics.CapHeight / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets the Western x-height relative to em size for the font represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double XHeight
	{
		get
		{
			CheckInitialized();
			return (double)(int)_font.Metrics.XHeight / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Media.GlyphTypeface" /> font conforms to Unicode encoding.</summary>
	/// <returns>true if the font conforms to Unicode encoding; otherwise, false.</returns>
	public bool Symbol
	{
		get
		{
			CheckInitialized();
			return _font.IsSymbolFont;
		}
	}

	/// <summary>Gets the position of the underline in the <see cref="T:System.Windows.Media.GlyphTypeface" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> value that represents the position of the underline.</returns>
	public double UnderlinePosition
	{
		get
		{
			CheckInitialized();
			return (double)_font.Metrics.UnderlinePosition / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets the thickness of the underline relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double UnderlineThickness
	{
		get
		{
			CheckInitialized();
			return (double)(int)_font.Metrics.UnderlineThickness / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets a value that indicates the distance from the baseline to the strikethrough for the typeface.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the strikethrough position.</returns>
	public double StrikethroughPosition
	{
		get
		{
			CheckInitialized();
			return (double)_font.Metrics.StrikethroughPosition / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets a value that indicates the thickness of the strikethrough relative to the font em size.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the strikethrough thickness, expressed as a fraction of the font em size.</returns>
	public double StrikethroughThickness
	{
		get
		{
			CheckInitialized();
			return (double)(int)_font.Metrics.StrikethroughThickness / (double)(int)_font.Metrics.DesignUnitsPerEm;
		}
	}

	/// <summary>Gets the font embedding permission for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.FontEmbeddingRight" /> values that represents the font embedding permission</returns>
	public FontEmbeddingRight EmbeddingRights
	{
		get
		{
			CheckInitialized();
			return _fontFace.EmbeddingRights;
		}
	}

	double ITypefaceMetrics.CapsHeight => CapsHeight;

	double ITypefaceMetrics.XHeight => XHeight;

	bool ITypefaceMetrics.Symbol => Symbol;

	double ITypefaceMetrics.UnderlinePosition => UnderlinePosition;

	double ITypefaceMetrics.UnderlineThickness => UnderlineThickness;

	double ITypefaceMetrics.StrikethroughPosition => StrikethroughPosition;

	double ITypefaceMetrics.StrikethroughThickness => StrikethroughThickness;

	/// <summary>Gets the advance widths for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represents advance width information for the glyphs. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the advance width.</returns>
	public IDictionary<ushort, double> AdvanceWidths
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetAdvanceWidth);
		}
	}

	/// <summary>Gets the advance heights for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key-value pairs that represents advance height information for the glyphs. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the advance height.</returns>
	public IDictionary<ushort, double> AdvanceHeights
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetAdvanceHeight);
		}
	}

	/// <summary>Gets the distance from the leading end of the advance vector to the left edge of the black box for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent distance information for the glyphs. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the distance.</returns>
	public IDictionary<ushort, double> LeftSideBearings
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetLeftSidebearing);
		}
	}

	/// <summary>Gets the distance from the right edge of the black box to the right end of the advance vector for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent distance information. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the distance.</returns>
	public IDictionary<ushort, double> RightSideBearings
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetRightSidebearing);
		}
	}

	/// <summary>Gets the distance from the top end of the vertical advance vector to the top edge of the black box for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent distance information. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the distance.</returns>
	public IDictionary<ushort, double> TopSideBearings
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetTopSidebearing);
		}
	}

	/// <summary>Gets the distance from bottom edge of the black box to the bottom end of the advance vector for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent distance information. The key is a <see cref="T:System.UInt16" /> that identifies the glyph. The value is a <see cref="T:System.Double" /> that represents the distance.</returns>
	public IDictionary<ushort, double> BottomSideBearings
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetBottomSidebearing);
		}
	}

	/// <summary>Gets the offset value from the horizontal Western baseline to the bottom of the glyph black box for the glyphs represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains key/value pairs that represent offsets for the glyphs. The key is a <see cref="T:System.UInt16" /> that identifies the glyph index. The value is a <see cref="T:System.Double" /> that represents the offset value.</returns>
	public IDictionary<ushort, double> DistancesFromHorizontalBaselineToBlackBoxBottom
	{
		get
		{
			CheckInitialized();
			return CreateGlyphIndexer(GetBaseline);
		}
	}

	/// <summary>Gets the nominal mapping of a Unicode code point to a glyph index as defined by the font 'CMAP' table.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IDictionary`2" /> object that contains the mapping of a Unicode code points to glyph indices for all glyphs in the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</returns>
	public IDictionary<int, ushort> CharacterToGlyphMap
	{
		get
		{
			CheckInitialized();
			return _fontFace.CharacterMap;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.StyleSimulations" /> for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.StyleSimulations" /> values that represents the style simulation for the font.</returns>
	public StyleSimulations StyleSimulations
	{
		get
		{
			CheckInitialized();
			return _styleSimulations;
		}
		set
		{
			CheckInitializing();
			_styleSimulations = value;
		}
	}

	/// <summary>Gets the number of glyphs for the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>The total number of glyphs.</returns>
	public int GlyphCount
	{
		get
		{
			CheckInitialized();
			FontFace fontFace = _font.GetFontFace();
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

	internal Font FontDWrite
	{
		get
		{
			CheckInitialized();
			return _font;
		}
	}

	internal FontSource FontSource
	{
		get
		{
			CheckInitialized();
			return _fontSource;
		}
	}

	internal int FaceIndex
	{
		get
		{
			CheckInitialized();
			FontFace fontFace = _font.GetFontFace();
			try
			{
				return checked((int)fontFace.Index);
			}
			finally
			{
				fontFace.Release();
			}
		}
	}

	internal FontFaceLayoutInfo FontFaceLayoutInfo
	{
		get
		{
			CheckInitialized();
			return _fontFace;
		}
	}

	internal ushort BlankGlyphIndex
	{
		get
		{
			CheckInitialized();
			return _fontFace.BlankGlyph;
		}
	}

	internal FontTechnology FontTechnology
	{
		get
		{
			CheckInitialized();
			return _fontFace.FontTechnology;
		}
	}

	internal ushort DesignEmHeight
	{
		get
		{
			CheckInitialized();
			return _font.Metrics.DesignUnitsPerEm;
		}
	}

	internal nint GetDWriteFontAddRef
	{
		get
		{
			CheckInitialized();
			return _font.DWriteFontAddRef;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphTypeface" /> class.</summary>
	public GlyphTypeface()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphTypeface" /> class using the specified font file location.</summary>
	/// <param name="typefaceSource">The URI that specifies the location of the font file.</param>
	public GlyphTypeface(Uri typefaceSource)
		: this(typefaceSource, StyleSimulations.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphTypeface" /> class using the specified font file location and <see cref="T:System.Windows.Media.StyleSimulations" /> value.</summary>
	/// <param name="typefaceSource">The URI that specifies the location of the font file.</param>
	/// <param name="styleSimulations">One of the <see cref="T:System.Windows.Media.StyleSimulations" /> values.</param>
	public GlyphTypeface(Uri typefaceSource, StyleSimulations styleSimulations)
	{
		Initialize(typefaceSource, styleSimulations);
	}

	internal GlyphTypeface(Font font)
	{
		StyleSimulations simulationFlags = (StyleSimulations)font.SimulationFlags;
		_font = font;
		FontFace fontFace = _font.GetFontFace();
		string uriPath;
		try
		{
			using (FontFile fontFile = fontFace.GetFileZero())
			{
				uriPath = fontFile.GetUriPath();
			}
			_originalUri = new SecurityCriticalDataClass<Uri>(Util.CombineUriWithFaceIndex(uriPath, checked((int)fontFace.Index)));
		}
		finally
		{
			fontFace.Release();
		}
		Uri fontUri = new Uri(uriPath);
		_fontFace = new FontFaceLayoutInfo(font);
		_fontSource = new FontSource(fontUri, skipDemand: true);
		Invariant.Assert(simulationFlags == StyleSimulations.None || simulationFlags == StyleSimulations.ItalicSimulation || simulationFlags == StyleSimulations.BoldSimulation || simulationFlags == StyleSimulations.BoldItalicSimulation);
		_styleSimulations = simulationFlags;
		_initializationState = InitializationState.IsInitialized;
	}

	private void Initialize(Uri typefaceSource, StyleSimulations styleSimulations)
	{
		if (typefaceSource == null)
		{
			throw new ArgumentNullException("typefaceSource");
		}
		if (!typefaceSource.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriNotAbsolute, "typefaceSource");
		}
		_originalUri = new SecurityCriticalDataClass<Uri>(typefaceSource);
		Util.SplitFontFaceIndex(typefaceSource, out var fontSourceUri, out var faceIndex);
		if (styleSimulations != 0 && styleSimulations != StyleSimulations.ItalicSimulation && styleSimulations != StyleSimulations.BoldSimulation && styleSimulations != StyleSimulations.BoldItalicSimulation)
		{
			throw new InvalidEnumArgumentException("styleSimulations", (int)styleSimulations, typeof(StyleSimulations));
		}
		_styleSimulations = styleSimulations;
		FontCollection fontCollectionFromFile = DWriteFactory.GetFontCollectionFromFile(fontSourceUri);
		using (FontFace fontFace = DWriteFactory.Instance.CreateFontFace(fontSourceUri, (uint)faceIndex, (FontSimulations)styleSimulations))
		{
			if (fontFace == null)
			{
				throw new FileFormatException(typefaceSource);
			}
			_font = fontCollectionFromFile.GetFontFromFontFace(fontFace);
		}
		_fontFace = new FontFaceLayoutInfo(_font);
		_fontSource = new FontSource(fontSourceUri, skipDemand: true);
		_initializationState = InitializationState.IsInitialized;
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.GlyphTypeface" />. </summary>
	/// <returns>A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		CheckInitialized();
		return _originalUri.Value.GetHashCode() ^ (int)StyleSimulations;
	}

	/// <summary>Determines whether the specified object is equal to the current <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Media.GlyphTypeface" /> and is equal to the current <see cref="T:System.Windows.Media.GlyphTypeface" />; otherwise, false. </returns>
	/// <param name="o">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</param>
	public override bool Equals(object o)
	{
		CheckInitialized();
		if (!(o is GlyphTypeface glyphTypeface))
		{
			return false;
		}
		if (StyleSimulations == glyphTypeface.StyleSimulations)
		{
			return _originalUri.Value == glyphTypeface._originalUri.Value;
		}
		return false;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Geometry" /> value describing the path for a single glyph in the font.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> value that represents the path of the glyph.</returns>
	/// <param name="glyphIndex">The index of the glyph to get the outline for.</param>
	/// <param name="renderingEmSize">The font size in drawing surface units.</param>
	/// <param name="hintingEmSize">The size to hint for in points.</param>
	[CLSCompliant(false)]
	public Geometry GetGlyphOutline(ushort glyphIndex, double renderingEmSize, double hintingEmSize)
	{
		CheckInitialized();
		return ComputeGlyphOutline(glyphIndex, sideways: false, renderingEmSize);
	}

	/// <summary>Returns the binary image of the font subset based on a specified collection of glyphs.</summary>
	/// <returns>A <see cref="T:System.Byte" /> array that represents the binary image of the font subset.</returns>
	/// <param name="glyphs">The collection of glyph indices to be included in the subset.</param>
	[CLSCompliant(false)]
	public byte[] ComputeSubset(ICollection<ushort> glyphs)
	{
		CheckInitialized();
		if (glyphs == null)
		{
			throw new ArgumentNullException("glyphs");
		}
		if (glyphs.Count <= 0)
		{
			throw new ArgumentException(SR.CollectionNumberOfElementsMustBeGreaterThanZero, "glyphs");
		}
		if (glyphs.Count > 65535)
		{
			throw new ArgumentException(SR.Format(SR.CollectionNumberOfElementsMustBeLessOrEqualTo, ushort.MaxValue), "glyphs");
		}
		UnmanagedMemoryStream unmanagedStream = FontSource.GetUnmanagedStream();
		try
		{
			TrueTypeFontDriver trueTypeFontDriver = new TrueTypeFontDriver(unmanagedStream, _originalUri.Value);
			trueTypeFontDriver.SetFace(FaceIndex);
			return trueTypeFontDriver.ComputeFontSubset(glyphs);
		}
		catch (SEHException e)
		{
			throw Util.ConvertInPageException(FontSource, e);
		}
		finally
		{
			unmanagedStream.Close();
		}
	}

	/// <summary>Returns the font file stream represented by the <see cref="T:System.Windows.Media.GlyphTypeface" /> object.</summary>
	/// <returns>A <see cref="T:System.IO.Stream" /> value that represents the font file.</returns>
	public Stream GetFontStream()
	{
		CheckInitialized();
		return FontSource.GetStream();
	}

	internal bool HasCharacter(uint unicodeValue)
	{
		return FontDWrite.HasCharacter(unicodeValue);
	}

	internal double GetAdvanceWidth(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		CheckInitialized();
		return (double)GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways).AdvanceWidth / (double)(int)DesignEmHeight;
	}

	private double GetAdvanceHeight(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		GetGlyphMetrics(glyph, 1.0, 1.0, pixelsPerDip, textFormattingMode, isSideways, out var _, out var ah, out var _, out var _, out var _, out var _, out var _);
		return ah;
	}

	private unsafe GlyphMetrics GlyphMetrics(ushort glyphIndex, double emSize, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		FontFace fontFace = _font.GetFontFace();
		GlyphMetrics result;
		try
		{
			if (glyphIndex >= fontFace.GlyphCount)
			{
				throw new ArgumentOutOfRangeException("glyphIndex", SR.Format(SR.GlyphIndexOutOfRange, glyphIndex));
			}
			result = default(GlyphMetrics);
			if (textFormattingMode == TextFormattingMode.Ideal)
			{
				fontFace.GetDesignGlyphMetrics(&glyphIndex, 1u, &result);
			}
			else
			{
				fontFace.GetDisplayGlyphMetrics(&glyphIndex, 1u, &result, (float)emSize, textFormattingMode != TextFormattingMode.Display, isSideways, pixelsPerDip);
			}
		}
		finally
		{
			fontFace.Release();
		}
		return result;
	}

	private unsafe void GlyphMetrics(ushort* pGlyphIndices, int characterCount, GlyphMetrics* pGlyphMetrics, double emSize, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		FontFace fontFace = _font.GetFontFace();
		checked
		{
			try
			{
				if (textFormattingMode == TextFormattingMode.Ideal)
				{
					fontFace.GetDesignGlyphMetrics(pGlyphIndices, (uint)characterCount, pGlyphMetrics);
				}
				else
				{
					fontFace.GetDisplayGlyphMetrics(pGlyphIndices, (uint)characterCount, pGlyphMetrics, (float)emSize, textFormattingMode != TextFormattingMode.Display, isSideways, pixelsPerDip);
				}
			}
			finally
			{
				fontFace.Release();
			}
		}
	}

	private double GetLeftSidebearing(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		return (double)GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways).LeftSideBearing / (double)(int)DesignEmHeight;
	}

	private double GetRightSidebearing(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		return (double)GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways).RightSideBearing / (double)(int)DesignEmHeight;
	}

	private double GetTopSidebearing(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		return (double)GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways).TopSideBearing / (double)(int)DesignEmHeight;
	}

	private double GetBottomSidebearing(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		return (double)GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways).BottomSideBearing / (double)(int)DesignEmHeight;
	}

	private double GetBaseline(ushort glyph, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways)
	{
		return BaselineHelper(GlyphMetrics(glyph, (int)DesignEmHeight, pixelsPerDip, textFormattingMode, isSideways)) / (double)(int)DesignEmHeight;
	}

	internal static double BaselineHelper(GlyphMetrics metrics)
	{
		return -1.0 * ((double)metrics.BottomSideBearing + (double)metrics.VerticalOriginY - (double)metrics.AdvanceHeight);
	}

	internal void GetGlyphMetrics(ushort glyph, double renderingEmSize, double scalingFactor, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways, out double aw, out double ah, out double lsb, out double rsb, out double tsb, out double bsb, out double baseline)
	{
		CheckInitialized();
		GlyphMetrics metrics = GlyphMetrics(glyph, renderingEmSize, pixelsPerDip, textFormattingMode, isSideways);
		double num = renderingEmSize / (double)(int)DesignEmHeight;
		if (TextFormattingMode.Display == textFormattingMode)
		{
			aw = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.AdvanceWidth, pixelsPerDip) * scalingFactor;
			ah = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.AdvanceHeight, pixelsPerDip) * scalingFactor;
			lsb = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.LeftSideBearing, pixelsPerDip) * scalingFactor;
			rsb = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.RightSideBearing, pixelsPerDip) * scalingFactor;
			tsb = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.TopSideBearing, pixelsPerDip) * scalingFactor;
			bsb = TextFormatterImp.RoundDipForDisplayMode(num * (double)metrics.BottomSideBearing, pixelsPerDip) * scalingFactor;
			baseline = TextFormatterImp.RoundDipForDisplayMode(num * BaselineHelper(metrics), pixelsPerDip) * scalingFactor;
		}
		else
		{
			aw = num * (double)metrics.AdvanceWidth * scalingFactor;
			ah = num * (double)metrics.AdvanceHeight * scalingFactor;
			lsb = num * (double)metrics.LeftSideBearing * scalingFactor;
			rsb = num * (double)metrics.RightSideBearing * scalingFactor;
			tsb = num * (double)metrics.TopSideBearing * scalingFactor;
			bsb = num * (double)metrics.BottomSideBearing * scalingFactor;
			baseline = num * BaselineHelper(metrics) * scalingFactor;
		}
	}

	internal unsafe void GetGlyphMetrics(ushort[] glyphs, int glyphsLength, double renderingEmSize, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways, GlyphMetrics[] glyphMetrics)
	{
		CheckInitialized();
		Invariant.Assert(glyphsLength <= glyphs.Length);
		fixed (GlyphMetrics* pGlyphMetrics = glyphMetrics)
		{
			fixed (ushort* pGlyphIndices = &glyphs[0])
			{
				GlyphMetrics(pGlyphIndices, glyphsLength, pGlyphMetrics, renderingEmSize, pixelsPerDip, textFormattingMode, isSideways);
			}
		}
	}

	internal unsafe Geometry ComputeGlyphOutline(ushort glyphIndex, bool sideways, double renderingEmSize)
	{
		CheckInitialized();
		FontFace fontFace = _font.GetFontFace();
		byte* pPathGeometryData;
		uint pSize;
		FillRule pFillRule;
		try
		{
			HRESULT.Check(MS.Win32.PresentationCore.UnsafeNativeMethods.MilCoreApi.MilGlyphRun_GetGlyphOutline(fontFace.DWriteFontFaceAddRef, glyphIndex, sideways, renderingEmSize, out pPathGeometryData, out pSize, out pFillRule));
		}
		finally
		{
			fontFace.Release();
		}
		Geometry.PathGeometryData pathData = default(Geometry.PathGeometryData);
		byte[] array = new byte[pSize];
		Marshal.Copy(new IntPtr(pPathGeometryData), array, 0, checked((int)pSize));
		HRESULT.Check(MS.Win32.PresentationCore.UnsafeNativeMethods.MilCoreApi.MilGlyphRun_ReleasePathGeometryData(pPathGeometryData));
		pathData.SerializedData = array;
		pathData.FillRule = pFillRule;
		pathData.Matrix = CompositionResourceManager.MatrixToMilMatrix3x2D(Matrix.Identity);
		PathStreamGeometryContext pathStreamGeometryContext = new PathStreamGeometryContext(pFillRule, null);
		PathGeometry.ParsePathGeometryData(pathData, pathStreamGeometryContext);
		return pathStreamGeometryContext.GetPathGeometry();
	}

	internal unsafe void GetAdvanceWidthsUnshaped(char* unsafeCharString, int stringLength, double emSize, float pixelsPerDip, double scalingFactor, int* advanceWidthsUnshaped, bool nullFont, TextFormattingMode textFormattingMode, bool isSideways)
	{
		CheckInitialized();
		Invariant.Assert(stringLength > 0);
		if (!nullFont)
		{
			CharacterBufferRange characters = new CharacterBufferRange(unsafeCharString, stringLength);
			GlyphMetrics[] glyphMetrics = BufferCache.GetGlyphMetrics(stringLength);
			GetGlyphMetricsOptimized(characters, emSize, pixelsPerDip, textFormattingMode, isSideways, glyphMetrics);
			if (TextFormattingMode.Display == textFormattingMode)
			{
				double num = emSize / (double)(int)DesignEmHeight;
				for (int i = 0; i < stringLength; i++)
				{
					advanceWidthsUnshaped[i] = (int)Math.Round(TextFormatterImp.RoundDipForDisplayMode((double)glyphMetrics[i].AdvanceWidth * num, pixelsPerDip) * scalingFactor);
				}
			}
			else
			{
				double num2 = emSize * scalingFactor / (double)(int)DesignEmHeight;
				for (int j = 0; j < stringLength; j++)
				{
					advanceWidthsUnshaped[j] = (int)Math.Round((double)glyphMetrics[j].AdvanceWidth * num2);
				}
			}
			BufferCache.ReleaseGlyphMetrics(glyphMetrics);
		}
		else
		{
			int num3 = (int)Math.Round(TextFormatterImp.RoundDip(emSize * GetAdvanceWidth(0, pixelsPerDip, textFormattingMode, isSideways), pixelsPerDip, textFormattingMode) * scalingFactor);
			for (int k = 0; k < stringLength; k++)
			{
				advanceWidthsUnshaped[k] = num3;
			}
		}
	}

	internal GlyphRun ComputeUnshapedGlyphRun(Point origin, CharacterBufferRange charBufferRange, IList<double> charWidths, double emSize, float pixelsPerDip, double emHintingSize, bool nullGlyph, CultureInfo cultureInfo, string deviceFontName, TextFormattingMode textFormattingMode)
	{
		CheckInitialized();
		ushort[] array = new ushort[charBufferRange.Length];
		if (nullGlyph)
		{
			for (int i = 0; i < charBufferRange.Length; i++)
			{
				array[i] = 0;
			}
		}
		else
		{
			GetGlyphIndicesOptimized(charBufferRange, array, pixelsPerDip);
		}
		return GlyphRun.TryCreate(this, 0, isSideways: false, emSize, pixelsPerDip, array, origin, charWidths, null, new PartialList<char>(charBufferRange.CharacterBuffer, charBufferRange.OffsetToFirstChar, charBufferRange.Length), deviceFontName, null, null, XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag), textFormattingMode);
	}

	internal void GetGlyphIndicesOptimized(CharacterBufferRange characters, ushort[] glyphIndices, float pixelsPerDip)
	{
		GetGlyphMetricsOptimized(characters, 0.0, pixelsPerDip, glyphIndices, null, TextFormattingMode.Ideal, isSideways: false);
	}

	internal void GetGlyphMetricsOptimized(CharacterBufferRange characters, double emSize, float pixelsPerDip, TextFormattingMode textFormattingMode, bool isSideways, GlyphMetrics[] glyphMetrics)
	{
		GetGlyphMetricsOptimized(characters, emSize, pixelsPerDip, null, glyphMetrics, textFormattingMode, isSideways);
	}

	internal unsafe void GetGlyphMetricsOptimized(CharacterBufferRange characters, double emSize, float pixelsPerDip, ushort[] glyphIndices, GlyphMetrics[] glyphMetrics, TextFormattingMode textFormattingMode, bool isSideways)
	{
		if (characters.Length * 4 < 1024)
		{
			uint* ptr = stackalloc uint[characters.Length];
			for (int i = 0; i < characters.Length; i++)
			{
				ptr[i] = characters[i];
			}
			GetGlyphMetricsAndIndicesOptimized(ptr, characters.Length, emSize, pixelsPerDip, glyphIndices, glyphMetrics, textFormattingMode, isSideways);
			return;
		}
		uint[] array = new uint[characters.Length];
		for (int j = 0; j < characters.Length; j++)
		{
			array[j] = characters[j];
		}
		fixed (uint* pCodepoints = &array[0])
		{
			GetGlyphMetricsAndIndicesOptimized(pCodepoints, characters.Length, emSize, pixelsPerDip, glyphIndices, glyphMetrics, textFormattingMode, isSideways);
		}
	}

	private unsafe void GetGlyphMetricsAndIndicesOptimized(uint* pCodepoints, int characterCount, double emSize, float pixelsPerDip, ushort[] glyphIndices, GlyphMetrics[] glyphMetrics, TextFormattingMode textFormattingMode, bool isSideways)
	{
		bool flag = false;
		if (glyphIndices == null)
		{
			glyphIndices = BufferCache.GetUShorts(characterCount);
			flag = true;
		}
		fixed (ushort* ptr = &glyphIndices[0])
		{
			_fontFace.CharacterMap.TryGetValues(pCodepoints, checked((uint)characterCount), ptr);
			if (glyphMetrics != null)
			{
				fixed (GlyphMetrics* pGlyphMetrics = &glyphMetrics[0])
				{
					GlyphMetrics(ptr, characterCount, pGlyphMetrics, emSize, pixelsPerDip, textFormattingMode, isSideways);
				}
			}
		}
		if (flag)
		{
			BufferCache.ReleaseUShorts(glyphIndices);
		}
	}

	private IDictionary<CultureInfo, string> GetFontInfo(InformationalStringID informationalStringID)
	{
		if (_font.GetInformationalStrings(informationalStringID, out var informationalStrings))
		{
			return informationalStrings;
		}
		return new LocalizedStrings();
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ISupportInitialize.BeginInit" />.</summary>
	void ISupportInitialize.BeginInit()
	{
		if (_initializationState == InitializationState.IsInitialized)
		{
			throw new InvalidOperationException(SR.OnlyOneInitialization);
		}
		if (_initializationState == InitializationState.IsInitializing)
		{
			throw new InvalidOperationException(SR.InInitialization);
		}
		_initializationState = InitializationState.IsInitializing;
	}

	/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.ISupportInitialize.EndInit" />.</summary>
	void ISupportInitialize.EndInit()
	{
		if (_initializationState != InitializationState.IsInitializing)
		{
			throw new InvalidOperationException(SR.NotInInitialization);
		}
		Initialize((_originalUri == null) ? null : _originalUri.Value, _styleSimulations);
	}

	private void CheckInitialized()
	{
		if (_initializationState != InitializationState.IsInitialized)
		{
			throw new InvalidOperationException(SR.InitializationIncomplete);
		}
	}

	private void CheckInitializing()
	{
		if (_initializationState != InitializationState.IsInitializing)
		{
			throw new InvalidOperationException(SR.NotInInitialization);
		}
	}

	private GlyphIndexer CreateGlyphIndexer(GlyphAccessor accessor)
	{
		FontFace fontFace = _font.GetFontFace();
		try
		{
			return new GlyphIndexer(accessor, fontFace.GlyphCount);
		}
		finally
		{
			fontFace.Release();
		}
	}
}
