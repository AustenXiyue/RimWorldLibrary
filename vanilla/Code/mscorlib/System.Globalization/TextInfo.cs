using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using Unity;

namespace System.Globalization;

/// <summary>Defines text properties and behaviors, such as casing, that are specific to a writing system. </summary>
[Serializable]
[ComVisible(true)]
public class TextInfo : ICloneable, IDeserializationCallback
{
	[OptionalField(VersionAdded = 2)]
	private string m_listSeparator;

	[OptionalField(VersionAdded = 2)]
	private bool m_isReadOnly;

	[OptionalField(VersionAdded = 3)]
	private string m_cultureName;

	[NonSerialized]
	private CultureData m_cultureData;

	[NonSerialized]
	private string m_textInfoName;

	[NonSerialized]
	private bool? m_IsAsciiCasingSameAsInvariant;

	internal static volatile TextInfo s_Invariant;

	[OptionalField(VersionAdded = 2)]
	private string customCultureName;

	[OptionalField(VersionAdded = 1)]
	internal int m_nDataItem;

	[OptionalField(VersionAdded = 1)]
	internal bool m_useUserOverride;

	[OptionalField(VersionAdded = 1)]
	internal int m_win32LangID;

	private const int wordSeparatorMask = 536672256;

	internal static TextInfo Invariant
	{
		get
		{
			if (s_Invariant == null)
			{
				s_Invariant = new TextInfo(CultureData.Invariant);
			}
			return s_Invariant;
		}
	}

	/// <summary>Gets the American National Standards Institute (ANSI) code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</summary>
	/// <returns>The ANSI code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public virtual int ANSICodePage => m_cultureData.IDEFAULTANSICODEPAGE;

	/// <summary>Gets the original equipment manufacturer (OEM) code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</summary>
	/// <returns>The OEM code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public virtual int OEMCodePage => m_cultureData.IDEFAULTOEMCODEPAGE;

	/// <summary>Gets the Macintosh code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</summary>
	/// <returns>The Macintosh code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public virtual int MacCodePage => m_cultureData.IDEFAULTMACCODEPAGE;

	/// <summary>Gets the Extended Binary Coded Decimal Interchange Code (EBCDIC) code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</summary>
	/// <returns>The EBCDIC code page used by the writing system represented by the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public virtual int EBCDICCodePage => m_cultureData.IDEFAULTEBCDICCODEPAGE;

	/// <summary>Gets the culture identifier for the culture associated with the current <see cref="T:System.Globalization.TextInfo" /> object.</summary>
	/// <returns>A number that identifies the culture from which the current <see cref="T:System.Globalization.TextInfo" /> object was created.</returns>
	[ComVisible(false)]
	public int LCID => CultureInfo.GetCultureInfo(m_textInfoName).LCID;

	/// <summary>Gets the name of the culture associated with the current <see cref="T:System.Globalization.TextInfo" /> object.</summary>
	/// <returns>The name of a culture. </returns>
	[ComVisible(false)]
	public string CultureName => m_textInfoName;

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Globalization.TextInfo" /> object is read-only.</summary>
	/// <returns>true if the current <see cref="T:System.Globalization.TextInfo" /> object is read-only; otherwise, false.</returns>
	[ComVisible(false)]
	public bool IsReadOnly => m_isReadOnly;

	/// <summary>Gets or sets the string that separates items in a list.</summary>
	/// <returns>The string that separates items in a list.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value in a set operation is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">In a set operation, the current <see cref="T:System.Globalization.TextInfo" /> object is read-only.</exception>
	public virtual string ListSeparator
	{
		[SecuritySafeCritical]
		get
		{
			if (m_listSeparator == null)
			{
				m_listSeparator = m_cultureData.SLIST;
			}
			return m_listSeparator;
		}
		[ComVisible(false)]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value", Environment.GetResourceString("String reference not set to an instance of a String."));
			}
			VerifyWritable();
			m_listSeparator = value;
		}
	}

	private bool IsAsciiCasingSameAsInvariant
	{
		get
		{
			if (!m_IsAsciiCasingSameAsInvariant.HasValue)
			{
				m_IsAsciiCasingSameAsInvariant = !(m_cultureData.SISO639LANGNAME == "az") && !(m_cultureData.SISO639LANGNAME == "tr");
			}
			return m_IsAsciiCasingSameAsInvariant.Value;
		}
	}

	/// <summary>Gets a value indicating whether the current <see cref="T:System.Globalization.TextInfo" /> object represents a writing system where text flows from right to left.</summary>
	/// <returns>true if text flows from right to left; otherwise, false.</returns>
	[ComVisible(false)]
	public bool IsRightToLeft => m_cultureData.IsRightToLeft;

	internal TextInfo(CultureData cultureData)
	{
		m_cultureData = cultureData;
		m_cultureName = m_cultureData.CultureName;
		m_textInfoName = m_cultureData.STEXTINFO;
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext ctx)
	{
		m_cultureData = null;
		m_cultureName = null;
	}

	private void OnDeserialized()
	{
		if (m_cultureData != null)
		{
			return;
		}
		if (m_cultureName == null)
		{
			if (customCultureName != null)
			{
				m_cultureName = customCultureName;
			}
			else if (m_win32LangID == 0)
			{
				m_cultureName = "ar-SA";
			}
			else
			{
				m_cultureName = CultureInfo.GetCultureInfo(m_win32LangID).m_cultureData.CultureName;
			}
		}
		m_cultureData = CultureInfo.GetCultureInfo(m_cultureName).m_cultureData;
		m_textInfoName = m_cultureData.STEXTINFO;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext ctx)
	{
		OnDeserialized();
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext ctx)
	{
		m_useUserOverride = false;
		customCultureName = m_cultureName;
		m_win32LangID = CultureInfo.GetCultureInfo(m_cultureName).LCID;
	}

	internal static int GetHashCodeOrdinalIgnoreCase(string s)
	{
		return GetHashCodeOrdinalIgnoreCase(s, forceRandomizedHashing: false, 0L);
	}

	internal static int GetHashCodeOrdinalIgnoreCase(string s, bool forceRandomizedHashing, long additionalEntropy)
	{
		return Invariant.GetCaseInsensitiveHashCode(s, forceRandomizedHashing, additionalEntropy);
	}

	[SecuritySafeCritical]
	internal static int CompareOrdinalIgnoreCase(string str1, string str2)
	{
		return InternalCompareStringOrdinalIgnoreCase(str1, 0, str2, 0, str1.Length, str2.Length);
	}

	[SecuritySafeCritical]
	internal static int CompareOrdinalIgnoreCaseEx(string strA, int indexA, string strB, int indexB, int lengthA, int lengthB)
	{
		return InternalCompareStringOrdinalIgnoreCase(strA, indexA, strB, indexB, lengthA, lengthB);
	}

	internal static int IndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
	{
		if (source.Length == 0 && value.Length == 0)
		{
			return 0;
		}
		int num = startIndex + count - value.Length;
		while (startIndex <= num)
		{
			if (CompareOrdinalIgnoreCaseEx(source, startIndex, value, 0, value.Length, value.Length) == 0)
			{
				return startIndex;
			}
			startIndex++;
		}
		return -1;
	}

	internal static int LastIndexOfStringOrdinalIgnoreCase(string source, string value, int startIndex, int count)
	{
		if (value.Length == 0)
		{
			return startIndex;
		}
		int num = startIndex - count + 1;
		if (value.Length > 0)
		{
			startIndex -= value.Length - 1;
		}
		while (startIndex >= num)
		{
			if (CompareOrdinalIgnoreCaseEx(source, startIndex, value, 0, value.Length, value.Length) == 0)
			{
				return startIndex;
			}
			startIndex--;
		}
		return -1;
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Globalization.TextInfo" /> object.</summary>
	/// <returns>A new instance of <see cref="T:System.Object" /> that is the memberwise clone of the current <see cref="T:System.Globalization.TextInfo" /> object.</returns>
	[ComVisible(false)]
	public virtual object Clone()
	{
		object obj = MemberwiseClone();
		((TextInfo)obj).SetReadOnlyState(readOnly: false);
		return obj;
	}

	/// <summary>Returns a read-only version of the specified <see cref="T:System.Globalization.TextInfo" /> object.</summary>
	/// <returns>The <see cref="T:System.Globalization.TextInfo" /> object specified by the <paramref name="textInfo" /> parameter, if <paramref name="textInfo" /> is read-only.-or-A read-only memberwise clone of the <see cref="T:System.Globalization.TextInfo" /> object specified by <paramref name="textInfo" />, if <paramref name="textInfo" /> is not read-only.</returns>
	/// <param name="textInfo">A <see cref="T:System.Globalization.TextInfo" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textInfo" /> is null.</exception>
	[ComVisible(false)]
	public static TextInfo ReadOnly(TextInfo textInfo)
	{
		if (textInfo == null)
		{
			throw new ArgumentNullException("textInfo");
		}
		if (textInfo.IsReadOnly)
		{
			return textInfo;
		}
		TextInfo obj = (TextInfo)textInfo.MemberwiseClone();
		obj.SetReadOnlyState(readOnly: true);
		return obj;
	}

	private void VerifyWritable()
	{
		if (m_isReadOnly)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Instance is read-only."));
		}
	}

	internal void SetReadOnlyState(bool readOnly)
	{
		m_isReadOnly = readOnly;
	}

	/// <summary>Converts the specified character to lowercase.</summary>
	/// <returns>The specified character converted to lowercase.</returns>
	/// <param name="c">The character to convert to lowercase. </param>
	[SecuritySafeCritical]
	public virtual char ToLower(char c)
	{
		if (IsAscii(c) && IsAsciiCasingSameAsInvariant)
		{
			return ToLowerAsciiInvariant(c);
		}
		return ToLowerInternal(c);
	}

	/// <summary>Converts the specified string to lowercase.</summary>
	/// <returns>The specified string converted to lowercase.</returns>
	/// <param name="str">The string to convert to lowercase. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	[SecuritySafeCritical]
	public virtual string ToLower(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return ToLowerInternal(str);
	}

	private static char ToLowerAsciiInvariant(char c)
	{
		if ('A' <= c && c <= 'Z')
		{
			c = (char)(c | 0x20);
		}
		return c;
	}

	/// <summary>Converts the specified character to uppercase.</summary>
	/// <returns>The specified character converted to uppercase.</returns>
	/// <param name="c">The character to convert to uppercase. </param>
	[SecuritySafeCritical]
	public virtual char ToUpper(char c)
	{
		if (IsAscii(c) && IsAsciiCasingSameAsInvariant)
		{
			return ToUpperAsciiInvariant(c);
		}
		return ToUpperInternal(c);
	}

	/// <summary>Converts the specified string to uppercase.</summary>
	/// <returns>The specified string converted to uppercase.</returns>
	/// <param name="str">The string to convert to uppercase. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	[SecuritySafeCritical]
	public virtual string ToUpper(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return ToUpperInternal(str);
	}

	private static char ToUpperAsciiInvariant(char c)
	{
		if ('a' <= c && c <= 'z')
		{
			c = (char)(c & -33);
		}
		return c;
	}

	private static bool IsAscii(char c)
	{
		return c < '\u0080';
	}

	/// <summary>Determines whether the specified object represents the same writing system as the current <see cref="T:System.Globalization.TextInfo" /> object.</summary>
	/// <returns>true if <paramref name="obj" /> represents the same writing system as the current <see cref="T:System.Globalization.TextInfo" />; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current <see cref="T:System.Globalization.TextInfo" />. </param>
	public override bool Equals(object obj)
	{
		if (obj is TextInfo textInfo)
		{
			return CultureName.Equals(textInfo.CultureName);
		}
		return false;
	}

	/// <summary>Serves as a hash function for the current <see cref="T:System.Globalization.TextInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public override int GetHashCode()
	{
		return CultureName.GetHashCode();
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Globalization.TextInfo" />.</summary>
	/// <returns>A string that represents the current <see cref="T:System.Globalization.TextInfo" />.</returns>
	public override string ToString()
	{
		return "TextInfo - " + m_cultureData.CultureName;
	}

	/// <summary>Converts the specified string to title case (except for words that are entirely in uppercase, which are considered to be acronyms)..</summary>
	/// <returns>The specified string converted to title case.</returns>
	/// <param name="str">The string to convert to title case. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	public string ToTitleCase(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (str.Length == 0)
		{
			return str;
		}
		StringBuilder result = new StringBuilder();
		string text = null;
		int num;
		for (num = 0; num < str.Length; num++)
		{
			UnicodeCategory unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, num, out var charLength);
			if (char.CheckLetter(unicodeCategory))
			{
				num = AddTitlecaseLetter(ref result, ref str, num, charLength) + 1;
				int num2 = num;
				bool flag = unicodeCategory == UnicodeCategory.LowercaseLetter;
				while (num < str.Length)
				{
					unicodeCategory = CharUnicodeInfo.InternalGetUnicodeCategory(str, num, out charLength);
					if (IsLetterCategory(unicodeCategory))
					{
						if (unicodeCategory == UnicodeCategory.LowercaseLetter)
						{
							flag = true;
						}
						num += charLength;
					}
					else if (str[num] == '\'')
					{
						num++;
						if (flag)
						{
							if (text == null)
							{
								text = ToLower(str);
							}
							result.Append(text, num2, num - num2);
						}
						else
						{
							result.Append(str, num2, num - num2);
						}
						num2 = num;
						flag = true;
					}
					else
					{
						if (IsWordSeparator(unicodeCategory))
						{
							break;
						}
						num += charLength;
					}
				}
				int num3 = num - num2;
				if (num3 > 0)
				{
					if (flag)
					{
						if (text == null)
						{
							text = ToLower(str);
						}
						result.Append(text, num2, num3);
					}
					else
					{
						result.Append(str, num2, num3);
					}
				}
				if (num < str.Length)
				{
					num = AddNonLetter(ref result, ref str, num, charLength);
				}
			}
			else
			{
				num = AddNonLetter(ref result, ref str, num, charLength);
			}
		}
		return result.ToString();
	}

	private static int AddNonLetter(ref StringBuilder result, ref string input, int inputIndex, int charLen)
	{
		if (charLen == 2)
		{
			result.Append(input[inputIndex++]);
			result.Append(input[inputIndex]);
		}
		else
		{
			result.Append(input[inputIndex]);
		}
		return inputIndex;
	}

	private int AddTitlecaseLetter(ref StringBuilder result, ref string input, int inputIndex, int charLen)
	{
		if (charLen == 2)
		{
			result.Append(ToUpper(input.Substring(inputIndex, charLen)));
			inputIndex++;
		}
		else
		{
			switch (input[inputIndex])
			{
			case 'Ǆ':
			case 'ǅ':
			case 'ǆ':
				result.Append('ǅ');
				break;
			case 'Ǉ':
			case 'ǈ':
			case 'ǉ':
				result.Append('ǈ');
				break;
			case 'Ǌ':
			case 'ǋ':
			case 'ǌ':
				result.Append('ǋ');
				break;
			case 'Ǳ':
			case 'ǲ':
			case 'ǳ':
				result.Append('ǲ');
				break;
			default:
				result.Append(ToUpper(input[inputIndex]));
				break;
			}
		}
		return inputIndex;
	}

	private static bool IsWordSeparator(UnicodeCategory category)
	{
		return (0x1FFCF800 & (1 << (int)category)) != 0;
	}

	private static bool IsLetterCategory(UnicodeCategory uc)
	{
		if (uc != 0 && uc != UnicodeCategory.LowercaseLetter && uc != UnicodeCategory.TitlecaseLetter && uc != UnicodeCategory.ModifierLetter)
		{
			return uc == UnicodeCategory.OtherLetter;
		}
		return true;
	}

	/// <summary>Raises the deserialization event when deserialization is complete.</summary>
	/// <param name="sender">The source of the deserialization event. </param>
	void IDeserializationCallback.OnDeserialization(object sender)
	{
		OnDeserialized();
	}

	[SecuritySafeCritical]
	internal int GetCaseInsensitiveHashCode(string str)
	{
		return GetCaseInsensitiveHashCode(str, forceRandomizedHashing: false, 0L);
	}

	[SecuritySafeCritical]
	internal int GetCaseInsensitiveHashCode(string str, bool forceRandomizedHashing, long additionalEntropy)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (this != s_Invariant)
		{
			return StringComparer.CurrentCultureIgnoreCase.GetHashCode(str);
		}
		return GetInvariantCaseInsensitiveHashCode(str);
	}

	private unsafe int GetInvariantCaseInsensitiveHashCode(string str)
	{
		fixed (char* ptr = str)
		{
			char* ptr2 = ptr;
			char* ptr3 = ptr2 + str.Length - 1;
			int num = 0;
			for (; ptr2 < ptr3; ptr2 += 2)
			{
				num = (num << 5) - num + char.ToUpperInvariant(*ptr2);
				num = (num << 5) - num + char.ToUpperInvariant(ptr2[1]);
			}
			ptr3++;
			if (ptr2 < ptr3)
			{
				num = (num << 5) - num + char.ToUpperInvariant(*ptr2);
			}
			return num;
		}
	}

	private unsafe string ToUpperInternal(string str)
	{
		if (str.Length == 0)
		{
			return string.Empty;
		}
		string text = string.FastAllocateString(str.Length);
		fixed (char* ptr3 = str)
		{
			fixed (char* ptr = text)
			{
				char* ptr2 = ptr;
				char* ptr4 = ptr3;
				for (int i = 0; i < str.Length; i++)
				{
					*ptr2 = ToUpper(*ptr4);
					ptr4++;
					ptr2++;
				}
			}
		}
		return text;
	}

	private unsafe string ToLowerInternal(string str)
	{
		if (str.Length == 0)
		{
			return string.Empty;
		}
		string text = string.FastAllocateString(str.Length);
		fixed (char* ptr3 = str)
		{
			fixed (char* ptr = text)
			{
				char* ptr2 = ptr;
				char* ptr4 = ptr3;
				for (int i = 0; i < str.Length; i++)
				{
					*ptr2 = ToLower(*ptr4);
					ptr4++;
					ptr2++;
				}
			}
		}
		return text;
	}

	private char ToUpperInternal(char c)
	{
		if (!m_cultureData.IsInvariantCulture)
		{
			switch (c)
			{
			case 'µ':
				return 'Μ';
			case 'ı':
				return 'I';
			case 'ſ':
				return 'S';
			case 'ǅ':
			case 'ǈ':
			case 'ǋ':
			case 'ǲ':
				return (char)(c - 1);
			case '\u0345':
				return 'Ι';
			case 'ς':
				return 'Σ';
			case 'ϐ':
				return 'Β';
			case 'ϑ':
				return 'Θ';
			case 'ϕ':
				return 'Φ';
			case 'ϖ':
				return 'Π';
			case 'ϰ':
				return 'Κ';
			case 'ϱ':
				return 'Ρ';
			case 'ϵ':
				return 'Ε';
			case 'ẛ':
				return 'Ṡ';
			case 'ι':
				return 'Ι';
			}
			if (!IsAsciiCasingSameAsInvariant)
			{
				if (c == 'i')
				{
					return 'İ';
				}
				if (IsAscii(c))
				{
					return ToUpperAsciiInvariant(c);
				}
			}
		}
		if (c >= 'à' && c <= 'ֆ')
		{
			return TextInfoToUpperData.range_00e0_0586[c - 224];
		}
		if (c >= 'ḁ' && c <= 'ῳ')
		{
			return TextInfoToUpperData.range_1e01_1ff3[c - 7681];
		}
		if (c >= 'ⅰ' && c <= 'ↄ')
		{
			return TextInfoToUpperData.range_2170_2184[c - 8560];
		}
		if (c >= 'ⓐ' && c <= 'ⓩ')
		{
			return TextInfoToUpperData.range_24d0_24e9[c - 9424];
		}
		if (c >= 'ⰰ' && c <= 'ⳣ')
		{
			return TextInfoToUpperData.range_2c30_2ce3[c - 11312];
		}
		if (c >= 'ⴀ' && c <= 'ⴥ')
		{
			return TextInfoToUpperData.range_2d00_2d25[c - 11520];
		}
		if (c >= 'ꙁ' && c <= 'ꚗ')
		{
			return TextInfoToUpperData.range_a641_a697[c - 42561];
		}
		if (c >= 'ꜣ' && c <= 'ꞌ')
		{
			return TextInfoToUpperData.range_a723_a78c[c - 42787];
		}
		if ('ａ' <= c && c <= 'ｚ')
		{
			return (char)(c - 32);
		}
		return c switch
		{
			'ᵹ' => 'Ᵹ', 
			'ᵽ' => 'Ᵽ', 
			'ⅎ' => 'Ⅎ', 
			_ => c, 
		};
	}

	private char ToLowerInternal(char c)
	{
		if (!m_cultureData.IsInvariantCulture)
		{
			switch (c)
			{
			case 'İ':
				return 'i';
			case 'ǅ':
			case 'ǈ':
			case 'ǋ':
			case 'ǲ':
				return (char)(c + 1);
			case 'ϒ':
				return 'υ';
			case 'ϓ':
				return 'ύ';
			case 'ϔ':
				return 'ϋ';
			case 'ϴ':
				return 'θ';
			case 'ẞ':
				return 'ß';
			case 'Ω':
				return 'ω';
			case 'K':
				return 'k';
			case 'Å':
				return 'å';
			}
			if (!IsAsciiCasingSameAsInvariant)
			{
				if (c == 'I')
				{
					return 'ı';
				}
				if (IsAscii(c))
				{
					return ToLowerAsciiInvariant(c);
				}
			}
		}
		if (c >= 'À' && c <= 'Ֆ')
		{
			return TextInfoToLowerData.range_00c0_0556[c - 192];
		}
		if (c >= 'Ⴀ' && c <= 'Ⴥ')
		{
			return TextInfoToLowerData.range_10a0_10c5[c - 4256];
		}
		if (c >= 'Ḁ' && c <= 'ῼ')
		{
			return TextInfoToLowerData.range_1e00_1ffc[c - 7680];
		}
		if (c >= 'Ⅰ' && c <= 'Ⅿ')
		{
			return TextInfoToLowerData.range_2160_216f[c - 8544];
		}
		if (c >= 'Ⓐ' && c <= 'Ⓩ')
		{
			return TextInfoToLowerData.range_24b6_24cf[c - 9398];
		}
		if (c >= 'Ⰰ' && c <= 'Ⱞ')
		{
			return TextInfoToLowerData.range_2c00_2c2e[c - 11264];
		}
		if (c >= 'Ⱡ' && c <= 'Ⳣ')
		{
			return TextInfoToLowerData.range_2c60_2ce2[c - 11360];
		}
		if (c >= 'Ꙁ' && c <= 'Ꚗ')
		{
			return TextInfoToLowerData.range_a640_a696[c - 42560];
		}
		if (c >= 'Ꜣ' && c <= 'Ꞌ')
		{
			return TextInfoToLowerData.range_a722_a78b[c - 42786];
		}
		if ('Ａ' <= c && c <= 'Ｚ')
		{
			return (char)(c + 32);
		}
		return c switch
		{
			'Ⅎ' => 'ⅎ', 
			'Ↄ' => 'ↄ', 
			_ => c, 
		};
	}

	private unsafe static int InternalCompareStringOrdinalIgnoreCase(string strA, int indexA, string strB, int indexB, int lenA, int lenB)
	{
		if (strA == null)
		{
			if (strB != null)
			{
				return -1;
			}
			return 0;
		}
		if (strB == null)
		{
			return 1;
		}
		int num = Math.Min(lenA, strA.Length - indexA);
		int num2 = Math.Min(lenB, strB.Length - indexB);
		if (num == num2 && (object)strA == strB)
		{
			return 0;
		}
		fixed (char* ptr = strA)
		{
			fixed (char* ptr4 = strB)
			{
				char* ptr2 = ptr + indexA;
				char* ptr3 = ptr2 + Math.Min(num, num2);
				char* ptr5 = ptr4 + indexB;
				while (ptr2 < ptr3)
				{
					if (*ptr2 != *ptr5)
					{
						char c = char.ToUpperInvariant(*ptr2);
						char c2 = char.ToUpperInvariant(*ptr5);
						if (c != c2)
						{
							return c - c2;
						}
					}
					ptr2++;
					ptr5++;
				}
				return num - num2;
			}
		}
	}

	internal TextInfo()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
