using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Specifies the details of a single typeface supported by a <see cref="T:System.Windows.Media.FontFamily" />.</summary>
public class FamilyTypeface : IDeviceFont, ITypefaceMetrics
{
	private bool _readOnly;

	private FontStyle _style;

	private FontWeight _weight;

	private FontStretch _stretch;

	private double _underlinePosition;

	private double _underlineThickness;

	private double _strikeThroughPosition;

	private double _strikeThroughThickness;

	private double _capsHeight;

	private double _xHeight;

	private string _deviceFontName;

	private CharacterMetricsDictionary _characterMetrics;

	/// <summary>Gets or sets the style of the font family typeface design. </summary>
	/// <returns>A value of type <see cref="T:System.Windows.FontStyle" />.</returns>
	public FontStyle Style
	{
		get
		{
			return _style;
		}
		set
		{
			VerifyChangeable();
			_style = value;
		}
	}

	/// <summary>Gets or sets the designed weight of this font family typeface.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.FontWeight" />.</returns>
	public FontWeight Weight
	{
		get
		{
			return _weight;
		}
		set
		{
			VerifyChangeable();
			_weight = value;
		}
	}

	/// <summary>Gets or sets the designed stretch of the font family typeface.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.FontStretch" />.</returns>
	public FontStretch Stretch
	{
		get
		{
			return _stretch;
		}
		set
		{
			VerifyChangeable();
			_stretch = value;
		}
	}

	/// <summary>Gets or sets the position of underline value relative to the baseline. The value is also relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double UnderlinePosition
	{
		get
		{
			return _underlinePosition;
		}
		set
		{
			CompositeFontParser.VerifyMultiplierOfEm("UnderlinePosition", ref value);
			VerifyChangeable();
			_underlinePosition = value;
		}
	}

	/// <summary>Gets or sets the thickness of underline relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double UnderlineThickness
	{
		get
		{
			return _underlineThickness;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("UnderlineThickness", ref value);
			VerifyChangeable();
			_underlineThickness = value;
		}
	}

	/// <summary>Gets or sets the position of the strikethrough value relative to the baseline. The value is also relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double StrikethroughPosition
	{
		get
		{
			return _strikeThroughPosition;
		}
		set
		{
			CompositeFontParser.VerifyMultiplierOfEm("StrikethroughPosition", ref value);
			VerifyChangeable();
			_strikeThroughPosition = value;
		}
	}

	/// <summary>Gets or sets the thickness of the strikethrough relative to em size. </summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double StrikethroughThickness
	{
		get
		{
			return _strikeThroughThickness;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("StrikethroughThickness", ref value);
			VerifyChangeable();
			_strikeThroughThickness = value;
		}
	}

	/// <summary>Gets or sets the distance from baseline to top of an English capital, relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double CapsHeight
	{
		get
		{
			return _capsHeight;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("CapsHeight", ref value);
			VerifyChangeable();
			_capsHeight = value;
		}
	}

	/// <summary>Gets or sets the Western x-height relative to em size.</summary>
	/// <returns>A value of type <see cref="T:System.Double" />.</returns>
	public double XHeight
	{
		get
		{
			return _xHeight;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("XHeight", ref value);
			VerifyChangeable();
			_xHeight = value;
		}
	}

	bool ITypefaceMetrics.Symbol => false;

	StyleSimulations ITypefaceMetrics.StyleSimulations => StyleSimulations.None;

	/// <summary>Gets a collection of localized face names adjusted by the font differentiator.</summary>
	/// <returns>An array of type <see cref="T:System.Collections.Generic.IDictionary`2" /> that represent the localized typeface names.</returns>
	public IDictionary<XmlLanguage, string> AdjustedFaceNames => FontDifferentiator.ConstructFaceNamesByStyleWeightStretch(_style, _weight, _stretch);

	/// <summary>Gets or sets the name or unique identifier for a device font family typeface.</summary>
	/// <returns>A value of type <see cref="T:System.String" /> that represents the device font name.</returns>
	public string DeviceFontName
	{
		get
		{
			return _deviceFontName;
		}
		set
		{
			VerifyChangeable();
			_deviceFontName = value;
		}
	}

	/// <summary>Gets the collection of character metrics for a device font family typeface.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.CharacterMetricsDictionary" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public CharacterMetricsDictionary DeviceFontCharacterMetrics
	{
		get
		{
			if (_characterMetrics == null)
			{
				_characterMetrics = new CharacterMetricsDictionary();
			}
			return _characterMetrics;
		}
	}

	string IDeviceFont.Name => _deviceFontName;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FamilyTypeface" /> class. </summary>
	public FamilyTypeface()
	{
	}

	internal FamilyTypeface(Typeface face)
	{
		_style = face.Style;
		_weight = face.Weight;
		_stretch = face.Stretch;
		_underlinePosition = face.UnderlinePosition;
		_underlineThickness = face.UnderlineThickness;
		_strikeThroughPosition = face.StrikethroughPosition;
		_strikeThroughThickness = face.StrikethroughThickness;
		_capsHeight = face.CapsHeight;
		_xHeight = face.XHeight;
		_readOnly = true;
	}

	/// <summary>Compares two font family typefaces for equality.</summary>
	/// <returns>true if <paramref name="typeface" /> is not null and has the same properties as this font family typeface; otherwise, false.</returns>
	/// <param name="typeface">The <see cref="T:System.Windows.Media.FamilyTypeface" /> value to compare.</param>
	public bool Equals(FamilyTypeface typeface)
	{
		if (typeface == null)
		{
			return false;
		}
		if (Style == typeface.Style && Weight == typeface.Weight)
		{
			return Stretch == typeface.Stretch;
		}
		return false;
	}

	/// <summary>Compares two font family typefaces for equality.</summary>
	/// <returns>true if <paramref name="typeface" /> is not null and has the same properties as this typeface; otherwise, false.</returns>
	/// <param name="o">The <see cref="T:System.Object" /> value that represents the typeface to compare.</param>
	public override bool Equals(object o)
	{
		return Equals(o as FamilyTypeface);
	}

	/// <summary>Serves as a hash function for a <see cref="T:System.Windows.Media.FamilyTypeface" /> object. The <see cref="M:System.Windows.Media.FamilyTypeface.GetHashCode" /> method is suitable for use in hashing algorithms and data structures like a hash table.</summary>
	/// <returns>A value of type <see cref="T:System.Int32" />.</returns>
	public override int GetHashCode()
	{
		return _style.GetHashCode() ^ _weight.GetHashCode() ^ _stretch.GetHashCode();
	}

	private void VerifyChangeable()
	{
		if (_readOnly)
		{
			throw new NotSupportedException(SR.General_ObjectIsReadOnly);
		}
	}

	bool IDeviceFont.ContainsCharacter(int unicodeScalar)
	{
		if (_characterMetrics != null)
		{
			return _characterMetrics.GetValue(unicodeScalar) != null;
		}
		return false;
	}

	unsafe void IDeviceFont.GetAdvanceWidths(char* characterString, int characterLength, double emSize, int* pAdvances)
	{
		for (int i = 0; i < characterLength; i++)
		{
			CharacterMetrics value = _characterMetrics.GetValue(characterString[i]);
			if (value != null)
			{
				pAdvances[i] = Math.Max(0, (int)((value.BlackBoxWidth + value.LeftSideBearing + value.RightSideBearing) * emSize));
			}
			else
			{
				pAdvances[i] = 0;
			}
		}
	}
}
