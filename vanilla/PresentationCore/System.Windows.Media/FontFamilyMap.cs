using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Markup;
using MS.Internal.FontFace;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Defines which <see cref="T:System.Windows.Media.FontFamily" /> to use for a specified set of Unicode code points and a culture-specific language.</summary>
public class FontFamilyMap
{
	internal class Range
	{
		private int _first;

		private uint _delta;

		internal int First => _first;

		internal int Last => _first + (int)_delta;

		internal uint Delta => _delta;

		internal Range(int first, int last)
		{
			_first = first;
			_delta = (uint)(last - _first);
		}

		internal bool InRange(int ch)
		{
			return (uint)(ch - _first) <= _delta;
		}
	}

	private Range[] _ranges;

	private XmlLanguage _language;

	private double _scaleInEm;

	private string _targetFamilyName;

	internal const int LastUnicodeScalar = 1114111;

	private static readonly Range[] _defaultRanges = new Range[1]
	{
		new Range(0, 1114111)
	};

	internal static readonly FontFamilyMap Default = new FontFamilyMap(0, 1114111, null, null, 1.0);

	/// <summary>Gets or sets a string value representing one or more Unicode code point ranges.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing Unicode code point ranges. The default value is "0000-10ffff".</returns>
	/// <exception cref="T:System.FormatException">Unicode range not valid.</exception>
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	public string Unicode
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _ranges.Length; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.AppendFormat(NumberFormatInfo.InvariantInfo, "{0:x4}-{1:x4}", _ranges[i].First, _ranges[i].Last);
			}
			return stringBuilder.ToString();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_ranges = ParseUnicodeRanges(value);
		}
	}

	/// <summary>Gets or sets the target font family name for which the Unicode range applies to.</summary>
	/// <returns>A <see cref="T:System.String" /> value representing the font family name. The default value is a null string.</returns>
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	public string Target
	{
		get
		{
			return _targetFamilyName;
		}
		set
		{
			_targetFamilyName = value;
		}
	}

	/// <summary>Gets or sets the font scale factor for the target <see cref="T:System.Windows.Media.FontFamily" />.</summary>
	/// <returns>A <see cref="T:System.Double" /> value representing the scale factor. The default value is 1.0.</returns>
	public double Scale
	{
		get
		{
			return _scaleInEm;
		}
		set
		{
			CompositeFontParser.VerifyPositiveMultiplierOfEm("Scale", ref value);
			_scaleInEm = value;
		}
	}

	/// <summary>Gets or sets the culture-specific language for the <see cref="T:System.Windows.Media.FontFamilyMap" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Markup.XmlLanguage" /> value representing the culture-specific language. The default value is a null string.</returns>
	public XmlLanguage Language
	{
		get
		{
			return _language;
		}
		set
		{
			_language = ((value == XmlLanguage.Empty) ? null : value);
			_language = value;
		}
	}

	internal bool IsSimpleFamilyMap
	{
		get
		{
			if (_language == null && _scaleInEm == 1.0)
			{
				return _ranges == _defaultRanges;
			}
			return false;
		}
	}

	internal Range[] Ranges => _ranges;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontFamilyMap" /> class.</summary>
	public FontFamilyMap()
		: this(0, 1114111, null, null, 1.0)
	{
	}

	internal FontFamilyMap(int firstChar, int lastChar, XmlLanguage language, string targetFamilyName, double scaleInEm)
	{
		if (firstChar == 0 && lastChar == 1114111)
		{
			_ranges = _defaultRanges;
		}
		else
		{
			_ranges = new Range[1]
			{
				new Range(firstChar, lastChar)
			};
		}
		_language = language;
		_scaleInEm = scaleInEm;
		_targetFamilyName = targetFamilyName;
	}

	internal static bool MatchLanguage(XmlLanguage familyMapLanguage, XmlLanguage language)
	{
		if (familyMapLanguage == null)
		{
			return true;
		}
		if (language != null)
		{
			return familyMapLanguage.RangeIncludes(language);
		}
		return false;
	}

	internal static bool MatchCulture(XmlLanguage familyMapLanguage, CultureInfo culture)
	{
		if (familyMapLanguage == null)
		{
			return true;
		}
		if (culture != null)
		{
			return familyMapLanguage.RangeIncludes(culture);
		}
		return false;
	}

	private static void ThrowInvalidUnicodeRange()
	{
		throw new FormatException(SR.CompositeFontInvalidUnicodeRange);
	}

	private static Range[] ParseUnicodeRanges(string unicodeRanges)
	{
		List<Range> list = new List<Range>(3);
		for (int i = 0; i < unicodeRanges.Length; i++)
		{
			if (!ParseHexNumber(unicodeRanges, ref i, out var number))
			{
				ThrowInvalidUnicodeRange();
			}
			int number2 = number;
			if (i < unicodeRanges.Length)
			{
				if (unicodeRanges[i] == '?')
				{
					do
					{
						number *= 16;
						number2 = number2 * 16 + 15;
						i++;
					}
					while (i < unicodeRanges.Length && unicodeRanges[i] == '?' && number2 <= 1114111);
				}
				else if (unicodeRanges[i] == '-')
				{
					i++;
					if (!ParseHexNumber(unicodeRanges, ref i, out number2))
					{
						ThrowInvalidUnicodeRange();
					}
				}
			}
			if (number > number2 || number2 > 1114111 || (i < unicodeRanges.Length && unicodeRanges[i] != ','))
			{
				ThrowInvalidUnicodeRange();
			}
			list.Add(new Range(number, number2));
		}
		return list.ToArray();
	}

	internal static bool ParseHexNumber(string numString, ref int index, out int number)
	{
		while (index < numString.Length && numString[index] == ' ')
		{
			index++;
		}
		int num = index;
		number = 0;
		while (index < numString.Length)
		{
			int num2 = numString[index];
			if (num2 >= 48 && num2 <= 57)
			{
				number = number * 16 + (num2 - 48);
				index++;
				continue;
			}
			num2 |= 0x20;
			if (num2 < 97 || num2 > 102)
			{
				break;
			}
			number = number * 16 + (num2 - 87);
			index++;
		}
		bool result = index > num;
		while (index < numString.Length && numString[index] == ' ')
		{
			index++;
		}
		return result;
	}

	internal bool InRange(int ch)
	{
		for (int i = 0; i < _ranges.Length; i++)
		{
			if (_ranges[i].InRange(ch))
			{
				return true;
			}
		}
		return false;
	}
}
