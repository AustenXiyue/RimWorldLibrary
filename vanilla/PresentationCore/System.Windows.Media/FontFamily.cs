using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.FontCache;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;
using MS.Internal.Shaping;

namespace System.Windows.Media;

/// <summary>Represents a family of related fonts.</summary>
[TypeConverter(typeof(FontFamilyConverter))]
[ValueSerializer(typeof(FontFamilyValueSerializer))]
[Localizability(LocalizationCategory.Font)]
public class FontFamily
{
	private FontFamilyIdentifier _familyIdentifier;

	private IFontFamily _firstFontFamily;

	internal static readonly CanonicalFontFamilyReference NullFontFamilyCanonicalName = CanonicalFontFamilyReference.Create(null, "#ARIAL");

	internal const string GlobalUI = "#GLOBAL USER INTERFACE";

	internal static FontFamily FontFamilyGlobalUI = new FontFamily("#GLOBAL USER INTERFACE");

	private static volatile FamilyCollection _defaultFamilyCollection = PreCreateDefaultFamilyCollection();

	private static FontFamilyMapCollection _emptyFamilyMaps = null;

	/// <summary>Gets a collection of strings and <see cref="T:System.Globalization.CultureInfo" /> values that represent the font family names of the <see cref="T:System.Windows.Media.FontFamily" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.LanguageSpecificStringDictionary" /> that represents the font family names.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public LanguageSpecificStringDictionary FamilyNames
	{
		get
		{
			if (FirstFontFamily is CompositeFontFamily compositeFontFamily)
			{
				return compositeFontFamily.FamilyNames;
			}
			return new LanguageSpecificStringDictionary(FirstFontFamily.Names);
		}
	}

	/// <summary>Gets a collection of typefaces for the <see cref="T:System.Windows.Media.FontFamily" /> object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.FamilyTypefaceCollection" /> that represents a collection of typefaces for the <see cref="T:System.Windows.Media.FontFamily" /> object.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public FamilyTypefaceCollection FamilyTypefaces
	{
		get
		{
			if (FirstFontFamily is CompositeFontFamily compositeFontFamily)
			{
				return compositeFontFamily.FamilyTypefaces;
			}
			return new FamilyTypefaceCollection(FirstFontFamily.GetTypefaces(_familyIdentifier));
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Windows.Media.FontFamilyMap" /> objects. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.FontFamilyMapCollection" /> that contains <see cref="T:System.Windows.Media.FontFamilyMap" /> objects.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public FontFamilyMapCollection FamilyMaps
	{
		get
		{
			if (FirstFontFamily is CompositeFontFamily compositeFontFamily)
			{
				return compositeFontFamily.FamilyMaps;
			}
			if (_emptyFamilyMaps == null)
			{
				_emptyFamilyMaps = new FontFamilyMapCollection(null);
			}
			return _emptyFamilyMaps;
		}
	}

	/// <summary>Gets the font family name that is used to construct the <see cref="T:System.Windows.Media.FontFamily" /> object.</summary>
	/// <returns>The font family name of the <see cref="T:System.Windows.Media.FontFamily" /> object.</returns>
	public string Source => _familyIdentifier.Source;

	/// <summary>Gets the base uniform resource identifier (URI) that is used to resolve a font family name.</summary>
	/// <returns>A value of type <see cref="T:System.Uri" />.</returns>
	public Uri BaseUri => _familyIdentifier.BaseUri;

	internal FontFamilyIdentifier FamilyIdentifier => _familyIdentifier;

	/// <summary>Gets or sets the distance between the baseline and the character cell top.</summary>
	/// <returns>A <see cref="T:System.Double" /> that indicates the distance between the baseline and the character cell top, expressed as a fraction of the font em size.</returns>
	public double Baseline
	{
		get
		{
			return FirstFontFamily.BaselineDesign;
		}
		set
		{
			VerifyMutable().SetBaseline(value);
		}
	}

	/// <summary>Gets or sets the line spacing value for the <see cref="T:System.Windows.Media.FontFamily" /> object. The line spacing is the recommended baseline-to-baseline distance for the text in this font relative to the em size.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the line spacing for the <see cref="T:System.Windows.Media.FontFamily" /> object.</returns>
	public double LineSpacing
	{
		get
		{
			return FirstFontFamily.LineSpacingDesign;
		}
		set
		{
			VerifyMutable().SetLineSpacing(value);
		}
	}

	internal IFontFamily FirstFontFamily
	{
		get
		{
			IFontFamily fontFamily = _firstFontFamily;
			if (fontFamily == null)
			{
				_familyIdentifier.Canonicalize();
				fontFamily = TypefaceMetricsCache.ReadonlyLookup(FamilyIdentifier) as IFontFamily;
				if (fontFamily == null)
				{
					FontStyle style = FontStyles.Normal;
					FontWeight weight = FontWeights.Normal;
					FontStretch stretch = FontStretches.Normal;
					fontFamily = FindFirstFontFamilyAndFace(ref style, ref weight, ref stretch);
					if (fontFamily == null)
					{
						fontFamily = LookupFontFamily(NullFontFamilyCanonicalName);
						Invariant.Assert(fontFamily != null);
					}
					TypefaceMetricsCache.Add(FamilyIdentifier, fontFamily);
				}
				_firstFontFamily = fontFamily;
			}
			return fontFamily;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontFamily" /> class from the specified font family name.</summary>
	/// <param name="familyName">The family name or names that comprise the new <see cref="T:System.Windows.Media.FontFamily" />. Multiple family names should be separated by commas. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="familyName" /> cannot be null.</exception>
	public FontFamily(string familyName)
		: this(null, familyName)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontFamily" /> class from the specified font family name and an optional base uniform resource identifier (URI) value.</summary>
	/// <param name="baseUri">Specifies the base URI that is used to resolve <paramref name="familyName" />.</param>
	/// <param name="familyName">The family name or names that comprise the new <see cref="T:System.Windows.Media.FontFamily" />. Multiple family names should be separated by commas.</param>
	public FontFamily(Uri baseUri, string familyName)
	{
		if (familyName == null)
		{
			throw new ArgumentNullException("familyName");
		}
		if (baseUri != null && !baseUri.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriNotAbsolute, "baseUri");
		}
		_familyIdentifier = new FontFamilyIdentifier(familyName, baseUri);
	}

	internal FontFamily(FontFamilyIdentifier familyIdentifier)
	{
		_familyIdentifier = familyIdentifier;
	}

	/// <summary>Initializes a new instance of an anonymous <see cref="T:System.Windows.Media.FontFamily" /> class.</summary>
	public FontFamily()
	{
		_familyIdentifier = new FontFamilyIdentifier(null, null);
		_firstFontFamily = new CompositeFontFamily();
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.FontFamily.Source" /> property.</summary>
	/// <returns>The source location of the <see cref="T:System.Windows.Media.FontFamily" /> object, including the directory or uniform resource identifier (URI) location reference.</returns>
	public override string ToString()
	{
		string source = _familyIdentifier.Source;
		if (source == null)
		{
			return string.Empty;
		}
		return source;
	}

	internal double GetLineSpacingForDisplayMode(double emSize, double pixelsPerDip)
	{
		return FirstFontFamily.LineSpacing(emSize, 1.0, pixelsPerDip, TextFormattingMode.Display);
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Media.Typeface" /> objects that represent the type faces in the default system font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.Typeface" /> objects.</returns>
	[CLSCompliant(false)]
	public ICollection<Typeface> GetTypefaces()
	{
		return FirstFontFamily.GetTypefaces(_familyIdentifier);
	}

	/// <summary>Serves as a hash function for <see cref="T:System.Windows.Media.FontFamily" />. It is suitable for use in hashing algorithms and data structures such as a hash table.</summary>
	/// <returns>An <see cref="T:System.Int32" /> value that represents the hash code for the current object.</returns>
	public override int GetHashCode()
	{
		if (_familyIdentifier.Source != null)
		{
			return _familyIdentifier.GetHashCode();
		}
		return base.GetHashCode();
	}

	/// <summary>Gets a value that indicates whether the current font family object and the specified font family object are the same.</summary>
	/// <returns>true if <paramref name="o" /> is equal to the current <see cref="T:System.Windows.Media.FontFamily" /> object; otherwise, false. If <paramref name="o" /> is not a <see cref="T:System.Windows.Media.FontFamily" /> object, false is returned.</returns>
	/// <param name="o">The <see cref="T:System.Windows.Media.FontFamily" /> object to compare.</param>
	public override bool Equals(object o)
	{
		if (!(o is FontFamily fontFamily))
		{
			return false;
		}
		if (_familyIdentifier.Source != null)
		{
			return _familyIdentifier.Equals(fontFamily._familyIdentifier);
		}
		return base.Equals(o);
	}

	private CompositeFontFamily VerifyMutable()
	{
		return (_firstFontFamily as CompositeFontFamily) ?? throw new NotSupportedException(SR.FontFamily_ReadOnly);
	}

	internal static IFontFamily FindFontFamilyFromFriendlyNameList(string friendlyNameList)
	{
		IFontFamily fontFamily = null;
		FontFamilyIdentifier fontFamilyIdentifier = new FontFamilyIdentifier(friendlyNameList, null);
		int num = 0;
		int count = fontFamilyIdentifier.Count;
		while (fontFamily == null && num < count)
		{
			fontFamily = LookupFontFamily(fontFamilyIdentifier[num]);
			num++;
		}
		if (fontFamily == null)
		{
			fontFamily = LookupFontFamily(NullFontFamilyCanonicalName);
			Invariant.Assert(fontFamily != null);
		}
		return fontFamily;
	}

	internal static IFontFamily SafeLookupFontFamily(CanonicalFontFamilyReference canonicalName, out bool nullFont)
	{
		nullFont = false;
		IFontFamily fontFamily = LookupFontFamily(canonicalName);
		if (fontFamily == null)
		{
			nullFont = true;
			fontFamily = LookupFontFamily(NullFontFamilyCanonicalName);
			Invariant.Assert(fontFamily != null, "Unable to create null font family");
		}
		return fontFamily;
	}

	internal static IFontFamily LookupFontFamily(CanonicalFontFamilyReference canonicalName)
	{
		FontStyle style = FontStyles.Normal;
		FontWeight weight = FontWeights.Normal;
		FontStretch stretch = FontStretches.Normal;
		return LookupFontFamilyAndFace(canonicalName, ref style, ref weight, ref stretch);
	}

	private static FamilyCollection PreCreateDefaultFamilyCollection()
	{
		return FamilyCollection.FromWindowsFonts(Util.WindowsFontsUriObject);
	}

	internal IFontFamily FindFirstFontFamilyAndFace(ref FontStyle style, ref FontWeight weight, ref FontStretch stretch)
	{
		if (_familyIdentifier.Source == null)
		{
			Invariant.Assert(_firstFontFamily != null, "Unnamed FontFamily should have a non-null first font family");
			return _firstFontFamily;
		}
		IFontFamily fontFamily = null;
		_familyIdentifier.Canonicalize();
		int num = 0;
		int count = _familyIdentifier.Count;
		while (fontFamily == null && num < count)
		{
			fontFamily = LookupFontFamilyAndFace(_familyIdentifier[num], ref style, ref weight, ref stretch);
			num++;
		}
		return fontFamily;
	}

	internal static IFontFamily LookupFontFamilyAndFace(CanonicalFontFamilyReference canonicalFamilyReference, ref FontStyle style, ref FontWeight weight, ref FontStretch stretch)
	{
		if (canonicalFamilyReference == null || canonicalFamilyReference == CanonicalFontFamilyReference.Unresolved)
		{
			return null;
		}
		try
		{
			FamilyCollection familyCollection = ((canonicalFamilyReference.LocationUri == null && canonicalFamilyReference.EscapedFileName == null) ? _defaultFamilyCollection : ((!(canonicalFamilyReference.LocationUri != null)) ? FamilyCollection.FromWindowsFonts(new Uri(Util.WindowsFontsUriObject, canonicalFamilyReference.EscapedFileName)) : FamilyCollection.FromUri(canonicalFamilyReference.LocationUri)));
			return familyCollection.LookupFamily(canonicalFamilyReference.FamilyName, ref style, ref weight, ref stretch);
		}
		catch (FileFormatException)
		{
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (ArgumentException)
		{
		}
		catch (NotSupportedException)
		{
		}
		catch (UriFormatException)
		{
		}
		return null;
	}
}
