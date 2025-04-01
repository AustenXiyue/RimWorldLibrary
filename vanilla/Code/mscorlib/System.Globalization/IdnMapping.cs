using System.Text;

namespace System.Globalization;

/// <summary>Supports the use of non-ASCII characters for Internet domain names. This class cannot be inherited.</summary>
public sealed class IdnMapping
{
	private bool allow_unassigned;

	private bool use_std3;

	private Punycode puny = new Punycode();

	/// <summary>Gets or sets a value that indicates whether unassigned Unicode code points are used in operations performed by members of the current <see cref="T:System.Globalization.IdnMapping" /> object.</summary>
	/// <returns>true if unassigned code points are used in operations; otherwise, false.</returns>
	public bool AllowUnassigned
	{
		get
		{
			return allow_unassigned;
		}
		set
		{
			allow_unassigned = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether standard or relaxed naming conventions are used in operations performed by members of the current <see cref="T:System.Globalization.IdnMapping" /> object.</summary>
	/// <returns>true if standard naming conventions are used in operations; otherwise, false.</returns>
	public bool UseStd3AsciiRules
	{
		get
		{
			return use_std3;
		}
		set
		{
			use_std3 = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Globalization.IdnMapping" /> class. </summary>
	public IdnMapping()
	{
	}

	/// <summary>Indicates whether a specified object and the current <see cref="T:System.Globalization.IdnMapping" /> object are equal.</summary>
	/// <returns>true if the object specified by the <paramref name="obj" /> parameter is derived from <see cref="T:System.Globalization.IdnMapping" /> and its <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties are equal; otherwise, false. </returns>
	/// <param name="obj">The object to compare to the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is IdnMapping idnMapping && allow_unassigned == idnMapping.allow_unassigned)
		{
			return use_std3 == idnMapping.use_std3;
		}
		return false;
	}

	/// <summary>Returns a hash code for this <see cref="T:System.Globalization.IdnMapping" /> object.</summary>
	/// <returns>One of four 32-bit signed constants derived from the properties of an <see cref="T:System.Globalization.IdnMapping" /> object.  The return value has no special meaning and is not suitable for use in a hash code algorithm.</returns>
	public override int GetHashCode()
	{
		return (allow_unassigned ? 2 : 0) + (use_std3 ? 1 : 0);
	}

	/// <summary>Encodes a string of domain name labels that consist of Unicode characters to a string of displayable Unicode characters in the US-ASCII character range. The string is formatted according to the IDNA standard.</summary>
	/// <returns>The equivalent of the string specified by the <paramref name="unicode" /> parameter, consisting of displayable Unicode characters in the US-ASCII character range (U+0020 to U+007E) and formatted according to the IDNA standard.</returns>
	/// <param name="unicode">The string to convert, which consists of one or more domain name labels delimited with label separators.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="unicode" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="unicode" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetAscii(string unicode)
	{
		if (unicode == null)
		{
			throw new ArgumentNullException("unicode");
		}
		return GetAscii(unicode, 0, unicode.Length);
	}

	/// <summary>Encodes a substring of domain name labels that include Unicode characters outside the US-ASCII character range. The substring is converted to a string of displayable Unicode characters in the US-ASCII character range and is formatted according to the IDNA standard.  </summary>
	/// <returns>The equivalent of the substring specified by the <paramref name="unicode" /> and <paramref name="index" /> parameters, consisting of displayable Unicode characters in the US-ASCII character range (U+0020 to U+007E) and formatted according to the IDNA standard.</returns>
	/// <param name="unicode">The string to convert, which consists of one or more domain name labels delimited with label separators.</param>
	/// <param name="index">A zero-based offset into <paramref name="unicode" /> that specifies the start of the substring to convert. The conversion operation continues to the end of the <paramref name="unicode" /> string.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="unicode" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than the length of <paramref name="unicode" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="unicode" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetAscii(string unicode, int index)
	{
		if (unicode == null)
		{
			throw new ArgumentNullException("unicode");
		}
		return GetAscii(unicode, index, unicode.Length - index);
	}

	/// <summary>Encodes the specified number of characters in a  substring of domain name labels that include Unicode characters outside the US-ASCII character range. The substring is converted to a string of displayable Unicode characters in the US-ASCII character range and is formatted according to the IDNA standard. </summary>
	/// <returns>The equivalent of the substring specified by the <paramref name="unicode" />, <paramref name="index" />, and <paramref name="count" /> parameters, consisting of displayable Unicode characters in the US-ASCII character range (U+0020 to U+007E) and formatted according to the IDNA standard.</returns>
	/// <param name="unicode">The string to convert, which consists of one or more domain name labels delimited with label separators.</param>
	/// <param name="index">A zero-based offset into <paramref name="unicode" /> that specifies the start of the substring.</param>
	/// <param name="count">The number of characters to convert in the substring that starts at the position specified by  <paramref name="index" /> in the <paramref name="unicode" /> string. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="unicode" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or-<paramref name="index" /> is greater than the length of <paramref name="unicode" />.-or-<paramref name="index" /> is greater than the length of <paramref name="unicode" /> minus <paramref name="count" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="unicode" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetAscii(string unicode, int index, int count)
	{
		if (unicode == null)
		{
			throw new ArgumentNullException("unicode");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index must be non-negative value");
		}
		if (count < 0 || index + count > unicode.Length)
		{
			throw new ArgumentOutOfRangeException("index + count must point inside the argument unicode string");
		}
		return Convert(unicode, index, count, toAscii: true);
	}

	private string Convert(string input, int index, int count, bool toAscii)
	{
		string text = input.Substring(index, count);
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] >= '\u0080')
			{
				text = text.ToLower(CultureInfo.InvariantCulture);
				break;
			}
		}
		string[] array = text.Split('.', '。', '．', '｡');
		int num = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].Length != 0 || j + 1 != array.Length)
			{
				if (toAscii)
				{
					array[j] = ToAscii(array[j], num);
				}
				else
				{
					array[j] = ToUnicode(array[j], num);
				}
			}
			num += array[j].Length;
		}
		return string.Join(".", array);
	}

	private string ToAscii(string s, int offset)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] < ' ' || s[i] == '\u007f')
			{
				throw new ArgumentException($"Not allowed character was found, at {offset + i}");
			}
			if (s[i] >= '\u0080')
			{
				s = NamePrep(s, offset);
				break;
			}
		}
		if (use_std3)
		{
			VerifyStd3AsciiRules(s, offset);
		}
		for (int j = 0; j < s.Length; j++)
		{
			if (s[j] >= '\u0080')
			{
				if (s.StartsWith("xn--", StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException($"The input string must not start with ACE (xn--), at {offset + j}");
				}
				s = puny.Encode(s, offset);
				s = "xn--" + s;
				break;
			}
		}
		VerifyLength(s, offset);
		return s;
	}

	private void VerifyLength(string s, int offset)
	{
		if (s.Length == 0)
		{
			throw new ArgumentException($"A label in the input string resulted in an invalid zero-length string, at {offset}");
		}
		if (s.Length > 63)
		{
			throw new ArgumentException($"A label in the input string exceeded the length in ASCII representation, at {offset}");
		}
	}

	private string NamePrep(string s, int offset)
	{
		s = s.Normalize(NormalizationForm.FormKC);
		VerifyProhibitedCharacters(s, offset);
		if (!allow_unassigned)
		{
			for (int i = 0; i < s.Length; i++)
			{
				if (char.GetUnicodeCategory(s, i) == UnicodeCategory.OtherNotAssigned)
				{
					throw new ArgumentException($"Use of unassigned Unicode characer is prohibited in this IdnMapping, at {offset + i}");
				}
			}
		}
		return s;
	}

	private void VerifyProhibitedCharacters(string s, int offset)
	{
		for (int i = 0; i < s.Length; i++)
		{
			switch (char.GetUnicodeCategory(s, i))
			{
			case UnicodeCategory.SpaceSeparator:
				if (s[i] < '\u0080')
				{
					continue;
				}
				break;
			case UnicodeCategory.Control:
				if (s[i] != 0 && s[i] < '\u0080')
				{
					continue;
				}
				break;
			default:
			{
				char c = s[i];
				if (('\ufddf' > c || c > '\ufdef') && (c & 0xFFFF) != 65534 && ('\ufff9' > c || c > '\ufffd') && ('⿰' > c || c > '⿻') && ('\u202a' > c || c > '\u202e') && ('\u206a' > c || c > '\u206f'))
				{
					switch (c)
					{
					case '\u0340':
					case '\u0341':
					case '\u200e':
					case '\u200f':
					case '\u2028':
					case '\u2029':
						break;
					default:
						continue;
					}
				}
				break;
			}
			case UnicodeCategory.Surrogate:
			case UnicodeCategory.PrivateUse:
				break;
			}
			throw new ArgumentException($"Not allowed character was in the input string, at {offset + i}");
		}
	}

	private void VerifyStd3AsciiRules(string s, int offset)
	{
		if (s.Length > 0 && s[0] == '-')
		{
			throw new ArgumentException($"'-' is not allowed at head of a sequence in STD3 mode, found at {offset}");
		}
		if (s.Length > 0 && s[s.Length - 1] == '-')
		{
			throw new ArgumentException($"'-' is not allowed at tail of a sequence in STD3 mode, found at {offset + s.Length - 1}");
		}
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c != '-' && (c <= '/' || (':' <= c && c <= '@') || ('[' <= c && c <= '`') || ('{' <= c && c <= '\u007f')))
			{
				throw new ArgumentException($"Not allowed character in STD3 mode, found at {offset + i}");
			}
		}
	}

	/// <summary>Decodes a string of one or more domain name labels, encoded according to the IDNA standard, to a string of Unicode characters. </summary>
	/// <returns>The Unicode equivalent of the IDNA substring specified by the <paramref name="ascii" /> parameter.</returns>
	/// <param name="ascii">The string to decode, which consists of one or more labels in the US-ASCII character range (U+0020 to U+007E) encoded according to the IDNA standard. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ascii" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ascii" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetUnicode(string ascii)
	{
		if (ascii == null)
		{
			throw new ArgumentNullException("ascii");
		}
		return GetUnicode(ascii, 0, ascii.Length);
	}

	/// <summary>Decodes a substring of one or more domain name labels, encoded according to the IDNA standard, to a string of Unicode characters. </summary>
	/// <returns>The Unicode equivalent of the IDNA substring specified by the <paramref name="ascii" /> and <paramref name="index" /> parameters.</returns>
	/// <param name="ascii">The string to decode, which consists of one or more labels in the US-ASCII character range (U+0020 to U+007E) encoded according to the IDNA standard. </param>
	/// <param name="index">A zero-based offset into <paramref name="ascii" /> that specifies the start of the substring to decode. The decoding operation continues to the end of the <paramref name="ascii" /> string.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ascii" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than the length of <paramref name="ascii" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ascii" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetUnicode(string ascii, int index)
	{
		if (ascii == null)
		{
			throw new ArgumentNullException("ascii");
		}
		return GetUnicode(ascii, index, ascii.Length - index);
	}

	/// <summary>Decodes a substring of a specified length that contains one or more domain name labels, encoded according to the IDNA standard, to a string of Unicode characters. </summary>
	/// <returns>The Unicode equivalent of the IDNA substring specified by the <paramref name="ascii" />, <paramref name="index" />, and <paramref name="count" /> parameters.</returns>
	/// <param name="ascii">The string to decode, which consists of one or more labels in the US-ASCII character range (U+0020 to U+007E) encoded according to the IDNA standard. </param>
	/// <param name="index">A zero-based offset into <paramref name="ascii" /> that specifies the start of the substring. </param>
	/// <param name="count">The number of characters to convert in the substring that starts at the position specified by <paramref name="index" /> in the <paramref name="ascii" /> string. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ascii" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index" /> or <paramref name="count" /> is less than zero.-or-<paramref name="index" /> is greater than the length of <paramref name="ascii" />.-or-<paramref name="index" /> is greater than the length of <paramref name="ascii" /> minus <paramref name="count" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ascii" /> is invalid based on the <see cref="P:System.Globalization.IdnMapping.AllowUnassigned" /> and <see cref="P:System.Globalization.IdnMapping.UseStd3AsciiRules" /> properties, and the IDNA standard.</exception>
	public string GetUnicode(string ascii, int index, int count)
	{
		if (ascii == null)
		{
			throw new ArgumentNullException("ascii");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index must be non-negative value");
		}
		if (count < 0 || index + count > ascii.Length)
		{
			throw new ArgumentOutOfRangeException("index + count must point inside the argument ascii string");
		}
		return Convert(ascii, index, count, toAscii: false);
	}

	private string ToUnicode(string s, int offset)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] >= '\u0080')
			{
				s = NamePrep(s, offset);
				break;
			}
		}
		if (!s.StartsWith("xn--", StringComparison.OrdinalIgnoreCase))
		{
			return s;
		}
		s = s.ToLower(CultureInfo.InvariantCulture);
		string strA = s;
		s = s.Substring(4);
		s = puny.Decode(s, offset);
		string result = s;
		s = ToAscii(s, offset);
		if (string.Compare(strA, s, StringComparison.OrdinalIgnoreCase) != 0)
		{
			throw new ArgumentException($"ToUnicode() failed at verifying the result, at label part from {offset}");
		}
		return result;
	}
}
